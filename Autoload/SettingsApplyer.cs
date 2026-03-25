using Godot;
using projecthorizonscs.Autoload;
using System;

public partial class SettingsApplyer : Node
{
    public static SettingsApplyer I;

    public override void _Ready()
    {
        I = this;
		ApplySettings();
    }

    public Godot.Collections.Array<Vector2I> AvailableResolutions = new()
    {
        new Vector2I(1280, 720),
        new Vector2I(1280, 800),
        new Vector2I(1366, 768),
        new Vector2I(1440, 900),
        new Vector2I(1600, 900),
        new Vector2I(1680, 1050),
        new Vector2I(1920, 1080),
        new Vector2I(1920, 1200)
    };

	public void ApplySettings()
	{
		SetResolution(AvailableResolutions[(int)DataManager.I.GameDataDictionary["Settings.Resolution"]]);
		SetVSync((int)DataManager.I.GameDataDictionary["Settings.Vsync"] == 1);
		SetFullscreen((int)DataManager.I.GameDataDictionary["Settings.Fullscreen"] == 1);
	}

    public void SetResolution(Vector2I newResolution)
    {
        DisplayServer.WindowSetSize(newResolution);

        if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Windowed)
        {
            Vector2I screenSize = DisplayServer.ScreenGetSize();
            Vector2I windowPos = (screenSize - newResolution) / 2;
            DisplayServer.WindowSetPosition(windowPos);
        }

        GD.Print($"resolução alterada para: {newResolution.X}x{newResolution.Y}");
    }

    public void SetVSync(bool enabled)
    {
        DisplayServer.VSyncMode mode = enabled
            ? DisplayServer.VSyncMode.Enabled
            : DisplayServer.VSyncMode.Disabled;

        DisplayServer.WindowSetVsyncMode(mode);

        GD.Print($"vsync: {(enabled ? "ligado" : "desligado")}");
    }

    public void SetFullscreen(bool enabled)
    {
        DisplayServer.WindowMode mode = enabled
            ? DisplayServer.WindowMode.Fullscreen
            : DisplayServer.WindowMode.Windowed;

        DisplayServer.WindowSetMode(mode);

        GD.Print($"fullscreen: {(enabled ? "ligado" : "desligado")}");
    }
	
}
