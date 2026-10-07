using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Misc;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Events.Relics;

internal sealed class TimespinnerWheelItem : GameEvent
{
	private const string SaveKey = "RELIC_CUTSCENE_WHEEL";

	private const float GlowFrequency = 1f;

	private const float GlowAmplitude = 1f;

	private const float GlowOffset = 2f;

	private const float GlowGrowthRate = 10f;

	private const float GlowMaxGrowth = 10f;

	private const float FloatHeight = 4f;

	private const float FloatFrequency = 1f;

	private const float SpinGrowthRate = 1f;

	private const float SpinMaxSpeed = 5f;

	private const float SpinMultiplier = 0.2f;

	private readonly Point _startingPoint;

	private readonly TimespinnerWheelLeakParticleSystem _tinySparkles;

	private bool _isSpinning;

	private bool _isFirstEncounter = true;

	private float _glowDelta;

	private float _glowGrowthDelta;

	private float _floatDelta;

	private float _spinSpeed;

	private TimespinnerAbsorbAnimation _absorbAnimation;

	private SoulStreamEvent _soulStream;

	public TimespinnerWheelItem(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_startingPoint = inPosition;
		_sprite = _level.GCM.SpTheTimespinner;
		Bbox = new Rectangle(0, 0, 51, 51);
		base.TriggerBbox = new Rectangle(0, 0, 150, 150);
		DrawOrigin = new Vector2(25.5f, 25.5f);
		ChangeAnimation(1);
		_isRepeatedTrigger = false;
		_isAffectedByGravity = false;
		_isFlying = true;
		_isAffectedByTime = false;
		_isGlowing = true;
		_glowBase = 1f;
		_glowColor = new Color(1f, 0.9f, 0.9f, 0.5f);
		base.DoesDrawAura = true;
		base.AuraColor = new Color(1f, 0.75f, 0.75f, 0.3f);
		base.AuraSize = 0.05f;
		base.AuraOffset = new Vector2(1f, 1f);
		base.AuraFrequency = 8f;
		_tinySparkles = new TimespinnerWheelLeakParticleSystem(_level.GCM.TxParticleEnergy, 10);
		_particleSystems.Add(_tinySparkles);
	}

	public override void Initialize()
	{
		if (_level.GameSave.GetSaveBool("RELIC_CUTSCENE_WHEEL"))
		{
			_isFirstEncounter = false;
			SilentKill();
		}
		else
		{
			_level.FreezeTime(ETeamSide.Heroes, reFreeze: false);
			_level.JukeBox.FadeOutSong(0.25f);
			base.Initialize();
		}
	}

	public override void Update(float delta)
	{
		_tinySparkles.AddParticles(Bbox.Center.ToVector2());
		_glowDelta += delta;
		if (_glowDelta > 614f)
		{
			_glowDelta -= 614f;
		}
		_glowBase = 2f + (float)Math.Sin((double)_glowDelta * Math.PI * 1.0) * 1f + _glowGrowthDelta;
		_floatDelta += delta;
		if (_floatDelta > 614f)
		{
			_floatDelta -= 614f;
		}
		Position = _startingPoint.Add(0, (int)(Math.Sin((double)_floatDelta * Math.PI * 1.0) * 4.0));
		if (_isSpinning)
		{
			if (_spinSpeed < 5f)
			{
				_spinSpeed += delta * 1f;
				if (_spinSpeed > 5f)
				{
					_spinSpeed = 5f;
				}
			}
			base.Rotation += _spinSpeed * 0.2f;
			if (base.Rotation > 614f)
			{
				base.Rotation -= 614f;
			}
			_glowGrowthDelta += delta * 10f;
			if (_glowGrowthDelta > 10f)
			{
				_glowGrowthDelta = 10f;
			}
		}
		if (_soulStream != null && _isSpinning)
		{
			_soulStream.Update(delta);
		}
		base.Update(delta);
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		if (!_isTriggered)
		{
			StartCutscene();
		}
		return base.TriggerEvent(who, depth);
	}

	public override void SilentKill()
	{
		if (_isFirstEncounter)
		{
			_level.UnfreezeTime(ETeamSide.Heroes);
			if (_absorbAnimation != null)
			{
				_absorbAnimation.End();
			}
			if (_soulStream != null)
			{
				_soulStream.End();
			}
		}
		base.SilentKill();
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (_soulStream != null)
		{
			_soulStream.Draw(spriteBatch);
		}
		base.Draw(spriteBatch);
	}

	private void StartCutscene()
	{
		Protagonist mainHero = _level.MainHero;
		_absorbAnimation = new TimespinnerAbsorbAnimation(_level, Bbox.Center, _level.MainHero, 4f)
		{
			TeamSide = ETeamSide.Heroes
		};
		_soulStream = new SoulStreamEvent(_level, Bbox.Center, isAnchoredToPlayer: true)
		{
			DrawPlane = EDrawPlane.Back
		};
		AddLevelScriptAction(new ScriptAction
		{
			ScriptType = EScriptType.CutsceneStart,
			DoesBlockQueue = false,
			Arguments = new Vector4(1f, 0f, 0f, 0f)
		});
		AddLevelScriptAction(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.SheatheWeapon
		});
		AddLevelScriptAction(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.Run,
			ActionTimer = 0.033f,
			DoesBlockQueue = true,
			Arguments = new Vector4(1f, 0f, 0f, 0f)
		});
		AddLevelScriptAction(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.Idle,
			ActionTimer = 0.5f,
			DoesBlockQueue = true
		});
		AddDialogue("cs_relic_wheel_01");
		AddDialogue("cs_relic_wheel_02");
		AddDialogue("cs_relic_wheel_03");
		AddLevelScriptAction(new ScriptAction(new AnimationSpec
		{
			Start = 38,
			Length = 1,
			Speed = 0.066f,
			Type = EAnimationType.Once
		}, mainHero)
		{
			DoesBlockQueue = true
		});
		AddLevelScriptAction(new ScriptAction(new AnimationSpec
		{
			Start = 39,
			Length = 5,
			Speed = 0.066f,
			Type = EAnimationType.Cycle
		}, mainHero));
		AddLevelScriptAction(new ScriptAction
		{
			ScriptType = EScriptType.Delegate,
			Delegate = StartSpinning
		});
		AddLevelScriptAction(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.Idle,
			ActionTimer = 4f,
			DoesBlockQueue = false
		});
		AddWaitScript(4f);
		AddLevelScriptAction(new ScriptAction
		{
			ScriptType = EScriptType.Delegate,
			Delegate = delegate
			{
				_level.RequestScreenFlash(1f, 1f, 1f);
			},
			ActionTimer = 0.5f,
			DoesBlockQueue = true
		});
		AddLevelScriptAction(new ScriptAction
		{
			ScriptType = EScriptType.Delegate,
			Delegate = SilentKill
		});
		AddLevelScriptAction(new ScriptAction(new AnimationSpec
		{
			Start = 38,
			Length = 1,
			Speed = 0.066f,
			Type = EAnimationType.Once
		}, mainHero)
		{
			DoesBlockQueue = true
		});
		AddLevelScriptAction(new ScriptAction(new AnimationSpec
		{
			Start = 0,
			Length = 5,
			Speed = 0.11f,
			Type = EAnimationType.Cycle
		}, mainHero));
		AddLevelScriptAction(new ScriptAction
		{
			TargetType = EScriptTargetType.Player1,
			ActionType = EScriptActionType.SheatheWeapon,
			Arguments = new Vector4(1f, 0f, 0f, 0f)
		});
		AddWaitScript(0.1f);
		AddLevelScriptAction(new ScriptAction(EInventoryRelicType.TimespinnerWheel));
		AddWaitScript(0.1f);
		AddLevelScriptAction(new ScriptAction
		{
			ScriptType = EScriptType.Delegate,
			Delegate = EndCutscene
		});
	}

	private void StartSpinning()
	{
		_isSpinning = true;
		base.AuraColor = Color.Blue * 0.6f;
		if (_absorbAnimation != null)
		{
			_level.AddAnimation(_absorbAnimation);
		}
	}

	private void EndCutscene()
	{
		_level.UnlockRelic(EInventoryRelicType.TimespinnerWheel);
		_level.GameSave.SetValue("RELIC_CUTSCENE_WHEEL", value: true);
		_level.JukeBox.PlaySong(EBGM.Level01, shouldForceRestart: false, shouldImmediatelyStopPreviousSong: false);
	}
}
