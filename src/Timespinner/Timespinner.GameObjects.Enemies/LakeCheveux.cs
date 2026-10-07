using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Enemies._07_LakeSerene;

namespace Timespinner.GameObjects.Enemies;

internal sealed class LakeCheveux : Monster
{
	private const int BlinkIntervals = 5;

	private const float BlinkIntervalTime = 0.5f;

	private const int MaxFireProjectiles = 40;

	private const float BaseFireAngle = 0.25f;

	private const float BaseFireSpeed = 300f;

	private const float FireAngleJitter = 0.5f;

	private const float TimeForWindup = 0.5f;

	private const float TimeForEntireAttack = 1.5f;

	private const float TimeBetweenFlameEmission = 0.03f;

	private const float TimeToWaitAfterSquawking = 1f;

	private readonly bool _isWild;

	private readonly CharacterSequenceSpecification _idleSequence;

	private readonly CharacterSequenceSpecification _turnSequence;

	private readonly CharacterSequenceSpecification _walkSequence;

	private readonly CharacterSequenceSpecification _openMouthSequence;

	private readonly CharacterSequenceSpecification _closeMouthSequence;

	private readonly CharacterSequenceSpecification _squawkSequence;

	private readonly CharacterSequenceSpecification _blinkSequence;

	private readonly LakeCheveuxFireProjectile[] _fireProjectiles = new LakeCheveuxFireProjectile[40];

	private bool _hasSquawked;

	private bool _isDoneSquawking;

	private int _flamesEmitted;

	private float _fireEmissionTimer;

	private float _blinkTimer;

	public LakeCheveux(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_isWild = objectSpec == null || objectSpec.Argument == 0;
		_currentAI = EAIStrategy.FollowAttack;
		_movementType = EAIMovementType.Walk;
		_agility = 1f;
		Bbox = new Rectangle(0, 0, 48, 25);
		_canLoseAggro = false;
		_attackDistanceThresholdX = 104;
		_attackDistanceThresholdY = -400;
		_timeToIdleAfterMoving = 0.5f;
		_timeToIdleAfterAttacking = 1f;
		_timeToMove = 0.05f;
		_isAfraidOfBeingTooClose = true;
		_retreatDistanceThresholdX = 64;
		_doAppendagesMatchImageFacing = true;
		_doesUseAppendageCollision = true;
		_doesDrawBaseSprite = false;
		base.CannotBeGrabbed = true;
		base.DoesTouchDamageKnockback = true;
		IsFacingLeft = objectSpec == null || !objectSpec.IsFlippedHorizontally;
		if (base.CharacterSpecification != null && base.CharacterSpecification.Sequences.Count > 7)
		{
			_idleSequence = base.CharacterSpecification.Sequences[0];
			_turnSequence = base.CharacterSpecification.Sequences[1];
			_walkSequence = base.CharacterSpecification.Sequences[3];
			_openMouthSequence = base.CharacterSpecification.Sequences[4];
			_closeMouthSequence = base.CharacterSpecification.Sequences[5];
			_squawkSequence = base.CharacterSpecification.Sequences[6];
			_blinkSequence = base.CharacterSpecification.Sequences[7];
			_timeToTurnAround = 0.2f;
			SetCharacterSequence(_idleSequence);
		}
	}

	public override void SetState(EAFSM state)
	{
		base.SetState(state);
		if (_hasSquawked && !_isDoneSquawking)
		{
			_isDoneSquawking = true;
		}
		switch (state)
		{
		case EAFSM.Idle:
			SetCharacterSequence(_idleSequence);
			break;
		case EAFSM.Running:
			SetCharacterSequence(_walkSequence);
			break;
		}
	}

	protected override void OnAggroed()
	{
		Squawk();
		base.OnAggroed();
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (!_isCarryingOutAbility && _turnAroundTimer <= 0f && _hasSquawked == _isDoneSquawking)
			{
				_blinkTimer -= delta;
				if (_blinkTimer < 0f && _blinkSequence != null)
				{
					_blinkTimer = (float)_level.NextRandomInt(1, 5) * 0.5f;
					SetCharacterSequence(_blinkSequence);
				}
			}
			else
			{
				_blinkTimer = 0.5f;
			}
		}
		base.Update(delta);
	}

	protected override void DoTurningAroundAnimation(EAFSM lastState, bool lastFacingRight)
	{
		if (_turnSequence != null)
		{
			int followingSequenceIndex;
			if (!_hasSquawked)
			{
				followingSequenceIndex = 7;
				_currentAction = EAIAction.Idle;
				_currentState = EAFSM.Idle;
				_nextActionTimer = 1f;
				_hasSquawked = true;
			}
			else
			{
				followingSequenceIndex = ((_currentState != EAFSM.Running) ? 1 : 4);
			}
			_turnSequence.FollowingSequenceIndex = followingSequenceIndex;
			SetCharacterSequence(_turnSequence);
			_movementX = 0f;
		}
		base.DoTurningAroundAnimation(lastState, lastFacingRight);
	}

	private void Squawk()
	{
		if (!_hasSquawked && _level.GetNearestProtagonistPosition(Position).X < Position.X != !IsFacingLeft)
		{
			SetCharacterSequence(_squawkSequence);
			_currentAction = EAIAction.Idle;
			_currentState = EAFSM.Idle;
			_nextActionTimer = 1f;
			_movementX = 0f;
			_hasSquawked = true;
		}
	}

	internal override void TriggerCharacterAction(CharacterAction specification)
	{
		switch (specification.IntArgument)
		{
		case 0:
			PlayCue(ESFX.EnemyTeenCheveuxAggro);
			PlayCue2D(ESFX.EnemyTeenCheveuxAggro2D);
			break;
		case 1:
			PlayCue(ESFX.EnemyTeenCheveuxFootRight);
			break;
		case 2:
			PlayCue(ESFX.EnemyTeenCheveuxFootLeft);
			break;
		case 3:
			PlayCue(ESFX.EnemyTeenCheveuxBreath);
			break;
		}
		base.TriggerCharacterAction(specification);
	}

	public override void UpdateAbility(float delta)
	{
		if (_abilityTimer <= 0f)
		{
			SetState(EAFSM.Idle);
			SetCharacterSequence(_openMouthSequence);
		}
		if (_abilityTimer >= 0.5f)
		{
			_fireEmissionTimer -= delta;
			if (_fireEmissionTimer <= 0f)
			{
				Point center = _bbox.Center;
				int num = ((!IsFacingLeft) ? 1 : (-1));
				center.X += 30 * num;
				center.Y -= 34;
				float num2 = 0.25f + (float)_level.NextRandomDouble() * 0.5f;
				EmitFlame(iV: new Vector2((float)num * (float)Math.Cos(num2) * 300f, (float)Math.Sin(num2) * 300f), startPoint: center);
				_fireEmissionTimer = 0.03f;
			}
		}
		if (_abilityTimer > 1.5f)
		{
			_isCarryingOutAbility = false;
			SetCharacterSequence(_closeMouthSequence);
			_currentAction = EAIAction.Idle;
			_nextActionTimer = _timeToIdleAfterAttacking;
			_fireEmissionTimer = 0f;
		}
	}

	private void EmitFlame(Point startPoint, Vector2 iV)
	{
		LakeCheveuxFireProjectile lakeCheveuxFireProjectile = null;
		if (_flamesEmitted < 40)
		{
			lakeCheveuxFireProjectile = new LakeCheveuxFireProjectile(_level, startPoint, iV, base.DefaultTeam, _sprite, base.Damage);
			_fireProjectiles[_flamesEmitted] = lakeCheveuxFireProjectile;
			_flamesEmitted++;
		}
		else
		{
			LakeCheveuxFireProjectile[] fireProjectiles = _fireProjectiles;
			foreach (LakeCheveuxFireProjectile lakeCheveuxFireProjectile2 in fireProjectiles)
			{
				if (lakeCheveuxFireProjectile2.IsFinished)
				{
					lakeCheveuxFireProjectile2.Reset(startPoint, iV);
					lakeCheveuxFireProjectile = lakeCheveuxFireProjectile2;
					break;
				}
			}
		}
		if (lakeCheveuxFireProjectile != null)
		{
			_level.AddProjectile(lakeCheveuxFireProjectile);
		}
	}

	protected override void UpdateDeathScript(float delta)
	{
		foreach (Appendage appendage in base.Appendages)
		{
			if (appendage.Bbox.Height > 17)
			{
				Point center = appendage.Bbox.Center;
				BattleAnimation battleAnimation = new BattleAnimation(_level.GCM.SpEffectsLarge, center, _level);
				battleAnimation.TeamSide = base.DefaultTeam;
				battleAnimation.AnimationStart = 5;
				battleAnimation.AnimationLength = 6;
				battleAnimation.AnimationSpeed = 0.04f;
				battleAnimation.DrawColor = Color.White * 0.8f;
				battleAnimation.ParticleSystem = new FeatherExplosionParticleSystem(_sprite, 1, 28, 4);
				BattleAnimation newAnimation = battleAnimation;
				BattleAnimation battleAnimation2 = new BattleAnimation(null, center, _level);
				battleAnimation2.TeamSide = base.DefaultTeam;
				battleAnimation2.ParticleSystem = new FeatherExplosionParticleSystem(_sprite, 1, 32, 4);
				BattleAnimation newAnimation2 = battleAnimation2;
				_level.AddAnimation(newAnimation);
				_level.AddAnimation(newAnimation2);
				if (!_isWild && appendage.Appendages.Count > 3)
				{
					Point position = appendage.Appendages[3].Position;
					_level.AddAnimation(EBattleAnimationType.AuraExplosion, new Point(position.X, position.Y + 5), ETeamSide.Enemies, IsFacingLeft);
				}
			}
		}
		_level.PlayCue(ESFX.FoleyExplosionFeather, _bbox.Center);
		DropLootAndRemove();
	}
}
