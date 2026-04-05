using System;
using Godot;
using projecthorizonscs.Autoload;

namespace projecthorizonscs.Interface.SavesManager;

public partial class SavesManager : Control
{
	private PackedScene _saveSlotScene;
	private VBoxContainer _savesVBoxContainer;
	private Panel _newSavePanel;
	private Panel _savesPanel;
	private Button _createSaveButton;
	private TextEdit _newSaveNameTextEdit;
	private TextEdit _newSaveSeedTextEdit;
	private OptionButton _newSaveDifficultyOptionButton;
	private CheckButton _newSaveMultiplayerEnabledCheckButton;

	private Godot.Collections.Array<string> _saveNames1 =
	[
		"",
		"A",
		"The",
		"First",
		"Second",
		"Third",
		"Frontier",
		"Fucking"
	];

	private Godot.Collections.Array<string> _saveNames2 =
	[
		"",
		"Crazy",
		"Wild",
		"Beautiful",
		"Epic",
		"Legendary",
		"Strange",
		"Unique",
		"Unusual",
		"Fantastic",
		"Insane",
		"Mad",
		"Great",
		"Awesome",
		"Cool",
		"Dope",
		"Rad",
		"Sick",
		"Fun",
		"Nice",
		"Good",
		"Bad",
		"Terrible",
		"Awful",
		"Garbage",
		"Shitty",
		"Trash",
		"Crappy"
	];

	private Godot.Collections.Array<string> _saveNames3 =
	[
		"",
		"Place",
		"World",
		"Land",
		"Planet",
		"Galaxy",
		"Universe",
		"Dimension",
		"Reality",
		"Realm",
		"Zone",
		"Area",
		"Region",
		"Location",
		"Spot",
		"Field",
		"Ground",
		"Camp",
		"Base",
		"Station",
		"Outpost",
		"Village",
		"City",
		"Town",
		"Metropolis"
	];

	public override void _Ready()
	{
		_saveSlotScene = GD.Load<PackedScene>("uid://6afjihoylen2");
		_savesVBoxContainer = GetNode<VBoxContainer>("Panel/HBoxContainer/VBoxContainer/Panel/ScrollContainer/VBoxContainer");

		_newSavePanel = GetNode<Panel>("Panel2");
		_savesPanel = GetNode<Panel>("Panel");
		_createSaveButton = GetNode<Button>("Panel2/HBoxContainer/VBoxContainer/VBoxContainer/HBoxContainer5/Button2");

		_newSaveNameTextEdit = GetNode<TextEdit>("Panel2/HBoxContainer/VBoxContainer/VBoxContainer/HBoxContainer/TextEdit");
		_newSaveSeedTextEdit = GetNode<TextEdit>("Panel2/HBoxContainer/VBoxContainer/VBoxContainer/HBoxContainer3/TextEdit");
		_newSaveDifficultyOptionButton = GetNode<OptionButton>("Panel2/HBoxContainer/VBoxContainer/VBoxContainer/HBoxContainer2/OptionButton");
		_newSaveMultiplayerEnabledCheckButton = GetNode<CheckButton>("Panel2/HBoxContainer/VBoxContainer/VBoxContainer/HBoxContainer4/CheckButton");

		ClearSaveSlots();
		LoadSaves();
	}

	private void ClearSaveSlots()
	{
		var placeholderSaveSlots = _savesVBoxContainer.GetChildren();
		foreach (var child in placeholderSaveSlots)
			child.QueueFree();
	}

	private void _OnBackButtonUp()
	{
		GetTree().ChangeSceneToFile("uid://c25rg72x1rdir");
	}

	private void _OnNewSaveBackButtonUp()
	{
		_newSavePanel.Visible = false;
		_savesPanel.Visible = true;
	}

	private void _OnGivenNewSaveButtonUp()
	{
		_newSaveNameTextEdit.Text = CreateRandomSaveName();
		_newSaveSeedTextEdit.Text = CreateRandomSeed().ToString();
	}

	private void _OnCreateNewSaveButtonUp()
	{
		_savesPanel.Visible = false;
		_newSavePanel.Visible = true;
		_newSaveNameTextEdit.Text = "";
		_newSaveSeedTextEdit.Text = "";
		_newSaveDifficultyOptionButton.Selected = 1;
		_newSaveMultiplayerEnabledCheckButton.ButtonPressed = false;
	}

	private void _OnNewSaveButtonUp()
	{
		DataManager.I.CurrentWorldData["SaveName"] = _newSaveNameTextEdit.Text;
		DataManager.I.CurrentWorldData["SaveDifficulty"] = _newSaveDifficultyOptionButton.Selected;
		DataManager.I.CurrentWorldData["SaveSeed"] = _newSaveSeedTextEdit.Text;
		DataManager.I.CurrentWorldData["Multiplayer"] = _newSaveMultiplayerEnabledCheckButton.ButtonPressed;
		((Godot.Collections.Array)DataManager.I.GameDataDictionary["Saves"]).Add(_newSaveNameTextEdit.Text.ToSnakeCase());
		DataManager.I.SaveWorldData(_newSaveNameTextEdit.Text.ToSnakeCase());
		DataManager.I.SaveGameData();

		if ((int)DataManager.I.GameDataDictionary["Settings.WorldGeneration"] == 3)
		{
			DataManager.I.LoadWorldData(_newSaveNameTextEdit.Text.ToSnakeCase());
			GetTree().ChangeSceneToFile("uid://c3px4n5nm3vcc");
		}
		else
		{
			_savesPanel.Show();
			_newSavePanel.Hide();
			ClearSaveSlots();
			LoadSaves();
		}
	}

	public override void _Process(double delta)
	{
		if (_newSavePanel.Visible)
			UpdateCanCreate();
	}

	private void UpdateCanCreate()
	{
		bool canCreate = _newSaveNameTextEdit.Text != "" && _newSaveSeedTextEdit.Text != "";
		_createSaveButton.Disabled = !canCreate;
	}

	private void LoadSaves()
	{
		foreach (string saveName in (Godot.Collections.Array)DataManager.I.GameDataDictionary["Saves"])
		{
			DataManager.I.LoadWorldData(saveName);
			var saveData = DataManager.I.CurrentWorldData;

			var saveSlot = _saveSlotScene.Instantiate<SaveSlot>();
			_savesVBoxContainer.AddChild(saveSlot);
			saveSlot.Name = saveName;
			saveSlot.SetData(saveData);
		}
	}

	private static int CreateRandomSeed()
	{
		Random random = new();
		return random.Next(int.MinValue, int.MaxValue);
	}

	private string CreateRandomSaveName()
	{
		Random random = new();
		return _saveNames1[random.Next(_saveNames1.Count)] + " " +
			   _saveNames2[random.Next(_saveNames2.Count)] + " " +
			   _saveNames3[random.Next(_saveNames3.Count)];
	}
}
