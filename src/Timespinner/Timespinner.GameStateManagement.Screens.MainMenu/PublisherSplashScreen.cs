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

internal class PublisherSplashScreen : GameScreen
{
	private const float TimeForShowWait = 1f;

	private const int TotalLogoHalfWidth = 32;

	private const int TotalLogoHalfHeight = 32;

	private const float TimeForFadeIn = 0.5f;

	private const float TimeForFadeOut = 0.5f;

	private const float TimeBeforeFadingOut = 1.5f;

	private const float TimeForEntireScreen = 2f;

	private static readonly Color BaseFillColor = Color.Black;

	private bool _hasPlayedJingle;

	private bool _isRemovingSelf;

	private int _scale;

	private float _screenTimer;

	private Color _fillColor = Color.Black;

	private Color _drawColor = Color.Transparent;

	private Vector2 _screenCenter;

	private Vector2 _finalLogoOffset;

	private SpriteSheet _sprite;

	private SoundEffect _jingleSoundEffect;

	private ContentManager _content;

	public PublisherSplashScreen()
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
		_jingleSoundEffect = _content.Load<SoundEffect>("Audio/SFX/Other/sfx_publisher_jingle");
		GCM gCM = base.ScreenManager.GCM;
		_sprite = gCM.GetTextureAtlas("Overlays/PublisherLogo", _content);
		RefreshZoom();
		base.ScreenManager.Jukebox.StopSong();
	}

	private void RefreshZoom()
	{
		_scale = Constants.InGameZoom;
		_screenCenter = base.ScreenManager.ScreenCenter;
		_finalLogoOffset = new Vector2(_screenCenter.X - (float)(32 * _scale), _screenCenter.Y - (float)(32 * _scale));
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
			base.ScreenManager.AddScreen(new LunarLogoSplashScreen(), null);
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
		float screenTimer = _screenTimer;
		_screenTimer += num;
		if (!_hasPlayedJingle)
		{
			_hasPlayedJingle = true;
			_jingleSoundEffect.Play();
		}
		if (_screenTimer < 2f)
		{
			if (_screenTimer < 0.5f)
			{
				float num2 = _screenTimer / 0.5f;
				_fillColor = BaseFillColor * num2;
				_drawColor = Color.White * num2;
			}
			else if (_screenTimer < 1.5f)
			{
				if (screenTimer < 0.5f)
				{
					_drawColor = Color.White;
					_fillColor = BaseFillColor;
				}
			}
			else
			{
				float num3 = 1f - (_screenTimer - 1.5f) / 0.5f;
				_fillColor = BaseFillColor * num3;
				_drawColor = Color.White * num3;
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
		Rectangle frameSource = _sprite.GetFrameSource(0);
		spriteBatch.Draw(_sprite.Texture, _finalLogoOffset, frameSource, _drawColor, 0f, Vector2.Zero, _scale, SpriteEffects.None, 0f);
	}
}
