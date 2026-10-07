using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using Microsoft.Xna.Framework.Input;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameAbstractions.Saving;

[Serializable]
public class GameConfigSave
{
	private const string XmlGameConfigNodeName = "GameConfig";

	private const string XmlConfigPlayerControlsNodeName = "PlayerControls";

	private const string XmlConfigFamiliarControlsNodeName = "FamiliarControls";

	private const string XmlConfigMenuControlsNodeName = "MenuControls";

	private const string XmlConfigGameClearedAttributeName = "Cleared";

	private const string XmlConfigLevelCap1ClearedAttributeName = "Cap1Cleared";

	private const string XmlConfigAudioMasterAttributeName = "VolumeMaster";

	private const string XmlConfigAudioSFXAttributeName = "VolumeSFX";

	private const string XmlConfigAudioVOAttributeName = "VolumeVO";

	private const string XmlConfigAudioMusicAttributeName = "VolumeMusic";

	private const string XmlConfigScreenResolutionAttributeName = "ScreenResolution";

	private const string XmlConfigLocaleAttributeName = "Locale";

	private const string XmlConfigHasPickedLocaleAttributeName = "HasPickedLocale";

	private const string XmlConfigIsFullScreenAttributeName = "IsFullScreen";

	private const string XmlConfigDoesDrawBorderFrameAttributeName = "DoesDrawBorderFrame";

	private const string XmlConfigIsKeyboardPreferredAttributeName = "IsKeyboardPreferred";

	private const string XmlConfigButtonDisplayTypeAttributeName = "ButtonDisplayType";

	private const string XmlControlsRumbleAttributeName = "Rumble";

	private const string XmlControlsMappingNodeName = "Map";

	private const string XmlControlsMappingDestinationAttributeName = "Destination";

	private const string XmlControlsMappingSourcesNodeName = "Sources";

	private const string XmlControlsMappingSourceNodeName = "Source";

	private const string XmlControlsMappingSourceTypeAttributeName = "Type";

	private const string XmlControlsMappingSourceValueAttributeName = "Value";

	public bool HasGameBeenCleared { get; set; }

	public bool HasLevelCap1BeenCleared { get; set; }

	public bool IsFullScreen { get; set; }

	public bool DoesDrawBorderFrame { get; set; }

	public bool IsKeyboardPreferred { get; set; }

	public bool HasPickedLocale { get; set; }

	public int LocaleType { get; set; }

	public int ScreenResolutionType { get; set; }

	public int ButtonDisplayType { get; set; }

	public float AudioVolumeMaster { get; set; }

	public float AudioVolumeSFX { get; set; }

	public float AudioVolumeVO { get; set; }

	public float AudioVolumeMusic { get; set; }

	public ControllerMapping PlayerControllerMapping { get; set; }

	public ControllerMapping FamiliarControllerMapping { get; set; }

	public ControllerMapping MenuControllerMapping { get; set; }

	public static GameConfigSave EditorSave
	{
		get
		{
			GameConfigSave gameConfigSave = new GameConfigSave();
			gameConfigSave.PlayerControllerMapping = ControllerMapping.DefaultMapping;
			gameConfigSave.FamiliarControllerMapping = ControllerMapping.DefaultFamiliarMapping;
			gameConfigSave.MenuControllerMapping = ControllerMapping.DefaultMenuMapping;
			GameConfigSave gameConfigSave2 = gameConfigSave;
			gameConfigSave2.PlayerControllerMapping.DoesUseControllerRumble = false;
			return gameConfigSave2;
		}
	}

	public GameConfigSave()
	{
		InitializePostLoad();
	}

	public static GameConfigSave LoadBinary(Stream stream)
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		GameConfigSave gameConfigSave = (GameConfigSave)binaryFormatter.Deserialize(stream);
		gameConfigSave.InitializePostLoad();
		return gameConfigSave;
	}

	public static GameConfigSave LoadXml(Stream stream)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		GameConfigSave gameConfigSave = new GameConfigSave();
		ControllerMapping controllerMapping = null;
		ButtonMapping buttonMapping = null;
		XmlReader val = XmlReader.Create(stream);
		try
		{
			while (val.Read())
			{
				if ((int)val.NodeType != 1)
				{
					continue;
				}
				switch (val.LocalName)
				{
				case "GameConfig":
					while (val.MoveToNextAttribute())
					{
						switch (val.Name)
						{
						case "Cleared":
							gameConfigSave.HasGameBeenCleared = bool.Parse(val.Value);
							break;
						case "Cap1Cleared":
							gameConfigSave.HasLevelCap1BeenCleared = bool.Parse(val.Value);
							break;
						case "VolumeMaster":
							gameConfigSave.AudioVolumeMaster = val.Value.ParseFloat();
							break;
						case "VolumeSFX":
							gameConfigSave.AudioVolumeSFX = val.Value.ParseFloat();
							break;
						case "VolumeMusic":
							gameConfigSave.AudioVolumeMusic = val.Value.ParseFloat();
							break;
						case "VolumeVO":
							gameConfigSave.AudioVolumeVO = val.Value.ParseFloat();
							break;
						case "ScreenResolution":
							gameConfigSave.ScreenResolutionType = val.Value.ParseInt32();
							break;
						case "Locale":
							gameConfigSave.LocaleType = val.Value.ParseInt32();
							break;
						case "HasPickedLocale":
							gameConfigSave.HasPickedLocale = bool.Parse(val.Value);
							break;
						case "IsFullScreen":
							gameConfigSave.IsFullScreen = bool.Parse(val.Value);
							break;
						case "DoesDrawBorderFrame":
							gameConfigSave.DoesDrawBorderFrame = bool.Parse(val.Value);
							break;
						case "IsKeyboardPreferred":
							gameConfigSave.IsKeyboardPreferred = bool.Parse(val.Value);
							break;
						case "ButtonDisplayType":
							gameConfigSave.ButtonDisplayType = val.Value.ParseInt32();
							break;
						}
					}
					break;
				case "PlayerControls":
				{
					if (buttonMapping != null && controllerMapping != null)
					{
						controllerMapping.Mappings.Add((int)buttonMapping.Destination, buttonMapping);
						buttonMapping = null;
					}
					ControllerMapping controllerMapping2 = new ControllerMapping();
					controllerMapping2.Mappings = new Dictionary<int, ButtonMapping>();
					ControllerMapping controllerMapping3 = controllerMapping2;
					while (val.MoveToNextAttribute())
					{
						string name;
						if ((name = val.Name) != null && name == "Rumble")
						{
							controllerMapping3.DoesUseControllerRumble = bool.Parse(val.Value);
						}
					}
					controllerMapping = controllerMapping3;
					gameConfigSave.PlayerControllerMapping = controllerMapping3;
					break;
				}
				case "FamiliarControls":
				{
					if (buttonMapping != null && controllerMapping != null)
					{
						controllerMapping.Mappings.Add((int)buttonMapping.Destination, buttonMapping);
						buttonMapping = null;
					}
					ControllerMapping controllerMapping6 = new ControllerMapping();
					controllerMapping6.Mappings = new Dictionary<int, ButtonMapping>();
					ControllerMapping controllerMapping7 = controllerMapping6;
					while (val.MoveToNextAttribute())
					{
						string name2;
						if ((name2 = val.Name) != null && name2 == "Rumble")
						{
							controllerMapping7.DoesUseControllerRumble = bool.Parse(val.Value);
						}
					}
					controllerMapping = controllerMapping7;
					gameConfigSave.FamiliarControllerMapping = controllerMapping7;
					break;
				}
				case "MenuControls":
				{
					if (buttonMapping != null && controllerMapping != null)
					{
						controllerMapping.Mappings.Add((int)buttonMapping.Destination, buttonMapping);
						buttonMapping = null;
					}
					ControllerMapping controllerMapping4 = new ControllerMapping();
					controllerMapping4.Mappings = new Dictionary<int, ButtonMapping>();
					controllerMapping4.IsMenuMapping = true;
					controllerMapping = (gameConfigSave.MenuControllerMapping = controllerMapping4);
					break;
				}
				case "Map":
					if (buttonMapping != null)
					{
						controllerMapping?.Mappings.Add((int)buttonMapping.Destination, buttonMapping);
					}
					buttonMapping = new ButtonMapping();
					while (val.MoveToNextAttribute())
					{
						string name3;
						if ((name3 = val.Name) != null && name3 == "Destination")
						{
							buttonMapping.Destination = (ButtonMapping.EDestinationType)val.Value.ParseInt32();
						}
					}
					break;
				case "Source":
				{
					if (buttonMapping == null)
					{
						break;
					}
					ButtonMappingSource buttonMappingSource = new ButtonMappingSource();
					int num = 0;
					while (val.MoveToNextAttribute())
					{
						switch (val.Name)
						{
						case "Type":
							buttonMappingSource.SourceType = (ButtonMappingSource.ESourceType)val.Value.ParseInt32();
							break;
						case "Value":
							num = val.Value.ParseInt32();
							break;
						}
					}
					switch (buttonMappingSource.SourceType)
					{
					case ButtonMappingSource.ESourceType.GamepadButton:
						buttonMappingSource.GamepadButton = (Buttons)num;
						break;
					case ButtonMappingSource.ESourceType.Keyboard:
						buttonMappingSource.KeyboardKey = (Keys)num;
						break;
					case ButtonMappingSource.ESourceType.Mouse:
						buttonMappingSource.MouseButton = default(MouseState);
						break;
					}
					buttonMapping.Sources.Add(buttonMappingSource);
					break;
				}
				}
			}
			if (buttonMapping != null)
			{
				controllerMapping?.Mappings.Add((int)buttonMapping.Destination, buttonMapping);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		return gameConfigSave;
	}

	public void SaveBinary(Stream stream)
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		binaryFormatter.Serialize(stream, this);
	}

	public void SaveXml(Stream stream)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		XmlDocument val = new XmlDocument();
		XmlNode node = val.AddElement("GameConfig");
		node.AddAttributeNoDefault("Cleared", HasGameBeenCleared);
		node.AddAttributeNoDefault("Cap1Cleared", HasLevelCap1BeenCleared);
		node.AddAttributeNoDefault("VolumeMaster", AudioVolumeMaster);
		node.AddAttributeNoDefault("VolumeSFX", AudioVolumeSFX);
		node.AddAttributeNoDefault("VolumeMusic", AudioVolumeMusic);
		node.AddAttributeNoDefault("VolumeVO", AudioVolumeVO);
		node.AddAttributeNoDefault("Locale", LocaleType);
		node.AddAttributeNoDefault("HasPickedLocale", HasPickedLocale);
		node.AddAttributeNoDefault("ScreenResolution", ScreenResolutionType);
		node.AddAttributeNoDefault("IsFullScreen", IsFullScreen);
		node.AddAttributeNoDefault("DoesDrawBorderFrame", DoesDrawBorderFrame);
		node.AddAttributeNoDefault("IsKeyboardPreferred", IsKeyboardPreferred);
		node.AddAttributeNoDefault("ButtonDisplayType", ButtonDisplayType);
		XmlNode node2 = node.AddElement("PlayerControls");
		SaveControlsXml(node2, PlayerControllerMapping);
		XmlNode node3 = node.AddElement("FamiliarControls");
		SaveControlsXml(node3, FamiliarControllerMapping);
		XmlNode node4 = node.AddElement("MenuControls");
		SaveControlsXml(node4, MenuControllerMapping);
		val.Save(stream);
	}

	private static void SaveControlsXml(XmlNode node, ControllerMapping controls)
	{
		node.AddAttributeNoDefault("Rumble", controls.DoesUseControllerRumble);
		foreach (KeyValuePair<int, ButtonMapping> mapping in controls.Mappings)
		{
			XmlNode node2 = node.AddElement("Map");
			node2.AddAttribute("Destination", mapping.Key);
			XmlNode node3 = node2.AddElement("Sources");
			foreach (ButtonMappingSource source in mapping.Value.Sources)
			{
				XmlNode node4 = node3.AddElement("Source");
				node4.AddAttribute("Type", (int)source.SourceType);
				int value = 0;
				switch (source.SourceType)
				{
				case ButtonMappingSource.ESourceType.GamepadButton:
					value = (int)source.GamepadButton;
					break;
				case ButtonMappingSource.ESourceType.Keyboard:
					value = (int)source.KeyboardKey;
					break;
				case ButtonMappingSource.ESourceType.Mouse:
					value = (int)source.MouseButton.LeftButton;
					break;
				}
				node4.AddAttributeNoDefault("Value", value);
			}
		}
	}

	public void InitializePostLoad()
	{
		if (PlayerControllerMapping == null)
		{
			PlayerControllerMapping = ControllerMapping.DefaultMapping;
		}
		if (FamiliarControllerMapping == null)
		{
			FamiliarControllerMapping = ControllerMapping.DefaultFamiliarMapping;
		}
		if (MenuControllerMapping == null)
		{
			MenuControllerMapping = ControllerMapping.DefaultMenuMapping;
		}
	}
}
