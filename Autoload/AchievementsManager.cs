using Godot;
using Godot.Collections;
using projecthorizonscs.Autoload;
using System;

namespace projecthorizonscs;

public partial class AchievementsManager : Node
{
	public static AchievementsManager I {get; private set;}
	public Dictionary<string, int> EnemiesKilled = new();
	public int CurrentLevel = 0;
	public int PlayerDeathsCount = 0;
	public Array<string> BossesDefeated = new();
	public Array<string> DiscoveredItems = new();
	public Array<string> DiscoveredBiomes = new();
	public Array<string> DiscoveredBiomeEvents = new();

	public Array<Achievement> Achievements = new(){};

	public Array<string> AchievementsPath = new()
	{
		"res://Achievements/Hunt_01.tres",
		"res://Achievements/Hunt_02.tres",
		"res://Achievements/Hunt_03.tres",
		"res://Achievements/Exploration_01.tres",
		"res://Achievements/Exploration_02.tres",
		"res://Achievements/Exploration_03.tres",
		"res://Achievements/Exploration_04.tres",
		"res://Achievements/Mission_01.tres",
		"res://Achievements/Mission_02.tres",
		"res://Achievements/Collection_01.tres",
		"res://Achievements/Collection_02.tres"
	};

	public override void _Ready()
	{
		I = this;
		LoadAchievements();
		LoadProgress();
	}

	public void LoadAchievements()
	{
		Achievements.Clear();
		foreach (string i in AchievementsPath)
		{
			Achievement achievement = GD.Load<Achievement>(i);
			if (achievement != null)
				Achievements.Add(achievement);
		}
	}


	public void RegisterEnemyKilled(string enemyName, int amount = 1)
	{
		if (!EnemiesKilled.ContainsKey(enemyName))
			EnemiesKilled[enemyName] = 0;
		EnemiesKilled[enemyName] += amount;
		CheckAchievements("EnemyKilled");
		SaveProgress();
	}

	public void SetCurrentLevel(int level)
	{
		CurrentLevel = level;
		CheckAchievements("CurrentLevel");
		SaveProgress();
	}

	public void RegisterPlayerDeath()
	{
		PlayerDeathsCount++;
		CheckAchievements("PlayerDefeated");
		SaveProgress();
	}

	public void RegisterBossDefeated(string bossName)
	{
		if (!BossesDefeated.Contains(bossName))
			BossesDefeated.Add(bossName);
		CheckAchievements("BossDefeated");
		SaveProgress();
	}

	public void RegisterDiscoveredItem(string itemName)
	{
		if (!DiscoveredItems.Contains(itemName))
			DiscoveredItems.Add(itemName);
		CheckAchievements("ItemDiscovered");
		SaveProgress();
	}

	public void RegisterDiscoveredBiome(string itemName)
	{
		if (!DiscoveredBiomes.Contains(itemName))
			DiscoveredBiomes.Add(itemName);
		CheckAchievements("BiomeDiscovered");
		SaveProgress();
	}

	public void RegisterDiscoveredBiomeEvent(string itemName)
	{
		if (!DiscoveredBiomeEvents.Contains(itemName))
			DiscoveredBiomeEvents.Add(itemName);
		CheckAchievements("BiomeEventDiscovered");
		SaveProgress();
	}

	private void CheckAchievements(string triggerEvent)
	{
		foreach (var achievement in Achievements)
		{
			if (achievement == null)
				continue;
			if (achievement.Unlocked)
				continue;
			if (achievement.TriggerEvent != triggerEvent)
				continue;
			
			if (achievement.IsUnlocked(this))
			{
				achievement.Unlocked = true;
				SaveProgress();
				NotifyAchievementUnlocked(achievement);
			}
		}
	}

	private void LoadProgress()
	{
		if (DataManager.I == null)
			return;

		EnemiesKilled = LoadEnemiesKilled();
		CurrentLevel = GetIntValue("Achievements.CurrentLevel", CurrentLevel);
		PlayerDeathsCount = GetIntValue("Achievements.PlayerDeaths", PlayerDeathsCount);
		BossesDefeated = GetStringArray("Achievements.BossesDefeated");
		DiscoveredItems = GetStringArray("Achievements.DiscoveredItems");
		DiscoveredBiomes = GetStringArray("Achievements.DiscoveredBiomes");
		DiscoveredBiomeEvents = GetStringArray("Achievements.DiscoveredBiomeEvents");

		var unlockedIds = GetStringArray("Achievements.UnlockedIds");
		foreach (var achievement in Achievements)
		{
			if (achievement == null)
				continue;

			achievement.Unlocked = unlockedIds.Contains(achievement.Id);
		}
	}

	private void SaveProgress()
	{
		if (DataManager.I == null)
			return;

		DataManager.I.GameDataDictionary["Achievements.EnemiesKilled"] = ToGodotDictionary(EnemiesKilled);
		DataManager.I.GameDataDictionary["Achievements.CurrentLevel"] = CurrentLevel;
		DataManager.I.GameDataDictionary["Achievements.PlayerDeaths"] = PlayerDeathsCount;
		DataManager.I.GameDataDictionary["Achievements.BossesDefeated"] = BossesDefeated;
		DataManager.I.GameDataDictionary["Achievements.DiscoveredItems"] = DiscoveredItems;
		DataManager.I.GameDataDictionary["Achievements.DiscoveredBiomes"] = DiscoveredBiomes;
		DataManager.I.GameDataDictionary["Achievements.DiscoveredBiomeEvents"] = DiscoveredBiomeEvents;
		DataManager.I.GameDataDictionary["Achievements.UnlockedIds"] = GetUnlockedIds();
		DataManager.I.SaveGameData();
	}

	private Dictionary<string, int> LoadEnemiesKilled()
	{
		var result = new Dictionary<string, int>();
		if (!DataManager.I.GameDataDictionary.TryGetValue("Achievements.EnemiesKilled", out Variant variant))
			return result;

		var dictionary = variant.AsGodotDictionary();
		foreach (Variant key in dictionary.Keys)
			result[key.AsString()] = dictionary[key].AsInt32();

		return result;
	}

	private Array<string> GetUnlockedIds()
	{
		var unlockedIds = new Array<string>();
		foreach (var achievement in Achievements)
		{
			if (achievement != null && achievement.Unlocked && !string.IsNullOrWhiteSpace(achievement.Id))
				unlockedIds.Add(achievement.Id);
		}

		return unlockedIds;
	}

	private Array<string> GetStringArray(string key)
	{
		if (!DataManager.I.GameDataDictionary.TryGetValue(key, out Variant variant))
			return new Array<string>();

		return variant.AsGodotArray<string>();
	}

	private int GetIntValue(string key, int fallback)
	{
		if (!DataManager.I.GameDataDictionary.TryGetValue(key, out Variant variant))
			return fallback;

		return variant.AsInt32();
	}

	private static Godot.Collections.Dictionary ToGodotDictionary(Godot.Collections.Dictionary<string, int> source)
	{
		var dictionary = new Godot.Collections.Dictionary();
		foreach (var pair in source)
			dictionary[pair.Key] = pair.Value;

		return dictionary;
	}

	private void NotifyAchievementUnlocked(Achievement achievement)
	{
		if (Globals.I.LocalAchievementsOverlay == null)
		{
			return;
		}
		Globals.I.LocalAchievementsOverlay.Notify(achievement);
	}
}
