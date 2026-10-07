using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions.Saving;

namespace Timespinner.GameStateManagement.Screens.MainMenu;

internal class SaveFileMenuEntryCollection : MenuEntryCollection
{
	private readonly int _zoom;

	private readonly string _blankSaveString;

	private readonly SpriteSheet _sprite;

	private readonly Action<SaveFileMenuEntry, PlayerIndex> _onSelectedAction;

	private readonly List<SaveFileMenuEntry> _saveFiles = new List<SaveFileMenuEntry>();

	internal bool IsSelectingNonBlankSave => !_saveFiles[base.SelectedIndex].IsEmptySaveSlot;

	internal List<SaveFileMenuEntry> SaveFiles => _saveFiles;

	internal float DeletePercentage { get; set; }

	internal SaveFileMenuEntryCollection(IEnumerable<GameSave> saveFiles, Action<SaveFileMenuEntry, PlayerIndex> onSelectedAction, SpriteFont font, SpriteSheet sprite, int zoom)
	{
		_onSelectedAction = onSelectedAction;
		base.Font = font;
		_sprite = sprite;
		_zoom = zoom;
		base.DoesMenuSlideOnTransition = false;
		string mapString = Loc.Get("SaveSelectMapRate");
		string playtimeString = Loc.Get("SaveSelectTime");
		string levelString = Loc.Get("SaveSelectLevel");
		_blankSaveString = Loc.Get("SaveSelectBlankSave");
		int i = 0;
		foreach (GameSave saveFile in saveFiles)
		{
			for (; saveFile.SaveFileIndex > i; i++)
			{
				SaveFileMenuEntry item = new SaveFileMenuEntry(null, base.Font, zoom, "", "", _blankSaveString);
				_saveFiles.Add(item);
				base.Entries.Add(item);
			}
			SaveFileMenuEntry item2 = new SaveFileMenuEntry(saveFile, base.Font, zoom, mapString, playtimeString, levelString);
			_saveFiles.Add(item2);
			base.Entries.Add(item2);
			i++;
		}
		for (; i < 8; i++)
		{
			SaveFileMenuEntry item3 = new SaveFileMenuEntry(null, base.Font, zoom, mapString, playtimeString, _blankSaveString);
			_saveFiles.Add(item3);
			base.Entries.Add(item3);
		}
	}

	internal void RefreshSizes(int zoom)
	{
		foreach (SaveFileMenuEntry saveFile in _saveFiles)
		{
			saveFile.RefreshSizes(zoom);
		}
	}

	internal bool IsSelectingClearedSave()
	{
		bool result = false;
		SaveFileMenuEntry saveFileMenuEntry = _saveFiles[base.SelectedIndex];
		if (!saveFileMenuEntry.IsEmptySaveSlot && saveFileMenuEntry.SaveFile != null && saveFileMenuEntry.SaveFile.IsGameCleared)
		{
			result = true;
		}
		return result;
	}

	internal void Remove(int selectedIndex)
	{
		_saveFiles.RemoveAt(base.SelectedIndex);
		base.Entries.RemoveAt(base.SelectedIndex);
	}

	public override bool SelectEntry(PlayerIndex playerIndex)
	{
		if (base.Entries.Count > 0 && base.Entries.Count > base.SelectedIndex)
		{
			_onSelectedAction(_saveFiles[base.SelectedIndex], playerIndex);
		}
		return false;
	}

	public override void Draw(SpriteBatch spriteBatch, float zoom)
	{
		int num = 0;
		if (base.Font == null || !base.IsVisible)
		{
			return;
		}
		foreach (SaveFileMenuEntry saveFile in _saveFiles)
		{
			if (!saveFile.IsScrolledOff)
			{
				saveFile.Draw(spriteBatch, _sprite, 1f, base.SelectedIndex == num, DeletePercentage);
			}
			num++;
		}
	}

	public void DeleteSelectedFile()
	{
		SaveFileMenuEntry value = new SaveFileMenuEntry(null, base.Font, _zoom, "", "", _blankSaveString);
		_saveFiles[base.SelectedIndex] = value;
		base.Entries[base.SelectedIndex] = value;
	}
}
