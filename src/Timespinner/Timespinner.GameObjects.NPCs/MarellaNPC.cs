using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.NPCs;

internal sealed class MarellaNPC : NPCBase
{
	private const int Anim_IdleStart = 35;

	private const int Anim_IdleLength = 4;

	private const int Anim_WalkStart = 39;

	private const int Anim_WalkLength = 6;

	private const int Anim_CakeWalkStart = 45;

	private const int Anim_CakeWalkLength = 4;

	private const int Anim_CakeSetDownStart = 49;

	private const int Anim_CakeSetDownLength = 4;

	private const float Anim_IdleSpeed = 0.2f;

	private const float Anim_WalkSpeed = 0.1f;

	private bool _doesHaveCake;

	public MarellaNPC(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inLevel.GCM.SpWinderians, inID)
	{
		_npcType = ENPCType.Marella;
		_bbox = new Rectangle(0, 0, 18, 31);
		_bboxOffset = new Point(7, 4);
		base.TriggerBbox = new Rectangle(0, 0, 64, 32);
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_isAffectedByTime = true;
		base.CannotBeGrabbed = true;
		_isAffectedByGravity = true;
		_isSolid = false;
		_agility = 0.5f;
		_isAffectedByLevelBounds = false;
		_npcTriggerType = ENPCTriggerType.Talk;
		SetState(EAFSM.Idle);
	}

	public override void SetState(EAFSM state)
	{
		base.SetState(state);
		switch (state)
		{
		case EAFSM.Running:
			if (_doesHaveCake)
			{
				ChangeAnimation(45, 4, 0.1f, EAnimationType.Cycle);
			}
			else
			{
				ChangeAnimation(39, 6, 0.1f, EAnimationType.Cycle);
			}
			break;
		case EAFSM.Idle:
			if (_doesHaveCake)
			{
				ChangeAnimation(45);
			}
			else
			{
				ChangeAnimation(35, 4, 0.2f, EAnimationType.Cycle);
			}
			break;
		}
	}

	internal void ToggleCake()
	{
		_doesHaveCake = !_doesHaveCake;
		_agility = (_doesHaveCake ? 0.3f : 0.5f);
		_maxMoveSpeed = (_doesHaveCake ? 60 : 100);
	}

	internal void ToggleMoonWalking()
	{
		base.IsMoonWalking = !base.IsMoonWalking;
	}

	internal void SetDownCake()
	{
		AnimationSpec[] newAnimations = new AnimationSpec[2]
		{
			new AnimationSpec
			{
				Start = 49,
				Length = 4,
				Speed = 0.1f,
				Type = EAnimationType.Once
			},
			new AnimationSpec
			{
				Start = 35,
				Length = 4,
				Speed = 0.2f,
				Type = EAnimationType.Once
			}
		};
		ChangeAnimation(newAnimations);
	}

	protected override void TriggerNPC(Protagonist who)
	{
		base.TriggerNPC(who);
		StartNPCDialogue();
		MovePlayerToTalkingPosition();
		AddDialogue("cs_pro_lun_08");
		AddDialogue("cs_pro_mar_09");
		AddDialogue("cs_pro_mar_10");
		AddDialogue("cs_pro_lun_11");
		AddDialogue("cs_prom_mar_00");
		AddDialogue("cs_prom_lun_01");
		EndNPCDialogue();
	}
}
