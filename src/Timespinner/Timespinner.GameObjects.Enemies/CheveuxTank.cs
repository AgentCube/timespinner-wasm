using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class CheveuxTank : Monster
{
	private readonly LandingDustParticleSystem _landingParticles;

	private bool _hasLandedBefore;

	public CheveuxTank(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.FollowAttack;
		_movementType = EAIMovementType.MoveJump;
		_agility = 0.4f;
		_bboxOffset = new Point(6, 8);
		Bbox = new Rectangle(_position.X, _position.Y, 34, 35);
		_isAfraidOfFalling = false;
		_timeToIdleAfterMoving = 0.1f;
		_landingParticles = new LandingDustParticleSystem(_level.GCM.TxParticleDust, 10, _level.ID, 0);
		_particleSystems.Add(_landingParticles);
	}

	public override void Update(float delta)
	{
		if (!_wasGrounded && _isGrounded)
		{
			_landingParticles.AddParticles(new Vector2(Bbox.Center.X, Bbox.Bottom));
			if (_hasLandedBefore)
			{
				PlayCue(ESFX.EnemyLand, Position);
			}
			else
			{
				_hasLandedBefore = true;
			}
		}
		base.Update(delta);
	}

	public override void SetState(EAFSM state)
	{
		base.SetState(state);
		switch (state)
		{
		case EAFSM.Jumping:
			ChangeAnimation(5, 3, 0.1f, EAnimationType.Once);
			PlayCue(ESFX.EnemySpringJump, _position);
			break;
		case EAFSM.Falling:
			ChangeAnimation(8, 2, 0.1f, EAnimationType.Once);
			break;
		default:
			ChangeAnimation(0, 4, 0.1f, EAnimationType.Cycle);
			break;
		}
	}

	protected override void DoTurningAroundAnimation(EAFSM lastState, bool lastFacingRight)
	{
		switch (lastState)
		{
		case EAFSM.Jumping:
			ChangeAnimation(5, 3, 0.1f, EAnimationType.Once, 4, 0, 0.075f);
			break;
		case EAFSM.Falling:
			ChangeAnimation(7, 0, 0.1f, EAnimationType.None, 4, 0, 0.075f);
			break;
		default:
			ChangeAnimation(0, 4, 0.1f, EAnimationType.PingPong, 4, 0, 0.075f);
			break;
		}
		base.DoTurningAroundAnimation(lastState, lastFacingRight);
	}
}
