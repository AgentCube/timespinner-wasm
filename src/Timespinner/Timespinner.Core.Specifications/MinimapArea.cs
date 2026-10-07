using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Timespinner.Core.Specifications;

public class MinimapArea
{
	private readonly List<MinimapRoom> _rooms = new List<MinimapRoom>();

	public int LevelID { get; set; }

	public int Height => Rooms.Max((MinimapRoom a) => a.Height);

	public int Width => Rooms.Max((MinimapRoom a) => a.Width);

	public EMinimapRoomColor DefaultColor { get; set; }

	public List<MinimapRoom> Rooms => _rooms;

	public static MinimapArea FromLevel(LevelSpecification level)
	{
		MinimapArea minimapArea = new MinimapArea();
		minimapArea.LevelID = level.ID;
		MinimapArea minimapArea2 = minimapArea;
		Point zero = Point.Zero;
		foreach (RoomSpecification room in level.Rooms)
		{
			MinimapRoom minimapRoom = MinimapRoom.FromRoom(room, minimapArea2);
			minimapRoom.Position = zero;
			minimapArea2.Rooms.Add(minimapRoom);
			zero.X += minimapRoom.Width;
		}
		return minimapArea2;
	}

	public void Draw(SpriteBatch spriteBatch, SpriteSheet minimapSprite, Vector2 drawPosition, float zoom, bool shouldDrawDebug)
	{
		float num = 4f * zoom;
		foreach (MinimapRoom room in Rooms)
		{
			Vector2 drawPosition2 = Vector2.Add(drawPosition, new Vector2((float)room.Position.X * num, (float)room.Position.Y * num));
			room.Draw(spriteBatch, minimapSprite, drawPosition2, zoom, shouldDrawDebug);
		}
	}

	public void Translate(Point offset)
	{
		foreach (MinimapRoom room in _rooms)
		{
			room.Translate(offset);
		}
	}

	public int GenerateNewRoomID()
	{
		int num = -1;
		int num2 = 0;
		List<int> list = new List<int>();
		foreach (MinimapRoom room in _rooms)
		{
			if (!list.Contains(room.RoomID))
			{
				list.Add(room.RoomID);
			}
			num2++;
		}
		int num3 = num2 + 1;
		for (int i = 0; i <= num3; i++)
		{
			if (!list.Contains(i))
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			num = num3 + 1;
		}
		return num;
	}

	internal bool IsAdjacentBlockFound(MinimapRoom askingRoom, Point globalBlockPosition, int directionIndex)
	{
		bool result = false;
		switch (directionIndex)
		{
		case 0:
			globalBlockPosition = new Point(globalBlockPosition.X - 1, globalBlockPosition.Y);
			break;
		case 1:
			globalBlockPosition = new Point(globalBlockPosition.X, globalBlockPosition.Y - 1);
			break;
		case 2:
			globalBlockPosition = new Point(globalBlockPosition.X + 1, globalBlockPosition.Y);
			break;
		case 3:
			globalBlockPosition = new Point(globalBlockPosition.X, globalBlockPosition.Y + 1);
			break;
		}
		foreach (MinimapRoom room in Rooms)
		{
			if (room == askingRoom)
			{
				continue;
			}
			Point key = new Point(globalBlockPosition.X - room.Position.X, globalBlockPosition.Y - room.Position.Y);
			if (room.Blocks.ContainsKey(key))
			{
				MinimapBlock minimapBlock = room.Blocks[key];
				if (!minimapBlock.IsSolidWall && minimapBlock.IsKnown)
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}
}
