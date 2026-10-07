using Timespinner.Core.Localization;

namespace Timespinner.GameAbstractions.Inventory;

public class InventoryEquipment : InventoryItem
{
	private readonly EInventoryEquipmentType _equipmentType;

	private readonly EEquipmentSlotType _slotType;

	public EInventoryEquipmentType EquipmentType => _equipmentType;

	public int Count { get; set; }

	public EEquipmentSlotType SlotType => _slotType;

	public int StackCap { get; set; }

	public override EInventoryCategoryType Category => EInventoryCategoryType.Equipment;

	public override int Key => (int)EquipmentType;

	public InventoryEquipment(EInventoryEquipmentType equipmentType)
		: base("eq_" + equipmentType)
	{
		_equipmentType = equipmentType;
		_slotType = GetSlotType(_equipmentType);
		StackCap = 9;
		base.MonetaryValue = GetMonetaryValue(_equipmentType);
	}

	internal static string GetEquipmentName(EInventoryEquipmentType itemType)
	{
		return Loc.Get("inv_eq_" + itemType);
	}

	private static int GetMonetaryValue(EInventoryEquipmentType itemType)
	{
		int result = 0;
		switch (itemType)
		{
		case EInventoryEquipmentType.Sunglasses:
			result = 50;
			break;
		case EInventoryEquipmentType.SecurityVisor:
			result = 75;
			break;
		case EInventoryEquipmentType.EngineerGoggles:
			result = 160;
			break;
		case EInventoryEquipmentType.LeatherHelmet:
			result = 125;
			break;
		case EInventoryEquipmentType.PointyHat:
			result = 150;
			break;
		case EInventoryEquipmentType.CopperHelmet:
			result = 200;
			break;
		case EInventoryEquipmentType.CalvaryHelmet:
			result = 360;
			break;
		case EInventoryEquipmentType.BuckleHat:
			result = 300;
			break;
		case EInventoryEquipmentType.AdvisorHat:
			result = 300;
			break;
		case EInventoryEquipmentType.LibrarianHat:
			result = 400;
			break;
		case EInventoryEquipmentType.CombatHelmet:
			result = 600;
			break;
		case EInventoryEquipmentType.CaptainsCap:
			result = 900;
			break;
		case EInventoryEquipmentType.LabGlasses:
			result = 200;
			break;
		case EInventoryEquipmentType.LachiemCrown:
			result = 0;
			break;
		case EInventoryEquipmentType.VileteCrown:
			result = 0;
			break;
		case EInventoryEquipmentType.EternalTiara:
			result = 0;
			break;
		case EInventoryEquipmentType.OldCoat:
			result = 30;
			break;
		case EInventoryEquipmentType.TrendyJacket:
			result = 100;
			break;
		case EInventoryEquipmentType.SecurityVest:
			result = 150;
			break;
		case EInventoryEquipmentType.TravelersCloak:
			result = 200;
			break;
		case EInventoryEquipmentType.LeatherArmor:
			result = 250;
			break;
		case EInventoryEquipmentType.CopperArmor:
			result = 350;
			break;
		case EInventoryEquipmentType.CalvaryArmor:
			result = 500;
			break;
		case EInventoryEquipmentType.MidnightCloak:
			result = 666;
			break;
		case EInventoryEquipmentType.AdvisorRobe:
			result = 650;
			break;
		case EInventoryEquipmentType.LibrarianRobe:
			result = 750;
			break;
		case EInventoryEquipmentType.MilitaryArmor:
			result = 800;
			break;
		case EInventoryEquipmentType.CaptainsJacket:
			result = 1000;
			break;
		case EInventoryEquipmentType.LabCoat:
			result = 500;
			break;
		case EInventoryEquipmentType.EmpressCoat:
			result = 0;
			break;
		case EInventoryEquipmentType.VileteDress:
			result = 0;
			break;
		case EInventoryEquipmentType.EternalCoat:
			result = 0;
			break;
		case EInventoryEquipmentType.SyntheticPlume:
			result = 15;
			break;
		case EInventoryEquipmentType.CheveuxPlume:
			result = 30;
			break;
		case EInventoryEquipmentType.MetalWristband:
			result = 40;
			break;
		case EInventoryEquipmentType.SirenHairband:
			result = 60;
			break;
		case EInventoryEquipmentType.MotherOfPearl:
			result = 100;
			break;
		case EInventoryEquipmentType.BirdStatue:
			result = 200;
			break;
		case EInventoryEquipmentType.DemonStole:
			result = 90;
			break;
		case EInventoryEquipmentType.Pendulum:
			result = 500;
			break;
		case EInventoryEquipmentType.DemonHorn:
			result = 100;
			break;
		case EInventoryEquipmentType.FiligreeClasp:
			result = 400;
			break;
		case EInventoryEquipmentType.AzureStole:
			result = 777;
			break;
		case EInventoryEquipmentType.LuckyCoin:
			result = 3000;
			break;
		case EInventoryEquipmentType.ShinyRock:
			result = 9999;
			break;
		case EInventoryEquipmentType.NelisteEarring:
			result = 0;
			break;
		case EInventoryEquipmentType.SelenBangle:
			result = 0;
			break;
		case EInventoryEquipmentType.GlassPumpkin:
			result = 4;
			break;
		case EInventoryEquipmentType.FamiliarEgg:
			result = 99999;
			break;
		}
		return result;
	}

	private static int GetEquipmentDefense(EInventoryEquipmentType itemType)
	{
		int result = 0;
		switch (itemType)
		{
		case EInventoryEquipmentType.Sunglasses:
			result = 1;
			break;
		case EInventoryEquipmentType.SecurityVisor:
			result = 2;
			break;
		case EInventoryEquipmentType.EngineerGoggles:
			result = 2;
			break;
		case EInventoryEquipmentType.PointyHat:
			result = 3;
			break;
		case EInventoryEquipmentType.LeatherHelmet:
			result = 3;
			break;
		case EInventoryEquipmentType.CopperHelmet:
			result = 4;
			break;
		case EInventoryEquipmentType.CalvaryHelmet:
			result = 5;
			break;
		case EInventoryEquipmentType.BuckleHat:
			result = 5;
			break;
		case EInventoryEquipmentType.AdvisorHat:
			result = 3;
			break;
		case EInventoryEquipmentType.LibrarianHat:
			result = 4;
			break;
		case EInventoryEquipmentType.CombatHelmet:
			result = 6;
			break;
		case EInventoryEquipmentType.CaptainsCap:
			result = 6;
			break;
		case EInventoryEquipmentType.LabGlasses:
			result = 1;
			break;
		case EInventoryEquipmentType.LachiemCrown:
			result = 6;
			break;
		case EInventoryEquipmentType.VileteCrown:
			result = 6;
			break;
		case EInventoryEquipmentType.EternalTiara:
			result = 8;
			break;
		case EInventoryEquipmentType.OldCoat:
			result = 1;
			break;
		case EInventoryEquipmentType.TrendyJacket:
			result = 2;
			break;
		case EInventoryEquipmentType.SecurityVest:
			result = 3;
			break;
		case EInventoryEquipmentType.LeatherArmor:
			result = 4;
			break;
		case EInventoryEquipmentType.TravelersCloak:
			result = 5;
			break;
		case EInventoryEquipmentType.CopperArmor:
			result = 6;
			break;
		case EInventoryEquipmentType.CalvaryArmor:
			result = 8;
			break;
		case EInventoryEquipmentType.MidnightCloak:
			result = 7;
			break;
		case EInventoryEquipmentType.AdvisorRobe:
			result = 5;
			break;
		case EInventoryEquipmentType.LibrarianRobe:
			result = 6;
			break;
		case EInventoryEquipmentType.MilitaryArmor:
			result = 10;
			break;
		case EInventoryEquipmentType.CaptainsJacket:
			result = 10;
			break;
		case EInventoryEquipmentType.LabCoat:
			result = 7;
			break;
		case EInventoryEquipmentType.EmpressCoat:
			result = 10;
			break;
		case EInventoryEquipmentType.VileteDress:
			result = 8;
			break;
		case EInventoryEquipmentType.EternalCoat:
			result = 15;
			break;
		case EInventoryEquipmentType.SyntheticPlume:
			result = 0;
			break;
		case EInventoryEquipmentType.CheveuxPlume:
			result = 0;
			break;
		case EInventoryEquipmentType.MetalWristband:
			result = 1;
			break;
		case EInventoryEquipmentType.SirenHairband:
			result = 2;
			break;
		case EInventoryEquipmentType.MotherOfPearl:
			result = 1;
			break;
		case EInventoryEquipmentType.BirdStatue:
			result = 0;
			break;
		case EInventoryEquipmentType.DemonStole:
			result = 1;
			break;
		case EInventoryEquipmentType.Pendulum:
			result = 0;
			break;
		case EInventoryEquipmentType.DemonHorn:
			result = 2;
			break;
		case EInventoryEquipmentType.FiligreeClasp:
			result = 0;
			break;
		case EInventoryEquipmentType.AzureStole:
			result = 1;
			break;
		case EInventoryEquipmentType.LuckyCoin:
			result = 2;
			break;
		case EInventoryEquipmentType.ShinyRock:
			result = 0;
			break;
		case EInventoryEquipmentType.NelisteEarring:
			result = 0;
			break;
		case EInventoryEquipmentType.SelenBangle:
			result = 1;
			break;
		case EInventoryEquipmentType.GlassPumpkin:
			result = 0;
			break;
		case EInventoryEquipmentType.FamiliarEgg:
			result = 0;
			break;
		}
		return result;
	}

	internal static int[] GetEquipmentStats(EInventoryEquipmentType itemType)
	{
		int[] array = new int[4]
		{
			GetEquipmentDefense(itemType),
			0,
			0,
			0
		};
		switch (itemType)
		{
		case EInventoryEquipmentType.EngineerGoggles:
			array[1] = 1;
			break;
		case EInventoryEquipmentType.PointyHat:
			array[1] = 2;
			break;
		case EInventoryEquipmentType.BuckleHat:
			array[1] = 1;
			array[3] = 1;
			break;
		case EInventoryEquipmentType.AdvisorHat:
			array[1] = 3;
			break;
		case EInventoryEquipmentType.LibrarianHat:
			array[1] = 4;
			break;
		case EInventoryEquipmentType.CombatHelmet:
			array[2] = 2;
			break;
		case EInventoryEquipmentType.CaptainsCap:
			array[1] = 8;
			break;
		case EInventoryEquipmentType.LabGlasses:
			array[1] = 8;
			break;
		case EInventoryEquipmentType.LachiemCrown:
			array[1] = 12;
			break;
		case EInventoryEquipmentType.VileteCrown:
			array[1] = 6;
			break;
		case EInventoryEquipmentType.EternalTiara:
			array[1] = 9;
			break;
		case EInventoryEquipmentType.TravelersCloak:
			array[1] = 1;
			break;
		case EInventoryEquipmentType.CalvaryArmor:
			array[2] = 2;
			break;
		case EInventoryEquipmentType.MidnightCloak:
			array[1] = 2;
			array[3] = 2;
			break;
		case EInventoryEquipmentType.AdvisorRobe:
			array[1] = 4;
			break;
		case EInventoryEquipmentType.LibrarianRobe:
			array[1] = 5;
			break;
		case EInventoryEquipmentType.MilitaryArmor:
			array[2] = 4;
			break;
		case EInventoryEquipmentType.CaptainsJacket:
			array[3] = 7;
			break;
		case EInventoryEquipmentType.LabCoat:
			array[1] = 8;
			break;
		case EInventoryEquipmentType.EmpressCoat:
			array[1] = 6;
			array[2] = 3;
			break;
		case EInventoryEquipmentType.VileteDress:
			array[1] = 7;
			array[2] = 3;
			break;
		case EInventoryEquipmentType.EternalCoat:
			array[1] = 10;
			array[2] = 5;
			break;
		case EInventoryEquipmentType.SyntheticPlume:
			array[3] = 1;
			break;
		case EInventoryEquipmentType.CheveuxPlume:
			array[3] = 2;
			break;
		case EInventoryEquipmentType.SirenHairband:
			array[1] = 1;
			break;
		case EInventoryEquipmentType.MotherOfPearl:
			array[1] = 1;
			array[2] = 1;
			array[3] = 1;
			break;
		case EInventoryEquipmentType.FiligreeClasp:
			array[3] = 10;
			break;
		case EInventoryEquipmentType.DemonStole:
			array[1] = 4;
			break;
		case EInventoryEquipmentType.DemonHorn:
			array[2] = 4;
			break;
		case EInventoryEquipmentType.AzureStole:
			array[1] = 6;
			break;
		case EInventoryEquipmentType.LuckyCoin:
			array[3] = 3;
			break;
		}
		return array;
	}

	internal static float GetDropRate(EInventoryUseItemType itemType)
	{
		return 0.05f;
	}

	internal static EEquipmentSlotType GetSlotType(EInventoryEquipmentType itemType)
	{
		EEquipmentSlotType result = EEquipmentSlotType.Head;
		if (itemType >= EInventoryEquipmentType.OldCoat)
		{
			result = ((itemType < EInventoryEquipmentType.SyntheticPlume) ? EEquipmentSlotType.Body : EEquipmentSlotType.Trinket);
		}
		return result;
	}
}
