using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneEmpTower4 : CutsceneBase
{
	private readonly bool _hasVanillaCutsceneBeenTriggered;

	public CutsceneEmpTower4(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.WallTrigger;
		base.DoesFadeOutWhenSkipped = false;
		_hasVanillaCutsceneBeenTriggered = CutsceneBase.GetIsCutsceneTriggeredByType(ECutsceneType.EmpTower0_Door, _level.GameSave);
	}

	internal override bool AreTriggerConditionsMet()
	{
		return _level.GameSave.GetSaveBool("IsTerrilisDead") || _level.GameSave.GetSaveBool("IsPrinceDead");
	}

	internal override void DoCutscene()
	{
		AddSummonMeyef();
		if (!_hasVanillaCutsceneBeenTriggered)
		{
			AddDialogue("cs_emp_0_lun_00");
			AddDialogue("cs_emp_0_lun_01");
		}
		AddDialogue("cs_emp_4_mey_00");
		AddDialogue("cs_emp_4_lun_01");
		AddDismissMeyef();
	}
}
