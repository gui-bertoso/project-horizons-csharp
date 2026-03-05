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
	private Label _CurrentChunkLabel;
	private TextEdit _CheatInput;

    public override void _Ready()
    {
        _PlayerPositionLabel = GetNode<Label>("Panel/VBoxContainer/HBoxContainer/Value");
        _PlayerVelocityLabel = GetNode<Label>("Panel/VBoxContainer/HBoxContainer2/Value");
        _DashDurationCountdownLabel = GetNode<Label>("Panel/VBoxContainer/HBoxContainer3/Value");
        _DashCooldownCountLabel = GetNode<Label>("Panel/VBoxContainer/HBoxContainer4/Value");
        _UsedDashChargesLabel = GetNode<Label>("Panel/VBoxContainer/HBoxContainer5/Value");
        _NotDashingTimeLabel = GetNode<Label>("Panel/VBoxContainer/HBoxContainer6/Value");
        _FramesPerSecondLabel = GetNode<Label>("Panel2/VBoxContainer/HBoxContainer/Value");
        _CurrentChunkLabel = GetNode<Label>("Panel/VBoxContainer/HBoxContainer7/Value");
		_CheatInput = GetNode<TextEdit>("Panel4/TextEdit");
    }

	public override void _Process(double delta)
	{
		UpdateData();
		UpdateCheatEditor();
	}

	public void UpdateData()
	{
		_PlayerPositionLabel.Text = Globals.I.LocalPlayer.GlobalPosition.ToString();
		_CurrentChunkLabel.Text = Globals.I.CurrentPlayerChunk.ToString();
		_PlayerVelocityLabel.Text = Globals.I.LocalPlayer.Velocity.ToString();
		_DashDurationCountdownLabel.Text = Globals.I.LocalPlayer.DashDurationCountdown.ToString();
		_DashCooldownCountLabel.Text = Globals.I.LocalPlayer.DashCooldownCount.ToString();
		_UsedDashChargesLabel.Text = Globals.I.LocalPlayer.UsedDashCharges.ToString();
		_NotDashingTimeLabel.Text = Globals.I.LocalPlayer.NotDashingTime.ToString();
		_FramesPerSecondLabel.Text = Engine.GetFramesPerSecond().ToString();
	}

	public void UpdateCheatEditor()
	{
		if (_CheatInput.HasFocus())
		{
			if (Input.IsActionJustPressed("ui_accept"))
			{
				VerifyCheatCode();
				_CheatInput.Text = "";
				_CheatInput.ReleaseFocus();
			}
		}
	}


	public void VerifyCheatCode()
	{
		string input = _CheatInput.Text.ToLower();
		switch (input)
		{
			case ("code.devmode.true"):
				Globals.I.DevModeEnabled = true;
				Globals.I.EmitSignal(Globals.SignalName.DevModeUpdated);
				break;
			case ("code.devmode.false"):
				Globals.I.DevModeEnabled = false;
				Globals.I.EmitSignal(Globals.SignalName.DevModeUpdated);
				break;
		}
	}
}
