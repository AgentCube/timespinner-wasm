using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneForest0 : CutsceneBase
{
	private SFXCueInstance _windyLoopCue;

	public CutsceneForest0(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.DoesMakePlayerIdleAtStart = false;
		AddHideFamiliar(0f);
		_level.IsMufflingPlayerSFX = true;
	}

	internal override void DoCutscene()
	{
		_windyLoopCue = PlayCue(ESFX.CsFallingWind, Position, isLooped: true);
		if (_windyLoopCue != null)
		{
			_windyLoopCue.Anchor = _level.MainHero;
			_windyLoopCue.UpdateType = SFXCueInstance.ECueInstanceUpdateType.Anchor;
		}
		AddUnskippableWaitScript(4.3f);
		FadeOut(0.5f, 3.5f, 0f);
		AddDelegateScript(DoFadeWind);
		AddUnskippablePlaySFX(ESFX.CsBranchesSplash);
		AddUnskippableWaitScript(1.5f);
		AddUnskippablePlaySFX(ESFX.FoleyWaterSplashDeep);
		AddUnskippableWaitScript(1f);
		TeleportToLevelAndRoom(3, 11, ECutsceneType.Forest1_Pond);
	}

	private void AddUnskippablePlaySFX(ESFX sfx)
	{
		AddLevelScriptAction(new ScriptAction(sfx, new Point(-1, -1))
		{
			IsUnskippable = true
		});
	}

	private void DoFadeWind()
	{
		if (_windyLoopCue != null)
		{
			_windyLoopCue.Stop(1f);
		}
	}
}
