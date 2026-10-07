using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Bosses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneCavesPast4 : CutsceneBase
{
	private const int SpitEndX = 208;

	private const int SpitEndY = 192;

	private const float StandAnimationSpeed = 0.125f;

	private MawBoss _mawBoss;

	public CutsceneCavesPast4(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.IsCreatedAfterWarp = true;
		_level.RequestScreenFadeOut(0f, 0.25f, 0.25f, 0f);
	}

	public override void Initialize()
	{
		List<Monster> visibleEnemies = _level.GetVisibleEnemies();
		foreach (Monster item in visibleEnemies)
		{
			_mawBoss = item as MawBoss;
			if (_mawBoss != null)
			{
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
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.MawDoorSpit, 0f, 0.45f, new Vector4(Position.X, Position.Y, 208f, 192f))
		{
			DoesBlockQueue = false
		});
		AddUnskippableWaitScript(0.35f);
		AddScript(new ScriptAction(ESFX.BossMawRoar, new Point(0, 120)));
		AddScreenShake(new Vector2(1f, 2f), 3f, 12f);
		AddUnskippableWaitScript(1.5f);
		AddLevelScriptAction(new ScriptAction
		{
			Delegate = CloseMawDoor,
			ScriptType = EScriptType.Delegate
		});
		PlayScriptedSFX(ESFX.LunaisStandFromLyingDown, mainHero.Position);
		AddScript(new ScriptAction(newAnim, mainHero)
		{
			DoesBlockQueue = true,
			IsUnskippable = true
		});
		AddScript(new ScriptAction(newAnim2, mainHero)
		{
			DoesBlockQueue = true,
			IsUnskippable = true
		});
		AddScript(new ScriptAction(newAnim3, mainHero)
		{
			DoesBlockQueue = false
		});
		AddDialogue("cs_maw_1_lun_00");
		AddDialogue("cs_maw_1_lun_01");
		AddDialogue("cs_maw_1_lun_02");
		AddWaitScript(0.25f);
		CutsceneBase.AddLunaisPalmPunch(_level);
		AddDialogue("cs_maw_1_lun_03");
	}

	private void CloseMawDoor()
	{
		if (_mawBoss != null)
		{
			_mawBoss.DoIntroCloseMouth();
		}
	}
}
