using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class ForestBabyCheveux : Monster
{
	private const float PlummetGravity = 1500f;

	private const float FloatGravity = 200f;

	private const float TimeBeforeDyingInWater = 3f;

	private readonly LandingDustParticleSystem _landingParticles;

	private bool _hasLandedBefore;

	private float _waterDeathTimer;

	private SFXCueInstance _dropCueInstance;

	private SFXCueInstance _flapCueInstance;

	public ForestBabyCheveux(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.FollowAttack;
		_movementType = EAIMovementType.MoveJump;
		_agility = 0.4f;
		_bboxOffset = new Point(9, 8);
		Bbox = new Rectangle(_position.X, _position.Y, 21, 37);
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_isAfraidOfFalling = false;
		_maxJumpTime = 2f;
		_jumpLaunchVelocity = -200f;
		_maxFallSpeed = 800f;
		_gravityAcceleration = 1500f;
		_attackDistanceThresholdX = 16;
		_attackDistanceThresholdY = -400;
		_timeToIdleAfterMoving = 1f;
		_timeToIdleAfterAttacking = 1f;
		_timeToMove = 0.05f;
		_landingParticles = new LandingDustParticleSystem(_level.GCM.TxParticleDust, 10, _level.ID, 0);
		_particleSystems.Add(_landingParticles);
	}

	public override void Update(float delta)
	{
		if (!_wasGrounded && _isGrounded)
		{
			if (_hasLandedBefore)
			{
				_landingParticles.AddParticles(new Vector2(Bbox.Center.X, Bbox.Bottom), 10f);
				PlayCue(ESFX.EnemyBabyCheveuxLand, Position);
				if (_dropCueInstance != null && !_dropCueInstance.IsFinished)
				{
					_dropCueInstance.Stop();
					_dropCueInstance = null;
				}
			}
			else
			{
				_hasLandedBefore = true;
			}
			_nextActionTimer += _timeToIdleAfterMoving;
		}
		if (IsInWater)
		{
			_waterDeathTimer += delta;
			if (_waterDeathTimer >= 3f)
			{
				Kill();
			}
		}
		else if (base.WasInWater)
		{
			_waterDeathTimer = 0f;
		}
		base.Update(delta);
	}

	public override void SetState(EAFSM state)
	{
		base.SetState(state);
		switch (state)
		{
		case EAFSM.Jumping:
			ChangeAnimation(4, 4, 0.05f, EAnimationType.Cycle);
			if (_flapCueInstance != null)
			{
				_flapCueInstance.Resume();
			}
			else
			{
				_flapCueInstance = PlayCue(ESFX.EnemyBabyCheveuxFlap, isLooped: true);
			}
			return;
		case EAFSM.Falling:
			return;
		}
		if (_flapCueInstance != null && !_flapCueInstance.IsManuallyPaused)
		{
			_flapCueInstance.Pause();
		}
		if (_wasCarryingOutAbility)
		{
			ChangeAnimation(new AnimationSpec[2]
			{
				new AnimationSpec
				{
					Start = 9,
					Length = 1,
					Speed = 0.25f,
					Type = EAnimationType.Once
				},
				new AnimationSpec
				{
					Start = 0,
					Length = 4,
					Speed = 0.18f,
					Type = EAnimationType.Cycle
				}
			});
		}
		else
		{
			ChangeAnimation(0, 4, 0.18f, EAnimationType.Cycle);
		}
	}

	protected override void OnAggroed()
	{
		SetState(EAFSM.Idle);
		if (base.HeroWhoAggroedMe != null)
		{
			IsFacingLeft = Position.X > base.HeroWhoAggroedMe.Position.X;
		}
		_currentAction = EAIAction.None;
		_nextActionTimer = 1f;
		ChangeAnimation(new AnimationSpec[4]
		{
			new AnimationSpec
			{
				Start = 10,
				Length = 1,
				Speed = 0.05f,
				Type = EAnimationType.Once
			},
			new AnimationSpec
			{
				Start = 11,
				Length = 1,
				Speed = 0.3f,
				Type = EAnimationType.Once
			},
			new AnimationSpec
			{
				Start = 10,
				Length = 1,
				Speed = 0.05f,
				Type = EAnimationType.Once
			},
			new AnimationSpec
			{
				Start = 0,
				Length = 4,
				Speed = 0.18f,
				Type = EAnimationType.Cycle
			}
		});
		PlayCue(ESFX.EnemyBabyCheveuxSquawk, Position);
		base.OnAggroed();
	}

	public override void StartAbility(int whichAbility)
	{
		if (!_isGrounded)
		{
			base.StartAbility(whichAbility);
			_totalAbilityTime = 100f;
		}
		else
		{
			_isCarryingOutAbility = false;
			_currentAction = EAIAction.Idle;
			_nextActionTimer = 0f;
		}
	}

	public override void UpdateAbility(float delta)
	{
		if (_abilityTimer <= 0f)
		{
			ChangeAnimation(7, 2, 0.05f, EAnimationType.Once);
			_gravityAcceleration = 200f;
			if (_flapCueInstance != null && !_flapCueInstance.IsManuallyPaused)
			{
				_flapCueInstance.Pause(0.3f);
			}
		}
		else if (_abilityTimer >= 0.2f && _lastAbilityTimer < 0.2f)
		{
			_gravityAcceleration = 1500f;
			_dropCueInstance = PlayCue(ESFX.EnemyBabyCheveuxDrop);
		}
		if (_isGrounded)
		{
			_isCarryingOutAbility = false;
			_currentAction = EAIAction.Idle;
			_nextActionTimer = _timeToIdleAfterAttacking;
		}
	}

	protected override void UpdateDeathScript(float delta)
	{
		if (_flapCueInstance != null)
		{
			_flapCueInstance.Stop();
		}
		BattleAnimation battleAnimation = new BattleAnimation(_level.GCM.SpEffectsLarge, _bbox.Center, _level);
		battleAnimation.TeamSide = base.DefaultTeam;
		battleAnimation.AnimationStart = 5;
		battleAnimation.AnimationLength = 6;
		battleAnimation.AnimationSpeed = 0.04f;
		battleAnimation.DrawColor = Color.White * 0.8f;
		battleAnimation.ParticleSystem = new FeatherExplosionParticleSystem(_sprite, 1, 16, 4);
		BattleAnimation newAnimation = battleAnimation;
		BattleAnimation battleAnimation2 = new BattleAnimation(null, _bbox.Center, _level);
		battleAnimation2.TeamSide = base.DefaultTeam;
		battleAnimation2.ParticleSystem = new FeatherExplosionParticleSystem(_sprite, 1, 12, 4);
		BattleAnimation newAnimation2 = battleAnimation2;
		_level.AddAnimation(newAnimation);
		_level.AddAnimation(newAnimation2);
		_level.PlayCue(ESFX.FoleyExplosionFeather, _bbox.Center);
		DropLootAndRemove();
	}
}
