using Godot;
using System.Collections.Generic;

namespace projecthorizonscs;

public sealed class DeltaThreadGenerationResult
{
	public Dictionary<Vector2I, DeltaBakeChunkData> Chunks = new();

	public DeltaThreadGenerationResult()
	{
	}

	public DeltaThreadGenerationResult(int capacity)
	{
		Chunks = capacity > 0
			? new Dictionary<Vector2I, DeltaBakeChunkData>(capacity)
			: new Dictionary<Vector2I, DeltaBakeChunkData>();
	}

	public void Clear()
	{
		foreach (DeltaBakeChunkData chunk in Chunks.Values)
			chunk.Clear();

		Chunks.Clear();
	}
}
