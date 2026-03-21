using Godot;
using Godot.Collections;
using System;

namespace projecthorizonscs;

public partial class PhysicWeapon : Weapon
{
	private CollisionShape2D _attackArea;

	[Export]
	public int Damage = 1;
	[Export]
	public Godot.Collections.Array Effects = new();

	public override void _Ready()
	{
		_attackArea = GetNode<CollisionShape2D>("AttackArea/Collision");
	}
	
	public void EnableAttackArea()
	{
		_attackArea.Disabled = false;
	}
	public void DisableAttackArea()
	{
		_attackArea.Disabled = true;
	}

	public override void _Process(double delta)
	{
	}
}
