using Godot;
using System;

public partial class SettingsMenu : Control
{
	public void _OnBackButtonUp()
	{
		GetTree().ChangeSceneToFile("uid://c25rg72x1rdir");
	}
}
