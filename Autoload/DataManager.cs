using System.IO;
using Godot;
using Godot.Collections;

namespace projecthorizonscs.Autoload;

public partial class DataManager : Node
{
	private const string SavePath = "user://save.dat";
	private const string WorldSavesPath = "user://worldsaves/";

	public static DataManager I {get; private set;}
	public object CurrentSaveData { get;}

	public override void _Ready()
	{
		I = this;
	}

	private string _currentSaveName = "";

	public Dictionary<string, Variant> GameDataDictionary = new()
	{
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

		{"Saves", new Array<string>()}
	};

	private Dictionary<string, Variant> _currentWorldData = new()
	{
		{"CurrentLevel", 1},
		{"PlayedTime", 0f},
		
		{"SaveSeed", 0},
		{"SaveDifficulty", 0},
		{"SaveName", ""},

		{"PlayerPosition", Vector2.Zero},
		{"PlayerHealth", 100},
		{"PlayerExtraHealth", 100},
		{"PlayerMoveSpeed", 50},
		{"PlayerExtraMoveSpeed", 50},
	};

	public DataManager(object currentSaveData)
	{
		CurrentSaveData = currentSaveData;
	}

	public DataManager()
	{
	}

	private void SaveGameData()
	{
		GD.Print("Saving game data...");
		using var file = Godot.FileAccess.Open(SavePath, Godot.FileAccess.ModeFlags.Write);
		file.StoreVar(GameDataDictionary);
		GD.Print("Saved game data");
	}

	private void LoadGameData()
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
		GD.Print("Loaded game data");
	}

	private void SaveWorldData(string saveName)
	{
		if (!Godot.FileAccess.FileExists(WorldSavesPath))
		{
			Directory.CreateDirectory(WorldSavesPath);
		}

		GD.Print("Saving world data...");
		using var file = Godot.FileAccess.Open(WorldSavesPath + saveName + ".dat", Godot.FileAccess.ModeFlags.Write);
		file.StoreVar(_currentWorldData);
		GD.Print("Saved world data");
	}

	private void QuickSaveWorldData()
	{
		if (_currentSaveName == "")
		{
			GD.Print("No save name set, cannot quicksave world data");
			return;
		}
		SaveWorldData(_currentSaveName);
	}

	private void QuickLoadWorldData()
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
		using var file = Godot.FileAccess.Open(WorldSavesPath + saveName + ".dat", Godot.FileAccess.ModeFlags.Read);
		var data = file.GetVar();

		var dictionary = data.AsGodotDictionary();

		_currentWorldData = new Dictionary<string, Variant>();

		foreach (var key in dictionary.Keys)
		{
			_currentWorldData[key.AsString()] = dictionary[key];
		}
		GD.Print("Loaded world data");
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