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
		"res://Achievements/Exploration_01.tres",
		"res://Achievements/Exploration_02.tres",
		"res://Achievements/Exploration_03.tres"
	};

	public override void _Ready()
	{
		I = this;
		LoadAchievements();
	}

	public void LoadAchievements()
	{
		foreach (string i in AchievementsPath)
		{
			Achievement achievement = GD.Load<Achievement>(i);
			Achievements.Add(achievement);
		}
	}


	public void RegisterEnemyKilled(string enemyName, int amount = 1)
	{
		if (!EnemiesKilled.ContainsKey(enemyName))
			EnemiesKilled[enemyName] = 0;
		EnemiesKilled[enemyName] += amount;
		CheckAchievements("EnemyKilled");
	}

	public void SetCurrentLevel(int level)
	{
		CurrentLevel = level;
		CheckAchievements("CurrentLevel");
	}

	public void RegisterPlayerDeath()
	{
		PlayerDeathsCount++;
		CheckAchievements("PlayerDefeated");
	}

	public void RegisterBossDefeated(string bossName)
	{
		if (!BossesDefeated.Contains(bossName))
			BossesDefeated.Add(bossName);
		CheckAchievements("BossDefeated");
	}

	public void RegisterDiscoveredItem(string itemName)
	{
		if (!DiscoveredItems.Contains(itemName))
			DiscoveredItems.Add(itemName);
		CheckAchievements("ItemDiscovered");
	}

	public void RegisterDiscoveredBiome(string itemName)
	{
		if (!DiscoveredBiomes.Contains(itemName))
			DiscoveredBiomes.Add(itemName);
		CheckAchievements("BiomeDiscovered");
	}

	public void RegisterDiscoveredBiomeEvent(string itemName)
	{
		if (!DiscoveredBiomeEvents.Contains(itemName))
			DiscoveredBiomeEvents.Add(itemName);
		CheckAchievements("BiomeEventDiscovered");
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
				NotifyAchievementUnlocked(achievement);
			}
		}
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
