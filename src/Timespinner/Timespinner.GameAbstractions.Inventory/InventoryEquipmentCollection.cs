namespace Timespinner.GameAbstractions.Inventory;

public class InventoryEquipmentCollection : InventoryCollection<InventoryEquipment>
{
	public override EInventoryCategoryType Type => EInventoryCategoryType.Equipment;

	public override void AddItem(int item)
	{
		AddItem(item, 1);
	}

	public void AddItem(int item, int amount)
	{
		if (!base.Inventory.ContainsKey(item))
		{
			InventoryEquipment inventoryEquipment = new InventoryEquipment((EInventoryEquipmentType)item);
			inventoryEquipment.Count = amount;
			InventoryEquipment value = inventoryEquipment;
			base.Inventory.Add(item, value);
			return;
		}
		InventoryEquipment inventoryEquipment2 = base.Inventory[item];
		inventoryEquipment2.Count += amount;
		if (inventoryEquipment2.Count > inventoryEquipment2.StackCap)
		{
			inventoryEquipment2.Count = inventoryEquipment2.StackCap;
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
			InventoryEquipment inventoryEquipment = base.Inventory[item];
			inventoryEquipment.Count -= amount;
			if (inventoryEquipment.Count <= 0)
			{
				base.Inventory.Remove(item);
			}
		}
	}

	public int GetCount(EInventoryEquipmentType itemType)
	{
		int result = 0;
		if (base.Inventory.ContainsKey((int)itemType))
		{
			result = base.Inventory[(int)itemType].Count;
		}
		return result;
	}

	public override void RefreshItemNameAndDescriptions()
	{
		foreach (InventoryEquipment value in base.Inventory.Values)
		{
			value.RefreshNameAndDescription();
		}
	}
}
