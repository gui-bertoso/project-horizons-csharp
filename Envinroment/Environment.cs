using Godot;
using System;

public partial class Environment : WorldEnvironment
{
	private CanvasModulate _BaseModulate;
	private Node2D _SnowBiomeEffects;

	public override void _Ready()
	{
		_BaseModulate = GetNode<CanvasModulate>("BaseModulate");
		_SnowBiomeEffects = GetNode<Node2D>("SnowBiomeEffects");
		SetToBiome();
	}

	public void SetToBiome()
	{
		if (Globals.I.LocalLevelGenerator == null)
		{
			_BaseModulate.Visible = true;
		}
		else
		{
			int biomeID = Globals.I.LocalLevelGenerator.LevelBiome_ID;
			switch (biomeID)
			{
				case 3:
					for (int i = 0; i < _SnowBiomeEffects.GetChildCount(); i++)
					{
						var child = _SnowBiomeEffects.GetChild(i);
						if (child is CanvasLayer)
						{
							((CanvasLayer)child).Visible = true;
							((GpuParticles2D)child.GetChild(0)).Amount = 16;
						}
						else
						{
							((Node2D)child).Visible = true;
						}
						_SnowBiomeEffects.Visible = true;
					}
					break;
				case 4:
					for (int i = 0; i < _SnowBiomeEffects.GetChildCount(); i++)
					{
						var child = _SnowBiomeEffects.GetChild(i);
						if (child is CanvasLayer)
						{
							((CanvasLayer)child).Visible = true;
							((GpuParticles2D)child.GetChild(0)).Amount = 42;
						}
						else
						{
							((Node2D)child).Visible = true;
						}
						_SnowBiomeEffects.Visible = true;
					}
					break;
				default: _BaseModulate.Visible = true; break;
			}
		}
	}
}
