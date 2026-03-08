using Godot;

namespace projecthorizonscs.PhysicItem;

public partial class PhysicItem : Node2D
{
	private Sprite2D _sprite;
	private Label _label;

	private float _collectDistance = 40f;

	private bool _canCollect;

	private int _tickCounter;
	private int _ticksToLive = 300;

	[Export]
	public Items.Item Item;

	public override void _Ready()
	{
		_sprite = GetNode<Sprite2D>("Sprite");
		_label = GetNode<Label>("Label");
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
		UpdateCanCollect();
		if (!Input.IsActionJustPressed("collect") || !_canCollect) return;
		Autoload.Globals.I.LocalItemsDisplay.EquipItem(Item.ItemType,Item);
		QueueFree();
	}

	private void UpdateCanCollect()
	{
		if (Autoload.Globals.I.LocalPlayer == null) return;
		var distanceToPlayer = Autoload.Globals.I.LocalPlayer.GlobalPosition.DistanceTo(GlobalPosition);
		if (distanceToPlayer <= _collectDistance)
		{
			_canCollect = true;
			_label.Visible = true;
		}
		else
		{
			_label.Visible = false;
			_canCollect = false;
		}
	}

	private void UpdateData()
	{
		_sprite.Texture = Item.ItemTexture;
	}
}