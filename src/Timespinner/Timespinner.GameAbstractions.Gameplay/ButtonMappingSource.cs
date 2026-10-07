using Microsoft.Xna.Framework.Input;

namespace Timespinner.GameAbstractions.Gameplay;

public class ButtonMappingSource
{
	public enum ESourceType
	{
		None,
		GamepadButton,
		Keyboard,
		Mouse
	}

	public ESourceType SourceType { get; set; }

	public Keys KeyboardKey { get; set; }

	public Buttons GamepadButton { get; set; }

	public MouseState MouseButton { get; set; }

	public string GetSourceText()
	{
		string result = "";
		switch (SourceType)
		{
		case ESourceType.GamepadButton:
			result = GamepadButton.ToString();
			break;
		case ESourceType.Keyboard:
			result = KeyboardKey.ToString();
			break;
		case ESourceType.Mouse:
			result = MouseButton.ToString();
			break;
		}
		return result;
	}
}
