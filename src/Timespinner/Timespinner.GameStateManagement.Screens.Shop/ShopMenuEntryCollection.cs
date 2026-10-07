using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions.Inventory;

namespace Timespinner.GameStateManagement.Screens.Shop;

internal class ShopMenuEntryCollection : MenuEntryCollection
{
	private const int LeftArrowMarginX = -15;

	private const int RightArrowMarginX = 1;

	private const int ItemCountMargin = -4;

	private const int ItemCostMarginX = -22;

	private readonly bool _isBuying;

	private readonly float _priceModifier;

	private readonly SpriteSheet _pauseMenuSpriteSheet;

	private readonly Action<ShopMenuEntry> _onSelectedAction;

	private readonly List<ShopMenuEntry> _items = new List<ShopMenuEntry>();

	private int _itemCountMargin;

	private int _itemCostMarginX;

	private int _leftArrowMarginX;

	private int _rightArrowMarginX;

	private int _arrowDrawOffsetY;

	private int _zoom;

	public List<ShopMenuEntry> Items => _items;

	public ShopMenuEntryCollection(float priceModifier, Action<ShopMenuEntry> onSelectAction, SpriteSheet pauseMenuSpriteSheet, bool isBuying)
	{
		_isBuying = isBuying;
		_priceModifier = priceModifier;
		_onSelectedAction = onSelectAction;
		_pauseMenuSpriteSheet = pauseMenuSpriteSheet;
		base.ColumnCount = 1;
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
		RefreshSizes();
	}

	internal void RefreshSizes()
	{
		_zoom = Constants.InGameZoom;
		_itemCountMargin = -4 * _zoom;
		_itemCostMarginX = -22 * _zoom;
		_leftArrowMarginX = -15 * _zoom;
		_rightArrowMarginX = _zoom;
		_arrowDrawOffsetY = -4 * _zoom;
	}

	public void AddEntries(IEnumerable<InventoryItem> items, EInventoryCategoryType itemType)
	{
		foreach (InventoryItem item4 in items)
		{
			if (item4.Category != EInventoryCategoryType.Orb)
			{
				if (_isBuying || item4.MonetaryValue > 0)
				{
					ShopMenuEntry shopMenuEntry = new ShopMenuEntry(item4, itemType);
					shopMenuEntry.ShopPrice = (int)((float)item4.MonetaryValue * _priceModifier);
					ShopMenuEntry item = shopMenuEntry;
					base.Entries.Add(item);
					_items.Add(item);
				}
			}
			else if (item4 is InventoryOrb inventoryOrb)
			{
				if (inventoryOrb.IsSpellUnlocked)
				{
					ShopMenuEntry shopMenuEntry2 = new ShopMenuEntry(inventoryOrb, EOrbSlot.Spell);
					shopMenuEntry2.ShopPrice = (int)((float)item4.MonetaryValue * _priceModifier);
					ShopMenuEntry item2 = shopMenuEntry2;
					base.Entries.Add(item2);
					_items.Add(item2);
				}
				if (inventoryOrb.IsPassiveUnlocked)
				{
					ShopMenuEntry shopMenuEntry3 = new ShopMenuEntry(inventoryOrb, EOrbSlot.Passive);
					shopMenuEntry3.ShopPrice = (int)((float)item4.MonetaryValue * _priceModifier);
					ShopMenuEntry item3 = shopMenuEntry3;
					base.Entries.Add(item3);
					_items.Add(item3);
				}
			}
		}
	}

	public void UpdateItems(int playerMoney)
	{
		foreach (ShopMenuEntry item in _items)
		{
			item.IsAffordable = playerMoney >= item.TotalPrice;
		}
	}

	public InventoryItem GetSelectedItem()
	{
		return GetSelectedShopEntry()?.Item;
	}

	internal override EInventoryItemIcon GetSelectedIcon()
	{
		EInventoryItemIcon result = EInventoryItemIcon.None;
		int count = base.Entries.Count;
		if (count > 0 && base.SelectedIndex < count)
		{
			ShopMenuEntry shopMenuEntry = _items[base.SelectedIndex];
			result = ((shopMenuEntry.OrbType == EInventoryOrbType.None) ? InventoryItem.GetIconFromItem(shopMenuEntry.Item) : InventoryItem.GetIconFromItem(shopMenuEntry.OrbType, shopMenuEntry.OrbSlot));
		}
		return result;
	}

	public ShopMenuEntry GetSelectedShopEntry()
	{
		ShopMenuEntry result = null;
		int count = base.Entries.Count;
		if (count > 0 && count > base.SelectedIndex)
		{
			result = _items[base.SelectedIndex];
		}
		return result;
	}

	public void RemoveItemAt(int index)
	{
		base.Entries.RemoveAt(index);
		Items.RemoveAt(index);
		int count = base.Entries.Count;
		if (base.SelectedIndex >= count)
		{
			base.SelectedIndex = Math.Max(0, base.SelectedIndex - 1);
			RefreshScrollWindow();
		}
	}

	public override bool SelectEntry(PlayerIndex playerIndex)
	{
		if (base.Entries.Count > 0 && base.Entries.Count > base.SelectedIndex)
		{
			_onSelectedAction(_items[base.SelectedIndex]);
		}
		return false;
	}

	public override void Draw(SpriteBatch spriteBatch, float zoom)
	{
		if (base.IsVisible)
		{
			int num = 0;
			if (base.Font != null)
			{
				Vector2 origin = new Vector2(0f, (float)base.Font.LineSpacing / 2f);
				foreach (MenuEntry entry in base.Entries)
				{
					if (base.DoesMenuAllowScrolling && !entry.IsScrolledOff)
					{
						ShopMenuEntry shopMenuEntry = _items[num];
						string text = shopMenuEntry.QuanityToBuy.ToString(CultureInfo.InvariantCulture);
						float num2 = base.Font.MeasureString(text).X * zoom;
						Color color = ((!_isBuying || (shopMenuEntry.IsAffordable && shopMenuEntry.IsAvailable)) ? MenuEntry.UnselectedColor : MenuEntry.UnavailableColor);
						Vector2 a = entry.DrawPosition.Add(new Point(base.ColumnWidth - _itemCountMargin, 0));
						Vector2 drawPos = a.Add(new Point(-(int)num2, 0));
						DrawingEx.DrawString(spriteBatch, base.Font, text, drawPos, color, origin, zoom);
						if (entry.WasSelected && num == base.SelectedIndex)
						{
							Rectangle frameSource = _pauseMenuSpriteSheet.GetFrameSource(56);
							Vector2 position = a.Add(new Point(_leftArrowMarginX, _arrowDrawOffsetY));
							spriteBatch.Draw(_pauseMenuSpriteSheet.Texture, position, frameSource, Color.White, 0f, Vector2.Zero, _zoom, SpriteEffects.FlipHorizontally, 0f);
							position = a.Add(new Point(_rightArrowMarginX, _arrowDrawOffsetY));
							spriteBatch.Draw(_pauseMenuSpriteSheet.Texture, position, frameSource, Color.White, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
						}
						string text2 = shopMenuEntry.TotalPrice.ToString(CultureInfo.InvariantCulture);
						num2 = base.Font.MeasureString(text2).X * zoom;
						drawPos = a.Add(new Point(base.ColumnWidth - (int)num2 + _itemCostMarginX, 0));
						DrawingEx.DrawString(spriteBatch, base.Font, text2, drawPos, color, origin, zoom);
					}
					num++;
				}
			}
		}
		base.Draw(spriteBatch, zoom);
	}
}
