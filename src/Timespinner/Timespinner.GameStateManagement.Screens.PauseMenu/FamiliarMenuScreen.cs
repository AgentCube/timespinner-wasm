using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

namespace Timespinner.GameStateManagement.Screens.PauseMenu;

internal class FamiliarMenuScreen : InventoryMenuScreen
{
	private const int PortraitBackDrawOffsetX = 2;

	private const int PortraitBackDrawOffsetY = 39;

	private const float DisplayPortraitPositionRatioX = 0.025f;

	private const float DisplayPortraitPositionRatioY = 13f / 64f;

	private const float DisplayStatsPositionRatioX = 0.55f;

	private const float DisplayStatsPositionRatioY = 5f / 32f;

	private const float DisplayStatsWidthRatio = 0.4f;

	private const float MenuFrameDrawPositionRatioX = 1f / 160f;

	private const float PrimaryMenuDisplayRatioX = 21f / 160f;

	private const float StatsBackgroundDrawOffsetY = 7f / 64f;

	private const float StatsBackgroundHeightRatio = 83f / 192f;

	private readonly MenuFamiliarInventory _familiarMenuInventory;

	private readonly StatCollection _selectedFamiliarStats = new StatCollection();

	private readonly SpriteSheet _familiarPortraitsSprite;

	private readonly Action _onExitFamiliarScreenAction;

	private EInventoryFamiliarType _selectedFamiliarType;

	private Vector2 _portraitDrawPosition;

	private Vector2 _portraitBackDrawPosition;

	private Rectangle _portraitBackFrameSource;

	private Rectangle _menuBackgroundDrawRectangle;

	private Rectangle _statsBackgroundDrawRectangle;

	private Rectangle _menuFrameDrawRectangle;

	public FamiliarMenuScreen(GameSave inSave, GCM gcm, Action onExit, Action fullExitAction)
		: base(Loc.Get("FamiliarMenuTitle"), inSave, gcm, fullExitAction)
	{
		_onExitFamiliarScreenAction = onExit;
		_familiarPortraitsSprite = gcm.SpMenuCharacters;
		base.DoesDrawBrackets = true;
		base.DoesDrawBracketsOverAll = false;
		base.DoesDrawTopLowerFrame = true;
		base.DoesDrawScrollbarWidget = true;
		base.StatCollections.Add(_selectedFamiliarStats);
		_familiarMenuInventory = new MenuFamiliarInventory(inSave.Inventory.FamiliarInventory, OnFamiliarEquip, OnFamiliarUnequip, gcm.SpPauseMenu)
		{
			IsVisible = true
		};
		_primaryMenuCollection = _familiarMenuInventory;
		_selectedMenuCollection = _familiarMenuInventory;
		RefreshEquippedFamiliarIcon();
	}

	internal override void RefreshSizes()
	{
		base.RefreshSizes();
		int num = (int)(7f / 64f * (float)_topSectionHeight);
		_menuBackgroundDrawRectangle = new Rectangle(_screenLeft + 3 * base.Zoom, _screenTop + num, (int)((float)_screenWidth / 2f - (float)(3 * base.Zoom)), (int)(83f / 192f * (float)_topSectionHeight));
		_statsBackgroundDrawRectangle = new Rectangle(_menuBackgroundDrawRectangle.Right, _menuBackgroundDrawRectangle.Top, _menuBackgroundDrawRectangle.Width, _menuBackgroundDrawRectangle.Height);
		_menuFrameDrawRectangle = new Rectangle(_screenLeft + (int)(1f / 160f * (float)_screenWidth), _screenTop + (int)(23f / 192f * (float)_topSectionHeight), (int)(0.490625f * (float)_screenWidth), (int)(27f / 64f * (float)_topSectionHeight));
		_primaryMenuCollection.DrawPosition = new Vector2(21f / 160f * (float)_screenWidth + (float)_screenLeft, _primaryMenuCollection.DrawPosition.Y);
		_selectedFamiliarStats.Location = new Vector2(0.55f * (float)_screenWidth + (float)_screenLeft, (float)_screenTop + 5f / 32f * (float)_topSectionHeight);
		_selectedFamiliarStats.Width = (int)(0.4f * (float)_screenWidth);
		_familiarMenuInventory.DrawPosition = base.ListTextDrawPosition;
		_familiarMenuInventory.SetColumnWidth(base.ListColumnWidth, base.Zoom);
		_portraitDrawPosition = new Vector2(0.025f * (float)_screenWidth + (float)_screenLeft, (float)_screenTop + 13f / 64f * (float)_topSectionHeight);
		_portraitBackDrawPosition = new Vector2(_screenLeft + 2 * base.Zoom, _screenTop + 39 * base.Zoom);
		_portraitBackFrameSource = base.Sprite.GetFrameSource(141);
		base.CursorOffset = Point.Zero;
		RefreshDisplayStats();
	}

	private void OnFamiliarEquip(InventoryFamiliar familiar)
	{
		base.SaveFile.Inventory.EquippedFamiliar = familiar.FamiliarType;
		OnEquipItem();
	}

	private void OnEquipItem()
	{
		base.ScreenManager.Jukebox.PlayCue(ESFX.MenuEquip);
		RefreshEquippedFamiliarIcon();
	}

	private void OnFamiliarUnequip()
	{
		base.SaveFile.Inventory.EquippedFamiliar = EInventoryFamiliarType.None;
		OnUnequipItem();
	}

	private void OnUnequipItem()
	{
		base.ScreenManager.Jukebox.PlayCue(ESFX.MenuCancel);
		RefreshEquippedFamiliarIcon();
	}

	private void RefreshEquippedFamiliarIcon()
	{
		_familiarMenuInventory.EquippedFamiliar = base.SaveFile.Inventory.EquippedFamiliar;
	}

	protected override void OnSelectedEntryChanged(int entryIndex)
	{
		base.OnSelectedEntryChanged(entryIndex);
		base.CursorOffset = Point.Zero;
		_selectedFamiliarType = _familiarMenuInventory.GetSelected()?.FamiliarType ?? EInventoryFamiliarType.None;
		RefreshDisplayStats();
	}

	public override void ExitScreen()
	{
		_onExitFamiliarScreenAction();
		base.ExitScreen();
	}

	private void RefreshDisplayStats()
	{
		_selectedFamiliarStats.Entries.Clear();
		AddStatEntries(_familiarMenuInventory.GetSelected());
	}

	private void AddStatEntries(InventoryFamiliar familiar)
	{
		if (familiar != null)
		{
			_selectedFamiliarStats.Entries.Add(new StatEntry
			{
				Title = Loc.Get("StatLevel"),
				Type = StatEntry.EStatDisplayType.Number,
				Value = familiar.VisibleLevel
			});
			_selectedFamiliarStats.Entries.Add(new StatEntry
			{
				Title = Loc.Get("StatDamage"),
				Type = StatEntry.EStatDisplayType.Number,
				Value = familiar.Damage
			});
			_selectedFamiliarStats.Entries.Add(new StatEntry
			{
				Title = Loc.Get("StatKills"),
				Type = StatEntry.EStatDisplayType.Number,
				Value = familiar.Experience
			});
			_selectedFamiliarStats.Entries.Add(new StatEntry
			{
				Title = Loc.Get("StatNextLevel"),
				Type = StatEntry.EStatDisplayType.Number,
				Value = familiar.NextLevel
			});
		}
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
		DrawingEx.DrawIrregularBox(spriteBatch, _menuBackgroundDrawRectangle, drawColor, base.Sprite, base.Zoom, new int[9] { 41, 42, 41, 43, 44, 43, 41, 42, 41 }, array, shouldTile: true);
		DrawingEx.DrawIrregularBox(spriteBatch, _statsBackgroundDrawRectangle, drawColor, base.Sprite, base.Zoom, new int[9] { 45, 46, 45, 47, 48, 47, 45, 46, 45 }, array, shouldTile: true);
		array[2] = SpriteEffects.FlipHorizontally;
		array[6] = SpriteEffects.FlipVertically;
		array[8] = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
		DrawingEx.DrawIrregularBox(spriteBatch, _menuFrameDrawRectangle, drawColor, base.Sprite, base.Zoom, new int[9] { -1, -1, 34, -1, -1, -1, -1, -1, 34 }, array);
		spriteBatch.Draw(base.Sprite.Texture, _portraitBackDrawPosition, _portraitBackFrameSource, drawColor, 0f, Vector2.Zero, base.Zoom, SpriteEffects.FlipHorizontally, 0f);
		if (_selectedFamiliarType != 0)
		{
			Vector2 portraitDrawPosition = _portraitDrawPosition;
			int selectedFamiliarType = (int)_selectedFamiliarType;
			Rectangle frameSource = _familiarPortraitsSprite.GetFrameSource(selectedFamiliarType);
			spriteBatch.Draw(_familiarPortraitsSprite.Texture, portraitDrawPosition, frameSource, drawColor, 0f, Vector2.Zero, base.Zoom, SpriteEffects.None, 0f);
		}
		base.DrawFrames(spriteBatch, drawColor);
	}
}
