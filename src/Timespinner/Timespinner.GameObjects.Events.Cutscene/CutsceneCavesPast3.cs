using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneCavesPast3 : CutsceneBase
{
	private const int SuckOffsetY = -32;

	public CutsceneCavesPast3(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.IsWarpingAtEndOfCutscene = true;
	}

	internal override void DoCutscene()
	{
		Protagonist mainHero = _level.MainHero;
		bool flag = mainHero.Position.X > 160;
		_level.RequestScreenShake(new Vector2(2f, 0f), 3f, 10f, isAffectedByTime: true);
		AddScript(new ScriptAction(ESFX.BossMawVacuum, new Point(400, 120)));
		AddScript(new ScriptAction
		{
			ScriptType = EScriptType.Delegate,
			SleepTime = 0.55f,
			Delegate = delegate
			{
				_level.JukeBox.FadeOutSong(5f);
			}
		});
		if (!flag)
		{
			_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.Idle, 0f, 0.75f, Vector4.Zero)
			{
				DoesBlockQueue = true
			});
		}
		else
		{
			_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.Run, 0f, 0.75f, new Vector4(-1f, 0f, 0f, 0f))
			{
				DoesBlockQueue = true
			});
		}
		AddLevelScriptAction(new ScriptAction(new AnimationSpec
		{
			Start = 216,
			Length = 2,
			Speed = 0.066f,
			Type = EAnimationType.Cycle
		}, mainHero));
		AddLevelScriptAction(new ScriptAction
		{
			ScriptType = EScriptType.FadeInFadeOut,
			Arguments = new Vector4(1f, 0.5f, 0f, 1f)
		});
		AddLevelScriptAction(new ScriptAction(EScriptActionType.MawDoorSuck, 0f, 1f, new Vector4(mainHero.Position.X, mainHero.Position.Y, Position.X, Position.Y + -32))
		{
			TargetType = EScriptTargetType.Player1,
			DoesBlockQueue = false
		});
		AddUnskippableWaitScript(1f);
		FadeOut(0.4f, 0.2f, 0f);
		AddDelegateScript(delegate
		{
			_level.RequestChangeRoom(new LevelChangeRequest
			{
				RoomID = 7,
				EnterDirection = EDirection.East
			});
		});
	}
}
