using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.NPCs.Misc;

internal sealed class FutureAdvisorNPC : NPCBase
{
	public FutureAdvisorNPC(Level inLevel, Point inPosition, SpriteSheet inSpriteSheet, ObjectTileSpecification objSpec)
		: base(inLevel, inPosition, inSpriteSheet, -1)
	{
		_npcType = ENPCType.FutureAdvisor;
		_npcTriggerType = ENPCTriggerType.None;
		Bbox = new Rectangle(inPosition.X, inPosition.Y, 20, 40);
		base.DrawPlane = EDrawPlane.Front;
		_agility = 1f;
		_maxMoveSpeed = 150f;
		IsFacingLeft = !objSpec.IsFlippedHorizontally;
		_isAffectedByLevelBounds = false;
		ChangeAnimation(67, 5, 0.2f, EAnimationType.Cycle);
	}
}
