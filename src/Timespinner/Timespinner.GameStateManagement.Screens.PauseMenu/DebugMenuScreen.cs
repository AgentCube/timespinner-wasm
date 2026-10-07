using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Cutscene;
using Timespinner.GameObjects.Heroes;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

namespace Timespinner.GameStateManagement.Screens.PauseMenu;

internal class DebugMenuScreen : InventoryMenuScreen
{
	private const float MapBackgroundDrawOffsetY = 7f / 64f;

	private readonly Level _level;

	private readonly Action _onExitAction;

	private readonly Action _fullExitAction;

	private Rectangle _mapBackgroundDrawRectangle;

	public DebugMenuScreen(GameSave inSave, GCM gcm, Level inLevel, Action onExitAction, Action fullExitAction)
		: base("Debug", inSave, gcm, fullExitAction)
	{
		_level = inLevel;
		_onExitAction = onExitAction;
		_fullExitAction = fullExitAction;
		base.DoesDrawBrackets = false;
		base.DoesHaveWideColumns = true;
		base.DoesDrawTopLowerFrame = false;
		_doesUseCursor = true;
		_primaryMenuCollection.ColumnCount = 2;
		_primaryMenuCollection.SetColumnWidth(140 * Constants.InGameZoom, base.Zoom);
		MenuEntry menuEntry = new MenuEntry("Set Save Variables")
		{
			Description = "Set save bools and integers."
		};
		menuEntry.Selected += OnSaveDataEntrySelected;
		base.MenuEntries.Add(menuEntry);
		MenuEntry menuEntry2 = new MenuEntry("Unlock All Items")
		{
			Description = "Unlocks all orbs, usable items, and abilities."
		};
		EventHandler<PlayerIndexEventArgs> value = delegate
		{
			UnlockItems();
		};
		menuEntry2.Selected += value;
		base.MenuEntries.Add(menuEntry2);
		MenuEntry menuEntry3 = new MenuEntry("Gimme Cash")
		{
			Description = "Drops some fat cash in yo wallet"
		};
		menuEntry3.Selected += delegate
		{
			DropCash();
		};
		base.MenuEntries.Add(menuEntry3);
		MenuEntry menuEntry4 = new MenuEntry("Gimme Exp")
		{
			Description = "Gives 5000 Experience Points."
		};
		menuEntry4.Selected += delegate
		{
			GiveExp();
		};
		base.MenuEntries.Add(menuEntry4);
		MenuEntry menuEntry5 = new MenuEntry("Reveal Entire Map")
		{
			Description = "Unhides every room in the game's minimap"
		};
		menuEntry5.Selected += delegate
		{
			RevealMap();
		};
		base.MenuEntries.Add(menuEntry5);
		MenuEntry menuEntry6 = new MenuEntry("Hide Known But Not Visisted Map")
		{
			Description = "Hides every room that hasn't been vissited in the game's minimap"
		};
		menuEntry6.Selected += delegate
		{
			HideMap();
		};
		base.MenuEntries.Add(menuEntry6);
		MenuEntry menuEntry7 = new MenuEntry("Unlock All Journal Entries")
		{
			Description = "Get All Memories and Journals."
		};
		menuEntry7.Selected += delegate
		{
			UnlockJournal();
		};
		base.MenuEntries.Add(menuEntry7);
		MenuEntry menuEntry8 = new MenuEntry("Unlock All Melee orbs")
		{
			Description = "Unlocks all melee orbs only"
		};
		menuEntry8.Selected += delegate
		{
			UnlockMeleeOrbs();
		};
		base.MenuEntries.Add(menuEntry8);
		MenuEntry menuEntry9 = new MenuEntry("Unlock All Relics")
		{
			Description = "Unlocks all relics only"
		};
		menuEntry9.Selected += delegate
		{
			UnlockRelics();
		};
		base.MenuEntries.Add(menuEntry9);
		MenuEntry menuEntry10 = new MenuEntry("Unlock All UseItems")
		{
			Description = "Unlocks all use items only"
		};
		menuEntry10.Selected += delegate
		{
			UnlockUseItems();
		};
		base.MenuEntries.Add(menuEntry10);
		MenuEntry menuEntry11 = new MenuEntry("Unlock All Equipment")
		{
			Description = "Unlocks all use equipment (head, body, and trinket) only"
		};
		menuEntry11.Selected += delegate
		{
			UnlockEquipment();
		};
		base.MenuEntries.Add(menuEntry11);
		MenuEntry menuEntry12 = new MenuEntry("Revive All Bosses")
		{
			Description = "Bring all killed bosses back to life."
		};
		menuEntry12.Selected += delegate
		{
			ReviveAllBosses();
		};
		base.MenuEntries.Add(menuEntry12);
		MenuEntry menuEntry13 = new MenuEntry("Unlock All Camp NPCs")
		{
			Description = "Populate the NPC Camp in the Forest"
		};
		menuEntry13.Selected += delegate
		{
			UnlockAllNPCs();
		};
		base.MenuEntries.Add(menuEntry13);
		MenuEntry menuEntry14 = new MenuEntry("Unlock Bestiary")
		{
			Description = "Add 1 kill for every enemy in the game"
		};
		menuEntry14.Selected += delegate
		{
			KillAllEnemies();
		};
		base.MenuEntries.Add(menuEntry14);
		MenuEntry menuEntry15 = new MenuEntry("Toggle Easy")
		{
			Description = "Turn Easy Mode On or Off."
		};
		menuEntry15.Selected += delegate
		{
			OnEasyModeToggle();
		};
		base.MenuEntries.Add(menuEntry15);
		MenuEntry menuEntry16 = new MenuEntry("Toggle Hard")
		{
			Description = "Turn Hard Mode On or Off."
		};
		menuEntry16.Selected += delegate
		{
			OnHardModeToggle();
		};
		base.MenuEntries.Add(menuEntry16);
		MenuEntry menuEntry17 = new MenuEntry("Unlock Feats")
		{
			Description = "Unlock all Feats at once."
		};
		menuEntry17.Selected += delegate
		{
			OnUnlockFeats();
		};
		base.MenuEntries.Add(menuEntry17);
		MenuEntry menuEntry18 = new MenuEntry("Loc Dialogue Test")
		{
			Description = "Pick text to play in Dialogue Boxes."
		};
		menuEntry18.Selected += OnLocDebugSelect;
		base.MenuEntries.Add(menuEntry18);
	}

	private void OnSaveDataEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		base.ScreenManager.AddScreen(new DebugSaveDataMenuScreen(base.SaveFile, base.GCM, FullExit), e.PlayerIndex);
	}

	private void OnLocDebugSelect(object sender, PlayerIndexEventArgs e)
	{
		base.ScreenManager.AddScreen(new DebugLocMenuScreen("Loc Debug", base.SaveFile, base.GCM, FullExit), e.PlayerIndex);
	}

	private void DropCash()
	{
		base.SaveFile.Money += 1000;
		base.ScreenManager.Jukebox.PlayCue(ESFX.ItemGetMoney);
	}

	private void GiveExp()
	{
		base.SaveFile.CharacterStats.GiveExperience(5000);
		base.SaveFile.CharacterStats.RefreshBaseStats();
	}

	private void RevealMap()
	{
		foreach (MinimapArea area in _level.Minimap.Areas)
		{
			foreach (MinimapRoom room in area.Rooms)
			{
				foreach (MinimapBlock value in room.Blocks.Values)
				{
					value.IsKnown = true;
				}
			}
		}
		base.ScreenManager.Jukebox.PlayCue(ESFX.MenuBuy);
	}

	private void HideMap()
	{
		foreach (MinimapArea area in _level.Minimap.Areas)
		{
			foreach (MinimapRoom room in area.Rooms)
			{
				foreach (MinimapBlock value in room.Blocks.Values)
				{
					if (!value.IsVisited && value.IsKnown)
					{
						value.IsKnown = false;
					}
				}
			}
		}
		base.ScreenManager.Jukebox.PlayCue(ESFX.DoorKeycardError);
	}

	internal override void RefreshSizes()
	{
		base.RefreshSizes();
		int num = (int)(7f / 64f * (float)_topSectionHeight);
		_mapBackgroundDrawRectangle = new Rectangle(_screenLeft + 3 * base.Zoom, _screenTop + num, _screenWidth - 6 * base.Zoom, _topSectionHeight - num - 24);
	}

	public override void DrawFrames(SpriteBatch spriteBatch, Color drawColor)
	{
		DrawingEx.DrawIrregularBox(flipped: new SpriteEffects[9]
		{
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.FlipHorizontally,
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.FlipHorizontally,
			SpriteEffects.FlipVertically,
			SpriteEffects.FlipVertically,
			SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically
		}, spriteBatch: spriteBatch, backgroundRectangle: _mapBackgroundDrawRectangle, color: drawColor, sprite: base.Sprite, zoom: base.Zoom, frames: new int[9] { 45, 46, 45, 47, 48, 47, 45, 46, 45 }, shouldTile: true);
		base.DrawFrames(spriteBatch, drawColor);
	}

	private void UnlockItems()
	{
		_level.JukeBox.PlayCue(ESFX.MenuHeal);
		UnlockUseItems();
		UnlockOrbs(EOrbSlot.All);
		UnlockFamiliars();
		UnlockRelics();
		UnlockEquipment();
	}

	private void UnlockMeleeOrbs()
	{
		UnlockOrbs(EOrbSlot.Melee);
	}

	private void UnlockJournal()
	{
		_level.JukeBox.PlayCue(ESFX.MenuSell);
		if (base.SaveFile != null)
		{
			for (int i = 0; i < 11; i++)
			{
				base.SaveFile.UnlockJournal((EInventoryJournalType)i);
				base.SaveFile.UnlockJournal((EInventoryJournalType)(i + 32));
				base.SaveFile.UnlockJournal((EInventoryJournalType)(i + 64));
				base.SaveFile.UnlockJournal((EInventoryJournalType)(i + 64 + 10));
			}
		}
	}

	private void UnlockUseItems()
	{
		Protagonist mainHero = _level.MainHero;
		if (mainHero != null)
		{
			mainHero.GiveLoot(EInventoryUseItemType.Potion, 5);
			mainHero.GiveLoot(EInventoryUseItemType.Ether, 2);
			mainHero.GiveLoot(EInventoryUseItemType.HiPotion, 5);
			mainHero.GiveLoot(EInventoryUseItemType.HiEther, 2);
			mainHero.GiveLoot(EInventoryUseItemType.FuturePotion, 5);
			mainHero.GiveLoot(EInventoryUseItemType.FutureHiPotion, 5);
			mainHero.GiveLoot(EInventoryUseItemType.FutureEther, 5);
			mainHero.GiveLoot(EInventoryUseItemType.FutureHiEther, 5);
			mainHero.GiveLoot(EInventoryUseItemType.Antidote, 3);
			mainHero.GiveLoot(EInventoryUseItemType.ChaosHeal, 3);
			mainHero.GiveLoot(EInventoryUseItemType.WarpCard, 3);
			mainHero.GiveLoot(EInventoryUseItemType.LachiemiSun, 1);
			mainHero.GiveLoot(EInventoryUseItemType.Jerky, 3);
			mainHero.GiveLoot(EInventoryUseItemType.Biscuit, 1);
			mainHero.GiveLoot(EInventoryUseItemType.CheveuxBreast, 20);
			mainHero.GiveLoot(EInventoryUseItemType.SandBottle, 3);
			mainHero.GiveLoot(EInventoryUseItemType.HiSandBottle, 3);
			mainHero.GiveLoot(EInventoryUseItemType.AlchemistTools, 1);
			mainHero.GiveLoot(EInventoryUseItemType.EssenceCrystal, 5);
			mainHero.GiveLoot(EInventoryUseItemType.GalaxyStone, 1);
			mainHero.GiveLoot(EInventoryUseItemType.MagicMarbles, 3);
			mainHero.GiveLoot(EInventoryUseItemType.GoldRing, 6);
			mainHero.GiveLoot(EInventoryUseItemType.GoldNecklace, 6);
			mainHero.GiveLoot(EInventoryUseItemType.CheveuxFeather, 1);
			mainHero.GiveLoot(EInventoryUseItemType.SirenInk, 1);
			mainHero.GiveLoot(EInventoryUseItemType.Herb, 1);
			mainHero.GiveLoot(EInventoryUseItemType.Mushroom, 1);
			mainHero.GiveLoot(EInventoryUseItemType.RadiationCrystal, 1);
			mainHero.GiveLoot(EInventoryUseItemType.PlasmaIV, 1);
			mainHero.GiveLoot(EInventoryUseItemType.Drumstick, 1);
			mainHero.GiveLoot(EInventoryUseItemType.WyvernTail, 1);
			mainHero.GiveLoot(EInventoryUseItemType.EelMeat, 1);
			mainHero.GiveLoot(EInventoryUseItemType.PlasmaCore, 1);
			mainHero.GiveLoot(EInventoryUseItemType.FoodSynth, 1);
			mainHero.GiveLoot(EInventoryUseItemType.CheveuxBreast, 1);
			mainHero.GiveLoot(EInventoryUseItemType.FriedCheveux, 1);
			mainHero.GiveLoot(EInventoryUseItemType.SauteedTail, 1);
			mainHero.GiveLoot(EInventoryUseItemType.UnagiRoll, 1);
			mainHero.GiveLoot(EInventoryUseItemType.CheveuxAuVin, 1);
			mainHero.GiveLoot(EInventoryUseItemType.Casserole, 1);
			mainHero.GiveLoot(EInventoryUseItemType.Spaghetti, 1);
			mainHero.GiveLoot(EInventoryUseItemType.PlumpMaggot, 1);
			mainHero.GiveLoot(EInventoryUseItemType.RottenTail, 1);
			mainHero.GiveLoot(EInventoryUseItemType.OrangeJuice, 1);
			mainHero.GiveLoot(EInventoryUseItemType.FiligreeTea, 1);
			mainHero.GiveLoot(EInventoryUseItemType.EmpressCake, 1);
			mainHero.GiveLoot(EInventoryUseItemType.SilverOre, 1);
			mainHero.GiveLoot(EInventoryUseItemType.HistoricalDocuments, 1);
		}
	}

	private void UnlockEquipment()
	{
		Protagonist mainHero = _level.MainHero;
		if (mainHero == null)
		{
			return;
		}
		foreach (EInventoryEquipmentType value in Enum.GetValues(typeof(EInventoryEquipmentType)))
		{
			mainHero.GiveLoot(value);
		}
	}

	private void UnlockRelics()
	{
		Protagonist mainHero = _level.MainHero;
		if (mainHero != null)
		{
			mainHero.GiveLoot(EInventoryRelicType.TimespinnerWheel);
			mainHero.GiveLoot(EInventoryRelicType.TimespinnerSpindle);
			mainHero.GiveLoot(EInventoryRelicType.TimespinnerGear1);
			mainHero.GiveLoot(EInventoryRelicType.TimespinnerGear2);
			mainHero.GiveLoot(EInventoryRelicType.TimespinnerGear3);
			mainHero.GiveLoot(EInventoryRelicType.PyramidsKey);
			mainHero.GiveLoot(EInventoryRelicType.EssenceOfSpace);
			mainHero.GiveLoot(EInventoryRelicType.DoubleJump);
			mainHero.GiveLoot(EInventoryRelicType.Dash);
			mainHero.GiveLoot(EInventoryRelicType.WaterMask);
			mainHero.GiveLoot(EInventoryRelicType.AirMask);
			mainHero.GiveLoot(EInventoryRelicType.FoeScanner);
			mainHero.GiveLoot(EInventoryRelicType.ScienceKeycardA);
			mainHero.GiveLoot(EInventoryRelicType.ScienceKeycardB);
			mainHero.GiveLoot(EInventoryRelicType.ScienceKeycardC);
			mainHero.GiveLoot(EInventoryRelicType.ScienceKeycardD);
			mainHero.GiveLoot(EInventoryRelicType.ScienceKeycardV);
			mainHero.GiveLoot(EInventoryRelicType.Tablet);
			mainHero.GiveLoot(EInventoryRelicType.ElevatorKeycard);
			mainHero.GiveLoot(EInventoryRelicType.JewelryBox);
			mainHero.GiveLoot(EInventoryRelicType.FamiliarAltMeyef);
			mainHero.GiveLoot(EInventoryRelicType.FamiliarAltCrow);
		}
	}

	private void UnlockOrbs(EOrbSlot slot)
	{
		Protagonist mainHero = _level.MainHero;
		if (mainHero != null)
		{
			mainHero.GiveLoot(EInventoryOrbType.Blue, slot);
			mainHero.GiveLoot(EInventoryOrbType.Blade, slot);
			mainHero.GiveLoot(EInventoryOrbType.Flame, slot);
			mainHero.GiveLoot(EInventoryOrbType.Pink, slot);
			mainHero.GiveLoot(EInventoryOrbType.Iron, slot);
			mainHero.GiveLoot(EInventoryOrbType.Ice, slot);
			mainHero.GiveLoot(EInventoryOrbType.Wind, slot);
			mainHero.GiveLoot(EInventoryOrbType.Gun, slot);
			mainHero.GiveLoot(EInventoryOrbType.Umbra, slot);
			mainHero.GiveLoot(EInventoryOrbType.Empire, slot);
			mainHero.GiveLoot(EInventoryOrbType.Eye, slot);
			mainHero.GiveLoot(EInventoryOrbType.Blood, slot);
			mainHero.GiveLoot(EInventoryOrbType.Barrier, slot);
			mainHero.GiveLoot(EInventoryOrbType.Nether, slot);
			mainHero.GiveLoot(EInventoryOrbType.Book, slot);
			mainHero.GiveLoot(EInventoryOrbType.Moon, slot);
			mainHero.GiveLoot(EInventoryOrbType.Monske, slot);
		}
	}

	private void UnlockFamiliars()
	{
		Protagonist mainHero = _level.MainHero;
		if (mainHero != null)
		{
			mainHero.GiveLoot(EInventoryFamiliarType.Meyef);
			mainHero.GiveLoot(EInventoryFamiliarType.Griffin);
			mainHero.GiveLoot(EInventoryFamiliarType.Sprite);
			mainHero.GiveLoot(EInventoryFamiliarType.Kobo);
			mainHero.GiveLoot(EInventoryFamiliarType.MerchantCrow);
			mainHero.GiveLoot(EInventoryFamiliarType.Demon);
		}
	}

	private void ReviveAllBosses()
	{
		_level.JukeBox.PlayCue(ESFX.MenuHeal);
		for (int i = 0; i < 18; i++)
		{
			string bossKillKeyFromLevelID = BossClass.GetBossKillKeyFromLevelID(i);
			if (bossKillKeyFromLevelID != null)
			{
				_level.GameSave.SetValue(bossKillKeyFromLevelID, value: false);
			}
		}
		string saveKeyByBossType = BossClass.GetSaveKeyByBossType(EBossType.Cantoran);
		_level.GameSave.SetValue(saveKeyByBossType, value: false);
		CutsceneBase.SetIsCutsceneTriggeredByType(CutsceneBase.ECutsceneType.Keep0_Demons0, value: false, _level.GameSave);
		CutsceneBase.SetIsCutsceneTriggeredByType(CutsceneBase.ECutsceneType.Temple0_Boss, value: false, _level.GameSave);
		CutsceneBase.SetIsCutsceneTriggeredByType(CutsceneBase.ECutsceneType.Alt0_Nuvius, value: false, _level.GameSave);
		CutsceneBase.SetIsCutsceneTriggeredByType(CutsceneBase.ECutsceneType.Alt1_Vol, value: false, _level.GameSave);
	}

	private void UnlockAllNPCs()
	{
		GameSave gameSave = _level.GameSave;
		gameSave.SetValue(NPCBase.GetIsNPCUnlockedKeyFromType(NPCBase.ENPCType.Astrologer), value: true);
		gameSave.SetValue(NPCBase.GetIsNPCUnlockedKeyFromType(NPCBase.ENPCType.Captain), value: true);
		gameSave.SetValue(NPCBase.GetIsNPCUnlockedKeyFromType(NPCBase.ENPCType.Quartermaster), value: true);
		gameSave.SetValue(NPCBase.GetIsNPCUnlockedKeyFromType(NPCBase.ENPCType.Medic), value: true);
		gameSave.SetValue(NPCBase.GetIsNPCUnlockedKeyFromType(NPCBase.ENPCType.SickSoldier), value: true);
	}

	private void KillAllEnemies()
	{
		_level.JukeBox.PlayCue(ESFX.MenuHeal);
		foreach (EEnemyTileType value in Enum.GetValues(typeof(EEnemyTileType)))
		{
			string killKeyFromTypeAndArgument = Monster.GetKillKeyFromTypeAndArgument(value, 0);
			int saveInt = _level.GameSave.GetSaveInt(killKeyFromTypeAndArgument);
			_level.GameSave.SetValue(killKeyFromTypeAndArgument, saveInt + 1);
			if (ObjectTileSpecification.DoesUseArgumentForKey(value))
			{
				killKeyFromTypeAndArgument = Monster.GetKillKeyFromTypeAndArgument(value, 1);
				saveInt = _level.GameSave.GetSaveInt(killKeyFromTypeAndArgument);
				_level.GameSave.SetValue(killKeyFromTypeAndArgument, saveInt + 1);
			}
		}
	}

	private void ToggleVilete()
	{
		_level.GameSave.SetValue("IsVileteSaved", !_level.GameSave.GetSaveBool("IsVileteSaved"));
	}

	private void OnEasyModeToggle()
	{
		if (!base.SaveFile.GetSaveBool("IsEasyMode"))
		{
			base.SaveFile.SetValue("IsEasyMode", value: true);
			base.SaveFile.SetValue("IsHardMode", value: false);
		}
		else
		{
			base.SaveFile.SetValue("IsEasyMode", value: false);
		}
	}

	private void OnHardModeToggle()
	{
		if (!base.SaveFile.GetSaveBool("IsHardMode"))
		{
			base.SaveFile.SetValue("IsHardMode", value: true);
			base.SaveFile.SetValue("IsEasyMode", value: false);
		}
		else
		{
			base.SaveFile.SetValue("IsHardMode", value: false);
		}
	}

	private void OnUnlockFeats()
	{
		foreach (EBossType value in Enum.GetValues(typeof(EBossType)))
		{
			base.SaveFile.UnlockFeat(value, 1);
		}
		foreach (EGameFeatType value2 in Enum.GetValues(typeof(EGameFeatType)))
		{
			if (value2 != EGameFeatType.Boss && value2 != EGameFeatType.AllFeats)
			{
				base.SaveFile.UnlockFeat(value2);
			}
		}
	}

	private void FullExit()
	{
		ExitScreen();
		_fullExitAction();
	}

	public override void ExitScreen()
	{
		_onExitAction();
		base.ExitScreen();
	}
}
