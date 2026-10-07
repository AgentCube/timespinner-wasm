using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.NPCs;

internal sealed class YorneNPC : NPCBase
{
	private const int Anim_IdleStart = 0;

	private const int Anim_IdleLength = 4;

	private const int Anim_CrossArmsStart = 4;

	private const int Anim_CrossArmsLength = 3;

	private const int Anim_SpitStart = 6;

	private const int Anim_SpitLength = 3;

	private const int Anim_RecoilStart = 9;

	private const int Anim_RecoilLength = 1;

	private const int Anim_WalkStart = 10;

	private const int Anim_WalkLength = 6;

	private const int Anim_TurnStart = 16;

	private const int Anim_TurnLength = 1;

	private const int Anim_TableFlipStart = 17;

	private const int Anim_TableFlipLength = 7;

	private const int Anim_PostRecoilStart = 3;

	private const int Anim_PostRecoilLength = 2;

	private const float Anim_IdleSpeed = 0.2f;

	private const float Anim_WalkSpeed = 0.1f;

	private readonly YorneSpitParticleSystem _spitParticles;

	private bool _isWalkingFast;

	public YorneNPC(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inLevel.GCM.SpWinderians, inID)
	{
		_npcType = ENPCType.Yorne;
		_bbox = new Rectangle(0, 0, 18, 39);
		_bboxOffset = new Point(3, 1);
		base.TriggerBbox = new Rectangle(0, 0, 64, 32);
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_isAffectedByTime = true;
		base.CannotBeGrabbed = true;
		_isAffectedByGravity = true;
		_isSolid = false;
		_agility = 0.5f;
		_npcTriggerType = ENPCTriggerType.Talk;
		_isAffectedByLevelBounds = false;
		_spitParticles = new YorneSpitParticleSystem(_level.GCM.TxParticleEnergy, 1);
		_particleSystems.Add(_spitParticles);
		ToggleFastMode();
		ToggleFastMode();
	}

	public override void SetState(EAFSM state)
	{
		base.SetState(state);
		switch (state)
		{
		case EAFSM.Running:
		{
			float speed = (_isWalkingFast ? 0.1f : 0.15f);
			ChangeAnimation(10, 6, speed, EAnimationType.Cycle);
			break;
		}
		case EAFSM.Idle:
			ChangeAnimation(0, 4, 0.2f, EAnimationType.Cycle);
			break;
		}
	}

	internal void ToggleFastMode()
	{
		_isWalkingFast = !_isWalkingFast;
		_agility = (_isWalkingFast ? 0.5f : 0.3f);
		_maxMoveSpeed = (_isWalkingFast ? 100 : 60);
	}

	internal void CrossArms()
	{
		ChangeAnimation(4, 3, 0.1f, EAnimationType.Once);
	}

	internal void Spit()
	{
		AnimationSpec[] newAnimations = new AnimationSpec[2]
		{
			new AnimationSpec
			{
				Start = 6,
				Length = 3,
				Speed = 0.1f,
				Type = EAnimationType.Once
			},
			new AnimationSpec
			{
				Start = 6,
				Length = 3,
				Speed = 0.1f,
				Type = EAnimationType.Once,
				IsInReverse = true
			}
		};
		ChangeAnimation(newAnimations);
	}

	internal void EmitSpit()
	{
		_spitParticles.AddParticles(new Vector2(Position.X + 4, Position.Y + -29));
	}

	internal void DoTableFlip()
	{
		AnimationSpec[] newAnimations = new AnimationSpec[2]
		{
			new AnimationSpec
			{
				Start = 17,
				Length = 7,
				Speed = 0.1f,
				Type = EAnimationType.Once
			},
			new AnimationSpec
			{
				Start = 0,
				Length = 4,
				Speed = 0.2f,
				Type = EAnimationType.Cycle
			}
		};
		ChangeAnimation(newAnimations);
		ToggleFastMode();
	}

	internal void Recoil()
	{
		AnimationSpec[] newAnimations = new AnimationSpec[2]
		{
			new AnimationSpec
			{
				Start = 9,
				Length = 1,
				Speed = 0.4f,
				Type = EAnimationType.Once
			},
			new AnimationSpec
			{
				Start = 3,
				Length = 2,
				Speed = 0.1f,
				Type = EAnimationType.Once,
				IsInReverse = true
			}
		};
		ChangeAnimation(newAnimations);
	}

	protected override void TriggerNPC(Protagonist who)
	{
		base.TriggerNPC(who);
		StartNPCDialogue();
		MovePlayerToTalkingPosition();
		AddDialogue("cs_pro_lun_02");
		AddDialogue("cs_pro_yor_03");
		AddDialogue("cs_pro_lun_04");
		AddDialogue("cs_pro_yor_05");
		AddDialogue("cs_pro_lun_06");
		EndNPCDialogue();
	}
}
