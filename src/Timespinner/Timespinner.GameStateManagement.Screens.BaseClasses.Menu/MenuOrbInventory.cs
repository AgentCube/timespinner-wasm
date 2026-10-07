using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions.Inventory;

namespace Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

internal sealed class MenuOrbInventory : MenuInventoryWithIconCollection
{
	private const int OrbTextMarginX = -16;

	private readonly bool _doesUnequip;

	private readonly EOrbSlot _orbSlot;

	private readonly InventoryOrbCollection _collection;

	private readonly Action<InventoryOrb> _onSelectedAction;

	private readonly Action _onUnequipAction;

	private int _lastRecentCount;

	internal EInventoryOrbType EquippedOrb { get; set; }

	public MenuOrbInventory(InventoryOrbCollection collection, List<EInventoryOrbType> recentOrbs, Action<InventoryOrb> onSelected, Action onUnequip, bool doesAddUnequip, EOrbSlot orbSlot, SpriteSheet pauseSprite)
		: base(collection.Inventory.Values, doesAddUnequip, pauseSprite)
	{
		_collection = collection;
		_onSelectedAction = onSelected;
		_onUnequipAction = onUnequip;
		_orbSlot = orbSlot;
		_doesUnequip = doesAddUnequip;
		base.TextMarginX = -16;
		PopulateEntries();
		AddRecentEntries(recentOrbs);
		base.IconFrameIndex = 111;
	}

	internal void AddRecentEntries(List<EInventoryOrbType> recentOrbs)
	{
		int index = (_doesUnequip ? 1 : 0);
		int count = recentOrbs.Count;
		for (int i = 0; i < _lastRecentCount; i++)
		{
			base.Entries.RemoveAt(index);
			base.KeyToItemLookup.RemoveAt(index);
		}
		int num = 0;
		string format = Loc.Get("EquipmentRecentDescription");
		for (int num2 = count - 1; num2 >= 0; num2--)
		{
			EInventoryOrbType eInventoryOrbType = recentOrbs[num2];
			int num3 = (int)eInventoryOrbType;
			if (_collection.Inventory.ContainsKey(num3))
			{
				InventoryOrb item = _collection.Inventory[num3];
				MenuEntry menuEntry = CreateNewMenuEntryFromItem(item);
				menuEntry.BaseDrawColor = MenuInventoryWithIconCollection.RecentDrawColor;
				menuEntry.Description = string.Format(format, menuEntry.Description);
				base.KeyToItemLookup.Insert(index, num3);
				base.Entries.Insert(index, menuEntry);
				num++;
			}
		}
		RefreshEntryWidths();
		_lastRecentCount = num;
	}

	internal override MenuEntry CreateNewMenuEntryFromItem(InventoryItem item)
	{
		switch (_orbSlot)
		{
		case EOrbSlot.Spell:
		{
			MenuEntry menuEntry3 = new MenuEntry(InventoryItem.GetOrbNameBySlot(item, EOrbSlot.Spell));
			menuEntry3.Description = InventoryItem.GetOrbDescriptionBySlot(item, EOrbSlot.Spell);
			return menuEntry3;
		}
		case EOrbSlot.Passive:
		{
			MenuEntry menuEntry2 = new MenuEntry(InventoryItem.GetOrbNameBySlot(item, EOrbSlot.Passive));
			menuEntry2.Description = InventoryItem.GetOrbDescriptionBySlot(item, EOrbSlot.Passive);
			return menuEntry2;
		}
		default:
		{
			MenuEntry menuEntry = new MenuEntry(item.Name);
			menuEntry.Description = item.Description;
			return menuEntry;
		}
		}
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
				_onSelectedAction(_collection.Inventory[num]);
			}
		}
		return false;
	}

	internal override EInventoryItemIcon GetSelectedIcon()
	{
		EInventoryItemIcon result = EInventoryItemIcon.None;
		int count = base.Entries.Count;
		if (count > 0 && base.SelectedIndex < count && base.SelectedIndex < base.KeyToItemLookup.Count && base.KeyToItemLookup[base.SelectedIndex] != -1)
		{
			InventoryOrb inventoryOrb = _collection.Inventory[base.KeyToItemLookup[base.SelectedIndex]];
			result = InventoryItem.GetIconFromItem(inventoryOrb.OrbType, _orbSlot);
		}
		return result;
	}

	public InventoryOrb GetSelected()
	{
		InventoryOrb result = null;
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

	internal override bool IsItemVisible(InventoryItem item)
	{
		bool result = false;
		if (item is InventoryOrb inventoryOrb)
		{
			switch (_orbSlot)
			{
			case EOrbSlot.Melee:
				result = true;
				break;
			case EOrbSlot.Spell:
				result = inventoryOrb.IsSpellUnlocked;
				break;
			case EOrbSlot.Passive:
				result = inventoryOrb.IsPassiveUnlocked;
				break;
			}
		}
		return result;
	}

	internal override bool IsIconVisibleByIndex(int index)
	{
		bool result = false;
		int key = base.KeyToItemLookup[index];
		if (_collection.Inventory.ContainsKey(key))
		{
			InventoryOrb inventoryOrb = _collection.Inventory[key];
			if (inventoryOrb != null && inventoryOrb.OrbType != 0 && inventoryOrb.OrbType == EquippedOrb)
			{
				result = true;
			}
		}
		return result;
	}
}
