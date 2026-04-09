using Godot;
using Godot.Collections;
using projecthorizonscs;
using projecthorizonscs.Autoload;
using projecthorizonscs.Combat;
using System;

namespace projecthorizonscs;

public partial class DistanceWeapon : Weapon
{
	[Export]
	public PackedScene _projectileScene;

	[Export]
	public float _projectileScale = 1f;

	public Marker2D _projectileSpawnMarker;
	private WeaponClassProfile _classProfile = WeaponClassProfile.Default;

	public override void _Ready()
	{
		base._Ready();
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
		if (!CanUse())
			return;

		SpawnProjectile();
		TriggerCooldown(1f / Mathf.Max(0.01f, _classProfile.AttackAnimationSpeedMultiplier));
	}

	public void SpawnProjectile()
	{
		if (_projectileScene == null)
			return;

		int projectileCount = Mathf.Max(1, _classProfile.ProjectileCount);
		float spreadStep = projectileCount > 1 ? _classProfile.ProjectileSpreadDegrees / (projectileCount - 1) : 0f;
		float spreadStart = -_classProfile.ProjectileSpreadDegrees * 0.5f;

		for (int i = 0; i < projectileCount; i++)
		{
			Projectile projectile = _projectileScene.Instantiate<Projectile>();
			GetTree().CurrentScene.AddChild(projectile);

			float spreadDegrees = spreadStart + (spreadStep * i);
			projectile.GlobalPosition = _projectileSpawnMarker.GlobalPosition;
			projectile.GlobalScale = Vector2.One * (_projectileScale * _classProfile.ProjectileScaleMultiplier);
			projectile.Rotation = _projectileSpawnMarker.Rotation + Mathf.DegToRad(spreadDegrees);
			projectile.Speed *= _classProfile.ProjectileSpeedMultiplier;
			projectile.Damage += _classProfile.ProjectileDamageBonus;
		}
	}

	public void ApplyClassProfile(WeaponClassProfile profile)
	{
		_classProfile = profile ?? WeaponClassProfile.Default;
	}
}
