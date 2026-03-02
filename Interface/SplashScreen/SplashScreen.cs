using Godot;
using System;

public partial class SplashScreen : Control
{
	public void _OnAnimationFinished(string _animName)
	{
		GetTree().ChangeSceneToFile("uid://c25rg72x1rdir");
	}
}
