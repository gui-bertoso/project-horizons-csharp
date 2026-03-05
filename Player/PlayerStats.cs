using Godot;
using System;

public partial class PlayerStats : Node
{
	public int BaseHealth = 10;
	public int BaseMoveSpeed = 180;

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
		Globals.I.DevModeUpdated += UpdateStats;
		UpdateStats();
	}

	public void UpdateStats()
	{
		if (Globals.I.DevModeEnabled == true)
		{
			Health = 3000;
			MoveSpeed = 350;
			DashDuration = 0.05f;
			DashCooldown = .5f;
			DashSpeed = 4000;
			DashCharges = 5;
			return;
		}
		Health = BaseHealth + ExtraHealth;
		MoveSpeed = BaseMoveSpeed + ExtraMoveSpeed;
		DashDuration = BaseDashDuration + ExtraDashDuration;
		DashCooldown = BaseDashCooldown + ExtraDashCooldown;
		DashSpeed = BaseDashSpeed + ExtraDashSpeed;
		DashCharges = BaseDashCharges + ExtraDashCharges;
	}
}
