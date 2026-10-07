using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.NPCs;

internal sealed class MessengerNPC : NPCBase
{
	private const int Anim_IdleStart = 24;

	private const int Anim_IdleLength = 5;

	public MessengerNPC(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inLevel.GCM.SpWinderians, inID)
	{
		_npcType = ENPCType.Messenger;
		_bbox = new Rectangle(0, 0, 18, 34);
		_bboxOffset = new Point(1, 0);
		base.TriggerBbox = new Rectangle(0, 0, 64, 32);
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_isAffectedByTime = true;
		base.CannotBeGrabbed = true;
		_isAffectedByGravity = false;
		_isSolid = false;
		_npcTriggerType = ENPCTriggerType.Talk;
		ChangeAnimation(24, 5, 0.15f, EAnimationType.Cycle);
	}

	protected override void TriggerNPC(Protagonist who)
	{
		base.TriggerNPC(who);
		StartNPCDialogue();
		MovePlayerToTalkingPosition();
		AddDialogue("cs_pro_car_07");
		EndNPCDialogue();
	}
}
