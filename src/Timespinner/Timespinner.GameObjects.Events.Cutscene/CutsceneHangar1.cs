using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneHangar1 : CutsceneBase
{
	public CutsceneHangar1(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.DoesHideOrbsAutomatically = false;
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.DoesFadeOutWhenSkipped = false;
	}

	internal override void DoCutscene()
	{
		AddUnskippableWaitScript(2f);
		AddScript(new ScriptAction(EScriptActionType.Run, 0f, 2f, new Vector4(1f, 0f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Player1
		});
		AddUnskippableWaitScript(2f);
		CutsceneBase.CreateAndCallCutscene(ECutsceneType.Hangar0_Blocked, _level, Position);
	}
}
