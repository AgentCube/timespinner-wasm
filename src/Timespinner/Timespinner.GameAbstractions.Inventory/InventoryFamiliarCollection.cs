namespace Timespinner.GameAbstractions.Inventory;

public class InventoryFamiliarCollection : InventoryCollection<InventoryFamiliar>
{
	public override EInventoryCategoryType Type => EInventoryCategoryType.Familiar;

	public override void AddItem(int item)
	{
		if (!base.Inventory.ContainsKey(item))
		{
			InventoryFamiliar value = new InventoryFamiliar((EInventoryFamiliarType)item, GetSumOfFamiliarLevels);
			base.Inventory.Add(item, value);
		}
	}

	public void InitializePostLoad()
	{
		foreach (InventoryFamiliar value in base.Inventory.Values)
		{
			value.GiveFamiliarLevelSumFunction(GetSumOfFamiliarLevels);
			value.RefreshFamiliarStats();
		}
	}

	public InventoryFamiliar GetFamiliarItem(EInventoryFamiliarType familiarType)
	{
		InventoryFamiliar result = null;
		if (base.Inventory.ContainsKey((int)familiarType))
		{
			result = base.Inventory[(int)familiarType];
		}
		return result;
	}

	internal bool GiveFamiliarExperience(EInventoryFamiliarType familiarType, int amount)
	{
		bool result = false;
		if (base.Inventory.ContainsKey((int)familiarType))
		{
			result = base.Inventory[(int)familiarType].GiveExperience(amount);
		}
		return result;
	}

	public override void RefreshItemNameAndDescriptions()
	{
		foreach (InventoryFamiliar value in base.Inventory.Values)
		{
			value.RefreshNameAndDescription();
		}
	}

	internal int GetSumOfFamiliarLevels()
	{
		int num = 0;
		foreach (InventoryFamiliar value in base.Inventory.Values)
		{
			num += value.Level + 1;
		}
		return num;
	}
}
