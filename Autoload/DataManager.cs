using System.IO;
using Godot;
using Godot.Collections;

namespace projecthorizonscs.Autoload;

public partial class DataManager : Node
{
	private const string SavePath = "user://save.txt";
	private const string WorldSavesPath = "user://";

	public static DataManager I {get; private set;}

	public override void _Ready()
	{
		I = this;
	}

	private string _currentSaveName = "";

	public Dictionary<string, Variant> GameDataDictionary = new()
	{
		{"Settings.Resolution", 0},
		{"Settings.FrameRate", 1},
		{"Settings.Fullscreen", 1},
		{"Settings.Vsync", 0},
		{"Settings.Bloom", 1},
		{"Settings.Details", 2},
		{"Settings.Particles", 2},
		{"Settings.PostProcessing", 2},
		{"Settings.Antialiasing", 2},
		{"Settings.GeneralVolume", 100},
		{"Settings.MusicVolume", 50},
		{"Settings.PlayerVolume", 50},
		{"Settings.EnemyVolume", 50},
		{"Settings.WorldGeneration", 0},

		{"Saves", new Array<string>()}
	};

	public Dictionary<string, Variant> CurrentWorldData = new()
	{
		{"CurrentLevel", 1},
		{"PlayedTime", 0f},
		
		{"SaveSeed", 0},
		{"SaveDifficulty", 0},
		{"SaveName", ""},
		{"Multiplayer", false},

		{"PlayerPosition", Vector2.Zero},
		{"PlayerHealth", 100},
		{"PlayerExtraHealth", 100},
		{"PlayerMoveSpeed", 50},
		{"PlayerExtraMoveSpeed", 50},

		{"EquippedHeadArmor", new Item()},
		{"EquippedBodyArmor", new Item()},
		{"EquippedFootArmor", new Item()},
		{"EquippedWeapon", new Item()},
		{"EquippedConsumable", new Item()},
		{"EquippedAcessory1", new Item()},
		{"EquippedAcessory2", new Item()},
	};
	
	public void SaveGameData()
	{
		GD.Print("Saving game data...");
		using var file = Godot.FileAccess.Open(SavePath, Godot.FileAccess.ModeFlags.Write);
		file.StoreVar(GameDataDictionary);
		GD.Print($"Saved game data {GameDataDictionary}");
	}

	public void LoadGameData()
	{
		GD.Print("Loading game data");
		using var file = Godot.FileAccess.Open(SavePath, Godot.FileAccess.ModeFlags.Read);
		var data = file.GetVar();

		var dictionary = data.AsGodotDictionary();

		GameDataDictionary = new Dictionary<string, Variant>();

		foreach (var key in dictionary.Keys)
		{
			GameDataDictionary[key.AsString()] = dictionary[key];
		}
		GD.Print($"Loaded game data {GameDataDictionary}");
	}

	public void SaveWorldData(string saveName)
	{
		GD.Print("Saving world data...");
		using var file = Godot.FileAccess.Open(WorldSavesPath + saveName + ".txt", Godot.FileAccess.ModeFlags.Write);
		file.StoreVar(CurrentWorldData, true);
		GD.Print($"Saved world data: {CurrentWorldData}");
	}

	public void QuickSaveWorldData()
	{
		if (_currentSaveName == "")
		{
			GD.Print("No save name set, cannot quicksave world data");
			return;
		}
		SaveWorldData(_currentSaveName);
	}

	public void QuickLoadWorldData()
	{
		if (_currentSaveName == "")
		{
			GD.Print("No save name set, cannot quickload world data");
			return;
		}
		LoadWorldData(_currentSaveName);
	}
	
	public void LoadWorldData(string saveName)
	{
		GD.Print("Loading world data...");
		using var file = Godot.FileAccess.Open(WorldSavesPath + saveName + ".txt", Godot.FileAccess.ModeFlags.Read);
		var data = file.GetVar(true);

		var dictionary = data.AsGodotDictionary();

		GD.Print($"New Data: {dictionary}");

		foreach (var variable in dictionary)
		{
			CurrentWorldData[(string)variable.Key] = variable.Value;
			GD.Print($"Loaded Data Variable: {variable}");
		}
		_currentSaveName = saveName;
		GD.Print("Loaded world data");
		GD.Print($"Loaded game data {CurrentWorldData}");
	}

	public override void _EnterTree()
	{
		if (Godot.FileAccess.FileExists(SavePath))
		{
			LoadGameData();
		}
	}

	public override void _ExitTree()
	{
		SaveGameData();
	}
}