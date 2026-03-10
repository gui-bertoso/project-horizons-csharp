using Godot;

namespace projecthorizonscs.Player;

public partial class Player : CharacterBody2D
{
	private PlayerStats _stats;

	private Sprite2D _topSprite;
	private Sprite2D _bottomSprite;


	private bool _onDash;
	public float DashDurationCountdown;
	public float DashCooldownCount;
	public int UsedDashCharges;
	private Vector2 _dashDirection;

	public float NotDashingTime;
	
	public override void _Ready()
	{
		Autoload.Globals.I.LocalPlayer = this;

		_topSprite = GetNode<Sprite2D>("Body/TopSprite");
		_bottomSprite = GetNode<Sprite2D>("Body/BottomSprite");

		_stats = GetNode<PlayerStats>("Stats");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Autoload.Globals.I.InMenu)
		{
			return;
		}
		MovementBehavior();
		DashBehavior((float)delta);
		FlipToDirection();
		MoveAndSlide();
	}
	/*
	public override void _Process(float _delta)
	{
		FlipToDirection();
	}
	*/

	private void MovementBehavior()
	{
		var velocity = Velocity;

		if (_onDash)
		{
			velocity = _dashDirection * _stats.DashSpeed;
		}
		else
		{
			var speed = _stats.MoveSpeed;

			var direction = Input.GetVector("move_left", "move_right", "move_up", "move_down").Normalized();

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

	public void FlipToDirection()
	{
		var velocity = Velocity;
		if (velocity != Vector2.Zero)
		{
			if (velocity.Y < 0f)
			{
				_topSprite.Visible = true;
				_bottomSprite.Visible = false;
			}
			else if (velocity.Y > 0f)
			{
				_topSprite.Visible = false;
				_bottomSprite.Visible = true;
			}
	
			if (velocity.X < 0f)
			{
				_topSprite.FlipH = false;
				_bottomSprite.FlipH = false;
			}
			else if (velocity.X > 0f)
			{
				_topSprite.FlipH = true;
				_bottomSprite.FlipH = true;
			}
		}
	}

	private void DashBehavior(float delta)
	{
		if (_onDash)
		{
			DashDurationCountdown -= delta;
			if (!(DashDurationCountdown <= 0)) return;
			DashDurationCountdown = 0;
			_onDash = false;
			if (UsedDashCharges == _stats.DashCharges)
			{
				DashCooldownCount = _stats.DashCooldown;
			}
			return;
		}

		if (DashDurationCountdown == 0 && DashCooldownCount == 0 && !_onDash)
		{
			if (Input.IsActionJustPressed("dash"))
			{
				_dashDirection = Input.GetVector("move_left", "move_right", "move_up", "move_down").Normalized();
				DashDurationCountdown = _stats.DashDuration;
				_onDash = true;
				UsedDashCharges += 1;
			}
			else
			{
				if (UsedDashCharges <= 0) return;
				NotDashingTime += delta;
				if (!(NotDashingTime > (_stats.DashCooldown / _stats.DashCharges) * 2)) return;
				UsedDashCharges -= 1;
				NotDashingTime = 0;
			}
		}
		else
		{
			if (DashCooldownCount > 0)
			{
				DashCooldownCount -= delta;
			}

			if (!(DashCooldownCount < 0) && !(DashDurationCountdown < 0)) return;
			DashCooldownCount = 0;
			DashDurationCountdown = 0;
			UsedDashCharges = 0;
		}
	}
}