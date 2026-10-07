using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutscenePrologue5 : CutsceneBase
{
	private const int PlayerOffsetX = 28;

	public CutscenePrologue5(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.WallTrigger;
	}

	internal override void DoCutscene()
	{
		MovePlayerToPosition(new Point(Position.X + 28, Position.Y), shouldFaceLeftAfter: false, shouldStandFancyAfter: true);
		AddDialogue("cs_pros_sel_00");
		AddDialogue("cs_pros_lun_01");
		AddDialogue("cs_pros_sel_02");
		AddDialogue("cs_pros_lun_03");
		AddDialogue("cs_pros_sel_04");
		AddDialogue("cs_pros_lun_05");
		AddDialogue("cs_pros_sel_06");
		AddDialogue("cs_pros_sel_07");
		TeleportToLevelAndRoom(0, 4, ECutsceneType.Prologue4_TutorialM);
	}
}
