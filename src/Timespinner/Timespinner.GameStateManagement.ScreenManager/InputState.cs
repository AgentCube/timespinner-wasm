using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameStateManagement.ScreenManager;

public class InputState
{
	public const int MaxInputs = 4;

	public readonly bool[] GamePadWasConnected;

	public readonly KeyboardState[] CurrentKeyboardStates;

	public readonly GamePadState[] CurrentGamePadStates;

	public readonly KeyboardState[] LastKeyboardStates;

	public readonly GamePadState[] LastGamePadStates;

	public ControllerMapping ControllerMapping { get; set; }

	public InputState()
	{
		GamePadWasConnected = new bool[4];
		CurrentKeyboardStates = new KeyboardState[4];
		CurrentGamePadStates = new GamePadState[4];
		LastKeyboardStates = new KeyboardState[4];
		LastGamePadStates = new GamePadState[4];
	}

	private bool _hasResetVibration;

	public void Update(bool doesAllowControllerVibration)
	{
		KeyboardState keyboardState = Keyboard.GetState();
		for (int i = 0; i < 4; i++)
		{
			LastKeyboardStates[i] = CurrentKeyboardStates[i];
			LastGamePadStates[i] = CurrentGamePadStates[i];
			PlayerIndex playerIndex = (PlayerIndex)i;
			CurrentKeyboardStates[i] = keyboardState;
			if (i == 0 || GamePadWasConnected[i])
			{
				CurrentGamePadStates[i] = GamePad.GetState(playerIndex);
				if (CurrentGamePadStates[i].IsConnected)
				{
					GamePadWasConnected[i] = true;
				}
			}
			else
			{
				CurrentGamePadStates[i] = default(GamePadState);
			}
			if (!doesAllowControllerVibration && !_hasResetVibration)
			{
				GamePad.SetVibration(playerIndex, 0f, 0f);
			}
		}
		if (!doesAllowControllerVibration)
		{
			_hasResetVibration = true;
		}
		else
		{
			_hasResetVibration = false;
		}
	}

	public bool IsNewKeyPress(Keys key, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
	{
		if (controllingPlayer.HasValue)
		{
			playerIndex = controllingPlayer.Value;
			int num = (int)playerIndex;
			if (CurrentKeyboardStates[num].IsKeyDown(key))
			{
				return LastKeyboardStates[num].IsKeyUp(key);
			}
			return false;
		}
		if (!IsNewKeyPress(key, PlayerIndex.One, out playerIndex) && !IsNewKeyPress(key, PlayerIndex.Two, out playerIndex) && !IsNewKeyPress(key, PlayerIndex.Three, out playerIndex))
		{
			return IsNewKeyPress(key, PlayerIndex.Four, out playerIndex);
		}
		return true;
	}

	public bool IsKeyHold(Keys key, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
	{
		if (controllingPlayer.HasValue)
		{
			playerIndex = controllingPlayer.Value;
			int num = (int)playerIndex;
			return CurrentKeyboardStates[num].IsKeyDown(key);
		}
		if (!IsKeyHold(key, PlayerIndex.One, out playerIndex) && !IsKeyHold(key, PlayerIndex.Two, out playerIndex) && !IsKeyHold(key, PlayerIndex.Three, out playerIndex))
		{
			return IsKeyHold(key, PlayerIndex.Four, out playerIndex);
		}
		return true;
	}

	public bool IsNewButtonPress(Buttons button, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
	{
		if (controllingPlayer.HasValue)
		{
			playerIndex = controllingPlayer.Value;
			int num = (int)playerIndex;
			if (CurrentGamePadStates[num].IsButtonDown(button))
			{
				return LastGamePadStates[num].IsButtonUp(button);
			}
			return false;
		}
		if (!IsNewButtonPress(button, PlayerIndex.One, out playerIndex) && !IsNewButtonPress(button, PlayerIndex.Two, out playerIndex) && !IsNewButtonPress(button, PlayerIndex.Three, out playerIndex))
		{
			return IsNewButtonPress(button, PlayerIndex.Four, out playerIndex);
		}
		return true;
	}

	public bool IsButtonHold(Buttons button, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
	{
		if (controllingPlayer.HasValue)
		{
			playerIndex = controllingPlayer.Value;
			int num = (int)playerIndex;
			return CurrentGamePadStates[num].IsButtonDown(button);
		}
		if (!IsButtonHold(button, PlayerIndex.One, out playerIndex) && !IsButtonHold(button, PlayerIndex.Two, out playerIndex) && !IsButtonHold(button, PlayerIndex.Three, out playerIndex))
		{
			return IsButtonHold(button, PlayerIndex.Four, out playerIndex);
		}
		return true;
	}

	public bool IsPressConfirm(PlayerIndex? controllingPlayer)
	{
		return IsMenuPress(ButtonMapping.EDestinationType.Confirm, controllingPlayer);
	}

	public bool IsPressCancel(PlayerIndex? controllingPlayer)
	{
		return IsMenuPress(ButtonMapping.EDestinationType.Cancel, controllingPlayer);
	}

	public bool IsPressSecondary(PlayerIndex? controllingPlayer)
	{
		return IsMenuPress(ButtonMapping.EDestinationType.Secondary, controllingPlayer);
	}

	public bool IsPressTertiary(PlayerIndex? controllingPlayer)
	{
		return IsMenuPress(ButtonMapping.EDestinationType.Tertiary, controllingPlayer);
	}

	public bool IsPressMenuUp(PlayerIndex? controllingPlayer)
	{
		return IsMenuPress(ButtonMapping.EDestinationType.Up, controllingPlayer);
	}

	public bool IsPressMenuDown(PlayerIndex? controllingPlayer)
	{
		return IsMenuPress(ButtonMapping.EDestinationType.Down, controllingPlayer);
	}

	public bool IsPressMenuRight(PlayerIndex? controllingPlayer)
	{
		return IsMenuPress(ButtonMapping.EDestinationType.Right, controllingPlayer);
	}

	public bool IsPressMenuLeft(PlayerIndex? controllingPlayer)
	{
		return IsMenuPress(ButtonMapping.EDestinationType.Left, controllingPlayer);
	}

	private bool IsMenuPress(ButtonMapping.EDestinationType destination, PlayerIndex? controllingPlayer)
	{
		bool result = false;
		if (ControllerMapping.Mappings.ContainsKey((int)destination))
		{
			result = IsMappingPress(ControllerMapping.Mappings[(int)destination], controllingPlayer);
		}
		return result;
	}

	private bool IsMappingPress(ButtonMapping mapping, PlayerIndex? playerIndex)
	{
		PlayerIndex pressedIndex;
		return IsMappingPress(mapping, playerIndex, out pressedIndex);
	}

	private bool IsMappingPress(ButtonMapping mapping, PlayerIndex? playerIndex, out PlayerIndex pressedIndex)
	{
		if (playerIndex.HasValue)
		{
			pressedIndex = playerIndex.Value;
			int value = (int)playerIndex.Value;
			return mapping.IsButtonDown(CurrentGamePadStates[value], CurrentKeyboardStates[value]);
		}
		if (!IsMappingPress(mapping, PlayerIndex.One, out pressedIndex) && !IsMappingPress(mapping, PlayerIndex.Two, out pressedIndex) && !IsMappingPress(mapping, PlayerIndex.Three, out pressedIndex))
		{
			return IsMappingPress(mapping, PlayerIndex.Four, out pressedIndex);
		}
		return true;
	}

	public bool IsNewPressConfirm(PlayerIndex? controllingPlayer)
	{
		return IsNewMenuPress(ButtonMapping.EDestinationType.Confirm, controllingPlayer);
	}

	public bool IsNewPressCancel(PlayerIndex? controllingPlayer)
	{
		return IsNewMenuPress(ButtonMapping.EDestinationType.Cancel, controllingPlayer);
	}

	public bool IsNewPressConfirmCancel(PlayerIndex? controllingPlayer)
	{
		if (!IsNewPressConfirm(controllingPlayer))
		{
			return IsNewPressCancel(controllingPlayer);
		}
		return true;
	}

	public bool IsNewPressFinished(PlayerIndex? controllingPlayer)
	{
		if (!IsNewPressConfirm(controllingPlayer) && !IsNewPressCancel(controllingPlayer))
		{
			return IsNewPressExit(controllingPlayer);
		}
		return true;
	}

	public bool IsNewPressSecondary(PlayerIndex? controllingPlayer)
	{
		return IsNewMenuPress(ButtonMapping.EDestinationType.Secondary, controllingPlayer);
	}

	public bool IsNewPressTertiary(PlayerIndex? controllingPlayer)
	{
		return IsNewMenuPress(ButtonMapping.EDestinationType.Tertiary, controllingPlayer);
	}

	public bool IsNewPressPageLeft(PlayerIndex? controllingPlayer)
	{
		return IsNewMenuPress(ButtonMapping.EDestinationType.PageLeft, controllingPlayer);
	}

	public bool IsNewPressPageRight(PlayerIndex? controllingPlayer)
	{
		return IsNewMenuPress(ButtonMapping.EDestinationType.PageRight, controllingPlayer);
	}

	public bool IsNewPressMenuUp(PlayerIndex? controllingPlayer)
	{
		return IsNewMenuPress(ButtonMapping.EDestinationType.Up, controllingPlayer);
	}

	public bool IsNewPressMenuDown(PlayerIndex? controllingPlayer)
	{
		return IsNewMenuPress(ButtonMapping.EDestinationType.Down, controllingPlayer);
	}

	public bool IsNewPressMenuRight(PlayerIndex? controllingPlayer)
	{
		return IsNewMenuPress(ButtonMapping.EDestinationType.Right, controllingPlayer);
	}

	public bool IsNewPressMenuLeft(PlayerIndex? controllingPlayer)
	{
		return IsNewMenuPress(ButtonMapping.EDestinationType.Left, controllingPlayer);
	}

	public bool IsNewPressPause(PlayerIndex? controllingPlayer)
	{
		return IsNewMenuPress(ButtonMapping.EDestinationType.Pause, controllingPlayer);
	}

	public bool IsNewPressCutsceneSkip(PlayerIndex? controllingPlayer)
	{
		return IsNewMenuPress(ButtonMapping.EDestinationType.Skip, controllingPlayer);
	}

	public bool IsNewPressMap(PlayerIndex? controllingPlayer)
	{
		return IsNewMenuPress(ButtonMapping.EDestinationType.Map, controllingPlayer);
	}

	public bool IsNewPressExit(PlayerIndex? controllingPlayer)
	{
		return IsNewMenuPress(ButtonMapping.EDestinationType.Exit, controllingPlayer);
	}

	private bool IsNewMenuPress(ButtonMapping.EDestinationType destination, PlayerIndex? controllingPlayer)
	{
		bool result = false;
		if (ControllerMapping.Mappings.ContainsKey((int)destination))
		{
			result = IsNewMappingPress(ControllerMapping.Mappings[(int)destination], controllingPlayer);
		}
		return result;
	}

	private bool IsNewMappingPress(ButtonMapping mapping, PlayerIndex? playerIndex)
	{
		PlayerIndex pressedIndex;
		return IsNewMappingPress(mapping, playerIndex, out pressedIndex);
	}

	private bool IsNewMappingPress(ButtonMapping mapping, PlayerIndex? playerIndex, out PlayerIndex pressedIndex)
	{
		if (playerIndex.HasValue)
		{
			pressedIndex = playerIndex.Value;
			int value = (int)playerIndex.Value;
			bool flag = mapping.IsButtonDown(CurrentGamePadStates[value], CurrentKeyboardStates[value]);
			bool flag2 = mapping.IsButtonDown(LastGamePadStates[value], LastKeyboardStates[value]);
			if (flag)
			{
				return !flag2;
			}
			return false;
		}
		if (!IsNewMappingPress(mapping, PlayerIndex.One, out pressedIndex) && !IsNewMappingPress(mapping, PlayerIndex.Two, out pressedIndex) && !IsNewMappingPress(mapping, PlayerIndex.Three, out pressedIndex))
		{
			return IsNewMappingPress(mapping, PlayerIndex.Four, out pressedIndex);
		}
		return true;
	}

	public bool IsNewPressConfirm(PlayerIndex? controllingPlayer, out PlayerIndex pressedPlayer)
	{
		return IsNewMenuPress(ButtonMapping.EDestinationType.Confirm, controllingPlayer, out pressedPlayer);
	}

	public bool IsNewPressCancel(PlayerIndex? controllingPlayer, out PlayerIndex pressedPlayer)
	{
		return IsNewMenuPress(ButtonMapping.EDestinationType.Cancel, controllingPlayer, out pressedPlayer);
	}

	private bool IsNewMenuPress(ButtonMapping.EDestinationType destination, PlayerIndex? controllingPlayer, out PlayerIndex pressedPlayer)
	{
		bool result = false;
		if (ControllerMapping.Mappings.ContainsKey((int)destination))
		{
			result = IsNewMappingPress(ControllerMapping.Mappings[(int)destination], controllingPlayer, out pressedPlayer);
		}
		else
		{
			pressedPlayer = PlayerIndex.One;
		}
		return result;
	}

	public bool WasPressSecondary(PlayerIndex? controllingPlayer)
	{
		return WasMenuPress(ButtonMapping.EDestinationType.Secondary, controllingPlayer);
	}

	public bool WasPressTertiary(PlayerIndex? controllingPlayer)
	{
		return WasMenuPress(ButtonMapping.EDestinationType.Tertiary, controllingPlayer);
	}

	private bool WasMenuPress(ButtonMapping.EDestinationType destination, PlayerIndex? controllingPlayer)
	{
		bool result = false;
		if (ControllerMapping.Mappings.ContainsKey((int)destination))
		{
			result = WasMappingPress(ControllerMapping.Mappings[(int)destination], controllingPlayer);
		}
		return result;
	}

	private bool WasMappingPress(ButtonMapping mapping, PlayerIndex? playerIndex)
	{
		PlayerIndex pressedIndex;
		return WasMappingPress(mapping, playerIndex, out pressedIndex);
	}

	private bool WasMappingPress(ButtonMapping mapping, PlayerIndex? playerIndex, out PlayerIndex pressedIndex)
	{
		if (playerIndex.HasValue)
		{
			pressedIndex = playerIndex.Value;
			int value = (int)playerIndex.Value;
			return mapping.IsButtonDown(LastGamePadStates[value], LastKeyboardStates[value]);
		}
		if (!WasMappingPress(mapping, PlayerIndex.One, out pressedIndex) && !WasMappingPress(mapping, PlayerIndex.Two, out pressedIndex) && !WasMappingPress(mapping, PlayerIndex.Three, out pressedIndex))
		{
			return WasMappingPress(mapping, PlayerIndex.Four, out pressedIndex);
		}
		return true;
	}
}
