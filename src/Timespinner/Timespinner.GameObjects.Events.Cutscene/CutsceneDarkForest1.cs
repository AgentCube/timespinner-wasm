using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Events.EnvironmentPrefabs;
using Timespinner.GameObjects.Events.EnvironmentPrefabs.L15_DarkForest;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneDarkForest1 : CutsceneBase
{
	private const int SandmanX = 688;

	private const int SandmanY = 192;

	private const int CameraPanY = 120;

	private const int EnvPrefabID = 491;

	private const int SandShadeArgument = 1502;

	private readonly EnvPrefabDarkForestShade _sandmanShade;

	public CutsceneDarkForest1(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.WallTrigger;
		base.DoesFadeOutWhenSkipped = false;
		_sandmanShade = new EnvPrefabDarkForestShade(_level, new Point(688, 192), -1, new ObjectTileSpecification(491)
		{
			Argument = 1502
		}, EEnvironmentPrefabType.L15_SandShade);
		_sandmanShade.Initialize();
	}

	internal override void DoCutscene()
	{
		int x = _level.RoomSize.X - 200;
		AddLockCamera();
		AddCameraPan(new Point(x, 120), 1f, doesBlockQueue: false);
		AddWaitScript(0.5f);
		PlayCue(ESFX.BossSandmanAppear, _sandmanShade.Position);
		_level.RequestAddObject(_sandmanShade);
		AddSummonMeyef();
		AddGhostDialogue("cs_dfo_1_san_00");
		AddMeyefMew();
		AddGhostDialogue("cs_dfo_1_san_01");
		AddDialogue("cs_dfo_1_mey_02");
		AddGhostDialogue("cs_dfo_1_san_03");
		AddDialogue("cs_dfo_1_lun_04");
		AddGhostDialogue("cs_dfo_1_san_05");
		AddDialogue("cs_dfo_1_lun_06");
		AddGhostDialogue("cs_dfo_1_san_07");
		AddDialogue("cs_dfo_1_lun_08");
		AddGhostDialogue("cs_dfo_1_san_09");
		AddDelegateScript(FadeSandShade);
		AddCameraPan(new Point(Position.X, 120), 0.5f, doesBlockQueue: true);
		AddUnlockCamera();
		AddDismissMeyef();
	}

	private void FadeSandShade()
	{
		_level.PlayCue(ESFX.BossSandmanDisappear, _sandmanShade.Position);
		_sandmanShade.FadeOut();
	}
}
