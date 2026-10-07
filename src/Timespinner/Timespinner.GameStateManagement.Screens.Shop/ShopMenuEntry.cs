using Timespinner.GameAbstractions.Inventory;

namespace Timespinner.GameStateManagement.Screens.Shop;

internal class ShopMenuEntry : MenuEntry
{
	private readonly EOrbSlot _orbSlot;

	private readonly EInventoryOrbType _orbType;

	private readonly EInventoryCategoryType _itemType;

	private readonly InventoryItem _item;

	public bool IsAffordable { get; set; }

	public bool IsAvailable { get; set; }

	internal EOrbSlot OrbSlot => _orbSlot;

	internal EInventoryOrbType OrbType => _orbType;

	public EInventoryCategoryType ItemType => _itemType;

	public int ShopPrice { get; set; }

	public int TotalPrice => ShopPrice * QuanityToBuy;

	public int QuanityToBuy { get; set; }

	public InventoryItem Item => _item;

	public ShopMenuEntry(InventoryItem item, EInventoryCategoryType itemType)
		: base(item.Name)
	{
		_item = item;
		_itemType = itemType;
		base.Description = item.Description;
		QuanityToBuy = 1;
		base.DoesConfirmationPlaySound = false;
		IsAvailable = true;
		_orbType = EInventoryOrbType.None;
	}

	public ShopMenuEntry(InventoryOrb orb, EOrbSlot slot)
		: base(InventoryItem.NameFromType(orb.OrbType, slot))
	{
		_item = orb;
		_itemType = EInventoryCategoryType.Orb;
		base.Description = InventoryItem.GetOrbDescriptionBySlot(orb, slot);
		_orbSlot = slot;
		_orbType = orb.OrbType;
		QuanityToBuy = 1;
		base.DoesConfirmationPlaySound = false;
		IsAvailable = true;
	}
}
