using System.Collections.Generic;
using Godot;

namespace projecthorizonscs.NewLevelGeneratorTests;

public sealed class ChunkData(List<int> layer0, List<int> layer1)
{
	public readonly List<int> Layer0 = layer0;
	public List<int> Layer1 = layer1;
}

public partial class NextLevelGenerator : TileMapLayer
{

	private FastNoiseLite _layer0NoiseImage;
	private FastNoiseLite.NoiseTypeEnum _layer0NoiseType = FastNoiseLite.NoiseTypeEnum.ValueCubic;
	private int _layer0Seed;
	private float _layer0Frequency = .01f;
	private int _layer0FractalOctaves = 8;

	private float _insideDetails = 40f;
	private float _coastDetails = 90.0f;
	private float _coastStrength = .55f;
	private float _threshould = .45f;

	private double _delta;

	private int _chunkSizeX = 25;
	private int _chunkSizeY = 75;

	private int _levelSizeX = 2000;
	private int _levelSizeY = 2800;

	private Vector2I _playerPosition;
	private Vector2I _currentPlayerChunk = new (0, 0);

	private const int SourceId = 0;

	private Vector2I _lastRenderedChunk;

	private readonly Dictionary<Vector2I, ChunkData> _chunksDataDictionary = new ();

	private readonly Dictionary<int, Vector2I> _blockIDs = new()
	{
		{0, new Vector2I(0, 0)}, //void
		{1, new Vector2I(1, 0)}, //half-void
		{2, new Vector2I(0, 1)}, //grass
		{3, new Vector2I(0, 2)}, //dirt
	};

	public override void _Process(double delta)
	{
		_delta = delta;

		var (x, y) = LocalToMap(ToLocal(Autoload.Globals.I.LocalPlayer.Position));
		Vector2I currentPlayerChunk = new (Mathf.FloorToInt(x / _chunkSizeX), Mathf.FloorToInt(y / _chunkSizeY));
		Autoload.Globals.I.CurrentPlayerChunk = currentPlayerChunk;

		RenderChunks();
		
	}

	private void RenderChunks()
	{
		const int renderDistance = 1;
		for (var y = -renderDistance; y <= renderDistance; y++)
		for(var x = -renderDistance; x <= renderDistance; x++)
			RenderChunk(_currentPlayerChunk + new Vector2I(x, y));
	}

	private void RenderChunk(Vector2I chunkPosition)
	{
		var halfX = _chunkSizeX / 2;
		var halfY = _chunkSizeY / 2;

		var originX = chunkPosition.X * _chunkSizeX;
		var originY = chunkPosition.Y * _chunkSizeY;

		var i = 0;
		for (var y = -halfY; y < halfY; y++)
		{
			for (var x = -halfX; x < halfX; x++)
			{
				var currentChunk = _chunksDataDictionary[chunkPosition];
				var id = currentChunk.Layer0[i++];
				
				Vector2I cell = new (originX + x, originY + y);

				if (id == -1)
				{
					SetCell(cell, 0);
					continue;
				}

				SetCell(cell, SourceId, _blockIDs[id]);

			}
		}
	}

	public override void _Ready()
	{
		SetNoises();
		GenerateLevel();
	}

	private void GenerateLevel()
	{
		var chunksX = _levelSizeX / _chunkSizeX;
		var chunksY = _levelSizeY / _chunkSizeY;
		
		for (var y = -chunksY; y < chunksY; y++)
		{
			for (var x = -chunksX; x < chunksX; x++)
			{
				Vector2I gridCoordinade = new (x, y);
				GenerateChunkData(gridCoordinade);
			}
		}
	}

	private void GenerateChunkData(Vector2I gridCoordinades)
	{

		GD.Print($"CHUNK SYSTEM: creating new in {gridCoordinades}");

		List<int> newChunkLayer0Data = [];

		var levelRadius = Mathf.Min(_levelSizeX, _levelSizeY) * .42f;

		var halfX = _chunkSizeX / 2;
		var halfY = _chunkSizeY / 2;

		for (var y = -halfY; y < halfY; y++)
		{
			for (var x = -halfX; x < halfX; x++)
			{
				Vector2I global = new(
					gridCoordinades.X * _chunkSizeX + x,
					gridCoordinades.Y * _chunkSizeY + y
				);

				var dist = global.Length();
				var angle = Mathf.Atan2(global.Y, global.X);
				var ax = Mathf.Cos(angle) * _coastDetails;
				var ay = Mathf.Sin(angle) * _coastDetails;

				var coastN = _layer0NoiseImage.GetNoise2D(ax + 123.4f, ay - 567.8f);
				var coast01 = (coastN + 1f) * .5f;
				var radius = levelRadius * (1f + (coast01 - .5f) * 2f * _coastStrength);

				var mask = 1f - (dist/radius);
				mask = Mathf.Clamp(mask, 0f, 1f);
				mask = mask * mask;

				var n = _layer0NoiseImage.GetNoise2D(global.X * _insideDetails, global.Y * _insideDetails);
				var inside01 = (n + 1f) * .5f;

				var value = mask * (.65f + inside01 * .35f);

				if (value > _threshould)
				{
					newChunkLayer0Data.Add(1);
				} else if (value > _threshould - .05f && value < _threshould)
				{
					newChunkLayer0Data.Add(2);
				}
				else
				{
					newChunkLayer0Data.Add(-1);
				}
			}
		}
		GD.Print($"CHUNK SYSTEM: created in {_delta}ms");

		ChunkData newChunkData = new(newChunkLayer0Data, []);

		_chunksDataDictionary[gridCoordinades] = newChunkData;
	}


	private void SetNoises()
	{
		_layer0NoiseImage = new FastNoiseLite();
		_layer0NoiseImage.NoiseType = _layer0NoiseType;

		var rng = new RandomNumberGenerator();
		_layer0Seed = rng.RandiRange(0, 99999999);
		_layer0NoiseImage.Seed = _layer0Seed;
		_layer0NoiseImage.FractalOctaves = _layer0FractalOctaves;
		_layer0NoiseImage.Frequency = _layer0Frequency;
	}
}
