using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class CavesCopperWyvern : Monster
{
	private const float TimeToStartSpitAnimation = 0.6f;

	private const float TimeToThrowSpit = 0.67f;

	private const float TimeToEndAbility = 1.32f;

	private readonly LandingDustParticleSystem _landingParticles;

	public CavesCopperWyvern(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.FollowAttack;
		_movementType = EAIMovementType.MoveJump;
		_attackDistanceThresholdX = 160;
		_attackDistanceThresholdY = -16;
		_agility = 0.6f;
		_bboxOffset = new Point(8, 12);
		Bbox = new Rectangle(_position.X, _position.Y, 28, 19);
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_maxJumpTime = 0.2f;
		_jumpLaunchVelocity = -400f;
		_currentSproingTime = 0.11f;
		_wasGrounded = true;
		_timeToTurnAround = 0.05f;
		_isGrounded = true;
		_wasGrounded = true;
		ChangeAnimation(0, 3, 0.15f, EAnimationType.Cycle);
		_landingParticles = new LandingDustParticleSystem(_level.GCM.TxParticleDust, 10, _level.ID, 0);
		_particleSystems.Add(_landingParticles);
	}

	public override void Update(float delta)
	{
		if (!_wasGrounded && _isGrounded)
		{
			_landingParticles.AddParticles(new Vector2(Bbox.Center.X, Bbox.Bottom));
		}
		base.Update(delta);
	}

	public override void SetState(EAFSM state)
	{
		base.SetState(state);
		switch (state)
		{
		case EAFSM.Jumping:
			ChangeAnimation(4, 3, 0.05f, EAnimationType.Once);
			PlayCue(ESFX.EnemyCopperWyvernHop, Position);
			return;
		case EAFSM.Falling:
			ChangeAnimation(6, 0, 0.1f, EAnimationType.None);
			return;
		}
		if (!_wasGrounded)
		{
			ChangeAnimation(0, 3, 0.15f, EAnimationType.PingPong, 7, 1, 0.1f);
			PlayCue(ESFX.EnemyCopperWyvernLand, Position);
		}
		else
		{
			ChangeAnimation(0, 3, 0.15f, EAnimationType.PingPong);
		}
	}

	public override void UpdateAbility(float delta)
	{
		if (_abilityTimer == 0f)
		{
			SetState(EAFSM.Idle);
			ChangeAnimation(8, 3, 0.1f, EAnimationType.Once);
			PlayCue(ESFX.EnemyCopperWyvernPrespit, Position);
		}
		if (_abilityTimer >= 0.6f && _lastAbilityTimer < 0.6f)
		{
			ChangeAnimation(11, 2, 0.033f, EAnimationType.Once);
			int num = ((!IsFacingLeft) ? 1 : (-1));
			Point center = _bbox.Center;
			center.X += 12 * num;
			center.Y = center.Y;
			_level.AddAnimation(new BattleAnimation(_sprite, center, _level)
			{
				TeamSide = ETeamSide.Enemies,
				AnimationSpeed = 0.035f,
				AnimationStart = 18,
				AnimationLength = 4,
				IsFacingLeft = IsFacingLeft
			});
		}
		if (_abilityTimer >= 0.67f && _lastAbilityTimer < 0.67f)
		{
			float num2 = (IsFacingLeft ? (-1f) : 1f);
			Vector2 iV = new Vector2(num2 * 350f, -150f);
			Point center2 = _bbox.Center;
			center2.X += (int)(12f * num2);
			center2.Y -= 2;
			_level.AddProjectile(new CavesCopperWyvernBullet(_level, center2, iV, ETeamSide.Enemies, _sprite, base.Damage));
			PlayCue(ESFX.EnemyCopperWyvernSpit, Position);
		}
		if (_abilityTimer >= 1.32f)
		{
			_isCarryingOutAbility = false;
			ChangeAnimation(0, 3, 0.15f, EAnimationType.PingPong, 13, 0, 0.1f);
			_currentAction = EAIAction.Idle;
			_nextActionTimer = _timeToIdleAfterAttacking;
		}
	}

	protected override void DoTurningAroundAnimation(EAFSM lastState, bool lastFacingRight)
	{
		switch (lastState)
		{
		case EAFSM.Jumping:
			ChangeAnimation(3, 4, 0.04f, EAnimationType.Once);
			break;
		case EAFSM.Falling:
			ChangeAnimation(6, 0, 0.1f, EAnimationType.None, 8, 0, 0.075f);
			break;
		default:
			ChangeAnimation(0, 3, 0.15f, EAnimationType.PingPong, 8, 0, 0.075f);
			break;
		}
		base.DoTurningAroundAnimation(lastState, lastFacingRight);
	}

	protected override void UpdateDeathScript(float delta)
	{
		_level.AddAnimation(EBattleAnimationType.WetSplashLarge, Bbox.Center);
		DropLootAndRemove();
	}
}
