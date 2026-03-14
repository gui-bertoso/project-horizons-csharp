using Godot;

namespace projecthorizonscs.Player;

public partial class Player : CharacterBody2D
{
	private PlayerStats _stats;

	private Node2D _topSprite;
	private Node2D _bottomSprite;
	private Node2D _body;
	private AnimationPlayer _animationPlayer;
	private GpuParticles2D _walkParticles;
	private GpuParticles2D _dashParticles;

	private bool _onDash;
	public float DashDurationCountdown;
	public float DashCooldownCount;
	public int UsedDashCharges;
	private Vector2 _dashDirection;

	public int currentSide = -1;

	public float NotDashingTime;

	private bool _isCollecting = false;
	
	public override void _Ready()
	{
		Autoload.Globals.I.LocalPlayer = this;

		_topSprite = GetNode<Node2D>("Body/Back");
		_bottomSprite = GetNode<Node2D>("Body/Side");
		_animationPlayer = GetNode<AnimationPlayer>("Body/AnimationPlayer");
		_walkParticles = GetNode<GpuParticles2D>("Node2D/WalkParticles");
		_dashParticles = GetNode<GpuParticles2D>("Node2D/DashParticles");
		_body = GetNode<Node2D>("Body");

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
		MoveAndSlide();
	}

	public override void _Process(double delta)
	{
		FlipToDirection();
	}

	public async void CollectItem(PhysicItem node)
	{
		GD.Print("Collect 1");
		_isCollecting = true;
		if (currentSide == 1) _animationPlayer.Play("collect_side");
		else _animationPlayer.Play("collect_back");

		await ToSignal(GetTree().CreateTimer(_animationPlayer.CurrentAnimationLength), SceneTreeTimer.SignalName.Timeout);

		GD.Print("Collect 2");
		_isCollecting = false;
		node.Collect();
	}

	private void MovementBehavior()
	{
		var velocity = Velocity;
		var speed = _stats.MoveSpeed;

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
			var direction = Input.GetVector("move_left", "move_right", "move_up", "move_down").Normalized();

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

	public void FlipToDirection()
	{
		var velocity = Velocity;
		if (velocity != Vector2.Zero)
		{
			if (velocity.Y < 0f)
			{
				_topSprite.Visible = true;
				_bottomSprite.Visible = false;
				currentSide = -1;
			}
			else if (velocity.Y > 0f)
			{
				_topSprite.Visible = false;
				_bottomSprite.Visible = true;
				currentSide = 1;
			}
	
			if (velocity.X < 0f)
			{
				_body.Scale = new Vector2(1f, 1f);
			}
			else if (velocity.X > 0f)
			{
				_body.Scale = new Vector2(-1f, 1f);
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
			_dashParticles.Emitting = false;
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
				_dashParticles.Emitting = true;
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