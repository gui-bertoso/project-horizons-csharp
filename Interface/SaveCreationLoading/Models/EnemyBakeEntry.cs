namespace projecthorizonscs;

public sealed class EnemyBakeEntry
{
	public string ScenePath;
	public int Chance;

	public EnemyBakeEntry(string scenePath, int chance)
	{
		ScenePath = scenePath;
		Chance = chance;
	}
}
