using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameStateManagement.ScreenManager;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

namespace Timespinner.GameStateManagement.Screens.Shop;

internal class ShopLobbyMenuScreen : InventoryMenuScreen
{
	private const float MenuFramePositionXRatio = 0.0375f;

	private const float MenuFramePositionYRatio = 25f / 64f;

	private const float MenuFramePositionWidthRatio = 0.2875f;

	private const float MenuFrameHeightRatio = 23f / 48f;

	private const float MenuPositionXRatio = 0.075f;

	private const float MenuPositionYRatio = 47f / 96f;

	private const float StatsBackgroundDrawOffsetY = 7f / 64f;

	private readonly NPCBase.ENPCType _npcType;

	private readonly MerchantInventory _merchandiseInventory;

	private Rectangle _statsBackgroundDrawRectangle;

	private Rectangle _menuEntriesFrameRect;

	public ShopLobbyMenuScreen(GameSave inSave, GCM gcm, NPCBase.ENPCType npcType, MerchantInventory merchandiseInventory)
		: base(Loc.Get("shop_shop"), inSave, gcm, null)
	{
		_npcType = npcType;
		_merchandiseInventory = merchandiseInventory;
		base.DoesDrawTopLowerFrame = false;
		MenuEntry menuEntry = new MenuEntry(Loc.Get("shop_buy"))
		{
			Description = Loc.Get("shop_buy_desc")
		};
		MenuEntry menuEntry2 = new MenuEntry(Loc.Get("shop_sell"))
		{
			Description = Loc.Get("shop_sell_desc")
		};
		MenuEntry menuEntry3 = new MenuEntry(Loc.Get("shop_exit"))
		{
			Description = Loc.Get("shop_exit_desc")
		};
		EventHandler<PlayerIndexEventArgs> value = delegate
		{
			BuyMenuEntrySelected();
		};
		menuEntry.Selected += value;
		menuEntry2.Selected += delegate
		{
			SellMenuEntrySelected();
		};
		menuEntry3.Selected += base.OnCancel;
		base.MenuEntries.Add(menuEntry);
		base.MenuEntries.Add(menuEntry2);
		base.MenuEntries.Add(menuEntry3);
	}

	internal override void RefreshSizes()
	{
		base.RefreshSizes();
		int num = (int)(7f / 64f * (float)_topSectionHeight);
		_statsBackgroundDrawRectangle = new Rectangle(_screenLeft + 3 * base.Zoom, _screenTop + num, _screenWidth - 6 * base.Zoom, _topSectionHeight - num - 8 * base.Zoom);
		_menuEntriesFrameRect = new Rectangle(_screenLeft + (int)(0.0375f * (float)_screenWidth), _screenTop + (int)(25f / 64f * (float)_topSectionHeight), (int)(0.2875f * (float)_screenWidth), (int)(23f / 48f * (float)_topSectionHeight));
		_primaryMenuCollection.Font = base.ScreenManager.MenuFont;
		_primaryMenuCollection.DrawPosition = new Vector2((float)_screenLeft + 0.075f * (float)_screenWidth, (float)_screenTop + 47f / 96f * (float)_topSectionHeight);
	}

	private void BuyMenuEntrySelected()
	{
		ShopMenuScreen screen = new ShopMenuScreen(base.SaveFile, base.GCM, _merchandiseInventory, isBuying: true, ExitScreen);
		base.ScreenManager.AddScreen(screen, base.ControllingPlayer);
	}

	private void SellMenuEntrySelected()
	{
		bool flag = false;
		MerchantInventory merchantInventory = new MerchantInventory();
		foreach (KeyValuePair<int, InventoryUseItem> item in base.SaveFile.Inventory.UseItemInventory.Inventory)
		{
			merchantInventory.AddItem(item.Value.UseItemType);
			flag = true;
		}
		foreach (KeyValuePair<int, InventoryEquipment> item2 in base.SaveFile.Inventory.EquipmentInventory.Inventory)
		{
			merchantInventory.AddItem(item2.Value.EquipmentType);
			flag = true;
		}
		if (flag)
		{
			ShopMenuScreen screen = new ShopMenuScreen(base.SaveFile, base.GCM, merchantInventory, isBuying: false, ExitScreen);
			base.ScreenManager.AddScreen(screen, base.ControllingPlayer);
		}
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
		DrawingEx.DrawIrregularBox(spriteBatch, _statsBackgroundDrawRectangle, drawColor, base.Sprite, base.Zoom, new int[9] { 41, 42, 41, 43, 44, 43, 41, 42, 41 }, array, shouldTile: true);
		base.DrawFrames(spriteBatch, drawColor);
		array[1] = SpriteEffects.FlipVertically;
		array[2] = SpriteEffects.FlipHorizontally;
		array[6] = SpriteEffects.FlipVertically;
		array[7] = SpriteEffects.None;
		array[8] = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
		DrawingEx.DrawIrregularBox(spriteBatch, _menuEntriesFrameRect, drawColor, base.GCM.SpPauseMenu, base.Zoom, new int[9] { 28, 23, 28, -1, 1, -1, 28, 23, 28 }, array);
	}

	public override void DrawHeader(SpriteBatch spriteBatch, Color drawColor)
	{
		DrawingEx.DrawIrregularBox(flipped: new SpriteEffects[9]
		{
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.FlipHorizontally,
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.FlipVertically,
			SpriteEffects.FlipVertically,
			SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically
		}, spriteBatch: spriteBatch, backgroundRectangle: new Rectangle(_screenLeft, _screenTop, _screenWidth, _topSectionHeight), color: drawColor, sprite: base.Sprite, zoom: base.Zoom, frames: new int[9] { 53, 33, 53, 25, -1, 26, 35, 36, 35 }, shouldTile: true);
	}
}
