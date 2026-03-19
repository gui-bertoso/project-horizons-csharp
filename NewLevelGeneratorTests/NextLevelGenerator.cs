using Godot;
using Godot.Collections;
using projecthorizonscs.Autoload;
using System;
using System.Collections.Generic;
using System.Linq;
using Array = Godot.Collections.Array;

namespace projecthorizonscs;

public partial class ChunkData: Node
{
    public Array blocksID = new();
    public Array detailsID = new();
    public bool SpawnEnemys;
}

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
    public int chunksX = 125;
    public int chunksY = 250;

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

    /*
    Blocks translation - I need to create this, I don't want to create this, I stay confuse

    100 grass (1,0)
    101 coast (0,1)
    110 dark grass (1,4)
    120 dry grass (1,3)
    130 snow (1,2)
    140 ice1 (2,1)
    141 ice2 (1,1)

    */
    
    public Godot.Collections.Dictionary<Vector2I, ChunkData> chunksDictionary = new();

    public override void _Ready()
    {
        GD.Print("Loading packed scenes and references");
		_initialPortalScene = (PackedScene)ResourceLoader.Load("res://Portal/InitialPortal.tscn");
		_exitPortalScene = (PackedScene)ResourceLoader.Load("res://Portal/ExitPortal.tscn");

        _detailsTileMap = GetNode<TileMapLayer>("Details");

        GD.Print("Setting Biome");
        SetBiome();

        GD.Print("Setting Seeds");
        SetSeeds();

        GD.Print("Setting noises");
        SetNoises();

        GD.Print("Generating Chunk grid");
        CreateChunksGrid();

        GD.Print("Generating Chunk data");
        GenerateChunksData();
        GD.Print("Chunk data generated");

        GD.Print("Creating portals");
        SpawnInitialPortal();
        SpawnExitPortal();

        UpdateVisibleChunks(Vector2.Zero);
        GD.Print("Visible chunks loaded");
    }

    public override void _Process(double delta)
    {
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
        _levelBiomeId = new RandomNumberGenerator().RandiRange(0, 5);
        //_levelBiomeId = 0;
    }

    public void SetNoises()
    {
		_blocksNoiseImage = new ();
		_blocksNoiseImage.NoiseType = FastNoiseLite.NoiseTypeEnum.ValueCubic;
		_blocksNoiseImage.Seed = _levelSeed;
		_blocksNoiseImage.FractalOctaves = _blocksFractalOctaves;
		_blocksNoiseImage.Frequency = .01f;

		_detailsNoiseImage = new ();
		_detailsNoiseImage.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
		_detailsNoiseImage.Seed = _levelSeed + 1;
		_detailsNoiseImage.Frequency = 0.2f; // more frequency = more details

        _secondaryBlocksNoise = new ();
        _secondaryBlocksNoise.Seed = _levelSeed + 2;
        _secondaryBlocksNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
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
        Vector2 viewportSize = GetViewportRect().Size;

        int visibleCellsX = Mathf.CeilToInt(viewportSize.X / tileSize);
        int visibleCellsY = Mathf.CeilToInt(viewportSize.Y / tileSize);

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
        for (var y = -chunksY/2; y < chunksY/2; y++)
        {
            for (var x = -chunksX/2; x < chunksX/2; x++)
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
            100 => new Vector2I(1, 0), // grass
            101 => new Vector2I(0, 1), // half void
            110 => new Vector2I(1, 4), // dark grass
            120 => new Vector2I(1, 3),// dry grass
            130 => new Vector2I(1, 2), // snow grass
            140 => new Vector2I(2, 1), // ice
            141 => new Vector2I(1, 1), // ice and snow
            150 => new Vector2I(3, 2), // half2 sand
            151 => new Vector2I(3, 1), // half sand
            152 => new Vector2I(3, 0), // sand
            160 => new Vector2I(0, 19), // water
            161 => new Vector2I(0, 19), // deep water
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
            230 => new Vector2I(17, 5),
            231 => new Vector2I(17, 6),
            240 => new Vector2I(17, 7),
            //241 => new Vector2I(17, 8),
            //250 => new Vector2I(17, 9), desert doens't have grass 
            //251 => new Vector2I(17, 10),
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
        }
    }

    private int GenerateBlockId(int x, int y)
    {
        var levelRadius = Mathf.Min(chunksX * chunkSize, chunksY * chunkSize) * 0.5f;;

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
                    if (lakeNoise > .55f) return 161; // deep water
                    if (lakeNoise > .35f) return 160; // water

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

        switch (_levelBiomeId)
        {
            case 0:
                if (blockId == 100)
                {
                    if (value > .35f) return 200;
                    if (value > .15f) return 201;
                }
                return -1;
            case 1:
                if (blockId == 110)
                {
                    if (value > .35f) return 210;
                    if (value > .15f) return 211;
                }
                return -1;
            case 2:
                if (blockId == 120)
                {
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
        {
            candidateCells.AddRange(GetValidPortalCellsInChunk(chunkCoord, 2, 3));
        }

        if (candidateCells.Count == 0)
        {
            GD.Print("ERROR: no valid initial portal position found");
            return;
        }

        candidateCells.Sort((a, b) =>
            a.DistanceSquaredTo(Vector2I.Zero).CompareTo(b.DistanceSquaredTo(Vector2I.Zero)));

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

        // fase 1: perto do void
        var candidates = CollectExitCandidates(true, 25);
        GD.Print($"exit candidates near void: {candidates.Count}");

        // fase 2: só longe do initial
        if (candidates.Count == 0)
        {
            candidates = CollectExitCandidates(false, 20);
            GD.Print($"exit candidates fallback outer: {candidates.Count}");
        }

        // fase 3: mais longe possível
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

        result.Sort((a, b) =>
            b.DistanceSquaredTo(Vector2I.Zero).CompareTo(a.DistanceSquaredTo(Vector2I.Zero)));

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

        // mais perto do void, mas ainda seguro
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