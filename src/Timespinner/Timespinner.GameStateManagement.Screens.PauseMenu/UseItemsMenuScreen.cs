using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.Heroes;
using Timespinner.GameObjects.StatusEffects;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

namespace Timespinner.GameStateManagement.Screens.PauseMenu;

internal class UseItemsMenuScreen : InventoryMenuScreen
{
	private const float StatsBackgroundDrawOffsetY = 7f / 64f;

	private const float StatsBackgroundHeightRatio = 0.4375f;

	private const float PlayerStatsRatioX = 0.125f;

	private const float PlayerStatsRatioY = 0.1875f;

	private const float PlayerStatsWidthRatio = 0.4f;

	private const float PlayerStatusRatioX = 0.275f;

	private const float PlayerStatusRatioY = 0.375f;

	private const float PlayerStatusWidthRatio = 0.4f;

	private const float HealthBarPositionXRatio = 0.6f;

	private const float HealthBarPositionYRatio = 25f / 96f;

	private readonly MenuUseItemInventory _useItemInventory;

	private readonly ConfirmationMenuEntryCollection _confirmationMenuEntryCollection;

	private readonly StatCollection _playerStats = new StatCollection();

	private readonly StatCollection _playerStatusCollection = new StatCollection();

	private readonly Protagonist _protagonist;

	private readonly Level _level;

	private readonly Action _onExitScreen;

	private readonly Action _exitPauseMenu;

	private Rectangle _statsBackgroundDrawRectangle;

	private InventoryUseItem _currentItemToUse;

	public UseItemsMenuScreen(GameSave inSave, GCM gcm, Level level, Action onExitUseItemScreen, Action fullExitAction)
		: base(Loc.Get("UseItemsMenuTitle"), inSave, gcm, fullExitAction)
	{
		_level = level;
		_useItemInventory = new MenuUseItemInventory(inSave.Inventory.UseItemInventory, ItemSelectedAction)
		{
			IsVisible = true
		};
		_primaryMenuCollection = _useItemInventory;
		_selectedMenuCollection = _primaryMenuCollection;
		_onExitScreen = onExitUseItemScreen;
		_exitPauseMenu = fullExitAction;
		base.DoesDrawBrackets = true;
		base.DoesDrawBracketsOverAll = true;
		base.DoesDrawHealthbar = true;
		base.DoesDrawTopLowerFrame = true;
		base.DoesDrawScrollbarWidget = true;
		base.StatCollections.Add(_playerStats);
		base.StatCollections.Add(_playerStatusCollection);
		_confirmationMenuEntryCollection = new ConfirmationMenuEntryCollection(Loc.Get("use_item_yes"), Loc.Get("use_item_no"), Loc.Get("use_item_confirm"), OnUseItemConfirm, base.OnCancel);
		_subMenuCollections.Add(_confirmationMenuEntryCollection);
		_protagonist = _level.MainHero;
	}

	internal override void RefreshSizes()
	{
		base.RefreshSizes();
		int num = (int)(7f / 64f * (float)_topSectionHeight);
		_statsBackgroundDrawRectangle = new Rectangle(_screenLeft + 2 * base.Zoom, _screenTop + num, _screenWidth - 4 * base.Zoom, (int)(0.4375f * (float)_topSectionHeight));
		_useItemInventory.SetColumnWidth(base.ListColumnWidth, base.Zoom);
		_useItemInventory.DrawPosition = base.ListTextDrawPosition;
		base.HealthBarDrawPosition = new Vector2((int)(0.6f * (float)_screenWidth) + _screenLeft, _screenTop + (int)(25f / 96f * (float)_topSectionHeight));
		_playerStats.Width = (int)(0.4f * (float)_screenWidth);
		_playerStats.Location = new Vector2((float)_screenLeft + 0.125f * (float)_screenWidth, (float)_screenTop + (float)_topSectionHeight * 0.1875f);
		RefreshPlayerStats();
		_playerStatusCollection.Width = (int)(0.4f * (float)_screenWidth);
		_playerStatusCollection.Location = new Vector2((float)_screenLeft + 0.275f * (float)_screenWidth, (float)_screenTop + (float)_topSectionHeight * 0.375f);
		Vector2 drawPosition = new Vector2((float)(int)base.DescriptionDrawPosition.X + (float)_screenWidth * 0.125f, (int)(base.DescriptionDrawPosition.Y + (float)_bottomSectionHeight * 0.5f));
		_confirmationMenuEntryCollection.DrawPosition = drawPosition;
		_confirmationMenuEntryCollection.SetColumnWidth(base.ListColumnWidth, base.Zoom);
	}

	protected override void OnSelectedEntryChanged(int entryIndex)
	{
		base.OnSelectedEntryChanged(entryIndex);
		if (_selectedMenuCollection != _confirmationMenuEntryCollection)
		{
			_confirmationMenuEntryCollection.IsVisible = false;
		}
	}

	private bool ItemSelectedAction(InventoryItem item)
	{
		bool result = false;
		if (item is InventoryUseItem inventoryUseItem && inventoryUseItem.IsUsable(_protagonist, base.SaveFile))
		{
			result = true;
			_currentItemToUse = inventoryUseItem;
			ChangeMenuCollection(_confirmationMenuEntryCollection, shouldPush: true);
		}
		else
		{
			PlayErrorSound();
		}
		return result;
	}

	private void OnUseItemConfirm(object arg1, PlayerIndexEventArgs arg2)
	{
		bool flag = _currentItemToUse.UseItem(_protagonist, base.SaveFile);
		RefreshPlayerStats();
		base.ScreenManager.Jukebox.PlayCue(ESFX.MenuHeal);
		bool flag2 = _useItemInventory.RemoveItem(_currentItemToUse);
		if (flag2)
		{
			int num = _useItemInventory.SelectedIndex;
			if (num >= _useItemInventory.Entries.Count)
			{
				num--;
			}
			if (num >= 0)
			{
				if (_useItemInventory.SetSelectedIndex(num))
				{
					OnSelectedEntryChanged(num);
					Update(new GameTime(), otherScreenHasFocus: false, coveredByOtherScreen: false);
				}
			}
			else
			{
				OnCancel(base.ControllingPlayer ?? PlayerIndex.One);
			}
		}
		if (!flag)
		{
			if (flag2 || !_currentItemToUse.IsUsable(_protagonist, base.SaveFile))
			{
				OnCancel(arg1, arg2);
			}
		}
		else
		{
			ExitScreen();
			_exitPauseMenu();
		}
	}

	private void RefreshPlayerStats()
	{
		_playerHealth = _protagonist.HP;
		_playerMaxHealth = _protagonist.MaxHP;
		_playerSand = _protagonist.MP;
		_playerMaxSand = _protagonist.MaxMP;
		_playerAura = _protagonist.Aura;
		_playerMaxAura = _protagonist.MaxAura;
		_playerStatus = _protagonist.GetFirstActiveStatusEffect();
		_playerStats.Entries.Clear();
		_playerStats.Entries.Add(new StatEntry
		{
			Title = Loc.Get("StatHealth"),
			Type = StatEntry.EStatDisplayType.Ratio,
			Value = _playerHealth,
			Value2 = _playerMaxHealth
		});
		_playerStats.Entries.Add(new StatEntry
		{
			Title = Loc.Get("StatAura"),
			Type = StatEntry.EStatDisplayType.Ratio,
			Value = _playerAura,
			Value2 = _playerMaxAura
		});
		_playerStats.Entries.Add(new StatEntry
		{
			Title = Loc.Get("StatSand"),
			Type = StatEntry.EStatDisplayType.Ratio,
			Value = _playerSand,
			Value2 = _playerMaxSand
		});
		_playerStatusCollection.Entries.Clear();
		Color textColor = Color.White;
		switch (_playerStatus)
		{
		case EStatusEffectType.None:
			textColor = new Color(74, 149, 86);
			break;
		case EStatusEffectType.Poison:
			textColor = new Color(160, 96, 196);
			break;
		case EStatusEffectType.Burn:
			textColor = new Color(240, 176, 64);
			break;
		case EStatusEffectType.Suffocate:
			textColor = Color.Teal;
			break;
		case EStatusEffectType.Chaos:
			textColor = new Color(240, 40, 60);
			break;
		}
		_playerStatusCollection.Entries.Add(new StatEntry
		{
			Title = Loc.Get("StatStatus"),
			Type = StatEntry.EStatDisplayType.ColoredText,
			Text = Loc.Get($"StatStatus{_playerStatus}"),
			TextColor = textColor
		});
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
		}, spriteBatch: spriteBatch, backgroundRectangle: _statsBackgroundDrawRectangle, color: drawColor, sprite: base.Sprite, zoom: base.Zoom, frames: new int[9] { 41, 42, 41, 43, 44, 43, 41, 42, 41 }, shouldTile: true);
		base.DrawFrames(spriteBatch, drawColor);
	}

	public override void ExitScreen()
	{
		_onExitScreen();
		base.ExitScreen();
	}

	internal void GoToUseItem(int itemValue)
	{
		Dictionary<int, InventoryUseItem> inventory = base.SaveFile.Inventory.UseItemInventory.Inventory;
		if (inventory.ContainsKey(itemValue))
		{
			_useItemInventory.SelectItem(itemValue);
			OnSelectedEntryChanged(_selectedMenuCollection.SelectedIndex);
		}
	}
}
