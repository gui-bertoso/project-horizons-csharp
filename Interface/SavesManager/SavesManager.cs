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
		{
			child.QueueFree();
		}
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
		GD.Print("Creating new save");
		DataManager.I.CurrentWorldData["SaveName"] = _newSaveNameTextEdit.Text;
		DataManager.I.CurrentWorldData["SaveDifficulty"] = _newSaveDifficultyOptionButton.Selected;
		DataManager.I.CurrentWorldData["SaveSeed"] = _newSaveSeedTextEdit.Text;
		DataManager.I.CurrentWorldData["Multiplayer"] = _newSaveMultiplayerEnabledCheckButton.ButtonPressed;
		((Godot.Collections.Array)DataManager.I.GameDataDictionary["Saves"]).Add(_newSaveNameTextEdit.Text.ToSnakeCase());
		DataManager.I.SaveWorldData(_newSaveNameTextEdit.Text.ToSnakeCase());
		DataManager.I.SaveGameData();
		GD.Print($"New save created {DataManager.I.CurrentWorldData}");
		_newSavePanel.Visible = false;
		_savesPanel.Visible = true;
		ClearSaveSlots();
		LoadSaves();
	}

	public override void _Process(double delta)
	{
		if (_newSavePanel.Visible)
		{
			UpdateCanCreate();
		}
	}

	private void UpdateCanCreate()
	{
		var canCreate = false;
		if (_newSaveNameTextEdit.Text != "")
		{
			if (_newSaveSeedTextEdit.Text != "")
			{
				canCreate = true;
			}
		}

		if (canCreate)
		{
			if (_createSaveButton.Disabled)
			{
				_createSaveButton.Disabled = false;
			}
		}
		else
		{
			if (!_createSaveButton.Disabled)
			{
				_createSaveButton.Disabled = true;
			}
		}
	}

	private void LoadSaves()
	{
		GD.Print("Loading saves");
		foreach (string saveName in (Godot.Collections.Array)DataManager.I.GameDataDictionary["Saves"])
		{
			GD.Print($"Loading slot >{saveName}<");
			DataManager.I.LoadWorldData(saveName);
			var saveData = DataManager.I.CurrentWorldData;
			GD.Print($"Loaded save slot data: {saveName}");

			var saveSlot = _saveSlotScene.Instantiate<SaveSlot>();
			_savesVBoxContainer.AddChild(saveSlot);
			saveSlot.Name = saveName;
			saveSlot.SetData(saveData);
			GD.Print($"Loaded save slot: {saveSlot}");
			
		}
		GD.Print("Saves loaded");
	}

	private static int CreateRandomSeed()
	{
		Random random = new();
		var seed = random.Next(int.MinValue, int.MaxValue);
		return seed;
	}

	private string CreateRandomSaveName()
	{
		Random random = new();
		var saveName = _saveNames1[random.Next(_saveNames1.Count)] + " " + _saveNames2[random.Next(_saveNames2.Count)] + " " + _saveNames3[random.Next(_saveNames3.Count)];
		return saveName;
	}
}