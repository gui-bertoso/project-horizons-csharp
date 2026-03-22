using Godot;
using projecthorizonscs.Autoload;
using projecthorizonscs.Enemys;

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

	private int _maxHealth;

	public int MoveSpeed;
	public float DashDuration;
	public float DashCooldown;
	public int DashSpeed;
	public int DashCharges;

	private Player _player;

	private PackedScene _floatTextScene;

	public override void _Ready()
	{
		_floatTextScene = GD.Load<PackedScene>("uid://g635rxmee8pj");
		_player = GetParent<Player>();
		Autoload.Globals.I.DevModeUpdated += UpdateStats;
		UpdateMaxStats();
		UpdateStats();
	}

	private void OnHitboxAreaEntered(Area2D area)
	{
		var node = area.GetParent();
		if (node is Projectile)
		{
			var damage = ((Projectile)node).Damage;
			UpdateHealth("Decrease", damage);
		}
		else
		{
			var damage = ((EnemyTemplate)node).Damage;
			UpdateHealth("Decrease", damage);
		}
	}

	private void UpdateHealth(string type, int value)
	{
		if (type == "Decrease")
		{
			_health -= value;
			SpawnFloatText("Health", "Decrease", value);
			if (_health < 0)
			{
				GetTree().ReloadCurrentScene();
			}
		}
		else
		{
			_health += value;
			if (_health > _maxHealth)
			{
				_health = _maxHealth;
			}
		}
		Globals.I.StatsContainer.UpdateHealth(_health);
	}
	
	public void SpawnFloatText(string stat, string type, int value)
	{
		var newFloatText = _floatTextScene.Instantiate<FloatText>();
		GetTree().CurrentScene.AddChild(newFloatText);
		newFloatText.GlobalPosition = _player.GlobalPosition;
		newFloatText.SetData($"{stat}{type}", value);
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

	private void UpdateMaxStats()
	{
		_maxHealth = _baseHealth + _extraHealth;
		Globals.I.StatsContainer.UpdateMaxHealth(_maxHealth);
	}
}