using System.IO;
using Godot;
using Godot.Collections;

namespace projecthorizonscs.Autoload;

public partial class DataManager : Node
{
	private const string SavePath = "user://save.txt";
	private const string WorldSavesPath = "user://";
	private static readonly string[] EquippedItemKeys =
	[
		"EquippedHeadArmor",
		"EquippedBodyArmor",
		"EquippedFootArmor",
		"EquippedWeapon",
		"EquippedConsumable",
		"EquippedAcessory1",
		"EquippedAcessory2"
	];

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

		{"OpenedChests", new Array<string>()},
	};
	
	public void SaveGameData()
	{
		using var file = Godot.FileAccess.Open(SavePath, Godot.FileAccess.ModeFlags.Write);
		file.StoreVar(GameDataDictionary);
	}

	public void LoadGameData()
	{
		using var file = Godot.FileAccess.Open(SavePath, Godot.FileAccess.ModeFlags.Read);
		var data = file.GetVar();

		var dictionary = data.AsGodotDictionary();

		GameDataDictionary = new Dictionary<string, Variant>();

		foreach (var key in dictionary.Keys)
		{
			GameDataDictionary[key.AsString()] = dictionary[key];
		}
	}

	public void SaveWorldData(string saveName)
	{
		using var file = Godot.FileAccess.Open(WorldSavesPath + saveName + ".txt", Godot.FileAccess.ModeFlags.Write);
		file.StoreVar(SerializeWorldData(), true);
	}

	public void DeleteWorldData(string saveName)
	{
		if (string.IsNullOrWhiteSpace(saveName))
			return;

		var saveFilePath = ProjectSettings.GlobalizePath(WorldSavesPath + saveName + ".txt");
		if (File.Exists(saveFilePath))
			File.Delete(saveFilePath);

		Array<string> saves = GameDataDictionary["Saves"].AsGodotArray<string>();
		saves.Remove(saveName);
		GameDataDictionary["Saves"] = saves;

		if (_currentSaveName == saveName)
			_currentSaveName = "";

		SaveGameData();
	}

	public void QuickSaveWorldData()
	{
		if (_currentSaveName == "")
			return;
		SaveWorldData(_currentSaveName);
	}

	public void QuickLoadWorldData()
	{
		if (_currentSaveName == "")
			return;
		LoadWorldData(_currentSaveName);
	}

	public bool IsChestOpened(string chestId)
	{
		if (string.IsNullOrWhiteSpace(chestId))
			return false;

		if (!CurrentWorldData.TryGetValue("OpenedChests", out Variant openedVar))
			return false;

		Array<string> openedChests = openedVar.AsGodotArray<string>();
		return openedChests.Contains(chestId);
	}

	public void SetChestOpened(string chestId, bool opened = true)
	{
		if (string.IsNullOrWhiteSpace(chestId))
			return;

		Array<string> openedChests;

		if (CurrentWorldData.TryGetValue("OpenedChests", out Variant openedVar))
			openedChests = openedVar.AsGodotArray<string>();
		else
			openedChests = new Array<string>();

		if (opened)
		{
			if (!openedChests.Contains(chestId))
				openedChests.Add(chestId);
		}
		else
		{
			openedChests.Remove(chestId);
		}

		CurrentWorldData["OpenedChests"] = openedChests;
	}
	
	public void LoadWorldData(string saveName)
	{
		using var file = Godot.FileAccess.Open(WorldSavesPath + saveName + ".txt", Godot.FileAccess.ModeFlags.Read);
		var data = file.GetVar(true);

		var dictionary = data.AsGodotDictionary();

		foreach (var variable in dictionary)
		{
			string key = (string)variable.Key;
			CurrentWorldData[key] = DeserializeWorldValue(key, variable.Value);
		}
		_currentSaveName = saveName;
	}

	private Dictionary<string, Variant> SerializeWorldData()
	{
		var serializedData = new Dictionary<string, Variant>();

		foreach (var entry in CurrentWorldData)
		{
			serializedData[entry.Key] = SerializeWorldValue(entry.Key, entry.Value);
		}

		return serializedData;
	}

	private static Variant SerializeWorldValue(string key, Variant value)
	{
		if (System.Array.IndexOf(EquippedItemKeys, key) < 0)
			return value;

		if (value.AsGodotObject() is not Item item || IsEmptyItem(item))
			return new Godot.Collections.Dictionary();

		return Variant.From(SerializeItem(item));
	}

	private static Variant DeserializeWorldValue(string key, Variant value)
	{
		if (System.Array.IndexOf(EquippedItemKeys, key) < 0)
			return value;

		if (value.VariantType == Variant.Type.Nil)
			return new Item();

		if (value.AsGodotObject() is Item item)
			return item;

		var itemData = value.AsGodotDictionary();
		if (itemData.Count == 0)
			return new Item();

		return DeserializeItem(itemData);
	}

	private static Godot.Collections.Dictionary SerializeItem(Item item)
	{
		return new Godot.Collections.Dictionary
		{
			{ "ItemName", item.ItemName ?? "" },
			{ "ItemType", (int)item.ItemType },
			{ "ItemClass", (int)item.ItemClass },
			{ "ItemDescription", item.ItemDescription ?? "" },
			{ "ItemAmount", item.ItemAmount },
			{ "ItemTexture", item.ItemTexture },
			{ "ItemScene", item.ItemScene },
			{ "ArmorSpriteSheet", item.ArmorSpriteSheet }
		};
	}

	private static Item DeserializeItem(Godot.Collections.Dictionary itemData)
	{
		return new Item
		{
			ItemName = itemData.ContainsKey("ItemName") ? itemData["ItemName"].AsString() : "",
			ItemType = (Item.ITEM_TYPE)(itemData.ContainsKey("ItemType") ? itemData["ItemType"].AsInt32() : 0),
			ItemClass = (Item.ITEM_CLASS)(itemData.ContainsKey("ItemClass") ? itemData["ItemClass"].AsInt32() : 0),
			ItemDescription = itemData.ContainsKey("ItemDescription") ? itemData["ItemDescription"].AsString() : "",
			ItemAmount = itemData.ContainsKey("ItemAmount") ? itemData["ItemAmount"].AsInt32() : 1,
			ItemTexture = itemData.ContainsKey("ItemTexture") ? itemData["ItemTexture"].AsGodotObject() as CompressedTexture2D : null,
			ItemScene = itemData.ContainsKey("ItemScene") ? itemData["ItemScene"].AsGodotObject() as PackedScene : null,
			ArmorSpriteSheet = itemData.ContainsKey("ArmorSpriteSheet") ? itemData["ArmorSpriteSheet"].AsGodotObject() as CompressedTexture2D : null,
		};
	}

	private static bool IsEmptyItem(Item item)
	{
		return string.IsNullOrWhiteSpace(item.ItemName) &&
			   item.ItemTexture == null &&
			   item.ItemScene == null &&
			   item.ArmorSpriteSheet == null;
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
