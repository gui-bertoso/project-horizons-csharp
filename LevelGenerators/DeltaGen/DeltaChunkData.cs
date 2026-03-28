using Godot;

namespace projecthorizonscs;

public partial class DeltaChunkData : GodotObject
{
    public Godot.Collections.Array<Vector4I> GroundTiles = new();
    public Godot.Collections.Array<Vector4I> SmallDetailTiles = new();
    public Godot.Collections.Array<Vector4I> MediumDetailTiles = new();
    public Godot.Collections.Array<Vector4I> ObjectTiles = new();
    public Godot.Collections.Array<Vector4I> ShadowTiles = new();

    public Godot.Collections.Dictionary Serialize()
    {
        return new Godot.Collections.Dictionary
        {
            {"G", GroundTiles},
            {"S", SmallDetailTiles},
            {"M", MediumDetailTiles},
            {"O", ObjectTiles},
            {"Sh", ShadowTiles}
        };
    }

    public void Deserialize(Godot.Collections.Dictionary dict)
    {
        if (dict.TryGetValue("G", out Variant g)) GroundTiles = g.AsGodotArray<Vector4I>();
        if (dict.TryGetValue("S", out Variant s)) SmallDetailTiles = s.AsGodotArray<Vector4I>();
        if (dict.TryGetValue("M", out Variant m)) MediumDetailTiles = m.AsGodotArray<Vector4I>();
        if (dict.TryGetValue("O", out Variant o)) ObjectTiles = o.AsGodotArray<Vector4I>();
        if (dict.TryGetValue("Sh", out Variant sh)) ShadowTiles = sh.AsGodotArray<Vector4I>();
    }
}

public static class DeltaChunkTileData
{
    public static Vector4I Pack(Vector2I cell, Vector2I atlas)
    {
        return new Vector4I(cell.X, cell.Y, atlas.X, atlas.Y);
    }

    public static void Unpack(Vector4I packed, out Vector2I cell, out Vector2I atlas) 
    {
        cell = new Vector2I(packed.X, packed.Y);
        atlas = new Vector2I(packed.Z, packed.W);
    }
}
