using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace projecthorizonscs.Enemies;

public partial class BushBoss : EnemyTemplate
{
	private static readonly StringName BiteAnimation = "Bite";
	private static readonly StringName BreathAnimation = "Breath";
	private static readonly StringName DefenseAnimation = "Defense";
	private static readonly StringName RootAttacksAnimation = "RootAttacks";

	private static readonly StringName[] RangedAttackAnimations = [BreathAnimation, RootAttacksAnimation];

	[Export]
	public int MinimumDetectionDistance = 600;

	[Export]
	public float MinimumAttackDistance = 240f;

	[Export]
	public float BiteDistanceThreshold = 80f;

	[Export]
	public float BreathOrRootChanceAboveFortyPercent = 0.75f;

	[Export]
	public float AttackRecoveryTime = 0.35f;

	private bool _attackAnimationPlaying;
	private bool _attackRecoveryPending;
	private StringName _currentAttackAnimation = default;
	private int _maxHealth;
	private readonly RandomNumberGenerator _rng = new();

	public override void _Ready()
	{
		base._Ready();
		DetectionDistance = Mathf.Max(DetectionDistance, MinimumDetectionDistance);
		AttackDistance = Mathf.Max(AttackDistance, MinimumAttackDistance);
		_maxHealth = Health;
		AnimPlayer.AnimationFinished += OnBushBossAnimationFinished;
	}

	public override void UpdateState()
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

	public override void ApplyStatePhysics()
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
				if (CurrentState == EnemyState.Attack && ShouldStopMovingDuringCurrentAttack())
				{
					velocity.X = Mathf.MoveToward(velocity.X, 0, MoveSpeed);
					velocity.Y = Mathf.MoveToward(velocity.Y, 0, MoveSpeed);
					Velocity = velocity;
					break;
				}

				velocity = Velocity;
				var direction = (PlayerReference.GlobalPosition - GlobalPosition).Normalized();
				var moveSpeed = GetMoveSpeedForCurrentAttack();
				if (direction != Vector2.Zero)
				{
					velocity = direction * moveSpeed;
				}
				else
				{
					velocity.X = Mathf.MoveToward(velocity.X, 0, moveSpeed);
					velocity.Y = Mathf.MoveToward(velocity.Y, 0, moveSpeed);
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

	public override void ApplyState()
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

	public override void ApplyStateAnimation()
	{
		switch (CurrentState)
		{
			case EnemyState.Idle:
				ResetAttackCycle();
				AnimPlayer.Play("Idle");
				break;
			case EnemyState.Chase:
			case EnemyState.Wander:
				ResetAttackCycle();
				AnimPlayer.Play("Move");
				break;
			case EnemyState.Attack:
				PlayNextAttackAnimationIfNeeded();
				break;
			case EnemyState.Death:
				ResetAttackCycle();
				AnimPlayer.Play("Death");
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private void PlayNextAttackAnimationIfNeeded()
	{
		if (_attackAnimationPlaying || _attackRecoveryPending)
			return;

		var animationName = SelectNextAttackAnimation();
		_attackAnimationPlaying = true;
		_currentAttackAnimation = animationName;
		AnimPlayer.Play(animationName);
	}

	private StringName SelectNextAttackAnimation()
	{
		if (DistanceToPlayer < BiteDistanceThreshold)
			return BiteAnimation;

		if (GetHealthRatio() <= 0.4f)
			return DefenseAnimation;

		if (_rng.Randf() <= BreathOrRootChanceAboveFortyPercent)
			return RangedAttackAnimations[_rng.RandiRange(0, RangedAttackAnimations.Length - 1)];

		return BiteAnimation;
	}

	private void ResetAttackCycle()
	{
		_attackAnimationPlaying = false;
		_attackRecoveryPending = false;
		_currentAttackAnimation = default;
	}

	private bool ShouldStopMovingDuringCurrentAttack()
	{
		return _currentAttackAnimation == BreathAnimation
			|| _currentAttackAnimation == DefenseAnimation
			|| _currentAttackAnimation == RootAttacksAnimation;
	}

	private float GetMoveSpeedForCurrentAttack()
	{
		if (CurrentState == EnemyState.Attack && _currentAttackAnimation == BiteAnimation)
			return MoveSpeed * 0.5f;

		return MoveSpeed;
	}

	private async void OnBushBossAnimationFinished(StringName animName)
	{
		if (!IsBossAttackAnimation(animName))
			return;

		_attackAnimationPlaying = false;
		_currentAttackAnimation = default;

		if (CurrentState != EnemyState.Attack)
			return;

		_attackRecoveryPending = true;
		await ToSignal(GetTree().CreateTimer(AttackRecoveryTime), SceneTreeTimer.SignalName.Timeout);
		_attackRecoveryPending = false;

		if (CurrentState == EnemyState.Attack && !_attackAnimationPlaying)
			PlayNextAttackAnimationIfNeeded();
	}

	private float GetHealthRatio()
	{
		if (_maxHealth <= 0)
			return 0f;

		return (float)Health / _maxHealth;
	}

	private static bool IsBossAttackAnimation(StringName animName)
	{
		return animName == BiteAnimation
			|| animName == BreathAnimation
			|| animName == DefenseAnimation
			|| animName == RootAttacksAnimation;
	}
}
