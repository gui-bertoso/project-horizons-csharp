using Godot;
using Godot.Collections;

namespace projecthorizonscs.Autoload;

public partial class DataManager
{
	private Dictionary<string, Variant> SerializeWorldData()
	{
		var serializedData = new Dictionary<string, Variant>();

		foreach (var entry in CurrentWorldData)
			serializedData[entry.Key] = SerializeWorldValue(entry.Key, entry.Value);

		return serializedData;
	}

	private static Variant SerializeWorldValue(string key, Variant value)
	{
		if (System.Array.IndexOf(EquippedItemKeys, key) < 0)
			return value;

		if (value.AsGodotObject() is not Item item || IsEmptyItem(item))
			return new Dictionary();

		return Variant.From(SerializeItem(item));
	}

	private static Variant DeserializeWorldValue(string key, Variant value)
	{
		if (System.Array.IndexOf(EquippedItemKeys, key) < 0)
			return value;

		if (value.VariantType == Variant.Type.Nil)
			return new Item();

		if (value.AsGodotObject() is Item item)
			return item;

		var itemData = value.AsGodotDictionary();
		if (itemData.Count == 0)
			return new Item();

		return DeserializeItem(itemData);
	}

	private static Dictionary SerializeItem(Item item)
	{
		return new Dictionary
		{
			{ "ItemName", item.ItemName ?? "" },
			{ "ItemType", (int)item.ItemType },
			{ "ItemClass", (int)item.ItemClass },
			{ "ItemDescription", item.ItemDescription ?? "" },
			{ "ItemAmount", item.ItemAmount },
			{ "ItemTexture", item.ItemTexture },
			{ "ItemScene", item.ItemScene },
			{ "ArmorSpriteSheet", item.ArmorSpriteSheet }
		};
	}

	private static Item DeserializeItem(Dictionary itemData)
	{
		return new Item
		{
			ItemName = itemData.ContainsKey("ItemName") ? itemData["ItemName"].AsString() : "",
			ItemType = (Item.ITEM_TYPE)(itemData.ContainsKey("ItemType") ? itemData["ItemType"].AsInt32() : 0),
			ItemClass = (Item.ITEM_CLASS)(itemData.ContainsKey("ItemClass") ? itemData["ItemClass"].AsInt32() : 0),
			ItemDescription = itemData.ContainsKey("ItemDescription") ? itemData["ItemDescription"].AsString() : "",
			ItemAmount = itemData.ContainsKey("ItemAmount") ? itemData["ItemAmount"].AsInt32() : 1,
			ItemTexture = itemData.ContainsKey("ItemTexture") ? itemData["ItemTexture"].AsGodotObject() as CompressedTexture2D : null,
			ItemScene = itemData.ContainsKey("ItemScene") ? itemData["ItemScene"].AsGodotObject() as PackedScene : null,
			ArmorSpriteSheet = itemData.ContainsKey("ArmorSpriteSheet") ? itemData["ArmorSpriteSheet"].AsGodotObject() as CompressedTexture2D : null
		};
	}

	private static bool IsEmptyItem(Item item)
	{
		return string.IsNullOrWhiteSpace(item.ItemName) &&
			   item.ItemTexture == null &&
			   item.ItemScene == null &&
			   item.ArmorSpriteSheet == null;
	}
}
