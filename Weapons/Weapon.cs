using Godot;
using System;

namespace projecthorizonscs;

public partial class Weapon : Node2D
{
	private Sprite2D _sprite;
	private float _cooldownTimer;

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

	public override void _PhysicsProcess(double delta)
	{
		if (_cooldownTimer > 0f)
			_cooldownTimer -= (float)delta;
	}

	public virtual void Action()
	{
		
	}

	protected bool CanUse()
	{
		return _cooldownTimer <= 0f;
	}

	protected void TriggerCooldown(float multiplier = 1f)
	{
		_cooldownTimer = Mathf.Max(0.01f, Cooldown * multiplier);
	}
}
