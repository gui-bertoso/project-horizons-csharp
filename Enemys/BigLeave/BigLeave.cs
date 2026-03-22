using System;
using Godot;

namespace projecthorizonscs.Enemys.BigLeave;

public partial class BigLeave : EnemyTemplate
{
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
                AnimPlayer.Play("Slash");   
				break;
			case EnemyState.Death:
				AnimPlayer.Play("Death");
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}
}