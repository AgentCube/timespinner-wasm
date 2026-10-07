using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.Heroes;
using Timespinner.GameStateManagement.ScreenManager;
using Timespinner.GameStateManagement.Screens.BaseClasses;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;
using Timespinner.GameStateManagement.Screens.MainMenu;

namespace Timespinner.GameStateManagement.Screens.PauseMenu;

internal class PauseMenuScreen : InventoryMenuScreen
{
	private const int LatinMainMenuMarginY = -6;

	private const int MainMenuBaseColumnWidth = 80;

	private const int AsianMainMenuMarginY = -4;

	private const int AsianOffsetLeftSideY = -2;

	private const int AsianMainMenuFrameHeightAddition = 14;

	private const int AsianCoreStatsOffsetY = -5;

	private const int AsianGameStatsOffsetY = -4;

	private const int AsianFamiliarStatsOffsetY = 2;

	private const int LatinHeroStatsBufferX = 0;

	private const int AsianHeroStatsBufferX = 3;

	private const int HeroStatsBaseWidth = 67;

	private const int EquipStatsWidth = 124;

	private const int FamiliarStatsWidth = 90;

	private const float MenuPositionXRatio = 0.075f;

	private const float MenuPositionYRatio = 29f / 64f;

	private const float MenuFramePositionXRatio = 0.0375f;

	private const float MenuFramePositionYRatio = 25f / 64f;

	private const float MenuFramePositionWidthRatio = 0.2875f;

	private const float MenuFrameHeightRatio = 35f / 64f;

	private const float LunaisLeftFramePositionXRatio = 0.025f;

	private const float LunaisLeftFramePositionYRatio = 23f / 192f;

	private const float LunaisRightFramePositionXRatio = 7f / 32f;

	private const float LunaisRightFramePositionYRatio = 1f / 48f;

	private const float LunaisPortraitPositionXRatio = 13f / 160f;

	private const float LunaisPortraitPositionYRatio = 1f / 64f;

	private const float LunaisLevelPositionXRatio = 0.2375f;

	private const float LunaisLevelPositionYRatio = 31f / 96f;

	private const float LunaisLevelNumberPositionOffsetX = 20f;

	private const float MeyefFramePositionXRatio = 119f / 160f;

	private const float MeyefFramePositionYRatio = 107f / 192f;

	private const float MeyefOffsetXMultiplier = 2f;

	private const float MeyefOffsetYMultiplier = 10f;

	private const float HealthBarPositionXRatio = 0.75f;

	private const float HealthBarPositionYRatio = 7f / 96f;

	private const float LeftJustifiedStatsXRatio = 0.346875f;

	private const float MiddleJustifiedStatsXRatio = 29f / 64f;

	private const float GameStatsWidthRatio = 0.509375f;

	private const float HeroStatsYRatio = 7f / 192f;

	private const float GameStatsYRatio = 7f / 32f;

	private const float EquipStatsYRatio = 33f / 64f;

	private const float FamiliarTitleYRatio = 133f / 192f;

	private const float FamiliarStatsYRatio = 145f / 192f;

	private readonly bool _isAsianLoc;

	private readonly int _elapsedGameSeconds;

	private readonly int _mapCompletionPercentage;

	private readonly MenuEntry _familiarMenuEntry;

	private readonly MenuEntry _useItemsMenuEntry;

	private readonly MenuEntry _mapMenuEntry;

	private readonly MenuEntry _relicsMenuEntry;

	private readonly Level _level;

	private readonly SpriteSheet _pauseSprite;

	private readonly PlayerInventory _playerInventory;

	private readonly Action _onExitAction;

	private bool _isFamiliarMenuAvailable;

	private bool _isUseItemsMenuAvailable;

	private bool _isMapMenuAvailable;

	private bool _isRelicsMenuAvailable;

	private int _levelDigit1;

	private int _levelDigit2;

	private int _levelDigit3;

	private int _lunaisSkinIndex;

	private float _meyefOffsetX;

	private float _meyefOffsetY;

	private Vector2 _lunaisDrawPosition;

	private Vector2 _meyefDrawPosition;

	private Vector2 _lunaisLeftFrameDrawPosition;

	private Vector2 _lunaisRightFrameDrawPosition;

	private Vector2 _lunaisLevelDrawPosition;

	private Vector2 _lunaisLevelNumberDrawPosition;

	private Vector2 _meyefFrameDrawPosition;

	private Rectangle _menuEntriesFrameRect;

	private InventoryOrb _equippedMeleeOrbA;

	private InventoryOrb _equippedMeleeOrbB;

	private InventoryOrb _equippedSpellOrb;

	private InventoryOrb _equippedPassiveOrb;

	private InventoryFamiliar _equippedFamiliar;

	public PauseMenuScreen(Level inLevel, GCM inGCM, GameSave inSave, int elapsedSeconds, float mapCompletion, Action onExitAction)
		: base("", inSave, inGCM, null)
	{
		_level = inLevel;
		_pauseSprite = base.GCM.SpPauseMenu;
		_playerInventory = inSave.Inventory;
		_elapsedGameSeconds = elapsedSeconds;
		_mapCompletionPercentage = (int)mapCompletion;
		_onExitAction = onExitAction;
		_isAsianLoc = Loc.IsAsianLocale;
		if (!_isAsianLoc)
		{
			_primaryMenuCollection.EntryHeightOffset = -6;
		}
		else
		{
			_primaryMenuCollection.EntryHeightOffset = -4;
		}
		base.DoesDrawHealthbar = true;
		string description = Loc.Get("FamiliarMenuDescription");
		MenuEntry menuEntry = new MenuEntry(Loc.Get("OrbMenuTitle"))
		{
			Description = Loc.Get("OrbMenuDescription")
		};
		MenuEntry menuEntry2 = new MenuEntry(Loc.Get("EquipmentMenuTitle"))
		{
			Description = Loc.Get("EquipmentMenuDescription")
		};
		_familiarMenuEntry = new MenuEntry(Loc.Get("FamiliarMenuTitle"))
		{
			Description = description
		};
		_useItemsMenuEntry = new MenuEntry(Loc.Get("UseItemsMenuTitle"))
		{
			Description = Loc.Get("UseItemsMenuDescription")
		};
		_mapMenuEntry = new MenuEntry(Loc.Get("MapMenuTitle"))
		{
			Description = Loc.Get("MapMenuDescription")
		};
		_relicsMenuEntry = new MenuEntry(Loc.Get("RelicsMenuTitle"))
		{
			Description = Loc.Get("RelicsMenuDescription")
		};
		MenuEntry menuEntry3 = new MenuEntry(Loc.Get("JournalMenuTitle"))
		{
			Description = Loc.Get("JournalMenuDescription")
		};
		MenuEntry menuEntry4 = new MenuEntry(Loc.Get("OptionsMenuTitle"))
		{
			Description = Loc.Get("OptionsMenuDescription")
		};
		MenuEntry menuEntry5 = new MenuEntry(Loc.Get("QuitGame"))
		{
			Description = Loc.Get("QuitGameDescription")
		};
		menuEntry.Selected += OrbMenuEntrySelected;
		menuEntry2.Selected += EquipmentMenuEntrySelected;
		_familiarMenuEntry.Selected += FamiliarMenuEntrySelected;
		_useItemsMenuEntry.Selected += UseItemsMenuEntrySelected;
		_mapMenuEntry.Selected += MapMenuEntrySelected;
		menuEntry3.Selected += JournalMenuEntrySelected;
		_relicsMenuEntry.Selected += RelicsMenuEntrySelected;
		menuEntry4.Selected += OptionsMenuEntrySelected;
		menuEntry5.Selected += QuitGameMenuEntrySelected;
		base.MenuEntries.Add(menuEntry);
		base.MenuEntries.Add(menuEntry2);
		base.MenuEntries.Add(_familiarMenuEntry);
		base.MenuEntries.Add(_useItemsMenuEntry);
		base.MenuEntries.Add(_mapMenuEntry);
		base.MenuEntries.Add(_relicsMenuEntry);
		base.MenuEntries.Add(menuEntry3);
		base.MenuEntries.Add(menuEntry4);
		base.MenuEntries.Add(menuEntry5);
		RefreshAvailableMenus();
	}

	internal override void RefreshSizes()
	{
		base.RefreshSizes();
		int num = (_isAsianLoc ? (-2 * base.Zoom) : 0);
		int num2 = (_isAsianLoc ? (14 * base.Zoom) : 0);
		_menuEntriesFrameRect = new Rectangle(_screenLeft + (int)(0.0375f * (float)_screenWidth), _screenTop + (int)(25f / 64f * (float)_topSectionHeight) + num, (int)(0.2875f * (float)_screenWidth), (int)(35f / 64f * (float)_topSectionHeight) + num2);
		_lunaisLeftFrameDrawPosition = new Vector2(_screenLeft + (int)(0.025f * (float)_screenWidth), _screenTop + (int)(23f / 192f * (float)_topSectionHeight) + num);
		_lunaisRightFrameDrawPosition = new Vector2(_screenLeft + (int)(7f / 32f * (float)_screenWidth), _screenTop + (int)(1f / 48f * (float)_topSectionHeight));
		_meyefFrameDrawPosition = new Vector2(_screenLeft + (int)(119f / 160f * (float)_screenWidth), _screenTop + (int)(107f / 192f * (float)_topSectionHeight));
		_lunaisLevelDrawPosition = new Vector2(_screenLeft + (int)(0.2375f * (float)_screenWidth), _screenTop + (int)(31f / 96f * (float)_topSectionHeight) + num);
		_lunaisLevelNumberDrawPosition = Vector2.Add(_lunaisLevelDrawPosition, new Vector2(20f * (float)base.Zoom, 0f));
		_lunaisDrawPosition = new Vector2(_screenLeft + (int)(13f / 160f * (float)_screenWidth), _screenTop + (int)(1f / 64f * (float)_topSectionHeight) + num);
		_meyefOffsetX = 2f * (float)base.Zoom;
		_meyefOffsetY = 10f * (float)base.Zoom;
		_meyefDrawPosition = Vector2.Add(_meyefFrameDrawPosition, new Vector2(_meyefOffsetX, _meyefOffsetY));
		base.HealthBarDrawPosition = new Vector2(_screenLeft + (int)(0.75f * (float)_screenWidth), _screenTop + (int)(7f / 96f * (float)_topSectionHeight));
		LoadStats();
		_primaryMenuCollection.Font = base.ScreenManager.MenuFont;
		_primaryMenuCollection.DrawPosition = new Vector2((float)_screenLeft + 0.075f * (float)_screenWidth, (float)_screenTop + 29f / 64f * (float)_topSectionHeight + (float)num);
		int width = 80 * base.Zoom;
		_primaryMenuCollection.SetColumnWidth(width, base.Zoom);
	}

	private void LoadStats()
	{
		base.StatCollections.Clear();
		CharacterStats characterStats = base.SaveFile.CharacterStats;
		InventoryRelicCollection relicInventory = base.SaveFile.Inventory.RelicInventory;
		if (relicInventory.IsRelicActive(EInventoryRelicType.EternalBrooch))
		{
			_lunaisSkinIndex = 2;
		}
		else if (relicInventory.IsRelicActive(EInventoryRelicType.EmpireBrooch))
		{
			_lunaisSkinIndex = 1;
		}
		else
		{
			_lunaisSkinIndex = 0;
		}
		Protagonist mainHero = _level.MainHero;
		_playerHealth = mainHero.HP;
		_playerMaxHealth = mainHero.MaxHP;
		_playerSand = mainHero.MP;
		_playerMaxSand = mainHero.MaxMP;
		_playerAura = mainHero.Aura;
		_playerMaxAura = mainHero.MaxAura;
		_playerStatus = mainHero.GetFirstActiveStatusEffect();
		characterStats.HP = _playerHealth;
		characterStats.Sand = _playerSand;
		characterStats.Aura = _playerAura;
		characterStats.CurrentStatus = _playerStatus;
		int visibleLevel = characterStats.VisibleLevel;
		if (visibleLevel <= 9)
		{
			_levelDigit1 = 0;
			_levelDigit2 = visibleLevel;
			_levelDigit3 = -1;
		}
		else if (visibleLevel <= 99)
		{
			_levelDigit1 = visibleLevel / 10;
			_levelDigit2 = visibleLevel % 10;
			_levelDigit3 = -1;
		}
		else
		{
			_levelDigit1 = visibleLevel / 100;
			int num = visibleLevel - _levelDigit1 * 100;
			_levelDigit2 = num / 10;
			_levelDigit3 = num % 10;
		}
		int num2 = (_isAsianLoc ? 3 : 0);
		string text = Loc.Get("StatHealth");
		string text2 = Loc.Get("StatAura");
		string text3 = Loc.Get("StatSand");
		int num3 = (int)((float)base.Zoom * ((float)num2 + MathEx.Max(new float[3]
		{
			base.Font.MeasureString(text).X,
			base.Font.MeasureString(text2).X,
			base.Font.MeasureString(text3).X
		})));
		int width = num3 + 67 * base.Zoom;
		StatCollection statCollection = new StatCollection();
		statCollection.BracketType = StatCollection.EBracketType.Left;
		statCollection.Width = width;
		StatCollection statCollection2 = statCollection;
		StatCollection statCollection3 = new StatCollection();
		statCollection3.BracketType = StatCollection.EBracketType.Right;
		statCollection3.Width = (int)(0.509375f * (float)_screenWidth);
		StatCollection statCollection4 = statCollection3;
		StatCollection statCollection5 = new StatCollection();
		statCollection5.BracketType = StatCollection.EBracketType.Left;
		statCollection5.Width = 124 * base.Zoom;
		StatCollection statCollection6 = statCollection5;
		StatCollection statCollection7 = new StatCollection();
		statCollection7.BracketType = StatCollection.EBracketType.None;
		StatCollection statCollection8 = statCollection7;
		StatCollection statCollection9 = new StatCollection();
		statCollection9.BracketType = StatCollection.EBracketType.None;
		statCollection9.Width = 90 * base.Zoom;
		StatCollection statCollection10 = statCollection9;
		statCollection2.Entries.Add(new StatEntry
		{
			Title = text,
			Type = StatEntry.EStatDisplayType.Ratio,
			Value = _playerHealth,
			Value2 = _playerMaxHealth
		});
		statCollection2.Entries.Add(new StatEntry
		{
			Title = text2,
			Type = StatEntry.EStatDisplayType.Ratio,
			Value = _playerAura,
			Value2 = _playerMaxAura
		});
		statCollection2.Entries.Add(new StatEntry
		{
			Title = text3,
			Type = StatEntry.EStatDisplayType.Ratio,
			Value = _playerSand,
			Value2 = _playerMaxSand
		});
		int experience = base.SaveFile.CharacterStats.Experience;
		int num4 = base.SaveFile.CharacterStats.NextLevelThreshold - experience;
		if (num4 < 0)
		{
			num4 = 0;
		}
		statCollection4.Entries.Add(new StatEntry
		{
			Title = Loc.Get("StatExperience"),
			Type = StatEntry.EStatDisplayType.Number,
			Value = experience
		});
		statCollection4.Entries.Add(new StatEntry
		{
			Title = Loc.Get("StatNextLevel"),
			Type = StatEntry.EStatDisplayType.Number,
			Value = num4
		});
		statCollection4.Entries.Add(new StatEntry
		{
			Title = Loc.Get("StatEntropyGems"),
			Type = StatEntry.EStatDisplayType.NumberWithIcon,
			Value = base.SaveFile.Money,
			IconIndex = 0
		});
		statCollection4.Entries.Add(new StatEntry
		{
			Title = Loc.Get("StatTime"),
			Type = StatEntry.EStatDisplayType.Time,
			Value = _elapsedGameSeconds
		});
		statCollection4.Entries.Add(new StatEntry
		{
			Title = Loc.Get("StatRate"),
			Type = StatEntry.EStatDisplayType.Percentage,
			Value = _mapCompletionPercentage
		});
		_equippedMeleeOrbA = (_playerInventory.OrbInventory.Inventory.ContainsKey((int)_playerInventory.EquippedMeleeOrbA) ? _playerInventory.OrbInventory.Inventory[(int)_playerInventory.EquippedMeleeOrbA] : new InventoryOrb(_playerInventory.EquippedMeleeOrbA));
		_equippedMeleeOrbB = (_playerInventory.OrbInventory.Inventory.ContainsKey((int)_playerInventory.EquippedMeleeOrbB) ? _playerInventory.OrbInventory.Inventory[(int)_playerInventory.EquippedMeleeOrbB] : new InventoryOrb(_playerInventory.EquippedMeleeOrbB));
		_equippedSpellOrb = (_playerInventory.OrbInventory.Inventory.ContainsKey((int)_playerInventory.EquippedSpellOrb) ? _playerInventory.OrbInventory.Inventory[(int)_playerInventory.EquippedSpellOrb] : new InventoryOrb(_playerInventory.EquippedSpellOrb));
		_equippedPassiveOrb = (_playerInventory.OrbInventory.Inventory.ContainsKey((int)_playerInventory.EquippedPassiveOrb) ? _playerInventory.OrbInventory.Inventory[(int)_playerInventory.EquippedPassiveOrb] : new InventoryOrb(_playerInventory.EquippedPassiveOrb));
		statCollection6.Entries.Add(new StatEntry
		{
			Title = _equippedMeleeOrbA.Name,
			Type = StatEntry.EStatDisplayType.NumberWithIcon,
			Value = _equippedMeleeOrbA.Level,
			IconIndex = _equippedMeleeOrbA.GetIconIndex()
		});
		statCollection6.Entries.Add(new StatEntry
		{
			Title = _equippedMeleeOrbB.Name,
			Type = StatEntry.EStatDisplayType.NumberWithIcon,
			Value = _equippedMeleeOrbB.Level,
			IconIndex = _equippedMeleeOrbB.GetIconIndex()
		});
		statCollection6.Entries.Add(new StatEntry
		{
			Title = _equippedSpellOrb.SpellName,
			Type = StatEntry.EStatDisplayType.NumberWithIcon,
			Value = _equippedSpellOrb.Level,
			IconIndex = _equippedSpellOrb.GetIconIndex()
		});
		statCollection6.Entries.Add(new StatEntry
		{
			Title = _equippedPassiveOrb.PassiveName,
			Type = StatEntry.EStatDisplayType.NumberWithIcon,
			Value = _equippedPassiveOrb.Level,
			IconIndex = _equippedPassiveOrb.GetIconIndex()
		});
		int equippedFamiliar = (int)_playerInventory.EquippedFamiliar;
		_equippedFamiliar = (_playerInventory.FamiliarInventory.Inventory.ContainsKey(equippedFamiliar) ? _playerInventory.FamiliarInventory.Inventory[equippedFamiliar] : null);
		if (_equippedFamiliar != null)
		{
			statCollection8.Entries.Add(new StatEntry
			{
				Title = Loc.Get("StatFamiliar"),
				Type = StatEntry.EStatDisplayType.None
			});
			statCollection10.Entries.Add(new StatEntry
			{
				Title = _equippedFamiliar.Name,
				Type = StatEntry.EStatDisplayType.None
			});
			statCollection10.Entries.Add(new StatEntry
			{
				Title = Loc.Get("StatLevelAbbreviation"),
				Type = StatEntry.EStatDisplayType.Number,
				Value = _equippedFamiliar.VisibleLevel
			});
			statCollection10.Entries.Add(new StatEntry
			{
				Title = Loc.Get("StatNextLevel"),
				Type = StatEntry.EStatDisplayType.Number,
				Value = _equippedFamiliar.NextLevel
			});
		}
		int num5 = (int)(0.346875f * (float)_screenWidth) + _screenLeft;
		int num6 = (int)(29f / 64f * (float)_screenWidth) + _screenLeft;
		int num7 = (_isAsianLoc ? (-5 * base.Zoom) : 0);
		int num8 = (_isAsianLoc ? (-4 * base.Zoom) : 0);
		int num9 = (_isAsianLoc ? (2 * base.Zoom) : 0);
		statCollection2.Location = new Vector2(num5, (float)_screenTop + 7f / 192f * (float)_topSectionHeight + (float)num7);
		statCollection4.Location = new Vector2(num6, (float)_screenTop + 7f / 32f * (float)_topSectionHeight + (float)num8);
		statCollection6.Location = new Vector2(num5, (float)_screenTop + 33f / 64f * (float)_topSectionHeight);
		statCollection8.Location = new Vector2(num5, (float)_screenTop + 133f / 192f * (float)_topSectionHeight);
		statCollection10.Location = new Vector2(num6, (float)_screenTop + 145f / 192f * (float)_topSectionHeight + (float)num9);
		int num10 = 11 * base.Zoom;
		statCollection2.Height = statCollection2.Entries.Count * num10;
		statCollection4.Height = statCollection4.Entries.Count * num10;
		statCollection6.Height = statCollection6.Entries.Count * num10;
		statCollection8.Height = statCollection8.Entries.Count * num10;
		statCollection10.Height = statCollection10.Entries.Count * num10;
		base.StatCollections.Add(statCollection2);
		base.StatCollections.Add(statCollection4);
		base.StatCollections.Add(statCollection6);
		if (_equippedFamiliar != null)
		{
			base.StatCollections.Add(statCollection10);
		}
	}

	private void RefreshAvailableMenus()
	{
		_isFamiliarMenuAvailable = base.SaveFile.Inventory.FamiliarInventory.Inventory.Count > 0;
		_familiarMenuEntry.BaseDrawColor = (_isFamiliarMenuAvailable ? MenuEntry.UnselectedColor : MenuEntry.UnavailableColor);
		_isUseItemsMenuAvailable = base.SaveFile.Inventory.UseItemInventory.Inventory.Count > 0;
		_useItemsMenuEntry.BaseDrawColor = (_isUseItemsMenuAvailable ? MenuEntry.UnselectedColor : MenuEntry.UnavailableColor);
		_isMapMenuAvailable = _level.Minimap.AreAnyRoomsVisible();
		_mapMenuEntry.BaseDrawColor = (_isMapMenuAvailable ? MenuEntry.UnselectedColor : MenuEntry.UnavailableColor);
		_isRelicsMenuAvailable = base.SaveFile.Inventory.RelicInventory.Inventory.Count > 0;
		_relicsMenuEntry.BaseDrawColor = (_isRelicsMenuAvailable ? MenuEntry.UnselectedColor : MenuEntry.UnavailableColor);
	}

	private void OrbMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		base.ScreenManager.AddScreen(new OrbMenuScreen(base.SaveFile, base.GCM, OnExitEquipmentScreen, ExitScreen), e.PlayerIndex);
	}

	private void EquipmentMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		base.ScreenManager.AddScreen(new EquipmentMenuScreen(base.SaveFile, base.GCM, OnExitEquipmentScreen, ExitScreen), e.PlayerIndex);
	}

	private void FamiliarMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		_familiarMenuEntry.DoesConfirmationPlaySound = _isFamiliarMenuAvailable;
		if (_isFamiliarMenuAvailable)
		{
			base.ScreenManager.AddScreen(new FamiliarMenuScreen(base.SaveFile, base.GCM, OnExitUseItemScreen, ExitScreen), e.PlayerIndex);
		}
		else
		{
			PlayErrorSound();
		}
	}

	private void UseItemsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		_useItemsMenuEntry.DoesConfirmationPlaySound = _isUseItemsMenuAvailable;
		if (_isUseItemsMenuAvailable)
		{
			base.ScreenManager.AddScreen(new UseItemsMenuScreen(base.SaveFile, base.GCM, _level, OnExitUseItemScreen, ExitScreen), e.PlayerIndex);
		}
		else
		{
			PlayErrorSound();
		}
	}

	private void MapMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		_mapMenuEntry.DoesConfirmationPlaySound = _isMapMenuAvailable;
		if (_isMapMenuAvailable)
		{
			ControllerMapping menuControllerMapping = base.ScreenManager.MenuControllerMapping;
			base.ScreenManager.AddScreen(new MapMenuScreen(base.SaveFile, base.GCM, _level, menuControllerMapping, ExitScreen), e.PlayerIndex);
		}
		else
		{
			PlayErrorSound();
		}
	}

	private void RelicsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		_relicsMenuEntry.DoesConfirmationPlaySound = _isRelicsMenuAvailable;
		if (_isRelicsMenuAvailable)
		{
			base.ScreenManager.AddScreen(new RelicsMenuScreen(base.SaveFile, base.GCM, ExitScreen), e.PlayerIndex);
		}
		else
		{
			PlayErrorSound();
		}
	}

	private void JournalMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		base.ScreenManager.AddScreen(new JournalMenuScreen(base.SaveFile, base.GCM, ExitScreen), e.PlayerIndex);
	}

	private void OptionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		GameConfigSave configSave = base.ScreenManager.SaveFileManager.ConfigSave;
		base.ScreenManager.AddScreen(new OptionsMenuScreen(base.SaveFile, configSave, base.GCM, ExitScreen), e.PlayerIndex);
	}

	private void ConfirmQuitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
	{
		base.ScreenManager.Jukebox.FadeOutSong(0.5f);
		LoadingScreen.Load(base.ScreenManager, false, null, new TitleBackgroundScreen(shouldDoFullIntro: true));
	}

	private void QuitGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		MessageBoxScreen messageBoxScreen = new MessageBoxScreen(Loc.Get("ConfirmReturnTitle"), base.ScreenManager.MenuControllerMapping);
		messageBoxScreen.Accepted += ConfirmQuitMessageBoxAccepted;
		base.ScreenManager.AddScreen(messageBoxScreen, base.ControllingPlayer);
	}

	public override void HandleInput(InputState input)
	{
		bool flag = true;
		if (input.IsNewPressExit(base.ControllingPlayer))
		{
			flag = false;
			base.ScreenManager.Jukebox.PlayCue(ESFX.MenuCancel);
			ExitScreen();
		}
		if (flag)
		{
			base.HandleInput(input);
		}
	}

	private void OnExitUseItemScreen()
	{
		RefreshAvailableMenus();
		LoadStats();
	}

	private void OnExitEquipmentScreen()
	{
		base.SaveFile.Inventory.RefreshEquippedOrbs();
		LoadStats();
	}

	public override void ExitScreen()
	{
		_onExitAction();
		base.ExitScreen();
	}

	public override void DrawFrames(SpriteBatch spriteBatch, Color drawColor)
	{
		SpriteEffects[] array = new SpriteEffects[9]
		{
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.FlipHorizontally,
			SpriteEffects.None,
			SpriteEffects.FlipHorizontally
		};
		DrawingEx.DrawIrregularBox(spriteBatch, new Rectangle(_screenLeft, _screenTop, _screenWidth, _topSectionHeight), drawColor, _pauseSprite, base.Zoom, new int[9] { 19, 20, 21, 25, 0, 26, 24, 23, 22 }, array);
		array[2] = SpriteEffects.FlipHorizontally;
		array[6] = SpriteEffects.FlipVertically;
		array[8] = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
		DrawingEx.DrawIrregularBox(spriteBatch, new Rectangle(_screenLeft, _screenTop + _topSectionHeight, _screenWidth, _bottomSectionHeight), drawColor, _pauseSprite, base.Zoom, new int[9] { 27, 20, 27, -1, 0, -1, 28, 23, 28 }, array);
		DrawCharacters(spriteBatch, drawColor);
		array[1] = SpriteEffects.FlipVertically;
		array[2] = SpriteEffects.FlipHorizontally;
		array[6] = SpriteEffects.FlipVertically;
		array[8] = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
		DrawingEx.DrawIrregularBox(spriteBatch, _menuEntriesFrameRect, drawColor, _pauseSprite, base.Zoom, new int[9] { 28, 23, 28, -1, 1, -1, 28, 23, 28 }, array);
	}

	private void DrawCharacters(SpriteBatch spriteBatch, Color drawColor)
	{
		SpriteSheet spriteSheet;
		Rectangle frameSource;
		switch (_lunaisSkinIndex)
		{
		case 1:
			spriteSheet = base.GCM.SpPortraits;
			frameSource = spriteSheet.GetFrameSource(11);
			break;
		case 2:
			spriteSheet = base.GCM.SpPortraits2;
			frameSource = spriteSheet.GetFrameSource(11);
			break;
		default:
			spriteSheet = base.GCM.SpPortraits;
			frameSource = spriteSheet.GetFrameSource(0);
			break;
		}
		spriteBatch.Draw(spriteSheet.Texture, _lunaisDrawPosition, frameSource, drawColor, 0f, Vector2.Zero, base.Zoom, SpriteEffects.None, 0f);
		frameSource = base.Sprite.GetFrameSource(118);
		spriteBatch.Draw(base.Sprite.Texture, _lunaisLevelDrawPosition, frameSource, drawColor, 0f, Vector2.Zero, base.Zoom, SpriteEffects.None, 0f);
		frameSource = base.Sprite.GetFrameSource(119 + _levelDigit1);
		spriteBatch.Draw(base.Sprite.Texture, _lunaisLevelNumberDrawPosition, frameSource, drawColor, 0f, Vector2.Zero, base.Zoom, SpriteEffects.None, 0f);
		frameSource = base.Sprite.GetFrameSource(119 + _levelDigit2);
		Vector2 vector = Vector2.Add(_lunaisLevelNumberDrawPosition, new Vector2(7 * base.Zoom, 0f));
		spriteBatch.Draw(base.Sprite.Texture, vector, frameSource, drawColor, 0f, Vector2.Zero, base.Zoom, SpriteEffects.None, 0f);
		if (_levelDigit3 > -1)
		{
			frameSource = base.Sprite.GetFrameSource(119 + _levelDigit3);
			vector = Vector2.Add(vector, new Vector2(7 * base.Zoom, 0f));
			spriteBatch.Draw(base.Sprite.Texture, vector, frameSource, drawColor, 0f, Vector2.Zero, base.Zoom, SpriteEffects.None, 0f);
		}
		if (_equippedFamiliar != null && _equippedFamiliar.FamiliarType != 0)
		{
			int familiarType = (int)_equippedFamiliar.FamiliarType;
			frameSource = base.GCM.SpMenuCharacters.GetFrameSource(familiarType);
			spriteBatch.Draw(base.GCM.SpMenuCharacters.Texture, _meyefDrawPosition, frameSource, drawColor, 0f, Vector2.Zero, base.Zoom, SpriteEffects.None, 0f);
		}
	}

	public override void DrawCharacterFrames(SpriteBatch spriteBatch, Color drawColor)
	{
		Rectangle frameSource = _pauseSprite.GetFrameSource(30);
		spriteBatch.Draw(_pauseSprite.Texture, _lunaisLeftFrameDrawPosition, frameSource, drawColor, 0f, Vector2.Zero, base.Zoom, SpriteEffects.None, 0f);
		frameSource = _pauseSprite.GetFrameSource(69);
		spriteBatch.Draw(_pauseSprite.Texture, _lunaisRightFrameDrawPosition, frameSource, drawColor, 0f, Vector2.Zero, base.Zoom, SpriteEffects.FlipVertically, 0f);
		frameSource = _pauseSprite.GetFrameSource(31);
		spriteBatch.Draw(_pauseSprite.Texture, _meyefFrameDrawPosition, frameSource, drawColor, 0f, Vector2.Zero, base.Zoom, SpriteEffects.None, 0f);
	}

	internal void GoToItem(EInventoryCategoryType itemCategory, int itemValue)
	{
		switch (itemCategory)
		{
		case EInventoryCategoryType.UseItem:
		{
			UseItemsMenuScreen useItemsMenuScreen = new UseItemsMenuScreen(base.SaveFile, base.GCM, _level, OnExitUseItemScreen, ExitScreen);
			base.ScreenManager.AddScreen(useItemsMenuScreen, base.ControllingPlayer);
			useItemsMenuScreen.GoToUseItem(itemValue);
			SetMenuSelectedIndex(3);
			break;
		}
		case EInventoryCategoryType.Equipment:
		{
			EquipmentMenuScreen equipmentMenuScreen = new EquipmentMenuScreen(base.SaveFile, base.GCM, OnExitEquipmentScreen, ExitScreen);
			base.ScreenManager.AddScreen(equipmentMenuScreen, base.ControllingPlayer);
			equipmentMenuScreen.GoToEquipment(itemValue);
			SetMenuSelectedIndex(1);
			break;
		}
		case EInventoryCategoryType.Journal:
		{
			JournalMenuScreen journalMenuScreen = new JournalMenuScreen(base.SaveFile, base.GCM, ExitScreen);
			base.ScreenManager.AddScreen(journalMenuScreen, base.ControllingPlayer);
			journalMenuScreen.GoToJournal(itemValue);
			SetMenuSelectedIndex(6);
			break;
		}
		}
	}

	internal void GoToMap(int levelID)
	{
		if (_isMapMenuAvailable)
		{
			ControllerMapping menuControllerMapping = base.ScreenManager.MenuControllerMapping;
			MapMenuScreen mapMenuScreen = new MapMenuScreen(base.SaveFile, base.GCM, _level, menuControllerMapping, ExitScreen);
			base.ScreenManager.AddScreen(mapMenuScreen, base.ControllingPlayer);
			if (levelID != -1)
			{
				mapMenuScreen.GoToLevel(levelID);
			}
			SetMenuSelectedIndex(4);
		}
	}
}
