using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.Misc;

public class TutorialEvent : GameEvent
{
	public enum ETutorialType
	{
		Movement,
		Attack,
		Spell,
		Jumping,
		Timestop,
		Platforms
	}

	private const int PlatformTutorialOffsetX = 6;

	private const int PlatformTutorialOffsetY = -8;

	private readonly ETutorialType _tutorialType;

	public TutorialEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_tutorialType = (ETutorialType)(objectSpec?.Argument ?? 0);
		Bbox = new Rectangle(0, 0, 24, 24);
		_doesDrawSpriteAndAppendages = false;
		_isSolid = false;
		base.CanBeTriggered = true;
		_isRepeatedTrigger = true;
		base.IsTriggerableByMonsters = false;
		base.CannotBeGrabbed = true;
		base.DoesCollideWithTiles = false;
		base.DoesCollideWithProjectiles = true;
		_isAffectedByGravity = false;
		_isFlying = true;
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		if (who is LunaisObj lunaisObj)
		{
			ETutorialType tutorialType = _tutorialType;
			if (tutorialType == ETutorialType.Platforms && lunaisObj.IsDucking)
			{
				_level.RequestButtonPrompt(0, new Point(Position.X + 6, Position.Y + -8));
			}
		}
		return base.TriggerEvent(who, depth);
	}
}
