using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class RedCheveux : Monster
{
	private const int DashMovementX = 400;

	private const float TimeBetweenDustEmission = 0.165f;

	private const float TimeToChargeUp = 1.2f;

	private const float TimeToShowWarningFlash = 1.5f;

	private const float TimeToSuperChargeUp = 2f;

	private const float TimeToDash = 2.5f;

	private const float TimeToTurnAroundWhileSkidding = 2.7f;

	private const float TimeToSkid = 2.9f;

	private readonly int _baseTouchDamage;

	private readonly HalfDustParticleSystem _chargeDustParticles;

	private readonly HalfDustParticleSystem _skidDustParticles;

	private readonly TreadDustParticleSystem _treadDustParticles;

	private bool _isChargingLeft;

	private float _dustEmissionTimer;

	public RedCheveux(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_baseTouchDamage = _damageCaused;
		_currentAI = EAIStrategy.FollowAttack;
		_movementType = EAIMovementType.Walk;
		_agility = 0.1f;
		base.SpriteFrameOffset = 10;
		_bboxOffset = new Point(6, 11);
		Bbox = new Rectangle(_position.X, _position.Y, 34, 33);
		_isAfraidOfFalling = false;
		_maxMoveSpeed = 400f;
		base.AggroBboxDimensions = new Point(400, 150);
		base.IsVelocityAffectedBySlopes = false;
		_chargeDustParticles = new HalfDustParticleSystem(_level.GCM.TxParticleDust, 10);
		_skidDustParticles = new HalfDustParticleSystem(_level.GCM.TxParticleDust, 10);
		_treadDustParticles = new TreadDustParticleSystem(_level.GCM.TxParticleDust, 5);
		_particleSystems.Add(_chargeDustParticles);
		_particleSystems.Add(_skidDustParticles);
		_particleSystems.Add(_treadDustParticles);
	}

	public override void Update(float delta)
	{
		if (_currentState == EAFSM.Running)
		{
			_treadDustParticles.IsParticleSystemFacingLeft = IsFacingLeft;
			_treadDustParticles.AddParticles(new Vector2(IsFacingLeft ? (Bbox.Left + 8) : (Bbox.Right - 8), Bbox.Bottom));
		}
		base.Update(delta);
	}

	public override void SetState(EAFSM state)
	{
		base.SetState(state);
		switch (state)
		{
		case EAFSM.Running:
			ChangeAnimation(0, 4, 0.1f, EAnimationType.Cycle);
			break;
		case EAFSM.Idle:
			ChangeAnimation(0, 0, 0.1f, EAnimationType.None);
			break;
		}
	}

	protected override void DoTurningAroundAnimation(EAFSM lastState, bool lastFacingRight)
	{
		if (lastState == EAFSM.Running)
		{
			ChangeAnimation(0, 4, 0.1f, EAnimationType.Cycle, 4, 0, 0.075f);
		}
		base.DoTurningAroundAnimation(lastState, lastFacingRight);
	}

	public override void UpdateAbility(float delta)
	{
		if (_abilityTimer <= 0f)
		{
			SetState(EAFSM.Idle);
			ChangeAnimation(5, 4, 0.1f, EAnimationType.Cycle);
			PlayCue(ESFX.EnemyTreadCharge, Position);
		}
		if (_abilityTimer < 1.2f)
		{
			_dustEmissionTimer += delta;
			if (_dustEmissionTimer >= 0.165f)
			{
				_dustEmissionTimer = 0f;
				_chargeDustParticles.AddParticles(new Vector2(_isChargingLeft ? (Bbox.Left + 8) : (Bbox.Right - 8), Bbox.Bottom));
			}
			_animationSpeed = 0.1f * (1f - _abilityTimer / 1.2f);
		}
		else if (_abilityTimer < 2f)
		{
			_chargeDustParticles.AddParticles(new Vector2(_isChargingLeft ? (Bbox.Left + 8) : (Bbox.Right - 8), Bbox.Bottom));
			if (_abilityTimer >= 1.5f && _lastAbilityTimer < 1.5f)
			{
				_level.AddAnimation(new BattleAnimation(_level.GCM.SpEffectsSmall, new Point(Bbox.Center.X + (IsFacingLeft ? 1 : (-1)), Bbox.Center.Y - 8), _level)
				{
					TeamSide = base.DefaultTeam,
					AnimationStart = 52,
					AnimationLength = 6
				});
			}
		}
		else if (_abilityTimer < 2.5f)
		{
			if (_lastAbilityTimer <= 2f)
			{
				PlayCue(ESFX.EnemyTreadDash, Position);
			}
			_chargeDustParticles.AddParticles(new Vector2(_isChargingLeft ? (Bbox.Left + 8) : (Bbox.Right - 8), Bbox.Bottom));
			_movementX = 400 * ((!IsFacingLeft) ? 1 : (-1));
			ChangeAnimation(0, 4, _animationSpeed, EAnimationType.Cycle);
			base.DoesTouchDamageKnockback = true;
			_damageCaused = (int)Math.Ceiling((float)_baseTouchDamage * 1.5f);
		}
		else if (_abilityTimer < 2.9f)
		{
			if (_lastAbilityTimer <= 2.5f)
			{
				ChangeAnimation(4, 0, 1f, EAnimationType.None);
			}
			else if (_abilityTimer >= 2.7f && _lastAbilityTimer < 2.7f)
			{
				IsFacingLeft = !IsFacingLeft;
			}
			float num = (2.9f - _abilityTimer) / 0.4000001f;
			_movementX = num * 400f * (float)((!_isChargingLeft) ? 1 : (-1));
			_chargeDustParticles.AddParticles(new Vector2(_isChargingLeft ? (Bbox.Left + 8) : (Bbox.Right - 8), Bbox.Bottom));
			_skidDustParticles.AddParticles(new Vector2(_isChargingLeft ? (Bbox.Left - 8) : (Bbox.Right + 8), Bbox.Bottom));
		}
		else
		{
			base.DoesTouchDamageKnockback = false;
			_isCarryingOutAbility = false;
			_animationIndex = 10;
			ChangeAnimation(0, 1, 0.1f, EAnimationType.None);
			_currentAction = EAIAction.Idle;
			_nextActionTimer = 0.25f;
			_damageCaused = _baseTouchDamage;
		}
	}

	public override void StartAbility(int whichAbility)
	{
		base.StartAbility(whichAbility);
		_totalAbilityTime = 100f;
		_isChargingLeft = IsFacingLeft;
		_chargeDustParticles.IsParticleSystemFacingLeft = IsFacingLeft;
		_skidDustParticles.IsParticleSystemFacingLeft = !IsFacingLeft;
	}
}
