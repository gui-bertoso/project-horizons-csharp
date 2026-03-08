using System;
using Godot;

namespace projecthorizonscs.Interface.ItemsDisplay;

public partial class ItemsDisplay : Control
{
	private TextureRect _armorTextureRect;
	private TextureRect _bodyTextureRect;
	private TextureRect _footTextureRect;
	private TextureRect _weaponTextureRect;
	private TextureRect _consumableTextureRect;
	private TextureRect _acessory1TextureRect;
	private TextureRect _acessory2TextureRect;

	private Items.Item _equippedHeadArmor;
	private Items.Item _equippedBodyArmor;
	private Items.Item _equippedFootArmor;
	private Items.Item _equippedWeapon;
	private Items.Item _equippedConsumable;
	private Items.Item _equippedAcessory1;
	private Items.Item _equippedAcessory2;

	public override void _Ready()
	{
		Autoload.Globals.I.LocalItemsDisplay = this;

		_armorTextureRect = GetNode<TextureRect>("TexureRect/TextureRect2");
		_bodyTextureRect = GetNode<TextureRect>("TexureRect2/TextureRect2");
		_footTextureRect = GetNode<TextureRect>("TexureRect3/TextureRect2");
		_weaponTextureRect = GetNode<TextureRect>("TexureRect4/TextureRect2");
		_consumableTextureRect = GetNode<TextureRect>("TexureRect5/TextureRect2");
		_acessory1TextureRect = GetNode<TextureRect>("TexureRect6/TextureRect2");
		_acessory2TextureRect = GetNode<TextureRect>("TexureRect7/TextureRect2");
	}

	public void EquipItem(Items.Item.ITEM_TYPE slot, Items.Item item)
	{
		switch (slot)
		{
			case Items.Item.ITEM_TYPE.HeadArmor:
				_armorTextureRect.Texture = item.ItemTexture;
				_equippedHeadArmor = item;
				break;
			case Items.Item.ITEM_TYPE.BodyArmor:
				_bodyTextureRect.Texture = item.ItemTexture;
				_equippedBodyArmor = item;
				break;
			case Items.Item.ITEM_TYPE.FootArmor:
				_footTextureRect.Texture = item.ItemTexture;
				_equippedFootArmor = item;
				break;
			case Items.Item.ITEM_TYPE.Weapon:
				_weaponTextureRect.Texture = item.ItemTexture;
				_equippedWeapon = item;
				Autoload.Globals.I.LocalPlayerHand.EquipItem(_equippedWeapon);
				break;
			case Items.Item.ITEM_TYPE.Consumable:
				_consumableTextureRect.Texture = item.ItemTexture;
				_equippedConsumable = item;
				break;
			case Items.Item.ITEM_TYPE.Acessory1:
				_acessory1TextureRect.Texture = item.ItemTexture;
				_equippedAcessory1 = item;
				break;
			case Items.Item.ITEM_TYPE.Acessory2:
				_acessory2TextureRect.Texture = item.ItemTexture;
				_equippedAcessory2 = item;
				break;
			case Items.Item.ITEM_TYPE.Acessory:
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
		}
	}

	public override void _Process(double delta)
	{
	}
}