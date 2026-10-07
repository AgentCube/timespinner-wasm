using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.HUD;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameStateManagement.ScreenManager;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

namespace Timespinner.GameStateManagement.Screens.PauseMenu;

internal class DebugLocMenuScreen : InventoryMenuScreen
{
	private const float Delta = 1f / 60f;

	private const float BackgroundDrawOffsetY = 7f / 64f;

	private const float ScrollbarPositionRatioY = 11f / 64f;

	private const float ScrollbarHeightRatio = 133f / 192f;

	private Rectangle _backgroundDrawRectangle;

	private Rectangle _menuFrameDrawRectangle;

	private DialogueBox _activeDialogue;

	public DebugLocMenuScreen(string title, GameSave inSave, GCM gcm, Action fullExitAction)
		: base(title, inSave, gcm, fullExitAction)
	{
		base.DoesDrawBrackets = false;
		base.DoesHaveWideColumns = true;
		base.DoesDrawTopLowerFrame = false;
		base.DoesDrawScrollbarWidget = true;
		if (!Loc.IsAsianLocale)
		{
			_primaryMenuCollection.ScrollRowHeight = 13;
			_primaryMenuCollection.EntryHeightOffset = -6;
		}
		else
		{
			_primaryMenuCollection.ScrollRowHeight = 10;
			_primaryMenuCollection.EntryHeightOffset = -2;
		}
		_primaryMenuCollection.ColumnCount = 1;
		_primaryMenuCollection.DoesMenuAllowScrolling = true;
	}

	internal override void RefreshSizes()
	{
		base.RefreshSizes();
		int num = (int)(7f / 64f * (float)_topSectionHeight);
		_backgroundDrawRectangle = new Rectangle(_screenLeft + 2 * base.Zoom, _screenTop + num, _screenWidth - 4 * base.Zoom, _topSectionHeight - num - 8 * base.Zoom);
		_primaryMenuCollection.DrawPosition += new Vector2(0f, 4 * base.Zoom);
		int screenWidth = _screenWidth;
		_menuFrameDrawRectangle = new Rectangle(_screenLeft + base.Zoom, _screenTop + num, screenWidth - 2 * base.Zoom, _topSectionHeight - num);
		base.ScrollBarDrawPosition = new Vector2(base.ScrollBarDrawPosition.X, _screenTop + (int)(11f / 64f * (float)_topSectionHeight));
		base.ScrollBarHeight = (int)(133f / 192f * (float)_topSectionHeight);
	}

	private void AddDialogueBox(string key)
	{
		StringInstance dialogue = Loc.GetDialogue(key);
		DialogueBox.EDialogueBoxType dialogueType = (dialogue.Speaker.Contains("narration") ? DialogueBox.EDialogueBoxType.Ghost : DialogueBox.EDialogueBoxType.Default);
		_activeDialogue = new DialogueBox(dialogue.Text, dialogue.Speaker, inHideUI: true, base.GCM, base.ScreenManager.Jukebox, base.SaveFile, dialogueType, base.ScreenManager.MenuControllerMapping);
	}

	public override void HandleInput(InputState input)
	{
		if (_activeDialogue != null)
		{
			_activeDialogue.Update(1f / 60f, input, base.ControllingPlayer);
			if (_activeDialogue.IsFinished)
			{
				_activeDialogue = null;
			}
		}
		else
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
		DrawingEx.DrawIrregularBox(spriteBatch, _backgroundDrawRectangle, drawColor, base.Sprite, base.Zoom, new int[9] { 41, 42, 41, 43, 44, 43, 41, 42, 41 }, array, shouldTile: true);
		base.DrawFrames(spriteBatch, drawColor);
		array[2] = SpriteEffects.FlipHorizontally;
		array[6] = SpriteEffects.FlipVertically;
		array[8] = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
		DrawingEx.DrawIrregularBox(spriteBatch, _menuFrameDrawRectangle, drawColor, base.Sprite, base.Zoom, new int[9] { 35, 36, 35, -1, -1, 25, -1, -1, 35 }, array, shouldTile: true);
		base.ScrollbarWidget.Draw(spriteBatch, base.ScrollBarDrawPosition, base.Sprite, base.GCM.EfBrighten, drawColor, base.Zoom, base.ScrollBarHeight);
	}

	protected override void DrawMenus(SpriteBatch spriteBatch)
	{
		base.DrawMenus(spriteBatch);
		if (_activeDialogue != null)
		{
			_activeDialogue.Draw(spriteBatch);
		}
		_doesUseCursor = _activeDialogue == null;
	}
}
