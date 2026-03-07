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

	private Label _CurrentCellLabel;
	private Label _CellTypeLabel;
	private Label _LevelBiomeLabel;
	private Label _CurrentLevelLabel;

	private Panel _Panel3;

	private Player LocalPlayer => Globals.I.LocalPlayer;
	private LevelGenerator LocalLevelGenerator => Globals.I.LocalLevelGenerator;

	/*
	Biome IDS
		- 0 = Forest
		- 1 = Dark Forest
		- 2 = Dry Forest
		- 3 = Snowlands
		- 4 = Icelands
		- 5 = Desert
		- 6 = Beach
		- 7 = Old Beach
		- 8 = Vulcanic
	*/

	public string GetBiomeByID(int id)
	{
		string biomeName = "";
		switch (id)
		{
			case 0:biomeName = "Forest"; break;
			case 1:biomeName = "Dark Forest"; break;
			case 2:biomeName = "Dry Forest"; break;
			case 3:biomeName = "Snowlands"; break;
			case 4:biomeName = "Icelands"; break;
			case 5:biomeName = "Desert"; break;
			case 6:biomeName = "Beach"; break;
			case 7:biomeName = "Old Beach"; break;
			case 8:biomeName = "Vulcanic"; break;
		}
		return biomeName;
	}
	public string GetBlockByCoords(Vector2I coords)
	{
		string blockName = "";
		switch (coords)
		{
			case Vector2I(0, 0):blockName = "Void Block"; break;
			case Vector2I(0, 1):blockName = "Half Void Block"; break;
			case Vector2I(1, 0):blockName = "Grass"; break;
			case Vector2I(1, 1):blockName = "Frezzed Grass"; break;
			case Vector2I(1, 2):blockName = "Snow Block"; break;
			case Vector2I(1, 3):blockName = "Dry Grass"; break;
			case Vector2I(1, 4):blockName = "Dark Grass"; break;
			case Vector2I(2, 0):blockName = "Dirt"; break;
			case Vector2I(2, 1):blockName = "Half Ice Block"; break;
			case Vector2I(3, 0):blockName = "Old Sand"; break;
			case Vector2I(3, 1):blockName = "Half Old Sand"; break;
			case Vector2I(3, 2):blockName = "Half Old Sand 2"; break;
			case Vector2I(4, 0):blockName = "Sand"; break;
			case Vector2I(4, 1):blockName = "Half Sand"; break;
			case Vector2I(4, 2):blockName = "Half Sand 2"; break;
		}
		return blockName;
	}

    public override void _Ready()
    {
        _PlayerPositionLabel = GetNode<Label>("VBoxContainer/Panel/VBoxContainer/HBoxContainer/Value");
        _PlayerVelocityLabel = GetNode<Label>("VBoxContainer/Panel/VBoxContainer/HBoxContainer2/Value");
        _DashDurationCountdownLabel = GetNode<Label>("VBoxContainer/Panel/VBoxContainer/HBoxContainer3/Value");
        _DashCooldownCountLabel = GetNode<Label>("VBoxContainer/Panel/VBoxContainer/HBoxContainer4/Value");
        _UsedDashChargesLabel = GetNode<Label>("VBoxContainer/Panel/VBoxContainer/HBoxContainer5/Value");
        _NotDashingTimeLabel = GetNode<Label>("VBoxContainer/Panel/VBoxContainer/HBoxContainer6/Value");
        _FramesPerSecondLabel = GetNode<Label>("VBoxContainer/Panel2/VBoxContainer/HBoxContainer/Value");
        _CurrentChunkLabel = GetNode<Label>("VBoxContainer/Panel3/VBoxContainer/HBoxContainer7/Value");
		_CheatInput = GetNode<TextEdit>("VBoxContainer/Panel4/TextEdit");

        _CurrentCellLabel = GetNode<Label>("VBoxContainer/Panel3/VBoxContainer/HBoxContainer4/Value");
        _CellTypeLabel = GetNode<Label>("VBoxContainer/Panel3/VBoxContainer/HBoxContainer3/Value");
        _LevelBiomeLabel = GetNode<Label>("VBoxContainer/Panel3/VBoxContainer/HBoxContainer2/Value");
        _CurrentLevelLabel = GetNode<Label>("VBoxContainer/Panel3/VBoxContainer/HBoxContainer4/Value");
        _Panel3 = GetNode<Panel>("VBoxContainer/Panel3");
		
    }

	public override void _Process(double delta)
	{
		UpdateData();
		UpdateCheatEditor();
	}

	public void UpdateData()
	{
		if (LocalLevelGenerator != null)
		{
			_CurrentCellLabel.Text = LocalLevelGenerator.LocalToMap(LocalPlayer.GlobalPosition).ToString();
			_CellTypeLabel.Text = GetBlockByCoords(LocalLevelGenerator.GetCellAtlasCoords(LocalLevelGenerator.LocalToMap(LocalPlayer.Position)));
			_LevelBiomeLabel.Text = GetBiomeByID(LocalLevelGenerator.LevelBiome_ID);
			_CurrentLevelLabel.Text = Globals.I.CurrentLevel.ToString();
			if (_Panel3.Visible == false)
			{
				_Panel3.Visible = true;
			}
		}
		else
		{
			if (_Panel3.Visible == true)
			{
				_Panel3.Visible = false;
			}
		}


		_PlayerPositionLabel.Text = LocalPlayer.GlobalPosition.ToString();
		_CurrentChunkLabel.Text = Globals.I.CurrentPlayerChunk.ToString();
		_PlayerVelocityLabel.Text = LocalPlayer.Velocity.ToString();
		_DashDurationCountdownLabel.Text = LocalPlayer.DashDurationCountdown.ToString();
		_DashCooldownCountLabel.Text = LocalPlayer.DashCooldownCount.ToString();
		_UsedDashChargesLabel.Text = LocalPlayer.UsedDashCharges.ToString();
		_NotDashingTimeLabel.Text = LocalPlayer.NotDashingTime.ToString();
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
				Globals.I.InMenu= false;
			}
			Globals.I.InMenu= true;
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
