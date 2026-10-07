using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.EnvironmentPrefabs;
using Timespinner.GameObjects.Events.EnvironmentPrefabs.L11_Lab;
using Timespinner.GameObjects.Events.Relics;

namespace Timespinner.GameObjects.Events.Cutscene;

internal sealed class CutsceneLab2 : CutsceneBase
{
	private const int EnvPrefabID = 491;

	private const int WinderiaPromptArgument = 1119;

	private const int ViletePromptArgument = 1120;

	private const int WinderiaPlacementX = 120;

	private const int ViletePlacementX = 280;

	private const int PlacementY = 224;

	public CutsceneLab2(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.WallTrigger;
		base.DoesFadeOutWhenSkipped = false;
		base.WallTriggerWidth = 48;
	}

	internal override bool AreTriggerConditionsMet()
	{
		bool saveBool = _level.GameSave.GetSaveBool(BossClass.GetSaveKeyByBossType(EBossType.Shapeshift));
		int collectedTimespinnerGearCount = TimespinnerGearItem.GetCollectedTimespinnerGearCount(_level.GameSave);
		if (saveBool)
		{
			return collectedTimespinnerGearCount >= 3;
		}
		return false;
	}

	internal override void DoCutscene()
	{
		AddSummonMeyef();
		AddMeyefFlyInFrontOfPlayer(0.5f, doesBlock: false);
		AddDialogue("cs_labts_1_lun_00");
		AddDialogue("cs_labts_1_lun_01");
		AddDialogue("cs_labts_1_lun_02");
		AddDialogue("cs_labts_1_lun_03");
		AddDialogue("cs_labts_1_lun_04");
		AddDialogue("cs_labts_1_lun_05");
		AddDialogue("cs_labts_1_lun_06");
		AddDialogue("cs_labts_1_lun_07");
		AddDialogue("cs_labts_1_mey_08");
		AddDialogue("cs_labts_1_lun_09");
		AddDialogue("cs_labts_1_mey_10");
		AddDialogue("cs_labts_1_mey_11");
		AddDialogue("cs_labts_1_mey_12");
		AddDialogue("cs_labts_1_lun_13");
		AddDialogue("cs_labts_1_mey_14");
		AddDialogue("cs_labts_1_mey_15");
		AddDialogue("cs_labts_1_lun_16");
		AddDialogue("cs_labts_1_lun_17");
		_level.GameSave.SetValue("IsLabTSReady", value: true);
		AddDelegateScript(AddWarps);
		AddDismissMeyef();
	}

	private void AddWarps()
	{
		EnvPrefabLabWinderia newObject = new EnvPrefabLabWinderia(_level, new Point(120, 224), -1, new ObjectTileSpecification(491)
		{
			Argument = 1119
		}, EEnvironmentPrefabType.L11_SwitchWinderia);
		EnvPrefabLabVilete newObject2 = new EnvPrefabLabVilete(_level, new Point(280, 224), -1, new ObjectTileSpecification(491)
		{
			Argument = 1120
		}, EEnvironmentPrefabType.L11_SwitchVilete);
		_level.RequestAddObject(newObject);
		_level.RequestAddObject(newObject2);
	}
}
