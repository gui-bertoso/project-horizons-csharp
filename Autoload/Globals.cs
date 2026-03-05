using Godot;
using System;

public partial class Globals : Node
{
    [Signal]
    public delegate void DevModeUpdatedEventHandler();
	public static Globals I {get; private set;}
	public Player LocalPlayer {get; set;}
	public bool DevModeEnabled = false;

    public Vector2 CurrentPlayerChunk;

    public override void _Ready()
    {
        I = this;
    }
}
