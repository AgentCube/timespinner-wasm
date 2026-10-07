using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneHaristelMaw : CutsceneBase
{
	private const int HaristelMoveLeftX = 248;

	private const int HaristelMoveRightX = 992;

	private const int HaristelStartLeftX = 416;

	private const int HaristelStartRightX = 768;

	private const int PlayerStartLeftX = 200;

	private const int PlayerStartRightX = 1064;

	private const int RoomCenter = 576;

	private const int FloorY = 368;

	private const int HaristelFinalMoveX = 552;

	private const float HaristelFinalMoveTime = 3f;

	private bool _isSceneOver;

	private bool _hasWarpedHaristel;

	private ScriptAction _haristelEndWalkScript;

	private NPCBase _haristel;

	public CutsceneHaristelMaw(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Instant;
	}

	internal override bool AreTriggerConditionsMet()
	{
		bool saveBool = _level.GameSave.GetSaveBool($"IsBossDead_{EBossType.Maw}");
		bool saveBool2 = _level.GameSave.GetSaveBool("IsDoingRamedaFoundCutscene");
		if (saveBool)
		{
			return !saveBool2;
		}
		return false;
	}

	internal override void DoCutscene()
	{
		Protagonist mainHero = _level.MainHero;
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 8;
		animationSpec.Length = 5;
		animationSpec.Speed = 0.15f;
		animationSpec.Type = EAnimationType.Cycle;
		AnimationSpec newAnim = animationSpec;
		AnimationSpec animationSpec2 = new AnimationSpec();
		animationSpec2.Start = 237;
		animationSpec2.Length = 3;
		animationSpec2.Speed = 0.15f;
		animationSpec2.Type = EAnimationType.PingPong;
		AnimationSpec newAnim2 = animationSpec2;
		bool flag = mainHero.Position.X < 576;
		int x = (flag ? 416 : 768);
		int num = (flag ? 248 : 992);
		int x2 = (flag ? 200 : 1064);
		_haristel = CutsceneBase.GrabOrCreateNPC(_level, NPCBase.ENPCType.Captain, new Point(x, 368));
		ScriptAction scriptAction = new ScriptAction(EScriptActionType.GoToPoint, 0f, 3f, new Vector4(num, 368f, 0f, 0f));
		scriptAction.ScriptTarget = _haristel;
		scriptAction.TargetType = EScriptTargetType.Specified;
		scriptAction.DoesBlockQueue = false;
		ScriptAction inAction = scriptAction;
		MovePlayerToPosition(new Point(x2, 368), !flag, shouldStandFancyAfter: true);
		AddLevelScriptAction(inAction);
		AddDialogue("cs_har_maw_har_00");
		AddDialogue("cs_har_maw_lun_01");
		AddDialogue("cs_har_maw_har_02");
		AddScript(new ScriptAction(newAnim2, mainHero)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim, mainHero)
		{
			DoesBlockQueue = false
		});
		AddDialogue("cs_har_maw_lun_03");
		AddDialogue("cs_har_maw_lun_04");
		AddWaitScript(1f);
		AddDialogue("cs_har_maw_har_05");
		AddDialogue("cs_har_maw_har_06");
		_haristelEndWalkScript = new ScriptAction(EScriptActionType.GoToPoint, 0f, 3f, new Vector4(552f, 0f, 0f, 0f))
		{
			ScriptTarget = _haristel,
			TargetType = EScriptTargetType.Specified,
			DoesBlockQueue = false
		};
		AddLevelScriptAction(_haristelEndWalkScript);
		ScriptAction scriptAction2 = new ScriptAction(EScriptActionType.LookDirection, 5f, 0.1f, new Vector4(1f, 0f, 0f, 0f));
		scriptAction2.ScriptTarget = _haristel;
		scriptAction2.TargetType = EScriptTargetType.Specified;
		scriptAction2.DoesBlockQueue = false;
		ScriptAction inAction2 = scriptAction2;
		AddLevelScriptAction(inAction2);
		AddWaitScript(0.25f);
		AddDialogue("cs_har_maw_lun_07");
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
				_haristel.Position = new Point(552, 368);
				_haristel.IsFacingLeft = false;
			}
			_hasWarpedHaristel = true;
		}
		base.Update(delta);
	}
}
