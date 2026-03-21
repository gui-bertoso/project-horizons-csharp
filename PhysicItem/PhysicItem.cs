using Godot;

namespace projecthorizonscs;

public partial class PhysicItem : Node2D
{
	private Sprite2D _sprite;
	private Label _label;

	private float _collectDistance = 40f;

	private bool _canCollect;

	private int _tickCounter;
	private int _ticksToLive = 300;

	private bool collected = false;

	[Export]
	public Item Item;

	public override void _Ready()
	{
		GD.Print("Item here");
		GD.Print($"ITEM {GlobalPosition}");
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
		Autoload.Globals.I.LocalPlayer.CollectItem(this);
		GD.Print("Collect 1");
	}

	public void Collect()
	{
		GD.Print("Collect 5");
		Autoload.Globals.I.LocalItemsDisplay.EquipItem(Item.ItemType,Item);
		QueueFree();
	}

	private void UpdateCanCollect()
	{
		if (Autoload.Globals.I.LocalPlayer == null){
			GD.Print("dont have player");
			return;
		}
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