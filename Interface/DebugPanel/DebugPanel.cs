using System.Globalization;
using Godot;

namespace projecthorizonscs.Interface.DebugPanel;

public partial class DebugPanel(
	Label playerPositionLabel,
	Label dashDurationCountdownLabel,
	Label usedDashChargesLabel,
	Label framesPerSecondLabel)
	: Control
{
	private float _updateCooldown = 0.5f;
	private float _updateCountdown;

	private Label _playerPositionLabel = playerPositionLabel;
	private Label _playerVelocityLabel;
	private Label _dashDurationCountdownLabel = dashDurationCountdownLabel;
	private Label _dashCooldownCountLabel;
	private Label _usedDashChargesLabel = usedDashChargesLabel;
	private Label _notDashingTimeLabel;
	private Label _framesPerSecondLabel = framesPerSecondLabel;
	private Label _currentChunkLabel;
	private TextEdit _cheatInput;

	private Label _currentCellLabel;
	private Label _cellTypeLabel;
	private Label _levelBiomeLabel;
	private Label _currentLevelLabel;

	private Panel _panel3;

	private VBoxContainer VContainer => GetNode<VBoxContainer>("VBoxContainer");

	private static Player.Player LocalPlayer => Autoload.Globals.I.LocalPlayer;
	private static LevelGenerator.LevelGenerator LocalLevelGenerator => Autoload.Globals.I.LocalLevelGenerator;

	private bool _enabled;

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

	private static string GetBiomeById(int id)
	{
		var biomeName = id switch
		{
			0 => "Forest",
			1 => "Dark Forest",
			2 => "Dry Forest",
			3 => "Snowlands",
			4 => "Iceland",
			5 => "Desert",
			6 => "Beach",
			7 => "Old Beach",
			8 => "Vulcanic",
			_ => ""
		};
		return biomeName;
	}

	private static string GetBlockByCoords(Vector2I coords)
	{
		var blockName = coords switch
		{
			(0, 0) => "Void Block",
			(0, 1) => "Half Void Block",
			(1, 0) => "Grass",
			(1, 1) => "Freeze Grass",
			(1, 2) => "Snow Block",
			(1, 3) => "Dry Grass",
			(1, 4) => "Dark Grass",
			(2, 0) => "Dirt",
			(2, 1) => "Half Ice Block",
			(3, 0) => "Old Sand",
			(3, 1) => "Half Old Sand",
			(3, 2) => "Half Old Sand 2",
			(4, 0) => "Sand",
			(4, 1) => "Half Sand",
			(4, 2) => "Half Sand 2",
			_ => ""
		};
		return blockName;
	}

	public override void _Ready()
	{
		_playerPositionLabel = GetNode<Label>("VBoxContainer/Panel/VBoxContainer/HBoxContainer/Value");
		_playerVelocityLabel = GetNode<Label>("VBoxContainer/Panel/VBoxContainer/HBoxContainer2/Value");
		_dashDurationCountdownLabel = GetNode<Label>("VBoxContainer/Panel/VBoxContainer/HBoxContainer3/Value");
		_dashCooldownCountLabel = GetNode<Label>("VBoxContainer/Panel/VBoxContainer/HBoxContainer4/Value");
		_usedDashChargesLabel = GetNode<Label>("VBoxContainer/Panel/VBoxContainer/HBoxContainer5/Value");
		_notDashingTimeLabel = GetNode<Label>("VBoxContainer/Panel/VBoxContainer/HBoxContainer6/Value");
		_framesPerSecondLabel = GetNode<Label>("VBoxContainer/Panel2/VBoxContainer/HBoxContainer/Value");
		_currentChunkLabel = GetNode<Label>("VBoxContainer/Panel3/VBoxContainer/HBoxContainer7/Value");
		_cheatInput = GetNode<TextEdit>("VBoxContainer/Panel4/TextEdit");

		_currentCellLabel = GetNode<Label>("VBoxContainer/Panel3/VBoxContainer/HBoxContainer4/Value");
		_cellTypeLabel = GetNode<Label>("VBoxContainer/Panel3/VBoxContainer/HBoxContainer3/Value");
		_levelBiomeLabel = GetNode<Label>("VBoxContainer/Panel3/VBoxContainer/HBoxContainer2/Value");
		_currentLevelLabel = GetNode<Label>("VBoxContainer/Panel3/VBoxContainer/HBoxContainer4/Value");
		_panel3 = GetNode<Panel>("VBoxContainer/Panel3");
		
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("debug"))
		{
			_enabled = !_enabled;
			VContainer.Visible = _enabled;
		}

		if (!_enabled) return;
		UpdateData();
		UpdateCheatEditor();
	}

	private void UpdateData()
	{
		if (LocalLevelGenerator != null)
		{
			_currentCellLabel.Text = LocalLevelGenerator.LocalToMap(LocalPlayer.GlobalPosition).ToString();
			_cellTypeLabel.Text = GetBlockByCoords(LocalLevelGenerator.GetCellAtlasCoords(LocalLevelGenerator.LocalToMap(LocalPlayer.Position)));
			_levelBiomeLabel.Text = GetBiomeById(LocalLevelGenerator.LevelBiomeId);
			_currentLevelLabel.Text = Autoload.Globals.I.CurrentLevel.ToString();
			if (!_panel3.Visible)
			{
				_panel3.Visible = true;
			}
		}
		else
		{
			if (_panel3.Visible)
			{
				_panel3.Visible = false;
			}
		}


		_playerPositionLabel.Text = LocalPlayer.GlobalPosition.ToString();
		_currentChunkLabel.Text = Autoload.Globals.I.CurrentPlayerChunk.ToString();
		_playerVelocityLabel.Text = LocalPlayer.Velocity.ToString();
		_dashDurationCountdownLabel.Text = LocalPlayer.DashDurationCountdown.ToString(CultureInfo.CurrentCulture);
		_dashCooldownCountLabel.Text = LocalPlayer.DashCooldownCount.ToString(CultureInfo.CurrentCulture);
		_usedDashChargesLabel.Text = LocalPlayer.UsedDashCharges.ToString();
		_notDashingTimeLabel.Text = LocalPlayer.NotDashingTime.ToString(CultureInfo.CurrentCulture);
		_framesPerSecondLabel.Text = Engine.GetFramesPerSecond().ToString(CultureInfo.CurrentCulture);
	}

	private void UpdateCheatEditor()
	{
		if (!_cheatInput.HasFocus()) return;
		if (Input.IsActionJustPressed("ui_accept"))
		{
			VerifyCheatCode();
			_cheatInput.Text = "";
			_cheatInput.ReleaseFocus();
			Autoload.Globals.I.InMenu= false;
		}
		Autoload.Globals.I.InMenu= true;
	}


	private void VerifyCheatCode()
	{
		var input = _cheatInput.Text.ToLower();
		switch (input)
		{
			case ("code.devmode.true"):
				Autoload.Globals.I.DevModeEnabled = true;
				Autoload.Globals.I.EmitSignal(Autoload.Globals.SignalName.DevModeUpdated);
				break;
			case ("code.devmode.false"):
				Autoload.Globals.I.DevModeEnabled = false;
				Autoload.Globals.I.EmitSignal(Autoload.Globals.SignalName.DevModeUpdated);
				break;
		}
	}
}