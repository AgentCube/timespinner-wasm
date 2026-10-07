using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneCavesPast2 : CutsceneBase
{
	public CutsceneCavesPast2(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Instant;
		base.DoesFadeOutWhenSkipped = false;
	}

	internal override void DoCutscene()
	{
		AddScript(new ScriptAction(EScriptActionType.Run, 0f, 0.5f, new Vector4(1f, 0f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Player1
		});
		AddDialogue("cs_maw_0_lun_00");
		AddDialogue("cs_maw_0_lun_01");
	}
}
