using Godot;

namespace projecthorizonscs.Interface.MainMenu;

public partial class MainMenu : Control
{
	private Button _continueButton;
	public override void _Ready()
	{
		_continueButton = GetNodeOrNull<Button>("VBoxContainer/Button2");
		if (_continueButton != null)
			_continueButton.Disabled = true;
	}

	private void _OnPlayButtonUp()
	{
		GetTree().ChangeSceneToFile("uid://dd2nrqr1prju0");
	}

	private void _OnQuitButtonUp()
	{
		GetTree().Quit();
	}

	private void _OnSettingsButtonUp()
	{
		GetTree().ChangeSceneToFile("uid://dduowujep6yb0");
	}

	private void _OnCompendiumButtonUp()
	{
		GetTree().ChangeSceneToFile("res://Interface/AchievementsMenu/AchievementsMenu.tscn");
	}
}
