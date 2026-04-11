using Godot;

namespace projecthorizonscs;

public partial class Environment : WorldEnvironment
{
	private CanvasModulate _baseModulate;

	private Node2D _snowBiomeEffects;
	private Node2D _desertBiomeEffects;
	private Node2D _forestBiomeEffects;
	private Node2D _darkForestBiomeEffects;

	private DirectionalLight2D _directionalLight2D;

	private readonly RandomNumberGenerator _rng = new();
	private Godot.Environment _environment;

	public override void _Ready()
	{
		_rng.Randomize();

		_baseModulate = GetNode<CanvasModulate>("BaseModulate");

		_snowBiomeEffects = GetNode<Node2D>("SnowBiomeEffects");
		_desertBiomeEffects = GetNode<Node2D>("DesertBiomeEffects");
		_forestBiomeEffects = GetNode<Node2D>("ForestBiomeEffects");
		_darkForestBiomeEffects = GetNode<Node2D>("DarkForestBiomeEffects");

		_directionalLight2D = GetNode<DirectionalLight2D>("DirectionalLight2D");

		ResetVisualState();
		ApplyMinimalEnvironment();
		SetToBiome();
	}

	private void ResetVisualState()
	{
		_baseModulate.Visible = false;

		_snowBiomeEffects.Visible = false;
		_desertBiomeEffects.Visible = false;
		_forestBiomeEffects.Visible = false;
		_darkForestBiomeEffects.Visible = false;

		SetChildrenVisible(_snowBiomeEffects, false);
		SetChildrenVisible(_desertBiomeEffects, false);
		SetChildrenVisible(_forestBiomeEffects, false);
		SetChildrenVisible(_darkForestBiomeEffects, false);

		_directionalLight2D.Visible = true;
		_directionalLight2D.Color = Colors.White;
		_directionalLight2D.Energy = 0.60f;
		_directionalLight2D.Rotation = Mathf.DegToRad(-26.0f);
	}

	private void ApplyMinimalEnvironment()
	{
		_environment = new Godot.Environment
		{
			BackgroundMode = Godot.Environment.BGMode.Canvas,
			TonemapMode = Godot.Environment.ToneMapper.Agx,
			TonemapExposure = 0.90f,

			AdjustmentEnabled = true,
			AdjustmentBrightness = 0.95f,
			AdjustmentContrast = 1.05f,
			AdjustmentSaturation = 0.92f,

			GlowEnabled = false
		};

		Environment = _environment;

		_baseModulate.Color = Colors.White;
	}

	private void SetToBiome()
	{
		var levelGenerator = Autoload.Globals.I.LocalLevelGenerator;
		if (levelGenerator == null)
		{
			_baseModulate.Visible = true;
			return;
		}

		var biomeId = levelGenerator.LevelBiomeId;

		switch (biomeId)
		{
			case 1:
				ApplyForestBiome();
				break;

			case 2:
				ApplyDarkForestBiome();
				break;

			case 3:
				ApplySnowBiome(snowChancePercent: 30);
				break;

			case 4:
				ApplySnowBiome(snowChancePercent: 50);
				break;

			case 5:
				ApplyDesertBiome(windChancePercent: 70);
				break;

			default:
				_baseModulate.Visible = true;
				break;
		}
	}

	private void ApplyForestBiome()
	{
		_forestBiomeEffects.Visible = true;

		var calmModulate = _forestBiomeEffects.GetNodeOrNull<CanvasModulate>("CalmModulate");
		var rainModulate = _forestBiomeEffects.GetNodeOrNull<CanvasModulate>("RainModulate");
		var rainLayer = _forestBiomeEffects.GetNodeOrNull<CanvasLayer>("RainLayer");

		var isRaining = Roll(20);

		if (isRaining)
		{
			if (rainModulate != null)
				rainModulate.Visible = true;

			if (rainLayer != null)
				rainLayer.Visible = true;
		}
		else
		{
			if (calmModulate != null)
				calmModulate.Visible = true;
		}
	}

	private void ApplyDarkForestBiome()
	{
		_darkForestBiomeEffects.Visible = true;

		var calmModulate = _darkForestBiomeEffects.GetNodeOrNull<CanvasModulate>("CalmModulate");
		var rainModulate = _darkForestBiomeEffects.GetNodeOrNull<CanvasModulate>("RainModulate");
		var rainLayer = _darkForestBiomeEffects.GetNodeOrNull<CanvasLayer>("RainLayer");

		var isRaining = Roll(30);

		if (isRaining)
		{
			if (rainModulate != null)
				rainModulate.Visible = true;

			if (rainLayer != null)
				rainLayer.Visible = true;
		}
		else
		{
			if (calmModulate != null)
				calmModulate.Visible = true;
		}
	}

	private void ApplySnowBiome(int snowChancePercent)
	{
		_snowBiomeEffects.Visible = true;

		var baseModulate = _snowBiomeEffects.GetNodeOrNull<CanvasModulate>("BaseSnowModulate");
		var snowLayer = _snowBiomeEffects.GetNodeOrNull<CanvasLayer>("SnowLayer");

		if (baseModulate != null)
			baseModulate.Visible = true;

		if (snowLayer != null && Roll(snowChancePercent))
			snowLayer.Visible = true;
	}

	private void ApplyDesertBiome(int windChancePercent)
	{
		_desertBiomeEffects.Visible = true;

		var baseModulate = _desertBiomeEffects.GetNodeOrNull<CanvasModulate>("BaseDesertModulate");
		var windLayer = _desertBiomeEffects.GetNodeOrNull<CanvasLayer>("WindLayer");

		if (baseModulate != null)
			baseModulate.Visible = true;

		if (windLayer != null && Roll(windChancePercent))
			windLayer.Visible = true;
	}

	private bool Roll(int chancePercent)
	{
		return _rng.RandiRange(1, 100) <= chancePercent;
	}

	private static void SetChildrenVisible(Node parent, bool visible)
	{
		foreach (Node child in parent.GetChildren())
		{
			if (child is CanvasItem canvasItem)
				canvasItem.Visible = visible;
		}
	}
}
