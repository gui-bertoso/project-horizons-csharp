using Godot;

namespace projecthorizonscs;

public partial class Achievement : Resource
{
	public enum AchievementClass
	{
		Hunt,
		Exploration,
		Mission,
		Collectionator
	}
	public enum AchievementType
	{
		KillEnemy,
		ReachLevel,
		PlayerDeaths,
		DiscoverBiome,
		DiscoverBiomeEvent,
		DefeatBoss,
		DiscoverItem
	}


	[Export]
	public string Id;
	[Export]
	public string AchievementName;
	[Export]
	public AchievementClass ClassType;
	[Export(PropertyHint.MultilineText)]
	public string AchievementDescription = "";
	[Export]
	public CompressedTexture2D AchievementTexture;
	[Export]
	public AchievementType Type;
	[Export]
	public string TriggerEvent = "";
	[Export]
	public string TargetId = "";
	[Export]
	public int RequiredAmount = 1;

	public bool Unlocked = false;

	public bool IsUnlocked(AchievementsManager manager)
	{
		switch (Type)
		{
			case AchievementType.KillEnemy:
				return manager.EnemiesKilled.ContainsKey(TargetId) &&
					manager.EnemiesKilled[TargetId] >= RequiredAmount;
			case AchievementType.ReachLevel:
				return manager.CurrentLevel >= RequiredAmount;
			case AchievementType.PlayerDeaths:
				return manager.PlayerDeathsCount >= RequiredAmount;
			case AchievementType.DefeatBoss:
				return manager.BossesDefeated.Contains(TargetId);
			case AchievementType.DiscoverItem:
				return manager.DiscoveredItems.Contains(TargetId);
			default:
				return false;
		}
	}
}