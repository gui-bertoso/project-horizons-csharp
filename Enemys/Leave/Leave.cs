using System;
using Godot;

namespace projecthorizonscs.Enemys.Leave;

public partial class Leave : projecthorizonscs.Enemys.EnemyTemplate.EnemyTemplate
{

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
}