using Godot;

namespace projecthorizonscs.Interface.SavesManager;

public partial class SaveSlot : VBoxContainer
{
	private Panel _dataPanel;
	private Button _showDataButton;
	private Button _hideDataButton;

	private Label _playedHoursLabel;
	private Label _playedMinutesLabel;
	private Label _playedSecondsLabel;

	private Label _currentLevelLabel;
	private Label _levelNameLabel;
	private Label _levelDifficultyLabel;
	private Label _seedLabel;

	public override void _Ready()
	{
		_dataPanel = GetNode<Panel>("Panel");
		_showDataButton = GetNode<Button>("Panel2/VBoxContainer/VBoxContainer2/HBoxContainer/Button");
		_hideDataButton = GetNode<Button>("Panel2/VBoxContainer/VBoxContainer2/HBoxContainer/Button4");

		_playedHoursLabel = GetNode<Label>("Panel2/VBoxContainer/VBoxContainer2/HBoxContainer2/Label2");
		_playedMinutesLabel = GetNode<Label>("Panel2/VBoxContainer/VBoxContainer2/HBoxContainer2/Label4");
		_playedSecondsLabel = GetNode<Label>("Panel2/VBoxContainer/VBoxContainer2/HBoxContainer2/Label6");

		_seedLabel = GetNode<Label>("Panel/HBoxContainer/VBoxContainer2/HBoxContainer4/Label3");
		_currentLevelLabel = GetNode<Label>("Panel/HBoxContainer/VBoxContainer2/HBoxContainer3/Label3");
		_levelNameLabel = GetNode<Label>("Panel2/VBoxContainer/VBoxContainer/Label");
		_levelDifficultyLabel = GetNode<Label>("Panel2/VBoxContainer/VBoxContainer/Label2");
	}

	private void _OnShowDataButtonUp()
	{
		_dataPanel.Visible = true;
		_showDataButton.Visible = false;
		_hideDataButton.Visible = true;
	}

	private void _OnHideDataButtonUp()
	{
		_dataPanel.Visible = false;
		_showDataButton.Visible = true;
		_hideDataButton.Visible = false;
	}

	private static void _OnDeleteButtonUp()
	{
		
	}

	private static void _OnPlayButtonUp()
	{
		
	}

	public void SetData(Godot.Collections.Dictionary<string, Variant> saveData)
	{
		var playedTime = (int)saveData["PlayedTime"];
		var hours = playedTime / 3600;
		var minutes = (playedTime % 3600) / 60;
		var seconds = playedTime % 60;

		_playedHoursLabel.Text = hours.ToString("D2");
		_playedMinutesLabel.Text = minutes.ToString("D2");
		_playedSecondsLabel.Text = seconds.ToString("D2");

		_currentLevelLabel.Text = saveData["CurrentLevel"].ToString();
		_seedLabel.Text = saveData["SaveSeed"].ToString();
	}
}