using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions.Inventory;

namespace Timespinner.GameStateManagement.Screens.Shop;

internal class OrbShopMenuEntryCollection : MenuEntryCollection
{
	private const int ItemCountMargin = 71;

	private readonly EOrbSlot _slot;

	private readonly Rectangle _essenceGemIconFrameSource;

	private readonly Rectangle _goldRingIconFrameSource;

	private readonly Rectangle _goldNecklaceIconFrameSource;

	private readonly Rectangle _magicMarblesIconFrameSource;

	private readonly SpriteSheet _pauseMenuSpriteSheet;

	private readonly Action<OrbShopMenuEntry> _onSelectedAction;

	private readonly List<OrbShopMenuEntry> _items = new List<OrbShopMenuEntry>();

	private int _zoom;

	private int _itemCountMargin;

	public List<OrbShopMenuEntry> Items => _items;

	public OrbShopMenuEntryCollection(Action<OrbShopMenuEntry> onSelectAction, SpriteSheet pauseMenuSpriteSheet, EOrbSlot slot)
	{
		_slot = slot;
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
		_essenceGemIconFrameSource = _pauseMenuSpriteSheet.GetFrameSource(102);
		_goldRingIconFrameSource = _pauseMenuSpriteSheet.GetFrameSource(103);
		_goldNecklaceIconFrameSource = _pauseMenuSpriteSheet.GetFrameSource(104);
		_magicMarblesIconFrameSource = _pauseMenuSpriteSheet.GetFrameSource(140);
		RefreshSizes();
	}

	internal void RefreshSizes()
	{
		_zoom = Constants.InGameZoom;
		_itemCountMargin = 71 * _zoom;
	}

	public void AddEntries(IEnumerable<InventoryOrb> items)
	{
		foreach (InventoryOrb item2 in items)
		{
			if ((_slot == EOrbSlot.Spell && !item2.IsSpellUnlocked) || (_slot == EOrbSlot.Passive && !item2.IsPassiveUnlocked) || _slot == EOrbSlot.Melee)
			{
				OrbShopMenuEntry item = new OrbShopMenuEntry(item2, _slot);
				base.Entries.Add(item);
				_items.Add(item);
			}
		}
	}

	public void UpdateItems(int gems, int necklaces, int rings, int marbles)
	{
		foreach (OrbShopMenuEntry item in _items)
		{
			if (item.OrbSlot == EOrbSlot.Melee)
			{
				item.IsAffordable = marbles > 0;
			}
			else
			{
				item.IsAffordable = gems > 0 && ((item.OrbSlot == EOrbSlot.Spell) ? necklaces : rings) > 0;
			}
		}
	}

	public InventoryItem GetSelectedItem()
	{
		return GetSelectedShopEntry()?.Orb;
	}

	public OrbShopMenuEntry GetSelectedShopEntry()
	{
		OrbShopMenuEntry result = null;
		int count = base.Entries.Count;
		if (count > 0 && count > base.SelectedIndex)
		{
			result = _items[base.SelectedIndex];
		}
		return result;
	}

	internal override EInventoryItemIcon GetSelectedIcon()
	{
		EInventoryItemIcon result = EInventoryItemIcon.None;
		int count = base.Entries.Count;
		if (count > 0 && base.SelectedIndex < count)
		{
			OrbShopMenuEntry orbShopMenuEntry = _items[base.SelectedIndex];
			result = InventoryItem.GetIconFromItem(orbShopMenuEntry.Orb.OrbType, _slot);
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
				float num2 = base.Font.MeasureString("1").X * zoom;
				float num3 = 6f * zoom;
				float num4 = (num2 + num3) * 2f;
				Vector2 origin2 = new Vector2(4f, 4f);
				foreach (MenuEntry entry in base.Entries)
				{
					if (base.DoesMenuAllowScrolling && !entry.IsScrolledOff)
					{
						OrbShopMenuEntry orbShopMenuEntry = _items[num];
						Color color = (orbShopMenuEntry.IsAffordable ? MenuEntry.UnselectedColor : MenuEntry.UnavailableColor);
						Color color2 = (orbShopMenuEntry.IsAffordable ? Color.White : Color.Gray);
						Vector2 a = entry.DrawPosition.Add(new Point(base.ColumnWidth - _itemCountMargin, 0));
						Vector2 position = a.Add(new Point(base.ColumnWidth - (int)num4, 0));
						if (orbShopMenuEntry.OrbSlot == EOrbSlot.Melee)
						{
							spriteBatch.Draw(_pauseMenuSpriteSheet.Texture, position, _magicMarblesIconFrameSource, color2, 0f, origin2, zoom, SpriteEffects.None, 0f);
							DrawingEx.DrawString(drawPos: new Vector2(position.X + num3, position.Y), spriteBatch: spriteBatch, font: base.Font, text: "1", color: color, origin: origin, zoom: zoom);
						}
						else
						{
							spriteBatch.Draw(_pauseMenuSpriteSheet.Texture, position, _essenceGemIconFrameSource, color2, 0f, origin2, zoom, SpriteEffects.None, 0f);
							position = new Vector2(position.X + num3, position.Y);
							DrawingEx.DrawString(spriteBatch, base.Font, "1", position, color, origin, zoom);
							Rectangle value = ((orbShopMenuEntry.OrbSlot == EOrbSlot.Passive) ? _goldRingIconFrameSource : _goldNecklaceIconFrameSource);
							position = new Vector2(position.X + num3 * 2f, position.Y);
							spriteBatch.Draw(_pauseMenuSpriteSheet.Texture, position, value, color2, 0f, origin2, zoom, SpriteEffects.None, 0f);
							DrawingEx.DrawString(drawPos: new Vector2(position.X + num3, position.Y), spriteBatch: spriteBatch, font: base.Font, text: "1", color: color, origin: origin, zoom: zoom);
						}
					}
					num++;
				}
			}
		}
		base.Draw(spriteBatch, zoom);
	}

	public InventoryOrb GetSelected()
	{
		InventoryOrb result = null;
		if (base.SelectedIndex < _items.Count)
		{
			result = _items[base.SelectedIndex].Orb;
		}
		return result;
	}
}
