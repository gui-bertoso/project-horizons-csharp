using Godot;
using System;

public partial class InitialPortal : Node2D
{
	public void SpawnPlayer()
	{
		var player = GD.Load<PackedScene>("res://Player/Player.tscn").Instantiate<CharacterBody2D>();
		var rng = new RandomNumberGenerator();
		var spawnOffset = new Vector2(rng.RandiRange(-10, 10), rng.RandiRange(-10, 10));
		player.GlobalPosition = GlobalPosition + spawnOffset;

		var scenario = GetTree().CurrentScene?.GetNodeOrNull<Node2D>("Scenario");
		if (scenario != null)
		{
			scenario.AddChild(player);
			return;
		}

		GetParent().GetParent().AddChild(player);
	}
}
