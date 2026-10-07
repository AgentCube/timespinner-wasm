using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameStateManagement.Screens.PauseMenu;

internal class SaveDataMenuEntryCollection : MenuEntryCollection
{
	private readonly bool _isIntegers;

	private readonly GameSave _saveFile;

	private readonly List<SaveDataMenuEntry> _saveDataMenuEntries = new List<SaveDataMenuEntry>();

	internal SaveDataMenuEntryCollection(GameSave saveFile, bool isIntegers)
	{
		_saveFile = saveFile;
		_isIntegers = isIntegers;
		base.ColumnCount = 1;
		base.ScrollRowHeight = 12;
		base.DoesMenuAllowScrolling = true;
		base.EntryHeightOffset = -6;
		PopulateEntries();
	}

	private void PopulateEntries()
	{
		if (!_isIntegers)
		{
			Dictionary<string, bool> dataKeyBools = _saveFile.DataKeyBools;
			foreach (KeyValuePair<string, bool> item4 in dataKeyBools)
			{
				SaveDataMenuEntry item = new SaveDataMenuEntry(item4.Key, item4.Value);
				_saveDataMenuEntries.Add(item);
				base.Entries.Add(item);
			}
			string[] array = new string[7]
			{
				"11_LabPower",
				"IsVileteSaved",
				"IsPastCleared",
				BossClass.GetSaveKeyByBossType(EBossType.Demon),
				BossClass.GetSaveKeyByBossType(EBossType.Maw),
				BossClass.GetSaveKeyByBossType(EBossType.Sorceress),
				"IsCantoranActive"
			};
			string[] array2 = array;
			foreach (string key in array2)
			{
				if (!dataKeyBools.ContainsKey(key))
				{
					SaveDataMenuEntry item2 = new SaveDataMenuEntry(key, boolValue: false);
					_saveDataMenuEntries.Add(item2);
					base.Entries.Add(item2);
				}
			}
			return;
		}
		Dictionary<string, int> dataKeyInts = _saveFile.DataKeyInts;
		foreach (KeyValuePair<string, int> item5 in dataKeyInts)
		{
			SaveDataMenuEntry item3 = new SaveDataMenuEntry(item5.Key, item5.Value);
			_saveDataMenuEntries.Add(item3);
			base.Entries.Add(item3);
		}
	}

	public override bool SelectEntry(PlayerIndex playerIndex)
	{
		if (base.Entries.Count > 0)
		{
			SaveDataMenuEntry saveDataMenuEntry = _saveDataMenuEntries[base.SelectedIndex];
			if (!_isIntegers)
			{
				bool boolValue = saveDataMenuEntry.BoolValue;
				bool value = !boolValue;
				_saveFile.DataKeyBools[saveDataMenuEntry.Key] = value;
				saveDataMenuEntry.SetValue(value);
			}
			else
			{
				int intValue = saveDataMenuEntry.IntValue;
				int value2 = (intValue + 1) % 10;
				_saveFile.DataKeyInts[saveDataMenuEntry.Key] = value2;
				saveDataMenuEntry.SetValue(value2);
			}
		}
		return true;
	}
}
