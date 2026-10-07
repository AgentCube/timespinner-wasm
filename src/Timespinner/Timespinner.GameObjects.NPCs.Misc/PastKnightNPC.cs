using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.NPCs.Misc;

internal sealed class PastKnightNPC : NPCBase
{
	public PastKnightNPC(Level inLevel, Point inPosition, SpriteSheet inSpriteSheet)
		: base(inLevel, inPosition, inSpriteSheet, -1)
	{
		_npcType = ENPCType.PastKnight;
		_npcTriggerType = ENPCTriggerType.None;
		Bbox = new Rectangle(inPosition.X, inPosition.Y, 32, 48);
		base.DrawPlane = EDrawPlane.Front;
		_agility = 1f;
		_maxMoveSpeed = 150f;
		_isAffectedByLevelBounds = false;
		ChangeAnimation(65);
	}
}
