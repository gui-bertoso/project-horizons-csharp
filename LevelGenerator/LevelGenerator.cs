using Godot;

namespace projecthorizonscs.LevelGenerator;

public partial class LevelGenerator : TileMapLayer
{

	private TileMapLayer _detailsTileMap;
	private TileMapLayer _details2TileMap;

	private FastNoiseLite _layer0NoiseImage;
	private FastNoiseLite _layer1NoiseImage;
	private FastNoiseLite _layer2NoiseImage;
	private FastNoiseLite.NoiseTypeEnum _layer0NoiseType = FastNoiseLite.NoiseTypeEnum.ValueCubic;
	private int _layer0Seed;
	private float _layer0Frequency = .01f;
	private int _layer0FractalOctaves = 8;

	public int LevelBiomeId;
	/*
Biome IDS
	- 0 = Forest
	- 1 = Dark Forest
	- 2 = Dry Forest
	- 3 = Snowlands
	- 4 = Icelands
	- 5 = Desert
	- 6 = Beach
	- 7 = Old Beach
	- 8 = Vulcanic
*/

	private int _levelSizeX = 400;
	private int _levelSizeY = 600;

	private float _insideDetails = 40f;
	private float _coastDetails = 90.0f;
	private float _coastStrength = .55f;


	public override void _Ready()
	{
		Autoload.Globals.I.LocalLevelGenerator = this;
		_detailsTileMap = GetNode<TileMapLayer>("Details");
		_details2TileMap = GetNode<TileMapLayer>("Details2");
		SetLevelData();
	}

	private void SetLevelData()
	{
		SetNoises();
		SetBiome();
		GenerateLevel();
	}

	private void GenerateLevel()
	{
		var levelRadius = Mathf.Min(_levelSizeX, _levelSizeY) * .42f;

		var halfX = _levelSizeX / 2;
		var halfY = _levelSizeY / 2;

		//Generate base layer - Layer0
		for (var y = -halfY; y < halfY; y++)
		{
			for (var x = -halfX; x < halfX; x++)
			{
				Vector2 p = new(x, y);

				var dist = p.Length();
				var angle = Mathf.Atan2(p.Y, p.X);
				var ax = Mathf.Cos(angle) * _coastDetails;
				var ay = Mathf.Sin(angle) * _coastDetails;

				var coastN = _layer0NoiseImage.GetNoise2D(ax + 123.4f, ay - 567.8f);
				var coast01 = (coastN + 1f) * .5f;
				var radius = levelRadius * (1f + (coast01 - .5f) * 2f * _coastStrength);

				var mask = 1f - (dist/radius);
				mask = Mathf.Clamp(mask, 0f, 1f);
				mask *= mask;

				var n = _layer0NoiseImage.GetNoise2D(x * _insideDetails, y * _insideDetails);
				var inside01 = (n + 1f);

				var value = mask * (.65f + inside01 * .35f);

				switch (LevelBiomeId)
				{
					case (0): 
						switch (value)
						{
							case > .45f:
								SetCell(new Vector2I(x, y), 0, new Vector2I(1, 0));
								break;
							case > .40f and < .45f:
								SetCell(new Vector2I(x, y), 0, new Vector2I(0, 1));
								break;
							default:
								SetCell(new Vector2I(x, y), 0);
								break;
						}
						break;
					case (1): 
						switch (value)
						{
							case > .45f:
								SetCell(new Vector2I(x, y), 0, new Vector2I(1, 4));
								break;
							case > .40f and < .45f:
								SetCell(new Vector2I(x, y), 0, new Vector2I(0, 1));
								break;
							default:
								SetCell(new Vector2I(x, y), 0);
								break;
						}
						break;
					case (2): 
						switch (value)
						{
							case > .45f:
								SetCell(new Vector2I(x, y), 0, new Vector2I(1, 3));
								break;
							case > .40f and < .45f:
								SetCell(new Vector2I(x, y), 0, new Vector2I(0, 1));
								break;
							default:
								SetCell(new Vector2I(x, y), 0);
								break;
						}
						break;
					case (3):
						using (var secondaryNoise = new FastNoiseLite())
						{
							secondaryNoise.Seed = _layer0Seed;
						
							switch (value)
							{
								case > .40f when secondaryNoise.GetNoise2D(x, y) > .15f && secondaryNoise.GetNoise2D(x, y) < .65f:
									SetCell(new Vector2I(x, y), 0, new Vector2I(1, 2));
									break;
								case > .40f:
									SetCell(new Vector2I(x, y), 0, new Vector2I(1, 4));
									break;
								case > .35f and < .40f:
									SetCell(new Vector2I(x, y), 0, new Vector2I(0, 1));
									break;
								default:
									SetCell(new Vector2I(x, y), 0);
									break;
							}
							break;
						}

					case (4): 

						var iceNoise = new FastNoiseLite();
						iceNoise.Seed = _layer0Seed;

						switch (value)
						{
							case > .40f when iceNoise.GetNoise2D(x, y) > .35f:
								SetCell(new Vector2I(x, y), 0, new Vector2I(2, 1));
								break;
							case > .40f when iceNoise.GetNoise2D(x, y) > .25f && iceNoise.GetNoise2D(x, y) < .35f:
								SetCell(new Vector2I(x, y), 0, new Vector2I(1, 1));
								break;
							case > .40f:
								SetCell(new Vector2I(x, y), 0, new Vector2I(1, 2));
								break;
							case > .35f and < .40f:
								SetCell(new Vector2I(x, y), 0, new Vector2I(0, 1));
								break;
							default:
								SetCell(new Vector2I(x, y), 0);
								break;
						}


						break;
					case (5): 
						var secondaryBlockNoise = new FastNoiseLite();
						secondaryBlockNoise.Seed = _layer0Seed;

						switch (value)
						{
							case > .40f when secondaryBlockNoise.GetNoise2D(x, y) > .35f:
								SetCell(new Vector2I(x, y), 0, new Vector2I(3, 2));
								break;
							case > .40f when secondaryBlockNoise.GetNoise2D(x, y) > .27f && secondaryBlockNoise.GetNoise2D(x, y) < .35f:
								SetCell(new Vector2I(x, y), 0, new Vector2I(3, 1));
								break;
							case > .40f:
								SetCell(new Vector2I(x, y), 0, new Vector2I(3, 0));
								break;
							case > .35f and < .40f:
								SetCell(new Vector2I(x, y), 0, new Vector2I(0, 1));
								break;
							default:
								SetCell(new Vector2I(x, y), 0);
								break;
						}
						break;
				}


			}
		}

		//Generate details layer - Layer1
		for (var y = -halfY; y < halfY; y++)
		{
			for (var x = -halfX; x < halfX; x++)
			{
				var value = _layer1NoiseImage.GetNoise2D(x, y);

				Vector2I cellPosition = new (x, y);

				var blockCell = GetCellSourceId(cellPosition);

				switch (LevelBiomeId)
				{
					case (0):
						if (value > 0.2 && blockCell == 0 && GetCellAtlasCoords(cellPosition) == new Vector2(1, 0))
						{
							var rng = new RandomNumberGenerator();
							_detailsTileMap.SetCell(new Vector2I(x, y), 0,
								rng.RandiRange(0, 2) == 1 ? new Vector2I(17, 0) : new Vector2I(17, 1));
						}
						else
						{
							_detailsTileMap.SetCell(new Vector2I(x, y), 0);
						}
						break;
					case (1):
						if (value > 0.15f && blockCell == 0 && GetCellAtlasCoords(cellPosition) == new Vector2(1, 4))
						{
							var rng = new RandomNumberGenerator();
							_detailsTileMap.SetCell(new Vector2I(x, y), 0,
								rng.RandiRange(0, 2) == 1 ? new Vector2I(17, 0) : new Vector2I(17, 1));
						}
						else
						{
							_detailsTileMap.SetCell(new Vector2I(x, y), 0);
						}
						break;
					case (2):
						if (value > 0.1f && blockCell == 0 && GetCellAtlasCoords(cellPosition) == new Vector2(1, 3))
						{
							var rng = new RandomNumberGenerator();
							_detailsTileMap.SetCell(new Vector2I(x, y), 0,
								rng.RandiRange(0, 2) == 1 ? new Vector2I(17, 4) : new Vector2I(17, 5));
						}
						else
						{
							_detailsTileMap.SetCell(new Vector2I(x, y), 0);
						}
						break;
					case (3):
						if (value > 0.2f && blockCell == 0 && GetCellAtlasCoords(cellPosition) == new Vector2(1, 4))
						{
							var rng = new RandomNumberGenerator();
							_detailsTileMap.SetCell(new Vector2I(x, y), 0,
								rng.RandiRange(0, 2) == 1 ? new Vector2I(17, 2) : new Vector2I(17, 3));
						}
						else
						{
							_detailsTileMap.SetCell(new Vector2I(x, y), 0);
						}
						break;
					case (4):
						if (value > 0.2f && blockCell == 0 && GetCellAtlasCoords(cellPosition) == new Vector2(1, 0))
						{
							var rng = new RandomNumberGenerator();
							_detailsTileMap.SetCell(new Vector2I(x, y), 0,
								rng.RandiRange(0, 2) == 1 ? new Vector2I(17, 0) : new Vector2I(17, 1));
						}
						else
						{
							_detailsTileMap.SetCell(new Vector2I(x, y), 0);
						}
						break;
					case (5):
						break;
				}
			}
		}
		
		//Generate details 2 layer - Layer2
		for (var y = -halfY; y < halfY; y++)
		{
			for (var x = -halfX; x < halfX; x++)
			{
				var value = _layer2NoiseImage.GetNoise2D(x, y);

				Vector2I cellPosition = new (x, y);

				var blockCell = GetCellSourceId(cellPosition);

				switch (LevelBiomeId)
				{
					case (0):
						if (value > 0.5f && blockCell == 0 && GetCellAtlasCoords(cellPosition) == new Vector2(1, 0))
						{
							var rng = new RandomNumberGenerator();
							var value2 = rng.RandiRange(0, 49);
							switch (value2)
							{
								case 3:
									_details2TileMap.SetCell(new Vector2I(x, y), 0, new Vector2I(19, 17));
									break;
								case 9:
									_details2TileMap.SetCell(new Vector2I(x, y), 0, new Vector2I(19, 15));
									break;
							}
						}
						else
						{
							_details2TileMap.SetCell(new Vector2I(x, y), 0);
						}
						break;
					case (1):
						if (value > 0.15f && blockCell == 0 && GetCellAtlasCoords(cellPosition) == new Vector2(1, 4))
						{
							var rng = new RandomNumberGenerator();
							var value2 = rng.RandiRange(0, 49);
							switch (value2)
							{
								case 3:
									_details2TileMap.SetCell(new Vector2I(x, y), 0, new Vector2I(18, 17));
									break;
								case 9:
									_details2TileMap.SetCell(new Vector2I(x, y), 0, new Vector2I(18, 15));
									break;
							}
						}
						else
						{
							_details2TileMap.SetCell(new Vector2I(x, y), 0);
						}
						break;
					case (2):
						if (value > 0.1f && blockCell == 0 && GetCellAtlasCoords(cellPosition) == new Vector2(1, 3))
						{
							var rng = new RandomNumberGenerator();
							var value2 = rng.RandiRange(0, 49);
							switch (value2)
							{
								case 3:
									_details2TileMap.SetCell(new Vector2I(x, y), 0, new Vector2I(17, 17));
									break;
								case 9:
									_details2TileMap.SetCell(new Vector2I(x, y), 0, new Vector2I(17, 15));
									break;
							}
						}
						else
						{
							_details2TileMap.SetCell(new Vector2I(x, y), 0);
						}
						break;
					case (3):
					case (4):
					case (5):
						break;
				}
			}
		}
	}

	private void SetNoises()
	{
		_layer0NoiseImage = new FastNoiseLite();
		_layer0NoiseImage.NoiseType = _layer0NoiseType;
		_layer1NoiseImage = new FastNoiseLite();
		_layer2NoiseImage = new FastNoiseLite();
		_layer2NoiseImage.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;

		var rng = new RandomNumberGenerator();
		_layer0Seed = rng.RandiRange(0, 99999999);
		_layer0NoiseImage.Seed = _layer0Seed;
		_layer0NoiseImage.FractalOctaves = _layer0FractalOctaves;
		_layer0NoiseImage.Frequency = _layer0Frequency;

		_levelSizeX = rng.RandiRange(400, 400*4);
		_levelSizeY = rng.RandiRange(600, 600*4);
	}

	private void SetBiome()
	{
		var rng = new RandomNumberGenerator();
		LevelBiomeId = rng.RandiRange(0, 5);
		//LevelBiome_ID = 1;
	}
}