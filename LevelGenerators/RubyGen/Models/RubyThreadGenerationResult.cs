using Godot;
using System.Collections.Generic;

namespace projecthorizonscs;

public sealed class RubyThreadGenerationResult
{
	public Dictionary<Vector2I, RubyChunkData> Chunks = new();
}
