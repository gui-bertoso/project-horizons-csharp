using Godot;
using projecthorizonscs;
using projecthorizonscs.Autoload;
using System;

public partial class DebugLevel : Node2D
{
	public override void _Ready()
	{
		AchievementsManager.I.SetCurrentLevel(Globals.I.CurrentLevel);
	}

	public override void _Process(double delta)
	{
	}
}
