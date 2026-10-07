using System;
using System.Collections.Generic;
using Timespinner.Core.Localization;

namespace Timespinner.Core.Specifications;

[Serializable]
public class BestiaryEntrySpecification
{
	public const string NameSuffix = "_name";

	public const string DescriptionSuffix = "_desc";

	private const int ByteBitCount = 8;

	private const int FullByteValue = 255;

	private const int ElementCount = 8;

	private readonly EElementalWeaknessState[] _elementalWeaknesses = new EElementalWeaknessState[8];

	private readonly List<BestiaryItemDropSpecification> _lootTable = new List<BestiaryItemDropSpecification>();

	private readonly Dictionary<string, int> _damageTable = new Dictionary<string, int>();

	public bool IsEntryInvisible { get; set; }

	public EEnemyTileType EnemyType { get; set; }

	public int EnemyTypeInt { get; set; }

	public int EnemyArgument { get; set; }

	public int Index { get; set; }

	public int CameraOffsetX { get; set; }

	public int CameraOffsetY { get; set; }

	public string Key { get; set; }

	public string VisibleName { get; set; }

	public string VisibleDescription { get; set; }

	public int HP { get; set; }

	public int Exp { get; set; }

	public int TouchDamage { get; set; }

	public EElementalWeaknessState[] ElementalWeaknesses => _elementalWeaknesses;

	public List<BestiaryItemDropSpecification> LootTable => _lootTable;

	public Dictionary<string, int> DamageTable => _damageTable;

	public ulong MuxElementalWeaknesses()
	{
		ulong num = 0uL;
		for (int i = 0; i < 8; i++)
		{
			num <<= 8;
			_ = _elementalWeaknesses[i];
			_ = 2;
			num |= (ulong)_elementalWeaknesses[i];
		}
		_ = 0;
		return num;
	}

	public void DemuxElementalWeaknesses(ulong muxedValue)
	{
		for (int num = 7; num >= 0; num--)
		{
			_elementalWeaknesses[num] = (EElementalWeaknessState)((byte)muxedValue & 0xFFu);
			muxedValue >>= 8;
		}
	}

	public void RefreshNameAndDescription()
	{
		VisibleName = Loc.Get(Key + "_name");
		VisibleDescription = Loc.Get(Key + "_desc");
	}

	public BestiaryEntrySpecification Duplicate()
	{
		BestiaryEntrySpecification bestiaryEntrySpecification = new BestiaryEntrySpecification();
		bestiaryEntrySpecification.HP = HP;
		bestiaryEntrySpecification.Exp = Exp;
		bestiaryEntrySpecification.EnemyType = EnemyType;
		bestiaryEntrySpecification.EnemyTypeInt = EnemyTypeInt;
		bestiaryEntrySpecification.EnemyArgument = EnemyArgument;
		bestiaryEntrySpecification.CameraOffsetX = CameraOffsetX;
		bestiaryEntrySpecification.CameraOffsetY = CameraOffsetY;
		bestiaryEntrySpecification.Key = Key;
		bestiaryEntrySpecification.TouchDamage = TouchDamage;
		bestiaryEntrySpecification.VisibleName = VisibleName;
		bestiaryEntrySpecification.VisibleDescription = VisibleDescription;
		bestiaryEntrySpecification.IsEntryInvisible = IsEntryInvisible;
		BestiaryEntrySpecification bestiaryEntrySpecification2 = bestiaryEntrySpecification;
		int num = 0;
		EElementalWeaknessState[] elementalWeaknesses = _elementalWeaknesses;
		foreach (EElementalWeaknessState eElementalWeaknessState in elementalWeaknesses)
		{
			bestiaryEntrySpecification2.ElementalWeaknesses[num] = eElementalWeaknessState;
			num++;
		}
		foreach (KeyValuePair<string, int> item in DamageTable)
		{
			bestiaryEntrySpecification2.DamageTable.Add(item.Key, item.Value);
		}
		foreach (BestiaryItemDropSpecification item2 in LootTable)
		{
			bestiaryEntrySpecification2.LootTable.Add(item2.Duplicate());
		}
		return bestiaryEntrySpecification2;
	}
}
