using Godot;
using projecthorizonscs;
using System;

public partial class AchievementNotification : ColorRect
{

	private SceneTreeTimer _despawnTimer;

	private TextureRect _achievementTextureRect;
	private Label _achievementNameLabel;
	private Label _achievementDescriptionLabel;
	private Label _achievementTypeLabel;
	private float _despawnTime = 2.4f;
	
	public override void _Ready()
	{
		_despawnTimer = GetTree().CreateTimer(_despawnTime);
		_despawnTimer.Timeout += _Despawn;

		_achievementTextureRect = GetNode<TextureRect>("%Texture");
		_achievementNameLabel = GetNode<Label>("%Name");
		_achievementDescriptionLabel = GetNode<Label>("%Description");
		_achievementTypeLabel = GetNode<Label>("%Type");
	}

	private void _Despawn()
	{
		QueueFree();
	}

	public void SetData(Achievement achievement)
	{
		_achievementTextureRect.Texture = achievement.AchievementTexture;
		_achievementNameLabel.Text = achievement.AchievementName;
		_achievementTypeLabel.Text = achievement.Type.ToString();
		_achievementDescriptionLabel.Text = achievement.AchievementDescription;
	}

	
}
