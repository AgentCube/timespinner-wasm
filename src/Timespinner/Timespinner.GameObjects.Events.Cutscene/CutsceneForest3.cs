using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.NPCs;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneForest3 : CutsceneBase
{
	private const int HaristelFinalMoveX = 624;

	private const int NelisteMoveX = 944;

	private const int FloorY = 368;

	private const float HaristelFinalMoveTime = 3f;

	private const float HaristelFinalMoveSleepTime = 0.2f;

	private static readonly Point HaristelStart = new Point(1200, 368);

	private static readonly Point NelisteStart = new Point(720, 368);

	private readonly CaptainNPC _haristel;

	private bool _isSceneOver;

	private bool _hasWarpedHaristel;

	private NPCBase _neliste;

	private ScriptAction _haristelEndWalkScript;

	public CutsceneForest3(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.WallTrigger;
		_haristel = new CaptainNPC(_level, HaristelStart, -1, new ObjectTileSpecification(487)
		{
			Argument = 3
		})
		{
			IsSpawnedForCutscene = true
		};
	}

	internal override void DoCutscene()
	{
		_level.RequestAddObject(_haristel);
		_neliste = CutsceneBase.GrabOrCreateNPC(_level, NPCBase.ENPCType.Astrologer, NelisteStart);
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.LookDirection,
			ActionTimer = 0.25f,
			DoesBlockQueue = true,
			Arguments = new Vector4(1f, 0f, 0f, 0f)
		});
		AddDialogue("cs_for_1_voi_00");
		ScriptAction scriptAction = new ScriptAction(EScriptActionType.GoToPoint, 0.2f, 3f, new Vector4(1056f, 0f, 0f, 0f));
		scriptAction.ScriptTarget = _haristel;
		scriptAction.TargetType = EScriptTargetType.Specified;
		ScriptAction inAction = scriptAction;
		AddLevelScriptAction(inAction);
		AddDialogue("cs_for_1_sol_01");
		AddDialogue("cs_for_1_lun_02");
		AddDialogue("cs_for_1_sol_03");
		ScriptAction scriptAction2 = new ScriptAction(EScriptActionType.GoToPoint, 0.2f, 3f, new Vector4(944f, 0f, 0f, 0f));
		scriptAction2.ScriptTarget = _neliste;
		scriptAction2.TargetType = EScriptTargetType.Specified;
		ScriptAction inAction2 = scriptAction2;
		AddLevelScriptAction(inAction2);
		AddDialogue("cs_for_1_nel_04");
		AddDialogue("cs_for_1_nel_05");
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.FancyIdle,
			DoesBlockQueue = false
		});
		AddDialogue("cs_for_1_har_06");
		AddDialogue("cs_for_1_lun_07");
		AddDialogue("cs_for_1_har_08");
		AddDialogue("cs_for_1_lun_09");
		AddDialogue("cs_for_1_har_10");
		AddDialogue("cs_for_1_lun_11");
		AddDialogue("cs_for_1_har_12");
		AddDialogue("cs_for_1_har_13");
		AddDialogue("cs_for_1_nel_14");
		AddDialogue("cs_for_1_har_15");
		AddDialogue("cs_for_1_har_16");
		AddDialogue("cs_for_1_har_17");
		_level.GameSave.SetValue(NPCBase.GetIsNPCUnlockedKeyFromType(NPCBase.ENPCType.Captain), value: true);
		AddLevelScriptAction(new ScriptAction
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = _neliste,
			ActionType = EScriptActionType.WarpToPoint,
			Arguments = new Vector4(944f, 368f, 0f, 0f)
		});
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = _neliste,
			ActionType = EScriptActionType.LookDirection,
			Arguments = new Vector4(1f, 0f, 0f, 0f)
		});
		_haristelEndWalkScript = new ScriptAction(EScriptActionType.GoToPoint, 0.2f, 3f, new Vector4(624f, 0f, 0f, 0f))
		{
			ScriptTarget = _haristel,
			TargetType = EScriptTargetType.Specified,
			DoesBlockQueue = false
		};
		AddLevelScriptAction(_haristelEndWalkScript);
		AddDelegateScript(FlagEndOfScene);
	}

	private void FlagEndOfScene()
	{
		_isSceneOver = true;
	}

	public override void Update(float delta)
	{
		if (_isSceneOver && !base.IsFrozen && !_hasWarpedHaristel && _haristelEndWalkScript != null)
		{
			if (_haristelEndWalkScript.IsBeingSkipped)
			{
				_haristel.Position = new Point(624, 368);
			}
			_hasWarpedHaristel = true;
		}
		base.Update(delta);
	}
}
