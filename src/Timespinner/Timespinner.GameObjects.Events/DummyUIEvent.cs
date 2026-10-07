using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Misc;

namespace Timespinner.GameObjects.Events;

internal class DummyUIEvent : GameEvent
{
	public DummyUIEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_sprite = _level.GCM.SpPauseMenu;
		_doesDrawBaseSprite = false;
		_doesUseAppendageCollision = false;
		_doAppendagesMatchImageFacing = true;
		Bbox = new Rectangle(0, 0, 16, 16);
		_level.RequestAddObject(new SoulStreamEvent(_level, inPosition, isAnchoredToPlayer: true));
	}
}
