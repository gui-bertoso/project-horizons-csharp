using Godot;
using projecthorizonscs.Autoload;

namespace projecthorizonscs;

public partial class AchievementsOverlay : Control
{
	private VBoxContainer _notificationsContainer;
	private PackedScene _achievementNotificationScene;

	public override void _EnterTree()
	{
	}

	public override void _Ready()
	{
		_notificationsContainer = GetNode<VBoxContainer>("NotificationsContainer");
		_achievementNotificationScene = GD.Load<PackedScene>("uid://bap1fxae03lej");
		Globals.I.LocalAchievementsOverlay = this;
	}

	public override void _Process(double delta)
	{
	}

	public void Notify(Achievement achievement)
	{
		AchievementNotification achievementScene = _achievementNotificationScene.Instantiate<AchievementNotification>();
		_notificationsContainer.AddChild(achievementScene);
		achievementScene.SetData(achievement);
	}
}
