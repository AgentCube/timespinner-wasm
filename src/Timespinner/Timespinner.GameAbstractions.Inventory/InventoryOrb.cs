using System;
using Timespinner.Core.Constants;

namespace Timespinner.GameAbstractions.Inventory;

public class InventoryOrb : InventoryItem
{
	private readonly EInventoryOrbType _orbType;

	public bool IsSpellUnlocked { get; set; }

	public bool IsPassiveUnlocked { get; set; }

	public EInventoryOrbType OrbType => _orbType;

	public int Experience { get; set; }

	public int Level { get; set; }

	public int NextLevel { get; set; }

	public int NextLevelThreshold { get; set; }

	public int BaseDamage { get; set; }

	internal string SpellName => InventoryItem.NameFromType(OrbType, EOrbSlot.Spell);

	internal string PassiveName => InventoryItem.NameFromType(OrbType, EOrbSlot.Passive);

	public override EInventoryCategoryType Category => EInventoryCategoryType.Orb;

	public override int Key => (int)OrbType;

	public InventoryOrb(EInventoryOrbType orbType)
		: base("orb_" + orbType)
	{
		_orbType = orbType;
		if (orbType == EInventoryOrbType.None)
		{
			base.Name = "---";
		}
		else
		{
			RefreshOrbLevel();
		}
		base.MonetaryValue = GetMonetaryValue(orbType);
	}

	internal void RefreshOrbLevel()
	{
		Level = Constants.OrbLevelFromExperience(Experience);
		BaseDamage = GetBaseDamageByOrbType(OrbType);
		if (Level >= 999)
		{
			NextLevel = 0;
			NextLevelThreshold = 0;
		}
		else
		{
			NextLevelThreshold = Level * 50;
			NextLevel = NextLevelThreshold - Experience;
		}
	}

	internal int GetMeleeDamage(int willpower)
	{
		return (int)(((float)BaseDamage / 2f + (float)willpower / 12f + (float)(BaseDamage * willpower) / 16f) * (1f + (float)(Level - 1) / 100f));
	}

	internal int GetSpellDamage(int willpower)
	{
		float spellScalingByOrbType = GetSpellScalingByOrbType(OrbType);
		return (int)((float)GetMeleeDamage(willpower) * spellScalingByOrbType);
	}

	internal int GetPassiveDamage(int willpower)
	{
		float passiveScalingByOrbType = GetPassiveScalingByOrbType(OrbType);
		return (int)Math.Ceiling((float)GetMeleeDamage(willpower) * passiveScalingByOrbType);
	}

	public int GetIconIndex()
	{
		int result = -1;
		if (_orbType != 0)
		{
			result = (int)_orbType;
		}
		return result;
	}

	internal bool GiveExperience()
	{
		bool result = false;
		if (Level < 999)
		{
			Experience++;
			if (Experience >= NextLevelThreshold)
			{
				result = true;
				LevelUp();
			}
			else
			{
				NextLevel = NextLevelThreshold - Experience;
			}
		}
		return result;
	}

	internal bool GiveInfusionExperience()
	{
		bool flag = false;
		if (Level < 999)
		{
			Experience += 250;
			while (Experience >= NextLevelThreshold)
			{
				flag = true;
				LevelUp();
				if (Level >= 999)
				{
					break;
				}
			}
			if (!flag)
			{
				NextLevel = NextLevelThreshold - Experience;
			}
		}
		return flag;
	}

	private void LevelUp()
	{
		Level++;
		RefreshOrbLevel();
	}

	internal InventoryOrb GetReinforcedPreview()
	{
		InventoryOrb inventoryOrb = new InventoryOrb(_orbType);
		inventoryOrb.Experience = Experience + 250;
		InventoryOrb inventoryOrb2 = inventoryOrb;
		inventoryOrb2.RefreshOrbLevel();
		return inventoryOrb2;
	}

	private static int GetBaseDamageByOrbType(EInventoryOrbType orbType)
	{
		int result = 1;
		switch (orbType)
		{
		case EInventoryOrbType.Blue:
			result = 4;
			break;
		case EInventoryOrbType.Blade:
			result = 7;
			break;
		case EInventoryOrbType.Iron:
			result = 10;
			break;
		case EInventoryOrbType.Ice:
			result = 3;
			break;
		case EInventoryOrbType.Wind:
			result = 3;
			break;
		case EInventoryOrbType.Flame:
			result = 6;
			break;
		case EInventoryOrbType.Pink:
			result = 6;
			break;
		case EInventoryOrbType.Gun:
			result = 9;
			break;
		case EInventoryOrbType.Umbra:
			result = 4;
			break;
		case EInventoryOrbType.Empire:
			result = 10;
			break;
		case EInventoryOrbType.Eye:
			result = 3;
			break;
		case EInventoryOrbType.Blood:
			result = 3;
			break;
		case EInventoryOrbType.Book:
			result = 6;
			break;
		case EInventoryOrbType.Moon:
			result = 3;
			break;
		case EInventoryOrbType.Nether:
			result = 6;
			break;
		case EInventoryOrbType.Barrier:
			result = 8;
			break;
		case EInventoryOrbType.Monske:
			result = 6;
			break;
		}
		return result;
	}

	private static float GetSpellScalingByOrbType(EInventoryOrbType orbType)
	{
		float result = 1f;
		switch (orbType)
		{
		case EInventoryOrbType.Blue:
			result = 3f;
			break;
		case EInventoryOrbType.Blade:
			result = 6f;
			break;
		case EInventoryOrbType.Iron:
			result = 6f;
			break;
		case EInventoryOrbType.Ice:
			result = 2.5f;
			break;
		case EInventoryOrbType.Wind:
			result = 1f;
			break;
		case EInventoryOrbType.Flame:
			result = 1f;
			break;
		case EInventoryOrbType.Pink:
			result = 0.5f;
			break;
		case EInventoryOrbType.Gun:
			result = 1f;
			break;
		case EInventoryOrbType.Umbra:
			result = 1.5f;
			break;
		case EInventoryOrbType.Empire:
			result = 0.4f;
			break;
		case EInventoryOrbType.Eye:
			result = 1.75f;
			break;
		case EInventoryOrbType.Blood:
			result = 3f;
			break;
		case EInventoryOrbType.Book:
			result = 9f;
			break;
		case EInventoryOrbType.Moon:
			result = 3f;
			break;
		case EInventoryOrbType.Nether:
			result = 3f;
			break;
		case EInventoryOrbType.Barrier:
			result = 1f;
			break;
		case EInventoryOrbType.Monske:
			result = 1f;
			break;
		}
		return result;
	}

	private static float GetPassiveScalingByOrbType(EInventoryOrbType orbType)
	{
		float result = 1f;
		switch (orbType)
		{
		case EInventoryOrbType.Blue:
			result = 1f;
			break;
		case EInventoryOrbType.Blade:
			result = 0.5f;
			break;
		case EInventoryOrbType.Flame:
			result = 0.25f;
			break;
		case EInventoryOrbType.Pink:
			result = 1.2f;
			break;
		case EInventoryOrbType.Ice:
			result = 0.25f;
			break;
		case EInventoryOrbType.Blood:
			result = 0.05f;
			break;
		case EInventoryOrbType.Umbra:
			result = 0.25f;
			break;
		}
		return result;
	}

	private static int GetMonetaryValue(EInventoryOrbType orbType)
	{
		int result = 0;
		if (orbType == EInventoryOrbType.Blade)
		{
			result = 100;
		}
		return result;
	}

	internal static bool DoesOrbPassiveDealDamage(EInventoryOrbType orbType)
	{
		bool result = false;
		switch (orbType)
		{
		case EInventoryOrbType.Blade:
		case EInventoryOrbType.Flame:
		case EInventoryOrbType.Ice:
			result = true;
			break;
		}
		return result;
	}
}
