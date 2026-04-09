using Godot;

namespace projecthorizonscs.Enemies.Slime;

public partial class Slime : projecthorizonscs.Enemies.EnemyTemplate
{
	private Color[] _colors =
	{
		new(0.4f, 1f, 0.4f),
		new(0.4f, 0.8f, 1f),
		new(1f, 0.4f, 0.4f),
		new(1f, 0.9f, 0.3f),
		new(0.8f, 0.4f, 1f),
		new(1f, 1f, 1f),
		new(0.4f, 0.4f, 0.4f)
	};

	public override void _Ready()
	{
		base._Ready();

		var rng = new RandomNumberGenerator();
		rng.Randomize();

		var color = _colors[rng.RandiRange(0, _colors.Length - 1)];

		if (_bodySprite.Material is ShaderMaterial shaderMaterial)
		{
			shaderMaterial.SetShaderParameter("base_tint", color);
		}
	}
}