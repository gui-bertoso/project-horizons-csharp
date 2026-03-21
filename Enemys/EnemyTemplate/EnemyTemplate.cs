using System;
using Godot;

namespace projecthorizonscs.Enemys;

public partial class EnemyTemplate : CharacterBody2D
{
	protected AnimationPlayer AnimPlayer;
	private Sprite2D _bodySprite;
	private Label _debugLabel;
	private Area2D _attackArea;
	private Area2D _hitboxArea;

	protected Player.Player PlayerReference;

	[Export]
	public string EnemyName = "EnemyName";
	[Export]
	public int Health = 10;
	[Export]
	public int MoveSpeed = 100;
	[Export]
	public int DetectionDistance = 200;

	public enum EnemyState
	{
		Idle,
		Chase,
		Wander,
		Attack,
		Death
	}

	public float DistanceToPlayer;

	public EnemyState CurrentState = EnemyState.Idle;

	public float AttackDistance = 70f;

	public override void _Ready()
	{
		AnimPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_bodySprite = GetNode<Sprite2D>("Sprite");
		_debugLabel = GetNode<Label>("Label");
		_attackArea = GetNode<Area2D>("AttackArea");
		_hitboxArea = GetNode<Area2D>("HitBox");

		AnimPlayer.AnimationFinished += OnAnimationFinished;
	}

	public void OnAnimationFinished(StringName animName)
	{
		if (animName == "Death")
		{
			QueueFree();
		}
	}

	public void OnHitboxAreaEntered(Area2D area)
	{
		var node = area.GetParent<PhysicWeapon>();
		ApplyDamage(node.Damage);
		ApplyIFrames(0.1f);
	}

	public async void ApplyIFrames(float value)
	{
		_hitboxArea.Monitoring = false;
		await ToSignal(GetTree().CreateTimer(value), SceneTreeTimer.SignalName.Timeout);
		_hitboxArea.Monitoring = true;
	}

	public void ApplyDamage(int value)
	{
		Health -= value;
		if (Health < 0)
		{
			CurrentState = EnemyState.Death;
		}
	}

	public override void _Process(double delta)
	{
		if (Autoload.Globals.I.LocalPlayer == null)
		{
			return;
		}
		UpdateDistanceToPlayer();
		UpdateState();
		ApplyState();
		ApplyStateAnimation();

		_debugLabel.Text = $"State: {CurrentState}\nDistanceToPlayer: {DistanceToPlayer}";
	}

	public override void _PhysicsProcess(double delta)
	{
		ApplyStatePhysics();
		MoveAndSlide();
	}

	public void UpdateState()
	{
		switch (CurrentState)
		{
			case EnemyState.Idle:
				if (DistanceToPlayer < DetectionDistance)
				{
					CurrentState = DistanceToPlayer < AttackDistance ? EnemyState.Attack : EnemyState.Chase;

					PlayerReference = Autoload.Globals.I.LocalPlayer;
				}
				else
				{
					PlayerReference = null;
				}
				break;
			
			case EnemyState.Chase:
				if (DistanceToPlayer > DetectionDistance)
				{
					CurrentState = EnemyState.Idle;
				} else if (DistanceToPlayer < AttackDistance)
				{
					CurrentState = EnemyState.Attack;
				}
				break;
			
			case EnemyState.Attack:
				if (DistanceToPlayer > AttackDistance)
				{
					CurrentState = EnemyState.Chase;
				}
				break;
			
			case EnemyState.Wander:
			
			case EnemyState.Death:
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	public void ApplyStatePhysics()
	{
		var velocity = Velocity;

		switch (CurrentState)
		{
			case EnemyState.Idle:
				if (Velocity != Vector2.Zero)
				{
					velocity.X = Mathf.MoveToward(velocity.X, 0, MoveSpeed);
					velocity.Y = Mathf.MoveToward(velocity.Y, 0, MoveSpeed);
				}
				break;
			case EnemyState.Chase:
			case EnemyState.Attack:
				velocity = Velocity;
				var direction = (PlayerReference.GlobalPosition - GlobalPosition).Normalized();
				if (direction != Vector2.Zero)
				{
					velocity = direction * MoveSpeed;
				}
				else
				{
					velocity.X = Mathf.MoveToward(velocity.X, 0, MoveSpeed);
					velocity.Y = Mathf.MoveToward(velocity.Y, 0, MoveSpeed);
				}
				Velocity = velocity;
				break;
			case EnemyState.Death:
				if (Velocity != Vector2.Zero)
				{
					velocity.X = Mathf.MoveToward(velocity.X, 0, MoveSpeed);
					velocity.Y = Mathf.MoveToward(velocity.Y, 0, MoveSpeed);
				}
				break;
			case EnemyState.Wander:
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}

		Velocity = velocity;
	}

	public void ApplyState()
	{
		switch (CurrentState)
		{
			case EnemyState.Idle:
				break;
			case EnemyState.Chase:
			case EnemyState.Wander:
				LookToTarget(PlayerReference.GlobalPosition);
				break;
			case EnemyState.Attack:
				LookToTarget(PlayerReference.GlobalPosition);
				RotateAttackAreaTowardsPlayer();
				break;
			case EnemyState.Death:
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	public void ApplyStateAnimation()
	{
		switch (CurrentState)
		{
			case EnemyState.Idle:
				AnimPlayer.Play("Idle");
				break;
			case EnemyState.Chase:
			case EnemyState.Wander:
				AnimPlayer.Play("Move");
				break;
			case EnemyState.Attack:
				AnimPlayer.Play("Bite");
				break;
			case EnemyState.Death:
				AnimPlayer.Play("Death");
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	public void UpdateDistanceToPlayer()
	{
		DistanceToPlayer = Autoload.Globals.I.LocalPlayer.GlobalPosition.DistanceTo(GlobalPosition);
	}

	public void RotateAttackAreaTowardsPlayer()
	{
		if (PlayerReference == null) return;
		var directionToPlayer = (PlayerReference.GlobalPosition - GlobalPosition).Normalized();
		_attackArea.Rotation = directionToPlayer.Angle();
	}

	public void LookToTarget(Vector2 targetPosition)
	{
		_bodySprite.FlipH = targetPosition.X < GlobalPosition.X;
	}

}