using Godot;

namespace projecthorizonscs.Interface.Hud;

public partial class Hud : CanvasLayer
{
	private PauseMenu.PauseMenu _pauseMenu;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		_pauseMenu = GetNode<PauseMenu.PauseMenu>("PauseMenu");
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("pause"))
		{
			_pauseMenu.Toggle();
			GetViewport().SetInputAsHandled();
		}
	}
}
