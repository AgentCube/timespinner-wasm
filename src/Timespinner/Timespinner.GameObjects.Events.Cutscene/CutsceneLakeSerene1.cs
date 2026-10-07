using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneLakeSerene1 : CutsceneBase
{
	private const int SeykisTalkX = 160;

	private const int SeykisTalkY = 224;

	private readonly int _roomWidth;

	private NPCBase _seykis;

	public CutsceneLakeSerene1(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		_roomWidth = _level.RoomSize.X;
	}

	internal override void DoCutscene()
	{
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 65;
		animationSpec.Length = 4;
		animationSpec.Type = EAnimationType.Once;
		animationSpec.Speed = 0.15f;
		AnimationSpec newAnim = animationSpec;
		AnimationSpec animationSpec2 = new AnimationSpec();
		animationSpec2.Start = 50;
		animationSpec2.Length = 5;
		animationSpec2.Type = EAnimationType.Cycle;
		animationSpec2.Speed = 0.125f;
		AnimationSpec newAnim2 = animationSpec2;
		AddWaitScript(0.5f);
		AddDelegateScript(DoMiniBossOpenDoors);
		_seykis = CutsceneBase.GrabNPC(_level, NPCBase.ENPCType.Quartermaster);
		AddLevelScriptAction(new ScriptAction(EScriptActionType.ChangeColor, 0f, 0.5f, new Vector4(0.75f, 1f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = _seykis
		});
		AddScript(new ScriptAction(newAnim, _seykis)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim2, _seykis));
		MovePlayerToTalk();
		AddDialogue("cs_lse_0_sol_01");
		AddDialogue("cs_lse_0_lun_02");
		AddDialogue("cs_lse_0_sey_03");
		AddDialogue("cs_lse_0_lun_04");
		AddDialogue("cs_lse_0_sey_05");
		ScriptAction scriptAction = new ScriptAction(EScriptActionType.GoToPoint, 0f, 3f, new Vector4(_roomWidth + 32, 0f, 0f, 0f));
		scriptAction.ScriptTarget = _seykis;
		scriptAction.TargetType = EScriptTargetType.Specified;
		ScriptAction newScript = scriptAction;
		AddScript(newScript);
	}

	private void DoMiniBossOpenDoors()
	{
		_level.TogglePlayerIsInvulnerable(isInvulnerable: false);
		_level.OpenAllBossDoors(1f);
		_level.ToggleExits(isEnabled: true);
	}

	private void MovePlayerToTalk()
	{
		Point playerPosition = _level.GetPlayerPosition();
		Point point = new Point(160, 224);
		bool flag = playerPosition.X > point.X;
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.StopCast
		});
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.GoToPoint,
			ActionTimer = 1f,
			DoesBlockQueue = true,
			Arguments = new Vector4(point.X, point.Y, 0f, 0f)
		});
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.Idle,
			ActionTimer = 0.1f,
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.FancyIdle,
			DoesBlockQueue = false
		});
		if (flag)
		{
			AddScript(new ScriptAction
			{
				TargetType = EScriptTargetType.Player1,
				ActionType = EScriptActionType.LookDirection,
				ActionTimer = 0.25f,
				DoesBlockQueue = true,
				Arguments = new Vector4(1f, 0f, 0f, 0f)
			});
		}
	}
}
