using Godot;

public partial class Projectile : Node2D
{
	[Export]
	public float Speed = 600f;

	[Export]
	public float LifeTime = 3f;

	private float _lifeTimer;
	private Vector2 _direction = Vector2.Zero;

	public override void _Ready()
	{
		_lifeTimer = LifeTime;
	}

	public void SetDirection(Vector2 dir)
	{
		_direction = dir.Normalized();
		Rotation = _direction.Angle();
	}

	public override void _PhysicsProcess(double delta)
	{
		float d = (float)delta;

		GlobalPosition += _direction * Speed * d;

		_lifeTimer -= d;
		if (_lifeTimer <= 0f)
		{
			QueueFree();
		}
	}
}