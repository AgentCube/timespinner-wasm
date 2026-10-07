using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ZlibNet;

namespace Timespinner.Core.Specifications;

public sealed class TextureAtlasDatabase
{
	private readonly Dictionary<string, TextureAtlasSpecification> _textureAtlasSpecifications = new Dictionary<string, TextureAtlasSpecification>();

	public Dictionary<string, TextureAtlasSpecification> TextureAtlasSpecifications => _textureAtlasSpecifications;

	public static TextureAtlasDatabase LoadXmlTextureAtlasDatabase(string filepath)
	{
		using FileStream filestream = File.Open(filepath, FileMode.Open);
		return LoadXmlTextureAtlasDatabase(filestream);
	}

	public static TextureAtlasDatabase LoadXmlTextureAtlasDatabase(Stream filestream)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Invalid comparison between Unknown and I4
		TextureAtlasDatabase textureAtlasDatabase = new TextureAtlasDatabase();
		XmlReader val = XmlReader.Create(filestream);
		try
		{
			bool flag = false;
			while (flag || val.Read())
			{
				flag = false;
				string localName;
				if ((int)val.NodeType == 1 && (localName = val.LocalName) != null && localName == "Atlas")
				{
					TextureAtlasSpecification textureAtlasSpecification = TextureAtlasSpecification.LoadXmlTextureAtlasSpecification(val);
					textureAtlasDatabase.TextureAtlasSpecifications.Add(textureAtlasSpecification.ContentPath, textureAtlasSpecification);
					flag = (int)val.NodeType == 1;
				}
			}
			return textureAtlasDatabase;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static TextureAtlasDatabase FromCompressedFile(string filepath)
	{
		using ZInOutStream filestream = new ZInOutStream(TitleContainer.OpenStream(filepath));
		return LoadXmlTextureAtlasDatabase(filestream);
	}

	public static TextureAtlasDatabase FromUncompressedFile(string filepath)
	{
		using Stream filestream = TitleContainer.OpenStream(filepath);
		return LoadXmlTextureAtlasDatabase(filestream);
	}

	public SpriteSheet LoadTextureAtlas(string contentPath, ContentManager content)
	{
		TextureAtlasSpecification textureAtlasSpecification = (_textureAtlasSpecifications.ContainsKey(contentPath) ? _textureAtlasSpecifications[contentPath] : TextureAtlasSpecification.MissingTextureSheet);
		try
		{
			return new TextureAtlas(textureAtlasSpecification.FileName, content.Load<Texture2D>(textureAtlasSpecification.ContentPath), textureAtlasSpecification.GetAtlasFrames());
		}
		catch (Exception)
		{
			textureAtlasSpecification = TextureAtlasSpecification.MissingTextureSheet;
			return new TextureAtlas(textureAtlasSpecification.FileName, content.Load<Texture2D>(textureAtlasSpecification.ContentPath), textureAtlasSpecification.GetAtlasFrames());
		}
	}
}
