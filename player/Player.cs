using Godot;
using System;
using System.Numerics;

public partial class Player : CharacterBody2D
{
	public const float Speed = 300.0f;
	public const float JumpVelocity = -400.0f;

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

		if (direction != Vector2.Zero)
		{
			velocity = direction * Speed;
		}

		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Y = Mathf.MoveToward(Velocity.Y, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	private MovementBehavior()
	{
		Vector2 velocity = Velocity;

		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

		if (direction != Vector2.Zero)
		{
			velocity = direction * Speed;
		}
		else
		{
			velocity.X = MathF.MoveToward(velocity.X, 0, Speed);
			velocity.X = MathF.MoveToward(velocity.X, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide()

	}
}
