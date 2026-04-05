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
	private TextureRect _accessory1PlaceholderTextureRect;
	private TextureRect _accessory2PlaceholderTextureRect;

	private Label _consumableAmountLabel;

	private TextureRect _headArmorTextureRect;
	private TextureRect _bodyArmorTextureRect;
	private TextureRect _footArmorTextureRect;
	private TextureRect _weaponTextureRect;
	private TextureRect _consumableTextureRect;
	private TextureRect _accessory1TextureRect;
	private TextureRect _accessory2TextureRect;

	private Item _equippedHeadArmor;
	private Item _equippedBodyArmor;
	private Item _equippedFootArmor;
	private Item _equippedWeapon;
	private Item _equippedConsumable;
	private Item _equippedAccessory1;
	private Item _equippedAccessory2;
	private bool _equipmentLoaded;
	private bool _equipmentLoadPending;

	public override void _Ready()
	{
		Autoload.Globals.I.LocalItemsDisplay = this;

		_headArmorPlaceholderTextureRect = GetNode<TextureRect>("%HeadArmorPlaceholderTextureRect");
		_bodyArmorPlaceholderTextureRect = GetNode<TextureRect>("%BodyArmorPlaceholderTextureRect");
		_footArmorPlaceholderTextureRect = GetNode<TextureRect>("%FootArmorPlaceholderTextureRect");
		_weaponPlaceholderTextureRect = GetNode<TextureRect>("%WeaponPlaceholderTextureRect");
		_consumablePlaceholderTextureRect = GetNode<TextureRect>("%ConsumablePlaceholderTextureRect");
		_accessory1PlaceholderTextureRect = GetNode<TextureRect>("%Acessory1PlaceholderTextureRect");
		_accessory2PlaceholderTextureRect = GetNode<TextureRect>("%Acessory2PlaceholderTextureRect");
		_consumableAmountLabel = GetNode<Label>("%ConsumableAmountLabel");

		_headArmorTextureRect = GetNode<TextureRect>("%HeadArmorTextureRect");
		_bodyArmorTextureRect = GetNode<TextureRect>("%BodyArmorTextureRect");
		_footArmorTextureRect = GetNode<TextureRect>("%FootArmorTextureRect");
		_weaponTextureRect = GetNode<TextureRect>("%WeaponTextureRect");
		_consumableTextureRect = GetNode<TextureRect>("%ConsumableTextureRect");
		_accessory1TextureRect = GetNode<TextureRect>("%Acessory1TextureRect");
		_accessory2TextureRect = GetNode<TextureRect>("%Acessory2TextureRect");

		_equipmentLoadPending = true;
	}

	public void LoadEquipment()
	{
		if (_equipmentLoaded)
			return;

		if (!IsInstanceValid(this) || !IsInsideTree() || IsQueuedForDeletion())
			return;

		if (Autoload.Globals.I.LocalPlayerBody == null || Autoload.Globals.I.LocalPlayerHand == null)
		{
			_equipmentLoadPending = true;
			return;
		}

		if (TryGetEquippedItem("EquippedHeadArmor", out Item headItem))
			EquipItem(Item.ITEM_TYPE.HeadArmor, headItem);
		if (TryGetEquippedItem("EquippedBodyArmor", out Item bodyItem))
			EquipItem(Item.ITEM_TYPE.BodyArmor, bodyItem);
		if (TryGetEquippedItem("EquippedFootArmor", out Item footItem))
			EquipItem(Item.ITEM_TYPE.FootArmor, footItem);
		if (TryGetEquippedItem("EquippedWeapon", out Item weaponItem))
			EquipItem(Item.ITEM_TYPE.Weapon, weaponItem);
		if (TryGetEquippedItem("EquippedConsumable", out Item consumableItem))
			EquipItem(Item.ITEM_TYPE.Consumable, consumableItem);
		if (TryGetEquippedItem("EquippedAcessory1", out Item accessory1Item))
			EquipItem(Item.ITEM_TYPE.Acessory1, accessory1Item);
		if (TryGetEquippedItem("EquippedAcessory2", out Item accessory2Item))
			EquipItem(Item.ITEM_TYPE.Acessory2, accessory2Item);

		_equipmentLoaded = true;
		_equipmentLoadPending = false;
	}

	public void EquipItem(Item.ITEM_TYPE slot, Item newItem)
	{
		var item = (Item)newItem.Duplicate(true);

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
				_footArmorTextureRect.Texture = item.ItemTexture;
				_footArmorPlaceholderTextureRect.Visible = false;
				_equippedFootArmor = item;
				DataManager.I.CurrentWorldData["EquippedFootArmor"] = item;
				Globals.I.LocalPlayerBody.SetArmorTexture(item);
				break;
			case Item.ITEM_TYPE.Weapon:
				_weaponTextureRect.Texture = item.ItemTexture;
				_weaponPlaceholderTextureRect.Visible = false;
				_equippedWeapon = item;
				DataManager.I.CurrentWorldData["EquippedWeapon"] = item;
				Autoload.Globals.I.LocalPlayerHand.EquipItem(_equippedWeapon);
				break;
			case Item.ITEM_TYPE.Consumable:
				if (_equippedConsumable == null)
				{
					_consumableTextureRect.Texture = item.ItemTexture;
					_consumablePlaceholderTextureRect.Visible = false;
					_consumableTextureRect.Visible = true;
					_equippedConsumable = item;
					_consumableAmountLabel.Text = _equippedConsumable.ItemAmount.ToString();
				}
				else
				{
					_equippedConsumable.ItemAmount += item.ItemAmount;
					_consumableAmountLabel.Text = _equippedConsumable.ItemAmount.ToString();
				}
				DataManager.I.CurrentWorldData["EquippedConsumable"] = _equippedConsumable;
				break;
			case Item.ITEM_TYPE.Acessory1:
				_accessory1TextureRect.Texture = item.ItemTexture;
				_accessory1PlaceholderTextureRect.Visible = false;
				_equippedAccessory1 = item;
				DataManager.I.CurrentWorldData["EquippedAcessory1"] = item;
				break;
			case Item.ITEM_TYPE.Acessory2:
				_accessory2TextureRect.Texture = item.ItemTexture;
				_accessory2PlaceholderTextureRect.Visible = false;
				_equippedAccessory2 = item;
				DataManager.I.CurrentWorldData["EquippedAcessory2"] = item;
				break;
			case Item.ITEM_TYPE.Acessory:
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
		}

		PersistEquipmentChanges();
	}

	public void ClearConsumableSlot()
	{
		_equippedConsumable = null;
		_consumablePlaceholderTextureRect.Visible = true;
		_consumableTextureRect.Visible = false;
		_consumableTextureRect.Texture = null;
		_consumableAmountLabel.Text = "";
		DataManager.I.CurrentWorldData["EquippedConsumable"] = new Item();
		PersistEquipmentChanges();
	}

	public override void _Process(double delta)
	{
		if (_equipmentLoadPending && !_equipmentLoaded)
			LoadEquipment();

		ActionBehavior();
	}

	public void ActionBehavior()
	{
		if (_equippedConsumable != null && Input.IsActionJustPressed("consume"))
			UseConsumable();
	}

	public void UseConsumable()
	{
		var item = (Item)_equippedConsumable.Duplicate(true);
		item.ItemAmount = 1;

		_equippedConsumable.ItemAmount -= 1;
		_consumableAmountLabel.Text = _equippedConsumable.ItemAmount.ToString();
		DataManager.I.CurrentWorldData["EquippedConsumable"] = _equippedConsumable;
		PersistEquipmentChanges();
		if (_equippedConsumable.ItemAmount <= 0)
			ClearConsumableSlot();
	}

	private static bool TryGetEquippedItem(string key, out Item item)
	{
		item = null;

		if (!DataManager.I.CurrentWorldData.TryGetValue(key, out Variant itemVar))
			return false;

		item = itemVar.AsGodotObject() as Item;
		return HasItemData(item);
	}

	private static bool HasItemData(Item item)
	{
		return item != null &&
			   (!string.IsNullOrWhiteSpace(item.ItemName) ||
			    item.ItemTexture != null ||
			    item.ItemScene != null ||
			    item.ArmorSpriteSheet != null);
	}

	private void PersistEquipmentChanges()
	{
		DataManager.I.QuickSaveWorldData();
	}
}
