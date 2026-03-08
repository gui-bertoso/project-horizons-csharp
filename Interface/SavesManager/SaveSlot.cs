using Godot;
using System;

public partial class SaveSlot : VBoxContainer
{
	public Panel _DataPanel;
	public Button _ShowDataButton;
	public Button _HideDataButton;

	public Label _PlayedHoursLabel;
	public Label _PlayedMinutesLabel;
	public Label _PlayedSecondsLabel;

	public Label _CurrentLevelLabel;
	public Label _LevelNameLabel;
	public Label _LevelDifficultyLabel;
	public Label _SeedLabel;

	public override void _Ready()
	{
		_DataPanel = GetNode<Panel>("Panel");
		_ShowDataButton = GetNode<Button>("Panel2/VBoxContainer/VBoxContainer2/HBoxContainer/Button");
		_HideDataButton = GetNode<Button>("Panel2/VBoxContainer/VBoxContainer2/HBoxContainer/Button4");

		_PlayedHoursLabel = GetNode<Label>("Panel2/VBoxContainer/VBoxContainer2/HBoxContainer2/Label2");
		_PlayedMinutesLabel = GetNode<Label>("Panel2/VBoxContainer/VBoxContainer2/HBoxContainer2/Label4");
		_PlayedSecondsLabel = GetNode<Label>("Panel2/VBoxContainer/VBoxContainer2/HBoxContainer2/Label6");

		_SeedLabel = GetNode<Label>("Panel/HBoxContainer/VBoxContainer2/HBoxContainer4/Label3");
		_CurrentLevelLabel = GetNode<Label>("Panel/HBoxContainer/VBoxContainer2/HBoxContainer3/Label3");
		_LevelNameLabel = GetNode<Label>("Panel2/VBoxContainer/VBoxContainer/Label");
		_LevelDifficultyLabel = GetNode<Label>("Panel2/VBoxContainer/VBoxContainer/Label2");
	}

	public void _OnShowDataButtonUp()
	{
		_DataPanel.Visible = true;
		_ShowDataButton.Visible = false;
		_HideDataButton.Visible = true;
	}

	public void _OnHideDataButtonUp()
	{
		_DataPanel.Visible = false;
		_ShowDataButton.Visible = true;
		_HideDataButton.Visible = false;
	}

	public void _OnDeleteButtonUp()
	{
		
	}

	public void _OnPlayButtonUp()
	{
		
	}

	public void SetData(Godot.Collections.Dictionary<string, Variant> saveData)
	{
		int playedTime = (int)saveData["PlayedTime"];
		int hours = playedTime / 3600;
		int minutes = (playedTime % 3600) / 60;
		int seconds = playedTime % 60;

		_PlayedHoursLabel.Text = hours.ToString("D2");
		_PlayedMinutesLabel.Text = minutes.ToString("D2");
		_PlayedSecondsLabel.Text = seconds.ToString("D2");

		_CurrentLevelLabel.Text = saveData["CurrentLevel"].ToString();
		_SeedLabel.Text = saveData["SaveSeed"].ToString();
	}
}
