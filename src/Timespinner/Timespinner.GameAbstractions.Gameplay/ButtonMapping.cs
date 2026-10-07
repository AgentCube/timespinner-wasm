using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Timespinner.Core;
using Timespinner.Core.Localization;

namespace Timespinner.GameAbstractions.Gameplay;

public class ButtonMapping
{
	public enum EDestinationType
	{
		Jump = 1,
		Melee = 2,
		Spell = 3,
		Time = 4,
		Start = 5,
		Back = 6,
		Dash = 7,
		Backdash = 8,
		Left = 9,
		Up = 10,
		Right = 11,
		Down = 12,
		ToggleLeft = 13,
		ToggleRight = 14,
		Confirm = 32,
		Cancel = 33,
		Secondary = 34,
		Tertiary = 35,
		Pause = 36,
		Map = 37,
		Exit = 38,
		Skip = 39,
		PageLeft = 40,
		PageRight = 41
	}

	private const float DefaultStickSensitivity = 0.05f;

	private const float StickDuckDownThreshold = 0.4f;

	private const float TriggerSensitivityThreshold = 0.75f;

	private const float ToggleStickThreshold = 0.35f;

	private UIButton _uiButton;

	public EDestinationType Destination { get; set; }

	internal UIButton UIButton => _uiButton;

	public List<ButtonMappingSource> Sources { get; set; }

	public ButtonMapping()
	{
		Sources = new List<ButtonMappingSource>();
		RefreshButtonIcon();
	}

	public ButtonMapping Duplicate()
	{
		ButtonMapping buttonMapping = new ButtonMapping();
		buttonMapping.Destination = Destination;
		ButtonMapping buttonMapping2 = buttonMapping;
		foreach (ButtonMappingSource source in Sources)
		{
			buttonMapping2.Sources.Add(new ButtonMappingSource
			{
				SourceType = source.SourceType,
				GamepadButton = source.GamepadButton,
				KeyboardKey = source.KeyboardKey,
				MouseButton = source.MouseButton
			});
		}
		buttonMapping2.RefreshButtonIcon();
		return buttonMapping2;
	}

	internal void RefreshButtonIcon()
	{
		_uiButton = null;
		if (Sources.Count > 0)
		{
			_uiButton = ControllerMapping.CreateUIButtonFromSources(Sources);
		}
	}

	internal void AddSource(Buttons button)
	{
		Sources.Add(new ButtonMappingSource
		{
			SourceType = ButtonMappingSource.ESourceType.GamepadButton,
			GamepadButton = button
		});
		RefreshButtonIcon();
	}

	internal void AddSource(Keys key)
	{
		Sources.Add(new ButtonMappingSource
		{
			SourceType = ButtonMappingSource.ESourceType.Keyboard,
			KeyboardKey = key
		});
		RefreshButtonIcon();
	}

	internal void AddSource(MouseState button)
	{
		Sources.Add(new ButtonMappingSource
		{
			SourceType = ButtonMappingSource.ESourceType.Mouse,
			MouseButton = button
		});
		RefreshButtonIcon();
	}

	public string GetDestinationText(EControllerMappingType mappingType)
	{
		string text = Destination.ToString();
		if (mappingType == EControllerMappingType.Familiar && (Destination == EDestinationType.Dash || Destination == EDestinationType.Backdash))
		{
			text += "Familiar";
		}
		return Loc.Get("ControlsButtonMapping" + text);
	}

	public string GetSourceText()
	{
		if (Sources.Count <= 0)
		{
			return "--";
		}
		return Sources[0].GetSourceText();
	}

	public void SetNewButton(Buttons button)
	{
		Sources.Clear();
		AddSource(button);
	}

	public void SetNewButton(Keys button)
	{
		Sources.Clear();
		AddSource(button);
	}

	public bool IsButtonDown(GamePadState gamePadState, KeyboardState keyboardState)
	{
		bool flag = false;
		foreach (ButtonMappingSource source in Sources)
		{
			if (source.SourceType == ButtonMappingSource.ESourceType.GamepadButton)
			{
				if (!gamePadState.IsButtonDown(source.GamepadButton))
				{
					continue;
				}
				flag = true;
				Buttons gamepadButton = source.GamepadButton;
				switch (gamepadButton)
				{
				case Buttons.RightTrigger:
				case Buttons.LeftTrigger:
				{
					float num = ((source.GamepadButton == Buttons.LeftTrigger) ? gamePadState.Triggers.Left : gamePadState.Triggers.Right);
					flag = num > 0.75f;
					break;
				}
				case Buttons.LeftThumbstickLeft:
				case Buttons.RightThumbstickUp:
				case Buttons.RightThumbstickDown:
				case Buttons.RightThumbstickRight:
				case Buttons.RightThumbstickLeft:
				case Buttons.LeftThumbstickUp:
				case Buttons.LeftThumbstickDown:
				case Buttons.LeftThumbstickRight:
				{
					bool flag2 = gamepadButton == Buttons.LeftThumbstickDown || gamepadButton == Buttons.LeftThumbstickLeft || gamepadButton == Buttons.LeftThumbstickRight || gamepadButton == Buttons.LeftThumbstickUp;
					bool flag3 = gamepadButton == Buttons.LeftThumbstickDown || gamepadButton == Buttons.LeftThumbstickUp || gamepadButton == Buttons.RightThumbstickDown || gamepadButton == Buttons.RightThumbstickUp;
					bool flag4 = gamepadButton == Buttons.LeftThumbstickRight || gamepadButton == Buttons.LeftThumbstickUp || gamepadButton == Buttons.RightThumbstickRight || gamepadButton == Buttons.RightThumbstickUp;
					Vector2 vector = (flag2 ? gamePadState.ThumbSticks.Left : gamePadState.ThumbSticks.Right);
					float num = (flag3 ? vector.Y : vector.X);
					float num2 = 0.05f;
					switch (Destination)
					{
					case EDestinationType.Down:
						num2 = 0.4f;
						break;
					case EDestinationType.ToggleLeft:
					case EDestinationType.ToggleRight:
						num2 = 0.35f;
						break;
					}
					flag = (flag4 ? (num > num2) : (num < 0f - num2));
					break;
				}
				}
				if (flag)
				{
					break;
				}
			}
			else if (source.SourceType == ButtonMappingSource.ESourceType.Keyboard && keyboardState.IsKeyDown(source.KeyboardKey))
			{
				flag = true;
				break;
			}
		}
		return flag;
	}

	internal static bool IsEqual(ButtonMappingSource sourceA, ButtonMappingSource sourceB)
	{
		bool result = false;
		if (sourceA != null && sourceB != null && sourceA.SourceType == sourceB.SourceType)
		{
			switch (sourceA.SourceType)
			{
			case ButtonMappingSource.ESourceType.None:
				result = true;
				break;
			case ButtonMappingSource.ESourceType.GamepadButton:
				result = sourceA.GamepadButton == sourceB.GamepadButton;
				break;
			case ButtonMappingSource.ESourceType.Keyboard:
				result = sourceA.KeyboardKey == sourceB.KeyboardKey;
				break;
			case ButtonMappingSource.ESourceType.Mouse:
				result = sourceA.MouseButton == sourceB.MouseButton;
				break;
			}
		}
		return result;
	}
}
