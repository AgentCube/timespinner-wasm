using System;
using Timespinner.Core.Constants;

namespace Timespinner.GameAbstractions.Inventory;

public class InventoryFamiliar : InventoryItem
{
	private readonly EInventoryFamiliarType _familiarType;

	private Func<int> _getFamiliarLevelSum;

	public EInventoryFamiliarType FamiliarType => _familiarType;

	public int Experience { get; set; }

	public int Damage { get; set; }

	public int Health { get; set; }

	public int BaseMaxHealth { get; set; }

	public int MaxHealth { get; set; }

	public int Level { get; set; }

	public int VisibleLevel => Level + 1;

	public int NextLevel { get; set; }

	public int NextLevelThreshold { get; set; }

	public override EInventoryCategoryType Category => EInventoryCategoryType.Familiar;

	public override int Key => (int)FamiliarType;

	public InventoryFamiliar(EInventoryFamiliarType familiarType, Func<int> getFamiliarLevelSum)
		: base("fam_" + familiarType)
	{
		_familiarType = familiarType;
		_getFamiliarLevelSum = getFamiliarLevelSum;
		if (familiarType == EInventoryFamiliarType.None)
		{
			base.Name = "---";
		}
		else
		{
			RefreshFamiliarStats();
		}
	}

	internal void GiveFamiliarLevelSumFunction(Func<int> getFamiliarLevelSum)
	{
		_getFamiliarLevelSum = getFamiliarLevelSum;
	}

	internal bool GiveExperience(int amount)
	{
		bool result = false;
		if (Level < 99)
		{
			Experience += amount;
			if (Experience > NextLevelThreshold)
			{
				result = true;
				LevelUp();
			}
			else
			{
				NextLevel = NextLevelThreshold - Experience + 1;
			}
		}
		return result;
	}

	private void LevelUp()
	{
		Level++;
		RefreshFamiliarStats();
	}

	internal void RefreshFamiliarStats()
	{
		Level = Constants.FamiliarLevelFromExperience(Experience);
		BaseMaxHealth = GetBaseMaxHealthByFamiliarType(FamiliarType);
		float maxHealthScalingByFamiliarType = GetMaxHealthScalingByFamiliarType(FamiliarType);
		MaxHealth = Constants.FamiliarMaxHealth(BaseMaxHealth, Level, maxHealthScalingByFamiliarType);
		Health = MaxHealth;
		int baseDamageByFamiliarType = GetBaseDamageByFamiliarType(FamiliarType);
		float damageScalingByFamiliarType = GetDamageScalingByFamiliarType(FamiliarType);
		int sumOfFamiliarLevels = ((_getFamiliarLevelSum != null) ? _getFamiliarLevelSum() : 0);
		Damage = Constants.FamiliarDamage(baseDamageByFamiliarType, Level, damageScalingByFamiliarType, sumOfFamiliarLevels);
		NextLevelThreshold = Constants.FamiliarLevelUpThresholdFromLevel(Level);
		if (Level < 99)
		{
			NextLevel = NextLevelThreshold - Experience + 1;
		}
		else
		{
			NextLevel = 0;
		}
	}

	private static int GetBaseMaxHealthByFamiliarType(EInventoryFamiliarType familiarType)
	{
		int result = 1;
		switch (familiarType)
		{
		case EInventoryFamiliarType.MerchantCrow:
			result = 40;
			break;
		case EInventoryFamiliarType.Griffin:
			result = 60;
			break;
		case EInventoryFamiliarType.Kobo:
			result = 60;
			break;
		case EInventoryFamiliarType.Meyef:
			result = 50;
			break;
		case EInventoryFamiliarType.Sprite:
			result = 30;
			break;
		case EInventoryFamiliarType.Demon:
			result = 66;
			break;
		}
		return result;
	}

	private static float GetMaxHealthScalingByFamiliarType(EInventoryFamiliarType familiarType)
	{
		float result = 1f;
		switch (familiarType)
		{
		case EInventoryFamiliarType.MerchantCrow:
			result = 10f;
			break;
		case EInventoryFamiliarType.Griffin:
			result = 10f;
			break;
		case EInventoryFamiliarType.Kobo:
			result = 10f;
			break;
		case EInventoryFamiliarType.Meyef:
			result = 10f;
			break;
		case EInventoryFamiliarType.Sprite:
			result = 10f;
			break;
		case EInventoryFamiliarType.Demon:
			result = 6f;
			break;
		}
		return result;
	}

	private static int GetBaseDamageByFamiliarType(EInventoryFamiliarType familiarType)
	{
		int result = 1;
		switch (familiarType)
		{
		case EInventoryFamiliarType.MerchantCrow:
			result = 1;
			break;
		case EInventoryFamiliarType.Griffin:
			result = 1;
			break;
		case EInventoryFamiliarType.Kobo:
			result = 1;
			break;
		case EInventoryFamiliarType.Meyef:
			result = 1;
			break;
		case EInventoryFamiliarType.Sprite:
			result = 1;
			break;
		case EInventoryFamiliarType.Demon:
			result = 1;
			break;
		}
		return result;
	}

	private static float GetDamageScalingByFamiliarType(EInventoryFamiliarType familiarType)
	{
		float result = 1f;
		switch (familiarType)
		{
		case EInventoryFamiliarType.MerchantCrow:
			result = 1f;
			break;
		case EInventoryFamiliarType.Griffin:
			result = 1f;
			break;
		case EInventoryFamiliarType.Kobo:
			result = 1f;
			break;
		case EInventoryFamiliarType.Meyef:
			result = 1f;
			break;
		case EInventoryFamiliarType.Sprite:
			result = 1f;
			break;
		case EInventoryFamiliarType.Demon:
			result = 1f;
			break;
		}
		return result;
	}
}
