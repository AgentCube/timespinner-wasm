using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneLakeDesolation1 : CutsceneBase
{
	public CutsceneLakeDesolation1(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.WallTrigger;
		base.DoesFadeOutWhenSkipped = false;
	}

	internal override void DoCutscene()
	{
		_level.PlayLevelSong();
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 237;
		animationSpec.Length = 3;
		animationSpec.Speed = 0.15f;
		animationSpec.Type = EAnimationType.PingPong;
		AnimationSpec newAnim = animationSpec;
		AnimationSpec animationSpec2 = new AnimationSpec();
		animationSpec2.Start = 233;
		animationSpec2.Length = 3;
		animationSpec2.Speed = 0.15f;
		animationSpec2.Type = EAnimationType.Once;
		AnimationSpec newAnim2 = animationSpec2;
		AnimationSpec animationSpec3 = new AnimationSpec();
		animationSpec3.Start = 236;
		animationSpec3.Length = 1;
		animationSpec3.Speed = 0.15f;
		animationSpec3.Type = EAnimationType.Once;
		AnimationSpec newAnim3 = animationSpec3;
		AnimationSpec animationSpec4 = new AnimationSpec();
		animationSpec4.Start = 8;
		animationSpec4.Length = 5;
		animationSpec4.Speed = 0.15f;
		animationSpec4.Type = EAnimationType.Cycle;
		AnimationSpec newAnim4 = animationSpec4;
		Protagonist mainHero = _level.MainHero;
		AddScript(new ScriptAction(EScriptActionType.FancyIdle, 0f, 0f, Vector4.Zero)
		{
			TargetType = EScriptTargetType.Player1
		});
		AddWaitScript(1.25f);
		AddScript(new ScriptAction(newAnim, mainHero)
		{
			DoesBlockQueue = true
		});
		AddWaitScript(1f);
		AddScript(new ScriptAction(newAnim2, mainHero)
		{
			DoesBlockQueue = true
		});
		AddDialogue("cs_lde_1_lun_00");
		AddScript(new ScriptAction(newAnim3, mainHero)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim4, mainHero)
		{
			DoesBlockQueue = false
		});
		AddDialogue("cs_lde_1_lun_01");
	}
}
