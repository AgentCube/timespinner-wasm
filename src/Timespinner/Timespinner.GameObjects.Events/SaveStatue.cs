using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;
using Timespinner.GameObjects.StatusEffects;

namespace Timespinner.GameObjects.Events;

public sealed class SaveStatue : GameEvent
{
	private enum ESaveOrbState
	{
		Dead,
		Glowing,
		Falling
	}

	private const int Anim_OrbStart = 1;

	private const int Anim_OrbCrackedStart = 5;

	private const float OscillFrequencyY = (float)Math.PI;

	private const float OscillRadius = 3f;

	private const float OrbFallRate = 10f;

	private const float OrbGlowFrequency = (float)Math.PI;

	private const float TimeForGlowToFade = 0.5f;

	private const float SubGlowColorPercentage = 0.8f;

	private const float TimeToWaitBeforeBeingUsable = 0.1f;

	private static readonly Vector4 WhiteColorVector4 = Color.White.ToVector4();

	private static readonly Vector4 BaseOrbGlowColor = new Vector4(0.3f, 0.6f, 1f, 1f);

	private readonly bool _isBroken;

	private readonly int _whichCheckpoint;

	private readonly SaveStatueLeakParticleSystem _pixelLeakParticleSystem;

	private readonly BattleAnimation _outerGlowAnimation;

	private readonly Appendage _orbAppendage;

	private readonly Appendage _dragonAppendage;

	private readonly Appendage _nearPedestalLeftAppendage;

	private readonly Appendage _nearPedestalRightAppendage;

	private bool _isFound;

	private bool _isTryingToSave;

	private ESaveOrbState _orbSaveState;

	private float _saveCoolDown;

	private float _oscillDelta;

	private float _lastYShift;

	private float _glowFadeTimer;

	private float _timeSinceBeingTouched;

	private Vector4 _orbGlowColor;

	private SFXCueInstance _glowCueInstance;

	public bool IsFound => _isFound;

	public int GetID => _whichCheckpoint;

	public Point OrbBasePosition => new Point(_position.X + 1, _position.Y - _bbox.Height);

	public SaveStatue(Level inLevel, SpriteSheet inSprite, Point inPosition, int checkpointID, bool inFound, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_sprite = inSprite;
		_bboxOffset = new Point(3, 4);
		_bbox = new Rectangle(inPosition.X, inPosition.Y + 8, 26, 26);
		_isFound = inFound;
		_whichCheckpoint = checkpointID;
		IsFacingLeft = true;
		_isAffectedByGravity = false;
		if (objectSpec != null)
		{
			if (objectSpec.Argument == 1 || objectSpec.Argument == 3)
			{
				Position = new Point(Position.X - 8, Position.Y);
			}
			if (_level.IsHardMode && (objectSpec.Argument == 2 || objectSpec.Argument == 3))
			{
				_isBroken = true;
				base.IsAffectedByTime = true;
			}
		}
		base.EventType = EEventTileType.Checkpoint;
		_doesPersist = true;
		_isRepeatedTrigger = true;
		_orbAppendage = new Appendage(this, new Point(24, 24), new Point(5, 5), _level, _sprite);
		_dragonAppendage = new Appendage(_orbAppendage, new Point(28, 16), new Point(2, 0), _level, _sprite)
		{
			AnchorObject = _orbAppendage,
			AnchorOffset = new Point(-1, -19),
			FollowType = EAppendageFollowType.AnchorLocked
		};
		_nearPedestalLeftAppendage = new Appendage(this, new Point(16, 12), new Point(0, 2), _level, _sprite)
		{
			FollowType = EAppendageFollowType.ParentObjectLocked,
			AnchorObject = this,
			AnchorOffset = new Point(-9, -_bbox.Height)
		};
		_nearPedestalRightAppendage = new Appendage(this, new Point(16, 12), new Point(0, 2), _level, _sprite)
		{
			FollowType = EAppendageFollowType.ParentObjectLocked,
			AnchorObject = this,
			AnchorOffset = new Point(-9, -_bbox.Height),
			IsFacingLeft = false
		};
		PopulateAppendages();
		_orbSaveState = ((!_isBroken) ? ESaveOrbState.Glowing : ESaveOrbState.Dead);
		_pixelLeakParticleSystem = new SaveStatueLeakParticleSystem(_level.GCM.TxParticleEnergy, 10)
		{
			BaseColor = new Vector4(0.5f, 0.75f, 1f, 1f)
		};
		_particleSystems.Add(_pixelLeakParticleSystem);
		_outerGlowAnimation = new BattleAnimation(_level.GCM.SpEffectsMedium, _bbox.Center, _level)
		{
			TeamSide = ETeamSide.Heroes,
			AnchorObject = _orbAppendage,
			AnchorOffset = new Point(-1, -13),
			DrawColor = Color.White * 0.9f,
			DoesRepeat = true,
			AnimationSpeed = 0.066f,
			AnimationStart = 35,
			AnimationLength = 4
		};
		if (!_isBroken)
		{
			_orbAppendage.AddBattleAnimation(_outerGlowAnimation);
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		if (_orbSaveState != 0)
		{
			_glowCueInstance = PlayCue(ESFX.FoleySaveStatueIdle, isLooped: true);
			if (_glowCueInstance != null)
			{
				_glowCueInstance.UpdateType = SFXCueInstance.ECueInstanceUpdateType.Protagonist;
				_glowCueInstance.Protagonist = _level.MainHero;
				_glowCueInstance.FadeIn(0.25f);
				_glowCueInstance.Update(0f);
			}
		}
	}

	private void PopulateAppendages()
	{
		Appendage appendage = new Appendage(this, new Point(16, 12), new Point(0, 2), _level, _sprite);
		appendage.FollowType = EAppendageFollowType.ParentObjectLocked;
		appendage.AnchorObject = this;
		appendage.AnchorOffset = new Point(-8, -_bbox.Height);
		Appendage appendage2 = appendage;
		appendage2.ChangeAnimation(3);
		_appendages.Add(appendage2);
		Appendage appendage3 = new Appendage(this, new Point(16, 12), new Point(0, 2), _level, _sprite);
		appendage3.FollowType = EAppendageFollowType.ParentObjectLocked;
		appendage3.AnchorObject = this;
		appendage3.AnchorOffset = new Point(-8, -_bbox.Height);
		appendage3.IsFacingLeft = false;
		Appendage appendage4 = appendage3;
		appendage4.ChangeAnimation(3);
		_appendages.Add(appendage4);
		_orbAppendage.ChangeAnimation((!_isBroken) ? 1 : 5);
		_appendages.Add(_orbAppendage);
		_dragonAppendage.ChangeAnimation(2);
		if (!_isBroken)
		{
			_appendages.Add(_dragonAppendage);
		}
		_nearPedestalLeftAppendage.ChangeAnimation(4);
		_appendages.Add(_nearPedestalLeftAppendage);
		_nearPedestalRightAppendage.ChangeAnimation(4);
		_appendages.Add(_nearPedestalRightAppendage);
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		base.TriggerEvent(who, depth);
		if (!_isBroken)
		{
			if (_timeSinceBeingTouched >= 0.1f)
			{
				if (!_isFound)
				{
					_isFound = true;
				}
				_isTryingToSave = true;
			}
			_timeSinceBeingTouched = 0f;
			if (!base.IsTriggerableByMonsters && who is Protagonist protagonist)
			{
				protagonist.HealHP(25);
				protagonist.HealAura(25);
				protagonist.ManageManaRestore(25f);
				protagonist.HealStatus(EStatusEffectType.All);
			}
		}
		return false;
	}

	public override void Update(float delta)
	{
		UpdateOrb(delta);
		if (_saveCoolDown > 0f)
		{
			_saveCoolDown -= delta;
			if (_saveCoolDown < 0f)
			{
				_saveCoolDown = 0f;
			}
		}
		if (_timeSinceBeingTouched < 0.1f)
		{
			_timeSinceBeingTouched += delta;
		}
		if (_isTryingToSave)
		{
			if (_isBeingTriggered && !_wasBeingTriggered && _saveCoolDown <= 0f)
			{
				_level.SaveTheGame(_whichCheckpoint);
				_saveCoolDown = 3f;
				if (_orbSaveState == ESaveOrbState.Glowing)
				{
					TurnOffOrb();
				}
				else
				{
					PlayCue(ESFX.FoleySaveStatueTouchAgain);
				}
			}
			_isTryingToSave = false;
		}
		base.Update(delta);
	}

	private void TurnOffOrb()
	{
		_orbSaveState = ESaveOrbState.Falling;
		_level.AddAnimation(EBattleAnimationType.BigRipple, _orbAppendage.Bbox.Center);
		PlayCue(ESFX.FoleySaveStatueFlash, Position);
		if (_glowCueInstance != null)
		{
			_glowCueInstance.Stop();
		}
		_orbAppendage.ClearBattleAnimations();
		_glowFadeTimer = 0.5f;
	}

	private void UpdateOrb(float delta)
	{
		float num = 0f;
		switch (_orbSaveState)
		{
		case ESaveOrbState.Glowing:
		{
			_oscillDelta += delta;
			if (_oscillDelta > 10f)
			{
				_oscillDelta -= 10f;
			}
			num = (float)Math.Sin((0f - _oscillDelta) * (float)Math.PI) * 3f;
			num -= 5f;
			_orbGlowColor = BaseOrbGlowColor;
			_orbGlowColor.W = (float)Math.Cos(_oscillDelta * (float)Math.PI) * 0.25f + 0.35f;
			_orbAppendage.IsGlowing = true;
			_orbAppendage.GlowColor = new Color(_orbGlowColor);
			Vector4 color = _orbGlowColor.EaseTo(WhiteColorVector4, 0.8f);
			Color glowColor2 = new Color(color);
			_nearPedestalLeftAppendage.IsGlowing = true;
			_nearPedestalRightAppendage.IsGlowing = true;
			_dragonAppendage.IsGlowing = true;
			_nearPedestalLeftAppendage.GlowColor = glowColor2;
			_nearPedestalRightAppendage.GlowColor = glowColor2;
			_dragonAppendage.GlowColor = glowColor2;
			_pixelLeakParticleSystem.AddParticles(_orbAppendage.Bbox.Center.ToVector2());
			break;
		}
		case ESaveOrbState.Falling:
			num = _lastYShift + delta * 10f;
			if (num >= 0f)
			{
				_orbSaveState = ESaveOrbState.Dead;
				num = 0f;
			}
			if (_glowFadeTimer > 0f)
			{
				_glowFadeTimer -= delta;
				if (_glowFadeTimer > 0f)
				{
					float amount = 1f - _glowFadeTimer / 0.5f;
					Vector4 vector = _orbGlowColor.EaseTo(WhiteColorVector4, amount);
					_orbAppendage.GlowColor = new Color(vector);
					Color glowColor = new Color(vector.EaseTo(WhiteColorVector4, 0.8f));
					_nearPedestalLeftAppendage.GlowColor = glowColor;
					_nearPedestalRightAppendage.GlowColor = glowColor;
					_dragonAppendage.GlowColor = glowColor;
				}
				else
				{
					_orbAppendage.IsGlowing = false;
					_nearPedestalLeftAppendage.IsGlowing = false;
					_nearPedestalRightAppendage.IsGlowing = false;
					_dragonAppendage.IsGlowing = false;
				}
			}
			break;
		}
		_orbAppendage.Position = new Point(OrbBasePosition.X, (int)Math.Round((float)OrbBasePosition.Y + num));
		_lastYShift = num;
	}

	internal void HideDragon()
	{
		if (_dragonAppendage != null)
		{
			_dragonAppendage.ChangeAnimation(-1);
		}
	}

	internal void StopGlowingNow()
	{
		_orbSaveState = ESaveOrbState.Dead;
		_orbAppendage.IsGlowing = false;
		_nearPedestalLeftAppendage.IsGlowing = false;
		_nearPedestalRightAppendage.IsGlowing = false;
		_dragonAppendage.IsGlowing = false;
		if (_glowCueInstance != null)
		{
			_glowCueInstance.Stop();
		}
		if (_orbAppendage != null)
		{
			_orbAppendage.ClearBattleAnimations();
		}
	}
}
