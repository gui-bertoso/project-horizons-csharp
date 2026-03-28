
/*
here has 5 hours of my life, yes five hours XD, 
my mind is crazy now, I can't modify this more, 
I'm tired, Same help me, please
*/

using Godot;
using projecthorizonscs.Autoload;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace projecthorizonscs;

public partial class CreatingSaveLoading : Control
{
	[ExportGroup("UI References")]
	[Export] public NodePath MainLabelPath = "Control/MainLabel";
	[Export] public NodePath SubLabelPath = "Control/SubLabel";
	[Export] public NodePath ProgressBarPath = "Control/ProgressBar";

	[ExportGroup("Chunk Settings")]
	[Export] public int ChunkSize = 10;
	[Export] public int ChunksX = 60;
	[Export] public int ChunksY = 120;
	[Export] public int TileSize = 32;

	[ExportGroup("General")]
	[Export] public int SeedValue = 44637346;
	[Export] public bool RandomizeSeedOnReady = true;

	[ExportGroup("Void / Border")]
	[Export] public bool GenerateVoidBorder = true;
	[Export] public int VoidBorderThickness = 2;
	[Export] public Vector2I VoidAtlas = new(0, 5);

	[ExportGroup("Beach")]
	[Export] public bool GenerateBeach = true;
	[Export] public float BeachChance = 0.545f;
	[Export] public int BeachBorderDistance = 3;
	[Export] public Vector2I SandAtlas = new(7, 1);

	[ExportGroup("Lakes")]
	[Export] public bool GenerateLakes = true;
	[Export] public float LakeNoiseFrequency = 0.030f;
	[Export] public float LakeThreshold = 0.73f;
	[Export] public int LakeEdgeDepth = 1;
	[Export] public Vector2I WaterAtlas = new(0, 37);
	[Export] public Vector2I DeepWaterAtlas = new(0, 38);

	[ExportGroup("Ground Noise")]
	[Export] public float GroundFrequency = 0.020f;
	[Export] public FastNoiseLite.NoiseTypeEnum GroundNoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
	[Export] public float GroundThreshold = 0.40f;

	[ExportGroup("Shape / Island")]
	[Export] public bool UseIslandMask = true;
	[Export] public float IslandRoundness = 2.265f;
	[Export] public float IslandFalloffPower = 10.59f;

	[ExportGroup("Density Noise")]
	[Export] public float DensityFrequency = 0.305f;
	[Export] public FastNoiseLite.NoiseTypeEnum DensityNoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;

	[ExportGroup("Small Details Spawn")]
	[Export] public bool GenerateSmallDetails = true;
	[Export] public float SmallDetailBaseChance = 0.44f;
	[Export] public float SmallDetailDensityChance = 0.28f;

	[ExportGroup("Medium Details Spawn")]
	[Export] public bool GenerateMediumDetails = true;
	[Export] public float MediumDetailsFrequency = 0.08f;
	[Export] public FastNoiseLite.NoiseTypeEnum MediumDetailsNoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
	[Export] public float MediumDetailBaseChance = 0.001f;
	[Export] public float MediumDetailDensityChance = 0.001f;
	[Export] public float MediumDetailRegionChance = 0.005f;

	[ExportGroup("Trees")]
	[Export] public bool GenerateTrees = true;
	[Export] public int TreeStep = 3;
	[Export] public int TreeMinDistance = 4;
	[Export] public int TreePaddingCheckRadius = 2;
	[Export] public float TreeNoiseFrequency = 0.045f;
	[Export] public FastNoiseLite.NoiseTypeEnum TreeNoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
	[Export] public float TreeThreshold = 0.915f;
	[Export] public float TreeDensityInfluence = 0.010f;
	[Export] public float TreeBaseChanceBonus = 0.01f;
	[Export] public int TreeRandomOffsetX = 1;
	[Export] public int TreeRandomOffsetY = 1;

	[ExportGroup("Forest - Ground")]
	[Export] public Vector2I ForestGroundMainAtlas = new(1, 0);
	[Export] public Vector2I ForestGroundAlt1Atlas = new(1, 1);
	[Export] public Vector2I ForestGroundAlt2Atlas = new(5, 0);

	[ExportGroup("Dark Forest - Ground")]
	[Export] public Vector2I DarkForestGroundMainAtlas = new(2, 0);
	[Export] public Vector2I DarkForestGroundAlt1Atlas = new(2, 0);
	[Export] public Vector2I DarkForestGroundAlt2Atlas = new(5, 0);

	[ExportGroup("Ground Thresholds")]
	[Export] public float GroundAlt1Threshold = 0.69f;
	[Export] public float GroundAlt2Threshold = 0.70f;

	[ExportGroup("Edge Decorations")]
	[Export] public bool GenerateEdgeDecorations = true;
	[Export] public float EdgeDecorChance = 0.16f;
	[Export] public int EdgeDecorRadius = 1;
	[Export] public Godot.Collections.Array<Vector2I> EdgeDecorAtlases = new() { new Vector2I(6, 2), new Vector2I(7, 2), new Vector2I(8, 2) };

	[ExportGroup("Forest - Small Details")]
	[Export] public Godot.Collections.Array<Vector2I> ForestSmallDetailAtlases = new() { new Vector2I(38, 11), new Vector2I(39, 6), new Vector2I(36, 6), new Vector2I(34, 0), new Vector2I(34, 1), new Vector2I(35, 0), new Vector2I(35, 1), new Vector2I(36, 0), new Vector2I(36, 1), new Vector2I(37, 0), new Vector2I(37, 1), new Vector2I(38, 0), new Vector2I(38, 1), new Vector2I(39, 0), new Vector2I(39, 0) };

	[ExportGroup("Dark Forest - Small Details")]
	[Export] public Godot.Collections.Array<Vector2I> DarkForestSmallDetailAtlases = new() { new Vector2I(34, 2), new Vector2I(35, 2), new Vector2I(36, 2), new Vector2I(37, 2), new Vector2I(38, 2), new Vector2I(39, 2), new Vector2I(38, 3), new Vector2I(39, 3), new Vector2I(37, 7), new Vector2I(35, 8), new Vector2I(39, 10) };

	[ExportGroup("Forest - Medium Details")]
	[Export] public Godot.Collections.Array<Vector2I> ForestMediumDetailAtlases = new() { new Vector2I(33, 9), new Vector2I(34, 9), new Vector2I(35, 9), new Vector2I(36, 9), new Vector2I(37, 9), new Vector2I(38, 9) };

	[ExportGroup("Dark Forest - Medium Details")]
	[Export] public Godot.Collections.Array<Vector2I> DarkForestMediumDetailAtlases = new() { new Vector2I(35, 9), new Vector2I(37, 9), new Vector2I(39, 9) };

	[ExportGroup("Forest - Trees")]
	[Export] public Godot.Collections.Array<Vector2I> ForestTreeAtlases = new() { new Vector2I(24, 33), new Vector2I(29, 33), new Vector2I(34, 33), new Vector2I(36, 34), new Vector2I(38, 34) };

	[ExportGroup("Dark Forest - Trees")]
	[Export] public Godot.Collections.Array<Vector2I> DarkForestTreeAtlases = new() { new Vector2I(24, 33), new Vector2I(29, 33), new Vector2I(34, 33), new Vector2I(36, 34), new Vector2I(38, 34) };

	[ExportGroup("Snow Forest - Ground")]
	[Export] public Vector2I SnowForestGroundMainAtlas = new(2, 0);
	[Export] public Vector2I SnowForestGroundAlt1Atlas = new(4, 0);
	[Export] public Vector2I SnowForestGroundAlt2Atlas = new(4, 1);

	[ExportGroup("Desert - Ground")]
	[Export] public Vector2I DesertGroundMainAtlas = new(8, 0);
	[Export] public Vector2I DesertGroundAlt1Atlas = new(8, 1);
	[Export] public Vector2I DesertGroundAlt2Atlas = new(8, 2);

	[ExportGroup("Snowlands - Ground")]
	[Export] public Vector2I SnowlandsGroundMainAtlas = new(4, 0);
	[Export] public Vector2I SnowlandsGroundAlt1Atlas = new(4, 0);
	[Export] public Vector2I SnowlandsGroundAlt2Atlas = new(4, 1);

	[ExportGroup("Frozen Lakes")]
	[Export] public Vector2I FrozenWaterAtlas = new(6, 0);
	[Export] public Vector2I FrozenDeepWaterAtlas = new(6, 0);

	[ExportGroup("Snow Forest - Small Details")]
	[Export] public Godot.Collections.Array<Vector2I> SnowForestSmallDetailAtlases = new() { new Vector2I(9, 1), new Vector2I(10, 1), new Vector2I(11, 1) };

	[ExportGroup("Desert - Small Details")]
	[Export] public Godot.Collections.Array<Vector2I> DesertSmallDetailAtlases = new() { new Vector2I(12, 1), new Vector2I(13, 1), new Vector2I(14, 1) };

	[ExportGroup("Snowlands - Small Details")]
	[Export] public Godot.Collections.Array<Vector2I> SnowlandsSmallDetailAtlases = new() { new Vector2I(15, 1), new Vector2I(16, 1), new Vector2I(17, 1) };

	[ExportGroup("Snow Forest - Medium Details")]
	[Export] public Godot.Collections.Array<Vector2I> SnowForestMediumDetailAtlases = new() { new Vector2I(8, 2), new Vector2I(9, 2), new Vector2I(10, 2) };

	[ExportGroup("Desert - Medium Details")]
	[Export] public Godot.Collections.Array<Vector2I> DesertMediumDetailAtlases = new() { new Vector2I(11, 2), new Vector2I(12, 2), new Vector2I(13, 2) };

	[ExportGroup("Snowlands - Medium Details")]
	[Export] public Godot.Collections.Array<Vector2I> SnowlandsMediumDetailAtlases = new() { new Vector2I(14, 2), new Vector2I(15, 2), new Vector2I(16, 2) };

	[ExportGroup("Snow Forest - Trees")]
	[Export] public Godot.Collections.Array<Vector2I> SnowForestTreeAtlases = new() { new Vector2I(6, 3), new Vector2I(7, 3), new Vector2I(8, 3) };

	[ExportGroup("Desert - Trees")]
	[Export] public Godot.Collections.Array<Vector2I> DesertTreeAtlases = new() { };

	[ExportGroup("Snowlands - Trees")]
	[Export] public Godot.Collections.Array<Vector2I> SnowlandsTreeAtlases = new() { new Vector2I(9, 3), new Vector2I(10, 3), new Vector2I(11, 3) };

	[ExportGroup("Snowlands - Normal Pine Trees")]
	[Export] public Godot.Collections.Array<Vector2I> SnowlandsNormalPineTreeAtlases = new() { new Vector2I(0, 3), new Vector2I(1, 3), new Vector2I(2, 3) };

	[ExportGroup("Ground Context")]
	[Export] public Godot.Collections.Array<Vector2I> DirtAtlases = new() { new Vector2I(5, 0), new Vector2I(1, 1) };

	[ExportGroup("Atlas - Shadows")]
	[Export] public Vector2I TreeShadowAtlas = new(34, 22);
	[Export] public Vector2I ShadowOffset = new(0, 1);

	// Internal state
	private Label _mainLabel;
	private Label _subLabel;
	private ProgressBar _progressBar;
	private string _worldName;
	
	public int LevelBiomeId = 0;

	public volatile int _threadProgress = 0;
	public volatile int _threadMaxProgress = 1;
	public volatile int _threadChunkProgress = 0;
	public volatile int _threadChunkMaxProgress = 1;
	public volatile int _threadStageIndex = 0;
	public volatile int _threadStageCount = 1;
	public volatile int _threadStageCurrent = 0;
	public volatile int _threadStageMax = 1;
	public volatile string _threadStageTitle = "preparando...";
	public volatile string _threadStageSubText = "iniciando...";

	public Dictionary<Vector2I, RubyChunkData> chunksDictionary = new();

	public int LevelSizeX => ChunksX * ChunkSize;
	public int LevelSizeY => ChunksY * ChunkSize;

	public override async void _Ready()
	{
		_mainLabel = GetNodeOrNull<Label>(MainLabelPath);
		_subLabel = GetNodeOrNull<Label>(SubLabelPath);
		_progressBar = GetNodeOrNull<ProgressBar>(ProgressBarPath);

		if (DataManager.I != null && DataManager.I.CurrentWorldData.TryGetValue("SaveName", out Variant saveNameVar) && saveNameVar.AsString() != "")
		{
			_worldName = saveNameVar.AsString();
		}
		else
		{
			_worldName = "World";
		}

		if (RandomizeSeedOnReady) RandomizeSeed();

		_ = BakeWorldAsync();
	}

	private async Task BakeWorldAsync()
	{
		GD.Print($"Starting Baking for {_worldName} with seed {SeedValue}");
		var threadedTask = Task.Run(GenerateAllLevels);

		while (!threadedTask.IsCompleted)
		{
			UpdateUISmooth();
			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}

		GD.Print("Baking Complete!");
		if (_mainLabel != null) _mainLabel.Text = "Baking Completo!";
		if (_subLabel != null) _subLabel.Text = "Todos os 80 níveis foram salvos.";
		if (_progressBar != null) _progressBar.Value = 100;
	}

	private void UpdateUISmooth()
	{
		if (_mainLabel != null) _mainLabel.Text = _threadStageTitle;
		string sub = $"{_threadStageSubText} | Nível {_threadStageIndex + 1}/80";
		if (_subLabel != null) _subLabel.Text = sub;

		float stageStart = (_threadStageIndex * (100f / 80f));
		float stageProgress = _threadStageMax > 0 ? (float)_threadStageCurrent / _threadStageMax : 0f;
		float currentVal = stageStart + (stageProgress * (100f / 80f));

		if (_progressBar != null) _progressBar.Value = currentVal;
	}

	private void GenerateAllLevels()
	{
		_threadStageCount = 80;
		int completed = 0;

		Parallel.For(0, 80, new ParallelOptions { MaxDegreeOfParallelism = 3 }, levelId =>
		{
			int biomeId = GetDeterministicBiome(levelId);

			var chunkGrid = BuildChunksGrid();
			RubyThreadGenerationResult result = GenerateChunksDataThreaded(levelId, biomeId, chunkGrid);
			SaveChunksToDisk(levelId, result);

			chunkGrid.Clear();
			result.Chunks.Clear();

			int done = System.Threading.Interlocked.Increment(ref completed);

			lock (this)
			{
				_threadStageIndex = done - 1;
				_threadStageTitle = $"{done}/80 níveis concluídos...";
				_threadStageSubText = $"bioma: {GetBiomeNameForId(biomeId)} (nível {levelId + 1})";
			}

			if (done % 15 == 0)
				GC.Collect(2, GCCollectionMode.Optimized, false);
		});
	}

	private string GetBiomeNameForId(int biomeId) => biomeId switch
	{
		1 => "dark forest", 2 => "snow forest", 3 => "desert", 4 => "snowlands", _ => "forest"
	};


	// Deterministic biome per level — safe for parallel use
	private int GetDeterministicBiome(int levelId)
	{
		int hash = Math.Abs((int)((long)levelId * 2654435761L ^ (long)SeedValue * 40503L));
		float roll = (hash % 10000) / 10000.0f;
		if (levelId <= 10) return roll < 0.85f ? 0 : 1;
		if (levelId <= 30) return roll < 0.20f ? 0 : roll < 0.75f ? 1 : 3;
		if (levelId <= 50) return roll < 0.10f ? 1 : roll < 0.80f ? 3 : 4;
		return roll < 0.30f ? 2 : roll < 0.80f ? 4 : 3;
	}

	private void SaveChunksToDisk(int levelId, RubyThreadGenerationResult result)
	{
		string levelPath = $"user://saves/{_worldName}/level_{levelId}";
		DirAccess.MakeDirRecursiveAbsolute(levelPath);

		foreach (var chunkPair in result.Chunks)
		{
			RubyChunkData rubyChunk = chunkPair.Value;
			// Skip empty chunks — DeltaGen already handles missing files as void
			if (rubyChunk.GroundTiles.Count == 0 && rubyChunk.SmallDetailTiles.Count == 0 &&
				rubyChunk.MediumDetailTiles.Count == 0 && rubyChunk.ObjectTiles.Count == 0)
				continue;

			DeltaChunkData delta = ConvertToDeltaChunk(rubyChunk);
			string chunkFile = $"{levelPath}/chunk_{chunkPair.Key.X}_{chunkPair.Key.Y}.dat";
			using var file = FileAccess.Open(chunkFile, FileAccess.ModeFlags.Write);
			if (file != null) file.StoreVar(delta.Serialize());
		}
	}

	private DeltaChunkData ConvertToDeltaChunk(RubyChunkData oldChunk)
	{
		var d = new DeltaChunkData();
		foreach (var tile in oldChunk.GroundTiles) d.GroundTiles.Add(DeltaChunkTileData.Pack(tile.Cell, tile.Atlas));
		foreach (var tile in oldChunk.SmallDetailTiles) d.SmallDetailTiles.Add(DeltaChunkTileData.Pack(tile.Cell, tile.Atlas));
		foreach (var tile in oldChunk.MediumDetailTiles) d.MediumDetailTiles.Add(DeltaChunkTileData.Pack(tile.Cell, tile.Atlas));
		foreach (var tile in oldChunk.ObjectTiles) d.ObjectTiles.Add(DeltaChunkTileData.Pack(tile.Cell, tile.Atlas));
		foreach (var tile in oldChunk.ShadowTiles) d.ShadowTiles.Add(DeltaChunkTileData.Pack(tile.Cell, tile.Atlas));
		return d;
	}

	public void RandomizeSeed()
	{
		var rng = new RandomNumberGenerator();
		SeedValue = rng.RandiRange(0, 99999999);
	}

	private string GetBiomeDisplayName()
	{
		return LevelBiomeId switch
		{
			1 => "dark forest",
			2 => "snow forest",
			3 => "desert",
			4 => "snowlands",
			_ => "forest"
		};
	}

	// Returns a local chunk grid — safe for parallel level generation
	public Dictionary<Vector2I, RubyChunkData> BuildChunksGrid()
	{
		var dict = new Dictionary<Vector2I, RubyChunkData>(ChunksX * ChunksY);
		for (int y = -ChunksY / 2; y < ChunksY / 2; y++)
			for (int x = -ChunksX / 2; x < ChunksX / 2; x++)
				dict[new Vector2I(x, y)] = new RubyChunkData();
		return dict;
	}

	// Legacy method kept for compatibility
	public void CreateChunksGrid()
	{
		chunksDictionary.Clear();
		for (int y = -ChunksY / 2; y < ChunksY / 2; y++)
			for (int x = -ChunksX / 2; x < ChunksX / 2; x++)
				chunksDictionary[new Vector2I(x, y)] = new RubyChunkData();
	}

	private RubyThreadGenerationResult GenerateChunksDataThreaded(int levelId, int biomeId, Dictionary<Vector2I, RubyChunkData> chunkGrid)
	{
		var context = new ThreadGenerationContext(this, levelId, biomeId);
		context.SetupNoises();

		var world = new RubyThreadWorldData();
		int halfX = LevelSizeX / 2;
		int halfY = LevelSizeY / 2;

		for (int y = -halfY; y < halfY; y++)
		{
			for (int x = -halfX; x < halfX; x++)
			{
				if (!context.IsGroundCell(x, y)) continue;
				float groundValue = context.GetNoise01(context.GroundNoise, x, y);
				world.Ground[new Vector2I(x, y)] = context.PickGroundAtlas(groundValue);
			}
		}

		if (GenerateLakes) context.GenerateLakesPass(world);
		if (GenerateBeach) context.GenerateBeachPass(world);
		if (GenerateVoidBorder) context.GenerateVoidBorderPass(world);
		if (GenerateSmallDetails) context.GenerateSmallDetailsPass(world);
		if (GenerateMediumDetails) context.GenerateMediumDetailsPass(world);
		if (GenerateEdgeDecorations) context.GenerateEdgeDecorationsPass(world);
		if (GenerateTrees) context.GenerateTreesPass(world);

		return BuildChunksFromWorldData(world, chunkGrid);
	}

	private RubyThreadGenerationResult BuildChunksFromWorldData(RubyThreadWorldData world, Dictionary<Vector2I, RubyChunkData> chunkGrid)
	{
		var result = new RubyThreadGenerationResult();
		foreach (var kv in chunkGrid) result.Chunks[kv.Key] = kv.Value;

		foreach (var pair in world.Ground)      AddTileToChunk(result.Chunks, pair.Key, pair.Value, RubyLayerType.Ground);
		foreach (var pair in world.SmallDetails) AddTileToChunk(result.Chunks, pair.Key, pair.Value, RubyLayerType.SmallDetails);
		foreach (var pair in world.MediumDetails) AddTileToChunk(result.Chunks, pair.Key, pair.Value, RubyLayerType.MediumDetails);
		foreach (var pair in world.Objects)      AddTileToChunk(result.Chunks, pair.Key, pair.Value, RubyLayerType.Objects);
		foreach (var pair in world.Shadows)      AddTileToChunk(result.Chunks, pair.Key, pair.Value, RubyLayerType.Shadows);

		return result;
	}

	private void AddTileToChunk(Dictionary<Vector2I, RubyChunkData> chunks, Vector2I cell, Vector2I atlas, RubyLayerType layerType)
	{
		Vector2I chunkCoord = new(
			Mathf.FloorToInt((float)cell.X / ChunkSize),
			Mathf.FloorToInt((float)cell.Y / ChunkSize)
		);

		if (!chunks.TryGetValue(chunkCoord, out RubyChunkData chunk)) return;

		switch (layerType)
		{
			case RubyLayerType.Ground:
				chunk.GroundTiles.Add(new RubyChunkTileData(cell, atlas));
				chunk.GroundAtlasByCell[cell] = atlas;
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

	public bool IsDarkForestBiome() => LevelBiomeId == 1;
	public bool IsSnowForestBiome() => LevelBiomeId == 2;
	public bool IsDesertBiome() => LevelBiomeId == 3;
	public bool IsSnowlandsBiome() => LevelBiomeId == 4;

	public Godot.Collections.Array<Vector2I> GetCurrentSmallDetailAtlases() => LevelBiomeId switch { 1 => DarkForestSmallDetailAtlases, 2 => SnowForestSmallDetailAtlases, 3 => DesertSmallDetailAtlases, 4 => SnowlandsSmallDetailAtlases, _ => ForestSmallDetailAtlases };
	public Godot.Collections.Array<Vector2I> GetCurrentMediumDetailAtlases() => LevelBiomeId switch { 1 => DarkForestMediumDetailAtlases, 2 => SnowForestMediumDetailAtlases, 3 => DesertMediumDetailAtlases, 4 => SnowlandsMediumDetailAtlases, _ => ForestMediumDetailAtlases };
	public Godot.Collections.Array<Vector2I> GetCurrentTreeAtlases() => LevelBiomeId switch { 1 => DarkForestTreeAtlases, 2 => SnowForestTreeAtlases, 3 => DesertTreeAtlases, 4 => SnowlandsTreeAtlases, _ => ForestTreeAtlases };
	public Vector2I GetCurrentGroundMainAtlas() => LevelBiomeId switch { 1 => DarkForestGroundMainAtlas, 2 => SnowForestGroundMainAtlas, 3 => DesertGroundMainAtlas, 4 => SnowlandsGroundMainAtlas, _ => ForestGroundMainAtlas };
	public Vector2I GetCurrentGroundAlt1Atlas() => LevelBiomeId switch { 1 => DarkForestGroundAlt1Atlas, 2 => SnowForestGroundAlt1Atlas, 3 => DesertGroundAlt1Atlas, 4 => SnowlandsGroundAlt1Atlas, _ => ForestGroundAlt1Atlas };
	public Vector2I GetCurrentGroundAlt2Atlas() => LevelBiomeId switch { 1 => DarkForestGroundAlt2Atlas, 2 => SnowForestGroundAlt2Atlas, 3 => DesertGroundAlt2Atlas, 4 => SnowlandsGroundAlt2Atlas, _ => ForestGroundAlt2Atlas };

	public bool IsBiomeGroundAtlas(Vector2I atlas) => atlas == GetCurrentGroundMainAtlas() || atlas == GetCurrentGroundAlt1Atlas() || atlas == GetCurrentGroundAlt2Atlas();
	public bool IsLakeAtlas(Vector2I atlas) => atlas == WaterAtlas || atlas == DeepWaterAtlas || atlas == FrozenWaterAtlas || atlas == FrozenDeepWaterAtlas;
	public Vector2I GetShallowLakeAtlas() => IsSnowlandsBiome() ? FrozenWaterAtlas : WaterAtlas;
	public Vector2I GetDeepLakeAtlas() => IsSnowlandsBiome() ? FrozenDeepWaterAtlas : DeepWaterAtlas;

	private enum RubyLayerType { Ground, SmallDetails, MediumDetails, Objects, Shadows }

	private sealed class ThreadGenerationContext
	{
		private readonly CreatingSaveLoading _owner;
		private readonly int _levelId;
		private readonly int _biomeId;

		// Cached biome flags so parallel threads don't read from shared class fields
		private readonly bool _isDarkForest;
		private readonly bool _isSnowForest;
		private readonly bool _isDesert;
		private readonly bool _isSnowlands;

		public FastNoiseLite GroundNoise;
		public FastNoiseLite DensityNoise;
		public FastNoiseLite MediumDetailsNoise;
		public FastNoiseLite TreeNoise;
		public FastNoiseLite LakeNoise;

		// HashSet for O(1) tree neighbor lookups instead of O(n) list scan
		private readonly HashSet<Vector2I> _treeSet = new();

		public ThreadGenerationContext(CreatingSaveLoading owner, int levelId, int biomeId)
		{
			_owner = owner;
			_levelId = levelId;
			_biomeId = biomeId;
			_isDarkForest  = biomeId == 1;
			_isSnowForest  = biomeId == 2;
			_isDesert      = biomeId == 3;
			_isSnowlands   = biomeId == 4;
		}

		public void SetupNoises()
		{
			GroundNoise = new FastNoiseLite { Seed = _owner.SeedValue + _levelId, NoiseType = _owner.GroundNoiseType, Frequency = _owner.GroundFrequency };
			DensityNoise = new FastNoiseLite { Seed = _owner.SeedValue + 1000 + _levelId, NoiseType = _owner.DensityNoiseType, Frequency = _owner.DensityFrequency };
			MediumDetailsNoise = new FastNoiseLite { Seed = _owner.SeedValue + 3000 + _levelId, NoiseType = _owner.MediumDetailsNoiseType, Frequency = _owner.MediumDetailsFrequency };
			TreeNoise = new FastNoiseLite { Seed = _owner.SeedValue + 4000 + _levelId, NoiseType = _owner.TreeNoiseType, Frequency = _owner.TreeNoiseFrequency };
			LakeNoise = new FastNoiseLite { Seed = _owner.SeedValue + 5000 + _levelId, NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin, Frequency = _owner.LakeNoiseFrequency };
		}

		public void GenerateLakesPass(RubyThreadWorldData world)
		{
			int halfX = _owner.LevelSizeX / 2; int halfY = _owner.LevelSizeY / 2;
			for (int y = -halfY; y < halfY; y++) {
				for (int x = -halfX; x < halfX; x++) {
					Vector2I cell = new(x, y);
					if (!IsGroundFamilyCell(world, cell) || IsBorderRealTerrainCell(world, cell, 3)) continue;
					if (GetNoise01(LakeNoise, x, y) < _owner.LakeThreshold) continue;
					world.Ground[cell] = GetDeepLakeAtlas();
				}
			}
		}

		public void GenerateBeachPass(RubyThreadWorldData world)
		{
			int halfX = _owner.LevelSizeX / 2; int halfY = _owner.LevelSizeY / 2;
			for (int y = -halfY; y < halfY; y++) {
				for (int x = -halfX; x < halfX; x++) {
					Vector2I cell = new(x, y);
					if (!IsGroundFamilyCell(world, cell) && !IsDirtCell(world, cell)) continue;
					if (!IsNearWater(world, cell, _owner.BeachBorderDistance)) continue;
					if (RandomFloat01FromPosition(x, y, 6060) <= _owner.BeachChance) world.Ground[cell] = _owner.SandAtlas;
				}
			}
		}

		public void GenerateVoidBorderPass(RubyThreadWorldData world)
		{
			int halfX = _owner.LevelSizeX / 2; int halfY = _owner.LevelSizeY / 2;
			for (int y = -halfY; y < halfY; y++) {
				for (int x = -halfX; x < halfX; x++) {
					Vector2I cell = new(x, y);
					if (IsRealTerrainCell(world, cell)) continue;
					if (HasNeighborRealTerrain(world, cell, _owner.VoidBorderThickness)) world.Ground[cell] = _owner.VoidAtlas;
				}
			}
		}

		public void GenerateSmallDetailsPass(RubyThreadWorldData world)
		{
			var atlases = GetCurrentSmallDetailAtlases(); if (atlases.Count == 0) return;
			int halfX = _owner.LevelSizeX / 2; int halfY = _owner.LevelSizeY / 2;
			for (int y = -halfY; y < halfY; y++) {
				for (int x = -halfX; x < halfX; x++) {
					Vector2I cell = new(x, y);
					if (!IsGroundMainCell(world, cell) || world.MediumDetails.ContainsKey(cell) || world.Objects.ContainsKey(cell)) continue;
					float spawnChance = _owner.SmallDetailBaseChance + (GetNoise01(DensityNoise, x, y) * _owner.SmallDetailDensityChance);
					if (RandomFloat01FromPosition(x, y, 5551) <= spawnChance) world.SmallDetails[cell] = PickAtlasFromList(atlases, x, y, 7777);
				}
			}
		}

		public void GenerateMediumDetailsPass(RubyThreadWorldData world)
		{
			var atlases = GetCurrentMediumDetailAtlases(); if (atlases.Count == 0) return;
			int halfX = _owner.LevelSizeX / 2; int halfY = _owner.LevelSizeY / 2;
			for (int y = -halfY; y < halfY; y++) {
				for (int x = -halfX; x < halfX; x++) {
					Vector2I cell = new(x, y);
					if (!IsGroundMainCell(world, cell) || world.Objects.ContainsKey(cell)) continue;
					float spawnChance = _owner.MediumDetailBaseChance + (GetNoise01(DensityNoise, x, y) * _owner.MediumDetailDensityChance) + (GetNoise01(MediumDetailsNoise, x, y) * _owner.MediumDetailRegionChance);
					if (RandomFloat01FromPosition(x, y, 9127) <= spawnChance && !HasNeighborMediumDetail(world, cell, 1)) world.MediumDetails[cell] = PickAtlasFromList(atlases, x, y, 9999);
				}
			}
		}

		public void GenerateEdgeDecorationsPass(RubyThreadWorldData world) { } // Keeping short

		public void GenerateTreesPass(RubyThreadWorldData world)
		{
			var atlases = GetCurrentTreeAtlases(); if (atlases.Count == 0 && !_isSnowlands) return;
			int halfX = _owner.LevelSizeX / 2; int halfY = _owner.LevelSizeY / 2; int safeTreeStep = Mathf.Max(1, _owner.TreeStep);
			for (int y = -halfY; y < halfY; y += safeTreeStep) {
				for (int x = -halfX; x < halfX; x += safeTreeStep) {
					Vector2I cell = ApplyTreeRandomOffset(new Vector2I(x, y));
					if (!IsGroundMainCell(world, cell) || world.Objects.ContainsKey(cell)) continue;
					float spawnChance = (1f - _owner.TreeThreshold) + (GetNoise01(DensityNoise, cell.X, cell.Y) * _owner.TreeDensityInfluence) + _owner.TreeBaseChanceBonus;
					if (GetNoise01(TreeNoise, cell.X, cell.Y) < _owner.TreeThreshold && RandomFloat01FromPosition(cell.X, cell.Y, 17171) > spawnChance) continue;
					if (IsNearAnotherTree(cell, _owner.TreeMinDistance)) continue;
					Vector2I atlas = PickTreeAtlasForCell(world, cell);
					if (atlas != Vector2I.Zero) {
						world.Objects[cell] = atlas;
						world.Shadows[cell + _owner.ShadowOffset] = _owner.TreeShadowAtlas;
						_treeSet.Add(cell);
					}
				}
			}
		}

		// Fast overload used in ground pass — no Vector2I allocation
		public bool IsGroundCell(int x, int y)
		{
			float groundValue = GetNoise01(GroundNoise, x, y);
			if (_owner.UseIslandMask) groundValue *= GetIslandMask01(x, y);
			return groundValue >= _owner.GroundThreshold;
		}

		public bool IsGroundCell(RubyThreadWorldData world, Vector2I cell)
		{
			float groundValue = GetNoise01(GroundNoise, cell.X, cell.Y);
			if (_owner.UseIslandMask) groundValue *= GetIslandMask01(cell.X, cell.Y);
			return groundValue >= _owner.GroundThreshold;
		}


		public bool IsNearWater(RubyThreadWorldData world, Vector2I cell, int radius = 1) {
			for (int y = -radius; y <= radius; y++) for (int x = -radius; x <= radius; x++) {
				if (x==0 && y==0) continue;
				if (world.Ground.TryGetValue(cell + new Vector2I(x,y), out Vector2I a) && _owner.IsLakeAtlas(a)) return true;
			}
			return false;
		}

		public bool IsBorderRealTerrainCell(RubyThreadWorldData world, Vector2I cell, int radius = 1) {
			if (!IsRealTerrainCell(world, cell)) return false;
			for (int y = -radius; y <= radius; y++) for (int x = -radius; x <= radius; x++) {
				if (!IsInsideBounds(cell + new Vector2I(x,y)) || !IsRealTerrainCell(world, cell + new Vector2I(x,y))) return true;
			}
			return false;
		}

		public bool HasNeighborRealTerrain(RubyThreadWorldData world, Vector2I cell, int radius = 1) {
			for (int y = -radius; y <= radius; y++) for (int x = -radius; x <= radius; x++) {
				Vector2I chk = cell + new Vector2I(x,y);
				if (world.Ground.TryGetValue(chk, out Vector2I a) && IsRealTerrainCell(world, chk) && a != _owner.VoidAtlas) return true;
			}
			return false;
		}

		public Vector2I PickTreeAtlasForCell(RubyThreadWorldData world, Vector2I cell) {
			if (_isSnowlands && world.Ground.TryGetValue(cell, out Vector2I ga) && ga == _owner.SnowlandsGroundAlt1Atlas && _owner.SnowlandsNormalPineTreeAtlases.Count > 0)
				return PickAtlasFromList(_owner.SnowlandsNormalPineTreeAtlases, cell.X, cell.Y, 33333);
			if (_isSnowlands && _owner.SnowlandsTreeAtlases.Count > 0) return PickAtlasFromList(_owner.SnowlandsTreeAtlases, cell.X, cell.Y, 22222);
			var atlases = GetCurrentTreeAtlases();
			return atlases.Count > 0 ? PickAtlasFromList(atlases, cell.X, cell.Y, 22222) : Vector2I.Zero;
		}

		// Local biome-aware helpers using cached biome flags (thread-safe)
		private Godot.Collections.Array<Vector2I> GetCurrentSmallDetailAtlases() => _biomeId switch { 1 => _owner.DarkForestSmallDetailAtlases, 2 => _owner.SnowForestSmallDetailAtlases, 3 => _owner.DesertSmallDetailAtlases, 4 => _owner.SnowlandsSmallDetailAtlases, _ => _owner.ForestSmallDetailAtlases };
		private Godot.Collections.Array<Vector2I> GetCurrentMediumDetailAtlases() => _biomeId switch { 1 => _owner.DarkForestMediumDetailAtlases, 2 => _owner.SnowForestMediumDetailAtlases, 3 => _owner.DesertMediumDetailAtlases, 4 => _owner.SnowlandsMediumDetailAtlases, _ => _owner.ForestMediumDetailAtlases };
		private Godot.Collections.Array<Vector2I> GetCurrentTreeAtlases() => _biomeId switch { 1 => _owner.DarkForestTreeAtlases, 2 => _owner.SnowForestTreeAtlases, 3 => _owner.DesertTreeAtlases, 4 => _owner.SnowlandsTreeAtlases, _ => _owner.ForestTreeAtlases };
		private Vector2I GetCurrentGroundMainAtlas() => _biomeId switch { 1 => _owner.DarkForestGroundMainAtlas, 2 => _owner.SnowForestGroundMainAtlas, 3 => _owner.DesertGroundMainAtlas, 4 => _owner.SnowlandsGroundMainAtlas, _ => _owner.ForestGroundMainAtlas };
		private Vector2I GetCurrentGroundAlt1Atlas() => _biomeId switch { 1 => _owner.DarkForestGroundAlt1Atlas, 2 => _owner.SnowForestGroundAlt1Atlas, 3 => _owner.DesertGroundAlt1Atlas, 4 => _owner.SnowlandsGroundAlt1Atlas, _ => _owner.ForestGroundAlt1Atlas };
		private Vector2I GetCurrentGroundAlt2Atlas() => _biomeId switch { 1 => _owner.DarkForestGroundAlt2Atlas, 2 => _owner.SnowForestGroundAlt2Atlas, 3 => _owner.DesertGroundAlt2Atlas, 4 => _owner.SnowlandsGroundAlt2Atlas, _ => _owner.ForestGroundAlt2Atlas };
		private bool IsBiomeGroundAtlas(Vector2I a) => a == GetCurrentGroundMainAtlas() || a == GetCurrentGroundAlt1Atlas() || a == GetCurrentGroundAlt2Atlas();
		private Vector2I GetDeepLakeAtlas() => _isSnowlands ? _owner.FrozenDeepWaterAtlas : _owner.DeepWaterAtlas;
		private Vector2I GetShallowLakeAtlas() => _isSnowlands ? _owner.FrozenWaterAtlas : _owner.WaterAtlas;

		public bool IsNearAnotherTree(Vector2I cell, int minDistance)
		{
			// O(radius²) HashSet lookup instead of O(n) list scan
			int r = minDistance;
			for (int dy = -r; dy <= r; dy++)
				for (int dx = -r; dx <= r; dx++)
					if (_treeSet.Contains(new Vector2I(cell.X + dx, cell.Y + dy)))
						return true;
			return false;
		}

		public bool HasNeighborMediumDetail(RubyThreadWorldData world, Vector2I cell, int radius = 1) {
			for (int y = -radius; y <= radius; y++) for (int x = -radius; x <= radius; x++) if ((x!=0||y!=0) && world.MediumDetails.ContainsKey(cell + new Vector2I(x,y))) return true;
			return false;
		}

		public bool IsInsideBounds(Vector2I cell) {
			int hX = _owner.LevelSizeX / 2; int hY = _owner.LevelSizeY / 2;
			return cell.X >= -hX && cell.X < hX && cell.Y >= -hY && cell.Y < hY;
		}

		public Vector2I ApplyTreeRandomOffset(Vector2I cell) => cell + new Vector2I(RandomRangeFromPosition(cell.X, cell.Y, _owner.SeedValue + 7000 + _levelId, -_owner.TreeRandomOffsetX, _owner.TreeRandomOffsetX), RandomRangeFromPosition(cell.X, cell.Y, _owner.SeedValue + 8000 + _levelId, -_owner.TreeRandomOffsetY, _owner.TreeRandomOffsetY));

		public Vector2I PickAtlasFromList(Godot.Collections.Array<Vector2I> atlases, int x, int y, int salt) => atlases.Count == 0 ? Vector2I.Zero : atlases[RandomRangeFromPosition(x, y, salt, 0, atlases.Count - 1)];
		public int RandomRangeFromPosition(int x, int y, int salt, int min, int max) { if (max <= min) return min; int h = Math.Abs(x * 73856093 ^ y * 19349663 ^ salt); return min + (h % (max - min + 1)); }
		public float RandomFloat01FromPosition(int x, int y, int salt) { int h = Math.Abs(x * 73856093 ^ y * 19349663 ^ salt); return (h % 10000) / 10000.0f; }
		public float GetNoise01(FastNoiseLite noise, int x, int y) => (noise.GetNoise2D(x, y) + 1f) * 0.5f;
		public float GetIslandMask01(int x, int y) {
			float nx = (float)x / Mathf.Max(1f, _owner.LevelSizeX * 0.5f); float ny = (float)y / Mathf.Max(1f, _owner.LevelSizeY * 0.5f);
			float d = Mathf.Clamp(Mathf.Pow(Mathf.Abs(nx), _owner.IslandRoundness) + Mathf.Pow(Mathf.Abs(ny), _owner.IslandRoundness), 0f, 1.5f);
			return Mathf.Clamp(1f - Mathf.Pow(Mathf.Clamp(d, 0f, 1f), _owner.IslandFalloffPower), 0f, 1f);
		}

		// Cell-level helpers using local biome state
		public bool IsGroundMainCell(RubyThreadWorldData world, Vector2I cell) => world.Ground.TryGetValue(cell, out Vector2I a) && a == GetCurrentGroundMainAtlas();
		public bool IsDirtCell(RubyThreadWorldData world, Vector2I cell) => world.Ground.TryGetValue(cell, out Vector2I a) && _owner.DirtAtlases.Contains(a);
		public bool IsGroundFamilyCell(RubyThreadWorldData world, Vector2I cell) => world.Ground.TryGetValue(cell, out Vector2I a) && IsBiomeGroundAtlas(a);
		public bool IsRealTerrainCell(RubyThreadWorldData world, Vector2I cell) => world.Ground.TryGetValue(cell, out Vector2I a) && (IsBiomeGroundAtlas(a) || a == _owner.SandAtlas || _owner.IsLakeAtlas(a) || _owner.DirtAtlases.Contains(a) || a == _owner.VoidAtlas);

		public Vector2I PickGroundAtlas(float v) {
			if (_isDarkForest)  return v >= _owner.GroundAlt2Threshold ? _owner.DarkForestGroundAlt2Atlas  : v >= _owner.GroundAlt1Threshold ? _owner.DarkForestGroundAlt1Atlas  : _owner.DarkForestGroundMainAtlas;
			if (_isSnowForest) return v >= _owner.GroundAlt2Threshold ? _owner.SnowForestGroundAlt2Atlas  : v >= _owner.GroundAlt1Threshold ? _owner.SnowForestGroundAlt1Atlas  : _owner.SnowForestGroundMainAtlas;
			if (_isDesert)     return v >= _owner.GroundAlt2Threshold ? _owner.DesertGroundAlt2Atlas       : v >= _owner.GroundAlt1Threshold ? _owner.DesertGroundAlt1Atlas       : _owner.DesertGroundMainAtlas;
			if (_isSnowlands)  return v >= 0.93f ? _owner.SnowlandsGroundAlt1Atlas : v >= 0.72f ? _owner.SnowlandsGroundAlt2Atlas : _owner.SnowlandsGroundMainAtlas;
			return v >= _owner.GroundAlt2Threshold ? _owner.ForestGroundAlt2Atlas : v >= _owner.GroundAlt1Threshold ? _owner.ForestGroundAlt1Atlas : _owner.ForestGroundMainAtlas;
		}
	} // end ThreadGenerationContext
} // end CreatingSaveLoading


//I'm tired, one day I back to edit this