using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.NPCs.Misc;

internal sealed class PhiliaNPC : NPCBase
{
	public PhiliaNPC(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inLevel.GCM.SpAelana, inID)
	{
		_npcType = ENPCType.Philia;
		_bboxOffset = new Point(8, 6);
		Bbox = new Rectangle(_position.X, _position.Y, 25, 38);
		base.TriggerBbox = new Rectangle(0, 0, 80, 32);
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_isAffectedByTime = true;
		base.CannotBeGrabbed = true;
		_isAffectedByGravity = false;
		_isSolid = false;
		ChangeAnimation(71, 4, 0.125f, EAnimationType.PingPong);
		Update(0f);
	}

	protected override void TriggerNPC(Protagonist who)
	{
		base.TriggerNPC(who);
		StartNPCDialogue();
		MovePlayerToTalkingPosition();
		AddDialogue("Something By Aelana");
		EndNPCDialogue();
	}
}
