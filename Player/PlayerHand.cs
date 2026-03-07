using Godot;
using System;
using System.Runtime.Serialization.Formatters;

public partial class PlayerHand : Marker2D
{
	Item EquippedItem;
	Node EquippedItemReference;

    public override void _Ready()
    {
        Globals.I.LocalPlayerHand = this;
    }

	public void EquipItem(Item item)
	{
		EquippedItem = item;
		if (EquippedItem.ItemScene != null)
		{
			EquippedItemReference = EquippedItem.ItemScene.Instantiate();
			AddChild(EquippedItemReference);
		}
		else
		{
			EquippedItemReference = new TextureRect();
			((TextureRect)EquippedItemReference).Texture = EquippedItem.ItemTexture;
			AddChild(EquippedItemReference);
		}
	}

	public void ClearEquippedItem()
	{
		for (int i = 0; i < GetChildCount(); i++)
		{
			Node child = GetChild(i);
			if (child == EquippedItemReference)
			{
				child.QueueFree();
				break;
			}
		}
		EquippedItem = null;
		EquippedItemReference = null;
	}

	public void UnequipItem()
	{
		PhysicItem droppedItem = (PhysicItem)Globals.I.PhysicItemScene.Instantiate();
		droppedItem.GlobalPosition = GlobalPosition;
		droppedItem.Item = EquippedItem;
		ClearEquippedItem();
		GetTree().CurrentScene.AddChild(droppedItem);
	}
}
