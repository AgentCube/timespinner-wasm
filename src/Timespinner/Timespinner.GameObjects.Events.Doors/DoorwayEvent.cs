using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.Doors;

internal sealed class DoorwayEvent : TeleportEvent
{
	public DoorwayEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification specification, int doorID)
		: base(inLevel, inPosition, (!specification.IsFlippedVertically) ? EDirection.North : EDirection.South, inID, doorID, specification)
	{
		_bbox = new Rectangle(0, 0, 32, 64);
		Position = new Point(_position.X, _position.Y);
		_isSolid = false;
		_doesPersist = true;
		_isAffectedByGravity = false;
		_isRepeatedTrigger = false;
		base.IsTriggerableByMonsters = false;
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		if (who is Protagonist protagonist)
		{
			_isTriggered = true;
			if (protagonist.CheckButton(4, isNewPressOnly: true) && !protagonist.CheckButton(5) && !protagonist.CheckButton(7))
			{
				_level.RequestChangeRoom(LevelChangeRequest.TeleportLookup(who, _level, _position, base.Direction, isOrientatedUpright: true, _bbox));
			}
		}
		return false;
	}

	public override void Update(float delta)
	{
		if (_isTriggered)
		{
			_level.RequestButtonPrompt(4, new Point(Bbox.Center.X, Bbox.Top));
		}
		base.Update(delta);
		_isTriggered = false;
	}
}
