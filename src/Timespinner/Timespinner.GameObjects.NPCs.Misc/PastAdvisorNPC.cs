using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.NPCs.Misc;

internal sealed class PastAdvisorNPC : NPCBase
{
	public PastAdvisorNPC(Level inLevel, Point inPosition, SpriteSheet inSpriteSheet)
		: base(inLevel, inPosition, inSpriteSheet, -1)
	{
		_npcType = ENPCType.PastAdvisor;
		_npcTriggerType = ENPCTriggerType.None;
		Bbox = new Rectangle(inPosition.X, inPosition.Y, 20, 40);
		base.DrawPlane = EDrawPlane.Front;
		_agility = 1f;
		_maxMoveSpeed = 150f;
		_isAffectedByLevelBounds = false;
		ChangeAnimation(60, 5, 0.2f, EAnimationType.Cycle);
	}
}
