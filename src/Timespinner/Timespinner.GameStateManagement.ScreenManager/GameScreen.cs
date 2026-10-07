using System;
using Microsoft.Xna.Framework;

namespace Timespinner.GameStateManagement.ScreenManager;

public abstract class GameScreen
{
	protected bool _isFirstTransitionOn = true;

	private bool _doesOtherScreenHaveFocus;

	private float _transitionOffPercentage = 1f;

	private PlayerIndex? _controllingPlayer;

	public bool IsExiting { get; set; }

	public bool IsPopupScreen { get; protected set; }

	public bool IsOverlayScreen { get; protected set; }

	public bool DoesLeaveIfNotInFocus { get; protected set; }

	public bool IsVibrationEnabled { get; protected set; }

	public EScreenState ScreenState { get; protected set; }

	public TimeSpan TransitionOnTime { get; protected set; }

	public TimeSpan TransitionOffTime { get; protected set; }

	public bool IsActive
	{
		get
		{
			if (!_doesOtherScreenHaveFocus)
			{
				if (ScreenState != 0)
				{
					return ScreenState == EScreenState.Active;
				}
				return true;
			}
			return false;
		}
	}

	public byte TransitionAlpha => (byte)(255f - TransitionOffPercentage * 255f);

	public float TransitionOffPercentage
	{
		get
		{
			return _transitionOffPercentage;
		}
		protected set
		{
			_transitionOffPercentage = value;
		}
	}

	public PlayerIndex? ControllingPlayer
	{
		get
		{
			return _controllingPlayer;
		}
		internal set
		{
			_controllingPlayer = value;
		}
	}

	public ScreenManager ScreenManager { get; internal set; }

	public virtual void LoadContent()
	{
	}

	public virtual void UnloadContent()
	{
	}

	public virtual void HandleInput(InputState input)
	{
	}

	public virtual void Draw(GameTime gameTime)
	{
	}

	internal virtual void LoadAtOnce()
	{
	}

	internal virtual float SlowLoad()
	{
		return -1f;
	}

	internal virtual void OnScreenResize()
	{
	}

	public virtual void Update(GameTime gameTime, bool doesOtherScreenHasFocus, bool isCoveredByOtherScreen)
	{
		_doesOtherScreenHaveFocus = doesOtherScreenHasFocus;
		if (IsExiting)
		{
			ScreenState = EScreenState.TransitionOff;
			if (!UpdateTransition(gameTime, TransitionOffTime, 1))
			{
				ScreenManager.RemoveScreen(this);
			}
		}
		else if (isCoveredByOtherScreen || (DoesLeaveIfNotInFocus && doesOtherScreenHasFocus))
		{
			if (UpdateTransition(gameTime, TransitionOffTime, 1))
			{
				ScreenState = EScreenState.TransitionOff;
			}
			else if (!DoesLeaveIfNotInFocus || !doesOtherScreenHasFocus)
			{
				ScreenState = EScreenState.Hidden;
			}
		}
		else if (UpdateTransition(gameTime, TransitionOnTime, -1))
		{
			ScreenState = EScreenState.TransitionOn;
		}
		else
		{
			ScreenState = EScreenState.Active;
			_isFirstTransitionOn = false;
		}
	}

	private bool UpdateTransition(GameTime gameTime, TimeSpan time, int direction)
	{
		float num = ((!(time == TimeSpan.Zero)) ? ((float)(gameTime.ElapsedGameTime.TotalMilliseconds / time.TotalMilliseconds)) : 1f);
		_transitionOffPercentage += num * (float)direction;
		if ((direction < 0 && _transitionOffPercentage <= 0f) || (direction > 0 && _transitionOffPercentage >= 1f))
		{
			_transitionOffPercentage = MathHelper.Clamp(_transitionOffPercentage, 0f, 1f);
			return false;
		}
		return true;
	}

	public virtual void ExitScreen()
	{
		if (TransitionOffTime == TimeSpan.Zero)
		{
			ScreenManager.RemoveScreen(this);
		}
		else
		{
			IsExiting = true;
		}
	}
}
