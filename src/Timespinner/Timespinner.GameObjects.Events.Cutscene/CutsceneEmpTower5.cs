using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneEmpTower5 : CutsceneBase
{
	public CutsceneEmpTower5(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Instant;
		base.DoesFadeOutWhenSkipped = false;
	}

	internal override bool AreTriggerConditionsMet()
	{
		return _level.GameSave.GetSaveBool("IsEmperorKilledAfterAlts");
	}

	internal override void DoCutscene()
	{
		AddWaitScript(0.75f);
		CutsceneBase.AddLunaisPalmPunch(_level);
		AddDialogue("cs_emp_5_lun_00");
	}
}
