using Godot;

namespace projecthorizonscs;

public static class DeltaChunkTileData
{
	public static Vector4I Pack(Vector2I cell, Vector2I atlas)
	{
		return new Vector4I(cell.X, cell.Y, atlas.X, atlas.Y);
	}
}
