using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Saving;

namespace Timespinner.GameStateManagement.Screens.PauseMenu.Journal;

internal class JournalEntryMemoryScreen : JournalEntryScreen
{
	private const int MemoryMaxLines = 11;

	private const int TextBoxHeight = 192;

	private const int DialogueWidth = 320;

	private const int TextOffsetY = 8;

	private const int NewLineIconMarginX = -2;

	private const int NewLineIconMarginY = -24;

	private const int ShadyCharacterCount = 3;

	private const float ShadyColorMultiplier = 2f / 3f;

	private const float TimeBetweenSmokeEmission = 0.03f;

	private const float SpirographRotationSpeed = 0.5f;

	private const float SpirographRotationIncrement = 4f;

	private static readonly Vector2 SpirographDrawOrigin = new Vector2(16f, 16f);

	private static readonly Color ScreenFillColor = new Color(16, 16, 16);

	private static readonly Color TextColor = new Color(200, 232, 255);

	private static readonly Color ShadowColor = new Color(80, 32, 32);

	private static readonly Color UpperTextColor = new Color(32, 24, 32);

	private static readonly Color UpperTextShadowColor = new Color(16, 8, 16);

	private static readonly Color ShadyTextColor = new Color(128, 64, 104, 64);

	private static readonly Color ShadyTextShadowColor = ShadyTextColor * 0.35f;

	private static readonly Vector4 SpirographGradientColor1 = new Vector4(0.4f, 0.4f, 0.3f, 1f);

	private static readonly Vector4 SpirographGradientColor2 = new Vector4(0.3f, 0.3f, 0.6f, 1f);

	private readonly Rectangle _spirographFrameSource;

	private readonly Random _random = new Random();

	private readonly MemorySmokeParticleSystem _topBotSmokeParticleSystem;

	private readonly MemorySmokeParticleSystem _sidesSmokeParticleSystem;

	private readonly SpriteSheet _sprite;

	private EBGM _songThatWasPlayingBefore;

	private int _drawTicks;

	private float _baseRotation;

	private float _spirographTimer;

	private float _gradientTranslation;

	private float _smokeEmissionTimer;

	private float _spirographRotation;

	private Rectangle _messageBoxDimensions;

	private Color _spirographDrawColor;

	public JournalEntryMemoryScreen(List<string> entries, SpriteFont font, GCM gcm, int scale, GameConfigSave gameConfig, Action fullExitAction)
		: base(entries, font, gcm, scale, 320, 11, doesAddNewlines: true, gameConfig, fullExitAction)
	{
		base.DoesDelayFromNewlines = true;
		base.DoesDelayEachLetter = true;
		base.NewLineIconIndex = 26;
		_sprite = gcm.SpJournalMenu;
		_spirographFrameSource = _sprite.GetFrameSource(29);
		_topBotSmokeParticleSystem = new MemorySmokeParticleSystem(_sprite, 80);
		_sidesSmokeParticleSystem = new MemorySmokeParticleSystem(_sprite, 48);
	}

	public override void LoadContent()
	{
		base.LoadContent();
		RefreshSizes();
		_songThatWasPlayingBefore = base.ScreenManager.Jukebox.CurrentSongEnum;
		base.ScreenManager.Jukebox.PlaySong(EBGM.CsSelen);
	}

	internal override void OnScreenResize()
	{
		base.OnScreenResize();
		RefreshSizes();
	}

	private void RefreshSizes()
	{
		_messageBoxDimensions = new Rectangle(base.LetterBoxOffsetX + 40 * base.Scale, base.LetterBoxOffsetY + 32 * base.Scale, 320 * base.Scale, 192 * base.Scale);
		base.NewlineIconDrawPosition = new Vector2(_messageBoxDimensions.Right + -2 * base.Scale, _messageBoxDimensions.Bottom + -24 * base.Scale);
	}

	public override void ExitScreen()
	{
		base.ScreenManager.Jukebox.PlaySong(_songThatWasPlayingBefore);
		base.ExitScreen();
	}

	public override void Update(GameTime gameTime, bool doesOtherScreenHasFocus, bool isCoveredByOtherScreen)
	{
		base.Update(gameTime, doesOtherScreenHasFocus, isCoveredByOtherScreen);
		if (!doesOtherScreenHasFocus)
		{
			float num = (float)gameTime.ElapsedGameTime.TotalSeconds;
			_smokeEmissionTimer -= num;
			if (_smokeEmissionTimer <= 0f)
			{
				_smokeEmissionTimer += 0.03f;
				float x = (float)(_random.NextDouble() * 400.0);
				float y = ((_random.NextDouble() >= 0.5) ? 240 : 0);
				_topBotSmokeParticleSystem.AddParticles(new Vector2(x, y));
				x = ((!(_random.NextDouble() >= 0.5)) ? 400 : 0);
				y = (float)(_random.NextDouble() * 240.0);
				_sidesSmokeParticleSystem.AddParticles(new Vector2(x, y));
			}
			_topBotSmokeParticleSystem.Update(num);
			_sidesSmokeParticleSystem.Update(num);
			UpdateSpirograph(num);
		}
	}

	private void UpdateSpirograph(float delta)
	{
		_baseRotation += delta * 0.5f;
		_spirographTimer += delta;
		if (_spirographTimer > (float)Math.PI * 2f)
		{
			_spirographTimer -= (float)Math.PI * 2f;
		}
		_gradientTranslation = ((float)Math.Sin(_spirographTimer) + 1f) / 2f;
		_spirographDrawColor = new Color(_gradientTranslation, 0f, 0f, 0f);
		_drawTicks = 16;
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
			if (num < 0)
			{
				Color shadyTextColor = ShadyTextColor;
				Color shadyTextShadowColor = ShadyTextShadowColor;
				for (int i = 1; i <= 3; i++)
				{
					dialogueLine.VisibleCharacters = dialogueLine.Length + num + i;
					dialogueLine.Draw(spriteBatch, drawPosition, shadyTextColor, shadyTextShadowColor, base.Scale, 1f);
					shadyTextColor *= 2f / 3f;
					shadyTextShadowColor *= 2f / 3f;
				}
			}
			dialogueLine.VisibleCharacters = dialogueLine.Length + num;
			dialogueLine.Draw(spriteBatch, new Vector2(drawPosition.X - (float)base.Scale, drawPosition.Y - (float)base.Scale), UpperTextColor, UpperTextShadowColor, base.Scale, 1f);
			dialogueLine.Draw(spriteBatch, drawPosition, TextColor, ShadowColor, base.Scale, 1f);
			drawPosition.Y += base.LineHeight;
			num2++;
		}
		base.DrawText(spriteBatch);
	}

	internal override void DrawBackground(SpriteBatch spriteBatch)
	{
		spriteBatch.End();
		base.ScreenManager.GraphicsDevice.SetRenderTarget(base.GCM.LevelRenderTarget);
		base.ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0f, 0);
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
		DrawSmoke(spriteBatch);
		DrawSpirograph(spriteBatch);
		spriteBatch.End();
		base.ScreenManager.GraphicsDevice.SetRenderTarget(null);
		base.ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, ScreenFillColor, 0f, 0);
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
		spriteBatch.Draw(base.GCM.LevelRenderTarget, base.BackgroundDrawPosition, null, Color.White, 0f, Vector2.Zero, base.Scale, SpriteEffects.None, 0f);
	}

	private void DrawSmoke(SpriteBatch spriteBatch)
	{
		_sidesSmokeParticleSystem.Draw(spriteBatch, Vector2.Zero, Vector2.Zero, 1f);
		_topBotSmokeParticleSystem.Draw(spriteBatch, Vector2.Zero, Vector2.Zero, 1f);
	}

	private void DrawSpirograph(SpriteBatch spriteBatch)
	{
		spriteBatch.End();
		base.GCM.EfSlidingGradient.Parameters["colorOne"].SetValue(SpirographGradientColor1);
		base.GCM.EfSlidingGradient.Parameters["colorTwo"].SetValue(SpirographGradientColor2);
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, base.GCM.EfSlidingGradient);
		for (int i = 0; i < 2; i++)
		{
			Vector2 position = ((i == 0) ? Vector2.Zero : new Vector2(400f, 240f));
			for (int j = 0; j < _drawTicks; j++)
			{
				_spirographRotation = _baseRotation + (float)j * 4f;
				spriteBatch.Draw(_sprite.Texture, position, _spirographFrameSource, _spirographDrawColor, _spirographRotation, SpirographDrawOrigin, 1f, SpriteEffects.None, 0f);
			}
		}
		spriteBatch.End();
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null);
	}
}
