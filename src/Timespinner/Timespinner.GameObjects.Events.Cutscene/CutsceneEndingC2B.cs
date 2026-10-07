using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.NPCs.Misc;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneEndingC2B : CutsceneBase
{
	private const int NPCIndex = 487;

	private const int AelanaStartX = 304;

	private const int AelanaStartY = 160;

	private readonly AelanaNPC _aelana;

	public CutsceneEndingC2B(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		AddLockCamera();
		AddHidePlayer();
		AddCameraPan(new Point(200, 120), 0f, doesBlockQueue: false);
		_aelana = new AelanaNPC(_level, new Point(304, 160), -1, new ObjectTileSpecification(487)
		{
			Argument = 16
		})
		{
			CannotBeTalkedTo = true,
			IsSpawnedForCutscene = true,
			IsFacingLeft = true
		};
		_level.RequestAddObject(_aelana);
	}

	internal override void DoCutscene()
	{
		AddWaitScript(1f);
		AddDialogue("cs_endc_2_ael_06");
		AddWaitScript(0.5f);
		TeleportToLevelAndRoom(17, 11, ECutsceneType.EndingC3_Present1);
	}
}
