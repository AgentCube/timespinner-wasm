using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneLakeDesolation0 : CutsceneBase
{
	private const float StandAnimationSpeed = 0.125f;

	public CutsceneLakeDesolation0(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.DoesMakePlayerIdleAtStart = false;
	}

	internal override void DoCutscene()
	{
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 205;
		animationSpec.Length = 2;
		animationSpec.Speed = 0.125f;
		animationSpec.Type = EAnimationType.Once;
		AnimationSpec newAnim = animationSpec;
		AnimationSpec animationSpec2 = new AnimationSpec();
		animationSpec2.Start = 212;
		animationSpec2.Length = 2;
		animationSpec2.Speed = 0.125f;
		animationSpec2.Type = EAnimationType.Once;
		AnimationSpec newAnim2 = animationSpec2;
		AnimationSpec animationSpec3 = new AnimationSpec();
		animationSpec3.Start = 0;
		animationSpec3.Length = 5;
		animationSpec3.Speed = 0.11f;
		animationSpec3.Type = EAnimationType.Cycle;
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
		animationSpec7.Start = 44;
		animationSpec7.Length = 5;
		animationSpec7.Speed = 0.1f;
		animationSpec7.Type = EAnimationType.Once;
		AnimationSpec newAnim7 = animationSpec7;
		AnimationSpec animationSpec8 = new AnimationSpec();
		animationSpec8.Start = 49;
		animationSpec8.Length = 2;
		animationSpec8.Speed = 0.1f;
		animationSpec8.Type = EAnimationType.Once;
		AnimationSpec newAnim8 = animationSpec8;
		Protagonist mainHero = _level.MainHero;
		AddWaitScript(1.5f);
		AddDialogue("cs_lde_0_lun_00");
		AddWaitScript(0.75f);
		PlayScriptedSFX(ESFX.LunaisStandFromLyingDown, mainHero.Position);
		AddScript(new ScriptAction(newAnim, mainHero)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim2, mainHero)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(EScriptActionType.FancyIdle, 0f, 0f, Vector4.Zero)
		{
			TargetType = EScriptTargetType.Player1
		});
		AddWaitScript(1.25f);
		AddScript(new ScriptAction(newAnim4, mainHero)
		{
			DoesBlockQueue = true
		});
		AddWaitScript(1f);
		AddScript(new ScriptAction(newAnim5, mainHero)
		{
			DoesBlockQueue = true
		});
		AddDialogue("cs_lde_0_lun_01");
		AddScript(new ScriptAction(newAnim6, mainHero)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim7, mainHero)
		{
			DoesBlockQueue = true
		});
		AddDialogue("cs_lde_0_lun_02");
		AddDialogue("cs_lde_0_lun_03");
		AddScript(new ScriptAction(newAnim8, mainHero)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim3, mainHero)
		{
			DoesBlockQueue = false
		});
	}
}
