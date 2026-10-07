using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Misc;
using Timespinner.GameObjects.StatusEffects;

namespace Timespinner.GameObjects.Enemies;

internal sealed class EmpRoyalGuard : Monster
{
	private const int DeathPushOffset = 32;

	private const float TimeDeadBeforeBurning = 0.5f;

	private const float AggroAnimationSpeed = 0.1f;

	private const float TimeBeforeAggroFlying = 0.5f;

	private const float TimeToHoverAfterAggroJumping = 0.5f;

	private const float AggroJumpInitialVelocityY = -200f;

	private static readonly Point DefaultDustParticleEmissionOffset = new Point(0, 0);

	private readonly LandingDustParticleSystem _dustParticles;

	private readonly CharacterSequenceSpecification _aggroSequence;

	private readonly CharacterSequenceSpecification _turnSequence;

	private readonly CharacterSequenceSpecification _castSequence;

	private readonly CharacterSequenceSpecification _deathSequence;

	private readonly EmpRoyalGuardProjectile _projectile;

	private bool _hasBeenAggroed;

	private bool _hasPlayedCastCueButHasNotCast;

	private float _aggroJumpTimer;

	private float _aggroHoverTimer;

	private float _castCooldownTimer;

	private Point _deathPoint;

	internal Point DustParticleEmissionOffset
	{
		get
		{
			if (!IsImageFacingLeft)
			{
				return new Point(-DefaultDustParticleEmissionOffset.X, DefaultDustParticleEmissionOffset.Y);
			}
			return DefaultDustParticleEmissionOffset;
		}
	}

	public EmpRoyalGuard(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		IsImageFacingLeft = IsFacingLeft;
		_nonAggroStrategy = EAIStrategy.None;
		_currentAI = EAIStrategy.None;
		_movementType = EAIMovementType.Fly;
		_attackDistanceThresholdX = 128;
		_retreatDistanceThresholdX = 60;
		base.FollowAttackTargetOffset = new Point(0, 32);
		_agility = 0.25f;
		_timeToMove = 0.5f;
		Bbox = new Rectangle(_position.X, _position.Y, 32, 32);
		_doesDrawBaseSprite = false;
		_doAppendagesMatchImageFacing = true;
		_timeToTurnAround = 0.15f;
		base.CannotBeGrabbed = true;
		base.DoesTouchDamageKnockback = true;
		base.DoesDrawAura = true;
		base.DoesDrawAppendageAuras = true;
		base.AuraColor = new Color(0.3f, 0.25f, 0.7f, 0.25f);
		base.AuraOffset = new Vector2(2f, 2f);
		base.AuraFrequency = 9f;
		base.AuraSize = 0.075f;
		_auraCount = 5f;
		SetDoesDrawAppendageTrails(value: true, isHost: true, 6, 3f);
		_dustParticles = new LandingDustParticleSystem(_level.GCM.TxParticleDust, 10, _level.ID, 100);
		_particleSystems.Add(_dustParticles);
		_projectile = new EmpRoyalGuardProjectile(_level, Position, Vector2.Zero, ETeamSide.Enemies, _sprite, base.Damage);
		if (base.CharacterSpecification != null && base.CharacterSpecification.Sequences.Count > 4)
		{
			_aggroSequence = base.CharacterSpecification.Sequences[1];
			_turnSequence = base.CharacterSpecification.Sequences[2];
			_castSequence = base.CharacterSpecification.Sequences[3];
			_deathSequence = base.CharacterSpecification.Sequences[4];
		}
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_aggroJumpTimer > 0f)
			{
				_aggroJumpTimer -= delta;
				if (_aggroJumpTimer <= 0f)
				{
					_aggroJumpTimer = 0f;
					_aggroHoverTimer = 0.5f;
					_isGrounded = false;
					_isAffectedByGravity = false;
					_isFlying = true;
					base.DoesCollideWithTiles = false;
					_isAlwaysAggroed = true;
					_velocity = new Vector2(0f, -200f);
					_dustParticles.AddParticles(Position.ToVector2(), 32f);
					base.OnAggroed();
				}
			}
			else if (_aggroHoverTimer > 0f)
			{
				_aggroHoverTimer -= delta;
				if (_aggroHoverTimer <= 0f)
				{
					_aggroHoverTimer = 0f;
					_currentAI = EAIStrategy.FollowAttack;
					_nonAggroAction = EAIAction.FloatInPlace;
				}
				else
				{
					float num = _aggroHoverTimer / 0.5f;
					_velocity = new Vector2(0f, -200f * num);
				}
			}
			if (_castCooldownTimer > 0f)
			{
				_castCooldownTimer -= delta;
			}
		}
		base.Update(delta);
	}

	protected override void OnAggroed()
	{
		if (!_hasBeenAggroed)
		{
			PlayCue(ESFX.EnemyRoyalGuardAggro);
			SetCharacterSequence(_aggroSequence);
			_aggroJumpTimer = 0.5f;
		}
		else
		{
			base.OnAggroed();
		}
		_hasBeenAggroed = true;
	}

	protected override void DoTurningAroundAnimation(EAFSM lastState, bool lastFacingRight)
	{
		SetCharacterSequence(_turnSequence);
	}

	public override void StartAbility(int whichAbility)
	{
		if (_castCooldownTimer <= 0f)
		{
			base.StartAbility(whichAbility);
			return;
		}
		_isCarryingOutAbility = false;
		_currentAction = EAIAction.Idle;
		_nextActionTimer = 0f;
	}

	public override void UpdateAbility(float delta)
	{
		if (_abilityTimer <= 0f)
		{
			PlayCue(ESFX.EnemyRoyalGuardSpellPrep);
			SetCharacterSequence(_castSequence);
		}
	}

	internal override void TriggerCharacterAction(CharacterAction specification)
	{
		if (specification != null)
		{
			switch (specification.IntArgument)
			{
			case 0:
				_projectile.Position = Position;
				_projectile.PlayCue(ESFX.EnemyRoyalGuardSpellCast);
				_hasPlayedCastCueButHasNotCast = true;
				break;
			case 1:
			{
				float num = (IsFacingLeft ? (-1f) : 1f);
				Vector2 iV = new Vector2(num * 100f, 0f);
				Point startPoint = new Point(Position.X, Position.Y - 40);
				_projectile.Reset(startPoint, iV);
				_level.AddProjectile(_projectile);
				_castCooldownTimer = 3.2f;
				_hasPlayedCastCueButHasNotCast = false;
				break;
			}
			case 2:
				_isCarryingOutAbility = false;
				_currentAction = EAIAction.Idle;
				_nextActionTimer = _timeToIdleAfterAttacking;
				break;
			}
		}
	}

	protected override void UpdateDeathScript(float delta)
	{
		if (_deathScriptTimer <= 0f && delta > 0f)
		{
			if (_hasPlayedCastCueButHasNotCast)
			{
				_projectile.StopAllSFX();
			}
			DropLoot();
			PlayCue(ESFX.EnemyRoyalGuardDeathCry);
			SetCharacterSequence(_deathSequence);
			_deathPoint = Position;
		}
		if (_deathScriptTimer > 0.5f)
		{
			DistintegrateEvent newEvent = new DistintegrateEvent(this, _sprite, EDisintegrateType.Chaos, 32, 4, new Vector4(0.8f, 0.3f, 0.9f, 1f));
			_level.AddEvent(newEvent);
			RemoveInstance();
		}
		else
		{
			float num = _deathScriptTimer / 0.5f;
			int num2 = (int)(Math.Sin(num * ((float)Math.PI / 2f)) * 32.0);
			Position = new Point(_deathPoint.X + (IsFacingLeft ? num2 : (-num2)), _deathPoint.Y + (int)((float)(-num2) * 0.5f));
			SnapBboxToPosition();
			SnapFrameToBbox();
			UpdateCharacterSequences(delta);
			UpdateAnimation(delta);
			UpdateAppendages(delta);
			UpdateTrail(delta);
		}
		_deathScriptTimer += delta;
	}

	protected override void AfterDealingTouchDamage(Alive target, Point effectPosition)
	{
		target.GiveStatusEffect(EStatusEffectType.Chaos, 0);
		base.AfterDealingTouchDamage(target, effectPosition);
	}
}
