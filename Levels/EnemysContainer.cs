using Godot;
using System;

public partial class EnemysContainer : Node2D
{
	public override void _Ready()
	{
		EnemysManager.I.EnemysContainer = this;
	}
}
