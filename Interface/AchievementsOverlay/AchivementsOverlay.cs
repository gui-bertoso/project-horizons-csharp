using Godot;
using projecthorizonscs;
using projecthorizonscs.Autoload;
using System;

public partial class AchievementsOverlay : Control
{
	private VBoxContainer _notificationsContainer;
	private PackedScene _achievementNotificationScene;
	public override void _Ready()
	{
		Globals.I.LocalAchievementsOverlay = this;
		_notificationsContainer = GetNode<VBoxContainer>("NotificationsContainer");
		_achievementNotificationScene = GD.Load<PackedScene>("uid://bap1fxae03lej");
	}
	public void Notify(Achievement achievement)
	{
		AchievementNotification achievementScene = _achievementNotificationScene.Instantiate<AchievementNotification>();
		_notificationsContainer.AddChild(achievementScene);
		achievementScene.SetData(achievement);
	}
}
