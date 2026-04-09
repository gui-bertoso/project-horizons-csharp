using Godot;

namespace projecthorizonscs.Player;

public partial class Player
{
	private void MovementBehavior()
	{
		Vector2 velocity = Velocity;
		float speed = _stats.MoveSpeed * GetCombatProfile().MoveSpeedMultiplier;

		if (_isCollecting)
		{
			velocity.X = Mathf.MoveToward(velocity.X, 0, speed);
			velocity.Y = Mathf.MoveToward(velocity.Y, 0, speed);
			Velocity = velocity;
			return;
		}

		if (_onDash)
		{
			velocity = _dashDirection * _stats.DashSpeed;
		}
		else
		{
			Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down").Normalized();

			if (direction != Vector2.Zero)
			{
				_walkParticles.Emitting = true;
				velocity = direction * speed;
			}
			else
			{
				_walkParticles.Emitting = false;
				velocity.X = Mathf.MoveToward(velocity.X, 0, speed);
				velocity.Y = Mathf.MoveToward(velocity.Y, 0, speed);
			}
		}

		Velocity = velocity;
	}

	private void UpdateFacingFromInput()
	{
		Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");

		if (direction == Vector2.Zero)
			return;

		if (direction.Y < 0f)
		{
			_topSprite.Visible = true;
			_bottomSprite.Visible = false;
			_currentSide = -1;
		}
		else if (direction.Y > 0f)
		{
			_topSprite.Visible = false;
			_bottomSprite.Visible = true;
			_currentSide = 1;
		}

		if (direction.X < 0f)
			_body.Scale = new Vector2(1f, 1f);
		else if (direction.X > 0f)
			_body.Scale = new Vector2(-1f, 1f);
	}

	private void DashBehavior(float delta)
	{
		float dashCooldown = _stats.DashCooldown * GetCombatProfile().DashCooldownMultiplier;

		if (_onDash)
		{
			DashDurationCountdown -= delta;

			if (DashDurationCountdown > 0)
				return;

			DashDurationCountdown = 0;
			_onDash = false;
			_dashParticles.Emitting = false;

			if (UsedDashCharges == _stats.DashCharges)
				DashCooldownCount = dashCooldown;

			return;
		}

		if (DashDurationCountdown == 0 && DashCooldownCount == 0 && !_onDash)
		{
			if (Input.IsActionJustPressed("dash"))
			{
				Vector2 inputDirection = Input.GetVector("move_left", "move_right", "move_up", "move_down").Normalized();

				if (inputDirection == Vector2.Zero)
					return;

				_dashParticles.Emitting = true;
				_dashDirection = inputDirection;
				DashDurationCountdown = _stats.DashDuration;
				_onDash = true;
				UsedDashCharges += 1;
			}
			else
			{
				if (UsedDashCharges <= 0)
					return;

				NotDashingTime += delta;

				if (NotDashingTime > (dashCooldown / _stats.DashCharges) * 2)
				{
					UsedDashCharges -= 1;
					NotDashingTime = 0;
				}
			}
		}
		else
		{
			if (DashCooldownCount > 0)
				DashCooldownCount -= delta;

			if (DashCooldownCount < 0 || DashDurationCountdown < 0)
			{
				DashCooldownCount = 0;
				DashDurationCountdown = 0;
				UsedDashCharges = 0;
			}
		}
	}
}
