using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Events.EnvironmentPrefabs;
using Timespinner.GameObjects.Events.EnvironmentPrefabs.L11_Lab;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneEndingA4 : CutsceneBase
{
	private const int EmpireBroochKey = 21;

	private const int EternalBroochKey = 24;

	private const int EnvPrefabID = 491;

	private const int TubeArgument = 1705;

	private const int TubeX = 392;

	private const int TubeY = 128;

	private const int LunaisWalkX = 376;

	private const int LunaisWalkY = 144;

	private readonly EnvPrefabLabTube _tube;

	public CutsceneEndingA4(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		AddLockCamera();
		AddUnhidePlayer();
		AddCameraPan(new Point(328, 120), 0f, doesBlockQueue: false);
		_tube = new EnvPrefabLabTube(_level, new Point(392, 128), -1, new ObjectTileSpecification(491)
		{
			Argument = 1705
		}, EEnvironmentPrefabType.L17_TubeLargeChild);
		_tube.Initialize();
		_level.RequestAddObject(_tube);
		InventoryRelicCollection relicInventory = _level.GameSave.Inventory.RelicInventory;
		relicInventory.AddItem(21);
		_level.GameSave.Inventory.RelicInventory.Inventory[21].IsActive = true;
		if (relicInventory.Inventory.ContainsKey(24))
		{
			relicInventory.Inventory[24].IsActive = false;
		}
		if (_level.MainHero != null)
		{
			_level.MainHero.RefreshStats(_level.GameSave);
		}
	}

	internal override void DoCutscene()
	{
		AddHideFamiliar(0f);
		AddPlayerFaceRoomCenter();
		AddDialogue("cs_enda_4_lun_00");
		AddDialogue("cs_enda_4_lun_01");
		MovePlayerToPosition(new Point(376, 144), shouldFaceLeftAfter: false, shouldStandFancyAfter: true);
		PlayScriptedSFX(ESFX.ItemGetDownload, new Point(376, 144));
		AddDelegateScript(KillTube);
		AddAutoplayGhostDialogue("cs_enda_4_lun_02");
		AddSepiaFade(5f, doesBlock: false);
		AddAutoplayGhostDialogue("cs_enda_4_lun_03");
		AddAutoplayGhostDialogue("cs_enda_4_lun_04");
		AddDelegateScript(base.StartLevelFadeOut);
		AddAutoplayGhostDialogue("cs_enda_4_lun_05");
		AddAutoplayGhostDialogue("cs_enda_4_lun_06");
		AddEndSepiaFade(0f, doesBlock: true);
		AddSongFadeOut(3f, doesBlock: false);
		AddWaitScript(2f);
		TeleportToLevelAndRoom(17, 4, ECutsceneType.EndingA5_Winderia0);
	}

	private void KillTube()
	{
		_tube.SetCharacterSequenceByName("Die");
	}
}
