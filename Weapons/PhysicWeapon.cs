using Godot;
using Godot.Collections;
using System;

namespace projecthorizonscs;

public partial class PhysicWeapon : Weapon
{
	[Export]
	public int Damage = 1;
	[Export]
	public Godot.Collections.Array Effects = new();

	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}
}
