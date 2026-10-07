using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.EnvironmentPrefabs;
using Timespinner.GameObjects.Events.Misc;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.Cutscene;

internal sealed class CutsceneCavesPast6 : CutsceneBase
{
	private const int EmissionX = 380;

	private const int EmissionY = 112;

	private const int SpitStartX = 368;

	private const int SpitStartY = 112;

	private const int SpitEndX = 176;

	private const int SpitEndY = 192;

	private const int WalkEndX = 288;

	private const float StandAnimationSpeed = 0.125f;

	private readonly Point _explosionSoundPosition;

	private readonly Vector2 _smokeEmissionPosition;

	private readonly Vector2 _burstEmissionPosition;

	private readonly MawExplosionSmokeParticleSystem _smokeParticles;

	private readonly MawExplosionBurstParticleSystem _burstParticles;

	private EnvPrefabCavesMawDoor _mawDoor;

	public CutsceneCavesPast6(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.CutsceneDisappearType = ECutsceneDisappearType.Never;
		base.DoesFadeOutWhenSkipped = false;
		_level.RequestScreenFadeOut(0f, 0.25f, 0.25f, 0f);
		_smokeEmissionPosition = new Vector2(380f, 112f);
		_burstEmissionPosition = new Vector2(_smokeEmissionPosition.X - 16f, _smokeEmissionPosition.Y);
		_explosionSoundPosition = new Point(380, 112);
		_smokeParticles = new MawExplosionSmokeParticleSystem(_level.GCM.TxParticleSmoke, 1, isExplodingToTheLeft: true);
		_burstParticles = new MawExplosionBurstParticleSystem(_level.GCM.TxParticleSmoke, 1, isExplodingToTheLeft: true);
		_particleSystems.Add(_burstParticles);
		_particleSystems.Add(_smokeParticles);
	}

	public override void Initialize()
	{
		IEnumerable<GameEvent> eventAllEventsOfType = _level.GetEventAllEventsOfType(EEventTileType.EnvironmentPrefab);
		foreach (GameEvent item in eventAllEventsOfType)
		{
			_mawDoor = item as EnvPrefabCavesMawDoor;
			if (_mawDoor != null)
			{
				_mawDoor.HideBones();
				break;
			}
		}
		base.Initialize();
	}

	internal override void DoCutscene()
	{
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 205;
		animationSpec.Length = 2;
		animationSpec.Speed = 0.125f;
		animationSpec.Type = EAnimationType.Once;
		AnimationSpec newAnim = animationSpec;
		AnimationSpec animationSpec2 = new AnimationSpec();
		animationSpec2.Start = 212;
		animationSpec2.Length = 2;
		animationSpec2.Speed = 0.125f;
		animationSpec2.Type = EAnimationType.Once;
		AnimationSpec newAnim2 = animationSpec2;
		AnimationSpec animationSpec3 = new AnimationSpec();
		animationSpec3.Start = 0;
		animationSpec3.Length = 5;
		animationSpec3.Speed = 0.11f;
		animationSpec3.Type = EAnimationType.Cycle;
		AnimationSpec newAnim3 = animationSpec3;
		AnimationSpec animationSpec4 = new AnimationSpec();
		animationSpec4.Start = 237;
		animationSpec4.Length = 3;
		animationSpec4.Speed = 0.15f;
		animationSpec4.Type = EAnimationType.PingPong;
		AnimationSpec newAnim4 = animationSpec4;
		AnimationSpec animationSpec5 = new AnimationSpec();
		animationSpec5.Start = 233;
		animationSpec5.Length = 3;
		animationSpec5.Speed = 0.15f;
		animationSpec5.Type = EAnimationType.Once;
		AnimationSpec newAnim5 = animationSpec5;
		AnimationSpec animationSpec6 = new AnimationSpec();
		animationSpec6.Start = 235;
		animationSpec6.Length = 2;
		animationSpec6.Speed = 0.15f;
		animationSpec6.Type = EAnimationType.Once;
		AnimationSpec newAnim6 = animationSpec6;
		AnimationSpec animationSpec7 = new AnimationSpec();
		animationSpec7.Start = 8;
		animationSpec7.Length = 5;
		animationSpec7.Speed = 0.15f;
		animationSpec7.Type = EAnimationType.Cycle;
		AnimationSpec newAnim7 = animationSpec7;
		Protagonist mainHero = _level.MainHero;
		AddLevelScriptAction(new ScriptAction
		{
			ScriptType = EScriptType.FadeInFadeOut,
			Arguments = new Vector4(0f, 1f, 0.5f, 1f),
			DoesBlockQueue = false
		});
		AddLevelScriptAction(new ScriptAction
		{
			ScriptType = EScriptType.FadeInFadeOut,
			Arguments = new Vector4(0f, 0.5f, 0.5f, 0f)
		});
		AddUnskippableWaitScript(1f);
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.MawDoorSpit, 0f, 0.45f, new Vector4(368f, 112f, 176f, 192f))
		{
			DoesBlockQueue = false
		});
		AddUnskippableWaitScript(0.9f);
		AddLevelScriptAction(new ScriptAction
		{
			ScriptType = EScriptType.FadeInFadeOut,
			Arguments = new Vector4(0.15f, 1f, 1f, 1f),
			DoesBlockQueue = false
		});
		AddDelegateScript(DoExplosion);
		AddUnskippableWaitScript(0.75f);
		AddDelegateScript(EmitSmoke);
		AddDelegateScript(ShowRubble);
		AddUnskippableWaitScript(2.5f);
		PlayScriptedSFX(ESFX.LunaisStandFromLyingDown, mainHero.Position);
		AddScript(new ScriptAction(newAnim, mainHero)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim2, mainHero)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim3, mainHero)
		{
			DoesBlockQueue = false
		});
		AddWaitScript(0.75f);
		MovePlayerToPosition(new Point(288, 192), shouldFaceLeftAfter: false, shouldStandFancyAfter: true);
		AddScript(new ScriptAction(newAnim4, mainHero)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim5, mainHero)
		{
			DoesBlockQueue = true
		});
		AddDialogue("cs_maw_2_lun_00");
		AddDialogue("cs_maw_2_lun_01");
		AddScript(new ScriptAction(newAnim6, mainHero)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(newAnim7, mainHero)
		{
			DoesBlockQueue = false
		});
		AddDialogue("cs_maw_2_lun_02");
		AddScript(new ScriptAction(EBGM.CsPortal));
		_level.TogglePlayerIsInvulnerable(isInvulnerable: false);
	}

	private void ShowRubble()
	{
		if (_mawDoor != null)
		{
			_level.GameSave.SetValue("IsVileteSaved", value: true);
			_mawDoor.AddRubble();
		}
	}

	private void DoExplosion()
	{
		_burstParticles.AddParticles(_burstEmissionPosition);
		_level.PlayCue(ESFX.CsExplosionRubble, _explosionSoundPosition);
		_level.AddEvent(new SandStreamerEvent(_level, _burstEmissionPosition.ToPoint(), ESandStreamerType.BossDeath));
		_level.JukeBox.StopSong();
	}

	private void EmitSmoke()
	{
		_smokeParticles.AddParticles(_smokeEmissionPosition);
	}
}
