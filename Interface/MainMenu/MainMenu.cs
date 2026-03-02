using Godot;
using System;

public partial class MainMenu : Control
{
	public Button ContinueButton;
    public override void _Ready()
    {
        ContinueButton = GetNode<Button>("VBoxContainer/Button2");
		ContinueButton.Disabled = true;
    }
	public void _OnQuitButtonUp()
	{
		GetTree().Quit();
	}
	public void _OnSettingsButtonUp()
	{
		GetTree().ChangeSceneToFile("uid://dduowujep6yb0");
	}
}
