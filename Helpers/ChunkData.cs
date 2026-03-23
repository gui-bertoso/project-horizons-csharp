using Godot;
using System;
using Godot.Collections;
using Array = Godot.Collections.Array;

public partial class ChunkData: Node
{
    public Array blocksID = new();
    public Array detailsID = new();
    public bool SpawnEnemys;
}