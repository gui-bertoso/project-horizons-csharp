using Godot;
using System;
using projecthorizonscs.Autoload;

namespace projecthorizonscs;

public partial class EmeraldLevel : Node2D
{
	public float playedTime;

	public override void _Ready()
	{
		AchievementsManager.I.SetCurrentLevel(Globals.I.CurrentLevel);
		playedTime = (float)DataManager.I.CurrentWorldData["PlayedTime"];
		GD.Print($"Loaded played time: {playedTime}");
	}

    public override void _Process(double delta)
    {
        playedTime += (float)delta;
    }

    public override void _ExitTree()
    {
		GD.Print($"Final played time to save: {playedTime}");
		DataManager.I.CurrentWorldData["PlayedTime"] = playedTime;
        DataManager.I.QuickSaveWorldData();
    }

}
