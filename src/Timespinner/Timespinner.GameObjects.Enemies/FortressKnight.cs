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

namespace Timespinner.GameObjects.Enemies;

internal sealed class FortressKnight : Monster
{
	private const int ShieldHitBeamEmitThreshold = 3;

	private const int ShieldBeamSpeed = 450;

	private const float ShieldHitGlowFrequencyMultiplier = 3f;

	private const float TimeBeforeShieldShoots = 1f;

	private const float WindupAnimationSpeed = 0.15f;

	private const float SwingAnimationSpeed = 0.067f;

	private const float TimeForWindup = 0.45000002f;

	private const float TimeForWindupPlusSwingAnimation = 0.651f;

	private const float LungeVelocity = 100f;

	private const float SwingVelocityX = 2500f;

	private const float SwingVelocityY = -5000f;

	private const int MaxDamageBboxThresholdX = 64;

	private readonly float _attackSequenceDuration;

	private readonly Appendage _shieldAppendage;

	private readonly CharacterSequenceSpecification _idleSequence;

	private readonly CharacterSequenceSpecification _moveSequence;

	private readonly CharacterSequenceSpecification _turnIdleSequence;

	private readonly CharacterSequenceSpecification _turnMoveSequence;

	private readonly CharacterSequenceSpecification _attackSequence;

	private readonly FortressKnightShieldChargePS _shieldChargePS;

	private bool _isShieldUp;

	private int _shieldHitCount;

	private float _shieldGlowTimer;

	private float _shieldChargeTimer;

	private SFXCueInstance _shieldEnergyLoopCueInstance;

	private FortressKnightShieldBeam _shieldBeam;

	public FortressKnight(Point inPosition, Level inLevel, SpriteSheet sprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, sprite, inID, objectSpec)
	{
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_doAppendagesMatchImageFacing = true;
		_doAppendagesInheritDrawColor = false;
		_currentAI = EAIStrategy.FollowAttack;
		_movementType = EAIMovementType.Walk;
		_isAfraidOfFalling = true;
		_isAfraidOfAttackingNearCliff = true;
		_attackDistanceThresholdX = 60;
		_agility = 0.15f;
		_isAffectedByGravity = true;
		_isFlying = false;
		base.DoesTouchDamageKnockback = true;
		_bboxOffset = new Point(5, 10);
		Bbox = new Rectangle(_position.X, _position.Y, 32, 40);
		_shieldChargePS = new FortressKnightShieldChargePS(_level.GCM.TxParticleEnergy, 5)
		{
			BaseColor = new Vector4(0.75f, 1f, 0.88f, 0.9f)
		};
		_particleSystems.Add(_shieldChargePS);
		if (base.CharacterSpecification != null && base.CharacterSpecification.Sequences.Count > 4)
		{
			_idleSequence = base.CharacterSpecification.Sequences[0];
			_moveSequence = base.CharacterSpecification.Sequences[1];
			_turnIdleSequence = base.CharacterSpecification.Sequences[2];
			_turnMoveSequence = base.CharacterSpecification.Sequences[3];
			_attackSequence = base.CharacterSpecification.Sequences[4];
			_attackSequenceDuration = _attackSequence.EstimateDuration();
		}
		_shieldAppendage = base.Appendages[0];
		SetCharacterSequence(_idleSequence);
		_isShieldUp = true;
		_timeToTurnAround = 0.1f;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && _isShieldUp)
		{
			if (_shieldHitCount >= 3)
			{
				_shieldChargeTimer += delta;
				_shieldChargePS.AddParticles(_shieldAppendage.Bbox.Center.ToVector2());
				if (_shieldChargeTimer > 1f)
				{
					_shieldChargeTimer = 0f;
					_shieldHitCount = 0;
					ShootShieldBeam();
				}
			}
			_shieldGlowTimer += delta * ((float)(1 + (_shieldHitCount + _shieldHitCount)) * 3f);
			if (_shieldGlowTimer > (float)Math.PI * 2f)
			{
				_shieldGlowTimer -= (float)Math.PI * 2f;
			}
			_shieldAppendage.IsGlowing = true;
			_shieldAppendage.GlowColor = new Color(1f, 1f, 1f, (float)Math.Cos(_shieldGlowTimer) * 0.5f + 0.5f);
			_shieldAppendage.GlowBase = 1f * (float)(1 + (_shieldHitCount + 1) / 4);
		}
		base.Update(delta);
	}

	private void ShootShieldBeam()
	{
		int num = ((!IsFacingLeft) ? 1 : (-1));
		Point center = _shieldAppendage.Bbox.Center;
		_shieldBeam = new FortressKnightShieldBeam(iV: new Vector2(num * 450, 0f), inLevel: _level, inPosition: center, inSide: ETeamSide.Neutral, sprite: _sprite, baseDamage: base.Damage);
		_level.AddProjectile(_shieldBeam);
		PlayCue(ESFX.EnemyFortressKnightShoot, center);
		AddBattleAnimation(new BattleAnimation(_sprite, center, _level)
		{
			AnimationStart = 22,
			AnimationLength = 5,
			DoesFadeOut = true,
			IsFacingLeft = IsFacingLeft,
			AnchorObject = _shieldAppendage,
			AnchorOffset = new Point(0, -16)
		});
		if (_shieldEnergyLoopCueInstance != null && !_shieldEnergyLoopCueInstance.IsFinished)
		{
			_shieldEnergyLoopCueInstance.Pause(0.25f);
		}
	}

	public override void SetState(EAFSM state)
	{
		base.SetState(state);
		switch (state)
		{
		case EAFSM.Idle:
			SetCharacterSequence(_idleSequence);
			break;
		case EAFSM.Running:
			SetCharacterSequence(_moveSequence);
			break;
		}
	}

	protected override void DoTurningAroundAnimation(EAFSM lastState, bool lastFacingRight)
	{
		SetCharacterSequence((_currentState == EAFSM.Running) ? _turnMoveSequence : _turnIdleSequence);
		base.DoTurningAroundAnimation(lastState, lastFacingRight);
	}

	public override void UpdateAbility(float delta)
	{
		if (_abilityTimer <= 0f)
		{
			if (_isAfraidOfMoving)
			{
				_isCarryingOutAbility = false;
				_currentAction = EAIAction.Idle;
				_nextActionTimer = _timeToIdleAfterAttacking;
			}
			else
			{
				SetState(EAFSM.Idle);
				SetCharacterSequence(_attackSequence);
				_isShieldUp = false;
				_shieldChargeTimer = 0f;
				PlayCue(ESFX.EnemyFortressKnightSword);
			}
		}
		if (_abilityTimer >= 0.45000002f && _abilityTimer <= 0.651f)
		{
			_velocity.X = 2500f * (float)((!IsFacingLeft) ? 1 : (-1));
			if (_abilityTimer < 0.517f)
			{
				_velocity.Y = -5000f * delta;
			}
		}
		else if (_abilityTimer > 0.651f && _lastAbilityTimer <= 0.651f)
		{
			_velocity.X = 100f * (float)((!IsFacingLeft) ? 1 : (-1));
		}
		if (_abilityTimer >= _attackSequenceDuration)
		{
			_isCarryingOutAbility = false;
			_currentAction = EAIAction.Idle;
			_nextActionTimer = _timeToIdleAfterAttacking;
			_isShieldUp = true;
		}
	}

	public override bool ManageDamage(int damage, Vector2 velocity, Point where, Rectangle sourceRectangle, EDamageType type, EDamageElement element, bool doesKnockBack)
	{
		bool result = false;
		bool flag = false;
		if (_shieldBeam == null || !(sourceRectangle == _shieldBeam.Bbox))
		{
			if (_isShieldUp)
			{
				Rectangle shieldBbox = GetShieldBbox();
				bool flag2 = (sourceRectangle.Intersects(shieldBbox) || shieldBbox.Contains(sourceRectangle)) && sourceRectangle.Width < 64;
				if (flag2)
				{
					flag2 = (IsFacingLeft ? (sourceRectangle.Left < shieldBbox.Right) : (sourceRectangle.Right > shieldBbox.Left));
				}
				if (flag2)
				{
					bool flag3 = sourceRectangle.Width == 12 && sourceRectangle.Height == 31 && doesKnockBack && element == EDamageElement.None;
					flag = true;
					if (!flag3)
					{
						Point position = new Point(IsFacingLeft ? shieldBbox.Left : shieldBbox.Right, where.Y);
						_level.AddAnimation(EBattleAnimationType.SmallFail, position, ETeamSide.Heroes, IsFacingLeft, doesPlaySFX: false);
						if (_shieldHitCount <= 3)
						{
							_shieldHitCount++;
						}
					}
					else
					{
						result = true;
						_shieldHitCount = 3;
					}
				}
				else
				{
					result = base.ManageDamage(damage, velocity, where, sourceRectangle, type, element, doesKnockBack);
				}
			}
			else
			{
				result = base.ManageDamage(damage, velocity, where, sourceRectangle, type, element, doesKnockBack);
			}
		}
		if (flag)
		{
			PlayCue(ESFX.EnemyFortressKnightAbsorb, where);
			if (_shieldEnergyLoopCueInstance == null)
			{
				_shieldEnergyLoopCueInstance = PlayCue(ESFX.EnemyFortressKnightEnergyLoop, isLooped: true);
			}
			else if (_shieldEnergyLoopCueInstance.IsPaused)
			{
				_shieldEnergyLoopCueInstance.Resume();
			}
			if (_shieldEnergyLoopCueInstance != null)
			{
				int num = MathEx.Min(_shieldHitCount, 3);
				_shieldEnergyLoopCueInstance.Pitch = (float)num / 3f;
			}
		}
		return result;
	}

	private Rectangle GetShieldBbox()
	{
		return _shieldAppendage.Bbox;
	}

	protected override void UpdateDeathScript(float delta)
	{
		_level.AddAnimation(EBattleAnimationType.AuraExplosion, Position, ETeamSide.Enemies, IsFacingLeft, doesPlaySFX: true);
		DropLootAndRemove();
	}
}
