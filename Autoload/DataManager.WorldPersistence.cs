using System.IO;
using Godot;
using Godot.Collections;

namespace projecthorizonscs.Autoload;

public partial class DataManager
{
	public void SaveWorldData(string saveName)
	{
		EnsureWorldDataIntegrity();
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

		Array<string> openedChests = CurrentWorldData.TryGetValue("OpenedChests", out Variant openedVar)
			? openedVar.AsGodotArray<string>()
			: new Array<string>();

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
		ResetWorldData();

		using var file = Godot.FileAccess.Open(WorldSavesPath + saveName + ".txt", Godot.FileAccess.ModeFlags.Read);
		var data = file.GetVar(true);
		var dictionary = data.AsGodotDictionary();

		foreach (var variable in dictionary)
		{
			string key = variable.Key.AsString();
			CurrentWorldData[key] = DeserializeWorldValue(key, variable.Value);
		}

		EnsureWorldDataIntegrity();
		_currentSaveName = saveName;
	}
}
