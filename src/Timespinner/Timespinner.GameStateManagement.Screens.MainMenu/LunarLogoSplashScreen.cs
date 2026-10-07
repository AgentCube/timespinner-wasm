using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.GameAbstractions;
using Timespinner.GameStateManagement.ScreenManager;

namespace Timespinner.GameStateManagement.Screens.MainMenu;

internal class LunarLogoSplashScreen : GameScreen
{
	private const int FrameLogoHeight = 40;

	private const int TotalLogoWidth = 273;

	private const float MoonRotationMultiplier = 2f;

	private const float MoonRotationOffset = 4.712389f;

	private const float BaseShineAmount = 7.5f;

	private const float MoonRayAnimationSpeed = 0.067f;

	private const float TimeForMoonToSpin = 0.5f;

	private const float TimeForMoonRaysToShow = 0.201f;

	private const float TimeForTextToShow = 0.5f;

	private const float TimeToFadeOut = 0.5f;

	private const float TimeForMoonAndRaysToShow = 0.701f;

	private const float TimeForMoonRaysAndTextToShow = 1.201f;

	private const float TimeBeforeFadingOut = 2.5f;

	private const float TimeForEntireScreen = 3f;

	private static readonly Vector2 MoonDrawOrigin = new Vector2(18f, 18f);

	private bool _isMoonVisible;

	private bool _isMoonRayVisible;

	private bool _isFrameAndTextVisible;

	private bool _hasPlayedCue;

	private bool _isRemovingSelf;

	private int _moonRayAnimationIndex;

	private int _zoom;

	private float _shineAmount;

	private float _screenShowTimer;

	private float _moonRotation;

	private float _moonRayAnimationTimer;

	private Point _mainFrameOffset;

	private Point _textOffset;

	private Point _moonFrameOffset;

	private Point _moonOffset;

	private Point _moonRayOffset;

	private Vector2 _baseFrameLocation;

	private Vector2 _screenCenter;

	private Vector2 _finalLogoOffset;

	private Color _frameDrawColor = Color.White;

	private Color _moonDrawColor = Color.White;

	private ContentManager _content;

	private Effect _efBrighten;

	private SpriteSheet _lunarRayGamesLogo;

	private SoundEffect _lunarRaySplashSound;

	public LunarLogoSplashScreen()
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
		_efBrighten = _content.Load<Effect>("Effects/Brighten");
		_lunarRaySplashSound = _content.Load<SoundEffect>("Audio/SFX/Other/sfx_lunar_ray_logo");
		GCM gCM = base.ScreenManager.GCM;
		_lunarRayGamesLogo = gCM.GetTextureAtlas("Overlays/LunarRayGamesLogo", _content);
		RefreshZoom();
		base.ScreenManager.Jukebox.StopSong();
	}

	private void RefreshZoom()
	{
		_zoom = Constants.InGameZoom;
		_screenCenter = base.ScreenManager.ScreenCenter;
		_finalLogoOffset = new Vector2(_screenCenter.X - (float)(_zoom * 273) / 2f, _screenCenter.Y - (float)(40 * _zoom));
		_textOffset = new Point(35, 4).Multiply(_zoom);
		_moonFrameOffset = new Point(1, -1).Multiply(_zoom);
		_moonOffset = new Point(1, -1).Add(MoonDrawOrigin).Multiply(_zoom);
		_moonRayOffset = new Point(1, -1).Multiply(_zoom);
		_mainFrameOffset = new Point(39, 0).Multiply(_zoom);
	}

	internal override void OnScreenResize()
	{
		base.OnScreenResize();
		RefreshZoom();
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
		if (!_isRemovingSelf)
		{
			_isRemovingSelf = true;
			base.ScreenManager.RemoveScreen(this);
			base.ScreenManager.AddScreen(new TitleBackgroundScreen(shouldDoFullIntro: true), null);
		}
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		if (!base.IsActive)
		{
			return;
		}
		float num = (float)gameTime.ElapsedGameTime.TotalSeconds;
		if (!_hasPlayedCue)
		{
			_lunarRaySplashSound.Play();
			_hasPlayedCue = true;
		}
		_screenShowTimer += num;
		if (_screenShowTimer >= 2.5f)
		{
			if (_screenShowTimer <= 3f)
			{
				float num2 = (_screenShowTimer - 2.5f) / 0.5f;
				float num3 = (float)Math.Cos(num2 * ((float)Math.PI / 2f));
				_moonDrawColor = Color.White * num3;
				_frameDrawColor = _moonDrawColor;
			}
			else
			{
				RemoveSelf();
			}
		}
		else
		{
			if (!(_screenShowTimer >= 0f))
			{
				return;
			}
			if (_screenShowTimer <= 0.5f)
			{
				_isMoonVisible = true;
				float num4 = _screenShowTimer / 0.5f;
				_moonRotation = 0f - (float)Math.Sin(num4) * ((float)Math.PI * 2f) * 2f + 4.712389f;
				float amount = 1f - (float)Math.Cos(num4 * ((float)Math.PI / 2f));
				_baseFrameLocation = new Vector2(MathHelper.Lerp(_screenCenter.X * 2f + (float)(40 * _zoom), _finalLogoOffset.X, amount), _finalLogoOffset.Y);
			}
			else if (_screenShowTimer <= 0.701f)
			{
				_isMoonRayVisible = true;
				_moonRotation = 0f;
				_baseFrameLocation = _finalLogoOffset;
				_moonRayAnimationTimer += num;
				if (_moonRayAnimationTimer >= 0.067f)
				{
					_moonRayAnimationTimer -= 0.067f;
					_moonRayAnimationIndex++;
				}
			}
			else if (_screenShowTimer <= 1.201f)
			{
				_isFrameAndTextVisible = true;
				float num5 = (_screenShowTimer - 0.701f) / 0.5f;
				_baseFrameLocation = _finalLogoOffset;
				float num6 = (float)Math.Cos(num5 * ((float)Math.PI / 2f));
				_shineAmount = num6 * 7.5f;
				_frameDrawColor.A = (byte)((1f - num6) * 255f);
				_moonDrawColor.A = _frameDrawColor.A;
			}
			else
			{
				_baseFrameLocation = _finalLogoOffset;
			}
		}
	}

	public override void HandleInput(InputState input)
	{
		if (input.IsNewButtonPress(Buttons.Start, null, out var _))
		{
			RemoveSelf();
		}
		else
		{
			base.HandleInput(input);
		}
	}

	public override void Draw(GameTime gameTime)
	{
		base.ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 0f, 0);
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		_efBrighten.Parameters["shinyAmount"].SetValue(_shineAmount);
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, _efBrighten);
		DrawLogoAnimation(spriteBatch);
		spriteBatch.End();
		if (base.TransitionOffPercentage > 0f)
		{
			base.ScreenManager.FadeBackBufferToBlack(255 - base.TransitionAlpha);
		}
	}

	private void DrawLogoAnimation(SpriteBatch spriteBatch)
	{
		Vector2 zero = Vector2.Zero;
		if (_isFrameAndTextVisible)
		{
			Rectangle frameSource = _lunarRayGamesLogo.GetFrameSource(7);
			spriteBatch.Draw(_lunarRayGamesLogo.Texture, _baseFrameLocation.Add(_mainFrameOffset), frameSource, _frameDrawColor, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
			frameSource = _lunarRayGamesLogo.GetFrameSource(6);
			spriteBatch.Draw(_lunarRayGamesLogo.Texture, _baseFrameLocation.Add(_textOffset), frameSource, _frameDrawColor, 0f, zero, _zoom, SpriteEffects.None, 0f);
			frameSource = _lunarRayGamesLogo.GetFrameSource(0);
			spriteBatch.Draw(_lunarRayGamesLogo.Texture, _baseFrameLocation.Add(_moonFrameOffset), frameSource, _frameDrawColor, 0f, zero, _zoom, SpriteEffects.None, 0f);
		}
		if (_isMoonVisible)
		{
			Rectangle frameSource = _lunarRayGamesLogo.GetFrameSource(1);
			spriteBatch.Draw(_lunarRayGamesLogo.Texture, _baseFrameLocation.Add(_moonOffset), frameSource, _moonDrawColor, _moonRotation, MoonDrawOrigin, _zoom, SpriteEffects.None, 0f);
		}
		if (_isMoonRayVisible)
		{
			Rectangle frameSource = _lunarRayGamesLogo.GetFrameSource(2 + _moonRayAnimationIndex);
			spriteBatch.Draw(_lunarRayGamesLogo.Texture, _baseFrameLocation.Add(_moonRayOffset), frameSource, _moonDrawColor, 0f, zero, _zoom, SpriteEffects.None, 0f);
		}
	}
}
