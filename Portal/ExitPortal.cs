using Godot;
using projecthorizonscs.Autoload;
using projecthorizonscs.NewLevelGeneratorTests;
using System;
using System.Threading.Tasks;

public partial class ExitPortal : Node2D
{
	public void _OnAreaBodyEntered(Node2D body)
	{
		Globals.I.CurrentLevel++;
		GetTree().ReloadCurrentScene();
	}
}
