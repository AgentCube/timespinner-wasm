using Timespinner.GameAbstractions.Inventory;

namespace Timespinner.GameStateManagement.Screens.Shop;

internal class OrbShopMenuEntry : MenuEntry
{
	private readonly EOrbSlot _orbSlot;

	private readonly InventoryOrb _orb;

	public bool IsAffordable { get; set; }

	public EOrbSlot OrbSlot => _orbSlot;

	public int ShopPrice { get; set; }

	public int TotalPrice => ShopPrice * QuanityToBuy;

	public int QuanityToBuy { get; set; }

	public InventoryOrb Orb => _orb;

	public OrbShopMenuEntry(InventoryOrb orb, EOrbSlot slot)
		: base(InventoryItem.NameFromType(orb.OrbType, slot))
	{
		_orb = orb;
		_orbSlot = slot;
		base.Description = InventoryItem.GetOrbDescriptionBySlot(_orb, slot);
		QuanityToBuy = 1;
		base.DoesConfirmationPlaySound = false;
	}
}
