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

	private int _currentSide = 1;
	private int _attackSide = 1;

	public float NotDashingTime;

	private bool _isCollecting;
	private bool _isAttacking;
	private bool _isSwordAttacking;

	private RangedAttackState _rangedState = RangedAttackState.None;

	public int CurrentSide => _currentSide;

	public override void _Ready()
	{
		Autoload.Globals.I.LocalPlayer = this;

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

	public void ActionCurrentWeapon()
	{
		_hand.ActionCurrentWeapon();
	}
}
