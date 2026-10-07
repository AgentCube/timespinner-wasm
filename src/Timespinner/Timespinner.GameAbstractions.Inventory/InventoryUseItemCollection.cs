namespace Timespinner.GameAbstractions.Inventory;

public class InventoryUseItemCollection : InventoryCollection<InventoryUseItem>
{
	public override EInventoryCategoryType Type => EInventoryCategoryType.UseItem;

	public override void AddItem(int item)
	{
		AddItem(item, 1);
	}

	public void AddItem(int item, int amount)
	{
		if (!base.Inventory.ContainsKey(item))
		{
			InventoryUseItem inventoryUseItem = new InventoryUseItem((EInventoryUseItemType)item);
			inventoryUseItem.Count = amount;
			InventoryUseItem value = inventoryUseItem;
			base.Inventory.Add(item, value);
			return;
		}
		InventoryUseItem inventoryUseItem2 = base.Inventory[item];
		inventoryUseItem2.Count += amount;
		if (inventoryUseItem2.Count > inventoryUseItem2.StackCap)
		{
			inventoryUseItem2.Count = inventoryUseItem2.StackCap;
		}
	}

	public override void RemoveItem(int item)
	{
		RemoveItem(item, 1);
	}

	public void RemoveItem(int item, int amount)
	{
		if (base.Inventory.ContainsKey(item))
		{
			InventoryUseItem inventoryUseItem = base.Inventory[item];
			inventoryUseItem.Count -= amount;
			if (inventoryUseItem.Count <= 0)
			{
				base.Inventory.Remove(item);
			}
		}
	}

	public override void RefreshItemNameAndDescriptions()
	{
		foreach (InventoryUseItem value in base.Inventory.Values)
		{
			value.RefreshNameAndDescription();
		}
	}
}
