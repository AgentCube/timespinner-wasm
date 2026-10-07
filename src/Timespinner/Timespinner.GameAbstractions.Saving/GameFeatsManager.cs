using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steamworks;
using Timespinner.Core.Constants;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions.Saving;

public class GameFeatsManager
{
	private readonly object _featUploadLock;

	private readonly List<GameFeat> _featsToUpload = new List<GameFeat>();

	private readonly Dictionary<string, GameFeat> _feats = new Dictionary<string, GameFeat>();

	private bool _isUploadingFeats;

	private GameFeatsSave _featsSave;

	internal Dictionary<string, GameFeat> Feats => _feats;

	public GameFeatsManager()
	{
		_featUploadLock = new object();
		PopulateAllFeats();
	}

	internal void InitializePostLoad(GameFeatsSave featsSave)
	{
		_featsSave = featsSave;
		foreach (string key in _featsSave.FeatsData.Keys)
		{
			if (_feats.ContainsKey(key))
			{
				_feats[key].InitializeFromSave(_featsSave.FeatsData[key]);
			}
		}
	}

	private void PopulateAllFeats()
	{
		foreach (EGameFeatType value in Enum.GetValues(typeof(EGameFeatType)))
		{
			switch (value)
			{
			case EGameFeatType.Boss:
				foreach (EBossType value2 in Enum.GetValues(typeof(EBossType)))
				{
					if (value2 != 0)
					{
						GameFeat gameFeat2 = new GameFeat(value, value2);
						_feats.Add(gameFeat2.Key, gameFeat2);
					}
				}
				break;
			default:
			{
				GameFeat gameFeat = new GameFeat(value);
				_feats.Add(gameFeat.Key, gameFeat);
				break;
			}
			case EGameFeatType.None:
				break;
			}
		}
	}

	internal void UnlockFeat(EGameFeatType basicType)
	{
		string key = $"{basicType}";
		SaveFeatUnlock(key);
	}

	internal bool UnlockFeat(EBossType bossType, int bossDeathData)
	{
		string key = $"{EGameFeatType.Boss}_{bossType}";
		bool result = false;
		bool flag = false;
		bool flag2 = (bossDeathData & 8) != 0;
		if (flag2)
		{
			flag = GetArePerfectBossesComplete(this);
		}
		SaveFeatUnlock(key, bossDeathData);
		if (flag2 && !flag)
		{
			bool arePerfectBossesComplete = GetArePerfectBossesComplete(this);
			result = arePerfectBossesComplete;
		}
		return result;
	}

	internal void SaveFeatUnlock(string key)
	{
		SaveFeatUnlock(key, 1);
	}

	internal void SaveFeatUnlock(string key, int value)
	{
		if (Feats.ContainsKey(key) && !Timespinner.Core.Constants.Constants.IsAnySpeedrunActive)
		{
			GameFeat gameFeat = Feats[key];
			gameFeat.Unlock();
			gameFeat.ProgressValue |= value;
			_featsToUpload.Add(gameFeat);
			_featsSave.FeatsData[key] = gameFeat.ProgressValue;
			UnlockFinalFeat();
		}
	}

	private void UnlockFinalFeat()
	{
		bool flag = true;
		bool flag2 = false;
		foreach (GameFeat value in _feats.Values)
		{
			if (value.IsUnlocked && value.Type == EGameFeatType.AllFeats)
			{
				flag2 = true;
			}
			if (!value.IsUnlocked && value.Type != EGameFeatType.AllFeats && value.Type != 0)
			{
				flag = false;
				break;
			}
		}
		if (flag && !flag2)
		{
			string key = $"{EGameFeatType.AllFeats}";
			SaveFeatUnlock(key);
		}
	}

	internal void UnlockFamiliarAchievement(InventoryFamiliarCollection familiarInventory)
	{
		UnlockFeat(EGameFeatType.GetFamiliar);
		bool flag = true;
		foreach (EInventoryFamiliarType value in Enum.GetValues(typeof(EInventoryFamiliarType)))
		{
			if (value != 0 && !familiarInventory.Inventory.ContainsKey((int)value))
			{
				flag = false;
			}
			if (!flag)
			{
				break;
			}
		}
		if (flag)
		{
			UnlockFeat(EGameFeatType.GetAllFamiliars);
		}
	}

	internal void UnlockOrbAchievement(InventoryOrbCollection orbInventory, EOrbSlot slot, EInventoryOrbType orbType)
	{
		if ((slot == EOrbSlot.Melee || slot == EOrbSlot.All) && orbType != EInventoryOrbType.Blue)
		{
			UnlockFeat(EGameFeatType.GetOrb);
		}
		if ((slot == EOrbSlot.Melee || slot == EOrbSlot.All) && AreAllOrbsUnlocked(orbInventory, EOrbSlot.Melee))
		{
			UnlockFeat(EGameFeatType.GetAllOrbs);
		}
		if (slot == EOrbSlot.Spell && orbType != EInventoryOrbType.Blue)
		{
			UnlockFeat(EGameFeatType.MakeSpell);
		}
		if (slot == EOrbSlot.Spell && AreAllOrbsUnlocked(orbInventory, EOrbSlot.Spell))
		{
			UnlockFeat(EGameFeatType.MakeAllSpells);
		}
		if (slot == EOrbSlot.Passive)
		{
			UnlockFeat(EGameFeatType.MakePassive);
		}
		if (slot == EOrbSlot.Passive && AreAllOrbsUnlocked(orbInventory, EOrbSlot.Passive))
		{
			UnlockFeat(EGameFeatType.MakeAllPassives);
		}
	}

	private static bool AreAllOrbsUnlocked(InventoryOrbCollection orbCollection, EOrbSlot slot)
	{
		bool flag = true;
		foreach (EInventoryOrbType value in Enum.GetValues(typeof(EInventoryOrbType)))
		{
			if (value != 0 && value != EInventoryOrbType.Monske && value != EInventoryOrbType.Umbra)
			{
				if (orbCollection.Inventory.ContainsKey((int)value))
				{
					InventoryOrb inventoryOrb = orbCollection.Inventory[(int)value];
					switch (slot)
					{
					case EOrbSlot.Spell:
						if (!inventoryOrb.IsSpellUnlocked)
						{
							flag = false;
						}
						break;
					case EOrbSlot.Passive:
						if (!inventoryOrb.IsPassiveUnlocked)
						{
							flag = false;
						}
						break;
					}
				}
				else
				{
					flag = false;
				}
			}
			if (!flag)
			{
				break;
			}
		}
		return flag;
	}

	private static bool GetArePerfectBossesComplete(GameFeatsManager featsManager)
	{
		bool result = true;
		foreach (EBossType value in Enum.GetValues(typeof(EBossType)))
		{
			if (value != 0)
			{
				string key = $"{EGameFeatType.Boss}_{value}";
				if (!featsManager.Feats.ContainsKey(key))
				{
					result = false;
					break;
				}
				GameFeat gameFeat = featsManager.Feats[key];
				if ((gameFeat.ProgressValue & 8) <= 0)
				{
					result = false;
					break;
				}
			}
		}
		return result;
	}

	internal void UnlockJournalAchievement(InventoryJournalCollection journalInventory, EInventoryJournalType journalType)
	{
		EJournalCategoryType journalCategory = InventoryJournal.GetJournalCategory(journalType);
		bool flag = false;
		if (journalCategory == EJournalCategoryType.Memories && journalInventory.AreAllEntriesInCategoryFound(EJournalCategoryType.Memories))
		{
			flag = true;
			UnlockFeat(EGameFeatType.GetAllMemories);
		}
		if (journalCategory == EJournalCategoryType.Letters && journalInventory.AreAllEntriesInCategoryFound(EJournalCategoryType.Letters))
		{
			flag = true;
			UnlockFeat(EGameFeatType.GetAllLetters);
		}
		if (journalCategory == EJournalCategoryType.Files && journalInventory.AreAllEntriesInCategoryFound(EJournalCategoryType.Files))
		{
			flag = true;
			UnlockFeat(EGameFeatType.GetAllFiles);
		}
		if (flag && journalInventory.AreAllEntriesInCategoryFound(EJournalCategoryType.Memories) && journalInventory.AreAllEntriesInCategoryFound(EJournalCategoryType.Letters) && journalInventory.AreAllEntriesInCategoryFound(EJournalCategoryType.Files))
		{
			UnlockFeat(EGameFeatType.GetAllJournals);
		}
	}

	internal void Update()
	{
		if (_isUploadingFeats || _featsToUpload.Count <= 0)
		{
			return;
		}
		_isUploadingFeats = true;
		List<GameFeat> list = new List<GameFeat>();
		foreach (GameFeat item in _featsToUpload)
		{
			list.Add(item);
		}
		_featsToUpload.Clear();
		UploadFeats(list);
	}

	private void UploadFeats(IList<GameFeat> feats)
	{
		if (OperatingSystem.IsBrowser())
		{
			UploadSteamAchievements(feats);
		}
		else
		{
			Task task = new Task(delegate
			{
				UploadSteamAchievements(feats);
			});
			task.Start();
		}
	}

	private void UploadSteamAchievements(IEnumerable<GameFeat> feats)
	{
		lock (_featUploadLock)
		{
			try
			{
				foreach (GameFeat feat in feats)
				{
					SteamUserStats.SetAchievement(feat.Icon.ToString());
				}
				SteamUserStats.StoreStats();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Failed to Unlock Steam Achievements: " + ex);
			}
			_isUploadingFeats = false;
		}
	}
}
