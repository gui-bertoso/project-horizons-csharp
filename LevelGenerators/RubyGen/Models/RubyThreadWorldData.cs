using Godot;
using System.Collections.Generic;

namespace projecthorizonscs;

public sealed class RubyThreadWorldData
{
	public Dictionary<Vector2I, Vector2I> Ground = new();
	public Dictionary<Vector2I, Vector2I> SmallDetails = new();
	public Dictionary<Vector2I, Vector2I> MediumDetails = new();
	public Dictionary<Vector2I, Vector2I> Objects = new();
	public Dictionary<Vector2I, Vector2I> Shadows = new();
	public Dictionary<Vector2I, Vector2I> Canopy = new();
}
