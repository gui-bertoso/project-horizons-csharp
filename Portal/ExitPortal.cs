using Godot;
using projecthorizonscs.Autoload;

namespace projecthorizonscs;

public partial class ExitPortal : Node2D
{
	[Export] public int MaxLevel = 79;

	private bool _isTransitioning = false;

	public void _OnAreaBodyEntered(Node2D body)
	{
		if (_isTransitioning)
			return;

		if (body == null || Globals.I == null || body != Globals.I.LocalPlayer)
			return;

		int generationMode = GetGenerationMode();

		int currentLevel = GetCurrentLevel(generationMode);
		int nextLevel = currentLevel + 1;

		if (nextLevel > MaxLevel)
			return;

		_isTransitioning = true;

		// mantém os dois sincronizados
		if (Globals.I != null)
			Globals.I.CurrentLevel = nextLevel;

		if (generationMode == 3 && DataManager.I != null)
		{
			DataManager.I.CurrentWorldData["CurrentLevel"] = nextLevel;

			if (!DataManager.I.CurrentWorldData.ContainsKey("SaveName") &&
				DataManager.I.GameDataDictionary.TryGetValue("SaveName", out Variant saveName))
			{
				DataManager.I.CurrentWorldData["SaveName"] = saveName;
			}
		}

		GD.Print($"ExitPortal: generationMode={generationMode} currentLevel={currentLevel} nextLevel={nextLevel}");
		GD.Print("ANTES DO RELOAD:", DataManager.I.CurrentWorldData["CurrentLevel"]);

		GetTree().CallDeferred(SceneTree.MethodName.ReloadCurrentScene);
	}

	private int GetGenerationMode()
	{
		if (DataManager.I == null)
			return 0;

		if (!DataManager.I.GameDataDictionary.TryGetValue("Settings.WorldGeneration", out Variant value))
			return 0;

		return value.AsInt32();
	}

	private int GetCurrentLevel(int generationMode)
	{
		// modo delta: fonte de verdade = CurrentWorldData
		if (generationMode == 3 && DataManager.I != null)
		{
			if (DataManager.I.CurrentWorldData.TryGetValue("CurrentLevel", out Variant currentLevelValue))
				return currentLevelValue.AsInt32();
		}

		// fallback
		if (Globals.I != null)
			return Globals.I.CurrentLevel;

		return 0;
	}
}