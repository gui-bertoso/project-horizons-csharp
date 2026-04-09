using Godot;

namespace projecthorizonscs;

public partial class RubyChunkTileData : RefCounted
{
	public Vector2I Cell;
	public Vector2I Atlas;

	public RubyChunkTileData()
	{
	}

	public RubyChunkTileData(Vector2I cell, Vector2I atlas)
	{
		Cell = cell;
		Atlas = atlas;
	}
}
