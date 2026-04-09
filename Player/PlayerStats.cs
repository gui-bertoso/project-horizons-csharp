using Godot;
using projecthorizonscs.Autoload;
using projecthorizonscs.Enemies;

namespace projecthorizonscs.Player;

public partial class PlayerStats : Node
{
	private const int BaseXpToNextLevel = 100;
	private const int XpGrowthPerLevel = 25;

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
	private int _xp;
	private int _level = 1;
	private int _kills;

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
		LoadProgress();
		RefreshHud();
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
				AchievementsManager.I?.RegisterPlayerDeath();
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

		PersistProgress();
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

	public void AddXp(int amount)
	{
		if (amount <= 0)
			return;

		_xp += amount;
		SpawnFloatText("XP", "Increase", amount);

		bool leveledUp = false;

		while (_xp >= GetXpToNextLevel())
		{
			_xp -= GetXpToNextLevel();
			_level++;
			leveledUp = true;
		}

		if (leveledUp)
		{
			UpdateMaxStats();
			_health = _maxHealth;
			Globals.I.StatsContainer.UpdateHealth(_health);
		}

		PersistProgress();
		RefreshHud();
	}

	public void AddKill()
	{
		_kills++;
		PersistProgress();
		RefreshHud();
	}

	private int GetXpToNextLevel()
	{
		return BaseXpToNextLevel + ((_level - 1) * XpGrowthPerLevel);
	}

	private void LoadProgress()
	{
		if (DataManager.I == null)
			return;

		if (DataManager.I.CurrentWorldData.TryGetValue("PlayerXp", out Variant xpValue))
			_xp = Mathf.Max(0, xpValue.AsInt32());

		if (DataManager.I.CurrentWorldData.TryGetValue("PlayerLevel", out Variant levelValue))
			_level = Mathf.Max(1, levelValue.AsInt32());

		if (DataManager.I.CurrentWorldData.TryGetValue("PlayerKills", out Variant killsValue))
			_kills = Mathf.Max(0, killsValue.AsInt32());

		if (DataManager.I.CurrentWorldData.TryGetValue("PlayerHealth", out Variant healthValue))
		{
			_health = Mathf.Clamp(healthValue.AsInt32(), 0, _maxHealth);
			Globals.I.StatsContainer.UpdateHealth(_health);
		}
	}

	private void PersistProgress()
	{
		if (DataManager.I == null)
			return;

		DataManager.I.CurrentWorldData["PlayerXp"] = _xp;
		DataManager.I.CurrentWorldData["PlayerLevel"] = _level;
		DataManager.I.CurrentWorldData["PlayerKills"] = _kills;
		DataManager.I.CurrentWorldData["PlayerHealth"] = _health;
	}

	private void RefreshHud()
	{
		if (Globals.I?.StatsContainer == null)
			return;

		Globals.I.StatsContainer.UpdateMaxHealth(_maxHealth);
		Globals.I.StatsContainer.UpdateHealth(_health);
		Globals.I.StatsContainer.UpdateMaxXp(GetXpToNextLevel());
		Globals.I.StatsContainer.UpdateXp(_xp);
		Globals.I.StatsContainer.UpdateLevel(_level);
		Globals.I.StatsContainer.UpdateKills(_kills);
	}
}
