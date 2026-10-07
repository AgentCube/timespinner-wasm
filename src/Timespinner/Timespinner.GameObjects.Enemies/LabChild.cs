using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.StatusEffects;

namespace Timespinner.GameObjects.Enemies;

internal sealed class LabChild : Monster
{
	private const int UnfrozenMaxSpeed = 300;

	private readonly GlowTexture _glowTexture;

	private bool _isFallingDuringJump;

	private bool _isDefyingTimeFreeze;

	private float _abilityMovementX;

	private SFXCueInstance _moveLoopCueInstance;

	public LabChild(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.FollowAttack;
		_movementType = EAIMovementType.Walk;
		_agility = 1f;
		_maxMoveSpeed = 300f;
		_attackDistanceThresholdX = 256;
		_canLoseAggro = false;
		base.IsAffectedByTime = false;
		_bboxOffset = new Point(6, 10);
		Bbox = new Rectangle(_position.X, _position.Y, 28, 12);
		_glowTexture = new GlowTexture(_level)
		{
			GlowCircleRadius = 64,
			BaseColor = new Color(0.5f, 0.1f, 0.15f, 0.1f)
		};
		_jumpLaunchVelocity = -250f;
		_trailFadeRate = 3f;
		_trailLength = 8;
	}

	public override void Update(float delta)
	{
		if (_isDefyingTimeFreeze)
		{
			delta *= 0.75f;
		}
		base.Update(delta);
		_glowTexture.Center = Bbox.Center;
		_glowTexture.Update(delta);
	}

	public override void Freeze()
	{
		_isDefyingTimeFreeze = true;
		_doesDrawTrail = true;
		ClearTrailHistory();
		base.Freeze();
	}

	public override void Unfreeze()
	{
		_isDefyingTimeFreeze = false;
		_doesDrawTrail = false;
		base.Unfreeze();
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		_glowTexture.Draw(spriteBatch);
		base.Draw(spriteBatch);
	}

	public override void SetState(EAFSM state)
	{
		switch (state)
		{
		case EAFSM.Idle:
			ChangeAnimation(0);
			StopRunningCue();
			break;
		case EAFSM.Running:
			ChangeAnimation(1, 6, 0.065f, EAnimationType.Cycle);
			StartRunningCue();
			break;
		}
		base.SetState(state);
	}

	private void StartRunningCue()
	{
		if (_moveLoopCueInstance == null)
		{
			_moveLoopCueInstance = CreateCue(ESFX.EnemyLabChildMoveLoop, Position, isLooped: true);
			if (_moveLoopCueInstance != null)
			{
				_moveLoopCueInstance.PlayWhenInRange();
			}
		}
		else if (_moveLoopCueInstance.IsPaused)
		{
			_moveLoopCueInstance.Resume();
		}
	}

	private void StopRunningCue()
	{
		if (_moveLoopCueInstance != null && !_moveLoopCueInstance.IsPaused)
		{
			_moveLoopCueInstance.Pause(0.1f);
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
			ChangeAnimation(7);
			_isJumping = true;
			_isFallingDuringJump = false;
			PlayCue(ESFX.EnemyLabChildShriek);
			StopRunningCue();
		}
		else if (_isGrounded && _abilityTimer > 0.034f)
		{
			_isJumping = false;
			_isCarryingOutAbility = false;
			_currentAction = EAIAction.Idle;
			SetState(EAFSM.Idle);
			_nextActionTimer = _timeToIdleAfterAttacking;
			ChangeAnimation(0);
		}
		else if (_velocity.Y > 1f && !_isFallingDuringJump)
		{
			_isFallingDuringJump = true;
			ChangeAnimation(8);
		}
	}

	protected override void AfterDealingTouchDamage(Alive target, Point effectPosition)
	{
		target.GiveStatusEffect(EStatusEffectType.Chaos, 0);
		base.AfterDealingTouchDamage(target, effectPosition);
	}

	protected override void UpdateDeathScript(float delta)
	{
		BattleAnimation battleAnimation = BattleAnimation.Create(EBattleAnimationType.WetSplashLarge, Bbox.Center, ETeamSide.Enemies, IsFacingLeft, _level, doesPlaySFX: true);
		battleAnimation.DrawColor = new Color(0.7f, 0.25f, 0.5f, 1f);
		_level.AddAnimation(battleAnimation);
		DropLootAndRemove();
	}
}
