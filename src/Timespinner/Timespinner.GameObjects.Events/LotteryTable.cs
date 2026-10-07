using System.Collections.Generic;
using Timespinner.GameAbstractions.Inventory;

namespace Timespinner.GameObjects.Events;

internal class LotteryTable
{
	private readonly List<int> _items = new List<int>();

	private readonly List<ETreasureLootType> _categories = new List<ETreasureLootType>();

	private readonly List<float> _rates = new List<float>();

	internal void Add(EInventoryUseItemType item, float rate)
	{
		_items.Add((int)item);
		_categories.Add(ETreasureLootType.UseItem);
		_rates.Add(rate);
	}

	internal void Add(EInventoryEquipmentType item, float rate)
	{
		_items.Add((int)item);
		_categories.Add(ETreasureLootType.Equipment);
		_rates.Add(rate);
	}

	internal void Add(EInventoryFamiliarType item, float rate)
	{
		_items.Add((int)item);
		_categories.Add(ETreasureLootType.Familiar);
		_rates.Add(rate);
	}

	internal void GetItemByRoll(double roll, out ETreasureLootType treasureLootType, out EInventoryUseItemType lootUseItemType, out EInventoryEquipmentType lootEquipmentType, out EInventoryFamiliarType lootFamiliarType)
	{
		int count = _items.Count;
		if (count > 0)
		{
			int index = 0;
			for (int i = 0; i < count; i++)
			{
				if ((double)_rates[i] >= roll)
				{
					index = i;
					break;
				}
			}
			treasureLootType = _categories[index];
			if (treasureLootType == ETreasureLootType.UseItem)
			{
				lootUseItemType = (EInventoryUseItemType)_items[index];
				lootEquipmentType = EInventoryEquipmentType.None;
				lootFamiliarType = EInventoryFamiliarType.None;
			}
			else if (treasureLootType == ETreasureLootType.Equipment)
			{
				lootUseItemType = EInventoryUseItemType.None;
				lootEquipmentType = (EInventoryEquipmentType)_items[index];
				lootFamiliarType = EInventoryFamiliarType.None;
			}
			else
			{
				lootUseItemType = EInventoryUseItemType.None;
				lootEquipmentType = EInventoryEquipmentType.None;
				lootFamiliarType = (EInventoryFamiliarType)_items[index];
			}
		}
		else
		{
			treasureLootType = ETreasureLootType.UseItem;
			lootUseItemType = EInventoryUseItemType.Potion;
			lootEquipmentType = EInventoryEquipmentType.None;
			lootFamiliarType = EInventoryFamiliarType.None;
		}
	}
}
