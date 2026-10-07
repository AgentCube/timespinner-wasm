using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Doors;

public class TeleportEvent : GameEvent
{
	private const float TimeToWaitBeforeUse = 0.03f;

	private readonly bool _isOrientatedUpright;

	private readonly int _doorCount;

	private readonly EDirection _direction;

	private readonly Point _originalPosition;

	private float _timeSinceZoned;

	public bool IsOrientatedUpright => _isOrientatedUpright;

	internal bool IsActive { get; set; }

	public EDirection Direction => _direction;

	public int DoorCount => _doorCount;

	public Point DefaultTeleportPosition
	{
		get
		{
			Point position = Position;
			if (_direction == EDirection.South)
			{
				position.Y -= 16;
			}
			return position;
		}
	}

	public TeleportEvent(Level inLevel, Point inPosition, EDirection whichDirection, int inID, int inDoorCount, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_originalPosition = inPosition;
		_direction = whichDirection;
		switch (_direction)
		{
		case EDirection.West:
			_isOrientatedUpright = true;
			_bbox = new Rectangle(0, 0, 16, 96);
			base.EventType = EEventTileType.WestTeleport;
			_originalPosition = new Point(_originalPosition.X, _originalPosition.Y - 1);
			break;
		case EDirection.North:
			_isOrientatedUpright = false;
			_bbox = new Rectangle(0, 0, 144, 16);
			base.EventType = EEventTileType.NorthTeleport;
			break;
		case EDirection.East:
			_isOrientatedUpright = true;
			_bbox = new Rectangle(0, 0, 16, 96);
			base.EventType = EEventTileType.EastTeleport;
			_originalPosition = new Point(_originalPosition.X, _originalPosition.Y - 1);
			break;
		case EDirection.South:
			_isOrientatedUpright = false;
			_bbox = new Rectangle(0, 0, 144, 16);
			base.EventType = EEventTileType.SouthTeleport;
			break;
		default:
			_isOrientatedUpright = true;
			_bbox = new Rectangle(0, 0, 16, 16);
			break;
		}
		_isAffectedByGravity = false;
		_doorCount = inDoorCount;
		_doesPersist = true;
		_isRepeatedTrigger = false;
		IsActive = true;
	}

	public override void Initialize()
	{
		SnapBboxToPosition();
		base.Initialize();
	}

	public override void Update(float delta)
	{
		if (_timeSinceZoned < 0.03f)
		{
			_timeSinceZoned += delta;
		}
		base.Update(delta);
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		bool result = false;
		if (_level.AreTeleportExitsUnlocked && !_level.IsPaused && IsActive && (!_isOrientatedUpright || _timeSinceZoned >= 0.03f))
		{
			if (!_isSolid)
			{
				if (CheckIfTriggered(who))
				{
					System.Console.WriteLine($"[TeleportEvent] Triggered door dir {_direction} at {_originalPosition}, Level {_level.ID}, Room {_level.RoomID}");
					base.TriggerEvent(who, depth);
					LevelChangeRequest levelChangeRequest = LevelChangeRequest.TeleportLookup(who, _level, _originalPosition, _direction, _isOrientatedUpright, _bbox);
					if (levelChangeRequest != null)
					{
						if (levelChangeRequest.LevelID == _level.ID)
						{
							_level.RequestChangeRoom(levelChangeRequest);
						}
						else
						{
							levelChangeRequest.HeroOffset = Point.Zero;
							_level.RequestChangeLevel(levelChangeRequest);
						}
					}
					else
					{
						System.Console.WriteLine($"[TeleportEvent WARNING] TeleportLookup returned null for Level {_level.ID}, Room {_level.RoomID}, dir {_direction} at {_originalPosition}!");
					}
				}
			}
			else
			{
				base.TriggerEvent(who, depth);
				result = true;
			}
		}
		return result;
	}

	private bool CheckIfTriggered(Alive who)
	{
		bool result = true;
		switch (_direction)
		{
		case EDirection.West:
			result = who.Bbox.Left <= _bbox.Left && who.IsMovingLeft;
			break;
		case EDirection.East:
			result = who.Bbox.Right >= _bbox.Right && who.IsMovingRight;
			break;
		case EDirection.North:
			result = who.Bbox.Bottom <= _bbox.Bottom && who.Velocity.Y <= 0f;
			break;
		case EDirection.South:
			result = who.Bbox.Bottom >= _bbox.Bottom + 16 && who.Velocity.Y > 0f;
			break;
		}
		return result;
	}
}
