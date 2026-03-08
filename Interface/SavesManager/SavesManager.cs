using System;
using Godot;

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
		"Beauriful",
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

	public SavesManager(VBoxContainer savesVBoxContainer)
	{
		_savesVBoxContainer = savesVBoxContainer;
	}

	public SavesManager()
	{
	}

	public override void _Ready()
	{
		_saveSlotScene = GD.Load<PackedScene>("uid://6afjihoylen2");
		_savesVBoxContainer = GetNode<VBoxContainer>("Panel/HBoxContainer/VBoxContainer/Panel/ScrollContainer/VBoxContainer");

		_newSavePanel = GetNode<Panel>("Panel2");
		_savesPanel = GetNode<Panel>("Panel");
		_createSaveButton = GetNode<Button>("Panel2/HBoxContainer/VBoxContainer/VBoxContainer/HBoxContainer5/Button2");

		_newSaveNameTextEdit = GetNode<TextEdit>("Panel2/HBoxContainer/VBoxContainer/VBoxContainer/HBoxContainer/TextEdit");
		_newSaveSeedTextEdit = GetNode<TextEdit>("Panel2/HBoxContainer/VBoxContainer/VBoxContainer/HBoxContainer3/TextEdit");
		_newSaveDifficultyOptionButton = GetNode<OptionButton>("Panel2/HBoxContainer/VBoxContainer/VBoxContainer/HBoxContainer3/OptionButton");	
		_newSaveMultiplayerEnabledCheckButton = GetNode<CheckButton>("Panel2/HBoxContainer/VBoxContainer/VBoxContainer/HBoxContainer4/CheckButton");	

		ClearPlaceholderSaveSlots();
	}

	private void ClearPlaceholderSaveSlots()
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

	private void _OnNewSaveButtonUp()
	{
		_newSavePanel.Visible = true;
		_savesPanel.Visible = false;
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
		for (var i = 0; i < Autoload.DataManager.I.GameDataDictionary["Saves"].AsGodotArray().Count; i++)
		{
			var saveName = Autoload.DataManager.I.GameDataDictionary["Saves"].AsGodotArray()[i].ToString();
			Autoload.DataManager.I.LoadWorldData(saveName);
			var saveData = (Godot.Collections.Dictionary<string, Variant>)Autoload.DataManager.I.CurrentSaveData;

			var saveSlot = _saveSlotScene.Instantiate<SaveSlot>();
			saveSlot.SetData(saveData);
			_savesVBoxContainer.AddChild(saveSlot);
			saveSlot.Name = saveName;
		}
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