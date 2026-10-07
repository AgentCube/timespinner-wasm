using System.Collections.Generic;
using System.Xml;
using Timespinner.Core;

namespace Timespinner.GameAbstractions.Inventory;

public class PlayerInventory
{
	private const int MaxRecentlyEquippedListSize = 5;

	internal const int TotalMemories = 11;

	internal const int TotalLetters = 11;

	internal const int TotalFiles = 14;

	internal const string XmlInventoryNodeName = "Inventory";

	private const string XmlInventoryEquippedHelmetAttributeName = "Helmet";

	private const string XmlInventoryEquippedArmorAttributeName = "Armor";

	private const string XmlInventoryEquippedTrinketAAttributeName = "TrinketA";

	private const string XmlInventoryEquippedTrinketBAttributeName = "TrinketB";

	private const string XmlInventoryEquippedFamiliarAttributeName = "Familiar";

	private const string XmlInventoryEquippedOrbSetAttributeName = "OrbSet";

	private const string XmlInventoryRecentItemAttributeNamePrefix = "Entry";

	private const string XmlInventoryRecentMeleeOrbsNodeName = "RecentMelees";

	private const string XmlInventoryRecentSpellOrbsNodeName = "RecentSpells";

	private const string XmlInventoryRecentPassiveOrbsNodeName = "RecentPassives";

	private const string XmlInventoryRecentHelmetsNodeName = "RecentHelmets";

	private const string XmlInventoryRecentArmorsNodeName = "RecentArmors";

	private const string XmlInventoryRecentTrinketsNodeName = "RecentTrinkets";

	private const string XmlInventoryItemNodeName = "Item";

	private const string XmlInventoryOrbsNodeName = "Orbs";

	private const string XmlInventoryFamiliarsNodeName = "Familiars";

	private const string XmlInventoryRelicsNodeName = "Relics";

	private const string XmlInventoryUseItemsNodeName = "UseItems";

	private const string XmlInventoryEquipmentNodeName = "Equipment";

	private const string XmlInventoryJournalsNodeName = "Journals";

	private const string XmlInventoryItemKeyAttributeName = "Key";

	private const string XmlInventoryOrbsSpellUnlockedAttributeName = "Spell";

	private const string XmlInventoryOrbsPassiveUnlockedAttributeName = "Passive";

	private const string XmlInventoryItemExperienceAttributeName = "Exp";

	private const string XmlInventoryRelicIsActiveAttributeName = "IsActive";

	private const string XmlInventoryItemCountAttributeName = "Count";

	private const string XmlInventoryJournalIsReadAttributeName = "IsRead";

	private const string XmlInventoryOrbSetsNodeName = "OrbSets";

	private const string XmlInventoryOrbSetNodeName = "OrbSet";

	private const string XmlInventoryOrbSetMeleeAAttributeName = "MeleeA";

	private const string XmlInventoryOrbSetMeleeBAtributeName = "MeleeB";

	private const string XmlInventoryOrbSetSpellAttributeName = "Spell";

	private const string XmlInventoryOrbSetPassiveAttributeName = "Passive";

	private readonly List<EInventoryOrbType> _recentlyEquippedMeleeOrbs = new List<EInventoryOrbType>();

	private readonly List<EInventoryOrbType> _recentlyEquippedSpellOrbs = new List<EInventoryOrbType>();

	private readonly List<EInventoryOrbType> _recentlyEquippedPassiveOrbs = new List<EInventoryOrbType>();

	private readonly List<EInventoryEquipmentType> _recentlyEquippedHeadItems = new List<EInventoryEquipmentType>();

	private readonly List<EInventoryEquipmentType> _recentlyEquippedBodyItems = new List<EInventoryEquipmentType>();

	private readonly List<EInventoryEquipmentType> _recentlyEquippedTrinketItems = new List<EInventoryEquipmentType>();

	private readonly InventoryOrbCollection _orbInventory = new InventoryOrbCollection();

	private readonly InventoryFamiliarCollection _familiarInventory = new InventoryFamiliarCollection();

	private readonly InventoryRelicCollection _relicInventory = new InventoryRelicCollection();

	private readonly InventoryUseItemCollection _useItemInventory = new InventoryUseItemCollection();

	private readonly InventoryEquipmentCollection _equipmentInventory = new InventoryEquipmentCollection();

	private readonly InventoryJournalCollection _journalInventory = new InventoryJournalCollection();

	public EInventoryOrbType EquippedMeleeOrbA { get; set; }

	public EInventoryOrbType EquippedMeleeOrbB { get; set; }

	public EInventoryOrbType EquippedSpellOrb { get; set; }

	public EInventoryOrbType EquippedPassiveOrb { get; set; }

	public EInventoryEquipmentType EquippedHelmet { get; set; }

	public EInventoryEquipmentType EquippedArmor { get; set; }

	public EInventoryEquipmentType EquippedTrinketA { get; set; }

	public EInventoryEquipmentType EquippedTrinketB { get; set; }

	public EInventoryFamiliarType EquippedFamiliar { get; set; }

	public int EquippedOrbSetIndex { get; set; }

	public List<EInventoryOrbType> RecentlyEquippedMeleeOrbs => _recentlyEquippedMeleeOrbs;

	public List<EInventoryOrbType> RecentlyEquippedSpellOrbs => _recentlyEquippedSpellOrbs;

	public List<EInventoryOrbType> RecentlyEquippedPassiveOrbs => _recentlyEquippedPassiveOrbs;

	public List<EInventoryEquipmentType> RecentlyEquippedHeadItems => _recentlyEquippedHeadItems;

	public List<EInventoryEquipmentType> RecentlyEquippedBodyItems => _recentlyEquippedBodyItems;

	public List<EInventoryEquipmentType> RecentlyEquippedTrinketItems => _recentlyEquippedTrinketItems;

	public InventoryOrbCollection OrbInventory => _orbInventory;

	public InventoryFamiliarCollection FamiliarInventory => _familiarInventory;

	public InventoryRelicCollection RelicInventory => _relicInventory;

	public InventoryUseItemCollection UseItemInventory => _useItemInventory;

	public InventoryEquipmentCollection EquipmentInventory => _equipmentInventory;

	public InventoryJournalCollection JournalCollection => _journalInventory;

	public List<OrbSet> OrbSets { get; set; }

	public OrbSet EquippedOrbSet => OrbSets[EquippedOrbSetIndex];

	public PlayerInventory()
	{
		OrbSets = new List<OrbSet>();
	}

	internal void AddItem(EInventoryOrbType type, EOrbSlot slot)
	{
		_orbInventory.AddItem((int)type, slot);
	}

	internal void AddItem(EInventoryFamiliarType type)
	{
		_familiarInventory.AddItem((int)type);
	}

	internal void AddItem(EInventoryRelicType type)
	{
		_relicInventory.AddItem((int)type);
	}

	internal void AddItem(EInventoryUseItemType type, int amount)
	{
		_useItemInventory.AddItem((int)type, amount);
	}

	internal void AddItem(EInventoryJournalType type)
	{
		_journalInventory.AddItem((int)type);
	}

	internal void AddItem(EInventoryEquipmentType type)
	{
		_equipmentInventory.AddItem((int)type);
	}

	internal void AddToRecentlyEquipped(EInventoryOrbType orb, EOrbSlot slot)
	{
		List<EInventoryOrbType> list = null;
		switch (slot)
		{
		case EOrbSlot.Melee:
			list = _recentlyEquippedMeleeOrbs;
			break;
		case EOrbSlot.Spell:
			list = _recentlyEquippedSpellOrbs;
			break;
		case EOrbSlot.Passive:
			list = _recentlyEquippedPassiveOrbs;
			break;
		}
		if (list != null)
		{
			list.Remove(orb);
			list.Insert(0, orb);
			int count = list.Count;
			if (count > 5)
			{
				list.RemoveAt(count - 1);
			}
		}
	}

	internal void AddToRecentlyEquipped(EInventoryEquipmentType equipment, EEquipmentSlotType slot)
	{
		List<EInventoryEquipmentType> list = null;
		switch (slot)
		{
		case EEquipmentSlotType.Head:
			list = _recentlyEquippedHeadItems;
			break;
		case EEquipmentSlotType.Body:
			list = _recentlyEquippedBodyItems;
			break;
		case EEquipmentSlotType.Trinket:
			list = _recentlyEquippedTrinketItems;
			break;
		}
		if (list != null)
		{
			list.Remove(equipment);
			list.Insert(0, equipment);
			int count = list.Count;
			if (count > 5)
			{
				list.RemoveAt(count - 1);
			}
		}
	}

	internal void IntializePostLoad()
	{
		OrbInventory.InitializePostLoad();
		FamiliarInventory.InitializePostLoad();
		RefreshEquippedOrbs();
	}

	internal void RefreshEquippedOrbs()
	{
		if (OrbSets != null && EquippedOrbSetIndex < OrbSets.Count)
		{
			OrbSet equippedOrbSet = EquippedOrbSet;
			EquippedMeleeOrbA = equippedOrbSet.MeleeOrbA;
			EquippedMeleeOrbB = equippedOrbSet.MeleeOrbB;
			EquippedSpellOrb = equippedOrbSet.SpellOrb;
			EquippedPassiveOrb = equippedOrbSet.PassiveOrb;
		}
	}

	internal int[,] GetEquipmentStats()
	{
		int[,] array = new int[4, 4];
		for (int i = 0; i < 4; i++)
		{
			EInventoryEquipmentType eInventoryEquipmentType = EInventoryEquipmentType.None;
			switch (i)
			{
			case 0:
				eInventoryEquipmentType = EquippedHelmet;
				break;
			case 1:
				eInventoryEquipmentType = EquippedArmor;
				break;
			case 2:
				eInventoryEquipmentType = EquippedTrinketA;
				break;
			case 3:
				eInventoryEquipmentType = EquippedTrinketB;
				break;
			}
			if (eInventoryEquipmentType != 0)
			{
				int[] equipmentStats = InventoryEquipment.GetEquipmentStats(eInventoryEquipmentType);
				array[i, 0] = equipmentStats[0];
				array[i, 1] = equipmentStats[1];
				array[i, 2] = equipmentStats[2];
				array[i, 3] = equipmentStats[3];
			}
		}
		return array;
	}

	internal int GetEquipmentEquippedCount(int equipmentKey)
	{
		int num = 0;
		if (EquipmentInventory.Inventory.ContainsKey(equipmentKey))
		{
			InventoryEquipment inventoryEquipment = EquipmentInventory.Inventory[equipmentKey];
			switch (inventoryEquipment.SlotType)
			{
			case EEquipmentSlotType.Body:
				num = ((EquippedArmor == inventoryEquipment.EquipmentType) ? 1 : 0);
				break;
			case EEquipmentSlotType.Head:
				num = ((EquippedHelmet == inventoryEquipment.EquipmentType) ? 1 : 0);
				break;
			case EEquipmentSlotType.Trinket:
				if (EquippedTrinketA == inventoryEquipment.EquipmentType)
				{
					num++;
				}
				if (EquippedTrinketB == inventoryEquipment.EquipmentType)
				{
					num++;
				}
				break;
			}
		}
		return num;
	}

	internal int GetUseItemCount(EInventoryUseItemType itemType)
	{
		int result = 0;
		if (UseItemInventory.Inventory.ContainsKey((int)itemType))
		{
			result = UseItemInventory.Inventory[(int)itemType].Count;
		}
		return result;
	}

	internal void RefreshInventoryNamesAndDescriptions()
	{
		OrbInventory.RefreshItemNameAndDescriptions();
		FamiliarInventory.RefreshItemNameAndDescriptions();
		RelicInventory.RefreshItemNameAndDescriptions();
		UseItemInventory.RefreshItemNameAndDescriptions();
		EquipmentInventory.RefreshItemNameAndDescriptions();
		JournalCollection.RefreshItemNameAndDescriptions();
	}

	public void SaveXml(XmlNode root)
	{
		XmlNode node = root.AddElement("Inventory");
		node.AddAttributeNoDefault("Helmet", (int)EquippedHelmet);
		node.AddAttributeNoDefault("Armor", (int)EquippedArmor);
		node.AddAttributeNoDefault("TrinketA", (int)EquippedTrinketA);
		node.AddAttributeNoDefault("TrinketB", (int)EquippedTrinketB);
		node.AddAttributeNoDefault("Familiar", (int)EquippedFamiliar);
		node.AddAttributeNoDefault("OrbSet", EquippedOrbSetIndex);
		int num = 0;
		XmlNode node2 = node.AddElement("RecentMelees");
		foreach (EInventoryOrbType recentlyEquippedMeleeOrb in RecentlyEquippedMeleeOrbs)
		{
			node2.AddAttribute("Entry" + num, (int)recentlyEquippedMeleeOrb);
			num++;
		}
		num = 0;
		XmlNode node3 = node.AddElement("RecentSpells");
		foreach (EInventoryOrbType recentlyEquippedSpellOrb in RecentlyEquippedSpellOrbs)
		{
			node3.AddAttribute("Entry" + num, (int)recentlyEquippedSpellOrb);
			num++;
		}
		num = 0;
		XmlNode node4 = node.AddElement("RecentPassives");
		foreach (EInventoryOrbType recentlyEquippedPassiveOrb in RecentlyEquippedPassiveOrbs)
		{
			node4.AddAttribute("Entry" + num, (int)recentlyEquippedPassiveOrb);
			num++;
		}
		num = 0;
		XmlNode node5 = node.AddElement("RecentHelmets");
		foreach (EInventoryEquipmentType recentlyEquippedHeadItem in RecentlyEquippedHeadItems)
		{
			node5.AddAttribute("Entry" + num, (int)recentlyEquippedHeadItem);
			num++;
		}
		num = 0;
		XmlNode node6 = node.AddElement("RecentArmors");
		foreach (EInventoryEquipmentType recentlyEquippedBodyItem in RecentlyEquippedBodyItems)
		{
			node6.AddAttribute("Entry" + num, (int)recentlyEquippedBodyItem);
			num++;
		}
		num = 0;
		XmlNode node7 = node.AddElement("RecentTrinkets");
		foreach (EInventoryEquipmentType recentlyEquippedTrinketItem in RecentlyEquippedTrinketItems)
		{
			node7.AddAttribute("Entry" + num, (int)recentlyEquippedTrinketItem);
			num++;
		}
		XmlNode node8 = node.AddElement("Orbs");
		foreach (KeyValuePair<int, InventoryOrb> item in OrbInventory.Inventory)
		{
			InventoryOrb value = item.Value;
			XmlNode node9 = node8.AddElement("Item");
			node9.AddAttribute("Key", item.Key);
			node9.AddAttributeNoDefault("Spell", value.IsSpellUnlocked);
			node9.AddAttributeNoDefault("Passive", value.IsPassiveUnlocked);
			node9.AddAttributeNoDefault("Exp", value.Experience);
		}
		XmlNode node10 = node.AddElement("Familiars");
		foreach (KeyValuePair<int, InventoryFamiliar> item2 in FamiliarInventory.Inventory)
		{
			InventoryFamiliar value2 = item2.Value;
			XmlNode node11 = node10.AddElement("Item");
			node11.AddAttribute("Key", item2.Key);
			node11.AddAttributeNoDefault("Exp", value2.Experience);
		}
		XmlNode node12 = node.AddElement("Relics");
		foreach (KeyValuePair<int, InventoryRelic> item3 in RelicInventory.Inventory)
		{
			InventoryRelic value3 = item3.Value;
			XmlNode node13 = node12.AddElement("Item");
			node13.AddAttribute("Key", item3.Key);
			node13.AddAttributeNoDefault("IsActive", value3.IsActive);
		}
		XmlNode node14 = node.AddElement("UseItems");
		foreach (KeyValuePair<int, InventoryUseItem> item4 in UseItemInventory.Inventory)
		{
			InventoryUseItem value4 = item4.Value;
			XmlNode node15 = node14.AddElement("Item");
			node15.AddAttribute("Key", item4.Key);
			node15.AddAttributeNoDefault("Count", value4.Count);
		}
		XmlNode node16 = node.AddElement("Equipment");
		foreach (KeyValuePair<int, InventoryEquipment> item5 in EquipmentInventory.Inventory)
		{
			InventoryEquipment value5 = item5.Value;
			XmlNode node17 = node16.AddElement("Item");
			node17.AddAttribute("Key", item5.Key);
			node17.AddAttributeNoDefault("Count", value5.Count);
		}
		XmlNode node18 = node.AddElement("Journals");
		foreach (KeyValuePair<int, InventoryJournal> item6 in JournalCollection.Inventory)
		{
			InventoryJournal value6 = item6.Value;
			XmlNode node19 = node18.AddElement("Item");
			node19.AddAttribute("Key", item6.Key);
			node19.AddAttributeNoDefault("IsRead", value6.IsRead);
		}
		XmlNode node20 = node.AddElement("OrbSets");
		foreach (OrbSet orbSet in OrbSets)
		{
			XmlNode node21 = node20.AddElement("OrbSet");
			node21.AddAttributeNoDefault("MeleeA", (int)orbSet.MeleeOrbA);
			node21.AddAttributeNoDefault("MeleeB", (int)orbSet.MeleeOrbB);
			node21.AddAttributeNoDefault("Spell", (int)orbSet.SpellOrb);
			node21.AddAttributeNoDefault("Passive", (int)orbSet.PassiveOrb);
		}
	}

	public static void LoadXml(XmlReader reader, PlayerInventory inventory)
	{
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Invalid comparison between Unknown and I4
		//IL_0659: Unknown result type (might be due to invalid IL or missing references)
		//IL_0660: Invalid comparison between Unknown and I4
		while (reader.MoveToNextAttribute())
		{
			switch (reader.Name)
			{
			case "Helmet":
				inventory.EquippedHelmet = (EInventoryEquipmentType)reader.Value.ParseInt32();
				break;
			case "Armor":
				inventory.EquippedArmor = (EInventoryEquipmentType)reader.Value.ParseInt32();
				break;
			case "TrinketA":
				inventory.EquippedTrinketA = (EInventoryEquipmentType)reader.Value.ParseInt32();
				break;
			case "TrinketB":
				inventory.EquippedTrinketB = (EInventoryEquipmentType)reader.Value.ParseInt32();
				break;
			case "Familiar":
				inventory.EquippedFamiliar = (EInventoryFamiliarType)reader.Value.ParseInt32();
				break;
			case "OrbSet":
				inventory.EquippedOrbSetIndex = reader.Value.ParseInt32();
				break;
			}
		}
		bool flag = false;
		EInventoryCategoryType eInventoryCategoryType = EInventoryCategoryType.Orb;
		while (!flag && reader.Read())
		{
			if ((int)reader.NodeType == 1)
			{
				switch (reader.LocalName)
				{
				case "RecentMelees":
					while (reader.MoveToNextAttribute())
					{
						inventory.RecentlyEquippedMeleeOrbs.Add((EInventoryOrbType)reader.Value.ParseInt32());
					}
					break;
				case "RecentSpells":
					while (reader.MoveToNextAttribute())
					{
						inventory.RecentlyEquippedSpellOrbs.Add((EInventoryOrbType)reader.Value.ParseInt32());
					}
					break;
				case "RecentPassives":
					while (reader.MoveToNextAttribute())
					{
						inventory.RecentlyEquippedPassiveOrbs.Add((EInventoryOrbType)reader.Value.ParseInt32());
					}
					break;
				case "RecentHelmets":
					while (reader.MoveToNextAttribute())
					{
						inventory.RecentlyEquippedHeadItems.Add((EInventoryEquipmentType)reader.Value.ParseInt32());
					}
					break;
				case "RecentArmors":
					while (reader.MoveToNextAttribute())
					{
						inventory.RecentlyEquippedBodyItems.Add((EInventoryEquipmentType)reader.Value.ParseInt32());
					}
					break;
				case "RecentTrinkets":
					while (reader.MoveToNextAttribute())
					{
						inventory.RecentlyEquippedTrinketItems.Add((EInventoryEquipmentType)reader.Value.ParseInt32());
					}
					break;
				case "Orbs":
					eInventoryCategoryType = EInventoryCategoryType.Orb;
					break;
				case "Familiars":
					eInventoryCategoryType = EInventoryCategoryType.Familiar;
					break;
				case "Relics":
					eInventoryCategoryType = EInventoryCategoryType.Relic;
					break;
				case "UseItems":
					eInventoryCategoryType = EInventoryCategoryType.UseItem;
					break;
				case "Equipment":
					eInventoryCategoryType = EInventoryCategoryType.Equipment;
					break;
				case "Journals":
					eInventoryCategoryType = EInventoryCategoryType.Journal;
					break;
				case "Item":
				{
					int num = 0;
					int experience = 0;
					int num2 = 0;
					bool isSpellUnlocked = false;
					bool isPassiveUnlocked = false;
					bool isActive = false;
					bool isRead = false;
					while (reader.MoveToNextAttribute())
					{
						switch (reader.Name)
						{
						case "Key":
							num = reader.Value.ParseInt32();
							break;
						case "Exp":
							experience = reader.Value.ParseInt32();
							break;
						case "Count":
							num2 = reader.Value.ParseInt32();
							break;
						case "Spell":
							isSpellUnlocked = bool.Parse(reader.Value);
							break;
						case "Passive":
							isPassiveUnlocked = bool.Parse(reader.Value);
							break;
						case "IsActive":
							isActive = bool.Parse(reader.Value);
							break;
						case "IsRead":
							isRead = bool.Parse(reader.Value);
							break;
						}
					}
					switch (eInventoryCategoryType)
					{
					case EInventoryCategoryType.UseItem:
						inventory.UseItemInventory.AddItem(num, num2);
						break;
					case EInventoryCategoryType.Orb:
						inventory.OrbInventory.Inventory[num] = new InventoryOrb((EInventoryOrbType)num)
						{
							IsSpellUnlocked = isSpellUnlocked,
							IsPassiveUnlocked = isPassiveUnlocked,
							Experience = experience
						};
						break;
					case EInventoryCategoryType.Familiar:
						inventory.FamiliarInventory.Inventory[num] = new InventoryFamiliar((EInventoryFamiliarType)num, inventory.FamiliarInventory.GetSumOfFamiliarLevels)
						{
							Experience = experience
						};
						break;
					case EInventoryCategoryType.Relic:
						inventory.RelicInventory.Inventory[num] = new InventoryRelic((EInventoryRelicType)num)
						{
							IsActive = isActive
						};
						break;
					case EInventoryCategoryType.Journal:
						inventory.JournalCollection.Inventory[num] = new InventoryJournal((EInventoryJournalType)num)
						{
							IsRead = isRead
						};
						break;
					case EInventoryCategoryType.Equipment:
						inventory.EquipmentInventory.Inventory[num] = new InventoryEquipment((EInventoryEquipmentType)num)
						{
							Count = num2
						};
						break;
					}
					break;
				}
				case "OrbSet":
				{
					OrbSet orbSet = new OrbSet();
					while (reader.MoveToNextAttribute())
					{
						switch (reader.Name)
						{
						case "MeleeA":
							orbSet.MeleeOrbA = (EInventoryOrbType)reader.Value.ParseInt32();
							break;
						case "MeleeB":
							orbSet.MeleeOrbB = (EInventoryOrbType)reader.Value.ParseInt32();
							break;
						case "Spell":
							orbSet.SpellOrb = (EInventoryOrbType)reader.Value.ParseInt32();
							break;
						case "Passive":
							orbSet.PassiveOrb = (EInventoryOrbType)reader.Value.ParseInt32();
							break;
						}
					}
					inventory.OrbSets.Add(orbSet);
					break;
				}
				default:
					flag = true;
					break;
				case "OrbSets":
					break;
				}
			}
			else if ((int)reader.NodeType == 15 && reader.Name == "Inventory")
			{
				flag = true;
			}
		}
	}
}
