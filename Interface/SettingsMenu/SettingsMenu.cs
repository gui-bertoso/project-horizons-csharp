using Godot;

namespace projecthorizonscs.Interface.SettingsMenu;

public partial class SettingsMenu : Control
{
	private OptionButton _frameRateOption;
	private OptionButton _vsyncOption;
	private OptionButton _bloomOption;
	private OptionButton _antialiasingOption;
	private OptionButton _detailsOption;
	private OptionButton _particlesOption;
	private OptionButton _postProcessingOption;
	private OptionButton _worldGenerationOption;

	private HSlider _generalVolumeSlider;
	private HSlider _musicVolumeSlider;
	private HSlider _playerVolumeSlider;
	private HSlider _enemyVolumeSlider;

	public override void _Ready()
	{
		_frameRateOption = GetNode<OptionButton>("%FrameRate");
		_vsyncOption = GetNode<OptionButton>("%VSync");
		_bloomOption = GetNode<OptionButton>("%Bloom");
		_antialiasingOption = GetNode<OptionButton>("%AntiAliasing");
		_detailsOption = GetNode<OptionButton>("%Details");
		_particlesOption = GetNode<OptionButton>("%Particles");
		_postProcessingOption = GetNode<OptionButton>("%PostProcessing");
		_worldGenerationOption = GetNode<OptionButton>("%WorldGeneration");

		_generalVolumeSlider = GetNode<HSlider>("%GeneralVolume");
		_musicVolumeSlider = GetNode<HSlider>("%MusicVolume");
		_playerVolumeSlider = GetNode<HSlider>("%PlayerVolume");
		_enemyVolumeSlider = GetNode<HSlider>("%EnemyVolume");

		LoadSettings();
	}

	private void _OnBackButtonUp()
	{
		GetTree().ChangeSceneToFile("uid://c25rg72x1rdir");
		SaveSettings();
	}

	private void SaveSettings()
	{
		Autoload.DataManager.I.GameDataDictionary["Settings.FrameRate"] = _frameRateOption.Selected;
		Autoload.DataManager.I.GameDataDictionary["Settings.Vsync"] = _vsyncOption.Selected;
		Autoload.DataManager.I.GameDataDictionary["Settings.Bloom"] = _bloomOption.Selected;
		Autoload.DataManager.I.GameDataDictionary["Settings.Antialiasing"] = _antialiasingOption.Selected;
		Autoload.DataManager.I.GameDataDictionary["Settings.Details"] = _detailsOption.Selected;
		Autoload.DataManager.I.GameDataDictionary["Settings.Particles"] = _particlesOption.Selected;
		Autoload.DataManager.I.GameDataDictionary["Settings.PostProcessing"] = _postProcessingOption.Selected;
		Autoload.DataManager.I.GameDataDictionary["Settings.WorldGeneration"] = _worldGenerationOption.Selected;

		Autoload.DataManager.I.GameDataDictionary["Settings.GeneralVolume"] = _generalVolumeSlider.Value;
		Autoload.DataManager.I.GameDataDictionary["Settings.MusicVolume"] = _musicVolumeSlider.Value;
		Autoload.DataManager.I.GameDataDictionary["Settings.PlayerVolume"] = _playerVolumeSlider.Value;
		Autoload.DataManager.I.GameDataDictionary["Settings.EnemyVolume"] = _enemyVolumeSlider.Value;
	}

	private void LoadSettings()
	{
		_frameRateOption.Selected = (int)Autoload.DataManager.I.GameDataDictionary["Settings.FrameRate"];
		_vsyncOption.Selected = (int)Autoload.DataManager.I.GameDataDictionary["Settings.Vsync"];
		_bloomOption.Selected = (int)Autoload.DataManager.I.GameDataDictionary["Settings.Bloom"];
		_antialiasingOption.Selected = (int)Autoload.DataManager.I.GameDataDictionary["Settings.Antialiasing"];
		_detailsOption.Selected = (int)Autoload.DataManager.I.GameDataDictionary["Settings.Details"];
		_particlesOption.Selected = (int)Autoload.DataManager.I.GameDataDictionary["Settings.Particles"];
		_postProcessingOption.Selected = (int)Autoload.DataManager.I.GameDataDictionary["Settings.PostProcessing"];
		_worldGenerationOption.Selected = (int)Autoload.DataManager.I.GameDataDictionary["Settings.WorldGeneration"];

		_generalVolumeSlider.Value = (float)Autoload.DataManager.I.GameDataDictionary["Settings.GeneralVolume"];
		_musicVolumeSlider.Value = (float)Autoload.DataManager.I.GameDataDictionary["Settings.MusicVolume"];
		_playerVolumeSlider.Value = (float)Autoload.DataManager.I.GameDataDictionary["Settings.PlayerVolume"];
		_enemyVolumeSlider.Value = (float)Autoload.DataManager.I.GameDataDictionary["Settings.EnemyVolume"];
	}
}