using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameStateManagement.ScreenManager;
using Timespinner.GameStateManagement.Screens.InGame.Toasts;

namespace Timespinner.GameStateManagement.Screens.InGame;

internal abstract class BaseToastPopup : GameScreen
{
	private const float TimeToFlash = 0.5f;

	private const float TimeToWait = 0.75f;

	private const float TimeToFade = 0.25f;

	private const float GlowIntensity = 8f;

	private static readonly Color FlashColor = new Color(1f, 1f, 0.9f, 1f);

	private readonly bool _doesFreezeGameplay;

	private readonly float _timeBeforeFlashing;

	private readonly float _timeToFlash;

	private readonly float _timeToWait;

	private readonly float _timeToFade;

	private readonly float _totalDisplayTime;

	private readonly SpriteSheet _sprite;

	private readonly GCM _gcm;

	private bool _isFlashing;

	private float _flashPercentage;

	private float _displayTimer;

	private float _drawColorPercentage;

	private Color _drawColor;

	private Color _glowColor;

	internal bool IsDoneFlashing { get; private set; }

	internal bool IsFinishedAndIsWaitingToClose { get; private set; }

	internal bool HasReceivedInputToClose { get; set; }

	internal bool DoesWaitForInputToFinish { get; set; }

	internal float DrawColorPercentage => _drawColorPercentage;

	internal SpriteSheet Sprite => _sprite;

	internal BaseToastPopup(SpriteSheet sprite, bool doesFreezeGameplay, GCM gcm)
		: this(sprite, doesFreezeGameplay, gcm, 0f, 0.5f, 0.75f, 0.25f)
	{
	}

	internal BaseToastPopup(SpriteSheet sprite, bool doesFreezeGameplay, GCM gcm, float timeBeforeFlashing, float timeToFlash, float timeToWait, float timeToFade)
	{
		_sprite = sprite;
		_doesFreezeGameplay = doesFreezeGameplay;
		_gcm = gcm;
		_timeBeforeFlashing = timeBeforeFlashing;
		_timeToFlash = timeToFlash;
		_timeToWait = timeToWait;
		_timeToFade = timeToFade;
		_totalDisplayTime = _timeBeforeFlashing + _timeToFlash + _timeToWait + _timeToFade;
		base.IsPopupScreen = true;
		base.IsOverlayScreen = !_doesFreezeGameplay;
	}

	public override void Update(GameTime gameTime, bool doesOtherScreenHasFocus, bool isCoveredByOtherScreen)
	{
		if (!doesOtherScreenHasFocus)
		{
			float num = (float)gameTime.ElapsedGameTime.TotalSeconds;
			_displayTimer += num;
			if (_displayTimer < _totalDisplayTime)
			{
				if (!(_displayTimer < _timeBeforeFlashing))
				{
					if (_displayTimer < _timeToFlash + _timeBeforeFlashing)
					{
						_isFlashing = true;
						float num2 = (_displayTimer - _timeBeforeFlashing) / _timeToFlash;
						_flashPercentage = (float)Math.Sin(num2 * ((float)Math.PI / 2f));
						_drawColorPercentage = _flashPercentage;
						_glowColor = FlashColor * _flashPercentage;
					}
					else if (_displayTimer < _timeToWait + _timeToFlash + _timeBeforeFlashing)
					{
						_isFlashing = false;
						IsDoneFlashing = true;
						_drawColor = Color.White;
						_drawColorPercentage = 1f;
					}
					else if (!DoesWaitForInputToFinish || HasReceivedInputToClose)
					{
						float num3 = (_displayTimer - (_timeToWait + _timeToFlash + _timeBeforeFlashing)) / _timeToFade;
						_drawColorPercentage = (float)Math.Cos(num3 * ((float)Math.PI / 2f));
						_drawColor = Color.White * _drawColorPercentage;
					}
					else
					{
						_displayTimer = _timeToWait + _timeToFlash + _timeBeforeFlashing;
						IsFinishedAndIsWaitingToClose = true;
					}
				}
			}
			else
			{
				ExitScreen();
			}
		}
		base.Update(gameTime, doesOtherScreenHasFocus, isCoveredByOtherScreen);
	}

	public override void Draw(GameTime gameTime)
	{
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		if (_isFlashing)
		{
			_gcm.EfBrighten.Parameters["shinyAmount"].SetValue(8f);
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, _gcm.EfBrighten);
		}
		else
		{
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
		}
		DrawToastContent(spriteBatch, _isFlashing ? _glowColor : _drawColor, Constants.InGameZoom);
		spriteBatch.End();
		base.Draw(gameTime);
	}

	internal virtual void DrawToastContent(SpriteBatch spriteBatch, Color drawColor, float zoom)
	{
	}

	public static BaseToastPopup ToastFromType(EToastType type, int argument, ScriptAction script, GCM gcm, Rectangle titleSafeArea, float zoom, ControllerMapping playerControls)
	{
		BaseToastPopup result = null;
		switch (type)
		{
		case EToastType.LevelUp:
			result = new CharacterLevelUpToast(gcm.SpLevelUp, gcm);
			break;
		case EToastType.Orb:
			result = new OrbLevelUpToast(gcm.SpPauseMenu, gcm, argument, titleSafeArea, zoom);
			break;
		case EToastType.AreaTitle:
			result = new AreaTitleToast(gcm.SpAreaTitles, gcm, argument, titleSafeArea, zoom);
			break;
		case EToastType.Health:
		case EToastType.Aura:
		case EToastType.Sand:
			result = new StatMaxUpToast(gcm.SpLevelUp, gcm, titleSafeArea, zoom, type);
			break;
		case EToastType.RelicOrbGet:
			result = new RelicOrbGetToast(gcm.SpLevelUp, gcm, script, playerControls);
			break;
		case EToastType.QuestComplete:
			result = new QuestCompleteToast(gcm.SpLevelUp, gcm, script, playerControls);
			break;
		}
		return result;
	}
}
