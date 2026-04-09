using System;
using Godot;

namespace projecthorizonscs.Enemies.RageFlower;

public partial class RageFlower : projecthorizonscs.Enemies.EnemyTemplate
{
    private PackedScene _seedProjectile;
    private PackedScene _greenSeedProjectile;

    private Marker2D _projectileSpawnMarker;

    private float _shotCooldown;

    public override void _Ready()
    {
        base._Ready();

        _projectileSpawnMarker = GetNode<Marker2D>("ProjectileSpawn");
        _seedProjectile = GD.Load<PackedScene>("uid://c41q8gni3ijp5");
        _greenSeedProjectile = GD.Load<PackedScene>("uid://c82canyapopma");
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
                if (_shotCooldown == 0f)
                {
				    AnimPlayer.Play("Shot");   
                }
                else
                {
				    AnimPlayer.Play("Idle");
                }
				break;
			case EnemyState.Death:
				AnimPlayer.Play("Death");
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_shotCooldown > 0f)
        {
            _shotCooldown -= (float)delta;
            if (_shotCooldown < 0f)
            {
                _shotCooldown = 0f;
            }
        }
    }


	public override void UpdateState()
	{
		switch (CurrentState)
		{
			case EnemyState.Idle:
				if (DistanceToPlayer < DetectionDistance)
				{
					CurrentState = EnemyState.Attack;

					PlayerReference = Autoload.Globals.I.LocalPlayer;
				}
				else
				{
					PlayerReference = null;
				}
				break;
			
			case EnemyState.Attack:
				if (DistanceToPlayer > DetectionDistance)
				{
					CurrentState = EnemyState.Idle;
				}
				break;
			
			case EnemyState.Chase:
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

        velocity.X = Mathf.MoveToward(velocity.X, 0, MoveSpeed);
        velocity.Y = Mathf.MoveToward(velocity.Y, 0, MoveSpeed);

		Velocity = velocity;
	}

    public void Shot()
    {
        var rng = new RandomNumberGenerator();
        Projectile projectile;
        if (rng.RandiRange(0, 12) == 1)
        {
            projectile = _greenSeedProjectile.Instantiate<Projectile>();
        }
        else
        {
            projectile = _seedProjectile.Instantiate<Projectile>();
        }

        GetTree().CurrentScene.AddChild(projectile);
        projectile.GlobalPosition = _projectileSpawnMarker.GlobalPosition;
        projectile.Rotation = _projectileSpawnMarker.Rotation;

        _shotCooldown = rng.RandfRange(.3f, 2f);
    }
}