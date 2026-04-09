using System;
using Godot;

namespace projecthorizonscs.Enemies.BigLeave;

public partial class BigLeave : EnemyTemplate
{
	enum LeafsState
	{
		Single,
		Full,
		OnlyLeft,
		OnlyRight
	}

	private PackedScene _leafScene;

	private Marker2D _leftSpawnMarker;
	private Marker2D _rightSpawnMarker;

	private LeafsState CurrentLeafState = LeafsState.Full;

    public override void _Ready()
    {
        base._Ready();
		_leafScene = GD.Load<PackedScene>("uid://834fniewtsf1");

		_leftSpawnMarker = GetNode<Marker2D>("LeftMarker");
		_rightSpawnMarker = GetNode<Marker2D>("RightMarker");
    }


	public override void ApplyStateAnimation()
	{
		if (AnimPlayer.CurrentAnimation == "DropLeaf") return;
		switch (CurrentState)
		{
			case EnemyState.Idle:
				AnimPlayer.Play("Idle");
				break;
			case EnemyState.Chase:
			case EnemyState.Wander:
				AnimPlayer.Play("Move");
				break;
			case EnemyState.Attack:
				if (CurrentLeafState != LeafsState.Single)
				{
					var rng = new RandomNumberGenerator();
					if (rng.RandiRange(0, 1370) == 71)
					{
						AnimPlayer.Play("DropLeaf");
					}
					else
					{
						AnimPlayer.Play("Slash");   
					}
				}
				else
				{
					AnimPlayer.Play("Slash");   
				}
				break;
			case EnemyState.Death:
				AnimPlayer.Play("Death");
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	public void DropLeaf()
	{
		Vector2 spawnPosition = new();

		switch (CurrentLeafState)
		{
			case LeafsState.Full:
				var rng = new RandomNumberGenerator();
				if (rng.RandiRange(0, 1) == 1)
				{
					CurrentLeafState = LeafsState.OnlyRight;
					spawnPosition = _leftSpawnMarker.GlobalPosition;
					SpawnLeaf(spawnPosition);
					_bodySprite.Texture = GD.Load<CompressedTexture2D>("uid://dewpkxruakcil");
				}
				else
				{
					CurrentLeafState = LeafsState.OnlyLeft;
					spawnPosition = _rightSpawnMarker.GlobalPosition;
					SpawnLeaf(spawnPosition);
					_bodySprite.Texture = GD.Load<CompressedTexture2D>("uid://bvdjk1lx4onp5");
				}
				break;
			case LeafsState.OnlyLeft:
				CurrentLeafState = LeafsState.Single;
				spawnPosition = _leftSpawnMarker.GlobalPosition;
				SpawnLeaf(spawnPosition);
				_bodySprite.Texture = GD.Load<CompressedTexture2D>("uid://bnponrbwx0dve");
				break;
			case LeafsState.OnlyRight:
				CurrentLeafState = LeafsState.Single;
				spawnPosition = _rightSpawnMarker.GlobalPosition;
				SpawnLeaf(spawnPosition);
				_bodySprite.Texture = GD.Load<CompressedTexture2D>("uid://bnponrbwx0dve");
				break;
		}
	}

	public void SpawnLeaf(Vector2 spawnPosition)
	{
		var newEnemy = _leafScene.Instantiate<EnemyTemplate>();
		GetTree().CurrentScene.AddChild(newEnemy);
		newEnemy.GlobalPosition = spawnPosition;
	}
}