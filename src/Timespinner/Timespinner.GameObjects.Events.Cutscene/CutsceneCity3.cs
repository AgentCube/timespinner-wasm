using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Misc;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutsceneCity3 : CutsceneBase
{
	private const int ChannelStandX = 120;

	private const int ChannelStandY = 192;

	private const int FamiliarCenterX = 264;

	private const int FamiliarCenterY = 152;

	private const int FamiliarRadius = 20;

	private const int FamiliarFlyX = 320;

	private const int FamiliarFlyY = 176;

	private const int GateCenterX = 208;

	private const int GateCenterY = 96;

	private const int TimespinnerCenterX = 208;

	private const int TimespinnerCenterY = 128;

	private const int TimespinnerRadius = 96;

	private const float TimespinnerGlowValue1 = 0.95f;

	private const float TimespinnerGlowValue2 = 0.85f;

	private const float TimespinnerGlowValue3 = 0f;

	private const float TimeBetweenExplosions = 0.1f;

	private bool _isBlowingUp;

	private float _blowUpTimer;

	private SoulStreamEvent _soulStream;

	private TheTimespinner _theTimespinner;

	private HaloRingAnimation _haloAnimation;

	public CutsceneCity3(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.CutsceneDisappearType = ECutsceneDisappearType.Never;
		base.IsWarpingAtEndOfCutscene = true;
	}

	internal override void DoCutscene()
	{
		Protagonist mainHero = _level.MainHero;
		_level.JukeBox.FadeOutSong(2f);
		GetTimespinner();
		AddSummonMeyef();
		AddMeyefFlyAround(new Point(264, 152), 1f, 20f);
		AddWaitScript(0.5f);
		AddScript(new ScriptAction(EScriptActionType.LookDirection, 0f, 0f, new Vector4(-1f, 0f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Familiar
		});
		AddMeyefMew();
		AddDialogue("cs_cit_3_lun_00");
		AddDialogue("cs_cit_3_lun_01");
		MovePlayerToPosition(new Point(120, 192), shouldFaceLeftAfter: false, shouldStandFancyAfter: false);
		AddDelegateScript(AddWheelAndSpindle);
		PlayScriptedSFX(ESFX.CsTimespinnerNormalStart);
		AddScript(new ScriptAction(new AnimationSpec
		{
			Start = 38,
			Length = 1,
			Speed = 0.066f,
			Type = EAnimationType.Once
		}, mainHero)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(new AnimationSpec
		{
			Start = 39,
			Length = 5,
			Speed = 0.066f,
			Type = EAnimationType.Cycle
		}, mainHero));
		AddWaitScript(0.5f);
		AddDialogue("cs_cit_3_lun_02");
		AddDelegateScript(StartScreenShake1);
		AddWaitScript(1f);
		AddScript(new ScriptAction(new AnimationSpec
		{
			Start = 38,
			Length = 1,
			Speed = 0.066f,
			Type = EAnimationType.Once,
			IsInReverse = true
		}, mainHero)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(new AnimationSpec
		{
			Start = 0,
			Length = 5,
			Speed = 0.11f,
			Type = EAnimationType.Cycle
		}, mainHero)
		{
			DoesBlockQueue = false
		});
		AddDialogue("cs_cit_3_lun_03");
		AddDialogue("cs_cit_3_lun_04");
		AddScript(new ScriptAction(new AnimationSpec
		{
			Start = 38,
			Length = 1,
			Speed = 0.066f,
			Type = EAnimationType.Once
		}, mainHero)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(new AnimationSpec
		{
			Start = 39,
			Length = 5,
			Speed = 0.066f,
			Type = EAnimationType.Cycle
		}, mainHero));
		AddDialogue("cs_cit_3_lun_05");
		AddScript(new ScriptAction(EScriptActionType.ChangeColor, 0f, 1f, new Vector4(1f, 0.95f, 1f, 1f))
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = _theTimespinner
		});
		AddDelegateScript(OpenGate);
		AddWaitScript(3f);
		AddScript(new ScriptAction(new AnimationSpec
		{
			Start = 38,
			Length = 1,
			Speed = 0.066f,
			Type = EAnimationType.Once,
			IsInReverse = true
		}, mainHero)
		{
			DoesBlockQueue = true
		});
		AddScript(new ScriptAction(new AnimationSpec
		{
			Start = 0,
			Length = 5,
			Speed = 0.11f,
			Type = EAnimationType.Cycle
		}, mainHero)
		{
			DoesBlockQueue = false
		});
		AddDelegateScript(FinishChannelling);
		AddScript(new ScriptAction(EScriptActionType.ChangeColor, 0f, 1f, new Vector4(0.95f, 0.85f, 1f, 1f))
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = _theTimespinner
		});
		AddWaitScript(0.25f);
		AddScript(new ScriptAction(EScriptActionType.Jump, 0f, 1f, Vector4.Zero)
		{
			TargetType = EScriptTargetType.Player1,
			DoesBlockQueue = false
		});
		AddScript(new ScriptAction(EScriptActionType.Run, 0f, 1f, new Vector4(1f, 0f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Player1,
			DoesBlockQueue = false
		});
		AddDelegateScript(StartBlowingUp);
		AddMeyefFlyAround(new Point(320, 176), 2f, 20f);
		AddWaitScript(0.1f);
		AddScript(new ScriptAction(EScriptActionType.StartGlowing, 0f, 0.4f, new Vector4(1f, 0f, 2f, 15f))
		{
			TargetType = EScriptTargetType.Player1,
			DoesBlockQueue = true
		});
		AddDelegateScript(AddFlashOnPlayer);
		AddWaitScript(0.01f);
		AddScript(new ScriptAction
		{
			ScriptType = EScriptType.HideShowPlayer,
			Arguments = new Vector4(0f, 1f, 0f, 0f)
		});
		AddWaitScript(0.15f);
		AddDelegateScript(RemoveWheelAndSpindle);
		AddScript(new ScriptAction(EScriptActionType.ChangeColor, 0f, 3f, new Vector4(0.85f, 0f, 1f, 1f))
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = _theTimespinner,
			DoesBlockQueue = false
		});
		AddLevelScriptAction(new ScriptAction
		{
			ScriptType = EScriptType.FadeInFadeOut,
			Arguments = new Vector4(1.75f, 0.25f, 0.25f, 1f),
			DoesBlockQueue = false
		});
		AddWaitScript(0.15f);
		AddMeyefFlyTo(new Point(208, 96), 0.5f, doesBlock: false);
		AddScript(new ScriptAction(EScriptActionType.StartGlowing, 0f, 0.5f, new Vector4(1f, 0f, 2f, 15f))
		{
			TargetType = EScriptTargetType.Familiar,
			DoesBlockQueue = true
		});
		AddDelegateScript(AddFlashOnFamiliar);
		AddScript(new ScriptAction
		{
			ScriptType = EScriptType.HideShowPlayer,
			Arguments = Vector4.Zero
		});
		AddWaitScript(0.1f);
		AddDelegateScript(CloseGate);
		AddWaitScript(0.5f);
		AddDismissMeyefSilent();
		LevelChangeRequest levelChangeRequest = new LevelChangeRequest();
		levelChangeRequest.LevelID = 3;
		levelChangeRequest.RoomID = 28;
		levelChangeRequest.PreviousLevelID = _level.ID;
		levelChangeRequest.IsUsingWarp = true;
		levelChangeRequest.IsUsingWhiteFadeOut = true;
		levelChangeRequest.FadeOutTime = 0.5f;
		levelChangeRequest.FadeInTime = 1f;
		levelChangeRequest.AdditionalBlackScreenTime = 1f;
		levelChangeRequest.CutsceneToCall = ECutsceneType.Forest0_Warp;
		LevelChangeRequest levelRoomChangeRequest = levelChangeRequest;
		AddLevelScriptAction(new ScriptAction(levelRoomChangeRequest));
	}

	private void GetTimespinner()
	{
		IEnumerable<GameEvent> eventAllEventsOfType = _level.GetEventAllEventsOfType(EEventTileType.TheTimespinner);
		foreach (GameEvent item in eventAllEventsOfType)
		{
			_theTimespinner = item as TheTimespinner;
			if (_theTimespinner != null)
			{
				break;
			}
		}
	}

	private void AddWheelAndSpindle()
	{
		if (_theTimespinner != null)
		{
			_theTimespinner.AddWheelAndSpindle();
			_soulStream = _theTimespinner.AddSoulStream(isAnchoredToPlayer: true);
		}
	}

	private void StartScreenShake1()
	{
		_level.RequestScreenShake(new Vector2(0f, 1f), -1f, 5f, isAffectedByTime: true);
	}

	private void RemoveWheelAndSpindle()
	{
		if (_theTimespinner != null)
		{
			_theTimespinner.RemoveWheelAndSpindle();
		}
		_level.RequestScreenShake(new Vector2(2f, 3f), -1f, 25f, isAffectedByTime: true);
	}

	private void OpenGate()
	{
		if (_theTimespinner != null)
		{
			_theTimespinner.ChangeTimespinnerState(TheTimespinner.ETimespinnerState.Haywire);
			_theTimespinner.OpenTimeGate();
			_theTimespinner.SetTimeGateTargetLevel(18);
		}
		_level.RequestScreenShake(new Vector2(0f, 1f), -1f, 15f, isAffectedByTime: true);
	}

	private void FinishChannelling()
	{
		if (_soulStream != null)
		{
			_soulStream.DoFade(isFadingOut: true);
		}
		_level.RequestScreenShake(new Vector2(0f, 1f), -1f, 20f, isAffectedByTime: true);
	}

	private void StartBlowingUp()
	{
		_isBlowingUp = true;
	}

	private void CloseGate()
	{
		if (_theTimespinner != null)
		{
			_theTimespinner.CloseTimeGate();
		}
	}

	private void AddFlashOnPlayer()
	{
		Protagonist mainHero = _level.MainHero;
		if (mainHero != null)
		{
			_haloAnimation = new HaloRingAnimation(_level)
			{
				Center = mainHero.Bbox.Center
			};
		}
	}

	private void AddFlashOnFamiliar()
	{
		if (_haloAnimation != null)
		{
			_haloAnimation.Reset();
			_haloAnimation.Center = new Point(208, 96);
		}
	}

	private void AddExplosion()
	{
		double num = _level.NextRandomDouble() * 6.2831854820251465;
		int num2 = (int)Math.Round(Math.Cos(num) * 96.0);
		int num3 = (int)Math.Round(Math.Sin(num) * 96.0);
		Point position = new Point(208 + num2, 128 + num3);
		EBattleAnimationType animationType = ((!(_level.NextRandomDouble() < 0.6600000262260437)) ? EBattleAnimationType.SmallBoom : EBattleAnimationType.Boom);
		_level.AddAnimation(animationType, position, ETeamSide.Neutral, isFacingRight: true, doesPlaySFX: true);
	}

	public override void Update(float delta)
	{
		if (_haloAnimation != null)
		{
			_haloAnimation.Update(delta);
		}
		if (_isBlowingUp)
		{
			_blowUpTimer -= delta;
			if (_blowUpTimer <= 0f)
			{
				AddExplosion();
				_blowUpTimer += 0.1f;
			}
		}
		base.Update(delta);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (_haloAnimation != null && !_haloAnimation.IsFinished)
		{
			_haloAnimation.Draw(spriteBatch);
		}
		base.Draw(spriteBatch);
	}
}
