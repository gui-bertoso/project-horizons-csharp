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
		checkAchievements("EnemyKilled");
	}

	public void SetCurrentLevel(int level)
	{
		CurrentLevel = level;
		checkAchievements("CurrentLevel");
	}

	public void RegisterPlayerDeath()
	{
		PlayerDeathsCount++;
		checkAchievements("PlayerDefeated");
	}

	public void RegisterBossDefeated(string bossName)
	{
		if (!BossesDefeated.Contains(bossName))
			BossesDefeated.Add(bossName);
		checkAchievements("BossDefeated");
	}

	public void RegisterDiscoveredItem(string itemName)
	{
		if (!DiscoveredItems.Contains(itemName))
			DiscoveredItems.Add(itemName);
		checkAchievements("ItemDiscovered");
	}

	public void RegisterDiscoveredBiome(string itemName)
	{
		if (!DiscoveredBiomes.Contains(itemName))
			DiscoveredBiomes.Add(itemName);
		checkAchievements("BiomeDiscovered");
	}

	public void RegisterDiscoveredBiomeEvent(string itemName)
	{
		if (!DiscoveredBiomeEvents.Contains(itemName))
			DiscoveredBiomeEvents.Add(itemName);
		checkAchievements("BiomeEventDiscovered");
	}

	private void checkAchievements(string TriggerEvent)
	{
		foreach (var achievement in Achievements)
		{
			if (achievement == null)
				continue;
			if (achievement.Unlocked)
				continue;
			if (achievement.TriggerEvent != TriggerEvent)
				continue;
			
			if (achievement.IsUnlocked(this))
			{
				achievement.Unlocked = true;
				NotifyAchievementUnlocked(achievement);
				GD.Print($"achievement unlocked: {achievement.AchievementName}");
			}
		}
	}

	private void NotifyAchievementUnlocked(Achievement achievement)
	{
		if (Globals.I.LocalAchievementsOverlay == null)
		{
			return;
		}
		GD.Print($"overlay: {Globals.I.LocalAchievementsOverlay}");
		GD.Print($"notifiyng achievement: {achievement.AchievementName}");
		Globals.I.LocalAchievementsOverlay.Notify(achievement);
	}
}
