using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Cutscene;
using ZlibNet;

namespace Timespinner.GameAbstractions.Saving;

public class GameSave
{
	public enum EGameDifficultyType
	{
		None,
		Easy,
		Normal,
		Hard,
		HardCap1,
		HardCap255
	}

	public const int TotalLevels = 19;

	private const int PrologueStartLevel = 0;

	private const int PrologueStartRoom = 3;

	private const int SpeedrunStartRoom = 0;

	internal const string EasyModeKey = "IsEasyMode";

	internal const string HardModeKey = "IsHardMode";

	internal const string LevelCap1Key = "IsLevelCap1";

	internal const string LevelCap255Key = "IsLevelCap255";

	internal const string GameClearedKey = "IsGameCleared";

	internal const string EndingABClearedKey = "IsEndingABCleared";

	internal const string EndingCDClearedKey = "IsEndingCDCleared";

	private const string XmlGameSaveNodeName = "TimespinnerSave";

	private const string XmlSaveIndexAttributeName = "Index";

	private const string XmlSaveLevelAttributeName = "Level";

	private const string XmlSaveRoomAttributeName = "Room";

	private const string XmlSaveCheckpointAttributeName = "Checkpoint";

	private const string XmlSaveWarpLevelAttributeName = "WarpLevel";

	private const string XmlSaveWarpRoomAttributeName = "WarpRoom";

	private const string XmlSaveMoneyAttributeName = "Money";

	private const string XmlSaveKillsAttributeName = "Kills";

	private const string XmlSaveTimeAttributeName = "Time";

	private const string XmlSaveWriteTimeAttributeName = "Write";

	private const string XmlSaveDataCharacterStatsNodeName = "Stats";

	private const string XmlSaveDataCharacterStatsExpAttributeName = "Exp";

	private const string XmlSaveDataCharacterStatsMaxHPAttributeName = "MaxHP";

	private const string XmlSaveDataCharacterStatsMaxAuraAttributeName = "MaxAura";

	private const string XmlSaveDataCharacterStatsMaxSandAttributeName = "MaxSand";

	private const string XmlSaveDataKeyBoolNodeName = "DataBools";

	private const string XmlSaveDataKeyIntNodeName = "DataInts";

	private const string XmlSaveDataKeyStringNodeName = "DataStrings";

	private const string XmlSaveLevelKeyBoolNodeName = "LevelBools";

	private const string XmlSaveLevelKeyIntNodeName = "LevelInts";

	private const string XmlSaveFeatsNodeName = "Feats";

	private const string XmlSaveDictEntryNodeName = "Entry";

	private const string XmlSaveDictKeyAttributeName = "Key";

	private const string XmlSaveDictValueAttributeName = "Value";

	private readonly GameFeatsManager _featsManager;

	public bool DoesNeedSave;

	internal bool IsCorrupt { get; private set; }

	public bool IsGameCleared => GetSaveBool("IsGameCleared");

	public bool IsEasyMode => GetSaveBool("IsEasyMode");

	public bool IsHardMode => GetSaveBool("IsHardMode");

	public bool IsLevelCap1 => GetSaveBool("IsLevelCap1");

	public bool IsLevelCap255 => GetSaveBool("IsLevelCap255");

	public bool IsAnySpeedrunActive
	{
		get
		{
			if (!IsSpeedrunAActive)
			{
				return IsSpeedrunBActive;
			}
			return true;
		}
	}

	public bool IsSpeedrunAActive => GetSaveBool("IsActiveSpeedrunA");

	public bool IsSpeedrunBActive => GetSaveBool("IsActiveSpeedrunB");

	public int SaveFileIndex { get; set; }

	public int CurrentLevel { get; set; }

	public int CurrentRoom { get; set; }

	public int CurrentCheckpoint { get; set; }

	public int LastWarpLevel { get; set; }

	public int LastWarpRoom { get; set; }

	public int Money { get; set; }

	public int Kills { get; set; }

	public int ElapsedGameSeconds { get; set; }

	public DateTime FileWriteTime { get; set; }

	public CharacterStats CharacterStats { get; set; }

	public bool IsTimeStopUnlocked => Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.TimespinnerWheel);

	public bool IsDashingUnlocked => Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.Dash);

	public bool IsDoubleJumpUnlocked => Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.DoubleJump);

	public bool IsWaterMaskUnlocked => Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.WaterMask);

	public bool IsOrbSwitchingUnlocked => Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.JewelryBox);

	public Dictionary<string, bool> DataKeyBools { get; private set; }

	public Dictionary<string, int> DataKeyInts { get; private set; }

	public Dictionary<string, string> DataKeyStrings { get; private set; }

	public Dictionary<string, bool> LevelSaveBools { get; private set; }

	public Dictionary<string, int> LevelSaveInts { get; private set; }

	public GameFeatsSave FeatsSave { get; set; }

	internal GameFeatsManager FeatsManager => _featsManager;

	public MinimapSpecificationSave MinimapSave { get; set; }

	public PlayerInventory Inventory { get; set; }

	public static GameSave EditorSave
	{
		get
		{
			GameSave gameSave = new GameSave();
			gameSave.Inventory.EquippedMeleeOrbA = EInventoryOrbType.Blue;
			gameSave.Inventory.EquippedMeleeOrbB = EInventoryOrbType.Blue;
			gameSave.Inventory.EquippedSpellOrb = EInventoryOrbType.Blue;
			gameSave.CharacterStats.Experience = 99999;
			GameSave gameSave2 = gameSave;
			gameSave2.Inventory.AddItem(EInventoryOrbType.Blue, EOrbSlot.Melee);
			gameSave2.Inventory.AddItem(EInventoryOrbType.Blue, EOrbSlot.Spell);
			gameSave2.GiveOrbExperience(EInventoryOrbType.Blue);
			gameSave2.UnlockRelic(EInventoryRelicType.DoubleJump);
			gameSave2.UnlockRelic(EInventoryRelicType.Dash);
			gameSave2.UnlockRelic(EInventoryRelicType.TimespinnerWheel);
			gameSave2.UnlockRelic(EInventoryRelicType.WaterMask);
			gameSave2.UnlockRelic(EInventoryRelicType.JewelryBox);
			gameSave2.UnlockRelic(EInventoryRelicType.EssenceOfSpace);
			gameSave2.UnlockRelic(EInventoryRelicType.ElevatorKeycard);
			gameSave2.SetValue("IsEasyMode", value: true);
			return gameSave2;
		}
	}

	public static GameSave DemoSave
	{
		get
		{
			GameSave gameSave = new GameSave();
			gameSave.Inventory.EquippedMeleeOrbA = EInventoryOrbType.Blue;
			gameSave.Inventory.EquippedMeleeOrbB = EInventoryOrbType.Blue;
			gameSave.Inventory.EquippedSpellOrb = EInventoryOrbType.Blue;
			GameSave gameSave2 = gameSave;
			gameSave2.Inventory.OrbSets = new List<OrbSet>
			{
				new OrbSet
				{
					MeleeOrbA = EInventoryOrbType.Blue,
					MeleeOrbB = EInventoryOrbType.Blue,
					SpellOrb = EInventoryOrbType.Blue
				},
				new OrbSet(),
				new OrbSet()
			};
			gameSave2.Inventory.AddItem(EInventoryOrbType.Blue, EOrbSlot.Melee);
			gameSave2.Inventory.AddItem(EInventoryOrbType.Blue, EOrbSlot.Spell);
			gameSave2.GiveOrbExperience(EInventoryOrbType.Blue);
			gameSave2.Inventory.AddItem(EInventoryUseItemType.LachiemiSun, 1);
			gameSave2.CurrentLevel = 1;
			gameSave2.CurrentRoom = 0;
			gameSave2.CurrentCheckpoint = 0;
			return gameSave2;
		}
	}

	public static GameSave CorruptSave
	{
		get
		{
			GameSave gameSave = new GameSave();
			gameSave.IsCorrupt = true;
			return gameSave;
		}
	}

	public GameSave()
	{
		CharacterStats = new CharacterStats();
		Inventory = new PlayerInventory();
		FeatsSave = new GameFeatsSave();
		_featsManager = new GameFeatsManager();
		DebugSetBuffDefaults();
		DataKeyBools = new Dictionary<string, bool>();
		DataKeyInts = new Dictionary<string, int>();
		DataKeyStrings = new Dictionary<string, string>();
		LevelSaveBools = new Dictionary<string, bool>();
		LevelSaveInts = new Dictionary<string, int>();
		InitializePostLoad();
	}

	public static GameSave CreateNewSave(int newIndex, EGameDifficultyType difficulty)
	{
		GameSave gameSave = new GameSave();
		gameSave.SaveFileIndex = newIndex;
		gameSave.CurrentLevel = 0;
		gameSave.CurrentRoom = 3;
		GameSave gameSave2 = gameSave;
		switch (difficulty)
		{
		case EGameDifficultyType.Easy:
			gameSave2.SetValue("IsEasyMode", value: true);
			break;
		case EGameDifficultyType.Hard:
			gameSave2.SetValue("IsHardMode", value: true);
			break;
		case EGameDifficultyType.HardCap1:
			gameSave2.SetValue("IsHardMode", value: true);
			gameSave2.SetValue("IsLevelCap1", value: true);
			break;
		case EGameDifficultyType.HardCap255:
			gameSave2.SetValue("IsHardMode", value: true);
			gameSave2.SetValue("IsLevelCap255", value: true);
			break;
		}
		int levelCap = GetLevelCap(gameSave2);
		gameSave2.CharacterStats.InitializePostLoad(levelCap);
		gameSave2.Inventory.OrbSets.Add(new OrbSet());
		gameSave2.Inventory.OrbSets.Add(new OrbSet());
		gameSave2.Inventory.OrbSets.Add(new OrbSet());
		gameSave2.Inventory.UseItemInventory.AddItem(16);
		return gameSave2;
	}

	public static GameSave CreateNewSpeedrunASave(int newIndex, EGameDifficultyType difficulty)
	{
		GameSave gameSave = CreateNewSave(newIndex, difficulty);
		gameSave.CurrentRoom = 0;
		gameSave.Inventory.AddItem(EInventoryOrbType.Blue, EOrbSlot.Melee);
		gameSave.Inventory.AddItem(EInventoryOrbType.Blue, EOrbSlot.Spell);
		gameSave.Inventory.EquippedMeleeOrbA = EInventoryOrbType.Blue;
		gameSave.Inventory.EquippedMeleeOrbB = EInventoryOrbType.Blue;
		gameSave.Inventory.EquippedSpellOrb = EInventoryOrbType.Blue;
		gameSave.Inventory.OrbSets[0].MeleeOrbA = EInventoryOrbType.Blue;
		gameSave.Inventory.OrbSets[0].MeleeOrbB = EInventoryOrbType.Blue;
		gameSave.Inventory.OrbSets[0].SpellOrb = EInventoryOrbType.Blue;
		gameSave.SetValue("IsActiveSpeedrunA", value: true);
		gameSave.SetValue("IsFlaggedSpeedrunA", value: true);
		gameSave.SetValue("IsGameCleared", value: true);
		gameSave.UnlockRelic(EInventoryRelicType.EmpireBrooch);
		return gameSave;
	}

	public void SetValue(string key, bool value)
	{
		DataKeyBools[key] = value;
	}

	public void SetValue(string key, int value)
	{
		DataKeyInts[key] = value;
	}

	public void SetValue(string key, string value)
	{
		DataKeyStrings[key] = value;
	}

	public bool GetSaveBool(string key)
	{
		bool result = false;
		if (DataKeyBools.ContainsKey(key))
		{
			result = DataKeyBools[key];
		}
		return result;
	}

	public int GetSaveInt(string key)
	{
		int result = 0;
		if (DataKeyInts.ContainsKey(key))
		{
			result = DataKeyInts[key];
		}
		return result;
	}

	public string GetSaveString(string key)
	{
		string result = string.Empty;
		if (DataKeyStrings.ContainsKey(key))
		{
			result = DataKeyStrings[key];
		}
		return result;
	}

	public static GameSave Load(Stream stream)
	{
		long position = stream.Position;
		try
		{
			using ZInOutStream stream2 = new ZInOutStream(stream);
			return LoadXml(stream2);
		}
		catch (ZStreamException)
		{
			stream.Position = position;
			return LoadXml(stream);
		}
	}

	public static GameSave LoadXml(Stream stream)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		GameSave gameSave = new GameSave();
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
				case "TimespinnerSave":
					while (val.MoveToNextAttribute())
					{
						switch (val.Name)
						{
						case "Index":
							gameSave.SaveFileIndex = val.Value.ParseInt32();
							break;
						case "Level":
							gameSave.CurrentLevel = val.Value.ParseInt32();
							break;
						case "Room":
							gameSave.CurrentRoom = val.Value.ParseInt32();
							break;
						case "Checkpoint":
							gameSave.CurrentCheckpoint = val.Value.ParseInt32();
							break;
						case "WarpLevel":
							gameSave.LastWarpLevel = val.Value.ParseInt32();
							break;
						case "WarpRoom":
							gameSave.LastWarpRoom = val.Value.ParseInt32();
							break;
						case "Money":
							gameSave.Money = val.Value.ParseInt32();
							break;
						case "Kills":
							gameSave.Kills = val.Value.ParseInt32();
							break;
						case "Time":
							gameSave.ElapsedGameSeconds = val.Value.ParseInt32();
							break;
						case "Write":
							gameSave.FileWriteTime = val.Value.ParseDateTime();
							break;
						}
					}
					break;
				case "Stats":
					while (val.MoveToNextAttribute())
					{
						switch (val.Name)
						{
						case "Exp":
							gameSave.CharacterStats.Experience = val.Value.ParseInt32();
							break;
						case "MaxHP":
							gameSave.CharacterStats.MaxHPFound = val.Value.ParseInt32();
							break;
						case "MaxAura":
							gameSave.CharacterStats.MaxAuraFound = val.Value.ParseInt32();
							break;
						case "MaxSand":
							gameSave.CharacterStats.MaxSandFound = val.Value.ParseInt32();
							break;
						}
					}
					break;
				case "DataBools":
					LoadXmlBools(val, gameSave.DataKeyBools);
					break;
				case "DataInts":
					LoadXmlInts(val, gameSave.DataKeyInts);
					break;
				case "DataStrings":
					LoadXmlStrings(val, gameSave.DataKeyStrings);
					break;
				case "LevelBools":
					LoadXmlBools(val, gameSave.LevelSaveBools);
					break;
				case "LevelInts":
					LoadXmlInts(val, gameSave.LevelSaveInts);
					break;
				case "Feats":
					LoadXmlInts(val, gameSave.FeatsSave.FeatsData);
					break;
				case "Map":
					gameSave.MinimapSave = MinimapSpecificationSave.LoadXml(val);
					break;
				case "Inventory":
					PlayerInventory.LoadXml(val, gameSave.Inventory);
					break;
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		gameSave.InitializePostLoad();
		return gameSave;
	}

	private static void LoadXmlBools(XmlReader reader, Dictionary<string, bool> dataBools)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Invalid comparison between Unknown and I4
		bool flag = false;
		while (!flag && reader.Read())
		{
			if ((int)reader.NodeType == 1)
			{
				string localName;
				if ((localName = reader.LocalName) != null && localName == "Entry")
				{
					while (reader.MoveToNextAttribute())
					{
						string name;
						if ((name = reader.Name) != null && name == "Key")
						{
							dataBools.Add(reader.Value, value: true);
						}
					}
				}
				else
				{
					flag = true;
				}
			}
			else if ((int)reader.NodeType == 15 && reader.Name != "Entry")
			{
				flag = true;
			}
		}
	}

	private static void LoadXmlInts(XmlReader reader, Dictionary<string, int> dataInts)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Invalid comparison between Unknown and I4
		bool flag = false;
		while (!flag && reader.Read())
		{
			if ((int)reader.NodeType == 1)
			{
				string localName;
				if ((localName = reader.LocalName) != null && localName == "Entry")
				{
					string text = null;
					int num = 0;
					while (reader.MoveToNextAttribute())
					{
						switch (reader.Name)
						{
						case "Key":
							text = reader.Value;
							break;
						case "Value":
							num = reader.Value.ParseInt32();
							break;
						}
					}
					if (text != null && num != 0)
					{
						dataInts.Add(text, num);
					}
				}
				else
				{
					flag = true;
				}
			}
			else if ((int)reader.NodeType == 15 && reader.Name != "Entry")
			{
				flag = true;
			}
		}
	}

	private static void LoadXmlStrings(XmlReader reader, Dictionary<string, string> dataStrings)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Invalid comparison between Unknown and I4
		bool flag = false;
		while (!flag && reader.Read())
		{
			if ((int)reader.NodeType == 1)
			{
				string localName;
				if ((localName = reader.LocalName) != null && localName == "Entry")
				{
					string text = null;
					string text2 = null;
					while (reader.MoveToNextAttribute())
					{
						switch (reader.Name)
						{
						case "Key":
							text = reader.Value;
							break;
						case "Value":
							text2 = reader.Value;
							break;
						}
					}
					if (text != null && text2 != null)
					{
						dataStrings.Add(text, text2);
					}
				}
				else
				{
					flag = true;
				}
			}
			else if ((int)reader.NodeType == 15 && reader.Name != "Entry")
			{
				flag = true;
			}
		}
	}

	private static int GetLevelCap(GameSave save)
	{
		bool saveBool = save.GetSaveBool("IsLevelCap1");
		bool saveBool2 = save.GetSaveBool("IsLevelCap255");
		if (!saveBool && !saveBool2)
		{
			return 99;
		}
		if (saveBool)
		{
			return 0;
		}
		return 254;
	}

	private void InitializePostLoad()
	{
		Inventory.IntializePostLoad();
		CharacterStats.InitializePostLoad(GetLevelCap(this));
		CharacterStats.RefreshEquipmentStats(Inventory.GetEquipmentStats());
		_featsManager.InitializePostLoad(FeatsSave);
	}

	public void Save(Stream stream)
	{
		using ZInOutStream stream2 = new ZInOutStream(stream, 9);
		SaveXml(stream2);
	}

	public void SaveXml(Stream stream)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		XmlDocument val = new XmlDocument();
		XmlNode val2 = val.AddElement("TimespinnerSave");
		val2.AddAttributeNoDefault("Index", SaveFileIndex);
		val2.AddAttributeNoDefault("Level", CurrentLevel);
		val2.AddAttributeNoDefault("Room", CurrentRoom);
		val2.AddAttributeNoDefault("Checkpoint", CurrentCheckpoint);
		val2.AddAttributeNoDefault("WarpLevel", LastWarpLevel);
		val2.AddAttributeNoDefault("WarpRoom", LastWarpRoom);
		val2.AddAttributeNoDefault("Money", Money);
		val2.AddAttributeNoDefault("Kills", Kills);
		val2.AddAttributeNoDefault("Time", ElapsedGameSeconds);
		val2.AddAttributeNoDefault("Write", FileWriteTime);
		XmlNode node = val2.AddElement("Stats");
		node.AddAttributeNoDefault("Exp", CharacterStats.Experience);
		node.AddAttributeNoDefault("MaxHP", CharacterStats.MaxHPFound);
		node.AddAttributeNoDefault("MaxAura", CharacterStats.MaxAuraFound);
		node.AddAttributeNoDefault("MaxSand", CharacterStats.MaxSandFound);
		if (DataKeyBools.Count > 0)
		{
			XmlNode node2 = val2.AddElement("DataBools");
			foreach (KeyValuePair<string, bool> dataKeyBool in DataKeyBools)
			{
				if (dataKeyBool.Value)
				{
					XmlNode node3 = node2.AddElement("Entry");
					node3.AddAttribute("Key", dataKeyBool.Key);
				}
			}
		}
		if (DataKeyInts.Count > 0)
		{
			XmlNode node4 = val2.AddElement("DataInts");
			foreach (KeyValuePair<string, int> dataKeyInt in DataKeyInts)
			{
				XmlNode node5 = node4.AddElement("Entry");
				node5.AddAttribute("Key", dataKeyInt.Key);
				node5.AddAttribute("Value", dataKeyInt.Value);
			}
		}
		if (DataKeyStrings.Count > 0)
		{
			XmlNode node6 = val2.AddElement("DataStrings");
			foreach (KeyValuePair<string, string> dataKeyString in DataKeyStrings)
			{
				XmlNode node7 = node6.AddElement("Entry");
				node7.AddAttribute("Key", dataKeyString.Key);
				node7.AddAttribute("Value", dataKeyString.Value);
			}
		}
		if (LevelSaveBools.Count > 0)
		{
			XmlNode node8 = val2.AddElement("LevelBools");
			foreach (KeyValuePair<string, bool> levelSaveBool in LevelSaveBools)
			{
				if (levelSaveBool.Value)
				{
					XmlNode node9 = node8.AddElement("Entry");
					node9.AddAttribute("Key", levelSaveBool.Key);
				}
			}
		}
		if (LevelSaveInts.Count > 0)
		{
			XmlNode node10 = val2.AddElement("LevelInts");
			foreach (KeyValuePair<string, int> levelSaveInt in LevelSaveInts)
			{
				XmlNode node11 = node10.AddElement("Entry");
				node11.AddAttribute("Key", levelSaveInt.Key);
				node11.AddAttribute("Value", levelSaveInt.Value);
			}
		}
		if (FeatsSave.FeatsData.Count > 0)
		{
			XmlNode node12 = val2.AddElement("Feats");
			foreach (KeyValuePair<string, int> featsDatum in FeatsSave.FeatsData)
			{
				XmlNode node13 = node12.AddElement("Entry");
				node13.AddAttribute("Key", featsDatum.Key);
				node13.AddAttribute("Value", featsDatum.Value);
			}
		}
		MinimapSave.SaveXml(val2);
		Inventory.SaveXml(val2);
		val.Save(stream);
	}

	private void DebugSetBuffDefaults()
	{
		GiveOrbExperience(EInventoryOrbType.Blue);
	}

	internal void UnlockRelic(EInventoryRelicType type)
	{
		Inventory.RelicInventory.AddItem((int)type);
	}

	internal void UnlockJournal(EInventoryJournalType type)
	{
		Inventory.AddItem(type);
	}

	internal void GiveFamiliar(EInventoryFamiliarType type)
	{
		Inventory.FamiliarInventory.AddItem((int)type);
		_featsManager.UnlockFamiliarAchievement(Inventory.FamiliarInventory);
	}

	internal void GiveOrb(EInventoryOrbType orbType, EOrbSlot slot)
	{
		Inventory.OrbInventory.AddItem((int)orbType, slot);
		_featsManager.UnlockOrbAchievement(Inventory.OrbInventory, slot, orbType);
	}

	internal bool GiveOrbExperience(EInventoryOrbType orbColor)
	{
		bool isDoublingExp = Inventory.EquippedTrinketA == EInventoryEquipmentType.NelisteEarring || Inventory.EquippedTrinketB == EInventoryEquipmentType.NelisteEarring;
		return Inventory.OrbInventory.GiveOrbExperience(orbColor, isDoublingExp);
	}

	internal int GetOrbDamage(EInventoryOrbType orbColor)
	{
		return Inventory.OrbInventory.GetOrbDamage(orbColor, EOrbSlot.Melee, CharacterStats.Willpower);
	}

	internal int GetOrbSpellDamage(EInventoryOrbType orbColor)
	{
		return Inventory.OrbInventory.GetOrbDamage(orbColor, EOrbSlot.Spell, CharacterStats.Willpower);
	}

	internal int GetOrbPassiveDamage(EInventoryOrbType orbColor)
	{
		return Inventory.OrbInventory.GetOrbDamage(orbColor, EOrbSlot.Passive, CharacterStats.Willpower);
	}

	internal bool GiveFamiliarExperience(EInventoryFamiliarType familiarType)
	{
		int num = 1;
		if (Inventory.EquippedTrinketA == EInventoryEquipmentType.FamiliarEgg)
		{
			num *= 3;
		}
		if (Inventory.EquippedTrinketB == EInventoryEquipmentType.FamiliarEgg)
		{
			num *= 3;
		}
		return Inventory.FamiliarInventory.GiveFamiliarExperience(familiarType, num);
	}

	internal InventoryFamiliar GetFamiliarItem(EInventoryFamiliarType familiarType)
	{
		return Inventory.FamiliarInventory.GetFamiliarItem(familiarType);
	}

	internal bool CheckSwitch(SwitchSpecification targetSwitch)
	{
		bool result = false;
		switch (targetSwitch.SwitchValueType)
		{
		case ESwitchValueType.Bool:
			result = GetSaveBool(targetSwitch.Key) == targetSwitch.BoolValue;
			break;
		case ESwitchValueType.Int:
			result = GetSaveInt(targetSwitch.Key) == targetSwitch.IntValue;
			break;
		case ESwitchValueType.String:
			result = GetSaveString(targetSwitch.Key) == targetSwitch.StringValue;
			break;
		}
		return result;
	}

	public bool AreAnyQuestsKnown()
	{
		return NPCBase.AreAnyQuestsActive(this);
	}

	internal void UnlockFeat(EGameFeatType basicType)
	{
		_featsManager.UnlockFeat(basicType);
	}

	internal bool UnlockFeat(EBossType bossType, int bossDeathData)
	{
		bool result = false;
		if (!IsEasyMode)
		{
			result = _featsManager.UnlockFeat(bossType, bossDeathData);
		}
		return result;
	}

	internal void SavePostEndingAB()
	{
		if (GetSaveBool("IsPrinceDead") || GetSaveBool("IsTerrilisDead"))
		{
			SetValue("IsEmperorKilledAfterAlts", value: true);
		}
		else
		{
			SetValue("IsEmperorKilledAfterAlts", value: false);
		}
		SetValue("IsGameCleared", value: true);
		SetValue("IsEndingABCleared", value: true);
		SetValue(BossClass.GetSaveKeyByBossType(EBossType.Emperor), value: false);
		UnlockFeat(EGameFeatType.FinishGame);
		if (IsHardMode)
		{
			UnlockFeat(EGameFeatType.FinishGameHard);
		}
		CurrentLevel = 12;
		CurrentRoom = 11;
		CurrentCheckpoint = 1;
		LevelSaveBools.Clear();
		LevelSaveInts.Clear();
		DoesNeedSave = true;
	}

	internal void SavePostEndingCD(bool isEndingD)
	{
		SetValue("IsGameCleared", value: true);
		SetValue("IsEndingCDCleared", value: true);
		SetValue(BossClass.GetSaveKeyByBossType(EBossType.Sandman), value: false);
		SetValue(BossClass.GetSaveKeyByBossType(EBossType.Nightmare), value: false);
		CutsceneBase.SetIsCutsceneTriggeredByType(CutsceneBase.ECutsceneType.Temple0_Boss, value: false, this);
		UnlockFeat(isEndingD ? EGameFeatType.EndingD : EGameFeatType.EndingC);
		if (IsHardMode)
		{
			UnlockFeat(EGameFeatType.FinishGameHard);
		}
		CurrentLevel = 16;
		CurrentRoom = 5;
		CurrentCheckpoint = 1;
		LevelSaveBools.Clear();
		LevelSaveInts.Clear();
		DoesNeedSave = true;
	}

	internal void ResetForNewGamePlus(EGameDifficultyType difficulty)
	{
		CurrentLevel = 0;
		CurrentRoom = 3;
		CurrentCheckpoint = 0;
		LastWarpLevel = 0;
		LastWarpRoom = 0;
		LevelSaveBools.Clear();
		LevelSaveInts.Clear();
		List<int> list = new List<int>();
		Dictionary<int, InventoryRelic> inventory = Inventory.RelicInventory.Inventory;
		foreach (int key in inventory.Keys)
		{
			EInventoryRelicType eInventoryRelicType = (EInventoryRelicType)key;
			if (eInventoryRelicType != EInventoryRelicType.EmpireBrooch && eInventoryRelicType != EInventoryRelicType.JewelryBox && eInventoryRelicType != EInventoryRelicType.FoeScanner && eInventoryRelicType != EInventoryRelicType.Dash && eInventoryRelicType != EInventoryRelicType.FamiliarAltMeyef && eInventoryRelicType != EInventoryRelicType.FamiliarAltCrow && eInventoryRelicType != EInventoryRelicType.EternalBrooch)
			{
				list.Add(key);
			}
		}
		foreach (int item in list)
		{
			inventory.Remove(item);
		}
		Inventory.JournalCollection.Inventory.Clear();
		List<string> list2 = new List<string>();
		foreach (string key2 in DataKeyBools.Keys)
		{
			if (key2 != null && !key2.StartsWith("DROP_"))
			{
				list2.Add(key2);
			}
		}
		foreach (string item2 in list2)
		{
			DataKeyBools.Remove(item2);
		}
		DataKeyBools["IsGameCleared"] = true;
		DataKeyBools["IsEasyMode"] = false;
		DataKeyBools["IsHardMode"] = false;
		DataKeyBools["IsLevelCap1"] = false;
		DataKeyBools["IsLevelCap255"] = false;
		switch (difficulty)
		{
		case EGameDifficultyType.Easy:
			DataKeyBools["IsEasyMode"] = true;
			break;
		case EGameDifficultyType.Hard:
			DataKeyBools["IsHardMode"] = true;
			break;
		case EGameDifficultyType.HardCap1:
			DataKeyBools["IsHardMode"] = true;
			DataKeyBools["IsLevelCap1"] = true;
			break;
		case EGameDifficultyType.HardCap255:
			DataKeyBools["IsHardMode"] = true;
			DataKeyBools["IsLevelCap255"] = true;
			break;
		}
		int levelCap = GetLevelCap(this);
		CharacterStats.InitializePostLoad(levelCap);
		List<string> list3 = new List<string>();
		foreach (string key3 in DataKeyInts.Keys)
		{
			if (key3 != null && !key3.StartsWith("KILL_"))
			{
				list3.Add(key3);
			}
		}
		foreach (string item3 in list3)
		{
			DataKeyInts.Remove(item3);
		}
		DataKeyStrings.Clear();
		MinimapSave.ResetForNewGamePlus();
		Inventory.UseItemInventory.AddItem(16);
	}

	internal void ResetForSpeedrunB(EGameDifficultyType difficulty)
	{
		ResetForNewGamePlus(difficulty);
		CurrentRoom = 0;
		SetValue("IsActiveSpeedrunB", value: true);
		FlagSpeedrun(2);
		Inventory.UseItemInventory.Inventory.Clear();
		Inventory.UseItemInventory.AddItem(16);
		Inventory.UseItemInventory.AddItem(13, 9);
		Inventory.UseItemInventory.AddItem(5, 9);
		if (CharacterStats.Experience < 125000)
		{
			CharacterStats.Experience = 125000;
			CharacterStats.MaxHPFound = 100;
			CharacterStats.MaxAuraFound = 100;
			CharacterStats.MaxSandFound = 100;
			CharacterStats.InitializePostLoad(GetLevelCap(this));
		}
		Dictionary<int, InventoryFamiliar> inventory = Inventory.FamiliarInventory.Inventory;
		foreach (InventoryFamiliar value in inventory.Values)
		{
			value.Experience = 16000;
			value.RefreshFamiliarStats();
		}
		Dictionary<int, InventoryOrb> inventory2 = Inventory.OrbInventory.Inventory;
		foreach (InventoryOrb value2 in inventory2.Values)
		{
			value2.Experience = 50000;
			value2.RefreshOrbLevel();
		}
		UnlockRelic(EInventoryRelicType.EternalBrooch);
		Inventory.RelicInventory.Inventory[24].IsActive = true;
		UnlockRelic(EInventoryRelicType.Dash);
	}

	internal void Update()
	{
		_featsManager.Update();
	}

	public void FlagSpeedrun(int speedrunIndex)
	{
		switch (speedrunIndex)
		{
		case 1:
			SetValue("IsFlaggedSpeedrunA", value: true);
			SetValue("IsFlaggedSpeedrunB", value: false);
			break;
		case 2:
			SetValue("IsFlaggedSpeedrunA", value: false);
			SetValue("IsFlaggedSpeedrunB", value: true);
			break;
		}
		SetValue("IsGameCleared", value: true);
	}
}
