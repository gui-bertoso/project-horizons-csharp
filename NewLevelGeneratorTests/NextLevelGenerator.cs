using Godot;
using Godot.Collections;
using System;
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
	private FastNoiseLite _blocksNoiseImage;
	private FastNoiseLite _detailsNoiseImage;

    public int chunkSize = 112;
    public int chunksX = 12;
    public int chunksY = 16;

	private float _insideDetails = 40f;
	private float _coastDetails = 90.0f;
	private float _coastStrength = .55f;

    private int _blocksSeed = 0;
	private float _blocksFrequency = .01f;
	private int _blocksFractalOctaves = 8;

    private int _levelBiomeId = 0;
    private FastNoiseLite _secondaryBlocksNoise;
    

	public int LevelBiomeId;

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
    
    public Dictionary<Vector2I, ChunkData> chunksDictionary = new();

    public override void _Ready()
    {
        GD.Print("Setting Seeds");
        SetSeeds();
        GD.Print("Setting noises");
        SetNoises();

        GD.Print("Generating Chunk grid");
        CreateChunksGrid();
        GD.Print("Generating Chunk data");
        GenerateChunksData();
        GD.Print("Chunk data generated");
    }

    public void SetSeeds()
    {
		var rng = new RandomNumberGenerator();
		_blocksSeed = rng.RandiRange(0, 99999999);
    }

    public void SetNoises()
    {
		_blocksNoiseImage = new FastNoiseLite();
		_blocksNoiseImage.NoiseType = FastNoiseLite.NoiseTypeEnum.ValueCubic;

		_blocksNoiseImage.Seed = _blocksSeed;
		_blocksNoiseImage.FractalOctaves = _blocksFractalOctaves;
		_blocksNoiseImage.Frequency = _blocksFrequency;

        _secondaryBlocksNoise = new FastNoiseLite();
        _secondaryBlocksNoise.Seed = _blocksSeed + 1;
        _secondaryBlocksNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
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
                if (value > .45f) return 100;
                if (value > .40f) return 101;
                return -1;
            case 1:
                if (value > .45f) return 110;
                if (value > .40f) return 101;
                return -1;
            case 2:
                if (value > .45f) return 120;
                if (value > .40f) return 101;
                return -1;
            case 3:
                var secondaryNoise = new FastNoiseLite();
                secondaryNoise.Seed = _blocksSeed;

                if (value > .40f && secondaryNoise.GetNoise2D(x, y) > .15f && secondaryNoise.GetNoise2D(x, y) < .65f) return 130;
                if (value > .40f) return 110;
                if (value > .35f) return 101;
                return -1;
            case 4:
                var secondaryNoise2 = new FastNoiseLite();
                secondaryNoise2.Seed = _blocksSeed;

                if (value > .40f && secondaryNoise2.GetNoise2D(x, y) > .35f) return 140;
                if (value > .40f && secondaryNoise2.GetNoise2D(x, y) > .25f && secondaryNoise2.GetNoise2D(x, y) < .35f) return 141;
                if (value > .40f) return 130;
                if (value > .35f) return 101;
                return -1;
            case 5:
                var secondaryNoise3 = new FastNoiseLite();
                secondaryNoise3.Seed = _blocksSeed;

                if (value > .40f && secondaryNoise3.GetNoise2D(x, y) > .35f) return 150;
                if (value > .40f && secondaryNoise3.GetNoise2D(x, y) > .27f && secondaryNoise3.GetNoise2D(x, y) < .35f) return 151;
                if (value > .40f) return 152;
                if (value > .35f) return 101;
                return -1;
        }
        return -1;
    }
}