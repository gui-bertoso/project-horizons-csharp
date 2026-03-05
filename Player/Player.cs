using Godot;
using System;
using System.Data.Common;

public partial class Player : CharacterBody2D
{
	private PlayerStats _Stats;


	private bool OnDash = false;
	public float DashDurationCountdown;
	public float DashCooldownCount;
	public int UsedDashCharges;
	private Vector2 DashDirection;

	public float NotDashingTime;
	
	public override void _Ready()
	{
		Globals.I.LocalPlayer = this;

		_Stats = GetNode<PlayerStats>("Stats");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Globals.I.InMenu)
		{
			return;
		}
		MovementBehavior();
		DashBehavior((float)delta);
		MoveAndSlide();
	}

	private void MovementBehavior()
	{
		Vector2 velocity = Velocity;

		if (OnDash)
		{
			velocity = DashDirection * _Stats.DashSpeed;
		}
		else
		{
			int speed = _Stats.MoveSpeed;

			Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down").Normalized();

			if (direction != Vector2.Zero)
			{
				velocity = direction * speed;
			}
			else
			{
				velocity.X = Mathf.MoveToward(velocity.X, 0, speed);
				velocity.Y = Mathf.MoveToward(velocity.Y, 0, speed);
			}
		}

		Velocity = velocity;
	}

	private void DashBehavior(float delta)
	{
		if (OnDash)
		{
			DashDurationCountdown -= delta;
			if (DashDurationCountdown <= 0)
			{
				DashDurationCountdown = 0;
				OnDash = false;
				if (UsedDashCharges == _Stats.DashCharges)
				{
					DashCooldownCount = _Stats.DashCooldown;
				}
			}
			return;
		}

		if (DashDurationCountdown == 0 && DashCooldownCount == 0 && !OnDash)
		{
			if (Input.IsActionJustPressed("dash"))
			{
				DashDirection = Input.GetVector("move_left", "move_right", "move_up", "move_down").Normalized();
				DashDurationCountdown = _Stats.DashDuration;
				OnDash = true;
				UsedDashCharges += 1;
			}
			else
			{
				if (UsedDashCharges > 0)
				{
					NotDashingTime += delta;
					if (NotDashingTime > (_Stats.DashCooldown / _Stats.DashCharges)*2)
					{
						UsedDashCharges -= 1;
						NotDashingTime = 0;
					}
				}
			}
		}
		else
		{
			if (DashCooldownCount > 0)
			{
				DashCooldownCount -= delta;
			}

			if (DashCooldownCount < 0 || DashDurationCountdown < 0)
			{
				DashCooldownCount = 0;
				DashDurationCountdown = 0;
				UsedDashCharges = 0;
			}
		}
	}
}
