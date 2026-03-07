using Godot;

public partial class Environment : WorldEnvironment
{
	private CanvasModulate _BaseModulate;
	private Node2D _SnowBiomeEffects;
	private Node2D _DesertBiomeEffects;
	private Node2D _ForestBiomeEffects;
	private Node2D _DarkForestBiomeEffects;

	private Variant env;

	public override void _Ready()
	{
		_BaseModulate = GetNode<CanvasModulate>("BaseModulate");
		_SnowBiomeEffects = GetNode<Node2D>("SnowBiomeEffects");
		_DesertBiomeEffects = GetNode<Node2D>("DesertBiomeEffects");
		_ForestBiomeEffects = GetNode<Node2D>("ForestBiomeEffects");
		_DarkForestBiomeEffects = GetNode<Node2D>("DarkForestBiomeEffects");
		SetToBiome();
	}

	public void SetToBiome()
	{
		if (Globals.I.LocalLevelGenerator == null)
		{
			_BaseModulate.Visible = true;
		}
		else
		{
			int biomeID = Globals.I.LocalLevelGenerator.LevelBiome_ID;
			switch (biomeID)
			{
				case 1:
					bool IsRaining = new RandomNumberGenerator().RandiRange(0, 9) < 2;
					_ForestBiomeEffects.Visible = true;
					if (IsRaining)
					{
						_ForestBiomeEffects.GetChild<CanvasModulate>(1).Visible = true;
						_ForestBiomeEffects.GetChild<CanvasLayer>(2).Visible = true;
					}
					else
					{
						_ForestBiomeEffects.GetChild<CanvasModulate>(0).Visible = true;
					}

					env = ResourceLoader.Load("res://BiomeEnvinroments/ForestEnvinroment.tres");
					Environment = (Godot.Environment)env;
					break;
				case 2:
					bool IsRaining2 = new RandomNumberGenerator().RandiRange(0, 9) < 3;
					_ForestBiomeEffects.Visible = true;
					if (IsRaining2)
					{
						_DarkForestBiomeEffects.GetChild<CanvasModulate>(1).Visible = true;
						_DarkForestBiomeEffects.GetChild<CanvasLayer>(2).Visible = true;
					}
					else
					{
						_DarkForestBiomeEffects.GetChild<CanvasModulate>(0).Visible = true;
					}

					env = ResourceLoader.Load("res://BiomeEnvinroments/DarkForestEnvinroment.tres");
					Environment = (Godot.Environment)env;
					break;
				case 3:
					bool IsSnowing = new RandomNumberGenerator().RandiRange(0, 9) < 3;
					_SnowBiomeEffects.Visible = true;
					if (IsSnowing)
					{
						_SnowBiomeEffects.GetChild<CanvasModulate>(0).Visible = true;
						_SnowBiomeEffects.GetChild<CanvasLayer>(1).Visible = true;
					}
					else
					{
						_SnowBiomeEffects.GetChild<CanvasModulate>(0).Visible = true;
					}

					env = ResourceLoader.Load("res://BiomeEnvinroments/SnowEnvinroment.tres");
					Environment = (Godot.Environment)env;
					break;
				case 4:
					bool IsSnowing2 = new RandomNumberGenerator().RandiRange(0, 9) < 5;
					_SnowBiomeEffects.Visible = true;
					if (IsSnowing2)
					{
						_SnowBiomeEffects.GetChild<CanvasModulate>(0).Visible = true;
						_SnowBiomeEffects.GetChild<CanvasLayer>(1).Visible = true;
					}
					else
					{
						_SnowBiomeEffects.GetChild<CanvasModulate>(0).Visible = true;
					}

					env = ResourceLoader.Load("res://BiomeEnvinroments/SnowEnvinroment.tres");
					Environment = (Godot.Environment)env;
					break;
				case 5:
					bool IsWindy = new RandomNumberGenerator().RandiRange(0, 9) < 7;
					_DesertBiomeEffects.Visible = true;
					if (IsWindy)
					{
						_DesertBiomeEffects.GetChild<CanvasModulate>(0).Visible = true;
						_DesertBiomeEffects.GetChild<CanvasLayer>(1).Visible = true;
					}
					else
					{
						_DesertBiomeEffects.GetChild<CanvasModulate>(0).Visible = true;
					}

					env = ResourceLoader.Load("res://BiomeEnvinroments/DesertEnvinroment.tres");
					Environment = (Godot.Environment)env;
					break;
				default: _BaseModulate.Visible = true; break;
			}
		}
	}
}
