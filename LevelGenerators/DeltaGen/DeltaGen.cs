using Godot;
using projecthorizonscs.Autoload;
using System.Collections.Generic;
using System.Linq;

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
	
	private TileMapLayer _ground;
	private TileMapLayer _detailsSmall;
	private TileMapLayer _detailsMedium;
	private TileMapLayer _objects;
	private TileMapLayer _shadows;

	private Vector2I _currentCenterChunk = new(int.MinValue, int.MinValue);
	private Dictionary<Vector2I, DeltaChunkData> _loadedChunks = new();
	private string _worldName = "World";
	private int _currentLevel = 0;

	public override void _Ready()
	{
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

		if (DataManager.I != null)
		{
			if (DataManager.I.CurrentWorldData.TryGetValue("SaveName", out Variant saveName) && saveName.AsString() != "")
				_worldName = saveName.AsString();

			if (DataManager.I.CurrentWorldData.TryGetValue("CurrentLevel", out Variant cLevel))
				_currentLevel = cLevel.AsInt32();
		}

		GD.Print($"DeltaGen: Ready. Streaming Map: {_worldName} | Level: {_currentLevel}");

		// Try immediate load if player is already in the scene
		if (Globals.I != null && Globals.I.LocalPlayer != null)
		{
			Vector2I playerCell = WorldToCell(Globals.I.LocalPlayer.GlobalPosition);
			Vector2I startChunk = WorldToChunk(playerCell);
			_currentCenterChunk = startChunk;
			GD.Print($"DeltaGen: Player found on Ready. Loading initial chunks around {startChunk}");
			UpdateVisibleChunks(startChunk);
		}
		else
		{
			GD.Print("DeltaGen: Player not found on Ready. Will load chunks once player is available.");
		}
	}

	public override void _Process(double delta)
	{
		if (Globals.I == null || Globals.I.LocalPlayer == null)
			return;

		Vector2I playerCell = WorldToCell(Globals.I.LocalPlayer.GlobalPosition);
		Vector2I newCenterChunk = WorldToChunk(playerCell);

		// _currentCenterChunk starts at int.MinValue — triggers load on first valid frame
		if (newCenterChunk != _currentCenterChunk)
		{
			GD.Print($"DeltaGen: Chunk center changed to {newCenterChunk}");
			_currentCenterChunk = newCenterChunk;
			UpdateVisibleChunks(newCenterChunk);
		}
	}

	private void UpdateVisibleChunks(Vector2I centerChunk)
	{
		HashSet<Vector2I> neededChunks = GetNeededChunks(centerChunk);
		int loaded = 0;

		foreach (Vector2I chunkCoord in neededChunks)
		{
			if (_loadedChunks.ContainsKey(chunkCoord))
				continue;

			LoadChunkFromDisk(chunkCoord);
			loaded++;
		}

		if (loaded > 0)
			GD.Print($"DeltaGen: Loaded {loaded} chunks around {centerChunk}");

		foreach (Vector2I chunkCoord in _loadedChunks.Keys.ToList())
		{
			if (neededChunks.Contains(chunkCoord))
				continue;

			UnloadChunk(chunkCoord);
		}
	}

	private HashSet<Vector2I> GetNeededChunks(Vector2I centerChunk)
	{
		HashSet<Vector2I> neededChunks = new();

		for (int y = -VerticalRenderRadius; y <= VerticalRenderRadius; y++)
		{
			for (int x = -HorizontalRenderRadius; x <= HorizontalRenderRadius; x++)
			{
				neededChunks.Add(new Vector2I(centerChunk.X + x, centerChunk.Y + y));
			}
		}

		return neededChunks;
	}

	private void LoadChunkFromDisk(Vector2I chunkCoord)
	{
		string chunkFile = $"user://saves/{_worldName}/level_{_currentLevel}/chunk_{chunkCoord.X}_{chunkCoord.Y}.dat";

		if (!FileAccess.FileExists(chunkFile))
		{
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
		if (chunk == null) return;
		if (_ground == null) return;

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
		if (!_loadedChunks.TryGetValue(chunkCoord, out DeltaChunkData chunk))
			return;

		_loadedChunks.Remove(chunkCoord);

		if (chunk == null) return; // Was an empty/void chunk placeholder

		if (_ground != null)
			foreach (Vector4I tile in chunk.GroundTiles) _ground.EraseCell(new Vector2I(tile.X, tile.Y));
		if (_detailsSmall != null)
			foreach (Vector4I tile in chunk.SmallDetailTiles) _detailsSmall.EraseCell(new Vector2I(tile.X, tile.Y));
		if (_detailsMedium != null)
			foreach (Vector4I tile in chunk.MediumDetailTiles) _detailsMedium.EraseCell(new Vector2I(tile.X, tile.Y));
		if (_objects != null)
			foreach (Vector4I tile in chunk.ObjectTiles) _objects.EraseCell(new Vector2I(tile.X, tile.Y));
		if (_shadows != null)
			foreach (Vector4I tile in chunk.ShadowTiles) _shadows.EraseCell(new Vector2I(tile.X, tile.Y));
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
		if (_ground == null) return Vector2I.Zero;
		Vector2 localToGround = _ground.ToLocal(globalPosition);
		return _ground.LocalToMap(localToGround);
	}
}
