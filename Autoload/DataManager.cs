using System.IO;
using Godot;

public partial class DataManager : Node
{
	public const string SavePath = "user://save.dat";
	public const string WorldSavesPath = "user://worldsaves/";

	public static DataManager I {get; private set;}
    public object CurrentSaveData { get; internal set; }

    public override void _Ready()
    {
        I = this;
    }

	public string CurrentSaveName = "";

	public Godot.Collections.Dictionary<string, Variant> GameDataDictionary = new()
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

		{"Saves", new Godot.Collections.Array<string>()}
	};

	public Godot.Collections.Dictionary<string, Variant> CurrentWorldData = new()
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
	
	public void SaveGameData()
	{
		GD.Print("Saving game data...");
		using var file = Godot.FileAccess.Open(SavePath, Godot.FileAccess.ModeFlags.Write);
		file.StoreVar(GameDataDictionary);
		GD.Print("Saved game data");
	}
	public void LoadGameData()
	{
		GD.Print("Loading game data");
		using var file = Godot.FileAccess.Open(SavePath, Godot.FileAccess.ModeFlags.Read);
		Variant data = file.GetVar();

		var Dictionary = data.AsGodotDictionary();

		GameDataDictionary = new ();

		foreach (var key in Dictionary.Keys)
		{
			GameDataDictionary[key.AsString()] = Dictionary[key];
		}
		GD.Print("Loaded game data");
	}

	public void SaveWorldData(string SaveName)
	{
		if (!Godot.FileAccess.FileExists(WorldSavesPath))
		{
			Directory.CreateDirectory(WorldSavesPath);
		}

		GD.Print("Saving world data...");
		using var file = Godot.FileAccess.Open(WorldSavesPath + SaveName + ".dat", Godot.FileAccess.ModeFlags.Write);
		file.StoreVar(CurrentWorldData);
		GD.Print("Saved world data");
	}

	public void QuickSaveWorldData()
	{
		if (CurrentSaveName == "")
		{
			GD.Print("No save name set, cannot quicksave world data");
			return;
		}
		SaveWorldData(CurrentSaveName);
	}

	public void QuickLoadWorldData()
	{
		if (CurrentSaveName == "")
		{
			GD.Print("No save name set, cannot quickload world data");
			return;
		}
		LoadWorldData(CurrentSaveName);
	}
	
	public void LoadWorldData(string SaveName)
	{
		GD.Print("Loading world data...");
		using var file = Godot.FileAccess.Open(WorldSavesPath + SaveName + ".dat", Godot.FileAccess.ModeFlags.Read);
		Variant data = file.GetVar();

		var Dictionary = data.AsGodotDictionary();

		CurrentWorldData = new ();

		foreach (var key in Dictionary.Keys)
		{
			CurrentWorldData[key.AsString()] = Dictionary[key];
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
