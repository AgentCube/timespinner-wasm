using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.Core.Localization;
using Timespinner.GameStateManagement.ScreenManager;

namespace Timespinner.GameStateManagement.Screens.BaseClasses;

internal class LoadingScreen : GameScreen
{
	private const int Anim_LoadingBarFiligreeStart = 0;

	private const int Anim_LoadingBarUnderVialStart = 1;

	private const int Anim_LoadingBarLoadedVialStart = 2;

	private const int Anim_LoadingBarLoadedVialEndStart = 3;

	private const int Anim_LoadingBarLoadedVialTipStart = 4;

	private const int Anim_MeyefStart = 5;

	private const int Anim_ZzzStart = 6;

	private const int Anim_ZzzLength = 5;

	private const float TimeForZzzLife = 1f;

	private const float Anim_ZzzSpeed = 0.15f;

	private const float ZzzRiseWidth = -4f;

	private const float ZzzRiseHeight = -20f;

	private const int DisplayTextOffsetY = -26;

	private const int MeyefOffsetX = -11;

	private const int MeyefOffsetY = -53;

	private const int ZzzBaseOffsetX = 0;

	private const int ZzzBaseOffsetY = 6;

	private const int LoadingBarFiligreeOffsetY = -59;

	private const int LoadingBarVialTopOffsetY = -46;

	private const int LoadingBarUnderVialTopOffsetY = -44;

	private const float UpdateDelta = 1f / 60f;

	private const float TimeToFadeOut = 0.5f;

	private const int LoadingVialWidth = 150;

	private const int LoadingVileEndWidth = 8;

	private const int UnderVialHeight = 12;

	private static readonly Color BaseLoadingTextColor = new Color(255, 250, 245);

	private readonly bool _isLoadingSlow;

	private readonly bool _isDrawingBorderFrame;

	private readonly Rectangle _meyefFrameSource;

	private readonly Rectangle _loadingBarFiligreeSource;

	private readonly Rectangle _loadingBarUnderVialSource;

	private readonly Rectangle _loadingBarLoadedVialSource;

	private readonly Rectangle _loadingBarLoadedVialEndSource;

	private readonly Rectangle _loadingBarLoadedVialTipSource;

	private readonly SpriteSheet _sprite;

	private readonly GameScreen[] _screensToLoad;

	private bool _areOtherScreensGone;

	private bool _hasInitialized;

	private bool _hasPreloaded;

	private bool _isFinishedLoading;

	private int _zoom;

	private int _zzzOffsetX;

	private int _zzzOffsetY;

	private int _zzzAnimationIndex;

	private int _vialLeft;

	private int _vialTop;

	private int _loadedVialWidth;

	private int _drawnLoadedVialWidth;

	private float _loadPercentage;

	private float _zzzAnimationTimer;

	private float _zzzLifeTimer;

	private float _fadeOutTimer;

	private float _fadePercentage;

	private string _message;

	private Vector2 _textPosition;

	private Vector2 _meyefDrawPos;

	private Vector2 _leftLoadingBarFiligreeDrawPos;

	private Vector2 _rightLoadingBarFiligreeDrawPos;

	private Vector2 _loadingTextOrigin;

	private Vector2 _textSize;

	private Color _loadingTextDrawColor;

	private Rectangle _zzzFrameSource;

	private Rectangle _underVialDrawRectangle;

	private GraphicsDevice _graphicsDevice;

	internal LoadingScreen(Timespinner.GameStateManagement.ScreenManager.ScreenManager screenManager, bool isLoadingSlow, GameScreen[] screensToLoad)
	{
		_isLoadingSlow = isLoadingSlow;
		_screensToLoad = screensToLoad;
		_sprite = screenManager.GCM.SpLoadingScreen;
		_meyefFrameSource = _sprite.GetFrameSource(5);
		_loadingBarFiligreeSource = _sprite.GetFrameSource(0);
		_loadingBarUnderVialSource = _sprite.GetFrameSource(1);
		_loadingBarLoadedVialSource = _sprite.GetFrameSource(2);
		_loadingBarLoadedVialEndSource = _sprite.GetFrameSource(3);
		_loadingBarLoadedVialTipSource = _sprite.GetFrameSource(4);
		base.TransitionOnTime = TimeSpan.FromSeconds(0.5);
		_graphicsDevice = screenManager.GraphicsDevice;
		_isDrawingBorderFrame = screenManager.SaveFileManager.ConfigSave.DoesDrawBorderFrame;
	}

	public static void Load(Timespinner.GameStateManagement.ScreenManager.ScreenManager screenManager, bool isLoadingSlow, PlayerIndex? controllingPlayer, params GameScreen[] screensToLoad)
	{
		GameScreen[] screens = screenManager.GetScreens();
		foreach (GameScreen gameScreen in screens)
		{
			gameScreen.ExitScreen();
		}
		LoadingScreen screen = new LoadingScreen(screenManager, isLoadingSlow, screensToLoad);
		screenManager.AddScreen(screen, controllingPlayer);
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		float num = (float)gameTime.ElapsedGameTime.TotalSeconds;
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		if (_areOtherScreensGone && !_hasPreloaded)
		{
			GameScreen[] screensToLoad = _screensToLoad;
			foreach (GameScreen gameScreen in screensToLoad)
			{
				if (gameScreen != null)
				{
					base.ScreenManager.PreloadScreen(gameScreen, base.ControllingPlayer);
				}
			}
			_hasPreloaded = true;
		}
		if (_hasPreloaded && !_isFinishedLoading)
		{
			bool flag = true;
			float num2 = 1f;
			GameScreen[] screensToLoad2 = _screensToLoad;
			foreach (GameScreen gameScreen2 in screensToLoad2)
			{
				float num3 = gameScreen2.SlowLoad();
				if (num3 >= 0f)
				{
					flag = false;
					if (num3 <= num2)
					{
						_loadPercentage = num3;
						num2 = num3;
					}
				}
			}
			_loadedVialWidth = (int)Math.Ceiling(_loadPercentage * 150f);
			if (flag)
			{
				_isFinishedLoading = true;
				_loadPercentage = 1f;
				num = 0f;
				base.ScreenManager.Game.ResetElapsedTime();
			}
			else
			{
				_graphicsDevice.Clear(Color.Black);
				base.ScreenManager.Draw(new GameTime());
				_graphicsDevice.Present();
			}
		}
		if (!_isFinishedLoading)
		{
			return;
		}
		_fadeOutTimer += num;
		if (_fadeOutTimer < 0.5f && _isLoadingSlow)
		{
			_fadePercentage = 1f - (float)Math.Cos((float)Math.PI / 2f * _fadeOutTimer / 0.5f);
			return;
		}
		base.ScreenManager.RemoveScreen(this);
		GameScreen[] screensToLoad3 = _screensToLoad;
		foreach (GameScreen screen in screensToLoad3)
		{
			base.ScreenManager.AddPreloadedScreen(screen);
		}
	}

	internal override void OnScreenResize()
	{
		base.OnScreenResize();
		RefreshSizes();
	}

	private void RefreshSizes()
	{
		_zoom = Constants.InGameZoom;
		Rectangle titleSafeArea = base.ScreenManager.TitleSafeArea;
		int x = titleSafeArea.Center.X;
		_loadingTextOrigin = new Vector2((int)(_textSize.X * 0.5f), 0f);
		int num = titleSafeArea.Bottom + -59 * _zoom;
		int num2 = _loadingBarFiligreeSource.Width * _zoom;
		_leftLoadingBarFiligreeDrawPos = new Vector2(x - num2, num);
		_rightLoadingBarFiligreeDrawPos = new Vector2(x, num);
		_vialLeft = x + -75 * _zoom;
		_vialTop = titleSafeArea.Bottom + -46 * _zoom;
		_underVialDrawRectangle = new Rectangle(_vialLeft, titleSafeArea.Bottom + -44 * _zoom, 150 * _zoom, 12 * _zoom);
		_textPosition = new Vector2(x, titleSafeArea.Bottom + -26 * _zoom);
		_meyefDrawPos = new Vector2(_textPosition.X + (float)(-11 * _zoom), _textPosition.Y + (-53f + _textSize.Y) * (float)_zoom);
	}

	public override void Draw(GameTime gameTime)
	{
		DoDraw(doesClearAndPresent: false);
	}

	private void DoDraw(bool doesClearAndPresent)
	{
		if (doesClearAndPresent)
		{
			_graphicsDevice.Clear(Color.Black);
		}
		if (base.ScreenState == EScreenState.Active && base.ScreenManager.GetScreens().Length == 1)
		{
			_areOtherScreensGone = true;
		}
		if (_isLoadingSlow)
		{
			SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
			SpriteFont menuFont = base.ScreenManager.MenuFont;
			if (!_hasInitialized)
			{
				_hasInitialized = true;
				_message = Loc.Get("Loading");
				_textSize = menuFont.MeasureString(_message);
				RefreshSizes();
			}
			UpdateZzz(1f / 60f);
			Vector2 position = new Vector2(_meyefDrawPos.X + (float)(_zzzOffsetX * _zoom), _meyefDrawPos.Y + (float)((6 + _zzzOffsetY) * _zoom));
			float num = 1f - (_isFinishedLoading ? _fadePercentage : base.TransitionOffPercentage);
			Color color = Color.White * num;
			Texture2D texture = _sprite.Texture;
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
			spriteBatch.Draw(texture, _underVialDrawRectangle, _loadingBarUnderVialSource, color, 0f, Vector2.Zero, SpriteEffects.None, 0f);
			DrawLoadedVial(spriteBatch, texture, color);
			spriteBatch.Draw(texture, _leftLoadingBarFiligreeDrawPos, _loadingBarFiligreeSource, color, 0f, Vector2.Zero, _zoom, SpriteEffects.FlipHorizontally, 0f);
			spriteBatch.Draw(texture, _rightLoadingBarFiligreeDrawPos, _loadingBarFiligreeSource, color, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
			DrawingEx.DrawString(spriteBatch, menuFont, _message, _textPosition, _loadingTextDrawColor * num, _loadingTextOrigin, _zoom);
			spriteBatch.Draw(texture, _meyefDrawPos, _meyefFrameSource, color, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
			spriteBatch.Draw(texture, position, _zzzFrameSource, color, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
			if (doesClearAndPresent && _isDrawingBorderFrame)
			{
				base.ScreenManager.GCM.DrawScreenBorderFrame(spriteBatch, base.ScreenManager.SmallScreenRect, base.ScreenManager.ScreenSize);
			}
			spriteBatch.End();
		}
		if (doesClearAndPresent)
		{
			_graphicsDevice.Present();
		}
	}

	private void UpdateZzz(float delta)
	{
		_zzzLifeTimer += delta;
		if (_zzzLifeTimer >= 1f)
		{
			_zzzAnimationIndex = 0;
			_zzzAnimationTimer = 0f;
			_zzzLifeTimer = 0f;
			_zzzOffsetX = 0;
			_zzzOffsetY = 0;
		}
		else
		{
			float num = _zzzLifeTimer / 1f;
			_zzzOffsetX = (int)Math.Round(num * -4f);
			_zzzOffsetY = (int)Math.Round(num * -20f);
			float num2 = (float)Math.Sin(num * (float)Math.PI);
			float num3 = num2 / 2f + 0.5f;
			_loadingTextDrawColor = BaseLoadingTextColor * num3;
		}
		if (_zzzAnimationIndex < 4)
		{
			_zzzAnimationTimer -= delta;
			if (_zzzAnimationTimer <= 0f)
			{
				_zzzAnimationTimer += 0.15f;
				_zzzFrameSource = _sprite.GetFrameSource(6 + _zzzAnimationIndex);
				_zzzAnimationIndex++;
			}
		}
	}

	private void DrawLoadedVial(SpriteBatch spriteBatch, Texture2D texture, Color color)
	{
		int num = _loadedVialWidth - _drawnLoadedVialWidth;
		int num2 = (_drawnLoadedVialWidth = ((num >= 1) ? (_drawnLoadedVialWidth + (int)Math.Ceiling((float)num * 0.2f)) : _loadedVialWidth));
		if (num2 <= 0)
		{
			return;
		}
		int num3 = num2;
		int val = num3 - 142;
		int num4 = Math.Max(0, -142);
		int num5 = Math.Min(num2, 8);
		int num6 = MathEx.Clamp(Math.Min(num2, val), 0, 8);
		int num7 = num2 - ((num5 > 0) ? num5 : 0) - num4 - num6;
		Vector2 vector = new Vector2(_vialLeft, _vialTop);
		Vector2 position = vector;
		Rectangle value = _loadingBarLoadedVialEndSource;
		if (num5 != 8)
		{
			value = new Rectangle(value.X, value.Y, num5, value.Height);
		}
		spriteBatch.Draw(texture, position, value, color, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
		if (num7 > 0)
		{
			Point point = new Point((int)vector.X + Math.Max(8, 0) * _zoom, (int)vector.Y);
			value = _loadingBarLoadedVialSource;
			Rectangle destinationRectangle = new Rectangle(point.X, point.Y, num7 * _zoom, value.Height * _zoom);
			spriteBatch.Draw(texture, destinationRectangle, value, color);
		}
		if (num6 > 0)
		{
			Vector2 position2 = new Vector2(vector.X + (float)((142 + num4) * _zoom), vector.Y);
			value = _loadingBarLoadedVialEndSource;
			if (num6 != 8)
			{
				value = new Rectangle(value.X + num4, value.Y, num6, value.Height);
			}
			spriteBatch.Draw(texture, position2, value, color, 0f, Vector2.Zero, _zoom, SpriteEffects.FlipHorizontally, 0f);
		}
		else
		{
			Vector2 position3 = new Vector2((int)vector.X + num2 * _zoom, (int)vector.Y);
			value = _loadingBarLoadedVialTipSource;
			spriteBatch.Draw(texture, position3, value, color, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
		}
	}
}
