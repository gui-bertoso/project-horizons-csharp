using System.Globalization;
using Godot;

namespace projecthorizonscs.Interface;

public partial class DebugPanel : Control
{
	private Label _playerPositionLabel;
	private Label _playerVelocityLabel;
	private Label _dashDurationCountdownLabel;
	private Label _dashCooldownCountLabel;
	private Label _usedDashChargesLabel;
	private Label _notDashingTimeLabel;
	private Label _framesPerSecondLabel;
	private Label _currentChunkLabel;
	private TextEdit _cheatInput;

	private Label _currentCellLabel;
	private Label _cellTypeLabel;
	private Label _levelBiomeLabel;
	private Label _currentLevelLabel;

	private Panel _panel3;
	private VBoxContainer _vContainer;

	private bool _enabled;

	private static string GetBiomeById(int id)
	{
		return id switch
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
			_ => "Unknown"
		};
	}

	private static string GetBlockByCoords(Vector2I coords)
	{
		return coords switch
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
			_ => "Unknown"
		};
	}

	public override void _Ready()
	{
		_vContainer = GetNode<VBoxContainer>("%BaseContainer");
		_playerPositionLabel = GetNode<Label>("%PlayerPosition");
		_playerVelocityLabel = GetNode<Label>("%Velocity");
		_dashDurationCountdownLabel = GetNode<Label>("%DashDurationCountdown");
		_dashCooldownCountLabel = GetNode<Label>("%DashCooldownCount");
		_usedDashChargesLabel = GetNode<Label>("%UsedDashCharges");
		_notDashingTimeLabel = GetNode<Label>("%NotDashingTime");
		_framesPerSecondLabel = GetNode<Label>("%FPS");
		_currentChunkLabel = GetNode<Label>("%CurrentChunk");
		_cheatInput = GetNode<TextEdit>("%CheatInput");

		_currentCellLabel = GetNode<Label>("%CurrentCell");
		_cellTypeLabel = GetNode<Label>("%CellType");
		_levelBiomeLabel = GetNode<Label>("%LevelBiome");
		_currentLevelLabel = GetNode<Label>("%CurrentLevel");
		_panel3 = GetNode<Panel>("%Panel3");

		_enabled = false;
		_vContainer.Visible = false;
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("debug"))
		{
			_enabled = !_enabled;
			_vContainer.Visible = _enabled;
		}

		if (!_enabled)
			return;

		UpdateData();
		UpdateCheatEditor();
	}

	private void UpdateData()
	{
		var player = Autoload.Globals.I.LocalPlayer;
		var levelGenerator = Autoload.Globals.I.LocalLevelGenerator;

		if (player == null)
			return;

		_playerPositionLabel.Text = player.GlobalPosition.ToString();
		_currentChunkLabel.Text = Autoload.Globals.I.CurrentPlayerChunk.ToString();
		_playerVelocityLabel.Text = player.Velocity.ToString();
		_dashDurationCountdownLabel.Text = player.DashDurationCountdown.ToString(CultureInfo.CurrentCulture);
		_dashCooldownCountLabel.Text = player.DashCooldownCount.ToString(CultureInfo.CurrentCulture);
		_usedDashChargesLabel.Text = player.UsedDashCharges.ToString();
		_notDashingTimeLabel.Text = player.NotDashingTime.ToString(CultureInfo.CurrentCulture);
		_framesPerSecondLabel.Text = Engine.GetFramesPerSecond().ToString(CultureInfo.CurrentCulture);

		if (levelGenerator != null)
		{
			var localPlayerPos = levelGenerator.ToLocal(player.GlobalPosition);
			var playerCell = levelGenerator.LocalToMap(localPlayerPos);

			_currentCellLabel.Text = playerCell.ToString();
			_cellTypeLabel.Text = GetBlockByCoords(levelGenerator.GetCellAtlasCoords(playerCell));
			_levelBiomeLabel.Text = GetBiomeById(levelGenerator.LevelBiomeId);
			_currentLevelLabel.Text = Autoload.Globals.I.CurrentLevel.ToString();
			_panel3.Visible = true;
		}
		else
		{
			_panel3.Visible = false;
		}
	}

	private void UpdateCheatEditor()
	{
		if (_cheatInput == null)
		{
			Autoload.Globals.I.InMenu = false;
			return;
		}

		if (!_cheatInput.HasFocus())
		{
			Autoload.Globals.I.InMenu = false;
			return;
		}

		Autoload.Globals.I.InMenu = true;

		if (Input.IsActionJustPressed("ui_accept"))
		{
			VerifyCheatCode();
			_cheatInput.Text = "";
			_cheatInput.ReleaseFocus();
			Autoload.Globals.I.InMenu = false;
		}
	}

	private void VerifyCheatCode()
	{
		var input = _cheatInput.Text.ToLower().StripEdges();

		switch (input)
		{
			case "code.devmode.true":
				Autoload.Globals.I.DevModeEnabled = true;
				Autoload.Globals.I.EmitSignal(Autoload.Globals.SignalName.DevModeUpdated);
				break;

			case "code.devmode.false":
				Autoload.Globals.I.DevModeEnabled = false;
				Autoload.Globals.I.EmitSignal(Autoload.Globals.SignalName.DevModeUpdated);
				break;
		}
	}
}