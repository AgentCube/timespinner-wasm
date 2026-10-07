using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Cutscene;
using Timespinner.GameObjects.Events.EnvironmentPrefabs;
using Timespinner.GameObjects.Events.EnvironmentPrefabs.L00_Prologue;
using Timespinner.GameObjects.Events.Lanterns;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.NPCs;

internal sealed class SelenNPC : NPCBase
{
	private const int EnvPrefabID = 491;

	private const int DummyArgument = 3;

	private const int DummySpawnX = 136;

	private const int ObjectSpawnY = 192;

	private const int EmperorAnchorOffsetX = 32;

	private const int EmperorAnchorOffsetY = -34;

	internal const byte Anim_IdleLength = 5;

	private const byte Anim_PreRunLength = 5;

	private const byte Anim_RunLength = 10;

	private const byte Anim_EndRunLength = 4;

	internal const byte Anim_PreTimespinnerLength = 1;

	internal const byte Anim_TimespinnerLength = 4;

	internal const byte Anim_FloatingLength = 4;

	internal const byte Anim_DieLength = 8;

	internal const byte Anim_IdleStart = 0;

	private const byte Anim_PreRunStart = 5;

	private const byte Anim_RunStart = 10;

	private const byte Anim_LandStart = 20;

	private const byte Anim_EndRunStart = 21;

	internal const byte Anim_PreTimespinnerStart = 25;

	internal const byte Anim_TimespinnerStart = 26;

	internal const byte Anim_FloatingStart = 30;

	internal const byte Anim_DieStart = 34;

	internal const byte Anim_PickUpStart = 56;

	internal const byte Anim_PickUpLength = 4;

	internal const byte Anim_HoldLookStart = 59;

	internal const byte Anim_HoldLookLength = 3;

	internal const float Anim_IdleSpeed = 0.13f;

	private const float Anim_RunningSpeed = 0.07f;

	private const float MaxRunningSpeed = 155f;

	private readonly List<BaseLantern> _lanterns = new List<BaseLantern>();

	private bool _isCutsceneFloating;

	private bool _isCutsceneFloatingFalling;

	private int _tutorialSection;

	private int _lanternCount;

	private float _cutsceneFloatTimer;

	private float _timeForCutsceneFloat;

	private Point _cutsceneFloatStartingPoint;

	private Point _cutsceneFloatTarget;

	private EnvPrefabProDummy _dummy;

	private Mobile _anchorEmperor;

	private Protagonist _lunais;

	internal bool IsDoingTutorial { get; set; }

	public SelenNPC(Level inLevel, Point inPosition, int inID)
		: base(inLevel, inPosition, inLevel.GCM.SpSelen, inID)
	{
		_npcType = ENPCType.Selen;
		_isAffectedByGravity = true;
		IsFacingLeft = false;
		_isAffectedByTime = true;
		base.CannotBeGrabbed = true;
		_maxMoveSpeed = 155f;
		_npcAIType = ENPCAIType.None;
		_npcTriggerType = ENPCTriggerType.Talk;
		base.TriggerBbox = new Rectangle(0, 0, 64, 64);
		_bbox = new Rectangle(0, 0, 16, 32);
		_bboxOffset = new Point(15, 7);
		_sprite = _level.GCM.SpSelen;
		ChangeAnimation(0, 5, 0.13f, EAnimationType.Cycle);
	}

	public override void SetState(EAFSM state)
	{
		EAFSM currentState = _currentState;
		if (currentState != state)
		{
			_hasStartedChargingAnimation = false;
		}
		base.SetState(state);
		switch (state)
		{
		case EAFSM.Running:
			ChangeAnimation(10, 10, 0.07f, EAnimationType.Cycle, 5, 5, 0.07f);
			break;
		case EAFSM.Idle:
			if (currentState == EAFSM.Running)
			{
				ChangeAnimation(0, 5, 0.13f, EAnimationType.Cycle, 21, 4, 0.1f);
			}
			else
			{
				ChangeAnimation(0, 5, 0.13f, EAnimationType.Cycle);
			}
			break;
		}
	}

	protected override void TriggerNPC(Protagonist who)
	{
		base.TriggerNPC(who);
		StartNPCDialogue();
		MovePlayerToTalkingPosition();
		if (IsDoingTutorial)
		{
			switch (_tutorialSection)
			{
			case 0:
				DoTutorialDefault0();
				break;
			case 1:
				DoTutorialDefault1();
				break;
			case 2:
				DoTutorialDefault2();
				break;
			case 3:
				DoTutorialDefault3();
				break;
			}
		}
		else
		{
			DoFirstDialogue();
		}
		EndNPCDialogue();
	}

	public override void Update(float delta)
	{
		if (_isCutsceneFloating)
		{
			UpdateCutsceneFloatToPoint(delta);
		}
		base.Update(delta);
		if (!IsDoingTutorial)
		{
			return;
		}
		switch (_tutorialSection)
		{
		case 0:
		{
			PlayerInventory inventory = _level.GameSave.Inventory;
			if (inventory.EquippedMeleeOrbA != 0 && inventory.EquippedMeleeOrbB != 0)
			{
				StartTutorialSegment();
				DoTutorialEnd0();
				EndTutorialSegment();
			}
			break;
		}
		case 1:
		{
			if (_dummy != null && _dummy.IsDestroyed)
			{
				StartTutorialSegment();
				DoTutorialEnd1();
				EndTutorialSegment();
			}
			int num = 0;
			foreach (BaseLantern lantern in _lanterns)
			{
				if (!lantern.IsBroken)
				{
					num++;
				}
			}
			if (num != _lanternCount)
			{
				StartTutorialSegment();
				DoLanternDialogue();
				EndTutorialSegment();
			}
			_lanternCount = num;
			break;
		}
		case 2:
			if (_level.GameSave.Inventory.EquippedSpellOrb != 0)
			{
				StartTutorialSegment();
				DoTutorialEnd2();
				EndTutorialSegment();
			}
			break;
		case 3:
			if (_lunais != null && _lunais.HasCastASpell)
			{
				StartTutorialSegment();
				DoTutorialEnd3();
				EndTutorialSegment();
			}
			break;
		}
	}

	private void DoFirstDialogue()
	{
	}

	internal void SpawnDummy()
	{
		_dummy = new EnvPrefabProDummy(_level, new Point(136, 192), -1, new ObjectTileSpecification(491)
		{
			Argument = 3,
			IsFlippedHorizontally = true
		}, EEnvironmentPrefabType.L0_Dummy);
		_dummy.Initialize();
		_level.RequestAddObject(_dummy);
	}

	private void StartTutorialSegment()
	{
		AddLevelScriptAction(new ScriptAction
		{
			ScriptType = EScriptType.CutsceneStart,
			DoesBlockQueue = false
		});
	}

	private void EndTutorialSegment()
	{
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.Idle,
			ActionTimer = 0.03f,
			DoesBlockQueue = true
		});
	}

	private void DoTutorialDefault0()
	{
		AddDialogue("cs_protut_0_sel_05");
		_level.ShowMenuDialogueMessage("cs_protut_0_A");
	}

	private void DoTutorialEnd0()
	{
		_tutorialSection = 1;
		_level.PlayCue(ESFX.MenuBuy, new Point(200, 120));
		AddWaitScript(0.15f);
		AddDialogue("cs_protut_0_lun_06");
		AddDialogue("cs_protut_0_sel_07");
		CutsceneBase.AddLunaisPalmPunch(_level);
		AddDialogue("cs_protut_0_lun_08");
		AddDialogue("cs_protut_0_sel_09");
		AddDialogue("cs_protut_0_lun_10");
		AddDialogue("cs_protut_0_B");
		IEnumerable<GameEvent> eventAllEventsOfType = _level.GetEventAllEventsOfType(EEventTileType.Lantern);
		foreach (GameEvent item2 in eventAllEventsOfType)
		{
			if (item2 is BaseLantern item)
			{
				_lanterns.Add(item);
				_lanternCount++;
			}
		}
		if (_dummy != null)
		{
			_dummy.CanBeDamaged = true;
		}
	}

	private void DoTutorialDefault1()
	{
		AddDialogue("cs_protut_0_sel_09b");
		AddDialogue("cs_protut_0_B");
	}

	private void DoTutorialEnd1()
	{
		_tutorialSection = 2;
		_level.PlayCue(ESFX.MenuBuy, new Point(200, 120));
		AddWaitScript(0.25f);
		AddDialogue("cs_protut_0_sel_11");
		MovePlayerToTalkingPosition();
		AddDialogue("cs_protut_2_sel_00");
		AddWaitScript(0.01f);
		_level.GameSave.GiveOrb(EInventoryOrbType.Blue, EOrbSlot.Spell);
		AddScript(new ScriptAction(EInventoryOrbType.Blue, EOrbSlot.Spell));
		AddWaitScript(0.01f);
		AddDialogue("cs_protut_2_sel_01");
		_level.ShowMenuDialogueMessage("cs_protut_2_A");
	}

	private void DoTutorialDefault2()
	{
		AddDialogue("cs_protut_2_sel_02");
		_level.ShowMenuDialogueMessage("cs_protut_2_A");
	}

	private void DoTutorialEnd2()
	{
		_tutorialSection = 3;
		_level.PlayCue(ESFX.MenuBuy, new Point(200, 120));
		AddWaitScript(0.25f);
		AddDialogue("cs_protut_2_sel_02b");
		AddDialogue("cs_protut_2_B");
		_lunais = _level.MainHero;
		if (_lunais != null)
		{
			_lunais.HasCastASpell = false;
		}
	}

	private void DoTutorialDefault3()
	{
		AddDialogue("cs_protut_2_sel_02b");
		AddDialogue("cs_protut_2_B");
	}

	private void DoTutorialEnd3()
	{
		_tutorialSection = 4;
		_level.PlayCue(ESFX.MenuBuy, new Point(200, 120));
		AddWaitScript(0.25f);
		AddDialogue("cs_protut_2_lun_03");
		AddDialogue("cs_protut_2_sel_04");
		AddDialogue("cs_protut_2_lun_04");
		AddDialogue("cs_protut_2_sel_05");
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = this,
			ScriptType = EScriptType.Action,
			ActionType = EScriptActionType.Run,
			Arguments = new Vector4(1f, 0f, 0f, 0f),
			ActionTimer = 0.55f,
			DoesBlockQueue = false
		});
		AddWaitScript(0.1f);
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ScriptType = EScriptType.Action,
			ActionType = EScriptActionType.Run,
			Arguments = new Vector4(1f, 0f, 0f, 0f),
			ActionTimer = 0.55f,
			DoesBlockQueue = false
		});
		AddLevelScriptAction(new ScriptAction
		{
			ScriptType = EScriptType.FadeInFadeOut,
			Arguments = new Vector4(0.5f, 0f, 0f, 0f)
		});
		AddUnskippableWaitScript(0.5f);
		LevelChangeRequest levelChangeRequest = new LevelChangeRequest();
		levelChangeRequest.LevelID = 0;
		levelChangeRequest.RoomID = 0;
		LevelChangeRequest levelRoomChangeRequest = levelChangeRequest;
		AddLevelScriptAction(new ScriptAction(levelRoomChangeRequest));
	}

	private void DoLanternDialogue()
	{
		AddDialogue((_lanternCount == 2) ? "cs_protut_1_sel_lamp" : "cs_protut_1_sel_lamp2");
	}

	internal void CutsceneFloatToPoint(Point targetPoint, float duration, bool isFalling)
	{
		_cutsceneFloatTarget = targetPoint;
		_cutsceneFloatStartingPoint = Position;
		_timeForCutsceneFloat = duration;
		_cutsceneFloatTimer = 0f;
		_isCutsceneFloating = true;
		_isAffectedByGravity = false;
		_isFlying = true;
		base.DrawPlane = EDrawPlane.Front;
		_isCutsceneFloatingFalling = isFalling;
		SetState(EAFSM.Falling);
		if (!_isCutsceneFloatingFalling)
		{
			ChangeAnimation(30, 4, 0.1f, EAnimationType.Cycle);
			base.DoesDrawAura = true;
			base.AuraColor = new Color(0.25f, 0.25f, 0.75f, 0.25f);
			base.AuraOffset = new Vector2(2f, 2f);
			base.AuraFrequency = 9f;
			base.AuraSize = 0.1f;
			_auraCount = 5f;
		}
		else
		{
			base.DoesDrawAura = false;
		}
	}

	private void UpdateCutsceneFloatToPoint(float delta)
	{
		if (_cutsceneFloatTimer < _timeForCutsceneFloat)
		{
			_cutsceneFloatTimer += delta;
			if (_cutsceneFloatTimer >= _timeForCutsceneFloat)
			{
				_cutsceneFloatTimer = _timeForCutsceneFloat;
				Position = _cutsceneFloatTarget;
			}
			else
			{
				float amount = _cutsceneFloatTimer / _timeForCutsceneFloat;
				Position = _cutsceneFloatStartingPoint.CosInterpolate(_cutsceneFloatTarget, amount);
			}
			return;
		}
		Position = _cutsceneFloatTarget;
		if (_isCutsceneFloatingFalling)
		{
			ChangeAnimation(34, 8, 0.13f, EAnimationType.Once);
			_isCutsceneFloating = false;
		}
		else if (_anchorEmperor != null)
		{
			Position = new Point(_anchorEmperor.Position.X + 32, _anchorEmperor.Position.Y + -34);
		}
	}

	internal void SetEmperorAnchor(Mobile emperor)
	{
		_anchorEmperor = emperor;
	}
}
