using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions.GameObjects;

internal sealed class DiscStatue : Monster
{
	private const float ArmRotationChargeUp = (float)Math.PI / 2f;

	private const float ArmRotationFinalSwipe = -(float)Math.PI / 4f;

	private const float TimeToChargeUp = 0.25f;

	private const float TimeToWaitWhileCharged = 0.05f;

	private const float TimeToSwipe = 0.25f;

	private const float TimeToRecover = 0.15f;

	private const float TimeBeforeSwiping = 0.3f;

	private const float TimeBeforeRecovering = 0.55f;

	private const float TimeForEntireAttack = 0.55f;

	private const float FarArmTimeOffset = 0.3f;

	private const float TimeForCrazyClawAttack = 1.95f;

	private const float TimeForEarlyAttackEnd = 1.1f;

	private const float SlowAgility = 0.15f;

	private const float FastAgility = 0.4f;

	private static readonly Vector2 ArmDrawOrigin = new Vector2(21f, 5f);

	private static readonly Vector2 ArmDrawOriginFlipped = new Vector2(11f, 5f);

	private readonly DiscStatueArm _nearArmAppendage;

	private readonly DiscStatueArm _farArmAppendage;

	private readonly Appendage _eyeAppendage;

	private readonly CharacterSequenceSpecification _turnAround;

	private bool _isSlowingDown;

	private bool _hasTouchedSomeoneDuringCharge;

	private SFXCueInstance _whirrCueInstance;

	public DiscStatue(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.FollowAttack;
		_movementType = EAIMovementType.Walk;
		_nonAggroStrategy = EAIStrategy.Pace;
		_paceLength = 5f;
		_timeToIdleAfterMoving = 0f;
		_agility = 0.15f;
		_bboxOffset = Point.Zero;
		Bbox = new Rectangle(_position.X, _position.Y, 23, 46);
		IsFacingLeft = objectSpec != null && !objectSpec.IsFlippedHorizontally;
		base.CannotBeGrabbed = true;
		base.DoesTouchDamageKnockback = true;
		_doAppendagesMatchImageFacing = true;
		_doesUseAppendageCollision = false;
		if (base.Appendages.Count > 0)
		{
			Appendage appendage = base.Appendages[0];
			if (appendage.Appendages.Count > 3)
			{
				_eyeAppendage = appendage.Appendages[2];
				_farArmAppendage = new DiscStatueArm(appendage.Appendages[0]);
				_nearArmAppendage = new DiscStatueArm(appendage.Appendages[3]);
			}
		}
		_turnAround = GetCharacterSequenceByName("TurnAround");
		SetCharacterSequenceByName("Idle");
		_whirrCueInstance = CreateCue(ESFX.EnemyDiscMovement, Position, isLooped: true);
		if (_whirrCueInstance != null)
		{
			_whirrCueInstance.PlayWhenInRange();
		}
	}

	public override void UpdateAbility(float delta)
	{
		if (_nearArmAppendage != null && _farArmAppendage != null)
		{
			Vector2 drawOrigin = (IsFacingLeft ? ArmDrawOrigin : ArmDrawOriginFlipped);
			_nearArmAppendage.SetDrawOrigin(drawOrigin);
			_farArmAppendage.SetDrawOrigin(drawOrigin);
		}
		if (_abilityTimer <= 0f)
		{
			_hasTouchedSomeoneDuringCharge = false;
			SetState(EAFSM.Idle);
			_isSlowingDown = false;
			_agility = 0.4f;
			_movementX = ((!IsFacingLeft) ? 1 : (-1));
			if (_eyeAppendage != null)
			{
				_eyeAppendage.ChangeAnimation(7, 3, 0.1f, EAnimationType.Once);
			}
			if (_whirrCueInstance != null)
			{
				_whirrCueInstance.Pitch = 0.5f;
			}
			else
			{
				_whirrCueInstance = PlayCue(ESFX.EnemyDiscMovement, isLooped: true);
				if (_whirrCueInstance != null)
				{
					_whirrCueInstance.Pitch = 0.5f;
				}
			}
			PlayCue(ESFX.EnemyDiscAggro, Position);
		}
		else if ((base.IsTargetBehindMe || _hasTouchedSomeoneDuringCharge) && _abilityTimer < 1.1f)
		{
			_isSlowingDown = true;
			_abilityTimer = 1.1f + _abilityTimer.Mod(0.55f);
			if (_nearArmAppendage != null && _farArmAppendage != null)
			{
				_nearArmAppendage.Reset(0.3f);
				_farArmAppendage.Reset(0.3f);
			}
		}
		if (_isSlowingDown)
		{
			_movementX = _movementX.EaseTo(0f, delta * 5f);
			if (_whirrCueInstance != null)
			{
				_whirrCueInstance.Pitch = 0f;
			}
			else
			{
				_whirrCueInstance = PlayCue(ESFX.EnemyDiscMovement, isLooped: true);
			}
		}
		else
		{
			UpdateArmRotation(_nearArmAppendage, _abilityTimer, _lastAbilityTimer);
			UpdateArmRotation(_farArmAppendage, _abilityTimer + 0.3f, _lastAbilityTimer + 0.3f);
		}
		if (_abilityTimer >= 1.95f)
		{
			_isCarryingOutAbility = false;
			_currentAction = EAIAction.Idle;
			_nextActionTimer = _timeToIdleAfterAttacking;
			if (_whirrCueInstance != null)
			{
				_whirrCueInstance.Pitch = 0f;
			}
			if (_nearArmAppendage != null && _farArmAppendage != null)
			{
				_nearArmAppendage.Reset(0.15f);
				_farArmAppendage.Reset(0.15f);
			}
			if (_eyeAppendage != null)
			{
				_eyeAppendage.ChangeAnimation(new AnimationSpec
				{
					Start = 7,
					Length = 3,
					Speed = 0.1f,
					Type = EAnimationType.Once,
					IsInReverse = true
				});
			}
			_agility = 0.15f;
		}
	}

	protected override void AfterDealingTouchDamage(Alive target, Point effectPosition)
	{
		_hasTouchedSomeoneDuringCharge = true;
		base.AfterDealingTouchDamage(target, effectPosition);
	}

	private void UpdateArmRotation(DiscStatueArm arm, float inTime, float inLastTime)
	{
		if (arm != null)
		{
			int num = (IsFacingLeft ? 1 : (-1));
			float num2 = inTime.Mod(0.55f);
			float num3 = inLastTime.Mod(0.55f);
			if (num3 > num2 || num2 <= 0f)
			{
				arm.SetTargetRotation(0.25f, arm.Rotation, (float)Math.PI / 2f * (float)num);
			}
			else if (num3 < 0.3f && num2 >= 0.3f)
			{
				arm.SetTargetRotation(0.25f, arm.Rotation, -(float)Math.PI / 4f * (float)num);
			}
		}
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && _nearArmAppendage != null && _farArmAppendage != null)
		{
			_nearArmAppendage.Update(delta);
			_farArmAppendage.Update(delta);
		}
		base.Update(delta);
	}

	protected override void DoTurningAroundAnimation(EAFSM lastState, bool lastFacingRight)
	{
		if (_turnAround != null)
		{
			SetCharacterSequence(_turnAround);
		}
		base.DoTurningAroundAnimation(lastState, lastFacingRight);
	}
}
