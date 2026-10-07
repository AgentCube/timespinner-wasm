using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Cutscene;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.NPCs;

internal sealed class SickSoldierNPC : NPCBase
{
	private const int SickBlinkLength = 3;

	private const int SickCoughLength = 3;

	private const int SickNodLength = 3;

	private const int SickMotionLength = 3;

	private const int SickWriteLength = 2;

	private const int SickShowLength = 2;

	private const int SickThumbLength = 4;

	private const int FrameStartIndex = 70;

	private const int SickStartIndex = 90;

	private const int SickAwakeIndex = 92;

	private const int SickCoughIndex = 93;

	private const int SickNodIndex = 96;

	private const int SickMotionIndex = 99;

	private const int SickShowIndex = 102;

	private const int SickWriteIndex = 104;

	private const int SickThumbIndex = 106;

	private const int SittingUpStartIndex = 87;

	private const int VileteStartIndex = 88;

	internal const int HealthySitIndex = 87;

	private readonly bool _canBeSpokenTo;

	private bool _isInBed;

	private bool _isSittingUp;

	internal bool IsOnVilete { get; set; }

	internal bool IsSittingUp => _isSittingUp;

	public SickSoldierNPC(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inLevel.GCM.SpForestNPCs, inID)
	{
		_npcType = ENPCType.SickSoldier;
		base.TriggerBbox = new Rectangle(0, 0, 80, 32);
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_isAffectedByTime = true;
		base.CannotBeGrabbed = true;
		_isAffectedByGravity = false;
		_isSolid = false;
		_agility = 0.5f;
		_canBeSpokenTo = NPCBase.GetPrimaryQuestState(ENPCType.Medic, _level.GameSave) >= 2;
		_npcTriggerType = (_canBeSpokenTo ? ENPCTriggerType.Talk : ENPCTriggerType.None);
		base.CannotBeTalkedTo = !_canBeSpokenTo;
		Update(0f);
	}

	public override void Initialize()
	{
		base.Initialize();
		_isInBed = base.PrimaryProgress < 3;
		if (_isInBed)
		{
			int primaryQuestState = NPCBase.GetPrimaryQuestState(ENPCType.Medic, _level.GameSave);
			_isSittingUp = base.PrimaryProgress == 2 && primaryQuestState >= 4;
			if (!_isSittingUp)
			{
				_bbox = new Rectangle(0, 0, 42, 16);
				_bboxOffset = new Point(0, (!base.IsSpawnedForCutscene) ? 12 : 0);
				ChangeAnimation(90);
			}
			else
			{
				_bbox = new Rectangle(0, 0, 32, 32);
				_bboxOffset = new Point(0, 11);
				ChangeAnimation(87);
			}
		}
		else
		{
			_bbox = new Rectangle(0, 0, 16, 35);
			_bboxOffset = new Point(9, 6);
		}
		if (IsOnVilete)
		{
			ChangeAnimation(88);
			_bbox = new Rectangle(0, 0, 28, 37);
			_bboxOffset = Point.Zero;
		}
	}

	public override void SetState(EAFSM state)
	{
		if (!_isInBed)
		{
			if (state == EAFSM.Running || state == EAFSM.Moving)
			{
				ChangeAnimation(75, 6, 0.125f, EAnimationType.Cycle);
			}
			else
			{
				ChangeAnimation(70, 5, 0.125f, EAnimationType.Cycle);
			}
		}
		else
		{
			ChangeAnimation(_isSittingUp ? 87 : 90);
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
			AddSlowWakeUpAnimation();
			if (NPCBase.IsNextQuestAvailable(base.NPCType, base.PrimaryProgress, _level.GameSave))
			{
				if (base.SubProgress == 0)
				{
					StartQuest1();
					SetSubProgress(1);
					base.CannotBeTalkedTo = true;
				}
				else if (questProgress != EQuestStateType.ReadyToTurnIn)
				{
					DefaultQuest1();
				}
				else
				{
					EndQuest1();
					RemoveInventoryItems(EInventoryUseItemType.SirenInk, 1);
					RemoveInventoryItems(EInventoryUseItemType.CheveuxFeather, 1);
					SetPrimaryProgress(1);
					base.CannotBeTalkedTo = true;
				}
			}
			AddFallAsleepAnimation();
			break;
		case 1:
			AddWakeUpAnimation();
			if (base.SubProgress == 0)
			{
				if (!base.DoesNeedZoneBeforeNextQuest && NPCBase.IsNextQuestAvailable(base.NPCType, base.PrimaryProgress, _level.GameSave))
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
				RemoveInventoryItems(EInventoryUseItemType.SilverOre, 1);
				SetPrimaryProgress(2);
				base.CannotBeTalkedTo = true;
				base.DoesNeedZoneBeforeNextQuest = true;
			}
			AddFallAsleepAnimation();
			break;
		case 2:
			if (base.SubProgress == 0)
			{
				if (!base.DoesNeedZoneBeforeNextQuest && NPCBase.IsNextQuestAvailable(base.NPCType, base.PrimaryProgress, _level.GameSave))
				{
					StartQuest3();
					SetSubProgress(1);
					break;
				}
				if (!_isSittingUp)
				{
					AddWakeUpAnimation();
				}
				DoDefaultNoQuest();
				if (!_isSittingUp)
				{
					AddFallAsleepAnimation();
				}
			}
			else if (questProgress != EQuestStateType.ReadyToTurnIn)
			{
				DefaultQuest3();
			}
			else
			{
				EndQuest3();
			}
			break;
		default:
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
			AddDialogue("q_def_esc_00");
			break;
		case 2:
			AddDialogue("q_def_esc_02");
			break;
		case 3:
			AddDialogue("q_def_esc_03");
			break;
		case 4:
			AddDialogue("q_def_esc_04");
			break;
		}
	}

	private void StartQuest1()
	{
		AddDialogue("q_esc_0_lun_00");
		AddThumbsUpAnimation();
		int primaryQuestState = NPCBase.GetPrimaryQuestState(ENPCType.Quartermaster, _level.GameSave);
		bool flag = primaryQuestState != 1;
		NPCBase nPCBase = CutsceneBase.GrabNPC(_level, ENPCType.Quartermaster);
		if (nPCBase != null && nPCBase.Position.X > _level.RoomSize.X / 2)
		{
			flag = false;
		}
		if (flag)
		{
			AddDialogue("q_esc_0_ram_01");
		}
		else
		{
			AddDialogue("q_esc_0_sey_02");
		}
		AddMotionAnimation();
		AddDialogue("q_esc_0_lun_03");
		EndMotionAnimation();
		AddDialogue("q_esc_0_esc_04");
		AddDialogue("q_esc_0_lun_05");
		AddCoughAnimation();
		AddDialogue("q_esc_0_esc_06");
		AddDialogue("q_esc_0_lun_07");
	}

	private void DefaultQuest1()
	{
		AddDialogue("q_esc_0_lun_08");
	}

	private void EndQuest1()
	{
		base.DoesNeedZoneBeforeNextQuest = true;
		NPCBase scriptTarget = CutsceneBase.GrabNPC(_level, ENPCType.Medic);
		AddScript(new ScriptAction(EScriptActionType.LookDirection, 0f, 0f, new Vector4(1f, 0f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = scriptTarget
		});
		NPCBase nPCBase = CutsceneBase.GrabNPC(_level, ENPCType.Quartermaster);
		if (nPCBase != null)
		{
			nPCBase.DoesNeedZoneBeforeNextQuest = true;
		}
		AddDialogue("q_esc_0_lun_09");
		AddNodAnimation();
		AddDialogue("q_esc_0_lun_10");
		AddWriteAnimation();
		AddDialogue("q_esc_0_lun_11");
		AddWriteAnimation();
		AddDialogue("q_esc_0_lun_12");
		AddDialogue("q_esc_0_ram_13");
		AddWriteAnimation();
		AddDialogue("q_esc_0_lun_14");
		AddDialogue("q_esc_0_sey_15");
		AddWriteAnimation();
		AddDialogue("q_esc_0_lun_16");
		AddDialogue("q_esc_0_ram_17");
		AddWriteAnimation();
		AddDialogue("q_esc_0_lun_18");
		AddCoughAnimation();
	}

	private void StartQuest2()
	{
		AddDialogue("q_esc_1_lun_00");
		AddCoughAnimation();
		AddDialogue("q_esc_1_esc_01");
		AddCoughAnimation();
		AddCoughAnimation();
		AddCoughAnimation();
		AddDialogue("q_esc_1_esc_02");
		AddDialogue("q_esc_1_lun_03");
		AddCoughAnimation();
		AddDialogue("q_esc_1_esc_04");
		AddDialogue("q_esc_1_lun_05");
		AddDialogue("q_esc_1_esc_06");
		AddDialogue("q_esc_1_esc_07");
		AddDialogue("q_esc_1_lun_08");
		AddCoughAnimation();
		AddDialogue("q_esc_1_esc_09");
		AddDialogue("q_esc_1_lun_10");
		AddDialogue("q_esc_1_esc_11");
		AddDialogue("q_esc_1_lun_12");
		AddCoughAnimation();
		AddDialogue("q_esc_1_esc_13");
		AddDialogue("q_esc_1_esc_14");
		AddCoughAnimation();
		AddDialogue("q_esc_1_esc_14b");
		AddDialogue("q_esc_1_lun_15");
		AddDialogue("q_esc_1_esc_16");
		AddCoughAnimation();
		AddCoughAnimation();
		AddCoughAnimation();
		AddDialogue("q_esc_1_esc_17");
		AddFallAsleepAnimation();
		AddDialogue("q_esc_1_lun_18");
		AddCoughAnimation();
		AddDialogue("q_esc_1_esc_19");
		AddDialogue("q_esc_1_esc_20");
		AddDialogue("q_esc_1_esc_21");
		AddDialogue("q_esc_1_lun_22");
	}

	private void DefaultQuest2()
	{
		AddDialogue("q_esc_1_lun_23");
		AddCoughAnimation();
		AddDialogue("q_esc_1_esc_24");
		AddDialogue("q_esc_1_lun_25");
		AddThumbsUpAnimation();
		AddDialogue("q_esc_1_esc_26");
	}

	private void EndQuest2()
	{
		base.DoesNeedZoneBeforeNextQuest = true;
		AddDialogue("q_esc_1_lun_27");
		AddFallAsleepAnimation();
		AddDialogue("q_esc_1_esc_28");
		AddDialogue("q_esc_1_lun_29");
		AddWakeUpAnimation();
		AddCoughAnimation();
		AddDialogue("q_esc_1_esc_30");
		AddDialogue("q_esc_1_esc_31");
		AddDialogue("q_esc_1_esc_32");
	}

	private void StartQuest3()
	{
		AddDialogue("q_esc_2_lun_00");
		AddDialogue("q_esc_2_esc_01");
		AddDialogue("q_esc_2_lun_02");
		AddDialogue("q_esc_2_esc_03");
		AddDialogue("q_esc_2_lun_04");
		AddDialogue("q_esc_2_esc_05");
		AddDialogue("q_esc_2_lun_06");
		AddDialogue("q_esc_2_esc_07");
		AddDialogue("q_esc_2_lun_08");
		AddDialogue("q_esc_2_esc_09");
		AddDialogue("q_esc_2_lun_10");
		AddDialogue("q_esc_2_esc_11");
	}

	private void DefaultQuest3()
	{
		AddDialogue("q_esc_2_esc_12");
	}

	private void EndQuest3()
	{
		_level.SetLevelSaveBool("IsEscortMissionActive", value: true);
		AddDialogue("q_esc_2_esc_12");
		LevelChangeRequest levelChangeRequest = new LevelChangeRequest();
		levelChangeRequest.LevelID = 3;
		levelChangeRequest.RoomID = 16;
		levelChangeRequest.CutsceneToCall = CutsceneBase.ECutsceneType.Forest8_EscortStart;
		levelChangeRequest.AdditionalBlackScreenTime = 0.5f;
		levelChangeRequest.FadeOutTime = 0.25f;
		levelChangeRequest.FadeInTime = 0.25f;
		levelChangeRequest.ShouldPlayLevelSong = true;
		LevelChangeRequest levelRoomChangeRequest = levelChangeRequest;
		AddLevelScriptAction(new ScriptAction(levelRoomChangeRequest));
	}

	internal void AddWakeUpAnimation()
	{
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 90;
		animationSpec.Length = 3;
		animationSpec.Speed = 0.1f;
		animationSpec.Type = EAnimationType.Once;
		ScriptAction scriptAction = new ScriptAction(animationSpec, this);
		scriptAction.DoesBlockQueue = true;
		ScriptAction newScript = scriptAction;
		AddScript(newScript);
	}

	internal void AddSlowWakeUpAnimation()
	{
		AddWakeUpAnimation();
		AddFallAsleepAnimation();
		AddWakeUpAnimation();
	}

	internal void AddSlowFallAsleepAnimation()
	{
		AddFallAsleepAnimation();
		AddWakeUpAnimation();
		AddFallAsleepAnimation();
	}

	internal void AddFallAsleepAnimation()
	{
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 90;
		animationSpec.Length = 3;
		animationSpec.Speed = 0.1f;
		animationSpec.Type = EAnimationType.Once;
		animationSpec.IsInReverse = true;
		ScriptAction scriptAction = new ScriptAction(animationSpec, this);
		scriptAction.DoesBlockQueue = true;
		ScriptAction newScript = scriptAction;
		AddScript(newScript);
	}

	private void AddEndAnimation()
	{
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 92;
		animationSpec.Length = 1;
		animationSpec.Speed = 0.1f;
		animationSpec.Type = EAnimationType.Once;
		AnimationSpec newAnim = animationSpec;
		ScriptAction scriptAction = new ScriptAction(newAnim, this);
		scriptAction.DoesBlockQueue = true;
		ScriptAction newScript = scriptAction;
		AddScript(newScript);
	}

	internal void AddCoughAnimation()
	{
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 93;
		animationSpec.Length = 3;
		animationSpec.Speed = 0.1f;
		animationSpec.Type = EAnimationType.Once;
		AnimationSpec newAnim = animationSpec;
		ScriptAction scriptAction = new ScriptAction(newAnim, this);
		scriptAction.DoesBlockQueue = true;
		ScriptAction newScript = scriptAction;
		AddScript(newScript);
		AddEndAnimation();
	}

	internal void AddThumbsUpAnimation()
	{
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 106;
		animationSpec.Length = 4;
		animationSpec.Speed = 0.15f;
		animationSpec.Type = EAnimationType.Once;
		AnimationSpec newAnim = animationSpec;
		AnimationSpec animationSpec2 = new AnimationSpec();
		animationSpec2.Start = 106;
		animationSpec2.Length = 4;
		animationSpec2.Speed = 0.15f;
		animationSpec2.Type = EAnimationType.Once;
		animationSpec2.IsInReverse = true;
		AnimationSpec newAnim2 = animationSpec2;
		ScriptAction scriptAction = new ScriptAction(newAnim, this);
		scriptAction.DoesBlockQueue = true;
		ScriptAction newScript = scriptAction;
		ScriptAction scriptAction2 = new ScriptAction(newAnim2, this);
		scriptAction2.DoesBlockQueue = true;
		ScriptAction newScript2 = scriptAction2;
		AddScript(newScript);
		AddWaitScript(1f);
		AddScript(newScript2);
		AddEndAnimation();
	}

	private void AddMotionAnimation()
	{
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 99;
		animationSpec.Length = 3;
		animationSpec.Speed = 0.15f;
		animationSpec.Type = EAnimationType.Once;
		AnimationSpec newAnim = animationSpec;
		AnimationSpec animationSpec2 = new AnimationSpec();
		animationSpec2.Start = 100;
		animationSpec2.Length = 2;
		animationSpec2.Speed = 0.15f;
		animationSpec2.Type = EAnimationType.Cycle;
		AnimationSpec newAnim2 = animationSpec2;
		ScriptAction scriptAction = new ScriptAction(newAnim, this);
		scriptAction.DoesBlockQueue = true;
		ScriptAction newScript = scriptAction;
		ScriptAction scriptAction2 = new ScriptAction(newAnim2, this);
		scriptAction2.DoesBlockQueue = true;
		ScriptAction newScript2 = scriptAction2;
		AddScript(newScript);
		AddScript(newScript2);
	}

	private void EndMotionAnimation()
	{
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 99;
		animationSpec.Length = 3;
		animationSpec.Speed = 0.1f;
		animationSpec.Type = EAnimationType.Once;
		animationSpec.IsInReverse = true;
		AnimationSpec newAnim = animationSpec;
		ScriptAction scriptAction = new ScriptAction(newAnim, this);
		scriptAction.DoesBlockQueue = true;
		ScriptAction newScript = scriptAction;
		AddScript(newScript);
		AddEndAnimation();
	}

	private void AddNodAnimation()
	{
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 96;
		animationSpec.Length = 3;
		animationSpec.Speed = 0.1f;
		animationSpec.Type = EAnimationType.Once;
		AnimationSpec newAnim = animationSpec;
		ScriptAction scriptAction = new ScriptAction(newAnim, this);
		scriptAction.DoesBlockQueue = true;
		ScriptAction newScript = scriptAction;
		AddScript(newScript);
		AddEndAnimation();
	}

	private void AddWriteAnimation()
	{
		AnimationSpecCollection animationSpecCollection = new AnimationSpecCollection();
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 102;
		animationSpec.Length = 2;
		animationSpec.Speed = 0.15f;
		animationSpec.Type = EAnimationType.Once;
		AnimationSpec item = animationSpec;
		AnimationSpec animationSpec2 = new AnimationSpec();
		animationSpec2.Start = 104;
		animationSpec2.Length = 2;
		animationSpec2.Speed = 0.15f;
		animationSpec2.Type = EAnimationType.Cycle;
		AnimationSpec item2 = animationSpec2;
		animationSpecCollection.Collection.Add(item);
		animationSpecCollection.Collection.Add(item2);
		AnimationSpec animationSpec3 = new AnimationSpec();
		animationSpec3.Start = 102;
		animationSpec3.Length = 2;
		animationSpec3.Speed = 0.15f;
		animationSpec3.Type = EAnimationType.Once;
		animationSpec3.IsInReverse = true;
		AnimationSpec newAnim = animationSpec3;
		ScriptAction scriptAction = new ScriptAction(animationSpecCollection, this);
		scriptAction.DoesBlockQueue = false;
		ScriptAction newScript = scriptAction;
		ScriptAction scriptAction2 = new ScriptAction(newAnim, this);
		scriptAction2.DoesBlockQueue = true;
		ScriptAction newScript2 = scriptAction2;
		AddScript(newScript);
		AddWaitScript(2f);
		AddScript(newScript2);
	}

	internal void StartEscortQuest(bool shouldSetState)
	{
		_isAffectedByGravity = true;
		_isFlying = false;
		_maxMoveSpeed = 100f;
		if (shouldSetState)
		{
			ChangeNPCAIType(ENPCAIType.EscortFollow);
		}
		_isSittingUp = false;
		_isInBed = false;
		_bbox = new Rectangle(0, 0, 16, 35);
		_bboxOffset = new Point(9, 6);
		SetState(EAFSM.Idle);
	}
}
