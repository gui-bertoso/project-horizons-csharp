using Godot;

namespace projecthorizonscs.Autoload;

public partial class DataManager : Node
{
	public const int GameDataVersion = 2;
	public const int WorldDataVersion = 2;

	private const string SavePath = "user://save.txt";
	private const string WorldSavesPath = "user://";

	public static DataManager I { get; private set; }

	private string _currentSaveName = "";

	public override void _EnterTree()
	{
		if (Godot.FileAccess.FileExists(SavePath))
			LoadGameData();
		else
			ResetGameData();
	}

	public override void _Ready()
	{
		I = this;
		InputBindingsManager.EnsureBindings(GameDataDictionary);
	}

	public override void _ExitTree()
	{
		SaveGameData();
	}

	public static string GetCurrentWorldCreationVersion()
	{
		if (ProjectSettings.HasSetting("application/config/version"))
		{
			string version = ProjectSettings.GetSetting("application/config/version", "").AsString();
			if (!string.IsNullOrWhiteSpace(version))
				return version;
		}

		return $"world-data-v{WorldDataVersion}";
	}
}
