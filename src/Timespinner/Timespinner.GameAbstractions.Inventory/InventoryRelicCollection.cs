namespace Timespinner.GameAbstractions.Inventory;

public class InventoryRelicCollection : InventoryCollection<InventoryRelic>
{
	public override EInventoryCategoryType Type => EInventoryCategoryType.Relic;

	public override void AddItem(int item)
	{
		if (!base.Inventory.ContainsKey(item))
		{
			InventoryRelic inventoryRelic = new InventoryRelic((EInventoryRelicType)item);
			inventoryRelic.IsActive = true;
			InventoryRelic value = inventoryRelic;
			base.Inventory.Add(item, value);
		}
	}

	public bool IsRelicActive(EInventoryRelicType type)
	{
		if (base.Inventory.ContainsKey((int)type))
		{
			return base.Inventory[(int)type].IsActive;
		}
		return false;
	}

	public override void RefreshItemNameAndDescriptions()
	{
		foreach (InventoryRelic value in base.Inventory.Values)
		{
			value.RefreshNameAndDescription();
		}
	}
}
