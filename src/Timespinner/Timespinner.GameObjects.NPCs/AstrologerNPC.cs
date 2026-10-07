using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Etc;
using Timespinner.GameObjects.Events.Cutscene;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.NPCs;

internal sealed class AstrologerNPC : NPCBase
{
	private const string AlchemyTutorialKey = "alc_tut";

	internal const int AmuletGreenFrameIndex = 223;

	internal const int AmuletPinkFrameIndex = 221;

	internal const int FireOrbFrameIndex = 222;

	internal const int Anim_IdleStart = 0;

	internal const int Anim_IdleLength = 5;

	internal const int Anim_KneelStart = 16;

	internal const int Anim_KneelHealLength = 4;

	internal const int Anim_HealStart = 20;

	internal const int Anim_HealLength = 3;

	internal const int Anim_GiveStart = 23;

	internal const int Anim_GiveLength = 3;

	internal const int Anim_KneelStandStart = 26;

	internal const int Anim_KneelStandLength = 2;

	internal const float Anim_IdleSpeed = 0.125f;

	private const int DefaultBboxOffsetX = 4;

	private const int GivingBboxOffsetX = 6;

	private static readonly Color AmuletGlowColor = new Color(112, 200, 160, 200);

	private bool _hasGivenAlchemyTutorial;

	private CutscenePropAppendage _amulet;

	private CutscenePropAppendage _fireOrb;

	public AstrologerNPC(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inLevel.GCM.SpForestNPCs, inID)
	{
		_npcType = ENPCType.Astrologer;
		_bbox = new Rectangle(0, 0, 16, 35);
		_bboxOffset = new Point(4, 1);
		base.TriggerBbox = new Rectangle(0, 0, 80, 32);
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_isAffectedByTime = true;
		base.CannotBeGrabbed = true;
		_isAffectedByGravity = false;
		_isSolid = false;
		_isAffectedByLevelBounds = false;
		_agility = 0.5f;
		_maxMoveSpeed = 100f;
		_npcTriggerType = ENPCTriggerType.Talk;
		Update(0f);
		_hasGivenAlchemyTutorial = _level.GameSave.GetSaveBool("alc_tut");
	}

	public override void SetState(EAFSM state)
	{
		if (!base.DoesNotMakeSplashesInWater)
		{
			if (state == EAFSM.Running || state == EAFSM.Moving)
			{
				ChangeAnimation(5, 6, 0.125f, EAnimationType.Cycle);
			}
			else
			{
				ChangeAnimation(0, 5, 0.125f, EAnimationType.Cycle);
			}
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
				if (NPCBase.IsNextQuestAvailable(base.NPCType, base.PrimaryProgress, _level.GameSave))
				{
					StartQuest1();
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
				if (!_hasGivenAlchemyTutorial)
				{
					DoAlchemyTutorial();
				}
				else
				{
					DefaultQuest1();
				}
				flag = true;
			}
			else
			{
				RemoveInventoryItems(EInventoryUseItemType.AlchemistTools, 1);
				EndQuest1();
				SetPrimaryProgress(1);
				AddGiveItemScript(EInventoryUseItemType.EssenceCrystal, 1);
			}
			break;
		case 1:
			if (base.SubProgress == 0)
			{
				if (NPCBase.IsNextQuestAvailable(base.NPCType, base.PrimaryProgress, _level.GameSave))
				{
					if (!_hasGivenAlchemyTutorial)
					{
						DoAlchemyTutorial();
						flag = true;
					}
					else
					{
						StartQuest2();
						SetSubProgress(1);
					}
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
				SetPrimaryProgress(2);
				AddGiveItemScript(EInventoryUseItemType.GoldRing, 1);
			}
			break;
		case 2:
			if (base.SubProgress == 0)
			{
				if (NPCBase.IsNextQuestAvailable(base.NPCType, base.PrimaryProgress, _level.GameSave))
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
				SetPrimaryProgress(3);
				AddGiveItemScript(EInventoryUseItemType.GoldNecklace, 1);
			}
			break;
		case 3:
			if (base.SubProgress == 0)
			{
				if (NPCBase.IsNextQuestAvailable(base.NPCType, base.PrimaryProgress, _level.GameSave))
				{
					StartQuest4();
					SetSubProgress(1);
					break;
				}
				if (_level.GameSave.GetSaveBool(BossClass.GetSaveKeyByBossType(EBossType.Maw)))
				{
					AddDialogue("q_nel_3_hint");
				}
				else
				{
					DoDefaultNoQuest();
				}
				flag = true;
			}
			else if (questProgress != EQuestStateType.ReadyToTurnIn)
			{
				DefaultQuest4();
				flag = true;
			}
			else
			{
				RemoveInventoryItems(EInventoryUseItemType.PlasmaCore, 3);
				EndQuest4();
				SetPrimaryProgress(4);
				AddGiveItemScript(EInventoryUseItemType.MagicMarbles, 1);
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
				RemoveInventoryItems(EInventoryUseItemType.GalaxyStone, 1);
				EndQuest5();
				SetPrimaryProgress(5);
			}
			break;
		case 5:
			DoDefaultNoQuest();
			flag = true;
			break;
		}
		if (flag)
		{
			AddScript(new ScriptAction
			{
				ScriptType = EScriptType.Delegate,
				Delegate = delegate
				{
					OpenShop(_npcType, null);
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

	private void DoDefaultNoQuest()
	{
		if (!_hasGivenAlchemyTutorial)
		{
			DoAlchemyTutorial();
			return;
		}
		switch (GetDefaultSpeechNumber())
		{
		case 0:
			AddDialogue("q_def_nel_00");
			break;
		case 1:
			AddDialogue("q_def_nel_01");
			break;
		case 2:
			AddDialogue("q_def_nel_02");
			break;
		case 3:
			AddDialogue("q_def_nel_03");
			break;
		case 4:
			AddDialogue("q_def_nel_04");
			break;
		}
	}

	private void StartQuest1()
	{
		_amulet = new CutscenePropAppendage(this, new Point(4, 4), Point.Zero, _level, _level.GCM.SpItems)
		{
			BaseGlowColor = AmuletGlowColor
		};
		_amulet.ChangeAnimation(223);
		_amulet.Hide();
		base.Appendages.Add(_amulet);
		_fireOrb = new CutscenePropAppendage(this, new Point(8, 8), new Point(4, 4), _level, _level.GCM.SpItems);
		_fireOrb.ChangeAnimation(222);
		_fireOrb.Hide();
		base.Appendages.Add(_fireOrb);
		List<KeyValuePair<int, Point>> list = new List<KeyValuePair<int, Point>>();
		list.Add(new KeyValuePair<int, Point>(20, new Point(-8, -16)));
		list.Add(new KeyValuePair<int, Point>(21, new Point(-1, -19)));
		list.Add(new KeyValuePair<int, Point>(22, new Point(0, -20)));
		List<KeyValuePair<int, Point>> pairs = list;
		_amulet.AddOffsets(pairs);
		List<KeyValuePair<int, Point>> list2 = new List<KeyValuePair<int, Point>>();
		list2.Add(new KeyValuePair<int, Point>(23, new Point(6, -13)));
		list2.Add(new KeyValuePair<int, Point>(24, new Point(9, -17)));
		list2.Add(new KeyValuePair<int, Point>(25, new Point(10, -18)));
		List<KeyValuePair<int, Point>> pairs2 = list2;
		_fireOrb.AddOffsets(pairs2);
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 20;
		animationSpec.Length = 3;
		animationSpec.Speed = 0.1f;
		animationSpec.Type = EAnimationType.Once;
		AnimationSpec newAnim = animationSpec;
		AnimationSpec animationSpec2 = new AnimationSpec();
		animationSpec2.Start = 20;
		animationSpec2.Length = 3;
		animationSpec2.Speed = 0.1f;
		animationSpec2.Type = EAnimationType.Once;
		animationSpec2.IsInReverse = true;
		AnimationSpec newAnim2 = animationSpec2;
		AnimationSpec animationSpec3 = new AnimationSpec();
		animationSpec3.Start = 23;
		animationSpec3.Length = 3;
		animationSpec3.Speed = 0.1f;
		animationSpec3.Type = EAnimationType.Once;
		AnimationSpec newAnim3 = animationSpec3;
		AnimationSpec animationSpec4 = new AnimationSpec();
		animationSpec4.Start = 23;
		animationSpec4.Length = 3;
		animationSpec4.Speed = 0.1f;
		animationSpec4.Type = EAnimationType.Once;
		animationSpec4.IsInReverse = true;
		AnimationSpec newAnim4 = animationSpec4;
		AnimationSpec animationSpec5 = new AnimationSpec();
		animationSpec5.Start = 0;
		animationSpec5.Length = 5;
		animationSpec5.Speed = 0.125f;
		animationSpec5.Type = EAnimationType.Cycle;
		AnimationSpec newAnim5 = animationSpec5;
		AddDialogue("q_nel_0_nel_00");
		AddDialogue("q_nel_0_lun_01");
		AddDialogue("q_nel_0_nel_02");
		AddScript(new ScriptAction(EScriptActionType.Run, 0f, 0.03f, new Vector4(1f, 0f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = this,
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim, this)
		{
			DoesBlockQueue = false
		});
		AddDelegateScript(_amulet.Unhide);
		AddDelegateScript(_amulet.StartGlowing);
		AddDialogue("q_nel_0_lun_03");
		AddDialogue("q_nel_0_nel_04");
		AddDialogue("q_nel_0_lun_05");
		AddScript(new ScriptAction(newAnim2, this)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim5, this));
		AddDelegateScript(_amulet.Hide);
		AddDialogue("q_nel_0_nel_06");
		AddDialogue("q_nel_0_lun_07");
		AddDialogue("q_nel_0_nel_08");
		if (_level.GameSave.Inventory.RelicInventory.Inventory.ContainsKey(6))
		{
			AddDialogue("q_nel_0_lun_09");
		}
		else
		{
			AddDialogue("q_nel_0_lun_10");
		}
		AddDialogue("q_nel_0_lun_11");
		AddDialogue("q_nel_0_nel_12");
		AddDialogue("q_nel_0_nel_13");
		AddDialogue("q_nel_0_lun_14");
		AddDialogue("q_nel_0_nel_15");
		AddDialogue("q_nel_0_har_16");
		AddDialogue("q_nel_0_nel_17");
		AddDialogue("q_nel_0_nel_18");
		AddDialogue("q_nel_0_nel_19");
		AddDialogue("q_nel_0_har_20");
		AddDialogue("q_nel_0_nel_21");
		AddDialogue("q_nel_0_lun_23");
		_level.AddScript(new ScriptAction
		{
			ScriptType = EScriptType.Delegate,
			Delegate = SetBboxOffsetToGiving,
			DoesBlockQueue = false
		});
		AddScript(new ScriptAction(newAnim3, this)
		{
			DoesBlockQueue = false
		});
		AddDelegateScript(_fireOrb.Unhide);
		AddDialogue("q_nel_0_nel_22");
		AddDialogue("q_nel_0_lun_23b");
		_level.GameSave.GiveOrb(EInventoryOrbType.Flame, EOrbSlot.Melee);
		AddScript(new ScriptAction(EInventoryOrbType.Flame, EOrbSlot.Melee));
		AddDelegateScript(_fireOrb.Hide);
		AddScript(new ScriptAction(newAnim4, this)
		{
			DoesBlockQueue = true
		});
		_level.AddScript(new ScriptAction
		{
			ScriptType = EScriptType.Delegate,
			Delegate = SetBboxOffsetToDefault,
			DoesBlockQueue = false
		});
		AddScript(new ScriptAction(newAnim5, this));
	}

	private void DefaultQuest1()
	{
		AddDialogue("q_nel_0_nel_24");
		AddDialogue("q_nel_0_nel_25");
	}

	private void EndQuest1()
	{
		Protagonist mainHero = _level.MainHero;
		if (mainHero != null)
		{
			_fireOrb = new CutscenePropAppendage(mainHero, new Point(8, 8), new Point(4, 4), _level, _level.GCM.SpItems);
			_fireOrb.ChangeAnimation(222);
			_fireOrb.Hide();
			mainHero.Appendages.Add(_fireOrb);
		}
		List<KeyValuePair<int, Point>> list = new List<KeyValuePair<int, Point>>();
		list.Add(new KeyValuePair<int, Point>(221, new Point(-6, -13)));
		list.Add(new KeyValuePair<int, Point>(222, new Point(-9, -17)));
		list.Add(new KeyValuePair<int, Point>(223, new Point(-10, -18)));
		list.Add(new KeyValuePair<int, Point>(224, new Point(-10, -19)));
		List<KeyValuePair<int, Point>> pairs = list;
		_fireOrb.AddOffsets(pairs);
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 221;
		animationSpec.Length = 4;
		animationSpec.Speed = 0.1f;
		animationSpec.Type = EAnimationType.Once;
		AnimationSpec newAnim = animationSpec;
		AnimationSpec animationSpec2 = new AnimationSpec();
		animationSpec2.Start = 221;
		animationSpec2.Length = 4;
		animationSpec2.Speed = 0.1f;
		animationSpec2.Type = EAnimationType.Once;
		animationSpec2.IsInReverse = true;
		AnimationSpec newAnim2 = animationSpec2;
		AnimationSpec animationSpec3 = new AnimationSpec();
		animationSpec3.Start = 8;
		animationSpec3.Length = 5;
		animationSpec3.Speed = 0.15f;
		animationSpec3.Type = EAnimationType.Cycle;
		AnimationSpec newAnim3 = animationSpec3;
		AddDialogue("q_nel_0_lun_26");
		AddDialogue("q_nel_0_nel_27");
		if (_level.GameSave.GetSaveBool(NPCBase.GetIsNPCUnlockedKeyFromType(ENPCType.Medic)))
		{
			AddDialogue("q_nel_0_nel_28");
		}
		AddScript(new ScriptAction(newAnim, mainHero)
		{
			DoesBlockQueue = false
		});
		if (_fireOrb != null)
		{
			AddDelegateScript(_fireOrb.Unhide);
		}
		AddDialogue("q_nel_0_lun_29");
		AddDialogue("q_nel_0_nel_30");
		AddDialogue("q_nel_0_lun_31");
		AddScript(new ScriptAction(newAnim2, mainHero)
		{
			DoesBlockQueue = true
		});
		if (_fireOrb != null)
		{
			AddDelegateScript(_fireOrb.Hide);
		}
		AddScript(new ScriptAction(newAnim3, mainHero));
		AddDialogue("q_nel_0_nel_32");
	}

	private void StartQuest2()
	{
		AddDialogue("q_nel_1_nel_00");
		AddDialogue("q_nel_1_lun_01");
		AddDialogue("q_nel_1_lun_02");
		AddDialogue("q_nel_1_nel_03");
		AddDialogue("q_nel_1_lun_04");
		AddDialogue("q_nel_1_nel_05");
		AddDialogue("q_nel_1_lun_06");
		AddDialogue("q_nel_1_lun_08");
		AddDialogue("q_nel_1_nel_09");
		AddDialogue("q_nel_1_lun_10");
		AddDialogue("q_nel_1_nel_11");
		AddDialogue("q_nel_1_lun_12");
		AddDialogue("q_nel_1_nel_13");
		AddDialogue("q_nel_1_nel_14");
		AddDialogue("q_nel_1_lun_15");
		AddDialogue("q_nel_1_nel_16");
		AddDialogue("q_nel_1_lun_17");
		AddDialogue("q_nel_1_nel_18");
		AddDialogue("q_nel_1_lun_19");
		AddDialogue("q_nel_1_lun_20");
		AddDialogue("q_nel_1_lun_21");
		AddDialogue("q_nel_1_lun_22");
		AddDialogue("q_nel_1_nel_23");
	}

	private void DefaultQuest2()
	{
		AddDialogue("q_nel_1_nel_24");
		AddDialogue("q_nel_1_lun_25");
	}

	private void EndQuest2()
	{
		AddDialogue("q_nel_1_nel_26");
		AddDialogue("q_nel_1_lun_27");
		AddDialogue("q_nel_1_nel_28");
		AddDialogue("q_nel_1_lun_29");
		AddDialogue("q_nel_1_nel_30");
		AddDialogue("q_nel_1_lun_31");
		AddDialogue("q_nel_1_nel_32");
		AddDialogue("q_nel_1_lun_33");
		AddDialogue("q_nel_1_nel_34");
		AddDialogue("q_nel_1_lun_35");
		AddDialogue("q_nel_1_nel_36");
		AddDialogue("q_nel_1_lun_37");
		AddDialogue("q_nel_1_nel_38");
		AddDialogue("q_nel_1_lun_39");
		AddDialogue("q_nel_1_nel_40");
		AddDialogue("q_nel_1_nel_41");
		AddDialogue("q_nel_1_lun_42");
		AddDialogue("q_nel_1_nel_43");
		AddDialogue("q_nel_1_nel_44");
		AddDialogue("q_nel_1_nel_45");
		AddDialogue("q_nel_1_nel_46");
		AddDialogue("q_nel_1_nel_47");
		AddDialogue("q_nel_1_nel_48");
		AddDialogue("q_nel_1_lun_49");
		AddDialogue("q_nel_1_nel_50");
		AddDialogue("q_nel_1_lun_51");
		AddDialogue("q_nel_1_nel_52");
		AddDialogue("q_nel_1_lun_53");
		AddDialogue("q_nel_1_nel_54");
		AddDialogue("q_nel_1_nel_55");
		AddDialogue("q_nel_1_lun_56");
		AddDialogue("q_nel_1_nel_57");
		AddDialogue("q_nel_1_nel_58");
		AddDialogue("q_nel_1_lun_59");
		AddDialogue("q_nel_1_lun_60");
	}

	private void StartQuest3()
	{
		AddDialogue("q_nel_2_nel_00");
		AddDialogue((!GetIsBossDead(EBossType.Maw)) ? "q_nel_2_lun_01" : "q_nel_2_lun_02");
		AddDialogue("q_nel_2_nel_03");
		AddDialogue("q_nel_2_nel_04");
		AddDialogue("q_nel_2_lun_05");
		AddDialogue("q_nel_2_nel_06");
		AddDialogue("q_nel_2_lun_07");
		AddDialogue("q_nel_2_lun_08");
	}

	private void DefaultQuest3()
	{
		AddDialogue("q_nel_2_nel_09");
		AddDialogue("q_nel_2_lun_10");
		AddDialogue("q_nel_2_nel_11");
	}

	private void EndQuest3()
	{
		AddDialogue("q_nel_2_nel_12");
		AddDialogue("q_nel_2_lun_13");
		AddDialogue("q_nel_2_lun_14");
		AddDialogue("q_nel_2_nel_15");
		AddDialogue("q_nel_2_lun_16");
		AddDialogue("q_nel_2_nel_17");
		AddDialogue("q_nel_2_lun_18");
		AddDialogue("q_nel_2_nel_19");
		if (!GetIsBossDead(EBossType.Sorceress))
		{
			AddDialogue("q_nel_2_lun_20");
			AddDialogue("q_nel_2_nel_21");
		}
		else
		{
			AddDialogue("q_nel_2_lun_22");
		}
		AddWaitScript(0.66f);
	}

	private void StartQuest4()
	{
		bool saveBool = _level.GameSave.GetSaveBool(BossClass.GetSaveKeyByBossType(EBossType.Maw));
		bool saveBool2 = _level.GameSave.GetSaveBool(BossClass.GetSaveKeyByBossType(EBossType.Sorceress));
		AddDialogue("q_nel_3_lun_00");
		AddDialogue("q_nel_3_nel_01");
		AddDialogue("q_nel_3_lun_02");
		if (saveBool)
		{
			AddDialogue("q_nel_3_lun_03");
			AddDialogue("q_nel_3_nel_04");
			AddDialogue("q_nel_3_lun_05");
			AddDialogue("q_nel_3_nel_06");
			AddDialogue("q_nel_3_lun_07");
		}
		else
		{
			AddDialogue("q_nel_3_lun_08");
			AddDialogue("q_nel_3_nel_09");
			AddDialogue("q_nel_3_lun_10");
		}
		AddDialogue("q_nel_3_nel_11");
		AddDialogue("q_nel_3_nel_12");
		AddDialogue("q_nel_3_lun_13");
		AddDialogue("q_nel_3_nel_14");
		AddDialogue("q_nel_3_lun_15");
		AddDialogue("q_nel_3_nel_16");
		AddDialogue("q_nel_3_nel_17");
		AddDialogue("q_nel_3_nel_18");
		AddDialogue(saveBool2 ? "q_nel_3_lun_19" : "q_nel_3_lun_20");
		AddDialogue("q_nel_3_nel_21");
		AddDialogue("q_nel_3_nel_22");
		AddDialogue("q_nel_3_nel_23");
		AddDialogue("q_nel_3_lun_24");
	}

	private void DefaultQuest4()
	{
		AddDialogue("q_nel_3_lun_df");
		AddDialogue("q_nel_3_nel_df");
	}

	private void EndQuest4()
	{
		AddDialogue("q_nel_3_lun_25");
		AddDialogue("q_nel_3_nel_26");
		AddScript(new ScriptAction(EScriptActionType.Run, 0f, 0.1f, new Vector4(1f, 0f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Player1,
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(EScriptActionType.Idle, 0f, 0.04f, new Vector4(1f, 0f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Player1,
			DoesBlockQueue = false
		});
		AddDialogue("q_nel_3_nel_27");
		AddScript(new ScriptAction(EScriptActionType.Run, 0f, 0.06f, new Vector4(-1f, 0f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Player1,
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(EScriptActionType.Idle, 0f, 0.04f, new Vector4(1f, 0f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Player1,
			DoesBlockQueue = false
		});
		AddDialogue("q_nel_3_lun_28");
		AddDialogue("q_nel_3_nel_29");
		AddDialogue("q_nel_3_lun_30");
		AddDialogue("q_nel_3_nel_31");
		AddDialogue("q_nel_3_lun_32");
		AddDialogue("q_nel_3_nel_33");
		AddDialogue("q_nel_3_nel_34");
		AddDialogue("q_nel_3_nel_35");
		AddDialogue("q_nel_3_nel_36");
		AddDialogue("q_nel_3_lun_37");
		AddDialogue("q_nel_3_nel_38");
		AddDialogue("q_nel_3_lun_39");
		AddDialogue("q_nel_3_nel_40");
		AddDialogue("q_nel_3_lun_41");
		CutsceneBase.AddLunaisPalmPunch(_level);
		AddDialogue("q_nel_3_lun_42");
		AddDialogue("q_nel_3_lun_43");
		AddDialogue("q_nel_3_nel_44");
		AddDialogue("q_nel_3_lun_45");
		GiveAelanaEmpathy();
		base.DoesNeedZoneBeforeNextQuest = true;
	}

	private void StartQuest5()
	{
		AddDialogue("q_nel_4_lun_00");
		AddDialogue("q_nel_4_nel_01");
		AddDialogue("q_nel_4_lun_02");
		AddDialogue("q_nel_4_nel_03");
		AddDialogue("q_nel_4_lun_04");
		AddDialogue("q_nel_4_lun_05");
		AddDialogue("q_nel_4_lun_06");
		AddDialogue("q_nel_4_lun_07");
		AddDialogue("q_nel_4_nel_08");
		AddDialogue("q_nel_4_nel_09");
		AddDialogue("q_nel_4_nel_10");
		AddDialogue("q_nel_4_lun_11");
		AddDialogue("q_nel_4_nel_12");
		AddDialogue("q_nel_4_nel_13");
		AddDialogue("q_nel_4_lun_14");
		AddDialogue("q_nel_4_nel_15");
		AddDialogue("q_nel_4_lun_16");
		AddDialogue("q_nel_4_nel_17");
	}

	private void DefaultQuest5()
	{
		AddDialogue("q_nel_4_nel_18");
		AddDialogue("q_nel_4_lun_19");
		AddDialogue("q_nel_4_nel_20");
	}

	private void EndQuest5()
	{
		AddDialogue("q_nel_4_lun_21");
		AddDialogue("q_nel_4_nel_22");
		AddDialogue("q_nel_4_nel_23");
		AddDialogue("q_nel_4_lun_24");
		AddDialogue("q_nel_4_nel_25");
		AddDialogue("q_nel_4_nel_26");
		AddDialogue("q_nel_4_nel_27");
		AddDialogue("q_nel_4_lun_28");
		AddDialogue("q_nel_4_lun_29");
		AddDialogue("q_nel_4_nel_30");
		AddLevelScriptAction(new ScriptAction
		{
			ScriptType = EScriptType.FadeInFadeOut,
			Arguments = new Vector4(0.5f, 0f, 0f, 0f)
		});
		AddUnskippableWaitScript(0.5f);
		LevelChangeRequest levelChangeRequest = new LevelChangeRequest();
		levelChangeRequest.LevelID = 3;
		levelChangeRequest.RoomID = 27;
		levelChangeRequest.CutsceneToCall = CutsceneBase.ECutsceneType.Misc3_QuestsEnd1;
		LevelChangeRequest levelRoomChangeRequest = levelChangeRequest;
		AddLevelScriptAction(new ScriptAction(levelRoomChangeRequest));
	}

	private void DoAlchemyTutorial()
	{
		if (base.PrimaryProgress <= 0 && base.SubProgress <= 0)
		{
			AddDialogue("cs_nel_orb_nel_00");
		}
		else
		{
			AddDialogue("cs_nel_orb_nel_00b");
		}
		AddDialogue("cs_nel_orb_nel_01");
		AddDialogue("cs_nel_orb_lun_02");
		AddDialogue("cs_nel_orb_nel_03");
		AddDialogue("cs_nel_orb_nel_04");
		AddDialogue("cs_nel_orb_lun_05");
		AddDialogue("cs_nel_orb_nel_06");
		AddDialogue("cs_nel_orb_nel_07");
		AddDialogue("cs_nel_orb_lun_08");
		_hasGivenAlchemyTutorial = true;
		_level.GameSave.SetValue("alc_tut", value: true);
	}

	internal void SetBboxOffsetToDefault()
	{
		_bboxOffset = new Point(4, 1);
	}

	internal void SetBboxOffsetToGiving()
	{
		_bboxOffset = new Point(6, 1);
	}
}
