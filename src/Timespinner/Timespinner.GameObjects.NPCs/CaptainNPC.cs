using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.NPCs;

internal sealed class CaptainNPC : NPCBase
{
	private const int FrameStartIndex = 110;

	internal const int Anim_SitStart = 120;

	private const int Quest1RewardAmount = 250;

	private const int Quest2RewardAmount = 500;

	private const int Quest3RewardAmount = 750;

	private const int Quest4RewardAmount = 1000;

	private const int Quest5RewardAmount = 2000;

	public CaptainNPC(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inLevel.GCM.SpForestNPCs, inID)
	{
		_npcType = ENPCType.Captain;
		_bbox = new Rectangle(0, 0, 23, 50);
		_bboxOffset = new Point(3, 5);
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
			ChangeAnimation(114, 6, 0.15f, EAnimationType.Cycle);
		}
		else
		{
			AnimationSpecCollection animationSpecCollection = new AnimationSpecCollection();
			animationSpecCollection.DoesRepeat = true;
			AnimationSpecCollection animationSpecCollection2 = animationSpecCollection;
			animationSpecCollection2.Collection.Add(new AnimationSpec
			{
				Start = 110,
				Length = 4,
				Speed = 0.125f,
				Type = EAnimationType.Once
			});
			animationSpecCollection2.Collection.Add(new AnimationSpec
			{
				Start = 113,
				Length = 1,
				Speed = 0.125f,
				Type = EAnimationType.Once
			});
			animationSpecCollection2.Collection.Add(new AnimationSpec
			{
				Start = 110,
				Length = 4,
				Speed = 0.125f,
				Type = EAnimationType.Once,
				IsInReverse = true
			});
			animationSpecCollection2.Collection.Add(new AnimationSpec
			{
				Start = 110,
				Length = 1,
				Speed = 3f,
				Type = EAnimationType.Once
			});
			ChangeAnimation(animationSpecCollection2);
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
				if (NPCBase.IsNextQuestAvailable(base.NPCType, base.PrimaryProgress, _level.GameSave))
				{
					StartQuest1();
					SetSubProgress(1);
					SetInitialKillProgress(EEnemyTileType.ForestPlantBat);
				}
				else
				{
					DoDefaultNoQuest();
				}
			}
			else if (questProgress != EQuestStateType.ReadyToTurnIn)
			{
				DefaultQuest1();
			}
			else
			{
				EndQuest1();
				SetPrimaryProgress(1);
				GiveReward();
			}
			break;
		case 1:
			if (base.SubProgress == 0)
			{
				StartQuest2();
				SetSubProgress(1);
				SetInitialKillProgress(EEnemyTileType.CavesSiren);
			}
			else if (questProgress != EQuestStateType.ReadyToTurnIn)
			{
				DefaultQuest2();
			}
			else
			{
				EndQuest2();
				SetPrimaryProgress(2);
				GiveReward();
			}
			break;
		case 2:
			if (base.SubProgress == 0)
			{
				StartQuest3();
				SetSubProgress(1);
				SetInitialKillProgress(EEnemyTileType.CastleShieldKnight);
				SetInitialKillProgress(EEnemyTileType.CastleArcher);
			}
			else if (questProgress != EQuestStateType.ReadyToTurnIn)
			{
				DefaultQuest3();
			}
			else
			{
				EndQuest3();
				SetPrimaryProgress(3);
				GiveReward();
			}
			break;
		case 3:
			if (base.SubProgress == 0)
			{
				StartQuest4();
				SetSubProgress(1);
				SetInitialKillProgress(EEnemyTileType.TowerRoyalGuard);
			}
			else if (questProgress != EQuestStateType.ReadyToTurnIn)
			{
				DefaultQuest4();
			}
			else
			{
				EndQuest4();
				SetPrimaryProgress(4);
				GiveReward();
			}
			break;
		case 4:
			if (base.SubProgress == 0)
			{
				if (NPCBase.IsNextQuestAvailable(base.NPCType, base.PrimaryProgress, _level.GameSave))
				{
					StartQuest5();
					SetSubProgress(1);
					SetInitialKillProgress(EEnemyTileType.CantoranBoss);
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
				SetPrimaryProgress(5);
				GiveReward();
			}
			break;
		case 5:
			DoDefaultNoQuest();
			break;
		}
		EndNPCDialogue();
	}

	private void GiveReward()
	{
		AddScript(new ScriptAction
		{
			Delegate = GiveRewardDelegate,
			ScriptType = EScriptType.Delegate
		});
	}

	private void GiveRewardDelegate()
	{
		int num = 0;
		switch (base.PrimaryProgress)
		{
		case 1:
			num = 250;
			break;
		case 2:
			num = 500;
			break;
		case 3:
			num = 750;
			break;
		case 4:
			num = 1000;
			break;
		case 5:
			num = 2000;
			break;
		}
		if (num > 0)
		{
			_level.MainHero.GetPowerup(EItemType.Money, num);
		}
	}

	private void DoDefaultNoQuest()
	{
		switch (GetDefaultSpeechNumber())
		{
		case 0:
			AddDialogue("q_def_har_00");
			break;
		case 1:
			AddDialogue("q_def_har_01");
			break;
		case 2:
			AddDialogue("q_def_har_02");
			break;
		case 3:
			AddDialogue("q_def_har_03");
			break;
		case 4:
			AddDialogue("q_def_har_04");
			break;
		}
	}

	private void StartQuest1()
	{
		AddDialogue("q_har_0_har_00");
		AddDialogue("q_har_0_lun_01");
		AddDialogue((!GetIsBossDead(EBossType.Maw)) ? "q_har_0_har_02" : "q_har_0_har_03");
		if (!GetIsBossDead(EBossType.Sorceress))
		{
			AddDialogue("q_har_0_har_04");
		}
		else
		{
			AddDialogue("q_har_0_har_05");
			AddDialogue("q_har_0_lun_06");
		}
		AddDialogue("q_har_0_har_07");
		AddDialogue("q_har_0_har_08");
		AddDialogue("q_har_0_lun_09");
		AddDialogue("q_har_0_har_10");
		AddDialogue("q_har_0_har_11");
		AddDialogue("q_har_0_lun_12");
		AddDialogue("q_har_0_har_13");
	}

	private void DefaultQuest1()
	{
		AddDialogue("q_har_0_har_14");
		AddDialogue("q_har_0_lun_15");
		AddDialogue("q_har_0_har_16");
	}

	private void EndQuest1()
	{
		AddDialogue("q_har_0_har_17");
		AddDialogue("q_har_0_lun_18");
		AddDialogue("q_har_0_har_19");
		AddDialogue("q_har_0_lun_20");
		AddDialogue("q_har_0_lun_21");
		AddDialogue("q_har_0_har_22");
		AddDialogue("q_har_0_lun_23");
		AddDialogue("q_har_0_har_24");
		AddDialogue("q_har_0_lun_25");
		AddDialogue("q_har_0_har_26");
		AddDialogue("q_har_0_har_27");
		if (NPCBase.GetPrimaryQuestState(ENPCType.Astrologer, _level.GameSave) >= 2)
		{
			AddDialogue("q_har_0_lun_28");
			AddDialogue("q_har_0_har_29");
		}
		else
		{
			AddDialogue("q_har_0_har_30");
		}
		bool isBossDead = GetIsBossDead(EBossType.Sorceress);
		AddDialogue((!isBossDead) ? "q_har_0_lun_31" : "q_har_0_lun_32");
		AddDialogue("q_har_0_har_33");
		if (isBossDead)
		{
			AddDialogue("q_har_0_lun_34");
			AddDialogue("q_har_0_har_35");
		}
	}

	private void StartQuest2()
	{
		AddDialogue("q_har_1_har_00");
		AddDialogue("q_har_1_lun_01");
		bool isBossDead = GetIsBossDead(EBossType.Demon);
		bool isBossDead2 = GetIsBossDead(EBossType.Sorceress);
		if (isBossDead)
		{
			AddDialogue("q_har_1_har_02");
			if (isBossDead2)
			{
				AddDialogue("q_har_1_har_03");
				AddDialogue("q_har_1_lun_04");
			}
			AddDialogue("q_har_1_har_05");
		}
		else
		{
			AddDialogue("q_har_1_har_06");
		}
		AddDialogue("q_har_1_har_07");
		AddDialogue("q_har_1_har_08");
		AddDialogue("q_har_1_lun_09");
		AddDialogue("q_har_1_har_10");
		AddDialogue("q_har_1_har_11");
		AddDialogue("q_har_1_har_12");
		AddDialogue("q_har_1_lun_13");
		AddDialogue("q_har_1_har_14");
	}

	private void DefaultQuest2()
	{
		AddDialogue("q_har_1_har_15");
		AddDialogue("q_har_1_lun_16");
		AddDialogue("q_har_1_har_17");
	}

	private void EndQuest2()
	{
		AddDialogue("q_har_1_har_18");
		AddDialogue("q_har_1_lun_19");
		AddDialogue("q_har_1_har_20");
		AddDialogue("q_har_1_lun_21");
		AddDialogue("q_har_1_har_22");
	}

	private void StartQuest3()
	{
		bool isBossDead = GetIsBossDead(EBossType.Maw);
		bool isBossDead2 = GetIsBossDead(EBossType.Sorceress);
		int primaryQuestState = NPCBase.GetPrimaryQuestState(ENPCType.Medic, _level.GameSave);
		AddDialogue("q_har_2_har_00");
		if (isBossDead)
		{
			AddDialogue("q_har_2_lun_01");
			AddDialogue("q_har_2_har_02");
		}
		if (primaryQuestState >= 3 && !isBossDead2)
		{
			AddDialogue("q_har_2_lun_03");
		}
		if (isBossDead2)
		{
			AddDialogue("q_har_2_lun_04");
		}
		if (isBossDead2 && isBossDead)
		{
			AddDialogue("q_har_2_lun_05");
		}
		if (primaryQuestState >= 3 || isBossDead2)
		{
			AddDialogue("q_har_2_har_06");
		}
		AddDialogue("q_har_2_har_07");
		AddDialogue("q_har_2_har_08");
		if (isBossDead || isBossDead2)
		{
			AddDialogue("q_har_2_lun_09");
		}
		AddDialogue("q_har_2_lun_10");
	}

	private void DefaultQuest3()
	{
		AddDialogue("q_har_2_har_11");
	}

	private void EndQuest3()
	{
		AddDialogue("q_har_2_har_12");
		AddDialogue("q_har_2_lun_13");
		if (!GetIsBossDead(EBossType.Maw))
		{
			AddDialogue("q_har_2_har_14");
			AddDialogue("q_har_2_lun_15");
		}
		else
		{
			AddDialogue("q_har_2_har_16");
			AddDialogue("q_har_2_lun_17");
		}
	}

	private void StartQuest4()
	{
		int primaryQuestState = NPCBase.GetPrimaryQuestState(ENPCType.Medic, _level.GameSave);
		int primaryQuestState2 = NPCBase.GetPrimaryQuestState(ENPCType.Astrologer, _level.GameSave);
		int primaryQuestState3 = NPCBase.GetPrimaryQuestState(ENPCType.Quartermaster, _level.GameSave);
		bool isBossDead = GetIsBossDead(EBossType.Sorceress);
		AddDialogue("q_har_3_har_00");
		if (isBossDead || primaryQuestState >= 4 || primaryQuestState3 >= 4 || primaryQuestState2 >= 4)
		{
			AddDialogue("q_har_3_lun_01");
			AddDialogue("q_har_3_har_02");
		}
		else
		{
			AddDialogue("q_har_3_lun_03");
		}
		AddDialogue("q_har_3_har_04");
		AddDialogue("q_har_3_lun_05");
		AddDialogue("q_har_3_har_06");
		if (primaryQuestState2 >= 2)
		{
			AddDialogue("q_har_3_har_07");
		}
		AddDialogue("q_har_3_lun_08");
		if (primaryQuestState2 >= 2 || primaryQuestState >= 3 || primaryQuestState3 >= 3)
		{
			AddDialogue("q_har_3_lun_09");
		}
		AddDialogue("q_har_3_har_10");
		AddDialogue("q_har_3_lun_11");
		AddDialogue("q_har_3_har_12");
		AddDialogue("q_har_3_lun_13");
		AddDialogue("q_har_3_har_14");
		AddDialogue("q_har_3_lun_15");
		AddDialogue("q_har_3_har_16");
		AddDialogue("q_har_3_lun_17");
	}

	private void DefaultQuest4()
	{
		AddDialogue("q_har_3_har_18");
		AddDialogue(GetIsBossDead(EBossType.Demon) ? "q_har_3_lun_19" : "q_har_3_lun_20");
		AddDialogue("q_har_3_har_21");
	}

	private void EndQuest4()
	{
		AddDialogue("q_har_3_har_22");
		AddDialogue("q_har_3_lun_23");
		AddDialogue("q_har_3_har_24");
		AddDialogue("q_har_3_har_25");
		AddDialogue("q_har_3_lun_26");
		AddDialogue("q_har_3_lun_27");
		AddDialogue("q_har_3_har_28");
		GiveAelanaEmpathy();
	}

	private void StartQuest5()
	{
		AddDialogue("q_har_4_har_00");
		AddDialogue("q_har_4_lun_01");
		AddDialogue("q_har_4_har_02");
		AddDialogue("q_har_4_lun_03");
		AddDialogue("q_har_4_har_04");
		AddDialogue("q_har_4_lun_05");
		AddDialogue("q_har_4_har_06");
		AddDialogue("q_har_4_lun_07");
		AddDialogue("q_har_4_har_08");
		AddDialogue("q_har_4_har_09");
		AddDialogue("q_har_4_har_10");
		AddDialogue("q_har_4_lun_11");
		AddDialogue("q_har_4_har_12");
		AddDialogue("q_har_4_lun_13");
		_level.GameSave.SetValue("IsCantoranActive", value: true);
	}

	private void DefaultQuest5()
	{
		AddDialogue("q_har_4_har_14");
		AddDialogue("q_har_4_lun_15");
		AddDialogue("q_har_4_har_16");
		AddDialogue("q_har_4_lun_17");
	}

	private void EndQuest5()
	{
		AddDialogue("q_har_4_lun_18");
		AddDialogue("q_har_4_har_19");
		AddDialogue("q_har_4_lun_20");
		AddDialogue("q_har_4_har_21");
		AddDialogue("q_har_4_lun_22");
		AddDialogue("q_har_4_har_23");
		AddDialogue("q_har_4_har_24");
	}
}
