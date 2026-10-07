using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;

namespace Timespinner.Core;

public class MinimapAreaSave
{
	public Dictionary<int, MinimapRoomSave> Rooms { get; set; }

	public MinimapAreaSave()
	{
		Rooms = new Dictionary<int, MinimapRoomSave>();
	}

	public static MinimapAreaSave FromArea(MinimapArea area)
	{
		MinimapAreaSave minimapAreaSave = new MinimapAreaSave();
		foreach (MinimapRoom room in area.Rooms)
		{
			MinimapRoomSave minimapRoomSave = new MinimapRoomSave();
			foreach (KeyValuePair<Point, MinimapBlock> block in room.Blocks)
			{
				minimapRoomSave.KnownBlocks[block.Key] = block.Value.IsKnown;
				minimapRoomSave.VisitedBlocks[block.Key] = block.Value.IsVisited;
			}
			minimapAreaSave.Rooms.Add(room.RoomID, minimapRoomSave);
		}
		return minimapAreaSave;
	}

	public void PopulateArea(MinimapArea area)
	{
		foreach (MinimapRoom room in area.Rooms)
		{
			if (!Rooms.ContainsKey(room.RoomID))
			{
				continue;
			}
			MinimapRoomSave minimapRoomSave = Rooms[room.RoomID];
			foreach (KeyValuePair<Point, MinimapBlock> block in room.Blocks)
			{
				if (minimapRoomSave.KnownBlocks.ContainsKey(block.Key))
				{
					block.Value.IsKnown = minimapRoomSave.KnownBlocks[block.Key];
				}
				if (minimapRoomSave.VisitedBlocks.ContainsKey(block.Key))
				{
					block.Value.IsVisited = minimapRoomSave.VisitedBlocks[block.Key];
				}
			}
		}
	}
}
