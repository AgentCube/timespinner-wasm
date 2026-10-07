using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.NPCs;

internal sealed class JianaNPC : NPCBase
{
	private const int Anim_IdleStart = 29;

	private const int Anim_IdleLength = 4;

	public JianaNPC(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inLevel.GCM.SpWinderians, inID)
	{
		_npcType = ENPCType.Jiana;
		_bbox = new Rectangle(0, 0, 18, 37);
		_bboxOffset = new Point(3, 0);
		base.TriggerBbox = new Rectangle(0, 0, 64, 32);
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_isAffectedByTime = true;
		base.CannotBeGrabbed = true;
		_isAffectedByGravity = false;
		_isSolid = false;
		_npcTriggerType = ENPCTriggerType.Talk;
		ChangeAnimation(29, 4, 0.2f, EAnimationType.Cycle);
	}

	protected override void TriggerNPC(Protagonist who)
	{
		base.TriggerNPC(who);
		StartNPCDialogue();
		MovePlayerToTalkingPosition();
		AddDialogue("cs_pro_jia_00");
		EndNPCDialogue();
	}
}
