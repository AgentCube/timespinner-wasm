using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Cutscene;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.NPCs;

internal sealed class MedicNPC : NPCBase
{
	internal const int FrameStartIndex = 30;

	internal const int KneelingStartIndex = 41;

	internal const int KneelingStandLength = 2;

	internal const int IdleAnimationLength = 5;

	internal const float IdleAnimationSpeed = 0.125f;

	public MedicNPC(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inLevel.GCM.SpForestNPCs, inID)
	{
		_npcType = ENPCType.Medic;
		_bbox = new Rectangle(0, 0, 16, 35);
		_bboxOffset = new Point(4, 0);
		base.TriggerBbox = new Rectangle(0, 0, 80, 32);
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_isAffectedByTime = true;
		base.CannotBeGrabbed = true;
		_isAffectedByGravity = false;
		_isSolid = false;
		_agility = 0.5f;
		_npcTriggerType = ENPCTriggerType.Talk;
		Update(0f);
	}

	public override void Initialize()
	{
		base.Initialize();
		bool flag = true;
		int primaryQuestState = NPCBase.GetPrimaryQuestState(ENPCType.Quartermaster, _level.GameSave);
		if (primaryQuestState == 3)
		{
			int subQuestState = NPCBase.GetSubQuestState(ENPCType.Quartermaster, _level.GameSave);
			EQuestStateType questProgress = NPCBase.GetQuestProgress(ENPCType.Quartermaster, primaryQuestState, subQuestState, _level.GameSave);
			if (questProgress == EQuestStateType.ReadyToTurnIn)
			{
				flag = false;
				base.CannotBeTalkedTo = true;
			}
		}
		if (!flag)
		{
			return;
		}
		if (base.PrimaryProgress == 3 && base.SubProgress == 0)
		{
			IsFacingLeft = false;
			base.IsTalkingOffsetXFlipped = true;
		}
		else if (base.PrimaryProgress == 4)
		{
			if (base.SubProgress == 0)
			{
				base.IsTalkingOffsetXFlipped = true;
			}
			else
			{
				IsFacingLeft = false;
			}
		}
	}

	public override void SetState(EAFSM state)
	{
		if (state == EAFSM.Running || state == EAFSM.Moving)
		{
			ChangeAnimation(35, 6, 0.125f, EAnimationType.Cycle);
		}
		else
		{
			ChangeAnimation(30, 5, 0.125f, EAnimationType.Cycle);
		}
		base.SetState(state);
	}

	protected override void TriggerNPC(Protagonist who)
	{
		base.TriggerNPC(who);
		StartNPCDialogue();
		MovePlayerToTalkingPosition();
		EQuestStateType questProgress = GetQuestProgress();
		switch (base.PrimaryProgress)
		{
		case 0:
			if (base.SubProgress == 0)
			{
				StartQuest1();
				SetSubProgress(1);
				break;
			}
			if (questProgress != EQuestStateType.ReadyToTurnIn)
			{
				DefaultQuest1();
				break;
			}
			EndQuest1();
			RemoveInventoryItems(EInventoryUseItemType.Herb, 5);
			SetPrimaryProgress(1);
			AddGiveItemScript(EInventoryUseItemType.Antidote, 1);
			break;
		case 1:
			if (base.SubProgress == 0)
			{
				if (NPCBase.IsNextQuestAvailable(base.NPCType, base.PrimaryProgress, _level.GameSave))
				{
					StartQuest2();
					SetSubProgress(1);
				}
				else
				{
					DoDefaultNoQuest();
				}
			}
			else if (questProgress != EQuestStateType.ReadyToTurnIn)
			{
				DefaultQuest2();
			}
			else
			{
				EndQuest2();
				RemoveInventoryItems(EInventoryUseItemType.Mushroom, 3);
				SetPrimaryProgress(2);
				AddGiveItemScript(EInventoryUseItemType.Potion, 1);
			}
			break;
		case 2:
			if (base.SubProgress == 0)
			{
				StartQuest3();
				SetSubProgress(1);
				break;
			}
			if (questProgress != EQuestStateType.ReadyToTurnIn)
			{
				DefaultQuest3();
				break;
			}
			EndQuest3();
			RemoveInventoryItems(EInventoryUseItemType.RadiationCrystal, 1);
			SetPrimaryProgress(3);
			AddGiveItemScript(EInventoryUseItemType.Ether, 1);
			break;
		case 3:
			if (base.SubProgress == 0)
			{
				if (NPCBase.IsNextQuestAvailable(base.NPCType, base.PrimaryProgress, _level.GameSave))
				{
					StartQuest4();
					SetSubProgress(1);
				}
				else
				{
					DoDefaultNoQuest();
				}
			}
			else if (questProgress != EQuestStateType.ReadyToTurnIn)
			{
				DefaultQuest4();
			}
			else
			{
				EndQuest4();
				RemoveInventoryItems(EInventoryUseItemType.PlasmaIV, 1);
				SetPrimaryProgress(4);
				AddGiveItemScript(EInventoryUseItemType.HiEther, 1);
				base.DoesNeedZoneBeforeNextQuest = true;
			}
			break;
		case 4:
			if (base.SubProgress == 0)
			{
				if (!base.DoesNeedZoneBeforeNextQuest && NPCBase.IsNextQuestAvailable(base.NPCType, base.PrimaryProgress, _level.GameSave))
				{
					StartQuest5();
					SetSubProgress(1);
				}
				else
				{
					DoDefaultNoQuest();
				}
			}
			else if (questProgress != EQuestStateType.ReadyToTurnIn)
			{
				DefaultQuest5();
			}
			else
			{
				EndQuest5();
				RemoveInventoryItems(EInventoryUseItemType.HistoricalDocuments, 1);
				SetPrimaryProgress(5);
			}
			break;
		case 5:
			DoDefaultNoQuest();
			break;
		}
		EndNPCDialogue();
	}

	private void DoDefaultNoQuest()
	{
		switch (GetDefaultSpeechNumber())
		{
		case 0:
		case 1:
			AddDialogue("q_def_ram_00");
			break;
		case 2:
			AddDialogue("q_def_ram_02");
			break;
		case 3:
			AddDialogue("q_def_ram_03");
			break;
		case 4:
			AddDialogue("q_def_ram_04");
			break;
		}
	}

	private void StartQuest1()
	{
		AddDialogue("q_ram_0_ram_00");
		AddDialogue("q_ram_0_lun_01");
		AddDialogue("q_ram_0_ram_02");
		AddDialogue("q_ram_0_ram_03");
		AddDialogue("q_ram_0_ram_04");
		AddDialogue("q_ram_0_lun_05");
		AddDialogue("q_ram_0_ram_06");
		AddDialogue("q_ram_0_lun_07");
		AddDialogue("q_ram_0_ram_08");
		AddDialogue("q_ram_0_ram_09");
		AddDialogue("q_ram_0_lun_10");
		AddDialogue("q_ram_0_ram_11");
	}

	private void DefaultQuest1()
	{
		AddDialogue("q_ram_0_ram_12");
		AddDialogue("q_ram_0_lun_13");
		AddDialogue("q_ram_0_ram_14");
	}

	private void EndQuest1()
	{
		bool saveBool = _level.GameSave.GetSaveBool(NPCBase.GetIsNPCUnlockedKeyFromType(ENPCType.Quartermaster));
		AddDialogue("q_ram_0_lun_15");
		AddDialogue("q_ram_0_ram_16");
		AddDialogue("q_ram_0_ram_17");
		AddDialogue("q_ram_0_ram_18");
		AddDialogue("q_ram_0_ram_19");
		AddDialogue("q_ram_0_lun_20");
		AddDialogue("q_ram_0_ram_21");
		AddDialogue(saveBool ? "q_ram_0_ram_22" : "q_ram_0_ram_22b");
		AddDialogue("q_ram_0_ram_23");
	}

	private bool IsNearEschem()
	{
		int num = _level.RoomSize.X / 2;
		return Position.X > num;
	}

	private void StartQuest2()
	{
		AddDialogue("q_ram_1_ram_00");
		AddDialogue("q_ram_1_lun_01");
		AddDialogue("q_ram_1_ram_02");
		AddDialogue("q_ram_1_lun_03");
		AddDialogue("q_ram_1_ram_04");
		AddDialogue("q_ram_1_ram_05");
		AddDialogue("q_ram_1_ram_06");
		AddDialogue((!GetIsBossDead(EBossType.Sorceress)) ? "q_ram_1_lun_07" : "q_ram_1_lun_08");
		AddDialogue("q_ram_1_ram_09");
		AddDialogue(IsNearEschem() ? "q_ram_1_ram_10" : "q_ram_1_ram_10_far");
		AddDialogue("q_ram_1_ram_11");
		AddDialogue("q_ram_1_ram_12");
		AddDialogue("q_ram_1_lun_13");
		AddDialogue("q_ram_1_ram_14");
		AddDialogue("q_ram_1_lun_15");
	}

	private void DefaultQuest2()
	{
		AddDialogue(IsNearEschem() ? "q_ram_1_ram_16" : "q_ram_1_ram_16_far");
		AddDialogue("q_ram_1_lun_17");
	}

	private void EndQuest2()
	{
		CutsceneBase.CreateAndCallCutscene(CutsceneBase.ECutsceneType.Forest5_RamedaQ2End, _level, Position);
	}

	private void StartQuest3()
	{
		AddSummonMeyef();
		AddWaitScript(0.75f);
		AddMeyefMew();
		AddDialogue("q_ram_2_lun_00");
		AddDialogue("q_ram_2_ram_01");
		int primaryQuestState = NPCBase.GetPrimaryQuestState(ENPCType.SickSoldier, _level.GameSave);
		AddDialogue((primaryQuestState >= 1) ? "q_ram_2_ram_02a" : "q_ram_2_ram_02b");
		AddDialogue("q_ram_2_ram_03");
		AddDialogue("q_ram_2_lun_04");
		AddDialogue("q_ram_2_ram_05");
		AddDialogue("q_ram_2_lun_06");
		AddDismissMeyef();
	}

	private void DefaultQuest3()
	{
		AddDialogue("q_ram_2_lun_07");
		AddDialogue("q_ram_2_ram_08");
	}

	private void EndQuest3()
	{
		AddDialogue("q_ram_2_lun_09");
		AddDialogue("q_ram_2_ram_10");
		AddDialogue("q_ram_2_ram_11");
		AddDialogue("q_ram_2_ram_12");
		AddDialogue("q_ram_2_ram_13");
		AddDialogue("q_ram_2_ram_14");
		AddWaitScript(1f);
		AddDialogue("q_ram_2_lun_15");
		AddDialogue("q_ram_2_ram_16");
		AddDialogue("q_ram_2_ram_17");
		AddDialogue("q_ram_2_lun_18");
	}

	private void StartQuest4()
	{
		CutsceneBase.CreateAndCallCutscene(CutsceneBase.ECutsceneType.Forest6_RamedaQ4Start, _level, Position);
		base.IsTalkingOffsetXFlipped = true;
	}

	private void DefaultQuest4()
	{
		AddDialogue("q_ram_3_ram_11");
		AddDialogue("q_ram_3_lun_12");
	}

	private void EndQuest4()
	{
		base.DoesNeedZoneBeforeNextQuest = true;
		if (base.IsTalkingOffsetXFlipped)
		{
			IsFacingLeft = true;
			base.IsTalkingOffsetXFlipped = false;
		}
		AddDialogue("q_ram_3_lun_13");
		AddDialogue("q_ram_3_ram_14");
		CutsceneBase.CreateAndCallCutscene(CutsceneBase.ECutsceneType.Forest7_RamedaQ4End, _level, Position);
		GiveAelanaEmpathy();
	}

	private void StartQuest5()
	{
		bool saveBool = _level.GameSave.GetSaveBool(BossClass.GetSaveKeyByBossType(EBossType.Sorceress));
		Protagonist mainHero = _level.MainHero;
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 233;
		animationSpec.Length = 3;
		animationSpec.Speed = 0.15f;
		animationSpec.Type = EAnimationType.Once;
		AnimationSpec newAnim = animationSpec;
		AnimationSpec animationSpec2 = new AnimationSpec();
		animationSpec2.Start = 236;
		animationSpec2.Length = 1;
		animationSpec2.Speed = 0.15f;
		animationSpec2.Type = EAnimationType.Once;
		AnimationSpec newAnim2 = animationSpec2;
		AddDialogue("q_ram_4_ram_00");
		AddDialogue("q_ram_4_har_01");
		AddDialogue("q_ram_4_ram_02");
		AddDialogue("q_ram_4_har_03");
		AddDialogue("q_ram_4_ram_04");
		AddDialogue("q_ram_4_ram_05");
		AddDialogue("q_ram_4_har_06");
		AddDialogue("q_ram_4_ram_07");
		AddDialogue("q_ram_4_ram_08");
		AddDialogue("q_ram_4_har_09");
		AddDialogue("q_ram_4_ram_10");
		AddDialogue("q_ram_4_har_11");
		AddDialogue("q_ram_4_ram_12");
		AddDialogue("q_ram_4_har_13");
		if (saveBool)
		{
			AddDialogue("q_ram_4_ram_14");
		}
		AddDialogue("q_ram_4_ram_15");
		AddDialogue("q_ram_4_har_16");
		AddDialogue("q_ram_4_har_17");
		AddDialogue("q_ram_4_har_18");
		AddDialogue("q_ram_4_ram_19");
		AddScript(new ScriptAction(EScriptActionType.Run, 0f, 0.06f, new Vector4(1f, 0f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Player1,
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(EScriptActionType.Idle, 0f, 0.04f, new Vector4(1f, 0f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Player1,
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim, mainHero)
		{
			DoesBlockQueue = true
		});
		AddWaitScript(0.5f);
		AddDialogue("q_ram_4_lun_20");
		AddScript(new ScriptAction(newAnim2, mainHero)
		{
			DoesBlockQueue = true
		});
		MovePlayerToTalkingPosition();
		AddDialogue("q_ram_4_lun_21");
		AddScript(new ScriptAction(EScriptActionType.LookDirection, 0f, 0f, new Vector4(1f, 0f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = this
		});
		AddDialogue("q_ram_4_ram_22");
		AddDialogue("q_ram_4_har_23");
		AddScript(new ScriptAction(EScriptActionType.LookDirection, 0f, 0f, new Vector4(-1f, 0f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = this,
			DoesBlockQueue = false
		});
		AddDialogue("q_ram_4_ram_24");
		AddDialogue("q_ram_4_har_25");
		AddWaitScript(0.5f);
		AddScript(new ScriptAction(EScriptActionType.LookDirection, 0f, 0f, new Vector4(1f, 0f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = this
		});
		AddWaitScript(0.25f);
		AddDialogue("q_ram_4_ram_26");
		base.IsTalkingOffsetXFlipped = false;
	}

	private void DefaultQuest5()
	{
		AddDialogue("q_ram_4_ram_27");
		AddDialogue("q_ram_4_lun_28");
	}

	private void EndQuest5()
	{
		bool saveBool = _level.GameSave.GetSaveBool(BossClass.GetSaveKeyByBossType(EBossType.Sorceress));
		Protagonist mainHero = _level.MainHero;
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 233;
		animationSpec.Length = 3;
		animationSpec.Speed = 0.15f;
		animationSpec.Type = EAnimationType.Once;
		AnimationSpec newAnim = animationSpec;
		AnimationSpec animationSpec2 = new AnimationSpec();
		animationSpec2.Start = 236;
		animationSpec2.Length = 1;
		animationSpec2.Speed = 0.15f;
		animationSpec2.Type = EAnimationType.Once;
		AnimationSpec newAnim2 = animationSpec2;
		AnimationSpec animationSpec3 = new AnimationSpec();
		animationSpec3.Start = 8;
		animationSpec3.Length = 5;
		animationSpec3.Speed = 0.15f;
		animationSpec3.Type = EAnimationType.Cycle;
		AnimationSpec newAnim3 = animationSpec3;
		AddDialogue("q_ram_4_lun_32");
		AddDialogue("q_ram_4_ram_33");
		AddDialogue("q_ram_4_lun_34");
		AddDialogue("q_ram_4_lun_35");
		AddDialogue("q_ram_4_lun_36");
		AddDialogue("q_ram_4_lun_37");
		AddDialogue("q_ram_4_har_38");
		AddDialogue("q_ram_4_lun_39");
		AddDialogue("q_ram_4_lun_40");
		AddDialogue("q_ram_4_har_41");
		AddScript(new ScriptAction(EScriptActionType.LookDirection, 0f, 0f, new Vector4(-1f, 0f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = this,
			DoesBlockQueue = false
		});
		AddDialogue("q_ram_4_ram_42");
		AddDialogue("q_ram_4_har_43");
		AddDialogue("q_ram_4_har_44");
		AddDialogue("q_ram_4_har_45");
		AddDialogue("q_ram_4_lun_46");
		AddScript(new ScriptAction(newAnim, mainHero)
		{
			DoesBlockQueue = true
		});
		AddDialogue("q_ram_4_har_47");
		AddDialogue("q_ram_4_ram_48");
		AddDialogue(saveBool ? "q_ram_4_ram_49" : "q_ram_4_ram_50");
		AddScript(new ScriptAction(newAnim2, mainHero)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim3, mainHero)
		{
			DoesBlockQueue = false
		});
		AddDialogue("q_ram_4_ram_51");
		AddDialogue("q_ram_4_har_52");
		AddWaitScript(0.5f);
		AddScript(new ScriptAction(EScriptActionType.LookDirection, 0f, 0f, new Vector4(1f, 0f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = this
		});
		AddWaitScript(0.25f);
		AddDialogue("q_ram_4_ram_53");
	}
}
