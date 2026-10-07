namespace Timespinner.GameAbstractions.Inventory;

public class MerchantInventory
{
	private readonly InventoryOrbCollection _orbInventory = new InventoryOrbCollection();

	private readonly InventoryEquipmentCollection _equipmentInventory = new InventoryEquipmentCollection();

	private readonly InventoryFamiliarCollection _familiarInventory = new InventoryFamiliarCollection();

	private readonly InventoryRelicCollection _relicInventory = new InventoryRelicCollection();

	private readonly InventoryUseItemCollection _useItemInventory = new InventoryUseItemCollection();

	public InventoryOrbCollection OrbInventory => _orbInventory;

	public InventoryEquipmentCollection EquipmentInventory => _equipmentInventory;

	public InventoryFamiliarCollection FamiliarInventory => _familiarInventory;

	public InventoryRelicCollection RelicInventory => _relicInventory;

	public InventoryUseItemCollection UseItemInventory => _useItemInventory;

	public void AddItem(EInventoryOrbType type, EOrbSlot slot)
	{
		_orbInventory.AddItem((int)type, slot);
	}

	public void AddItem(EInventoryFamiliarType type)
	{
		_familiarInventory.AddItem((int)type);
	}

	public void AddItem(EInventoryRelicType type)
	{
		_relicInventory.AddItem((int)type);
	}

	public void AddItem(EInventoryUseItemType type)
	{
		_useItemInventory.AddItem((int)type, 1);
	}

	public void AddItem(EInventoryEquipmentType type)
	{
		_equipmentInventory.AddItem((int)type, 1);
	}
}
