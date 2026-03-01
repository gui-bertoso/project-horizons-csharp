using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public sealed class ChunkData
{
	public List<int> Layer0;
	public List<int> Layer1;

	public ChunkData(List<int> layer_0, List<int> layer_1)
	{
		Layer0 = layer_0;
		Layer1 = layer_1;
	}
}

public partial class NewLevelGenerator : TileMapLayer
{

	private FastNoiseLite Layer0_NoiseImage;
	private FastNoiseLite.NoiseTypeEnum Layer0_NoiseType = FastNoiseLite.NoiseTypeEnum.ValueCubic;
	private int Layer0_Seed;
	private float Layer0_Frequency = .01f;
	private int Layer0_FractalOctaves = 8;

	private float insideDetails = 40f;
	private float coastDetails = 90.0f;
	private float coastStrength = .55f;
	private float threshould = .45f;

	private double Delta;

	private int chunk_size_x = 100;
	private int chunk_size_y = 200;

	private int level_size_x = 400;
	private int level_size_y = 600;

	private Dictionary<Vector2I, ChunkData> chunksDataDictionary = new ();

	private Dictionary<int, Vector2I> blockIDs = new()
	{
		{0, new Vector2I(0, 0)}, //void
		{1, new Vector2I(1, 0)}, //half-void
		{2, new Vector2I(0, 1)}, //grass
		{3, new Vector2I(0, 2)}, //dirt
	};

    public override void _Process(double delta)
    {
        Delta = delta;
    }

	public void GenerateLevel()
	{
		
	}

	public void GenerateChunkData(Vector2I grid_coordinades)
	{

		GD.PrintT("CHUNK: creating new");

		List<int> new_chunk_layer0_data = [];
		List<int> new_chunk_layer1_data = [];
		
		Vector2 center = new(level_size_x/2f, level_size_y/2f);
		float levelRadius = Mathf.Min(level_size_x, level_size_y) * .42f;

		int halfX = chunk_size_x / 2;
		int halfY = chunk_size_y / 2;

		for (int y = -halfY; y < halfY; y++)
		{
			for (int x = -halfX; x < halfX; x++)
			{
				Vector2 p = new(x, y);
				Vector2 v = p;

				float dist = v.Length();
				float angle = Mathf.Atan2(v.Y, v.X);
				float ax = Mathf.Cos(angle) * coastDetails;
				float ay = Mathf.Sin(angle) * coastDetails;

				float coastN = Layer0_NoiseImage.GetNoise2D(ax + 123.4f, ay - 567.8f);
				float coast01 = (coastN + 1f) * .5f;
				float radius = levelRadius * (1f + (coast01 - .5f) * 2f * coastStrength);

				float mask = 1f - (dist/radius);
				mask = Mathf.Clamp(mask, 0f, 1f);
				mask = mask * mask;

				float n = Layer0_NoiseImage.GetNoise2D(x * insideDetails, y * insideDetails);
				float inside01 = (n + 1f);

				float value = mask * (.65f + inside01 * .35f);

				if (value > threshould)
				{
					new_chunk_layer0_data.Add(1);
				} else if (value > threshould - .05f && value < threshould)
				{
					new_chunk_layer0_data.Add(2);
				}
				else
				{
					new_chunk_layer0_data.Add(-1);
				}
			}
		}
		GD.PrintT($"CHUNK: created in {Delta}");

		ChunkData new_chunk_data = new(new_chunk_layer0_data, []);

		chunksDataDictionary[grid_coordinades] = new_chunk_data;
		GD.PrintT($"CHUNK: new chunk data: {new_chunk_data}");
	}


	public void SetNoises()
	{
		Layer0_NoiseImage = new FastNoiseLite();
		Layer0_NoiseImage.NoiseType = Layer0_NoiseType;

		var rng = new Godot.RandomNumberGenerator();
		Layer0_Seed = rng.RandiRange(0, 99999999);
		Layer0_NoiseImage.Seed = Layer0_Seed;
		Layer0_NoiseImage.FractalOctaves = Layer0_FractalOctaves;
		Layer0_NoiseImage.Frequency = Layer0_Frequency;
	}
}

// To use later
/*
		for (int y = -halfY; y < halfY; y++)
		{
			for (int x = -halfX; x < halfX; x++)
			{
				Vector2 p = new(x, y);
				Vector2 v = p;

				float dist = v.Length();
				float angle = Mathf.Atan2(v.Y, v.X);
				float ax = Mathf.Cos(angle) * coastDetails;
				float ay = Mathf.Sin(angle) * coastDetails;

				float coastN = Layer0_NoiseImage.GetNoise2D(ax + 123.4f, ay - 567.8f);
				float coast01 = (coastN + 1f) * .5f;
				float radius = levelRadius * (1f + (coast01 - .5f) * 2f * coastStrength);

				float mask = 1f - (dist/radius);
				mask = Mathf.Clamp(mask, 0f, 1f);
				mask = mask * mask;

				float n = Layer0_NoiseImage.GetNoise2D(x * insideDetails, y * insideDetails);
				float inside01 = (n + 1f);

				float value = mask * (.65f + inside01 * .35f);

				if (value > threshould)
				{
					SetCell(new Vector2I(x, y), 0, new Vector2I(1, 0));
				} else if (value > threshould - .05f && value < threshould)
				{
					SetCell(new Vector2I(x, y), 0, new Vector2I(0, 1));
				}
				else
				{
					SetCell(new Vector2I(x, y), -1);
				}
			}
		}
*/