using Godot;

namespace projecthorizonscs;

public partial class PhysicItem : Node2D
{
    [Signal]
    public delegate void CollectedEventHandler();

    private Sprite2D _sprite;
    private Label _label;

    private const float CollectDistance = 40f;
    private bool _canCollect;
    private bool _collected = false;

    [Export]
    public Item Item;

    public override void _Ready()
    {
        _sprite = GetNodeOrNull<Sprite2D>("Sprite");
        _label = GetNodeOrNull<Label>("Label");

        if (_label != null)
            _label.Visible = false;

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

        if (!Input.IsActionJustPressed("collect") || !_canCollect)
            return;

        Autoload.Globals.I.LocalPlayer?.CollectItem(this);
    }

    public void Collect()
    {
        if (_collected)
            return;

        _collected = true;

        Autoload.Globals.I.LocalItemsDisplay.EquipItem(Item.ItemType, Item);

        EmitSignal(SignalName.Collected);
        QueueFree();
    }

    private void UpdateCanCollect()
    {
        if (Autoload.Globals.I.LocalPlayer == null)
        {
            _canCollect = false;

            if (_label != null)
                _label.Visible = false;

            return;
        }

        float distanceToPlayer =
            Autoload.Globals.I.LocalPlayer.GlobalPosition.DistanceTo(GlobalPosition);

        if (distanceToPlayer <= CollectDistance)
        {
            _canCollect = true;

            if (_label != null)
                _label.Visible = true;
        }
        else
        {
            _canCollect = false;

            if (_label != null)
                _label.Visible = false;
        }
    }

    private void UpdateData()
    {
        if (_sprite != null && Item != null)
            _sprite.Texture = Item.ItemTexture;
    }
}