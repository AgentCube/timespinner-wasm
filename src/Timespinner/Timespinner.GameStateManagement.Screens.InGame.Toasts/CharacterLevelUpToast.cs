using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.GameAbstractions;

namespace Timespinner.GameStateManagement.Screens.InGame.Toasts;

internal class CharacterLevelUpToast : BaseToastPopup
{
	private const int TextIndexStart = 0;

	private const int TextAnimationLength = 6;

	private const int FrameIndexStart = 7;

	private const int DisplayOffsetY = -16;

	private const float TimeForRingToFlash = 0.3f;

	private const float AnimationSpeed = 0.07f;

	private const float TimeBeforeGivingBackControl = 0.75f;

	private static readonly Vector2 BottomFrameOffset = new Vector2(-48f, -2f);

	private static readonly Vector2 SmallTextDrawOrigin = new Vector2(7f, 10f);

	private static readonly Vector2 LargeTextDrawOrigin = new Vector2(42f, 10f);

	private static readonly Color BaseRingColor = new Color(1f, 0.9f, 0.6f, 0.5f);

	private readonly Texture2D _ringTexture;

	private readonly LevelUpSparkleParticleSystem _sparkleParticleSystem;

	private bool _isAnimating;

	private int _animationIndex;

	private float _animationTimer;

	private float _controlTimer;

	private float _ringPercentage;

	private float _baseZoom;

	private Vector2 _drawPosition;

	private Vector2 _textDrawOrigin;

	public CharacterLevelUpToast(SpriteSheet sprite, GCM gcm)
		: base(sprite, doesFreezeGameplay: false, gcm)
	{
		_textDrawOrigin = SmallTextDrawOrigin;
		_ringTexture = gcm.TxLargeRing;
		_isAnimating = true;
		base.IsOverlayScreen = false;
		base.IsPopupScreen = true;
		_sparkleParticleSystem = new LevelUpSparkleParticleSystem(sprite, 1, Color.White);
	}

	public override void LoadContent()
	{
		base.LoadContent();
		RefreshSizes();
		base.ScreenManager.Jukebox.PlayCue(ESFX.CharacterLevelUp);
	}

	internal override void OnScreenResize()
	{
		base.OnScreenResize();
		RefreshSizes();
	}

	private void RefreshSizes()
	{
		Rectangle rectangle = Rectangle.Empty;
		if (base.ScreenManager != null)
		{
			rectangle = base.ScreenManager.TitleSafeArea;
		}
		_baseZoom = Constants.InGameZoom;
		Vector2 drawPosition = new Vector2((float)rectangle.Center.X / _baseZoom, (float)rectangle.Center.Y / _baseZoom + -16f);
		_drawPosition = drawPosition;
	}

	public override void Update(GameTime gameTime, bool doesOtherScreenHasFocus, bool isCoveredByOtherScreen)
	{
		float num = (float)gameTime.ElapsedGameTime.TotalSeconds;
		if (!base.IsOverlayScreen)
		{
			_controlTimer += num;
			if (_controlTimer > 0.75f)
			{
				base.IsOverlayScreen = true;
			}
		}
		if (_isAnimating)
		{
			_animationTimer += num;
			if (_animationTimer >= 0.07f)
			{
				_animationTimer -= 0.07f;
				_animationIndex++;
				_textDrawOrigin = ((_animationIndex < 3) ? SmallTextDrawOrigin : LargeTextDrawOrigin);
				if (_animationIndex == 2)
				{
					Vector2 where = new Vector2(_drawPosition.X, _drawPosition.Y + 4f) * _baseZoom;
					_sparkleParticleSystem.AddParticles(where);
				}
				if (_animationIndex >= 6)
				{
					_animationIndex = 6;
					_isAnimating = false;
				}
			}
		}
		if (_controlTimer < 0.3f)
		{
			_ringPercentage = (float)Math.Sin((float)Math.PI / 2f * _controlTimer / 0.3f);
		}
		else
		{
			_ringPercentage = 0f;
		}
		_sparkleParticleSystem.Update(num);
		base.Update(gameTime, doesOtherScreenHasFocus, isCoveredByOtherScreen);
	}

	public override void Draw(GameTime gameTime)
	{
		Color color = Color.White * 0.175f * base.DrawColorPercentage;
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
		int inGameZoom = Constants.InGameZoom;
		Rectangle frameSource = base.Sprite.GetFrameSource(8);
		Vector2 a = _drawPosition * inGameZoom;
		a = a.Add(new Point(0, 6 * inGameZoom));
		spriteBatch.Draw(base.Sprite.Texture, a, frameSource, color, 0f, new Vector2(48f, 20f), inGameZoom, SpriteEffects.None, 0f);
		spriteBatch.Draw(base.Sprite.Texture, a, frameSource, color, 0f, new Vector2(0f, 20f), inGameZoom, SpriteEffects.FlipHorizontally, 0f);
		spriteBatch.End();
		base.Draw(gameTime);
	}

	internal override void DrawToastContent(SpriteBatch spriteBatch, Color drawColor, float zoom)
	{
		Vector2 vector = _drawPosition * zoom;
		if (_ringPercentage > 0f && _ringPercentage < 1f)
		{
			Color color = BaseRingColor * (1f - _ringPercentage);
			float scale = _ringPercentage * zoom;
			spriteBatch.Draw(position: new Vector2(vector.X, vector.Y + 4f * zoom), texture: _ringTexture, sourceRectangle: null, color: color, rotation: 0f, origin: new Vector2(64f, 64f), scale: scale, effects: SpriteEffects.None, layerDepth: 0f);
		}
		Rectangle frameSource = base.Sprite.GetFrameSource(7);
		Vector2 position2 = Vector2.Add(vector, BottomFrameOffset * zoom);
		spriteBatch.Draw(base.Sprite.Texture, position2, frameSource, drawColor, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
		position2.X += (0f - BottomFrameOffset.X) * zoom;
		spriteBatch.Draw(base.Sprite.Texture, position2, frameSource, drawColor, 0f, Vector2.Zero, zoom, SpriteEffects.FlipHorizontally, 0f);
		_sparkleParticleSystem.Draw(spriteBatch, Vector2.Zero, Vector2.Zero, 1f);
		frameSource = base.Sprite.GetFrameSource(_animationIndex);
		position2 = vector;
		spriteBatch.Draw(base.Sprite.Texture, position2, frameSource, drawColor, 0f, _textDrawOrigin, zoom, SpriteEffects.None, 0f);
	}
}
