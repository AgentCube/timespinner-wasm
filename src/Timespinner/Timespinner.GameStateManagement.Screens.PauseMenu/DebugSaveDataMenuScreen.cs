using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

namespace Timespinner.GameStateManagement.Screens.PauseMenu;

internal class DebugSaveDataMenuScreen : InventoryMenuScreen
{
	private const float BackgroundDrawOffsetY = 7f / 64f;

	private const float SubMenuDisplayOffsetX = 0.7f;

	private const float ScrollbarPositionRatioY = 11f / 64f;

	private const float ScrollbarHeightRatio = 133f / 192f;

	private readonly MenuEntry _boolsMenuEntry;

	private readonly MenuEntry _intsMenuEntry;

	private readonly SaveDataMenuEntryCollection _boolDataMenuEntryCollection;

	private readonly SaveDataMenuEntryCollection _intDataMenuEntryCollection;

	private Vector2 _subMenuDisplayPosition;

	private Rectangle _mainMenuBackgroundDrawRectangle;

	private Rectangle _subMenuBackgroundDrawRectangle;

	private Rectangle _mainMenuFrameDrawRectangle;

	private Rectangle _subMenuFrameDrawRectangle;

	public DebugSaveDataMenuScreen(GameSave inSave, GCM gcm, Action fullExitAction)
		: base("Edit Save Data", inSave, gcm, fullExitAction)
	{
		_boolsMenuEntry = new MenuEntry("Booleans")
		{
			Description = "Set savefile booleans."
		};
		_intsMenuEntry = new MenuEntry("Integers")
		{
			Description = "Set savefile integers."
		};
		_boolsMenuEntry.Selected += OnBoolsEntrySelected;
		_intsMenuEntry.Selected += OnIntsEntrySelected;
		base.MenuEntries.Add(_boolsMenuEntry);
		base.MenuEntries.Add(_intsMenuEntry);
		_boolDataMenuEntryCollection = new SaveDataMenuEntryCollection(base.SaveFile, isIntegers: false);
		_intDataMenuEntryCollection = new SaveDataMenuEntryCollection(base.SaveFile, isIntegers: true);
		_subMenuCollections.Add(_boolDataMenuEntryCollection);
		_subMenuCollections.Add(_intDataMenuEntryCollection);
		base.DoesDrawBrackets = false;
		base.DoesHaveWideColumns = true;
		base.DoesDrawTopLowerFrame = false;
		base.DoesDrawScrollbarWidget = true;
	}

	internal override void RefreshSizes()
	{
		base.RefreshSizes();
		int num = (int)(7f / 64f * (float)_topSectionHeight);
		int num2 = (int)((float)_screenWidth * 0.3f);
		int num3 = _screenWidth - num2;
		_mainMenuBackgroundDrawRectangle = new Rectangle(_screenLeft + 4 * base.Zoom, _screenTop + num + 8 * base.Zoom, num2 - 6 * base.Zoom, _topSectionHeight - num - 16 * base.Zoom);
		_subMenuBackgroundDrawRectangle = new Rectangle(_mainMenuBackgroundDrawRectangle.Right + 3 * base.Zoom, _mainMenuBackgroundDrawRectangle.Top, num3 - 9 * base.Zoom, _mainMenuBackgroundDrawRectangle.Height);
		_mainMenuFrameDrawRectangle = new Rectangle(_screenLeft + base.Zoom, _screenTop + num, num2 - 2 * base.Zoom, _topSectionHeight - num);
		_subMenuFrameDrawRectangle = new Rectangle(_mainMenuFrameDrawRectangle.Right + base.Zoom, _mainMenuFrameDrawRectangle.Top, num3 - base.Zoom, _mainMenuFrameDrawRectangle.Height);
		_primaryMenuCollection.DrawPosition = new Vector2(_primaryMenuCollection.DrawPosition.X, _primaryMenuCollection.DrawPosition.Y + (float)(12 * base.Zoom));
		_subMenuDisplayPosition = new Vector2(num2 + _screenLeft + 24 * base.Zoom, _primaryMenuCollection.DrawPosition.Y);
		_boolDataMenuEntryCollection.DrawPosition = _subMenuDisplayPosition;
		_intDataMenuEntryCollection.DrawPosition = _subMenuDisplayPosition;
		base.ScrollBarDrawPosition = new Vector2(base.ScrollBarDrawPosition.X, _screenTop + (int)(11f / 64f * (float)_topSectionHeight));
		base.ScrollBarHeight = (int)(133f / 192f * (float)_topSectionHeight);
	}

	private void OnBoolsEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		if (_boolDataMenuEntryCollection.Entries.Count > 0)
		{
			ChangeMenuCollection(_boolDataMenuEntryCollection, shouldPush: true);
		}
	}

	private void OnIntsEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		if (_intDataMenuEntryCollection.Entries.Count > 0)
		{
			ChangeMenuCollection(_intDataMenuEntryCollection, shouldPush: true);
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
		DrawingEx.DrawIrregularBox(spriteBatch, _mainMenuBackgroundDrawRectangle, drawColor, base.Sprite, base.Zoom, new int[9] { 37, 38, 37, 39, 40, 39, 37, 38, 37 }, array, shouldTile: true);
		array[2] = SpriteEffects.FlipHorizontally;
		array[5] = SpriteEffects.FlipHorizontally;
		array[6] = SpriteEffects.FlipVertically;
		array[7] = SpriteEffects.FlipVertically;
		array[8] = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
		DrawingEx.DrawIrregularBox(spriteBatch, _subMenuBackgroundDrawRectangle, drawColor, base.Sprite, base.Zoom, new int[9] { 41, 42, 41, 43, 44, 43, 41, 42, 41 }, array, shouldTile: true);
		base.DrawFrames(spriteBatch, drawColor);
		array[2] = SpriteEffects.FlipHorizontally;
		array[6] = SpriteEffects.FlipVertically;
		array[8] = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
		DrawingEx.DrawIrregularBox(spriteBatch, _mainMenuFrameDrawRectangle, drawColor, base.Sprite, base.Zoom, new int[9] { 35, 36, 35, -1, -1, 25, -1, -1, 35 }, array, shouldTile: true);
		DrawingEx.DrawIrregularBox(spriteBatch, _subMenuFrameDrawRectangle, drawColor, base.Sprite, base.Zoom, new int[9] { 35, 36, 35, 25, -1, -1, 35, -1, -1 }, array, shouldTile: true);
		base.ScrollbarWidget.Draw(spriteBatch, base.ScrollBarDrawPosition, base.Sprite, base.GCM.EfBrighten, drawColor, base.Zoom, base.ScrollBarHeight);
	}
}
