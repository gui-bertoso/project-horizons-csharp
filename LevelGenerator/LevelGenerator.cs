using Godot;
using System;

public partial class LevelGenerator : TileMapLayer
{

	private TileMapLayer DetailsTileMap;
	private TileMapLayer Details2TileMap;

	private FastNoiseLite Layer0_NoiseImage;
	private FastNoiseLite Layer1_NoiseImage;
	private FastNoiseLite Layer2_NoiseImage;
	private FastNoiseLite.NoiseTypeEnum Layer0_NoiseType = FastNoiseLite.NoiseTypeEnum.ValueCubic;
	private int Layer0_Seed;
	private float Layer0_Frequency = .01f;
	private int Layer0_FractalOctaves = 8;

	private int LevelBiome_ID;
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

	private int LevelSizeX = 400;
	private int LevelSizeY = 600;

	private float InsideDetails = 40f;
	private float CoastDetails = 90.0f;
	private float CoastStrength = .55f;


	public override void _Ready()
	{
		DetailsTileMap = GetNode<TileMapLayer>("Details");
		Details2TileMap = GetNode<TileMapLayer>("Details2");
		SetLevelData();
	}

	public void SetLevelData()
	{
		SetNoises();
		SetBiome();
		GenerateLevel();
	}

	public void GenerateLevel()
	{

		Vector2 center = new(LevelSizeX/2f, LevelSizeY/2f);
		float levelRadius = Mathf.Min(LevelSizeX, LevelSizeY) * .42f;

		int halfX = LevelSizeX / 2;
		int halfY = LevelSizeY / 2;

		//Generate base layer - Layer0
		for (int y = -halfY; y < halfY; y++)
		{
			for (int x = -halfX; x < halfX; x++)
			{
				Vector2 p = new(x, y);
				Vector2 v = p;

				float dist = v.Length();
				float angle = Mathf.Atan2(v.Y, v.X);
				float ax = Mathf.Cos(angle) * CoastDetails;
				float ay = Mathf.Sin(angle) * CoastDetails;

				float coastN = Layer0_NoiseImage.GetNoise2D(ax + 123.4f, ay - 567.8f);
				float coast01 = (coastN + 1f) * .5f;
				float radius = levelRadius * (1f + (coast01 - .5f) * 2f * CoastStrength);

				float mask = 1f - (dist/radius);
				mask = Mathf.Clamp(mask, 0f, 1f);
				mask *= mask;

				float n = Layer0_NoiseImage.GetNoise2D(x * InsideDetails, y * InsideDetails);
				float inside01 = (n + 1f);

				float value = mask * (.65f + inside01 * .35f);

				switch (LevelBiome_ID)
				{
					case (0): 
						if (value > .45f)
						{
							SetCell(new Vector2I(x, y), 0, new Vector2I(1, 0));
						} else if (value > .40f && value < .45f)
						{
							SetCell(new Vector2I(x, y), 0, new Vector2I(0, 1));
						}
						else
						{
							SetCell(new Vector2I(x, y), -1);
						}
						break;
					case (1): 
						if (value > .45f)
						{
							SetCell(new Vector2I(x, y), 0, new Vector2I(1, 4));
						} else if (value > .40f && value < .45f)
						{
							SetCell(new Vector2I(x, y), 0, new Vector2I(0, 1));
						}
						else
						{
							SetCell(new Vector2I(x, y), -1);
						}
						break;
					case (2): 
						if (value > .45f)
						{
							SetCell(new Vector2I(x, y), 0, new Vector2I(1, 3));
						} else if (value > .40f && value < .45f)
						{
							SetCell(new Vector2I(x, y), 0, new Vector2I(0, 1));
						}
						else
						{
							SetCell(new Vector2I(x, y), -1);
						}
						break;
					case (3): 
						FastNoiseLite SecondaryNoise = new FastNoiseLite();
						SecondaryNoise.Seed = Layer0_Seed;
						
						if (value > .40f)
						{						
							if (SecondaryNoise.GetNoise2D(x, y) > .15f && SecondaryNoise.GetNoise2D(x, y) < .65f)
							{
								SetCell(new Vector2I(x, y), 0, new Vector2I(1, 2));
							}
							else
							{
								SetCell(new Vector2I(x, y), 0, new Vector2I(1, 4));
							}
						} else if (value > .35f && value < .40f)
						{
							SetCell(new Vector2I(x, y), 0, new Vector2I(0, 1));
						}
						else
						{
							SetCell(new Vector2I(x, y), -1);
						}
						break;
					case (4): 

						FastNoiseLite IceNoise = new FastNoiseLite();
						IceNoise.Seed = Layer0_Seed;

						if (value > .40f)
						{						
							if (IceNoise.GetNoise2D(x, y) > .35f)
						{
							SetCell(new Vector2I(x, y), 0, new Vector2I(2, 1));
						} else if (IceNoise.GetNoise2D(x, y) > .25f && IceNoise.GetNoise2D(x, y) < .35f)
						{
							SetCell(new Vector2I(x, y), 0, new Vector2I(1, 1));
						}
						else
						{
							SetCell(new Vector2I(x, y), 0, new Vector2I(1, 2));
						}
						} else if (value > .35f && value < .40f)
						{
							SetCell(new Vector2I(x, y), 0, new Vector2I(0, 1));
						}
						else
						{
							SetCell(new Vector2I(x, y), -1);
						}


						break;
					case (5): 
						FastNoiseLite SecondaryBlockNoise = new FastNoiseLite();
						SecondaryBlockNoise.Seed = Layer0_Seed;

						if (value > .40f)
						{
							if (SecondaryBlockNoise.GetNoise2D(x, y) > .35f)
							{
								SetCell(new Vector2I(x, y), 0, new Vector2I(3, 2));
							} else if (SecondaryBlockNoise.GetNoise2D(x, y) > .27f && SecondaryBlockNoise.GetNoise2D(x, y) < .35f)
							{
								SetCell(new Vector2I(x, y), 0, new Vector2I(3, 1));
							}
							else
							{
								SetCell(new Vector2I(x, y), 0, new Vector2I(3, 0));
							}
						} else if (value > .35f && value < .40f)
						{
							SetCell(new Vector2I(x, y), 0, new Vector2I(0, 1));
						}
						else
						{
							SetCell(new Vector2I(x, y), -1);
						}
						break;
				}


			}
		}

		//Generate details layer - Layer1
		for (int y = -halfY; y < halfY; y++)
		{
			for (int x = -halfX; x < halfX; x++)
			{
				float value = Layer1_NoiseImage.GetNoise2D(x, y);

				Vector2I cell_position = new (x, y);

				var block_cell = GetCellSourceId(cell_position);

				switch (LevelBiome_ID)
				{
					case (0):
						if (value > 0.2 && block_cell == 0 && GetCellAtlasCoords(cell_position) == new Vector2(1, 0))
						{
							var rng = new RandomNumberGenerator();
							if (rng.RandiRange(0, 2) == 1)
							{
								DetailsTileMap.SetCell(new Vector2I(x, y), 0, new Vector2I(17, 0));
							}
							else
							{
								DetailsTileMap.SetCell(new Vector2I(x, y), 0, new Vector2I(17, 1));
							}
						}
						else
						{
							DetailsTileMap.SetCell(new Vector2I(x, y), -1);
						}
						break;
					case (1):
						if (value > 0.15f && block_cell == 0 && GetCellAtlasCoords(cell_position) == new Vector2(1, 4))
						{
							var rng = new RandomNumberGenerator();
							if (rng.RandiRange(0, 2) == 1)
							{
								DetailsTileMap.SetCell(new Vector2I(x, y), 0, new Vector2I(17, 0));
							}
							else
							{
								DetailsTileMap.SetCell(new Vector2I(x, y), 0, new Vector2I(17, 1));
							}
						}
						else
						{
							DetailsTileMap.SetCell(new Vector2I(x, y), -1);
						}
						break;
					case (2):
						if (value > 0.1f && block_cell == 0 && GetCellAtlasCoords(cell_position) == new Vector2(1, 3))
						{
							var rng = new RandomNumberGenerator();
							if (rng.RandiRange(0, 2) == 1)
							{
								DetailsTileMap.SetCell(new Vector2I(x, y), 0, new Vector2I(17, 4));
							}
							else
							{
								DetailsTileMap.SetCell(new Vector2I(x, y), 0, new Vector2I(17, 5));
							}
						}
						else
						{
							DetailsTileMap.SetCell(new Vector2I(x, y), -1);
						}
						break;
					case (3):
						if (value > 0.2f && block_cell == 0 && GetCellAtlasCoords(cell_position) == new Vector2(1, 4))
						{
							var rng = new RandomNumberGenerator();
							if (rng.RandiRange(0, 2) == 1)
							{
								DetailsTileMap.SetCell(new Vector2I(x, y), 0, new Vector2I(17, 2));
							}
							else
							{
								DetailsTileMap.SetCell(new Vector2I(x, y), 0, new Vector2I(17, 3));
							}
						}
						else
						{
							DetailsTileMap.SetCell(new Vector2I(x, y), -1);
						}
						break;
					case (4):
						if (value > 0.2f && block_cell == 0 && GetCellAtlasCoords(cell_position) == new Vector2(1, 0))
						{
							var rng = new RandomNumberGenerator();
							if (rng.RandiRange(0, 2) == 1)
							{
								DetailsTileMap.SetCell(new Vector2I(x, y), 0, new Vector2I(17, 0));
							}
							else
							{
								DetailsTileMap.SetCell(new Vector2I(x, y), 0, new Vector2I(17, 1));
							}
						}
						else
						{
							DetailsTileMap.SetCell(new Vector2I(x, y), -1);
						}
						break;
					case (5):
						break;
				}
			}
		}
		
		//Generate details 2 layer - Layer2
		for (int y = -halfY; y < halfY; y++)
		{
			for (int x = -halfX; x < halfX; x++)
			{
				float value = Layer2_NoiseImage.GetNoise2D(x, y);

				Vector2I cell_position = new (x, y);

				var block_cell = GetCellSourceId(cell_position);

				switch (LevelBiome_ID)
				{
					case (0):
						if (value > 0.5f && block_cell == 0 && GetCellAtlasCoords(cell_position) == new Vector2(1, 0))
						{
							var rng = new RandomNumberGenerator();
							int value2 = rng.RandiRange(0, 49);
							if (value2 == 3)
							{
								Details2TileMap.SetCell(new Vector2I(x, y), 0, new Vector2I(19, 17));
							}
							else if (value2 ==9)
							{
								Details2TileMap.SetCell(new Vector2I(x, y), 0, new Vector2I(19, 15));
							}
						}
						else
						{
							Details2TileMap.SetCell(new Vector2I(x, y), -1);
						}
						break;
					case (1):
						if (value > 0.15f && block_cell == 0 && GetCellAtlasCoords(cell_position) == new Vector2(1, 4))
						{
							var rng = new RandomNumberGenerator();
							int value2 = rng.RandiRange(0, 49);
							if (value2 == 3)
							{
								Details2TileMap.SetCell(new Vector2I(x, y), 0, new Vector2I(18, 17));
							}
							else if (value2 ==9)
							{
								Details2TileMap.SetCell(new Vector2I(x, y), 0, new Vector2I(18, 15));
							}
						}
						else
						{
							Details2TileMap.SetCell(new Vector2I(x, y), -1);
						}
						break;
					case (2):
						if (value > 0.1f && block_cell == 0 && GetCellAtlasCoords(cell_position) == new Vector2(1, 3))
						{
							var rng = new RandomNumberGenerator();
							int value2 = rng.RandiRange(0, 49);
							if (value2 == 3)
							{
								Details2TileMap.SetCell(new Vector2I(x, y), 0, new Vector2I(17, 17));
							}
							else if (value2 ==9)
							{
								Details2TileMap.SetCell(new Vector2I(x, y), 0, new Vector2I(17, 15));
							}
						}
						else
						{
							Details2TileMap.SetCell(new Vector2I(x, y), -1);
						}
						break;
					case (3):
						break;
					case (4):
						break;
					case (5):
						break;
				}
			}
		}
	}

	public void SetNoises()
	{
		Layer0_NoiseImage = new FastNoiseLite();
		Layer0_NoiseImage.NoiseType = Layer0_NoiseType;
		Layer1_NoiseImage = new FastNoiseLite();
		Layer2_NoiseImage = new FastNoiseLite();
		Layer2_NoiseImage.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;

		var rng = new Godot.RandomNumberGenerator();
		Layer0_Seed = rng.RandiRange(0, 99999999);
		Layer0_NoiseImage.Seed = Layer0_Seed;
		Layer0_NoiseImage.FractalOctaves = Layer0_FractalOctaves;
		Layer0_NoiseImage.Frequency = Layer0_Frequency;

		LevelSizeX = rng.RandiRange(400, 400*4);
		LevelSizeY = rng.RandiRange(600, 600*4);
	}

	public void SetBiome()
	{
		var rng = new Godot.RandomNumberGenerator();
		LevelBiome_ID = rng.RandiRange(0, 5);
		//LevelBiome_ID = 1;
	}
}