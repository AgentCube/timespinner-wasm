using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Inventory;

namespace Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

internal class MenuUseItemInventory : MenuInventoryCollection
{
	private const int ItemCountMargin = 22;

	private const int UseItemTextMarginX = -30;

	private readonly InventoryUseItemCollection _collection;

	private readonly Func<InventoryUseItem, bool> _onSelectedAction;

	public MenuUseItemInventory(InventoryUseItemCollection collection, Func<InventoryUseItem, bool> onSelected)
		: base(collection.Inventory.Values, doesAddUnequipEntry: false)
	{
		_collection = collection;
		_onSelectedAction = onSelected;
		base.TextMarginX = -30;
		PopulateEntries();
		foreach (MenuEntry entry in base.Entries)
		{
			entry.DoesConfirmationPlaySound = false;
		}
	}

	public InventoryUseItem GetSelected()
	{
		InventoryUseItem result = null;
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

	public override bool SelectEntry(PlayerIndex playerIndex)
	{
		bool result = true;
		int count = base.Entries.Count;
		if (count > 0 && count > base.SelectedIndex)
		{
			InventoryUseItem arg = _collection.Inventory[base.KeyToItemLookup[base.SelectedIndex]];
			result = _onSelectedAction(arg);
		}
		return result;
	}

	internal override EInventoryItemIcon GetSelectedIcon()
	{
		EInventoryItemIcon result = EInventoryItemIcon.None;
		int count = base.Entries.Count;
		if (count > 0 && base.SelectedIndex < count)
		{
			InventoryUseItem inventoryUseItem = _collection.Inventory[base.KeyToItemLookup[base.SelectedIndex]];
			result = InventoryItem.GetIconFromItem(inventoryUseItem.UseItemType);
		}
		return result;
	}

	public override void Draw(SpriteBatch spriteBatch, float zoom)
	{
		if (base.IsVisible)
		{
			int num = 0;
			if (base.Font != null)
			{
				Vector2 origin = new Vector2(0f, (float)base.Font.LineSpacing / 2f);
				float num2 = 22f * zoom;
				foreach (MenuEntry entry in base.Entries)
				{
					if (!base.DoesMenuAllowScrolling || !entry.IsScrolledOff)
					{
						InventoryUseItem inventoryUseItem = _collection.Inventory[base.KeyToItemLookup[num]];
						string text = inventoryUseItem.Count.ToString(CultureInfo.InvariantCulture);
						float num3 = base.Font.MeasureString(text).X * zoom;
						Vector2 vector = entry.DrawPosition.Add(new Point((int)((float)base.ColumnWidth - num3 - num2), 0));
						DrawingEx.DrawString(spriteBatch, base.Font, text, vector, MenuEntry.UnselectedColor, origin, zoom);
						if (base.DoesDisplayStoreMonetaryValue)
						{
							string text2 = ((int)Math.Ceiling((float)inventoryUseItem.MonetaryValue * base.MonetaryValueMultiplier)).ToString(CultureInfo.InvariantCulture);
							num3 = base.Font.MeasureString(text2).X * zoom;
							vector = vector.Add(new Point(base.ColumnWidth - (int)num3, 0));
							DrawingEx.DrawString(spriteBatch, base.Font, text2, vector, MenuEntry.UnselectedColor, origin, zoom);
						}
						if (!inventoryUseItem.IsItemUsableInMenu)
						{
							entry.OverrideDrawColor(MenuEntry.UnavailableColor);
						}
					}
					num++;
				}
			}
		}
		base.Draw(spriteBatch, zoom);
	}

	public bool RemoveItem(InventoryUseItem useItem)
	{
		bool result = false;
		_collection.RemoveItem(useItem.Key);
		if (_collection.GetItem(useItem.Key) == null)
		{
			PopulateEntries(_collection.Inventory.Values, doesSort: true);
			result = true;
		}
		return result;
	}
}
