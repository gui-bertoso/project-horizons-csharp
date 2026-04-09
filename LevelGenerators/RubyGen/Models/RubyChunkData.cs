using Godot;
using System.Collections.Generic;

namespace projecthorizonscs;

public partial class RubyChunkData : RefCounted
{
	public List<RubyChunkTileData> GroundTiles = new();
	public List<RubyChunkTileData> SmallDetailTiles = new();
	public List<RubyChunkTileData> MediumDetailTiles = new();
	public List<RubyChunkTileData> ObjectTiles = new();
	public List<RubyChunkTileData> ShadowTiles = new();
	public List<RubyChunkTileData> CanopyTiles = new();

	public List<Vector2I> ValidGroundCells = new();
	public HashSet<Vector2I> ValidGroundCellSet = new();
	public HashSet<Vector2I> ObjectCellSet = new();
	public Dictionary<Vector2I, Vector2I> GroundAtlasByCell = new();

	public bool SpawnEnemies;
	public int WaterCells;
	public int VoidCells;
}
