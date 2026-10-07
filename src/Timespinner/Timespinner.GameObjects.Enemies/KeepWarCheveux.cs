using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Cutscene;

namespace Timespinner.GameObjects.Enemies;

internal sealed class KeepWarCheveux : Monster
{
	private const float FloatGravity = 200f;

	private const float PlummetGravity = 1500f;

	private const float TimeBetweenJumps = 0.2f;

	private const float TimeToShowWarningFlash = 1f;

	private const float TimeToSuperChargeUp = 1.5f;

	private const float TimeToDash = 2f;

	private const float TimeToTurnAroundWhileSkidding = 2.2f;

	private const float TimeToSkid = 2.4f;

	private const float IdleAnimationSpeed = 0.18f;

	private readonly bool _isWild;

	private readonly LandingDustParticleSystem _landingParticles;

	private readonly HalfDustParticleSystem _chargeDustParticles;

	private readonly HalfDustParticleSystem _skidDustParticles;

	private readonly TreadDustParticleSystem _treadDustParticles;

	private bool _isChargingLeft;

	private bool _hasLandedBefore;

	private bool _isMiniboss;

	private bool _isDoingChargeAbility;

	private float _jumpTimer;

	private SFXCueInstance _flapCueInstance;

	private SFXCueInstance _dropCueInstance;

	public KeepWarCheveux(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.StandAttack;
		_movementType = EAIMovementType.MoveJump;
		_agility = 0.4f;
		_bboxOffset = new Point(9, 12);
		Bbox = new Rectangle(_position.X, _position.Y, 21, 42);
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_isAfraidOfFalling = false;
		_maxJumpTime = 0.5f;
		_jumpLaunchVelocity = -100f;
		_maxFallSpeed = 500f;
		_maxMoveSpeed = 400f;
		_gravityAcceleration = 1500f;
		base.DoesTouchDamageKnockback = true;
		_attackDistanceThresholdX = 16;
		_attackDistanceThresholdY = -400;
		_timeToIdleAfterMoving = 1f;
		_timeToIdleAfterAttacking = 1f;
		_timeToMove = 0.05f;
		_isWild = objectSpec.Argument == 1;
		if (_isWild)
		{
			base.SpriteFrameOffset = 20;
		}
		_landingParticles = new LandingDustParticleSystem(_level.GCM.TxParticleDust, 10, _level.ID, 0);
		_particleSystems.Add(_landingParticles);
		_chargeDustParticles = new HalfDustParticleSystem(_level.GCM.TxParticleDust, 10);
		_skidDustParticles = new HalfDustParticleSystem(_level.GCM.TxParticleDust, 10);
		_treadDustParticles = new TreadDustParticleSystem(_level.GCM.TxParticleDust, 5);
		_particleSystems.Add(_chargeDustParticles);
		_particleSystems.Add(_skidDustParticles);
		_particleSystems.Add(_treadDustParticles);
	}

	internal void MakeIntoMiniboss()
	{
		_isMiniboss = true;
		_isAggroed = true;
		_isAlwaysAggroed = true;
		PauseMiniboss();
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && !_wasGrounded && _isGrounded)
		{
			if (_hasLandedBefore)
			{
				_landingParticles.AddParticles(new Vector2(Bbox.Center.X, Bbox.Bottom), 10f);
				if (!_isDoingChargeAbility)
				{
					PlayCue(ESFX.EnemyBabyCheveuxLand, Position);
				}
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
		DoSquawk();
		base.OnAggroed();
	}

	internal void DoSquawk()
	{
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
	}

	internal void PauseMiniboss()
	{
		_currentAI = EAIStrategy.None;
		_currentAction = EAIAction.None;
	}

	internal void ResumeMiniboss()
	{
		_currentAI = EAIStrategy.StandAttack;
		_nextActionTimer = 0f;
	}

	protected override void DetermineAction(float delta)
	{
		if (_currentAI != 0 && _isWild)
		{
			if (base.IsGrounded)
			{
				_isDoingChargeAbility = _level.NextRandomInt(1, 2) == 2;
				if (_isDoingChargeAbility)
				{
					_currentAI = EAIStrategy.StandAttack;
					_movementType = EAIMovementType.MoveJump;
					_maxJumpTime = 0.5f;
					_jumpLaunchVelocity = -100f;
					_maxFallSpeed = 500f;
					_maxMoveSpeed = 400f;
				}
				else
				{
					_currentAI = EAIStrategy.FollowAttack;
					_movementType = EAIMovementType.MoveJump;
					_maxJumpTime = 2f;
					_jumpLaunchVelocity = -200f;
					_maxFallSpeed = 800f;
					_maxMoveSpeed = 200f;
				}
			}
		}
		else
		{
			_isDoingChargeAbility = true;
		}
		base.DetermineAction(delta);
	}

	public override void UpdateAbility(float delta)
	{
		if (_isDoingChargeAbility)
		{
			if (_abilityTimer <= 0f)
			{
				SetState(EAFSM.Idle);
				ChangeAnimation(4, 4, 0.07f, EAnimationType.Cycle);
				_jumpTimer = 0f;
				PlayCue(_isMiniboss ? ESFX.EnemyWildCheveurCharge : ESFX.EnemyFledglingWarbirdCharge);
			}
			if (_abilityTimer < 1.5f)
			{
				if (_abilityTimer >= 1f && _lastAbilityTimer < 1f)
				{
					_level.AddAnimation(new BattleAnimation(_level.GCM.SpEffectsSmall, new Point(Bbox.Center.X + (IsFacingLeft ? 1 : (-1)), Bbox.Center.Y - 12), _level)
					{
						TeamSide = base.DefaultTeam,
						AnimationStart = 52,
						AnimationLength = 6
					});
				}
				_jumpTimer += delta;
				if (_jumpTimer >= 0.2f)
				{
					_isJumping = !_isJumping;
					_jumpTimer -= 0.2f;
				}
			}
			else if (_abilityTimer < 2f)
			{
				if (_lastAbilityTimer <= 1.5f)
				{
					ChangeAnimation(4, 4, 0.05f, EAnimationType.Cycle);
				}
				_chargeDustParticles.AddParticles(new Vector2(_isChargingLeft ? (Bbox.Left + 8) : (Bbox.Right - 8), Bbox.Bottom));
				_movementX = 300 * ((!IsFacingLeft) ? 1 : (-1));
			}
			else if (_abilityTimer < 2.4f)
			{
				_isJumping = false;
				if (_lastAbilityTimer <= 2f)
				{
					ChangeAnimation(4, 4, 0.05f, EAnimationType.Cycle);
				}
				else if (_abilityTimer >= 2.2f && _lastAbilityTimer < 2.2f)
				{
					IsFacingLeft = !IsFacingLeft;
				}
				float num = (2.4f - _abilityTimer) / 0.4f;
				_movementX = num * 300f * (float)((!_isChargingLeft) ? 1 : (-1));
				_chargeDustParticles.AddParticles(new Vector2(_isChargingLeft ? (Bbox.Left + 8) : (Bbox.Right - 8), Bbox.Bottom));
				_skidDustParticles.AddParticles(new Vector2(_isChargingLeft ? (Bbox.Left - 8) : (Bbox.Right + 8), Bbox.Bottom));
			}
			else
			{
				_isCarryingOutAbility = false;
				_animationIndex = 10;
				ChangeAnimation(0, 4, 0.18f, EAnimationType.Cycle);
				_currentAction = EAIAction.Idle;
				_nextActionTimer = 0.25f;
			}
			return;
		}
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

	public override void StartAbility(int whichAbility)
	{
		base.StartAbility(whichAbility);
		if (_isDoingChargeAbility)
		{
			_totalAbilityTime = 100f;
			_isChargingLeft = IsFacingLeft;
			_chargeDustParticles.IsParticleSystemFacingLeft = IsFacingLeft;
			_skidDustParticles.IsParticleSystemFacingLeft = !IsFacingLeft;
		}
		else if (!_isGrounded)
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
		if (_isMiniboss)
		{
			CutsceneBase.CreateAndCallCutscene(CutsceneBase.ECutsceneType.LakeSerene1_SeykisEnd, _level, Position);
		}
	}
}
