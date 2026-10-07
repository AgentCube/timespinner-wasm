using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class LabAdultBlockerEvent : GameEvent
{
	public LabAdultBlockerEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		Bbox = new Rectangle(0, 0, 32, 144);
		_doesDrawBaseSprite = false;
		_isAffectedByGravity = false;
		_isFlying = true;
		_isSolid = true;
		base.IsTriggerableByMonsters = false;
		base.CanBeTriggered = true;
		base.DoesCollideWithTiles = false;
		base.CannotBeGrabbed = true;
	}
}
