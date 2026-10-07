using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.NPCs.Misc;

internal sealed class FutureKnightNPC : NPCBase
{
	public FutureKnightNPC(Level inLevel, Point inPosition, SpriteSheet inSpriteSheet, ObjectTileSpecification objSpec)
		: base(inLevel, inPosition, inSpriteSheet, -1)
	{
		_npcType = ENPCType.FutureKnight;
		_npcTriggerType = ENPCTriggerType.None;
		Bbox = new Rectangle(inPosition.X, inPosition.Y, 32, 48);
		base.DrawPlane = EDrawPlane.Front;
		_agility = 1f;
		_maxMoveSpeed = 150f;
		IsFacingLeft = !objSpec.IsFlippedHorizontally;
		_isAffectedByLevelBounds = false;
		ChangeAnimation(72);
	}
}
