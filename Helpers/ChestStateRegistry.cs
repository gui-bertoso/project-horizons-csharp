using Godot;
using System.Collections.Generic;

namespace projecthorizonscs;

public static class ChestStateRegistry
{
	private static readonly HashSet<string> _openedChests = new();

	public static bool IsOpened(string chestId)
	{
		if (string.IsNullOrWhiteSpace(chestId))
			return false;

		return _openedChests.Contains(chestId);
	}

	public static void SetOpened(string chestId, bool opened = true)
	{
		if (string.IsNullOrWhiteSpace(chestId))
			return;

		if (opened)
			_openedChests.Add(chestId);
		else
			_openedChests.Remove(chestId);
	}

	public static void Clear()
	{
		_openedChests.Clear();
	}

	public static Godot.Collections.Array<string> Serialize()
	{
		var arr = new Godot.Collections.Array<string>();

		foreach (string id in _openedChests)
			arr.Add(id);

		return arr;
	}

	public static void Deserialize(Godot.Collections.Array<string> arr)
	{
		_openedChests.Clear();

		if (arr == null)
			return;

		foreach (string id in arr)
		{
			if (!string.IsNullOrWhiteSpace(id))
				_openedChests.Add(id);
		}
	}
}