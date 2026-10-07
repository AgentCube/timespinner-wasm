using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Timespinner.Core.Specifications;

public class MinimapRoom
{
	public const int BlockSize = 4;

	public const int RoomToBlockSizeX = 25;

	public const int RoomToBlockSizeY = 20;

	private readonly MinimapArea _parentArea;

	private readonly Dictionary<Point, MinimapBlock> _blocks = new Dictionary<Point, MinimapBlock>();

	public bool IsDebug { get; set; }

	public int RoomID { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }

	public Point Position { get; set; }

	public EMinimapRoomColor DefaultColor { get; set; }

	public MinimapArea ParentArea => _parentArea;

	public Dictionary<Point, MinimapBlock> Blocks => _blocks;

	public MinimapRoom(MinimapArea parentArea)
	{
		_parentArea = parentArea;
	}

	public void Translate(Point offset)
	{
		Position = new Point(Position.X + offset.X, Position.Y + offset.Y);
	}

	public void SetVisited(bool value)
	{
		foreach (MinimapBlock value2 in Blocks.Values)
		{
			value2.IsVisited = value;
		}
	}

	public void SetKnown(bool value)
	{
		foreach (MinimapBlock value2 in Blocks.Values)
		{
			value2.IsKnown = value;
		}
	}

	public void SetColor(EMinimapRoomColor newColor)
	{
		DefaultColor = newColor;
		foreach (MinimapBlock value in _blocks.Values)
		{
			value.RoomColor = DefaultColor;
		}
	}

	public static MinimapRoom FromRoom(RoomSpecification room, MinimapArea parentArea)
	{
		int num = (int)Math.Ceiling((float)room.Width / 25f);
		int num2 = (int)Math.Ceiling((float)room.Height / 20f);
		MinimapRoom minimapRoom = new MinimapRoom(parentArea);
		minimapRoom.RoomID = room.ID;
		minimapRoom.Width = num;
		minimapRoom.Height = num2;
		MinimapRoom minimapRoom2 = minimapRoom;
		IEnumerable<ObjectTileSpecification> allMinimapObjects = room.GetAllMinimapObjects();
		Dictionary<Point, List<ObjectTileSpecification>> dictionary = new Dictionary<Point, List<ObjectTileSpecification>>();
		foreach (ObjectTileSpecification item in allMinimapObjects)
		{
			Point key = new Point((int)Math.Floor((float)item.X / 25f), (int)Math.Floor((float)item.Y / 20f));
			if (!dictionary.ContainsKey(key))
			{
				dictionary.Add(key, new List<ObjectTileSpecification>());
			}
			dictionary[key].Add(item);
		}
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				Point point = new Point(j, i);
				MinimapBlock minimapBlock = new MinimapBlock(minimapRoom2);
				minimapBlock.IsKnown = true;
				minimapBlock.IsVisited = true;
				minimapBlock.RoomColor = minimapRoom2.DefaultColor;
				minimapBlock.Position = point;
				MinimapBlock minimapBlock2 = minimapBlock;
				Rectangle tilesRectangle = new Rectangle(j * 25, i * 20, 25, 20);
				minimapBlock2.IsSolidWall = room.IsAreaSolidWall(tilesRectangle);
				if (!minimapBlock2.IsSolidWall && dictionary.ContainsKey(point))
				{
					List<ObjectTileSpecification> list = new List<ObjectTileSpecification>();
					List<ObjectTileSpecification> list2 = new List<ObjectTileSpecification>();
					foreach (ObjectTileSpecification item2 in dictionary[point])
					{
						if (item2.IsDoor())
						{
							list2.Add(item2);
							switch (item2.GetEventType())
							{
							case EEventTileType.WestTeleport:
								minimapBlock2.Doors[0] = true;
								break;
							case EEventTileType.NorthTeleport:
								minimapBlock2.Doors[1] = true;
								break;
							case EEventTileType.EastTeleport:
								minimapBlock2.Doors[2] = true;
								break;
							case EEventTileType.SouthTeleport:
								minimapBlock2.Doors[3] = true;
								break;
							case EEventTileType.Doorway:
								if (!item2.IsFlippedVertically)
								{
									minimapBlock2.Doors[1] = true;
								}
								else
								{
									minimapBlock2.Doors[3] = true;
								}
								break;
							}
						}
						else if (item2.IsCheckpoint())
						{
							minimapBlock2.IsCheckpoint = true;
						}
						else if (item2.IsTransition())
						{
							minimapBlock2.IsTransition = true;
						}
						else if (item2.IsTimespinner())
						{
							minimapBlock2.IsTimespinner = true;
						}
						else if (item2.IsBoss())
						{
							minimapBlock2.IsBoss = true;
						}
						else if (item2.IsBreakableWall())
						{
							list.Add(item2);
						}
					}
					foreach (ObjectTileSpecification item3 in list)
					{
						foreach (ObjectTileSpecification item4 in list2)
						{
							if (item3.X == item4.X && item3.Y == item4.Y)
							{
								switch (item4.GetEventType())
								{
								case EEventTileType.WestTeleport:
									minimapBlock2.SecretDoors[0] = true;
									break;
								case EEventTileType.NorthTeleport:
									minimapBlock2.SecretDoors[1] = true;
									break;
								case EEventTileType.EastTeleport:
									minimapBlock2.SecretDoors[2] = true;
									break;
								case EEventTileType.SouthTeleport:
									minimapBlock2.SecretDoors[3] = true;
									break;
								}
							}
						}
					}
				}
				minimapRoom2.Blocks.Add(point, minimapBlock2);
			}
		}
		minimapRoom2.RefreshWalls();
		return minimapRoom2;
	}

	public void RefreshWalls()
	{
		Vector4 roomBounds = GetRoomBounds();
		foreach (KeyValuePair<Point, MinimapBlock> block in Blocks)
		{
			Point key = block.Key;
			MinimapBlock value = block.Value;
			for (int i = 0; i < 4; i++)
			{
				value.Walls[i] = false;
			}
			value.HasSolidWallToNW = false;
			if ((float)key.X == roomBounds.X)
			{
				value.Walls[0] = true;
			}
			if ((float)key.Y == roomBounds.Y)
			{
				value.Walls[1] = true;
			}
			if ((float)key.X == roomBounds.Z)
			{
				value.Walls[2] = true;
			}
			if ((float)key.Y == roomBounds.W)
			{
				value.Walls[3] = true;
			}
			if (!value.IsSolidWall)
			{
				Point key2 = new Point(key.X - 1, key.Y);
				Point key3 = new Point(key.X, key.Y - 1);
				Point key4 = new Point(key.X + 1, key.Y);
				Point key5 = new Point(key.X, key.Y + 1);
				if (Blocks.ContainsKey(key2) && Blocks[key2].IsSolidWall)
				{
					value.Walls[0] = true;
				}
				if (Blocks.ContainsKey(key3) && Blocks[key3].IsSolidWall)
				{
					value.Walls[1] = true;
				}
				if (Blocks.ContainsKey(key4) && Blocks[key4].IsSolidWall)
				{
					value.Walls[2] = true;
				}
				if (Blocks.ContainsKey(key5) && Blocks[key5].IsSolidWall)
				{
					value.Walls[3] = true;
				}
				Point key6 = new Point(key.X - 1, key.Y - 1);
				if (Blocks.ContainsKey(key6) && Blocks[key6].IsSolidWall)
				{
					value.HasSolidWallToNW = true;
				}
			}
		}
	}

	public Vector4 GetRoomBounds()
	{
		int num = int.MaxValue;
		int num2 = int.MaxValue;
		int num3 = int.MinValue;
		int num4 = int.MinValue;
		foreach (MinimapBlock value in Blocks.Values)
		{
			if (value.Position.X < num)
			{
				num = value.Position.X;
			}
			if (value.Position.Y < num2)
			{
				num2 = value.Position.Y;
			}
			if (value.Position.X > num3)
			{
				num3 = value.Position.X;
			}
			if (value.Position.Y > num4)
			{
				num4 = value.Position.Y;
			}
		}
		return new Vector4(num, num2, num3, num4);
	}

	public void RefreshDimensions()
	{
		Vector4 roomBounds = GetRoomBounds();
		Width = (int)(roomBounds.Z - roomBounds.X) + 1;
		Height = (int)(roomBounds.W - roomBounds.Y) + 1;
	}

	internal bool IsAdjacentBlockFound(Point blockPosition, int directionIndex)
	{
		return ParentArea.IsAdjacentBlockFound(this, Position.Add(blockPosition), directionIndex);
	}

	public void Draw(SpriteBatch spriteBatch, SpriteSheet minimapSprite, Vector2 drawPosition, float zoom, bool shouldDrawDebug)
	{
		Color baseDrawColor = (IsDebug ? (Color.White * 0.66f) : Color.White);
		if (IsDebug && !shouldDrawDebug)
		{
			return;
		}
		foreach (KeyValuePair<Point, MinimapBlock> block in Blocks)
		{
			Point key = block.Key;
			Vector2 drawPosition2 = Vector2.Add(drawPosition, new Vector2((float)(key.X * 4) * zoom, (float)(key.Y * 4) * zoom));
			block.Value.Draw(spriteBatch, minimapSprite, drawPosition2, zoom, baseDrawColor);
		}
	}
}
