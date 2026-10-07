using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Gameplay.Scripts;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneRevive : CutsceneBase
{
	private const EInventoryFamiliarType MeyefFamiliarType = EInventoryFamiliarType.Meyef;

	private const int MeyefStatueOffsetY = -48;

	private const int LunaisDeathIndex = 204;

	private const float StandAnimationSpeed = 0.1f;

	private readonly bool _isFirstDeath;

	private readonly SaveStatue _saveStatue;

	public CutsceneRevive(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.DoesHideOrbsShowAnimation = false;
		_isFirstDeath = !_level.GameSave.Inventory.FamiliarInventory.Inventory.ContainsKey(1);
		AnimationSpec newAnim = new AnimationSpec
		{
			Start = 204,
			Length = 1
		};
		Protagonist mainHero = _level.MainHero;
		AddScript(new ScriptAction(newAnim, mainHero));
		AddLevelScriptAction(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.SheatheWeapon,
			Arguments = new Vector4(0f, 1f, 0f, 0f)
		});
		IEnumerable<GameEvent> eventAllEventsOfType = _level.GetEventAllEventsOfType(EEventTileType.Checkpoint);
		foreach (GameEvent item in eventAllEventsOfType)
		{
			_saveStatue = item as SaveStatue;
			if (_saveStatue != null)
			{
				if (_isFirstDeath)
				{
					_saveStatue.HideDragon();
					AddSummonMeyef();
					AddScript(new FamiliarScript(FamiliarScript.EFamiliarScriptType.SitStill));
					AddScript(new ScriptAction
					{
						TargetType = EScriptTargetType.Familiar,
						ActionType = EScriptActionType.WarpToPoint,
						Arguments = new Vector4(_saveStatue.Position.X, _saveStatue.Position.Y + -48, 0f, 0f)
					});
					AddFamiliarAnimation(39, 1, 0f, EAnimationType.Once, doesBlock: false);
				}
				_saveStatue.StopGlowingNow();
				break;
			}
		}
	}

	internal override void DoCutscene()
	{
		AnimationSpec animationSpec = new AnimationSpec();
		animationSpec.Start = 212;
		animationSpec.Length = 2;
		animationSpec.Speed = 0.1f;
		animationSpec.Type = EAnimationType.Once;
		AnimationSpec newAnim = animationSpec;
		AnimationSpec animationSpec2 = new AnimationSpec();
		animationSpec2.Start = 0;
		animationSpec2.Length = 5;
		animationSpec2.Speed = 0.11f;
		animationSpec2.Type = EAnimationType.Cycle;
		AnimationSpec newAnim2 = animationSpec2;
		Protagonist mainHero = _level.MainHero;
		AnimationSpec animationSpec3 = new AnimationSpec();
		animationSpec3.Start = 204;
		animationSpec3.Length = 1;
		AnimationSpec newAnim3 = animationSpec3;
		AddScript(new ScriptAction(newAnim3, mainHero));
		AddWaitScript(1f);
		if (_isFirstDeath)
		{
			AnimationSpec animationSpec4 = new AnimationSpec();
			animationSpec4.Start = 205;
			animationSpec4.Length = 1;
			animationSpec4.Speed = 0.1f;
			animationSpec4.Type = EAnimationType.Once;
			AnimationSpec newAnim4 = animationSpec4;
			AnimationSpec animationSpec5 = new AnimationSpec();
			animationSpec5.Start = 206;
			animationSpec5.Length = 1;
			animationSpec5.Speed = 0.1f;
			animationSpec5.Type = EAnimationType.Once;
			AnimationSpec newAnim5 = animationSpec5;
			AddDialogue("cs_mey_lun_00");
			AddWaitScript(0.25f);
			AddFamiliarAnimation(39, 2, 0.25f, EAnimationType.Once, doesBlock: true);
			AddFamiliarAnimation(39, 2, 0.25f, EAnimationType.Once, doesBlock: true);
			AddFamiliarAnimation(39, 2, 0.25f, EAnimationType.Once, doesBlock: true);
			AddScript(new ScriptAction(newAnim4, mainHero)
			{
				DoesBlockQueue = true
			});
			AddWaitScript(0.15f);
			AddDialogue("cs_mey_lun_01");
			PlayScriptedSFX(ESFX.MeyefPurr, new Point(_saveStatue.Position.X, _saveStatue.Position.Y));
			AddFamiliarAnimation(41, 3, 0.1f, EAnimationType.Once, doesBlock: true);
			AddWaitScript(0.5f);
			AddFamiliarAnimation(44, 3, 0.1f, EAnimationType.Once, doesBlock: true);
			AddMeyefFlyTo(new Point(mainHero.Position.X + 8, mainHero.Position.Y - 64), 0.5f, doesBlock: false);
			AddFamiliarAnimation(0, 5, 0.1f, EAnimationType.Cycle, doesBlock: false);
			AddWaitScript(0.5f);
			AddMeyefFlyAround(new Point(mainHero.Position.X + 32, mainHero.Position.Y - 24), 2f, 16f);
			AddWaitScript(0.5f);
			AddScript(new ScriptAction
			{
				ScriptType = EScriptType.Action,
				ActionType = EScriptActionType.LookDirection,
				TargetType = EScriptTargetType.Familiar,
				Arguments = new Vector4(-1f, 0f, 0f, 0f)
			});
			PlayScriptedSFX(ESFX.LunaisStandFromCrouch, mainHero.Position);
			AddScript(new ScriptAction(newAnim5, mainHero)
			{
				DoesBlockQueue = true
			});
			AddScript(new ScriptAction(newAnim, mainHero)
			{
				DoesBlockQueue = true
			});
			AddScript(new ScriptAction(newAnim2, mainHero)
			{
				DoesBlockQueue = false
			});
			AddWaitScript(0.5f);
			AddScript(new ScriptAction(EScriptActionType.Backdash, 0f, 0.52f, Vector4.Zero)
			{
				TargetType = EScriptTargetType.Player1
			});
			AddDialogue("cs_mey_lun_02");
			AddMeyefFlyTo(new Point(mainHero.Position.X + 4, mainHero.Position.Y - 16), 1.5f, doesBlock: true);
			AddWaitScript(0.65f);
			AddMeyefMew();
			AddDialogue("cs_mey_lun_03");
			AddMeyefMew();
			AddWaitScript(0.05f);
			AddDelegateScript(EquipMeyef);
			AddScript(new FamiliarScript(FamiliarScript.EFamiliarScriptType.Dismiss)
			{
				Arguments = new Vector4(1f, 0f, 0f, 0f)
			});
		}
		else
		{
			AnimationSpec animationSpec6 = new AnimationSpec();
			animationSpec6.Start = 205;
			animationSpec6.Length = 2;
			animationSpec6.Speed = 0.1f;
			animationSpec6.Type = EAnimationType.Once;
			AnimationSpec newAnim6 = animationSpec6;
			AddScript(new ScriptAction(newAnim6, mainHero)
			{
				DoesBlockQueue = true
			});
			AddScript(new ScriptAction(newAnim, mainHero)
			{
				DoesBlockQueue = true
			});
			AddScript(new ScriptAction(newAnim2, mainHero)
			{
				DoesBlockQueue = false
			});
		}
	}

	private void EquipMeyef()
	{
		_level.GameSave.Inventory.EquippedFamiliar = EInventoryFamiliarType.Meyef;
	}
}
