using Godot;
using projecthorizonscs;
using projecthorizonscs.Autoload;
using System;

public partial class DebugLevel : Node2D
{
	private Godot.Collections.Array<string> EnemiesScenePath = new()
	{
		"uid://cbi26ck84a7vt",
		"uid://834fniewtsf1",
		"uid://iqddsepl7qw2",
		"uid://dsme77lawliia"
	};

	public override void _Ready()
	{
		AchievementsManager.I.SetCurrentLevel(Globals.I.CurrentLevel);

		var rng = new RandomNumberGenerator();
		for (var i = 0; i < 10; i++)
		{
			EnemiesManager.I.SpawnEnemy(EnemiesScenePath[rng.RandiRange(0, EnemiesScenePath.Count-1)]);
		}
	}

	public override void _Process(double delta)
	{
	}
}
