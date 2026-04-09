using Godot;

namespace projecthorizonscs.Autoload;

public partial class DataManager
{
	public void SaveGameData()
	{
		EnsureGameDataIntegrity();
		using var file = Godot.FileAccess.Open(SavePath, Godot.FileAccess.ModeFlags.Write);
		file.StoreVar(GameDataDictionary);
	}

	public void LoadGameData()
	{
		if (!Godot.FileAccess.FileExists(SavePath))
		{
			ResetGameData();
			return;
		}

		using var file = Godot.FileAccess.Open(SavePath, Godot.FileAccess.ModeFlags.Read);
		GameDataDictionary = ToStringVariantDictionary(file.GetVar());
		EnsureGameDataIntegrity();
	}
}
