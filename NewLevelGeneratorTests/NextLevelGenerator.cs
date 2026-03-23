using Godot;
using Godot.Collections;
using projecthorizonscs.Autoload;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Array = Godot.Collections.Array;

namespace projecthorizonscs;

public partial class NextLevelGenerator : TileMapLayer
{
	private PackedScene _initialPortalScene;
	private Node2D _initialPortalReference;
	private PackedScene _exitPortalScene;
	private Node2D _exitPortalReference;

    private TileMapLayer _detailsTileMap;
	private FastNoiseLite _blocksNoiseImage;
	private FastNoiseLite _detailsNoiseImage;
    
    private Godot.Collections.Dictionary<Vector2I, bool> _validCellCache = new();

    public int chunkSize = 10;
    public int chunksX = 60;
    public int chunksY = 120;

	private float _insideDetails = 40f;
	private float _coastDetails = 90.0f;
	private float _coastStrength = .55f;

    private int _levelSeed = 0;
	private float _blocksFrequency = .01f;
	private int _blocksFractalOctaves = 8;

    private int _levelBiomeId = 0;
    private FastNoiseLite _secondaryBlocksNoise;

	public int LevelBiomeId;

    private HashSet<Vector2I> _loadedChunks = new();
    private Vector2I _currentCenterChunk = new Vector2I(int.MinValue, int.MinValue);

    public int tileSize = 32;

	private Vector2I _initialPortalCell;

    public int EnemysAmount;

    public volatile int _threadProgress = 0;
    public volatile int _threadMaxProgress = 1;
    
    public Godot.Collections.Dictionary<Vector2I, ChunkData> chunksDictionary = new();

    private class ThreadChunkData
    {
        public List<int> blocksID = new();
        public List<int> detailsID = new();
        public bool SpawnEnemys;
    }

    private class ThreadGenerationResult
    {
        public System.Collections.Generic.Dictionary<Vector2I, ThreadChunkData> chunksDictionary = new();
    }

    public override async void _Ready()
    {
        try
        {
            LoadingScreen.I?.ShowLoading("preparando geração...", 0f);
            LoadingScreen.I?.SetSubText("iniciando sistemas...");

            GD.Print("Loading packed scenes and references");
            _initialPortalScene = (PackedScene)ResourceLoader.Load("res://Portal/InitialPortal.tscn");
            _exitPortalScene = (PackedScene)ResourceLoader.Load("res://Portal/ExitPortal.tscn");

            _detailsTileMap = GetNode<TileMapLayer>("Details");

            GD.Print("Setting Biome");
            LoadingScreen.I?.SetText("definindo mundo...");
            LoadingScreen.I?.SetSubText("selecionando biome...");
            LoadingScreen.I?.SetProgress(2f);
            SetBiome();

            GD.Print("Setting Seeds");
            LoadingScreen.I?.SetText("definindo mundo...");
            LoadingScreen.I?.SetSubText("gerando seeds...");
            LoadingScreen.I?.SetProgress(4f);
            SetSeeds();

            GD.Print("Setting noises");
            LoadingScreen.I?.SetText("preparando geração...");
            LoadingScreen.I?.SetSubText("configurando noises...");
            LoadingScreen.I?.SetProgress(6f);
            SetNoises();

            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            GD.Print("Generating Chunk grid");
            LoadingScreen.I?.SetText("organizando mundo...");
            LoadingScreen.I?.SetSubText("criando grid de chunks...");
            LoadingScreen.I?.SetProgress(8f);
            CreateChunksGrid();

            GD.Print("Generating Chunk data");
            LoadingScreen.I?.SetText("gerando mundo...");
            LoadingScreen.I?.SetSubText($"chunks: 0 / {chunksDictionary.Count}");
            LoadingScreen.I?.SetProgress(10f);

            _threadProgress = 0;
            _threadMaxProgress = chunksDictionary.Count;

            var threadedTask = Task.Run(() => GenerateChunksDataThreaded());

            while (!threadedTask.IsCompleted)
            {
                UpdateLoadingScreen();
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }

            var threadedResult = await threadedTask;

            GD.Print("Thread finished");
            LoadingScreen.I?.SetText("finalizando geração...");
            LoadingScreen.I?.SetSubText("aplicando dados gerados...");
            LoadingScreen.I?.SetProgress(91f);

            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            LoadingScreen.I?.SetText("carregando vegetação...");
            LoadingScreen.I?.SetSubText("aplicando árvores e detalhes...");
            LoadingScreen.I?.SetProgress(92f);

            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            ApplyThreadedChunksData(threadedResult);
            GD.Print("Chunk data generated");

            GD.Print("Creating portals");
            LoadingScreen.I?.SetText("posicionando portais...");
            LoadingScreen.I?.SetSubText("portais: 0 / 2");
            LoadingScreen.I?.SetProgress(95f);

            SpawnInitialPortal();
            LoadingScreen.I?.SetSubText("portais: 1 / 2");

            SpawnExitPortal();
            LoadingScreen.I?.SetSubText("portais: 2 / 2");

            LoadingScreen.I?.SetText("carregando mundo...");
            LoadingScreen.I?.SetSubText("carregando chunks visíveis...");
            LoadingScreen.I?.SetProgress(97f);

            UpdateVisibleChunks(Vector2.Zero);
            GD.Print("Visible chunks loaded");

            GD.Print("Spawning enemys");
            LoadingScreen.I?.SetText("despertando criaturas...");
            LoadingScreen.I?.SetSubText("inimigos: 0 / ?");
            LoadingScreen.I?.SetProgress(99f);

            SpawnEnemys();
            GD.Print("Enemys spawned");

            LoadingScreen.I?.SetText("pronto");
            LoadingScreen.I?.SetSubText("mundo carregado");
            LoadingScreen.I?.SetProgress(100f);

            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            LoadingScreen.I?.HideLoading();
        }
        catch (Exception e)
        {
            GD.PrintErr($"READY ERROR: {e}");
            LoadingScreen.I?.HideLoading();
        }
    }

    public void SpawnEnemys()
    {
        EnemysAmount = (int)GD.RandRange(
            0,
            (chunksDictionary.Keys.Count / 1000)
            * ((int)DataManager.I.CurrentWorldData["SaveDifficulty"] + 1)
            / 2
        );

        GD.Print($"Enemys To Spawn {EnemysAmount}");

        LoadingScreen.I?.SetText("despertando criaturas...");
        LoadingScreen.I?.SetSubText($"inimigos: 0 / {EnemysAmount}");
        LoadingScreen.I?.SetProgress(99f);

        int spawnedCount = 0;

        for (int i = 0; i < EnemysAmount; i++)
        {
            Vector2 spawnPosition = GetRandomSpawnPosition();

            if (spawnPosition == Vector2.Zero)
                continue;

            var newEnemy = EnemysManager.I.GetRandomEnemyByChance(LevelBiomeId);

            if (string.IsNullOrEmpty(newEnemy))
                continue;

            EnemysManager.I.SpawnEnemy(newEnemy, spawnPosition);

            spawnedCount++;
            LoadingScreen.I?.SetSubText($"inimigos: {spawnedCount} / {EnemysAmount}");
            GD.Print("Enemy spawned");
        }
    }

    public Vector2 GetRandomSpawnPosition()
    {
        var spawnableChunks = new Godot.Collections.Array<ChunkData>();
        var spawnableChunksGridPositions = new Godot.Collections.Array<Vector2I>();

        Vector2? playerPosition = null;

        if (Globals.I != null && Globals.I.LocalPlayer != null)
            playerPosition = Globals.I.LocalPlayer.GlobalPosition;

        foreach (Vector2I gridPosition in _loadedChunks)
        {
            if (!chunksDictionary.ContainsKey(gridPosition))
                continue;

            ChunkData chunk = chunksDictionary[gridPosition];

            if (!chunk.SpawnEnemys)
                continue;

            if (playerPosition != null)
            {
                Vector2 chunkWorldCenter = ((Vector2)(gridPosition * chunkSize * tileSize)) +
                                        new Vector2(chunkSize * tileSize / 2f, chunkSize * tileSize / 2f);

                if (chunkWorldCenter.DistanceTo(playerPosition.Value) < 500f)
                    continue;
            }

            spawnableChunks.Add(chunk);
            spawnableChunksGridPositions.Add(gridPosition);
        }

        if (spawnableChunks.Count == 0)
            return Vector2.Zero;

        int chunkToSpawn = (int)GD.RandRange(0, spawnableChunks.Count - 1);

        return GetRandomPositionInChunk(
            spawnableChunks[chunkToSpawn],
            spawnableChunksGridPositions[chunkToSpawn]
        );
    }

    public Vector2 GetRandomPositionInChunk(ChunkData chunk, Vector2I chunkGridPosition)
    {
        float x = (float)GD.RandRange(0, chunkSize * tileSize);
        float y = (float)GD.RandRange(0, chunkSize * tileSize);

        return (Vector2)(chunkGridPosition * chunkSize * tileSize) + new Vector2(x, y);
    }

    public override void _Process(double delta)
    {
        if (Globals.I == null || Globals.I.LocalPlayer == null)
            return;

        var player = Globals.I.LocalPlayer;

        Vector2I playerCell = LocalToMap(ToLocal(player.GlobalPosition));
        Vector2I newCenterChunk = WorldToChunk(playerCell);

        if (newCenterChunk != _currentCenterChunk)
        {
            _currentCenterChunk = newCenterChunk;
            UpdateVisibleChunks(playerCell);
        }
    }

    public void SetSeeds()
    {
		var rng = new RandomNumberGenerator();
		_levelSeed = rng.RandiRange(0, 99999999);
    }

    public void SetBiome()
    {
        _levelBiomeId = 0;
        LevelBiomeId = _levelBiomeId;
    }

    public void SetNoises()
    {
		_blocksNoiseImage = new();
		_blocksNoiseImage.NoiseType = FastNoiseLite.NoiseTypeEnum.ValueCubic;
		_blocksNoiseImage.Seed = _levelSeed;
		_blocksNoiseImage.FractalOctaves = _blocksFractalOctaves;
		_blocksNoiseImage.Frequency = .01f;

		_detailsNoiseImage = new();
		_detailsNoiseImage.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
		_detailsNoiseImage.Seed = _levelSeed + 1;
		_detailsNoiseImage.Frequency = 0.2f;

        _secondaryBlocksNoise = new();
        _secondaryBlocksNoise.Seed = _levelSeed + 2;
        _secondaryBlocksNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
    }

    private void UpdateLoadingScreen()
    {
        if (LoadingScreen.I == null)
            return;

        float progress = 0f;

        if (_threadMaxProgress > 0)
            progress = (float)_threadProgress / _threadMaxProgress;

        float totalProgress = Mathf.Clamp(10f + (progress * 80f), 10f, 90f);

        string mainText;

        if (progress <= 0.08f)
            mainText = "gerando mundo...";
        else if (progress <= 0.20f)
            mainText = "moldando terreno...";
        else if (progress <= 0.35f)
            mainText = "esculpindo relevo...";
        else if (progress <= 0.50f)
            mainText = "preenchendo o mapa...";
        else if (progress <= 0.62f)
            mainText = "plantando árvores...";
        else if (progress <= 0.75f)
            mainText = "espalhando vegetação...";
        else if (progress <= 0.88f)
            mainText = "colocando detalhes...";
        else if (progress <= 0.96f)
            mainText = "carregando mundo...";
        else
            mainText = "quase pronto...";

        LoadingScreen.I.SetText(mainText);
        LoadingScreen.I.SetSubText($"chunks: {_threadProgress} / {_threadMaxProgress}");
        LoadingScreen.I.SetProgress(totalProgress);
    }

    private ThreadGenerationResult GenerateChunksDataThreaded()
    {
        try
        {
            var result = new ThreadGenerationResult();

            var blocksNoiseImage = new FastNoiseLite();
            blocksNoiseImage.NoiseType = FastNoiseLite.NoiseTypeEnum.ValueCubic;
            blocksNoiseImage.Seed = _levelSeed;
            blocksNoiseImage.FractalOctaves = _blocksFractalOctaves;
            blocksNoiseImage.Frequency = .01f;

            var detailsNoiseImage = new FastNoiseLite();
            detailsNoiseImage.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
            detailsNoiseImage.Seed = _levelSeed + 1;
            detailsNoiseImage.Frequency = 0.2f;

            var secondaryBlocksNoise = new FastNoiseLite();
            secondaryBlocksNoise.Seed = _levelSeed + 2;
            secondaryBlocksNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;

            var chunkCoords = chunksDictionary.Keys.ToArray();

            GD.Print($"thread start: {chunkCoords.Length} chunks");

            for (int i = 0; i < chunkCoords.Length; i++)
            {
                var chunkCoordinade = chunkCoords[i];
                var chunkData = new ThreadChunkData();

                for (int localY = 0; localY < chunkSize; localY++)
                {
                    for (int localX = 0; localX < chunkSize; localX++)
                    {
                        int worldX = chunkCoordinade.X * chunkSize + localX;
                        int worldY = chunkCoordinade.Y * chunkSize + localY;

                        int blockID = GenerateBlockIdThreaded(worldX, worldY, blocksNoiseImage, secondaryBlocksNoise);
                        chunkData.blocksID.Add(blockID);

                        int detailID = GenerateDetailIdThreaded(worldX, worldY, blockID, detailsNoiseImage);
                        chunkData.detailsID.Add(detailID);
                    }
                }

                chunkData.SpawnEnemys = IsSpawnableChunkThreaded(chunkData.blocksID);
                result.chunksDictionary[chunkCoordinade] = chunkData;

                _threadProgress = i + 1;

                if (i % 1000 == 0)
                    GD.Print($"thread progress: {i}/{chunkCoords.Length}");
            }

            GD.Print("thread end");
            return result;
        }
        catch (Exception e)
        {
            GD.PrintErr($"THREAD ERROR: {e}");
            throw;
        }
    }

    private void ApplyThreadedChunksData(ThreadGenerationResult threadedResult)
    {
        chunksDictionary.Clear();

        foreach (var pair in threadedResult.chunksDictionary)
        {
            var chunkData = new ChunkData();

            foreach (var blockId in pair.Value.blocksID)
                chunkData.blocksID.Add(blockId);

            foreach (var detailsId in pair.Value.detailsID)
                chunkData.detailsID.Add(detailsId);

            chunkData.SpawnEnemys = pair.Value.SpawnEnemys;

            chunksDictionary[pair.Key] = chunkData;
        }
    }

    private bool IsSpawnableChunkThreaded(List<int> blocksID)
    {
        var IsSpawnable = true;

        foreach (int tile in blocksID)
        {
            if (tile == 101 || tile == 160)
                IsSpawnable = false;
        }

        return IsSpawnable;
    }

    private int GenerateBlockIdThreaded(int x, int y, FastNoiseLite blocksNoiseImage, FastNoiseLite secondaryBlocksNoise)
    {
        var levelRadius = Mathf.Min(chunksX * chunkSize, chunksY * chunkSize) * 0.5f;

        Vector2 p = new(x, y);

        var dist = p.Length();
        var angle = Mathf.Atan2(p.Y, p.X);
        
        var ax = Mathf.Cos(angle) * _coastDetails;
        var ay = Mathf.Sin(angle) * _coastDetails;

        var coastN = blocksNoiseImage.GetNoise2D(ax + 123.4f, ay - 567.8f);
        var coast01 = (coastN + 1f) * .5f;
        var radius = levelRadius * (1f + (coast01 - .5f) * 2f * _coastStrength);

        var mask = 1f - (dist / radius);
        mask = Mathf.Clamp(mask, 0f, 1f);
        mask *= mask;

        var n = blocksNoiseImage.GetNoise2D(x * _insideDetails, y * _insideDetails);
        var inside01 = (n + 1f) * .5f;

        var value = mask * (.65f + inside01 * .35f);

        switch (_levelBiomeId)
        {
            case 0:
                float lakeNoise = secondaryBlocksNoise.GetNoise2D(x * 0.35f, y * 0.35f);

                if (value > .40f)
                {
                    if (lakeNoise > .55f) return 161;
                    if (lakeNoise > .35f) return 160;

                    if (value > .45f) return 100;
                    return 101;
                }

                return -1;
            case 1:
                if (value > .45f) return 110;
                if (value > .40f) return 101;
                return -1;
            case 2:
                if (value > .45f) return 120;
                if (value > .40f) return 121;
                return -1;
            case 3:
                if (value > .40f && secondaryBlocksNoise.GetNoise2D(x, y) > .15f && secondaryBlocksNoise.GetNoise2D(x, y) < .65f) return 130;
                if (value > .40f) return 130;
                if (value > .35f) return 131;
                return -1;
            case 4:
                if (value > .40f && secondaryBlocksNoise.GetNoise2D(x, y) > .35f) return 140;
                if (value > .40f && secondaryBlocksNoise.GetNoise2D(x, y) > .25f && secondaryBlocksNoise.GetNoise2D(x, y) < .35f) return 141;
                if (value > .40f) return 140;
                if (value > .35f) return 141;
                return -1;
            case 5:
                if (value > .40f && secondaryBlocksNoise.GetNoise2D(x, y) > .35f) return 150;
                if (value > .40f && secondaryBlocksNoise.GetNoise2D(x, y) > .27f && secondaryBlocksNoise.GetNoise2D(x, y) < .35f) return 151;
                if (value > .40f) return 150;
                if (value > .35f) return 151;
                return -1;
        }

        return -1;
    }

    private int GenerateDetailIdThreaded(int x, int y, int blockId, FastNoiseLite detailsNoiseImage)
    {
        if (blockId == -1)
            return -1;

        float value = detailsNoiseImage.GetNoise2D(x, y);

        float forestNoise = detailsNoiseImage.GetNoise2D(x * 0.035f + 173.2f, y * 0.035f - 541.8f);
        float treeChanceNoise = detailsNoiseImage.GetNoise2D(x * 0.18f + 928.4f, y * 0.18f - 417.2f);
        float treeVariationValue = detailsNoiseImage.GetNoise2D(x * 0.37f + 193.7f, y * 0.37f - 812.5f);

        int treeSpacing = 6;

        int cellX = Mathf.FloorToInt((float)x / treeSpacing);
        int cellY = Mathf.FloorToInt((float)y / treeSpacing);

        int hash = Mathf.Abs(cellX * 928371 + cellY * 523543 + 18231);

        int localTreeX = hash % treeSpacing;
        int localTreeY = (hash / 10) % treeSpacing;

        int expectedX = cellX * treeSpacing + localTreeX;
        int expectedY = cellY * treeSpacing + localTreeY;

        bool canPlaceTree = x == expectedX && y == expectedY;

        switch (_levelBiomeId)
        {
            case 0:
                if (blockId == 100)
                {
                    bool denseForest = forestNoise > 0.28f;
                    bool lightForest = forestNoise > 0.08f && treeChanceNoise > 0.48f;

                    if (canPlaceTree && (denseForest || lightForest))
                        return treeVariationValue > 0f ? 300 : 301;

                    if (value > .35f) return 200;
                    if (value > .15f) return 201;
                }
                return -1;

            case 1:
                if (blockId == 110)
                {
                    bool denseForest = forestNoise > 0.35f;
                    bool lightForest = forestNoise > 0.15f && treeChanceNoise > 0.55f;

                    if (canPlaceTree && (denseForest || lightForest))
                        return treeVariationValue > 0f ? 300 : 301;

                    if (value > .35f) return 210;
                    if (value > .15f) return 211;
                }
                return -1;

            case 2:
                if (blockId == 120)
                {
                    bool denseForest = forestNoise > 0.42f;
                    bool lightForest = forestNoise > 0.22f && treeChanceNoise > 0.62f;

                    if (canPlaceTree && (denseForest || lightForest))
                        return treeVariationValue > 0f ? 300 : 301;

                    if (value > .35f) return 220;
                    if (value > .15f) return 221;
                }
                return -1;

            case 3:
                if (blockId == 130 || blockId == 141)
                {
                    if (value > .35f) return 230;
                    if (value > .15f) return 231;
                }
                return -1;

            case 4:
                if (blockId == 140 || blockId == 141)
                {
                    if (value > .35f) return 240;
                    if (value > .15f) return 241;
                }
                return -1;

            case 5:
                if (blockId == 150 || blockId == 151 || blockId == 152)
                {
                    if (value > .35f) return 250;
                    if (value > .15f) return 251;
                }
                return -1;
        }

        return -1;
    }

    private Vector2I WorldToChunk(Vector2 worldCellPosition)
    {
        return new Vector2I(
            Mathf.FloorToInt(worldCellPosition.X / chunkSize),
            Mathf.FloorToInt(worldCellPosition.Y / chunkSize)
        );
    }

    private HashSet<Vector2I> GetNeededChunks(Vector2 centerWorldPosition)
    {
        HashSet<Vector2I> neededChunks = new();

        Vector2I centerChunk = WorldToChunk(centerWorldPosition);

        int horizontalRadius = 2;
        int verticalRadius = 6;

        for (int y = -verticalRadius; y <= verticalRadius; y++)
        {
            for (int x = -horizontalRadius; x <= horizontalRadius; x++)
            {
                Vector2I chunkCoordinade = new Vector2I(centerChunk.X + x, centerChunk.Y + y);

                if (chunksDictionary.ContainsKey(chunkCoordinade))
                    neededChunks.Add(chunkCoordinade);
            }
        }

        return neededChunks;
    }

    private void LoadChunk(Vector2I chunkCoordinade)
    {
        if (!chunksDictionary.ContainsKey(chunkCoordinade))
            return;
        
        var chunkData = chunksDictionary[chunkCoordinade];

        for (int index = 0; index < chunkData.blocksID.Count; index++)
        {
            int localX = index % chunkSize;
            int localY = index / chunkSize;

            int worldX = chunkCoordinade.X * chunkSize + localX;
            int worldY = chunkCoordinade.Y * chunkSize + localY;

            int blockId = (int)chunkData.blocksID[index];

            if (blockId != -1)
            {
                Vector2I blockAtlas = GetAtlasFromBlockId(blockId);

                if (blockAtlas.X != -1)
                    SetCell(new Vector2I(worldX, worldY), 0, blockAtlas);
            }

            int detailsId = (int)chunkData.detailsID[index];

            if (detailsId != -1)
            {
                Vector2I detailsAtlas = GetAtlasFromDetailsId(detailsId);

                if (detailsAtlas.X != -1)
                    _detailsTileMap.SetCell(new Vector2I(worldX, worldY), 0, detailsAtlas);
            }
        }
    }

    private void UnloadChunk(Vector2I chunkCoordinade)
    {
        for (int localY = 0; localY < chunkSize; localY++)
        {
            for (int localX = 0; localX < chunkSize; localX++)
            {
                int worldX = chunkCoordinade.X * chunkSize + localX;
                int worldY = chunkCoordinade.Y * chunkSize + localY;

                EraseCell(new Vector2I(worldX, worldY));
                _detailsTileMap.EraseCell(new Vector2I(worldX, worldY));
            }
        }
    }

    private void UpdateVisibleChunks(Vector2 centerWorldPosition)
    {
        var neededChunks = GetNeededChunks(centerWorldPosition);

        foreach (var chunkCoordinade in neededChunks)
        {
            if (!_loadedChunks.Contains(chunkCoordinade))
            {
                LoadChunk(chunkCoordinade);
                _loadedChunks.Add(chunkCoordinade);
            }
        }

        foreach (var chunkCoordinade in _loadedChunks.ToList())
        {
            if (!neededChunks.Contains(chunkCoordinade))
            {
                UnloadChunk(chunkCoordinade);
                _loadedChunks.Remove(chunkCoordinade);
            }
        }
    }

    public void CreateChunksGrid()
    {
        for (var y = -chunksY / 2; y < chunksY / 2; y++)
        {
            for (var x = -chunksX / 2; x < chunksX / 2; x++)
            {
                var chunkGridCoordinade = new Vector2I(x, y);
                chunksDictionary[chunkGridCoordinade] = new ChunkData();
            }
        }
    }

    private Vector2I GetAtlasFromBlockId(int blockId)
    {
        return blockId switch
        {
            100 => new Vector2I(1, 0),
            101 => new Vector2I(0, 1),
            110 => new Vector2I(1, 4),
            120 => new Vector2I(1, 3),
            130 => new Vector2I(1, 2),
            140 => new Vector2I(2, 1),
            141 => new Vector2I(1, 1),
            150 => new Vector2I(3, 2),
            151 => new Vector2I(3, 1),
            152 => new Vector2I(3, 0),
            160 => new Vector2I(0, 19),
            161 => new Vector2I(0, 19),
            _ => new Vector2I(-1, -1)
        };
    }

    private Vector2I GetAtlasFromDetailsId(int id)
    {
        return id switch
        {
            200 => new Vector2I(17, 0),
            201 => new Vector2I(17, 1),
            210 => new Vector2I(17, 2),
            211 => new Vector2I(17, 3),
            220 => new Vector2I(17, 4),
            221 => new Vector2I(17, 4),
            230 => new Vector2I(17, 5),
            231 => new Vector2I(17, 6),
            240 => new Vector2I(17, 7),
            241 => new Vector2I(17, 7),
            250 => new Vector2I(17, 7),
            251 => new Vector2I(17, 7),

            300 => new Vector2I(14, 13),
            301 => new Vector2I(17, 13),

            _ => new Vector2I(-1, -1)
        };
    }

    public void GenerateChunksData()
    {
        foreach (var pair in chunksDictionary.ToList())
        {
            var chunkCoordinade = pair.Key;
            var chunkData = pair.Value;

            chunkData.blocksID.Clear();
            chunkData.detailsID.Clear();

            for (int localY = 0; localY < chunkSize; localY++)
            {
                for (int localX = 0; localX < chunkSize; localX++)
                {
                    int worldX = chunkCoordinade.X * chunkSize + localX;
                    int worldY = chunkCoordinade.Y * chunkSize + localY;

                    int blockID = GenerateBlockId(worldX, worldY);
                    chunkData.blocksID.Add(blockID);

                    int detailID = GenerateDetailId(worldX, worldY, blockID);
                    chunkData.detailsID.Add(detailID);
                }
            }

            chunkData.SpawnEnemys = IsSpawnableChunk(chunkData.blocksID);
        }
    }

    private bool IsSpawnableChunk(Array blocksID)
    {
        var IsSpawnable = true;

        foreach (int tile in blocksID)
        {
            if (tile == 101 || tile == 160)
                IsSpawnable = false;
        }

        return IsSpawnable;
    }

    private int GenerateBlockId(int x, int y)
    {
        var levelRadius = Mathf.Min(chunksX * chunkSize, chunksY * chunkSize) * 0.5f;

        Vector2 p = new(x, y);

        var dist = p.Length();
        var angle = Mathf.Atan2(p.Y, p.X);
        
        var ax = Mathf.Cos(angle) * _coastDetails;
        var ay = Mathf.Sin(angle) * _coastDetails;

        var coastN = _blocksNoiseImage.GetNoise2D(ax + 123.4f, ay - 567.8f);
        var coast01 = (coastN + 1f) * .5f;
        var radius = levelRadius * (1f + (coast01 - .5f) * 2f * _coastStrength);

        var mask = 1f - (dist / radius);
        mask = Mathf.Clamp(mask, 0f, 1f);
        mask *= mask;

        var n = _blocksNoiseImage.GetNoise2D(x * _insideDetails, y * _insideDetails);
        var inside01 = (n + 1f) * .5f;

        var value = mask * (.65f + inside01 * .35f);

        switch (_levelBiomeId)
        {
            case 0:
                float lakeNoise = _secondaryBlocksNoise.GetNoise2D(x * 0.35f, y * 0.35f);

                if (value > .40f)
                {
                    if (lakeNoise > .55f) return 161;
                    if (lakeNoise > .35f) return 160;

                    if (value > .45f) return 100;
                    return 101;
                }

                return -1;
            case 1:
                if (value > .45f) return 110;
                if (value > .40f) return 101;
                return -1;
            case 2:
                if (value > .45f) return 120;
                if (value > .40f) return 121;
                return -1;
            case 3:
                if (value > .40f && _secondaryBlocksNoise.GetNoise2D(x, y) > .15f && _secondaryBlocksNoise.GetNoise2D(x, y) < .65f) return 130;
                if (value > .40f) return 130;
                if (value > .35f) return 131;
                return -1;
            case 4:
                if (value > .40f && _secondaryBlocksNoise.GetNoise2D(x, y) > .35f) return 140;
                if (value > .40f && _secondaryBlocksNoise.GetNoise2D(x, y) > .25f && _secondaryBlocksNoise.GetNoise2D(x, y) < .35f) return 141;
                if (value > .40f) return 140;
                if (value > .35f) return 141;
                return -1;
            case 5:
                if (value > .40f && _secondaryBlocksNoise.GetNoise2D(x, y) > .35f) return 150;
                if (value > .40f && _secondaryBlocksNoise.GetNoise2D(x, y) > .27f && _secondaryBlocksNoise.GetNoise2D(x, y) < .35f) return 151;
                if (value > .40f) return 150;
                if (value > .35f) return 151;
                return -1;
        }

        return -1;
    }

    private int GenerateDetailId(int x, int y, int blockId)
    {
        if (blockId == -1)
            return -1;

        float value = _detailsNoiseImage.GetNoise2D(x, y);

        float forestNoise = _detailsNoiseImage.GetNoise2D(x * 0.035f + 173.2f, y * 0.035f - 541.8f);
        float treeChanceNoise = _detailsNoiseImage.GetNoise2D(x * 0.18f + 928.4f, y * 0.18f - 417.2f);
        float treeVariationValue = _detailsNoiseImage.GetNoise2D(x * 0.37f + 193.7f, y * 0.37f - 812.5f);

        int treeSpacing = 6;

        int cellX = Mathf.FloorToInt((float)x / treeSpacing);
        int cellY = Mathf.FloorToInt((float)y / treeSpacing);

        int hash = Mathf.Abs(cellX * 928371 + cellY * 523543 + 18231);

        int localTreeX = hash % treeSpacing;
        int localTreeY = (hash / 10) % treeSpacing;

        int expectedX = cellX * treeSpacing + localTreeX;
        int expectedY = cellY * treeSpacing + localTreeY;

        bool canPlaceTree = x == expectedX && y == expectedY;

        switch (_levelBiomeId)
        {
            case 0:
                if (blockId == 100)
                {
                    bool denseForest = forestNoise > 0.28f;
                    bool lightForest = forestNoise > 0.08f && treeChanceNoise > 0.48f;

                    if (canPlaceTree && (denseForest || lightForest))
                        return treeVariationValue > 0f ? 300 : 301;

                    if (value > .35f) return 200;
                    if (value > .15f) return 201;
                }
                return -1;

            case 1:
                if (blockId == 110)
                {
                    bool denseForest = forestNoise > 0.35f;
                    bool lightForest = forestNoise > 0.15f && treeChanceNoise > 0.55f;

                    if (canPlaceTree && (denseForest || lightForest))
                        return treeVariationValue > 0f ? 300 : 301;

                    if (value > .35f) return 210;
                    if (value > .15f) return 211;
                }
                return -1;

            case 2:
                if (blockId == 120)
                {
                    bool denseForest = forestNoise > 0.42f;
                    bool lightForest = forestNoise > 0.22f && treeChanceNoise > 0.62f;

                    if (canPlaceTree && (denseForest || lightForest))
                        return treeVariationValue > 0f ? 300 : 301;

                    if (value > .35f) return 220;
                    if (value > .15f) return 221;
                }
                return -1;

            case 3:
                if (blockId == 130 || blockId == 141)
                {
                    if (value > .35f) return 230;
                    if (value > .15f) return 231;
                }
                return -1;

            case 4:
                if (blockId == 140 || blockId == 141)
                {
                    if (value > .35f) return 240;
                    if (value > .15f) return 241;
                }
                return -1;

            case 5:
                if (blockId == 150 || blockId == 151 || blockId == 152)
                {
                    if (value > .35f) return 250;
                    if (value > .15f) return 251;
                }
                return -1;
        }

        return -1;
    }

    private bool IsPortalValidBlock(int blockId)
    {
        return LevelBiomeId switch
        {
            0 => blockId == 100,
            1 => blockId == 110,
            2 => blockId == 120,
            3 => blockId == 130 || blockId == 110,
            4 => blockId == 140 || blockId == 141 || blockId == 130,
            5 => blockId == 150 || blockId == 151 || blockId == 152,
            _ => false
        };
    }

    private int GetBlockIdAtCell(Vector2I worldCell)
    {
        Vector2I chunkCoord = new Vector2I(
            Mathf.FloorToInt((float)worldCell.X / chunkSize),
            Mathf.FloorToInt((float)worldCell.Y / chunkSize)
        );

        if (!chunksDictionary.ContainsKey(chunkCoord))
            return -1;

        var chunkData = chunksDictionary[chunkCoord];

        int localX = worldCell.X - chunkCoord.X * chunkSize;
        int localY = worldCell.Y - chunkCoord.Y * chunkSize;

        if (localX < 0 || localX >= chunkSize || localY < 0 || localY >= chunkSize)
            return -1;

        int index = localY * chunkSize + localX;

        if (index < 0 || index >= chunkData.blocksID.Count)
            return -1;

        return (int)chunkData.blocksID[index];
    }

    private bool HasEnoughSpaceForPortal(Vector2I centerCell, int radius)
    {
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                Vector2I cell = centerCell + new Vector2I(x, y);
                int blockId = GetBlockIdAtCell(cell);

                if (!IsPortalValidBlock(blockId))
                    return false;
            }
        }

        return true;
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

    private List<Vector2I> GetValidPortalCellsInChunk(Vector2I chunkCoord, int sampleStep = 2, int portalRadius = 2)
    {
        var validCells = new List<Vector2I>();

        int startX = chunkCoord.X * chunkSize;
        int startY = chunkCoord.Y * chunkSize;

        for (int localY = portalRadius; localY < chunkSize - portalRadius; localY += sampleStep)
        {
            for (int localX = portalRadius; localX < chunkSize - portalRadius; localX += sampleStep)
            {
                Vector2I worldCell = new Vector2I(startX + localX, startY + localY);
                int blockId = GetBlockIdAtCell(worldCell);

                if (!IsPortalValidBlock(blockId))
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
        var rng = new RandomNumberGenerator();

        var candidateChunks = GetCentralChunkCandidates(12);
        var candidateCells = new List<Vector2I>();

        foreach (var chunkCoord in candidateChunks)
            candidateCells.AddRange(GetValidPortalCellsInChunk(chunkCoord, 2, 3));

        if (candidateCells.Count == 0)
        {
            GD.Print("ERROR: no valid initial portal position found");
            return;
        }

        candidateCells.Sort((a, b) => a.DistanceSquaredTo(Vector2I.Zero).CompareTo(b.DistanceSquaredTo(Vector2I.Zero)));

        int topCount = Mathf.Min(12, candidateCells.Count);
        Vector2I chosen = candidateCells[rng.RandiRange(0, topCount - 1)];

        _initialPortalCell = chosen;
        SpawnInitialPortalSceneAt(chosen);

        GD.Print($"Initial portal generated in: {chosen}");
    }

    private List<Vector2I> GetOuterChunkCandidates(int maxChunks = 20)
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

    private void SpawnExitPortal()
    {
        var rng = new RandomNumberGenerator();

        var candidates = CollectExitCandidates(true, 25);
        GD.Print($"exit candidates near void: {candidates.Count}");

        if (candidates.Count == 0)
        {
            candidates = CollectExitCandidates(false, 20);
            GD.Print($"exit candidates fallback outer: {candidates.Count}");
        }

        if (candidates.Count == 0)
        {
            var fallback = GetFarthestValidCellFromInitial2();

            if (fallback == null)
            {
                GD.Print("ERROR: no valid exit portal position found");
                return;
            }

            SpawnExitPortalSceneAt(fallback.Value);
            GD.Print($"Exit portal generated in fallback: {fallback.Value}");
            return;
        }

        var chosen = candidates[rng.RandiRange(0, candidates.Count - 1)];
        SpawnExitPortalSceneAt(chosen);
        GD.Print($"Exit portal generated in: {chosen}");
    }

    private List<Vector2I> GetExitPortalCandidatesInChunk(
        Vector2I chunkCoord,
        int sampleStep,
        int portalRadius,
        int minInvalidNearby,
        int maxInvalidNearby,
        int minDistanceFromInitial)
    {
        var result = new List<Vector2I>();

        int startX = chunkCoord.X * chunkSize;
        int startY = chunkCoord.Y * chunkSize;

        for (int localY = portalRadius; localY < chunkSize - portalRadius; localY += sampleStep)
        {
            for (int localX = portalRadius; localX < chunkSize - portalRadius; localX += sampleStep)
            {
                Vector2I worldCell = new Vector2I(startX + localX, startY + localY);

                int blockId = GetBlockIdAtCell(worldCell);
                if (!IsPortalValidBlock(blockId))
                    continue;

                if (worldCell.DistanceSquaredTo(_initialPortalCell) < minDistanceFromInitial * minDistanceFromInitial)
                    continue;

                if (!HasEnoughSpaceForPortal(worldCell, portalRadius))
                    continue;

                int invalidNearby = CountInvalidNearbyCellsForExit(worldCell, 3);

                if (invalidNearby < minInvalidNearby || invalidNearby > maxInvalidNearby)
                    continue;

                result.Add(worldCell);
            }
        }

        return result;
    }

    private Vector2I? GetFarthestValidCellFromInitial()
    {
        Vector2I? bestCell = null;
        int bestDistance = -1;

        foreach (var chunkCoord in chunksDictionary.Keys)
        {
            int startX = chunkCoord.X * chunkSize;
            int startY = chunkCoord.Y * chunkSize;

            for (int localY = 1; localY < chunkSize - 1; localY += 2)
            {
                for (int localX = 1; localX < chunkSize - 1; localX += 2)
                {
                    Vector2I worldCell = new Vector2I(startX + localX, startY + localY);

                    int blockId = GetBlockIdAtCell(worldCell);
                    if (!IsPortalValidBlock(blockId))
                        continue;

                    if (!HasEnoughSpaceForPortal(worldCell, 1))
                        continue;

                    int dist = worldCell.DistanceSquaredTo(_initialPortalCell);

                    if (dist > bestDistance)
                    {
                        bestDistance = dist;
                        bestCell = worldCell;
                    }
                }
            }
        }

        return bestCell;
    }

    private List<Vector2I> GetCornerChunkCandidates()
    {
        var result = new List<Vector2I>();

        int minCornerDistance = Mathf.Min(chunksX, chunksY) / 3;

        foreach (var chunk in chunksDictionary.Keys)
        {
            if (Mathf.Abs(chunk.X) >= minCornerDistance && Mathf.Abs(chunk.Y) >= minCornerDistance)
                result.Add(chunk);
        }

        result.Sort((a, b) => b.DistanceSquaredTo(Vector2I.Zero).CompareTo(a.DistanceSquaredTo(Vector2I.Zero)));

        return result;
    }

    private void SpawnExitPortalSceneAt(Vector2I mapCell)
    {
        if (_exitPortalScene == null)
        {
            GD.Print("ERROR: exit portal scene null");
            return;
        }

        var portal = _exitPortalScene.Instantiate<Node2D>();
        _exitPortalReference = portal;

        Vector2 pos = MapToLocal(mapCell);
        portal.Position = pos;

        AddChild(portal);
    }

    private void SpawnInitialPortalSceneAt(Vector2I mapCell)
    {
        if (_initialPortalScene == null)
        {
            GD.Print("ERROR: initial portal scene null");
            return;
        }

        var portal = _initialPortalScene.Instantiate<Node2D>();
        _initialPortalReference = portal;

        Vector2 pos = MapToLocal(mapCell);
        portal.Position = pos;

        AddChild(portal);
    }

    private int CountInvalidNearbyCells(Vector2I cell, int radius)
    {
        int invalidCount = 0;

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                Vector2I check = cell + new Vector2I(x, y);

                if (!IsPortalValidBlock(GetBlockIdAtCell(check)))
                    invalidCount++;
            }
        }

        return invalidCount;
    }

    private int CountInvalidNearbyCellsForExit(Vector2I cell, int radius)
    {
        int invalidCount = 0;

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                Vector2I check = cell + new Vector2I(x, y);

                if (!IsPortalValidBlock(GetBlockIdAtCell(check)))
                    invalidCount++;
            }
        }

        return invalidCount;
    }

    private bool IsGoodExitPortalCell(Vector2I cell)
    {
        int invalidNearby = 0;
        int validNearby = 0;

        for (int y = -3; y <= 3; y++)
        {
            for (int x = -3; x <= 3; x++)
            {
                Vector2I check = cell + new Vector2I(x, y);

                if (IsPortalValidBlock(GetBlockIdAtCell(check)))
                    validNearby++;
                else
                    invalidNearby++;
            }
        }

        return invalidNearby >= 10 && invalidNearby <= 20 && validNearby >= 24;
    }

    private bool IsNearVoidButStillSafe(Vector2I cell)
    {
        bool foundVoid = false;

        for (int y = -6; y <= 6; y++)
        {
            for (int x = -6; x <= 6; x++)
            {
                Vector2I check = cell + new Vector2I(x, y);

                if (!IsPortalValidBlock(GetBlockIdAtCell(check)))
                {
                    foundVoid = true;
                    break;
                }
            }

            if (foundVoid)
                break;
        }

        return foundVoid;
    }

    private bool IsNearVoid(Vector2I cell, int radius = 6)
    {
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                Vector2I check = cell + new Vector2I(x, y);

                if (!IsPortalValidBlock(GetBlockIdAtCell(check)))
                    return true;
            }
        }

        return false;
    }

    private bool HasEnoughFreeSpaceForExitPortal(Vector2I centerCell, int radius = 2)
    {
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                Vector2I cell = centerCell + new Vector2I(x, y);

                if (!IsPortalValidBlock(GetBlockIdAtCell(cell)))
                    return false;
            }
        }

        return true;
    }

    private List<Vector2I> CollectExitCandidates(bool requireNearVoid, int minDistanceFromInitial)
    {
        var result = new List<Vector2I>();

        foreach (var chunkCoord in GetOuterChunkCandidates(30))
        {
            int startX = chunkCoord.X * chunkSize;
            int startY = chunkCoord.Y * chunkSize;

            for (int localY = 2; localY < chunkSize - 2; localY += 2)
            {
                for (int localX = 2; localX < chunkSize - 2; localX += 2)
                {
                    Vector2I worldCell = new Vector2I(startX + localX, startY + localY);

                    int blockId = GetBlockIdAtCell(worldCell);
                    if (!IsPortalValidBlock(blockId))
                        continue;

                    if (worldCell.DistanceSquaredTo(_initialPortalCell) < minDistanceFromInitial * minDistanceFromInitial)
                        continue;

                    if (!HasEnoughFreeSpaceForExitPortal(worldCell, 2))
                        continue;

                    if (requireNearVoid && !IsNearVoid(worldCell, 6))
                        continue;

                    result.Add(worldCell);
                }
            }
        }

        return result;
    }

    private Vector2I? GetFarthestValidCellFromInitial2()
    {
        Vector2I? bestCell = null;
        int bestDistance = -1;

        foreach (var chunkCoord in chunksDictionary.Keys)
        {
            int startX = chunkCoord.X * chunkSize;
            int startY = chunkCoord.Y * chunkSize;

            for (int localY = 2; localY < chunkSize - 2; localY += 2)
            {
                for (int localX = 2; localX < chunkSize - 2; localX += 2)
                {
                    Vector2I worldCell = new Vector2I(startX + localX, startY + localY);

                    int blockId = GetBlockIdAtCell(worldCell);
                    if (!IsPortalValidBlock(blockId))
                        continue;

                    if (!HasEnoughFreeSpaceForExitPortal(worldCell, 2))
                        continue;

                    int dist = worldCell.DistanceSquaredTo(_initialPortalCell);

                    if (dist > bestDistance)
                    {
                        bestDistance = dist;
                        bestCell = worldCell;
                    }
                }
            }
        }

        return bestCell;
    }
}

//Have many bugs here, i tried, i really tried, but the FiberGen mode is so hard to fix, idk
//The loading screen in partial-fake? yes