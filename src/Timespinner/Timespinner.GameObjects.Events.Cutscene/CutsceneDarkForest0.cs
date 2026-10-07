using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneDarkForest0 : CutsceneBase
{
	private const int StartX = 1288;

	private const int StartY = 192;

	private const float StandAnimationSpeed = 0.125f;

	public CutsceneDarkForest0(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.DoesFadeOutWhenSkipped = false;
		_level.RequestScreenFadeOut(0f, 0.25f, 0.75f, 0f);
		_level.GameSave.LastWarpLevel = 0;
		_level.GameSave.LastWarpRoom = 0;
	}

	internal override void DoCutscene()
	{
		_level.PlayLevelSong();
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 204;
		animationSpec.Length = 1;
		animationSpec.Type = EAnimationType.Once;
		AnimationSpec newAnim = animationSpec;
		AnimationSpec animationSpec2 = new AnimationSpec();
		animationSpec2.Start = 205;
		animationSpec2.Length = 2;
		animationSpec2.Speed = 0.125f;
		animationSpec2.Type = EAnimationType.Once;
		AnimationSpec newAnim2 = animationSpec2;
		AnimationSpec animationSpec3 = new AnimationSpec();
		animationSpec3.Start = 212;
		animationSpec3.Length = 2;
		animationSpec3.Speed = 0.125f;
		animationSpec3.Type = EAnimationType.Once;
		AnimationSpec newAnim3 = animationSpec3;
		AnimationSpec animationSpec4 = new AnimationSpec();
		animationSpec4.Start = 237;
		animationSpec4.Length = 3;
		animationSpec4.Speed = 0.15f;
		animationSpec4.Type = EAnimationType.PingPong;
		AnimationSpec newAnim4 = animationSpec4;
		AnimationSpec animationSpec5 = new AnimationSpec();
		animationSpec5.Start = 233;
		animationSpec5.Length = 3;
		animationSpec5.Speed = 0.15f;
		animationSpec5.Type = EAnimationType.Once;
		AnimationSpec newAnim5 = animationSpec5;
		AnimationSpec animationSpec6 = new AnimationSpec();
		animationSpec6.Start = 235;
		animationSpec6.Length = 2;
		animationSpec6.Speed = 0.15f;
		animationSpec6.Type = EAnimationType.Once;
		AnimationSpec newAnim6 = animationSpec6;
		AnimationSpec animationSpec7 = new AnimationSpec();
		animationSpec7.Start = 8;
		animationSpec7.Length = 5;
		animationSpec7.Speed = 0.15f;
		animationSpec7.Type = EAnimationType.Cycle;
		AnimationSpec newAnim7 = animationSpec7;
		Protagonist mainHero = _level.MainHero;
		WarpPlayerToPosition(new Point(1288, 192));
		AddScript(new ScriptAction
		{
			ScriptType = EScriptType.Action,
			ActionType = EScriptActionType.LookDirection,
			TargetType = EScriptTargetType.Player1,
			Arguments = new Vector4(1f, 0f, 0f, 0f)
		});
		AddScript(new ScriptAction(newAnim, mainHero)
		{
			DoesBlockQueue = true
		});
		AddSummonMeyefSilent();
		AddLevelScriptAction(new ScriptAction
		{
			TargetType = EScriptTargetType.Familiar,
			ActionType = EScriptActionType.WarpToPoint,
			Arguments = new Vector4(1336f, 168f, 0f, 0f)
		});
		AddMeyefFlyAround(new Point(1328, 168), 0f, 12f);
		AddWaitScript(1f);
		PlayScriptedSFX(ESFX.LunaisStandFromLyingDown, mainHero.Position);
		AddScript(new ScriptAction(newAnim2, mainHero)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim3, mainHero)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(EScriptActionType.FancyIdle, 0f, 0f, Vector4.Zero)
		{
			TargetType = EScriptTargetType.Player1
		});
		AddWaitScript(1f);
		AddScript(new ScriptAction(newAnim4, mainHero)
		{
			DoesBlockQueue = true
		});
		AddWaitScript(0.5f);
		AddScript(new ScriptAction(newAnim7, mainHero)
		{
			DoesBlockQueue = true
		});
		AddDialogue("cs_dfo_0_lun_00");
		AddDialogue("cs_dfo_0_lun_01");
		AddDialogue("cs_dfo_0_mey_02");
		AddDialogue("cs_dfo_0_mey_03");
		AddDialogue("cs_dfo_0_lun_04");
		AddDialogue("cs_dfo_0_mey_05");
		AddScript(new ScriptAction(newAnim5, mainHero)
		{
			DoesBlockQueue = true
		});
		AddDialogue("cs_dfo_0_lun_06");
		AddScript(new ScriptAction(newAnim6, mainHero)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim7, mainHero)
		{
			DoesBlockQueue = false
		});
		AddDialogue("cs_dfo_0_mey_07");
		AddDialogue("cs_dfo_0_lun_08");
		AddDialogue("cs_dfo_0_mey_09");
		AddDialogue("cs_dfo_0_mey_10");
		AddDialogue("cs_dfo_0_lun_11");
		AddDialogue("cs_dfo_0_mey_12");
		AddDialogue("cs_dfo_0_lun_13");
		AddDialogue("cs_dfo_0_mey_14");
		AddDialogue("cs_dfo_0_lun_15");
		AddDialogue("cs_dfo_0_mey_16");
		AddDialogue("cs_dfo_0_lun_17");
		AddDialogue("cs_dfo_0_mey_18");
		AddDialogue("cs_dfo_0_lun_19");
		AddDialogue("cs_dfo_0_mey_20");
		AddDialogue("cs_dfo_0_lun_21");
		AddDialogue("cs_dfo_0_lun_22");
		AddDialogue("cs_dfo_0_mey_23");
		AddDialogue("cs_dfo_0_lun_24");
		AddDialogue("cs_dfo_0_mey_25");
		AddDialogue("cs_dfo_0_lun_26");
		AddDialogue("cs_dfo_0_lun_27");
		AddDialogue("cs_dfo_0_mey_28");
		AddDialogue("cs_dfo_0_mey_28b");
		CutsceneBase.AddLunaisPalmPunch(_level);
		AddDialogue("cs_dfo_0_lun_29");
		AddDismissMeyef();
	}
}
