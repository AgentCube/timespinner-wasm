using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Xna.Framework;
using ZlibNet;

namespace Timespinner.Core.Specifications;

public class LevelSpecification
{
	public const string XmlLevelNodeName = "LevelSpecification";

	public const string XmlRoomsNodeName = "Rooms";

	public const string XmlRoomNodeName = "Room";

	public const string XmlBottomNodeName = "BottomTiles";

	public const string XmlMiddleNodeName = "MiddleTiles";

	public const string XmlTopNodeName = "TopTiles";

	public const string XmlObjectNodeName = "ObjectTiles";

	public const string XmlTileSwathsNodeName = "TileSwaths";

	public const string XmlTileNodeName = "Tile";

	public const string XmlTileSwathNodeName = "TileSwath";

	public const string XmlDefaultBackgroundsNodeName = "DefaultBackgrounds";

	public const string XmlWarpBackgroundsNodeName = "WarpBackgrounds";

	public const string XmlBackgroundsNodeName = "Backgrounds";

	public const string XmlBackgroundNodeName = "Background";

	public const string XmlSwitchesNodeName = "Switches";

	public const string XmlSwitchNodeName = "Switch";

	public const string XmlIDAttribute = "ID";

	public const string XmlIndexAttribute = "Index";

	public const string XmlNameAttribute = "Name";

	public const string XmlTilesetAttribute = "Tileset";

	public const string XmlWidthAttribute = "Width";

	public const string XmlHeightAttribute = "Height";

	public const string XmlXAttribute = "X";

	public const string XmlYAttribute = "Y";

	public const string XmlLayerAttribute = "Layer";

	public const string XmlFlipHorizontalAttribute = "FlipX";

	public const string XmlFlipVerticalAttribute = "FlipY";

	public const string XmlObjectCategoryAttribute = "Category";

	public const string XmlObjectIDAttribute = "ObjectID";

	public const string XmlArgumentAttribute = "Argument";

	public const string XmlBackgroundWipeColorAttribute = "BackgroundWipeColor";

	public const int SpecialCoreTilesStart = 96;

	public const int SlopeCoreTilesStart = 114;

	public const int AnimatedTilesOffset = 224;

	public const int CoreTilesOffset = 384;

	public const int NpcTilesOffset = 64;

	public const int TilesetMaxSize = 512;

	public const int ObjectTilesCount = 128;

	private readonly List<RoomSpecification> _rooms = new List<RoomSpecification>();

	private readonly List<BackgroundSpecification> _defaultBackgrounds = new List<BackgroundSpecification>();

	private readonly List<BackgroundSpecification> _warpBackgrounds = new List<BackgroundSpecification>();

	public int ID { get; set; }

	public string Name { get; set; }

	public List<RoomSpecification> Rooms => _rooms;

	public List<BackgroundSpecification> DefaultBackgrounds => _defaultBackgrounds;

	public List<BackgroundSpecification> WarpBackgrounds => _warpBackgrounds;

	public static LevelSpecification DebugLevel
	{
		get
		{
			LevelSpecification levelSpecification = new LevelSpecification();
			RoomSpecification roomSpecification = new RoomSpecification();
			roomSpecification.Width = 25;
			roomSpecification.Height = 15;
			RoomSpecification roomSpecification2 = roomSpecification;
			for (int i = 0; i < 25; i++)
			{
				roomSpecification2.AddTile(new Point(i, 14), new TileSpecification
				{
					Layer = ETileLayerType.Middle,
					X = i,
					Y = 14
				});
			}
			levelSpecification.Rooms.Add(roomSpecification2);
			return levelSpecification;
		}
	}

	public static LevelSpecification LoadXmlLevel(string filepath)
	{
		return LoadXmlLevel(TitleContainer.OpenStream(filepath));
	}

	public static LevelSpecification LoadXmlLevel(Stream filestream)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Invalid comparison between Unknown and I4
		LevelSpecification levelSpecification = new LevelSpecification();
		List<BackgroundSpecification> list = levelSpecification.DefaultBackgrounds;
		RoomSpecification roomSpecification = null;
		List<SwitchSpecification> list2 = null;
		TileSpecification tile = null;
		BackgroundSpecification background = null;
		XmlReader val = XmlReader.Create(filestream);
		try
		{
			while (val.Read())
			{
				if ((int)val.NodeType != 1)
				{
					continue;
				}
				switch (val.LocalName)
				{
				case "LevelSpecification":
					while (val.MoveToNextAttribute())
					{
						switch (val.Name)
						{
						case "ID":
							levelSpecification.ID = val.Value.ParseInt32();
							break;
						case "Name":
							levelSpecification.Name = val.Value;
							break;
						}
					}
					break;
				case "Room":
				{
					if (roomSpecification != null)
					{
						levelSpecification.Rooms.Add(roomSpecification);
					}
					PushSwitches(background, tile, list2);
					background = null;
					tile = null;
					RoomSpecification roomSpecification2 = new RoomSpecification();
					roomSpecification2.BackgroundWipeColor = Color.Black;
					roomSpecification = roomSpecification2;
					while (val.MoveToNextAttribute())
					{
						switch (val.Name)
						{
						case "ID":
							roomSpecification.ID = val.Value.ParseInt32();
							break;
						case "Index":
							roomSpecification.Index = val.Value.ParseInt32();
							break;
						case "Name":
							roomSpecification.Name = val.Value;
							break;
						case "Tileset":
							roomSpecification.Tileset = EnumExtensions.EnumTryParse<ETilesetType>(val.Value);
							break;
						case "Width":
							roomSpecification.Width = val.Value.ParseInt32();
							break;
						case "Height":
							roomSpecification.Height = val.Value.ParseInt32();
							break;
						case "BackgroundWipeColor":
							roomSpecification.BackgroundWipeColor = MathEx.ParseColor(val.Value);
							break;
						}
					}
					break;
				}
				case "Tile":
				{
					PushSwitches(background, tile, list2);
					background = null;
					TileSpecification tileSpecification = new TileSpecification();
					int objectID = -1;
					int argument = 0;
					EObjectTileCategory category = EObjectTileCategory.None;
					while (val.MoveToNextAttribute())
					{
						switch (val.Name)
						{
						case "ID":
							tileSpecification.ID = val.Value.ParseInt32();
							break;
						case "X":
							tileSpecification.X = val.Value.ParseInt32();
							break;
						case "Y":
							tileSpecification.Y = val.Value.ParseInt32();
							break;
						case "Layer":
							tileSpecification.Layer = EnumExtensions.EnumParse<ETileLayerType>(val.Value);
							break;
						case "FlipX":
							tileSpecification.IsFlippedHorizontally = bool.Parse(val.Value);
							break;
						case "FlipY":
							tileSpecification.IsFlippedVertically = bool.Parse(val.Value);
							break;
						case "Category":
							category = EnumExtensions.EnumParse<EObjectTileCategory>(val.Value);
							break;
						case "ObjectID":
							objectID = val.Value.ParseInt32();
							break;
						case "Argument":
							argument = val.Value.ParseInt32();
							break;
						}
					}
					Point point = new Point(tileSpecification.X, tileSpecification.Y);
					switch (tileSpecification.Layer)
					{
					case ETileLayerType.Bottom:
						roomSpecification.AddStackedTile(point, tileSpecification, ETileLayerType.Bottom);
						break;
					case ETileLayerType.Middle:
						roomSpecification.MiddleTiles.Add(point, tileSpecification);
						break;
					case ETileLayerType.Top:
						roomSpecification.AddStackedTile(point, tileSpecification, ETileLayerType.Top);
						break;
					case ETileLayerType.Objects:
						roomSpecification.AddObjectTile(point, new ObjectTileSpecification(tileSpecification)
						{
							Category = category,
							ObjectID = objectID,
							Argument = argument
						});
						break;
					}
					tile = tileSpecification;
					break;
				}
				case "TileSwath":
				{
					PushSwitches(background, tile, list2);
					background = null;
					tile = null;
					TileSwathSpecification tileSwathSpecification = new TileSwathSpecification();
					while (val.MoveToNextAttribute())
					{
						switch (val.Name)
						{
						case "ID":
							tileSwathSpecification.ID = val.Value.ParseInt32();
							break;
						case "X":
							tileSwathSpecification.X = val.Value.ParseInt32();
							break;
						case "Y":
							tileSwathSpecification.Y = val.Value.ParseInt32();
							break;
						case "Width":
							tileSwathSpecification.Width = val.Value.ParseInt32();
							break;
						case "Height":
							tileSwathSpecification.Height = val.Value.ParseInt32();
							break;
						}
					}
					Point point2 = new Point(tileSwathSpecification.X, tileSwathSpecification.Y);
					roomSpecification.AddTileSwath(point2, tileSwathSpecification);
					break;
				}
				case "DefaultBackgrounds":
					list = levelSpecification.DefaultBackgrounds;
					PushSwitches(background, tile, list2);
					tile = null;
					background = null;
					break;
				case "WarpBackgrounds":
					list = levelSpecification.WarpBackgrounds;
					PushSwitches(background, tile, list2);
					tile = null;
					background = null;
					break;
				case "Backgrounds":
					PushSwitches(background, tile, list2);
					tile = null;
					background = null;
					if (roomSpecification != null)
					{
						list = roomSpecification.Backgrounds;
					}
					break;
				case "Background":
				{
					BackgroundSpecification backgroundSpecification = BackgroundSpecification.FromXml(val);
					list.Add(backgroundSpecification);
					PushSwitches(background, tile, list2);
					background = backgroundSpecification;
					tile = null;
					break;
				}
				case "Switches":
					PushSwitches(background, tile, list2);
					list2 = new List<SwitchSpecification>();
					break;
				case "Switch":
					list2?.Add(SwitchSpecification.FromXml(val));
					break;
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		PushSwitches(background, tile, list2);
		levelSpecification.Rooms.Add(roomSpecification);
		return levelSpecification;
	}

	private static void PushSwitches(BackgroundSpecification background, TileSpecification tile, List<SwitchSpecification> switches)
	{
		if (switches != null)
		{
			if (background != null)
			{
				background.Switches.AddRange(switches);
			}
			else
			{
				tile?.Switches.AddRange(switches);
			}
			switches.Clear();
		}
	}

	public static List<BackgroundSpecification> LoadXmlLevelWarpBackgrounds(string filepath)
	{
		return LoadXmlLevelWarpBackgrounds(TitleContainer.OpenStream(filepath));
	}

	private static readonly Dictionary<string, byte[]> s_decompressedBytesCache = new Dictionary<string, byte[]>();

	public static LevelSpecification FromCompressedFile(string filepath)
	{
		if (!s_decompressedBytesCache.TryGetValue(filepath, out byte[] bytes))
		{
			using (Stream rawStream = TitleContainer.OpenStream(filepath))
			using (ZInOutStream zStream = new ZInOutStream(rawStream))
			using (MemoryStream ms = new MemoryStream())
			{
				zStream.CopyTo(ms);
				bytes = ms.ToArray();
				s_decompressedBytesCache[filepath] = bytes;
			}
		}
		using MemoryStream filestream = new MemoryStream(bytes);
		return LoadXmlLevel(filestream);
	}

	public static LevelSpecification FromUncompressedFile(string filepath)
	{
		using Stream filestream = TitleContainer.OpenStream(filepath);
		return LoadXmlLevel(filestream);
	}

	public static List<BackgroundSpecification> LoadXmlLevelWarpBackgrounds(Stream filestream)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Invalid comparison between Unknown and I4
		List<BackgroundSpecification> list = new List<BackgroundSpecification>();
		XmlReader val = XmlReader.Create(filestream);
		try
		{
			while (val.Read())
			{
				if ((int)val.NodeType != 1 || !(val.LocalName == "WarpBackgrounds"))
				{
					continue;
				}
				while (val.Read())
				{
					if ((int)val.NodeType == 1)
					{
						if (val.LocalName != "Background")
						{
							break;
						}
						list.Add(BackgroundSpecification.FromXml(val));
					}
				}
			}
			return list;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static List<BackgroundSpecification> LoadCompressedLevelWarpBackgrounds(string filepath)
	{
		using ZInOutStream filestream = new ZInOutStream(TitleContainer.OpenStream(filepath));
		return LoadXmlLevelWarpBackgrounds(filestream);
	}

	public RoomSpecification GetRoomByID(int targetID)
	{
		RoomSpecification result = null;
		foreach (RoomSpecification room in Rooms)
		{
			if (room.ID == targetID)
			{
				result = room;
				break;
			}
		}
		return result;
	}

	public IEnumerable<TileSpecification> GetTilesFromRoomAndPosition(int roomID, Point position)
	{
		RoomSpecification roomSpecification = Rooms[roomID];
		List<TileSpecification> list = new List<TileSpecification>();
		if (roomSpecification.BottomTiles.ContainsKey(position))
		{
			list.AddRange(roomSpecification.BottomTiles[position]);
		}
		else
		{
			list.Add(new TileSpecification
			{
				ID = -1,
				Layer = ETileLayerType.Bottom,
				X = position.X,
				Y = position.Y
			});
		}
		list.Add(roomSpecification.MiddleTiles.ContainsKey(position) ? roomSpecification.MiddleTiles[position] : new TileSpecification
		{
			ID = -1,
			Layer = ETileLayerType.Middle,
			X = position.X,
			Y = position.Y
		});
		if (roomSpecification.TopTiles.ContainsKey(position))
		{
			list.AddRange(roomSpecification.TopTiles[position]);
		}
		else
		{
			list.Add(new TileSpecification
			{
				ID = -1,
				Layer = ETileLayerType.Top,
				X = position.X,
				Y = position.Y
			});
		}
		list.AddRange(GetObjectTilesFromRoomAndPosition(roomID, position));
		return list;
	}

	public IEnumerable<ObjectTileSpecification> GetObjectTilesFromRoomAndPosition(int roomID, Point position)
	{
		List<ObjectTileSpecification> list = new List<ObjectTileSpecification>();
		RoomSpecification roomSpecification = Rooms[roomID];
		if (roomSpecification.ObjectTiles.ContainsKey(position))
		{
			list.AddRange(roomSpecification.ObjectTiles[position]);
		}
		else
		{
			list.Add(new ObjectTileSpecification
			{
				ID = -1,
				Layer = ETileLayerType.Objects,
				X = position.X,
				Y = position.Y,
				Category = EObjectTileCategory.None
			});
		}
		return list;
	}

	public IEnumerable<BackgroundSpecification> GetBackgroundsForRoom(int roomID)
	{
		if (!DoesRoomHaveCustomBackgrounds(roomID))
		{
			return _defaultBackgrounds;
		}
		return GetRoomByID(roomID).Backgrounds;
	}

	public Color GetBackgroundWipeColorForRoom(int roomID)
	{
		Color result = Color.Black;
		RoomSpecification roomByID = GetRoomByID(roomID);
		if (roomByID != null)
		{
			result = roomByID.BackgroundWipeColor;
		}
		return result;
	}

	public bool DoesRoomHaveCustomBackgrounds(int roomID)
	{
		return GetRoomByID(roomID)?.Backgrounds.Any() ?? false;
	}
}
