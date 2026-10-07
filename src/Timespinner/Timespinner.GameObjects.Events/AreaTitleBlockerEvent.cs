using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events;

internal class AreaTitleBlockerEvent : GameEvent
{
	public AreaTitleBlockerEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.DoesDrawBaseSprite = false;
		base.CanBeTriggered = false;
		Bbox = new Rectangle(0, 0, 16, 16);
	}
}
