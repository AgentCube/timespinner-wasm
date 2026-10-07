using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;

namespace Timespinner.Core.Specifications;

public sealed class TextureAtlasSpecification
{
	public const string XmlTextureAtlasDatabaseNodeName = "TextureDatabase";

	public const string XmlTextureAtlasNodeName = "Atlas";

	public const string XmlAtlasFramesNodeName = "AtlasFrames";

	public const string XmlAtlasFrameNodeName = "AtlasFrame";

	public const string XmlFilenameAttribute = "FileName";

	public const string XmlContentPathAttribute = "ContentPath";

	public const string XmlWidthAttribute = "Width";

	public const string XmlHeightAttribute = "Height";

	public const string XmlFrameCountAttribute = "FrameCount";

	public const string XmlDoesNewRowUseStartXAttribute = "DoesNewRowUseStartX";

	public const string XmlCountAttribute = "Count";

	public const string XmlRowWidthAttribute = "RowWidth";

	public const string XmlStartIndexAttribute = "StartIndex";

	public const string XmlFrameSizeAttribute = "FrameSize";

	public const string XmlStartCoordinatesAttribute = "StartCoordinates";

	private readonly List<TextureAtlasFrameSpecification> _frameSpecifications = new List<TextureAtlasFrameSpecification>();

	private static TextureAtlasSpecification _missingTextureAtlasSpecification;

	public int Width { get; set; }

	public int Height { get; set; }

	public int FrameCount { get; set; }

	public string FileName { get; set; }

	public string ContentPath { get; set; }

	public List<TextureAtlasFrameSpecification> FrameSpecifications => _frameSpecifications;

	public static TextureAtlasSpecification MissingTextureSheet
	{
		get
		{
			if (_missingTextureAtlasSpecification == null)
			{
				TextureAtlasSpecification textureAtlasSpecification = new TextureAtlasSpecification();
				textureAtlasSpecification.FileName = "MissingTexture";
				textureAtlasSpecification.ContentPath = "MissingTexture";
				textureAtlasSpecification.FrameCount = 1;
				textureAtlasSpecification.Height = 128;
				textureAtlasSpecification.Width = 128;
				_missingTextureAtlasSpecification = textureAtlasSpecification;
				_missingTextureAtlasSpecification.FrameSpecifications.Add(new TextureAtlasFrameSpecification
				{
					Count = 1,
					FrameSize = new Point(128, 128)
				});
				_missingTextureAtlasSpecification.FrameSpecifications.Add(new TextureAtlasFrameSpecification
				{
					Count = 16,
					FrameSize = new Point(32, 32),
					RowWidth = 4
				});
				_missingTextureAtlasSpecification.FrameSpecifications.Add(new TextureAtlasFrameSpecification
				{
					Count = 64,
					FrameSize = new Point(16, 16),
					RowWidth = 8
				});
			}
			return _missingTextureAtlasSpecification;
		}
	}

	public static TextureAtlasSpecification LoadXmlTextureAtlasSpecification(XmlReader reader)
	{
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Invalid comparison between Unknown and I4
		bool flag = false;
		TextureAtlasSpecification textureAtlasSpecification = new TextureAtlasSpecification();
		while (reader.MoveToNextAttribute())
		{
			switch (reader.Name)
			{
			case "FileName":
				textureAtlasSpecification.FileName = reader.Value;
				break;
			case "ContentPath":
				textureAtlasSpecification.ContentPath = reader.Value;
				break;
			case "Width":
				textureAtlasSpecification.Width = reader.Value.ParseInt32();
				break;
			case "Height":
				textureAtlasSpecification.Height = reader.Value.ParseInt32();
				break;
			case "FrameCount":
				textureAtlasSpecification.FrameCount = reader.Value.ParseInt32();
				break;
			}
		}
		TextureAtlasFrameSpecification textureAtlasFrameSpecification = null;
		while (!flag && reader.Read())
		{
			if ((int)reader.NodeType != 1)
			{
				continue;
			}
			string localName;
			if ((localName = reader.LocalName) != null && localName == "AtlasFrame")
			{
				if (textureAtlasFrameSpecification != null)
				{
					textureAtlasSpecification.FrameSpecifications.Add(textureAtlasFrameSpecification);
				}
				textureAtlasFrameSpecification = new TextureAtlasFrameSpecification();
				while (reader.MoveToNextAttribute())
				{
					switch (reader.Name)
					{
					case "DoesNewRowUseStartX":
						textureAtlasFrameSpecification.DoesNewRowUseStartX = bool.Parse(reader.Value);
						break;
					case "Count":
						textureAtlasFrameSpecification.Count = reader.Value.ParseInt32();
						break;
					case "RowWidth":
						textureAtlasFrameSpecification.RowWidth = reader.Value.ParseInt32();
						break;
					case "StartIndex":
						textureAtlasFrameSpecification.StartIndex = reader.Value.ParseInt32();
						break;
					case "FrameSize":
						textureAtlasFrameSpecification.FrameSize = MathEx.ParsePoint(reader.Value);
						break;
					case "StartCoordinates":
						textureAtlasFrameSpecification.StartCoordinates = MathEx.ParsePoint(reader.Value);
						break;
					}
				}
			}
			else
			{
				flag = true;
			}
		}
		textureAtlasSpecification.FrameSpecifications.Add(textureAtlasFrameSpecification);
		textureAtlasSpecification.RefreshFrameSources();
		return textureAtlasSpecification;
	}

	public static string GetFileName(string path)
	{
		string result = path;
		int num = path.LastIndexOf('/');
		if (num != -1 && num + 1 < path.Length)
		{
			result = path.SafeSubstring(num + 1, path.Length - 1);
		}
		return result;
	}

	public List<string> GetFolders()
	{
		List<string> list = new List<string>();
		string text = ContentPath;
		for (int num = text.IndexOf('/'); num != -1; num = text.IndexOf('/'))
		{
			list.Add(text.SafeSubstring(0, num));
			if (num + 1 >= text.Length)
			{
				break;
			}
			text = text.SafeSubstring(num + 1, text.Length - num);
		}
		return list;
	}

	public void RefreshFrameSources()
	{
		FrameCount = 0;
		foreach (TextureAtlasFrameSpecification frameSpecification in _frameSpecifications)
		{
			if (frameSpecification != null)
			{
				frameSpecification.RefreshFrameSources(Width);
				FrameCount += frameSpecification.Count;
			}
		}
	}

	public List<TextureAtlasFrame> GetAtlasFrames()
	{
		List<TextureAtlasFrame> list = new List<TextureAtlasFrame>();
		foreach (TextureAtlasFrameSpecification frameSpecification in FrameSpecifications)
		{
			if (frameSpecification != null)
			{
				list.Add(frameSpecification.ToTextureAtlasFrame());
			}
		}
		return list;
	}
}
