using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Timespinner.Core;
using Timespinner.Core.Constants;

namespace Timespinner.GameAbstractions.Gameplay;

public class ControllerMapping
{
	public bool DoesUseControllerRumble { get; set; }

	public bool IsMenuMapping { get; set; }

	public Dictionary<int, ButtonMapping> Mappings { get; set; }

	public static ControllerMapping DefaultMapping
	{
		get
		{
			ControllerMapping controllerMapping = new ControllerMapping();
			controllerMapping.Mappings = new Dictionary<int, ButtonMapping>();
			ControllerMapping controllerMapping2 = controllerMapping;
			ButtonMapping buttonMapping = new ButtonMapping();
			buttonMapping.Destination = ButtonMapping.EDestinationType.Jump;
			ButtonMapping buttonMapping2 = buttonMapping;
			ButtonMapping buttonMapping3 = new ButtonMapping();
			buttonMapping3.Destination = ButtonMapping.EDestinationType.Spell;
			ButtonMapping buttonMapping4 = buttonMapping3;
			ButtonMapping buttonMapping5 = new ButtonMapping();
			buttonMapping5.Destination = ButtonMapping.EDestinationType.Melee;
			ButtonMapping buttonMapping6 = buttonMapping5;
			ButtonMapping buttonMapping7 = new ButtonMapping();
			buttonMapping7.Destination = ButtonMapping.EDestinationType.Time;
			ButtonMapping buttonMapping8 = buttonMapping7;
			ButtonMapping buttonMapping9 = new ButtonMapping();
			buttonMapping9.Destination = ButtonMapping.EDestinationType.Dash;
			ButtonMapping buttonMapping10 = buttonMapping9;
			ButtonMapping buttonMapping11 = new ButtonMapping();
			buttonMapping11.Destination = ButtonMapping.EDestinationType.Backdash;
			ButtonMapping buttonMapping12 = buttonMapping11;
			ButtonMapping buttonMapping13 = new ButtonMapping();
			buttonMapping13.Destination = ButtonMapping.EDestinationType.ToggleLeft;
			ButtonMapping buttonMapping14 = buttonMapping13;
			ButtonMapping buttonMapping15 = new ButtonMapping();
			buttonMapping15.Destination = ButtonMapping.EDestinationType.ToggleRight;
			ButtonMapping buttonMapping16 = buttonMapping15;
			ButtonMapping buttonMapping17 = new ButtonMapping();
			buttonMapping17.Destination = ButtonMapping.EDestinationType.Up;
			ButtonMapping buttonMapping18 = buttonMapping17;
			ButtonMapping buttonMapping19 = new ButtonMapping();
			buttonMapping19.Destination = ButtonMapping.EDestinationType.Right;
			ButtonMapping buttonMapping20 = buttonMapping19;
			ButtonMapping buttonMapping21 = new ButtonMapping();
			buttonMapping21.Destination = ButtonMapping.EDestinationType.Down;
			ButtonMapping buttonMapping22 = buttonMapping21;
			ButtonMapping buttonMapping23 = new ButtonMapping();
			buttonMapping23.Destination = ButtonMapping.EDestinationType.Left;
			ButtonMapping buttonMapping24 = buttonMapping23;
			ButtonMapping buttonMapping25 = new ButtonMapping();
			buttonMapping25.Destination = ButtonMapping.EDestinationType.Start;
			ButtonMapping buttonMapping26 = buttonMapping25;
			ButtonMapping buttonMapping27 = new ButtonMapping();
			buttonMapping27.Destination = ButtonMapping.EDestinationType.Back;
			ButtonMapping buttonMapping28 = buttonMapping27;
			buttonMapping2.AddSource(Buttons.A);
			buttonMapping4.AddSource(Buttons.B);
			buttonMapping6.AddSource(Buttons.X);
			buttonMapping8.AddSource(Buttons.Y);
			buttonMapping10.AddSource(Buttons.RightShoulder);
			buttonMapping12.AddSource(Buttons.LeftShoulder);
			buttonMapping18.AddSource(Buttons.DPadUp);
			buttonMapping18.AddSource(Buttons.LeftThumbstickUp);
			buttonMapping20.AddSource(Buttons.DPadRight);
			buttonMapping20.AddSource(Buttons.LeftThumbstickRight);
			buttonMapping22.AddSource(Buttons.DPadDown);
			buttonMapping22.AddSource(Buttons.LeftThumbstickDown);
			buttonMapping24.AddSource(Buttons.DPadLeft);
			buttonMapping24.AddSource(Buttons.LeftThumbstickLeft);
			buttonMapping26.AddSource(Buttons.Start);
			buttonMapping28.AddSource(Buttons.Back);
			buttonMapping14.AddSource(Buttons.LeftTrigger);
			buttonMapping16.AddSource(Buttons.RightTrigger);
			buttonMapping2.AddSource(Keys.Space);
			buttonMapping4.AddSource(Keys.W);
			buttonMapping6.AddSource(Keys.Q);
			buttonMapping8.AddSource(Keys.E);
			buttonMapping18.AddSource(Keys.Up);
			buttonMapping20.AddSource(Keys.Right);
			buttonMapping24.AddSource(Keys.Left);
			buttonMapping22.AddSource(Keys.Down);
			buttonMapping10.AddSource(Keys.R);
			buttonMapping12.AddSource(Keys.D);
			buttonMapping16.AddSource(Keys.Tab);
			buttonMapping14.AddSource(Keys.LeftShift);
			buttonMapping26.AddSource(Keys.Enter);
			buttonMapping28.AddSource(Keys.Escape);
			controllerMapping2.Mappings.Add(1, buttonMapping2);
			controllerMapping2.Mappings.Add(2, buttonMapping6);
			controllerMapping2.Mappings.Add(3, buttonMapping4);
			controllerMapping2.Mappings.Add(4, buttonMapping8);
			controllerMapping2.Mappings.Add(8, buttonMapping12);
			controllerMapping2.Mappings.Add(7, buttonMapping10);
			controllerMapping2.Mappings.Add(13, buttonMapping14);
			controllerMapping2.Mappings.Add(14, buttonMapping16);
			controllerMapping2.Mappings.Add(10, buttonMapping18);
			controllerMapping2.Mappings.Add(12, buttonMapping22);
			controllerMapping2.Mappings.Add(9, buttonMapping24);
			controllerMapping2.Mappings.Add(11, buttonMapping20);
			controllerMapping2.Mappings.Add(5, buttonMapping26);
			controllerMapping2.Mappings.Add(6, buttonMapping28);
			return controllerMapping2;
		}
	}

	public static ControllerMapping DefaultFamiliarMapping
	{
		get
		{
			ControllerMapping controllerMapping = new ControllerMapping();
			controllerMapping.Mappings = new Dictionary<int, ButtonMapping>();
			ControllerMapping controllerMapping2 = controllerMapping;
			ButtonMapping buttonMapping = new ButtonMapping();
			buttonMapping.Destination = ButtonMapping.EDestinationType.Spell;
			ButtonMapping buttonMapping2 = buttonMapping;
			ButtonMapping buttonMapping3 = new ButtonMapping();
			buttonMapping3.Destination = ButtonMapping.EDestinationType.Melee;
			ButtonMapping buttonMapping4 = buttonMapping3;
			ButtonMapping buttonMapping5 = new ButtonMapping();
			buttonMapping5.Destination = ButtonMapping.EDestinationType.Up;
			ButtonMapping buttonMapping6 = buttonMapping5;
			ButtonMapping buttonMapping7 = new ButtonMapping();
			buttonMapping7.Destination = ButtonMapping.EDestinationType.Right;
			ButtonMapping buttonMapping8 = buttonMapping7;
			ButtonMapping buttonMapping9 = new ButtonMapping();
			buttonMapping9.Destination = ButtonMapping.EDestinationType.Down;
			ButtonMapping buttonMapping10 = buttonMapping9;
			ButtonMapping buttonMapping11 = new ButtonMapping();
			buttonMapping11.Destination = ButtonMapping.EDestinationType.Left;
			ButtonMapping buttonMapping12 = buttonMapping11;
			ButtonMapping buttonMapping13 = new ButtonMapping();
			buttonMapping13.Destination = ButtonMapping.EDestinationType.Dash;
			ButtonMapping buttonMapping14 = buttonMapping13;
			ButtonMapping buttonMapping15 = new ButtonMapping();
			buttonMapping15.Destination = ButtonMapping.EDestinationType.Backdash;
			ButtonMapping buttonMapping16 = buttonMapping15;
			ButtonMapping buttonMapping17 = new ButtonMapping();
			buttonMapping17.Destination = ButtonMapping.EDestinationType.Start;
			ButtonMapping buttonMapping18 = buttonMapping17;
			buttonMapping2.AddSource(Buttons.B);
			buttonMapping4.AddSource(Buttons.X);
			buttonMapping6.AddSource(Buttons.DPadUp);
			buttonMapping6.AddSource(Buttons.LeftThumbstickUp);
			buttonMapping8.AddSource(Buttons.DPadRight);
			buttonMapping8.AddSource(Buttons.LeftThumbstickRight);
			buttonMapping10.AddSource(Buttons.DPadDown);
			buttonMapping10.AddSource(Buttons.LeftThumbstickDown);
			buttonMapping12.AddSource(Buttons.DPadLeft);
			buttonMapping12.AddSource(Buttons.LeftThumbstickLeft);
			buttonMapping14.AddSource(Buttons.RightShoulder);
			buttonMapping16.AddSource(Buttons.LeftShoulder);
			buttonMapping18.AddSource(Buttons.Start);
			controllerMapping2.Mappings.Add(2, buttonMapping4);
			controllerMapping2.Mappings.Add(3, buttonMapping2);
			controllerMapping2.Mappings.Add(10, buttonMapping6);
			controllerMapping2.Mappings.Add(12, buttonMapping10);
			controllerMapping2.Mappings.Add(9, buttonMapping12);
			controllerMapping2.Mappings.Add(11, buttonMapping8);
			controllerMapping2.Mappings.Add(8, buttonMapping16);
			controllerMapping2.Mappings.Add(7, buttonMapping14);
			controllerMapping2.Mappings.Add(5, buttonMapping18);
			return controllerMapping2;
		}
	}

	public static ControllerMapping DefaultMenuMapping
	{
		get
		{
			ControllerMapping controllerMapping = new ControllerMapping();
			controllerMapping.Mappings = new Dictionary<int, ButtonMapping>();
			controllerMapping.IsMenuMapping = true;
			ControllerMapping controllerMapping2 = controllerMapping;
			ButtonMapping buttonMapping = new ButtonMapping();
			buttonMapping.Destination = ButtonMapping.EDestinationType.Confirm;
			ButtonMapping buttonMapping2 = buttonMapping;
			ButtonMapping buttonMapping3 = new ButtonMapping();
			buttonMapping3.Destination = ButtonMapping.EDestinationType.Cancel;
			ButtonMapping buttonMapping4 = buttonMapping3;
			ButtonMapping buttonMapping5 = new ButtonMapping();
			buttonMapping5.Destination = ButtonMapping.EDestinationType.Secondary;
			ButtonMapping buttonMapping6 = buttonMapping5;
			ButtonMapping buttonMapping7 = new ButtonMapping();
			buttonMapping7.Destination = ButtonMapping.EDestinationType.Tertiary;
			ButtonMapping buttonMapping8 = buttonMapping7;
			ButtonMapping buttonMapping9 = new ButtonMapping();
			buttonMapping9.Destination = ButtonMapping.EDestinationType.PageLeft;
			ButtonMapping buttonMapping10 = buttonMapping9;
			ButtonMapping buttonMapping11 = new ButtonMapping();
			buttonMapping11.Destination = ButtonMapping.EDestinationType.PageRight;
			ButtonMapping buttonMapping12 = buttonMapping11;
			ButtonMapping buttonMapping13 = new ButtonMapping();
			buttonMapping13.Destination = ButtonMapping.EDestinationType.Up;
			ButtonMapping buttonMapping14 = buttonMapping13;
			ButtonMapping buttonMapping15 = new ButtonMapping();
			buttonMapping15.Destination = ButtonMapping.EDestinationType.Right;
			ButtonMapping buttonMapping16 = buttonMapping15;
			ButtonMapping buttonMapping17 = new ButtonMapping();
			buttonMapping17.Destination = ButtonMapping.EDestinationType.Down;
			ButtonMapping buttonMapping18 = buttonMapping17;
			ButtonMapping buttonMapping19 = new ButtonMapping();
			buttonMapping19.Destination = ButtonMapping.EDestinationType.Left;
			ButtonMapping buttonMapping20 = buttonMapping19;
			ButtonMapping buttonMapping21 = new ButtonMapping();
			buttonMapping21.Destination = ButtonMapping.EDestinationType.Pause;
			ButtonMapping buttonMapping22 = buttonMapping21;
			ButtonMapping buttonMapping23 = new ButtonMapping();
			buttonMapping23.Destination = ButtonMapping.EDestinationType.Map;
			ButtonMapping buttonMapping24 = buttonMapping23;
			ButtonMapping buttonMapping25 = new ButtonMapping();
			buttonMapping25.Destination = ButtonMapping.EDestinationType.Skip;
			ButtonMapping buttonMapping26 = buttonMapping25;
			ButtonMapping buttonMapping27 = new ButtonMapping();
			buttonMapping27.Destination = ButtonMapping.EDestinationType.Exit;
			ButtonMapping buttonMapping28 = buttonMapping27;
			buttonMapping2.AddSource(Buttons.A);
			buttonMapping4.AddSource(Buttons.B);
			buttonMapping4.AddSource(Buttons.Back);
			buttonMapping6.AddSource(Buttons.X);
			buttonMapping8.AddSource(Buttons.Y);
			buttonMapping14.AddSource(Buttons.DPadUp);
			buttonMapping14.AddSource(Buttons.LeftThumbstickUp);
			buttonMapping16.AddSource(Buttons.DPadRight);
			buttonMapping16.AddSource(Buttons.LeftThumbstickRight);
			buttonMapping18.AddSource(Buttons.DPadDown);
			buttonMapping18.AddSource(Buttons.LeftThumbstickDown);
			buttonMapping20.AddSource(Buttons.DPadLeft);
			buttonMapping20.AddSource(Buttons.LeftThumbstickLeft);
			buttonMapping22.AddSource(Buttons.Start);
			buttonMapping26.AddSource(Buttons.Start);
			buttonMapping28.AddSource(Buttons.Start);
			buttonMapping24.AddSource(Buttons.Back);
			buttonMapping10.AddSource(Buttons.LeftShoulder);
			buttonMapping12.AddSource(Buttons.RightShoulder);
			buttonMapping10.AddSource(Buttons.LeftTrigger);
			buttonMapping12.AddSource(Buttons.RightTrigger);
			buttonMapping2.AddSource(Keys.Enter);
			buttonMapping2.AddSource(Keys.Space);
			buttonMapping4.AddSource(Keys.Escape);
			buttonMapping6.AddSource(Keys.Q);
			buttonMapping8.AddSource(Keys.E);
			buttonMapping14.AddSource(Keys.Up);
			buttonMapping16.AddSource(Keys.Right);
			buttonMapping20.AddSource(Keys.Left);
			buttonMapping18.AddSource(Keys.Down);
			buttonMapping12.AddSource(Keys.Tab);
			buttonMapping10.AddSource(Keys.LeftShift);
			buttonMapping22.AddSource(Keys.Enter);
			buttonMapping22.AddSource(Keys.Escape);
			buttonMapping26.AddSource(Keys.Escape);
			buttonMapping24.AddSource(Keys.M);
			controllerMapping2.Mappings.Add(32, buttonMapping2);
			controllerMapping2.Mappings.Add(33, buttonMapping4);
			controllerMapping2.Mappings.Add(34, buttonMapping6);
			controllerMapping2.Mappings.Add(35, buttonMapping8);
			controllerMapping2.Mappings.Add(10, buttonMapping14);
			controllerMapping2.Mappings.Add(12, buttonMapping18);
			controllerMapping2.Mappings.Add(9, buttonMapping20);
			controllerMapping2.Mappings.Add(11, buttonMapping16);
			controllerMapping2.Mappings.Add(40, buttonMapping10);
			controllerMapping2.Mappings.Add(41, buttonMapping12);
			controllerMapping2.Mappings.Add(36, buttonMapping22);
			controllerMapping2.Mappings.Add(37, buttonMapping24);
			controllerMapping2.Mappings.Add(38, buttonMapping28);
			controllerMapping2.Mappings.Add(39, buttonMapping26);
			return controllerMapping2;
		}
	}

	public static ControllerMapping ControlAgnosticMapping
	{
		get
		{
			ControllerMapping controllerMapping = new ControllerMapping();
			controllerMapping.Mappings = new Dictionary<int, ButtonMapping>();
			ControllerMapping controllerMapping2 = controllerMapping;
			ButtonMapping buttonMapping = new ButtonMapping();
			buttonMapping.Destination = ButtonMapping.EDestinationType.Jump;
			ButtonMapping buttonMapping2 = buttonMapping;
			ButtonMapping buttonMapping3 = new ButtonMapping();
			buttonMapping3.Destination = ButtonMapping.EDestinationType.Spell;
			ButtonMapping buttonMapping4 = buttonMapping3;
			ButtonMapping buttonMapping5 = new ButtonMapping();
			buttonMapping5.Destination = ButtonMapping.EDestinationType.Melee;
			ButtonMapping buttonMapping6 = buttonMapping5;
			ButtonMapping buttonMapping7 = new ButtonMapping();
			buttonMapping7.Destination = ButtonMapping.EDestinationType.Time;
			ButtonMapping buttonMapping8 = buttonMapping7;
			ButtonMapping buttonMapping9 = new ButtonMapping();
			buttonMapping9.Destination = ButtonMapping.EDestinationType.Dash;
			ButtonMapping buttonMapping10 = buttonMapping9;
			ButtonMapping buttonMapping11 = new ButtonMapping();
			buttonMapping11.Destination = ButtonMapping.EDestinationType.Backdash;
			ButtonMapping buttonMapping12 = buttonMapping11;
			ButtonMapping buttonMapping13 = new ButtonMapping();
			buttonMapping13.Destination = ButtonMapping.EDestinationType.ToggleRight;
			ButtonMapping value = buttonMapping13;
			ButtonMapping buttonMapping14 = new ButtonMapping();
			buttonMapping14.Destination = ButtonMapping.EDestinationType.Up;
			ButtonMapping buttonMapping15 = buttonMapping14;
			ButtonMapping buttonMapping16 = new ButtonMapping();
			buttonMapping16.Destination = ButtonMapping.EDestinationType.Right;
			ButtonMapping buttonMapping17 = buttonMapping16;
			ButtonMapping buttonMapping18 = new ButtonMapping();
			buttonMapping18.Destination = ButtonMapping.EDestinationType.Down;
			ButtonMapping buttonMapping19 = buttonMapping18;
			ButtonMapping buttonMapping20 = new ButtonMapping();
			buttonMapping20.Destination = ButtonMapping.EDestinationType.Left;
			ButtonMapping buttonMapping21 = buttonMapping20;
			ButtonMapping buttonMapping22 = new ButtonMapping();
			buttonMapping22.Destination = ButtonMapping.EDestinationType.Start;
			ButtonMapping buttonMapping23 = buttonMapping22;
			ButtonMapping buttonMapping24 = new ButtonMapping();
			buttonMapping24.Destination = ButtonMapping.EDestinationType.Back;
			ButtonMapping buttonMapping25 = buttonMapping24;
			buttonMapping2.AddSource(Buttons.A);
			buttonMapping4.AddSource(Buttons.B);
			buttonMapping6.AddSource(Buttons.X);
			buttonMapping8.AddSource(Buttons.Y);
			buttonMapping10.AddSource(Buttons.RightShoulder);
			buttonMapping12.AddSource(Buttons.LeftShoulder);
			buttonMapping15.AddSource(Buttons.DPadUp);
			buttonMapping17.AddSource(Buttons.DPadRight);
			buttonMapping19.AddSource(Buttons.DPadDown);
			buttonMapping21.AddSource(Buttons.DPadLeft);
			buttonMapping23.AddSource(Buttons.Start);
			buttonMapping25.AddSource(Buttons.Back);
			controllerMapping2.Mappings.Add(1, buttonMapping2);
			controllerMapping2.Mappings.Add(2, buttonMapping6);
			controllerMapping2.Mappings.Add(3, buttonMapping4);
			controllerMapping2.Mappings.Add(4, buttonMapping8);
			controllerMapping2.Mappings.Add(8, buttonMapping12);
			controllerMapping2.Mappings.Add(7, buttonMapping10);
			controllerMapping2.Mappings.Add(14, value);
			controllerMapping2.Mappings.Add(10, buttonMapping15);
			controllerMapping2.Mappings.Add(12, buttonMapping19);
			controllerMapping2.Mappings.Add(9, buttonMapping21);
			controllerMapping2.Mappings.Add(11, buttonMapping17);
			controllerMapping2.Mappings.Add(5, buttonMapping23);
			controllerMapping2.Mappings.Add(6, buttonMapping25);
			return controllerMapping2;
		}
	}

	public static ControllerMapping OptionsMenuMapping
	{
		get
		{
			ControllerMapping controllerMapping = new ControllerMapping();
			controllerMapping.Mappings = new Dictionary<int, ButtonMapping>();
			ControllerMapping controllerMapping2 = controllerMapping;
			ButtonMapping buttonMapping = new ButtonMapping();
			buttonMapping.Destination = ButtonMapping.EDestinationType.Jump;
			ButtonMapping buttonMapping2 = buttonMapping;
			ButtonMapping buttonMapping3 = new ButtonMapping();
			buttonMapping3.Destination = ButtonMapping.EDestinationType.Spell;
			ButtonMapping buttonMapping4 = buttonMapping3;
			ButtonMapping buttonMapping5 = new ButtonMapping();
			buttonMapping5.Destination = ButtonMapping.EDestinationType.Melee;
			ButtonMapping buttonMapping6 = buttonMapping5;
			ButtonMapping buttonMapping7 = new ButtonMapping();
			buttonMapping7.Destination = ButtonMapping.EDestinationType.Time;
			ButtonMapping buttonMapping8 = buttonMapping7;
			ButtonMapping buttonMapping9 = new ButtonMapping();
			buttonMapping9.Destination = ButtonMapping.EDestinationType.Start;
			ButtonMapping buttonMapping10 = buttonMapping9;
			ButtonMapping buttonMapping11 = new ButtonMapping();
			buttonMapping11.Destination = ButtonMapping.EDestinationType.ToggleRight;
			ButtonMapping buttonMapping12 = buttonMapping11;
			buttonMapping2.AddSource(Buttons.A);
			buttonMapping4.AddSource(Buttons.B);
			buttonMapping6.AddSource(Buttons.X);
			buttonMapping8.AddSource(Buttons.Y);
			buttonMapping10.AddSource(Buttons.Start);
			buttonMapping12.AddSource(Buttons.Back);
			buttonMapping10.AddSource(Keys.Enter);
			buttonMapping12.AddSource(Keys.Escape);
			controllerMapping2.Mappings.Add(1, buttonMapping2);
			controllerMapping2.Mappings.Add(2, buttonMapping6);
			controllerMapping2.Mappings.Add(3, buttonMapping4);
			controllerMapping2.Mappings.Add(4, buttonMapping8);
			controllerMapping2.Mappings.Add(5, buttonMapping10);
			controllerMapping2.Mappings.Add(14, buttonMapping12);
			return controllerMapping2;
		}
	}

	public ControllerMapping Duplicate()
	{
		ControllerMapping controllerMapping = new ControllerMapping();
		controllerMapping.Mappings = new Dictionary<int, ButtonMapping>();
		controllerMapping.DoesUseControllerRumble = DoesUseControllerRumble;
		ControllerMapping controllerMapping2 = controllerMapping;
		foreach (KeyValuePair<int, ButtonMapping> mapping in Mappings)
		{
			controllerMapping2.Mappings.Add(mapping.Key, mapping.Value.Duplicate());
		}
		return controllerMapping2;
	}

	public bool IsButtonDown(int destination, GamePadState gamePadState, KeyboardState keyboardState)
	{
		if (Mappings.ContainsKey(destination))
		{
			return Mappings[destination].IsButtonDown(gamePadState, keyboardState);
		}
		return false;
	}

	internal UIButton CreateUIButtonFromDestination(ButtonMapping.EDestinationType destination)
	{
		UIButton uIButton = null;
		if (Mappings.ContainsKey((int)destination))
		{
			ButtonMapping buttonMapping = Mappings[(int)destination];
			uIButton = CreateUIButtonFromSources(buttonMapping.Sources);
		}
		return uIButton ?? new UIButton(Buttons.A);
	}

	internal static UIButton CreateUIButtonFromSources(IEnumerable<ButtonMappingSource> sources)
	{
		UIButton uIButton = null;
		bool isKeyboardPreferred = Constants.IsKeyboardPreferred;
		ButtonMappingSource buttonMappingSource = null;
		foreach (ButtonMappingSource source in sources)
		{
			if (source.SourceType == ButtonMappingSource.ESourceType.GamepadButton)
			{
				buttonMappingSource = source;
				if (!isKeyboardPreferred)
				{
					break;
				}
			}
			else if (source.SourceType == ButtonMappingSource.ESourceType.Keyboard)
			{
				buttonMappingSource = source;
				if (isKeyboardPreferred)
				{
					break;
				}
			}
		}
		if (buttonMappingSource != null)
		{
			switch (buttonMappingSource.SourceType)
			{
			case ButtonMappingSource.ESourceType.GamepadButton:
				uIButton = new UIButton(buttonMappingSource.GamepadButton);
				if (uIButton.Button == UIButton.EControllerButton.None)
				{
					uIButton = null;
				}
				break;
			case ButtonMappingSource.ESourceType.Keyboard:
				if (buttonMappingSource.KeyboardKey != 0)
				{
					uIButton = new UIButton(buttonMappingSource.KeyboardKey);
				}
				break;
			}
		}
		return uIButton;
	}
}
