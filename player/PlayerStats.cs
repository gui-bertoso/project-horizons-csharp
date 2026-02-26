using Godot;
using System;

public partial class PlayerStats : Node
{
	public int BaseHealth = 10;
	public int BaseMoveSpeed = 10;
	public int ExtraHealth = 0;
	public int ExtraMoveSpeed = 0;

	public int Health;
	public int MoveSpeed;

	public override void _Ready()
	{
		if (Globals.I.DevModeEnabled)
		{
			SetDevStats();
		}
		UpdateStats();
	}

	public override void _Process(double delta)
	{
	}

	public void UpdateStats()
	{
		Health = BaseHealth + ExtraHealth;
		MoveSpeed = BaseMoveSpeed + ExtraMoveSpeed;
	}

	public void SetDevStats()
	{
		ExtraHealth = 1200;
		ExtraMoveSpeed = 350;
	}
}
