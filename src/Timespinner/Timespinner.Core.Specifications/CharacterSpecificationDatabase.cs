using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using ZlibNet;

namespace Timespinner.Core.Specifications;

public sealed class CharacterSpecificationDatabase
{
	private readonly Dictionary<string, CharacterSpecification> _characterSpecifications = new Dictionary<string, CharacterSpecification>();

	public Dictionary<string, CharacterSpecification> CharacterSpecifications => _characterSpecifications;

	public static CharacterSpecificationDatabase LoadXmlCharacterSpecificationDatabase(string filepath)
	{
		using FileStream filestream = File.Open(filepath, FileMode.Open);
		return LoadXmlCharacterSpecificationDatabase(filestream);
	}

	public static CharacterSpecificationDatabase LoadXmlCharacterSpecificationDatabase(Stream filestream)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Invalid comparison between Unknown and I4
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Invalid comparison between Unknown and I4
		CharacterSpecificationDatabase characterSpecificationDatabase = new CharacterSpecificationDatabase();
		XmlReader val = XmlReader.Create(filestream);
		try
		{
			bool flag = false;
			while (flag || val.Read())
			{
				flag = false;
				string localName;
				if ((int)val.NodeType != 1 || (localName = val.LocalName) == null || !(localName == "CharacterSpecification"))
				{
					continue;
				}
				while ((int)val.NodeType == 1 && val.LocalName == "CharacterSpecification")
				{
					CharacterSpecification characterSpecification = CharacterSpecification.LoadXmlCharacterSpecification(val);
					if (characterSpecification.Key != null)
					{
						characterSpecificationDatabase.CharacterSpecifications.Add(characterSpecification.Key, characterSpecification);
					}
					flag = (int)val.NodeType == 1;
				}
			}
			return characterSpecificationDatabase;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static CharacterSpecificationDatabase FromCompressedFile(string filepath)
	{
		using ZInOutStream filestream = new ZInOutStream(TitleContainer.OpenStream(filepath));
		return LoadXmlCharacterSpecificationDatabase(filestream);
	}

	public static CharacterSpecificationDatabase FromUncompressedFile(string filepath)
	{
		using Stream filestream = TitleContainer.OpenStream(filepath);
		return LoadXmlCharacterSpecificationDatabase(filestream);
	}
}
