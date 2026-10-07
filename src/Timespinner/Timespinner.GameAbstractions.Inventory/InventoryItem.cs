using Timespinner.Core.Localization;

namespace Timespinner.GameAbstractions.Inventory;

public abstract class InventoryItem
{
	public const string ItemLocPrefix = "inv_";

	public const string ItemLocDescriptionPostfix = "_desc";

	public const string OrbLocPrefix = "orb_";

	internal const string OrbSpellPrefix = "_spell";

	internal const string OrbPassivePrefix = "_passive";

	public const string FamiliarLocPrefix = "fam_";

	public const string RelicLocPrefix = "rel_";

	public const string UseItemLocPrefix = "use_";

	public const string EquipmentLocPrefix = "eq_";

	internal const string JournalLocPrefix = "jou_";

	internal const string JournalContentPostfix = "_cont";

	public const string EmptySlotText = "---";

	public abstract EInventoryCategoryType Category { get; }

	public abstract int Key { get; }

	public bool IsSellable { get; set; }

	public int MonetaryValue { get; set; }

	public string Name { get; protected set; }

	public string NameKey { get; private set; }

	public string Description { get; private set; }

	public string DescriptionKey { get; private set; }

	protected InventoryItem(string nameKey)
	{
		NameKey = "inv_" + nameKey;
		Name = Loc.Get(NameKey);
		DescriptionKey = NameKey + "_desc";
		Description = Loc.Get(DescriptionKey);
		IsSellable = true;
	}

	internal void RefreshNameAndDescription()
	{
		Name = Loc.Get(NameKey);
		Description = Loc.Get(DescriptionKey);
	}

	internal static string NameFromType(EInventoryFamiliarType type)
	{
		if (type != 0)
		{
			return Loc.Get("inv_fam_" + type);
		}
		return "---";
	}

	internal static string NameFromType(EInventoryRelicType type)
	{
		if (type != 0)
		{
			return Loc.Get("inv_rel_" + type);
		}
		return "---";
	}

	internal static string NameFromType(EInventoryUseItemType type)
	{
		if (type != 0)
		{
			return Loc.Get("inv_use_" + type);
		}
		return "---";
	}

	internal static string NameFromType(EInventoryEquipmentType type)
	{
		if (type != 0)
		{
			return Loc.Get("inv_eq_" + type);
		}
		return "---";
	}

	internal static string NameFromType(EInventoryJournalType type)
	{
		if (type != 0)
		{
			return Loc.Get("inv_jou_" + type);
		}
		return "---";
	}

	internal static string NameFromType(EInventoryOrbType type, EOrbSlot slot)
	{
		if (type == EInventoryOrbType.None)
		{
			return "---";
		}
		return slot switch
		{
			EOrbSlot.Spell => Loc.Get(string.Concat("inv_orb_", type, "_spell")), 
			EOrbSlot.Passive => Loc.Get(string.Concat("inv_orb_", type, "_passive")), 
			_ => Loc.Get("inv_orb_" + type), 
		};
	}

	internal static string DescriptionFromType(EInventoryFamiliarType type)
	{
		if (type != 0)
		{
			return Loc.Get(string.Concat("inv_fam_", type, "_desc"));
		}
		return "";
	}

	internal static string DescriptionFromType(EInventoryRelicType type)
	{
		if (type != 0)
		{
			return Loc.Get(string.Concat("inv_rel_", type, "_desc"));
		}
		return "";
	}

	internal static string DescriptionFromType(EInventoryUseItemType type)
	{
		if (type != 0)
		{
			return Loc.Get(string.Concat("inv_use_", type, "_desc"));
		}
		return "";
	}

	internal static string DescriptionFromType(EInventoryEquipmentType type)
	{
		if (type != 0)
		{
			return Loc.Get(string.Concat("inv_eq_", type, "_desc"));
		}
		return "";
	}

	internal static string DescriptionFromType(EInventoryJournalType type)
	{
		if (type != 0)
		{
			return Loc.Get(string.Concat("inv_jou_", type, "_desc"));
		}
		return "";
	}

	internal static string DescriptionFromType(EInventoryOrbType type, EOrbSlot slot)
	{
		if (type == EInventoryOrbType.None)
		{
			return "";
		}
		return slot switch
		{
			EOrbSlot.Spell => Loc.Get(string.Concat("inv_orb_", type, "_spell_desc")), 
			EOrbSlot.Passive => Loc.Get(string.Concat("inv_orb_", type, "_passive_desc")), 
			_ => Loc.Get(string.Concat("inv_orb_", type, "_desc")), 
		};
	}

	internal static EInventoryItemIcon GetIconFromItem(InventoryItem item)
	{
		EInventoryItemIcon result = EInventoryItemIcon.None;
		switch (item.Category)
		{
		case EInventoryCategoryType.Relic:
			result = GetIconFromItem((EInventoryRelicType)item.Key);
			break;
		case EInventoryCategoryType.UseItem:
			result = GetIconFromItem((EInventoryUseItemType)item.Key);
			break;
		case EInventoryCategoryType.Equipment:
			result = GetIconFromItem((EInventoryEquipmentType)item.Key);
			break;
		case EInventoryCategoryType.Orb:
			result = GetIconFromItem((EInventoryOrbType)item.Key, EOrbSlot.All);
			break;
		case EInventoryCategoryType.Journal:
			result = GetIconFromItem((EInventoryJournalType)item.Key);
			break;
		}
		return result;
	}

	internal static EInventoryItemIcon GetIconFromItem(EInventoryRelicType relicType)
	{
		EInventoryItemIcon result = EInventoryItemIcon.None;
		switch (relicType)
		{
		case EInventoryRelicType.TimespinnerWheel:
			result = EInventoryItemIcon.TimespinnerWheel;
			break;
		case EInventoryRelicType.TimespinnerSpindle:
			result = EInventoryItemIcon.TimespinnerSpindle;
			break;
		case EInventoryRelicType.TimespinnerGear1:
			result = EInventoryItemIcon.TimespinnerGear1;
			break;
		case EInventoryRelicType.TimespinnerGear2:
			result = EInventoryItemIcon.TimespinnerGear2;
			break;
		case EInventoryRelicType.TimespinnerGear3:
			result = EInventoryItemIcon.TimespinnerGear3;
			break;
		case EInventoryRelicType.PyramidsKey:
			result = EInventoryItemIcon.PyramidsKey;
			break;
		case EInventoryRelicType.EssenceOfSpace:
			result = EInventoryItemIcon.EssenceOfSpace;
			break;
		case EInventoryRelicType.DoubleJump:
			result = EInventoryItemIcon.SuccusbusHairpin;
			break;
		case EInventoryRelicType.Dash:
			result = EInventoryItemIcon.Talaria;
			break;
		case EInventoryRelicType.WaterMask:
			result = EInventoryItemIcon.WaterMask;
			break;
		case EInventoryRelicType.AirMask:
			result = EInventoryItemIcon.AirMask;
			break;
		case EInventoryRelicType.FoeScanner:
			result = EInventoryItemIcon.FoeScanner;
			break;
		case EInventoryRelicType.ScienceKeycardA:
			result = EInventoryItemIcon.ScienceKeycardA;
			break;
		case EInventoryRelicType.ScienceKeycardB:
			result = EInventoryItemIcon.ScienceKeycardB;
			break;
		case EInventoryRelicType.ScienceKeycardC:
			result = EInventoryItemIcon.ScienceKeycardC;
			break;
		case EInventoryRelicType.ScienceKeycardD:
			result = EInventoryItemIcon.ScienceKeycardD;
			break;
		case EInventoryRelicType.ScienceKeycardV:
			result = EInventoryItemIcon.ScienceKeycardV;
			break;
		case EInventoryRelicType.Tablet:
			result = EInventoryItemIcon.Tablet;
			break;
		case EInventoryRelicType.ElevatorKeycard:
			result = EInventoryItemIcon.ElevatorKeycard;
			break;
		case EInventoryRelicType.JewelryBox:
			result = EInventoryItemIcon.JewelryBox;
			break;
		case EInventoryRelicType.EmpireBrooch:
			result = EInventoryItemIcon.EmpireBrooch;
			break;
		case EInventoryRelicType.FamiliarAltMeyef:
			result = EInventoryItemIcon.FamiliarAltMeyef;
			break;
		case EInventoryRelicType.FamiliarAltCrow:
			result = EInventoryItemIcon.FamiliarAltCrow;
			break;
		case EInventoryRelicType.EternalBrooch:
			result = EInventoryItemIcon.EternalBrooch;
			break;
		}
		return result;
	}

	internal static EInventoryItemIcon GetIconFromItem(EInventoryUseItemType itemType)
	{
		EInventoryItemIcon result = EInventoryItemIcon.None;
		switch (itemType)
		{
		case EInventoryUseItemType.Potion:
			result = EInventoryItemIcon.Potion;
			break;
		case EInventoryUseItemType.Ether:
			result = EInventoryItemIcon.Ether;
			break;
		case EInventoryUseItemType.SandBottle:
			result = EInventoryItemIcon.SandBottle;
			break;
		case EInventoryUseItemType.HiPotion:
			result = EInventoryItemIcon.HiPotion;
			break;
		case EInventoryUseItemType.HiEther:
			result = EInventoryItemIcon.HiEther;
			break;
		case EInventoryUseItemType.HiSandBottle:
			result = EInventoryItemIcon.HiSandBottle;
			break;
		case EInventoryUseItemType.FuturePotion:
			result = EInventoryItemIcon.FuturePotion;
			break;
		case EInventoryUseItemType.FutureHiPotion:
			result = EInventoryItemIcon.FutureHiPotion;
			break;
		case EInventoryUseItemType.FutureEther:
			result = EInventoryItemIcon.FutureEther;
			break;
		case EInventoryUseItemType.FutureHiEther:
			result = EInventoryItemIcon.FutureHiEther;
			break;
		case EInventoryUseItemType.Antidote:
			result = EInventoryItemIcon.Antidote;
			break;
		case EInventoryUseItemType.ChaosHeal:
			result = EInventoryItemIcon.ChaosHeal;
			break;
		case EInventoryUseItemType.WarpCard:
			result = EInventoryItemIcon.WarpCard;
			break;
		case EInventoryUseItemType.FamiliarTreat:
			result = EInventoryItemIcon.FamiliarTreat;
			break;
		case EInventoryUseItemType.PlaceHolderItem1:
			result = EInventoryItemIcon.HistoricalDocuments;
			break;
		case EInventoryUseItemType.LachiemiSun:
			result = EInventoryItemIcon.LachiemiSun;
			break;
		case EInventoryUseItemType.Jerky:
			result = EInventoryItemIcon.Jerky;
			break;
		case EInventoryUseItemType.Biscuit:
			result = EInventoryItemIcon.Biscuit;
			break;
		case EInventoryUseItemType.FriedCheveux:
			result = EInventoryItemIcon.FriedCheveux;
			break;
		case EInventoryUseItemType.SauteedTail:
			result = EInventoryItemIcon.SauteedTail;
			break;
		case EInventoryUseItemType.UnagiRoll:
			result = EInventoryItemIcon.UnagiRoll;
			break;
		case EInventoryUseItemType.CheveuxAuVin:
			result = EInventoryItemIcon.CheveuxAuVin;
			break;
		case EInventoryUseItemType.Casserole:
			result = EInventoryItemIcon.Casserole;
			break;
		case EInventoryUseItemType.Spaghetti:
			result = EInventoryItemIcon.Spaghetti;
			break;
		case EInventoryUseItemType.PlumpMaggot:
			result = EInventoryItemIcon.PlumpMaggot;
			break;
		case EInventoryUseItemType.OrangeJuice:
			result = EInventoryItemIcon.OrangeJuice;
			break;
		case EInventoryUseItemType.FiligreeTea:
			result = EInventoryItemIcon.FiligreeTea;
			break;
		case EInventoryUseItemType.EmpressCake:
			result = EInventoryItemIcon.EmpressCake;
			break;
		case EInventoryUseItemType.RottenTail:
			result = EInventoryItemIcon.RottenTail;
			break;
		case EInventoryUseItemType.AlchemistTools:
			result = EInventoryItemIcon.AlchemistTools;
			break;
		case EInventoryUseItemType.GalaxyStone:
			result = EInventoryItemIcon.GalaxyStone;
			break;
		case EInventoryUseItemType.MagicMarbles:
			result = EInventoryItemIcon.MagicMarbles;
			break;
		case EInventoryUseItemType.EssenceCrystal:
			result = EInventoryItemIcon.EssenceCrystal;
			break;
		case EInventoryUseItemType.GoldRing:
			result = EInventoryItemIcon.GoldRing;
			break;
		case EInventoryUseItemType.GoldNecklace:
			result = EInventoryItemIcon.GoldNecklace;
			break;
		case EInventoryUseItemType.Herb:
			result = EInventoryItemIcon.Herb;
			break;
		case EInventoryUseItemType.Mushroom:
			result = EInventoryItemIcon.Mushroom;
			break;
		case EInventoryUseItemType.RadiationCrystal:
			result = EInventoryItemIcon.RadiationCrystal;
			break;
		case EInventoryUseItemType.PlasmaIV:
			result = EInventoryItemIcon.PlasmaIV;
			break;
		case EInventoryUseItemType.Drumstick:
			result = EInventoryItemIcon.Drumstick;
			break;
		case EInventoryUseItemType.WyvernTail:
			result = EInventoryItemIcon.WyvernTail;
			break;
		case EInventoryUseItemType.EelMeat:
			result = EInventoryItemIcon.FishMeat;
			break;
		case EInventoryUseItemType.CheveuxBreast:
			result = EInventoryItemIcon.CheveuxBreast;
			break;
		case EInventoryUseItemType.FoodSynth:
			result = EInventoryItemIcon.FoodSynth;
			break;
		case EInventoryUseItemType.CheveuxFeather:
			result = EInventoryItemIcon.CheveuxFeather;
			break;
		case EInventoryUseItemType.SirenInk:
			result = EInventoryItemIcon.SirenInk;
			break;
		case EInventoryUseItemType.PlasmaCore:
			result = EInventoryItemIcon.PlasmaCore;
			break;
		case EInventoryUseItemType.SilverOre:
			result = EInventoryItemIcon.SilverOre;
			break;
		case EInventoryUseItemType.HistoricalDocuments:
			result = EInventoryItemIcon.HistoricalDocuments;
			break;
		}
		return result;
	}

	internal static EInventoryItemIcon GetIconFromItem(EInventoryEquipmentType itemType)
	{
		EInventoryItemIcon result = EInventoryItemIcon.None;
		switch (itemType)
		{
		case EInventoryEquipmentType.Sunglasses:
			result = EInventoryItemIcon.Sunglasses;
			break;
		case EInventoryEquipmentType.SecurityVisor:
			result = EInventoryItemIcon.SecurityVisor;
			break;
		case EInventoryEquipmentType.LeatherHelmet:
			result = EInventoryItemIcon.LeatherHelmet;
			break;
		case EInventoryEquipmentType.PointyHat:
			result = EInventoryItemIcon.PointyHat;
			break;
		case EInventoryEquipmentType.CopperHelmet:
			result = EInventoryItemIcon.CopperHelmet;
			break;
		case EInventoryEquipmentType.EngineerGoggles:
			result = EInventoryItemIcon.EngineerGoggles;
			break;
		case EInventoryEquipmentType.CalvaryHelmet:
			result = EInventoryItemIcon.CalvaryHelmet;
			break;
		case EInventoryEquipmentType.BuckleHat:
			result = EInventoryItemIcon.BuckleHat;
			break;
		case EInventoryEquipmentType.AdvisorHat:
			result = EInventoryItemIcon.AdvisorHat;
			break;
		case EInventoryEquipmentType.LibrarianHat:
			result = EInventoryItemIcon.LibrarianHat;
			break;
		case EInventoryEquipmentType.CombatHelmet:
			result = EInventoryItemIcon.CombatHelmet;
			break;
		case EInventoryEquipmentType.CaptainsCap:
			result = EInventoryItemIcon.CaptainsCap;
			break;
		case EInventoryEquipmentType.LabGlasses:
			result = EInventoryItemIcon.LabGlasses;
			break;
		case EInventoryEquipmentType.LachiemCrown:
			result = EInventoryItemIcon.LachiemCrown;
			break;
		case EInventoryEquipmentType.VileteCrown:
			result = EInventoryItemIcon.VileteCrown;
			break;
		case EInventoryEquipmentType.EternalTiara:
			result = EInventoryItemIcon.EternalTiara;
			break;
		case EInventoryEquipmentType.OldCoat:
			result = EInventoryItemIcon.OldCoat;
			break;
		case EInventoryEquipmentType.TrendyJacket:
			result = EInventoryItemIcon.TrendyJacket;
			break;
		case EInventoryEquipmentType.SecurityVest:
			result = EInventoryItemIcon.SecurityVest;
			break;
		case EInventoryEquipmentType.LeatherArmor:
			result = EInventoryItemIcon.LeatherArmor;
			break;
		case EInventoryEquipmentType.TravelersCloak:
			result = EInventoryItemIcon.TravelersCloak;
			break;
		case EInventoryEquipmentType.CopperArmor:
			result = EInventoryItemIcon.CopperArmor;
			break;
		case EInventoryEquipmentType.CalvaryArmor:
			result = EInventoryItemIcon.CalvaryArmor;
			break;
		case EInventoryEquipmentType.MidnightCloak:
			result = EInventoryItemIcon.MidnightCloak;
			break;
		case EInventoryEquipmentType.AdvisorRobe:
			result = EInventoryItemIcon.AdvisorRobe;
			break;
		case EInventoryEquipmentType.LibrarianRobe:
			result = EInventoryItemIcon.LibrarianRobe;
			break;
		case EInventoryEquipmentType.MilitaryArmor:
			result = EInventoryItemIcon.MilitaryArmor;
			break;
		case EInventoryEquipmentType.CaptainsJacket:
			result = EInventoryItemIcon.CaptainsJacket;
			break;
		case EInventoryEquipmentType.LabCoat:
			result = EInventoryItemIcon.LabCoat;
			break;
		case EInventoryEquipmentType.EmpressCoat:
			result = EInventoryItemIcon.EmpressCoat;
			break;
		case EInventoryEquipmentType.VileteDress:
			result = EInventoryItemIcon.VileteDress;
			break;
		case EInventoryEquipmentType.EternalCoat:
			result = EInventoryItemIcon.EternalCoat;
			break;
		case EInventoryEquipmentType.SyntheticPlume:
			result = EInventoryItemIcon.SyntheticPlume;
			break;
		case EInventoryEquipmentType.CheveuxPlume:
			result = EInventoryItemIcon.CheveuxPlume;
			break;
		case EInventoryEquipmentType.MetalWristband:
			result = EInventoryItemIcon.MetalWristband;
			break;
		case EInventoryEquipmentType.SirenHairband:
			result = EInventoryItemIcon.SirenHairband;
			break;
		case EInventoryEquipmentType.MotherOfPearl:
			result = EInventoryItemIcon.MotherOfPearl;
			break;
		case EInventoryEquipmentType.BirdStatue:
			result = EInventoryItemIcon.BirdStatue;
			break;
		case EInventoryEquipmentType.DemonStole:
			result = EInventoryItemIcon.DemonStole;
			break;
		case EInventoryEquipmentType.Pendulum:
			result = EInventoryItemIcon.Pendulum;
			break;
		case EInventoryEquipmentType.DemonHorn:
			result = EInventoryItemIcon.DemonHorn;
			break;
		case EInventoryEquipmentType.FiligreeClasp:
			result = EInventoryItemIcon.FiligreeClasp;
			break;
		case EInventoryEquipmentType.AzureStole:
			result = EInventoryItemIcon.AzureStole;
			break;
		case EInventoryEquipmentType.LuckyCoin:
			result = EInventoryItemIcon.LuckyCoin;
			break;
		case EInventoryEquipmentType.ShinyRock:
			result = EInventoryItemIcon.ShinyRock;
			break;
		case EInventoryEquipmentType.NelisteEarring:
			result = EInventoryItemIcon.NelisteEarring;
			break;
		case EInventoryEquipmentType.SelenBangle:
			result = EInventoryItemIcon.SelenBangle;
			break;
		case EInventoryEquipmentType.GlassPumpkin:
			result = EInventoryItemIcon.GlassPumpkin;
			break;
		case EInventoryEquipmentType.FamiliarEgg:
			result = EInventoryItemIcon.FamiliarEgg;
			break;
		}
		return result;
	}

	internal static EInventoryItemIcon GetIconFromItem(EInventoryFamiliarType familiarType)
	{
		EInventoryItemIcon result = EInventoryItemIcon.None;
		switch (familiarType)
		{
		case EInventoryFamiliarType.Meyef:
			result = EInventoryItemIcon.FamiliarMeyef;
			break;
		case EInventoryFamiliarType.Griffin:
			result = EInventoryItemIcon.FamiliarGriffin;
			break;
		case EInventoryFamiliarType.MerchantCrow:
			result = EInventoryItemIcon.FamiliarMerchantCrow;
			break;
		case EInventoryFamiliarType.Kobo:
			result = EInventoryItemIcon.FamiliarKobo;
			break;
		case EInventoryFamiliarType.Sprite:
			result = EInventoryItemIcon.FamiliarSprite;
			break;
		case EInventoryFamiliarType.Demon:
			result = EInventoryItemIcon.FamiliarDemon;
			break;
		}
		return result;
	}

	internal static EInventoryItemIcon GetIconFromItem(EInventoryOrbType orbType, EOrbSlot slot)
	{
		EInventoryItemIcon result = EInventoryItemIcon.None;
		switch (slot)
		{
		case EOrbSlot.Spell:
			switch (orbType)
			{
			case EInventoryOrbType.Blue:
				result = EInventoryItemIcon.BlueSpell;
				break;
			case EInventoryOrbType.Blade:
				result = EInventoryItemIcon.GreenSpell;
				break;
			case EInventoryOrbType.Flame:
				result = EInventoryItemIcon.RedSpell;
				break;
			case EInventoryOrbType.Pink:
				result = EInventoryItemIcon.PinkSpell;
				break;
			case EInventoryOrbType.Iron:
				result = EInventoryItemIcon.IronSpell;
				break;
			case EInventoryOrbType.Ice:
				result = EInventoryItemIcon.WaterSpell;
				break;
			case EInventoryOrbType.Wind:
				result = EInventoryItemIcon.WindSpell;
				break;
			case EInventoryOrbType.Gun:
				result = EInventoryItemIcon.GunSpell;
				break;
			case EInventoryOrbType.Umbra:
				result = EInventoryItemIcon.UmbraSpell;
				break;
			case EInventoryOrbType.Empire:
				result = EInventoryItemIcon.EmpireSpell;
				break;
			case EInventoryOrbType.Eye:
				result = EInventoryItemIcon.EyeSpell;
				break;
			case EInventoryOrbType.Blood:
				result = EInventoryItemIcon.BloodSpell;
				break;
			case EInventoryOrbType.Book:
				result = EInventoryItemIcon.BookSpell;
				break;
			case EInventoryOrbType.Moon:
				result = EInventoryItemIcon.MoonSpell;
				break;
			case EInventoryOrbType.Nether:
				result = EInventoryItemIcon.NetherSpell;
				break;
			case EInventoryOrbType.Barrier:
				result = EInventoryItemIcon.PrismSpell;
				break;
			case EInventoryOrbType.Monske:
				result = EInventoryItemIcon.MonskeSpell;
				break;
			}
			break;
		case EOrbSlot.Passive:
			switch (orbType)
			{
			case EInventoryOrbType.Blue:
				result = EInventoryItemIcon.BlueRing;
				break;
			case EInventoryOrbType.Blade:
				result = EInventoryItemIcon.GreenRing;
				break;
			case EInventoryOrbType.Flame:
				result = EInventoryItemIcon.RedRing;
				break;
			case EInventoryOrbType.Pink:
				result = EInventoryItemIcon.PinkRing;
				break;
			case EInventoryOrbType.Iron:
				result = EInventoryItemIcon.IronRing;
				break;
			case EInventoryOrbType.Ice:
				result = EInventoryItemIcon.WaterRing;
				break;
			case EInventoryOrbType.Wind:
				result = EInventoryItemIcon.WindRing;
				break;
			case EInventoryOrbType.Gun:
				result = EInventoryItemIcon.GunRing;
				break;
			case EInventoryOrbType.Umbra:
				result = EInventoryItemIcon.UmbraRing;
				break;
			case EInventoryOrbType.Empire:
				result = EInventoryItemIcon.EmpireRing;
				break;
			case EInventoryOrbType.Eye:
				result = EInventoryItemIcon.EyeRing;
				break;
			case EInventoryOrbType.Blood:
				result = EInventoryItemIcon.BloodRing;
				break;
			case EInventoryOrbType.Book:
				result = EInventoryItemIcon.BookRing;
				break;
			case EInventoryOrbType.Moon:
				result = EInventoryItemIcon.MoonRing;
				break;
			case EInventoryOrbType.Nether:
				result = EInventoryItemIcon.NetherRing;
				break;
			case EInventoryOrbType.Barrier:
				result = EInventoryItemIcon.PrismRing;
				break;
			case EInventoryOrbType.Monske:
				result = EInventoryItemIcon.MonskeRing;
				break;
			}
			break;
		default:
			switch (orbType)
			{
			case EInventoryOrbType.Blue:
				result = EInventoryItemIcon.BlueOrb;
				break;
			case EInventoryOrbType.Blade:
				result = EInventoryItemIcon.GreenOrb;
				break;
			case EInventoryOrbType.Flame:
				result = EInventoryItemIcon.RedOrb;
				break;
			case EInventoryOrbType.Pink:
				result = EInventoryItemIcon.PinkOrb;
				break;
			case EInventoryOrbType.Iron:
				result = EInventoryItemIcon.IronOrb;
				break;
			case EInventoryOrbType.Ice:
				result = EInventoryItemIcon.IceOrb;
				break;
			case EInventoryOrbType.Wind:
				result = EInventoryItemIcon.WindOrb;
				break;
			case EInventoryOrbType.Gun:
				result = EInventoryItemIcon.GunOrb;
				break;
			case EInventoryOrbType.Umbra:
				result = EInventoryItemIcon.UmbraOrb;
				break;
			case EInventoryOrbType.Empire:
				result = EInventoryItemIcon.EmpireOrb;
				break;
			case EInventoryOrbType.Eye:
				result = EInventoryItemIcon.EyeOrb;
				break;
			case EInventoryOrbType.Blood:
				result = EInventoryItemIcon.BloodOrb;
				break;
			case EInventoryOrbType.Book:
				result = EInventoryItemIcon.BookOrb;
				break;
			case EInventoryOrbType.Moon:
				result = EInventoryItemIcon.MoonOrb;
				break;
			case EInventoryOrbType.Nether:
				result = EInventoryItemIcon.NetherOrb;
				break;
			case EInventoryOrbType.Barrier:
				result = EInventoryItemIcon.BarrierOrb;
				break;
			case EInventoryOrbType.Monske:
				result = EInventoryItemIcon.MonskeOrb;
				break;
			}
			break;
		}
		return result;
	}

	internal static EInventoryItemIcon GetIconFromItem(EInventoryJournalType journalType)
	{
		if (journalType < EInventoryJournalType.Letter0)
		{
			return EInventoryItemIcon.JournalMemory;
		}
		if (journalType < EInventoryJournalType.File0)
		{
			return (journalType == EInventoryJournalType.Letter10) ? EInventoryItemIcon.JournalLetterBloody : EInventoryItemIcon.JournalLetter;
		}
		return EInventoryItemIcon.JournalFile;
	}

	internal static string GetOrbNameBySlot(InventoryItem orb, EOrbSlot slot)
	{
		string text = "";
		switch (slot)
		{
		case EOrbSlot.Spell:
			text = "_spell";
			break;
		case EOrbSlot.Passive:
			text = "_passive";
			break;
		}
		return Loc.Get(orb.NameKey + text);
	}

	internal static string GetOrbDescriptionBySlot(InventoryItem orb, EOrbSlot slot)
	{
		string text = "";
		switch (slot)
		{
		case EOrbSlot.Spell:
			text = "_spell";
			break;
		case EOrbSlot.Passive:
			text = "_passive";
			break;
		}
		return Loc.Get(orb.NameKey + text + "_desc");
	}

	internal static bool IsQuestItem(int item, int category)
	{
		bool result = false;
		if (category == 1)
		{
			switch ((EInventoryUseItemType)item)
			{
			case EInventoryUseItemType.Herb:
			case EInventoryUseItemType.Mushroom:
			case EInventoryUseItemType.Drumstick:
			case EInventoryUseItemType.WyvernTail:
			case EInventoryUseItemType.EelMeat:
			case EInventoryUseItemType.CheveuxBreast:
			case EInventoryUseItemType.CheveuxFeather:
			case EInventoryUseItemType.SirenInk:
			case EInventoryUseItemType.PlasmaCore:
				result = true;
				break;
			}
		}
		return result;
	}
}
