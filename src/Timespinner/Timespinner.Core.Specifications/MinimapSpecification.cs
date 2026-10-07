using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core.Specifications.Minimap;
using Timespinner.GameAbstractions.Gameplay;
using ZlibNet;

namespace Timespinner.Core.Specifications;

public class MinimapSpecification
{
	public const int CompletionMaxLevels = 16;

	public const int TotalAreas = 18;

	public const int EraSectionWidth = 100;

	public const int EraSectionHeight = 46;

	public const int EraSectionMarginY = 50;

	private const int MaxMarkerCounter = 32;

	public const string TitlePathToMinimapFile = "Content/Levels/Minimap.mms";

	public const string TitlePathToMinimapDatFile = "Content/Levels/Minimap.dat";

	public const string XmlMapNodeName = "Minimap";

	public const string XmlAreasNodeName = "Areas";

	public const string XmlAreaNodeName = "Area";

	public const string XmlRoomsNodeName = "Rooms";

	public const string XmlRoomNodeName = "Room";

	public const string XmlBlocksNodeName = "Blocks";

	public const string XmlBlockNodeName = "Block";

	public const string XmlMapRevealGroupsNodeName = "RevealGroups";

	public const string XmlMapRevealGroupNodeName = "RevealGroup";

	public const string XmlWallsAttributeName = "Walls";

	public const string XmlDoorsAttributeName = "Doors";

	public const string XmlSecretDoorsAttributeName = "SecretDoors";

	public const string XmlIDAttributeName = "ID";

	public const string XmlColorAttributeName = "Color";

	public const string XmlWidthAttributeName = "Width";

	public const string XmlHeightAttributeName = "Height";

	public const string XmlPositionAttributeName = "Position";

	public const string XmlIsKnownAttributeName = "IsKnown";

	public const string XmlIsVisitedAttributeName = "IsVisisted";

	public const string XmlIsCheckpointAttributeName = "IsCheckpoint";

	public const string XmlIsTransitionAttributeName = "IsTransition";

	public const string XmlIsBossAttributeName = "IsBoss";

	public const string XmlIsTimespinnerAttributeName = "IsTS";

	public const string XmlIsSolidAttributeName = "IsSolid";

	public const string XmlHasSolidToNWAttributeName = "HasSolidToNW";

	public const string XmlIsDebugRoomAttributeName = "IsDebug";

	public const string XmlRevealGroupListAttributeName = "RevealList";

	private readonly Dictionary<Point, MinimapMarker> _markers = new Dictionary<Point, MinimapMarker>();

	private readonly List<MinimapRevealGroup> _revealGroups = new List<MinimapRevealGroup>();

	private readonly List<MinimapArea> _areas = new List<MinimapArea>();

	private readonly Dictionary<Point, List<MinimapBlock>> _allBlocks = new Dictionary<Point, List<MinimapBlock>>();

	public Dictionary<Point, MinimapMarker> Markers => _markers;

	public List<MinimapRevealGroup> RevealGroups => _revealGroups;

	public List<MinimapArea> Areas => _areas;

	public void PopulateAllBlocks()
	{
		_allBlocks.Clear();
		foreach (MinimapArea area in _areas)
		{
			foreach (MinimapRoom room in area.Rooms)
			{
				Point point = new Point(room.Position.X, room.Position.Y);
				foreach (KeyValuePair<Point, MinimapBlock> block in room.Blocks)
				{
					Point key = new Point(block.Key.X + point.X, block.Key.Y + point.Y);
					if (!_allBlocks.ContainsKey(key))
					{
						_allBlocks.Add(key, new List<MinimapBlock>());
					}
					_allBlocks[key].Add(block.Value);
				}
			}
		}
	}

	public static MinimapSpecification FromAllLevels(int levelCount)
	{
		MinimapSpecification minimapSpecification = new MinimapSpecification();
		int num = 0;
		for (int i = 0; i < levelCount; i++)
		{
			LevelSpecification level = LevelSpecification.LoadXmlLevel(Level.GetLevelPathFromID(i, isCompressed: false));
			MinimapArea minimapArea = MinimapArea.FromLevel(level);
			minimapArea.Translate(new Point(0, num));
			minimapSpecification.Areas.Add(minimapArea);
			num += minimapArea.Height;
		}
		minimapSpecification.PopulateAllBlocks();
		return minimapSpecification;
	}

	public static MinimapSpecification FromXml(string filepath)
	{
		return FromXml(TitleContainer.OpenStream(filepath));
	}

	public static MinimapSpecification FromXml(Stream filestream)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		MinimapSpecification minimapSpecification = new MinimapSpecification();
		MinimapArea minimapArea = null;
		MinimapRoom minimapRoom = null;
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
				case "Area":
					if (minimapArea != null)
					{
						if (minimapRoom != null)
						{
							minimapArea.Rooms.Add(minimapRoom);
							minimapRoom = null;
						}
						minimapSpecification.Areas.Add(minimapArea);
					}
					minimapArea = new MinimapArea();
					while (val.MoveToNextAttribute())
					{
						switch (val.Name)
						{
						case "ID":
							minimapArea.LevelID = val.Value.ParseInt32();
							break;
						case "Color":
							minimapArea.DefaultColor = EnumExtensions.EnumParse<EMinimapRoomColor>(val.Value);
							break;
						}
					}
					break;
				case "Room":
					if (minimapRoom != null)
					{
						minimapArea?.Rooms.Add(minimapRoom);
					}
					minimapRoom = new MinimapRoom(minimapArea);
					while (val.MoveToNextAttribute())
					{
						switch (val.Name)
						{
						case "ID":
							minimapRoom.RoomID = val.Value.ParseInt32();
							break;
						case "Width":
							minimapRoom.Width = val.Value.ParseInt32();
							break;
						case "Height":
							minimapRoom.Height = val.Value.ParseInt32();
							break;
						case "Position":
							minimapRoom.Position = MathEx.ParsePoint(val.Value);
							break;
						case "Color":
							minimapRoom.DefaultColor = EnumExtensions.EnumParse<EMinimapRoomColor>(val.Value);
							break;
						case "IsDebug":
							minimapRoom.IsDebug = bool.Parse(val.Value);
							break;
						}
					}
					break;
				case "Block":
				{
					if (minimapRoom == null)
					{
						break;
					}
					MinimapBlock minimapBlock = new MinimapBlock(minimapRoom);
					while (val.MoveToNextAttribute())
					{
						switch (val.Name)
						{
						case "IsKnown":
							minimapBlock.IsKnown = bool.Parse(val.Value);
							break;
						case "IsVisisted":
							minimapBlock.IsVisited = bool.Parse(val.Value);
							break;
						case "Color":
							minimapBlock.RoomColor = EnumExtensions.EnumParse<EMinimapRoomColor>(val.Value);
							break;
						case "Position":
							minimapBlock.Position = MathEx.ParsePoint(val.Value);
							break;
						case "Walls":
							minimapBlock.SetBoolArrayFromString(minimapBlock.Walls, val.Value);
							break;
						case "Doors":
							minimapBlock.SetBoolArrayFromString(minimapBlock.Doors, val.Value);
							break;
						case "SecretDoors":
							minimapBlock.SetBoolArrayFromString(minimapBlock.SecretDoors, val.Value);
							break;
						case "IsCheckpoint":
							minimapBlock.IsCheckpoint = bool.Parse(val.Value);
							break;
						case "IsTransition":
							minimapBlock.IsTransition = bool.Parse(val.Value);
							break;
						case "IsBoss":
							minimapBlock.IsBoss = bool.Parse(val.Value);
							break;
						case "IsTS":
							minimapBlock.IsTimespinner = bool.Parse(val.Value);
							break;
						case "IsSolid":
							minimapBlock.IsSolidWall = bool.Parse(val.Value);
							break;
						case "HasSolidToNW":
							minimapBlock.HasSolidWallToNW = bool.Parse(val.Value);
							break;
						}
					}
					minimapBlock.IsKnown = false;
					if (!minimapRoom.Blocks.ContainsKey(minimapBlock.Position))
					{
						minimapRoom.Blocks.Add(minimapBlock.Position, minimapBlock);
					}
					break;
				}
				case "RevealGroup":
				{
					MinimapRevealGroup minimapRevealGroup = new MinimapRevealGroup();
					while (val.MoveToNextAttribute())
					{
						switch (val.Name)
						{
						case "ID":
							minimapRevealGroup.ID = val.Value.ParseInt32();
							break;
						case "RevealList":
							minimapRevealGroup.LoadFromString(val.Value);
							break;
						}
					}
					minimapSpecification.RevealGroups.Add(minimapRevealGroup);
					break;
				}
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		if (minimapArea != null)
		{
			if (minimapRoom != null)
			{
				minimapArea.Rooms.Add(minimapRoom);
			}
			minimapSpecification.Areas.Add(minimapArea);
		}
		minimapSpecification.PopulateAllBlocks();
		return minimapSpecification;
	}

	public static MinimapSpecification FromCompressedFile(string filepath)
	{
		using ZInOutStream filestream = new ZInOutStream(TitleContainer.OpenStream(filepath));
		return FromXml(filestream);
	}

	public static MinimapSpecification FromUncompressedFile(string filepath)
	{
		using Stream filestream = TitleContainer.OpenStream(filepath);
		return FromXml(filestream);
	}

	public void Draw(SpriteBatch spriteBatch, SpriteSheet minimapSprite, Vector2 drawPosition, float zoom, bool shouldDrawDebug)
	{
		foreach (MinimapArea area in Areas)
		{
			area.Draw(spriteBatch, minimapSprite, drawPosition, zoom, shouldDrawDebug);
		}
	}

	public void DrawView(SpriteBatch spriteBatch, SpriteSheet minimapSprite, Rectangle viewBlockRectangle, Vector2 drawPosition, int minimapZoom)
	{
		DrawView(spriteBatch, minimapSprite, viewBlockRectangle, drawPosition, minimapZoom, 1f, doesDrawDebugRooms: false);
	}

	public void DrawView(SpriteBatch spriteBatch, SpriteSheet minimapSprite, Rectangle viewBlockRectangle, Vector2 drawPosition, int minimapZoom, float alphaAmount, bool doesDrawDebugRooms)
	{
		int num = minimapZoom * 4;
		for (int i = viewBlockRectangle.Top; i < viewBlockRectangle.Bottom; i++)
		{
			for (int j = viewBlockRectangle.Left; j < viewBlockRectangle.Right; j++)
			{
				Point key = new Point(j, i);
				if (!_allBlocks.ContainsKey(key))
				{
					continue;
				}
				foreach (MinimapBlock item in _allBlocks[key])
				{
					Vector2 drawPosition2 = Vector2.Add(drawPosition, new Vector2((j - viewBlockRectangle.X) * num, (i - viewBlockRectangle.Y) * num));
					if (doesDrawDebugRooms || !item.ParentRoom.IsDebug)
					{
						item.Draw(spriteBatch, minimapSprite, drawPosition2, minimapZoom, Color.White, alphaAmount);
					}
				}
			}
		}
	}

	public MinimapBlock GetBlockFromPoint(Point targetPoint)
	{
		MinimapBlock result = null;
		if (_allBlocks.ContainsKey(targetPoint))
		{
			result = _allBlocks[targetPoint].FirstOrDefault((MinimapBlock block) => !block.IsSolidWall) ?? _allBlocks[targetPoint].First();
		}
		return result;
	}

	public IEnumerable<MinimapBlock> GetBlocksFromPoint(Point targetPoint)
	{
		List<MinimapBlock> result = null;
		if (_allBlocks.ContainsKey(targetPoint))
		{
			result = _allBlocks[targetPoint];
		}
		return result;
	}

	public MinimapRoom GetRoomFromPoint(Point targetPoint)
	{
		MinimapRoom minimapRoom = null;
		IEnumerable<MinimapBlock> blocksFromPoint = GetBlocksFromPoint(targetPoint);
		if (blocksFromPoint != null)
		{
			MinimapRoom minimapRoom2 = null;
			foreach (MinimapBlock item in blocksFromPoint)
			{
				if (item != null)
				{
					if (minimapRoom2 == null)
					{
						minimapRoom2 = item.ParentRoom;
					}
					if (!item.IsSolidWall)
					{
						minimapRoom = item.ParentRoom;
					}
				}
			}
			if (minimapRoom == null && minimapRoom2 != null)
			{
				minimapRoom = minimapRoom2;
			}
		}
		return minimapRoom;
	}

	public MinimapRoom GetRoomFromLevelAndRoom(int levelID, int roomID)
	{
		MinimapRoom result = null;
		if (levelID < Areas.Count)
		{
			MinimapArea minimapArea = Areas[levelID];
			foreach (MinimapRoom room in minimapArea.Rooms)
			{
				if (room.RoomID == roomID)
				{
					result = room;
					break;
				}
			}
		}
		return result;
	}

	public void SetAllVisitedAndKnown(bool value, bool onlySetVisited)
	{
		foreach (MinimapArea area in Areas)
		{
			foreach (MinimapRoom room in area.Rooms)
			{
				room.SetVisited(value);
				if (!onlySetVisited)
				{
					room.SetKnown(value);
				}
			}
		}
	}

	public float GetCompletionPercentage()
	{
		int num = 0;
		int num2 = 0;
		foreach (MinimapArea area in Areas)
		{
			if (area.LevelID < 1 || area.LevelID > 16)
			{
				continue;
			}
			foreach (MinimapRoom room in area.Rooms)
			{
				if (room.IsDebug)
				{
					continue;
				}
				foreach (MinimapBlock value in room.Blocks.Values)
				{
					if (!value.IsSolidWall)
					{
						num++;
						if (value.IsVisited)
						{
							num2++;
						}
					}
				}
			}
		}
		if (num <= 0)
		{
			return 0f;
		}
		return (float)num2 / (float)num * 100f;
	}

	public float GetCompletionPercentageByEra(EMinimapEraType era)
	{
		int num = 0;
		int num2 = 0;
		foreach (MinimapArea area in Areas)
		{
			if (area.LevelID < 1 || area.LevelID > 16)
			{
				continue;
			}
			EMinimapEraType eraFromMinimapColor = GetEraFromMinimapColor(area.DefaultColor);
			if (eraFromMinimapColor != era)
			{
				continue;
			}
			foreach (MinimapRoom room in area.Rooms)
			{
				if (room.IsDebug)
				{
					continue;
				}
				foreach (MinimapBlock value in room.Blocks.Values)
				{
					if (!value.IsSolidWall)
					{
						num++;
						if (value.IsVisited)
						{
							num2++;
						}
					}
				}
			}
		}
		if (num <= 0)
		{
			return 0f;
		}
		return (float)num2 / (float)num * 100f;
	}

	public static EMinimapRoomColor GetEraColorFromLocation(Point location)
	{
		if (location.Y <= 50)
		{
			return EMinimapRoomColor.Purple;
		}
		if (location.Y <= 100)
		{
			return EMinimapRoomColor.Blue;
		}
		return EMinimapRoomColor.Orange;
	}

	public bool AddMarker(Point location, MinimapMarker.EMinimapMarkerColor color)
	{
		bool result = false;
		if (_markers.Count < 32)
		{
			result = true;
			if (!_markers.ContainsKey(location))
			{
				EMinimapRoomColor eraColorFromLocation = GetEraColorFromLocation(location);
				_markers.Add(location, new MinimapMarker
				{
					EraColor = eraColorFromLocation,
					Location = location,
					MarkerColor = color
				});
			}
		}
		return result;
	}

	public bool TryRemoveMarker(Point location)
	{
		bool result = false;
		if (_markers.ContainsKey(location))
		{
			_markers.Remove(location);
			result = true;
		}
		else
		{
			List<Point> list = new List<Point>();
			list.Add(new Point(0, 1));
			list.Add(new Point(-1, 1));
			list.Add(new Point(0, 2));
			list.Add(new Point(-1, 2));
			list.Add(new Point(-2, 2));
			List<Point> list2 = list;
			foreach (Point item in list2)
			{
				Point key = location.Add(item);
				if (_markers.ContainsKey(key))
				{
					_markers.Remove(key);
					result = true;
					break;
				}
			}
		}
		return result;
	}

	public static EMinimapEraType GetEraFromMinimapColor(EMinimapRoomColor color)
	{
		EMinimapEraType result = EMinimapEraType.Other;
		switch (color)
		{
		case EMinimapRoomColor.Purple:
			result = EMinimapEraType.Present;
			break;
		case EMinimapRoomColor.Blue:
			result = EMinimapEraType.Past;
			break;
		}
		return result;
	}

	public static Point GetViewOffsetFromEra(EMinimapEraType era)
	{
		Point result;
		switch (era)
		{
		case EMinimapEraType.Present:
			return Point.Zero;
		case EMinimapEraType.Past:
			result = new Point(0, 50);
			break;
		default:
			result = new Point(0, 100);
			break;
		}
		return result;
	}

	public void RevealMapByRevealGroupID(int revealID)
	{
		foreach (MinimapRevealGroup revealGroup in _revealGroups)
		{
			if (revealGroup == null || revealGroup.ID != revealID)
			{
				continue;
			}
			List<Point> rooms = revealGroup.GetRooms();
			{
				foreach (Point item in rooms)
				{
					int x = item.X;
					int y = item.Y;
					foreach (MinimapArea area in _areas)
					{
						if (area.LevelID != x)
						{
							continue;
						}
						foreach (MinimapRoom room in area.Rooms)
						{
							if (room.RoomID != y)
							{
								continue;
							}
							foreach (KeyValuePair<Point, MinimapBlock> block in room.Blocks)
							{
								block.Value.IsKnown = true;
							}
							break;
						}
						break;
					}
				}
				break;
			}
		}
	}

	public bool AreAnyRoomsVisible()
	{
		bool flag = false;
		foreach (MinimapArea area in Areas)
		{
			foreach (MinimapRoom room in area.Rooms)
			{
				if (!room.IsDebug)
				{
					foreach (MinimapBlock value in room.Blocks.Values)
					{
						if (value.IsKnown || value.IsVisited)
						{
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
		return flag;
	}

	internal void ClearAfterGameOverContinue()
	{
		Markers.Clear();
		foreach (MinimapArea area in Areas)
		{
			foreach (MinimapRoom room in area.Rooms)
			{
				foreach (MinimapBlock value in room.Blocks.Values)
				{
					value.IsKnown = false;
					value.IsVisited = false;
				}
			}
		}
	}
}
