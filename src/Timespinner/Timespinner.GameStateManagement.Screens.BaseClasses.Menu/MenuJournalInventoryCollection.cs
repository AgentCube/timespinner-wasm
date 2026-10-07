using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions.Inventory;

namespace Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

internal sealed class MenuJournalInventoryCollection : MenuInventoryWithIconCollection
{
	private const int JournalCategoryEnumOffset = 32;

	private const int RatioOffsetX = 156;

	private const int RatioOffsetY = 112;

	private const string FoundRatioStringFormat = "{0}/{1}";

	private static readonly Color ShadowColor = new Color(32, 24, 24);

	private readonly int _ownedJournalCount;

	private readonly int _totalPossibleJournalCount;

	private readonly string _foundRatioString;

	private readonly EJournalCategoryType _categoryType;

	private readonly InventoryJournalCollection _collection;

	private readonly Action<InventoryJournal> _onSelectedAction;

	public MenuJournalInventoryCollection(InventoryJournalCollection collection, Action<InventoryJournal> onSelected, EJournalCategoryType category, SpriteSheet pauseSprite)
		: base(collection.Inventory.Values, doesAddUnequipEntry: false, pauseSprite)
	{
		_collection = collection;
		_onSelectedAction = onSelected;
		_categoryType = category;
		base.ColumnCount = 1;
		base.ScrollRowHeight = ((!Loc.IsAsianLocale) ? 12 : 9);
		List<InventoryJournal> list = new List<InventoryJournal>();
		foreach (InventoryJournal value in collection.Inventory.Values)
		{
			int journalType = (int)value.JournalType;
			switch (_categoryType)
			{
			case EJournalCategoryType.Memories:
				if (journalType < 32)
				{
					list.Add(value);
				}
				break;
			case EJournalCategoryType.Letters:
				if (journalType < 64 && journalType >= 32)
				{
					list.Add(value);
				}
				break;
			case EJournalCategoryType.Files:
				if (journalType >= 64)
				{
					list.Add(value);
				}
				break;
			}
		}
		PopulateEntries(list, doesSort: true);
		_ownedJournalCount = list.Count;
		switch (category)
		{
		case EJournalCategoryType.Memories:
			_totalPossibleJournalCount = 11;
			break;
		case EJournalCategoryType.Letters:
			_totalPossibleJournalCount = 11;
			break;
		case EJournalCategoryType.Files:
			_totalPossibleJournalCount = 14;
			break;
		}
		_foundRatioString = $"{_ownedJournalCount}/{_totalPossibleJournalCount}";
		base.IconFrameIndex = 112;
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
			InventoryJournal inventoryJournal = _collection.Inventory[base.KeyToItemLookup[base.SelectedIndex]];
			result = InventoryItem.GetIconFromItem(inventoryJournal.JournalType);
		}
		return result;
	}

	internal override bool IsIconVisibleByIndex(int index)
	{
		bool result = false;
		int key = base.KeyToItemLookup[index];
		if (_collection.Inventory.ContainsKey(key))
		{
			InventoryJournal inventoryJournal = _collection.Inventory[key];
			result = !inventoryJournal.IsRead;
		}
		return result;
	}

	public override void Draw(SpriteBatch spriteBatch, float zoom)
	{
		base.Draw(spriteBatch, zoom);
		if (base.IsVisible && _ownedJournalCount > 0)
		{
			float num = (float)(Loc.IsAsianLocale ? (-2) : 0) * zoom;
			float num2 = (float)(Loc.IsAsianLocale ? 3 : 0) * zoom;
			DrawShadowedString(drawPosition: new Vector2(base.DrawPosition.X + 156f * zoom + num, base.DrawPosition.Y + 112f * zoom + num2), spriteBatch: spriteBatch, font: base.Font, text: _foundRatioString, drawColor: MenuEntry.UnselectedColor, zoom: (int)zoom);
		}
	}

	private static void DrawShadowedString(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 drawPosition, Color drawColor, int zoom)
	{
		DrawingEx.DrawString(spriteBatch, font, text, drawPosition.Add(new Point(0, zoom)), ShadowColor, Vector2.Zero, zoom);
		DrawingEx.DrawString(spriteBatch, font, text, drawPosition, drawColor, Vector2.Zero, zoom);
	}
}
