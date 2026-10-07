using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;

namespace Timespinner.GameStateManagement.Screens.PauseMenu.Journal;

internal class JournalEntryLetterScreen : JournalEntryScreen
{
	private const int LetterMaxLines = 11;

	private const int TextOffsetX = 24;

	private const int TextOffsetY = 8;

	private const int RightMarginX = -16;

	private const int TextBoxHeight = 192;

	private const int DialogueWidth = 272;

	private const int NewLineIconMarginX = 16;

	private const int NewLineIconMarginY = -24;

	private const int WaxDrawOffsetX = -8;

	private const int WaxDrawOffsetY = -16;

	private const int LargeBloodDrawOffsetX = 304;

	private const int LargeBloodDrawOffsetY = 24;

	private const int MediumBloodDrawOffsetX = 36;

	private const int MediumBloodDrawOffsetY = 34;

	private const int SmallABloodDrawOffsetX = 20;

	private const int SmallABloodDrawOffsetY = 56;

	private const int SmallBBloodDrawOffsetX = 44;

	private const int SmallBBloodDrawOffsetY = 80;

	private const int TinyABloodDrawOffsetX = 36;

	private const int TinyABloodDrawOffsetY = 112;

	private const int TinyBBloodDrawOffsetX = -40;

	private const int TinyBBloodDrawOffsetY = 8;

	private static readonly Color TextColor = new Color(56, 40, 48);

	private static readonly Color ShadowColor = new Color(168, 128, 88);

	private static readonly Color BloodColor = Color.White * 0.65f;

	private readonly bool _isBloody;

	private readonly Rectangle _waxFrameSource;

	private readonly SpriteSheet _sprite;

	private EBGM _songThatWasPlayingBefore;

	private Vector2 _waxDrawPosition;

	private Vector2 _largeBloodDrawPosition;

	private Vector2 _mediumBloodDrawPosition;

	private Vector2 _smallABloodDrawPosition;

	private Vector2 _smallBBloodDrawPosition;

	private Vector2 _tinyABloodDrawPosition;

	private Vector2 _tinyBBloodDrawPosition;

	private Rectangle _messageBoxDimensions;

	private Rectangle _mainMenuBackgroundDrawRectangle;

	public JournalEntryLetterScreen(List<string> entries, SpriteFont font, GCM gcm, int scale, EInventoryJournalType journal, GameConfigSave configSave, Action fullExitAction)
		: base(entries, font, gcm, scale, 272, 11, doesAddNewlines: false, configSave, fullExitAction)
	{
		bool flag = journal != EInventoryJournalType.Letter3 && journal != EInventoryJournalType.Letter8 && journal != EInventoryJournalType.Letter9;
		_isBloody = journal == EInventoryJournalType.Letter10;
		base.NewLineIconIndex = 17;
		_sprite = gcm.SpJournalMenu;
		_waxFrameSource = _sprite.GetFrameSource(flag ? 7 : 8);
		base.DoesDelayFromNewlines = false;
		base.DoesDelayEachLetter = false;
	}

	public override void LoadContent()
	{
		base.LoadContent();
		RefreshSizes();
		_songThatWasPlayingBefore = base.ScreenManager.Jukebox.CurrentSongEnum;
		base.ScreenManager.Jukebox.PlaySong(EBGM.CsLetters);
	}

	internal override void OnScreenResize()
	{
		base.OnScreenResize();
		RefreshSizes();
	}

	private void RefreshSizes()
	{
		_mainMenuBackgroundDrawRectangle = new Rectangle(base.LetterBoxOffsetX, base.LetterBoxOffsetY, base.VisibleScreenWidth + -16 * base.Scale, base.VisibleScreenHeight);
		_waxDrawPosition = new Vector2(_mainMenuBackgroundDrawRectangle.Right + -8 * base.Scale, _mainMenuBackgroundDrawRectangle.Height / 2 + -16 * base.Scale + base.LetterBoxOffsetY);
		_messageBoxDimensions = new Rectangle(base.LetterBoxOffsetX + 64 * base.Scale, base.LetterBoxOffsetY + 32 * base.Scale, 272 * base.Scale, 192 * base.Scale);
		base.NewlineIconDrawPosition = new Vector2(_messageBoxDimensions.Right + 16 * base.Scale, _messageBoxDimensions.Bottom + -24 * base.Scale);
		_largeBloodDrawPosition = new Vector2(_mainMenuBackgroundDrawRectangle.Left + 304 * base.Scale, _mainMenuBackgroundDrawRectangle.Top + 24 * base.Scale);
		_mediumBloodDrawPosition = new Vector2(_largeBloodDrawPosition.X + (float)(36 * base.Scale), _largeBloodDrawPosition.Y + (float)(34 * base.Scale));
		_smallABloodDrawPosition = new Vector2(_largeBloodDrawPosition.X + (float)(20 * base.Scale), _largeBloodDrawPosition.Y + (float)(56 * base.Scale));
		_smallBBloodDrawPosition = new Vector2(_largeBloodDrawPosition.X + (float)(44 * base.Scale), _largeBloodDrawPosition.Y + (float)(80 * base.Scale));
		_tinyABloodDrawPosition = new Vector2(_largeBloodDrawPosition.X + (float)(36 * base.Scale), _largeBloodDrawPosition.Y + (float)(112 * base.Scale));
		_tinyBBloodDrawPosition = new Vector2(_largeBloodDrawPosition.X + (float)(-40 * base.Scale), _largeBloodDrawPosition.Y + (float)(8 * base.Scale));
	}

	public override void ExitScreen()
	{
		base.ScreenManager.Jukebox.PlaySong(_songThatWasPlayingBefore);
		base.ExitScreen();
	}

	internal override void DrawText(SpriteBatch spriteBatch)
	{
		Vector2 drawPosition = new Vector2(_messageBoxDimensions.X, _messageBoxDimensions.Y);
		int num = base.CurrentLastLetter;
		int num2 = 0;
		while (num > 0 && num2 < base.MaxLines && num2 < base.DialogueLines.Count)
		{
			DialogueLine dialogueLine = base.DialogueLines[num2];
			num -= dialogueLine.Length;
			dialogueLine.VisibleCharacters = dialogueLine.Length + num;
			dialogueLine.Draw(spriteBatch, drawPosition, TextColor, ShadowColor, base.Scale, 1f);
			drawPosition.Y += base.LineHeight;
			num2++;
		}
	}

	internal override void DrawBackground(SpriteBatch spriteBatch)
	{
		Color white = Color.White;
		SpriteEffects[] flipped = new SpriteEffects[9];
		DrawingEx.DrawIrregularBox(spriteBatch, _mainMenuBackgroundDrawRectangle, white, _sprite, base.Scale, new int[9] { -1, 2, 5, 0, 1, 4, -1, 3, 6 }, flipped, shouldTile: true);
		spriteBatch.Draw(_sprite.Texture, _waxDrawPosition, _waxFrameSource, white, 0f, Vector2.Zero, base.Scale, SpriteEffects.None, 0f);
		if (_isBloody)
		{
			Rectangle frameSource = _sprite.GetFrameSource(11);
			spriteBatch.Draw(_sprite.Texture, _largeBloodDrawPosition, frameSource, BloodColor, 0f, Vector2.Zero, base.Scale, SpriteEffects.None, 0f);
			frameSource = _sprite.GetFrameSource(12);
			spriteBatch.Draw(_sprite.Texture, _mediumBloodDrawPosition, frameSource, BloodColor, 0f, Vector2.Zero, base.Scale, SpriteEffects.None, 0f);
			frameSource = _sprite.GetFrameSource(14);
			spriteBatch.Draw(_sprite.Texture, _smallABloodDrawPosition, frameSource, BloodColor, 0f, Vector2.Zero, base.Scale, SpriteEffects.None, 0f);
			frameSource = _sprite.GetFrameSource(13);
			spriteBatch.Draw(_sprite.Texture, _smallBBloodDrawPosition, frameSource, BloodColor, 0f, Vector2.Zero, base.Scale, SpriteEffects.None, 0f);
			frameSource = _sprite.GetFrameSource(16);
			spriteBatch.Draw(_sprite.Texture, _tinyABloodDrawPosition, frameSource, BloodColor, 0f, Vector2.Zero, base.Scale, SpriteEffects.None, 0f);
			frameSource = _sprite.GetFrameSource(15);
			spriteBatch.Draw(_sprite.Texture, _tinyBBloodDrawPosition, frameSource, BloodColor, 0f, Vector2.Zero, base.Scale, SpriteEffects.None, 0f);
		}
	}
}
