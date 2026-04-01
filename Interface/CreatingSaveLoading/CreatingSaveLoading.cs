/*
here has 5 hours of my life, yes five hours XD, 
my mind is crazy now, I can't modify this more, 
I'm tired, Same help me, please

v3:
now this thing also pregenerates portals and enemies

full delta pass:
all ruby ghosts were evicted from this file
*/


using Godot;
using projecthorizonscs.Autoload;
using projecthorizonscs;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace projecthorizonscs;

[GlobalClass]
public sealed class DeltaChestSpawnData
{
	public string ChestScenePath = "";
	public Vector2I Cell = Vector2I.Zero;

	public Godot.Collections.Dictionary Serialize()
	{
		return new()
		{
			{ "scene", ChestScenePath },
			{ "cell_x", Cell.X },
			{ "cell_y", Cell.Y },
		};
	}

	public void Deserialize(Godot.Collections.Dictionary dict)
	{
		ChestScenePath = dict.ContainsKey("scene") ? dict["scene"].AsString() : "";

		int x = dict.ContainsKey("cell_x") ? dict["cell_x"].AsInt32() : 0;
		int y = dict.ContainsKey("cell_y") ? dict["cell_y"].AsInt32() : 0;

		Cell = new Vector2I(x, y);
	}
}

public sealed class ChestBakeEntry
{
	public string ScenePath;
	public float Chance;

	public ChestBakeEntry(string scenePath, float chance)
	{
		ScenePath = scenePath;
		Chance = chance;
	}
}

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

	[ExportGroup("Bake Settings")]
	[Export(PropertyHint.Range, "1,16,1")] public int MaxBakeThreads = 3;
	[Export(PropertyHint.Range, "1,100,1")] public int GcCollectEveryLevels = 15;
	[Export] public bool SkipSavingEmptyChunks = true;
	[Export] public bool UseSparseChunks = true;
	[Export] public int TotalLevelsToBake = 80;

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

	[ExportGroup("Portal")]
	[Export] public bool PregeneratePortals = true;
	[Export] public int InitialPortalRadius = 3;
	[Export] public int ExitPortalRadius = 2;
	[Export] public int MinExitDistanceFromInitial = 25;

	[ExportGroup("Enemies")]
	[Export] public bool PregenerateEnemies = true;
	[Export] public int MinEnemyDistanceFromInitialPortal = 10;
	[Export] public int MinDistanceBetweenPregeneratedEnemies = 6;

	[ExportGroup("Chests")]
	[Export] public bool PregenerateChests = true;
	[Export] public int MinChestDistanceFromInitialPortal = 8;
	[Export] public int MinDistanceBetweenPregeneratedChests = 10;
	[Export] public int MinChestDistanceFromExitPortal = 6;
	[Export] public int MaxChestsPerLevel = 3;

	[Export] public Godot.Collections.Array<DeltaChestSceneChanceData> ChestTable = new();

	private Label _mainLabel;
	private Label _subLabel;
	private ProgressBar _progressBar;
	private string _worldName;
	private readonly object _progressLock = new();

	public int LevelBiomeId = 0;

	public volatile int _threadStageIndex = 0;
	public volatile int _threadStageCount = 1;
	public volatile int _threadStageCurrent = 0;
	public volatile int _threadStageMax = 1;
	public volatile string _threadStageTitle = "preparando...";
	public volatile string _threadStageSubText = "iniciando...";

	public Dictionary<Vector2I, DeltaBakeChunkData> chunksDictionary = new();

	public int LevelSizeX => ChunksX * ChunkSize;
	public int LevelSizeY => ChunksY * ChunkSize;
	public int HalfLevelSizeX => LevelSizeX / 2;
	public int HalfLevelSizeY => LevelSizeY / 2;

	public List<DeltaChestSpawnData> Chests = new();

	private readonly List<ChestBakeEntry> _threadSafeChestTable = new();
	private int _cachedSaveDifficulty = 0;

	public override async void _Ready()
	{
		_mainLabel = GetNodeOrNull<Label>(MainLabelPath);
		_subLabel = GetNodeOrNull<Label>(SubLabelPath);
		_progressBar = GetNodeOrNull<ProgressBar>(ProgressBarPath);

		if (DataManager.I != null && DataManager.I.CurrentWorldData.TryGetValue("SaveName", out Variant saveNameVar) && saveNameVar.AsString() != "")
			_worldName = saveNameVar.AsString();
		else
			_worldName = "World";

		if (RandomizeSeedOnReady)
			RandomizeSeed();

		CacheThreadSafeData();

		_ = BakeWorldAsync();
	}

	private void CacheThreadSafeData()
	{
		_threadSafeChestTable.Clear();

		if (ChestTable != null)
		{
			for (int i = 0; i < ChestTable.Count; i++)
			{
				var entry = ChestTable[i];
				if (entry == null)
					continue;

				string path = entry.ChestScenePath?.Trim() ?? "";
				float chance = entry.Chance;

				if (string.IsNullOrWhiteSpace(path) || chance <= 0f)
					continue;

				_threadSafeChestTable.Add(new ChestBakeEntry(path, chance));
			}
		}

		_cachedSaveDifficulty = 0;

		try
		{
			if (DataManager.I != null &&
				DataManager.I.CurrentWorldData != null &&
				DataManager.I.CurrentWorldData.ContainsKey("SaveDifficulty"))
			{
				_cachedSaveDifficulty = (int)DataManager.I.CurrentWorldData["SaveDifficulty"];
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"failed to cache save difficulty: {ex}");
			_cachedSaveDifficulty = 0;
		}
	}

	private async Task BakeWorldAsync()
	{
		GD.Print($"Starting Baking for {_worldName} with seed {SeedValue}");

		try
		{
			var threadedTask = Task.Run(GenerateAllLevels);

			while (!threadedTask.IsCompleted)
			{
				UpdateUISmooth();
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			}

			await threadedTask;

			ValidateBakeOutput();

			GD.Print("Baking Complete!");
			if (_mainLabel != null) _mainLabel.Text = "baking completo!";
			if (_subLabel != null) _subLabel.Text = $"todos os {TotalLevelsToBake} níveis foram salvos.";
			if (_progressBar != null) _progressBar.Value = 100;
			await ToSignal(GetTree().CreateTimer(0.2f), SceneTreeTimer.SignalName.Timeout);
			GetTree().ChangeSceneToFile("uid://c5kpx6e4716b0");
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Bake failed: {ex}");
			GD.PushError($"erro ao gerar mundo: {ex}");

			if (_mainLabel != null) _mainLabel.Text = "erro ao gerar mundo";
			if (_subLabel != null) _subLabel.Text = ex.Message;
			if (_progressBar != null) _progressBar.Value = 0;
		}
	}

	private void ValidateBakeOutput()
	{
		string firstMetadata = $"user://saves/{_worldName}/level_0/level_metadata.dat";
		if (!FileAccess.FileExists(firstMetadata))
			throw new Exception($"o bake terminou mas o level_0 não existe: {firstMetadata}");
	}

	private void UpdateUISmooth()
	{
		if (_mainLabel != null) _mainLabel.Text = _threadStageTitle;
		if (_subLabel != null) _subLabel.Text = $"{_threadStageSubText} | nível {_threadStageIndex + 1}/{Mathf.Max(1, TotalLevelsToBake)}";

		float stageStart = _threadStageIndex * (100f / Mathf.Max(1, TotalLevelsToBake));
		float stageProgress = _threadStageMax > 0 ? (float)_threadStageCurrent / _threadStageMax : 0f;
		float currentVal = Mathf.Clamp(stageStart + (stageProgress * (100f / Mathf.Max(1, TotalLevelsToBake))), 0f, 100f);

		if (_progressBar != null)
			_progressBar.Value = currentVal;
	}

	private void GenerateAllLevels()
	{
		_threadStageCount = Mathf.Max(1, TotalLevelsToBake);

		if (TotalLevelsToBake <= 0)
			throw new Exception("TotalLevelsToBake está 0. nada para gerar.");

		for (int levelId = 0; levelId < TotalLevelsToBake; levelId++)
		{
			int biomeId = GetDeterministicBiome(levelId);

			lock (_progressLock)
			{
				_threadStageIndex = levelId;
				_threadStageCurrent = 0;
				_threadStageMax = 1;
				_threadStageTitle = $"gerando nível {levelId + 1}/{TotalLevelsToBake}...";
				_threadStageSubText = $"bioma: {GetBiomeNameForId(biomeId)}";
			}

			DeltaThreadGenerationResult result = GenerateChunksDataThreaded(levelId, biomeId);

			if (result == null)
				throw new Exception($"GenerateChunksDataThreaded retornou null no nível {levelId}.");

			if (result.Chunks == null || result.Chunks.Count == 0)
				throw new Exception($"nível {levelId} não gerou chunks.");

			DeltaLevelMetadata metadata = BuildLevelMetadata(levelId, biomeId, result);
			SaveLevelToDisk(levelId, result, metadata);
			ValidateSavedLevel(levelId);

			result.Clear();

			lock (_progressLock)
			{
				_threadStageIndex = levelId;
				_threadStageCurrent = 1;
				_threadStageMax = 1;
				_threadStageTitle = $"{levelId + 1}/{TotalLevelsToBake} níveis concluídos...";
				_threadStageSubText = $"bioma: {GetBiomeNameForId(biomeId)} (nível {levelId + 1})";
			}

			if (GcCollectEveryLevels > 0 && (levelId + 1) % GcCollectEveryLevels == 0)
				GC.Collect(2, GCCollectionMode.Optimized, false);
		}
	}

	private string GetBiomeNameForId(int biomeId) => biomeId switch
	{
		1 => "dark forest",
		2 => "snow forest",
		3 => "desert",
		4 => "snowlands",
		_ => "forest"
	};

	private int GetDeterministicBiome(int levelId)
	{
		int hash = Math.Abs((int)((long)levelId * 2654435761L ^ (long)SeedValue * 40503L));
		float roll = (hash % 10000) / 10000.0f;

		if (levelId <= 10) return roll < 0.85f ? 0 : 1;
		if (levelId <= 30) return roll < 0.20f ? 0 : roll < 0.75f ? 1 : 3;
		if (levelId <= 50) return roll < 0.10f ? 1 : roll < 0.80f ? 3 : 4;
		return roll < 0.30f ? 2 : roll < 0.80f ? 4 : 3;
	}

	private void ValidateSavedLevel(int levelId)
	{
		string levelPath = $"user://saves/{_worldName}/level_{levelId}";
		string metadataFile = $"{levelPath}/level_metadata.dat";

		if (!FileAccess.FileExists(metadataFile))
			throw new Exception($"metadata do nível {levelId} não foi salva em {metadataFile}");
	}

	private void SaveLevelToDisk(int levelId, DeltaThreadGenerationResult result, DeltaLevelMetadata metadata)
	{
		string levelPath = $"user://saves/{_worldName}/level_{levelId}";
		DirAccess.MakeDirRecursiveAbsolute(levelPath);

		foreach (var chunkPair in result.Chunks)
		{
			DeltaBakeChunkData rubyChunk = chunkPair.Value;

			if (SkipSavingEmptyChunks &&
				rubyChunk.GroundTiles.Count == 0 &&
				rubyChunk.SmallDetailTiles.Count == 0 &&
				rubyChunk.MediumDetailTiles.Count == 0 &&
				rubyChunk.ObjectTiles.Count == 0 &&
				rubyChunk.ShadowTiles.Count == 0)
			{
				continue;
			}

			DeltaChunkData delta = ConvertToDeltaChunk(rubyChunk);
			string chunkFile = $"{levelPath}/chunk_{chunkPair.Key.X}_{chunkPair.Key.Y}.dat";

			using var file = FileAccess.Open(chunkFile, FileAccess.ModeFlags.Write);
			if (file != null)
				file.StoreVar(delta.Serialize());
		}

		string metadataFile = $"{levelPath}/level_metadata.dat";
		using var metaFile = FileAccess.Open(metadataFile, FileAccess.ModeFlags.Write);
		if (metaFile != null)
			metaFile.StoreVar(metadata.Serialize());
	}

	private DeltaChunkData ConvertToDeltaChunk(DeltaBakeChunkData oldChunk)
	{
		var d = new DeltaChunkData();

		foreach (var tile in oldChunk.GroundTiles) d.GroundTiles.Add(DeltaChunkTileData.Pack(tile.Cell, tile.Atlas));
		foreach (var tile in oldChunk.SmallDetailTiles) d.SmallDetailTiles.Add(DeltaChunkTileData.Pack(tile.Cell, tile.Atlas));
		foreach (var tile in oldChunk.MediumDetailTiles) d.MediumDetailTiles.Add(DeltaChunkTileData.Pack(tile.Cell, tile.Atlas));
		foreach (var tile in oldChunk.ObjectTiles) d.ObjectTiles.Add(DeltaChunkTileData.Pack(tile.Cell, tile.Atlas));
		foreach (var tile in oldChunk.ShadowTiles) d.ShadowTiles.Add(DeltaChunkTileData.Pack(tile.Cell, tile.Atlas));

		return d;
	}

	private DeltaLevelMetadata BuildLevelMetadata(int levelId, int biomeId, DeltaThreadGenerationResult result)
	{
		var metadata = new DeltaLevelMetadata();
		metadata.LevelId = levelId;
		metadata.BiomeId = biomeId;
		metadata.SeedValue = SeedValue;

		if (PregeneratePortals)
			GeneratePortalsForLevel(metadata, result);

		if (PregenerateEnemies)
			GenerateEnemiesForLevel(metadata, result, biomeId, levelId, _cachedSaveDifficulty);

		if (PregenerateChests)
			GenerateChestsForLevel(metadata, result, levelId);

		return metadata;
	}

	private void GeneratePortalsForLevel(DeltaLevelMetadata metadata, DeltaThreadGenerationResult result)
	{
		List<Vector2I> allChunks = new(result.Chunks.Keys);

		allChunks.Sort((a, b) => a.DistanceSquaredTo(Vector2I.Zero).CompareTo(b.DistanceSquaredTo(Vector2I.Zero)));
		List<Vector2I> centerChunks = allChunks.GetRange(0, Mathf.Min(12, allChunks.Count));

		var initialCandidates = new List<Vector2I>();
		foreach (Vector2I chunkCoord in centerChunks)
			initialCandidates.AddRange(GetValidPortalCellsInChunk(result.Chunks, chunkCoord, 2, InitialPortalRadius));

		if (initialCandidates.Count == 0)
			return;

		initialCandidates.Sort((a, b) => a.DistanceSquaredTo(Vector2I.Zero).CompareTo(b.DistanceSquaredTo(Vector2I.Zero)));
		var rng = new Random(HashInt(SeedValue, metadata.LevelId, 9001));
		int topCount = Mathf.Min(12, initialCandidates.Count);
		Vector2I initialCell = initialCandidates[rng.Next(0, topCount)];
		metadata.HasInitialPortal = true;
		metadata.InitialPortalCell = initialCell;

		allChunks.Sort((a, b) => b.DistanceSquaredTo(Vector2I.Zero).CompareTo(a.DistanceSquaredTo(Vector2I.Zero)));
		List<Vector2I> outerChunks = allChunks.GetRange(0, Mathf.Min(30, allChunks.Count));

		var exitCandidates = new List<Vector2I>();

		foreach (Vector2I chunkCoord in outerChunks)
		{
			int startX = chunkCoord.X * ChunkSize;
			int startY = chunkCoord.Y * ChunkSize;

			for (int localY = ExitPortalRadius; localY < ChunkSize - ExitPortalRadius; localY += 2)
			{
				for (int localX = ExitPortalRadius; localX < ChunkSize - ExitPortalRadius; localX += 2)
				{
					Vector2I worldCell = new(startX + localX, startY + localY);

					if (!IsPortalValidAtCell(result.Chunks, worldCell))
						continue;

					if (worldCell.DistanceSquaredTo(initialCell) < MinExitDistanceFromInitial * MinExitDistanceFromInitial)
						continue;

					if (!HasEnoughSpaceForPortal(result.Chunks, worldCell, ExitPortalRadius))
						continue;

					if (!IsNearVoid(result.Chunks, worldCell, 6))
						continue;

					exitCandidates.Add(worldCell);
				}
			}
		}

		if (exitCandidates.Count == 0)
		{
			Vector2I? fallback = GetFarthestValidCellFromInitial(result.Chunks, initialCell);
			if (fallback != null)
			{
				metadata.HasExitPortal = true;
				metadata.ExitPortalCell = fallback.Value;
			}
			return;
		}

		Vector2I chosenExit = exitCandidates[rng.Next(0, exitCandidates.Count)];
		metadata.HasExitPortal = true;
		metadata.ExitPortalCell = chosenExit;
	}

	private void GenerateChestsForLevel(DeltaLevelMetadata metadata, DeltaThreadGenerationResult result, int levelId)
	{
		if (_threadSafeChestTable.Count == 0)
			return;

		var candidateCells = new List<Vector2I>();

		foreach (var pair in result.Chunks)
		{
			DeltaBakeChunkData chunk = pair.Value;
			if (!IsSpawnableChunk(chunk))
				continue;

			foreach (Vector2I cell in chunk.ValidGroundCells)
			{
				if (HasObjectAtCell(result.Chunks, cell))
					continue;

				if (metadata.HasInitialPortal &&
					cell.DistanceSquaredTo(metadata.InitialPortalCell) < MinChestDistanceFromInitialPortal * MinChestDistanceFromInitialPortal)
					continue;

				if (metadata.HasExitPortal &&
					cell.DistanceSquaredTo(metadata.ExitPortalCell) < MinChestDistanceFromExitPortal * MinChestDistanceFromExitPortal)
					continue;

				if (IsTooCloseToEnemies(metadata.Enemies, cell, 3))
					continue;

				candidateCells.Add(cell);
			}
		}

		if (candidateCells.Count == 0)
			return;

		var rng = new Random(HashInt(SeedValue, levelId, 424242));
		int chestAmount = Mathf.Min(MaxChestsPerLevel, Mathf.Max(1, candidateCells.Count / 800));

		for (int i = 0; i < chestAmount && candidateCells.Count > 0; i++)
		{
			int index = rng.Next(0, candidateCells.Count);
			Vector2I chosenCell = candidateCells[index];
			candidateCells.RemoveAt(index);

			if (!CanPlacePregeneratedChest(metadata.Chests, chosenCell))
				continue;

			string chestScene = GetDeterministicChestScene(chosenCell, levelId);
			if (string.IsNullOrEmpty(chestScene))
				continue;

			metadata.Chests.Add(new DeltaChestSpawnData
			{
				ChestScenePath = chestScene,
				Cell = chosenCell
			});
		}
	}

	private bool CanPlacePregeneratedChest(List<DeltaChestSpawnData> existing, Vector2I cell)
	{
		foreach (DeltaChestSpawnData chest in existing)
		{
			if (chest.Cell.DistanceSquaredTo(cell) < MinDistanceBetweenPregeneratedChests * MinDistanceBetweenPregeneratedChests)
				return false;
		}

		return true;
	}

	private bool IsTooCloseToEnemies(List<DeltaEnemySpawnData> enemies, Vector2I cell, int minDistance)
	{
		int minDistSq = minDistance * minDistance;

		foreach (DeltaEnemySpawnData enemy in enemies)
		{
			if (enemy.Cell.DistanceSquaredTo(cell) < minDistSq)
				return true;
		}

		return false;
	}

	private string GetDeterministicChestScene(Vector2I cell, int levelId)
	{
		if (_threadSafeChestTable.Count == 0)
			return "";

		float totalChance = 0f;
		for (int i = 0; i < _threadSafeChestTable.Count; i++)
			totalChance += Mathf.Max(0f, _threadSafeChestTable[i].Chance);

		if (totalChance <= 0f)
			return "";

		int hash = Math.Abs(HashInt(cell.X, cell.Y, levelId, SeedValue, 8181));
		float roll = (hash % 100000) / 100000f * totalChance;

		float acc = 0f;
		for (int i = 0; i < _threadSafeChestTable.Count; i++)
		{
			var entry = _threadSafeChestTable[i];
			acc += Mathf.Max(0f, entry.Chance);

			if (roll <= acc)
				return entry.ScenePath;
		}

		return _threadSafeChestTable[_threadSafeChestTable.Count - 1].ScenePath;
	}

	private void GenerateEnemiesForLevel(DeltaLevelMetadata metadata, DeltaThreadGenerationResult result, int biomeId, int levelId, int difficulty)
	{
		int enemyAmount = (int)Mathf.Round(
			Mathf.Max(1f, (result.Chunks.Count / 1000f) * ((difficulty + 1) / 2f))
		);

		var candidateCells = new List<Vector2I>();
		foreach (var pair in result.Chunks)
		{
			DeltaBakeChunkData chunk = pair.Value;
			if (!IsSpawnableChunk(chunk))
				continue;

			foreach (Vector2I cell in chunk.ValidGroundCells)
			{
				if (metadata.HasInitialPortal &&
					cell.DistanceSquaredTo(metadata.InitialPortalCell) < MinEnemyDistanceFromInitialPortal * MinEnemyDistanceFromInitialPortal)
					continue;

				candidateCells.Add(cell);
			}
		}

		if (candidateCells.Count == 0)
			return;

		var rng = new Random(HashInt(SeedValue, levelId, 13371337));

		for (int i = 0; i < enemyAmount && candidateCells.Count > 0; i++)
		{
			int index = rng.Next(0, candidateCells.Count);
			Vector2I chosenCell = candidateCells[index];
			candidateCells.RemoveAt(index);

			if (!CanPlacePregeneratedEnemy(metadata.Enemies, chosenCell))
				continue;

			string enemyId = GetDeterministicEnemyIdForBiome(biomeId, chosenCell, levelId);
			if (string.IsNullOrEmpty(enemyId))
				continue;

			metadata.Enemies.Add(new DeltaEnemySpawnData
			{
				EnemyId = enemyId,
				Cell = chosenCell
			});
		}
	}

	private bool CanPlacePregeneratedEnemy(List<DeltaEnemySpawnData> existing, Vector2I cell)
	{
		foreach (DeltaEnemySpawnData enemy in existing)
		{
			if (enemy.Cell.DistanceSquaredTo(cell) < MinDistanceBetweenPregeneratedEnemies * MinDistanceBetweenPregeneratedEnemies)
				return false;
		}
		return true;
	}

	private string GetDeterministicEnemyIdForBiome(int biomeId, Vector2I cell, int levelId)
	{
		int roll = Math.Abs(HashInt(cell.X, cell.Y, levelId, biomeId)) % 3;

		return biomeId switch
		{
			1 => roll switch { 0 => "dark_slime", 1 => "dark_wolf", _ => "shadow_beast" },
			2 => roll switch { 0 => "snow_slime", 1 => "ice_wolf", _ => "frozen_beast" },
			3 => roll switch { 0 => "sand_slime", 1 => "scorpion", _ => "desert_beast" },
			4 => roll switch { 0 => "snow_wolf", 1 => "frozen_wisp", _ => "ice_beast" },
			_ => roll switch { 0 => "slime", 1 => "wolf", _ => "forest_beast" }
		};
	}

	private bool IsSpawnableChunk(DeltaBakeChunkData chunk)
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

	public void RandomizeSeed()
	{
		var rng = new RandomNumberGenerator();
		SeedValue = rng.RandiRange(0, 99999999);
	}

	private DeltaThreadGenerationResult GenerateChunksDataThreaded(int levelId, int biomeId)
	{
		var context = new ThreadGenerationContext(this, levelId, biomeId);
		context.SetupNoises();

		var world = new DeltaThreadWorldData();
		int halfX = HalfLevelSizeX;
		int halfY = HalfLevelSizeY;

		for (int y = -halfY; y < halfY; y++)
		{
			for (int x = -halfX; x < halfX; x++)
			{
				if (!context.IsGroundCell(x, y))
					continue;

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

		return BuildChunksFromWorldData(world);
	}

	private DeltaThreadGenerationResult BuildChunksFromWorldData(DeltaThreadWorldData world)
	{
		var result = new DeltaThreadGenerationResult(
			UseSparseChunks ? 256 : ChunksX * ChunksY
		);

		foreach (var pair in world.Ground) AddTileToChunk(result.Chunks, pair.Key, pair.Value, DeltaBakeLayerType.Ground);
		foreach (var pair in world.SmallDetails) AddTileToChunk(result.Chunks, pair.Key, pair.Value, DeltaBakeLayerType.SmallDetails);
		foreach (var pair in world.MediumDetails) AddTileToChunk(result.Chunks, pair.Key, pair.Value, DeltaBakeLayerType.MediumDetails);
		foreach (var pair in world.Objects) AddTileToChunk(result.Chunks, pair.Key, pair.Value, DeltaBakeLayerType.Objects);
		foreach (var pair in world.Shadows) AddTileToChunk(result.Chunks, pair.Key, pair.Value, DeltaBakeLayerType.Shadows);

		world.Ground.Clear();
		world.SmallDetails.Clear();
		world.MediumDetails.Clear();
		world.Objects.Clear();
		world.Shadows.Clear();

		return result;
	}

	private void AddTileToChunk(Dictionary<Vector2I, DeltaBakeChunkData> chunks, Vector2I cell, Vector2I atlas, DeltaBakeLayerType layerType)
	{
		Vector2I chunkCoord = CellToChunk(cell);

		if (!chunks.TryGetValue(chunkCoord, out DeltaBakeChunkData chunk))
		{
			chunk = new DeltaBakeChunkData();
			chunks[chunkCoord] = chunk;
		}

		switch (layerType)
		{
			case DeltaBakeLayerType.Ground:
				chunk.GroundTiles.Add(new DeltaBakeTileData(cell, atlas));
				chunk.GroundAtlasByCell[cell] = atlas;
				if (IsPortalValidGroundAtlas(atlas))
				{
					chunk.ValidGroundCells.Add(cell);
					chunk.ValidGroundCellSet.Add(cell);
				}
				if (atlas == WaterAtlas || atlas == DeepWaterAtlas || atlas == FrozenWaterAtlas || atlas == FrozenDeepWaterAtlas)
					chunk.WaterCells++;
				if (atlas == VoidAtlas)
					chunk.VoidCells++;
				break;

			case DeltaBakeLayerType.SmallDetails:
				chunk.SmallDetailTiles.Add(new DeltaBakeTileData(cell, atlas));
				break;

			case DeltaBakeLayerType.MediumDetails:
				chunk.MediumDetailTiles.Add(new DeltaBakeTileData(cell, atlas));
				break;

			case DeltaBakeLayerType.Objects:
				chunk.ObjectTiles.Add(new DeltaBakeTileData(cell, atlas));
				chunk.ObjectCellSet.Add(cell);
				break;

			case DeltaBakeLayerType.Shadows:
				chunk.ShadowTiles.Add(new DeltaBakeTileData(cell, atlas));
				break;
		}
	}

	private Vector2I CellToChunk(Vector2I cell)
	{
		return new Vector2I(
			Mathf.FloorToInt((float)cell.X / ChunkSize),
			Mathf.FloorToInt((float)cell.Y / ChunkSize)
		);
	}

	private bool IsPortalValidGroundAtlas(Vector2I atlas)
	{
		return atlas == ForestGroundMainAtlas
			|| atlas == ForestGroundAlt1Atlas
			|| atlas == ForestGroundAlt2Atlas
			|| atlas == DarkForestGroundMainAtlas
			|| atlas == DarkForestGroundAlt1Atlas
			|| atlas == DarkForestGroundAlt2Atlas
			|| atlas == SnowForestGroundMainAtlas
			|| atlas == SnowForestGroundAlt1Atlas
			|| atlas == SnowForestGroundAlt2Atlas
			|| atlas == DesertGroundMainAtlas
			|| atlas == DesertGroundAlt1Atlas
			|| atlas == DesertGroundAlt2Atlas
			|| atlas == SnowlandsGroundMainAtlas
			|| atlas == SnowlandsGroundAlt1Atlas
			|| atlas == SnowlandsGroundAlt2Atlas;
	}

	private bool HasEnoughSpaceForPortal(Dictionary<Vector2I, DeltaBakeChunkData> chunks, Vector2I centerCell, int radius)
	{
		for (int y = -radius; y <= radius; y++)
		{
			for (int x = -radius; x <= radius; x++)
			{
				Vector2I cell = centerCell + new Vector2I(x, y);

				if (!IsPortalValidAtCell(chunks, cell))
					return false;

				if (HasObjectAtCell(chunks, cell))
					return false;
			}
		}
		return true;
	}

	private bool IsPortalValidAtCell(Dictionary<Vector2I, DeltaBakeChunkData> chunks, Vector2I cell)
	{
		Vector2I chunkCoord = CellToChunk(cell);
		if (!chunks.TryGetValue(chunkCoord, out DeltaBakeChunkData chunk))
			return false;
		return chunk.ValidGroundCellSet.Contains(cell);
	}

	private bool HasObjectAtCell(Dictionary<Vector2I, DeltaBakeChunkData> chunks, Vector2I cell)
	{
		Vector2I chunkCoord = CellToChunk(cell);
		if (!chunks.TryGetValue(chunkCoord, out DeltaBakeChunkData chunk))
			return false;
		return chunk.ObjectCellSet.Contains(cell);
	}

	private List<Vector2I> GetValidPortalCellsInChunk(Dictionary<Vector2I, DeltaBakeChunkData> chunks, Vector2I chunkCoord, int sampleStep, int portalRadius)
	{
		List<Vector2I> validCells = new();

		int startX = chunkCoord.X * ChunkSize;
		int startY = chunkCoord.Y * ChunkSize;

		for (int localY = portalRadius; localY < ChunkSize - portalRadius; localY += sampleStep)
		{
			for (int localX = portalRadius; localX < ChunkSize - portalRadius; localX += sampleStep)
			{
				Vector2I worldCell = new(startX + localX, startY + localY);

				if (!IsPortalValidAtCell(chunks, worldCell))
					continue;

				if (!HasEnoughSpaceForPortal(chunks, worldCell, portalRadius))
					continue;

				validCells.Add(worldCell);
			}
		}

		return validCells;
	}

	private Vector2I? GetFarthestValidCellFromInitial(Dictionary<Vector2I, DeltaBakeChunkData> chunks, Vector2I initialCell)
	{
		Vector2I? bestCell = null;
		int bestDistance = -1;

		foreach (var pair in chunks)
		{
			foreach (Vector2I worldCell in pair.Value.ValidGroundCells)
			{
				if (!HasEnoughSpaceForPortal(chunks, worldCell, ExitPortalRadius))
					continue;

				int dist = worldCell.DistanceSquaredTo(initialCell);
				if (dist <= bestDistance)
					continue;

				bestDistance = dist;
				bestCell = worldCell;
			}
		}

		return bestCell;
	}

	private bool IsNearVoid(Dictionary<Vector2I, DeltaBakeChunkData> chunks, Vector2I cell, int radius = 6)
	{
		for (int y = -radius; y <= radius; y++)
		{
			for (int x = -radius; x <= radius; x++)
			{
				Vector2I check = cell + new Vector2I(x, y);
				Vector2I chunkCoord = CellToChunk(check);

				if (!chunks.TryGetValue(chunkCoord, out DeltaBakeChunkData chunk))
					return true;

				if (!chunk.GroundAtlasByCell.TryGetValue(check, out Vector2I atlas))
					return true;

				if (atlas == VoidAtlas)
					return true;
			}
		}

		return false;
	}

	private int HashInt(params int[] values)
	{
		unchecked
		{
			int hash = 17;
			for (int i = 0; i < values.Length; i++)
				hash = hash * 31 + values[i];
			return hash;
		}
	}

	private enum DeltaBakeLayerType { Ground, SmallDetails, MediumDetails, Objects, Shadows }

	private sealed class ThreadGenerationContext
	{
		private readonly CreatingSaveLoading _owner;
		private readonly int _levelId;
		private readonly int _biomeId;

		private readonly bool _isDarkForest;
		private readonly bool _isSnowForest;
		private readonly bool _isDesert;
		private readonly bool _isSnowlands;

		public FastNoiseLite GroundNoise;
		public FastNoiseLite DensityNoise;
		public FastNoiseLite MediumDetailsNoise;
		public FastNoiseLite TreeNoise;
		public FastNoiseLite LakeNoise;

		private readonly HashSet<Vector2I> _treeSet = new();

		public ThreadGenerationContext(CreatingSaveLoading owner, int levelId, int biomeId)
		{
			_owner = owner;
			_levelId = levelId;
			_biomeId = biomeId;
			_isDarkForest = biomeId == 1;
			_isSnowForest = biomeId == 2;
			_isDesert = biomeId == 3;
			_isSnowlands = biomeId == 4;
		}

		public void SetupNoises()
		{
			GroundNoise = new FastNoiseLite { Seed = _owner.SeedValue + _levelId, NoiseType = _owner.GroundNoiseType, Frequency = _owner.GroundFrequency };
			DensityNoise = new FastNoiseLite { Seed = _owner.SeedValue + 1000 + _levelId, NoiseType = _owner.DensityNoiseType, Frequency = _owner.DensityFrequency };
			MediumDetailsNoise = new FastNoiseLite { Seed = _owner.SeedValue + 3000 + _levelId, NoiseType = _owner.MediumDetailsNoiseType, Frequency = _owner.MediumDetailsFrequency };
			TreeNoise = new FastNoiseLite { Seed = _owner.SeedValue + 4000 + _levelId, NoiseType = _owner.TreeNoiseType, Frequency = _owner.TreeNoiseFrequency };
			LakeNoise = new FastNoiseLite { Seed = _owner.SeedValue + 5000 + _levelId, NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin, Frequency = _owner.LakeNoiseFrequency };
		}

		public void GenerateLakesPass(DeltaThreadWorldData world)
		{
			int halfX = _owner.HalfLevelSizeX;
			int halfY = _owner.HalfLevelSizeY;

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

					for (int oy = -_owner.LakeEdgeDepth; oy <= _owner.LakeEdgeDepth && !nearNonLakeGround; oy++)
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
					}

					world.Ground[cell] = nearNonLakeGround ? GetShallowLakeAtlas() : GetDeepLakeAtlas();
				}
			}
		}

		public void GenerateBeachPass(DeltaThreadWorldData world)
		{
			int halfX = _owner.HalfLevelSizeX;
			int halfY = _owner.HalfLevelSizeY;

			for (int y = -halfY; y < halfY; y++)
			{
				for (int x = -halfX; x < halfX; x++)
				{
					Vector2I cell = new(x, y);

					if (!IsGroundFamilyCell(world, cell) && !IsDirtCell(world, cell))
						continue;

					if (!IsNearWater(world, cell, _owner.BeachBorderDistance))
						continue;

					if (RandomFloat01FromPosition(x, y, 6060) <= _owner.BeachChance)
						world.Ground[cell] = _owner.SandAtlas;
				}
			}
		}

		public void GenerateVoidBorderPass(DeltaThreadWorldData world)
		{
			int halfX = _owner.HalfLevelSizeX;
			int halfY = _owner.HalfLevelSizeY;

			for (int y = -halfY; y < halfY; y++)
			{
				for (int x = -halfX; x < halfX; x++)
				{
					Vector2I cell = new(x, y);

					if (IsRealTerrainCell(world, cell))
						continue;

					if (HasNeighborRealTerrain(world, cell, _owner.VoidBorderThickness))
						world.Ground[cell] = _owner.VoidAtlas;
				}
			}
		}

		public void GenerateSmallDetailsPass(DeltaThreadWorldData world)
		{
			var atlases = GetCurrentSmallDetailAtlases();
			if (atlases.Count == 0)
				return;

			int halfX = _owner.HalfLevelSizeX;
			int halfY = _owner.HalfLevelSizeY;

			for (int y = -halfY; y < halfY; y++)
			{
				for (int x = -halfX; x < halfX; x++)
				{
					Vector2I cell = new(x, y);

					if (!IsGroundMainCell(world, cell) || world.MediumDetails.ContainsKey(cell) || world.Objects.ContainsKey(cell))
						continue;

					float spawnChance = _owner.SmallDetailBaseChance + (GetNoise01(DensityNoise, x, y) * _owner.SmallDetailDensityChance);
					if (RandomFloat01FromPosition(x, y, 5551) <= spawnChance)
						world.SmallDetails[cell] = PickAtlasFromList(atlases, x, y, 7777);
				}
			}
		}

		public void GenerateMediumDetailsPass(DeltaThreadWorldData world)
		{
			var atlases = GetCurrentMediumDetailAtlases();
			if (atlases.Count == 0)
				return;

			int halfX = _owner.HalfLevelSizeX;
			int halfY = _owner.HalfLevelSizeY;

			for (int y = -halfY; y < halfY; y++)
			{
				for (int x = -halfX; x < halfX; x++)
				{
					Vector2I cell = new(x, y);

					if (!IsGroundMainCell(world, cell) || world.Objects.ContainsKey(cell))
						continue;

					float spawnChance =
						_owner.MediumDetailBaseChance +
						(GetNoise01(DensityNoise, x, y) * _owner.MediumDetailDensityChance) +
						(GetNoise01(MediumDetailsNoise, x, y) * _owner.MediumDetailRegionChance);

					if (RandomFloat01FromPosition(x, y, 9127) <= spawnChance && !HasNeighborMediumDetail(world, cell, 1))
						world.MediumDetails[cell] = PickAtlasFromList(atlases, x, y, 9999);
				}
			}
		}

		public void GenerateEdgeDecorationsPass(DeltaThreadWorldData world)
		{
			if (!_owner.GenerateEdgeDecorations || _owner.EdgeDecorAtlases.Count == 0)
				return;

			int halfX = _owner.HalfLevelSizeX;
			int halfY = _owner.HalfLevelSizeY;

			for (int y = -halfY; y < halfY; y++)
			{
				for (int x = -halfX; x < halfX; x++)
				{
					Vector2I cell = new(x, y);

					if (!IsGroundMainCell(world, cell))
						continue;

					if (!IsNearDirt(world, cell, _owner.EdgeDecorRadius))
						continue;

					if (world.Objects.ContainsKey(cell) || world.MediumDetails.ContainsKey(cell))
						continue;

					if (RandomFloat01FromPosition(x, y, 45454) > _owner.EdgeDecorChance)
						continue;

					world.MediumDetails[cell] = PickAtlasFromList(_owner.EdgeDecorAtlases, x, y, 56565);
				}
			}
		}

		public void GenerateTreesPass(DeltaThreadWorldData world)
		{
			var atlases = GetCurrentTreeAtlases();
			if (atlases.Count == 0 && !_isSnowlands)
				return;

			int halfX = _owner.HalfLevelSizeX;
			int halfY = _owner.HalfLevelSizeY;
			int safeTreeStep = Mathf.Max(1, _owner.TreeStep);

			for (int y = -halfY; y < halfY; y += safeTreeStep)
			{
				for (int x = -halfX; x < halfX; x += safeTreeStep)
				{
					Vector2I cell = ApplyTreeRandomOffset(new Vector2I(x, y));

					if (!IsInsideBounds(cell))
						continue;

					if (!HasEnoughSpaceForTree(world, cell))
						continue;

					if (!IsGroundMainCell(world, cell) || world.Objects.ContainsKey(cell))
						continue;

					float spawnChance =
						(1f - _owner.TreeThreshold) +
						(GetNoise01(DensityNoise, cell.X, cell.Y) * _owner.TreeDensityInfluence) +
						_owner.TreeBaseChanceBonus;

					if (GetNoise01(TreeNoise, cell.X, cell.Y) < _owner.TreeThreshold &&
						RandomFloat01FromPosition(cell.X, cell.Y, 17171) > spawnChance)
					{
						continue;
					}

					if (IsNearAnotherTree(cell, _owner.TreeMinDistance))
						continue;

					Vector2I atlas = PickTreeAtlasForCell(world, cell);
					if (atlas != Vector2I.Zero)
					{
						world.Objects[cell] = atlas;
						world.Shadows[cell + _owner.ShadowOffset] = _owner.TreeShadowAtlas;
						_treeSet.Add(cell);
					}
				}
			}
		}

		public bool IsGroundCell(int x, int y)
		{
			float groundValue = GetNoise01(GroundNoise, x, y);
			if (_owner.UseIslandMask)
				groundValue *= GetIslandMask01(x, y);
			return groundValue >= _owner.GroundThreshold;
		}

		public bool IsNearWater(DeltaThreadWorldData world, Vector2I cell, int radius = 1)
		{
			for (int y = -radius; y <= radius; y++)
				for (int x = -radius; x <= radius; x++)
				{
					if (x == 0 && y == 0)
						continue;

					if (world.Ground.TryGetValue(cell + new Vector2I(x, y), out Vector2I a) && IsLakeAtlas(a))
						return true;
				}
			return false;
		}

		public bool IsNearDirt(DeltaThreadWorldData world, Vector2I cell, int radius = 1)
		{
			for (int y = -radius; y <= radius; y++)
				for (int x = -radius; x <= radius; x++)
				{
					if (x == 0 && y == 0)
						continue;

					if (IsDirtCell(world, cell + new Vector2I(x, y)))
						return true;
				}
			return false;
		}

		public bool IsBorderRealTerrainCell(DeltaThreadWorldData world, Vector2I cell, int radius = 1)
		{
			if (!IsRealTerrainCell(world, cell))
				return false;

			for (int y = -radius; y <= radius; y++)
				for (int x = -radius; x <= radius; x++)
				{
					Vector2I check = cell + new Vector2I(x, y);
					if (!IsInsideBounds(check) || !IsRealTerrainCell(world, check))
						return true;
				}
			return false;
		}

		public bool HasNeighborRealTerrain(DeltaThreadWorldData world, Vector2I cell, int radius = 1)
		{
			for (int y = -radius; y <= radius; y++)
				for (int x = -radius; x <= radius; x++)
				{
					if (x == 0 && y == 0)
						continue;

					Vector2I chk = cell + new Vector2I(x, y);
					if (world.Ground.TryGetValue(chk, out Vector2I a) && IsRealTerrainCell(world, chk) && a != _owner.VoidAtlas)
						return true;
				}
			return false;
		}

		public Vector2I PickTreeAtlasForCell(DeltaThreadWorldData world, Vector2I cell)
		{
			if (_isSnowlands && world.Ground.TryGetValue(cell, out Vector2I ga) && ga == _owner.SnowlandsGroundAlt1Atlas && _owner.SnowlandsNormalPineTreeAtlases.Count > 0)
				return PickAtlasFromList(_owner.SnowlandsNormalPineTreeAtlases, cell.X, cell.Y, 33333);

			if (_isSnowlands && _owner.SnowlandsTreeAtlases.Count > 0)
				return PickAtlasFromList(_owner.SnowlandsTreeAtlases, cell.X, cell.Y, 22222);

			var atlases = GetCurrentTreeAtlases();
			return atlases.Count > 0 ? PickAtlasFromList(atlases, cell.X, cell.Y, 22222) : Vector2I.Zero;
		}

		private Godot.Collections.Array<Vector2I> GetCurrentSmallDetailAtlases() => _biomeId switch { 1 => _owner.DarkForestSmallDetailAtlases, 2 => _owner.SnowForestSmallDetailAtlases, 3 => _owner.DesertSmallDetailAtlases, 4 => _owner.SnowlandsSmallDetailAtlases, _ => _owner.ForestSmallDetailAtlases };
		private Godot.Collections.Array<Vector2I> GetCurrentMediumDetailAtlases() => _biomeId switch { 1 => _owner.DarkForestMediumDetailAtlases, 2 => _owner.SnowForestMediumDetailAtlases, 3 => _owner.DesertMediumDetailAtlases, 4 => _owner.SnowlandsMediumDetailAtlases, _ => _owner.ForestMediumDetailAtlases };
		private Godot.Collections.Array<Vector2I> GetCurrentTreeAtlases() => _biomeId switch { 1 => _owner.DarkForestTreeAtlases, 2 => _owner.SnowForestTreeAtlases, 3 => _owner.DesertTreeAtlases, 4 => _owner.SnowlandsTreeAtlases, _ => _owner.ForestTreeAtlases };
		private Vector2I GetCurrentGroundMainAtlas() => _biomeId switch { 1 => _owner.DarkForestGroundMainAtlas, 2 => _owner.SnowForestGroundMainAtlas, 3 => _owner.DesertGroundMainAtlas, 4 => _owner.SnowlandsGroundMainAtlas, _ => _owner.ForestGroundMainAtlas };
		private Vector2I GetCurrentGroundAlt1Atlas() => _biomeId switch { 1 => _owner.DarkForestGroundAlt1Atlas, 2 => _owner.SnowForestGroundAlt1Atlas, 3 => _owner.DesertGroundAlt1Atlas, 4 => _owner.SnowlandsGroundAlt1Atlas, _ => _owner.ForestGroundAlt1Atlas };
		private Vector2I GetCurrentGroundAlt2Atlas() => _biomeId switch { 1 => _owner.DarkForestGroundAlt2Atlas, 2 => _owner.SnowForestGroundAlt2Atlas, 3 => _owner.DesertGroundAlt2Atlas, 4 => _owner.SnowlandsGroundAlt2Atlas, _ => _owner.ForestGroundAlt2Atlas };
		private bool IsBiomeGroundAtlas(Vector2I a) => a == GetCurrentGroundMainAtlas() || a == GetCurrentGroundAlt1Atlas() || a == GetCurrentGroundAlt2Atlas();
		private bool IsLakeAtlas(Vector2I atlas) => atlas == _owner.WaterAtlas || atlas == _owner.DeepWaterAtlas || atlas == _owner.FrozenWaterAtlas || atlas == _owner.FrozenDeepWaterAtlas;
		private Vector2I GetDeepLakeAtlas() => _isSnowlands ? _owner.FrozenDeepWaterAtlas : _owner.DeepWaterAtlas;
		private Vector2I GetShallowLakeAtlas() => _isSnowlands ? _owner.FrozenWaterAtlas : _owner.WaterAtlas;

		public bool IsNearAnotherTree(Vector2I cell, int minDistance)
		{
			int r = minDistance;
			for (int dy = -r; dy <= r; dy++)
				for (int dx = -r; dx <= r; dx++)
					if (_treeSet.Contains(new Vector2I(cell.X + dx, cell.Y + dy)))
						return true;
			return false;
		}

		public bool HasNeighborMediumDetail(DeltaThreadWorldData world, Vector2I cell, int radius = 1)
		{
			for (int y = -radius; y <= radius; y++)
				for (int x = -radius; x <= radius; x++)
					if ((x != 0 || y != 0) && world.MediumDetails.ContainsKey(cell + new Vector2I(x, y)))
						return true;
			return false;
		}

		public bool HasEnoughSpaceForTree(DeltaThreadWorldData world, Vector2I centerCell)
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

		public bool IsInsideBounds(Vector2I cell)
		{
			return cell.X >= -_owner.HalfLevelSizeX &&
				   cell.X < _owner.HalfLevelSizeX &&
				   cell.Y >= -_owner.HalfLevelSizeY &&
				   cell.Y < _owner.HalfLevelSizeY;
		}

		public Vector2I ApplyTreeRandomOffset(Vector2I cell) =>
			cell + new Vector2I(
				RandomRangeFromPosition(cell.X, cell.Y, _owner.SeedValue + 7000 + _levelId, -_owner.TreeRandomOffsetX, _owner.TreeRandomOffsetX),
				RandomRangeFromPosition(cell.X, cell.Y, _owner.SeedValue + 8000 + _levelId, -_owner.TreeRandomOffsetY, _owner.TreeRandomOffsetY)
			);

		public Vector2I PickAtlasFromList(Godot.Collections.Array<Vector2I> atlases, int x, int y, int salt) =>
			atlases.Count == 0 ? Vector2I.Zero : atlases[RandomRangeFromPosition(x, y, salt, 0, atlases.Count - 1)];

		public int RandomRangeFromPosition(int x, int y, int salt, int min, int max)
		{
			if (max <= min)
				return min;
			int h = Math.Abs(x * 73856093 ^ y * 19349663 ^ salt);
			return min + (h % (max - min + 1));
		}

		public float RandomFloat01FromPosition(int x, int y, int salt)
		{
			int h = Math.Abs(x * 73856093 ^ y * 19349663 ^ salt);
			return (h % 10000) / 10000.0f;
		}

		public float GetNoise01(FastNoiseLite noise, int x, int y) => (noise.GetNoise2D(x, y) + 1f) * 0.5f;

		public float GetIslandMask01(int x, int y)
		{
			float nx = (float)x / Mathf.Max(1f, _owner.LevelSizeX * 0.5f);
			float ny = (float)y / Mathf.Max(1f, _owner.LevelSizeY * 0.5f);
			float d = Mathf.Clamp(Mathf.Pow(Mathf.Abs(nx), _owner.IslandRoundness) + Mathf.Pow(Mathf.Abs(ny), _owner.IslandRoundness), 0f, 1.5f);
			return Mathf.Clamp(1f - Mathf.Pow(Mathf.Clamp(d, 0f, 1f), _owner.IslandFalloffPower), 0f, 1f);
		}

		public bool IsGroundMainCell(DeltaThreadWorldData world, Vector2I cell) => world.Ground.TryGetValue(cell, out Vector2I a) && a == GetCurrentGroundMainAtlas();
		public bool IsDirtCell(DeltaThreadWorldData world, Vector2I cell) => world.Ground.TryGetValue(cell, out Vector2I a) && _owner.DirtAtlases.Contains(a);
		public bool IsGroundFamilyCell(DeltaThreadWorldData world, Vector2I cell) => world.Ground.TryGetValue(cell, out Vector2I a) && IsBiomeGroundAtlas(a);
		public bool IsRealTerrainCell(DeltaThreadWorldData world, Vector2I cell) => world.Ground.TryGetValue(cell, out Vector2I a) && (IsBiomeGroundAtlas(a) || a == _owner.SandAtlas || IsLakeAtlas(a) || _owner.DirtAtlases.Contains(a) || a == _owner.VoidAtlas);

		public Vector2I PickGroundAtlas(float v)
		{
			if (_isDarkForest) return v >= _owner.GroundAlt2Threshold ? _owner.DarkForestGroundAlt2Atlas : v >= _owner.GroundAlt1Threshold ? _owner.DarkForestGroundAlt1Atlas : _owner.DarkForestGroundMainAtlas;
			if (_isSnowForest) return v >= _owner.GroundAlt2Threshold ? _owner.SnowForestGroundAlt2Atlas : v >= _owner.GroundAlt1Threshold ? _owner.SnowForestGroundAlt1Atlas : _owner.SnowForestGroundMainAtlas;
			if (_isDesert) return v >= _owner.GroundAlt2Threshold ? _owner.DesertGroundAlt2Atlas : v >= _owner.GroundAlt1Threshold ? _owner.DesertGroundAlt1Atlas : _owner.DesertGroundMainAtlas;
			if (_isSnowlands) return v >= 0.93f ? _owner.SnowlandsGroundAlt1Atlas : v >= 0.72f ? _owner.SnowlandsGroundAlt2Atlas : _owner.SnowlandsGroundMainAtlas;
			return v >= _owner.GroundAlt2Threshold ? _owner.ForestGroundAlt2Atlas : v >= _owner.GroundAlt1Threshold ? _owner.ForestGroundAlt1Atlas : _owner.ForestGroundMainAtlas;
		}
	}
}

public sealed class DeltaBakeChunkData : RefCounted
{
	public List<DeltaBakeTileData> GroundTiles = new();
	public List<DeltaBakeTileData> SmallDetailTiles = new();
	public List<DeltaBakeTileData> MediumDetailTiles = new();
	public List<DeltaBakeTileData> ObjectTiles = new();
	public List<DeltaBakeTileData> ShadowTiles = new();

	public List<Vector2I> ValidGroundCells = new();
	public HashSet<Vector2I> ValidGroundCellSet = new();
	public HashSet<Vector2I> ObjectCellSet = new();
	public Dictionary<Vector2I, Vector2I> GroundAtlasByCell = new();

	public bool SpawnEnemys;
	public int WaterCells;
	public int VoidCells;

	public void Clear()
	{
		GroundTiles.Clear();
		SmallDetailTiles.Clear();
		MediumDetailTiles.Clear();
		ObjectTiles.Clear();
		ShadowTiles.Clear();
		ValidGroundCells.Clear();
		ValidGroundCellSet.Clear();
		ObjectCellSet.Clear();
		GroundAtlasByCell.Clear();
		SpawnEnemys = false;
		WaterCells = 0;
		VoidCells = 0;
	}
}

public sealed class DeltaBakeTileData : RefCounted
{
	public Vector2I Cell;
	public Vector2I Atlas;

	public DeltaBakeTileData()
	{
	}

	public DeltaBakeTileData(Vector2I cell, Vector2I atlas)
	{
		Cell = cell;
		Atlas = atlas;
	}
}

public sealed class DeltaThreadWorldData
{
	public Dictionary<Vector2I, Vector2I> Ground = new();
	public Dictionary<Vector2I, Vector2I> SmallDetails = new();
	public Dictionary<Vector2I, Vector2I> MediumDetails = new();
	public Dictionary<Vector2I, Vector2I> Objects = new();
	public Dictionary<Vector2I, Vector2I> Shadows = new();

	public void Clear()
	{
		Ground.Clear();
		SmallDetails.Clear();
		MediumDetails.Clear();
		Objects.Clear();
		Shadows.Clear();
	}
}

public sealed class DeltaThreadGenerationResult
{
	public Dictionary<Vector2I, DeltaBakeChunkData> Chunks = new();

	public DeltaThreadGenerationResult()
	{
	}

	public DeltaThreadGenerationResult(int capacity)
	{
		Chunks = capacity > 0
			? new Dictionary<Vector2I, DeltaBakeChunkData>(capacity)
			: new Dictionary<Vector2I, DeltaBakeChunkData>();
	}

	public void Clear()
	{
		foreach (DeltaBakeChunkData chunk in Chunks.Values)
			chunk.Clear();

		Chunks.Clear();
	}
}

public sealed class DeltaChunkData
{
	public List<Vector4I> GroundTiles = new();
	public List<Vector4I> SmallDetailTiles = new();
	public List<Vector4I> MediumDetailTiles = new();
	public List<Vector4I> ObjectTiles = new();
	public List<Vector4I> ShadowTiles = new();

	public void Clear()
	{
		GroundTiles.Clear();
		SmallDetailTiles.Clear();
		MediumDetailTiles.Clear();
		ObjectTiles.Clear();
		ShadowTiles.Clear();
	}

	public Godot.Collections.Dictionary Serialize()
	{
		return new Godot.Collections.Dictionary
		{
			{ "ground", PackList(GroundTiles) },
			{ "small", PackList(SmallDetailTiles) },
			{ "medium", PackList(MediumDetailTiles) },
			{ "objects", PackList(ObjectTiles) },
			{ "shadows", PackList(ShadowTiles) },
		};
	}

	public void Deserialize(Godot.Collections.Dictionary dict)
	{
		GroundTiles = UnpackList(dict, "ground");
		SmallDetailTiles = UnpackList(dict, "small");
		MediumDetailTiles = UnpackList(dict, "medium");
		ObjectTiles = UnpackList(dict, "objects");
		ShadowTiles = UnpackList(dict, "shadows");
	}

	private static Godot.Collections.Array<Vector4I> PackList(List<Vector4I> source)
	{
		var arr = new Godot.Collections.Array<Vector4I>();
		foreach (Vector4I item in source)
			arr.Add(item);
		return arr;
	}

	private static List<Vector4I> UnpackList(Godot.Collections.Dictionary dict, string key)
	{
		var list = new List<Vector4I>();

		if (!dict.ContainsKey(key))
			return list;

		var arr = dict[key].AsGodotArray<Vector4I>();
		foreach (Vector4I item in arr)
			list.Add(item);

		return list;
	}
}

public sealed class DeltaEnemySpawnData
{
	public string EnemyId = "";
	public Vector2I Cell = Vector2I.Zero;

	public Godot.Collections.Dictionary Serialize()
	{
		return new Godot.Collections.Dictionary
		{
			{ "enemy_id", EnemyId },
			{ "cell_x", Cell.X },
			{ "cell_y", Cell.Y },
		};
	}

	public void Deserialize(Godot.Collections.Dictionary dict)
	{
		if (dict.ContainsKey("enemy_id")) EnemyId = dict["enemy_id"].AsString();

		int x = dict.ContainsKey("cell_x") ? dict["cell_x"].AsInt32() : 0;
		int y = dict.ContainsKey("cell_y") ? dict["cell_y"].AsInt32() : 0;
		Cell = new Vector2I(x, y);
	}
}

public sealed class DeltaLevelMetadata
{
	public int LevelId;
	public int BiomeId;
	public int SeedValue;

	public bool HasInitialPortal;
	public Vector2I InitialPortalCell = Vector2I.Zero;

	public bool HasExitPortal;
	public Vector2I ExitPortalCell = Vector2I.Zero;

	public List<DeltaEnemySpawnData> Enemies = new();
	public List<DeltaChestSpawnData> Chests = new();

	public Godot.Collections.Dictionary Serialize()
	{
		var enemiesArray = new Godot.Collections.Array<Godot.Collections.Dictionary>();
		foreach (DeltaEnemySpawnData enemy in Enemies)
			enemiesArray.Add(enemy.Serialize());

		var chestsArray = new Godot.Collections.Array<Godot.Collections.Dictionary>();
		foreach (DeltaChestSpawnData chest in Chests)
			chestsArray.Add(chest.Serialize());

		return new Godot.Collections.Dictionary
		{
			{ "level_id", LevelId },
			{ "biome_id", BiomeId },
			{ "seed_value", SeedValue },
			{ "has_initial_portal", HasInitialPortal },
			{ "initial_portal_x", InitialPortalCell.X },
			{ "initial_portal_y", InitialPortalCell.Y },
			{ "has_exit_portal", HasExitPortal },
			{ "exit_portal_x", ExitPortalCell.X },
			{ "exit_portal_y", ExitPortalCell.Y },
			{ "enemies", enemiesArray },
			{ "chests", chestsArray },
		};
	}

	public void Deserialize(Godot.Collections.Dictionary dict)
	{
		if (dict.ContainsKey("level_id")) LevelId = dict["level_id"].AsInt32();
		if (dict.ContainsKey("biome_id")) BiomeId = dict["biome_id"].AsInt32();
		if (dict.ContainsKey("seed_value")) SeedValue = dict["seed_value"].AsInt32();

		HasInitialPortal = dict.ContainsKey("has_initial_portal") && dict["has_initial_portal"].AsBool();
		InitialPortalCell = new Vector2I(
			dict.ContainsKey("initial_portal_x") ? dict["initial_portal_x"].AsInt32() : 0,
			dict.ContainsKey("initial_portal_y") ? dict["initial_portal_y"].AsInt32() : 0
		);

		HasExitPortal = dict.ContainsKey("has_exit_portal") && dict["has_exit_portal"].AsBool();
		ExitPortalCell = new Vector2I(
			dict.ContainsKey("exit_portal_x") ? dict["exit_portal_x"].AsInt32() : 0,
			dict.ContainsKey("exit_portal_y") ? dict["exit_portal_y"].AsInt32() : 0
		);

		Enemies.Clear();
		if (dict.ContainsKey("enemies"))
		{
			var arr = dict["enemies"].AsGodotArray<Godot.Collections.Dictionary>();
			foreach (Godot.Collections.Dictionary enemyDict in arr)
			{
				var enemy = new DeltaEnemySpawnData();
				enemy.Deserialize(enemyDict);
				Enemies.Add(enemy);
			}
		}

		Chests.Clear();
		if (dict.ContainsKey("chests"))
		{
			var arr = dict["chests"].AsGodotArray<Godot.Collections.Dictionary>();
			foreach (Godot.Collections.Dictionary chestDict in arr)
			{
				var chest = new DeltaChestSpawnData();
				chest.Deserialize(chestDict);
				Chests.Add(chest);
			}
		}
	}
}

public static class DeltaChunkTileData
{
	public static Vector4I Pack(Vector2I cell, Vector2I atlas)
	{
		return new Vector4I(cell.X, cell.Y, atlas.X, atlas.Y);
	}
}



//I'm tired, one day I back to edit this
