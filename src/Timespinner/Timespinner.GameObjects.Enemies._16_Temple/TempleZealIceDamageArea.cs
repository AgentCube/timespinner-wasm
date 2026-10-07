using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies._16_Temple;

internal sealed class TempleZealIceDamageArea : DamageArea
{
	private const int IceUnitCount = 9;

	private const float TimeBetweenUnits = 0.1f;

	private const float TotalCreationTime = 0.90000004f;

	private const float MaxLife = 1.9f;

	private readonly TempleZealIceUnit[] _iceUnits = new TempleZealIceUnit[9];

	private bool _isTravelingLeft;

	private bool _isOnFloor;

	private int _creationIndex;

	private float _creationTimer;

	private Point _creationPoint;

	public TempleZealIceDamageArea(Level inLevel, Point inPosition, int inDamage, SpriteSheet sprite)
		: base(inLevel, inPosition, ETeamSide.Enemies, -1, null)
	{
		_sprite = sprite;
		_damageElement = EDamageElement.Ice;
		ChangeAnimation(-1);
		Bbox = Rectangle.Empty;
		_doesUseAppendageCollision = true;
		_doAppendagesInheritDrawColor = false;
		_life = 1.9f;
		base.DamageTimeoutTime = 0.7f;
		_power = inDamage;
	}

	internal static Tile FindNextTile(Level level, Point startPoint, bool isTravelingLeft, bool isOnFloor)
	{
		Tile tile = null;
		Point start = startPoint;
		EDirection where = ((!isTravelingLeft) ? EDirection.East : EDirection.West);
		if (level.CheckNearbySolid(where, start))
		{
			start = Level.GetPointFromDirection(start, where);
			where = (isOnFloor ? EDirection.North : EDirection.South);
			if (level.CheckNearbySolid(where, start))
			{
				start = Level.GetPointFromDirection(start, where);
				if (!level.CheckNearbySolid(where, start))
				{
					tile = level.SolidTiles[start];
				}
				else
				{
					start = Level.GetPointFromDirection(start, where);
					if (!level.CheckNearbySolid(where, start))
					{
						tile = level.SolidTiles[start];
					}
				}
			}
			else
			{
				tile = level.SolidTiles[start];
			}
		}
		else
		{
			start = Level.GetPointFromDirection(start, where);
			where = (isOnFloor ? EDirection.South : EDirection.North);
			if (level.CheckNearbySolid(where, start))
			{
				start = Level.GetPointFromDirection(start, where);
				tile = level.SolidTiles[start];
			}
		}
		if (tile != null)
		{
			where = (isOnFloor ? EDirection.South : EDirection.North);
			if (!level.CheckNearbySolid(where, tile.DictKey))
			{
				tile = null;
			}
		}
		return tile;
	}

	public override void Update(float delta)
	{
		if (base.IsFrozen)
		{
			_isFrozen = false;
		}
		if (!base.IsFrozen && _creationTimer < 0.90000004f)
		{
			_creationTimer += delta;
			while (_creationTimer > (float)_creationIndex * 0.1f && _creationIndex < 9)
			{
				CreateUnit();
				_creationIndex++;
			}
		}
		base.Update(delta);
	}

	private void CreateUnit()
	{
		if (_creationIndex >= 9)
		{
			return;
		}
		Tile tile = FindNextTile(startPoint: new Point(_creationPoint.X / 16, _creationPoint.Y / 16), level: _level, isTravelingLeft: _isTravelingLeft, isOnFloor: _isOnFloor);
		if (tile != null)
		{
			TempleZealIceUnit templeZealIceUnit;
			if (_iceUnits[_creationIndex] == null)
			{
				templeZealIceUnit = new TempleZealIceUnit(this, new Point(16, 16), Point.Zero, _level, _sprite);
				_iceUnits[_creationIndex] = templeZealIceUnit;
				_appendages.Add(templeZealIceUnit);
			}
			else
			{
				templeZealIceUnit = _iceUnits[_creationIndex];
			}
			bool isSkinny = IsCliffTile(tile, _isTravelingLeft, _isOnFloor);
			_creationPoint = tile.Bbox.Center;
			Point position = new Point(_creationPoint.X, _isOnFloor ? tile.Bbox.Bottom : tile.Bbox.Top);
			templeZealIceUnit.Reset(position, _isTravelingLeft, !_isOnFloor, isSkinny);
		}
		else
		{
			_creationIndex = 9;
		}
	}

	private static bool IsCliffTile(Tile tile, bool isTravelingLeft, bool isOnFloor)
	{
		bool result = false;
		Level level = tile.Level;
		EDirection where = ((!isTravelingLeft) ? EDirection.East : EDirection.West);
		Point dictKey = tile.DictKey;
		if (!level.CheckNearbySolid(where, dictKey))
		{
			dictKey = Level.GetPointFromDirection(dictKey, where);
			where = (isOnFloor ? EDirection.South : EDirection.North);
			if (!level.CheckNearbySolid(where, dictKey))
			{
				result = true;
			}
		}
		return result;
	}

	internal void Reset(Point position, bool isTravelingLeft, bool isOnFloor)
	{
		base.ID = -1;
		Position = position;
		_isTravelingLeft = isTravelingLeft;
		_isOnFloor = isOnFloor;
		_creationPoint = position;
		_creationIndex = 0;
		_creationTimer = 0f;
		_life = 1.9f;
		_isFading = false;
		_fadeTimer = 0f;
	}
}
