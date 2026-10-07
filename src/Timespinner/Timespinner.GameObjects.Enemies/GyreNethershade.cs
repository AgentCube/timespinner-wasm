using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Etc;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Enemies;

internal sealed class GyreNethershade : Monster
{
	private const int Anim_SwirlIndex = 11;

	private const int SwirlParticlesOffsetY = 2;

	private const int BestiaryFrames = 20;

	private const float BestiaryDelta = 0.1f;

	private const float TimeBeforeRetargeting = 3f;

	private const float BaseSwimVelocity = 25f;

	private const float AggroSwimVelocity = 350f;

	private const float TrackingMultiplier = 3.5f;

	private static readonly Color ShockwaveColor = new Color(0.4f, 0.3f, 0.6f, 0.25f);

	private readonly int _baseDamage;

	private readonly Point _spawnPoint;

	private readonly CharacterSequenceSpecification _idleSequence;

	private readonly CharacterSequenceSpecification _moveSequence;

	private readonly NethershadeOilParticleSystem _oilParticles;

	private readonly NethershadeSmokeParticleSystem _smokeParticles;

	private readonly NethershadeSwirlParticleSystem _swirlParticles;

	private readonly Appendage _headAppendage;

	private readonly List<BoundingBoxParticleEmitter> _oilParticleEmitters = new List<BoundingBoxParticleEmitter>();

	private readonly List<BoundingBoxParticleEmitter> _smokeParticleEmitters = new List<BoundingBoxParticleEmitter>();

	private bool _isChasingHero;

	private float _targetRotation;

	private float _actionTimer;

	private SFXCueInstance _chaseLoopCueInstance;

	private SFXCueInstance _chaseLoopCueInstance2D;

	public GyreNethershade(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_doesDrawBaseSprite = false;
		_doesDrawParticleSystemsUnder = true;
		ChangeAnimation(-1);
		Bbox = new Rectangle(0, 0, 32, 32);
		_doAppendagesMatchImageFacing = true;
		_doAppendagesInheritDrawColor = true;
		_baseDamage = base.Damage;
		_currentAI = EAIStrategy.CustomScriptAI;
		_isAlwaysAggroed = true;
		_isAffectedByGravity = false;
		_isFlying = true;
		_agility = 1f;
		base.IsAffectedByTime = false;
		base.IsSolidWhenFrozen = false;
		base.DoesCollideWithTiles = false;
		_doesUseAppendageCollision = false;
		_isAffectedByLevelBounds = false;
		_doesIgnoreOutOfBoundsDeath = true;
		_spawnPoint = inPosition;
		_smokeParticles = new NethershadeSmokeParticleSystem(_sprite, 128);
		_particleSystems.Add(_smokeParticles);
		_oilParticles = new NethershadeOilParticleSystem(_sprite, 256);
		_particleSystems.Add(_oilParticles);
		_swirlParticles = new NethershadeSwirlParticleSystem(_sprite, 8, 11);
		_particleSystems.Add(_swirlParticles);
		if (_appendages.Count > 0)
		{
			_headAppendage = _appendages[0];
		}
		int num = _level.NextRandomInt(0, 256);
		foreach (Appendage appendage in _appendages)
		{
			if (appendage.DoesInheritDrawColor)
			{
				BoundingBoxParticleEmitter item = new BoundingBoxParticleEmitter(appendage, _oilParticles, num++)
				{
					InstancesPerEmission = 2,
					TimeBetweenEmissions = 0.033f
				};
				_oilParticleEmitters.Add(item);
				BoundingBoxParticleEmitter item2 = new BoundingBoxParticleEmitter(appendage, _smokeParticles, num++)
				{
					InstancesPerEmission = 2,
					TimeBetweenEmissions = 0.033f
				};
				_smokeParticleEmitters.Add(item2);
			}
		}
		ToggleTimeState(isTimeFrozen: false);
		_idleSequence = GetCharacterSequenceByName("Idle");
		_moveSequence = GetCharacterSequenceByName("Move");
	}

	public override void Update(float delta)
	{
		bool isTimeFrozen = _level.IsTimeFrozen;
		if (_isChasingHero != isTimeFrozen)
		{
			ToggleTimeState(_level.IsTimeFrozen);
		}
		UpdateRotation(delta);
		UpdateParticleEmitters(delta);
		base.Update(delta);
		if (_isChasingHero)
		{
			base.IsWithinObjectVisibleArea = true;
		}
	}

	private void UpdateParticleEmitters(float delta)
	{
		if (_isRunningDeathScript)
		{
			return;
		}
		foreach (BoundingBoxParticleEmitter oilParticleEmitter in _oilParticleEmitters)
		{
			oilParticleEmitter.Update(delta);
		}
		if (!_isChasingHero)
		{
			return;
		}
		foreach (BoundingBoxParticleEmitter smokeParticleEmitter in _smokeParticleEmitters)
		{
			smokeParticleEmitter.Update(delta);
		}
		if (_headAppendage != null)
		{
			Point center = _headAppendage.Bbox.Center;
			_swirlParticles.AddParticles(new Vector2(center.X, center.Y + 2));
		}
	}

	private void UpdateRotation(float delta)
	{
		if (!_isRunningDeathScript)
		{
			if (Math.Abs(base.Rotation - _targetRotation) < 0.1f)
			{
				_actionTimer = 3f;
				base.Rotation = _targetRotation;
			}
			else if (_targetRotation > base.Rotation)
			{
				base.Rotation += 3.5f * delta;
			}
			else
			{
				base.Rotation -= 3.5f * delta;
			}
		}
	}

	protected override void PickNextCustomScriptAIAction(float delta)
	{
		_currentAction = EAIAction.Custom;
	}

	protected override void UpdateCustomScriptAIAction(float delta)
	{
		_actionTimer += delta;
		if (_actionTimer >= 3f)
		{
			_actionTimer -= 3f;
			_startPosition = _position;
			if (_isChasingHero)
			{
				Protagonist nearestProtagonist = _level.GetNearestProtagonist(_position);
				if (nearestProtagonist != null)
				{
					Point position = nearestProtagonist.Position;
					int height = nearestProtagonist.Bbox.Height;
					Point targetPosition = new Point(position.X, position.Y - height);
					_targetPosition = targetPosition;
				}
				else
				{
					_targetPosition = FindRandomNearbyOpenArea();
				}
			}
			else
			{
				_targetPosition = FindRandomNearbyOpenArea();
			}
			_targetRotation = (float)Math.Atan2(_targetPosition.X - _startPosition.X, -(_targetPosition.Y - _startPosition.Y)) + (float)Math.PI / 2f;
			if (_targetRotation < 0f && base.Rotation > (float)Math.PI)
			{
				_targetRotation += (float)Math.PI * 2f;
			}
		}
		ManageState(EAFSM.Moving);
		float num = (_isChasingHero ? 350f : 25f);
		_velocity = new Vector2(0f - (float)Math.Cos(base.Rotation), (float)(0.0 - Math.Sin(base.Rotation))) * num;
		IsFacingLeft = _velocity.X <= 0f;
	}

	private Point FindRandomNearbyOpenArea()
	{
		return _spawnPoint;
	}

	private void ToggleTimeState(bool isTimeFrozen)
	{
		if (isTimeFrozen)
		{
			_isChasingHero = true;
			_damageCaused = _baseDamage;
			_canBeDamaged = true;
			_timeToIdleAfterMoving = 0f;
			_timeToWaitAfterArriving = 0f;
			SetCharacterSequence(_moveSequence);
			PlayCue(ESFX.EnemyNethershadeAggro);
			if (_chaseLoopCueInstance == null)
			{
				_chaseLoopCueInstance = CreateCue(ESFX.EnemyNethershadeLoop, Position, isLooped: true);
				if (_chaseLoopCueInstance != null)
				{
					_chaseLoopCueInstance.PlayWhenInRange();
					_chaseLoopCueInstance.RangeMultiplier = 1.5f;
				}
			}
			else if (_chaseLoopCueInstance.IsPaused)
			{
				_chaseLoopCueInstance.Resume();
			}
			if (_chaseLoopCueInstance2D == null)
			{
				_chaseLoopCueInstance2D = CreateCue2D(ESFX.EnemyNethershadeLoop2D, isLooped: true);
				if (_chaseLoopCueInstance2D != null)
				{
					_chaseLoopCueInstance2D.PlayWhenInRange();
				}
			}
			else if (_chaseLoopCueInstance2D.IsPaused)
			{
				_chaseLoopCueInstance2D.Resume();
			}
			UpdateCustomScriptAIAction(0f);
			base.Rotation = _targetRotation;
			return;
		}
		if (_isChasingHero)
		{
			PlayCue(ESFX.EnemyNethershadeUnAggro);
			if (_chaseLoopCueInstance != null && !_chaseLoopCueInstance.IsPaused)
			{
				_chaseLoopCueInstance.Pause(0.15f);
			}
			if (_chaseLoopCueInstance2D != null && !_chaseLoopCueInstance2D.IsPaused)
			{
				_chaseLoopCueInstance2D.Pause(0.15f);
			}
		}
		_isChasingHero = false;
		_damageCaused = 0;
		_canBeDamaged = false;
		SetCharacterSequence(_idleSequence);
	}

	internal override void InitializeForBestiary()
	{
		SetCharacterSequence(_moveSequence);
		for (int i = 0; i < 20; i++)
		{
			Update(0.1f);
		}
		base.InitializeForBestiary();
	}

	protected override void UpdateDeathScript(float delta)
	{
		NethershadeDeathSmokeParticleSystem particleSystem = new NethershadeDeathSmokeParticleSystem(_level.GCM.TxParticleSmoke, 1);
		ShockwaveAnimation shockwaveAnimation = new ShockwaveAnimation(_level.GCM.SpOrbMeleeBarrier, Bbox.Center, _level, ShockwaveColor);
		shockwaveAnimation.ParticleSystem = particleSystem;
		shockwaveAnimation.TeamSide = ETeamSide.Heroes;
		ShockwaveAnimation newAnimation = shockwaveAnimation;
		_level.AddAnimation(newAnimation);
		_level.PlayCue(ESFX.EnemyNethershadeDeath, Position);
		DropLootAndRemove();
	}
}
