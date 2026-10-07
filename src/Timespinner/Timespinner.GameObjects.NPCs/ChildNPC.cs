using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.NPCs;

internal sealed class ChildNPC : NPCBase
{
	internal const int Anim_IdleStart = 48;

	private const int Anim_WalkStart = 49;

	private const int Anim_WalkLength = 4;

	internal const int Anim_PrayStart = 53;

	internal const int Anim_PrayLength = 2;

	private const float Anim_WalkSpeed = 0.15f;

	public ChildNPC(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inLevel.GCM.SpSelen, inID)
	{
		_npcType = ENPCType.Child;
		_bbox = new Rectangle(0, 0, 16, 25);
		_bboxOffset = new Point(0, 0);
		base.TriggerBbox = new Rectangle(0, 0, 64, 32);
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_isAffectedByTime = true;
		base.CannotBeGrabbed = true;
		_isAffectedByGravity = false;
		_isSolid = false;
		_agility = 0.3f;
		_npcTriggerType = ENPCTriggerType.Talk;
		ChangeAnimation(48);
	}

	public override void SetState(EAFSM state)
	{
		base.SetState(state);
		switch (state)
		{
		case EAFSM.Running:
			ChangeAnimation(49, 4, 0.15f, EAnimationType.Cycle);
			break;
		case EAFSM.Idle:
			ChangeAnimation(48);
			break;
		}
	}
}
