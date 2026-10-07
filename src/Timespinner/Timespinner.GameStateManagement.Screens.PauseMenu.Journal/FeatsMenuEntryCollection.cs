using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameStateManagement.Screens.BaseClasses.Inventory;

namespace Timespinner.GameStateManagement.Screens.PauseMenu.Journal;

internal class FeatsMenuEntryCollection : MenuEntryCollection
{
	private readonly SpriteSheet _uiSprite;

	private readonly SpriteSheet _iconsSprite;

	private readonly GameFeatsManager _featsManager;

	private readonly SpriteFont _font;

	private readonly List<FeatsMenuEntry> _featsEntries = new List<FeatsMenuEntry>();

	internal FeatsMenuEntryCollection(GameFeatsManager featsManager, SpriteSheet uiSprite, SpriteSheet iconsSprite, SpriteFont font)
	{
		_featsManager = featsManager;
		_uiSprite = uiSprite;
		_iconsSprite = iconsSprite;
		_font = font;
		base.ColumnCount = 1;
		base.ScrollRowHeight = 4;
		base.EntryHeightOffset = 15;
		base.DoesMenuAllowScrolling = true;
		PopulateCollection();
	}

	private void PopulateCollection()
	{
		string text = Loc.Get("feat_name_hidden");
		string text2 = Loc.Get("feat_desc_hidden");
		foreach (string key in _featsManager.Feats.Keys)
		{
			GameFeat gameFeat = _featsManager.Feats[key];
			bool flag = !gameFeat.IsUnlocked;
			bool flag2 = gameFeat.IsHidden && !gameFeat.IsUnlocked;
			string text3 = (flag ? text : gameFeat.Name);
			string description = (flag2 ? text2 : gameFeat.Description);
			FeatsMenuEntry featsMenuEntry = new FeatsMenuEntry(text3, gameFeat, _uiSprite, _iconsSprite);
			featsMenuEntry.Description = description;
			FeatsMenuEntry item = featsMenuEntry;
			base.Entries.Add(item);
			_featsEntries.Add(item);
		}
	}

	public override void Draw(SpriteBatch spriteBatch, float zoom)
	{
		if (base.Font == null || !base.IsVisible)
		{
			return;
		}
		foreach (FeatsMenuEntry featsEntry in _featsEntries)
		{
			if (!featsEntry.IsScrolledOff)
			{
				featsEntry.Draw(spriteBatch, _font, zoom, 1f);
			}
		}
	}
}
