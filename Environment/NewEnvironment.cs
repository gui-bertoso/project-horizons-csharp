using System;
using Godot;

namespace projecthorizonscs;

public partial class NewEnvironment : WorldEnvironment
{
	[Export]
	public int PreviewBiomeId = -1;

	private enum WeatherVariant
	{
		Calm,
		Rain,
		Fog,
		Spores,
		DustStorm,
		Heatwave,
		Snowfall
	}

	private sealed class BiomeVisualProfile
	{
		public required Godot.Environment.ToneMapper Tonemap { get; init; }
		public required float Exposure { get; init; }
		public required float Brightness { get; init; }
		public required float Contrast { get; init; }
		public required float Saturation { get; init; }
		public required float GlowBloom { get; init; }
		public required float LightEnergy { get; init; }
		public required float LightRotationDegrees { get; init; }
		public required Color CanvasTint { get; init; }
		public required Color LightColor { get; init; }
		public required Color SkyOverlayColor { get; init; }
		public required Color FarFogColor { get; init; }
		public required Color NearFogColor { get; init; }
		public required Color LightOverlayColor { get; init; }
		public required Color HeatHazeColor { get; init; }
		public required WeatherVariant[] Variants { get; init; }
		public required float[] VariantWeights { get; init; }
	}

	private readonly RandomNumberGenerator _rng = new();

	private GpuParticles2D _rainParticles;
	private GpuParticles2D _dustParticles;
	private GpuParticles2D _snowParticles;
	private GpuParticles2D _sporeParticles;
	private ColorRect _skyOverlay;
	private ColorRect _farFog;
	private ColorRect _nearFog;
	private ColorRect _lightOverlay;
	private ColorRect _heatHaze;
	private CanvasModulate _canvasModulate;
	private DirectionalLight2D _directionalLight;

	private Godot.Environment _env;

	private int _bloomId;
	private int _detailsId;
	private int _particlesId;
	private int _postProcessingId;

	private int _biomeId;
	private int _variantBiomeId = -999;
	private WeatherVariant _currentVariant = WeatherVariant.Calm;

	public override void _Ready()
	{
		_rng.Randomize();

		_rainParticles = GetNode<GpuParticles2D>("Weather/Rain");
		_dustParticles = GetNode<GpuParticles2D>("Weather/Dust");
		_snowParticles = GetNode<GpuParticles2D>("Weather/Snow");
		_sporeParticles = GetNode<GpuParticles2D>("AmbientParticles/Spores");
		_skyOverlay = GetNode<ColorRect>("SkyOverlay/ColorRect");
		_farFog = GetNode<ColorRect>("FarFog/ColorRect");
		_nearFog = GetNode<ColorRect>("NearFog/ColorRect");
		_lightOverlay = GetNode<ColorRect>("LightOverlay/ColorRect");
		_heatHaze = GetNode<ColorRect>("ScreenFx/HeatHaze");
		_directionalLight = GetNode<DirectionalLight2D>("DirectionalLight");
		_canvasModulate = GetNode<CanvasModulate>("CanvasModulate");

		ReloadSettingValues();
		GetData();
		EnsureVariantRoll(forceRoll: true);
		ApplyEnvironment();
	}

	private void GetData()
	{
		if (PreviewBiomeId >= 0)
		{
			_biomeId = PreviewBiomeId;
			return;
		}

		var levelGenerator = Autoload.Globals.I.LocalLevelGenerator;
		_biomeId = levelGenerator?.LevelBiomeId ?? 0;
	}

	public void ReloadSettings()
	{
		if (!IsInsideTree())
			return;

		ReloadSettingValues();
		GetData();
		EnsureVariantRoll(forceRoll: false);
		ApplyEnvironment();
	}

	public void SetPreviewBiome(int biomeId)
	{
		if (PreviewBiomeId == biomeId)
		{
			ReloadSettings();
			return;
		}

		PreviewBiomeId = biomeId;
		_variantBiomeId = -999;
		ReloadSettings();
	}

	private void ReloadSettingValues()
	{
		_bloomId = (int)Autoload.DataManager.I.GameDataDictionary["Settings.Bloom"];
		_detailsId = (int)Autoload.DataManager.I.GameDataDictionary["Settings.Details"];
		_particlesId = (int)Autoload.DataManager.I.GameDataDictionary["Settings.Particles"];
		_postProcessingId = (int)Autoload.DataManager.I.GameDataDictionary["Settings.PostProcessing"];
	}

	public void ApplyEnvironment()
	{
		var profile = GetBiomeProfile(_biomeId);

		_env = new Godot.Environment
		{
			BackgroundMode = Godot.Environment.BGMode.Canvas,
			TonemapMode = profile.Tonemap,
			TonemapExposure = profile.Exposure
		};

		ResetVisualState();
		ApplyPostProcessing(profile);
		ApplyDirectionalLight(profile);
		ApplyBaseLayers(profile);
		ApplyVariant(profile);

		Environment = _env;
	}

	private void EnsureVariantRoll(bool forceRoll)
	{
		var profile = GetBiomeProfile(_biomeId);
		if (!forceRoll && _variantBiomeId == _biomeId)
			return;

		_currentVariant = RollVariant(profile);
		_variantBiomeId = _biomeId;
	}

	private void ApplyPostProcessing(BiomeVisualProfile profile)
	{
		var postStrength = GetPostStrength();
		var detailStrength = GetDetailStrength();

		_env.AdjustmentEnabled = true;
		_env.AdjustmentBrightness = Mathf.Lerp(1.0f, profile.Brightness, postStrength);
		_env.AdjustmentContrast = Mathf.Lerp(1.0f, profile.Contrast, postStrength);
		_env.AdjustmentSaturation = Mathf.Lerp(1.0f, profile.Saturation, postStrength);

		if (_bloomId == 1 && profile.GlowBloom > 0.0f)
		{
			_env.GlowEnabled = true;
			_env.GlowBloom = profile.GlowBloom * Mathf.Lerp(0.7f, 1.0f, detailStrength);
		}

		_canvasModulate.Color = new Color(1.0f, 1.0f, 1.0f).Lerp(profile.CanvasTint, Mathf.Lerp(0.2f, 0.9f, postStrength));
	}

	private void ApplyDirectionalLight(BiomeVisualProfile profile)
	{
		var detailStrength = GetDetailStrength();
		_directionalLight.Visible = true;
		_directionalLight.Color = profile.LightColor;
		_directionalLight.Energy = profile.LightEnergy * Mathf.Lerp(0.6f, 1.0f, detailStrength);
		_directionalLight.Rotation = Mathf.DegToRad(profile.LightRotationDegrees);
	}

	private void ApplyBaseLayers(BiomeVisualProfile profile)
	{
		var detailStrength = GetDetailStrength();
		var postStrength = GetPostStrength();

		SetOverlay(_skyOverlay, profile.SkyOverlayColor, 0.2f + 0.35f * postStrength);
		SetOverlay(_farFog, profile.FarFogColor, 0.18f + 0.28f * detailStrength);
		SetOverlay(_nearFog, profile.NearFogColor, 0.12f + 0.22f * detailStrength * postStrength);
		SetOverlay(_lightOverlay, profile.LightOverlayColor, 0.1f + 0.18f * postStrength);

		if (profile.HeatHazeColor.A > 0.0f)
			SetOverlay(_heatHaze, profile.HeatHazeColor, 0.12f + 0.22f * postStrength);
	}

	private void ApplyVariant(BiomeVisualProfile profile)
	{
		var particleStrength = GetParticleStrength();
		var detailStrength = GetDetailStrength();
		var postStrength = GetPostStrength();

		switch (_currentVariant)
		{
			case WeatherVariant.Rain:
				SetParticleState(_rainParticles, true, 140, particleStrength);
				SetOverlay(_skyOverlay, profile.SkyOverlayColor.Darkened(0.18f), 0.38f + 0.22f * postStrength);
				SetOverlay(_nearFog, profile.NearFogColor.Lightened(0.08f), 0.2f + 0.22f * detailStrength);
				break;
			case WeatherVariant.Fog:
				SetOverlay(_farFog, profile.FarFogColor.Lightened(0.12f), 0.35f + 0.3f * detailStrength);
				SetOverlay(_nearFog, profile.NearFogColor.Lightened(0.18f), 0.28f + 0.28f * detailStrength);
				break;
			case WeatherVariant.Spores:
				SetParticleState(_sporeParticles, true, 95, particleStrength);
				SetOverlay(_farFog, profile.FarFogColor.Lightened(0.05f), 0.26f + 0.18f * detailStrength);
				break;
			case WeatherVariant.DustStorm:
				SetParticleState(_dustParticles, true, 220, particleStrength);
				SetOverlay(_skyOverlay, profile.SkyOverlayColor.Lightened(0.05f), 0.34f + 0.24f * postStrength);
				SetOverlay(_farFog, profile.FarFogColor.Lightened(0.1f), 0.28f + 0.28f * detailStrength);
				SetOverlay(_nearFog, profile.NearFogColor.Lightened(0.05f), 0.2f + 0.2f * detailStrength);
				SetOverlay(_heatHaze, profile.HeatHazeColor.Lightened(0.04f), 0.24f + 0.22f * postStrength);
				break;
			case WeatherVariant.Heatwave:
				SetOverlay(_heatHaze, profile.HeatHazeColor.Lightened(0.08f), 0.3f + 0.28f * postStrength);
				SetOverlay(_lightOverlay, profile.LightOverlayColor.Lightened(0.08f), 0.18f + 0.2f * postStrength);
				break;
			case WeatherVariant.Snowfall:
				SetParticleState(_snowParticles, true, 135, particleStrength);
				SetOverlay(_farFog, profile.FarFogColor.Lightened(0.18f), 0.28f + 0.22f * detailStrength);
				break;
			case WeatherVariant.Calm:
			default:
				if (_biomeId == 0)
					SetParticleState(_sporeParticles, true, 40, particleStrength * 0.55f);
				break;
		}
	}

	private void ResetVisualState()
	{
		_rainParticles.Emitting = false;
		_dustParticles.Emitting = false;
		_snowParticles.Emitting = false;
		_sporeParticles.Emitting = false;
		_rainParticles.Visible = false;
		_dustParticles.Visible = false;
		_snowParticles.Visible = false;
		_sporeParticles.Visible = false;

		_skyOverlay.Visible = false;
		_farFog.Visible = false;
		_nearFog.Visible = false;
		_lightOverlay.Visible = false;
		_heatHaze.Visible = false;

		_canvasModulate.Color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		_directionalLight.Visible = false;
	}

	private void SetParticleState(GpuParticles2D particles, bool enabled, int baseAmount, float strength)
	{
		if (!enabled || strength <= 0.01f)
		{
			particles.Visible = false;
			particles.Emitting = false;
			return;
		}

		particles.Amount = Math.Max(1, Mathf.RoundToInt(baseAmount * strength));
		particles.Visible = true;
		particles.Emitting = true;
	}

	private static void SetOverlay(ColorRect rect, Color color, float alpha)
	{
		var finalColor = new Color(color.R, color.G, color.B, Mathf.Clamp(alpha, 0.0f, 1.0f));
		rect.Color = finalColor;
		rect.Visible = finalColor.A > 0.01f;
	}

	private float GetDetailStrength()
	{
		return _detailsId switch
		{
			0 => 0.55f,
			1 => 0.8f,
			2 => 1.0f,
			_ => 0.8f
		};
	}

	private float GetParticleStrength()
	{
		return _particlesId switch
		{
			0 => 0.0f,
			1 => 0.7f,
			2 => 1.0f,
			_ => 0.7f
		};
	}

	private float GetPostStrength()
	{
		return _postProcessingId switch
		{
			0 => 0.35f,
			1 => 0.7f,
			2 => 1.0f,
			_ => 0.7f
		};
	}

	private WeatherVariant RollVariant(BiomeVisualProfile profile)
	{
		var totalWeight = 0.0f;
		foreach (var weight in profile.VariantWeights)
			totalWeight += weight;

		var rolled = _rng.RandfRange(0.0f, totalWeight);
		var currentWeight = 0.0f;

		for (var i = 0; i < profile.Variants.Length; i++)
		{
			currentWeight += profile.VariantWeights[i];
			if (rolled <= currentWeight)
				return profile.Variants[i];
		}

		return profile.Variants[0];
	}

	private BiomeVisualProfile GetBiomeProfile(int biomeId)
	{
		return biomeId switch
		{
			1 => CreateDarkForestProfile(),
			3 => CreateSnowProfile(brightExposure: 0.9f),
			4 => CreateSnowProfile(brightExposure: 0.8f),
			5 => CreateDesertProfile(),
			_ => CreateForestProfile()
		};
	}

	private static BiomeVisualProfile CreateForestProfile()
	{
		return new BiomeVisualProfile
		{
			Tonemap = Godot.Environment.ToneMapper.Filmic,
			Exposure = 0.78f,
			Brightness = 0.94f,
			Contrast = 1.08f,
			Saturation = 1.18f,
			GlowBloom = 0.08f,
			LightEnergy = 0.75f,
			LightRotationDegrees = -26.0f,
			CanvasTint = new Color(0.91f, 0.98f, 0.9f, 1.0f),
			LightColor = new Color(0.96f, 0.93f, 0.8f, 1.0f),
			SkyOverlayColor = new Color(0.61f, 0.82f, 0.58f, 0.0f),
			FarFogColor = new Color(0.73f, 0.86f, 0.64f, 0.0f),
			NearFogColor = new Color(0.52f, 0.69f, 0.49f, 0.0f),
			LightOverlayColor = new Color(0.96f, 0.9f, 0.66f, 0.0f),
			HeatHazeColor = new Color(0.0f, 0.0f, 0.0f, 0.0f),
			Variants = new[] { WeatherVariant.Calm, WeatherVariant.Rain, WeatherVariant.Spores },
			VariantWeights = new[] { 0.55f, 0.28f, 0.17f }
		};
	}

	private static BiomeVisualProfile CreateDarkForestProfile()
	{
		return new BiomeVisualProfile
		{
			Tonemap = Godot.Environment.ToneMapper.Agx,
			Exposure = 0.7f,
			Brightness = 0.78f,
			Contrast = 1.12f,
			Saturation = 0.74f,
			GlowBloom = 0.03f,
			LightEnergy = 0.58f,
			LightRotationDegrees = -36.0f,
			CanvasTint = new Color(0.82f, 0.9f, 0.96f, 1.0f),
			LightColor = new Color(0.46f, 0.62f, 0.9f, 1.0f),
			SkyOverlayColor = new Color(0.08f, 0.14f, 0.22f, 0.0f),
			FarFogColor = new Color(0.19f, 0.27f, 0.28f, 0.0f),
			NearFogColor = new Color(0.11f, 0.17f, 0.19f, 0.0f),
			LightOverlayColor = new Color(0.22f, 0.34f, 0.44f, 0.0f),
			HeatHazeColor = new Color(0.0f, 0.0f, 0.0f, 0.0f),
			Variants = new[] { WeatherVariant.Fog, WeatherVariant.Rain, WeatherVariant.Spores },
			VariantWeights = new[] { 0.46f, 0.33f, 0.21f }
		};
	}

	private static BiomeVisualProfile CreateDesertProfile()
	{
		return new BiomeVisualProfile
		{
			Tonemap = Godot.Environment.ToneMapper.Filmic,
			Exposure = 0.58f,
			Brightness = 1.01f,
			Contrast = 1.1f,
			Saturation = 1.3f,
			GlowBloom = 0.02f,
			LightEnergy = 0.96f,
			LightRotationDegrees = -12.0f,
			CanvasTint = new Color(1.0f, 0.95f, 0.84f, 1.0f),
			LightColor = new Color(1.0f, 0.92f, 0.67f, 1.0f),
			SkyOverlayColor = new Color(0.93f, 0.77f, 0.45f, 0.0f),
			FarFogColor = new Color(0.88f, 0.75f, 0.48f, 0.0f),
			NearFogColor = new Color(0.78f, 0.63f, 0.35f, 0.0f),
			LightOverlayColor = new Color(1.0f, 0.92f, 0.62f, 0.0f),
			HeatHazeColor = new Color(0.99f, 0.88f, 0.56f, 0.0f),
			Variants = new[] { WeatherVariant.Calm, WeatherVariant.DustStorm, WeatherVariant.Heatwave },
			VariantWeights = new[] { 0.36f, 0.4f, 0.24f }
		};
	}

	private static BiomeVisualProfile CreateSnowProfile(float brightExposure)
	{
		return new BiomeVisualProfile
		{
			Tonemap = Godot.Environment.ToneMapper.Linear,
			Exposure = brightExposure,
			Brightness = 0.95f,
			Contrast = 1.04f,
			Saturation = 0.88f,
			GlowBloom = 0.04f,
			LightEnergy = 0.68f,
			LightRotationDegrees = -18.0f,
			CanvasTint = new Color(0.94f, 0.98f, 1.0f, 1.0f),
			LightColor = new Color(0.84f, 0.91f, 1.0f, 1.0f),
			SkyOverlayColor = new Color(0.72f, 0.84f, 0.96f, 0.0f),
			FarFogColor = new Color(0.84f, 0.9f, 0.95f, 0.0f),
			NearFogColor = new Color(0.74f, 0.82f, 0.88f, 0.0f),
			LightOverlayColor = new Color(0.89f, 0.94f, 1.0f, 0.0f),
			HeatHazeColor = new Color(0.0f, 0.0f, 0.0f, 0.0f),
			Variants = new[] { WeatherVariant.Calm, WeatherVariant.Snowfall, WeatherVariant.Fog },
			VariantWeights = new[] { 0.42f, 0.38f, 0.2f }
		};
	}
}
