using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.NPCs;

internal sealed class FaronNPC : NPCBase
{
	private const int Anim_IdleStart = 33;

	public FaronNPC(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inLevel.GCM.SpWinderians, inID)
	{
		_npcType = ENPCType.Faron;
		Position = Position.Add(-8, 0);
		_bbox = new Rectangle(0, 0, 18, 53);
		_bboxOffset = new Point(0, 0);
		base.TriggerBbox = new Rectangle(0, 0, 64, 32);
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_isAffectedByTime = true;
		base.CannotBeGrabbed = true;
		_isAffectedByGravity = false;
		_isSolid = false;
		_npcTriggerType = ENPCTriggerType.Talk;
		ChangeAnimation(33);
	}

	protected override void TriggerNPC(Protagonist who)
	{
		base.TriggerNPC(who);
		StartNPCDialogue();
		MovePlayerToTalkingPosition();
		AddDialogue("cs_pro_far_01");
		AddDialogue("cs_prof_lun_00");
		AddDialogue("cs_prof_far_01");
		EndNPCDialogue();
	}
}
