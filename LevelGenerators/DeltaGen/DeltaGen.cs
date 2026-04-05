using Godot;
using projecthorizonscs.Autoload;
using System.Collections.Generic;

namespace projecthorizonscs;

public partial class DeltaGen : Node2D
{
	[ExportGroup("TileMap Layers")]
	[Export] public NodePath GroundPath = "Ground";
	[Export] public NodePath DetailsSmallPath = "DetailsSmall";
	[Export] public NodePath DetailsMediumPath = "DetailsMedium";
	[Export] public NodePath ObjectsPath = "Objects";
	[Export] public NodePath ShadowsPath = "Shadows";

	[ExportGroup("Streaming Settings")]
	[Export] public int ChunkSize = 10;
	[Export] public int TileSize = 32;
	[Export] public int HorizontalRenderRadius = 3;
	[Export] public int VerticalRenderRadius = 4;
	[Export] public int LoadChunksPerFrame = 2;
	[Export] public int UnloadChunksPerFrame = 3;
	[Export] public bool DebugLogs = false;
	[Export] public bool AutoFitRenderRadiusToScreen = true;
	[Export] public int ExtraChunkMargin = 1;
	[Export] public Vector2 BaseCameraZoom = Vector2.One;

	[ExportGroup("Level Transition")]
	[Export] public bool UseLevelEntryChunkOnReady = true;
	[Export] public bool MovePlayerToInitialPortalOnReady = true;
	[Export] public bool ForceFallbackToOriginChunk = true;

	[ExportGroup("Portal Scenes")]
	[Export] public string InitialPortalScenePath = "res://Portal/InitialPortal.tscn";
	[Export] public string ExitPortalScenePath = "res://Portal/ExitPortal.tscn";

	private TileMapLayer _ground;
	private TileMapLayer _detailsSmall;
	private TileMapLayer _detailsMedium;
	private TileMapLayer _objects;
	private TileMapLayer _shadows;

	private Vector2I _currentCenterChunk = new(int.MinValue, int.MinValue);
	private readonly Dictionary<Vector2I, DeltaChunkData> _loadedChunks = new();
	private readonly HashSet<Vector2I> _targetChunks = new();
	private readonly Queue<Vector2I> _loadQueue = new();
	private readonly Queue<Vector2I> _unloadQueue = new();
	private readonly HashSet<Vector2I> _queuedLoads = new();
	private readonly HashSet<Vector2I> _queuedUnloads = new();
	private readonly List<Vector2I> _scratchLoadedKeys = new();

	private string _worldName = "World";
	private int _currentLevel = 0;
	private Vector2I _lastComputedRadius = new(-1, -1);

	private DeltaLevelMetadata _levelMetadata;
	private PackedScene _initialPortalScene;
	private PackedScene _exitPortalScene;
	private Node2D _initialPortalReference;
	private Node2D _exitPortalReference;
	private bool _levelEntitiesSpawned = false;
	private bool _triedLatePlayerSnap = false;

	private readonly List<Node2D> _chestReferences = new();
	private readonly Dictionary<string, PackedScene> _chestSceneCache = new();

	public override void _Ready()
	{
		GD.Print("DEPOIS DO RELOAD:", DataManager.I.CurrentWorldData["CurrentLevel"]);
		_ground = GetNodeOrNull<TileMapLayer>(GroundPath);
		_detailsSmall = GetNodeOrNull<TileMapLayer>(DetailsSmallPath);
		_detailsMedium = GetNodeOrNull<TileMapLayer>(DetailsMediumPath);
		_objects = GetNodeOrNull<TileMapLayer>(ObjectsPath);
		_shadows = GetNodeOrNull<TileMapLayer>(ShadowsPath);

		if (_ground == null) GD.PrintErr("DeltaGen: Ground TileMapLayer not found! Check GroundPath export.");
		if (_detailsSmall == null) GD.PrintErr("DeltaGen: DetailsSmall TileMapLayer not found!");
		if (_detailsMedium == null) GD.PrintErr("DeltaGen: DetailsMedium TileMapLayer not found!");
		if (_objects == null) GD.PrintErr("DeltaGen: Objects TileMapLayer not found!");
		if (_shadows == null) GD.PrintErr("DeltaGen: Shadows TileMapLayer not found!");

		if (AutoFitRenderRadiusToScreen)
		{
			UpdateRenderRadiusFromViewport();
			GetViewport().SizeChanged += OnViewportSizeChanged;
		}

		if (DataManager.I != null)
		{
			if (DataManager.I.CurrentWorldData.TryGetValue("SaveName", out Variant saveName) && !string.IsNullOrWhiteSpace(saveName.AsString()))
				_worldName = saveName.AsString();

			if (DataManager.I.CurrentWorldData.TryGetValue("CurrentLevel", out Variant cLevel))
				_currentLevel = cLevel.AsInt32();
		}

		_initialPortalScene = ResourceLoader.Load<PackedScene>(InitialPortalScenePath);
		_exitPortalScene = ResourceLoader.Load<PackedScene>(ExitPortalScenePath);

		LoadLevelMetadata();

		if (DebugLogs)
		{
			GD.Print($"DeltaGen: Ready. Streaming Map: {_worldName} | Level: {_currentLevel}");
			string levelPath = $"user://saves/{_worldName}/level_{_currentLevel}";
			GD.Print($"DeltaGen: level path = {levelPath}");
			GD.Print($"DeltaGen: metadata exists = {FileAccess.FileExists(levelPath + "/level_metadata.dat")}");
		}

		// carrega chunks iniciais mesmo se o player ainda nao existir / estiver morto
		InitializeStartingChunks();

		// se ja tiver player valido, move pro spawn do level agora
		TrySnapPlayerToLevelEntry();

		TrySpawnLevelEntities();
	}

	public override void _Process(double delta)
	{
		if (_ground == null)
			return;

		if (AutoFitRenderRadiusToScreen)
			UpdateRenderRadiusFromViewport();

		// caso o player so apareca depois do reload da cena
		if (!_triedLatePlayerSnap)
			TrySnapPlayerToLevelEntry();

		if (Globals.I == null || Globals.I.LocalPlayer == null || !GodotObject.IsInstanceValid(Globals.I.LocalPlayer))
			return;

		Vector2I playerCell = WorldToCell(Globals.I.LocalPlayer.GlobalPosition);
		Vector2I newCenterChunk = WorldToChunk(playerCell);

		if (newCenterChunk != _currentCenterChunk)
		{
			_currentCenterChunk = newCenterChunk;
			RefreshTargetChunks(newCenterChunk);
		}

		ProcessChunkQueues();
		TrySpawnLevelEntities();
	}

	public override void _ExitTree()
	{
		if (AutoFitRenderRadiusToScreen && GetViewport() != null)
			GetViewport().SizeChanged -= OnViewportSizeChanged;
	}

	private void InitializeStartingChunks()
	{
		Vector2I startChunk = GetBestStartupChunk();
		_currentCenterChunk = startChunk;
		RefreshTargetChunks(startChunk);
		ProcessChunkQueuesImmediate();

		if (DebugLogs)
			GD.Print($"DeltaGen: initial startup chunk = {startChunk}");
	}

	private Vector2I GetBestStartupChunk()
	{
		// 1) usa o portal inicial do metadata
		if (UseLevelEntryChunkOnReady && _levelMetadata != null && _levelMetadata.HasInitialPortal)
		{
			Vector2I entryChunk = WorldToChunk(_levelMetadata.InitialPortalCell);
			if (DebugLogs)
				GD.Print($"DeltaGen: startup from metadata initial portal cell {_levelMetadata.InitialPortalCell} => chunk {entryChunk}");
			return entryChunk;
		}

		// 2) usa o player, se existir e for valido
		if (Globals.I != null && Globals.I.LocalPlayer != null && GodotObject.IsInstanceValid(Globals.I.LocalPlayer))
		{
			Vector2I playerCell = WorldToCell(Globals.I.LocalPlayer.GlobalPosition);
			Vector2I playerChunk = WorldToChunk(playerCell);
			if (DebugLogs)
				GD.Print($"DeltaGen: startup from player cell {playerCell} => chunk {playerChunk}");
			return playerChunk;
		}

		// 3) fallback bruto
		if (ForceFallbackToOriginChunk)
		{
			if (DebugLogs)
				GD.Print("DeltaGen: startup fallback to origin chunk (0,0)");
			return Vector2I.Zero;
		}

		return Vector2I.Zero;
	}

	private void LoadLevelMetadata()
	{
		string path = $"user://saves/{_worldName}/level_{_currentLevel}/level_metadata.dat";
		if (!FileAccess.FileExists(path))
		{
			if (DebugLogs)
				GD.Print($"DeltaGen: metadata missing at {path}");
			return;
		}

		using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			GD.PrintErr($"DeltaGen: failed to open level metadata: {path}");
			return;
		}

		Variant variant = file.GetVar();
		if (variant.VariantType != Variant.Type.Dictionary)
		{
			GD.PrintErr($"DeltaGen: invalid metadata format: {path}");
			return;
		}

		_levelMetadata = new DeltaLevelMetadata();
		_levelMetadata.Deserialize(variant.AsGodotDictionary());

		if (DebugLogs)
		{
			GD.Print($"DeltaGen: metadata loaded for level {_currentLevel}");
			GD.Print($"DeltaGen: has initial portal = {_levelMetadata.HasInitialPortal}");
			if (_levelMetadata.HasInitialPortal)
				GD.Print($"DeltaGen: initial portal cell = {_levelMetadata.InitialPortalCell}");
		}
	}

	private void TrySnapPlayerToLevelEntry()
	{
		if (_triedLatePlayerSnap || !MovePlayerToInitialPortalOnReady)
			return;

		if (_levelMetadata == null || !_levelMetadata.HasInitialPortal)
			return;

		if (Globals.I == null || Globals.I.LocalPlayer == null || !GodotObject.IsInstanceValid(Globals.I.LocalPlayer) || _ground == null)
			return;

		Vector2 spawnPos = CellToWorldCenter(_levelMetadata.InitialPortalCell);
		Globals.I.LocalPlayer.GlobalPosition = spawnPos;
		_triedLatePlayerSnap = true;

		if (DebugLogs)
			GD.Print($"DeltaGen: moved player to level entry cell {_levelMetadata.InitialPortalCell} => {spawnPos}");
	}

	private void TrySpawnLevelEntities()
	{
		if (_levelEntitiesSpawned || _levelMetadata == null || _ground == null)
			return;

		if (_levelMetadata.HasInitialPortal && _initialPortalReference == null && _initialPortalScene != null)
			_initialPortalReference = SpawnSceneAtCell(_initialPortalScene, _levelMetadata.InitialPortalCell);

		if (_levelMetadata.HasExitPortal && _exitPortalReference == null && _exitPortalScene != null)
			_exitPortalReference = SpawnSceneAtCell(_exitPortalScene, _levelMetadata.ExitPortalCell);

		foreach (DeltaChestSpawnData chest in _levelMetadata.Chests)
		{
			Node2D chestNode = SpawnChestAtCell(chest);
			if (chestNode != null)
				_chestReferences.Add(chestNode);
		}

		if (EnemysManager.I != null)
		{
			foreach (DeltaEnemySpawnData enemy in _levelMetadata.Enemies)
				EnemysManager.I.SpawnEnemy(enemy.EnemyId, CellToWorldCenter(enemy.Cell));
		}

		_levelEntitiesSpawned = true;
	}

	private Node2D SpawnSceneAtCell(PackedScene scene, Vector2I cell)
	{
		Node2D node = scene.Instantiate<Node2D>();
		node.GlobalPosition = CellToWorldCenter(cell);
		AddChild(node);
		return node;
	}
	private Node2D SpawnChestAtCell(DeltaChestSpawnData chestData)
	{
		if (chestData == null || string.IsNullOrWhiteSpace(chestData.ChestScenePath))
			return null;

		if (!_chestSceneCache.TryGetValue(chestData.ChestScenePath, out PackedScene scene))
		{
			scene = ResourceLoader.Load<PackedScene>(chestData.ChestScenePath);

			if (scene == null)
			{
				GD.PrintErr($"DeltaGen: failed to load chest scene: {chestData.ChestScenePath}");
				return null;
			}

			_chestSceneCache[chestData.ChestScenePath] = scene;
		}

		Node2D chestNode = scene.Instantiate<Node2D>();
		if (chestNode == null)
		{
			GD.PrintErr($"DeltaGen: chest scene root is not Node2D: {chestData.ChestScenePath}");
			return null;
		}

		chestNode.GlobalPosition = CellToWorldCenter(chestData.Cell);

		if (chestNode is IGeneratedChest generatedChest)
		{
			generatedChest.SetupChest(
				chestData.ChestId,
				_currentLevel,
				chestData.Cell
			);
		}

		AddChild(chestNode);

		if (DebugLogs)
			GD.Print($"DeltaGen: chest spawned at cell {chestData.Cell} using scene {chestData.ChestScenePath}");

		return chestNode;
	}
	private void RefreshTargetChunks(Vector2I centerChunk)
	{
		_targetChunks.Clear();

		for (int y = -VerticalRenderRadius; y <= VerticalRenderRadius; y++)
		{
			for (int x = -HorizontalRenderRadius; x <= HorizontalRenderRadius; x++)
			{
				Vector2I coord = new(centerChunk.X + x, centerChunk.Y + y);
				_targetChunks.Add(coord);

				if (!_loadedChunks.ContainsKey(coord) && _queuedLoads.Add(coord))
					_loadQueue.Enqueue(coord);
			}
		}

		_scratchLoadedKeys.Clear();
		foreach (Vector2I coord in _loadedChunks.Keys)
			_scratchLoadedKeys.Add(coord);

		foreach (Vector2I coord in _scratchLoadedKeys)
		{
			if (_targetChunks.Contains(coord))
				continue;

			if (_queuedUnloads.Add(coord))
				_unloadQueue.Enqueue(coord);
		}
	}

	private void ProcessChunkQueues()
	{
		int loads = 0;
		while (_loadQueue.Count > 0 && loads < Mathf.Max(1, LoadChunksPerFrame))
		{
			Vector2I coord = _loadQueue.Dequeue();
			_queuedLoads.Remove(coord);

			if (_loadedChunks.ContainsKey(coord) || !_targetChunks.Contains(coord))
				continue;

			LoadChunkFromDisk(coord);
			loads++;
		}

		int unloads = 0;
		while (_unloadQueue.Count > 0 && unloads < Mathf.Max(1, UnloadChunksPerFrame))
		{
			Vector2I coord = _unloadQueue.Dequeue();
			_queuedUnloads.Remove(coord);

			if (!_loadedChunks.ContainsKey(coord) || _targetChunks.Contains(coord))
				continue;

			UnloadChunk(coord);
			unloads++;
		}
	}

	private void ProcessChunkQueuesImmediate()
	{
		while (_loadQueue.Count > 0)
		{
			Vector2I coord = _loadQueue.Dequeue();
			_queuedLoads.Remove(coord);

			if (_loadedChunks.ContainsKey(coord) || !_targetChunks.Contains(coord))
				continue;

			LoadChunkFromDisk(coord);
		}

		while (_unloadQueue.Count > 0)
		{
			Vector2I coord = _unloadQueue.Dequeue();
			_queuedUnloads.Remove(coord);

			if (!_loadedChunks.ContainsKey(coord) || _targetChunks.Contains(coord))
				continue;

			UnloadChunk(coord);
		}
	}

	private void LoadChunkFromDisk(Vector2I chunkCoord)
	{
		string chunkFile = $"user://saves/{_worldName}/level_{_currentLevel}/chunk_{chunkCoord.X}_{chunkCoord.Y}.dat";

		if (DebugLogs)
			GD.Print($"DeltaGen: trying chunk {chunkFile}");

		if (!FileAccess.FileExists(chunkFile))
		{
			if (DebugLogs)
				GD.Print($"DeltaGen: missing chunk file {chunkFile}");
			return;
		}

		using var file = FileAccess.Open(chunkFile, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			GD.PrintErr($"DeltaGen: Failed to open chunk file: {chunkFile}");
			return;
		}

		Variant variant = file.GetVar();
		if (variant.VariantType != Variant.Type.Dictionary)
		{
			GD.PrintErr($"DeltaGen: Chunk file has wrong format: {chunkFile} (type={variant.VariantType})");
			return;
		}

		DeltaChunkData chunkData = new DeltaChunkData();
		chunkData.Deserialize(variant.AsGodotDictionary());

		_loadedChunks[chunkCoord] = chunkData;
		ApplyChunkToTilemap(chunkData);
	}

	private void ApplyChunkToTilemap(DeltaChunkData chunk)
	{
		if (chunk == null || _ground == null)
			return;

		foreach (Vector4I tile in chunk.GroundTiles)
			_ground.SetCell(new Vector2I(tile.X, tile.Y), 0, new Vector2I(tile.Z, tile.W));

		if (_detailsSmall != null)
			foreach (Vector4I tile in chunk.SmallDetailTiles)
				_detailsSmall.SetCell(new Vector2I(tile.X, tile.Y), 0, new Vector2I(tile.Z, tile.W));

		if (_detailsMedium != null)
			foreach (Vector4I tile in chunk.MediumDetailTiles)
				_detailsMedium.SetCell(new Vector2I(tile.X, tile.Y), 0, new Vector2I(tile.Z, tile.W));

		if (_objects != null)
			foreach (Vector4I tile in chunk.ObjectTiles)
				_objects.SetCell(new Vector2I(tile.X, tile.Y), 0, new Vector2I(tile.Z, tile.W));

		if (_shadows != null)
			foreach (Vector4I tile in chunk.ShadowTiles)
				_shadows.SetCell(new Vector2I(tile.X, tile.Y), 0, new Vector2I(tile.Z, tile.W));
	}

	private void UnloadChunk(Vector2I chunkCoord)
	{
		if (!_loadedChunks.Remove(chunkCoord, out DeltaChunkData chunk))
			return;

		if (_ground != null)
			foreach (Vector4I tile in chunk.GroundTiles)
				_ground.EraseCell(new Vector2I(tile.X, tile.Y));

		if (_detailsSmall != null)
			foreach (Vector4I tile in chunk.SmallDetailTiles)
				_detailsSmall.EraseCell(new Vector2I(tile.X, tile.Y));

		if (_detailsMedium != null)
			foreach (Vector4I tile in chunk.MediumDetailTiles)
				_detailsMedium.EraseCell(new Vector2I(tile.X, tile.Y));

		if (_objects != null)
			foreach (Vector4I tile in chunk.ObjectTiles)
				_objects.EraseCell(new Vector2I(tile.X, tile.Y));

		if (_shadows != null)
			foreach (Vector4I tile in chunk.ShadowTiles)
				_shadows.EraseCell(new Vector2I(tile.X, tile.Y));
	}

	private void OnViewportSizeChanged()
	{
		if (!AutoFitRenderRadiusToScreen)
			return;

		UpdateRenderRadiusFromViewport();

		if (_currentCenterChunk.X != int.MinValue && _currentCenterChunk.Y != int.MinValue)
			RefreshTargetChunks(_currentCenterChunk);
	}

	private void UpdateRenderRadiusFromViewport()
	{
		if (_ground == null || ChunkSize <= 0 || TileSize <= 0)
			return;

		Vector2 viewportSize = GetViewportRect().Size;

		Vector2 zoom = BaseCameraZoom;
		Camera2D camera = GetViewport().GetCamera2D();
		if (camera != null)
			zoom = camera.Zoom;

		zoom.X = Mathf.Max(zoom.X, 0.001f);
		zoom.Y = Mathf.Max(zoom.Y, 0.001f);

		float visibleWorldWidth = viewportSize.X * zoom.X;
		float visibleWorldHeight = viewportSize.Y * zoom.Y;

		float visibleTilesX = visibleWorldWidth / TileSize;
		float visibleTilesY = visibleWorldHeight / TileSize;

		int neededChunksX = Mathf.CeilToInt(visibleTilesX / ChunkSize);
		int neededChunksY = Mathf.CeilToInt(visibleTilesY / ChunkSize);

		int newHorizontalRadius = Mathf.Max(1, Mathf.CeilToInt(neededChunksX * 0.5f) + ExtraChunkMargin);
		int newVerticalRadius = Mathf.Max(1, Mathf.CeilToInt(neededChunksY * 0.5f) + ExtraChunkMargin + 1);

		Vector2I newRadius = new(newHorizontalRadius, newVerticalRadius);
		if (newRadius == _lastComputedRadius)
			return;

		HorizontalRenderRadius = newHorizontalRadius;
		VerticalRenderRadius = newVerticalRadius;
		_lastComputedRadius = newRadius;

		if (DebugLogs)
		{
			GD.Print(
				$"DeltaGen: viewport={viewportSize} zoom={zoom} " +
				$"=> radius H={HorizontalRenderRadius} V={VerticalRenderRadius}"
			);
		}
	}

	private Vector2I WorldToChunk(Vector2 worldCellPosition) =>
		new(Mathf.FloorToInt(worldCellPosition.X / ChunkSize), Mathf.FloorToInt(worldCellPosition.Y / ChunkSize));

	private Vector2I WorldToCell(Vector2 globalPosition)
	{
		if (_ground == null)
			return Vector2I.Zero;

		Vector2 localToGround = _ground.ToLocal(globalPosition);
		return _ground.LocalToMap(localToGround);
	}

	private Vector2 CellToWorldCenter(Vector2I cell)
	{
		Vector2 local = _ground.MapToLocal(cell);
		return _ground.ToGlobal(local);
	}
}
