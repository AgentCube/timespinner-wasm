using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneEmpTower2 : CutsceneBase
{
	public CutsceneEmpTower2(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.DoesFadeOutWhenSkipped = false;
		base.IsWarpingAtEndOfCutscene = true;
	}

	internal override void DoCutscene()
	{
		AddDialogue("cs_emp_2_lun_09");
		AddDialogue("cs_emp_2_lun_10");
		FadeOut(0.5f);
		AddDelegateScript(StartEnding);
	}

	private void StartEnding()
	{
		_level.GameSave.SavePostEndingAB();
		CutsceneBase.StartEnding(0, _level);
	}
}
