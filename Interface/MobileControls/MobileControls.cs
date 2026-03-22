using Godot;
using System;

public partial class MobileControls : Control
{
	public override void _Ready()
	{
		if (OS.HasFeature("mobile"))
		{
			((Control)GetNode("Control3")).Show();
		}
	}
}
