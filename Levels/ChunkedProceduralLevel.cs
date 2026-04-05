using Godot;
using projecthorizonscs.Autoload;

namespace projecthorizonscs;

public partial class ChunkedProceduralLevel : Node2D
{
	private float _playedTime;

	public override void _Ready()
	{
		AchievementsManager.I.SetCurrentLevel(Globals.I.CurrentLevel);
		_playedTime = (float)DataManager.I.CurrentWorldData["PlayedTime"];
	}

    public override void _Process(double delta)
    {
        _playedTime += (float)delta;
    }

    public override void _ExitTree()
    {
		DataManager.I.CurrentWorldData["PlayedTime"] = _playedTime;
        DataManager.I.QuickSaveWorldData();
    }
}
