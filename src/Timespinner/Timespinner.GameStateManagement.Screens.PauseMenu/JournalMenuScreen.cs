using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Localization;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameStateManagement.Screens.BaseClasses.Inventory;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;
using Timespinner.GameStateManagement.Screens.PauseMenu.Journal;

namespace Timespinner.GameStateManagement.Screens.PauseMenu;

internal class JournalMenuScreen : InventoryMenuScreen
{
	private const int QuestFrameOffsetY = 15;

	private const int QuestInventoryDrawOffsetX = 3;

	private const int QuestInventoryDrawOffsetY = -5;

	private const int QuestPortraitBackDrawPositionX = 231;

	private const int QuestPortraitBackDrawPositionY = 119;

	private const int FeatsInventoryOffsetX = 26;

	private const int FeatsInventoryOffsetY = 8;

	private const int JournalMenuBaseColumnWidth = 180;

	private const int PrimaryMenuBaseColumnWidth = 70;

	private const int BestiaryMenuBaseColumnWidth = 160;

	private const int FeatsMenuBaseColumnWidth = 140;

	private const int QuestsMenuBaseColumnWidth = 72;

	private const float BackgroundDrawOffsetY = 7f / 64f;

	private const float SubMenuDisplayOffsetX = 0.7f;

	private const float ScrollbarPositionRatioY = 11f / 64f;

	private const float ScrollbarHeightRatio = 133f / 192f;

	private const string JournalEntryContentKeyFormat = "inv_jou_{0}_{1}";

	private static readonly Vector2 GameScreenCenter = new Vector2(640f, 360f);

	private static readonly Vector2 LevelScreenCenter = new Vector2(256f, 128f);

	private readonly MenuEntry _memoriesMenuEntry;

	private readonly MenuEntry _lettersMenuEntry;

	private readonly MenuEntry _filesMenuEntry;

	private readonly MenuEntry _questsMenuEntry;

	private readonly MenuEntry _bestiaryMenuEntry;

	private readonly MenuEntry _featsMenuEntry;

	private readonly MenuJournalInventoryCollection _memoriesInventoryCollection;

	private readonly MenuJournalInventoryCollection _lettersInventoryCollection;

	private readonly MenuJournalInventoryCollection _filesInventoryCollection;

	private readonly CharacterQuestMenuEntryCollection _questInventory;

	private readonly BestiaryMenuEntryCollection _bestiaryInventory;

	private readonly FeatsMenuEntryCollection _featsInventory;

	private readonly Dictionary<int, List<NPCBase.EQuestStateType>> _questStates = new Dictionary<int, List<NPCBase.EQuestStateType>>();

	private readonly Action _finalFullExitAction;

	private bool _isQuestMenuSelected;

	private Point _featsCursorOffset;

	private Vector2 _subMenuDisplayPosition;

	private Vector2 _questPortraitBackDrawPosition;

	private Rectangle _questPortraitBackFrameSource;

	private Rectangle _mainMenuBackgroundDrawRectangle;

	private Rectangle _subMenuBackgroundDrawRectangle;

	private Rectangle _mainMenuFrameDrawRectangle;

	private Rectangle _subMenuFrameDrawRectangle;

	private MenuEntryCollection _selectedQuestCollection;

	private Level _bestiaryDummyLevel;

	public JournalMenuScreen(GameSave inSave, GCM gcm, Action fullExitAction)
		: base(Loc.Get("JournalMenuTitle"), inSave, gcm, fullExitAction)
	{
		_finalFullExitAction = fullExitAction;
		LoadQuestStates();
		_memoriesMenuEntry = new MenuEntry(Loc.Get("journal_category_memories"))
		{
			Description = Loc.Get("journal_category_memories_description")
		};
		_lettersMenuEntry = new MenuEntry(Loc.Get("journal_category_letters"))
		{
			Description = Loc.Get("journal_category_letters_description")
		};
		_filesMenuEntry = new MenuEntry(Loc.Get("journal_category_files"))
		{
			Description = Loc.Get("journal_category_files_description")
		};
		_questsMenuEntry = new MenuEntry(Loc.Get("journal_category_quests"))
		{
			Description = Loc.Get("journal_category_quests_description")
		};
		_bestiaryMenuEntry = new MenuEntry(Loc.Get("journal_category_bestiary"))
		{
			Description = Loc.Get("journal_category_bestiary_description")
		};
		_featsMenuEntry = new MenuEntry(Loc.Get("journal_category_feats"))
		{
			Description = Loc.Get("journal_category_feats_description")
		};
		base.DoesDrawScrollbarWidget = true;
		_memoriesMenuEntry.Selected += OnMemoriesEntrySelected;
		_lettersMenuEntry.Selected += OnLettersEntrySelected;
		_filesMenuEntry.Selected += OnFilesEntrySelected;
		_questsMenuEntry.Selected += OnQuestsEntrySelected;
		_bestiaryMenuEntry.Selected += OnBestiaryEntrySelected;
		_featsMenuEntry.Selected += OnFeatsEntrySelected;
		base.MenuEntries.Add(_memoriesMenuEntry);
		base.MenuEntries.Add(_lettersMenuEntry);
		base.MenuEntries.Add(_filesMenuEntry);
		base.MenuEntries.Add(_questsMenuEntry);
		base.MenuEntries.Add(_bestiaryMenuEntry);
		base.MenuEntries.Add(_featsMenuEntry);
		_memoriesInventoryCollection = new MenuJournalInventoryCollection(inSave.Inventory.JournalCollection, JournalSelectedAction, EJournalCategoryType.Memories, base.Sprite);
		_lettersInventoryCollection = new MenuJournalInventoryCollection(inSave.Inventory.JournalCollection, JournalSelectedAction, EJournalCategoryType.Letters, base.Sprite);
		_filesInventoryCollection = new MenuJournalInventoryCollection(inSave.Inventory.JournalCollection, JournalSelectedAction, EJournalCategoryType.Files, base.Sprite);
		_questInventory = new CharacterQuestMenuEntryCollection(_questStates, base.Sprite, base.GCM.SpPortraits, base.SaveFile, OnQuestCharacterSelected);
		_bestiaryInventory = new BestiaryMenuEntryCollection(inSave, base.Font, base.GCM.Bestiary, OnEnemySelected);
		_featsInventory = new FeatsMenuEntryCollection(inSave.FeatsManager, base.Sprite, base.GCM.SpMenuIcons, base.Font);
		_subMenuCollections.Add(_memoriesInventoryCollection);
		_subMenuCollections.Add(_lettersInventoryCollection);
		_subMenuCollections.Add(_filesInventoryCollection);
		_subMenuCollections.Add(_questInventory);
		_subMenuCollections.Add(_bestiaryInventory);
		_subMenuCollections.Add(_featsInventory);
		base.DoesDrawBrackets = false;
		base.DoesHaveWideColumns = true;
		base.DoesDrawTopLowerFrame = false;
	}

	private void LoadQuestStates()
	{
		for (int i = 0; i < 5; i++)
		{
			NPCBase.ENPCType eNPCType = (NPCBase.ENPCType)i;
			string key = $"NPC_Progress_{eNPCType}";
			string key2 = $"NPC_SubProgress_{eNPCType}";
			int saveInt = base.SaveFile.GetSaveInt(key);
			int saveInt2 = base.SaveFile.GetSaveInt(key2);
			List<NPCBase.EQuestStateType> list = new List<NPCBase.EQuestStateType>();
			for (int j = 0; j < saveInt; j++)
			{
				list.Add(NPCBase.EQuestStateType.Closed);
			}
			NPCBase.EQuestStateType questProgress = NPCBase.GetQuestProgress(eNPCType, saveInt, saveInt2, base.SaveFile);
			list.Add(questProgress);
			if (questProgress == NPCBase.EQuestStateType.QuestsCapped)
			{
				while (list.Count < 5)
				{
					list.Add(NPCBase.EQuestStateType.QuestsCapped);
				}
			}
			if (saveInt > 0 || saveInt2 > 0)
			{
				_questStates.Add(i, list);
			}
		}
	}

	public override void LoadContent()
	{
		base.LoadContent();
		RefreshQuestInventorySizes();
	}

	internal override void RefreshSizes()
	{
		base.RefreshSizes();
		int num = (int)(7f / 64f * (float)_topSectionHeight);
		int num2 = (int)((float)_screenWidth * 0.3f);
		int num3 = _screenWidth - num2;
		_mainMenuBackgroundDrawRectangle = new Rectangle(_screenLeft + 2 * base.Zoom, _screenTop + num + 8 * base.Zoom, num2 - 5 * base.Zoom, _topSectionHeight - num - 16 * base.Zoom);
		_subMenuBackgroundDrawRectangle = new Rectangle(_mainMenuBackgroundDrawRectangle.Right + 3 * base.Zoom, _mainMenuBackgroundDrawRectangle.Top, num3 - 9 * base.Zoom, _mainMenuBackgroundDrawRectangle.Height);
		_mainMenuFrameDrawRectangle = new Rectangle(_screenLeft + base.Zoom, _screenTop + num, num2 - 2 * base.Zoom, _topSectionHeight - num);
		_subMenuFrameDrawRectangle = new Rectangle(_mainMenuFrameDrawRectangle.Right + base.Zoom, _mainMenuFrameDrawRectangle.Top, num3 - base.Zoom, _mainMenuFrameDrawRectangle.Height);
		_primaryMenuCollection.DrawPosition = new Vector2(_primaryMenuCollection.DrawPosition.X, _primaryMenuCollection.DrawPosition.Y + (float)(12 * base.Zoom));
		_primaryMenuCollection.SetColumnWidth(70 * base.Zoom, base.Zoom);
		_subMenuDisplayPosition = new Vector2(num2 + _screenLeft + 24 * base.Zoom, _primaryMenuCollection.DrawPosition.Y);
		_memoriesInventoryCollection.DrawPosition = _subMenuDisplayPosition;
		_lettersInventoryCollection.DrawPosition = _subMenuDisplayPosition;
		_filesInventoryCollection.DrawPosition = _subMenuDisplayPosition;
		_questInventory.DrawPosition = _subMenuDisplayPosition;
		_bestiaryInventory.DrawPosition = _subMenuDisplayPosition;
		_featsInventory.DrawPosition = new Vector2(_subMenuDisplayPosition.X + (float)(26 * base.Zoom), _subMenuDisplayPosition.Y + (float)(8 * base.Zoom));
		_featsCursorOffset = new Point(-26 * base.Zoom, base.Zoom);
		int width = 180 * base.Zoom;
		_memoriesInventoryCollection.SetColumnWidth(width, base.Zoom);
		_lettersInventoryCollection.SetColumnWidth(width, base.Zoom);
		_filesInventoryCollection.SetColumnWidth(width, base.Zoom);
		_bestiaryInventory.SetColumnWidth(160 * base.Zoom, base.Zoom);
		_featsInventory.SetColumnWidth(140 * base.Zoom, base.Zoom);
		base.ScrollBarDrawPosition = new Vector2(base.ScrollBarDrawPosition.X, _screenTop + (int)(11f / 64f * (float)_topSectionHeight));
		base.ScrollBarHeight = (int)(133f / 192f * (float)_topSectionHeight);
		RefreshQuestInventorySizes();
		_questPortraitBackDrawPosition = new Vector2(_screenLeft + 231 * base.Zoom, _screenTop + 119 * base.Zoom);
		_questPortraitBackFrameSource = base.Sprite.GetFrameSource(141);
		base.CursorOffset = ((_selectedMenuCollection == _featsInventory) ? _featsCursorOffset : Point.Zero);
	}

	private void RefreshQuestInventorySizes()
	{
		if (_questInventory.Font != null)
		{
			_questInventory.DrawPosition = new Vector2((int)(_questInventory.DrawPosition.X + (float)(3 * base.Zoom)), (int)(_questInventory.DrawPosition.Y + (float)(-5 * base.Zoom)));
			_questInventory.EntryHeightOffset = 15 - _questInventory.LineSpacing;
			_questInventory.GetMenuDimensions();
			_questInventory.SetColumnWidth(72 * base.Zoom, base.Zoom);
			_questInventory.RefreshSizes();
		}
	}

	public override void UnloadContent()
	{
		if (_bestiaryDummyLevel != null)
		{
			_bestiaryDummyLevel.Dispose();
		}
		base.UnloadContent();
	}

	private void OnMemoriesEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		if (_memoriesInventoryCollection.Entries.Count > 0)
		{
			ChangeMenuCollection(_memoriesInventoryCollection, shouldPush: true);
		}
	}

	private void OnLettersEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		if (_lettersInventoryCollection.Entries.Count > 0)
		{
			ChangeMenuCollection(_lettersInventoryCollection, shouldPush: true);
		}
	}

	private void OnFilesEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		if (_filesInventoryCollection.Entries.Count > 0)
		{
			ChangeMenuCollection(_filesInventoryCollection, shouldPush: true);
		}
	}

	private void OnQuestsEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		if (_questInventory.Entries.Count > 0)
		{
			ChangeMenuCollection(_questInventory, shouldPush: true);
		}
	}

	private void OnBestiaryEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		if (_bestiaryInventory.Entries.Count > 0)
		{
			ChangeMenuCollection(_bestiaryInventory, shouldPush: true);
		}
	}

	private void OnFeatsEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		ChangeMenuCollection(_featsInventory, shouldPush: true);
	}

	private void JournalSelectedAction(InventoryItem item)
	{
		if (item is InventoryJournal inventoryJournal)
		{
			inventoryJournal.IsRead = true;
			List<string> list = new List<string>();
			int num = 0;
			string key = $"inv_jou_{inventoryJournal.JournalType}_{num}";
			while (Loc.DoesExist(key))
			{
				list.Add(Loc.Get(key));
				num++;
				key = $"inv_jou_{inventoryJournal.JournalType}_{num}";
			}
			GameConfigSave configSave = base.ScreenManager.SaveFileManager.ConfigSave;
			switch (inventoryJournal.JournalCategory)
			{
			case EJournalCategoryType.Memories:
				base.ScreenManager.AddScreen(new JournalEntryMemoryScreen(list, base.Font, base.GCM, base.Zoom, configSave, FullExit), base.ControllingPlayer);
				break;
			case EJournalCategoryType.Letters:
				base.ScreenManager.AddScreen(new JournalEntryLetterScreen(list, base.Font, base.GCM, base.Zoom, inventoryJournal.JournalType, configSave, FullExit), base.ControllingPlayer);
				break;
			case EJournalCategoryType.Files:
				base.ScreenManager.AddScreen(new JournalEntryFileScreen(list, base.Font, base.GCM, base.Zoom, item.Name, configSave, FullExit), base.ControllingPlayer);
				break;
			}
		}
	}

	private void OnQuestCharacterSelected(MenuEntryCollection selectedQuests)
	{
		if (selectedQuests != null && selectedQuests.Entries.Count > 0)
		{
			_selectedQuestCollection = selectedQuests;
			ChangeMenuCollection(selectedQuests, shouldPush: true);
		}
	}

	private void OnEnemySelected(BestiaryMenuEntry selectedEnemy)
	{
		if (selectedEnemy.KillCount > 0)
		{
			if (_bestiaryDummyLevel == null)
			{
				Jukebox inJukebox = new Jukebox(shouldLoad: false, null);
				LevelSpecification debugLevel = LevelSpecification.DebugLevel;
				LevelChangeRequest levelChangeRequest = new LevelChangeRequest();
				levelChangeRequest.LevelID = debugLevel.ID;
				LevelChangeRequest levelChangeRequest2 = levelChangeRequest;
				_bestiaryDummyLevel = new Level(base.GCM, debugLevel, new MinimapSpecification(), inJukebox, GameScreenCenter, LevelScreenCenter, 0, GameSave.EditorSave, GameConfigSave.EditorSave, levelChangeRequest2, new Dictionary<int, IEnumerable<BackgroundSpecification>>());
			}
			string title = Loc.Get("journal_category_bestiary");
			base.ScreenManager.AddScreen(new BestiaryEntryScreen(title, base.SaveFile, base.GCM, selectedEnemy, _bestiaryInventory, _bestiaryDummyLevel, FullExit), base.ControllingPlayer);
		}
		else
		{
			PlayErrorSound();
		}
	}

	private void FullExit()
	{
		ExitScreen();
		_finalFullExitAction();
	}

	protected override void OnSelectedEntryChanged(int entryIndex)
	{
		base.OnSelectedEntryChanged(entryIndex);
		_memoriesInventoryCollection.IsVisible = _selectedMenuCollection == _memoriesInventoryCollection || (_selectedMenuCollection == _primaryMenuCollection && entryIndex == 0);
		_lettersInventoryCollection.IsVisible = _selectedMenuCollection == _lettersInventoryCollection || (_selectedMenuCollection == _primaryMenuCollection && entryIndex == 1);
		_filesInventoryCollection.IsVisible = _selectedMenuCollection == _filesInventoryCollection || (_selectedMenuCollection == _primaryMenuCollection && entryIndex == 2);
		_questInventory.IsVisible = _selectedMenuCollection == _questInventory || (_selectedMenuCollection == _primaryMenuCollection && entryIndex == 3) || _selectedMenuCollection == _selectedQuestCollection;
		_bestiaryInventory.IsVisible = _selectedMenuCollection == _bestiaryInventory || (_selectedMenuCollection == _primaryMenuCollection && entryIndex == 4);
		base.CursorOffset = ((_selectedMenuCollection == _featsInventory) ? _featsCursorOffset : Point.Zero);
		_isQuestMenuSelected = _questInventory.IsVisible;
	}

	public override void DrawFrames(SpriteBatch spriteBatch, Color drawColor)
	{
		SpriteEffects[] array = new SpriteEffects[9]
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
		};
		DrawingEx.DrawIrregularBox(spriteBatch, _mainMenuBackgroundDrawRectangle, drawColor, base.Sprite, base.Zoom, new int[9] { 37, 38, 37, 39, 40, 39, 37, 38, 37 }, array, shouldTile: true);
		array[2] = SpriteEffects.FlipHorizontally;
		array[5] = SpriteEffects.FlipHorizontally;
		array[6] = SpriteEffects.FlipVertically;
		array[7] = SpriteEffects.FlipVertically;
		array[8] = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
		DrawingEx.DrawIrregularBox(spriteBatch, _subMenuBackgroundDrawRectangle, drawColor, base.Sprite, base.Zoom, new int[9] { 41, 42, 41, 43, 44, 43, 41, 42, 41 }, array, shouldTile: true);
		if (_isQuestMenuSelected)
		{
			spriteBatch.Draw(base.Sprite.Texture, _questPortraitBackDrawPosition, _questPortraitBackFrameSource, drawColor, 0f, Vector2.Zero, base.Zoom, SpriteEffects.None, 0f);
			_questInventory.DrawPortrait(spriteBatch, drawColor);
		}
		base.DrawFrames(spriteBatch, drawColor);
		array[2] = SpriteEffects.FlipHorizontally;
		array[6] = SpriteEffects.FlipVertically;
		array[8] = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
		DrawingEx.DrawIrregularBox(spriteBatch, _mainMenuFrameDrawRectangle, drawColor, base.Sprite, base.Zoom, new int[9] { 35, 36, 35, -1, -1, 25, -1, -1, 35 }, array, shouldTile: true);
		DrawingEx.DrawIrregularBox(spriteBatch, _subMenuFrameDrawRectangle, drawColor, base.Sprite, base.Zoom, new int[9] { 35, 36, 35, 25, -1, -1, 35, -1, -1 }, array, shouldTile: true);
		base.ScrollbarWidget.Draw(spriteBatch, base.ScrollBarDrawPosition, base.Sprite, base.GCM.EfBrighten, drawColor, base.Zoom, base.ScrollBarHeight);
	}

	internal void GoToJournal(int itemValue)
	{
		Dictionary<int, InventoryJournal> inventory = base.SaveFile.Inventory.JournalCollection.Inventory;
		if (inventory.ContainsKey(itemValue))
		{
			InventoryJournal inventoryJournal = inventory[itemValue];
			JournalSelectedAction(inventoryJournal);
			switch (inventoryJournal.JournalCategory)
			{
			case EJournalCategoryType.Memories:
				_primaryMenuCollection.SetSelectedIndex(0);
				ChangeMenuCollection(_memoriesInventoryCollection, shouldPush: true);
				_memoriesInventoryCollection.SelectItem(itemValue);
				break;
			case EJournalCategoryType.Letters:
				_primaryMenuCollection.SetSelectedIndex(1);
				ChangeMenuCollection(_lettersInventoryCollection, shouldPush: true);
				_lettersInventoryCollection.SelectItem(itemValue);
				break;
			case EJournalCategoryType.Files:
				_primaryMenuCollection.SetSelectedIndex(2);
				ChangeMenuCollection(_filesInventoryCollection, shouldPush: true);
				_filesInventoryCollection.SelectItem(itemValue);
				break;
			}
			OnSelectedEntryChanged(_selectedMenuCollection.SelectedIndex);
		}
	}
}
