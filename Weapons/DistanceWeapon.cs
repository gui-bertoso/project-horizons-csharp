using Godot;
using projecthorizonscs;
using System;

public partial class DistanceWeapon : Weapon
{
	[Export]
	public Projectile _projectileScene;
	[Export]
	public float _projectileScale = 1f;
	public Marker2D _projectileSpawnMarker;

	public override void _Ready()
	{
		_projectileSpawnMarker = GetNode<Marker2D>("ProjectileSpawn");
	}

	public override void _Process(double delta)
	{
	}
}
