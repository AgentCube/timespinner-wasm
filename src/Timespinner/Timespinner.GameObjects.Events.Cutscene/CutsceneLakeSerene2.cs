using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Gameplay.Scripts;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Bosses.Demon;
using Timespinner.GameObjects.Events.Doors;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneLakeSerene2 : CutsceneBase
{
	private const int DemonObjectTileID = 436;

	private const int PlayerStartX = 192;

	private const int PlayerStartY = 176;

	private const int DemonAppearY = 168;

	private const int IncubusAppearX = 112;

	private const int SuccubusAppearX = 288;

	private const int MeyefLandX = 216;

	private const int MeyefLandY = 170;

	private const int FutureLevelID = 1;

	private const int FutureRoomID = 25;

	private readonly bool _areDemonsDead;

	private readonly Point _incubusPosition;

	private readonly Point _succubusPosition;

	private readonly DemonHuman _incubus;

	private readonly DemonHuman _succubus;

	public CutsceneLakeSerene2(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.DoesHideOrbsAutomatically = false;
		base.DoesFadeOutWhenSkipped = false;
		_areDemonsDead = _level.GameSave.GetSaveBool(BossClass.GetSaveKeyByBossType(EBossType.Demon));
		_incubusPosition = new Point(112, 168);
		_succubusPosition = new Point(288, 168);
		if (!_areDemonsDead)
		{
			SpriteSheet spDemonBoss = _level.GCM.SpDemonBoss;
			_incubus = new DemonHuman(_incubusPosition, _level, spDemonBoss, -1, new ObjectTileSpecification(436)
			{
				Argument = 3
			});
			_succubus = new DemonHuman(_succubusPosition, _level, spDemonBoss, -1, new ObjectTileSpecification(436)
			{
				Argument = 4,
				IsFlippedHorizontally = true
			});
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		if (!_areDemonsDead)
		{
			_incubus.InitializeMob();
			_succubus.InitializeMob();
		}
	}

	internal override void DoCutscene()
	{
		_level.JukeBox.FadeOutSong(1f);
		AddSummonMeyef();
		if (!_areDemonsDead)
		{
			MovePlayerToPosition(new Point(192, 176), shouldFaceLeftAfter: true, shouldStandFancyAfter: false);
			AddWaitScript(0.75f);
			AddMeyefMew();
			AddDelegateScript(AddDemons);
			AddWaitScript(0.75f);
			AddScript(new FamiliarScript(FamiliarScript.EFamiliarScriptType.CutsceneFlyTo)
			{
				ActionTimer = 0.4f,
				Arguments = new Vector4(216f, 170f, 1f, 0f),
				DoesBlockQueue = false
			});
			AddWaitScript(0.3f);
			AddFamiliarAnimation(10, 2, 0.1f, EAnimationType.Once, doesBlock: true);
			AddScript(new FamiliarScript(FamiliarScript.EFamiliarScriptType.SitStill));
			AddFamiliarAnimation(47, 2, 0.1f, EAnimationType.Once, doesBlock: false);
			AddDialogue("cs_lse_1_lun_00");
			AddDialogue("cs_lse_1_suc_01");
			AddDialogue("cs_lse_1_inc_02");
			AddDialogue("cs_lse_1_suc_03");
			AddDialogue("cs_lse_1_inc_04");
			AddDialogue("cs_lse_1_suc_05");
			_level.AddScript(new ScriptAction
			{
				ScriptType = EScriptType.Delegate,
				Delegate = MoveIncubusForward,
				DoesBlockQueue = false
			});
			AddDialogue("cs_lse_1_inc_06");
			_level.AddScript(new ScriptAction
			{
				ScriptType = EScriptType.Delegate,
				Delegate = MoveSuccubusForward,
				DoesBlockQueue = false
			});
			AddDialogue("cs_lse_1_suc_07");
			AddMeyefGrowl();
			AddGhostDialogue("cs_lse_1_mey_08");
			AddDialogue("cs_lse_1_lun_09");
			AddDialogue("cs_lse_1_inc_10");
			AddDialogue("cs_lse_1_suc_11");
			AddDialogue("cs_lse_1_lun_12");
			AddMeyefGrowl();
			AddGhostDialogue("cs_lse_1_mey_13");
			AddDialogue("cs_lse_1_suc_14");
			AddDialogue("cs_lse_1_inc_15");
			AddDialogue("cs_lse_1_suc_16");
			AddDialogue("cs_lse_1_inc_17");
			AddDialogue("cs_lse_1_suc_18");
			AddDialogue("cs_lse_1_lun_19");
			AddDialogue("cs_lse_1_inc_20");
			AddDialogue("cs_lse_1_suc_21");
			AddDialogue("cs_lse_1_inc_22");
			MovePlayerToPosition(new Point(176, 176), shouldFaceLeftAfter: true, shouldStandFancyAfter: false);
			AddMeyefGrowl();
			AddDialogue("cs_lse_1_mey_23");
			AddDialogue("cs_lse_1_inc_24");
			AddDialogue("cs_lse_1_suc_25");
			AddDialogue("cs_lse_1_suc_25b");
			AddDialogue("cs_lse_1_inc_26");
			AddDelegateScript(MakeDemonsLaugh);
			PlayScriptedSFX(ESFX.BossDemonIncSuccLaughMSingle, _incubusPosition);
			PlayScriptedSFX(ESFX.BossDemonIncSuccLaughFSingle, _succubusPosition);
			AddWaitScript(1f);
			AddDelegateScript(RemoveDemons);
			AddFamiliarAnimation(47, 1, 0.1f, EAnimationType.Once, doesBlock: true);
			AddFamiliarAnimation(15, 2, 0.1f, EAnimationType.Once, doesBlock: true);
			AddFamiliarAnimation(0, 5, 0.1f, EAnimationType.Cycle, doesBlock: false);
		}
		AddMeyefFlyAround(new Point(232, 154), 2f, 16f);
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Familiar,
			ActionType = EScriptActionType.LookDirection,
			ActionTimer = 0.25f,
			DoesBlockQueue = false,
			Arguments = new Vector4(-1f, 0f, 0f, 0f)
		});
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.GoToPoint,
			ActionTimer = 2f,
			DoesBlockQueue = true,
			Arguments = new Vector4(192f, 176f, 0f, 0f)
		});
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.FancyIdle,
			DoesBlockQueue = false
		});
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.LookDirection,
			ActionTimer = 0.25f,
			DoesBlockQueue = true,
			Arguments = new Vector4(1f, 0f, 0f, 0f)
		});
		if (!_areDemonsDead)
		{
			AddDialogue("cs_lse_1_lun_27");
			AddDialogue("cs_lse_1_mey_28");
			AddDialogue("cs_lse_1_lun_29");
			AddDialogue("cs_lse_1_mey_30");
		}
		AddDialogue("cs_lse_1_lun_31");
		AddDialogue("cs_lse_1_mey_32");
		AddDialogue("cs_lse_1_lun_33");
		AddDialogue("cs_lse_1_mey_34");
		AddDialogue("cs_lse_1_lun_35");
		AddDialogue("cs_lse_1_mey_36");
		AddDialogue("cs_lse_1_lun_37");
		AddDialogue("cs_lse_1_mey_38");
		AddDialogue("cs_lse_1_lun_39");
		AddDialogue("cs_lse_1_lun_40");
		AddMeyefPurr();
		AddDialogue("cs_lse_1_lun_41");
		AddDialogue("cs_lse_1_mey_42");
		AddDialogue("cs_lse_1_lun_43");
		CutsceneBase.AddLunaisPalmPunch(_level);
		AddDismissMeyef();
		AddDelegateScript(TeleportToFuture);
		AddUnskippableWaitScript(1f);
	}

	private void AddMeyefGrowl()
	{
		AddFamiliarAnimation(48, 2, 0.1f, EAnimationType.Once, doesBlock: true);
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 48;
		animationSpec.Length = 3;
		animationSpec.Speed = 0.1f;
		animationSpec.Type = EAnimationType.Once;
		animationSpec.IsInReverse = true;
		AnimationSpec newAnim = animationSpec;
		AddScript(new ScriptAction(newAnim, EScriptTargetType.Familiar)
		{
			DoesBlockQueue = false
		});
		PlayScriptedSFX(ESFX.MeyefDamaged, new Point(216, 170));
	}

	private void AddDemons()
	{
		_incubus.Appear(_incubusPosition);
		_succubus.Appear(_succubusPosition);
		_level.RequestAddObject(_incubus);
		_level.RequestAddObject(_succubus);
	}

	private void MakeDemonsLaugh()
	{
		_incubus.DoLaugh();
		_succubus.DoLaugh();
	}

	private void RemoveDemons()
	{
		_incubus.Disappear();
		_succubus.Disappear();
	}

	private void MoveIncubusForward()
	{
		_incubus.DoMove();
	}

	private void MoveSuccubusForward()
	{
		_succubus.DoMove();
	}

	private void TeleportToFuture()
	{
		LevelChangeRequest levelChangeRequest = new LevelChangeRequest();
		levelChangeRequest.LevelID = 1;
		levelChangeRequest.PreviousLevelID = _level.ID;
		levelChangeRequest.RoomID = 25;
		levelChangeRequest.IsUsingWarp = true;
		levelChangeRequest.IsUsingWhiteFadeOut = true;
		levelChangeRequest.AdditionalBlackScreenTime = 0.25f;
		levelChangeRequest.FadeOutTime = 0.25f;
		levelChangeRequest.FadeInTime = 1f;
		LevelChangeRequest levelRequest = levelChangeRequest;
		IEnumerable<GameEvent> eventAllEventsOfType = _level.GetEventAllEventsOfType(EEventTileType.TransitionWarpEvent);
		foreach (GameEvent item in eventAllEventsOfType)
		{
			if (item is TransitionWarpEvent transitionWarpEvent)
			{
				transitionWarpEvent.StartWarpSequence(levelRequest);
				break;
			}
		}
	}
}
