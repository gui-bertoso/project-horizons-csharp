using Godot;

namespace projecthorizonscs.Player;

public partial class Player : CharacterBody2D
{
	private enum RangedAttackState
	{
		None,
		Starting,
		Looping,
		Ending
	}

	private PlayerStats _stats;

	private Node2D _topSprite;
	private Node2D _bottomSprite;
	private Node2D _body;
	private PlayerHand _hand;
	private AnimationPlayer _animationPlayer;
	private GpuParticles2D _walkParticles;
	private GpuParticles2D _dashParticles;

	private bool _onDash;
	public float DashDurationCountdown;
	public float DashCooldownCount;
	public int UsedDashCharges;
	private Vector2 _dashDirection;

	public int currentSide = 1;
	private int _attackSide = 1;

	public float NotDashingTime;

	private bool _isCollecting = false;
	private bool _isAttacking = false;
	private bool _isSwordAttacking = false;

	private RangedAttackState _rangedState = RangedAttackState.None;

	public override void _Ready()
	{
		Autoload.Globals.I.LocalPlayer = this;
		GD.Print($"player {GlobalPosition}");

		_topSprite = GetNode<Node2D>("Body/Back");
		_bottomSprite = GetNode<Node2D>("Body/Side");
		_animationPlayer = GetNode<AnimationPlayer>("Body/AnimationPlayer");
		_walkParticles = GetNode<GpuParticles2D>("Node2D/WalkParticles");
		_dashParticles = GetNode<GpuParticles2D>("Node2D/DashParticles");
		_body = GetNode<Node2D>("Body");
		_hand = GetNode<PlayerHand>("Body/Node2D/Hand");
		_stats = GetNode<PlayerStats>("Stats");

		_animationPlayer.AnimationFinished += OnAnimationFinished;
	}

	public override void _Process(double delta)
	{
		UpdateFacingFromInput();
		HandleActionInput();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Autoload.Globals.I.InMenu)
			return;

		MovementBehavior();
		DashBehavior((float)delta);
		MoveAndSlide();
	}

	public bool IsAttacking()
	{
		return _isAttacking;
	}

	private void HandleActionInput()
	{
		if (_isCollecting)
			return;

		string weaponClass = _hand.GetWeaponClass();

		if (weaponClass == "ranged")
		{
			HandleRangedAttackInput();
			return;
		}

		if (weaponClass == "sword")
		{
			HandleSwordAttack();
			return;
		}

		_isAttacking = false;
	}

	private void HandleSwordAttack()
	{
		if (_isSwordAttacking)
			return;

		if (!Input.IsActionJustPressed("action"))
			return;

		_isSwordAttacking = true;
		_isAttacking = true;

		if (currentSide == 1)
			_animationPlayer.Play("sword_attack_side");
		else
			_animationPlayer.Play("sword_attack_back");
	}

	private void HandleRangedAttackInput()
	{
		_isAttacking = Input.IsActionPressed("action");

		// recuperação de estado bugado:
		// se por algum motivo terminou animação de down mas o estado não resetou, força reset
		if (_rangedState == RangedAttackState.Ending)
		{
			if (_animationPlayer.CurrentAnimation != "ranged_attack_down_side" &&
				_animationPlayer.CurrentAnimation != "ranged_attack_down_back")
			{
				ResetRangedAttack();
			}
		}

		// se começou e ficou preso fora das animações de up, corrige
		if (_rangedState == RangedAttackState.Starting)
		{
			if (_animationPlayer.CurrentAnimation != "ranged_attack_up_side" &&
				_animationPlayer.CurrentAnimation != "ranged_attack_up_back")
			{
				ResetRangedAttack();
			}
		}

		if (_isAttacking && _rangedState == RangedAttackState.None)
		{
			StartRangedAttack();
			return;
		}

		if (!_isAttacking && _rangedState == RangedAttackState.Looping)
		{
			EndRangedAttack();
			return;
		}
	}

	private void StartRangedAttack()
	{
		_attackSide = currentSide;
		_rangedState = RangedAttackState.Starting;

		if (_attackSide == 1)
			_animationPlayer.Play("ranged_attack_up_side");
		else
			_animationPlayer.Play("ranged_attack_up_back");
	}

	private void EnterRangedLoop()
	{
		_rangedState = RangedAttackState.Looping;

		if (_attackSide == 1)
			_animationPlayer.Play("ranged_attack_loop_side");
		else
			_animationPlayer.Play("ranged_attack_loop_back");
	}

	private void EndRangedAttack()
	{
		if (_rangedState == RangedAttackState.Ending || _rangedState == RangedAttackState.None)
			return;

		_rangedState = RangedAttackState.Ending;

		if (_attackSide == 1)
			_animationPlayer.Play("ranged_attack_down_side");
		else
			_animationPlayer.Play("ranged_attack_down_back");
	}

	private void ResetRangedAttack()
	{
		_rangedState = RangedAttackState.None;
	}

	private void OnAnimationFinished(StringName animName)
	{
		string anim = animName.ToString();

		switch (anim)
		{
			case "ranged_attack_up_side":
			case "ranged_attack_up_back":
				if (_isAttacking)
					EnterRangedLoop();
				else
					EndRangedAttack();
				break;

			case "ranged_attack_down_side":
			case "ranged_attack_down_back":
				ResetRangedAttack();

				// se o jogador já estiver segurando de novo, recomeça instantaneamente
				if (_isAttacking)
					StartRangedAttack();

				break;

			case "collect_side":
			case "collect_back":
				_isCollecting = false;
				break;
			
			case "sword_attack_side":
			case "sword_attack_back":
				_isSwordAttacking = false;
				_isAttacking = false;
				break;
		}
	}

	public async void CollectItem(PhysicItem node)
	{
		if (_isCollecting)
			return;

		_isCollecting = true;
		_isAttacking = false;

		if (_rangedState != RangedAttackState.None)
			ResetRangedAttack();

		if (currentSide == 1)
			_animationPlayer.Play("collect_side");
		else
			_animationPlayer.Play("collect_back");

		await ToSignal(GetTree().CreateTimer(_animationPlayer.CurrentAnimationLength), SceneTreeTimer.SignalName.Timeout);

		node.Collect();
	}

	private void MovementBehavior()
	{
		Vector2 velocity = Velocity;
		float speed = _stats.MoveSpeed;

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
			currentSide = -1;
		}
		else if (direction.Y > 0f)
		{
			_topSprite.Visible = false;
			_bottomSprite.Visible = true;
			currentSide = 1;
		}

		if (direction.X < 0f)
			_body.Scale = new Vector2(1f, 1f);
		else if (direction.X > 0f)
			_body.Scale = new Vector2(-1f, 1f);
	}

	private void DashBehavior(float delta)
	{
		if (_onDash)
		{
			DashDurationCountdown -= delta;

			if (DashDurationCountdown > 0)
				return;

			DashDurationCountdown = 0;
			_onDash = false;
			_dashParticles.Emitting = false;

			if (UsedDashCharges == _stats.DashCharges)
				DashCooldownCount = _stats.DashCooldown;

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

				if (NotDashingTime > (_stats.DashCooldown / _stats.DashCharges) * 2)
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