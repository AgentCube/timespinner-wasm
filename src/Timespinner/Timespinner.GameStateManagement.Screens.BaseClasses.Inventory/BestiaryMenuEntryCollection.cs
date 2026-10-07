using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Localization;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Saving;

namespace Timespinner.GameStateManagement.Screens.BaseClasses.Inventory;

internal class BestiaryMenuEntryCollection : MenuEntryCollection
{
	internal const string UnknownEnemyName = "???";

	private readonly BestiarySpecification _bestiary;

	private readonly GameSave _saveFile;

	private readonly SpriteFont _font;

	private readonly Action<BestiaryMenuEntry> _onEntrySelected;

	private readonly List<BestiaryMenuEntry> _bestiaryMenuEntries = new List<BestiaryMenuEntry>();

	public BestiaryMenuEntryCollection(GameSave inSave, SpriteFont font, BestiarySpecification bestiary, Action<BestiaryMenuEntry> onBestiaryEntrySelected)
	{
		_bestiary = bestiary;
		_font = font;
		_saveFile = inSave;
		_onEntrySelected = onBestiaryEntrySelected;
		base.ColumnCount = 1;
		if (!Loc.IsAsianLocale)
		{
			base.ScrollRowHeight = 12;
			base.EntryHeightOffset = -6;
		}
		else
		{
			base.ScrollRowHeight = 9;
			base.EntryHeightOffset = -2;
		}
		base.DoesMenuAllowScrolling = true;
		PopulateCollection();
	}

	private void PopulateCollection()
	{
		foreach (BestiaryEntrySpecification bestiaryEntry in _bestiary.BestiaryEntries)
		{
			if (!bestiaryEntry.IsEntryInvisible)
			{
				string key = string.Format(bestiaryEntry.Key.Replace("Enemy_", "KILL_"));
				int saveInt = _saveFile.GetSaveInt(key);
				bool flag = saveInt > 0;
				if (bestiaryEntry.VisibleName == null)
				{
					bestiaryEntry.VisibleName = Loc.Get(bestiaryEntry.Key + "_name");
				}
				if (bestiaryEntry.VisibleDescription == null)
				{
					bestiaryEntry.VisibleDescription = Loc.Get(bestiaryEntry.Key + "_desc");
				}
				string title = (flag ? bestiaryEntry.VisibleName : "???");
				BestiaryMenuEntry item = new BestiaryMenuEntry(bestiaryEntry, saveInt, title);
				_bestiaryMenuEntries.Add(item);
				base.Entries.Add(item);
			}
		}
	}

	public override bool SelectEntry(PlayerIndex playerIndex)
	{
		bool result = false;
		if (base.Entries.Count > 0)
		{
			BestiaryMenuEntry bestiaryMenuEntry = _bestiaryMenuEntries[base.SelectedIndex];
			_onEntrySelected(bestiaryMenuEntry);
			result = bestiaryMenuEntry.KillCount > 0;
		}
		return result;
	}

	public override void Draw(SpriteBatch spriteBatch, float zoom)
	{
		if (base.IsVisible)
		{
			Vector2 drawOffset = new Vector2(24f * zoom, 0f);
			foreach (BestiaryMenuEntry bestiaryMenuEntry in _bestiaryMenuEntries)
			{
				if (!bestiaryMenuEntry.IsScrolledOff)
				{
					Vector2 drawPosition = bestiaryMenuEntry.DrawPosition;
					DrawingEx.DrawString(origin: new Vector2(0f, (_font.LineSpacing + 1) / 2), spriteBatch: spriteBatch, font: _font, text: bestiaryMenuEntry.IndexString, drawPos: drawPosition, color: MenuEntry.UnselectedColor, zoom: zoom);
				}
				bestiaryMenuEntry.DrawOffset = drawOffset;
			}
		}
		base.Draw(spriteBatch, zoom);
	}

	public BestiaryMenuEntry ToggleNextEnemy(int indexChange)
	{
		BestiaryMenuEntry result = null;
		int count = base.Entries.Count;
		int i = 0;
		int num = base.SelectedIndex + indexChange;
		for (; i < count; i++)
		{
			if (num < 0)
			{
				num = count - 1;
			}
			else if (num >= count)
			{
				num = 0;
			}
			BestiaryMenuEntry bestiaryMenuEntry = _bestiaryMenuEntries[num];
			if (bestiaryMenuEntry.KillCount > 0)
			{
				base.SelectedIndex = num;
				RefreshScrollWindow();
				result = bestiaryMenuEntry;
				break;
			}
			num += indexChange;
		}
		return result;
	}
}
