using Godot;
using System;

public partial class FloatText : Label
{
	enum TextType
	{
		HealthDecrease,
		ManaDecrease,
		ManaIncrease,
		HealthIncrease
	}
	private SceneTreeTimer _timer;

	private float lifetimeCount;
	private float lifetime = 1f;

	public void LifetimeEnded()
	{
		QueueFree();
	}

	public void SetData(string type, int value)
	{
		switch (type)
		{
			case "HealthDecrease":
				Modulate = new Color(1, 0, 0);
				Text = $"-{value}";
				break;
			case "HealthIncrease":
				Modulate = new Color(0, 1, 0);
				Text = $"{value}";
				break;

			case "ManaDecrease":
				Modulate = new Color(1, 0, 0);
				Text = $"-{value}";
				break;
			case "ManaIncrease":
				Modulate = new Color(0, 0, 1);
				Text = $"{value}";
				break;
		}
	}

	public override void _Process(double delta)
	{
		lifetimeCount += (float)delta;
		GlobalPosition += new Vector2(0, -0.1f);
		if (lifetimeCount >= lifetime) QueueFree();
	}
}
