using Godot;
using System.Collections.Generic;

namespace projecthorizonscs;

public sealed class DeltaLevelMetadata
{
	public int LevelId;
	public int BiomeId;
	public int SeedValue;

	public bool HasInitialPortal;
	public Vector2I InitialPortalCell = Vector2I.Zero;

	public bool HasExitPortal;
	public Vector2I ExitPortalCell = Vector2I.Zero;

	public List<DeltaEnemySpawnData> Enemies = new();
	public List<DeltaChestSpawnData> Chests = new();

	public Godot.Collections.Dictionary Serialize()
	{
		var enemiesArray = new Godot.Collections.Array<Godot.Collections.Dictionary>();
		foreach (DeltaEnemySpawnData enemy in Enemies)
			enemiesArray.Add(enemy.Serialize());

		var chestsArray = new Godot.Collections.Array<Godot.Collections.Dictionary>();
		foreach (DeltaChestSpawnData chest in Chests)
			chestsArray.Add(chest.Serialize());

		return new Godot.Collections.Dictionary
		{
			{ "level_id", LevelId },
			{ "biome_id", BiomeId },
			{ "seed_value", SeedValue },
			{ "has_initial_portal", HasInitialPortal },
			{ "initial_portal_x", InitialPortalCell.X },
			{ "initial_portal_y", InitialPortalCell.Y },
			{ "has_exit_portal", HasExitPortal },
			{ "exit_portal_x", ExitPortalCell.X },
			{ "exit_portal_y", ExitPortalCell.Y },
			{ "enemies", enemiesArray },
			{ "chests", chestsArray },
		};
	}

	public void Deserialize(Godot.Collections.Dictionary dict)
	{
		if (dict.ContainsKey("level_id")) LevelId = dict["level_id"].AsInt32();
		if (dict.ContainsKey("biome_id")) BiomeId = dict["biome_id"].AsInt32();
		if (dict.ContainsKey("seed_value")) SeedValue = dict["seed_value"].AsInt32();

		HasInitialPortal = dict.ContainsKey("has_initial_portal") && dict["has_initial_portal"].AsBool();
		InitialPortalCell = new Vector2I(
			dict.ContainsKey("initial_portal_x") ? dict["initial_portal_x"].AsInt32() : 0,
			dict.ContainsKey("initial_portal_y") ? dict["initial_portal_y"].AsInt32() : 0
		);

		HasExitPortal = dict.ContainsKey("has_exit_portal") && dict["has_exit_portal"].AsBool();
		ExitPortalCell = new Vector2I(
			dict.ContainsKey("exit_portal_x") ? dict["exit_portal_x"].AsInt32() : 0,
			dict.ContainsKey("exit_portal_y") ? dict["exit_portal_y"].AsInt32() : 0
		);

		Enemies.Clear();
		if (dict.ContainsKey("enemies"))
		{
			var arr = dict["enemies"].AsGodotArray<Godot.Collections.Dictionary>();
			foreach (Godot.Collections.Dictionary enemyDict in arr)
			{
				var enemy = new DeltaEnemySpawnData();
				enemy.Deserialize(enemyDict);
				Enemies.Add(enemy);
			}
		}

		Chests.Clear();
		if (dict.ContainsKey("chests"))
		{
			var arr = dict["chests"].AsGodotArray<Godot.Collections.Dictionary>();
			foreach (Godot.Collections.Dictionary chestDict in arr)
			{
				var chest = new DeltaChestSpawnData();
				chest.Deserialize(chestDict);
				Chests.Add(chest);
			}
		}
	}
}
