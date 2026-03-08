using Godot;

namespace projecthorizonscs.Envinroment;

public partial class Environment(Node2D desertBiomeEffects) : WorldEnvironment
{
	private CanvasModulate _baseModulate;
	private Node2D _snowBiomeEffects;
	private Node2D _desertBiomeEffects = desertBiomeEffects;
	private Node2D _forestBiomeEffects;
	private Node2D _darkForestBiomeEffects;

	private Variant _env;
	private DirectionalLight2D _directionalLight2D;

	public override void _Ready()
	{
		_baseModulate = GetNode<CanvasModulate>("BaseModulate");
		_snowBiomeEffects = GetNode<Node2D>("SnowBiomeEffects");
		_desertBiomeEffects = GetNode<Node2D>("DesertBiomeEffects");
		_forestBiomeEffects = GetNode<Node2D>("ForestBiomeEffects");
		_darkForestBiomeEffects = GetNode<Node2D>("DarkForestBiomeEffects");
		_directionalLight2D = GetNode<DirectionalLight2D>("DirectionalLight2D");
		
		SetToBiome();
	}

	private void SetToBiome()
	{
		if (Autoload.Globals.I.LocalLevelGenerator == null)
		{
			_baseModulate.Visible = true;
		}
		else
		{
			var biomeId = Autoload.Globals.I.LocalLevelGenerator.LevelBiomeId;
			switch (biomeId)
			{
				case 1:
					var isRaining = new RandomNumberGenerator().RandiRange(0, 9) < 2;
					_forestBiomeEffects.Visible = true;
					if (isRaining)
					{
						_forestBiomeEffects.GetChild<CanvasModulate>(1).Visible = true;
						_forestBiomeEffects.GetChild<CanvasLayer>(2).Visible = true;
					}
					else
					{
						_forestBiomeEffects.GetChild<CanvasModulate>(0).Visible = true;
					}

					_env = ResourceLoader.Load("res://BiomeEnvinroments/ForestEnvinroment.tres");
					Environment = (Godot.Environment)_env;
					break;
				case 2:
					var isRaining2 = new RandomNumberGenerator().RandiRange(0, 9) < 3;
					_forestBiomeEffects.Visible = true;
					if (isRaining2)
					{
						_darkForestBiomeEffects.GetChild<CanvasModulate>(1).Visible = true;
						_darkForestBiomeEffects.GetChild<CanvasLayer>(2).Visible = true;
					}
					else
					{
						_darkForestBiomeEffects.GetChild<CanvasModulate>(0).Visible = true;
					}

					_env = ResourceLoader.Load("res://BiomeEnvinroments/DarkForestEnvinroment.tres");
					Environment = (Godot.Environment)_env;
					break;
				case 3:
					var isSnowing = new RandomNumberGenerator().RandiRange(0, 9) < 3;
					_snowBiomeEffects.Visible = true;
					_snowBiomeEffects.GetChild<CanvasModulate>(0).Visible = true;
					if (isSnowing)
					{
						_snowBiomeEffects.GetChild<CanvasLayer>(1).Visible = true;
					}

					_env = ResourceLoader.Load("res://BiomeEnvinroments/SnowEnvinroment.tres");
					Environment = (Godot.Environment)_env;
					break;
				case 4:
					var isSnowing2 = new RandomNumberGenerator().RandiRange(0, 9) < 5;
					_snowBiomeEffects.Visible = true;
					_snowBiomeEffects.GetChild<CanvasModulate>(0).Visible = true;
					if (isSnowing2)
					{
						_snowBiomeEffects.GetChild<CanvasLayer>(1).Visible = true;
					}

					_env = ResourceLoader.Load("res://BiomeEnvinroments/SnowEnvinroment.tres");
					Environment = (Godot.Environment)_env;
					break;
				case 5:
					var isWindy = new RandomNumberGenerator().RandiRange(0, 9) < 7;
					_desertBiomeEffects.Visible = true;
					_desertBiomeEffects.GetChild<CanvasModulate>(0).Visible = true;
					if (isWindy)
					{
						_desertBiomeEffects.GetChild<CanvasLayer>(1).Visible = true;
					}

					_env = ResourceLoader.Load("res://BiomeEnvinroments/DesertEnvinroment.tres");
					Environment = (Godot.Environment)_env;
					break;
				default: _baseModulate.Visible = true; break;
			}
		}
	}
}