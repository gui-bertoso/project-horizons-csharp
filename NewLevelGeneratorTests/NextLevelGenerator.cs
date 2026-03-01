using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Numerics;
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

public partial class NextLevelGenerator : TileMapLayer
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

	private int chunkSizeX = 25;
	private int chunkSizeY = 75;

	private int levelSizeX = 2000;
	private int levelSizeY = 2800;

	private Vector2I playerPosition;
	private Vector2I currentPlayerChunk = new (0, 0);

	const int SourceID = 0;

	Vector2I lastRenderedChunk;

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

		Vector2I playerPosition = LocalToMap(ToLocal(Globals.I.LocalPlayer.Position));
		Vector2I currentPlayerChunk = new (Mathf.FloorToInt(playerPosition.X / chunkSizeX), Mathf.FloorToInt(playerPosition.Y / chunkSizeY));
		Globals.I.CurrentPlayerChunk = currentPlayerChunk;

		RenderChunks();
		
    }

	public void RenderChunks()
	{
		int renderDistance = 1;
		for (int y = -renderDistance; y <= renderDistance; y++)
		for(int x = -renderDistance; x <= renderDistance; x++)
			RenderChunk(currentPlayerChunk + new Vector2I(x, y));
	}

	public void RenderChunk(Vector2I ChunkPosition)
	{
		int halfX = chunkSizeX / 2;
		int halfY = chunkSizeY / 2;

		int originX = ChunkPosition.X * chunkSizeX;
		int originY = ChunkPosition.Y * chunkSizeY;

		int i = 0;
		for (int y = -halfY; y < halfY; y++)
		{
			for (int x = -halfX; x < halfX; x++)
			{
				ChunkData current_chunk = chunksDataDictionary[ChunkPosition];
				int id = current_chunk.Layer0[i++];
				
				Vector2I cell = new (originX + x, originY + y);

				if (id == -1)
				{
					SetCell(cell, -1);
					continue;
				}

				SetCell(cell, SourceID, blockIDs[id]);

			}
		}
	}

    public override void _Ready()
    {
        SetNoises();
		GenerateLevel();
    }

	public void GenerateLevel()
	{
		int chunksX = levelSizeX / chunkSizeX;
		int chunksY = levelSizeY / chunkSizeY;
		
		for (int y = -chunksY; y < chunksY; y++)
		{
			for (int x = -chunksX; x < chunksX; x++)
			{
				Vector2I grid_coordinade = new (x, y);
				GenerateChunkData(grid_coordinade);
			}
		}
	}

	public void GenerateChunkData(Vector2I gridCoordinades)
	{

		GD.Print($"CHUNK SYSTEM: creating new in {gridCoordinades}");

		List<int> newChunkLayer0Data = [];
		List<int> newChunkLayer1Dta = [];
		
		Vector2I center = new ((gridCoordinades.X * chunkSizeX)/2, (gridCoordinades.Y * chunkSizeY)/2);
		float levelRadius = Mathf.Min(levelSizeX, levelSizeY) * .42f;

		int halfX = chunkSizeX / 2;
		int halfY = chunkSizeY / 2;

		for (int y = -halfY; y < halfY; y++)
		{
			for (int x = -halfX; x < halfX; x++)
			{
				Vector2I global = new(
					gridCoordinades.X * chunkSizeX + x,
					gridCoordinades.Y * chunkSizeY + y
				);

				float dist = global.Length();
				float angle = Mathf.Atan2(global.Y, global.X);
				float ax = Mathf.Cos(angle) * coastDetails;
				float ay = Mathf.Sin(angle) * coastDetails;

				float coastN = Layer0_NoiseImage.GetNoise2D(ax + 123.4f, ay - 567.8f);
				float coast01 = (coastN + 1f) * .5f;
				float radius = levelRadius * (1f + (coast01 - .5f) * 2f * coastStrength);

				float mask = 1f - (dist/radius);
				mask = Mathf.Clamp(mask, 0f, 1f);
				mask = mask * mask;

				float n = Layer0_NoiseImage.GetNoise2D(global.X * insideDetails, global.Y * insideDetails);
				float inside01 = (n + 1f) * .5f;

				float value = mask * (.65f + inside01 * .35f);

				if (value > threshould)
				{
					newChunkLayer0Data.Add(1);
				} else if (value > threshould - .05f && value < threshould)
				{
					newChunkLayer0Data.Add(2);
				}
				else
				{
					newChunkLayer0Data.Add(-1);
				}
			}
		}
		GD.Print($"CHUNK SYSTEM: created in {Delta}ms");

		ChunkData new_chunk_data = new(newChunkLayer0Data, []);

		chunksDataDictionary[gridCoordinades] = new_chunk_data;
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