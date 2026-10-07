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

internal class RelicsMenuScreen : InventoryMenuScreen
{
	private const int SingleColumnWidth = 226;

	private const float BackgroundDrawOffsetY = 7f / 64f;

	private const float ScrollbarPositionRatioY = 11f / 64f;

	private const float ScrollbarHeightRatio = 133f / 192f;

	private readonly MenuRelicInventory _relicInventory;

	private Rectangle _backgroundDrawRectangle;

	private Rectangle _menuFrameDrawRectangle;

	public RelicsMenuScreen(GameSave inSave, GCM gcm, Action fullExitAction)
		: base(Loc.Get("RelicsMenuTitle"), inSave, gcm, fullExitAction)
	{
		_relicInventory = new MenuRelicInventory(inSave.Inventory.RelicInventory, RelicSelectedAction, base.Sprite)
		{
			IsVisible = true,
			ScrollRowHeight = 11
		};
		_primaryMenuCollection = _relicInventory;
		_selectedMenuCollection = _primaryMenuCollection;
		base.DoesDrawBrackets = false;
		base.DoesHaveWideColumns = true;
		base.DoesDrawTopLowerFrame = false;
		base.DoesDrawScrollbarWidget = true;
		_relicInventory.RefreshScrollWindow();
	}

	internal override void RefreshSizes()
	{
		base.RefreshSizes();
		int num = (int)(7f / 64f * (float)_topSectionHeight);
		_backgroundDrawRectangle = new Rectangle(_screenLeft + 2 * base.Zoom, _screenTop + num, _screenWidth - 4 * base.Zoom, _topSectionHeight - num - 8 * base.Zoom);
		switch (Loc.CurrentLocale)
		{
		case ELanguageLocale.BP:
		case ELanguageLocale.DE:
		case ELanguageLocale.ES:
		case ELanguageLocale.FR:
		case ELanguageLocale.JP:
			_relicInventory.ColumnCount = 1;
			_relicInventory.SetColumnWidth(226 * base.Zoom, base.Zoom);
			break;
		default:
			_relicInventory.SetColumnWidth(base.ListColumnWidth, base.Zoom);
			break;
		}
		_primaryMenuCollection.DrawPosition += new Vector2(0f, 4 * base.Zoom);
		int screenWidth = _screenWidth;
		_menuFrameDrawRectangle = new Rectangle(_screenLeft + base.Zoom, _screenTop + num, screenWidth - 2 * base.Zoom, _topSectionHeight - num);
		base.ScrollBarDrawPosition = new Vector2(base.ScrollBarDrawPosition.X, _screenTop + (int)(11f / 64f * (float)_topSectionHeight));
		base.ScrollBarHeight = (int)(133f / 192f * (float)_topSectionHeight);
	}

	private void RelicSelectedAction(InventoryItem item)
	{
		if (item is InventoryRelic inventoryRelic)
		{
			inventoryRelic.IsActive = !inventoryRelic.IsActive;
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
		DrawingEx.DrawIrregularBox(spriteBatch, _backgroundDrawRectangle, drawColor, base.Sprite, base.Zoom, new int[9] { 41, 42, 41, 43, 44, 43, 41, 42, 41 }, array, shouldTile: true);
		base.DrawFrames(spriteBatch, drawColor);
		array[2] = SpriteEffects.FlipHorizontally;
		array[6] = SpriteEffects.FlipVertically;
		array[8] = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
		DrawingEx.DrawIrregularBox(spriteBatch, _menuFrameDrawRectangle, drawColor, base.Sprite, base.Zoom, new int[9] { 35, 36, 35, -1, -1, 25, -1, -1, 35 }, array, shouldTile: true);
		base.ScrollbarWidget.Draw(spriteBatch, base.ScrollBarDrawPosition, base.Sprite, base.GCM.EfBrighten, drawColor, base.Zoom, base.ScrollBarHeight);
	}
}
