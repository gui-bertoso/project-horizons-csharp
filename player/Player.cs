using Godot;
using System;

public partial class Player : CharacterBody2D
{
	private PlayerStats _Stats;

	private float DashDurationCountdown;
	private float DashCooldownCount;
	
	public override void _Ready()
	{
		Globals.I.LocalPlayer = this;

		_Stats = GetNode<PlayerStats>("Stats");
	}

	public override void _PhysicsProcess(double delta)
	{
		MovementBehavior();
		DashBehavior();
	}

	private void MovementBehavior()
	{
		int speed = _Stats.MoveSpeed;

		Vector2 velocity = Velocity;

		Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");

		if (direction != Vector2.Zero)
		{
			velocity = direction * speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(velocity.X, 0, speed);
			velocity.X = Mathf.MoveToward(velocity.X, 0, speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	private void DashBehavior()
	{
		
	}
}
