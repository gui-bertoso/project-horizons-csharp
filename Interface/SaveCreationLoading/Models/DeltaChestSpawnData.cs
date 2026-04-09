using Godot;

namespace projecthorizonscs;

[GlobalClass]
public sealed class DeltaChestSpawnData
{
	public string ChestId = "";
	public string ChestScenePath = "";
	public Vector2I Cell = Vector2I.Zero;

	public Godot.Collections.Dictionary Serialize()
	{
		return new()
		{
			{ "chest_id", ChestId },
			{ "scene", ChestScenePath },
			{ "cell_x", Cell.X },
			{ "cell_y", Cell.Y },
		};
	}

	public void Deserialize(Godot.Collections.Dictionary dict)
	{
		ChestId = dict.ContainsKey("chest_id") ? dict["chest_id"].AsString() : "";
		ChestScenePath = dict.ContainsKey("scene") ? dict["scene"].AsString() : "";

		int x = dict.ContainsKey("cell_x") ? dict["cell_x"].AsInt32() : 0;
		int y = dict.ContainsKey("cell_y") ? dict["cell_y"].AsInt32() : 0;

		Cell = new Vector2I(x, y);
	}
}
