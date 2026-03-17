using Godot;
using Godot.Collections;
using projecthorizonscs.Autoload;

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

	private Dictionary<string, Variant> _saveData;

	public override void _Ready()
	{
		_dataPanel = GetNode<Panel>("Panel");
		_showDataButton = GetNode<Button>("%ShowData");
		_hideDataButton = GetNode<Button>("%HideData");

		_playedHoursLabel = GetNode<Label>("%Hours");
		_playedMinutesLabel = GetNode<Label>("%Minutes");
		_playedSecondsLabel = GetNode<Label>("%Seconds");

		_seedLabel = GetNode<Label>("%Seed");
		_currentLevelLabel = GetNode<Label>("%CurrentLevel");
		_levelNameLabel = GetNode<Label>("%Name");
		_levelDifficultyLabel = GetNode<Label>("%Difficulty");
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

	private void _OnPlayButtonUp()
	{
		var saveName = _levelNameLabel.Text;
		DataManager.I.LoadWorldData(saveName.ToSnakeCase());
		GetTree().ChangeSceneToFile("uid://caeqsflrr74fw");
	}

	public void SetData(Dictionary<string, Variant> saveData)
	{
		var playedTime = (int)saveData["PlayedTime"];
		var hours = playedTime / 3600;
		var minutes = playedTime % 3600 / 60;
		var seconds = playedTime % 60;

		_playedHoursLabel.Text = hours.ToString("D2");
		_playedMinutesLabel.Text = minutes.ToString("D2");
		_playedSecondsLabel.Text = seconds.ToString("D2");

		_currentLevelLabel.Text = saveData["CurrentLevel"].ToString();
		_seedLabel.Text = saveData["SaveSeed"].ToString();
		_levelNameLabel.Text = saveData["SaveName"].ToString();

		var difficultyId = (int)saveData["SaveDifficulty"];
		_levelDifficultyLabel.Text = GetDifficultyText(difficultyId);
		_levelDifficultyLabel.Modulate = GetDifficultyColor(difficultyId);
	}

	private string GetDifficultyText(int difficultyId)
	{
		switch (difficultyId)
		{
			case 0: return "Easy";
			case 1: return "Normal";
			case 2: return "Hard";
			case 3: return "Hardcore";
		}
		return "";
	}
	private Color GetDifficultyColor(int difficultyId)
	{
		switch (difficultyId)
		{
			case 0: return new Color(0, 1, 0);
			case 1: return new Color(1, 1, 0);
			case 2: return new Color(1, 0, 0);
			case 3: return new Color(.5f, .5f, .5f);
		}
		return new Color();
	}
}