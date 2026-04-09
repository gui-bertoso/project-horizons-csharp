using Godot;

namespace projecthorizonscs;

public sealed class DeltaEnemySpawnData
{
	public string EnemyId = "";
	public Vector2I Cell = Vector2I.Zero;

	public Godot.Collections.Dictionary Serialize()
	{
		return new Godot.Collections.Dictionary
		{
			{ "enemy_id", EnemyId },
			{ "cell_x", Cell.X },
			{ "cell_y", Cell.Y },
		};
	}

	public void Deserialize(Godot.Collections.Dictionary dict)
	{
		if (dict.ContainsKey("enemy_id")) EnemyId = dict["enemy_id"].AsString();

		int x = dict.ContainsKey("cell_x") ? dict["cell_x"].AsInt32() : 0;
		int y = dict.ContainsKey("cell_y") ? dict["cell_y"].AsInt32() : 0;
		Cell = new Vector2I(x, y);
	}
}
