namespace projecthorizonscs;

public sealed class ChestBakeEntry
{
	public string ScenePath;
	public float Chance;

	public ChestBakeEntry(string scenePath, float chance)
	{
		ScenePath = scenePath;
		Chance = chance;
	}
}
