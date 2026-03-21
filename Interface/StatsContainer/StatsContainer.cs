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

	public override void _Ready()
	{
		Globals.I.StatsContainer = this;
		_healthBar = GetNode<TextureProgressBar>("%HealthBar");
		_xpBar = GetNode<TextureProgressBar>("%XpBar");
		_killsLabel = GetNode<Label>("%KillsLabel");
		_levelLabel = GetNode<Label>("%LevelLabel");
	}

	public override void _Process(double delta)
	{
	}
}
