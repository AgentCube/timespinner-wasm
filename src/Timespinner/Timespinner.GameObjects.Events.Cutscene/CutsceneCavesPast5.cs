using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneCavesPast5 : CutsceneBase
{
	public CutsceneCavesPast5(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.IsWarpingAtEndOfCutscene = true;
	}

	internal override void DoCutscene()
	{
		Protagonist mainHero = _level.MainHero;
		AddScript(new ScriptAction(ESFX.BossMawRoar, new Point(0, 120)));
		AddScript(new ScriptAction(ESFX.BossMawVacuum, new Point(0, 120)));
		AddLevelScriptAction(new ScriptAction(new AnimationSpec
		{
			Start = 214,
			Length = 2,
			Speed = 0.066f,
			Type = EAnimationType.Cycle
		}, mainHero));
		AddUnskippableWaitScript(1.25f);
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
		AddLevelScriptAction(new ScriptAction(EScriptActionType.MawDoorSuck, 0f, 1f, new Vector4(mainHero.Position.X, mainHero.Position.Y, Position.X, Position.Y))
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
				RoomID = 13,
				EnterDirection = EDirection.West,
				CutsceneToCall = ECutsceneType.CavesPast6_MawBoom,
				ShouldPlayLevelSong = true
			});
		});
	}
}
