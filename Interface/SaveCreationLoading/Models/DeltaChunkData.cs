using Godot;
using System.Collections.Generic;

namespace projecthorizonscs;

public sealed class DeltaChunkData
{
	public List<Vector4I> GroundTiles = new();
	public List<Vector4I> SmallDetailTiles = new();
	public List<Vector4I> MediumDetailTiles = new();
	public List<Vector4I> ObjectTiles = new();
	public List<Vector4I> ShadowTiles = new();
	public List<Vector4I> CanopyTiles = new();

	public void Clear()
	{
		GroundTiles.Clear();
		SmallDetailTiles.Clear();
		MediumDetailTiles.Clear();
		ObjectTiles.Clear();
		ShadowTiles.Clear();
		CanopyTiles.Clear();
	}

	public Godot.Collections.Dictionary Serialize()
	{
		return new Godot.Collections.Dictionary
		{
			{ "ground", PackList(GroundTiles) },
			{ "small", PackList(SmallDetailTiles) },
			{ "medium", PackList(MediumDetailTiles) },
			{ "objects", PackList(ObjectTiles) },
			{ "shadows", PackList(ShadowTiles) },
			{ "canopy", PackList(CanopyTiles) },
		};
	}

	public void Deserialize(Godot.Collections.Dictionary dict)
	{
		GroundTiles = UnpackList(dict, "ground");
		SmallDetailTiles = UnpackList(dict, "small");
		MediumDetailTiles = UnpackList(dict, "medium");
		ObjectTiles = UnpackList(dict, "objects");
		ShadowTiles = UnpackList(dict, "shadows");
		CanopyTiles = UnpackList(dict, "canopy");
	}

	private static Godot.Collections.Array<Vector4I> PackList(List<Vector4I> source)
	{
		var arr = new Godot.Collections.Array<Vector4I>();
		foreach (Vector4I item in source)
			arr.Add(item);
		return arr;
	}

	private static List<Vector4I> UnpackList(Godot.Collections.Dictionary dict, string key)
	{
		var list = new List<Vector4I>();

		if (!dict.ContainsKey(key))
			return list;

		var arr = dict[key].AsGodotArray<Vector4I>();
		foreach (Vector4I item in arr)
			list.Add(item);

		return list;
	}
}
