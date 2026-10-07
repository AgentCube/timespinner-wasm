using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions.Inventory;

namespace Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

internal sealed class MenuEquipmentInventory : MenuInventoryWithIconCollection
{
	private const int ItemCountMargin = 22;

	private const int EquipmentTextMarginX = -32;

	private readonly bool _doesUnequip;

	private readonly EEquipmentSlotType _equipmentSlot;

	private readonly InventoryEquipmentCollection _collection;

	private readonly Action<InventoryEquipment> _onSelectedAction;

	private readonly Action _onUnequipAction;

	private int _lastRecentCount;

	internal EInventoryEquipmentType EquippedItem { get; set; }

	internal EInventoryEquipmentType EquippedItem2 { get; set; }

	public MenuEquipmentInventory(InventoryEquipmentCollection collection, List<EInventoryEquipmentType> recentEquipment, Action<InventoryEquipment> onSelected, Action onUnequip, EEquipmentSlotType equipmentSlot, bool doesAddUnequip, SpriteSheet pauseSprite)
		: base(collection.Inventory.Values, doesAddUnequip, pauseSprite)
	{
		_collection = collection;
		_onSelectedAction = onSelected;
		_onUnequipAction = onUnequip;
		_equipmentSlot = equipmentSlot;
		_doesUnequip = doesAddUnequip;
		base.TextMarginX = -32;
		PopulateEntries();
		AddRecentEntries(recentEquipment);
		base.IconFrameIndex = 111;
	}

	internal void AddRecentEntries(List<EInventoryEquipmentType> recentItems)
	{
		int index = (_doesUnequip ? 1 : 0);
		int count = recentItems.Count;
		for (int i = 0; i < _lastRecentCount; i++)
		{
			base.Entries.RemoveAt(index);
			base.KeyToItemLookup.RemoveAt(index);
		}
		int num = 0;
		string format = Loc.Get("EquipmentRecentDescription");
		for (int num2 = count - 1; num2 >= 0; num2--)
		{
			EInventoryEquipmentType eInventoryEquipmentType = recentItems[num2];
			int num3 = (int)eInventoryEquipmentType;
			if (_collection.Inventory.ContainsKey(num3))
			{
				InventoryEquipment item = _collection.Inventory[num3];
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

	internal override bool IsItemVisible(InventoryItem item)
	{
		bool result = false;
		if (item is InventoryEquipment { EquipmentType: not EInventoryEquipmentType.None } inventoryEquipment && inventoryEquipment.SlotType == _equipmentSlot)
		{
			result = true;
		}
		return result;
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
			InventoryEquipment inventoryEquipment = _collection.Inventory[base.KeyToItemLookup[base.SelectedIndex]];
			result = InventoryItem.GetIconFromItem(inventoryEquipment.EquipmentType);
		}
		return result;
	}

	public InventoryEquipment GetSelected()
	{
		InventoryEquipment result = null;
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

	internal override bool IsIconVisibleByIndex(int index)
	{
		bool result = false;
		int key = base.KeyToItemLookup[index];
		if (_collection.Inventory.ContainsKey(key))
		{
			InventoryEquipment inventoryEquipment = _collection.Inventory[key];
			if (inventoryEquipment != null && inventoryEquipment.EquipmentType != 0 && (inventoryEquipment.EquipmentType == EquippedItem || inventoryEquipment.EquipmentType == EquippedItem2))
			{
				result = true;
			}
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
						int num3 = base.KeyToItemLookup[num];
						if (num3 != -1)
						{
							InventoryEquipment inventoryEquipment = _collection.Inventory[num3];
							string text = inventoryEquipment.Count.ToString(CultureInfo.InvariantCulture);
							float num4 = base.Font.MeasureString(text).X * zoom;
							Vector2 vector = entry.DrawPosition.Add(new Point((int)((float)base.ColumnWidth - num4 - num2), 0));
							DrawingEx.DrawString(spriteBatch, base.Font, text, vector, MenuEntry.UnselectedColor, origin, zoom);
							if (base.DoesDisplayStoreMonetaryValue)
							{
								string text2 = ((int)Math.Ceiling((float)inventoryEquipment.MonetaryValue * base.MonetaryValueMultiplier)).ToString(CultureInfo.InvariantCulture);
								num4 = base.Font.MeasureString(text2).X * zoom;
								vector = vector.Add(new Point(base.ColumnWidth - (int)num4, 0));
								DrawingEx.DrawString(spriteBatch, base.Font, text2, vector, MenuEntry.UnselectedColor, origin, zoom);
							}
						}
					}
					num++;
				}
			}
		}
		base.Draw(spriteBatch, zoom);
	}
}
