using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameAbstractions;

public class GamePadWrapper
{
	public const int JumpInputBufferSize = 8;

	private readonly bool _canVibrate;

	private readonly bool[] _jumpInputBuffer = new bool[8];

	private readonly List<VibrationSpecification> _vibrationList = new List<VibrationSpecification>();

	private PlayerIndex _playerIndex;

	private int _playerNumber;

	private int _vibrationFrameUpdateCounter;

	private GamePadState _gamePadState;

	private KeyboardState _keyboardState;

	private KeyboardState _lastKeyboardState;

	public int PlayerNumber => _playerNumber;

	public PlayerIndex PlayerIndex => _playerIndex;

	public bool IsJumpDown { get; set; }

	public bool IsSpellDown { get; set; }

	public bool IsMeleeDown { get; private set; }

	public bool IsTimeDown { get; private set; }

	public bool IsStartDown { get; private set; }

	public bool IsBackDown { get; private set; }

	public bool IsDashDown { get; private set; }

	public bool IsRStickDown { get; private set; }

	public bool IsRTriggerDown { get; private set; }

	public bool IsBackdashDown { get; private set; }

	public bool IsLStickDown { get; private set; }

	public bool IsLTriggerDown { get; private set; }

	public bool WasJumpDown { get; set; }

	public bool WasSpellDown { get; set; }

	public bool WasMeleeDown { get; private set; }

	public bool WasTimeDown { get; private set; }

	public bool WasStartDown { get; private set; }

	public bool WasBackDown { get; private set; }

	public bool WasDashDown { get; private set; }

	public bool WasRStickDown { get; private set; }

	public bool WasRTriggerDown { get; private set; }

	public bool WasBackdashDown { get; private set; }

	public bool WasLStickDown { get; private set; }

	public bool WasLTriggerDown { get; private set; }

	public bool IsLeftDown { get; private set; }

	public bool IsRightDown { get; private set; }

	public bool IsUpDown { get; private set; }

	public bool IsDownDown { get; private set; }

	public bool WasLeftDown { get; private set; }

	public bool WasRightDown { get; private set; }

	public bool WasUpDown { get; private set; }

	public bool WasDownDown { get; private set; }

	public int JumpBufferValue { get; private set; }

	public Vector2 RStick { get; private set; }

	public Vector2 LStick { get; private set; }

	public Vector2 LastRStick { get; private set; }

	public Vector2 LastLStick { get; private set; }

	public GamePadState GamePadState => _gamePadState;

	public KeyboardState KeyboardState => _keyboardState;

	public KeyboardState LastKeyboardState => _lastKeyboardState;

	public ControllerMapping ControllerMapping { get; set; }

	public bool[] JumpInputBuffer => _jumpInputBuffer;

	public GamePadWrapper(int inPlayerNumber, ControllerMapping controllerMapping, bool canVibrate)
	{
		_canVibrate = canVibrate;
		ControllerMapping = controllerMapping;
		_playerNumber = inPlayerNumber;
		_playerIndex = XnaEx.IntToPlayerIndex(_playerNumber);
		UpdateState(areControlsLocked: false);
	}

	internal void SetPlayerIndex(PlayerIndex newIndex)
	{
		_playerIndex = newIndex;
		_playerNumber = XnaEx.PlayerIndexToInt(newIndex);
	}

	public void UpdateState(bool areControlsLocked)
	{
		Reset();
		_gamePadState = GamePad.GetState(_playerIndex);
		_keyboardState = Keyboard.GetState();
		if (!areControlsLocked)
		{
			IsJumpDown = ControllerMapping.IsButtonDown(1, _gamePadState, _keyboardState);
			IsSpellDown = ControllerMapping.IsButtonDown(3, _gamePadState, _keyboardState);
			IsMeleeDown = ControllerMapping.IsButtonDown(2, _gamePadState, _keyboardState);
			IsTimeDown = ControllerMapping.IsButtonDown(4, _gamePadState, _keyboardState);
			IsDashDown = ControllerMapping.IsButtonDown(7, _gamePadState, _keyboardState);
			IsBackdashDown = ControllerMapping.IsButtonDown(8, _gamePadState, _keyboardState);
			IsLTriggerDown = ControllerMapping.IsButtonDown(13, _gamePadState, _keyboardState);
			IsRTriggerDown = ControllerMapping.IsButtonDown(14, _gamePadState, _keyboardState);
			IsStartDown = ControllerMapping.IsButtonDown(5, _gamePadState, _keyboardState);
			IsBackDown = ControllerMapping.IsButtonDown(6, _gamePadState, _keyboardState);
			IsRightDown = ControllerMapping.IsButtonDown(11, _gamePadState, _keyboardState);
			IsLeftDown = ControllerMapping.IsButtonDown(9, _gamePadState, _keyboardState);
			IsUpDown = ControllerMapping.IsButtonDown(10, _gamePadState, _keyboardState);
			IsDownDown = ControllerMapping.IsButtonDown(12, _gamePadState, _keyboardState);
			if (_gamePadState.IsButtonDown(Buttons.RightStick))
			{
				IsRStickDown = true;
			}
			if (_gamePadState.IsButtonDown(Buttons.LeftStick))
			{
				IsLStickDown = true;
			}
			RStick = _gamePadState.ThumbSticks.Right;
			LStick = _gamePadState.ThumbSticks.Left;
			JumpBufferValue = 0;
			for (int num = 6; num >= 0; num--)
			{
				bool flag = _jumpInputBuffer[num];
				_jumpInputBuffer[num + 1] = flag;
				if (flag)
				{
					JumpBufferValue++;
				}
			}
			_jumpInputBuffer[0] = IsJumpDown;
			if (IsJumpDown)
			{
				JumpBufferValue++;
			}
		}
		UpdateVibration();
	}

	internal void UpdateVibration()
	{
		if (!_canVibrate || !ControllerMapping.DoesUseControllerRumble)
		{
			return;
		}
		float num = 0f;
		float num2 = 0f;
		for (int num3 = _vibrationList.Count - 1; num3 >= 0; num3--)
		{
			VibrationSpecification vibrationSpecification = _vibrationList[num3];
			vibrationSpecification.Duration--;
			if (vibrationSpecification.Duration < 0)
			{
				_vibrationList.RemoveAt(num3);
			}
			else
			{
				num2 = Math.Max(num2, vibrationSpecification.LeftIntensity);
				num = Math.Max(num, vibrationSpecification.RightIntensity);
			}
		}
		if (num > 0f || num2 > 0f)
		{
			GamePad.SetVibration(_playerIndex, num2, num);
			_vibrationFrameUpdateCounter = 0;
		}
		else if (_vibrationFrameUpdateCounter > 0)
		{
			_vibrationFrameUpdateCounter--;
		}
		else
		{
			GamePad.SetVibration(_playerIndex, 0f, 0f);
			_vibrationFrameUpdateCounter = 60;
		}
	}

	public void AddVibration(int duration, float leftIntensity, float rightIntensity)
	{
		if (_canVibrate && ControllerMapping.DoesUseControllerRumble)
		{
			_vibrationList.Add(new VibrationSpecification
			{
				Duration = duration,
				LeftIntensity = leftIntensity,
				RightIntensity = rightIntensity
			});
		}
	}

	public void Reset()
	{
		WasJumpDown = IsJumpDown;
		WasSpellDown = IsSpellDown;
		WasMeleeDown = IsMeleeDown;
		WasTimeDown = IsTimeDown;
		WasDashDown = IsDashDown;
		WasRStickDown = IsRStickDown;
		WasRTriggerDown = IsRTriggerDown;
		WasBackdashDown = IsBackdashDown;
		WasLStickDown = IsLStickDown;
		WasLTriggerDown = IsLTriggerDown;
		WasStartDown = IsStartDown;
		WasBackDown = IsBackDown;
		WasLeftDown = IsLeftDown;
		WasUpDown = IsUpDown;
		WasRightDown = IsRightDown;
		WasDownDown = IsDownDown;
		LastLStick = LStick;
		LastRStick = RStick;
		IsJumpDown = false;
		IsSpellDown = false;
		IsMeleeDown = false;
		IsTimeDown = false;
		IsDashDown = false;
		IsBackdashDown = false;
		IsRStickDown = false;
		IsLStickDown = false;
		IsRTriggerDown = false;
		IsLTriggerDown = false;
		IsStartDown = false;
		IsBackDown = false;
		IsRightDown = false;
		IsLeftDown = false;
		IsUpDown = false;
		IsDownDown = false;
		_lastKeyboardState = KeyboardState;
	}

	internal void PostMenuDisableInput()
	{
		IsJumpDown = false;
		WasJumpDown = false;
		IsSpellDown = false;
		WasSpellDown = false;
		IsTimeDown = false;
		WasTimeDown = false;
		IsMeleeDown = false;
		WasMeleeDown = false;
		IsBackdashDown = false;
		WasBackdashDown = false;
		IsDashDown = false;
		WasDashDown = false;
		for (int i = 0; i < 8; i++)
		{
			_jumpInputBuffer[i] = false;
		}
		JumpBufferValue = 0;
	}
}
