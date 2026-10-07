using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions.Inventory;

namespace Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

internal class MenuRelicInventory : MenuInventoryCollection
{
	private const int OnStatusMargin = 10;

	private const int RelicTextMarginX = -28;

	private const float BaseButtonDrawOffsetX = -32f;

	private const float BaseButtonDrawOffsetY = -6f;

	private const string DefaultOnText = "ON";

	private const string DefaultOffText = "OFF";

	private const string LocOnTextKey = "RelicsMenuOn";

	private const string LocOffTextKey = "RelicsMenuOff";

	private static readonly Color ShadowColor = new Color(32, 24, 24);

	private readonly string _onText;

	private readonly string _offText;

	private readonly InventoryRelicCollection _collection;

	private readonly Action<InventoryRelic> _onSelectedAction;

	private readonly SpriteSheet _sprite;

	public MenuRelicInventory(InventoryRelicCollection collection, Action<InventoryRelic> onSelected, SpriteSheet sprite)
		: base(collection.Inventory.Values, doesAddUnequipEntry: false)
	{
		_sprite = sprite;
		_collection = collection;
		_onSelectedAction = onSelected;
		base.TextMarginX = -28;
		base.EntryHeightOffset = -4;
		_onText = (Loc.DoesExist("RelicsMenuOn") ? Loc.Get("RelicsMenuOn") : "ON");
		_offText = (Loc.DoesExist("RelicsMenuOff") ? Loc.Get("RelicsMenuOff") : "OFF");
		PopulateEntries();
	}

	public override bool SelectEntry(PlayerIndex playerIndex)
	{
		if (base.Entries.Count > 0)
		{
			_onSelectedAction(_collection.Inventory[base.KeyToItemLookup[base.SelectedIndex]]);
		}
		return true;
	}

	internal override EInventoryItemIcon GetSelectedIcon()
	{
		EInventoryItemIcon result = EInventoryItemIcon.None;
		int count = base.Entries.Count;
		if (count > 0 && base.SelectedIndex < count)
		{
			InventoryRelic inventoryRelic = _collection.Inventory[base.KeyToItemLookup[base.SelectedIndex]];
			result = InventoryItem.GetIconFromItem(inventoryRelic.RelicType);
		}
		return result;
	}

	public override void Draw(SpriteBatch spriteBatch, float zoom)
	{
		int num = 0;
		if (base.Font != null)
		{
			Vector2 origin = new Vector2(0f, (float)base.Font.LineSpacing / 2f);
			float num2 = 10f * zoom;
			Vector2 vector = new Vector2((float)base.ColumnWidth + -32f * zoom, -6f * zoom);
			foreach (MenuEntry entry in base.Entries)
			{
				if (!entry.IsScrolledOff)
				{
					InventoryRelic inventoryRelic = _collection.Inventory[base.KeyToItemLookup[num]];
					bool isActive = inventoryRelic.IsActive;
					Vector2 position = entry.DrawPosition + vector;
					Rectangle frameSource = _sprite.GetFrameSource(isActive ? 95 : 94);
					spriteBatch.Draw(_sprite.Texture, position, frameSource, Color.White, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
					Color color = (inventoryRelic.IsActive ? MenuEntry.UnselectedColor : MenuEntry.UnavailableColor);
					string text = (isActive ? _onText : _offText);
					float num3 = base.Font.MeasureString(text).X * zoom;
					position = entry.DrawPosition.Add(new Point((int)((float)base.ColumnWidth - num3 - num2), 0));
					if (isActive)
					{
						DrawingEx.DrawString(spriteBatch, base.Font, text, position, ShadowColor, origin, zoom);
						position = new Vector2(position.X - zoom, position.Y);
					}
					DrawingEx.DrawString(spriteBatch, base.Font, text, position, color, origin, zoom);
				}
				num++;
			}
		}
		base.Draw(spriteBatch, zoom);
	}
}
