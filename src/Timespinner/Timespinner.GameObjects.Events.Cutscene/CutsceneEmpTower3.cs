using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneEmpTower3 : CutsceneBase
{
	public CutsceneEmpTower3(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.WallTrigger;
		base.CutsceneDisappearType = ECutsceneDisappearType.Never;
		base.DoesFadeOutWhenSkipped = false;
		base.IsWarpingAtEndOfCutscene = true;
	}

	internal override void DoCutscene()
	{
		AddDialogue("cs_emp_2_lun_11");
		AddDialogue("cs_emp_2_lun_12");
		FadeOut(0.5f);
		AddDelegateScript(StartEnding);
	}

	private void StartEnding()
	{
		_level.GameSave.SavePostEndingAB();
		CutsceneBase.StartEnding(1, _level);
	}
}
