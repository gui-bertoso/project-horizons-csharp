using Godot;

public partial class Projectile : Node2D
{
	[Export]
	public float Speed = 600f;

	[Export]
	public float LifeTime = 3f;

	private float _lifeTimer;
	
	[Export]
	public int Damage = 1;
	[Export]
	public Godot.Collections.Array Effects = new();

	public override void _Ready()
	{
		_lifeTimer = LifeTime;
	}

	public override void _PhysicsProcess(double delta)
	{
		float d = (float)delta;

		GlobalPosition += new Vector2(1, 0).Rotated(Rotation) * Speed * d;

		_lifeTimer -= d;
		if (_lifeTimer <= 0f)
		{
			QueueFree();
		}
	}
}