using Godot;
using System;

public partial class ItemsDisplay : Control
{
	public TextureRect ArmorTextureRect;
	public TextureRect BodyTextureRect;
	public TextureRect FootTextureRect;
	public TextureRect WeaponTextureRect;
	public TextureRect ConsumableTextureRect;
	public TextureRect Acessory1TextureRect;
	public TextureRect Acessory2TextureRect;

	private Item EquippedHeadArmor;
	private Item EquippedBodyArmor;
	private Item EquippedFootArmor;
	private Item EquippedWeapon;
	private Item EquippedConsumable;
	private Item EquippedAcessory1;
	private Item EquippedAcessory2;

	public override void _Ready()
	{
		Globals.I.LocalItemsDisplay = this;

		ArmorTextureRect = GetNode<TextureRect>("TexureRect/TextureRect2");
		BodyTextureRect = GetNode<TextureRect>("TexureRect2/TextureRect2");
		FootTextureRect = GetNode<TextureRect>("TexureRect3/TextureRect2");
		WeaponTextureRect = GetNode<TextureRect>("TexureRect4/TextureRect2");
		ConsumableTextureRect = GetNode<TextureRect>("TexureRect5/TextureRect2");
		Acessory1TextureRect = GetNode<TextureRect>("TexureRect6/TextureRect2");
		Acessory2TextureRect = GetNode<TextureRect>("TexureRect7/TextureRect2");
	}

	public void EquipItem(Item.ITEM_TYPE slot, Item item)
	{
		switch (slot)
		{
			case Item.ITEM_TYPE.HeadArmor:
				ArmorTextureRect.Texture = item.ItemTexture;
				EquippedHeadArmor = item;
				break;
			case Item.ITEM_TYPE.BodyArmor:
				BodyTextureRect.Texture = item.ItemTexture;
				EquippedBodyArmor = item;
				break;
			case Item.ITEM_TYPE.FootArmor:
				FootTextureRect.Texture = item.ItemTexture;
				EquippedFootArmor = item;
				break;
			case Item.ITEM_TYPE.Weapon:
				WeaponTextureRect.Texture = item.ItemTexture;
				EquippedWeapon = item;
				Globals.I.LocalPlayerHand.EquipItem(EquippedWeapon);
				break;
			case Item.ITEM_TYPE.Consumable:
				ConsumableTextureRect.Texture = item.ItemTexture;
				EquippedConsumable = item;
				break;
			case Item.ITEM_TYPE.Acessory1:
				Acessory1TextureRect.Texture = item.ItemTexture;
				EquippedAcessory1 = item;
				break;
			case Item.ITEM_TYPE.Acessory2:
				Acessory2TextureRect.Texture = item.ItemTexture;
				EquippedAcessory2 = item;
				break;	
		}
	}

	public override void _Process(double delta)
	{
	}
}
