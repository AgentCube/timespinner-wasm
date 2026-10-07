using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Timespinner.Core.Constants;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.Core;

internal class UIButton
{
	internal enum EControllerButton
	{
		None,
		A,
		B,
		X,
		Y,
		Start,
		Back,
		Rb,
		Lb,
		AnimatedUp,
		Up,
		Down,
		Left,
		Right,
		AnimatedUpAndDown,
		AnimatedDown,
		R2,
		L2,
		R3,
		L3,
		LeftStickUp,
		LeftStickDown,
		LeftStickLeft,
		LeftStickRight,
		RightStickUp,
		RightStickDown,
		RightStickLeft,
		RightStickRight,
		BigButton
	}

	internal enum EButtonDisplayType
	{
		Xbox360,
		PS4,
		PSVita,
		XboxOne,
		Switch
	}

	private const int Anim_ButtonXboxAIndex = 0;

	private const int Anim_ButtonXboxBIndex = 1;

	private const int Anim_ButtonXboxXIndex = 2;

	private const int Anim_ButtonXboxYIndex = 3;

	private const int Anim_ButtonSonyXIndex = 18;

	private const int Anim_ButtonSonyOIndex = 19;

	private const int Anim_ButtonSonySquareIndex = 20;

	private const int Anim_ButtonSonyTriangleIndex = 21;

	private const int Anim_ButtonXboxOneAIndex = 46;

	private const int Anim_ButtonXboxOneBIndex = 47;

	private const int Anim_ButtonXboxOneXIndex = 48;

	private const int Anim_ButtonXboxOneYIndex = 49;

	private const int Anim_ButtonSwitchBIndex = 63;

	private const int Anim_ButtonSwitchAIndex = 64;

	private const int Anim_ButtonSwitchYIndex = 65;

	private const int Anim_ButtonSwitchXIndex = 66;

	private const int Anim_ButtonXboxL1Index = 6;

	private const int Anim_ButtonXboxR1Index = 7;

	private const int Anim_ButtonPS4L1Index = 24;

	private const int Anim_ButtonPS4R1Index = 25;

	private const int Anim_ButtonPSVitaL1Index = 27;

	private const int Anim_ButtonPSVitaR1Index = 28;

	private const int Anim_ButtonXboxOneL1Index = 54;

	private const int Anim_ButtonXboxOneR1Index = 55;

	private const int Anim_ButtonSwitchL1Index = 71;

	private const int Anim_ButtonSwitchR1Index = 72;

	private const int Anim_ButtonXboxL2Index = 31;

	private const int Anim_ButtonXboxR2Index = 32;

	private const int Anim_ButtonPS4L2Index = 22;

	private const int Anim_ButtonPS4R2Index = 23;

	private const int Anim_ButtonXboxOneL2Index = 50;

	private const int Anim_ButtonXboxOneR2Index = 51;

	private const int Anim_ButtonSwitchL2Index = 67;

	private const int Anim_ButtonSwitchR2Index = 68;

	private const int Anim_ButtonL3Index = 33;

	private const int Anim_ButtonR3Index = 34;

	private const int Anim_ButtonSwitchL3Index = 69;

	private const int Anim_ButtonSwitchR3Index = 70;

	private const int Anim_ButtonXboxStartIndex = 12;

	private const int Anim_ButtonXboxBackStartIndex = 13;

	private const int Anim_ButtonPS4StartIndex = 45;

	private const int Anim_ButtonPSVitaStartIndex = 29;

	private const int Anim_ButtonPSVitaSelectIndex = 30;

	private const int Anim_ButtonXboxOneStartIndex = 57;

	private const int Anim_ButtonXboxOneSelectIndex = 56;

	private const int Anim_ButtonSwitchStartIndex = 74;

	private const int Anim_ButtonSwitchSelectIndex = 73;

	private const int Anim_ButtonLeftStickUp = 35;

	private const int Anim_ButtonLeftStickDown = 36;

	private const int Anim_ButtonLeftStickRight = 37;

	private const int Anim_ButtonLeftStickLeft = 38;

	private const int Anim_ButtonRightStickUp = 39;

	private const int Anim_ButtonRightStickDown = 40;

	private const int Anim_ButtonRightStickRight = 41;

	private const int Anim_ButtonRightStickLeft = 42;

	private const int Anim_ButtonXboxBigButton = 44;

	private const int Anim_ButtonSonyBigButton = 43;

	private const int Anim_ButtonXboxOneBigButton = 58;

	private const int Anim_ButtonSwitchBigButton = 75;

	private const int Anim_AnimatedDirectionUpIndex = 8;

	private const int Anim_AnimatedDirectionDownIndex = 9;

	private const int Anim_AnimatedXboxOneDirectionUpIndex = 59;

	private const int Anim_AnimatedXboxOneDirectionDownIndex = 60;

	private const int Anim_AnimatedSwitchDirectionUpIndex = 76;

	private const int Anim_AnimatedSwitchDirectionDownIndex = 77;

	private const int Anim_KeyboardKeySmall = 80;

	private const int Anim_KeyboardKeyWide = 81;

	private const int LatinKeyTextOffsetSmallX = 4;

	private const int LatinKeyTextOffsetWideX = 3;

	private const int AsianKeyTextOffsetSmallX = 4;

	private const int AsianKeyTextOffsetWideX = 2;

	private const int KeyTextOffsetY = -2;

	private const float AnimationSpeed = 20f;

	private static readonly Color KeyShadowColor = new Color(16, 16, 16);

	private static readonly Color KeyDrawColor = new Color(248, 240, 232);

	private readonly bool _isControllerButton;

	private readonly EControllerButton _button;

	private readonly int _textOffsetX;

	private readonly string _keyText;

	private bool _isAnimated;

	private int _animationStart;

	private int _animationLength;

	private int _animationIndex;

	private int _animationCounter;

	private Rectangle _frameSource;

	internal bool IsAnimated { get; set; }

	internal bool IsWide { get; private set; }

	internal EControllerButton Button => _button;

	internal int IndexInLine { get; set; }

	internal int BaseOffsetX { get; set; }

	internal int AnimationStart => _animationStart;

	internal UIButton(EControllerButton button)
	{
		_isControllerButton = true;
		_button = button;
		SetAnimationBasedOnButton(_button);
	}

	public UIButton(Buttons gamepadButton)
	{
		_isControllerButton = true;
		_button = GamepadButtonToButton(gamepadButton);
		SetAnimationBasedOnButton(_button);
	}

	public UIButton(Keys key)
	{
		_isControllerButton = false;
		_keyText = TrunctateKeyText(key);
		IsWide = _keyText.Length > 1;
		_animationStart = (IsWide ? 81 : 80);
		_textOffsetX = ((!IsWide) ? (Loc.IsAsianLocale ? 4 : 4) : (Loc.IsAsianLocale ? 2 : 3));
	}

	internal void RefreshAnimation()
	{
		_frameSource = Rectangle.Empty;
		SetAnimationBasedOnButton(_button);
	}

	private void SetAnimationBasedOnButton(EControllerButton button)
	{
		EButtonDisplayType buttonDisplayType = (EButtonDisplayType)Timespinner.Core.Constants.Constants.ButtonDisplayType;
		switch (button)
		{
		case EControllerButton.A:
			switch (buttonDisplayType)
			{
			case EButtonDisplayType.PS4:
			case EButtonDisplayType.PSVita:
				_animationStart = 18;
				break;
			case EButtonDisplayType.XboxOne:
				_animationStart = 46;
				break;
			case EButtonDisplayType.Switch:
				_animationStart = 63;
				break;
			default:
				_animationStart = 0;
				break;
			}
			break;
		case EControllerButton.B:
			switch (buttonDisplayType)
			{
			case EButtonDisplayType.PS4:
			case EButtonDisplayType.PSVita:
				_animationStart = 19;
				break;
			case EButtonDisplayType.XboxOne:
				_animationStart = 47;
				break;
			case EButtonDisplayType.Switch:
				_animationStart = 64;
				break;
			default:
				_animationStart = 1;
				break;
			}
			break;
		case EControllerButton.X:
			switch (buttonDisplayType)
			{
			case EButtonDisplayType.PS4:
			case EButtonDisplayType.PSVita:
				_animationStart = 20;
				break;
			case EButtonDisplayType.XboxOne:
				_animationStart = 48;
				break;
			case EButtonDisplayType.Switch:
				_animationStart = 65;
				break;
			default:
				_animationStart = 2;
				break;
			}
			break;
		case EControllerButton.Y:
			switch (buttonDisplayType)
			{
			case EButtonDisplayType.PS4:
			case EButtonDisplayType.PSVita:
				_animationStart = 21;
				break;
			case EButtonDisplayType.XboxOne:
				_animationStart = 49;
				break;
			case EButtonDisplayType.Switch:
				_animationStart = 66;
				break;
			default:
				_animationStart = 3;
				break;
			}
			break;
		case EControllerButton.AnimatedUp:
			_isAnimated = true;
			_animationLength = 2;
			switch (buttonDisplayType)
			{
			case EButtonDisplayType.XboxOne:
				_animationStart = 59;
				break;
			case EButtonDisplayType.Switch:
				_animationStart = 76;
				break;
			default:
				_animationStart = 8;
				break;
			}
			break;
		case EControllerButton.AnimatedUpAndDown:
			_isAnimated = true;
			_animationLength = 4;
			switch (buttonDisplayType)
			{
			case EButtonDisplayType.XboxOne:
				_animationStart = 59;
				break;
			case EButtonDisplayType.Switch:
				_animationStart = 76;
				break;
			default:
				_animationStart = 8;
				break;
			}
			break;
		case EControllerButton.AnimatedDown:
			_isAnimated = true;
			_animationLength = 2;
			switch (buttonDisplayType)
			{
			case EButtonDisplayType.XboxOne:
				_animationStart = 60;
				break;
			case EButtonDisplayType.Switch:
				_animationStart = 77;
				break;
			default:
				_animationStart = 9;
				break;
			}
			break;
		case EControllerButton.Start:
			switch (buttonDisplayType)
			{
			case EButtonDisplayType.PS4:
				_animationStart = 45;
				IsWide = true;
				break;
			case EButtonDisplayType.PSVita:
				_animationStart = 29;
				IsWide = true;
				break;
			case EButtonDisplayType.XboxOne:
				_animationStart = 57;
				break;
			case EButtonDisplayType.Switch:
				_animationStart = 74;
				break;
			default:
				_animationStart = 12;
				break;
			}
			break;
		case EControllerButton.Back:
			switch (buttonDisplayType)
			{
			case EButtonDisplayType.PS4:
			case EButtonDisplayType.PSVita:
				_animationStart = 30;
				IsWide = true;
				break;
			case EButtonDisplayType.XboxOne:
				_animationStart = 56;
				break;
			case EButtonDisplayType.Switch:
				_animationStart = 73;
				break;
			default:
				_animationStart = 13;
				break;
			}
			break;
		case EControllerButton.Lb:
			switch (buttonDisplayType)
			{
			case EButtonDisplayType.PS4:
				_animationStart = 24;
				break;
			case EButtonDisplayType.PSVita:
				_animationStart = 27;
				break;
			case EButtonDisplayType.XboxOne:
				_animationStart = 54;
				break;
			case EButtonDisplayType.Switch:
				_animationStart = 71;
				break;
			default:
				_animationStart = 6;
				break;
			}
			break;
		case EControllerButton.Rb:
			switch (buttonDisplayType)
			{
			case EButtonDisplayType.PS4:
				_animationStart = 25;
				break;
			case EButtonDisplayType.PSVita:
				_animationStart = 28;
				break;
			case EButtonDisplayType.XboxOne:
				_animationStart = 55;
				break;
			case EButtonDisplayType.Switch:
				_animationStart = 72;
				break;
			default:
				_animationStart = 7;
				break;
			}
			break;
		case EControllerButton.Up:
			_animationStart = 14;
			break;
		case EControllerButton.Down:
			_animationStart = 15;
			break;
		case EControllerButton.Right:
			_animationStart = 16;
			break;
		case EControllerButton.Left:
			_animationStart = 17;
			break;
		case EControllerButton.L2:
			switch (buttonDisplayType)
			{
			case EButtonDisplayType.PS4:
			case EButtonDisplayType.PSVita:
				_animationStart = 22;
				break;
			case EButtonDisplayType.XboxOne:
				_animationStart = 50;
				break;
			case EButtonDisplayType.Switch:
				_animationStart = 67;
				break;
			default:
				_animationStart = 31;
				break;
			}
			break;
		case EControllerButton.R2:
			switch (buttonDisplayType)
			{
			case EButtonDisplayType.PS4:
			case EButtonDisplayType.PSVita:
				_animationStart = 23;
				break;
			case EButtonDisplayType.XboxOne:
				_animationStart = 51;
				break;
			case EButtonDisplayType.Switch:
				_animationStart = 68;
				break;
			default:
				_animationStart = 32;
				break;
			}
			break;
		case EControllerButton.L3:
		{
			EButtonDisplayType eButtonDisplayType2 = buttonDisplayType;
			if (eButtonDisplayType2 == EButtonDisplayType.Switch)
			{
				_animationStart = 69;
			}
			else
			{
				_animationStart = 33;
			}
			break;
		}
		case EControllerButton.R3:
		{
			EButtonDisplayType eButtonDisplayType = buttonDisplayType;
			if (eButtonDisplayType == EButtonDisplayType.Switch)
			{
				_animationStart = 70;
			}
			else
			{
				_animationStart = 34;
			}
			break;
		}
		case EControllerButton.LeftStickUp:
			_animationStart = 35;
			break;
		case EControllerButton.LeftStickDown:
			_animationStart = 36;
			break;
		case EControllerButton.LeftStickRight:
			_animationStart = 37;
			break;
		case EControllerButton.LeftStickLeft:
			_animationStart = 38;
			break;
		case EControllerButton.RightStickUp:
			_animationStart = 39;
			break;
		case EControllerButton.RightStickDown:
			_animationStart = 40;
			break;
		case EControllerButton.RightStickRight:
			_animationStart = 41;
			break;
		case EControllerButton.RightStickLeft:
			_animationStart = 42;
			break;
		case EControllerButton.BigButton:
			switch (buttonDisplayType)
			{
			case EButtonDisplayType.PS4:
			case EButtonDisplayType.PSVita:
				_animationStart = 43;
				IsWide = true;
				break;
			case EButtonDisplayType.XboxOne:
				_animationStart = 58;
				break;
			case EButtonDisplayType.Switch:
				_animationStart = 75;
				break;
			default:
				_animationStart = 44;
				break;
			}
			break;
		}
	}

	internal void Draw(SpriteBatch spriteBatch, SpriteSheet sprite, SpriteFont font, Vector2 position, Color color, int zoom, int visibleCharacters)
	{
		int animationIndex = _animationIndex;
		if (_isAnimated)
		{
			_animationCounter++;
			if ((float)_animationCounter >= 20f)
			{
				_animationCounter = 0;
				_animationIndex = (_animationIndex + 1) % _animationLength;
			}
		}
		if (visibleCharacters == -1 || visibleCharacters >= IndexInLine)
		{
			if (_frameSource == Rectangle.Empty || animationIndex != _animationIndex)
			{
				_frameSource = sprite.GetFrameSource(_animationStart + _animationIndex);
			}
			spriteBatch.Draw(sprite.Texture, position.Add(new Point(BaseOffsetX * zoom, 0)), _frameSource, color, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
			if (!_isControllerButton && font != null && _keyText != null)
			{
				int x = (BaseOffsetX + _textOffsetX) * zoom;
				int y = -2 * zoom;
				Vector2 vector = position.Add(new Point(x, y));
				float num = (float)(int)color.A / 256f;
				DrawingEx.DrawString(spriteBatch, font, _keyText, vector.Add(0f, zoom), KeyShadowColor * num, Vector2.Zero, zoom);
				DrawingEx.DrawString(spriteBatch, font, _keyText, vector, KeyDrawColor * num, Vector2.Zero, zoom);
			}
		}
	}

	internal static UIButton CreateFromCharacter(char character, ControllerMapping controllerMapping)
	{
		bool flag = controllerMapping?.IsMenuMapping ?? false;
		ButtonMapping.EDestinationType destination = ButtonMapping.EDestinationType.Jump;
		EControllerButton button = EControllerButton.None;
		bool flag2 = false;
		switch (character)
		{
		case 'X':
			destination = (flag ? ButtonMapping.EDestinationType.Secondary : ButtonMapping.EDestinationType.Melee);
			button = EControllerButton.X;
			break;
		case 'Y':
			destination = (flag ? ButtonMapping.EDestinationType.Tertiary : ButtonMapping.EDestinationType.Time);
			button = EControllerButton.Y;
			break;
		case 'B':
			destination = (flag ? ButtonMapping.EDestinationType.Cancel : ButtonMapping.EDestinationType.Spell);
			button = EControllerButton.B;
			break;
		case 'A':
			destination = ((!flag) ? ButtonMapping.EDestinationType.Jump : ButtonMapping.EDestinationType.Confirm);
			button = EControllerButton.A;
			break;
		case 'U':
			flag2 = true;
			button = EControllerButton.AnimatedUp;
			break;
		case 'O':
			flag2 = true;
			button = EControllerButton.AnimatedDown;
			break;
		case 'V':
			flag2 = true;
			button = EControllerButton.AnimatedUpAndDown;
			break;
		case 'S':
			destination = (flag ? ButtonMapping.EDestinationType.Pause : ButtonMapping.EDestinationType.Start);
			button = EControllerButton.Start;
			break;
		case 'K':
			destination = (flag ? ButtonMapping.EDestinationType.Map : ButtonMapping.EDestinationType.ToggleRight);
			button = EControllerButton.Back;
			break;
		case 'R':
			destination = (flag ? ButtonMapping.EDestinationType.PageRight : ButtonMapping.EDestinationType.Dash);
			button = EControllerButton.Rb;
			break;
		case 'L':
			destination = (flag ? ButtonMapping.EDestinationType.PageLeft : ButtonMapping.EDestinationType.Backdash);
			button = EControllerButton.Lb;
			break;
		case 'P':
			destination = ButtonMapping.EDestinationType.Up;
			button = EControllerButton.Up;
			break;
		case 'D':
			destination = ButtonMapping.EDestinationType.Down;
			button = EControllerButton.Down;
			break;
		case 'E':
			destination = ButtonMapping.EDestinationType.Left;
			button = EControllerButton.Left;
			break;
		case 'I':
			destination = ButtonMapping.EDestinationType.Right;
			button = EControllerButton.Right;
			break;
		case 'F':
			destination = (flag ? ButtonMapping.EDestinationType.PageLeft : ButtonMapping.EDestinationType.ToggleLeft);
			button = EControllerButton.L2;
			break;
		case 'G':
			destination = (flag ? ButtonMapping.EDestinationType.PageRight : ButtonMapping.EDestinationType.ToggleRight);
			button = EControllerButton.R2;
			break;
		}
		if (flag2 || controllerMapping == null)
		{
			return new UIButton(button);
		}
		return controllerMapping.CreateUIButtonFromDestination(destination) ?? new UIButton(button);
	}

	internal static EControllerButton GamepadButtonToButton(Buttons gamepadButton)
	{
		EControllerButton result = EControllerButton.None;
		switch (gamepadButton)
		{
		case Buttons.A:
			result = EControllerButton.A;
			break;
		case Buttons.B:
			result = EControllerButton.B;
			break;
		case Buttons.X:
			result = EControllerButton.X;
			break;
		case Buttons.Y:
			result = EControllerButton.Y;
			break;
		case Buttons.LeftShoulder:
			result = EControllerButton.Lb;
			break;
		case Buttons.RightShoulder:
			result = EControllerButton.Rb;
			break;
		case Buttons.LeftTrigger:
			result = EControllerButton.L2;
			break;
		case Buttons.RightTrigger:
			result = EControllerButton.R2;
			break;
		case Buttons.LeftStick:
			result = EControllerButton.L3;
			break;
		case Buttons.RightStick:
			result = EControllerButton.R3;
			break;
		case Buttons.Start:
			result = EControllerButton.Start;
			break;
		case Buttons.Back:
			result = EControllerButton.Back;
			break;
		case Buttons.DPadUp:
			result = EControllerButton.Up;
			break;
		case Buttons.DPadDown:
			result = EControllerButton.Down;
			break;
		case Buttons.DPadRight:
			result = EControllerButton.Right;
			break;
		case Buttons.DPadLeft:
			result = EControllerButton.Left;
			break;
		case Buttons.LeftThumbstickUp:
			result = EControllerButton.LeftStickUp;
			break;
		case Buttons.LeftThumbstickDown:
			result = EControllerButton.LeftStickDown;
			break;
		case Buttons.LeftThumbstickRight:
			result = EControllerButton.LeftStickRight;
			break;
		case Buttons.LeftThumbstickLeft:
			result = EControllerButton.LeftStickLeft;
			break;
		case Buttons.RightThumbstickUp:
			result = EControllerButton.RightStickUp;
			break;
		case Buttons.RightThumbstickDown:
			result = EControllerButton.RightStickDown;
			break;
		case Buttons.RightThumbstickRight:
			result = EControllerButton.RightStickRight;
			break;
		case Buttons.RightThumbstickLeft:
			result = EControllerButton.RightStickLeft;
			break;
		case Buttons.BigButton:
			result = EControllerButton.BigButton;
			break;
		}
		return result;
	}

	private static string TrunctateKeyText(Keys key)
	{
		string text = key.ToString();
		if (text.Length > 3)
		{
			if (text != "Left" && text.StartsWith("Left"))
			{
				text = text.Replace("Left", "L");
			}
			if (text != "Right" && text.StartsWith("Right"))
			{
				text = text.Replace("Right", "R");
			}
			if (text.StartsWith("NumPad"))
			{
				text = text.Replace("NumPad", "N");
			}
			text = text.Replace("a", string.Empty).Replace("e", string.Empty).Replace("i", string.Empty)
				.Replace("o", string.Empty)
				.Replace("u", string.Empty)
				.Replace("y", string.Empty);
			if (text.Length > 3)
			{
				text = text.SafeSubstring(0, 3);
			}
		}
		return text;
	}
}
