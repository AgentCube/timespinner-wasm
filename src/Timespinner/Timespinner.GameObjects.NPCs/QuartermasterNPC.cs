using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Cutscene;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.NPCs;

internal sealed class QuartermasterNPC : NPCBase
{
	private const int FrameStartIndex = 50;

	internal const int IdleStart = 50;

	internal const int IdleLength = 5;

	internal const int FallenStartIndex = 61;

	internal const int FallenLength = 4;

	internal const int StandStartIndex = 65;

	internal const int StandLength = 4;

	internal const float IdleSpeed = 0.125f;

	public QuartermasterNPC(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inLevel.GCM.SpForestNPCs, inID)
	{
		_npcType = ENPCType.Quartermaster;
		_bbox = new Rectangle(0, 0, 16, 35);
		_bboxOffset = new Point(3, 6);
		base.TriggerBbox = new Rectangle(0, 0, 80, 32);
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_isAffectedByTime = true;
		base.CannotBeGrabbed = true;
		_isAffectedByGravity = false;
		_isSolid = false;
		_isAffectedByLevelBounds = false;
		_agility = 0.5f;
		_npcTriggerType = ENPCTriggerType.Talk;
		Update(0f);
	}

	public override void SetState(EAFSM state)
	{
		if (state == EAFSM.Running || state == EAFSM.Moving)
		{
			ChangeAnimation(55, 6, 0.125f, EAnimationType.Cycle);
		}
		else
		{
			ChangeAnimation(50, 5, 0.125f, EAnimationType.Cycle);
		}
		base.SetState(state);
	}

	protected override void TriggerNPC(Protagonist who)
	{
		base.TriggerNPC(who);
		StartNPCDialogue();
		MovePlayerToTalkingPosition();
		bool flag = false;
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
				flag = true;
				break;
			}
			EndQuest1();
			RemoveInventoryItems(EInventoryUseItemType.Drumstick, 5);
			SetPrimaryProgress(1);
			AddGiveItemScript(EInventoryUseItemType.FriedCheveux, 1);
			base.DoesNeedZoneBeforeNextQuest = true;
			break;
		case 1:
			if (!base.DoesNeedZoneBeforeNextQuest)
			{
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
						flag = true;
					}
				}
				else if (questProgress != EQuestStateType.ReadyToTurnIn)
				{
					DefaultQuest2();
					flag = true;
				}
				else
				{
					EndQuest2();
					RemoveInventoryItems(EInventoryUseItemType.WyvernTail, 3);
					SetPrimaryProgress(2);
					AddGiveItemScript(EInventoryUseItemType.SauteedTail, 1);
				}
			}
			else
			{
				flag = true;
				DoDefaultNoQuest();
			}
			break;
		case 2:
			if (base.SubProgress == 0)
			{
				if (NPCBase.IsNextQuestAvailable(base.NPCType, base.PrimaryProgress, _level.GameSave) && !base.DoesNeedZoneBeforeNextQuest)
				{
					StartQuest3();
					SetSubProgress(1);
				}
				else
				{
					DoDefaultNoQuest();
					flag = true;
				}
			}
			else if (questProgress != EQuestStateType.ReadyToTurnIn)
			{
				DefaultQuest3();
				flag = true;
			}
			else
			{
				EndQuest3();
				RemoveInventoryItems(EInventoryUseItemType.EelMeat, 1);
				SetPrimaryProgress(3);
				AddGiveItemScript(EInventoryUseItemType.UnagiRoll, 1);
			}
			break;
		case 3:
			if (base.SubProgress == 0)
			{
				if (NPCBase.IsNextQuestAvailable(base.NPCType, base.PrimaryProgress, _level.GameSave))
				{
					StartQuest4();
					SetSubProgress(1);
					base.DoesNeedZoneBeforeNextQuest = true;
				}
				else
				{
					DoDefaultNoQuest();
					flag = true;
				}
			}
			else if (questProgress != EQuestStateType.ReadyToTurnIn)
			{
				DefaultQuest4();
				flag = true;
			}
			else if (base.DoesNeedZoneBeforeNextQuest)
			{
				DoDefaultNoQuest();
				flag = true;
			}
			else
			{
				EndQuest4();
				RemoveInventoryItems(EInventoryUseItemType.CheveuxBreast, 1);
				SetPrimaryProgress(4);
				AddGiveItemScript(EInventoryUseItemType.CheveuxAuVin, 1);
			}
			break;
		case 4:
			if (base.SubProgress == 0)
			{
				if (NPCBase.IsNextQuestAvailable(base.NPCType, base.PrimaryProgress, _level.GameSave))
				{
					StartQuest5();
					SetSubProgress(1);
				}
				else
				{
					DoDefaultNoQuest();
					flag = true;
				}
			}
			else if (questProgress != EQuestStateType.ReadyToTurnIn)
			{
				DefaultQuest5();
				flag = true;
			}
			else
			{
				EndQuest5();
				RemoveInventoryItems(EInventoryUseItemType.FoodSynth, 1);
				SetPrimaryProgress(5);
				AddGiveItemScript(EInventoryUseItemType.Casserole, 1);
			}
			break;
		case 5:
			DoDefaultNoQuest();
			flag = true;
			break;
		}
		if (flag)
		{
			MerchantInventory inventory = new MerchantInventory();
			AddMedicItems(inventory);
			inventory.AddItem(EInventoryUseItemType.Jerky);
			inventory.AddItem(EInventoryEquipmentType.LeatherHelmet);
			inventory.AddItem(EInventoryEquipmentType.LeatherArmor);
			if (base.PrimaryProgress > 0)
			{
				inventory.AddItem(EInventoryUseItemType.FriedCheveux);
			}
			if (base.PrimaryProgress > 1)
			{
				inventory.AddItem(EInventoryUseItemType.SauteedTail);
			}
			if (base.PrimaryProgress > 2)
			{
				inventory.AddItem(EInventoryUseItemType.UnagiRoll);
			}
			if (base.PrimaryProgress > 3)
			{
				inventory.AddItem(EInventoryUseItemType.CheveuxAuVin);
			}
			if (base.PrimaryProgress > 4)
			{
				inventory.AddItem(EInventoryUseItemType.Casserole);
				inventory.AddItem(EInventoryUseItemType.Spaghetti);
			}
			AddCaptainItems(inventory);
			AddScript(new ScriptAction
			{
				ScriptType = EScriptType.Delegate,
				Delegate = delegate
				{
					OpenShop(ENPCType.Quartermaster, inventory);
				}
			});
			AddScript(new ScriptAction
			{
				TargetType = EScriptTargetType.Player1,
				ActionType = EScriptActionType.Idle,
				ActionTimer = 0.15f,
				DoesBlockQueue = true
			});
		}
		EndNPCDialogue();
	}

	private void AddMedicItems(MerchantInventory inventory)
	{
		int primaryQuestState = NPCBase.GetPrimaryQuestState(ENPCType.Medic, _level.GameSave);
		if (primaryQuestState > 0)
		{
			inventory.AddItem(EInventoryUseItemType.Antidote);
		}
		if (primaryQuestState > 1)
		{
			inventory.AddItem(EInventoryUseItemType.Potion);
		}
		if (primaryQuestState > 2)
		{
			inventory.AddItem(EInventoryUseItemType.Ether);
		}
		if (primaryQuestState > 3)
		{
			inventory.AddItem(EInventoryUseItemType.HiPotion);
			inventory.AddItem(EInventoryUseItemType.HiEther);
		}
	}

	private void AddCaptainItems(MerchantInventory inventory)
	{
		int primaryQuestState = NPCBase.GetPrimaryQuestState(ENPCType.Captain, _level.GameSave);
		if (primaryQuestState > 2)
		{
			inventory.AddItem(EInventoryEquipmentType.CopperHelmet);
			inventory.AddItem(EInventoryEquipmentType.CopperArmor);
		}
		if (primaryQuestState > 3)
		{
			inventory.AddItem(EInventoryEquipmentType.CalvaryArmor);
		}
	}

	private void DoDefaultNoQuest()
	{
		switch (GetDefaultSpeechNumber())
		{
		case 0:
		case 1:
			AddDialogue("q_def_sey_00");
			break;
		case 2:
			if (NPCBase.GetPrimaryQuestState(ENPCType.SickSoldier, _level.GameSave) >= 1)
			{
				AddDialogue("q_def_sey_02");
			}
			else
			{
				AddDialogue("q_def_sey_00");
			}
			break;
		case 3:
			AddDialogue("q_def_sey_03");
			break;
		case 4:
			AddDialogue("q_def_sey_04");
			break;
		}
	}

	private void StartQuest1()
	{
		AddDialogue("q_sey_0_sey_00");
		AddDialogue("q_sey_0_lun_01");
		AddDialogue("q_sey_0_sey_02");
		AddDialogue("q_sey_0_lun_03");
		AddDialogue("q_sey_0_sey_02b");
		AddDialogue("q_sey_0_lun_03b");
		AddDialogue("q_sey_0_sey_04");
		AddDialogue("q_sey_0_sey_05");
		if (GetIsBossDead(EBossType.Maw))
		{
			AddDialogue("q_sey_0_lun_06");
			AddDialogue("q_sey_0_sey_07");
		}
		AddDialogue("q_sey_0_sey_08");
		AddDialogue("q_sey_0_sey_09");
		AddDialogue("q_sey_0_lun_10");
	}

	private void DefaultQuest1()
	{
		AddDialogue("q_sey_0_sey_11");
		AddDialogue("q_sey_0_lun_12");
		AddDialogue("q_sey_0_sey_13");
	}

	private void EndQuest1()
	{
		AddDialogue("q_sey_0_lun_14");
		AddDialogue("q_sey_0_sey_15");
	}

	private void StartQuest2()
	{
		AddDialogue("q_sey_1_sey_00");
		AddDialogue("q_sey_1_lun_01");
		if (NPCBase.GetPrimaryQuestState(ENPCType.SickSoldier, _level.GameSave) >= 1)
		{
			AddDialogue("q_sey_1_sey_02");
		}
		else
		{
			AddDialogue("q_sey_1_sey_03");
		}
		AddDialogue("q_sey_1_sey_04");
		AddDialogue("q_sey_1_sey_05");
		AddDialogue("q_sey_1_sey_06");
		AddDialogue("q_sey_1_sey_07");
		AddDialogue("q_sey_1_sey_08");
		AddDialogue("q_sey_1_sey_09");
		AddDialogue("q_sey_1_sey_10");
		AddDialogue("q_sey_1_lun_11");
		AddDialogue("q_sey_1_sey_12");
	}

	private void DefaultQuest2()
	{
		AddDialogue("q_sey_1_sey_13");
	}

	private void EndQuest2()
	{
		AddDialogue("q_sey_1_lun_14");
		AddDialogue("q_sey_1_sey_15");
	}

	private void StartQuest3()
	{
		AddDialogue("q_sey_2_sey_00");
		AddDialogue("q_sey_2_lun_01");
		AddDialogue("q_sey_2_sey_02");
		AddDialogue((NPCBase.GetPrimaryQuestState(ENPCType.Medic, _level.GameSave) >= 3) ? "q_sey_2_sey_03" : "q_sey_2_sey_04");
		AddDialogue("q_sey_2_sey_05");
		AddDialogue("q_sey_2_lun_06");
		AddDialogue("q_sey_2_sey_07");
		AddDialogue("q_sey_2_sey_08");
		AddDialogue("q_sey_2_sey_09");
		AddDialogue("q_sey_2_lun_10");
		AddDialogue("q_sey_2_sey_11");
		AddDialogue("q_sey_2_sey_12");
		AddDialogue("q_sey_2_sey_13");
		AddDialogue("q_sey_2_lun_14");
		AddDialogue("q_sey_2_sey_15");
		AddDialogue("q_sey_2_lun_16");
		AddDialogue("q_sey_2_sey_17");
	}

	private void DefaultQuest3()
	{
		AddDialogue("q_sey_2_sey_18");
		AddDialogue("q_sey_2_lun_19");
		AddDialogue("q_sey_2_sey_20");
	}

	private void EndQuest3()
	{
		AddDialogue("q_sey_2_lun_21");
		AddDialogue("q_sey_2_sey_22");
		AddDialogue("q_sey_2_lun_23");
		AddDialogue("q_sey_2_sey_24");
	}

	private void StartQuest4()
	{
		AddDialogue("q_sey_3_lun_00");
		AddDialogue("q_sey_3_sey_01");
		AddDialogue("q_sey_3_lun_02");
		AddDialogue("q_sey_3_sey_03");
		AddDialogue("q_sey_3_sey_04");
		AddDialogue("q_sey_3_lun_05");
		AddDialogue("q_sey_3_sey_06");
		AddDialogue("q_sey_3_sey_07");
		AddDialogue("q_sey_3_sey_08");
		AddDialogue("q_sey_3_lun_09");
		AddDialogue("q_sey_3_sey_10");
		AddDialogue("q_sey_3_sey_11");
		AddDialogue("q_sey_3_lun_12");
		AddDialogue("q_sey_3_sey_13");
		AddDialogue("q_sey_3_lun_14");
	}

	private void DefaultQuest4()
	{
		AddDialogue("q_sey_3_sey_15");
		AddDialogue("q_sey_3_lun_16");
		AddDialogue("q_sey_3_sey_17");
	}

	private void EndQuest4()
	{
		NPCBase nPCBase = CutsceneBase.GrabNPC(_level, ENPCType.SickSoldier);
		nPCBase.DoesNeedZoneBeforeNextQuest = true;
		SickSoldierNPC sickSoldierNPC = nPCBase as SickSoldierNPC;
		bool flag = true;
		if (sickSoldierNPC != null && !sickSoldierNPC.IsSittingUp)
		{
			flag = false;
			sickSoldierNPC.AddWakeUpAnimation();
		}
		AddDialogue("q_sey_3_lun_18");
		AddDialogue("q_sey_3_sey_19");
		AddDialogue("q_sey_3_lun_20");
		AddDialogue("q_sey_3_sey_21");
		AddDialogue("q_sey_3_sey_22");
		AddDialogue("q_sey_3_ram_23");
		if (!flag)
		{
			sickSoldierNPC.AddCoughAnimation();
		}
		AddDialogue("q_sey_3_esc_24");
		AddDialogue("q_sey_3_sey_25");
		AddDialogue("q_sey_3_esc_26");
		AddDialogue("q_sey_3_sey_27");
		AddDialogue("q_sey_3_sey_28");
		if (!flag)
		{
			sickSoldierNPC.AddSlowFallAsleepAnimation();
		}
		NPCBase nPCBase2 = CutsceneBase.GrabNPC(_level, ENPCType.Medic);
		if (nPCBase2 != null)
		{
			nPCBase2.DoesNeedZoneBeforeNextQuest = true;
		}
		GiveAelanaEmpathy();
	}

	private void StartQuest5()
	{
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
		AddDialogue("q_sey_4_sey_00");
		if (NPCBase.GetPrimaryQuestState(ENPCType.SickSoldier, _level.GameSave) >= 3)
		{
			AddDialogue("q_sey_4_lun_01");
			AddDialogue("q_sey_4_esc_02");
			AddDialogue("q_sey_4_sey_03");
		}
		else
		{
			AddDialogue("q_sey_4_lun_04");
			AddDialogue("q_sey_4_sey_05");
			AddDialogue("q_sey_4_lun_06");
			AddDialogue("q_sey_4_sey_07");
		}
		AddDialogue("q_sey_4_sey_08");
		AddDialogue("q_sey_4_lun_09");
		AddDialogue("q_sey_4_sey_10");
		AddDialogue("q_sey_4_lun_11");
		AddDialogue("q_sey_4_sey_12");
		AddDialogue("q_sey_4_sey_13");
		AddDialogue("q_sey_4_lun_14");
		AddDialogue("q_sey_4_sey_15");
		AddDialogue("q_sey_4_lun_16");
		AddScript(new ScriptAction(EScriptActionType.Run, 0f, 0.5f, new Vector4(1f, 0f, 0f, 0f))
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
		AddDialogue("q_sey_4_lun_17");
		AddScript(new ScriptAction(newAnim2, mainHero)
		{
			DoesBlockQueue = true
		});
	}

	private void DefaultQuest5()
	{
		AddDialogue("q_sey_4_sey_18");
		AddDialogue("q_sey_4_lun_19");
		AddDialogue("q_sey_4_sey_20");
	}

	private void EndQuest5()
	{
		AddDialogue("q_sey_4_lun_21");
		AddDialogue("q_sey_4_sey_22");
		AddDialogue("q_sey_4_lun_23");
		AddDialogue("q_sey_4_sey_24");
		AddDialogue("q_sey_4_lun_25");
		AddDialogue("q_sey_4_sey_26");
		AddDialogue("q_sey_4_lun_27");
		AddDialogue("q_sey_4_sey_28");
		if (NPCBase.GetPrimaryQuestState(ENPCType.SickSoldier, _level.GameSave) >= 3)
		{
			AddDialogue("q_sey_4_sey_29");
		}
	}
}
