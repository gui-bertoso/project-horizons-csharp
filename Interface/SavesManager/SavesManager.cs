using Godot;
using System;

public partial class SavesManager : Control
{
	public PackedScene SaveSlotScene;

	public VBoxContainer _SavesVBoxContainer;

	public Panel _NewSavePanel;
	public Panel _SavesPanel;

	public Button _CreateSaveButton;

	public TextEdit _NewSaveNameTextEdit;
	public TextEdit _NewSaveSeedTextEdit;
	public OptionButton _NewSaveDifficultyOptionButton;

	public CheckButton _NewSaveMultiplayerEnabledCheckButton;


	public Godot.Collections.Array<string> SaveNames1 = new()
	{
		"",
		"A",
		"The",
		"First",
		"Second",
		"Third",
		"Frontier",
		"Fucking",
	};
	public Godot.Collections.Array<string> SaveNames2 = new()
	{
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
		"Crappy",
	};
	public Godot.Collections.Array<string> SaveNames3 = new()
	{
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
	};

	public override void _Ready()
	{
		SaveSlotScene = GD.Load<PackedScene>("uid://6afjihoylen2");
		_SavesVBoxContainer = GetNode<VBoxContainer>("Panel/HBoxContainer/VBoxContainer/Panel/ScrollContainer/VBoxContainer");

		_NewSavePanel = GetNode<Panel>("Panel2");
		_SavesPanel = GetNode<Panel>("Panel");
		_CreateSaveButton = GetNode<Button>("Panel2/HBoxContainer/VBoxContainer/VBoxContainer/Button2");

		_NewSaveNameTextEdit = GetNode<TextEdit>("Panel2/HBoxContainer/VBoxContainer/VBoxContainer/HBoxContainer/TextEdit");
		_NewSaveSeedTextEdit = GetNode<TextEdit>("Panel2/HBoxContainer/VBoxContainer/VBoxContainer/HBoxContainer3/TextEdit");
		_NewSaveDifficultyOptionButton = GetNode<OptionButton>("Panel2/HBoxContainer/VBoxContainer/VBoxContainer/HBoxContainer3/OptionButton");	
		_NewSaveMultiplayerEnabledCheckButton = GetNode<CheckButton>("Panel2/HBoxContainer/VBoxContainer/VBoxContainer/HBoxContainer4/CheckButton");	

		ClearPlaceholderSaveSlots();
	}

	public void ClearPlaceholderSaveSlots()
	{
		var placeholderSaveSlots = _SavesVBoxContainer.GetChildren();
		foreach (var child in placeholderSaveSlots)
		{
			child.QueueFree();
		}
	}

	public void _OnBackButtonUp()
	{
		GetTree().ChangeSceneToFile("uid://c25rg72x1rdir");
	}

	public void _OnNewSaveBackButtonUp()
	{
		_NewSavePanel.Visible = false;
		_SavesPanel.Visible = true;
	}

	public void _OnGivenNewSaveButtonUp()
	{
		_NewSaveNameTextEdit.Text = CreateRandomSaveName();
		_NewSaveSeedTextEdit.Text = CreateRandomSeed().ToString();
	}

	public void _OnNewSaveButtonUp()
	{
		_NewSavePanel.Visible = true;
		_SavesPanel.Visible = false;
	}

	public override void _Process(double delta)
	{
	}

	public void LoadSaves()
	{
		for (int i = 0; i < DataManager.I.GameDataDictionary["Saves"].AsGodotArray().Count; i++)
		{
			string saveName = DataManager.I.GameDataDictionary["Saves"].AsGodotArray()[i].ToString();
			DataManager.I.LoadWorldData(saveName);
			Godot.Collections.Dictionary<string, Variant> saveData = (Godot.Collections.Dictionary<string, Variant>)DataManager.I.CurrentSaveData;

			var saveSlot = SaveSlotScene.Instantiate<SaveSlot>();
			saveSlot.SetData(saveData);
			_SavesVBoxContainer.AddChild(saveSlot);
			saveSlot.Name = saveName;
		}
	}

	public int CreateRandomSeed()
	{
		Random random = new();
		int seed = random.Next(int.MinValue, int.MaxValue);
		return seed;
	}

	public string CreateRandomSaveName()
	{
		Random random = new();
		string saveName = SaveNames1[random.Next(SaveNames1.Count)] + " " + SaveNames2[random.Next(SaveNames2.Count)] + " " + SaveNames3[random.Next(SaveNames3.Count)];
		return saveName;
	}
}
