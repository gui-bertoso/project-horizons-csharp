using Godot;
using System;

public partial class Globals : Node
{
	public static Globals I {get; private set;}
	public Player LocalPlayer {get; set;}
	public bool DevModeEnabled = true;

    public override void _Ready()
    {
        I = this;
    }
}
