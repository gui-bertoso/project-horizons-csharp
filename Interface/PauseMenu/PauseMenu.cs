using Godot;
using projecthorizonscs.Autoload;
using SettingsMenuControl = projecthorizonscs.Interface.SettingsMenu.SettingsMenu;
using System;

namespace projecthorizonscs.Interface.PauseMenu;

public partial class PauseMenu : Control
{
	private Control _menuRoot;
	private SettingsMenuControl _settingsMenu;
	private Label _statusLabel;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		Visible = false;

		_menuRoot = GetNode<Control>("%MenuRoot");
		_settingsMenu = GetNode<SettingsMenuControl>("%SettingsMenu");
		_statusLabel = GetNode<Label>("%StatusLabel");

		_settingsMenu.ProcessMode = ProcessModeEnum.WhenPaused;
		_settingsMenu.SetEmbeddedMode(true);
		_settingsMenu.Connect("CloseRequested", Callable.From(OnSettingsClosed));
	}

	public void Toggle()
	{
		if (Visible)
		{
			if (_settingsMenu.Visible)
			{
				ShowMenuButtons();
				return;
			}

			ResumeGame();
			return;
		}

		OpenMenu();
	}

	private void OpenMenu()
	{
		Visible = true;
		ShowMenuButtons();
		GetTree().Paused = true;
		Globals.I.InMenu = true;
	}

	private void ResumeGame()
	{
		GetTree().Paused = false;
		Globals.I.InMenu = false;
		Visible = false;
		_statusLabel.Text = "";
	}

	private void ShowMenuButtons()
	{
		_menuRoot.Visible = true;
		_settingsMenu.Visible = false;
		_statusLabel.Text = "";
	}

	private void _OnContinueButtonUp()
	{
		ResumeGame();
	}

	private void _OnSaveButtonUp()
	{
		if (Globals.I.LocalPlayer != null)
			DataManager.I.CurrentWorldData["PlayerPosition"] = Globals.I.LocalPlayer.GlobalPosition;

		DataManager.I.CurrentWorldData["CurrentLevel"] = Globals.I.CurrentLevel;
		DataManager.I.SaveGameData();
		DataManager.I.QuickSaveWorldData();

		_statusLabel.Text = "Jogo salvo.";
	}

	private void _OnSettingsButtonUp()
	{
		_menuRoot.Visible = false;
		_settingsMenu.Visible = true;
		_statusLabel.Text = "";
	}

	private void _OnExitButtonUp()
	{
		GetTree().Paused = false;
		Globals.I.InMenu = false;
		Visible = false;
		GetTree().ChangeSceneToFile("uid://c25rg72x1rdir");
	}

	private void OnSettingsClosed()
	{
		ShowMenuButtons();
	}
}
