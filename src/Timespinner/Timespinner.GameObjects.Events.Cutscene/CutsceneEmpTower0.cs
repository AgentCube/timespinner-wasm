using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneEmpTower0 : CutsceneBase
{
	public CutsceneEmpTower0(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.WallTrigger;
		base.DoesFadeOutWhenSkipped = false;
	}

	internal override bool AreTriggerConditionsMet()
	{
		return !_level.GameSave.GetSaveBool("IsTerrilisDead") && !_level.GameSave.GetSaveBool("IsPrinceDead");
	}

	internal override void DoCutscene()
	{
		AddDialogue("cs_emp_0_lun_00");
		AddDialogue("cs_emp_0_lun_01");
	}
}
