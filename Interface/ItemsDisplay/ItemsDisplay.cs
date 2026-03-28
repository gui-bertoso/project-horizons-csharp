using System;
using Godot;
using projecthorizonscs.Autoload;

namespace projecthorizonscs.Interface;

public partial class ItemsDisplay : Control
{
	private TextureRect _headArmorPlaceholderTextureRect;
	private TextureRect _bodyArmorPlaceholderTextureRect;
	private TextureRect _footArmorPlaceholderTextureRect;
	private TextureRect _weaponPlaceholderTextureRect;
	private TextureRect _consumablePlaceholderTextureRect;
	private TextureRect _acessory1PlaceholderTextureRect;
	private TextureRect _acessory2PlaceholderTextureRect;

	private Label _consumableAmountLabel;

	private TextureRect _headArmorTextureRect;
	private TextureRect _bodyArmorTextureRect;
	private TextureRect _footArmorTextureRect;
	private TextureRect _weaponTextureRect;
	private TextureRect _consumableTextureRect;
	private TextureRect _acessory1TextureRect;
	private TextureRect _acessory2TextureRect;

	private Item _equippedHeadArmor;
	private Item _equippedBodyArmor;
	private Item _equippedFootArmor;
	private Item _equippedWeapon;
	private Item _equippedConsumable;
	private Item _equippedAcessory1;
	private Item _equippedAcessory2;

	public override void _Ready()
	{
		Autoload.Globals.I.LocalItemsDisplay = this;

		_headArmorPlaceholderTextureRect = GetNode<TextureRect>("%HeadArmorPlaceholderTextureRect");
		_bodyArmorPlaceholderTextureRect = GetNode<TextureRect>("%BodyArmorPlaceholderTextureRect");
		_footArmorPlaceholderTextureRect = GetNode<TextureRect>("%FootArmorPlaceholderTextureRect");
		_weaponPlaceholderTextureRect = GetNode<TextureRect>("%WeaponPlaceholderTextureRect");
		_consumablePlaceholderTextureRect = GetNode<TextureRect>("%ConsumablePlaceholderTextureRect");
		_acessory1PlaceholderTextureRect = GetNode<TextureRect>("%Acessory1PlaceholderTextureRect");
		_acessory2PlaceholderTextureRect = GetNode<TextureRect>("%Acessory2PlaceholderTextureRect");
		_consumableAmountLabel = GetNode<Label>("%ConsumableAmountLabel");

		_headArmorTextureRect = GetNode<TextureRect>("%HeadArmorTextureRect");
		_bodyArmorTextureRect = GetNode<TextureRect>("%BodyArmorTextureRect");
		_footArmorTextureRect = GetNode<TextureRect>("%FootArmorTextureRect");
		_weaponTextureRect = GetNode<TextureRect>("%WeaponTextureRect");
		_consumableTextureRect = GetNode<TextureRect>("%ConsumableTextureRect");
		_acessory1TextureRect = GetNode<TextureRect>("%Acessory1TextureRect");
		_acessory2TextureRect = GetNode<TextureRect>("%Acessory2TextureRect");

		LoadEquipment();
	}

	public void LoadEquipment()
	{
		if (DataManager.I.CurrentWorldData.TryGetValue("EquippedBodyHead", out var headVar) && headVar.AsGodotObject() is Item headItem) 
			EquipItem(Item.ITEM_TYPE.HeadArmor, headItem);
		if (DataManager.I.CurrentWorldData.TryGetValue("EquippedBodyArmor", out var bodyVar) && bodyVar.AsGodotObject() is Item bodyItem) 
			EquipItem(Item.ITEM_TYPE.BodyArmor, bodyItem);
		if (DataManager.I.CurrentWorldData.TryGetValue("EquippedFootArmor", out var footVar) && footVar.AsGodotObject() is Item footItem) 
			EquipItem(Item.ITEM_TYPE.FootArmor, footItem);
		if (DataManager.I.CurrentWorldData.TryGetValue("EquippedWeapon", out var weaponVar) && weaponVar.AsGodotObject() is Item weaponItem) 
			EquipItem(Item.ITEM_TYPE.Weapon, weaponItem);
		if (DataManager.I.CurrentWorldData.TryGetValue("EquippedConsumable", out var consVar) && consVar.AsGodotObject() is Item consItem) 
			EquipItem(Item.ITEM_TYPE.Consumable, consItem);
		if (DataManager.I.CurrentWorldData.TryGetValue("EquippedAcessory1", out var acc1Var) && acc1Var.AsGodotObject() is Item acc1Item) 
			EquipItem(Item.ITEM_TYPE.Acessory1, acc1Item);
		if (DataManager.I.CurrentWorldData.TryGetValue("EquippedAcessory2", out var acc2Var) && acc2Var.AsGodotObject() is Item acc2Item) 
			EquipItem(Item.ITEM_TYPE.Acessory2, acc2Item);
	}



	public void EquipItem(Item.ITEM_TYPE slot, Item newItem)
	{
		var item = (Item)newItem.Duplicate(true);

		GD.Print("Equiping item");
		switch (slot)
		{
			case Item.ITEM_TYPE.HeadArmor:
				_headArmorTextureRect.Texture = item.ItemTexture;
				_headArmorPlaceholderTextureRect.Visible = false;
				_equippedHeadArmor = item;
				DataManager.I.CurrentWorldData["EquippedHeadArmor"] = item;

				Globals.I.LocalPlayerBody.SetArmorTexture(item);
				break;
			case Item.ITEM_TYPE.BodyArmor:
				_bodyArmorTextureRect.Texture = item.ItemTexture;
				_bodyArmorPlaceholderTextureRect.Visible = false;
				_equippedBodyArmor = item;
				DataManager.I.CurrentWorldData["EquippedBodyArmor"] = item;

				Globals.I.LocalPlayerBody.SetArmorTexture(item);
				break;
			case Item.ITEM_TYPE.FootArmor:
				GD.Print("Test 000");
				_footArmorTextureRect.Texture = item.ItemTexture;
				GD.Print("Test 111");
				_footArmorPlaceholderTextureRect.Visible = false;
				GD.Print("Test 222");
				_equippedFootArmor = item;
				DataManager.I.CurrentWorldData["EquippedFootArmor"] = item;
				GD.Print("Test 333");

				Globals.I.LocalPlayerBody.SetArmorTexture(item);
				break;
			case Item.ITEM_TYPE.Weapon:
				_weaponTextureRect.Texture = item.ItemTexture;
				_weaponPlaceholderTextureRect.Visible = false;
				_equippedWeapon = item;
				DataManager.I.CurrentWorldData["EquippedWeapon"] = item;
				GD.Print("Equiping weapon");
				Autoload.Globals.I.LocalPlayerHand.EquipItem(_equippedWeapon);
				GD.Print("Weapon Equipped");
				break;
			case Item.ITEM_TYPE.Consumable:
				if (_equippedConsumable == null)
				{
					_consumableTextureRect.Texture = item.ItemTexture;
					_consumablePlaceholderTextureRect.Visible = false;
					_equippedConsumable = item;
				}
				else
				{
					_equippedConsumable.ItemAmount += item.ItemAmount;
					_consumableAmountLabel.Text = _equippedConsumable.ItemAmount.ToString();
				}
				DataManager.I.CurrentWorldData["EquippedConsumable"] = _equippedConsumable;

				break;
			case Item.ITEM_TYPE.Acessory1:
				_acessory1TextureRect.Texture = item.ItemTexture;
				_acessory1PlaceholderTextureRect.Visible = false;
				_equippedAcessory1 = item;
				DataManager.I.CurrentWorldData["EquippedAcessory1"] = item;
				break;
			case Item.ITEM_TYPE.Acessory2:
				_acessory2TextureRect.Texture = item.ItemTexture;
				_acessory2PlaceholderTextureRect.Visible = false;
				_equippedAcessory2 = item;
				DataManager.I.CurrentWorldData["EquippedAcessory2"] = item;
				break;
			case Item.ITEM_TYPE.Acessory:
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
		}
		GD.Print("Equipped item");
	}

	public void ClearConsumableSlot()
	{
		_equippedConsumable = null;
		_consumablePlaceholderTextureRect.Visible = true;
		_consumableTextureRect.Visible = false;
		_consumableTextureRect.Texture = null;
		_consumableAmountLabel.Text = "";
		DataManager.I.CurrentWorldData["EquippedConsumable"] = new Variant();
	}

	public override void _Process(double delta)
	{
		ActionBehavior();
	}

	public void ActionBehavior()
	{
		if (_equippedConsumable != null)
		{
			if (Input.IsActionJustPressed("consume"))
			{
				UseConsumable();
			}
		}
	}

	public void UseConsumable()
	{
		var item = (Item)_equippedConsumable.Duplicate(true);
		item.ItemAmount = 1;
		_consumableAmountLabel.Text = _equippedConsumable.ItemAmount.ToString();

		_equippedConsumable.ItemAmount -= 1;
		if (_equippedConsumable.ItemAmount <= 0)
		{
			ClearConsumableSlot();
		}
	}
}