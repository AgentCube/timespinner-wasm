using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Cutscene;

namespace Timespinner.GameAbstractions.Gameplay;

public class LevelChangeRequest
{
	private const int MaxHeroOffset = 72;

	private Point _heroOffset;

	public bool IsUsingWarp { get; set; }

	public bool ShouldPlayLevelSong { get; set; }

	public bool IsUsingWhiteFadeOut { get; set; }

	internal bool IsDebugRequest { get; set; }

	internal CutsceneBase.ECutsceneType CutsceneToCall { get; set; }

	public EDirection EnterDirection { get; set; }

	public int RoomID { get; set; }

	public int LevelID { get; set; }

	public int PreviousLevelID { get; set; }

	public int CheckpointID { get; set; }

	public float AdditionalBlackScreenTime { get; set; }

	public float BlackScreenTimeTimer { get; set; }

	public float FadeOutTime { get; set; }

	public float FadeInTime { get; set; }

	public Point HeroOffset
	{
		get
		{
			return _heroOffset;
		}
		set
		{
			_heroOffset = value;
			if (_heroOffset.Y < -72)
			{
				_heroOffset.Y = -72;
			}
		}
	}

	public Point TargetBlockKey { get; set; }

	public Point EntryPosition { get; set; }

	public static EDirection ReverseDirection(EDirection original)
	{
		EDirection result = EDirection.West;
		switch (original)
		{
		case EDirection.West:
			result = EDirection.East;
			break;
		case EDirection.North:
			result = EDirection.South;
			break;
		case EDirection.East:
			result = EDirection.West;
			break;
		case EDirection.South:
			result = EDirection.North;
			break;
		}
		return result;
	}

	public static LevelChangeRequest TeleportLookup(Alive who, Level level, Point originalPosition, EDirection direction, bool isOrientatedUpright, Rectangle bbox)
	{
		LevelChangeRequest result = null;
		MinimapSpecification minimap = level.Minimap;
		MinimapRoom roomFromLevelAndRoom = minimap.GetRoomFromLevelAndRoom(level.ID, level.RoomID);
		if (roomFromLevelAndRoom != null)
		{
			Vector2 vector = new Vector2((float)originalPosition.X / (float)level.RoomSize.X, (float)originalPosition.Y / (float)level.RoomSize.Y);
			Point key = new Point((int)MathHelper.Clamp((float)Math.Floor(vector.X * (float)roomFromLevelAndRoom.Width), 0f, roomFromLevelAndRoom.Width - 1), (int)MathHelper.Clamp((float)Math.Floor(vector.Y * (float)roomFromLevelAndRoom.Height), 0f, roomFromLevelAndRoom.Height - 1));
			MinimapBlock minimapBlock = null;
			if (roomFromLevelAndRoom.Blocks.ContainsKey(key))
			{
				minimapBlock = roomFromLevelAndRoom.Blocks[key];
			}

			MinimapBlock blockFromPoint = null;
			if (minimapBlock != null)
			{
				Point pointFromDirection = Level.GetPointFromDirection(minimapBlock.Position, direction);
				Point targetPoint = new Point(minimapBlock.ParentRoom.Position.X + pointFromDirection.X, minimapBlock.ParentRoom.Position.Y + pointFromDirection.Y);
				blockFromPoint = minimap.GetBlockFromPoint(targetPoint);
			}

			// Fallback: If direct point key failed, search all blocks in this room for an adjacent room in target direction
			if (blockFromPoint == null && roomFromLevelAndRoom.Blocks.Count > 0)
			{
				Console.WriteLine($"[TeleportLookup WARNING] Direct block lookup missed for Pos {originalPosition}, Key {key}. Searching room blocks...");
				int closestDist = int.MaxValue;
				MinimapBlock bestTarget = null;
				Point bestKey = key;

				foreach (KeyValuePair<Point, MinimapBlock> kvp in roomFromLevelAndRoom.Blocks)
				{
					Point ptDir = Level.GetPointFromDirection(kvp.Value.Position, direction);
					Point tgtPt = new Point(kvp.Value.ParentRoom.Position.X + ptDir.X, kvp.Value.ParentRoom.Position.Y + ptDir.Y);
					MinimapBlock candidate = minimap.GetBlockFromPoint(tgtPt);
					if (candidate != null)
					{
						int blockCenterX = kvp.Key.X * 400 + 200;
						int blockCenterY = kvp.Key.Y * 320 + 160;
						int dist = Math.Abs(originalPosition.X - blockCenterX) + Math.Abs(originalPosition.Y - blockCenterY);
						if (dist < closestDist)
						{
							closestDist = dist;
							bestTarget = candidate;
							bestKey = kvp.Key;
						}
					}
				}

				if (bestTarget != null)
				{
					blockFromPoint = bestTarget;
					key = bestKey;
					Console.WriteLine($"[TeleportLookup FALLBACK] Matched target room {bestTarget.ParentRoom.RoomID} via block {bestKey}");
				}
			}

			if (blockFromPoint != null)
			{
				Point zero = Point.Zero;
				if (isOrientatedUpright)
				{
					zero.Y = who.Bbox.Bottom - bbox.Bottom;
				}
				else
				{
					zero.X = who.Bbox.Center.X - bbox.Center.X;
					zero.Y = who.Bbox.Bottom - bbox.Bottom + ((direction == EDirection.North) ? 16 : 0);
				}
				Point entryPosition = new Point(originalPosition.X - key.X * 25 * 16, originalPosition.Y - key.Y * 20 * 16);
				LevelChangeRequest levelChangeRequest = new LevelChangeRequest();
				levelChangeRequest.TargetBlockKey = blockFromPoint.Position;
				levelChangeRequest.RoomID = blockFromPoint.ParentRoom.RoomID;
				levelChangeRequest.LevelID = blockFromPoint.ParentRoom.ParentArea.LevelID;
				levelChangeRequest.HeroOffset = zero;
				levelChangeRequest.EnterDirection = ReverseDirection(direction);
				levelChangeRequest.EntryPosition = entryPosition;
				result = levelChangeRequest;
				Console.WriteLine($"[Transition TeleportLookup] Success: from Level {level.ID}, Room {level.RoomID} -> to Level {result.LevelID}, Room {result.RoomID}, EnterDir: {result.EnterDirection}, HeroOffset: {result.HeroOffset}");
			}
			else
			{
				Console.WriteLine($"[Transition TeleportLookup ERROR] Failed to resolve target block for Level {level.ID}, Room {level.RoomID}, Pos: {originalPosition}, Dir: {direction}");
			}
		}
		return result;
	}

	internal static LevelChangeRequest FromDirection(EDirection direction, Level level)
	{
		LevelChangeRequest result = null;
		MinimapSpecification minimap = level.Minimap;
		List<MinimapBlock> list = new List<MinimapBlock>();
		MinimapRoom roomFromLevelAndRoom = minimap.GetRoomFromLevelAndRoom(level.ID, level.RoomID);
		if (roomFromLevelAndRoom != null)
		{
			foreach (KeyValuePair<Point, MinimapBlock> block in roomFromLevelAndRoom.Blocks)
			{
				Point pointFromDirection = Level.GetPointFromDirection(block.Value.Position, direction);
				Point targetPoint = new Point(block.Value.ParentRoom.Position.X + pointFromDirection.X, block.Value.ParentRoom.Position.Y + pointFromDirection.Y);
				MinimapBlock blockFromPoint = minimap.GetBlockFromPoint(targetPoint);
				if (blockFromPoint != null && blockFromPoint.ParentRoom.RoomID != roomFromLevelAndRoom.RoomID)
				{
					list.Add(blockFromPoint);
				}
			}
			if (list.Count > 0)
			{
				MinimapBlock minimapBlock = null;
				Point position = level.MainHero.Position;
				Vector2 vector = new Vector2((float)position.X / (float)level.RoomSize.X, (float)position.Y / (float)level.RoomSize.Y);
				Point a = new Point((int)MathHelper.Clamp((float)Math.Floor(vector.X * (float)roomFromLevelAndRoom.Width), 0f, roomFromLevelAndRoom.Width - 1), (int)MathHelper.Clamp((float)Math.Floor(vector.Y * (float)roomFromLevelAndRoom.Height), 0f, roomFromLevelAndRoom.Height - 1));
				a = a.Add(roomFromLevelAndRoom.Position);
				int num = int.MaxValue;
				foreach (MinimapBlock item in list)
				{
					int num2 = item.ParentRoom.Position.DistanceSquared(a);
					if (minimapBlock == null || num > num2)
					{
						num = num2;
						minimapBlock = item;
					}
				}
				if (minimapBlock != null)
				{
					LevelChangeRequest levelChangeRequest = new LevelChangeRequest();
					levelChangeRequest.TargetBlockKey = minimapBlock.Position;
					levelChangeRequest.RoomID = minimapBlock.ParentRoom.RoomID;
					levelChangeRequest.LevelID = minimapBlock.ParentRoom.ParentArea.LevelID;
					levelChangeRequest.EnterDirection = ReverseDirection(direction);
					levelChangeRequest.IsDebugRequest = true;
					result = levelChangeRequest;
				}
			}
		}
		return result;
	}
}
