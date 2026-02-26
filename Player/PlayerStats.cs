using Godot;
using System;

public partial class PlayerStats : Node
{
	public int BaseHealth = 10;
	public int BaseMoveSpeed = 10;

	public int ExtraHealth = 0;
	public int ExtraMoveSpeed = 0;

	public float BaseDashCooldown = 2.5f;
	public float ExtraDashCooldown = 0;

	public float BaseDashDuration = .05f;
	public float ExtraDashDuration = 0;

	public int BaseDashSpeed = 1000;
	public int ExtraDashSpeed = 0;
	public int BaseDashCharges = 0;
	public int ExtraDashCharges = 0;

	public int Health;
	public int MoveSpeed;
	public float DashDuration;
	public float DashCooldown;
	public int DashSpeed;
	public int DashCharges;

	public override void _Ready()
	{
		if (Globals.I.DevModeEnabled)
		{
			SetDevStats();
		}
		UpdateStats();
	}

	public void UpdateStats()
	{
		Health = BaseHealth + ExtraHealth;
		MoveSpeed = BaseMoveSpeed + ExtraMoveSpeed;
		DashDuration = BaseDashDuration + ExtraDashDuration;
		DashCooldown = BaseDashCooldown + ExtraDashCooldown;
		DashSpeed = BaseDashSpeed + ExtraDashSpeed;
		DashCharges = BaseDashCharges + ExtraDashCharges;
	}

	public void SetDevStats()
	{
		ExtraHealth = 1200;
		ExtraMoveSpeed = 350;
		ExtraDashCooldown = -1.2f;
		ExtraDashDuration = 0.05f;
		ExtraDashCharges = 5;
	}
}
