using Godot;

namespace projecthorizonscs.Player;

public partial class PlayerStats : Node
{
	private int _baseHealth = 10;
	private int _baseMoveSpeed = 180;

	private int _extraHealth;
	private int _extraMoveSpeed;

	private float _baseDashCooldown = 2.5f;
	private float _extraDashCooldown;

	private float _baseDashDuration = .05f;
	private float _extraDashDuration;

	private int _baseDashSpeed = 1000;
	private int _extraDashSpeed;
	private int _baseDashCharges;
	private int _extraDashCharges;

	private int _health;
	public int MoveSpeed;
	public float DashDuration;
	public float DashCooldown;
	public int DashSpeed;
	public int DashCharges;

	public override void _Ready()
	{
		Autoload.Globals.I.DevModeUpdated += UpdateStats;
		UpdateStats();
	}

	private void UpdateStats()
	{
		if (Autoload.Globals.I.DevModeEnabled)
		{
			_health = 3000;
			MoveSpeed = 350;
			DashDuration = 0.05f;
			DashCooldown = .5f;
			DashSpeed = 4000;
			DashCharges = 5;
			return;
		}
		_health = _baseHealth + _extraHealth;
		MoveSpeed = _baseMoveSpeed + _extraMoveSpeed;
		DashDuration = _baseDashDuration + _extraDashDuration;
		DashCooldown = _baseDashCooldown + _extraDashCooldown;
		DashSpeed = _baseDashSpeed + _extraDashSpeed;
		DashCharges = _baseDashCharges + _extraDashCharges;
	}
}