using Godot;

namespace projecthorizonscs.Player;

public partial class PlayerHand : Marker2D
{
	private Item equippedItem;
	private Node2D _equippedItemReference;
	private Node2D _handOffset;
	private Node2D _body;

	public override void _Ready()
	{
		Autoload.Globals.I.LocalPlayerHand = this;
		_handOffset = (Node2D)GetParent().GetParent().GetNode("Node2D");
		_body = (Node2D)GetParent();
	}

	public override void _Process(double delta)
	{
		/*
		var mousePosition = GetGlobalMousePosition();
		var mouseAngle = (mousePosition - GlobalPosition).Normalized();
		_handOffset.Rotation = mouseAngle.Angle();
		Scale = _handOffset.Rotation is < -1f or > 1.5f ? new Vector2(-1f, 1f) : new Vector2(1f, 1f);
		_body.Scale = _handOffset.Rotation is < -1f or > 1.5f ? new Vector2(-1f, 1f) : new Vector2(1f, 1f);
		GD.Print(_handOffset.Rotation);
		*/
	}

	public void ActionCurrentWeapon()
	{
		if (equippedItem==null) return;
		if (GetWeaponClass() == "ranged")
		{
			((Weapon)_equippedItemReference).Action();
		}
		GD.Print("Actionnnnn 22222");
	}

	public void EquipItem(Item item)
	{
		ClearEquippedItem();
		equippedItem = item;
		if (equippedItem.ItemScene != null)
		{
            _equippedItemReference = equippedItem.ItemScene.Instantiate<Node2D>();
		}
		else
		{
			_equippedItemReference = new Sprite2D();
			((Sprite2D)_equippedItemReference).Texture = equippedItem.ItemTexture;
			((Sprite2D)_equippedItemReference).Offset = new Vector2(0, -(equippedItem.ItemTexture.GetSize().Y/2)+4);
		}

		AddChild(_equippedItemReference);
	}

	private void ClearEquippedItem()
	{
		foreach (var i in GetChildren())
		{
			i.QueueFree();
		}
		equippedItem = null;
		_equippedItemReference = null;
	}

	private void UnequipItem()
	{
		var droppedItem = (PhysicItem)Autoload.Globals.I.PhysicItemScene.Instantiate();
		droppedItem.GlobalPosition = GlobalPosition;
		droppedItem.Item = equippedItem;
		ClearEquippedItem();
		GetTree().CurrentScene.AddChild(droppedItem);
	}

	public void EnableWeaponAttackArea()
	{
		((PhysicWeapon)_equippedItemReference).EnableAttackArea();
	}

	public void DisableWeaponAttackArea()
	{
		((PhysicWeapon)_equippedItemReference).DisableAttackArea();
	}

	public bool HasWeaponEquipped()
	{
		return equippedItem!=null;
	}

	public string GetWeaponClass()
	{
		if (equippedItem==null) return "hand";
		switch (equippedItem.ItemClass)
		{
			case Item.ITEM_CLASS.Ranged: return  "ranged";
			case Item.ITEM_CLASS.Mellee: return  "sword";
		}
		return "hand";
	}
 }
