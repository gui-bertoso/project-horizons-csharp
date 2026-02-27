using Godot;
using System;

public partial class LevelGenerator : TileMapLayer
{

	private FastNoiseLite Layer0_NoiseImage;
	private FastNoiseLite.NoiseTypeEnum Layer0_NoiseType = FastNoiseLite.NoiseTypeEnum.ValueCubic;
	private int Layer0_Seed;
	private float Layer0_Frequency = .01f;
	private int Layer0_FractalOctaves = 8;

	private int level_size_x = 400;
	private int level_size_y = 600;

	private float insideDetails = 40f;
	private float coastDetails = 90.0f;
	private float coastStrength = .55f;
	private float threshould = .45f;

	public override void _Ready()
	{
		SetNoises();
		GenerateLevel();
	}

	public void GenerateLevel()
	{

		Vector2 center = new(level_size_x/2f, level_size_y/2f);
		float levelRadius = Mathf.Min(level_size_x, level_size_y) * .42f;

		int halfX = level_size_x / 2;
		int halfY = level_size_y / 2;

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
