using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.NPCs;

internal sealed class ElderNPC : NPCBase
{
	private const int Anim_IdleStart = 82;

	private const int Anim_IdleLength = 4;

	public ElderNPC(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inLevel.GCM.SpSelen, inID)
	{
		_npcType = ENPCType.Elder;
		_bbox = new Rectangle(0, 0, 22, 38);
		_bboxOffset = new Point(0, 0);
		base.TriggerBbox = new Rectangle(0, 0, 64, 32);
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_isAffectedByTime = true;
		base.CannotBeGrabbed = true;
		_isAffectedByGravity = false;
		_isSolid = false;
		_npcTriggerType = ENPCTriggerType.Talk;
		ChangeAnimation(82, 4, 0.2f, EAnimationType.Cycle);
	}

	protected override void TriggerNPC(Protagonist who)
	{
	}
}
