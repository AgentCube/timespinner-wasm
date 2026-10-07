using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Relics;

namespace Timespinner.GameObjects.Events.Cutscene;

internal sealed class CutsceneLab1 : CutsceneBase
{
	private readonly bool _isGenzaDead;

	public CutsceneLab1(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.WallTrigger;
		base.DoesFadeOutWhenSkipped = false;
		base.WallTriggerWidth = 48;
		_isGenzaDead = _level.GameSave.GetSaveBool(BossClass.GetSaveKeyByBossType(EBossType.Shapeshift));
		if (_isGenzaDead)
		{
			_level.SetIsOverridingPowerOff(newValue: true);
		}
	}

	internal override bool AreTriggerConditionsMet()
	{
		int collectedTimespinnerGearCount = TimespinnerGearItem.GetCollectedTimespinnerGearCount(_level.GameSave);
		if (_isGenzaDead && collectedTimespinnerGearCount > 0)
		{
			return collectedTimespinnerGearCount < 3;
		}
		return false;
	}

	internal override void DoCutscene()
	{
		AddDialogue("cs_labts_0_lun_00");
		AddDialogue("cs_labts_0_lun_01");
		AddDialogue("cs_labts_0_lun_02");
	}
}
