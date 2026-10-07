using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events;

public sealed class WaterFillerEvent : GameEvent
{
	public enum EWaterType
	{
		FillerTopLeft,
		FillerBottomRight,
		Top,
		Bottom
	}

	public bool HasBeenHandled { get; set; }

	public bool IsVisible { get; set; }

	public EWaterType CurrentEWaterType { get; private set; }

	public Point Position16 { get; private set; }

	public WaterFillerEvent(Level inLevel, Point inPosition, Point inDictPosition, int inID, bool isTopLeft, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_sprite = inLevel.GCM.TsEventTiles;
		CurrentEWaterType = ((!isTopLeft) ? EWaterType.FillerBottomRight : EWaterType.FillerTopLeft);
		Position16 = inDictPosition;
		_isSolid = false;
		_doesPersist = true;
		_isAffectedByGravity = false;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (IsVisible)
		{
			base.Draw(spriteBatch);
		}
	}

	public void FillRoomWithWater(Dictionary<Point, WaterTile> inWaterTiles, List<WaterFillerEvent> inWaterFillers, List<WaterTile> updatableTiles)
	{
		if (CurrentEWaterType != 0 || HasBeenHandled)
		{
			return;
		}
		HasBeenHandled = true;
		WaterFillerEvent waterFillerEvent = FindNearestBottomRight(this, inWaterFillers);
		if (waterFillerEvent == null)
		{
			return;
		}
		waterFillerEvent.HasBeenHandled = true;
		for (int i = Position16.X; i <= waterFillerEvent.Position16.X; i++)
		{
			Point point = new Point(i, Position16.Y);
			if (inWaterTiles.ContainsKey(point))
			{
				continue;
			}
			if (IsTileValidForWater(point, _level.SolidTiles))
			{
				WaterTile waterTile = new WaterTile(new Point(point.X * 16, point.Y * 16), point, _level, _level.GCM.TsEventTiles, EWaterType.Top);
				inWaterTiles.Add(waterTile.Position16, waterTile);
				updatableTiles.Add(waterTile);
			}
			for (int j = Position16.Y + 1; j <= waterFillerEvent.Position16.Y; j++)
			{
				point.Y = j;
				if (IsTileValidForWater(point, _level.SolidTiles))
				{
					WaterTile waterTile2 = new WaterTile(new Point(point.X * 16, point.Y * 16), point, _level, _level.GCM.TsEventTiles, EWaterType.Bottom);
					if (!inWaterTiles.ContainsKey(waterTile2.Position16))
					{
						inWaterTiles.Add(waterTile2.Position16, waterTile2);
					}
				}
			}
		}
	}

	private static WaterFillerEvent FindNearestBottomRight(WaterFillerEvent inTopLeft, IEnumerable<WaterFillerEvent> inAllWaterEvents)
	{
		int num = 9999999;
		WaterFillerEvent result = null;
		foreach (WaterFillerEvent inAllWaterEvent in inAllWaterEvents)
		{
			if (inAllWaterEvent.CurrentEWaterType == EWaterType.FillerBottomRight && inTopLeft.Position.X < inAllWaterEvent.Position.X && inTopLeft.Position.Y < inAllWaterEvent.Position.Y && !inAllWaterEvent.HasBeenHandled)
			{
				int num2 = inAllWaterEvent.Position16.Y - inTopLeft.Position16.Y;
				int num3 = inAllWaterEvent.Position16.X - inTopLeft.Position16.X;
				int num4 = num3 * num3 + num2 * num2;
				if (num4 >= 0 && num4 < num)
				{
					result = inAllWaterEvent;
					num = num4;
				}
			}
		}
		return result;
	}

	private static bool IsTileValidForWater(Point inPoint, Dictionary<Point, Tile> inSolidTiles)
	{
		if (!inSolidTiles.ContainsKey(inPoint))
		{
			return true;
		}
		Tile tile = inSolidTiles[inPoint];
		if (tile.Type != ETileType.Solid || tile.Special == ETileSpecialType.HorizontalSpike || tile.Special == ETileSpecialType.VerticalSpike)
		{
			return true;
		}
		if (inSolidTiles.ContainsKey(new Point(inPoint.X + 1, inPoint.Y)) && inSolidTiles.ContainsKey(new Point(inPoint.X - 1, inPoint.Y)))
		{
			return false;
		}
		return true;
	}
}
