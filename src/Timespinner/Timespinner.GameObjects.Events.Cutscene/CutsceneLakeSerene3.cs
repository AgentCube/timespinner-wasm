using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneLakeSerene3 : CutsceneBase
{
	public CutsceneLakeSerene3(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.WallTrigger;
		base.DoesFadeOutWhenSkipped = false;
	}

	internal override bool AreTriggerConditionsMet()
	{
		return _level.GameSave.GetSaveBool("IsVileteSaved");
	}

	internal override void DoCutscene()
	{
		AddDialogue("cs_vil_lun_00");
		AddDialogue("cs_vil_lun_01");
		if (_level.GameSave.GetSaveBool(BossClass.GetSaveKeyByBossType(EBossType.Sorceress)))
		{
			AddDialogue("cs_vil_lun_02");
		}
	}
}
