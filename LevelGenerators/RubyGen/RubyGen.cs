using Godot;
using projecthorizonscs.Autoload;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace projecthorizonscs;

public partial class RubyGen : Node2D
{
	[ExportGroup("TileMap Layers")]
	[Export] public NodePath GroundPath = "Ground";
	[Export] public NodePath DetailsSmallPath = "DetailsSmall";
	[Export] public NodePath DetailsMediumPath = "DetailsMedium";
	[Export] public NodePath ObjectsPath = "Objects";
	[Export] public NodePath ShadowsPath = "Shadows";

	[ExportGroup("Chunk Settings")]
	[Export] public int ChunkSize = 10;
	[Export] public int ChunksX = 60;
	[Export] public int ChunksY = 120;
	[Export] public int TileSize = 32;
	[Export] public int HorizontalRenderRadius = 2;
	[Export] public int VerticalRenderRadius = 6;

	[ExportGroup("General")]
	[Export] public int SeedValue = 12345;
	[Export] public bool RandomizeSeedOnReady = true;
	[Export] public bool ClearBeforeGenerate = true;
	[Export] public bool GenerateOnReady = true;
	[Export] public int LevelBiomeId = 0;

	[ExportGroup("Void / Border")]
	[Export] public bool GenerateVoidBorder = true;
	[Export] public int VoidBorderThickness = 2;
	[Export] public Vector2I VoidAtlas = new(0, 5);

	[ExportGroup("Beach")]
	[Export] public bool GenerateBeach = true;
	[Export] public float BeachChance = 0.45f;
	[Export] public int BeachBorderDistance = 2;
	[Export] public Vector2I SandAtlas = new(1, 5);

	[ExportGroup("Lakes")]
	[Export] public bool GenerateLakes = true;
	[Export] public float LakeNoiseFrequency = 0.030f;
	[Export] public float LakeThreshold = 0.73f;
	[Export] public int LakeEdgeDepth = 1;
	[Export] public Vector2I WaterAtlas = new(2, 5);
	[Export] public Vector2I DeepWaterAtlas = new(3, 5);

	[ExportGroup("Ground Noise")]
	[Export] public float GroundFrequency = 0.035f;
	[Export] public FastNoiseLite.NoiseTypeEnum GroundNoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
	[Export] public float GroundThreshold = 0.40f;

	[ExportGroup("Shape / Island")]
	[Export] public bool UseIslandMask = true;
	[Export] public float IslandRoundness = 1.15f;
	[Export] public float IslandFalloffPower = 1.85f;

	[ExportGroup("Density Noise")]
	[Export] public float DensityFrequency = 0.010f;
	[Export] public FastNoiseLite.NoiseTypeEnum DensityNoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;

	[ExportGroup("Small Details Spawn")]
	[Export] public bool GenerateSmallDetails = true;
	[Export] public float SmallDetailBaseChance = 0.44f;
	[Export] public float SmallDetailDensityChance = 0.28f;

	[ExportGroup("Medium Details Spawn")]
	[Export] public bool GenerateMediumDetails = true;
	[Export] public float MediumDetailsFrequency = 0.08f;
	[Export] public FastNoiseLite.NoiseTypeEnum MediumDetailsNoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
	[Export] public float MediumDetailBaseChance = 0.004f;
	[Export] public float MediumDetailDensityChance = 0.012f;
	[Export] public float MediumDetailRegionChance = 0.010f;

	[ExportGroup("Trees")]
	[Export] public bool GenerateTrees = true;
	[Export] public int TreeStep = 3;
	[Export] public int TreeMinDistance = 4;
	[Export] public int TreePaddingCheckRadius = 2;
	[Export] public float TreeNoiseFrequency = 0.045f;
	[Export] public FastNoiseLite.NoiseTypeEnum TreeNoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
	[Export] public float TreeThreshold = 0.53f;
	[Export] public float TreeDensityInfluence = 0.24f;
	[Export] public float TreeBaseChanceBonus = 0.01f;
	[Export] public int TreeRandomOffsetX = 1;
	[Export] public int TreeRandomOffsetY = 1;

	[ExportGroup("Atlas - Ground")]
	[Export] public Vector2I GroundMainAtlas = new(0, 0);
	[Export] public Vector2I GroundAlt1Atlas = new(1, 0);
	[Export] public Vector2I GroundAlt2Atlas = new(2, 0);
	[Export] public float GroundAlt1Threshold = 0.60f;
	[Export] public float GroundAlt2Threshold = 0.78f;

	[ExportGroup("Edge Decorations")]
	[Export] public bool GenerateEdgeDecorations = true;
	[Export] public float EdgeDecorChance = 0.16f;
	[Export] public int EdgeDecorRadius = 1;
	[Export] public Godot.Collections.Array<Vector2I> EdgeDecorAtlases = new()
	{
		new Vector2I(6, 2),
		new Vector2I(7, 2),
		new Vector2I(8, 2),
	};

	[ExportGroup("Atlas - Small Details")]
	[Export] public Godot.Collections.Array<Vector2I> SmallDetailAtlases = new()
	{
		new Vector2I(0, 1),
		new Vector2I(1, 1),
		new Vector2I(2, 1),
		new Vector2I(3, 1),
		new Vector2I(4, 1),
		new Vector2I(5, 1),
	};

	[ExportGroup("Atlas - Medium Details")]
	[Export] public Godot.Collections.Array<Vector2I> MediumDetailAtlases = new()
	{
		new Vector2I(0, 2),
		new Vector2I(1, 2),
		new Vector2I(2, 2),
		new Vector2I(3, 2),
		new Vector2I(4, 2),
	};

	[ExportGroup("Atlas - Trees")]
	[Export] public Godot.Collections.Array<Vector2I> TreeAtlases = new()
	{
		new Vector2I(0, 3),
		new Vector2I(1, 3),
		new Vector2I(2, 3),
	};

	[ExportGroup("Ground Context")]
	[Export] public Godot.Collections.Array<Vector2I> DirtAtlases = new()
	{
		new Vector2I(10, 0),
		new Vector2I(11, 0)
	};

	[ExportGroup("Atlas - Shadows")]
	[Export] public Vector2I TreeShadowAtlas = new(0, 4);
	[Export] public Vector2I ShadowOffset = new(1, 1);

	[ExportGroup("Portal")]
	[Export] public string InitialPortalScenePath = "res://Portal/InitialPortal.tscn";
	[Export] public string ExitPortalScenePath = "res://Portal/ExitPortal.tscn";
	[Export] public int InitialPortalRadius = 3;
	[Export] public int ExitPortalRadius = 2;
	[Export] public int MinExitDistanceFromInitial = 25;

	private TileMapLayer _ground;
	private TileMapLayer _detailsSmall;
	private TileMapLayer _detailsMedium;
	private TileMapLayer _objects;
	private TileMapLayer _shadows;

	private PackedScene _initialPortalScene;
	private PackedScene _exitPortalScene;
	private Node2D _initialPortalReference;
	private Node2D _exitPortalReference;

	private HashSet<Vector2I> _loadedChunks = new();
	private Vector2I _currentCenterChunk = new(int.MinValue, int.MinValue);
	private Vector2I _initialPortalCell = Vector2I.Zero;

	public int EnemysAmount;

	public volatile int _threadProgress = 0;
	public volatile int _threadMaxProgress = 1;
	public volatile int _threadChunkProgress = 0;
	public volatile int _threadChunkMaxProgress = 1;
	public volatile int _threadCurrentChunkX = 0;
	public volatile int _threadCurrentChunkY = 0;

	public Godot.Collections.Dictionary<Vector2I, RubyChunkData> chunksDictionary = new();

	private readonly List<Vector2I> _treePositionsMainThread = new();

	public override async void _Ready()
	{
		if (!GenerateOnReady)
			return;

		try
		{
			if (!TryGetLayers())
			{
				GD.PrintErr("RubyGen: tilemap layers not found.");
				return;
			}

			if (RandomizeSeedOnReady)
				RandomizeSeed();

			_initialPortalScene = ResourceLoader.Load<PackedScene>(InitialPortalScenePath);
			_exitPortalScene = ResourceLoader.Load<PackedScene>(ExitPortalScenePath);

			LoadingScreen.I?.ShowLoading("preparando geração...", 0f);
			LoadingScreen.I?.SetSubText("iniciando ruby gen...");

			if (ClearBeforeGenerate)
				ClearAll();

			LoadingScreen.I?.SetText("organizando mundo...");
			LoadingScreen.I?.SetSubText("criando grid de chunks...");
			LoadingScreen.I?.SetProgress(5f);

			CreateChunksGrid();

			_threadProgress = 0;
			_threadMaxProgress = Mathf.Max(1, LevelSizeY);
			_threadChunkProgress = 0;
			_threadChunkMaxProgress = Mathf.Max(1, chunksDictionary.Count);
			_threadCurrentChunkX = 0;
			_threadCurrentChunkY = 0;

			LoadingScreen.I?.SetText("gerando mundo...");
			LoadingScreen.I?.SetSubText("thread iniciada...");
			LoadingScreen.I?.SetProgress(10f);

			var threadedTask = Task.Run(GenerateChunksDataThreaded);

			while (!threadedTask.IsCompleted)
			{
				UpdateLoadingScreen();
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			}

			RubyThreadGenerationResult threadedResult = await threadedTask;

			LoadingScreen.I?.SetText("materializando mundo...");
			LoadingScreen.I?.SetSubText("copiando chunks gerados...");
			LoadingScreen.I?.SetProgress(97.1f);
			await ApplyThreadedChunksDataAsync(threadedResult);

			LoadingScreen.I?.SetText("rasgando o primeiro portal...");
			LoadingScreen.I?.SetSubText("procurando ponto inicial...");
			LoadingScreen.I?.SetProgress(97.8f);
			SpawnInitialPortal();
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

			LoadingScreen.I?.SetText("rasgando o portal de saída...");
			LoadingScreen.I?.SetSubText("procurando ponto distante...");
			LoadingScreen.I?.SetProgress(98.2f);
			SpawnExitPortal();
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

			LoadingScreen.I?.SetText("trazendo o mundo à vista...");
			LoadingScreen.I?.SetSubText("renderizando chunks próximos...");
			LoadingScreen.I?.SetProgress(98.7f);
			await UpdateVisibleChunksAsync(Vector2.Zero);

			LoadingScreen.I?.SetText("despertando criaturas...");
			LoadingScreen.I?.SetSubText("espalhando ameaças...");
			LoadingScreen.I?.SetProgress(99.4f);
			await SpawnEnemysAsync();

			LoadingScreen.I?.SetText("mundo pronto");
			LoadingScreen.I?.SetSubText("ruby gen finalizado");
			LoadingScreen.I?.SetProgress(100f);

			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			LoadingScreen.I?.HideLoading();

			GD.Print($"RubyGen: generation finished. Seed={SeedValue} chunks={chunksDictionary.Count}");
		}
		catch (Exception e)
		{
			GD.PrintErr($"RubyGen READY ERROR: {e}");
			LoadingScreen.I?.HideLoading();
		}
	}

	public override void _Process(double delta)
	{
		if (Globals.I == null || Globals.I.LocalPlayer == null)
			return;

		Vector2I playerCell = WorldToCell(Globals.I.LocalPlayer.GlobalPosition);
		Vector2I newCenterChunk = WorldToChunk(playerCell);

		if (newCenterChunk != _currentCenterChunk)
		{
			_currentCenterChunk = newCenterChunk;
			UpdateVisibleChunks(playerCell);
		}
	}

	private bool TryGetLayers()
	{
		_ground = GetNodeOrNull<TileMapLayer>(GroundPath);
		_detailsSmall = GetNodeOrNull<TileMapLayer>(DetailsSmallPath);
		_detailsMedium = GetNodeOrNull<TileMapLayer>(DetailsMediumPath);
		_objects = GetNodeOrNull<TileMapLayer>(ObjectsPath);
		_shadows = GetNodeOrNull<TileMapLayer>(ShadowsPath);

		return _ground != null
			&& _detailsSmall != null
			&& _detailsMedium != null
			&& _objects != null
			&& _shadows != null;
	}

	public int LevelSizeX => ChunksX * ChunkSize;
	public int LevelSizeY => ChunksY * ChunkSize;

	public void RandomizeSeed()
	{
		var rng = new RandomNumberGenerator();
		SeedValue = rng.RandiRange(0, 99999999);
	}

	public void ClearAll()
	{
		_ground?.Clear();
		_detailsSmall?.Clear();
		_detailsMedium?.Clear();
		_objects?.Clear();
		_shadows?.Clear();
		_loadedChunks.Clear();
	}

	public void CreateChunksGrid()
	{
		chunksDictionary.Clear();

		for (int y = -ChunksY / 2; y < ChunksY / 2; y++)
		{
			for (int x = -ChunksX / 2; x < ChunksX / 2; x++)
			{
				Vector2I coord = new(x, y);
				chunksDictionary[coord] = new RubyChunkData();
			}
		}

		GD.Print($"RubyGen: chunk grid created ({chunksDictionary.Count})");
	}

	private void UpdateLoadingScreen()
	{
		if (LoadingScreen.I == null)
			return;

		float progress01 = _threadChunkMaxProgress > 0
			? (float)_threadChunkProgress / _threadChunkMaxProgress
			: 0f;

		float totalProgress = Mathf.Clamp(8f + (progress01 * 89f), 8f, 97f);

		string mainText;
		string subText;

		if (progress01 <= 0.05f)
		{
			mainText = "acordando a ilha...";
			subText = "preparando terreno base...";
		}
		else if (progress01 <= 0.12f)
		{
			mainText = "esculpindo o chão...";
			subText = "definindo massa de terra...";
		}
		else if (progress01 <= 0.20f)
		{
			mainText = "moldando o relevo...";
			subText = "aplicando variações do solo...";
		}
		else if (progress01 <= 0.30f)
		{
			mainText = "abrindo lagos...";
			subText = "escavando água rasa e profunda...";
		}
		else if (progress01 <= 0.38f)
		{
			mainText = "espalhando areia...";
			subText = "criando bordas de praia...";
		}
		else if (progress01 <= 0.46f)
		{
			mainText = "selando as bordas do mundo...";
			subText = "preenchendo o vazio ao redor...";
		}
		else if (progress01 <= 0.56f)
		{
			mainText = "espalhando vida baixa...";
			subText = "gerando detalhes pequenos...";
		}
		else if (progress01 <= 0.66f)
		{
			mainText = "erguendo vegetação...";
			subText = "gerando detalhes médios...";
		}
		else if (progress01 <= 0.76f)
		{
			mainText = "plantando árvores...";
			subText = "organizando sombras e espaço livre...";
		}
		else if (progress01 <= 0.86f)
		{
			mainText = "organizando território...";
			subText = "dividindo o mundo em chunks...";
		}
		else if (progress01 <= 0.94f)
		{
			mainText = "compactando dados...";
			subText = "encaixando células nas regiões...";
		}
		else
		{
			mainText = "quase pronto...";
			subText = "finalizando estrutura do mapa...";
		}

		LoadingScreen.I.SetText(mainText);
		LoadingScreen.I.SetSubText(
			$"{subText}  |  chunk atual: ({_threadCurrentChunkX}, {_threadCurrentChunkY})  |  {_threadChunkProgress} / {_threadChunkMaxProgress}"
		);
		LoadingScreen.I.SetProgress(totalProgress);
	}

	private async Task ApplyThreadedChunksDataAsync(RubyThreadGenerationResult threadedResult)
	{
		chunksDictionary.Clear();

		int count = 0;
		int total = threadedResult.Chunks.Count;

		foreach (var pair in threadedResult.Chunks)
		{
			chunksDictionary[pair.Key] = pair.Value;
			count++;

			if (count % 40 == 0)
			{
				LoadingScreen.I?.SetText("materializando mundo...");
				LoadingScreen.I?.SetSubText($"copiando chunks: {count} / {total}");
				LoadingScreen.I?.SetProgress(97.2f);

				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			}
		}
	}

	private async Task LoadChunkAsync(Vector2I chunkCoord)
	{
		if (!chunksDictionary.TryGetValue(chunkCoord, out RubyChunkData chunk))
			return;

		int ops = 0;

		foreach (RubyChunkTileData tile in chunk.GroundTiles)
		{
			_ground.SetCell(tile.Cell, 0, tile.Atlas);
			ops++;
			if (ops % 250 == 0)
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}

		foreach (RubyChunkTileData tile in chunk.SmallDetailTiles)
		{
			_detailsSmall.SetCell(tile.Cell, 0, tile.Atlas);
			ops++;
			if (ops % 250 == 0)
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}

		foreach (RubyChunkTileData tile in chunk.MediumDetailTiles)
		{
			_detailsMedium.SetCell(tile.Cell, 0, tile.Atlas);
			ops++;
			if (ops % 250 == 0)
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}

		foreach (RubyChunkTileData tile in chunk.ObjectTiles)
		{
			_objects.SetCell(tile.Cell, 0, tile.Atlas);
			ops++;
			if (ops % 250 == 0)
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}

		foreach (RubyChunkTileData tile in chunk.ShadowTiles)
		{
			_shadows.SetCell(tile.Cell, 0, tile.Atlas);
			ops++;
			if (ops % 250 == 0)
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
	}

	public async Task SpawnEnemysAsync()
	{
		int difficulty = 0;

		try
		{
			if (DataManager.I != null && DataManager.I.CurrentWorldData != null && DataManager.I.CurrentWorldData.ContainsKey("SaveDifficulty"))
				difficulty = (int)DataManager.I.CurrentWorldData["SaveDifficulty"];
		}
		catch
		{
			difficulty = 0;
		}

		EnemysAmount = (int)GD.RandRange(
			0,
			Mathf.Max(1, (chunksDictionary.Keys.Count / 1000) * ((difficulty + 1) / 2f))
		);

		GD.Print($"RubyGen: Enemys To Spawn {EnemysAmount}");

		int spawnedCount = 0;

		for (int i = 0; i < EnemysAmount; i++)
		{
			Vector2 spawnPosition = GetRandomSpawnPosition();
			if (spawnPosition == Vector2.Zero)
				continue;

			if (EnemysManager.I == null)
				break;

			string newEnemy = EnemysManager.I.GetRandomEnemyByChance(LevelBiomeId);
			if (string.IsNullOrEmpty(newEnemy))
				continue;

			EnemysManager.I.SpawnEnemy(newEnemy, spawnPosition);
			spawnedCount++;

			LoadingScreen.I?.SetText("despertando criaturas...");
			LoadingScreen.I?.SetSubText($"inimigos: {spawnedCount} / {EnemysAmount}");
			LoadingScreen.I?.SetProgress(99.6f);

			if (spawnedCount % 5 == 0)
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
	}

	private void UpdateVisibleChunks(Vector2 centerWorldPosition)
	{
		HashSet<Vector2I> neededChunks = GetNeededChunks(centerWorldPosition);

		foreach (Vector2I chunkCoord in neededChunks)
		{
			if (_loadedChunks.Contains(chunkCoord))
				continue;

			LoadChunk(chunkCoord);
			_loadedChunks.Add(chunkCoord);
		}

		foreach (Vector2I chunkCoord in _loadedChunks.ToList())
		{
			if (neededChunks.Contains(chunkCoord))
				continue;

			UnloadChunk(chunkCoord);
			_loadedChunks.Remove(chunkCoord);
		}
	}

	private void LoadChunk(Vector2I chunkCoord)
	{
		if (!chunksDictionary.TryGetValue(chunkCoord, out RubyChunkData chunk))
			return;

		foreach (RubyChunkTileData tile in chunk.GroundTiles)
			_ground.SetCell(tile.Cell, 0, tile.Atlas);

		foreach (RubyChunkTileData tile in chunk.SmallDetailTiles)
			_detailsSmall.SetCell(tile.Cell, 0, tile.Atlas);

		foreach (RubyChunkTileData tile in chunk.MediumDetailTiles)
			_detailsMedium.SetCell(tile.Cell, 0, tile.Atlas);

		foreach (RubyChunkTileData tile in chunk.ObjectTiles)
			_objects.SetCell(tile.Cell, 0, tile.Atlas);

		foreach (RubyChunkTileData tile in chunk.ShadowTiles)
			_shadows.SetCell(tile.Cell, 0, tile.Atlas);
	}

	private RubyThreadGenerationResult GenerateChunksDataThreaded()
	{
		var context = new ThreadGenerationContext(this);
		context.SetupNoises();

		var world = new RubyThreadWorldData();
		int halfX = LevelSizeX / 2;
		int halfY = LevelSizeY / 2;

		for (int y = -halfY; y < halfY; y++)
		{
			for (int x = -halfX; x < halfX; x++)
			{
				Vector2I cell = new(x, y);

				if (!context.IsGroundCell(world, cell))
					continue;

				float groundValue = context.GetNoise01(context.GroundNoise, x, y);
				Vector2I atlas = context.PickGroundAtlas(groundValue);
				world.Ground[cell] = atlas;
			}

			_threadProgress = y + halfY + 1;
		}

		if (GenerateLakes)
			context.GenerateLakesPass(world);

		if (GenerateBeach)
			context.GenerateBeachPass(world);

		if (GenerateVoidBorder)
			context.GenerateVoidBorderPass(world);

		if (GenerateSmallDetails)
			context.GenerateSmallDetailsPass(world);

		if (GenerateMediumDetails)
			context.GenerateMediumDetailsPass(world);

		if (GenerateEdgeDecorations)
			context.GenerateEdgeDecorationsPass(world);

		if (GenerateTrees)
			context.GenerateTreesPass(world);

		RubyThreadGenerationResult result = BuildChunksFromWorldData(world);
		return result;
	}

	private RubyThreadGenerationResult BuildChunksFromWorldData(RubyThreadWorldData world)
	{
		var result = new RubyThreadGenerationResult();

		foreach (Vector2I chunkCoord in chunksDictionary.Keys)
			result.Chunks[chunkCoord] = new RubyChunkData();

		int processedChunks = 0;
		int totalChunks = Mathf.Max(1, chunksDictionary.Count);

		foreach (var pair in world.Ground)
			AddTileToChunk(result.Chunks, pair.Key, pair.Value, RubyLayerType.Ground);

		foreach (var pair in world.SmallDetails)
			AddTileToChunk(result.Chunks, pair.Key, pair.Value, RubyLayerType.SmallDetails);

		foreach (var pair in world.MediumDetails)
			AddTileToChunk(result.Chunks, pair.Key, pair.Value, RubyLayerType.MediumDetails);

		foreach (var pair in world.Objects)
			AddTileToChunk(result.Chunks, pair.Key, pair.Value, RubyLayerType.Objects);

		foreach (var pair in world.Shadows)
			AddTileToChunk(result.Chunks, pair.Key, pair.Value, RubyLayerType.Shadows);

		foreach (var pair in result.Chunks)
		{
			_threadCurrentChunkX = pair.Key.X;
			_threadCurrentChunkY = pair.Key.Y;
			pair.Value.SpawnEnemys = IsSpawnableChunk(pair.Value);

			processedChunks++;
			_threadChunkProgress = processedChunks;
			_threadChunkMaxProgress = totalChunks;
		}

		return result;
	}

	private void AddTileToChunk(
		System.Collections.Generic.Dictionary<Vector2I, RubyChunkData> chunks,
		Vector2I cell,
		Vector2I atlas,
		RubyLayerType layerType)
	{
		Vector2I chunkCoord = CellToChunk(cell);

		if (!chunks.TryGetValue(chunkCoord, out RubyChunkData chunk))
			return;

		switch (layerType)
		{
			case RubyLayerType.Ground:
				chunk.GroundTiles.Add(new RubyChunkTileData(cell, atlas));
				if (IsPortalValidGroundAtlas(atlas))
					chunk.ValidGroundCells.Add(cell);
				if (atlas == WaterAtlas || atlas == DeepWaterAtlas)
					chunk.WaterCells++;
				if (atlas == VoidAtlas)
					chunk.VoidCells++;
				break;

			case RubyLayerType.SmallDetails:
				chunk.SmallDetailTiles.Add(new RubyChunkTileData(cell, atlas));
				break;

			case RubyLayerType.MediumDetails:
				chunk.MediumDetailTiles.Add(new RubyChunkTileData(cell, atlas));
				break;

			case RubyLayerType.Objects:
				chunk.ObjectTiles.Add(new RubyChunkTileData(cell, atlas));
				break;

			case RubyLayerType.Shadows:
				chunk.ShadowTiles.Add(new RubyChunkTileData(cell, atlas));
				break;
		}
	}

	private void ApplyThreadedChunksData(RubyThreadGenerationResult threadedResult)
	{
		chunksDictionary.Clear();

		foreach (var pair in threadedResult.Chunks)
			chunksDictionary[pair.Key] = pair.Value;
	}

	private Vector2I CellToChunk(Vector2I cell)
	{
		return new Vector2I(
			Mathf.FloorToInt((float)cell.X / ChunkSize),
			Mathf.FloorToInt((float)cell.Y / ChunkSize)
		);
	}

	private Vector2I WorldToChunk(Vector2 worldCellPosition)
	{
		return new Vector2I(
			Mathf.FloorToInt(worldCellPosition.X / ChunkSize),
			Mathf.FloorToInt(worldCellPosition.Y / ChunkSize)
		);
	}

	private Vector2I WorldToCell(Vector2 globalPosition)
	{
		Vector2 localToGround = _ground.ToLocal(globalPosition);
		return _ground.LocalToMap(localToGround);
	}

	private HashSet<Vector2I> GetNeededChunks(Vector2 centerWorldPosition)
	{
		HashSet<Vector2I> neededChunks = new();
		Vector2I centerChunk = WorldToChunk(centerWorldPosition);

		for (int y = -VerticalRenderRadius; y <= VerticalRenderRadius; y++)
		{
			for (int x = -HorizontalRenderRadius; x <= HorizontalRenderRadius; x++)
			{
				Vector2I chunkCoord = new(centerChunk.X + x, centerChunk.Y + y);

				if (chunksDictionary.ContainsKey(chunkCoord))
					neededChunks.Add(chunkCoord);
			}
		}

		return neededChunks;
	}

	private async Task UpdateVisibleChunksAsync(Vector2 centerWorldPosition)
	{
		HashSet<Vector2I> neededChunks = GetNeededChunks(centerWorldPosition);

		int loaded = 0;
		int total = neededChunks.Count;

		foreach (Vector2I chunkCoord in neededChunks)
		{
			if (_loadedChunks.Contains(chunkCoord))
				continue;

			await LoadChunkAsync(chunkCoord);
			_loadedChunks.Add(chunkCoord);
			loaded++;

			if (LoadingScreen.I != null)
			{
				LoadingScreen.I.SetText("trazendo o mundo à vista...");
				LoadingScreen.I.SetSubText($"renderizando chunks: {loaded} / {total}");
				LoadingScreen.I.SetProgress(98.8f);
			}

			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}

		foreach (Vector2I chunkCoord in _loadedChunks.ToList())
		{
			if (neededChunks.Contains(chunkCoord))
				continue;

			UnloadChunk(chunkCoord);
			_loadedChunks.Remove(chunkCoord);

			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}
	}

	private void UnloadChunk(Vector2I chunkCoord)
	{
		if (!chunksDictionary.TryGetValue(chunkCoord, out RubyChunkData chunk))
			return;

		foreach (RubyChunkTileData tile in chunk.GroundTiles)
			_ground.EraseCell(tile.Cell);

		foreach (RubyChunkTileData tile in chunk.SmallDetailTiles)
			_detailsSmall.EraseCell(tile.Cell);

		foreach (RubyChunkTileData tile in chunk.MediumDetailTiles)
			_detailsMedium.EraseCell(tile.Cell);

		foreach (RubyChunkTileData tile in chunk.ObjectTiles)
			_objects.EraseCell(tile.Cell);

		foreach (RubyChunkTileData tile in chunk.ShadowTiles)
			_shadows.EraseCell(tile.Cell);
	}

	private bool IsSpawnableChunk(RubyChunkData chunk)
	{
		int validGround = chunk.ValidGroundCells.Count;
		if (validGround < 10)
			return false;

		if (chunk.WaterCells > validGround / 2)
			return false;

		if (chunk.VoidCells > 0)
			return false;

		return true;
	}

	public void SpawnEnemys()
	{
		int difficulty = 0;

		try
		{
			if (DataManager.I != null && DataManager.I.CurrentWorldData != null && DataManager.I.CurrentWorldData.ContainsKey("SaveDifficulty"))
				difficulty = (int)DataManager.I.CurrentWorldData["SaveDifficulty"];
		}
		catch
		{
			difficulty = 0;
		}

		EnemysAmount = (int)GD.RandRange(
			0,
			Mathf.Max(1, (chunksDictionary.Keys.Count / 1000) * ((difficulty + 1) / 2f))
		);

		GD.Print($"RubyGen: Enemys To Spawn {EnemysAmount}");

		int spawnedCount = 0;

		for (int i = 0; i < EnemysAmount; i++)
		{
			Vector2 spawnPosition = GetRandomSpawnPosition();
			if (spawnPosition == Vector2.Zero)
				continue;

			if (EnemysManager.I == null)
				break;

			string newEnemy = EnemysManager.I.GetRandomEnemyByChance(LevelBiomeId);
			if (string.IsNullOrEmpty(newEnemy))
				continue;

			EnemysManager.I.SpawnEnemy(newEnemy, spawnPosition);
			spawnedCount++;

			LoadingScreen.I?.SetSubText($"inimigos: {spawnedCount} / {EnemysAmount}");
		}
	}

	public Vector2 GetRandomSpawnPosition()
	{
		var spawnableChunks = new List<(RubyChunkData chunk, Vector2I coord)>();
		Vector2? playerPosition = Globals.I?.LocalPlayer?.GlobalPosition;

		foreach (Vector2I gridPosition in _loadedChunks)
		{
			if (!chunksDictionary.TryGetValue(gridPosition, out RubyChunkData chunk))
				continue;

			if (!chunk.SpawnEnemys || chunk.ValidGroundCells.Count == 0)
				continue;

			if (playerPosition != null)
			{
				Vector2 chunkWorldCenter = CellToWorldCenter(gridPosition * ChunkSize + new Vector2I(ChunkSize / 2, ChunkSize / 2));

				if (chunkWorldCenter.DistanceTo(playerPosition.Value) < 500f)
					continue;
			}

			spawnableChunks.Add((chunk, gridPosition));
		}

		if (spawnableChunks.Count == 0)
			return Vector2.Zero;

		int chunkIndex = (int)GD.RandRange(0, spawnableChunks.Count - 1);
		return GetRandomPositionInChunk(spawnableChunks[chunkIndex].chunk);
	}

	public Vector2 GetRandomPositionInChunk(RubyChunkData chunk)
	{
		if (chunk.ValidGroundCells.Count == 0)
			return Vector2.Zero;

		int index = (int)GD.RandRange(0, chunk.ValidGroundCells.Count - 1);
		return CellToWorldCenter(chunk.ValidGroundCells[index]);
	}

	private Vector2 CellToWorldCenter(Vector2I cell)
	{
		Vector2 local = _ground.MapToLocal(cell);
		return _ground.ToGlobal(local);
	}

	private bool IsPortalValidGroundAtlas(Vector2I atlas)
	{
		return atlas == GroundMainAtlas || atlas == GroundAlt1Atlas || atlas == GroundAlt2Atlas;
	}

	private Vector2I? GetGroundCellForPortal(Vector2I worldCell)
	{
		Vector2I chunkCoord = CellToChunk(worldCell);

		if (!chunksDictionary.TryGetValue(chunkCoord, out RubyChunkData chunk))
			return null;

		return chunk.ValidGroundCells.Contains(worldCell) ? worldCell : null;
	}

	private bool HasEnoughSpaceForPortal(Vector2I centerCell, int radius)
	{
		for (int y = -radius; y <= radius; y++)
		{
			for (int x = -radius; x <= radius; x++)
			{
				Vector2I cell = centerCell + new Vector2I(x, y);

				if (!IsPortalValidAtCell(cell))
					return false;

				if (HasObjectAtCell(cell))
					return false;
			}
		}

		return true;
	}

	private bool IsPortalValidAtCell(Vector2I cell)
	{
		Vector2I chunkCoord = CellToChunk(cell);

		if (!chunksDictionary.TryGetValue(chunkCoord, out RubyChunkData chunk))
			return false;

		return chunk.ValidGroundCells.Contains(cell);
	}

	private bool HasObjectAtCell(Vector2I cell)
	{
		Vector2I chunkCoord = CellToChunk(cell);

		if (!chunksDictionary.TryGetValue(chunkCoord, out RubyChunkData chunk))
			return false;

		return chunk.ObjectTiles.Any(t => t.Cell == cell);
	}

	private List<Vector2I> GetCentralChunkCandidates(int maxChunks = 12)
	{
		var chunks = chunksDictionary.Keys.ToList();

		chunks.Sort((a, b) =>
		{
			int distA = a.DistanceSquaredTo(Vector2I.Zero);
			int distB = b.DistanceSquaredTo(Vector2I.Zero);
			return distA.CompareTo(distB);
		});

		return chunks.Take(Mathf.Min(maxChunks, chunks.Count)).ToList();
	}

	private List<Vector2I> GetOuterChunkCandidates(int maxChunks = 30)
	{
		var chunks = chunksDictionary.Keys.ToList();

		chunks.Sort((a, b) =>
		{
			int distA = b.DistanceSquaredTo(Vector2I.Zero);
			int distB = a.DistanceSquaredTo(Vector2I.Zero);
			return distA.CompareTo(distB);
		});

		return chunks.Take(Mathf.Min(maxChunks, chunks.Count)).ToList();
	}

	private List<Vector2I> GetValidPortalCellsInChunk(Vector2I chunkCoord, int sampleStep, int portalRadius)
	{
		List<Vector2I> validCells = new();

		int startX = chunkCoord.X * ChunkSize;
		int startY = chunkCoord.Y * ChunkSize;

		for (int localY = portalRadius; localY < ChunkSize - portalRadius; localY += sampleStep)
		{
			for (int localX = portalRadius; localX < ChunkSize - portalRadius; localX += sampleStep)
			{
				Vector2I worldCell = new(startX + localX, startY + localY);

				if (!IsPortalValidAtCell(worldCell))
					continue;

				if (!HasEnoughSpaceForPortal(worldCell, portalRadius))
					continue;

				validCells.Add(worldCell);
			}
		}

		return validCells;
	}

	private void SpawnInitialPortal()
	{
		if (_initialPortalScene == null)
		{
			GD.PrintErr("RubyGen: initial portal scene null");
			return;
		}

		var rng = new RandomNumberGenerator();
		var candidateChunks = GetCentralChunkCandidates(12);
		var candidateCells = new List<Vector2I>();

		foreach (Vector2I chunkCoord in candidateChunks)
			candidateCells.AddRange(GetValidPortalCellsInChunk(chunkCoord, 2, InitialPortalRadius));

		if (candidateCells.Count == 0)
		{
			GD.PrintErr("RubyGen: no valid initial portal position found");
			return;
		}

		candidateCells.Sort((a, b) => a.DistanceSquaredTo(Vector2I.Zero).CompareTo(b.DistanceSquaredTo(Vector2I.Zero)));

		int topCount = Mathf.Min(12, candidateCells.Count);
		Vector2I chosen = candidateCells[rng.RandiRange(0, topCount - 1)];

		_initialPortalCell = chosen;
		SpawnInitialPortalSceneAt(chosen);

		GD.Print($"RubyGen: Initial portal generated at {chosen}");
	}

	private void SpawnExitPortal()
	{
		if (_exitPortalScene == null)
		{
			GD.PrintErr("RubyGen: exit portal scene null");
			return;
		}

		var rng = new RandomNumberGenerator();
		List<Vector2I> candidates = CollectExitCandidates(true, MinExitDistanceFromInitial);

		if (candidates.Count == 0)
			candidates = CollectExitCandidates(false, MinExitDistanceFromInitial);

		if (candidates.Count == 0)
		{
			Vector2I? fallback = GetFarthestValidCellFromInitial();
			if (fallback == null)
			{
				GD.PrintErr("RubyGen: no valid exit portal position found");
				return;
			}

			SpawnExitPortalSceneAt(fallback.Value);
			GD.Print($"RubyGen: Exit portal fallback at {fallback.Value}");
			return;
		}

		Vector2I chosen = candidates[rng.RandiRange(0, candidates.Count - 1)];
		SpawnExitPortalSceneAt(chosen);

		GD.Print($"RubyGen: Exit portal generated at {chosen}");
	}

	private List<Vector2I> CollectExitCandidates(bool requireNearVoid, int minDistanceFromInitial)
	{
		List<Vector2I> result = new();

		foreach (Vector2I chunkCoord in GetOuterChunkCandidates(30))
		{
			int startX = chunkCoord.X * ChunkSize;
			int startY = chunkCoord.Y * ChunkSize;

			for (int localY = ExitPortalRadius; localY < ChunkSize - ExitPortalRadius; localY += 2)
			{
				for (int localX = ExitPortalRadius; localX < ChunkSize - ExitPortalRadius; localX += 2)
				{
					Vector2I worldCell = new(startX + localX, startY + localY);

					if (!IsPortalValidAtCell(worldCell))
						continue;

					if (worldCell.DistanceSquaredTo(_initialPortalCell) < minDistanceFromInitial * minDistanceFromInitial)
						continue;

					if (!HasEnoughSpaceForPortal(worldCell, ExitPortalRadius))
						continue;

					if (requireNearVoid && !IsNearVoid(worldCell, 6))
						continue;

					result.Add(worldCell);
				}
			}
		}

		return result;
	}

	private Vector2I? GetFarthestValidCellFromInitial()
	{
		Vector2I? bestCell = null;
		int bestDistance = -1;

		foreach (var pair in chunksDictionary)
		{
			foreach (Vector2I worldCell in pair.Value.ValidGroundCells)
			{
				if (!HasEnoughSpaceForPortal(worldCell, ExitPortalRadius))
					continue;

				int dist = worldCell.DistanceSquaredTo(_initialPortalCell);
				if (dist <= bestDistance)
					continue;

				bestDistance = dist;
				bestCell = worldCell;
			}
		}

		return bestCell;
	}

	private bool IsNearVoid(Vector2I cell, int radius = 6)
	{
		for (int y = -radius; y <= radius; y++)
		{
			for (int x = -radius; x <= radius; x++)
			{
				Vector2I check = cell + new Vector2I(x, y);
				Vector2I chunkCoord = CellToChunk(check);

				if (!chunksDictionary.TryGetValue(chunkCoord, out RubyChunkData chunk))
					return true;

				RubyChunkTileData tile = chunk.GroundTiles.FirstOrDefault(t => t.Cell == check);
				if (tile == null)
					return true;

				if (tile.Atlas == VoidAtlas)
					return true;
			}
		}

		return false;
	}

	private void SpawnInitialPortalSceneAt(Vector2I mapCell)
	{
		Node2D portal = _initialPortalScene.Instantiate<Node2D>();
		_initialPortalReference = portal;
		portal.GlobalPosition = CellToWorldCenter(mapCell);
		AddChild(portal);
	}

	private void SpawnExitPortalSceneAt(Vector2I mapCell)
	{
		Node2D portal = _exitPortalScene.Instantiate<Node2D>();
		_exitPortalReference = portal;
		portal.GlobalPosition = CellToWorldCenter(mapCell);
		AddChild(portal);
	}

	private enum RubyLayerType
	{
		Ground,
		SmallDetails,
		MediumDetails,
		Objects,
		Shadows
	}

	private sealed class ThreadGenerationContext
	{
		private readonly RubyGen _owner;

		public FastNoiseLite GroundNoise;
		public FastNoiseLite DensityNoise;
		public FastNoiseLite MediumDetailsNoise;
		public FastNoiseLite TreeNoise;
		public FastNoiseLite LakeNoise;

		private readonly List<Vector2I> _treePositions = new();

		public ThreadGenerationContext(RubyGen owner)
		{
			_owner = owner;
		}

		public void SetupNoises()
		{
			GroundNoise = new FastNoiseLite
			{
				Seed = _owner.SeedValue,
				NoiseType = _owner.GroundNoiseType,
				Frequency = _owner.GroundFrequency
			};

			DensityNoise = new FastNoiseLite
			{
				Seed = _owner.SeedValue + 1000,
				NoiseType = _owner.DensityNoiseType,
				Frequency = _owner.DensityFrequency
			};

			MediumDetailsNoise = new FastNoiseLite
			{
				Seed = _owner.SeedValue + 3000,
				NoiseType = _owner.MediumDetailsNoiseType,
				Frequency = _owner.MediumDetailsFrequency
			};

			TreeNoise = new FastNoiseLite
			{
				Seed = _owner.SeedValue + 4000,
				NoiseType = _owner.TreeNoiseType,
				Frequency = _owner.TreeNoiseFrequency
			};

			LakeNoise = new FastNoiseLite
			{
				Seed = _owner.SeedValue + 5000,
				NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin,
				Frequency = _owner.LakeNoiseFrequency
			};
		}

		public void GenerateLakesPass(RubyThreadWorldData world)
		{
			int halfX = _owner.LevelSizeX / 2;
			int halfY = _owner.LevelSizeY / 2;

			for (int y = -halfY; y < halfY; y++)
			{
				for (int x = -halfX; x < halfX; x++)
				{
					Vector2I cell = new(x, y);

					if (!IsGroundFamilyCell(world, cell))
						continue;

					if (IsBorderRealTerrainCell(world, cell, 3))
						continue;

					float lakeNoise = GetNoise01(LakeNoise, x, y);

					if (lakeNoise < _owner.LakeThreshold)
						continue;

					bool nearNonLakeGround = false;

					for (int oy = -_owner.LakeEdgeDepth; oy <= _owner.LakeEdgeDepth; oy++)
					{
						for (int ox = -_owner.LakeEdgeDepth; ox <= _owner.LakeEdgeDepth; ox++)
						{
							Vector2I check = cell + new Vector2I(ox, oy);

							if (!IsInsideBounds(check))
							{
								nearNonLakeGround = true;
								break;
							}

							if (!IsGroundFamilyCell(world, check))
							{
								nearNonLakeGround = true;
								break;
							}

							float otherLakeNoise = GetNoise01(LakeNoise, check.X, check.Y);
							if (otherLakeNoise < _owner.LakeThreshold)
							{
								nearNonLakeGround = true;
								break;
							}
						}

						if (nearNonLakeGround)
							break;
					}

					world.Ground[cell] = nearNonLakeGround ? _owner.WaterAtlas : _owner.DeepWaterAtlas;
				}
			}
		}

		public void GenerateBeachPass(RubyThreadWorldData world)
		{
			int halfX = _owner.LevelSizeX / 2;
			int halfY = _owner.LevelSizeY / 2;

			List<Vector2I> cellsToSand = new();

			for (int y = -halfY; y < halfY; y++)
			{
				for (int x = -halfX; x < halfX; x++)
				{
					Vector2I cell = new(x, y);

					if (!IsGroundFamilyCell(world, cell) && !IsDirtCell(world, cell))
						continue;

					if (!IsNearWater(world, cell, _owner.BeachBorderDistance))
						continue;

					float roll = RandomFloat01FromPosition(x, y, 6060);

					if (roll > _owner.BeachChance)
						continue;

					cellsToSand.Add(cell);
				}
			}

			foreach (Vector2I cell in cellsToSand)
				world.Ground[cell] = _owner.SandAtlas;
		}

		public void GenerateVoidBorderPass(RubyThreadWorldData world)
		{
			int halfX = _owner.LevelSizeX / 2;
			int halfY = _owner.LevelSizeY / 2;

			for (int y = -halfY; y < halfY; y++)
			{
				for (int x = -halfX; x < halfX; x++)
				{
					Vector2I cell = new(x, y);

					if (IsRealTerrainCell(world, cell))
						continue;

					if (!HasNeighborRealTerrain(world, cell, _owner.VoidBorderThickness))
						continue;

					world.Ground[cell] = _owner.VoidAtlas;
				}
			}
		}

		public void GenerateSmallDetailsPass(RubyThreadWorldData world)
		{
			if (_owner.SmallDetailAtlases.Count == 0)
				return;

			int halfX = _owner.LevelSizeX / 2;
			int halfY = _owner.LevelSizeY / 2;

			for (int y = -halfY; y < halfY; y++)
			{
				for (int x = -halfX; x < halfX; x++)
				{
					Vector2I cell = new(x, y);

					if (!IsGroundMainCell(world, cell))
						continue;

					if (world.MediumDetails.ContainsKey(cell) || world.Objects.ContainsKey(cell))
						continue;

					float density = GetNoise01(DensityNoise, x, y);
					float spawnChance = _owner.SmallDetailBaseChance + (density * _owner.SmallDetailDensityChance);
					float roll = RandomFloat01FromPosition(x, y, 5551);

					if (roll > spawnChance)
						continue;

					Vector2I atlas = PickAtlasFromList(_owner.SmallDetailAtlases, x, y, 7777);
					world.SmallDetails[cell] = atlas;
				}
			}
		}

		public void GenerateMediumDetailsPass(RubyThreadWorldData world)
		{
			if (_owner.MediumDetailAtlases.Count == 0)
				return;

			int halfX = _owner.LevelSizeX / 2;
			int halfY = _owner.LevelSizeY / 2;

			for (int y = -halfY; y < halfY; y++)
			{
				for (int x = -halfX; x < halfX; x++)
				{
					Vector2I cell = new(x, y);

					if (!IsGroundMainCell(world, cell))
						continue;

					if (world.Objects.ContainsKey(cell))
						continue;

					float density = GetNoise01(DensityNoise, x, y);
					float region = GetNoise01(MediumDetailsNoise, x, y);

					float spawnChance =
						_owner.MediumDetailBaseChance +
						(density * _owner.MediumDetailDensityChance) +
						(region * _owner.MediumDetailRegionChance);

					float roll = RandomFloat01FromPosition(x, y, 9127);

					if (roll > spawnChance)
						continue;

					if (HasNeighborMediumDetail(world, cell, 1))
						continue;

					Vector2I atlas = PickAtlasFromList(_owner.MediumDetailAtlases, x, y, 9999);
					world.MediumDetails[cell] = atlas;
				}
			}
		}

		public void GenerateEdgeDecorationsPass(RubyThreadWorldData world)
		{
			if (!_owner.GenerateEdgeDecorations || _owner.EdgeDecorAtlases.Count == 0)
				return;

			int halfX = _owner.LevelSizeX / 2;
			int halfY = _owner.LevelSizeY / 2;

			for (int y = -halfY; y < halfY; y++)
			{
				for (int x = -halfX; x < halfX; x++)
				{
					Vector2I cell = new(x, y);

					if (!IsGroundMainCell(world, cell))
						continue;

					if (!IsNearDirt(world, cell, _owner.EdgeDecorRadius))
						continue;

					if (world.Objects.ContainsKey(cell))
						continue;

					if (world.MediumDetails.ContainsKey(cell))
						continue;

					float roll = RandomFloat01FromPosition(x, y, 45454);

					if (roll > _owner.EdgeDecorChance)
						continue;

					Vector2I atlas = PickAtlasFromList(_owner.EdgeDecorAtlases, x, y, 56565);
					world.MediumDetails[cell] = atlas;
				}
			}
		}

		public void GenerateTreesPass(RubyThreadWorldData world)
		{
			if (_owner.TreeAtlases.Count == 0)
				return;

			int halfX = _owner.LevelSizeX / 2;
			int halfY = _owner.LevelSizeY / 2;
			int safeTreeStep = Mathf.Max(1, _owner.TreeStep);

			for (int y = -halfY; y < halfY; y += safeTreeStep)
			{
				for (int x = -halfX; x < halfX; x += safeTreeStep)
				{
					Vector2I baseCell = new(x, y);
					Vector2I offsetCell = ApplyTreeRandomOffset(baseCell);

					if (!HasEnoughSpaceForTree(world, offsetCell))
						continue;

					if (!IsGroundMainCell(world, offsetCell))
						continue;

					float density = GetNoise01(DensityNoise, offsetCell.X, offsetCell.Y);
					float noise = GetNoise01(TreeNoise, offsetCell.X, offsetCell.Y);
					float roll = RandomFloat01FromPosition(offsetCell.X, offsetCell.Y, 17171);

					float spawnChance = (1f - _owner.TreeThreshold) + (density * _owner.TreeDensityInfluence) + _owner.TreeBaseChanceBonus;

					if (noise < _owner.TreeThreshold && roll > spawnChance)
						continue;

					if (IsNearAnotherTree(offsetCell, _owner.TreeMinDistance))
						continue;

					Vector2I treeAtlas = PickAtlasFromList(_owner.TreeAtlases, offsetCell.X, offsetCell.Y, 22222);

					world.Objects[offsetCell] = treeAtlas;
					world.Shadows[offsetCell + _owner.ShadowOffset] = _owner.TreeShadowAtlas;

					_treePositions.Add(offsetCell);
				}
			}
		}

		public bool IsGroundCell(RubyThreadWorldData world, Vector2I cell)
		{
			float groundValue = GetNoise01(GroundNoise, cell.X, cell.Y);

			if (_owner.UseIslandMask)
			{
				float mask = GetIslandMask01(cell.X, cell.Y);
				groundValue *= mask;
			}

			return groundValue >= _owner.GroundThreshold;
		}

		public bool HasGround(RubyThreadWorldData world, Vector2I cell)
		{
			return world.Ground.ContainsKey(cell);
		}

		public bool IsGroundMainCell(RubyThreadWorldData world, Vector2I cell)
		{
			return world.Ground.TryGetValue(cell, out Vector2I atlas) && atlas == _owner.GroundMainAtlas;
		}

		public bool IsDirtCell(RubyThreadWorldData world, Vector2I cell)
		{
			if (!world.Ground.TryGetValue(cell, out Vector2I atlas))
				return false;

			foreach (Vector2I dirtAtlas in _owner.DirtAtlases)
			{
				if (atlas == dirtAtlas)
					return true;
			}

			return false;
		}

		public bool IsGroundFamilyCell(RubyThreadWorldData world, Vector2I cell)
		{
			if (!world.Ground.TryGetValue(cell, out Vector2I atlas))
				return false;

			return atlas == _owner.GroundMainAtlas
				|| atlas == _owner.GroundAlt1Atlas
				|| atlas == _owner.GroundAlt2Atlas;
		}

		public bool IsRealTerrainCell(RubyThreadWorldData world, Vector2I cell)
		{
			if (!world.Ground.TryGetValue(cell, out Vector2I atlas))
				return false;

			if (atlas == _owner.GroundMainAtlas || atlas == _owner.GroundAlt1Atlas || atlas == _owner.GroundAlt2Atlas)
				return true;

			if (atlas == _owner.SandAtlas || atlas == _owner.WaterAtlas || atlas == _owner.DeepWaterAtlas || atlas == _owner.VoidAtlas)
				return true;

			foreach (Vector2I dirtAtlas in _owner.DirtAtlases)
			{
				if (atlas == dirtAtlas)
					return true;
			}

			return false;
		}

		public bool IsNearWater(RubyThreadWorldData world, Vector2I cell, int radius = 1)
		{
			for (int y = -radius; y <= radius; y++)
			{
				for (int x = -radius; x <= radius; x++)
				{
					if (x == 0 && y == 0)
						continue;

					Vector2I check = cell + new Vector2I(x, y);

					if (!IsInsideBounds(check))
						continue;

					if (!world.Ground.TryGetValue(check, out Vector2I atlas))
						continue;

					if (atlas == _owner.WaterAtlas || atlas == _owner.DeepWaterAtlas)
						return true;
				}
			}

			return false;
		}

		public bool IsNearDirt(RubyThreadWorldData world, Vector2I cell, int radius = 1)
		{
			for (int y = -radius; y <= radius; y++)
			{
				for (int x = -radius; x <= radius; x++)
				{
					if (x == 0 && y == 0)
						continue;

					Vector2I check = cell + new Vector2I(x, y);

					if (!IsInsideBounds(check))
						continue;

					if (IsDirtCell(world, check))
						return true;
				}
			}

			return false;
		}

		public bool IsBorderRealTerrainCell(RubyThreadWorldData world, Vector2I cell, int radius = 1)
		{
			if (!IsRealTerrainCell(world, cell))
				return false;

			for (int y = -radius; y <= radius; y++)
			{
				for (int x = -radius; x <= radius; x++)
				{
					Vector2I check = cell + new Vector2I(x, y);

					if (!IsInsideBounds(check))
						return true;

					if (!IsRealTerrainCell(world, check))
						return true;
				}
			}

			return false;
		}

		public bool HasNeighborRealTerrain(RubyThreadWorldData world, Vector2I cell, int radius = 1)
		{
			for (int y = -radius; y <= radius; y++)
			{
				for (int x = -radius; x <= radius; x++)
				{
					if (x == 0 && y == 0)
						continue;

					Vector2I check = cell + new Vector2I(x, y);

					if (!IsInsideBounds(check))
						continue;

					if (IsRealTerrainCell(world, check) && world.Ground[check] != _owner.VoidAtlas)
						return true;
				}
			}

			return false;
		}

		public bool HasEnoughSpaceForTree(RubyThreadWorldData world, Vector2I centerCell)
		{
			for (int y = -_owner.TreePaddingCheckRadius; y <= _owner.TreePaddingCheckRadius; y++)
			{
				for (int x = -_owner.TreePaddingCheckRadius; x <= _owner.TreePaddingCheckRadius; x++)
				{
					Vector2I cell = centerCell + new Vector2I(x, y);

					if (!IsInsideBounds(cell))
						return false;

					if (!IsGroundMainCell(world, cell))
						return false;

					if (world.Objects.ContainsKey(cell))
						return false;
				}
			}

			return true;
		}

		public bool IsNearAnotherTree(Vector2I cell, int minDistance)
		{
			foreach (Vector2I treePos in _treePositions)
			{
				if (cell.DistanceTo(treePos) < minDistance)
					return true;
			}

			return false;
		}

		public bool HasNeighborMediumDetail(RubyThreadWorldData world, Vector2I cell, int radius = 1)
		{
			for (int y = -radius; y <= radius; y++)
			{
				for (int x = -radius; x <= radius; x++)
				{
					if (x == 0 && y == 0)
						continue;

					Vector2I checkCell = cell + new Vector2I(x, y);

					if (world.MediumDetails.ContainsKey(checkCell))
						return true;
				}
			}

			return false;
		}

		public bool IsInsideBounds(Vector2I cell)
		{
			int halfX = _owner.LevelSizeX / 2;
			int halfY = _owner.LevelSizeY / 2;

			return cell.X >= -halfX && cell.X < halfX && cell.Y >= -halfY && cell.Y < halfY;
		}

		public Vector2I ApplyTreeRandomOffset(Vector2I cell)
		{
			int offsetX = RandomRangeFromPosition(cell.X, cell.Y, _owner.SeedValue + 7000, -_owner.TreeRandomOffsetX, _owner.TreeRandomOffsetX);
			int offsetY = RandomRangeFromPosition(cell.X, cell.Y, _owner.SeedValue + 8000, -_owner.TreeRandomOffsetY, _owner.TreeRandomOffsetY);
			return cell + new Vector2I(offsetX, offsetY);
		}

		public Vector2I PickGroundAtlas(float groundValue)
		{
			if (groundValue >= _owner.GroundAlt2Threshold)
				return _owner.GroundAlt2Atlas;

			if (groundValue >= _owner.GroundAlt1Threshold)
				return _owner.GroundAlt1Atlas;

			return _owner.GroundMainAtlas;
		}

		public Vector2I PickAtlasFromList(Godot.Collections.Array<Vector2I> atlases, int x, int y, int salt)
		{
			if (atlases.Count == 0)
				return Vector2I.Zero;

			int index = RandomRangeFromPosition(x, y, salt, 0, atlases.Count - 1);
			return atlases[index];
		}

		public int RandomRangeFromPosition(int x, int y, int salt, int min, int max)
		{
			if (max <= min)
				return min;

			int hash = x * 73856093 ^ y * 19349663 ^ salt;
			hash = Math.Abs(hash);

			return min + (hash % (max - min + 1));
		}

		public float RandomFloat01FromPosition(int x, int y, int salt)
		{
			int hash = x * 73856093 ^ y * 19349663 ^ salt;
			hash = Math.Abs(hash);
			return (hash % 10000) / 10000.0f;
		}

		public float GetNoise01(FastNoiseLite noise, int x, int y)
		{
			return (noise.GetNoise2D(x, y) + 1f) * 0.5f;
		}

		public float GetIslandMask01(int x, int y)
		{
			float nx = (float)x / Mathf.Max(1f, _owner.LevelSizeX * 0.5f);
			float ny = (float)y / Mathf.Max(1f, _owner.LevelSizeY * 0.5f);

			float dist = Mathf.Pow(Mathf.Abs(nx), _owner.IslandRoundness) + Mathf.Pow(Mathf.Abs(ny), _owner.IslandRoundness);
			dist = Mathf.Clamp(dist, 0f, 1.5f);

			float mask = 1f - Mathf.Pow(Mathf.Clamp(dist, 0f, 1f), _owner.IslandFalloffPower);
			return Mathf.Clamp(mask, 0f, 1f);
		}
	}
}

public partial class RubyChunkData : RefCounted
{
	public List<RubyChunkTileData> GroundTiles = new();
	public List<RubyChunkTileData> SmallDetailTiles = new();
	public List<RubyChunkTileData> MediumDetailTiles = new();
	public List<RubyChunkTileData> ObjectTiles = new();
	public List<RubyChunkTileData> ShadowTiles = new();

	public List<Vector2I> ValidGroundCells = new();

	public bool SpawnEnemys;
	public int WaterCells;
	public int VoidCells;
}

public partial class RubyChunkTileData : RefCounted
{
	public Vector2I Cell;
	public Vector2I Atlas;

	public RubyChunkTileData()
	{
	}

	public RubyChunkTileData(Vector2I cell, Vector2I atlas)
	{
		Cell = cell;
		Atlas = atlas;
	}
}

public sealed class RubyThreadWorldData
{
	public Dictionary<Vector2I, Vector2I> Ground = new();
	public Dictionary<Vector2I, Vector2I> SmallDetails = new();
	public Dictionary<Vector2I, Vector2I> MediumDetails = new();
	public Dictionary<Vector2I, Vector2I> Objects = new();
	public Dictionary<Vector2I, Vector2I> Shadows = new();
}

public sealed class RubyThreadGenerationResult
{
	public Dictionary<Vector2I, RubyChunkData> Chunks = new();
}