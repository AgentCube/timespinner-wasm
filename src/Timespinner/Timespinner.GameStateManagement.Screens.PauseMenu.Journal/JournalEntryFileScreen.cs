using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Saving;

namespace Timespinner.GameStateManagement.Screens.PauseMenu.Journal;

internal class JournalEntryFileScreen : JournalEntryScreen
{
	private const int FileMaxLines = 9;

	private const int TextOffsetX = 8;

	private const int TextOffsetTop = 6;

	private const int ParagraphOffsetY = 24;

	private const int TextBoxHeight = 162;

	private const int DialogueWidth = 312;

	private const int NewLineIconMarginX = -21;

	private const int NewLineIconMarginY = -20;

	private const int ScreenTopBottomMargin = 2;

	private const int ShadyCharacterCount = 3;

	private const float ShadyColorMultiplier = 2f / 3f;

	private const float DefaultFlickerSpeed = 0.05f;

	private static readonly Color TextColor = new Color(240, 192, 240);

	private static readonly Color ShadowColor = new Color(48, 64, 96);

	private static readonly Color ShadyTextColor = new Color(128, 192, 248, 128);

	private static readonly Color ShadyTextShadowColor = ShadyTextColor * 0.35f;

	private readonly string _filename;

	private readonly SpriteFont _font;

	private readonly SpriteSheet _sprite;

	private bool _isFlickeredOff;

	private float _flickerTimer;

	private Vector2 _filenameDrawPosition;

	private Vector2 _buttonDrawPosition;

	private Color _textColor;

	private Color _textShadowColor;

	private Color _titleColor;

	private Color _titleShadowColor;

	private Rectangle _messageBoxDimensions;

	private Rectangle _mainMenuBackgroundDrawRectangle;

	private Rectangle _buttonFrameSource;

	public JournalEntryFileScreen(List<string> entries, SpriteFont font, GCM gcm, int scale, string filename, GameConfigSave configSave, Action fullExitAction)
		: base(entries, font, gcm, scale, 312, 9, doesAddNewlines: true, configSave, fullExitAction)
	{
		_filename = filename;
		_font = font;
		_sprite = gcm.SpJournalMenu;
		base.DoesDelayFromNewlines = false;
		base.DoesDelayEachLetter = true;
		base.NewLineIconIndex = 26;
		RefreshTextColor();
	}

	public override void LoadContent()
	{
		base.LoadContent();
		RefreshSizes();
	}

	internal override void OnScreenResize()
	{
		base.OnScreenResize();
		RefreshSizes();
	}

	private void RefreshSizes()
	{
		_filenameDrawPosition = new Vector2(base.LetterBoxOffsetX + 40 * base.Scale, base.LetterBoxOffsetY + 30 * base.Scale);
		_messageBoxDimensions = new Rectangle((int)_filenameDrawPosition.X, (int)_filenameDrawPosition.Y + 24 * base.Scale, 312 * base.Scale, 162 * base.Scale);
		int num = 2 * base.Scale;
		_mainMenuBackgroundDrawRectangle = new Rectangle(base.LetterBoxOffsetX, base.LetterBoxOffsetY + num, base.VisibleScreenWidth, base.VisibleScreenHeight - num * 2);
		_buttonDrawPosition = new Vector2(_mainMenuBackgroundDrawRectangle.Right - 16 * base.Scale, _mainMenuBackgroundDrawRectangle.Top + _mainMenuBackgroundDrawRectangle.Height / 2 - 27 * base.Scale);
		_buttonFrameSource = _sprite.GetFrameSource(25);
		base.NewlineIconDrawPosition = new Vector2(_messageBoxDimensions.Right + -21 * base.Scale, _messageBoxDimensions.Bottom + -20 * base.Scale);
	}

	public override void Update(GameTime gameTime, bool doesOtherScreenHasFocus, bool isCoveredByOtherScreen)
	{
		base.Update(gameTime, doesOtherScreenHasFocus, isCoveredByOtherScreen);
		float num = (float)gameTime.ElapsedGameTime.TotalSeconds;
		_flickerTimer += num;
		if (_flickerTimer >= 0.05f)
		{
			_flickerTimer -= 0.05f;
			_isFlickeredOff = !_isFlickeredOff;
			RefreshTextColor();
		}
	}

	private void RefreshTextColor()
	{
		_textColor = TextColor * (_isFlickeredOff ? 0.75f : 0.9f);
		_textShadowColor = ShadowColor * (_isFlickeredOff ? 0.75f : 0.9f);
		_titleColor = TextColor * (_isFlickeredOff ? 0.975f : 1f);
		_titleShadowColor = ShadowColor * (_isFlickeredOff ? 0.975f : 1f);
	}

	internal override void DrawText(SpriteBatch spriteBatch)
	{
		DrawingEx.DrawString(spriteBatch, _font, _filename, new Vector2(_filenameDrawPosition.X, _filenameDrawPosition.Y + (float)base.Scale), _titleShadowColor, Vector2.Zero, base.Scale);
		DrawingEx.DrawString(spriteBatch, _font, _filename, _filenameDrawPosition, _titleColor, Vector2.Zero, base.Scale);
		Vector2 drawPosition = new Vector2(_messageBoxDimensions.X, _messageBoxDimensions.Y);
		int num = base.CurrentLastLetter;
		int num2 = 0;
		while (num > 0 && num2 < base.MaxLines && num2 < base.DialogueLines.Count)
		{
			DialogueLine dialogueLine = base.DialogueLines[num2];
			num -= dialogueLine.Length;
			Color shadyTextColor = ShadyTextColor;
			Color shadyTextShadowColor = ShadyTextShadowColor;
			for (int i = 1; i <= 3; i++)
			{
				dialogueLine.VisibleCharacters = dialogueLine.Length + num + i;
				dialogueLine.Draw(spriteBatch, drawPosition, shadyTextColor, shadyTextShadowColor, base.Scale, 1f);
				shadyTextColor *= 2f / 3f;
				shadyTextShadowColor *= 2f / 3f;
			}
			dialogueLine.VisibleCharacters = dialogueLine.Length + num;
			dialogueLine.Draw(spriteBatch, drawPosition, _textColor, _textShadowColor, base.Scale, 1f);
			drawPosition.Y += base.LineHeight;
			num2++;
		}
		base.DrawText(spriteBatch);
	}

	internal override void DrawBackground(SpriteBatch spriteBatch)
	{
		Color white = Color.White;
		DrawingEx.DrawIrregularBox(flipped: new SpriteEffects[9]
		{
			SpriteEffects.FlipVertically,
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.FlipVertically,
			SpriteEffects.FlipVertically
		}, spriteBatch: spriteBatch, backgroundRectangle: _mainMenuBackgroundDrawRectangle, color: white, sprite: _sprite, zoom: base.Scale, frames: new int[9] { 20, 21, 19, 22, 24, 23, 20, 21, 19 }, shouldTile: false);
		spriteBatch.Draw(_sprite.Texture, _buttonDrawPosition, _buttonFrameSource, white, 0f, Vector2.Zero, base.Scale, SpriteEffects.None, 0f);
	}
}
