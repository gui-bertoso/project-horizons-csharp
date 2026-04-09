using Godot;
using System;

public partial class EnemiesContainer : Node2D
{
	public override void _Ready()
	{
		EnemiesManager.I.EnemiesContainer = this;
	}
}
