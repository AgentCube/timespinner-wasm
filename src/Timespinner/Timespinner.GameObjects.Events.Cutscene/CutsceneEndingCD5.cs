using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Gameplay.Scripts;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneEndingCD5 : CutsceneBase
{
	private const int RoomCenterY = 128;

	private const int MeyefStartX = 432;

	private const int MeyefStartY = 112;

	private const int MewX = 384;

	private const int MewY = 128;

	private const int MeyefFly1X = 204;

	private const int MeyefFly1Y = 128;

	private const int MeyefFly2X = -32;

	private const int MeyefFly2Y = 32;

	public CutsceneEndingCD5(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
	}

	internal override void DoCutscene()
	{
		AddScript(new ScriptAction
		{
			ScriptType = EScriptType.HideShowPlayer,
			Arguments = new Vector4(0f, 1f, 0f, 0f)
		});
		AddSummonMeyef();
		AddMeyefFlyTo(new Point(432, 112), 0f, doesBlock: true);
		AddWaitScript(0.5f);
		PlayScriptedSFX(ESFX.MeyefMeow, new Point(384, 128));
		AddWaitScript(1f);
		AddUnhideFamiliar();
		AddMeyefFlyTo(new Point(204, 128), 2f, doesBlock: false);
		AddWaitScript(3f);
		AddMeyefPurr();
		AddWaitScript(0.75f);
		AddScript(new FamiliarScript(FamiliarScript.EFamiliarScriptType.CutsceneFlyTo)
		{
			ActionTimer = 1.5f,
			Arguments = new Vector4(-32f, 32f, 0f, 1f),
			DoesBlockQueue = false
		});
		AddWaitScript(0.5f);
		AddHideFamiliar(1f);
		AddWaitScript(2.5f);
		AddSongFadeOut(5f, doesBlock: true);
		AddWaitScript(1.5f);
		AddRollCredits();
	}
}
