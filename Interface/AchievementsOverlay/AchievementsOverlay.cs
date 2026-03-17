using Godot;
using projecthorizonscs.Autoload;
using System;

namespace projecthorizonscs;
public partial class AchievementsOverlay : Control
{
	private VBoxContainer _notificationsContainer;
	private PackedScene _achievementNotificationScene;

    public override void _EnterTree()
    {
		GD.Print("Local Achievements Overlay Setted");
    }

	public override void _Ready()
	{
		_notificationsContainer = GetNode<VBoxContainer>("NotificationsContainer");
		_achievementNotificationScene = GD.Load<PackedScene>("uid://bap1fxae03lej");
		Globals.I.LocalAchievementsOverlay = this;
		GD.Print("Local Achievements Overlay Setted");


		GD.Print("Local Achievements Overlay init");
	}

    public override void _Process(double delta)
    {
    }

	public void Notify(Achievement achievement)
	{
		GD.Print($"notifiyng: {achievement.AchievementName}");
		AchievementNotification achievementScene = _achievementNotificationScene.Instantiate<AchievementNotification>();
		
		GD.Print($"settings notification: {achievement.AchievementName}");
		_notificationsContainer.AddChild(achievementScene);
		GD.Print($"notification spawned: {achievement.AchievementName}");
		achievementScene.SetData(achievement);
		GD.Print($"setting data achievement: {achievement.AchievementName}");
	}
}
