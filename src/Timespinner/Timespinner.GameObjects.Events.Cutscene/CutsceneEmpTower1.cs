using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Events.EnvironmentPrefabs;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneEmpTower1 : CutsceneBase
{
	private const int LeaveX = 24;

	private const int RunRightThresholdX = 48;

	private const int RunRightDestinationX = 144;

	private const int ExitX = 96;

	private const int ThroneX = 288;

	private const int FloorY = 208;

	public CutsceneEmpTower1(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.DoesFadeOutWhenSkipped = false;
	}

	internal override void DoCutscene()
	{
		AddPlayerFaceRoomCenter();
		AddWaitScript(0.1f);
		AddSummonMeyef();
		AddMeyefFlyInFrontOfPlayer(0.5f, doesBlock: false);
		Protagonist mainHero = _level.MainHero;
		if (mainHero.Position.X < 48)
		{
			MovePlayerToPosition(new Point(144, 208), shouldFaceLeftAfter: false, shouldStandFancyAfter: false);
		}
		AddDialogue("cs_emp_2_lun_00");
		AddDialogue("cs_emp_2_lun_01");
		AddDialogue("cs_emp_2_mey_02");
		AddDialogue("cs_emp_2_lun_03");
		AddDialogue("cs_emp_2_lun_04");
		AddDialogue("cs_emp_2_lun_05");
		AddDialogue("cs_emp_2_lun_06");
		AddDialogue("cs_emp_2_lun_07");
		AddDialogue("cs_emp_2_lun_08");
		AddDismissMeyef();
		AddDelegateScript(PlaceEndingTriggers);
	}

	private void PlaceEndingTriggers()
	{
		_level.JukeBox.StopSong();
		ObjectTileSpecification objectTileSpecification = new ObjectTileSpecification(491);
		objectTileSpecification.Argument = 1200;
		ObjectTileSpecification objectSpec = objectTileSpecification;
		EnvPrefabEmpTowerThrone newObject = new EnvPrefabEmpTowerThrone(_level, new Point(288, 208), -1, objectSpec, EEnvironmentPrefabType.L12_ThroneTrigger);
		_level.RequestAddObject(newObject);
		ObjectTileSpecification objectTileSpecification2 = new ObjectTileSpecification(491);
		objectTileSpecification2.Argument = 1201;
		ObjectTileSpecification objectSpec2 = objectTileSpecification2;
		EnvPrefabEmpTowerExit newObject2 = new EnvPrefabEmpTowerExit(_level, new Point(96, 208), -1, objectSpec2, EEnvironmentPrefabType.L12_ExitTrigger);
		_level.RequestAddObject(newObject2);
		ObjectTileSpecification objectTileSpecification3 = new ObjectTileSpecification(495);
		objectTileSpecification3.Argument = 123;
		ObjectTileSpecification objectSpec3 = objectTileSpecification3;
		CutsceneBase cutsceneBase = CutsceneBase.FromSpecification(_level, new Point(24, 208), -1, objectSpec3);
		cutsceneBase.Initialize();
		_level.RequestAddObject(cutsceneBase);
	}
}
