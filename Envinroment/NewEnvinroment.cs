using Godot;

namespace projecthorizonscs;

public partial class NewEnvinroment : WorldEnvironment
{
	private GpuParticles2D _rainParticles;
	private GpuParticles2D _snowParticles;
	private GpuParticles2D _dustParticles;
	private CanvasModulate _colorFilter;
	private DirectionalLight2D _directionalLight;

	private Godot.Environment _env;
	
	private int _bloomId;
	private int _detailsId;
	private int _particlesId;
	private int _postProcessingId;

	private bool _particlesEvent;

	private int _biomeId;

	public override void _Ready()
	{

		_bloomId = (int)Autoload.DataManager.I.GameDataDictionary["Settings.Bloom"];
		_detailsId = (int)Autoload.DataManager.I.GameDataDictionary["Settings.Details"];
		_particlesId = (int)Autoload.DataManager.I.GameDataDictionary["Settings.Particles"];
		_postProcessingId = (int)Autoload.DataManager.I.GameDataDictionary["Settings.PostProcessing"];

		_rainParticles = GetNode<GpuParticles2D>("Particles/Rain");
		_dustParticles = GetNode<GpuParticles2D>("Particles/Dust");
		_snowParticles = GetNode<GpuParticles2D>("Particles/Snow");
		_directionalLight = GetNode<DirectionalLight2D>("DirectionalLight");
		_colorFilter = GetNode<CanvasModulate>("CanvasModulate");


		GetData();
		ApplyEnvinroment();
	}

	public override void _Process(double delta)
	{
	}

	private void GetData()
	{
		var levelGenerator = Autoload.Globals.I.LocalLevelGenerator;
		_biomeId = levelGenerator?.LevelBiomeId ?? 0;
	}

	public void ApplyEnvinroment()
	{
		_env = new Godot.Environment();
		_env.BackgroundMode = Godot.Environment.BGMode.Canvas;

		switch (_biomeId)
		{
			case 0:
				ApplyForestEnvinroment();
				break;
			case 1:
				ApplyDarkForestEnvinroment();
				break;
			case 5:
				ApplyDesertEnvinroment();
				break;
			default:
				ApplyForestEnvinroment();
				break;
		}

		Environment = _env;
	}

	private void ApplyDarkForestEnvinroment()
	{
		if (_bloomId == 1)
		{
			_env.GlowEnabled = true;
			_env.GlowBloom = 0.02f;
		}
		switch (_postProcessingId)
		{
			case 2:
				if (_particlesId == 2)
				{
					_particlesEvent = new RandomNumberGenerator().RandiRange(0, 9) < 2;
					if (_particlesEvent)
					{
						_rainParticles.Emitting = true;
						_rainParticles.Visible = false;
						_env.TonemapMode = Godot.Environment.ToneMapper.Agx;
						_env.AdjustmentBrightness = 0.7f;
						_env.AdjustmentContrast = 0.9f;
						_env.AdjustmentSaturation = 0.63f;
						_env.AdjustmentEnabled = true;
						_env.TonemapExposure = 1.0f;
						_colorFilter.Visible = true;
						_directionalLight.Visible = true;
						_colorFilter.Color = new Color(0.031f, 0.15f, 1.0f);
						return;
					}
				}

				_env.TonemapMode = Godot.Environment.ToneMapper.Agx;
				_env.AdjustmentBrightness = 0.7f;
				_env.AdjustmentContrast = 0.9f;
				_env.AdjustmentSaturation = 0.63f;
				_env.AdjustmentEnabled = true;
				_env.TonemapExposure = 1.0f;
				_colorFilter.Visible = true;
				_colorFilter.Color = new Color(0.617f, 1.0f, 0.584f);
				_directionalLight.Visible = true;
				break;
			case 1:
				_env.TonemapMode = Godot.Environment.ToneMapper.Linear;
				_env.AdjustmentBrightness = 0.82f;
				_env.AdjustmentContrast = 0.95f;
				_env.AdjustmentSaturation = 0.78f;
				_env.AdjustmentEnabled = true;
				_colorFilter.Visible = false;
				_env.TonemapExposure = 0.95f;
				_directionalLight.Visible = true;
				break;
			case 0:
				_env.TonemapMode = Godot.Environment.ToneMapper.Linear;
				_env.AdjustmentEnabled = false;
				_colorFilter.Visible = false;
				_env.TonemapExposure = 1.0f;
				_directionalLight.Visible = true;
				break;
		}
	}

	private void ApplyDesertEnvinroment()
	{
		if (_bloomId == 1)
		{
			_env.GlowEnabled = true;
			_env.GlowBloom = 0.12f;
		}
		switch (_postProcessingId)
		{
			case 2:
				if (_particlesId == 2)
				{
					_particlesEvent = new RandomNumberGenerator().RandiRange(0, 9) < 2;
					if (_particlesEvent)
					{
						_dustParticles.Emitting = true;
						_dustParticles.Visible = false;
					}
				}

				_env.TonemapMode = Godot.Environment.ToneMapper.Filmic;
				_env.AdjustmentBrightness = 0.97f;
				_env.AdjustmentContrast = 1.075f;
				_env.AdjustmentSaturation = 1.44f;
				_env.AdjustmentEnabled = true;
				_env.TonemapExposure = 0.5f;
				_colorFilter.Visible = true;
				_colorFilter.Color = new Color(0.986f, 0.937f, 0.0f);
				_directionalLight.Visible = true;
				break;
			case 1:
				_env.TonemapMode = Godot.Environment.ToneMapper.Filmic;
				_env.AdjustmentBrightness = 1.0f;
				_env.AdjustmentContrast = 1.0f;
				_env.AdjustmentSaturation = 1.0f;
				_env.AdjustmentEnabled = true;
				_colorFilter.Visible = false;
				_env.TonemapExposure = 1.0f;
				_directionalLight.Visible = true;
				break;
			case 0:
				_env.TonemapMode = Godot.Environment.ToneMapper.Linear;
				_env.AdjustmentEnabled = false;
				_colorFilter.Visible = false;
				_env.TonemapExposure = 0.85f;
				_directionalLight.Visible = true;
				break;
		}
	}

	private void ApplyForestEnvinroment()
	{
		if (_bloomId == 1)
		{
			_env.GlowEnabled = true;
			_env.GlowBloom = 0.1f;
		}
		switch (_postProcessingId)
		{
			case 2:
				if (_particlesId == 2)
				{
					_particlesEvent = new RandomNumberGenerator().RandiRange(0, 9) < 2;
					if (_particlesEvent)
					{
						_rainParticles.Emitting = true;
						_rainParticles.Visible = false;
						_env.TonemapMode = Godot.Environment.ToneMapper.Agx;
						_env.AdjustmentBrightness = 0.77f;
						_env.AdjustmentContrast = 1.12f;
						_env.AdjustmentSaturation = 1.0f;
						_env.AdjustmentEnabled = true;
						_env.TonemapExposure = 0.44f;
						_colorFilter.Visible = true;
						_colorFilter.Color = new Color(0.015f, 0.06f, 0.098f);
						_directionalLight.Visible = true;
						return;
					}
				}

				_env.TonemapMode = Godot.Environment.ToneMapper.Filmic;
				_env.AdjustmentBrightness = 0.91f;
				_env.AdjustmentContrast = 1.065f;
				_env.AdjustmentSaturation = 1.39f;
				_env.AdjustmentEnabled = true;
				_env.TonemapExposure = 0.72f;
				_colorFilter.Visible = true;
				_colorFilter.Color = new Color(0.617f, 1.0f, 0.584f);
				_directionalLight.Visible = true;
				break;
			case 1:
				_env.TonemapMode = Godot.Environment.ToneMapper.Linear;
				_env.AdjustmentBrightness = 1.0f;
				_env.AdjustmentContrast = 1.05f;
				_env.AdjustmentSaturation = 1.1f;
				_env.AdjustmentEnabled = true;
				_env.TonemapExposure = 0.85f;
				_directionalLight.Visible = true;
				break;
			case 0:
				_env.TonemapMode = Godot.Environment.ToneMapper.Linear;
				_env.AdjustmentEnabled = false;
				_env.TonemapExposure = 1.0f;
				_colorFilter.Visible = false;
				_directionalLight.Visible = true;
				break;
		}
	}
}