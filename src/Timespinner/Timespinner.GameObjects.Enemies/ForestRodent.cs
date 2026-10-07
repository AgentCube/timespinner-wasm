using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class ForestRodent : Monster
{
	private const float TimeBetweenIdleSqueaks = 3f;

	private bool _isFallingDuringJump;

	private float _abilityMovementX;

	private float _timeSpentIdle;

	public ForestRodent(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_currentAI = EAIStrategy.FollowAttack;
		_movementType = EAIMovementType.Walk;
		_agility = 0.4f;
		_bboxOffset = new Point(5, 7);
		Bbox = new Rectangle(_position.X, _position.Y, 12, 13);
		_jumpLaunchVelocity = -250f;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_currentState == EAFSM.Idle)
			{
				_timeSpentIdle += delta;
				if (_timeSpentIdle > 3f)
				{
					_timeSpentIdle = 0f;
					PlayCue(ESFX.EnemyRatSqueak, Position, isLooped: false, 0.5f);
				}
			}
			else
			{
				_timeSpentIdle = 0f;
			}
		}
		base.Update(delta);
	}

	public override void SetState(EAFSM state)
	{
		base.SetState(state);
		switch (state)
		{
		case EAFSM.Running:
			ChangeAnimation(3, 5, 0.1f, EAnimationType.Cycle);
			break;
		case EAFSM.Idle:
			ChangeAnimation(0, 3, 0.15f, EAnimationType.Cycle);
			break;
		}
	}

	public override void StartAbility(int whichAbility)
	{
		_abilityMovementX = (IsFacingLeft ? (0f - _agility) : _agility) * 1.5f;
		base.StartAbility(whichAbility);
	}

	public override void UpdateAbility(float delta)
	{
		_movementX = _abilityMovementX;
		if (_abilityTimer <= 0f)
		{
			ChangeAnimation(8);
			_isJumping = true;
			_isFallingDuringJump = false;
			PlayCue(ESFX.EnemyRatLunge, Position);
		}
		else if (_isGrounded && _abilityTimer > 0.1f)
		{
			_isJumping = false;
			_isCarryingOutAbility = false;
			_currentAction = EAIAction.Idle;
			SetState(EAFSM.Idle);
			_nextActionTimer = _timeToIdleAfterAttacking;
			ChangeAnimation(new AnimationSpec[2]
			{
				new AnimationSpec
				{
					Start = 6,
					Length = 2,
					Speed = 0.1f,
					Type = EAnimationType.Once
				},
				new AnimationSpec
				{
					Start = 0,
					Length = 3,
					Speed = 0.15f,
					Type = EAnimationType.Cycle
				}
			});
		}
		else if (_velocity.Y > 1f && !_isFallingDuringJump)
		{
			_isFallingDuringJump = true;
			ChangeAnimation(4, 2, 0.1f, EAnimationType.Once);
		}
	}

	protected override void UpdateDeathScript(float delta)
	{
		_level.AddAnimation(EBattleAnimationType.WetSplashSmall, Bbox.Center, ETeamSide.Enemies, IsFacingLeft, doesPlaySFX: true);
		DropLootAndRemove();
	}
}
