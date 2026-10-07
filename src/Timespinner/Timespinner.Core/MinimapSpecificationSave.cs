using System.Collections.Generic;
using System.Xml;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;

namespace Timespinner.Core;

public class MinimapSpecificationSave
{
	internal const string XmlMinimapRootNodeName = "Map";

	private const string XmlMinimapCompletionAttributeName = "Completion";

	private const string XmlMinimapMarkersNodeName = "Markers";

	private const string XmlMinimapMarkerNodeName = "Marker";

	private const string XmlMinimapColorAttributeName = "Color";

	private const string XmlMinimapLocationAttributeName = "Location";

	private const string XmlMinimapKeyAttributeName = "Key";

	private const string XmlMinimapAreasNodeName = "Areas";

	private const string XmlMinimapAreaNodeName = "Area";

	private const string XmlMinimapRoomsNodeName = "Rooms";

	private const string XmlMinimapRoomNodeName = "Room";

	private const string XmlMinimapKnownBlocksNodeName = "Known";

	private const string XmlMinimapVisitedBlocksNodeName = "Visited";

	private const string XmlMinimapBlockNodeName = "Block";

	private const string XmlMinimapBlockKeyAttributeName = "Key";

	public int CompletionRate { get; set; }

	public List<MinimapMarkerSave> MarkerSaves { get; set; }

	public Dictionary<int, MinimapAreaSave> Areas { get; set; }

	public MinimapSpecificationSave()
	{
		MarkerSaves = new List<MinimapMarkerSave>();
		Areas = new Dictionary<int, MinimapAreaSave>();
	}

	public static MinimapSpecificationSave FromMinimap(MinimapSpecification minimap)
	{
		MinimapSpecificationSave minimapSpecificationSave = new MinimapSpecificationSave();
		foreach (MinimapArea area in minimap.Areas)
		{
			minimapSpecificationSave.Areas.Add(area.LevelID, MinimapAreaSave.FromArea(area));
		}
		foreach (MinimapMarker value in minimap.Markers.Values)
		{
			minimapSpecificationSave.MarkerSaves.Add(new MinimapMarkerSave
			{
				Location = value.Location,
				Color = value.MarkerColor
			});
		}
		minimapSpecificationSave.CompletionRate = (int)minimap.GetCompletionPercentage();
		return minimapSpecificationSave;
	}

	public void PopulateMinimap(MinimapSpecification minimap)
	{
		foreach (MinimapArea area in minimap.Areas)
		{
			if (Areas.ContainsKey(area.LevelID))
			{
				Areas[area.LevelID].PopulateArea(area);
			}
		}
		foreach (MinimapMarkerSave markerSafe in MarkerSaves)
		{
			markerSafe.Color = markerSafe.Color;
			EMinimapRoomColor eraColorFromLocation = MinimapSpecification.GetEraColorFromLocation(markerSafe.Location);
			minimap.Markers.Add(markerSafe.Location, new MinimapMarker
			{
				EraColor = eraColorFromLocation,
				Location = markerSafe.Location,
				MarkerColor = markerSafe.Color
			});
		}
	}

	internal void ResetForNewGamePlus()
	{
		MarkerSaves.Clear();
		foreach (KeyValuePair<int, MinimapAreaSave> area in Areas)
		{
			foreach (KeyValuePair<int, MinimapRoomSave> room in area.Value.Rooms)
			{
				MinimapRoomSave value = room.Value;
				value.VisitedBlocks.Clear();
				value.KnownBlocks.Clear();
			}
		}
	}

	public void SaveXml(XmlNode root)
	{
		XmlNode node = root.AddElement("Map");
		node.AddAttributeNoDefault("Completion", CompletionRate);
		XmlNode node2 = node.AddElement("Markers");
		foreach (MinimapMarkerSave markerSafe in MarkerSaves)
		{
			XmlNode node3 = node2.AddElement("Marker");
			node3.AddAttributeNoDefault("Color", (int)markerSafe.Color);
			node3.AddAttributeNoDefault("Location", markerSafe.Location);
		}
		XmlNode node4 = node.AddElement("Areas");
		foreach (KeyValuePair<int, MinimapAreaSave> area in Areas)
		{
			XmlNode node5 = node4.AddElement("Area");
			node5.AddAttribute("Key", area.Key);
			MinimapAreaSave value = area.Value;
			XmlNode node6 = node5.AddElement("Rooms");
			foreach (KeyValuePair<int, MinimapRoomSave> room in value.Rooms)
			{
				XmlNode node7 = node6.AddElement("Room");
				node7.AddAttribute("Key", room.Key);
				MinimapRoomSave value2 = room.Value;
				XmlNode node8 = node7.AddElement("Known");
				foreach (KeyValuePair<Point, bool> knownBlock in value2.KnownBlocks)
				{
					if (knownBlock.Value)
					{
						XmlNode node9 = node8.AddElement("Block");
						node9.AddAttribute("Key", knownBlock.Key);
					}
				}
				XmlNode node10 = node7.AddElement("Visited");
				foreach (KeyValuePair<Point, bool> visitedBlock in value2.VisitedBlocks)
				{
					if (visitedBlock.Value)
					{
						XmlNode node11 = node10.AddElement("Block");
						node11.AddAttribute("Key", visitedBlock.Key);
					}
				}
			}
		}
	}

	public static MinimapSpecificationSave LoadXml(XmlReader reader)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Invalid comparison between Unknown and I4
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Invalid comparison between Unknown and I4
		MinimapSpecificationSave minimapSpecificationSave = new MinimapSpecificationSave();
		bool flag = false;
		bool flag2 = false;
		int key = 0;
		int key2 = 0;
		MinimapAreaSave minimapAreaSave = null;
		MinimapRoomSave minimapRoomSave = null;
		while (reader.MoveToNextAttribute())
		{
			string name;
			if ((name = reader.Name) != null && name == "Completion")
			{
				minimapSpecificationSave.CompletionRate = reader.Value.ParseInt32();
			}
		}
		while (!flag && reader.Read())
		{
			if ((int)reader.NodeType == 1)
			{
				switch (reader.LocalName)
				{
				case "Marker":
				{
					MinimapMarkerSave minimapMarkerSave = new MinimapMarkerSave();
					bool flag3 = false;
					while (reader.MoveToNextAttribute())
					{
						switch (reader.Name)
						{
						case "Location":
							minimapMarkerSave.Location = MathEx.ParsePoint(reader.Value);
							flag3 = true;
							break;
						case "Color":
							minimapMarkerSave.Color = (MinimapMarker.EMinimapMarkerColor)reader.Value.ParseInt32();
							break;
						}
					}
					if (flag3)
					{
						minimapSpecificationSave.MarkerSaves.Add(minimapMarkerSave);
					}
					break;
				}
				case "Area":
					if (minimapAreaSave != null)
					{
						if (minimapRoomSave != null)
						{
							minimapAreaSave.Rooms.Add(key2, minimapRoomSave);
							minimapRoomSave = null;
						}
						minimapSpecificationSave.Areas.Add(key, minimapAreaSave);
					}
					minimapAreaSave = new MinimapAreaSave();
					while (reader.MoveToNextAttribute())
					{
						string name3;
						if ((name3 = reader.Name) != null && name3 == "Key")
						{
							key = reader.Value.ParseInt32();
						}
					}
					break;
				case "Room":
					if (minimapRoomSave != null)
					{
						minimapAreaSave?.Rooms.Add(key2, minimapRoomSave);
					}
					minimapRoomSave = new MinimapRoomSave();
					while (reader.MoveToNextAttribute())
					{
						string name4;
						if ((name4 = reader.Name) != null && name4 == "Key")
						{
							key2 = reader.Value.ParseInt32();
						}
					}
					break;
				case "Known":
					flag2 = true;
					break;
				case "Visited":
					flag2 = false;
					break;
				case "Block":
					if (minimapRoomSave == null)
					{
						break;
					}
					while (reader.MoveToNextAttribute())
					{
						string name2;
						if ((name2 = reader.Name) != null && name2 == "Key")
						{
							Point key3 = MathEx.ParsePoint(reader.Value);
							Dictionary<Point, bool> dictionary = (flag2 ? minimapRoomSave.KnownBlocks : minimapRoomSave.VisitedBlocks);
							dictionary[key3] = true;
						}
					}
					break;
				default:
					flag = true;
					break;
				case "Markers":
				case "Areas":
				case "Rooms":
					break;
				}
			}
			else if ((int)reader.NodeType == 15 && reader.Name == "Map")
			{
				flag = true;
			}
		}
		if (minimapAreaSave != null)
		{
			minimapSpecificationSave.Areas.Add(key, minimapAreaSave);
			if (minimapRoomSave != null)
			{
				minimapAreaSave.Rooms.Add(key2, minimapRoomSave);
			}
		}
		return minimapSpecificationSave;
	}
}
