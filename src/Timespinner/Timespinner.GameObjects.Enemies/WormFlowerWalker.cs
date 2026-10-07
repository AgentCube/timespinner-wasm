using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class WormFlowerWalker : WormFlower
{
	public WormFlowerWalker(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.FollowAttack;
		_movementType = EAIMovementType.Walk;
		_agility = 0.15f;
		_attackDistanceThresholdX = 180;
		_retreatDistanceThresholdX = 120;
		_isAfraidOfBeingTooClose = true;
		_timeToMove = 0.3f;
		base.DeaggroBboxDimensions = new Point(800, 300);
		base.SpriteFrameOffset = 24;
	}

	public override void SetState(EAFSM state)
	{
		base.SetState(state);
		if (state == EAFSM.Running)
		{
			ChangeAnimation(10, 4, 0.1f, EAnimationType.Cycle);
		}
		else
		{
			ChangeAnimation(10, 1, 0.1f, EAnimationType.None);
		}
	}
}
