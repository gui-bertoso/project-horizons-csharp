using Godot;
using System.Collections.Generic;

namespace projecthorizonscs;

public sealed class DeltaThreadWorldData
{
	public Dictionary<Vector2I, Vector2I> Ground = new();
	public Dictionary<Vector2I, Vector2I> SmallDetails = new();
	public Dictionary<Vector2I, Vector2I> MediumDetails = new();
	public Dictionary<Vector2I, Vector2I> Objects = new();
	public Dictionary<Vector2I, Vector2I> Shadows = new();
	public Dictionary<Vector2I, Vector2I> Canopy = new();

	public void Clear()
	{
		Ground.Clear();
		SmallDetails.Clear();
		MediumDetails.Clear();
		Objects.Clear();
		Shadows.Clear();
		Canopy.Clear();
	}
}
