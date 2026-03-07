using Godot;
using System;

public partial class PhysicItem : Node2D
{
	private Sprite2D _Sprite;
	private Label _Label;

	private float _CollectDistance = 40f;

	private bool CanCollect = false;

	private int _TickCounter = 0;
	private int _TicksToLive = 300;

	[Export]
	public Item Item;

	public override void _Ready()
	{
		_Sprite = GetNode<Sprite2D>("Sprite");
		_Label = GetNode<Label>("Label");
		if (Item != null)
		{
			UpdateData();
		}
		else
		{
			QueueFree();
		}
	}

    public override void _Process(double delta)
    {
		/*
		_TickCounter++;
		if (_TickCounter <= _TicksToLive)
		{
			return;
		}
		_TickCounter = 0;
		*/
        UpdateCanCollect();
		if (Input.IsActionJustPressed("collect") && CanCollect)
		{
			Globals.I.LocalItemsDisplay.EquipItem(Item.ItemType,Item);
			QueueFree();
		}
    }

	public void UpdateCanCollect()
	{
		if (Globals.I.LocalPlayer != null)
		{
			float distanceToPlayer = Globals.I.LocalPlayer.GlobalPosition.DistanceTo(GlobalPosition);
			if (distanceToPlayer <= _CollectDistance)
			{
				CanCollect = true;
				_Label.Visible = true;
			}
			else
			{
				_Label.Visible = false;
				CanCollect = false;
			}
		}
	}

	public void UpdateData()
	{
		_Sprite.Texture = Item.ItemTexture;
	}
}
