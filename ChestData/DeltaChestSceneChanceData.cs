using Godot;

namespace projecthorizonscs;

[GlobalClass]
public partial class DeltaChestSceneChanceData : Resource
{
	[Export] public string ChestScenePath = "";
	[Export] public float Chance = 1.0f;
}