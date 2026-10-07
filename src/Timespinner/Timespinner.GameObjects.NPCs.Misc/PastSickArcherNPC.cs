using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.NPCs.Misc;

internal sealed class PastSickArcherNPC : NPCBase
{
	private const int Anim_Start = 66;

	public PastSickArcherNPC(Level inLevel, Point inPosition, SpriteSheet inSpriteSheet)
		: base(inLevel, inPosition, inSpriteSheet, -1)
	{
		_npcType = ENPCType.PastSickArcher;
		_npcTriggerType = ENPCTriggerType.None;
		Bbox = new Rectangle(inPosition.X, inPosition.Y, 44, 12);
		base.DrawPlane = EDrawPlane.Front;
		ChangeAnimation(66);
	}
}
