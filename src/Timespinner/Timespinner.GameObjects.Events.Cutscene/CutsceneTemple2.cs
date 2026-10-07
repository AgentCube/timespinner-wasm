using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Gameplay.Scripts;
using Timespinner.GameObjects.Events.EnvironmentPrefabs;
using Timespinner.GameObjects.Events.EnvironmentPrefabs.L17_End;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneTemple2 : CutsceneBase
{
	private const int StartX = 200;

	private const int StartY = 176;

	private const int AscendStartY = 144;

	private const int MeyefPhase4PositionX = 272;

	private const int MeyefPhase4PositionY = 152;

	private const int PrefabID = 491;

	private readonly Point _roomCenter;

	private readonly EnvPrefabEndAscend _ascendEvent;

	public CutsceneTemple2(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.IsWarpingAtEndOfCutscene = true;
		base.DoesHideOrbsShowAnimation = false;
		base.IsWarpingAtEndOfCutscene = true;
		_ascendEvent = new EnvPrefabEndAscend(_level, new Point(200, 144), -1, new ObjectTileSpecification(491)
		{
			Argument = 1706
		}, EEnvironmentPrefabType.L17_Ascend)
		{
			IsAscendedVisible = false
		};
		_level.IsMufflingPlayerSFX = true;
		_roomCenter = new Point(200, 144);
		_level.JukeBox.StopSong();
	}

	internal override void DoCutscene()
	{
		_ascendEvent.Initialize();
		_level.RequestAddObject(_ascendEvent);
		WarpPlayerToPosition(new Point(200, 176));
		AddWaitScript(0.1f);
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.LookDirection,
			ActionTimer = 0.25f,
			DoesBlockQueue = true,
			Arguments = new Vector4(1f, 0f, 0f, 0f)
		});
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.FancyIdle,
			DoesBlockQueue = false
		});
		AddSummonMeyefSilent();
		AddMeyefFlyInFrontOfPlayer(0f, doesBlock: false);
		AddWaitScript(3f);
		AddDelegateScript(_ascendEvent.StartPhase1);
		AddWaitScript(1f);
		AddDialogue("cs_tem_1_lun_00");
		PlayScriptedSFX(ESFX.CsAscendPhase2Start, _roomCenter);
		AddDelegateScript(_ascendEvent.StartPhase2);
		AddWaitScript(1f);
		AddDialogue("cs_tem_1_lun_01");
		AddDelegateScript(_ascendEvent.StartPhase3);
		AddDialogue("cs_tem_1_mey_02");
		AddDialogue("cs_tem_1_lun_03");
		AddDialogue("cs_tem_1_mey_04");
		PlayScriptedSFX(ESFX.CsAscendPhase3Start, _roomCenter);
		AddDelegateScript(_ascendEvent.StartPhase4);
		AddScript(new FamiliarScript(FamiliarScript.EFamiliarScriptType.CutsceneFlyTo)
		{
			ActionTimer = 0f,
			Arguments = new Vector4(272f, 152f, -1f, 0f)
		});
		AddScript(new ScriptAction(EScriptType.HideShowPlayer, 0f, 0f, doesBlock: false, new Vector4(0f, 1f, 0f, 0f)));
		AddWaitScript(1.5f);
		AddDialogue("cs_tem_1_lun_05");
		AddDialogue("cs_tem_1_mey_06");
		AddDialogue("cs_tem_1_lun_07");
		AddDialogue("cs_tem_1_mey_08");
		AddWaitScript(0.5f);
		AddDelegateScript(_ascendEvent.DoNod);
		AddWaitScript(0.5f);
		PlayScriptedSFX(ESFX.CsAscendPhase4);
		AddDelegateScript(_ascendEvent.StartPhase5);
		AddWaitScript(5f);
		AddDismissMeyefSilent();
		AddUnhidePlayer();
		AddDelegateScript(StartEnding);
	}

	private void StartEnding()
	{
		CutsceneBase.StartSandmanEnding(_level);
	}
}
