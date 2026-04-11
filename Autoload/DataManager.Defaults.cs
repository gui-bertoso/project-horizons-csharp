using Godot;
using Godot.Collections;

namespace projecthorizonscs.Autoload;

public partial class DataManager
{
	private static readonly string[] EquippedItemKeys =
	[
		"EquippedHeadArmor",
		"EquippedBodyArmor",
		"EquippedFootArmor",
		"EquippedWeapon",
		"EquippedConsumable",
		"EquippedAcessory1",
		"EquippedAcessory2"
	];

	public Dictionary<string, Variant> GameDataDictionary = CreateDefaultGameData();
	public Dictionary<string, Variant> CurrentWorldData = CreateDefaultWorldData();

	private static Dictionary<string, Variant> CreateDefaultGameData()
	{
		return new Dictionary<string, Variant>
		{
			{ "Meta.GameDataVersion", GameDataVersion },
			{ "Settings.Resolution", 0 },
			{ "Settings.FrameRate", 1 },
			{ "Settings.Fullscreen", 1 },
			{ "Settings.Vsync", 0 },
			{ "Settings.Bloom", 1 },
			{ "Settings.Details", 2 },
			{ "Settings.Particles", 2 },
			{ "Settings.PostProcessing", 2 },
			{ "Settings.Antialiasing", 2 },
			{ "Settings.GeneralVolume", 100f },
			{ "Settings.MusicVolume", 50f },
			{ "Settings.PlayerVolume", 50f },
			{ "Settings.EnemyVolume", 50f },
			{ "Settings.WorldGeneration", 0 },
			{ "Settings.InputBindings", new Dictionary() },
			{ "Achievements.EnemiesKilled", new Dictionary() },
			{ "Achievements.CurrentLevel", 0 },
			{ "Achievements.PlayerDeaths", 0 },
			{ "Achievements.BossesDefeated", new Array<string>() },
			{ "Achievements.DiscoveredItems", new Array<string>() },
			{ "Achievements.DiscoveredBiomes", new Array<string>() },
			{ "Achievements.DiscoveredBiomeEvents", new Array<string>() },
			{ "Achievements.UnlockedIds", new Array<string>() },
			{ "Saves", new Array<string>() }
		};
	}

	private static Dictionary<string, Variant> CreateDefaultWorldData()
	{
		return new Dictionary<string, Variant>
		{
			{ "Meta.WorldDataVersion", WorldDataVersion },
			{ "Meta.WorldCreatedVersion", "" },
			{ "CurrentLevel", 1 },
			{ "PlayedTime", 0f },
			{ "SaveSeed", "" },
			{ "SaveDifficulty", 0 },
			{ "SaveName", "" },
			{ "Multiplayer", false },
			{ "PlayerPosition", Vector2.Zero },
			{ "PlayerHealth", 100 },
			{ "PlayerExtraHealth", 100 },
			{ "PlayerMoveSpeed", 50 },
			{ "PlayerExtraMoveSpeed", 50 },
			{ "PlayerXp", 0 },
			{ "PlayerLevel", 1 },
			{ "PlayerKills", 0 },
			{ "EquippedHeadArmor", new Item() },
			{ "EquippedBodyArmor", new Item() },
			{ "EquippedFootArmor", new Item() },
			{ "EquippedWeapon", new Item() },
			{ "EquippedConsumable", new Item() },
			{ "EquippedAcessory1", new Item() },
			{ "EquippedAcessory2", new Item() },
			{ "OpenedChests", new Array<string>() }
		};
	}

	private void ResetGameData()
	{
		GameDataDictionary = CreateDefaultGameData();
		InputBindingsManager.EnsureBindings(GameDataDictionary);
	}

	private void ResetWorldData()
	{
		CurrentWorldData = CreateDefaultWorldData();
	}

	private void EnsureGameDataIntegrity()
	{
		MergeDefaults(GameDataDictionary, CreateDefaultGameData());
		GameDataDictionary["Meta.GameDataVersion"] = GameDataVersion;
		InputBindingsManager.EnsureBindings(GameDataDictionary);
	}

	private void EnsureWorldDataIntegrity()
	{
		MergeDefaults(CurrentWorldData, CreateDefaultWorldData());
		CurrentWorldData["Meta.WorldDataVersion"] = WorldDataVersion;

		foreach (string key in EquippedItemKeys)
		{
			if (!CurrentWorldData.TryGetValue(key, out Variant itemValue) || itemValue.AsGodotObject() is not Item)
				CurrentWorldData[key] = new Item();
		}

		if (!CurrentWorldData.TryGetValue("OpenedChests", out Variant openedVar) ||
			openedVar.VariantType != Variant.Type.Array)
		{
			CurrentWorldData["OpenedChests"] = new Array<string>();
		}
	}

	private static void MergeDefaults(Dictionary<string, Variant> target, Dictionary<string, Variant> defaults)
	{
		foreach (var entry in defaults)
		{
			if (!target.ContainsKey(entry.Key))
				target[entry.Key] = entry.Value;
		}
	}

	private static Dictionary<string, Variant> ToStringVariantDictionary(Variant data)
	{
		var result = new Dictionary<string, Variant>();
		if (data.VariantType != Variant.Type.Dictionary)
			return result;

		var source = data.AsGodotDictionary();
		foreach (Variant key in source.Keys)
			result[key.AsString()] = source[key];

		return result;
	}
}
