using Godot;
using System;
using System.Collections.Generic;

[Tool]
public partial class EmeraldLevelGenerator : Node2D
{
	[ExportGroup("Editor Actions")]
	[Export]
	public bool GenerateInEditor
	{
		get => false;
		set
		{
			if (!value)
				return;

			if (Engine.IsEditorHint())
				Generate();
		}
	}

	[Export]
	public bool ClearInEditor
	{
		get => false;
		set
		{
			if (!value)
				return;

			if (Engine.IsEditorHint())
				ClearAll();
		}
	}

	[ExportGroup("Void / Border")]
	[Export] public bool GenerateVoidBorder = true;
	[Export] public int VoidBorderThickness = 2;
	[Export] public Vector2I VoidAtlas = new(0, 5);

	[ExportGroup("Beach")]
	[Export] public bool GenerateBeach = true;
	[Export] public float BeachChance = 0.45f;
	[Export] public int BeachBorderDistance = 2;
	[Export] public Vector2I SandAtlas = new(1, 5);

	[ExportGroup("Lakes")]
	[Export] public bool GenerateLakes = true;
	[Export] public float LakeNoiseFrequency = 0.030f;
	[Export] public float LakeThreshold = 0.73f;
	[Export] public int LakeEdgeDepth = 1;
	[Export] public Vector2I WaterAtlas = new(2, 5);
	[Export] public Vector2I DeepWaterAtlas = new(3, 5);

	[ExportGroup("TileMap Layers")]
	[Export] public NodePath GroundPath = "Ground";
	[Export] public NodePath DetailsSmallPath = "DetailsSmall";
	[Export] public NodePath DetailsMediumPath = "DetailsMedium";
	[Export] public NodePath ObjectsPath = "Objects";
	[Export] public NodePath ShadowsPath = "Shadows";

	[ExportGroup("Level Size")]
	[Export] public int LevelSizeX = 220;
	[Export] public int LevelSizeY = 140;

	[ExportGroup("General")]
	[Export] public int SeedValue = 12345;
	[Export] public bool GenerateOnReady = false;
	[Export] public bool ClearBeforeGenerate = true;

	[ExportGroup("Ground Noise")]
	[Export] public float GroundFrequency = 0.035f;
	[Export] public FastNoiseLite.NoiseTypeEnum GroundNoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
	[Export] public float GroundThreshold = 0.40f;

	[ExportGroup("Shape / Island")]
	[Export] public bool UseIslandMask = false;
	[Export] public float IslandRoundness = 1.15f;
	[Export] public float IslandFalloffPower = 1.85f;

	[ExportGroup("Density Noise")]
	[Export] public float DensityFrequency = 0.010f;
	[Export] public FastNoiseLite.NoiseTypeEnum DensityNoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;

	[ExportGroup("Small Details Spawn")]
	[Export] public bool GenerateSmallDetails = true;
	[Export] public float SmallDetailBaseChance = 0.44f;
	[Export] public float SmallDetailDensityChance = 0.28f;

	[ExportGroup("Medium Details Spawn")]
	[Export] public bool GenerateMediumDetails = true;
	[Export] public float MediumDetailsFrequency = 0.08f;
	[Export] public FastNoiseLite.NoiseTypeEnum MediumDetailsNoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
	[Export] public float MediumDetailBaseChance = 0.004f;
	[Export] public float MediumDetailDensityChance = 0.012f;
	[Export] public float MediumDetailRegionChance = 0.010f;

	[ExportGroup("Trees")]
	[Export] public bool GenerateTrees = true;
	[Export] public int TreeStep = 3;
	[Export] public int TreeMinDistance = 4;
	[Export] public int TreePaddingCheckRadius = 2;
	[Export] public float TreeNoiseFrequency = 0.045f;
	[Export] public FastNoiseLite.NoiseTypeEnum TreeNoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
	[Export] public float TreeThreshold = 0.53f;
	[Export] public float TreeDensityInfluence = 0.24f;
	[Export] public float TreeBaseChanceBonus = 0.01f;
	[Export] public int TreeRandomOffsetX = 1;
	[Export] public int TreeRandomOffsetY = 1;

	[ExportGroup("Atlas - Ground")]
	[Export] public Vector2I GroundMainAtlas = new(0, 0);
	[Export] public Vector2I GroundAlt1Atlas = new(1, 0);
	[Export] public Vector2I GroundAlt2Atlas = new(2, 0);
	[Export] public float GroundAlt1Threshold = 0.60f;
	[Export] public float GroundAlt2Threshold = 0.78f;

	[ExportGroup("Edge Decorations")]
	[Export] public bool GenerateEdgeDecorations = true;
	[Export] public float EdgeDecorChance = 0.16f;
	[Export] public int EdgeDecorRadius = 1;
	[Export] public Godot.Collections.Array<Vector2I> EdgeDecorAtlases = new()
	{
		new Vector2I(6, 2),
		new Vector2I(7, 2),
		new Vector2I(8, 2),
	};

	[ExportGroup("Atlas - Small Details")]
	[Export] public Godot.Collections.Array<Vector2I> SmallDetailAtlases = new()
	{
		new Vector2I(0, 1),
		new Vector2I(1, 1),
		new Vector2I(2, 1),
		new Vector2I(3, 1),
		new Vector2I(4, 1),
		new Vector2I(5, 1),
	};

	[ExportGroup("Atlas - Medium Details")]
	[Export] public Godot.Collections.Array<Vector2I> MediumDetailAtlases = new()
	{
		new Vector2I(0, 2),
		new Vector2I(1, 2),
		new Vector2I(2, 2),
		new Vector2I(3, 2),
		new Vector2I(4, 2),
	};

	[ExportGroup("Atlas - Trees")]
	[Export] public Godot.Collections.Array<Vector2I> TreeAtlases = new()
	{
		new Vector2I(0, 3),
		new Vector2I(1, 3),
		new Vector2I(2, 3),
	};

	[ExportGroup("Ground Context")]
	[Export] public Godot.Collections.Array<Vector2I> DirtAtlases = new()
	{
		new Vector2I(10, 0),
		new Vector2I(11, 0)
	};

	[ExportGroup("Atlas - Shadows")]
	[Export] public Vector2I TreeShadowAtlas = new(0, 4);
	[Export] public Vector2I ShadowOffset = new(1, 1);

	private TileMapLayer _ground;
	private TileMapLayer _detailsSmall;
	private TileMapLayer _detailsMedium;
	private TileMapLayer _objects;
	private TileMapLayer _shadows;

	private FastNoiseLite _groundNoise;
	private FastNoiseLite _densityNoise;
	private FastNoiseLite _mediumDetailsNoise;
	private FastNoiseLite _treeNoise;
	private FastNoiseLite _lakeNoise;

	private readonly List<Vector2I> _treePositions = new();

	public override void _Ready()
	{
		TryGetLayers();

		if (!Engine.IsEditorHint() && GenerateOnReady)
			Generate();
	}

	private bool TryGetLayers()
	{
		_ground = GetNodeOrNull<TileMapLayer>(GroundPath);
		_detailsSmall = GetNodeOrNull<TileMapLayer>(DetailsSmallPath);
		_detailsMedium = GetNodeOrNull<TileMapLayer>(DetailsMediumPath);
		_objects = GetNodeOrNull<TileMapLayer>(ObjectsPath);
		_shadows = GetNodeOrNull<TileMapLayer>(ShadowsPath);

		return _ground != null
			&& _detailsSmall != null
			&& _detailsMedium != null
			&& _objects != null
			&& _shadows != null;
	}

	public void Generate()
	{
		if (!TryGetLayers())
		{
			GD.Print("EmeraldGen: layers not found.");
			return;
		}

		SetupNoises();

		if (ClearBeforeGenerate)
			ClearAll();

		_treePositions.Clear();

		GenerateGroundPass();

		if (GenerateLakes)
			GenerateLakesPass();

		if (GenerateBeach)
			GenerateBeachPass();

		if (GenerateVoidBorder)
			GenerateVoidBorderPass();

		if (GenerateSmallDetails)
			GenerateSmallDetailsPass();

		if (GenerateMediumDetails)
			GenerateMediumDetailsPass();

		if (GenerateEdgeDecorations)
			GenerateEdgeDecorationsPass();

		if (GenerateTrees)
			GenerateTreesPass();
	}

	public void ClearAll()
	{
		if (!TryGetLayers())
			return;

		_ground.Clear();
		_detailsSmall.Clear();
		_detailsMedium.Clear();
		_objects.Clear();
		_shadows.Clear();
	}

	private void SetupNoises()
	{
		_groundNoise = new FastNoiseLite();
		_groundNoise.Seed = SeedValue;
		_groundNoise.NoiseType = GroundNoiseType;
		_groundNoise.Frequency = GroundFrequency;

		_densityNoise = new FastNoiseLite();
		_densityNoise.Seed = SeedValue + 1000;
		_densityNoise.NoiseType = DensityNoiseType;
		_densityNoise.Frequency = DensityFrequency;

		_mediumDetailsNoise = new FastNoiseLite();
		_mediumDetailsNoise.Seed = SeedValue + 3000;
		_mediumDetailsNoise.NoiseType = MediumDetailsNoiseType;
		_mediumDetailsNoise.Frequency = MediumDetailsFrequency;

		_treeNoise = new FastNoiseLite();
		_treeNoise.Seed = SeedValue + 4000;
		_treeNoise.NoiseType = TreeNoiseType;
		_treeNoise.Frequency = TreeNoiseFrequency;

		_lakeNoise = new FastNoiseLite();
		_lakeNoise.Seed = SeedValue + 5000;
		_lakeNoise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
		_lakeNoise.Frequency = LakeNoiseFrequency;
	}

	private void GenerateGroundPass()
	{
		int halfX = LevelSizeX / 2;
		int halfY = LevelSizeY / 2;

		for (int y = -halfY; y < halfY; y++)
		{
			for (int x = -halfX; x < halfX; x++)
			{
				Vector2I cell = new(x, y);

				if (!IsGroundCell(cell))
					continue;

				float groundValue = GetNoise01(_groundNoise, x, y);
				Vector2I atlas = PickGroundAtlas(groundValue);

				_ground.SetCell(cell, 0, atlas);
			}
		}
	}

	private void GenerateBeachPass()
	{
		int halfX = LevelSizeX / 2;
		int halfY = LevelSizeY / 2;

		List<Vector2I> cellsToSand = new();

		for (int y = -halfY; y < halfY; y++)
		{
			for (int x = -halfX; x < halfX; x++)
			{
				Vector2I cell = new(x, y);

				// só transforma chão normal/terra em areia
				if (!IsGroundFamilyCell(cell) && !IsDirtCell(cell))
					continue;

				// só se estiver perto de água
				if (!IsNearWater(cell, BeachBorderDistance))
					continue;

				float roll = RandomFloat01FromPosition(x, y, 6060);

				if (roll > BeachChance)
					continue;

				cellsToSand.Add(cell);
			}
		}

		foreach (Vector2I cell in cellsToSand)
		{
			_ground.SetCell(cell, 0, SandAtlas);
		}
	}

	private void GenerateLakesPass()
	{
		int halfX = LevelSizeX / 2;
		int halfY = LevelSizeY / 2;

		for (int y = -halfY; y < halfY; y++)
		{
			for (int x = -halfX; x < halfX; x++)
			{
				Vector2I cell = new(x, y);

				if (!IsGroundFamilyCell(cell))
					continue;

				if (IsBorderRealTerrainCell(cell, 3))
					continue;

				float lakeNoise = GetNoise01(_lakeNoise, x, y);

				if (lakeNoise < LakeThreshold)
					continue;

				bool nearNonLakeGround = false;

				for (int oy = -LakeEdgeDepth; oy <= LakeEdgeDepth; oy++)
				{
					for (int ox = -LakeEdgeDepth; ox <= LakeEdgeDepth; ox++)
					{
						Vector2I check = cell + new Vector2I(ox, oy);

						if (!IsInsideBounds(check))
						{
							nearNonLakeGround = true;
							break;
						}

						if (!IsGroundFamilyCell(check))
						{
							nearNonLakeGround = true;
							break;
						}

						float otherLakeNoise = GetNoise01(_lakeNoise, check.X, check.Y);
						if (otherLakeNoise < LakeThreshold)
						{
							nearNonLakeGround = true;
							break;
						}
					}

					if (nearNonLakeGround)
						break;
				}

				_ground.SetCell(cell, 0, nearNonLakeGround ? WaterAtlas : DeepWaterAtlas);
			}
		}
	}

	private void GenerateVoidBorderPass()
	{
		int halfX = LevelSizeX / 2;
		int halfY = LevelSizeY / 2;

		for (int y = -halfY; y < halfY; y++)
		{
			for (int x = -halfX; x < halfX; x++)
			{
				Vector2I cell = new(x, y);

				if (IsRealTerrainCell(cell))
					continue;

				if (!HasNeighborRealTerrain(cell, VoidBorderThickness))
					continue;

				_ground.SetCell(cell, 0, VoidAtlas);
			}
		}
	}

	private void GenerateSmallDetailsPass()
	{
		if (SmallDetailAtlases.Count == 0)
			return;

		int halfX = LevelSizeX / 2;
		int halfY = LevelSizeY / 2;

		for (int y = -halfY; y < halfY; y++)
		{
			for (int x = -halfX; x < halfX; x++)
			{
				Vector2I cell = new(x, y);

				if (!IsGroundMainCell(cell))
					continue;

				if (_detailsMedium.GetCellSourceId(cell) != -1 || _objects.GetCellSourceId(cell) != -1)
					continue;

				float density = GetNoise01(_densityNoise, x, y);
				float spawnChance = SmallDetailBaseChance + (density * SmallDetailDensityChance);
				float roll = RandomFloat01FromPosition(x, y, 5551);

				if (roll > spawnChance)
					continue;

				Vector2I atlas = PickAtlasFromList(SmallDetailAtlases, x, y, 7777);
				_detailsSmall.SetCell(cell, 0, atlas);
			}
		}
	}

	private void GenerateMediumDetailsPass()
	{
		if (MediumDetailAtlases.Count == 0)
			return;

		int halfX = LevelSizeX / 2;
		int halfY = LevelSizeY / 2;

		for (int y = -halfY; y < halfY; y++)
		{
			for (int x = -halfX; x < halfX; x++)
			{
				Vector2I cell = new(x, y);

				if (!IsGroundMainCell(cell))
					continue;

				if (_objects.GetCellSourceId(cell) != -1)
					continue;

				float density = GetNoise01(_densityNoise, x, y);
				float region = GetNoise01(_mediumDetailsNoise, x, y);

				float spawnChance =
					MediumDetailBaseChance +
					(density * MediumDetailDensityChance) +
					(region * MediumDetailRegionChance);

				float roll = RandomFloat01FromPosition(x, y, 9127);

				if (roll > spawnChance)
					continue;

				if (HasNeighborMediumDetail(cell, 1))
					continue;

				Vector2I atlas = PickAtlasFromList(MediumDetailAtlases, x, y, 9999);
				_detailsMedium.SetCell(cell, 0, atlas);
			}
		}
	}

	private bool IsNearWater(Vector2I cell, int radius = 1)
	{
		for (int y = -radius; y <= radius; y++)
		{
			for (int x = -radius; x <= radius; x++)
			{
				if (x == 0 && y == 0)
					continue;

				Vector2I check = cell + new Vector2I(x, y);

				if (!IsInsideBounds(check))
					continue;

				if (!HasGround(check))
					continue;

				Vector2I atlas = _ground.GetCellAtlasCoords(check);

				if (atlas == WaterAtlas || atlas == DeepWaterAtlas)
					return true;
			}
		}

		return false;
	}

	private void GenerateEdgeDecorationsPass()
	{
		if (!GenerateEdgeDecorations || EdgeDecorAtlases.Count == 0)
			return;

		int halfX = LevelSizeX / 2;
		int halfY = LevelSizeY / 2;

		for (int y = -halfY; y < halfY; y++)
		{
			for (int x = -halfX; x < halfX; x++)
			{
				Vector2I cell = new(x, y);

				if (!IsGroundMainCell(cell))
					continue;

				if (!IsNearDirt(cell, EdgeDecorRadius))
					continue;

				if (_objects.GetCellSourceId(cell) != -1)
					continue;

				if (_detailsMedium.GetCellSourceId(cell) != -1)
					continue;

				float roll = RandomFloat01FromPosition(x, y, 45454);

				if (roll > EdgeDecorChance)
					continue;

				Vector2I atlas = PickAtlasFromList(EdgeDecorAtlases, x, y, 56565);
				_detailsMedium.SetCell(cell, 0, atlas);
			}
		}
	}

	private void GenerateTreesPass()
	{
		if (TreeAtlases.Count == 0)
			return;

		int halfX = LevelSizeX / 2;
		int halfY = LevelSizeY / 2;
		int safeTreeStep = Mathf.Max(1, TreeStep);

		for (int y = -halfY; y < halfY; y += safeTreeStep)
		{
			for (int x = -halfX; x < halfX; x += safeTreeStep)
			{
				Vector2I baseCell = new(x, y);
				Vector2I offsetCell = ApplyTreeRandomOffset(baseCell);

				if (!HasEnoughSpaceForTree(offsetCell))
					continue;

				if (!IsGroundMainCell(offsetCell))
					continue;

				float density = GetNoise01(_densityNoise, offsetCell.X, offsetCell.Y);
				float noise = GetNoise01(_treeNoise, offsetCell.X, offsetCell.Y);
				float roll = RandomFloat01FromPosition(offsetCell.X, offsetCell.Y, 17171);

				float spawnChance = (1f - TreeThreshold) + (density * TreeDensityInfluence) + TreeBaseChanceBonus;

				if (noise < TreeThreshold && roll > spawnChance)
					continue;

				if (IsNearAnotherTree(offsetCell, TreeMinDistance))
					continue;

				Vector2I treeAtlas = PickAtlasFromList(TreeAtlases, offsetCell.X, offsetCell.Y, 22222);

				_objects.SetCell(offsetCell, 0, treeAtlas);
				_shadows.SetCell(offsetCell + ShadowOffset, 0, TreeShadowAtlas);

				_treePositions.Add(offsetCell);
			}
		}
	}

	private bool IsGroundCell(Vector2I cell)
	{
		float groundValue = GetNoise01(_groundNoise, cell.X, cell.Y);

		if (UseIslandMask)
		{
			float mask = GetIslandMask01(cell.X, cell.Y);
			groundValue *= mask;
		}

		return groundValue >= GroundThreshold;
	}

	private bool HasGround(Vector2I cell)
	{
		return _ground.GetCellSourceId(cell) != -1;
	}

	private bool IsGroundMainCell(Vector2I cell)
	{
		if (!HasGround(cell))
			return false;

		return _ground.GetCellAtlasCoords(cell) == GroundMainAtlas;
	}

	private bool IsDirtCell(Vector2I cell)
	{
		if (!HasGround(cell))
			return false;

		Vector2I atlas = _ground.GetCellAtlasCoords(cell);

		foreach (Vector2I dirtAtlas in DirtAtlases)
		{
			if (atlas == dirtAtlas)
				return true;
		}

		return false;
	}

	private bool IsGroundFamilyCell(Vector2I cell)
	{
		if (!HasGround(cell))
			return false;

		Vector2I atlas = _ground.GetCellAtlasCoords(cell);

		return atlas == GroundMainAtlas
			|| atlas == GroundAlt1Atlas
			|| atlas == GroundAlt2Atlas;
	}

	private bool IsRealTerrainCell(Vector2I cell)
	{
		if (!HasGround(cell))
			return false;

		Vector2I atlas = _ground.GetCellAtlasCoords(cell);

		if (atlas == GroundMainAtlas || atlas == GroundAlt1Atlas || atlas == GroundAlt2Atlas)
			return true;

		if (atlas == SandAtlas || atlas == WaterAtlas || atlas == DeepWaterAtlas || atlas == VoidAtlas)
			return true;

		foreach (Vector2I dirtAtlas in DirtAtlases)
		{
			if (atlas == dirtAtlas)
				return true;
		}

		return false;
	}

	private bool IsNearDirt(Vector2I cell, int radius = 1)
	{
		for (int y = -radius; y <= radius; y++)
		{
			for (int x = -radius; x <= radius; x++)
			{
				if (x == 0 && y == 0)
					continue;

				Vector2I check = cell + new Vector2I(x, y);

				if (!IsInsideBounds(check))
					continue;

				if (IsDirtCell(check))
					return true;
			}
		}

		return false;
	}

	private bool IsBorderRealTerrainCell(Vector2I cell, int radius = 1)
	{
		if (!IsRealTerrainCell(cell))
			return false;

		for (int y = -radius; y <= radius; y++)
		{
			for (int x = -radius; x <= radius; x++)
			{
				Vector2I check = cell + new Vector2I(x, y);

				if (!IsInsideBounds(check))
					return true;

				if (!IsRealTerrainCell(check))
					return true;
			}
		}

		return false;
	}

	private bool HasNeighborRealTerrain(Vector2I cell, int radius = 1)
	{
		for (int y = -radius; y <= radius; y++)
		{
			for (int x = -radius; x <= radius; x++)
			{
				if (x == 0 && y == 0)
					continue;

				Vector2I check = cell + new Vector2I(x, y);

				if (!IsInsideBounds(check))
					continue;

				if (IsRealTerrainCell(check) && _ground.GetCellAtlasCoords(check) != VoidAtlas)
					return true;
			}
		}

		return false;
	}

	private bool HasEnoughSpaceForTree(Vector2I centerCell)
	{
		for (int y = -TreePaddingCheckRadius; y <= TreePaddingCheckRadius; y++)
		{
			for (int x = -TreePaddingCheckRadius; x <= TreePaddingCheckRadius; x++)
			{
				Vector2I cell = centerCell + new Vector2I(x, y);

				if (!IsInsideBounds(cell))
					return false;

				if (!IsGroundMainCell(cell))
					return false;
			}
		}

		return true;
	}

	private bool IsNearAnotherTree(Vector2I cell, int minDistance)
	{
		foreach (Vector2I treePos in _treePositions)
		{
			if (cell.DistanceTo(treePos) < minDistance)
				return true;
		}

		return false;
	}

	private bool HasNeighborMediumDetail(Vector2I cell, int radius = 1)
	{
		for (int y = -radius; y <= radius; y++)
		{
			for (int x = -radius; x <= radius; x++)
			{
				if (x == 0 && y == 0)
					continue;

				Vector2I checkCell = cell + new Vector2I(x, y);

				if (_detailsMedium.GetCellSourceId(checkCell) != -1)
					return true;
			}
		}

		return false;
	}

	private bool IsInsideBounds(Vector2I cell)
	{
		int halfX = LevelSizeX / 2;
		int halfY = LevelSizeY / 2;

		return cell.X >= -halfX && cell.X < halfX && cell.Y >= -halfY && cell.Y < halfY;
	}

	private Vector2I ApplyTreeRandomOffset(Vector2I cell)
	{
		int offsetX = RandomRangeFromPosition(cell.X, cell.Y, SeedValue + 7000, -TreeRandomOffsetX, TreeRandomOffsetX);
		int offsetY = RandomRangeFromPosition(cell.X, cell.Y, SeedValue + 8000, -TreeRandomOffsetY, TreeRandomOffsetY);
		return cell + new Vector2I(offsetX, offsetY);
	}

	private Vector2I PickGroundAtlas(float groundValue)
	{
		if (groundValue >= GroundAlt2Threshold)
			return GroundAlt2Atlas;

		if (groundValue >= GroundAlt1Threshold)
			return GroundAlt1Atlas;

		return GroundMainAtlas;
	}

	private Vector2I PickAtlasFromList(Godot.Collections.Array<Vector2I> atlases, int x, int y, int salt)
	{
		if (atlases.Count == 0)
			return Vector2I.Zero;

		int index = RandomRangeFromPosition(x, y, salt, 0, atlases.Count - 1);
		return atlases[index];
	}

	private int RandomRangeFromPosition(int x, int y, int salt, int min, int max)
	{
		if (max <= min)
			return min;

		int hash = x * 73856093 ^ y * 19349663 ^ salt;
		hash = Math.Abs(hash);

		return min + (hash % (max - min + 1));
	}

	private float RandomFloat01FromPosition(int x, int y, int salt)
	{
		int hash = x * 73856093 ^ y * 19349663 ^ salt;
		hash = Math.Abs(hash);
		return (hash % 10000) / 10000.0f;
	}

	private float GetNoise01(FastNoiseLite noise, int x, int y)
	{
		return (noise.GetNoise2D(x, y) + 1f) * 0.5f;
	}

	private float GetIslandMask01(int x, int y)
	{
		float nx = (float)x / Mathf.Max(1f, LevelSizeX * 0.5f);
		float ny = (float)y / Mathf.Max(1f, LevelSizeY * 0.5f);

		float dist = Mathf.Pow(Mathf.Abs(nx), IslandRoundness) + Mathf.Pow(Mathf.Abs(ny), IslandRoundness);
		dist = Mathf.Clamp(dist, 0f, 1.5f);

		float mask = 1f - Mathf.Pow(Mathf.Clamp(dist, 0f, 1f), IslandFalloffPower);
		return Mathf.Clamp(mask, 0f, 1f);
	}
}