using Godot;
using System;

public partial class EnemyTemplate : CharacterBody2D
{
	public AnimationPlayer _AnimationPlayer;
	public Sprite2D _BodySprite;
	public Label _DebugLabel;	
	public Area2D _AttackArea;	

	public Player PlayerReference;

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
		_AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		_BodySprite = GetNode<Sprite2D>("Sprite");
		_DebugLabel = GetNode<Label>("Label");
		_AttackArea = GetNode<Area2D>("AttackArea");
	}

	public override void _Process(double delta)
	{
		if (Globals.I.LocalPlayer == null)
		{
			return;
		}
		UpdateDistanceToPlayer();
		UpdateState();
		ApplyState();
		ApplyStateAnimation();

		_DebugLabel.Text = $"State: {CurrentState}\nDistanceToPlayer: {DistanceToPlayer}";
	}

	public override void _PhysicsProcess(double delta)
	{
		ApplyStatePhysics();
		MoveAndSlide();
	}

	public virtual void ApplyStatePhysics()
	{
		
	}

	public virtual void UpdateState()
	{
		
	}

	public virtual void ApplyState()
	{

	}

	public virtual void ApplyStateAnimation()
	{
		
	}

	public void UpdateDistanceToPlayer()
	{
		DistanceToPlayer = Globals.I.LocalPlayer.GlobalPosition.DistanceTo(GlobalPosition);
	}

	public void RotateAttackAreaTowardsPlayer()
	{
		if (PlayerReference != null)
		{
			Vector2 DirectionToPlayer = (PlayerReference.GlobalPosition - GlobalPosition).Normalized();
			_AttackArea.Rotation = DirectionToPlayer.Angle();
		}
	}

	public void LookToTarget(Vector2 TargetPosition)
	{
		if (TargetPosition.X < GlobalPosition.X)
		{
			_BodySprite.FlipH = true;
		}
		else
		{
			_BodySprite.FlipH = false;
		}
	}

}
