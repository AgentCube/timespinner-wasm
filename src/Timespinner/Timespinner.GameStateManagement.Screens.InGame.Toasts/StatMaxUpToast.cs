using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions;

namespace Timespinner.GameStateManagement.Screens.InGame.Toasts;

internal class StatMaxUpToast : BaseToastPopup
{
	private const int DisplayOffsetY = -72;

	private const int BlackLeftDrawOffsetX = -64;

	private const int BlackLeftDrawOffsetY = -8;

	private const int BlackCenterWidth = 32;

	private const int BlackRightDrawOffsetX = 80;

	private const int EnglishTextFrameIndexStart = 17;

	private const int AsianTextOffsetY = 3;

	private const float TimeToFlash = 0.75f;

	private const float TimeToWait = 0.9f;

	private const float TimeToFade = 0.2f;

	private static readonly Vector2 BottomFrameOffset = new Vector2(-64f, 7f);

	private readonly EToastType _toastType;

	private readonly Vector2 _textDrawOrigin;

	private readonly Rectangle _textFrameSource;

	private readonly Rectangle _frameFrameSource;

	private readonly Rectangle _gemFrameSource;

	private readonly Rectangle _blackLeftFrameSource;

	private readonly Rectangle _blackMidFrameSource;

	private readonly LevelUpSparkleParticleSystem _sparkleParticleSystem;

	private readonly SpriteSheet _textSprite;

	private bool _hasPlayedCue;

	private int _zoom;

	private Vector2 _drawPosition;

	private Rectangle _titleSafeArea;

	public StatMaxUpToast(SpriteSheet sprite, GCM gcm, Rectangle titleSafe, float zoom, EToastType toastType)
		: base(sprite, doesFreezeGameplay: true, gcm, 0f, 0.75f, 0.9f, 0.2f)
	{
		_zoom = (int)zoom;
		_toastType = toastType;
		_titleSafeArea = titleSafe;
		RefreshZoom();
		_frameFrameSource = base.Sprite.GetFrameSource(20);
		_blackLeftFrameSource = base.Sprite.GetFrameSource(24);
		_blackMidFrameSource = base.Sprite.GetFrameSource(25);
		bool isAsianLocale = Loc.IsAsianLocale;
		ELanguageLocale currentLocale = Loc.CurrentLocale;
		bool flag = currentLocale == ELanguageLocale.EN || currentLocale == ELanguageLocale.PD;
		int num = 0;
		int num3;
		if (!flag)
		{
			_textSprite = (isAsianLocale ? gcm.SpLocToastsAsian : gcm.SpLocToastsLatin);
			num = (isAsianLocale ? 3 : 0);
			int num2 = 0;
			switch (currentLocale)
			{
			case ELanguageLocale.BP:
				num2 = 3;
				break;
			case ELanguageLocale.CN:
				num2 = 0;
				break;
			case ELanguageLocale.DE:
				num2 = 1;
				break;
			case ELanguageLocale.ES:
				num2 = 2;
				break;
			case ELanguageLocale.FR:
				num2 = 0;
				break;
			case ELanguageLocale.JP:
				num2 = 1;
				break;
			case ELanguageLocale.RU:
				num2 = 4;
				break;
			}
			num3 = num2 * 3;
		}
		else
		{
			_textSprite = base.Sprite;
			num3 = 17;
		}
		switch (_toastType)
		{
		case EToastType.Health:
			_textFrameSource = _textSprite.GetFrameSource(num3);
			_gemFrameSource = base.Sprite.GetFrameSource(21);
			break;
		case EToastType.Aura:
			_textFrameSource = _textSprite.GetFrameSource(num3 + 1);
			_gemFrameSource = base.Sprite.GetFrameSource(22);
			break;
		default:
			_textFrameSource = _textSprite.GetFrameSource(num3 + 2);
			_gemFrameSource = base.Sprite.GetFrameSource(23);
			break;
		}
		_textDrawOrigin = new Vector2((int)((float)_textFrameSource.Width * 0.5f), (int)((float)_textFrameSource.Height * 0.5f) + num);
		base.IsOverlayScreen = true;
		base.IsPopupScreen = true;
		_sparkleParticleSystem = new LevelUpSparkleParticleSystem(sprite, 1, Color.White);
	}

	private void RefreshZoom()
	{
		if (base.ScreenManager != null)
		{
			_titleSafeArea = base.ScreenManager.TitleSafeArea;
		}
		Vector2 drawPosition = new Vector2(_titleSafeArea.Center.X / _zoom, _titleSafeArea.Center.Y / _zoom + -72);
		_drawPosition = drawPosition;
	}

	public override void Update(GameTime gameTime, bool doesOtherScreenHasFocus, bool isCoveredByOtherScreen)
	{
		float num = (float)gameTime.ElapsedGameTime.TotalSeconds;
		if (num > 0f && !_hasPlayedCue)
		{
			_hasPlayedCue = true;
			base.ScreenManager.Jukebox.PlayCue(ESFX.CharacterStatUp);
		}
		_sparkleParticleSystem.Update(num);
		base.Update(gameTime, doesOtherScreenHasFocus, isCoveredByOtherScreen);
	}

	public override void Draw(GameTime gameTime)
	{
		Color color = Color.White * 0.25f * base.DrawColorPercentage;
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
		Vector2 a = _drawPosition * _zoom;
		a = a.Add(new Point(-64 * _zoom, -8 * _zoom));
		spriteBatch.Draw(base.Sprite.Texture, a, _blackLeftFrameSource, color, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
		spriteBatch.Draw(destinationRectangle: new Rectangle((int)(a.X + (float)(_blackLeftFrameSource.Width * _zoom)), (int)a.Y, 32 * _zoom, _blackMidFrameSource.Height * _zoom), texture: base.Sprite.Texture, sourceRectangle: _blackMidFrameSource, color: color, rotation: 0f, origin: Vector2.Zero, effects: SpriteEffects.None, layerDepth: 0f);
		a = a.Add(new Point(80 * _zoom, 0));
		spriteBatch.Draw(base.Sprite.Texture, a, _blackLeftFrameSource, color, 0f, Vector2.Zero, _zoom, SpriteEffects.FlipHorizontally, 0f);
		spriteBatch.End();
		base.Draw(gameTime);
	}

	internal override void DrawToastContent(SpriteBatch spriteBatch, Color drawColor, float zoom)
	{
		int num = (int)zoom;
		if (_zoom != num)
		{
			_zoom = num;
			RefreshZoom();
		}
		Vector2 vector = _drawPosition * zoom;
		Vector2 vector2 = Vector2.Add(vector, BottomFrameOffset * zoom);
		spriteBatch.Draw(base.Sprite.Texture, vector2, _frameFrameSource, drawColor, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
		vector2.X += (0f - BottomFrameOffset.X) * zoom;
		spriteBatch.Draw(base.Sprite.Texture, vector2, _frameFrameSource, drawColor, 0f, Vector2.Zero, zoom, SpriteEffects.FlipHorizontally, 0f);
		vector2 = Vector2.Add(vector2, new Vector2((float)(-(_gemFrameSource.Width / 2)) * zoom, zoom));
		spriteBatch.Draw(base.Sprite.Texture, vector2, _gemFrameSource, drawColor, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
		_sparkleParticleSystem.Draw(spriteBatch, Vector2.Zero, Vector2.Zero, 1f);
		vector2 = vector;
		spriteBatch.Draw(_textSprite.Texture, vector2, _textFrameSource, drawColor, 0f, _textDrawOrigin, zoom, SpriteEffects.None, 0f);
	}
}
