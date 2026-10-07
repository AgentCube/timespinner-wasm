using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Bosses;
using Timespinner.GameObjects.Bosses.Demon;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneKeep0 : CutsceneBase
{
	private const int DemonObjectTileID = 436;

	private const int DemonAppearY = 216;

	private const int IncubusAppearX = 96;

	private const int SuccubusAppearX = 304;

	private readonly bool _doesKnowAboutVilete;

	private readonly DemonHuman _incubus;

	private readonly DemonHuman _succubus;

	public CutsceneKeep0(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.WallTrigger;
		base.CutsceneDisappearType = ECutsceneDisappearType.WhenBossIsDead;
		base.BossTypeLinkedTo = EBossType.Demon;
		base.DoesFadeOutWhenSkipped = false;
		SpriteSheet spDemonBoss = _level.GCM.SpDemonBoss;
		_incubus = new DemonHuman(inPosition, _level, spDemonBoss, -1, new ObjectTileSpecification(436)
		{
			Argument = 3
		});
		_succubus = new DemonHuman(inPosition, _level, spDemonBoss, -1, new ObjectTileSpecification(436)
		{
			Argument = 4,
			IsFlippedHorizontally = true
		});
		_incubus.Sibling = _succubus;
		_succubus.Sibling = _incubus;
		NPCBase.EQuestStateType questProgress = NPCBase.GetQuestProgress(NPCBase.ENPCType.Astrologer, 1, 1, _level.GameSave);
		_doesKnowAboutVilete = questProgress == NPCBase.EQuestStateType.ReadyToTurnIn || questProgress == NPCBase.EQuestStateType.Closed;
	}

	public override void Initialize()
	{
		base.Initialize();
		foreach (Background foreground in _level.Foregrounds)
		{
			foreground.DrawColor = Color.Transparent;
		}
		if (base.IsCutsceneTriggered)
		{
			DemonBoss.PlaceHairpin(Position, _level);
			return;
		}
		bool flag = _level.GetNearestProtagonistPosition(Position).X < Position.X;
		_level.JukeBox.FadeOutSong(1f);
		_level.ToggleExits(isEnabled: false);
		_level.OpenAllBossDoors(-1f);
		_level.LockAllBossDoors(0.5f);
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.Idle, 0f, 0f, Vector4.Zero));
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.StopCast, 0f, 0f, Vector4.Zero));
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.Run, 0f, 0.3f, new Vector4(flag ? 1 : (-1), 0f, 0f, 0f))
		{
			DoesBlockQueue = true
		});
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.Idle, 0f, 0.55f, Vector4.Zero));
	}

	internal override void DoCutscene()
	{
		bool flag = !_level.GameSave.Inventory.RelicInventory.Inventory.ContainsKey(6);
		_incubus.InitializeMob();
		_succubus.InitializeMob();
		MovePlayerToPosition(Position, shouldFaceLeftAfter: true, shouldStandFancyAfter: true);
		AddDelegateScript(AddDemons);
		AddWaitScript(0.75f);
		if (!flag)
		{
			AddDialogue("cs_inc_0_inc_00");
			AddDialogue("cs_inc_0_suc_01");
			_level.AddScript(new ScriptAction
			{
				ScriptType = EScriptType.Delegate,
				Delegate = MoveIncubusForward,
				DoesBlockQueue = false
			});
			AddDialogue("cs_inc_0_inc_02");
			_level.AddScript(new ScriptAction
			{
				ScriptType = EScriptType.Delegate,
				Delegate = MoveSuccubusForward,
				DoesBlockQueue = false
			});
			AddDialogue("cs_inc_0_suc_03");
			AddDialogue("cs_inc_0_lun_04");
		}
		else
		{
			AddDialogue("cs_inc_ng_0_inc_00");
			AddDialogue("cs_inc_ng_0_suc_01");
			_level.AddScript(new ScriptAction
			{
				ScriptType = EScriptType.Delegate,
				Delegate = MoveIncubusForward,
				DoesBlockQueue = false
			});
			AddDialogue("cs_inc_ng_0_inc_02");
			_level.AddScript(new ScriptAction
			{
				ScriptType = EScriptType.Delegate,
				Delegate = MoveSuccubusForward,
				DoesBlockQueue = false
			});
			AddDialogue("cs_inc_ng_0_suc_03");
			AddDialogue("cs_inc_ng_0_lun_04");
		}
		AddDialogue("cs_inc_0_inc_05");
		AddDialogue("cs_inc_0_suc_06");
		if (_doesKnowAboutVilete)
		{
			AddDialogue("cs_inc_0_lun_07");
			AddDialogue("cs_inc_0_inc_08");
			AddDialogue("cs_inc_0_suc_09");
			AddDialogue("cs_inc_0_lun_10");
			AddDialogue("cs_inc_0_lun_11");
			AddDialogue("cs_inc_0_inc_12");
			AddDialogue("cs_inc_0_suc_13");
		}
		else
		{
			AddDialogue("cs_inc_alt_0_lun_00");
			AddDialogue("cs_inc_alt_0_inc_01");
			AddDialogue("cs_inc_alt_0_suc_02");
			AddDialogue("cs_inc_alt_0_inc_03");
			AddDialogue("cs_inc_alt_0_suc_04");
		}
		AddDelegateScript(MakeDemonsLaugh);
		AddDelegateScript(StartBattle);
	}

	private void StartBattle()
	{
		_incubus.StartFakeBattle();
		_succubus.StartFakeBattle();
		if (!_level.JukeBox.DoesNotPlaySounds)
		{
			_level.JukeBox.PlaySong(EBGM.Boss05A, shouldForceRestart: true, shouldImmediatelyStopPreviousSong: true);
		}
	}

	private void AddDemons()
	{
		_incubus.Appear(new Point(96, 216));
		_succubus.Appear(new Point(304, 216));
		_level.RequestAddObject(_incubus);
		_level.RequestAddObject(_succubus);
	}

	private void MakeDemonsLaugh()
	{
		_incubus.DoLaugh();
		_succubus.DoLaugh();
		_incubus.PlayCue(ESFX.BossDemonIncSuccLaughMSingle);
		_succubus.PlayCue(ESFX.BossDemonIncSuccLaughFSingle);
	}

	private void MoveIncubusForward()
	{
		_incubus.DoMove();
	}

	private void MoveSuccubusForward()
	{
		_succubus.DoMove();
	}
}
