using Godot;
using System;
using System.Data;

public partial class SettingsMenu : Control
{
	public OptionButton FrameRateOption;
	public OptionButton VsyncOption;
	public OptionButton BloomOption;
	public OptionButton AntialiasingOption;
	public OptionButton DetailsOption;
	public OptionButton ParticlesOption;
	public OptionButton PostProcessingOption;

	public HSlider GeneralVolumeSlider;
	public HSlider MusicVolumeSlider;
	public HSlider PlayerVolumeSlider;
	public HSlider EnemyVolumeSlider;

	public override void _Ready()
	{
		FrameRateOption = GetNode<OptionButton>("Panel/VBoxContainer/ScrollContainer/VBoxContainer/HBoxContainer/OptionButton");
		VsyncOption = GetNode<OptionButton>("Panel/VBoxContainer/ScrollContainer/VBoxContainer/HBoxContainer11/OptionButton");
		BloomOption = GetNode<OptionButton>("Panel/VBoxContainer/ScrollContainer/VBoxContainer/HBoxContainer10/OptionButton");
		AntialiasingOption = GetNode<OptionButton>("Panel/VBoxContainer/ScrollContainer/VBoxContainer/HBoxContainer2/OptionButton");
		DetailsOption = GetNode<OptionButton>("Panel/VBoxContainer/ScrollContainer/VBoxContainer/HBoxContainer3/OptionButton");
		ParticlesOption = GetNode<OptionButton>("Panel/VBoxContainer/ScrollContainer/VBoxContainer/HBoxContainer4/OptionButton");
		PostProcessingOption = GetNode<OptionButton>("Panel/VBoxContainer/ScrollContainer/VBoxContainer/HBoxContainer5/OptionButton");

		GeneralVolumeSlider = GetNode<HSlider>("Panel/VBoxContainer/ScrollContainer/VBoxContainer/HBoxContainer6/HSlider");
		MusicVolumeSlider = GetNode<HSlider>("Panel/VBoxContainer/ScrollContainer/VBoxContainer/HBoxContainer7/HSlider");
		PlayerVolumeSlider = GetNode<HSlider>("Panel/VBoxContainer/ScrollContainer/VBoxContainer/HBoxContainer8/HSlider");
		EnemyVolumeSlider = GetNode<HSlider>("Panel/VBoxContainer/ScrollContainer/VBoxContainer/HBoxContainer9/HSlider");

		LoadSettings();
	}

	public void _OnBackButtonUp()
	{
		GetTree().ChangeSceneToFile("uid://c25rg72x1rdir");
		SaveSettings();
	}

	public void SaveSettings()
	{
		DataManager.I.GameDataDictionary["Settings.FrameRate"] = FrameRateOption.Selected;
		DataManager.I.GameDataDictionary["Settings.Vsync"] = VsyncOption.Selected;
		DataManager.I.GameDataDictionary["Settings.Bloom"] = BloomOption.Selected;
		DataManager.I.GameDataDictionary["Settings.Antialiasing"] = AntialiasingOption.Selected;
		DataManager.I.GameDataDictionary["Settings.Details"] = DetailsOption.Selected;
		DataManager.I.GameDataDictionary["Settings.Particles"] = ParticlesOption.Selected;
		DataManager.I.GameDataDictionary["Settings.PostProcessing"] = PostProcessingOption.Selected;

		DataManager.I.GameDataDictionary["Settings.GeneralVolume"] = GeneralVolumeSlider.Value;
		DataManager.I.GameDataDictionary["Settings.MusicVolume"] = MusicVolumeSlider.Value;
		DataManager.I.GameDataDictionary["Settings.PlayerVolume"] = PlayerVolumeSlider.Value;
		DataManager.I.GameDataDictionary["Settings.EnemyVolume"] = EnemyVolumeSlider.Value;
	}

	public void LoadSettings()
	{
		FrameRateOption.Selected = (int)DataManager.I.GameDataDictionary["Settings.FrameRate"];
		VsyncOption.Selected = (int)DataManager.I.GameDataDictionary["Settings.Vsync"];
		BloomOption.Selected = (int)DataManager.I.GameDataDictionary["Settings.Bloom"];
		AntialiasingOption.Selected = (int)DataManager.I.GameDataDictionary["Settings.Antialiasing"];
		DetailsOption.Selected = (int)DataManager.I.GameDataDictionary["Settings.Details"];
		ParticlesOption.Selected = (int)DataManager.I.GameDataDictionary["Settings.Particles"];
		PostProcessingOption.Selected = (int)DataManager.I.GameDataDictionary["Settings.PostProcessing"];

		GeneralVolumeSlider.Value = (float)DataManager.I.GameDataDictionary["Settings.GeneralVolume"];
		MusicVolumeSlider.Value = (float)DataManager.I.GameDataDictionary["Settings.MusicVolume"];
		PlayerVolumeSlider.Value = (float)DataManager.I.GameDataDictionary["Settings.PlayerVolume"];
		EnemyVolumeSlider.Value = (float)DataManager.I.GameDataDictionary["Settings.EnemyVolume"];
	}
}
