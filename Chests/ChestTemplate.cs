using Godot;

namespace projecthorizonscs;

public partial class ChestDrop : Resource
{
    [Export]
    public int Chance = 1;

    [Export]
    public Item Item;

    public ChestDrop() { }

    public ChestDrop(int chance, Item item)
    {
        Chance = chance;
        Item = item;
    }
}

public partial class ChestTemplate : Node2D
{
    private Sprite2D _sprite;
    private Label _label;

    private const float OpenDistance = 40f;
    private bool _canOpen = false;

    private PhysicItem _spawnedItem;

    [Export]
    public bool IsOpened = false;

    public Godot.Collections.Array<ChestDrop> Drops = new();

    private static readonly PackedScene PhysicItemScene =
        ResourceLoader.Load<PackedScene>("uid://bqjq1yutufivv");

    public override void _Ready()
    {
        _sprite = GetNodeOrNull<Sprite2D>("Sprite");
        _label = GetNodeOrNull<Label>("Label");

        if (_label != null)
            _label.Visible = false;

        if (_sprite != null)
            _sprite.Frame = IsOpened ? 1 : 0;

        if (Drops == null || Drops.Count == 0)
            BuildDefaultDrops();

        GD.Print($"chest drops count: {Drops.Count}");

        if (IsOpened)
            DisableChestLogic();
    }

    public override void _Process(double delta)
    {
        if (IsOpened)
            return;

        UpdateCanOpen();

        if (!_canOpen)
            return;

        if (!Input.IsActionJustPressed("collect"))
            return;

        OpenChest();
    }

    private void BuildDefaultDrops()
    {
        Drops = new Godot.Collections.Array<ChestDrop>();

        AddDrop(10, "res://Items/AirBook.tres");
        AddDrop(10, "res://Items/Apple.tres");
        AddDrop(10, "res://Items/Armor0Foot.tres");
        AddDrop(10, "res://Items/Armor0Healmet.tres");
        AddDrop(10, "res://Items/Armor0Tome.tres");

        AddDrop(8, "res://Items/Armor1Foot.tres");
        AddDrop(8, "res://Items/Armor1Healmet.tres");
        AddDrop(8, "res://Items/Armor1Tome.tres");

        AddDrop(6, "res://Items/Armor2Foot.tres");
        AddDrop(6, "res://Items/Armor2Healmet.tres");
        AddDrop(6, "res://Items/Armor2Tome.tres");

        AddDrop(4, "res://Items/Armor3Foot.tres");
        AddDrop(4, "res://Items/Armor3Healmet.tres");
        AddDrop(4, "res://Items/Armor3Tome.tres");

        AddDrop(12, "res://Items/Branch.tres");
        AddDrop(8, "res://Items/BrokenSword.tres");
        AddDrop(7, "res://Items/BurnPotion.tres");

        AddDrop(2, "res://Items/DebugBook.tres");
        AddDrop(2, "res://Items/DebugBoomerang.tres");
        AddDrop(2, "res://Items/DebugBow.tres");
        AddDrop(2, "res://Items/DebugGun.tres");
        AddDrop(2, "res://Items/DebugHammer.tres");
        AddDrop(2, "res://Items/DebugRod.tres");
        AddDrop(2, "res://Items/DebugSpell.tres");
        AddDrop(2, "res://Items/DebugSword.tres");

        AddDrop(5, "res://Items/FireBook.tres");
        AddDrop(10, "res://Items/OldSword.tres");
        AddDrop(10, "res://Items/Orange.tres");
        AddDrop(14, "res://Items/Stick.tres");

        AddDrop(12, "res://Items/Sword0.tres");
        AddDrop(10, "res://Items/Sword1.tres");
        AddDrop(9, "res://Items/Sword2.tres");
        AddDrop(8, "res://Items/Sword3.tres");
        AddDrop(7, "res://Items/Sword4.tres");
        AddDrop(6, "res://Items/Sword5.tres");
        AddDrop(5, "res://Items/Sword6.tres");
        AddDrop(4, "res://Items/Sword7.tres");
        AddDrop(3, "res://Items/Sword8.tres");

        AddDrop(6, "res://Items/VenomPotion.tres");
    }

    private void AddDrop(int chance, string itemPath)
    {
        Item item = ResourceLoader.Load<Item>(itemPath);

        if (item == null)
        {
            GD.PrintErr($"falhou ao carregar item: {itemPath}");
            return;
        }

        Drops.Add(new ChestDrop(chance, item));
    }

    private void UpdateCanOpen()
    {
        if (Autoload.Globals.I.LocalPlayer == null)
        {
            _canOpen = false;

            if (_label != null)
                _label.Visible = false;

            return;
        }

        float distanceToPlayer =
            Autoload.Globals.I.LocalPlayer.GlobalPosition.DistanceTo(GlobalPosition);

        if (distanceToPlayer <= OpenDistance)
        {
            _canOpen = true;

            if (_label != null)
                _label.Visible = true;
        }
        else
        {
            _canOpen = false;

            if (_label != null)
                _label.Visible = false;
        }
    }

    public Item GetRandomDrop()
    {
        if (Drops == null || Drops.Count == 0)
        {
            GD.PrintErr("drops vazio ou null.");
            return null;
        }

        int totalChance = 0;

        foreach (ChestDrop drop in Drops)
        {
            GD.Print($"drop debug -> drop:{drop} item:{drop?.Item} chance:{drop?.Chance}");

            if (drop == null || drop.Item == null || drop.Chance <= 0)
                continue;

            totalChance += drop.Chance;
        }

        GD.Print($"total chance: {totalChance}");

        if (totalChance <= 0)
            return null;

        int roll = (int)(GD.Randi() % (uint)totalChance);
        int current = 0;

        foreach (ChestDrop drop in Drops)
        {
            if (drop == null || drop.Item == null || drop.Chance <= 0)
                continue;

            current += drop.Chance;

            if (roll < current)
                return drop.Item;
        }

        return null;
    }

    public void SpawnDroppedItem(Item item)
    {
        if (item == null)
            return;

        if (PhysicItemScene == null)
        {
            GD.PrintErr("erro: physicitem.tscn não carregou.");
            return;
        }

        PhysicItem physicItem = PhysicItemScene.Instantiate<PhysicItem>();
        physicItem.Item = item;

        GetParent().AddChild(physicItem);

        Vector2 offset = new Vector2(
            (float)GD.RandRange(-12, 12),
            (float)GD.RandRange(-12, 12)
        );

        physicItem.GlobalPosition = GlobalPosition + offset;

        _spawnedItem = physicItem;
        _spawnedItem.Collected += OnDroppedItemCollected;
    }

    private void OnDroppedItemCollected()
    {
        if (_spawnedItem != null)
        {
            _spawnedItem.Collected -= OnDroppedItemCollected;
            _spawnedItem = null;
        }

        DisableChestLogic();
    }

    private void DisableChestLogic()
    {
        _canOpen = false;

        if (_label != null)
            _label.Visible = false;

        SetProcess(false);
        SetPhysicsProcess(false);
    }

    public Item OpenChest()
    {
        if (IsOpened)
            return null;

        IsOpened = true;
        _canOpen = false;

        if (_label != null)
            _label.Visible = false;

        if (_sprite != null)
            _sprite.Frame = 1;

        Item reward = GetRandomDrop();

        if (reward != null)
        {
            GD.Print($"dropou: {reward.ResourcePath}");
            SpawnDroppedItem(reward);
        }
        else
        {
            GD.Print("baú sem drop válido.");
            DisableChestLogic();
        }

        return reward;
    }
}