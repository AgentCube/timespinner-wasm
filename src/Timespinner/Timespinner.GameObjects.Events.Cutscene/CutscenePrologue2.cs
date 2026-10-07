using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.NPCs;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutscenePrologue2 : CutsceneBase
{
	private const int StopY = 192;

	private const int SelenStartX = 336;

	private const int SelenStopX = 672;

	private const int LunaisStopX = 544;

	private const int EndX = 784;

	private readonly SelenNPC _selen;

	public CutscenePrologue2(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		_selen = new SelenNPC(_level, new Point(336, 192), -1);
		_selen.ChangeNPCAIType(NPCBase.ENPCAIType.None);
		_level.RequestAddObject(_selen);
		_level.GameSave.SetValue("IsJianaShocked", value: false);
	}

	public override void Initialize()
	{
		base.Initialize();
		_selen.Initialize();
	}

	internal override void DoCutscene()
	{
		MoveCharacterToPosition(_selen, new Point(672, 192), doesBlockQueue: false, 0f);
		MovePlayerToPosition(new Point(544, 192), shouldFaceLeftAfter: true, shouldStandFancyAfter: false);
		AddDialogue("cs_pro_2_lun_00");
		AddScript(new ScriptAction
		{
			ScriptTarget = _selen,
			TargetType = EScriptTargetType.Specified,
			ActionType = EScriptActionType.Run,
			ActionTimer = 0.25f,
			DoesBlockQueue = true,
			Arguments = new Vector4(-1f, 0f, 0f, 0f)
		});
		AddDialogue("cs_pro_2_sel_01");
		AddScript(new ScriptAction(EScriptActionType.Run, 0f, 0.15f, new Vector4(1f, 0f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Player1
		});
		AddDialogue("cs_pro_2_lun_02");
		AddDialogue("cs_pro_2_sel_03");
		AddDialogue("cs_pro_2_lun_04");
		AddDialogue("cs_pro_2_sel_05");
		MoveCharacterToPosition(targetPosition: new Point(784, 192), character: _selen, doesBlockQueue: false, sleepTime: 0f);
		AddScript(new ScriptAction(EScriptActionType.Run, 0f, 1.25f, new Vector4(1f, 0f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Player1
		});
		AddWaitScript(0.5f);
		TeleportToLevelAndRoom(0, 2, ECutsceneType.Prologue3_Temple);
	}
}
