using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneTemple3 : CutsceneBase
{
	public CutsceneTemple3(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.WallTrigger;
		base.DoesFadeOutWhenSkipped = false;
	}

	internal override void DoCutscene()
	{
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 240;
		animationSpec.Length = 3;
		animationSpec.Speed = 0.1f;
		animationSpec.Type = EAnimationType.Once;
		AnimationSpec newAnim = animationSpec;
		AnimationSpec animationSpec2 = new AnimationSpec();
		animationSpec2.Start = 240;
		animationSpec2.Length = 3;
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
		Protagonist mainHero = _level.MainHero;
		AddScript(new ScriptAction(newAnim, mainHero)
		{
			DoesBlockQueue = true
		});
		AddWaitScript(0.25f);
		AddDialogue("cs_tem_2_lun_00");
		AddDialogue("cs_tem_2_lun_01");
		AddScript(new ScriptAction(newAnim2, mainHero)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim3, mainHero)
		{
			DoesBlockQueue = false
		});
	}
}
