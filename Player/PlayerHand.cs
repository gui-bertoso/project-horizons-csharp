using Godot;

namespace projecthorizonscs.Player;

[Tool]
public partial class PlayerHand : Marker2D
{
	[Export]
	public bool setItem;
	[Export]
	public bool cleanItem;

	private Item _equippedItem;
	private Node _equippedItemReference;
	private Node2D _handOffset;
	private Node2D _body;

	public override void _Ready()
	{
		Autoload.Globals.I.LocalPlayerHand = this;
		_handOffset = (Node2D)GetParent().GetParent().GetNode("Node2D");
		_body = (Node2D)GetParent().GetParent().GetNode("Body");
	}

	public override void _Process(double delta)
	{
/*
		if (setItem) setItem = false; EquipItem((Item)ResourceLoader.Load("res://Items/BrokenSword.tres"));
		if (cleanItem) cleanItem = false; ClearEquippedItem();

		return;
*/
		var mousePosition = GetGlobalMousePosition();
		var mouseAngle = (mousePosition - GlobalPosition).Normalized();
		_handOffset.Rotation = mouseAngle.Angle();
		Scale = _handOffset.Rotation is < -1f or > 1.5f ? new Vector2(-1f, 1f) : new Vector2(1f, 1f);
		_body.Scale = _handOffset.Rotation is < -1f or > 1.5f ? new Vector2(-1f, 1f) : new Vector2(1f, 1f);
		GD.Print(_handOffset.Rotation);
	}

	public void EquipItem(Item item)
	{
		ClearEquippedItem();
		_equippedItem = item;
		if (_equippedItem.ItemScene != null)
		{
			_equippedItemReference = _equippedItem.ItemScene.Instantiate();
		}
		else
		{
			_equippedItemReference = new Sprite2D();
			((Sprite2D)_equippedItemReference).Texture = _equippedItem.ItemTexture;
		}

		AddChild(_equippedItemReference);
	}

	private void ClearEquippedItem()
	{
		for (var i = 0; i >= GetChildCount(); i++)
		{
			var child = GetChild(i);
			if (child != _equippedItemReference) continue;
			child.QueueFree();
			break;
		}
		_equippedItem = null;
		_equippedItemReference = null;
	}

	private void UnequipItem()
	{
		var droppedItem = (PhysicItem)Autoload.Globals.I.PhysicItemScene.Instantiate();
		droppedItem.GlobalPosition = GlobalPosition;
		droppedItem.Item = _equippedItem;
		ClearEquippedItem();
		GetTree().CurrentScene.AddChild(droppedItem);
	}
}
