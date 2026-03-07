using Godot;
using System;

public partial class Slime : EnemyTemplate
{

	public override void UpdateState()
	{
		switch (CurrentState)
		{
			case EnemyState.Idle:
				if (DistanceToPlayer < DetectionDistance)
				{
					if (DistanceToPlayer < AttackDistance)
					{
						CurrentState = EnemyState.Attack;
						PlayerReference = Globals.I.LocalPlayer;
					}
					else
					{
						CurrentState = EnemyState.Chase;
						PlayerReference = Globals.I.LocalPlayer;
					}
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
				break;
			
			case EnemyState.Death:
				break;
		}
	}

	public override void ApplyStatePhysics()
	{
		Vector2 velocity = Velocity;
		Vector2 Direction;
		
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
				velocity = Velocity;
				Direction = (PlayerReference.GlobalPosition - GlobalPosition).Normalized();
				if (Direction != Vector2.Zero)
				{
					velocity = Direction * MoveSpeed;
				}
				else
				{
					velocity.X = Mathf.MoveToward(velocity.X, 0, MoveSpeed);
					velocity.Y = Mathf.MoveToward(velocity.Y, 0, MoveSpeed);
				}
				Velocity = velocity;
				break;
			case EnemyState.Attack:
				velocity = Velocity;
				Direction = (PlayerReference.GlobalPosition - GlobalPosition).Normalized();
				if (Direction != Vector2.Zero)
				{
					velocity = Direction * MoveSpeed;
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
		}
	}

	public override void ApplyState()
	{
		switch (CurrentState)
		{
			case EnemyState.Idle:
				break;
			case EnemyState.Chase:
				LookToTarget(PlayerReference.GlobalPosition);
				break;
			case EnemyState.Wander:
				LookToTarget(PlayerReference.GlobalPosition);
				break;
			case EnemyState.Attack:
				LookToTarget(PlayerReference.GlobalPosition);
				RotateAttackAreaTowardsPlayer();
				break;
			case EnemyState.Death:
				break;
		}
	}

	public override void ApplyStateAnimation()
	{
		switch (CurrentState)
		{
			case EnemyState.Idle:
				_AnimationPlayer.Play("Idle");
				break;
			case EnemyState.Chase:
				_AnimationPlayer.Play("Move");
				break;
			case EnemyState.Wander:
				_AnimationPlayer.Play("Move");
				break;
			case EnemyState.Attack:
				_AnimationPlayer.Play("Bite");
				break;
			case EnemyState.Death:
				_AnimationPlayer.Play("Death");
				break;
		}
	}
}
