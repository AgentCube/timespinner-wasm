using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Bosses.Emperor;
using Timespinner.GameObjects.Enemies;
using Timespinner.GameObjects.Events.Doors;
using Timespinner.GameObjects.Events.Misc;
using Timespinner.GameObjects.Heroes;
using Timespinner.GameObjects.NPCs;

namespace Timespinner.GameObjects.Events.Cutscene;

internal class CutscenePrologue3 : CutsceneBase
{
	private const int KnightArgument = 426;

	private const int GunnerArgument = 427;

	private const int EmperorArgument = 439;

	private const int GyrePortalArgument = 498;

	private const int GyrePortalPrologueArgument = 4;

	private const int FloorY = 224;

	private const int RoomCenterX = 352;

	private const int RoomCenterY = 136;

	private const int SelenStartX = 192;

	private const int EnemyStartX = 88;

	private const int EnemyStartY = 224;

	private const int EmperorStartX = -32;

	private const int EmperorStartY = 192;

	private const int LunaisStandOffsetX = 56;

	private const int SelenStandOffsetX = -56;

	private const int LunaisFrozenX = 350;

	private const int LunaisFrozenY = 146;

	private const int GunnerMoveX = 92;

	private const int KnightMoveX = 128;

	private const int GyrePortalOffsetX = 8;

	private const int GyrePortalOffsetY = 57;

	private const int EmperorMove1X = 224;

	private const int EmperorMove1Y = 216;

	private const int EmperorLandX = 228;

	private const int EmperorLandY = 228;

	private const int SelenEmperorOffsetX = 32;

	private const int SelenEmperorOffsetY = -34;

	private const int EmperorMove2X = 192;

	private const int EmperorMove2Y = 224;

	private const int TimespinnerCenterX = 352;

	private const int TimespinnerCenterY = 128;

	private const int TimespinnerRadius = 96;

	private const float TimeBetweenExplosions = 0.1f;

	private static readonly Point TimespinnerPosition = new Point(344, 208);

	private static readonly Color SelenShockwaveColor = new Color(1f, 0.8f, 0.9f, 0.5f);

	private readonly ShockwaveAnimation _selenShockwave;

	private readonly SelenNPC _selen;

	private readonly FortressKnight _knight;

	private readonly FortressGunner _gunner;

	private readonly EmperorBoss _emperor;

	private readonly GyrePortalEvent _gyrePortal;

	private readonly TimespinnerShrapnelEvent _shrapnelEvent;

	private bool _isBlowingUp;

	private float _blowUpTimer;

	private HaloRingAnimation _haloAnimation;

	private SoulStreamEvent _soulStream;

	private SandStreamerEvent _streamerEvent;

	private TheTimespinner _timespinner;

	public CutscenePrologue3(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CutsceneTriggerType = ECutsceneTriggerType.Called;
		base.IsWarpingAtEndOfCutscene = true;
		_selen = new SelenNPC(_level, new Point(192, 224), -1);
		_selen.ChangeNPCAIType(NPCBase.ENPCAIType.None);
		Point inPosition2 = new Point(88, 224);
		_knight = new FortressKnight(inPosition2, _level, _level.GCM.SpFortressKnight, -1, new ObjectTileSpecification(426));
		_gunner = new FortressGunner(inPosition2, _level, _level.GCM.SpFortressGunner, -1, new ObjectTileSpecification(427));
		_knight.SetAgility(0.3f);
		_knight.SetCutsceneAI();
		_gunner.SetCutsceneAI();
		_emperor = new EmperorBoss(new Point(-32, 192), _level, _level.GCM.SpEmperor, -1, new ObjectTileSpecification(439))
		{
			IsSpawnedForCutscene = true
		};
		_emperor.SetCutsceneAI();
		_emperor.InitializeMob();
		_selenShockwave = new ShockwaveAnimation(_level.GCM.SpOrbMeleeBarrier, _selen.Position, _level, SelenShockwaveColor)
		{
			DrawPlane = EDrawPlane.Front
		};
		_gyrePortal = new GyrePortalEvent(inPosition: new Point(360, 193), inLevel: _level, inID: -1, objectSpec: new ObjectTileSpecification(498)
		{
			Argument = 4
		});
		_level.RequestAddObject(_selen);
		_shrapnelEvent = new TimespinnerShrapnelEvent(inPosition: new Point(352, 112), inLevel: _level, sprite: _level.GCM.SpTheTimespinner);
	}

	public override void Initialize()
	{
		IEnumerable<GameEvent> eventAllEventsOfType = _level.GetEventAllEventsOfType(EEventTileType.TheTimespinner);
		foreach (GameEvent item in eventAllEventsOfType)
		{
			_timespinner = (TheTimespinner)item;
			if (_timespinner != null)
			{
				break;
			}
		}
		base.Initialize();
		_selen.Initialize();
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

	internal override void DoCutscene()
	{
		Protagonist mainHero = _level.MainHero;
		AddHideFamiliar(0f);
		_level.SetCameraUpdateDisable(isDisabled: true);
		AddCameraPan(new Point(352, 136), 1f, doesBlockQueue: false);
		AddLevelScriptAction(new ScriptAction
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = _selen,
			ActionType = EScriptActionType.GoToPoint,
			ActionTimer = 0.1f,
			DoesBlockQueue = false,
			Arguments = new Vector4(TimespinnerPosition.X + -56, 224f, 0f, 0f)
		});
		AddLevelScriptAction(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.GoToPoint,
			ActionTimer = 0.1f,
			Arguments = new Vector4(408f, 224f, 0f, 0f)
		});
		AddLevelScriptAction(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.LookDirection,
			Arguments = new Vector4(-1f, 0f, 0f, 0f)
		});
		AddLevelScriptAction(new ScriptAction
		{
			TargetType = EScriptTargetType.Specified,
			ScriptTarget = _selen,
			ActionType = EScriptActionType.LookDirection,
			Arguments = new Vector4(1f, 0f, 0f, 0f)
		});
		AddWaitScript(0.25f);
		AddDialogue("cs_pro_3_lun_00");
		AddLevelScriptAction(new ScriptAction(new AnimationSpec
		{
			Start = 25,
			Length = 1,
			Speed = 0.066f,
			Type = EAnimationType.Once
		}, _selen)
		{
			DoesBlockQueue = true
		});
		AddLevelScriptAction(new ScriptAction(new AnimationSpec
		{
			Start = 26,
			Length = 4,
			Speed = 0.1f,
			Type = EAnimationType.Cycle
		}, _selen));
		AddWaitScript(0.25f);
		PlayScriptedSFX(ESFX.CsTimespinnerNormalStart);
		AddDelegateScript(StartTimespinner);
		AddDialogue("cs_pro_3_sel_01");
		AddDelegateScript(SpeedUpTimespinner);
		AddWaitScript(1f);
		AddDelegateScript(ShowSoldiers);
		AddWaitScript(0.5f);
		MoveCharacterToPosition(_knight, new Point(128, 224), doesBlockQueue: false, 0f);
		AddWaitScript(0.25f);
		MoveCharacterToPosition(_gunner, new Point(92, 224), doesBlockQueue: false, 0f);
		AddCameraPan(new Point(256, (int)_level.CameraPosition.Y), 1f, doesBlockQueue: false);
		AddDelegateScript(OpenTimeGate);
		AddWaitScript(0.5f);
		AddDelegateScript(DoGunnerAim);
		AddDialogue("cs_pro_3_sol_03");
		AddLevelScriptAction(new ScriptAction(new AnimationSpec
		{
			Start = 218,
			Length = 3,
			Speed = 0.1f,
			Type = EAnimationType.Once
		}, mainHero));
		AddDialogue("cs_pro_3_lun_04");
		AddDialogue("cs_pro_3_sel_05");
		AddScript(new ScriptAction(EScriptActionType.Run, 0f, 1f, new Vector4(-1f, 0f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Player1,
			DoesBlockQueue = false
		});
		AddWaitScript(0.1f);
		AddScript(new ScriptAction(EScriptActionType.Jump, 0f, 1f, Vector4.Zero)
		{
			TargetType = EScriptTargetType.Player1,
			DoesBlockQueue = false
		});
		AddWaitScript(0.15f);
		AddDelegateScript(AddEmperor);
		AddScript(new ScriptAction(EScriptActionType.StartFloating, 0f, 0.2f, new Vector4(350f, 146f, 0f, 0f))
		{
			TargetType = EScriptTargetType.Player1
		});
		AddScript(new ScriptAction(EScriptActionType.StartGlowing, 0f, 0.2f, new Vector4(1f, 0.75f, 2f, 2f))
		{
			TargetType = EScriptTargetType.Player1,
			DoesBlockQueue = true
		});
		AddDelegateScript(AddFlashOnPlayer);
		AddWaitScript(0.8f);
		AddDelegateScript(LandEmperor);
		AddDialogue("cs_pro_3_emp_06");
		AddDelegateScript(DoEmperorThrow);
		AddWaitScript(0.1f);
		AddDelegateScript(DoMoveSelenToEmperorA);
		AddDelegateScript(MakeTimespinnerHaywire);
		AddWaitScript(0.9f);
		AddDialogue("cs_pro_3_sel_07");
		AddDialogue("cs_pro_3_emp_08");
		AddDialogue("cs_pro_3_sel_09");
		AddDelegateScript(DoEmperorTakeOff);
		AddWaitScript(1f);
		AddScreenFlash(0.2f, 1f, 2f);
		PlayScriptedSFX(ESFX.CsTimespinnerSelenDeath, new Point(192, 224));
		AddDelegateScript(DoSelenFall);
		AddDialogue("cs_pro_3_lun_10");
		AddLevelScriptAction(new ScriptAction(new AnimationSpec
		{
			Start = 169,
			Length = 4,
			Speed = 0.07f,
			Type = EAnimationType.Once
		}, mainHero)
		{
			DoesBlockQueue = true
		});
		AddLevelScriptAction(new ScriptAction(new AnimationSpec
		{
			Start = 173,
			Length = 5,
			Speed = 0.07f,
			Type = EAnimationType.Cycle
		}, mainHero)
		{
			DoesBlockQueue = false
		});
		AddDialogue("cs_pro_3_lun_11");
		AddDelegateScript(MakeTimespinnerDead);
		AddCameraPan(new Point(352, 136), 4f, doesBlockQueue: false);
		AddScript(new ScriptAction(EScriptActionType.StartGlowing, 0f, 2f, new Vector4(0.75f, 0f, 2f, 12f))
		{
			TargetType = EScriptTargetType.Player1,
			DoesBlockQueue = false
		});
		AddLevelScriptAction(new ScriptAction
		{
			ScriptType = EScriptType.FadeInFadeOut,
			Arguments = new Vector4(1.75f, 0.5f, 0.25f, 1f),
			DoesBlockQueue = false
		});
		AddWaitScript(1.25f);
		AddDelegateScript(HideTimespinnerPieces);
		AddSongFadeOut(0.5f, doesBlock: false);
		AddWaitScript(0.25f);
		LevelChangeRequest levelChangeRequest = new LevelChangeRequest();
		levelChangeRequest.LevelID = 1;
		levelChangeRequest.RoomID = 0;
		levelChangeRequest.IsUsingWarp = true;
		levelChangeRequest.IsUsingWhiteFadeOut = true;
		levelChangeRequest.FadeOutTime = 0.25f;
		levelChangeRequest.FadeInTime = 1f;
		levelChangeRequest.AdditionalBlackScreenTime = 1f;
		levelChangeRequest.CutsceneToCall = ECutsceneType.LakeDesolation0_Warp;
		levelChangeRequest.ShouldPlayLevelSong = true;
		LevelChangeRequest levelRoomChangeRequest = levelChangeRequest;
		AddLevelScriptAction(new ScriptAction(levelRoomChangeRequest));
		AddEndSepiaFade(0f, doesBlock: false);
	}

	private void StartTimespinner()
	{
		if (_timespinner != null)
		{
			_timespinner.ChangeTimespinnerState(TheTimespinner.ETimespinnerState.StartingSlow);
			_soulStream = _timespinner.AddSoulStream(_selen);
		}
	}

	private void SpeedUpTimespinner()
	{
		if (_timespinner != null)
		{
			_timespinner.ChangeTimespinnerState(TheTimespinner.ETimespinnerState.StartingFast);
		}
	}

	private void OpenTimeGate()
	{
		if (_timespinner != null)
		{
			_timespinner.SetTimeGateTargetLevel(15);
			_timespinner.OpenTimeGate();
		}
		_level.RequestScreenShake(new Vector2(0f, 1f), -1f, 15f, isAffectedByTime: true);
	}

	private void MakeTimespinnerHaywire()
	{
		if (_timespinner != null)
		{
			_timespinner.ChangeTimespinnerState(TheTimespinner.ETimespinnerState.Haywire);
		}
		if (_soulStream != null)
		{
			_soulStream.StartSoulBreak(isFirstBreak: true);
		}
		_level.RequestScreenShake(new Vector2(0f, 1f), -1f, 20f, isAffectedByTime: true);
	}

	private void MakeTimespinnerDead()
	{
		if (_timespinner != null)
		{
			_timespinner.ChangeTimespinnerState(TheTimespinner.ETimespinnerState.Dead);
		}
		_shrapnelEvent.IsAbsorbingUnits = true;
		_isBlowingUp = true;
		_level.JukeBox.PlayCue(ESFX.CsTimespinnerAbsorb);
	}

	private void ShowSoldiers()
	{
		_level.RequestAddObject(_knight);
		_level.RequestAddObject(_gunner);
		_knight.InitializeMob();
		_gunner.InitializeMob();
	}

	private void DoGunnerAim()
	{
		_gunner.DoAim();
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

	private void AddEmperor()
	{
		_level.JukeBox.PlaySong(EBGM.CsEmperor);
		_level.RequestAddObject(_emperor);
		_emperor.IsFacingLeft = false;
		_emperor.MoveForCutscene(new Point(224, 216), 1.5f, isSinusoidal: true);
	}

	private void LandEmperor()
	{
		_emperor.LandAtLocation(new Point(228, 228), 0.25f, shouldDoLandingAnimation: true);
	}

	private void DoEmperorThrow()
	{
		_emperor.SetCharacterSequenceByName("ProThrow");
	}

	private void DoMoveSelenToEmperorA()
	{
		Point targetPoint = new Point(_emperor.Position.X + 32, _emperor.Position.Y + -34);
		_selen.CutsceneFloatToPoint(targetPoint, 1.5f, isFalling: false);
		_selen.SetEmperorAnchor(_emperor);
	}

	private void DoEmperorTakeOff()
	{
		_emperor.TakeOffFromGround();
		_emperor.MoveForCutscene(new Point(192, 224), 1f, isSinusoidal: false);
		if (_soulStream != null)
		{
			_soulStream.StartSoulBreak(isFirstBreak: false);
		}
	}

	private void DoSelenFall()
	{
		int x = _selen.Position.X;
		_selen.CutsceneFloatToPoint(new Point(x, 224), 0.35f, isFalling: true);
		if (_soulStream != null)
		{
			_soulStream.DoFade(isFadingOut: true);
		}
		if (_timespinner != null)
		{
			_timespinner.SetGyrePortal(_gyrePortal);
			_timespinner.SetCharacterSequenceByName("Break");
		}
		_streamerEvent = new SandStreamerEvent(_level, _selen.Bbox.Center, ESandStreamerType.SelenDeath);
		_level.RequestAddObject(_streamerEvent);
		_level.RequestAddObject(_shrapnelEvent);
		_selenShockwave.Position = _selen.Bbox.Center;
		_level.AddAnimation(_selenShockwave);
		_level.RequestScreenShake(new Vector2(0f, 2f), -1f, 20f, isAffectedByTime: true);
		_gunner.SetCharacterSequenceByName("EndAttack");
	}

	private void HideTimespinnerPieces()
	{
		if (_timespinner != null)
		{
			_timespinner.HidePieces();
		}
	}

	private void AddExplosion()
	{
		double num = _level.NextRandomDouble() * 6.2831854820251465;
		int num2 = (int)Math.Round(Math.Cos(num) * 96.0);
		int num3 = (int)Math.Round(Math.Sin(num) * 96.0);
		Point position = new Point(352 + num2, 128 + num3);
		EBattleAnimationType animationType = ((!(_level.NextRandomDouble() < 0.6600000262260437)) ? EBattleAnimationType.SmallBoom : EBattleAnimationType.Boom);
		_level.AddAnimation(animationType, position, ETeamSide.Neutral, isFacingRight: true, doesPlaySFX: true);
	}
}
