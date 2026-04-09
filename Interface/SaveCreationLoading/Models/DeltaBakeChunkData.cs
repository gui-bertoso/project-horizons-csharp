using Godot;
using System.Collections.Generic;

namespace projecthorizonscs;

public sealed class DeltaBakeChunkData : RefCounted
{
	public List<DeltaBakeTileData> GroundTiles = new();
	public List<DeltaBakeTileData> SmallDetailTiles = new();
	public List<DeltaBakeTileData> MediumDetailTiles = new();
	public List<DeltaBakeTileData> ObjectTiles = new();
	public List<DeltaBakeTileData> ShadowTiles = new();
	public List<DeltaBakeTileData> CanopyTiles = new();

	public List<Vector2I> ValidGroundCells = new();
	public HashSet<Vector2I> ValidGroundCellSet = new();
	public HashSet<Vector2I> ObjectCellSet = new();
	public Dictionary<Vector2I, Vector2I> GroundAtlasByCell = new();

	public bool SpawnEnemies;
	public int WaterCells;
	public int VoidCells;

	public void Clear()
	{
		GroundTiles.Clear();
		SmallDetailTiles.Clear();
		MediumDetailTiles.Clear();
		ObjectTiles.Clear();
		ShadowTiles.Clear();
		CanopyTiles.Clear();
		ValidGroundCells.Clear();
		ValidGroundCellSet.Clear();
		ObjectCellSet.Clear();
		GroundAtlasByCell.Clear();
		SpawnEnemies = false;
		WaterCells = 0;
		VoidCells = 0;
	}
}
