using Godot;
using System;

public partial class DebugPanel : Control
{
	private float UpdateCooldown = 0.5f;
	private float UpdateCountdown;

	private Label _PlayerPositionLabel;
	private Label _PlayerVelocityLabel;
	private Label _DashDurationCountdownLabel;
	private Label _DashCooldownCountLabel;
	private Label _UsedDashChargesLabel;
	private Label _NotDashingTimeLabel;
	private Label _FramesPerSecondLabel;


    public override void _Ready()
    {
        _PlayerPositionLabel = GetNode<Label>("Panel/VBoxContainer/HBoxContainer/Value");
        _PlayerVelocityLabel = GetNode<Label>("Panel/VBoxContainer/HBoxContainer2/Value");
        _DashDurationCountdownLabel = GetNode<Label>("Panel/VBoxContainer/HBoxContainer3/Value");
        _DashCooldownCountLabel = GetNode<Label>("Panel/VBoxContainer/HBoxContainer4/Value");
        _UsedDashChargesLabel = GetNode<Label>("Panel/VBoxContainer/HBoxContainer5/Value");
        _NotDashingTimeLabel = GetNode<Label>("Panel/VBoxContainer/HBoxContainer6/Value");
        _FramesPerSecondLabel = GetNode<Label>("Panel2/VBoxContainer/HBoxContainer/Value");
    }

	public override void _Process(double delta)
	{
		UpdateData();
	}

	public void UpdateData()
	{
		_PlayerPositionLabel.Text = Globals.I.LocalPlayer.GlobalPosition.ToString();
		_PlayerVelocityLabel.Text = Globals.I.LocalPlayer.Velocity.ToString();
		_DashDurationCountdownLabel.Text = Globals.I.LocalPlayer.DashDurationCountdown.ToString();
		_DashCooldownCountLabel.Text = Globals.I.LocalPlayer.DashCooldownCount.ToString();
		_UsedDashChargesLabel.Text = Globals.I.LocalPlayer.UsedDashCharges.ToString();
		_NotDashingTimeLabel.Text = Globals.I.LocalPlayer.NotDashingTime.ToString();
		_FramesPerSecondLabel.Text = Engine.GetFramesPerSecond().ToString();
	}
}
