using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameStateManagement.Screens.InGame.Toasts;

internal class AreaTitleToast : BaseToastPopup
{
	private enum ETitleSequenceState
	{
		Revealing,
		Flashing,
		Lingering,
		Fading,
		Finished
	}

	private enum ETitleSequenceRevealType
	{
		Sideswipe,
		FadeIn
	}

	private const int SparkleOscillationHeight = 12;

	private const int IndexBlackBackingLeft = 15;

	private const int IndexBlackBacking = 21;

	private const int IndexGoldFrameLeft = 16;

	private const int IndexGoldFrameMid = 17;

	private const int IndexGoldFrameRight = 18;

	private const int IndexDatePast = 19;

	private const int IndexDatePresent = 20;

	private const int IndexDateMystery = 22;

	private const int EntireDrawOffsetY = -48;

	private const int BlackBackingOffsetY = -14;

	private const int GoldFrameLengthMargin = -16;

	private const int GoldFrameOffsetX = 0;

	private const int GoldFrameOffsetY = -5;

	private const int TextOffsetX = 32;

	private const int LatinTextOffsetY = -13;

	private const int AsianTextOffsetY = -18;

	private const int EraOffsetX = -45;

	private const int EraOffsetY = 2;

	private const float TimeToReveal = 0.4f;

	private const float TimeToFlash = 0.5f;

	private const float TimeToLinger = 1f;

	private const float TimeToFade = 0.75f;

	private const float TimeBeforeLingering = 0.9f;

	private const float TimeBeforeFading = 1.9f;

	private const float TimeForTotalSequence = 2.65f;

	private const float TimeBeforeGivingBackControl = 0f;

	private readonly ETitleSequenceRevealType _revealType;

	private readonly int _areaID;

	private readonly int _indexAreaTitle;

	private readonly int _entireRenderWidth;

	private readonly int _goldFrameMidWidth;

	private readonly int _blackBackingMidWidth;

	private readonly int _textOffsetY;

	private readonly Rectangle _areaTitleFrameSource;

	private readonly Rectangle _dateTextFrameSource;

	private readonly Rectangle _blackBackingFrameSource;

	private readonly Rectangle _blackBackingLeftFrameSource;

	private readonly Rectangle _goldFrameLeftFrameSource;

	private readonly Rectangle _goldFrameMidFrameSource;

	private readonly Rectangle _goldFrameRightFrameSource;

	private readonly AreaTitleSparkleParticleSystem _sparkleParticleSystem;

	private ETitleSequenceState _sequenceState;

	private int _revealRightZoomed;

	private int _revealWidth;

	private int _revealLeft;

	private int _zoom;

	private float _controlTimer;

	private float _sequenceTimer;

	private float _sequencePercentage;

	private Vector2 _drawPosition;

	private Vector2 _zoomedDrawPosition;

	private Rectangle _titleSafeArea;

	public AreaTitleToast(SpriteSheet sprite, GCM gcm, int areaID, Rectangle titleSafe, float zoom)
		: base(sprite, doesFreezeGameplay: false, gcm, 0.4f, 0.5f, 1f, 0.75f)
	{
		_zoom = (int)zoom;
		_titleSafeArea = titleSafe;
		_areaID = areaID;
		_revealType = ETitleSequenceRevealType.Sideswipe;
		if (_areaID == 0)
		{
			_revealType = ETitleSequenceRevealType.FadeIn;
			_indexAreaTitle = 23;
		}
		else if (_areaID < 13)
		{
			_indexAreaTitle = _areaID - 1;
		}
		else if (_areaID >= 14)
		{
			switch (_areaID)
			{
			case 14:
				_indexAreaTitle = 14;
				break;
			case 15:
				_indexAreaTitle = 12;
				break;
			case 16:
				_indexAreaTitle = 13;
				break;
			}
		}
		int index = Level.GetEraByLevelID(_areaID) switch
		{
			EEraType.Past => 19, 
			EEraType.Present => 20, 
			_ => 22, 
		};
		_areaTitleFrameSource = sprite.GetFrameSource(_indexAreaTitle);
		_dateTextFrameSource = sprite.GetFrameSource(index);
		_blackBackingFrameSource = sprite.GetFrameSource(21);
		_blackBackingLeftFrameSource = sprite.GetFrameSource(15);
		_goldFrameLeftFrameSource = sprite.GetFrameSource(16);
		_goldFrameMidFrameSource = sprite.GetFrameSource(17);
		_goldFrameRightFrameSource = sprite.GetFrameSource(18);
		_entireRenderWidth = _goldFrameRightFrameSource.Width + _areaTitleFrameSource.Width + _goldFrameLeftFrameSource.Width + -16;
		_goldFrameMidWidth = _entireRenderWidth - _goldFrameLeftFrameSource.Width - _goldFrameRightFrameSource.Width;
		_blackBackingMidWidth = _entireRenderWidth - 2 * _blackBackingLeftFrameSource.Width;
		_textOffsetY = (Loc.IsAsianLocale ? (-18) : (-13));
		RefreshZoom();
		base.IsOverlayScreen = false;
		base.IsPopupScreen = true;
		_sparkleParticleSystem = new AreaTitleSparkleParticleSystem(gcm.TxParticleEnergy, 64);
	}

	private void RefreshZoom()
	{
		if (base.ScreenManager != null)
		{
			_titleSafeArea = base.ScreenManager.TitleSafeArea;
		}
		Vector2 drawPosition = new Vector2(_titleSafeArea.Center.X / _zoom - _entireRenderWidth / 2, _titleSafeArea.Center.Y / _zoom + -48);
		_drawPosition = drawPosition;
		_zoomedDrawPosition = _drawPosition * _zoom;
		_revealLeft = (int)_drawPosition.X;
	}

	internal override void OnScreenResize()
	{
		base.OnScreenResize();
		RefreshZoom();
	}

	public override void Update(GameTime gameTime, bool doesOtherScreenHasFocus, bool isCoveredByOtherScreen)
	{
		if (!doesOtherScreenHasFocus)
		{
			float num = (float)gameTime.ElapsedGameTime.TotalSeconds;
			if (!base.IsOverlayScreen)
			{
				_controlTimer += num;
				if (_controlTimer > 0f)
				{
					base.IsOverlayScreen = true;
				}
			}
			UpdateSequence(num);
			_sparkleParticleSystem.Update(num);
		}
		base.Update(gameTime, doesOtherScreenHasFocus, isCoveredByOtherScreen);
	}

	private void UpdateSequence(float delta)
	{
		if (delta > 0f && _sequenceTimer <= 0f && _revealType == ETitleSequenceRevealType.Sideswipe)
		{
			base.ScreenManager.Jukebox.PlayCue(ESFX.AreaTitle);
		}
		_sequenceTimer += delta;
		if (_sequenceTimer < 2.65f)
		{
			if (_sequenceTimer < 0.4f)
			{
				_sequenceState = ETitleSequenceState.Revealing;
				float num = _sequenceTimer / 0.4f;
				_sequencePercentage = 1f - (float)Math.Cos((float)Math.PI / 2f * num);
				if (_revealType == ETitleSequenceRevealType.Sideswipe)
				{
					_revealWidth = (int)((float)_entireRenderWidth * _sequencePercentage);
					_revealRightZoomed = (_revealLeft + _revealWidth) * _zoom;
					float num2 = num * ((float)Math.PI * 2f) * 4f;
					float num3 = (float)Math.Sin(num2);
					float y = (float)Math.Cos(num2);
					float num4 = num3 * 12f;
					Vector2 where = new Vector2(_drawPosition.X + (float)_revealWidth, _drawPosition.Y - num4) * _zoom;
					_sparkleParticleSystem.AddParticles(where, new Vector2(-1f, y));
					where = new Vector2(_drawPosition.X + (float)_revealWidth, _drawPosition.Y + num4) * _zoom;
					_sparkleParticleSystem.AddParticles(where, new Vector2(-1f, y));
				}
				else
				{
					_revealWidth = _entireRenderWidth;
					_revealRightZoomed = (_revealLeft + _revealWidth) * _zoom;
				}
			}
			else if (_sequenceTimer < 0.9f)
			{
				_sequenceState = ETitleSequenceState.Flashing;
				_sequencePercentage = (_sequenceTimer - 0.4f) / 0.5f;
			}
			else if (_sequenceTimer < 1.9f)
			{
				_sequenceState = ETitleSequenceState.Lingering;
				_sequencePercentage = 1f;
			}
			else
			{
				_sequenceState = ETitleSequenceState.Fading;
			}
		}
		else
		{
			_sequenceState = ETitleSequenceState.Finished;
		}
	}

	public override void Draw(GameTime gameTime)
	{
		Color drawColor = Color.White * 0.33f;
		if (_sequenceState != 0)
		{
			drawColor *= base.DrawColorPercentage;
		}
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
		Rectangle blackBackingLeftFrameSource = _blackBackingLeftFrameSource;
		Vector2 drawPosition = new Vector2(_zoomedDrawPosition.X, _zoomedDrawPosition.Y + (float)(-14 * _zoom));
		DrawUIElement(spriteBatch, drawPosition, blackBackingLeftFrameSource, drawColor, _zoom, SpriteEffects.None);
		blackBackingLeftFrameSource = _blackBackingFrameSource;
		Rectangle renderRectangle = new Rectangle((int)(_zoomedDrawPosition.X + (float)(_blackBackingLeftFrameSource.Width * _zoom)), (int)(_zoomedDrawPosition.Y + (float)(-14 * _zoom)), _blackBackingMidWidth * _zoom, blackBackingLeftFrameSource.Height * _zoom);
		DrawRectangleUIElement(spriteBatch, renderRectangle, blackBackingLeftFrameSource, drawColor, SpriteEffects.None);
		blackBackingLeftFrameSource = _blackBackingLeftFrameSource;
		drawPosition = new Vector2(renderRectangle.Right, drawPosition.Y);
		DrawUIElement(spriteBatch, drawPosition, blackBackingLeftFrameSource, drawColor, _zoom, SpriteEffects.FlipHorizontally);
		spriteBatch.End();
		base.Draw(gameTime);
	}

	internal override void DrawToastContent(SpriteBatch spriteBatch, Color drawColor, float zoom)
	{
		if (_sequenceState != ETitleSequenceState.Finished)
		{
			int num = (int)zoom;
			if (num != _zoom)
			{
				_zoom = num;
				RefreshZoom();
			}
			if (_sequenceState == ETitleSequenceState.Revealing)
			{
				float num2 = _sequencePercentage * 2f;
				if (num2 > 1f)
				{
					num2 = 1f;
				}
				drawColor = Color.White * num2;
			}
			Vector2 drawPosition = Vector2.Add(_zoomedDrawPosition, new Vector2(0f * zoom, -5f * zoom));
			DrawUIElement(spriteBatch, drawPosition, _goldFrameLeftFrameSource, drawColor, zoom, SpriteEffects.None);
			drawPosition.X += (float)_goldFrameLeftFrameSource.Width * zoom;
			Rectangle renderRectangle = new Rectangle((int)drawPosition.X, (int)drawPosition.Y, (int)((float)_goldFrameMidWidth * zoom), (int)((float)_goldFrameMidFrameSource.Height * zoom));
			DrawRectangleUIElement(spriteBatch, renderRectangle, _goldFrameMidFrameSource, drawColor, SpriteEffects.None);
			drawPosition.X += (float)_goldFrameMidWidth * zoom;
			DrawUIElement(spriteBatch, drawPosition, _goldFrameRightFrameSource, drawColor, zoom, SpriteEffects.None);
			drawPosition = new Vector2(_zoomedDrawPosition.X + (float)(_entireRenderWidth + -45) * zoom, _zoomedDrawPosition.Y + 2f * zoom);
			DrawUIElement(spriteBatch, drawPosition, _dateTextFrameSource, drawColor, zoom, SpriteEffects.None);
			drawPosition = new Vector2(_zoomedDrawPosition.X + 32f * zoom, _zoomedDrawPosition.Y + (float)_textOffsetY * zoom);
			DrawUIElement(spriteBatch, drawPosition, _areaTitleFrameSource, drawColor, zoom, SpriteEffects.None);
		}
		_sparkleParticleSystem.Draw(spriteBatch, Vector2.Zero, Vector2.Zero, 1f);
	}

	private void DrawUIElement(SpriteBatch spriteBatch, Vector2 drawPosition, Rectangle frameSource, Color drawColor, float zoom, SpriteEffects spriteEffects)
	{
		if (_sequenceState != 0)
		{
			spriteBatch.Draw(base.Sprite.Texture, drawPosition, frameSource, drawColor, 0f, Vector2.Zero, zoom, spriteEffects, 0f);
			return;
		}
		Rectangle renderRectangle = new Rectangle((int)drawPosition.X, (int)drawPosition.Y, (int)((float)frameSource.Width * zoom), (int)((float)frameSource.Height * zoom));
		DrawRectangleUIElement(spriteBatch, renderRectangle, frameSource, drawColor, spriteEffects);
	}

	private void DrawRectangleUIElement(SpriteBatch spriteBatch, Rectangle renderRectangle, Rectangle frameSource, Color drawColor, SpriteEffects spriteEffects)
	{
		if (_sequenceState != 0)
		{
			spriteBatch.Draw(base.Sprite.Texture, renderRectangle, frameSource, drawColor, 0f, Vector2.Zero, spriteEffects, 0f);
		}
		else
		{
			if (renderRectangle.Left > _revealRightZoomed)
			{
				return;
			}
			if (renderRectangle.Right > _revealRightZoomed)
			{
				int num = _revealRightZoomed - renderRectangle.Left;
				int num2 = num / _zoom;
				renderRectangle = new Rectangle(renderRectangle.X, renderRectangle.Y, num, renderRectangle.Height);
				if (num2 < frameSource.Width)
				{
					frameSource = new Rectangle(frameSource.X, frameSource.Y, num2, frameSource.Height);
				}
			}
			spriteBatch.Draw(base.Sprite.Texture, renderRectangle, frameSource, drawColor, 0f, Vector2.Zero, spriteEffects, 0f);
		}
	}
}
