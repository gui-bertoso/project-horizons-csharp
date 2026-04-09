using Godot;
using projecthorizonscs.Autoload;
using System;

public partial class StatsContainer : Control
{
	private TextureProgressBar _healthBar;
	private TextureProgressBar _xpBar;
	private Label _killsLabel;
	private Label _levelLabel;

	public void UpdateMaxHealth(int value)
	{
		_healthBar.MaxValue = value;
	}
	public void UpdateHealth(int value)
	{
		_healthBar.Value = value;
	}

	public void UpdateMaxXp(int value)
	{
		_xpBar.MaxValue = Math.Max(1, value);
	}

	public void UpdateXp(int value)
	{
		_xpBar.Value = Mathf.Clamp(value, 0, (int)_xpBar.MaxValue);
	}

	public void UpdateLevel(int value)
	{
		_levelLabel.Text = value.ToString();
	}

	public void UpdateKills(int value)
	{
		_killsLabel.Text = value.ToString();
	}

	public override void _Ready()
	{
		Globals.I.StatsContainer = this;
		_healthBar = GetNode<TextureProgressBar>("%HealthBar");
		_xpBar = GetNode<TextureProgressBar>("%XpBar");
		_killsLabel = GetNode<Label>("%KillsLabel");
		_levelLabel = GetNode<Label>("%LevelLabel");
		UpdateXp(0);
		UpdateMaxXp(100);
		UpdateLevel(1);
		UpdateKills(0);
	}

	public override void _Process(double delta)
	{
	}
}
