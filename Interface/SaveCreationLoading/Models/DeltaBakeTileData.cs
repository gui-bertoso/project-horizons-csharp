using Godot;

namespace projecthorizonscs;

public sealed class DeltaBakeTileData : RefCounted
{
	public Vector2I Cell;
	public Vector2I Atlas;

	public DeltaBakeTileData()
	{
	}

	public DeltaBakeTileData(Vector2I cell, Vector2I atlas)
	{
		Cell = cell;
		Atlas = atlas;
	}
}
