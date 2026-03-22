using Godot;
using projecthorizonscs.Autoload;

public partial class SeedProjectile : Projectile
{
    private float _uppingTime = 0.4f;
    public Vector2 targetPosition;

    public override void _Ready()
    {
        base._Ready();

        if (Globals.I.LocalPlayer != null)
            targetPosition = Globals.I.LocalPlayer.GlobalPosition;
        else
            targetPosition = GlobalPosition;
    }

    public override void _Process(double delta)
    {
        float d = (float)delta;

        _lifeTimer -= d;
        if (_lifeTimer <= 0f)
        {
            QueueFree();
            return;
        }

        if (_uppingTime > 0f)
        {
            _uppingTime -= d;
            GlobalPosition += Vector2.Right.Rotated(Rotation) * Speed * d;
        }
        else
        {
            Rotation = (targetPosition - GlobalPosition).Angle();
            GlobalPosition += Vector2.Right.Rotated(Rotation) * Speed * d;
        }

		VerifyOnTarget();
    }

	public virtual void VerifyOnTarget()
	{
		if (GlobalPosition.DistanceTo(targetPosition) < 0.5f)
		{
			QueueFree();
		}
	}
}