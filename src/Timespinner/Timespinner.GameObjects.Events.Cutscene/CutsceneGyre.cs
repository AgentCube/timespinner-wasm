using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneGyre : CutsceneBase
{
	public CutsceneGyre(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.WallTrigger;
		base.DoesFadeOutWhenSkipped = false;
	}

	internal override void DoCutscene()
	{
		Protagonist mainHero = _level.MainHero;
		bool isFacingLeft = mainHero.IsFacingLeft;
		Point position = mainHero.Position;
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.FancyIdle,
			DoesBlockQueue = false
		});
		AddSummonMeyef();
		AddMeyefFlyAround(new Point(position.X + (isFacingLeft ? (-64) : 64), position.Y - 40), 3f, 12f);
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Familiar,
			ScriptType = EScriptType.Action,
			ActionType = EScriptActionType.LookDirection,
			Arguments = new Vector4(isFacingLeft ? 1 : (-1), 0f, 0f, 0f)
		});
		AddDialogue("cs_gyre_lun_00");
		AddDialogue("cs_gyre_mey_01");
		AddDialogue("cs_gyre_mey_02");
		AddDialogue("cs_gyre_lun_03");
		AddDialogue("cs_gyre_mey_04");
		AddDialogue("cs_gyre_lun_05");
		AddDialogue("cs_gyre_mey_06");
		AddDismissMeyef();
	}
}
