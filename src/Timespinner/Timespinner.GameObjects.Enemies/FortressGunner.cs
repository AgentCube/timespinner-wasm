using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class FortressGunner : Monster
{
	private const int MuzzleFlashOffsetX = 18;

	private const float TimeForWindup = 0.8f;

	private const float TimeForEntireAttack = 1.2f;

	private readonly Point _arrowStartOffsetA = new Point(-14, -27);

	private readonly Point _arrowStartOffsetB = new Point(-11, -30);

	private readonly CharacterSequenceSpecification _moveSequence;

	private readonly CharacterSequenceSpecification _aimSequence;

	private readonly CharacterSequenceSpecification _shootSequence;

	private readonly CharacterSequenceSpecification _endAttackSequence;

	private readonly CharacterSequenceSpecification _turnSequence;

	internal Point ArrowStartOffsetA
	{
		get
		{
			if (!IsFacingLeft)
			{
				return new Point(-_arrowStartOffsetA.X, _arrowStartOffsetA.Y);
			}
			return _arrowStartOffsetA;
		}
	}

	internal Point ArrowStartOffsetB
	{
		get
		{
			if (!IsFacingLeft)
			{
				return new Point(-_arrowStartOffsetB.X, _arrowStartOffsetB.Y);
			}
			return _arrowStartOffsetB;
		}
	}

	public FortressGunner(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_doesDrawBaseSprite = false;
		_doAppendagesMatchImageFacing = true;
		_currentAI = EAIStrategy.FollowAttack;
		_movementType = EAIMovementType.Walk;
		_timeToMove = 0.4f;
		_isAfraidOfFalling = true;
		_isAfraidOfBeingTooClose = true;
		_attackDistanceThresholdX = 225;
		_retreatDistanceThresholdX = 160;
		_agility = 0.2f;
		_isAffectedByGravity = true;
		_isFlying = false;
		Bbox = new Rectangle(_position.X, _position.Y, 16, 16);
		base.AggroBboxDimensions = new Point(450, 150);
		base.DeaggroBboxDimensions = new Point(900, 800);
		ChangeAnimation(0);
		_timeToTurnAround = 0.07f;
		if (base.CharacterSpecification != null && base.CharacterSpecification.Sequences.Count > 5)
		{
			_moveSequence = base.CharacterSpecification.Sequences[1];
			_aimSequence = base.CharacterSpecification.Sequences[2];
			_shootSequence = base.CharacterSpecification.Sequences[3];
			_endAttackSequence = base.CharacterSpecification.Sequences[4];
			_turnSequence = base.CharacterSpecification.Sequences[5];
		}
	}

	public override void SetState(EAFSM state)
	{
		base.SetState(state);
		if (state == EAFSM.Running && _moveSequence != null)
		{
			SetCharacterSequence(_moveSequence);
		}
	}

	protected override void DoTurningAroundAnimation(EAFSM lastState, bool lastFacingRight)
	{
		if (_turnSequence != null)
		{
			if (_currentState == EAFSM.Running)
			{
				_turnSequence.FollowingSequenceIndex = 2;
			}
			else
			{
				_turnSequence.FollowingSequenceIndex = 1;
			}
			SetCharacterSequence(_turnSequence);
		}
		base.DoTurningAroundAnimation(lastState, lastFacingRight);
	}

	public override void UpdateAbility(float delta)
	{
		if (_abilityTimer <= 0f)
		{
			SetState(EAFSM.Idle);
			if (_aimSequence != null)
			{
				SetCharacterSequence(_aimSequence);
			}
			PlayCue(ESFX.EnemyFortressSniperAim, Position);
		}
		if (_abilityTimer >= 0.8f && _lastAbilityTimer < 0.8f)
		{
			if (_shootSequence != null)
			{
				SetCharacterSequence(_shootSequence);
			}
			int num = ((!IsFacingLeft) ? 1 : (-1));
			Point center = _bbox.Center;
			center.X += 20 * num;
			center.Y -= 21;
			Vector2 iV = new Vector2((float)num * 450f, 0f);
			_level.AddProjectile(new FortressGunnerBolt(_level, center, iV, base.DefaultTeam, _sprite, base.Damage));
			_level.AddProjectile(new FortressGunnerMiniBolt(_level, center, iV, base.DefaultTeam, _sprite, base.Damage, isTop: true));
			_level.AddProjectile(new FortressGunnerMiniBolt(_level, center, iV, base.DefaultTeam, _sprite, base.Damage, isTop: false));
			PlayCue(ESFX.EnemyFortressSniperFire, center);
			center = new Point(center.X + num * 18, center.Y);
			_level.AddAnimation(new BattleAnimation(_sprite, center, _level)
			{
				AnimationStart = 50,
				AnimationLength = 3,
				AnimationSpeed = 0.033f,
				IsFacingLeft = IsFacingLeft
			});
		}
		if (_abilityTimer > 1.2f)
		{
			_isCarryingOutAbility = false;
			if (_endAttackSequence != null)
			{
				SetCharacterSequence(_endAttackSequence);
			}
			_currentAction = EAIAction.Idle;
			_nextActionTimer = _timeToIdleAfterAttacking;
		}
	}

	protected override void UpdateDeathScript(float delta)
	{
		_level.AddAnimation(EBattleAnimationType.AuraExplosion, Position, ETeamSide.Enemies, IsFacingLeft, doesPlaySFX: true);
		DropLootAndRemove();
	}

	internal void DoAim()
	{
		SetCharacterSequence(_aimSequence);
	}
}
