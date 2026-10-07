using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.GameObjects.StatusEffects;

namespace Timespinner.GameAbstractions.Saving;

public class CharacterStats
{
	private readonly int[,] _equipmentStats = new int[4, 4];

	public int Experience { get; set; }

	public int MaxHPFound { get; set; }

	public int MaxSandFound { get; set; }

	public int MaxAuraFound { get; set; }

	internal int Level { get; set; }

	internal int MaxLevel { get; set; }

	internal int VisibleLevel => Level + 1;

	internal int NextLevelThreshold { get; set; }

	private int BaseMaxHP { get; set; }

	private int BaseMaxSand { get; set; }

	private int BaseMaxAura { get; set; }

	private int BaseWillpower { get; set; }

	private int BaseFortitude { get; set; }

	private int BaseLuck { get; set; }

	private int EquipmentDefense { get; set; }

	private int EquipmentWillpower { get; set; }

	private int EquipmentFortitude { get; set; }

	private int EquipmentLuck { get; set; }

	internal int BaseDefense { get; private set; }

	internal int MaxHP => BaseMaxHP;

	internal int MaxSand => BaseMaxSand;

	internal int MaxAura => BaseMaxAura;

	internal int Willpower => BaseWillpower + EquipmentWillpower;

	internal int Fortitude => BaseFortitude + EquipmentFortitude;

	internal int Luck => BaseLuck + EquipmentLuck;

	internal int Defense => BaseDefense + EquipmentDefense + Fortitude / 2;

	internal int HP { get; set; }

	internal int Sand { get; set; }

	internal int Aura { get; set; }

	internal EStatusEffectType CurrentStatus { get; set; }

	internal void InitializePostLoad(int levelCap)
	{
		MaxLevel = levelCap;
		Level = Constants.GetCharacterLevelFromExperience(Experience, MaxLevel);
		NextLevelThreshold = Constants.GetCharacterLevelUpThresholdFromLevel(Level, MaxLevel);
		RefreshBaseStats();
		HP = MaxHP;
		Sand = MaxSand;
		Aura = MaxAura;
	}

	internal void RefreshBaseStats()
	{
		int[] characterStatsByLevel = Constants.GetCharacterStatsByLevel(Level);
		if (MaxHPFound > 99)
		{
			MaxHPFound = 99;
		}
		if (MaxSandFound > 99)
		{
			MaxSandFound = 99;
		}
		if (MaxAuraFound > 99)
		{
			MaxAuraFound = 99;
		}
		BaseMaxHP = MathEx.Min(characterStatsByLevel[0] + MaxHPFound * 20, 999);
		BaseMaxSand = MathEx.Min(50 + MaxSandFound * 5, 200);
		BaseMaxAura = MathEx.Min(characterStatsByLevel[1] + MaxAuraFound * 5, 999);
		BaseWillpower = characterStatsByLevel[2];
		BaseFortitude = characterStatsByLevel[3];
		BaseLuck = characterStatsByLevel[4];
		BaseDefense = BaseFortitude / 2;
	}

	internal void RefreshEquipmentStats(int[,] equipmentStats)
	{
		EquipmentDefense = 0;
		EquipmentWillpower = 0;
		EquipmentFortitude = 0;
		EquipmentLuck = 0;
		for (int i = 0; i < 4; i++)
		{
			_equipmentStats[i, 0] = equipmentStats[i, 0];
			_equipmentStats[i, 1] = equipmentStats[i, 1];
			_equipmentStats[i, 2] = equipmentStats[i, 2];
			_equipmentStats[i, 3] = equipmentStats[i, 3];
			EquipmentDefense += _equipmentStats[i, 0];
			EquipmentWillpower += _equipmentStats[i, 1];
			EquipmentFortitude += _equipmentStats[i, 2];
			EquipmentLuck += _equipmentStats[i, 3];
		}
	}

	internal int[] GetEquipmentStatsPreview(int[] itemStats, int slot)
	{
		int[] array = new int[4];
		for (int i = 0; i < 4; i++)
		{
			if (i == slot)
			{
				array[0] += itemStats[0];
				array[1] += itemStats[1];
				array[2] += itemStats[2];
				array[3] += itemStats[3];
			}
			else
			{
				array[0] += _equipmentStats[i, 0];
				array[1] += _equipmentStats[i, 1];
				array[2] += _equipmentStats[i, 2];
				array[3] += _equipmentStats[i, 3];
			}
		}
		array[1] += BaseWillpower;
		array[2] += BaseFortitude;
		array[3] += BaseLuck;
		array[0] += BaseDefense + array[2] / 2;
		return array;
	}

	public bool GiveExperience(int amount)
	{
		bool flag = false;
		Experience += amount;
		while (Experience >= NextLevelThreshold && Level < MaxLevel)
		{
			flag = true;
			Level++;
			NextLevelThreshold = Constants.GetCharacterLevelUpThresholdFromLevel(Level, MaxLevel);
		}
		if (flag)
		{
			RefreshBaseStats();
		}
		return flag;
	}
}
