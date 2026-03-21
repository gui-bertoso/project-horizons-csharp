using Godot;
using System;

namespace projecthorizonscs;

public partial class Weapon : Node2D
{
	private Sprite2D _sprite;

	[Export]
	public float UseSpeed = 1.0f;
	[Export]
	public float Cooldown = 1.0f;
	[Export]
	public Item ItemData;

	public override void _Ready()
	{
		_sprite = GetNode<Sprite2D>("Sprite");
	}

	public virtual void Action()
	{
		
	}
}
