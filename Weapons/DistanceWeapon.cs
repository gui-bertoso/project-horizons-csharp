using Godot;
using Godot.Collections;
using projecthorizonscs;
using System;

namespace projecthorizonscs;

public partial class DistanceWeapon : Weapon
{
	[Export]
	public PackedScene _projectileScene;

	[Export]
	public float _projectileScale = 1f;

	public Marker2D _projectileSpawnMarker;

	public override void _Ready()
	{
		_projectileSpawnMarker = GetNode<Marker2D>("ProjectileSpawn");
	}

	public override void _Process(double delta)
	{
		var mousePosition = GetGlobalMousePosition();
		var mouseAngle = (mousePosition - GlobalPosition).Normalized();
		_projectileSpawnMarker.Rotation = mouseAngle.Angle();
	}

	public override void Action()
	{
		GD.Print("Actionnnnn 33333");
		SpawnProjectile();
	}

	public void SpawnProjectile()
	{
		if (_projectileScene == null)
			return;

		Projectile projectile = _projectileScene.Instantiate<Projectile>();

		GetTree().CurrentScene.AddChild(projectile);

		projectile.GlobalPosition = _projectileSpawnMarker.GlobalPosition;
		projectile.GlobalScale = Vector2.One * _projectileScale;

		projectile.Rotation = _projectileSpawnMarker.Rotation;
	}
}