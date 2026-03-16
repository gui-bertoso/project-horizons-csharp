using Godot;

namespace projecthorizonscs;

public partial class Item : Resource
{
	public enum ITEM_TYPE
	{
		Weapon,
		HeadArmor,
		BodyArmor,
		FootArmor,
		Acessory,
		Consumable,
		Acessory1,
		Acessory2
	}
	public enum ITEM_CLASS
	{
		All,
		Mellee,
		Mage,
		HeavyMellee,
		Wizard,
		Ranged,
		Archer,
		Bommet
	}

	[Export]
	public string ItemName;
	[Export]
	public ITEM_TYPE ItemType;
	[Export]
	public ITEM_CLASS ItemClass;
	[Export(PropertyHint.MultilineText)]
	public string ItemDescription = "";
	[Export]
	public int ItemAmount = 1;
	[Export]
	public CompressedTexture2D ItemTexture;
	[Export]
	public PackedScene ItemScene;
	[Export]
	public CompressedTexture2D ArmorSpriteSheet;
}