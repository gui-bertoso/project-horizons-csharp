using Godot;
using projecthorizonscs.Enemys.RageFlower;
using System;

public partial class GreenSeed : SeedProjectile
{
	public override void VerifyOnTarget()
	{
		if (GlobalPosition.DistanceTo(targetPosition) < 0.5f)
		{
			SpawnRageFlower();
			QueueFree();
		}
	}

	public void SpawnRageFlower()
	{
		var node = GD.Load<PackedScene>("uid://iqddsepl7qw2").Instantiate<RageFlower>();
		GetTree().CurrentScene.AddChild(node);
		node.GlobalPosition = GlobalPosition;
	}
}
