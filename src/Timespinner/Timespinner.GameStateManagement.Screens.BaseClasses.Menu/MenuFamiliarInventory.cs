using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Inventory;

namespace Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

internal class MenuFamiliarInventory : MenuInventoryWithIconCollection
{
	private readonly InventoryFamiliarCollection _collection;

	private readonly Action<InventoryFamiliar> _onSelectedAction;

	private readonly Action _onUnequipAction;

	internal EInventoryFamiliarType EquippedFamiliar { get; set; }

	public MenuFamiliarInventory(InventoryFamiliarCollection collection, Action<InventoryFamiliar> onSelected, Action onUnequip, SpriteSheet pauseSprite)
		: base(collection.Inventory.Values, doesAddUnequipEntry: true, pauseSprite)
	{
		_collection = collection;
		_onSelectedAction = onSelected;
		_onUnequipAction = onUnequip;
		PopulateEntries();
		base.IconFrameIndex = 111;
	}

	public override bool SelectEntry(PlayerIndex playerIndex)
	{
		if (base.Entries.Count > 0)
		{
			int num = base.KeyToItemLookup[base.SelectedIndex];
			if (num == -1)
			{
				_onUnequipAction();
			}
			else
			{
				_onSelectedAction(_collection.Inventory[base.KeyToItemLookup[base.SelectedIndex]]);
			}
		}
		return false;
	}

	public InventoryFamiliar GetSelected()
	{
		InventoryFamiliar result = null;
		if (base.Entries.Count > 0)
		{
			int num = base.KeyToItemLookup[base.SelectedIndex];
			if (num != -1)
			{
				result = _collection.Inventory[num];
			}
		}
		return result;
	}

	internal override EInventoryItemIcon GetSelectedIcon()
	{
		EInventoryItemIcon result = EInventoryItemIcon.None;
		int count = base.Entries.Count;
		if (count > 0 && base.SelectedIndex < count && base.SelectedIndex < base.KeyToItemLookup.Count && base.KeyToItemLookup[base.SelectedIndex] != -1)
		{
			InventoryFamiliar inventoryFamiliar = _collection.Inventory[base.KeyToItemLookup[base.SelectedIndex]];
			result = InventoryItem.GetIconFromItem(inventoryFamiliar.FamiliarType);
		}
		return result;
	}

	internal override bool IsIconVisibleByIndex(int index)
	{
		bool result = false;
		int key = base.KeyToItemLookup[index];
		if (_collection.Inventory.ContainsKey(key))
		{
			InventoryFamiliar inventoryFamiliar = _collection.Inventory[key];
			if (inventoryFamiliar != null && inventoryFamiliar.FamiliarType != 0 && inventoryFamiliar.FamiliarType == EquippedFamiliar)
			{
				result = true;
			}
		}
		return result;
	}
}
