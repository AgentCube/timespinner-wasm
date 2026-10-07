using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions;
using Timespinner.GameStateManagement.ScreenManager;

namespace Timespinner.GameStateManagement.Screens.MainMenu;

internal class EmbargoSplashScreen : GameScreen
{
	private const int TotalLogoHalfWidth = 32;

	private const int TotalLogoHalfHeight = 32;

	private const int LogoOffsetY = 32;

	private const int MainTextOffsetY = 32;

	private const int ButtonOffsetY = 16;

	private const float TimeForFadeIn = 0.5f;

	private const float TimeForShowWait = 10f;

	private const float TimeForFadeOut = 0.5f;

	private const float TimeBeforeFadingOut = 10.5f;

	private const float TimeForEntireScreen = 11f;

	private const float CountdownOscillationAmplitude = 0.25f;

	private const float CountdownOscillationFrequency = 5f;

	private const float CountdownBaseMultiplier = 0.75f;

	private static readonly Color BaseFillColor = Color.Black;

	private static readonly Color BaseTextColor = new Color(248, 232, 224);

	private static readonly Color BaseTextShadowColor = new Color(16, 16, 16);

	private bool _hasLoaded;

	private int _scale;

	private int _lastCountdownDigit = -1;

	private int _countdownOffsetX;

	private int _lineHeight;

	private float _screenTimer;

	private string _text;

	private string _sourceCountdownText;

	private string _displayCountdownText;

	private Color _fillColor = Color.Black;

	private Color _drawColor = Color.Transparent;

	private Color _textDrawColor = Color.Transparent;

	private Color _countdownDrawColor = Color.Transparent;

	private Color _textShadowColor = Color.Transparent;

	private Vector2 _screenCenter;

	private Vector2 _finalLogoOffset;

	private Rectangle _spriteDrawSource;

	private UIButton _aButton;

	private SpriteSheet _sprite;

	private SpriteSheet _buttonSprite;

	private SpriteFont _font;

	private ContentManager _content;

	private List<DialogueLine> _lines = new List<DialogueLine>();

	public EmbargoSplashScreen()
	{
		base.TransitionOnTime = TimeSpan.FromSeconds(0.0);
		base.TransitionOffTime = TimeSpan.FromSeconds(1.0);
	}

	public override void LoadContent()
	{
		if (_content == null)
		{
			_content = new ContentManager(base.ScreenManager.Game.Services, "Content");
		}
		GCM gCM = base.ScreenManager.GCM;
		_sprite = gCM.GetTextureAtlas("Overlays/PublisherLogo", _content);
		_spriteDrawSource = _sprite.GetFrameSource(1);
		_buttonSprite = gCM.SpUIButtons;
		_font = gCM.ActiveFont;
		_text = Loc.Get("EmbargoScreenText");
		_sourceCountdownText = Loc.Get("EmbargoScreenCountdown");
		_aButton = new UIButton(Buttons.A);
		RefreshSizes();
		_lines = DialogueLine.SplitMessageIntoLines(_text, 400 * _scale, _font, _buttonSprite, _scale, base.ScreenManager.MenuControllerMapping);
		base.ScreenManager.Jukebox.StopSong();
		_displayCountdownText = string.Format(_sourceCountdownText, _lastCountdownDigit);
		_hasLoaded = true;
	}

	internal override void OnScreenResize()
	{
		base.OnScreenResize();
		RefreshSizes();
	}

	private void RefreshSizes()
	{
		_scale = Constants.InGameZoom;
		_screenCenter = base.ScreenManager.ScreenCenter;
		_lineHeight = _font.LineSpacing * _scale;
		_countdownOffsetX = -(int)(_font.MeasureString(_sourceCountdownText).X * 0.5f);
		_finalLogoOffset = new Vector2(_screenCenter.X - (float)(32 * _scale), _screenCenter.Y - (float)(64 * _scale));
	}

	public override void UnloadContent()
	{
		if (_content != null)
		{
			_content.Unload();
		}
	}

	private void RemoveSelf()
	{
		base.ScreenManager.RemoveScreen(this);
		base.ScreenManager.AddScreen(new TitleBackgroundScreen(shouldDoFullIntro: true), null);
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		if (!base.IsActive)
		{
			return;
		}
		float num = (float)gameTime.ElapsedGameTime.TotalSeconds;
		float screenTimer = _screenTimer;
		_screenTimer += num;
		if (_screenTimer < 11f)
		{
			if (_screenTimer < 0.5f)
			{
				float num2 = _screenTimer / 0.5f;
				_fillColor = BaseFillColor * num2;
				_drawColor = Color.White * num2;
				_textDrawColor = BaseTextColor * num2;
				_countdownDrawColor = _textDrawColor;
				_textShadowColor = BaseTextShadowColor * num2;
			}
			else if (_screenTimer < 10.5f)
			{
				if (screenTimer < 0.5f)
				{
					_drawColor = Color.White;
					_textDrawColor = BaseTextColor;
					_fillColor = BaseFillColor;
					_textShadowColor = BaseTextShadowColor;
				}
				_countdownDrawColor = BaseTextColor * (0.75f + 0.25f * (float)((Math.Sin(_screenTimer * 5f) + 1.0) * 0.5));
			}
			else
			{
				float num3 = 1f - (_screenTimer - 10.5f) / 0.5f;
				_fillColor = BaseFillColor * num3;
				_drawColor = Color.White * num3;
				_textDrawColor = BaseTextColor * num3;
				_countdownDrawColor = _textDrawColor;
				_textShadowColor = BaseTextShadowColor * num3;
			}
			int num4 = (int)(10f - (_screenTimer - 0.5f));
			if (num4 != _lastCountdownDigit)
			{
				_lastCountdownDigit = num4;
				_displayCountdownText = string.Format(_sourceCountdownText, _lastCountdownDigit);
			}
		}
		else
		{
			_fillColor = Color.Black;
			RemoveSelf();
		}
	}

	public override void HandleInput(InputState input)
	{
		if (_screenTimer > 0.5f && input.IsNewPressConfirmCancel(base.ControllingPlayer))
		{
			DecrementCountdown();
		}
	}

	private void DecrementCountdown()
	{
		_screenTimer += 1f;
	}

	public override void Draw(GameTime gameTime)
	{
		base.ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, _fillColor, 0f, 0);
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
		DrawLogo(spriteBatch);
		spriteBatch.End();
		if (base.TransitionOffPercentage > 0f)
		{
			base.ScreenManager.FadeBackBufferToBlack(255 - base.TransitionAlpha);
		}
	}

	private void DrawLogo(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(_sprite.Texture, _finalLogoOffset, _spriteDrawSource, _drawColor, 0f, Vector2.Zero, _scale, SpriteEffects.None, 0f);
		if (!_hasLoaded)
		{
			return;
		}
		float num = _finalLogoOffset.Y + (float)(32 * _scale);
		foreach (DialogueLine line in _lines)
		{
			line.Draw(spriteBatch, new Vector2(_screenCenter.X - (float)line.Width * 0.5f * (float)_scale, num), _textDrawColor, _textShadowColor, _scale, 1f);
			num += (float)_lineHeight;
		}
		num += (float)_lineHeight;
		Vector2 drawPos = new Vector2(_screenCenter.X + (float)(_countdownOffsetX * _scale), num);
		Vector2 position = new Vector2(_screenCenter.X, drawPos.Y + (float)(16 * _scale));
		DrawingEx.DrawString(spriteBatch, _font, _displayCountdownText, drawPos, _countdownDrawColor, Vector2.Zero, _scale);
		_aButton.Draw(spriteBatch, _buttonSprite, _font, position, _drawColor, _scale, -1);
	}
}
