using Godot;

namespace projecthorizonscs.Enemys.EnemyTemplate;

public partial class EnemyTemplate : CharacterBody2D
{
	protected AnimationPlayer AnimPlayer;
	private Sprite2D _bodySprite;
	private Label _debugLabel;
	private Area2D _attackArea;

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