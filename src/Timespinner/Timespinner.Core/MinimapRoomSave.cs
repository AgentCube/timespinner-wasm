using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Timespinner.Core;

public class MinimapRoomSave
{
	public Dictionary<Point, bool> KnownBlocks { get; set; }

	public Dictionary<Point, bool> VisitedBlocks { get; set; }

	public MinimapRoomSave()
	{
		KnownBlocks = new Dictionary<Point, bool>();
		VisitedBlocks = new Dictionary<Point, bool>();
	}
}
