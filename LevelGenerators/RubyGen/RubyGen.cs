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
	[Export] public int HorizontalRenderRadius = 3;
	[Export] public int VerticalRenderRadius = 7;

	[ExportGroup("General")]
	[Export] public int SeedValue = 44637346;
	[Export] public bool RandomizeSeedOnReady = true;
	[Export] public bool ClearBeforeGenerate = true;
	[Export] public bool GenerateOnReady = true;
	[Export] public int LevelBiomeId = 0;
	[Export] public bool RandomizeBiomeOnReady = true;

	[ExportGroup("Biome Chance On Ready")]
	[Export(PropertyHint.Range, "0,1,0.01")] public float ForestBiomeChance = 0.34f;
	[Export(PropertyHint.Range, "0,1,0.01")] public float DarkForestBiomeChance = 0.70f;
	[Export(PropertyHint.Range, "0,1,0.01")] public float SnowForestBiomeChance = 0.18f;
	[Export(PropertyHint.Range, "0,1,0.01")] public float DesertBiomeChance = 0.16f;
	[Export(PropertyHint.Range, "0,1,0.01")] public float SnowlandsBiomeChance = 0.14f;

	[ExportGroup("Loading Screen")]
	[Export] public bool UseFakeLoadingSmoothing = true;
	[Export(PropertyHint.Range, "0.01,6,0.01")] public float FakeLoadingCatchupSpeed = 0.90f;
	[Export(PropertyHint.Range, "0.01,1,0.01")] public float FakeLoadingMaxStepPerFrame = 0.45f;
	[Export(PropertyHint.Range, "20,3000,1")] public int ApplyChunkCopyBatch = 20;
	[Export(PropertyHint.Range, "20,3000,1")] public int LoadChunkTileBatch = 120;
	[Export(PropertyHint.Range, "1,200,1")] public int InitialVisibleChunksBatch = 1;

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
	[Export] public Godot.Collections.Array<Vector2I> EdgeDecorAtlases = new()
	{
		new Vector2I(6, 2),
		new Vector2I(7, 2),
		new Vector2I(8, 2),
	};

	[ExportGroup("Forest - Small Details")]
	[Export] public Godot.Collections.Array<Vector2I> ForestSmallDetailAtlases = new()
	{
		new Vector2I(38, 11),
		new Vector2I(39, 6),
		new Vector2I(36, 6),
		new Vector2I(34, 0),
		new Vector2I(34, 1),
		new Vector2I(35, 0),
		new Vector2I(35, 1),
		new Vector2I(36, 0),
		new Vector2I(36, 1),
		new Vector2I(37, 0),
		new Vector2I(37, 1),
		new Vector2I(38, 0),
		new Vector2I(38, 1),
		new Vector2I(39, 0),
		new Vector2I(39, 0),
	};

	[ExportGroup("Dark Forest - Small Details")]
	[Export] public Godot.Collections.Array<Vector2I> DarkForestSmallDetailAtlases = new()
	{
		new Vector2I(34, 2),
		new Vector2I(35, 2),
		new Vector2I(36, 2),
		new Vector2I(37, 2),
		new Vector2I(38, 2),
		new Vector2I(39, 2),
		new Vector2I(38, 3),
		new Vector2I(39, 3),
		new Vector2I(37, 7),
		new Vector2I(35, 8),
		new Vector2I(39, 10),
	};

	[ExportGroup("Forest - Medium Details")]
	[Export] public Godot.Collections.Array<Vector2I> ForestMediumDetailAtlases = new()
	{
		new Vector2I(33, 9),
		new Vector2I(34, 9),
		new Vector2I(35, 9),
		new Vector2I(36, 9),
		new Vector2I(37, 9),
		new Vector2I(38, 9),
	};

	[ExportGroup("Dark Forest - Medium Details")]
	[Export] public Godot.Collections.Array<Vector2I> DarkForestMediumDetailAtlases = new()
	{
		new Vector2I(35, 9),
		new Vector2I(37, 9),
		new Vector2I(39, 9),
	};

	[ExportGroup("Forest - Trees")]
	[Export] public Godot.Collections.Array<Vector2I> ForestTreeAtlases = new()
	{
		new Vector2I(24, 33),
		new Vector2I(29, 33),
		new Vector2I(34, 33),
		new Vector2I(36, 34),
		new Vector2I(38, 34),
	};

	[ExportGroup("Dark Forest - Trees")]
	[Export] public Godot.Collections.Array<Vector2I> DarkForestTreeAtlases = new()
	{
		new Vector2I(24, 33),
		new Vector2I(29, 33),
		new Vector2I(34, 33),
		new Vector2I(36, 34),
		new Vector2I(38, 34),
	};

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
	[Export] public Godot.Collections.Array<Vector2I> SnowForestSmallDetailAtlases = new()
	{
		new Vector2I(9, 1),
		new Vector2I(10, 1),
		new Vector2I(11, 1),
	};

	[ExportGroup("Desert - Small Details")]
	[Export] public Godot.Collections.Array<Vector2I> DesertSmallDetailAtlases = new()
	{
		new Vector2I(12, 1),
		new Vector2I(13, 1),
		new Vector2I(14, 1),
	};

	[ExportGroup("Snowlands - Small Details")]
	[Export] public Godot.Collections.Array<Vector2I> SnowlandsSmallDetailAtlases = new()
	{
		new Vector2I(15, 1),
		new Vector2I(16, 1),
		new Vector2I(17, 1),
	};

	[ExportGroup("Snow Forest - Medium Details")]
	[Export] public Godot.Collections.Array<Vector2I> SnowForestMediumDetailAtlases = new()
	{
		new Vector2I(8, 2),
		new Vector2I(9, 2),
		new Vector2I(10, 2),
	};

	[ExportGroup("Desert - Medium Details")]
	[Export] public Godot.Collections.Array<Vector2I> DesertMediumDetailAtlases = new()
	{
		new Vector2I(11, 2),
		new Vector2I(12, 2),
		new Vector2I(13, 2),
	};

	[ExportGroup("Snowlands - Medium Details")]
	[Export] public Godot.Collections.Array<Vector2I> SnowlandsMediumDetailAtlases = new()
	{
		new Vector2I(14, 2),
		new Vector2I(15, 2),
		new Vector2I(16, 2),
	};

	[ExportGroup("Snow Forest - Trees")]
	[Export] public Godot.Collections.Array<Vector2I> SnowForestTreeAtlases = new()
	{
		new Vector2I(6, 3),
		new Vector2I(7, 3),
		new Vector2I(8, 3),
	};

	[ExportGroup("Desert - Trees")]
	[Export] public Godot.Collections.Array<Vector2I> DesertTreeAtlases = new()
	{
	};

	[ExportGroup("Snowlands - Trees")]
	[Export] public Godot.Collections.Array<Vector2I> SnowlandsTreeAtlases = new()
	{
		new Vector2I(9, 3),
		new Vector2I(10, 3),
		new Vector2I(11, 3),
	};

	[ExportGroup("Snowlands - Normal Pine Trees")]
	[Export] public Godot.Collections.Array<Vector2I> SnowlandsNormalPineTreeAtlases = new()
	{
		new Vector2I(0, 3),
		new Vector2I(1, 3),
		new Vector2I(2, 3),
	};

	[ExportGroup("Ground Context")]
	[Export] public Godot.Collections.Array<Vector2I> DirtAtlases = new()
	{
		new Vector2I(5, 0),
		new Vector2I(1, 1),
	};

	[ExportGroup("Atlas - Shadows")]
	[Export] public Vector2I TreeShadowAtlas = new(34, 22);
	[Export] public Vector2I ShadowOffset = new(0, 1);

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
	public volatile int _threadStageIndex = 0;
	public volatile int _threadStageCount = 1;
	public volatile int _threadStageCurrent = 0;
	public volatile int _threadStageMax = 1;
	public volatile int _threadStageDetailPercent = 0;
	public volatile string _threadStageTitle = "preparando geração...";
	public volatile string _threadStageSubText = "iniciando...";

	private float _displayedLoadingProgress = 0f;
	private float _lastTargetProgress = 0f;

	public Dictionary<Vector2I, RubyChunkData> chunksDictionary = new();
	[Export(PropertyHint.Range, "10,5000,1")] public int PortalSearchYieldEvery = 250;
	[Export(PropertyHint.Range, "10,5000,1")] public int UnloadChunkTileBatch = 250;

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

			if (RandomizeBiomeOnReady)
				RandomizeBiomeByChance();

			_initialPortalScene = ResourceLoader.Load<PackedScene>(InitialPortalScenePath);
			_exitPortalScene = ResourceLoader.Load<PackedScene>(ExitPortalScenePath);

			SetImmediateLoading("preparando geração...", $"seed {SeedValue} | bioma {GetBiomeDisplayName()}", 0f);

			if (ClearBeforeGenerate)
			{
				ClearAll();
				await YieldFrame();
			}

			SetImmediateLoading("organizando mundo...", "criando grid de chunks...", 2f);
			CreateChunksGrid();
			await YieldFrame();

			_threadProgress = 0;
			_threadMaxProgress = Mathf.Max(1, LevelSizeY);
			_threadChunkProgress = 0;
			_threadChunkMaxProgress = Mathf.Max(1, chunksDictionary.Count);
			_threadCurrentChunkX = 0;
			_threadCurrentChunkY = 0;
			_threadStageIndex = 0;
			_threadStageCount = 11;
			_threadStageCurrent = 0;
			_threadStageMax = 1;
			_threadStageTitle = "gerando mundo...";
			_threadStageSubText = "aquecendo thread...";
			_threadStageDetailPercent = 0;

			var threadedTask = Task.Run(GenerateChunksDataThreaded);

			while (!threadedTask.IsCompleted)
			{
				UpdateLoadingScreenSmooth();
				await YieldFrame();
			}

			RubyThreadGenerationResult threadedResult = await threadedTask;
			UpdateLoadingScreenSmooth(true);
			await YieldFrame();

			SetImmediateLoading("materializando mundo...", "copiando dados de chunk pro runtime...", 90.5f);
			await ApplyThreadedChunksDataAsync(threadedResult);

			SetImmediateLoading("rasgando o primeiro portal...", "procurando ponto inicial...", 93f);
			await SpawnInitialPortalAsync();

			SetImmediateLoading("rasgando o portal de saída...", "procurando ponto distante...", 94f);
			await SpawnExitPortalAsync();

			SetImmediateLoading("trazendo o mundo à vista...", "renderizando chunks próximos...", 95f);
			await UpdateVisibleChunksAsync(Vector2.Zero);

			SetImmediateLoading("despertando criaturas...", "espalhando ameaças...", 98f);
			await SpawnEnemysAsync();

			SetImmediateLoading("mundo pronto", $"ruby gen finalizado | {GetBiomeDisplayName()}", 100f);
			await YieldFrame();
			await YieldFrame();
			LoadingScreen.I?.HideLoading();

			GD.Print($"RubyGen: generation finished. Seed={SeedValue} biome={LevelBiomeId} chunks={chunksDictionary.Count}");
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

	public void RandomizeBiomeByChance()
	{
		float forest = Mathf.Max(0f, ForestBiomeChance);
		float darkForest = Mathf.Max(0f, DarkForestBiomeChance);
		float snowForest = Mathf.Max(0f, SnowForestBiomeChance);
		float desert = Mathf.Max(0f, DesertBiomeChance);
		float snowlands = Mathf.Max(0f, SnowlandsBiomeChance);
		float total = forest + darkForest + snowForest + desert + snowlands;

		if (total <= 0f)
		{
			LevelBiomeId = 0;
			return;
		}

		var rng = new RandomNumberGenerator();
		rng.Randomize();
		float roll = rng.RandfRange(0f, total);

		if (roll <= forest)
			LevelBiomeId = 0;
		else if (roll <= forest + darkForest)
			LevelBiomeId = 1;
		else if (roll <= forest + darkForest + snowForest)
			LevelBiomeId = 2;
		else if (roll <= forest + darkForest + snowForest + desert)
			LevelBiomeId = 3;
		else
			LevelBiomeId = 4;
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

	private void SetImmediateLoading(string text, string subText, float progress)
	{
		_displayedLoadingProgress = progress;
		_lastTargetProgress = progress;
		LoadingScreen.I?.ShowLoading(text, progress);
		LoadingScreen.I?.SetText(text);
		LoadingScreen.I?.SetSubText(subText);
		LoadingScreen.I?.SetProgress(progress);
	}

	private async Task YieldFrame()
	{
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
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

	private void UpdateLoadingScreenSmooth(bool force = false)
	{
		if (LoadingScreen.I == null)
			return;

		int safeStageCount = Mathf.Max(1, _threadStageCount);
		int safeStageIndex = Mathf.Clamp(_threadStageIndex, 0, safeStageCount - 1);
		float stageStart = 4f + (safeStageIndex * (86f / safeStageCount));
		float stageEnd = 4f + ((safeStageIndex + 1) * (86f / safeStageCount));
		float stageProgress = _threadStageMax > 0 ? (float)_threadStageCurrent / _threadStageMax : 0f;
		float targetProgress = Mathf.Clamp(Mathf.Lerp(stageStart, stageEnd, stageProgress), 4f, 90f);
		_lastTargetProgress = Mathf.Max(_lastTargetProgress, targetProgress);

		if (force || !UseFakeLoadingSmoothing)
		{
			_displayedLoadingProgress = _lastTargetProgress;
		}
		else
		{
			float diff = _lastTargetProgress - _displayedLoadingProgress;
			if (diff > 0f)
			{
				float step = Mathf.Clamp(diff * FakeLoadingCatchupSpeed, 0.02f, FakeLoadingMaxStepPerFrame);
				_displayedLoadingProgress = Mathf.Min(_displayedLoadingProgress + step, _lastTargetProgress);
			}
		}

		string subText =
			$"{_threadStageSubText} | etapa {_threadStageIndex + 1}/{safeStageCount} | {_threadStageCurrent}/{Mathf.Max(1, _threadStageMax)}";

		if (_threadStageIndex >= safeStageCount - 2)
			subText += $" | chunk ({_threadCurrentChunkX}, {_threadCurrentChunkY}) | {_threadChunkProgress}/{Mathf.Max(1, _threadChunkMaxProgress)}";

		LoadingScreen.I.SetText(_threadStageTitle);
		LoadingScreen.I.SetSubText(subText);
		LoadingScreen.I.SetProgress(_displayedLoadingProgress);
	}

	private async Task ApplyThreadedChunksDataAsync(RubyThreadGenerationResult threadedResult)
	{
		int total = threadedResult.Chunks.Count;
		int safeBatch = Mathf.Max(1, ApplyChunkCopyBatch);

		LoadingScreen.I?.SetText("materializando mundo...");
		LoadingScreen.I?.SetSubText($"sincronizando chunks: 0 / {total}");
		LoadingScreen.I?.SetProgress(90.5f);

		for (int i = 0; i < Mathf.Max(2, total / safeBatch); i++)
		{
			float fakeStep = Mathf.Lerp(90.5f, 92.8f, (i + 1f) / Mathf.Max(2f, total / (float)safeBatch));
			LoadingScreen.I?.SetSubText($"sincronizando chunks: {Mathf.Min(total, (i + 1) * safeBatch)} / {total}");
			LoadingScreen.I?.SetProgress(fakeStep);
			await YieldFrame();
		}

		chunksDictionary = threadedResult.Chunks;

		LoadingScreen.I?.SetSubText($"chunks sincronizados: {total} / {total}");
		LoadingScreen.I?.SetProgress(93f);
		await YieldFrame();
	}

	private async Task LoadChunkAsync(Vector2I chunkCoord)
	{
		if (!chunksDictionary.TryGetValue(chunkCoord, out RubyChunkData chunk))
			return;

		int safeBatch = Mathf.Max(1, LoadChunkTileBatch);
		int ops = 0;

		foreach (RubyChunkTileData tile in chunk.GroundTiles)
		{
			_ground.SetCell(tile.Cell, 0, tile.Atlas);
			ops++;
			if (ops % safeBatch == 0)
				await YieldFrame();
		}

		foreach (RubyChunkTileData tile in chunk.SmallDetailTiles)
		{
			_detailsSmall.SetCell(tile.Cell, 0, tile.Atlas);
			ops++;
			if (ops % safeBatch == 0)
				await YieldFrame();
		}

		foreach (RubyChunkTileData tile in chunk.MediumDetailTiles)
		{
			_detailsMedium.SetCell(tile.Cell, 0, tile.Atlas);
			ops++;
			if (ops % safeBatch == 0)
				await YieldFrame();
		}

		foreach (RubyChunkTileData tile in chunk.ObjectTiles)
		{
			_objects.SetCell(tile.Cell, 0, tile.Atlas);
			ops++;
			if (ops % safeBatch == 0)
				await YieldFrame();
		}

		foreach (RubyChunkTileData tile in chunk.ShadowTiles)
		{
			_shadows.SetCell(tile.Cell, 0, tile.Atlas);
			ops++;
			if (ops % safeBatch == 0)
				await YieldFrame();
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
			LoadingScreen.I?.SetProgress(Mathf.Lerp(98f, 99.8f, EnemysAmount > 0 ? (float)spawnedCount / EnemysAmount : 1f));

			if (spawnedCount % 2 == 0)
				await YieldFrame();
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

		SetThreadStage(0, "definindo o bioma...", $"bioma sorteado: {GetBiomeDisplayName()}", 1, 1);
		SetThreadStage(1, "esculpindo o chão...", "gerando terreno base...", LevelSizeY, 0);

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

			int current = y + halfY + 1;
			_threadProgress = current;
			_threadMaxProgress = Mathf.Max(1, LevelSizeY);
			_threadStageCurrent = current;
			_threadStageMax = Mathf.Max(1, LevelSizeY);
			_threadStageSubText = $"massa de terra: linha {current}/{LevelSizeY}";
		}

		if (GenerateLakes)
		{
			SetThreadStage(2, "abrindo lagos...", "espalhando água rasa e profunda...", LevelSizeY, 0);
			context.GenerateLakesPass(world);
		}

		if (GenerateBeach)
		{
			SetThreadStage(3, "espalhando praia...", "misturando areia perto da água...", LevelSizeY, 0);
			context.GenerateBeachPass(world);
		}

		if (GenerateVoidBorder)
		{
			SetThreadStage(4, "selando as bordas do mundo...", "criando void nas extremidades...", LevelSizeY, 0);
			context.GenerateVoidBorderPass(world);
		}

		if (GenerateSmallDetails)
		{
			SetThreadStage(5, "espalhando detalhes pequenos...", "grama, folhas e miudezas...", LevelSizeY, 0);
			context.GenerateSmallDetailsPass(world);
		}

		if (GenerateMediumDetails)
		{
			SetThreadStage(6, "espalhando detalhes médios...", "pedras, mato denso e variações...", LevelSizeY, 0);
			context.GenerateMediumDetailsPass(world);
		}

		if (GenerateEdgeDecorations)
		{
			SetThreadStage(7, "trabalhando as bordas...", "enfeitando encontros de terreno...", LevelSizeY, 0);
			context.GenerateEdgeDecorationsPass(world);
		}

		if (GenerateTrees)
		{
			SetThreadStage(8, "plantando árvores e sombras...", "checando espaço livre e espaçamento...", Mathf.CeilToInt((float)LevelSizeY / Mathf.Max(1, TreeStep)), 0);
			context.GenerateTreesPass(world);
		}

		SetThreadStage(9, "montando grid e chunk data...", "quebrando o mundo em pedaços...", Mathf.Max(1, chunksDictionary.Count), 0);
		RubyThreadGenerationResult result = BuildChunksFromWorldData(world);
		SetThreadStage(10, "polindo geração...", "fechando pacotes do mapa...", 1, 1);
		return result;
	}

	private void SetThreadStage(int index, string title, string subText, int stageMax, int stageCurrent)
	{
		_threadStageIndex = index;
		_threadStageTitle = title;
		_threadStageSubText = subText;
		_threadStageMax = Mathf.Max(1, stageMax);
		_threadStageCurrent = Mathf.Clamp(stageCurrent, 0, _threadStageMax);
	}

	private RubyThreadGenerationResult BuildChunksFromWorldData(RubyThreadWorldData world)
	{
		var result = new RubyThreadGenerationResult();

		foreach (Vector2I chunkCoord in chunksDictionary.Keys)
			result.Chunks[chunkCoord] = new RubyChunkData();

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

		int processedChunks = 0;
		int totalChunks = Mathf.Max(1, chunksDictionary.Count);

		foreach (var pair in result.Chunks)
		{
			_threadCurrentChunkX = pair.Key.X;
			_threadCurrentChunkY = pair.Key.Y;
			pair.Value.SpawnEnemys = IsSpawnableChunk(pair.Value);

			processedChunks++;
			_threadChunkProgress = processedChunks;
			_threadChunkMaxProgress = totalChunks;
			_threadStageCurrent = processedChunks;
			_threadStageMax = totalChunks;
			_threadStageSubText = $"chunks prontos: {processedChunks}/{totalChunks}";
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

			case RubyLayerType.SmallDetails:
				chunk.SmallDetailTiles.Add(new RubyChunkTileData(cell, atlas));
				break;

			case RubyLayerType.MediumDetails:
				chunk.MediumDetailTiles.Add(new RubyChunkTileData(cell, atlas));
				break;

 			case RubyLayerType.Objects:
				chunk.ObjectTiles.Add(new RubyChunkTileData(cell, atlas));
				chunk.ObjectCellSet.Add(cell);
				break;

			case RubyLayerType.Shadows:
				chunk.ShadowTiles.Add(new RubyChunkTileData(cell, atlas));
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
		int safeBatch = Mathf.Max(1, InitialVisibleChunksBatch);
		int sinceLastYield = 0;

		foreach (Vector2I chunkCoord in neededChunks)
		{
			if (_loadedChunks.Contains(chunkCoord))
				continue;

			await LoadChunkAsync(chunkCoord);
			_loadedChunks.Add(chunkCoord);
			loaded++;
			sinceLastYield++;

			if (LoadingScreen.I != null)
			{
				LoadingScreen.I.SetText("trazendo o mundo à vista...");
				LoadingScreen.I.SetSubText($"renderizando chunks: {loaded} / {total}");
				LoadingScreen.I.SetProgress(Mathf.Lerp(95f, 98f, total > 0 ? (float)loaded / total : 1f));
			}

			if (sinceLastYield >= safeBatch)
			{
				sinceLastYield = 0;
				await YieldFrame();
			}
		}

		foreach (Vector2I chunkCoord in _loadedChunks.ToList())
		{
			if (neededChunks.Contains(chunkCoord))
				continue;

			UnloadChunk(chunkCoord);
			_loadedChunks.Remove(chunkCoord);
			await YieldFrame();
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

	private bool IsForestBiome()
	{
		return LevelBiomeId == 0;
	}

	private bool IsDarkForestBiome()
	{
		return LevelBiomeId == 1;
	}

	private bool IsSnowForestBiome()
	{
		return LevelBiomeId == 2;
	}

	private bool IsDesertBiome()
	{
		return LevelBiomeId == 3;
	}

	private bool IsSnowlandsBiome()
	{
		return LevelBiomeId == 4;
	}

	private Godot.Collections.Array<Vector2I> GetCurrentSmallDetailAtlases()
	{
		return LevelBiomeId switch
		{
			1 => DarkForestSmallDetailAtlases,
			2 => SnowForestSmallDetailAtlases,
			3 => DesertSmallDetailAtlases,
			4 => SnowlandsSmallDetailAtlases,
			_ => ForestSmallDetailAtlases
		};
	}

	private Godot.Collections.Array<Vector2I> GetCurrentMediumDetailAtlases()
	{
		return LevelBiomeId switch
		{
			1 => DarkForestMediumDetailAtlases,
			2 => SnowForestMediumDetailAtlases,
			3 => DesertMediumDetailAtlases,
			4 => SnowlandsMediumDetailAtlases,
			_ => ForestMediumDetailAtlases
		};
	}

	private Godot.Collections.Array<Vector2I> GetCurrentTreeAtlases()
	{
		return LevelBiomeId switch
		{
			1 => DarkForestTreeAtlases,
			2 => SnowForestTreeAtlases,
			3 => DesertTreeAtlases,
			4 => SnowlandsTreeAtlases,
			_ => ForestTreeAtlases
		};
	}

	private Vector2I GetCurrentGroundMainAtlas()
	{
		return LevelBiomeId switch
		{
			1 => DarkForestGroundMainAtlas,
			2 => SnowForestGroundMainAtlas,
			3 => DesertGroundMainAtlas,
			4 => SnowlandsGroundMainAtlas,
			_ => ForestGroundMainAtlas
		};
	}

	private Vector2I GetCurrentGroundAlt1Atlas()
	{
		return LevelBiomeId switch
		{
			1 => DarkForestGroundAlt1Atlas,
			2 => SnowForestGroundAlt1Atlas,
			3 => DesertGroundAlt1Atlas,
			4 => SnowlandsGroundAlt1Atlas,
			_ => ForestGroundAlt1Atlas
		};
	}

	private Vector2I GetCurrentGroundAlt2Atlas()
	{
		return LevelBiomeId switch
		{
			1 => DarkForestGroundAlt2Atlas,
			2 => SnowForestGroundAlt2Atlas,
			3 => DesertGroundAlt2Atlas,
			4 => SnowlandsGroundAlt2Atlas,
			_ => ForestGroundAlt2Atlas
		};
	}

	private bool IsBiomeGroundAtlas(Vector2I atlas)
	{
		return atlas == GetCurrentGroundMainAtlas()
			|| atlas == GetCurrentGroundAlt1Atlas()
			|| atlas == GetCurrentGroundAlt2Atlas();
	}

	private bool IsLakeAtlas(Vector2I atlas)
	{
		return atlas == WaterAtlas
			|| atlas == DeepWaterAtlas
			|| atlas == FrozenWaterAtlas
			|| atlas == FrozenDeepWaterAtlas;
	}

	private Vector2I GetShallowLakeAtlas()
	{
		return IsSnowlandsBiome() ? FrozenWaterAtlas : WaterAtlas;
	}

	private Vector2I GetDeepLakeAtlas()
	{
		return IsSnowlandsBiome() ? FrozenDeepWaterAtlas : DeepWaterAtlas;
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

		return chunk.ValidGroundCellSet.Contains(cell);
	}

 	private bool HasObjectAtCell(Vector2I cell)
	{
		Vector2I chunkCoord = CellToChunk(cell);

		if (!chunksDictionary.TryGetValue(chunkCoord, out RubyChunkData chunk))
			return false;

		return chunk.ObjectCellSet.Contains(cell);
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

	private async Task SpawnInitialPortalAsync()
	{
		if (_initialPortalScene == null)
		{
			GD.PrintErr("RubyGen: initial portal scene null");
			return;
		}

		var rng = new RandomNumberGenerator();
		var candidateChunks = GetCentralChunkCandidates(12);
		var candidateCells = new List<Vector2I>();
		int scanned = 0;
		int totalChunks = Mathf.Max(1, candidateChunks.Count);

		foreach (Vector2I chunkCoord in candidateChunks)
		{
			candidateCells.AddRange(GetValidPortalCellsInChunk(chunkCoord, 2, InitialPortalRadius));
			scanned++;
			LoadingScreen.I?.SetText("rasgando o primeiro portal...");
			LoadingScreen.I?.SetSubText($"vasculhando centro: {scanned}/{totalChunks} chunks");
			LoadingScreen.I?.SetProgress(Mathf.Lerp(93f, 93.8f, scanned / (float)totalChunks));
			await YieldFrame();
		}

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
		LoadingScreen.I?.SetSubText($"portal inicial em {chosen}");
		LoadingScreen.I?.SetProgress(93.9f);
		await YieldFrame();

		GD.Print($"RubyGen: Initial portal generated at {chosen}");
	}

	private async Task SpawnExitPortalAsync()
	{
		if (_exitPortalScene == null)
		{
			GD.PrintErr("RubyGen: exit portal scene null");
			return;
		}

		var rng = new RandomNumberGenerator();
		List<Vector2I> candidates = await CollectExitCandidatesAsync(true, MinExitDistanceFromInitial);

		if (candidates.Count == 0)
			candidates = await CollectExitCandidatesAsync(false, MinExitDistanceFromInitial);

		if (candidates.Count == 0)
		{
			LoadingScreen.I?.SetSubText("fallback do portal de saída...");
			LoadingScreen.I?.SetProgress(94.6f);
			await YieldFrame();

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
		LoadingScreen.I?.SetSubText($"portal de saída em {chosen}");
		LoadingScreen.I?.SetProgress(94.9f);
		await YieldFrame();

		GD.Print($"RubyGen: Exit portal generated at {chosen}");
	}

	private async Task<List<Vector2I>> CollectExitCandidatesAsync(bool requireNearVoid, int minDistanceFromInitial)
	{
		List<Vector2I> result = new();
		var outerChunks = GetOuterChunkCandidates(30);
		int processedChunks = 0;
		int totalChunks = Mathf.Max(1, outerChunks.Count);
		int safeYield = Mathf.Max(1, PortalSearchYieldEvery);
		int ops = 0;

		foreach (Vector2I chunkCoord in outerChunks)
		{
			int startX = chunkCoord.X * ChunkSize;
			int startY = chunkCoord.Y * ChunkSize;

			for (int localY = ExitPortalRadius; localY < ChunkSize - ExitPortalRadius; localY += 2)
			{
				for (int localX = ExitPortalRadius; localX < ChunkSize - ExitPortalRadius; localX += 2)
				{
					Vector2I worldCell = new(startX + localX, startY + localY);

					if (!IsPortalValidAtCell(worldCell))
						goto PortalSearchContinue;

					if (worldCell.DistanceSquaredTo(_initialPortalCell) < minDistanceFromInitial * minDistanceFromInitial)
						goto PortalSearchContinue;

					if (!HasEnoughSpaceForPortal(worldCell, ExitPortalRadius))
						goto PortalSearchContinue;

					if (requireNearVoid && !IsNearVoid(worldCell, 6))
						goto PortalSearchContinue;

					result.Add(worldCell);

				PortalSearchContinue:
					ops++;
					if (ops % safeYield == 0)
					{
						LoadingScreen.I?.SetText("rasgando o portal de saída...");
						LoadingScreen.I?.SetSubText($"varrendo bordas: {processedChunks}/{totalChunks} chunks | candidatos {result.Count}");
						LoadingScreen.I?.SetProgress(Mathf.Lerp(94f, 94.7f, processedChunks / (float)totalChunks));
						await YieldFrame();
					}
				}
			}

			processedChunks++;
			LoadingScreen.I?.SetText("rasgando o portal de saída...");
			LoadingScreen.I?.SetSubText($"varrendo bordas: {processedChunks}/{totalChunks} chunks | candidatos {result.Count}");
			LoadingScreen.I?.SetProgress(Mathf.Lerp(94f, 94.7f, processedChunks / (float)totalChunks));
			await YieldFrame();
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

				if (!chunk.GroundAtlasByCell.TryGetValue(check, out Vector2I atlas))
					return true;

				if (atlas == VoidAtlas)
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

					world.Ground[cell] = nearNonLakeGround ? _owner.GetShallowLakeAtlas() : _owner.GetDeepLakeAtlas();
				}

				int current = y + halfY + 1;
				_owner._threadStageCurrent = current;
				_owner._threadStageMax = Mathf.Max(1, _owner.LevelSizeY);
				_owner._threadStageSubText = $"escavando água: linha {current}/{_owner.LevelSizeY}";
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

				int current = y + halfY + 1;
				_owner._threadStageCurrent = current;
				_owner._threadStageMax = Mathf.Max(1, _owner.LevelSizeY);
				_owner._threadStageSubText = $"misturando areia: linha {current}/{_owner.LevelSizeY}";
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

				int current = y + halfY + 1;
				_owner._threadStageCurrent = current;
				_owner._threadStageMax = Mathf.Max(1, _owner.LevelSizeY);
				_owner._threadStageSubText = $"borda do vazio: linha {current}/{_owner.LevelSizeY}";
			}
		}

		public void GenerateSmallDetailsPass(RubyThreadWorldData world)
		{
			var smallDetailAtlases = _owner.GetCurrentSmallDetailAtlases();
			if (smallDetailAtlases.Count == 0)
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

					Vector2I atlas = PickAtlasFromList(smallDetailAtlases, x, y, 7777);
					world.SmallDetails[cell] = atlas;
				}

				int current = y + halfY + 1;
				_owner._threadStageCurrent = current;
				_owner._threadStageMax = Mathf.Max(1, _owner.LevelSizeY);
				_owner._threadStageSubText = $"detalhes pequenos: linha {current}/{_owner.LevelSizeY}";
			}
		}

		public void GenerateMediumDetailsPass(RubyThreadWorldData world)
		{
			var mediumDetailAtlases = _owner.GetCurrentMediumDetailAtlases();
			if (mediumDetailAtlases.Count == 0)
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

					Vector2I atlas = PickAtlasFromList(mediumDetailAtlases, x, y, 9999);
					world.MediumDetails[cell] = atlas;
				}

				int current = y + halfY + 1;
				_owner._threadStageCurrent = current;
				_owner._threadStageMax = Mathf.Max(1, _owner.LevelSizeY);
				_owner._threadStageSubText = $"detalhes médios: linha {current}/{_owner.LevelSizeY}";
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

				int current = y + halfY + 1;
				_owner._threadStageCurrent = current;
				_owner._threadStageMax = Mathf.Max(1, _owner.LevelSizeY);
				_owner._threadStageSubText = $"enfeites de borda: linha {current}/{_owner.LevelSizeY}";
			}
		}

		public void GenerateTreesPass(RubyThreadWorldData world)
		{
			var treeAtlases = _owner.GetCurrentTreeAtlases();
			if (treeAtlases.Count == 0)
				return;

			int halfX = _owner.LevelSizeX / 2;
			int halfY = _owner.LevelSizeY / 2;
			int safeTreeStep = Mathf.Max(1, _owner.TreeStep);
			int lineIndex = 0;

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

					Vector2I treeAtlas = PickTreeAtlasForCell(world, offsetCell);
					if (treeAtlas == Vector2I.Zero)
						continue;

					world.Objects[offsetCell] = treeAtlas;
					world.Shadows[offsetCell + _owner.ShadowOffset] = _owner.TreeShadowAtlas;

					_treePositions.Add(offsetCell);
				}

				lineIndex++;
				_owner._threadStageCurrent = lineIndex;
				_owner._threadStageMax = Mathf.Max(1, Mathf.CeilToInt((float)_owner.LevelSizeY / safeTreeStep));
				_owner._threadStageSubText = $"plantando árvores: faixa {lineIndex}/{_owner._threadStageMax}";
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

		public bool IsGroundMainCell(RubyThreadWorldData world, Vector2I cell)
		{
			if (!world.Ground.TryGetValue(cell, out Vector2I atlas))
				return false;

			return atlas == _owner.GetCurrentGroundMainAtlas();
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

			return _owner.IsBiomeGroundAtlas(atlas);
		}

		public bool IsRealTerrainCell(RubyThreadWorldData world, Vector2I cell)
		{
			if (!world.Ground.TryGetValue(cell, out Vector2I atlas))
				return false;

			if (_owner.IsBiomeGroundAtlas(atlas))
				return true;

			if (atlas == _owner.SandAtlas || _owner.IsLakeAtlas(atlas) || atlas == _owner.VoidAtlas)
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

					if (_owner.IsLakeAtlas(atlas))
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

		public Vector2I PickTreeAtlasForCell(RubyThreadWorldData world, Vector2I cell)
		{
			if (_owner.IsSnowlandsBiome())
			{
				if (world.Ground.TryGetValue(cell, out Vector2I groundAtlas) && groundAtlas == _owner.SnowlandsGroundAlt1Atlas)
				{
					if (_owner.SnowlandsNormalPineTreeAtlases.Count > 0)
						return PickAtlasFromList(_owner.SnowlandsNormalPineTreeAtlases, cell.X, cell.Y, 33333);
				}

				if (_owner.SnowlandsTreeAtlases.Count > 0)
					return PickAtlasFromList(_owner.SnowlandsTreeAtlases, cell.X, cell.Y, 22222);

				return Vector2I.Zero;
			}

			var treeAtlases = _owner.GetCurrentTreeAtlases();
			if (treeAtlases.Count == 0)
				return Vector2I.Zero;

			return PickAtlasFromList(treeAtlases, cell.X, cell.Y, 22222);
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
			if (_owner.IsDarkForestBiome())
			{
				if (groundValue >= _owner.GroundAlt2Threshold)
					return _owner.DarkForestGroundAlt2Atlas;

				if (groundValue >= _owner.GroundAlt1Threshold)
					return _owner.DarkForestGroundAlt1Atlas;

				return _owner.DarkForestGroundMainAtlas;
			}

			if (_owner.IsSnowForestBiome())
			{
				if (groundValue >= _owner.GroundAlt2Threshold)
					return _owner.SnowForestGroundAlt2Atlas;

				if (groundValue >= _owner.GroundAlt1Threshold)
					return _owner.SnowForestGroundAlt1Atlas;

				return _owner.SnowForestGroundMainAtlas;
			}

			if (_owner.IsDesertBiome())
			{
				if (groundValue >= _owner.GroundAlt2Threshold)
					return _owner.DesertGroundAlt2Atlas;

				if (groundValue >= _owner.GroundAlt1Threshold)
					return _owner.DesertGroundAlt1Atlas;

				return _owner.DesertGroundMainAtlas;
			}

			if (_owner.IsSnowlandsBiome())
			{
				if (groundValue >= 0.93f)
					return _owner.SnowlandsGroundAlt1Atlas;

				if (groundValue >= 0.72f)
					return _owner.SnowlandsGroundAlt2Atlas;

				return _owner.SnowlandsGroundMainAtlas;
			}

			if (groundValue >= _owner.GroundAlt2Threshold)
				return _owner.ForestGroundAlt2Atlas;

			if (groundValue >= _owner.GroundAlt1Threshold)
				return _owner.ForestGroundAlt1Atlas;

			return _owner.ForestGroundMainAtlas;
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
	public HashSet<Vector2I> ValidGroundCellSet = new();
	public HashSet<Vector2I> ObjectCellSet = new();
	public Dictionary<Vector2I, Vector2I> GroundAtlasByCell = new();

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
