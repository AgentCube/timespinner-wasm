using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Bosses;
using Timespinner.GameObjects.Bosses.Demon;
using Timespinner.GameObjects.Events.Misc;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneKeep1 : CutsceneBase
{
	private const int DemonObjectTileID = 436;

	private const int DemonAppearY = 224;

	private const int IncubusAppearX = 128;

	private const int SuccubusAppearX = 272;

	private const int PlayerStandX = 200;

	private const int PlayerStandY = 224;

	private readonly bool _doesKnowAboutVilete;

	private readonly DemonHuman _incubus;

	private readonly DemonHuman _succubus;

	public CutsceneKeep1(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.DoesFadeOutWhenSkipped = false;
		NPCBase.EQuestStateType questProgress = NPCBase.GetQuestProgress(NPCBase.ENPCType.Astrologer, 1, 1, _level.GameSave);
		_doesKnowAboutVilete = questProgress == NPCBase.EQuestStateType.ReadyToTurnIn || questProgress == NPCBase.EQuestStateType.Closed;
		SpriteSheet spDemonBoss = _level.GCM.SpDemonBoss;
		_incubus = new DemonHuman(new Point(128, 224), _level, spDemonBoss, -1, new ObjectTileSpecification(436)
		{
			Argument = 3
		});
		_succubus = new DemonHuman(new Point(272, 224), _level, spDemonBoss, -1, new ObjectTileSpecification(436)
		{
			Argument = 4,
			IsFlippedHorizontally = true
		});
		_incubus.PostBossAppear();
		_succubus.PostBossAppear();
		_level.RequestAddObject(_incubus);
		_level.RequestAddObject(_succubus);
	}

	internal override void DoCutscene()
	{
		MovePlayerToPosition(new Point(200, 224), shouldFaceLeftAfter: true, shouldStandFancyAfter: true);
		bool flag = _level.GameSave.GetSaveInt("AelEmpath") > 1;
		AddDialogue("cs_inc_1_suc_00");
		AddDialogue("cs_inc_1_inc_01");
		AddDialogue("cs_inc_1_suc_02");
		AddDialogue("cs_inc_1_inc_03");
		AddDialogue("cs_inc_1_suc_04");
		AddDialogue("cs_inc_1_inc_05");
		if (_doesKnowAboutVilete)
		{
			AddDialogue("cs_inc_1_suc_06");
		}
		else
		{
			AddDialogue("cs_inc_alt_0_inc_05");
			AddDialogue("cs_inc_alt_0_suc_06");
			AddDialogue("cs_inc_alt_0_suc_07");
		}
		AddDialogue("cs_inc_1_inc_07");
		AddDialogue("cs_inc_1_suc_08");
		AddDialogue("cs_inc_1_inc_09");
		AddDialogue("cs_inc_1_suc_10");
		AddDialogue("cs_inc_1_inc_11");
		AddDialogue("cs_inc_1_suc_12");
		AddDelegateScript(RemoveDemons);
		AddSummonMeyef();
		AddMeyefFlyInFrontOfPlayer(0.5f, doesBlock: false);
		AddWaitScript(0.5f);
		AddScript(new ScriptAction
		{
			ScriptType = EScriptType.Action,
			ActionType = EScriptActionType.LookDirection,
			TargetType = EScriptTargetType.Familiar,
			Arguments = new Vector4(1f, 0f, 0f, 0f)
		});
		AddMeyefMew();
		if (!_doesKnowAboutVilete)
		{
			AddDialogue("cs_inc_1_lun_13alt");
		}
		AddDialogue((!flag) ? "cs_inc_1_lun_14" : "cs_inc_1_lun_15");
		AddDialogue("cs_inc_1_lun_16");
		AddDialogue("cs_inc_1_mey_17");
		AddDialogue("cs_inc_1_lun_18");
		AddDialogue("cs_inc_1_mey_19");
		AddDialogue("cs_inc_1_mey_20");
		AddDialogue("cs_inc_1_mey_21");
		AddDialogue("cs_inc_1_lun_22");
		AddDialogue("cs_inc_1_lun_23");
		AddDialogue("cs_inc_1_lun_24");
		AddDialogue("cs_inc_1_mey_25");
		AddDialogue("cs_inc_1_mey_26");
		AddDialogue("cs_inc_1_lun_27");
		AddDismissMeyef();
		AddPlaySong(EBGM.Level05, doesStopOtherSong: true);
	}

	private void RemoveDemons()
	{
		_incubus.Disappear();
		_succubus.Disappear();
		_level.AddEvent(new SandStreamerEvent(_level, _incubus.Bbox.Center, ESandStreamerType.BossDeath));
		DemonBoss.PlaceHairpin(new Point(_succubus.Position.X, _succubus.Bbox.Top), _level);
	}
}
