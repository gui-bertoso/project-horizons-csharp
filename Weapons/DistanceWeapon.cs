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
		_projectileSpawnMarker.LookAt(GetLocalMousePosition());
	}

	public override void Action()
	{
		GD.Print("Actionnnnn 33333");
		SpawnProjectile(GetLocalMousePosition());
	}

	public void SpawnProjectile(Vector2 targetPosition)
	{
		if (_projectileScene == null)
			return;

		Projectile projectile = _projectileScene.Instantiate<Projectile>();

		GetTree().CurrentScene.AddChild(projectile);

		projectile.GlobalPosition = _projectileSpawnMarker.GlobalPosition;
		projectile.GlobalScale = Vector2.One * _projectileScale;

		Vector2 dir = (targetPosition - projectile.GlobalPosition).Normalized();

		projectile.SetDirection(dir);
	}
}