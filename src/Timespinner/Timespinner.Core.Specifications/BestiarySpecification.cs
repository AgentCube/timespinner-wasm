using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using ZlibNet;

namespace Timespinner.Core.Specifications;

[Serializable]
public class BestiarySpecification
{
	public const string XmlBestiarySpecificationNodeName = "Bestiary";

	public const string XmlBestiaryEntrySpecificationNodeName = "BestiaryEntry";

	public const string XmlLootTableNodeName = "LootTable";

	public const string XmlDamageTableNodeName = "DamageTable";

	public const string XmlItemEntryNodeName = "ItemEntry";

	public const string XmlDamageEntryNodeName = "DamageEntry";

	public const string XmlEnemyTypeIntAttribute = "Enemy";

	public const string XmlEnemyArgumentAttribute = "Argument";

	public const string XmlKeyAttribute = "Key";

	public const string XmlHPAttribute = "HP";

	public const string XmlExpAttribute = "Exp";

	public const string XmlCameraOffsetXAttribute = "OffsetX";

	public const string XmlCameraOffsetYAttribute = "OffsetY";

	public const string XmlItemAttribute = "Item";

	public const string XmlCategoryAttribute = "Category";

	public const string XmlDropRateAttribute = "DropRate";

	public const string XmlTouchDamageAttribute = "Touch";

	public const string XmlDamageAttribute = "XmlDamageAttribute";

	public const string XmlElementalWeaknessesAttribute = "Elemental";

	public const string XmlIsEntryInvisibleAttribute = "IsInvisible";

	private readonly List<BestiaryEntrySpecification> _bestiaryEntries = new List<BestiaryEntrySpecification>();

	private readonly Dictionary<string, BestiaryEntrySpecification> _bestiaryEntriesMemoized = new Dictionary<string, BestiaryEntrySpecification>();

	public List<BestiaryEntrySpecification> BestiaryEntries => _bestiaryEntries;

	public static BestiarySpecification LoadXmlBestiarySpecification(string filepath)
	{
		using FileStream filestream = File.Open(filepath, FileMode.Open);
		return LoadXmlBestiarySpecification(filestream);
	}

	public static BestiarySpecification LoadXmlBestiarySpecification(Stream filestream)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Invalid comparison between Unknown and I4
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Invalid comparison between Unknown and I4
		BestiarySpecification bestiarySpecification = new BestiarySpecification();
		XmlReader val = XmlReader.Create(filestream);
		try
		{
			bool flag = false;
			while (flag || val.Read())
			{
				flag = false;
				string localName;
				if ((int)val.NodeType == 1 && (localName = val.LocalName) != null && localName == "BestiaryEntry")
				{
					int num = 0;
					while ((int)val.NodeType == 1 && val.LocalName == "BestiaryEntry")
					{
						BestiaryEntrySpecification bestiaryEntrySpecification = LoadXmlBestiaryEntrySpecification(val);
						bestiaryEntrySpecification.Index = num;
						num++;
						bestiarySpecification.BestiaryEntries.Add(bestiaryEntrySpecification);
						flag = (int)val.NodeType == 1;
					}
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		bestiarySpecification.LoadMemoizedEntries();
		return bestiarySpecification;
	}

	public static BestiaryEntrySpecification LoadXmlBestiaryEntrySpecification(XmlReader reader)
	{
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Invalid comparison between Unknown and I4
		bool flag = false;
		BestiaryEntrySpecification bestiaryEntrySpecification = new BestiaryEntrySpecification();
		while (reader.MoveToNextAttribute())
		{
			switch (reader.Name)
			{
			case "Enemy":
				bestiaryEntrySpecification.EnemyTypeInt = reader.Value.ParseInt32();
				bestiaryEntrySpecification.EnemyType = (EEnemyTileType)bestiaryEntrySpecification.EnemyTypeInt;
				break;
			case "Argument":
				bestiaryEntrySpecification.EnemyArgument = reader.Value.ParseInt32();
				break;
			case "HP":
				bestiaryEntrySpecification.HP = reader.Value.ParseInt32();
				break;
			case "Exp":
				bestiaryEntrySpecification.Exp = reader.Value.ParseInt32();
				break;
			case "Touch":
				bestiaryEntrySpecification.TouchDamage = reader.Value.ParseInt32();
				break;
			case "OffsetX":
				bestiaryEntrySpecification.CameraOffsetX = reader.Value.ParseInt32();
				break;
			case "OffsetY":
				bestiaryEntrySpecification.CameraOffsetY = reader.Value.ParseInt32();
				break;
			case "Elemental":
				bestiaryEntrySpecification.DemuxElementalWeaknesses(ulong.Parse(reader.Value));
				break;
			case "IsInvisible":
				bestiaryEntrySpecification.IsEntryInvisible = bool.Parse(reader.Value);
				break;
			}
		}
		while (!flag && reader.Read())
		{
			if ((int)reader.NodeType != 1)
			{
				continue;
			}
			switch (reader.LocalName)
			{
			case "ItemEntry":
			{
				BestiaryItemDropSpecification bestiaryItemDropSpecification = new BestiaryItemDropSpecification();
				while (reader.MoveToNextAttribute())
				{
					switch (reader.Name)
					{
					case "Category":
						bestiaryItemDropSpecification.Category = reader.Value.ParseInt32();
						break;
					case "Item":
						bestiaryItemDropSpecification.Item = reader.Value.ParseInt32();
						break;
					case "DropRate":
						bestiaryItemDropSpecification.DropRate = reader.Value.ParseInt32();
						break;
					}
				}
				bestiaryEntrySpecification.LootTable.Add(bestiaryItemDropSpecification);
				break;
			}
			case "DamageEntry":
			{
				string text = null;
				int value = 0;
				while (reader.MoveToNextAttribute())
				{
					switch (reader.Name)
					{
					case "Key":
						text = reader.Value;
						break;
					case "XmlDamageAttribute":
						value = reader.Value.ParseInt32();
						break;
					}
				}
				if (text != null && !bestiaryEntrySpecification.DamageTable.ContainsKey(text))
				{
					bestiaryEntrySpecification.DamageTable.Add(text, value);
				}
				break;
			}
			default:
				flag = true;
				break;
			case "LootTable":
			case "DamageTable":
				break;
			}
		}
		bestiaryEntrySpecification.Key = CharacterSpecification.KeyFromEnemyType(bestiaryEntrySpecification.EnemyType, bestiaryEntrySpecification.EnemyArgument);
		return bestiaryEntrySpecification;
	}

	public static BestiarySpecification FromCompressedFile(string filepath)
	{
		using ZInOutStream filestream = new ZInOutStream(TitleContainer.OpenStream(filepath));
		return LoadXmlBestiarySpecification(filestream);
	}

	public static BestiarySpecification FromUncompressedFile(string filepath)
	{
		using Stream filestream = TitleContainer.OpenStream(filepath);
		return LoadXmlBestiarySpecification(filestream);
	}

	private void LoadMemoizedEntries()
	{
		foreach (BestiaryEntrySpecification bestiaryEntry in _bestiaryEntries)
		{
			_bestiaryEntriesMemoized[bestiaryEntry.Key] = bestiaryEntry;
		}
	}

	public BestiaryEntrySpecification GetEntry(EEnemyTileType enemyTileType, int argument)
	{
		BestiaryEntrySpecification result = null;
		string key = CharacterSpecification.KeyFromEnemyType(enemyTileType, argument);
		if (_bestiaryEntriesMemoized.ContainsKey(key))
		{
			result = _bestiaryEntriesMemoized[key];
		}
		return result;
	}

	public void RefreshNamesAndDescriptions()
	{
		foreach (BestiaryEntrySpecification bestiaryEntry in BestiaryEntries)
		{
			bestiaryEntry.RefreshNameAndDescription();
		}
	}
}
