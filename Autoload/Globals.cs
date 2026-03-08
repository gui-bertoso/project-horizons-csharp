using Godot;

namespace projecthorizonscs.Autoload;

public partial class Globals : Node
{
	[Signal]
	public delegate void DevModeUpdatedEventHandler();
	public static Globals I {get; private set;}
	public Player.Player LocalPlayer {get; set;}
	public Player.PlayerHand LocalPlayerHand {get; set;}
	public LevelGenerator.LevelGenerator LocalLevelGenerator {get; set;}
	public Interface.ItemsDisplay.ItemsDisplay LocalItemsDisplay {get; set;}
	public bool DevModeEnabled = false;
	public bool InMenu = false;

	public int CurrentLevel = 1;

	public PackedScene PhysicItemScene = ResourceLoader.Load<PackedScene>("uid://bqjq1yutufivv");

	public Vector2 CurrentPlayerChunk;

	public override void _Ready()
	{
		I = this;
	}
}