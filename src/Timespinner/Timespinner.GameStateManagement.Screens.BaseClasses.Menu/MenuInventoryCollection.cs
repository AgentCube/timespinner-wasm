using System.Collections.Generic;
using System.Linq;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions.Inventory;

namespace Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

internal abstract class MenuInventoryCollection : MenuEntryCollection
{
	private readonly bool _doesAddUnequipEntry;

	private readonly List<int> _keyToItemLookup = new List<int>();

	private IEnumerable<InventoryItem> _items;

	public bool DoesDisplayStoreMonetaryValue { get; set; }

	public float MonetaryValueMultiplier { get; set; }

	public IList<int> KeyToItemLookup => _keyToItemLookup;

	protected MenuInventoryCollection(IEnumerable<InventoryItem> items, bool doesAddUnequipEntry)
	{
		_doesAddUnequipEntry = doesAddUnequipEntry;
		_items = items;
		base.ColumnCount = 2;
		base.DoesMenuAllowScrolling = true;
		if (!Loc.IsAsianLocale)
		{
			base.ScrollRowHeight = 5;
			base.EntryHeightOffset = -6;
		}
		else
		{
			base.ScrollRowHeight = 4;
			base.EntryHeightOffset = -2;
		}
	}

	public void PopulateEntries()
	{
		PopulateEntries(_items, doesSort: true);
	}

	public virtual void PopulateEntries(IEnumerable<InventoryItem> items, bool doesSort)
	{
		if (doesSort)
		{
			List<InventoryItem> list = items.ToList();
			list.Sort(CompareItemsForSorting);
			_items = list;
		}
		else
		{
			_items = items.ToList();
		}
		_keyToItemLookup.Clear();
		base.Entries.Clear();
		if (_doesAddUnequipEntry)
		{
			base.Entries.Add(new MenuEntry("---")
			{
				Description = "---"
			});
			_keyToItemLookup.Add(-1);
		}
		foreach (InventoryItem item2 in _items)
		{
			if (IsItemVisible(item2))
			{
				_keyToItemLookup.Add(item2.Key);
				MenuEntry item = CreateNewMenuEntryFromItem(item2);
				base.Entries.Add(item);
			}
		}
	}

	internal static int CompareItemsForSorting(InventoryItem itemA, InventoryItem itemB)
	{
		int num = itemA?.Key ?? 0;
		int value = itemB?.Key ?? 0;
		return num.CompareTo(value);
	}

	internal virtual MenuEntry CreateNewMenuEntryFromItem(InventoryItem item)
	{
		MenuEntry menuEntry = new MenuEntry(item.Name);
		menuEntry.Description = item.Description;
		return menuEntry;
	}

	internal virtual bool IsItemVisible(InventoryItem item)
	{
		return true;
	}

	internal void SelectItem(int key)
	{
		int num = -1;
		int num2 = 0;
		foreach (int item in KeyToItemLookup)
		{
			if (item == key)
			{
				num = num2;
				break;
			}
			num2++;
		}
		if (num >= 0)
		{
			SetSelectedIndex(num);
			RefreshScrollWindow();
		}
	}
}
