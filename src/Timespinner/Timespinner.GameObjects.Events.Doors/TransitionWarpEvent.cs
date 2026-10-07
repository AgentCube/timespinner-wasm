using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Cutscene;
using Timespinner.GameObjects.Heroes;
using Timespinner.GameStateManagement.Screens.InGame;

namespace Timespinner.GameObjects.Events.Doors;

internal sealed class TransitionWarpEvent : GameEvent
{
	private const int KeySpinFinalRadius = 32;

	private const int EntranceKeyRadius = 64;

	private const int EntranceKeyDiameter = 128;

	private const int SpindleOffsetY = -62;

	private const int SpindleRotationSpeedMin = 1;

	private const int SpindleRotationSpeedMax = 50;

	private const int CenterOffsetY = -71;

	private const int NightmareSmokeSimulationCount = 60;

	private const int NightmareSmokeMinRadius = 16;

	private const int NightmareSmokeMaxRadius = 28;

	private const int NightmareSmokeOffsetY = -1;

	private const int NightmareEmissionPerTick = 3;

	private const float TimeForKeysToReachFinalRadius = 1f;

	private const float TimeForEntireKeySequence = 1.3f;

	private const float TimeBeforeEntranceRingShows = 0.15f;

	private const float TimeForEntranceRingToShow = 0.25f;

	private const float TimeBetweenCheckingForKey = 1f;

	private const float TimeAfterSpatialWarpingToWaitForUsage = 1.75f;

	private const float TimeAfterTimeWarpingToWaitForUsage = 4.5f;

	private const float TimeBeforeStoppingSparkleEmission = 0.75f;

	private const float KeyGlowFrequency = 3f;

	private const float TimeBetweenNightmareSmokeParticles = 0.03f;

	private const float TimeAfterWarpingBeforePauseUsage = 0.75f;

	private static readonly Vector4 SmokeColor = new Vector4(0.6f, 0.6f, 0.5f, 0.1f);

	private readonly bool _isNightmareGate;

	private readonly Point _entranceRingDrawCenter;

	private readonly Vector2 _particleEmissionPoint;

	private readonly Appendage _leftSlotAppendage;

	private readonly Appendage _rightSlotAppendage;

	private readonly Appendage _leftGateAppendage;

	private readonly Appendage _rightGateAppendage;

	private readonly Appendage _leftKeyAppendage;

	private readonly Appendage _rightKeyAppendage;

	private readonly Appendage _spindleAppendage;

	private readonly Appendage _nightmareTopAppendage;

	private readonly Appendage _nightmareBottomAppendage;

	private readonly TransitionWarpLeakParticleSystem _sparkleParticles;

	private readonly TimeGateSwirlParticleSystem _nightmareSwirlParticles;

	private readonly Texture2D _entranceRingTexture;

	private bool _hasUpdatedOnce;

	private bool _doesPlayerHaveKey;

	private bool _isDoingKeySequence;

	private bool _isWarpingToADifferentEra;

	private bool _isWaitingForTimeWarpToFinish;

	private float _keyCheckTimer;

	private float _keyGlowTimer;

	private float _keySpinTimer;

	private float _playerSpitSequenceTimer;

	private float _entranceRingPercentage;

	private float _nightmareSmokeEmissionTimer;

	private Point _keyOrigin;

	private LevelChangeRequest _levelChangeRequest;

	internal bool IsSpittingOutPlayer { get; set; }

	internal CutsceneBase.ECutsceneType CutsceneToCall { get; set; }

	internal Point WarpInPoint { get; private set; }

	public TransitionWarpEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.EventType = EEventTileType.TransitionWarpEvent;
		Bbox = new Rectangle(0, 0, 48, 32);
		_sprite = _level.GCM.SpTimeGateAnimation;
		_doesDrawBaseSprite = false;
		_doAppendagesMatchImageFacing = false;
		WarpInPoint = new Point(inPosition.X, inPosition.Y - 72);
		_isNightmareGate = objectSpec.Argument == 1;
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_doesPersist = true;
		_isAffectedByGravity = false;
		_isRepeatedTrigger = false;
		_isAffectedByTime = true;
		_keyCheckTimer = 1f;
		_sparkleParticles = new TransitionWarpLeakParticleSystem(_level.GCM.TxParticleEnergy, 10);
		_particleEmissionPoint = new Vector2(inPosition.X, inPosition.Y - 56);
		_particleSystems.Add(_sparkleParticles);
		_leftSlotAppendage = new Appendage(this, new Point(9, 7), Point.Zero, _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorOffset = new Point(-32, -19),
			IsGlowing = true,
			IsFacingLeft = true
		};
		_rightSlotAppendage = new Appendage(this, new Point(9, 7), Point.Zero, _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorOffset = new Point(-31, -19),
			IsGlowing = true,
			IsFacingLeft = false
		};
		_leftSlotAppendage.ChangeAnimation(20);
		_rightSlotAppendage.ChangeAnimation(20);
		_leftKeyAppendage = new Appendage(this, new Point(16, 16), Point.Zero, _level, _sprite)
		{
			IsFacingLeft = true,
			DrawOrigin = new Vector2(8f, 8f),
			DoesDrawTrail = true,
			TrailLength = 6,
			TrailFadeRate = 2f
		};
		_rightKeyAppendage = new Appendage(this, new Point(16, 16), Point.Zero, _level, _sprite)
		{
			IsFacingLeft = false,
			DrawOrigin = new Vector2(8f, 8f),
			DoesDrawTrail = true,
			TrailLength = 6,
			TrailFadeRate = 2f
		};
		_spindleAppendage = new Appendage(this, new Point(21, 21), Point.Zero, _level, _sprite)
		{
			DrawOrigin = new Vector2(10.5f, 10.5f)
		};
		_leftGateAppendage = new Appendage(this, new Point(16, 52), Point.Zero, _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorOffset = new Point(-7, -28),
			IsFacingLeft = true,
			AuraColor = Color.White * 0.9f,
			AuraOffset = new Vector2(1f, 1f),
			AuraSize = 0.015f,
			AuraFrequency = 8f,
			DoesDrawAura = true
		};
		_rightGateAppendage = new Appendage(this, new Point(16, 52), Point.Zero, _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorOffset = new Point(-7, -28),
			IsFacingLeft = false,
			AuraColor = Color.White * 0.9f,
			AuraOffset = new Vector2(-1f, 1f),
			AuraSize = 0.015f,
			AuraFrequency = 8f,
			DoesDrawAura = true
		};
		_entranceRingTexture = _level.GCM.TxLargeRing;
		_entranceRingDrawCenter = new Point(Position.X + 64 - 8, Position.Y - 64 + 16);
		_leftGateAppendage.ChangeAnimation(-1);
		_rightGateAppendage.ChangeAnimation(-1);
		_appendages.Add(_leftGateAppendage);
		_appendages.Add(_rightGateAppendage);
		if (_isNightmareGate)
		{
			_isAffectedByTime = false;
			_nightmareSwirlParticles = new TimeGateSwirlParticleSystem(_sprite, 128)
			{
				BaseColor = SmokeColor
			};
			_particleSystems.Add(_nightmareSwirlParticles);
			Point bboxDimensions = new Point(92, 47);
			Color color = new Color(0.4f, 0.4f, 0.3f);
			_nightmareTopAppendage = new Appendage(this, bboxDimensions, Point.Zero, _level, _sprite)
			{
				FollowType = EAppendageFollowType.AnchorLocked,
				AnchorOffset = new Point(0, -71),
				DrawPriority = -1,
				DoesInheritDrawColor = false,
				DrawColor = color,
				DoesDrawAura = true,
				AuraCount = 4f,
				AuraColor = color
			};
			_nightmareBottomAppendage = new Appendage(this, bboxDimensions, Point.Zero, _level, _sprite)
			{
				FollowType = EAppendageFollowType.AnchorLocked,
				AnchorOffset = new Point(0, 73),
				DrawPriority = -1,
				DoesInheritDrawColor = false,
				DrawColor = color,
				IsFlippedVertically = true,
				DoesDrawAura = true,
				AuraCount = 4f,
				AuraColor = color
			};
			_nightmareTopAppendage.ChangeAnimation(4);
			_nightmareBottomAppendage.ChangeAnimation(4);
			base.Appendages.Add(_nightmareTopAppendage);
			base.Appendages.Add(_nightmareBottomAppendage);
			for (int i = 0; i < 60; i++)
			{
				UpdateNightmareGate(0.016666f);
				UpdateParticleSystems(0.016666f);
			}
		}
	}

	internal Point SpitOutPlayer(LevelChangeRequest levelChangeRequest)
	{
		Point result = WarpInPoint;
		EEraType eraFromLevelID = GetEraFromLevelID(levelChangeRequest.LevelID);
		EEraType eraFromLevelID2 = GetEraFromLevelID(levelChangeRequest.PreviousLevelID);
		bool flag = eraFromLevelID != eraFromLevelID2;
		_level.GameSave.LastWarpLevel = _level.ID;
		_level.GameSave.LastWarpRoom = _level.RoomID;
		if (flag)
		{
			Point point = new Point(Position.X + 8, Position.Y - 37);
			TimeGateEvent timeGateEvent = new TimeGateEvent(_level, point, -1, new ObjectTileSpecification());
			timeGateEvent.IsActive = true;
			timeGateEvent.TargetLevelID = levelChangeRequest.PreviousLevelID;
			timeGateEvent.CutsceneToCall = CutsceneToCall;
			TimeGateEvent newEvent = timeGateEvent;
			_isWaitingForTimeWarpToFinish = true;
			result = point;
			_level.AddEvent(newEvent);
			if (eraFromLevelID == EEraType.Present && !_level.GameSave.GetSaveBool("HasBeenToNewPresent") && _level.GameSave.GetSaveBool(BossClass.GetSaveKeyByBossType(EBossType.Demon)))
			{
				_level.GameSave.SetValue("HasBeenToNewPresent", value: true);
			}
		}
		else
		{
			IsSpittingOutPlayer = true;
		}
		return result;
	}

	private static EEraType GetEraFromLevelID(int levelID)
	{
		EEraType result = EEraType.Unknown;
		switch (levelID)
		{
		case 1:
		case 2:
		case 9:
		case 10:
		case 11:
		case 12:
			result = EEraType.Present;
			break;
		case 3:
		case 4:
		case 5:
		case 6:
		case 7:
		case 8:
			result = EEraType.Past;
			break;
		}
		return result;
	}

	public override void Update(float delta)
	{
		if (!_hasUpdatedOnce)
		{
			_hasUpdatedOnce = true;
			if (IsSpittingOutPlayer)
			{
				_level.IsUIRequestingHide = true;
			}
		}
		if (IsSpittingOutPlayer)
		{
			UpdatePlayerSpitSequence(delta);
		}
		else if (_isWaitingForTimeWarpToFinish)
		{
			_playerSpitSequenceTimer += delta;
			if (_playerSpitSequenceTimer >= 4.5f)
			{
				_isWaitingForTimeWarpToFinish = false;
			}
		}
		if (_isDoingKeySequence)
		{
			UpdateKeySequence(delta);
		}
		if (!base.IsFrozen)
		{
			if (!_isNightmareGate)
			{
				_keyCheckTimer += delta;
				bool doesPlayerHaveKey = _doesPlayerHaveKey;
				if (_keyCheckTimer >= 1f)
				{
					_doesPlayerHaveKey = _level.GameSave.Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.PyramidsKey);
					_keyCheckTimer -= 1f;
				}
				if (!doesPlayerHaveKey && _doesPlayerHaveKey)
				{
					_appendages.Add(_leftSlotAppendage);
					_appendages.Add(_rightSlotAppendage);
				}
				else if (doesPlayerHaveKey && !_doesPlayerHaveKey && !_isNightmareGate)
				{
					_appendages.Clear();
				}
				if (_doesPlayerHaveKey)
				{
					UpdateKeyGlow(delta);
				}
			}
			else
			{
				UpdateNightmareGate(delta);
			}
		}
		base.Update(delta);
	}

	private void UpdateNightmareGate(float delta)
	{
		_nightmareSmokeEmissionTimer -= delta;
		if (_nightmareSmokeEmissionTimer <= 0f)
		{
			_nightmareSmokeEmissionTimer += 0.03f;
			for (int i = 0; i < 3; i++)
			{
				int num = _level.NextRandomInt(16, 28);
				double num2 = _level.NextRandomDouble() * 6.2831854820251465;
				float num3 = (float)(Math.Cos(num2) * (double)num);
				float num4 = (float)(Math.Sin(num2) * (double)num);
				_nightmareSwirlParticles.AddParticles(new Vector2((float)WarpInPoint.X + num3, (float)WarpInPoint.Y + num4 + -1f));
			}
		}
	}

	private void UpdateKeyGlow(float delta)
	{
		_keyGlowTimer += delta * 3f;
		if (_keyGlowTimer >= (float)Math.PI * 2f)
		{
			_keyGlowTimer -= (float)Math.PI * 2f;
		}
		float alpha = (float)(Math.Sin(_keyGlowTimer) * 0.25 + 0.3499999940395355);
		Color glowColor = new Color(0.6f, 0.6f, 0.1f, alpha);
		_leftSlotAppendage.GlowColor = glowColor;
		_rightSlotAppendage.GlowColor = glowColor;
	}

	private void UpdateKeySequence(float delta)
	{
		_level.IsPreventingPauseMenuUsage = true;
		float keySpinTimer = _keySpinTimer;
		_keySpinTimer += delta;
		if (_keySpinTimer > 1.3f)
		{
			_level.RequestChangeLevel(_levelChangeRequest);
			return;
		}
		float num = 1f;
		if (_keySpinTimer < 1f)
		{
			num = MathEx.SineInterpolate(0f, 1f, _keySpinTimer / 1f);
		}
		if (_isWarpingToADifferentEra)
		{
			float num2 = MathHelper.Lerp(1f, 50f, num);
			_spindleAppendage.Rotation += delta * num2;
			if (_spindleAppendage.Rotation >= (float)Math.PI * 2f)
			{
				_spindleAppendage.Rotation -= (float)Math.PI * 2f;
			}
			_spindleAppendage.GlowColor = new Color(1f, 1f, 1f, 1f - num);
		}
		float num3 = MathEx.SineInterpolate(0f, 32f, num);
		float num4 = (float)Math.Cos(num * ((float)Math.PI * 2f)) * num3;
		float num5 = (float)Math.Sin(num * ((float)Math.PI * 2f));
		float num6 = num5 * num3;
		_leftKeyAppendage.Rotation = num5;
		_rightKeyAppendage.Rotation = num5;
		_leftKeyAppendage.Position = new Point(_keyOrigin.X + (int)num4, _keyOrigin.Y + (int)num6);
		_rightKeyAppendage.Position = new Point(_keyOrigin.X - (int)num4, _keyOrigin.Y + (int)num6);
		if (_keySpinTimer > 1f && keySpinTimer <= 1f)
		{
			AnimationSpec[] newAnimations = new AnimationSpec[1]
			{
				new AnimationSpec
				{
					Start = 26,
					Length = 5,
					Speed = 0.066f,
					Type = EAnimationType.Once
				}
			};
			_leftKeyAppendage.ChangeAnimation(newAnimations);
			_rightKeyAppendage.ChangeAnimation(newAnimations);
		}
	}

	private void UpdatePlayerSpitSequence(float delta)
	{
		float playerSpitSequenceTimer = _playerSpitSequenceTimer;
		_playerSpitSequenceTimer += delta;
		if (_playerSpitSequenceTimer > 0f && playerSpitSequenceTimer <= 0f)
		{
			_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.GateJump, 0f, 2f, new Vector4(WarpInPoint.X, WarpInPoint.Y, 0f, 0f)));
			AddDelegateScript(delegate
			{
				_level.IsUIRequestingHide = false;
			});
			PlayCue(_isNightmareGate ? ESFX.FoleyWarpGyreOut : ESFX.FoleyWarpCutsceneExit);
			AnimationSpec[] newAnimations = new AnimationSpec[3]
			{
				new AnimationSpec
				{
					Start = 14,
					Length = 1,
					Speed = 0.5f,
					Type = EAnimationType.Once
				},
				new AnimationSpec
				{
					Start = 15,
					Length = 5,
					Speed = 0.05f,
					Type = EAnimationType.Once
				},
				new AnimationSpec
				{
					Start = -1,
					Type = EAnimationType.None
				}
			};
			_leftGateAppendage.ChangeAnimation(newAnimations);
			_rightGateAppendage.ChangeAnimation(newAnimations);
		}
		else if (_playerSpitSequenceTimer > 1.75f)
		{
			IsSpittingOutPlayer = false;
		}
		else if (_playerSpitSequenceTimer < 0.75f)
		{
			_sparkleParticles.AddParticles(_particleEmissionPoint);
		}
		if (_playerSpitSequenceTimer < 0.75f)
		{
			_level.IsPreventingPauseMenuUsage = true;
		}
		if (_playerSpitSequenceTimer > 0.15f && _playerSpitSequenceTimer < 0.4f)
		{
			_entranceRingPercentage = (float)Math.Sin((float)Math.PI / 2f * (_playerSpitSequenceTimer - 0.15f) / 0.25f);
		}
		else
		{
			_entranceRingPercentage = 0f;
		}
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		if (!base.IsFrozen && (_doesPlayerHaveKey || _isNightmareGate) && !IsSpittingOutPlayer && !_isDoingKeySequence && !_isWaitingForTimeWarpToFinish)
		{
			Protagonist protagonist = who as Protagonist;
			_isTriggered = true;
			_level.RequestButtonPrompt(4, new Point(Bbox.Center.X - 2, Bbox.Top));
			if (protagonist != null && protagonist.CheckButton(4) && !protagonist.CheckButton(5) && !protagonist.CheckButton(7))
			{
				OnPlayerActivate();
			}
		}
		return base.TriggerEvent(who, depth);
	}

	private void OnPlayerActivate()
	{
		_level.AddScreen(new MapWarpScreen(_level.GCM, _level.Minimap, _level.ID, _level.RoomID, _level.Player1Controller, StartWarpSequence));
	}

	internal void StartWarpSequence(LevelChangeRequest levelRequest)
	{
		_levelChangeRequest = levelRequest;
		EEraType eraFromLevelID = GetEraFromLevelID(_level.ID);
		EEraType eraFromLevelID2 = GetEraFromLevelID(levelRequest.LevelID);
		_isWarpingToADifferentEra = eraFromLevelID != eraFromLevelID2;
		bool flag = _isWarpingToADifferentEra && !_isNightmareGate && eraFromLevelID2 == EEraType.Unknown;
		if (!_isNightmareGate && !flag)
		{
			_isDoingKeySequence = true;
			_level.JukeBox.PlayCue(ESFX.FoleyWarpCutsceneActivate);
			if (_isWarpingToADifferentEra)
			{
				_spindleAppendage.Position = new Point(Position.X, Position.Y + -62);
				_spindleAppendage.DoesInheritDrawColor = false;
				_spindleAppendage.IsGlowing = true;
				_spindleAppendage.GlowBase = 5f;
				_spindleAppendage.ChangeAnimation(31);
				base.Appendages.Add(_spindleAppendage);
			}
			_keyOrigin = new Point(Position.X, Bbox.Center.Y + 2);
			_leftKeyAppendage.Position = _keyOrigin;
			_rightKeyAppendage.Position = _keyOrigin;
			_leftKeyAppendage.ChangeAnimation(21, 5, 0.1f, EAnimationType.Cycle);
			_rightKeyAppendage.ChangeAnimation(21, 5, 0.1f, EAnimationType.Cycle);
			base.Appendages.Add(_leftKeyAppendage);
			base.Appendages.Add(_rightKeyAppendage);
		}
		_level.AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.StopCast
		});
		_level.AddScript(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.Idle,
			ActionTimer = 1.3f
		});
		if (_isNightmareGate || flag)
		{
			_level.JukeBox.PlayCue(ESFX.FoleyWarpGyreIn);
			_levelChangeRequest.PreviousLevelID = _levelChangeRequest.LevelID;
			_level.RequestChangeLevel(_levelChangeRequest);
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
		if (IsSpittingOutPlayer && _entranceRingPercentage > 0f && _entranceRingPercentage < 1f)
		{
			int num = (int)(128f * _entranceRingPercentage);
			Color color = Color.White * (1f - _entranceRingPercentage);
			spriteBatch.Draw(destinationRectangle: new Rectangle(_entranceRingDrawCenter.X - num / 2, _entranceRingDrawCenter.Y - num / 2, num, num), texture: _entranceRingTexture, color: color);
		}
	}
}
