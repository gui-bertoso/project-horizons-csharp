using System;
using System.Collections.Generic;
using Godot;

namespace projecthorizonscs.Interface.AchievementsMenu;

public partial class AchievementsMenu : Control
{
	private VBoxContainer _achievementsList;
	private VBoxContainer _itemsList;

	public override void _Ready()
	{
		_achievementsList = GetNode<VBoxContainer>("%AchievementsList");
		_itemsList = GetNode<VBoxContainer>("%ItemsList");

		BuildAchievementsList();
		BuildItemsList();
	}

	private void BuildAchievementsList()
	{
		foreach (Node child in _achievementsList.GetChildren())
			child.QueueFree();

		var manager = AchievementsManager.I;
		if (manager == null)
			return;

		var achievements = new List<Achievement>();
		foreach (var achievement in manager.Achievements)
		{
			if (achievement != null)
				achievements.Add(achievement);
		}

		achievements.Sort((a, b) =>
		{
			int classCompare = a.ClassType.CompareTo(b.ClassType);
			return classCompare != 0 ? classCompare : string.Compare(a.AchievementName, b.AchievementName, StringComparison.OrdinalIgnoreCase);
		});

		foreach (var achievement in achievements)
			_achievementsList.AddChild(CreateAchievementCard(achievement, manager));
	}

	private void BuildItemsList()
	{
		foreach (Node child in _itemsList.GetChildren())
			child.QueueFree();

		var itemPaths = DirAccess.GetFilesAt("res://Items");
		var items = new List<Item>();

		foreach (string fileName in itemPaths)
		{
			if (!fileName.EndsWith(".tres", StringComparison.OrdinalIgnoreCase))
				continue;

			var item = ResourceLoader.Load<Item>($"res://Items/{fileName}");
			if (item != null && !string.IsNullOrWhiteSpace(item.ItemName))
				items.Add(item);
		}

		items.Sort((a, b) => string.Compare(a.ItemName, b.ItemName, StringComparison.OrdinalIgnoreCase));

		foreach (var item in items)
			_itemsList.AddChild(CreateItemCard(item));
	}

	private Control CreateAchievementCard(Achievement achievement, AchievementsManager manager)
	{
		var panel = CreateCardPanel();
		var row = new HBoxContainer();
		panel.AddChild(row);

		row.AddChild(CreateIcon(achievement.AchievementTexture));

		var content = new VBoxContainer();
		content.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		row.AddChild(content);

		var title = new Label
		{
			Text = $"{achievement.AchievementName} [{GetStatusText(achievement)}]"
		};
		title.AddThemeColorOverride("font_color", achievement.Unlocked ? new Color("9ad06f") : new Color("d08a6f"));
		content.AddChild(title);

		var description = new Label
		{
			Text = achievement.AchievementDescription,
			AutowrapMode = TextServer.AutowrapMode.WordSmart
		};
		content.AddChild(description);

		var metadata = new Label
		{
			Text = $"Class: {achievement.ClassType} | Trigger: {achievement.TriggerEvent} | Progress: {GetProgressText(achievement, manager)}",
			AutowrapMode = TextServer.AutowrapMode.WordSmart
		};
		metadata.AddThemeColorOverride("font_color", new Color("b7b7b7"));
		content.AddChild(metadata);

		return panel;
	}

	private Control CreateItemCard(Item item)
	{
		var panel = CreateCardPanel();
		var row = new HBoxContainer();
		panel.AddChild(row);

		row.AddChild(CreateIcon(item.ItemTexture));

		var content = new VBoxContainer();
		content.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		row.AddChild(content);

		content.AddChild(new Label { Text = item.ItemName });

		var description = new Label
		{
			Text = string.IsNullOrWhiteSpace(item.ItemDescription) ? "No description." : item.ItemDescription,
			AutowrapMode = TextServer.AutowrapMode.WordSmart
		};
		content.AddChild(description);

		var metadata = new Label
		{
			Text = $"Type: {item.ItemType} | Class: {item.ItemClass} | Amount: {item.ItemAmount} | Path: {item.ResourcePath}",
			AutowrapMode = TextServer.AutowrapMode.WordSmart
		};
		metadata.AddThemeColorOverride("font_color", new Color("b7b7b7"));
		content.AddChild(metadata);

		return panel;
	}

	private static PanelContainer CreateCardPanel()
	{
		var panel = new PanelContainer();
		panel.SizeFlagsHorizontal = SizeFlags.ExpandFill;

		var styleBox = new StyleBoxFlat
		{
			BgColor = new Color(0.12f, 0.12f, 0.12f, 0.92f),
			BorderWidthLeft = 1,
			BorderWidthTop = 1,
			BorderWidthRight = 1,
			BorderWidthBottom = 1,
			BorderColor = new Color(0.28f, 0.28f, 0.28f, 1.0f),
			ContentMarginLeft = 10,
			ContentMarginRight = 10,
			ContentMarginTop = 10,
			ContentMarginBottom = 10
		};
		panel.AddThemeStyleboxOverride("panel", styleBox);
		return panel;
	}

	private static Control CreateIcon(Texture2D texture)
	{
		var textureRect = new TextureRect
		{
			CustomMinimumSize = new Vector2(52, 52),
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			Texture = texture
		};

		return textureRect;
	}

	private static string GetStatusText(Achievement achievement)
	{
		return achievement.Unlocked ? "Unlocked" : "Locked";
	}

	private static string GetProgressText(Achievement achievement, AchievementsManager manager)
	{
		return achievement.Type switch
		{
			Achievement.AchievementType.KillEnemy => $"{GetEnemyKillAmount(manager, achievement.TargetId)}/{achievement.RequiredAmount}",
			Achievement.AchievementType.ReachLevel => $"{manager.CurrentLevel}/{achievement.RequiredAmount}",
			Achievement.AchievementType.PlayerDeaths => $"{manager.PlayerDeathsCount}/{achievement.RequiredAmount}",
			Achievement.AchievementType.DefeatBoss => manager.BossesDefeated.Contains(achievement.TargetId) ? "Done" : "Missing",
			Achievement.AchievementType.DiscoverItem => manager.DiscoveredItems.Contains(achievement.TargetId) ? "Found" : "Missing",
			Achievement.AchievementType.DiscoverBiome => manager.DiscoveredBiomes.Contains(achievement.TargetId) ? "Found" : "Missing",
			Achievement.AchievementType.DiscoverBiomeEvent => manager.DiscoveredBiomeEvents.Contains(achievement.TargetId) ? "Found" : "Missing",
			_ => "-"
		};
	}

	private static int GetEnemyKillAmount(AchievementsManager manager, string targetId)
	{
		return manager.EnemiesKilled.ContainsKey(targetId) ? manager.EnemiesKilled[targetId] : 0;
	}

	private void _OnBackPressed()
	{
		GetTree().ChangeSceneToFile("res://Interface/MainMenu/MainMenu.tscn");
	}
}
