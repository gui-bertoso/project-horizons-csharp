using Godot;
using projecthorizonscs;
using projecthorizonscs.Autoload;
using System;

public partial class SettingsApplier : Node
{
    public static SettingsApplier I;

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
        SetResolution(AvailableResolutions[Mathf.Clamp((int)DataManager.I.GameDataDictionary["Settings.Resolution"], 0, AvailableResolutions.Count - 1)]);
        SetFrameRateLimit((int)DataManager.I.GameDataDictionary["Settings.FrameRate"]);
        SetVSync((int)DataManager.I.GameDataDictionary["Settings.Vsync"] == 1);
        SetFullscreen((int)DataManager.I.GameDataDictionary["Settings.Fullscreen"] == 1);
        ApplyAntialiasing((int)DataManager.I.GameDataDictionary["Settings.Antialiasing"]);
        ApplyAudioSettings();
        RefreshEnvironmentSettings();
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
    }

    public void SetVSync(bool enabled)
    {
        DisplayServer.VSyncMode mode = enabled
            ? DisplayServer.VSyncMode.Enabled
            : DisplayServer.VSyncMode.Disabled;

        DisplayServer.WindowSetVsyncMode(mode);
    }

    public void SetFullscreen(bool enabled)
    {
        DisplayServer.WindowMode mode = enabled
            ? DisplayServer.WindowMode.Fullscreen
            : DisplayServer.WindowMode.Windowed;

        DisplayServer.WindowSetMode(mode);
    }

    public void SetFrameRateLimit(int frameRateSetting)
    {
        Engine.MaxFps = frameRateSetting switch
        {
            0 => 30,
            1 => 60,
            2 => 75,
            3 => 120,
            4 => 144,
            5 => 200,
            6 => 0,
            _ => 60
        };
    }

    public void ApplyAntialiasing(int antialiasingSetting)
    {
        foreach (Viewport viewport in GetTree().Root.FindChildren("*", "Viewport", true, false))
            ApplyAntialiasingToViewport(viewport, antialiasingSetting);

        ApplyAntialiasingToViewport(GetTree().Root, antialiasingSetting);
    }

    public void ApplyAudioSettings()
    {
        float generalVolume = Mathf.Clamp((float)DataManager.I.GameDataDictionary["Settings.GeneralVolume"], 0f, 100f);
        float musicVolume = Mathf.Clamp((float)DataManager.I.GameDataDictionary["Settings.MusicVolume"], 0f, 100f);
        float playerVolume = Mathf.Clamp((float)DataManager.I.GameDataDictionary["Settings.PlayerVolume"], 0f, 100f);
        float enemyVolume = Mathf.Clamp((float)DataManager.I.GameDataDictionary["Settings.EnemyVolume"], 0f, 100f);

        SetBusVolumeByName("Master", generalVolume);
        SetBusVolumeByName("Music", musicVolume);
        SetBusVolumeByName("Player", playerVolume);
        SetBusVolumeByName("Enemy", enemyVolume);
    }

    public void RefreshEnvironmentSettings()
    {
        foreach (Node node in GetTree().Root.FindChildren("*", nameof(NewEnvironment), true, false))
        {
            if (node is NewEnvironment environment)
                environment.ReloadSettings();
        }
    }

    private static void ApplyAntialiasingToViewport(Viewport viewport, int antialiasingSetting)
    {
        viewport.Msaa2D = Viewport.Msaa.Disabled;
        viewport.ScreenSpaceAA = Viewport.ScreenSpaceAAEnum.Disabled;
        viewport.UseTaa = false;

        switch (antialiasingSetting)
        {
            case 1:
                viewport.Msaa2D = Viewport.Msaa.Msaa2X;
                break;
            case 2:
                viewport.Msaa2D = Viewport.Msaa.Msaa8X;
                break;
            case 3:
                viewport.ScreenSpaceAA = Viewport.ScreenSpaceAAEnum.Fxaa;
                break;
            case 4:
                viewport.UseTaa = true;
                break;
        }
    }

    private static void SetBusVolumeByName(string busName, float sliderValue)
    {
        int busIndex = AudioServer.GetBusIndex(busName);
        if (busIndex < 0)
            return;

        float linear = Mathf.Clamp(sliderValue / 100f, 0f, 1f);
        float db = linear <= 0.0001f ? -80f : Mathf.LinearToDb(linear);
        AudioServer.SetBusVolumeDb(busIndex, db);
    }
}
