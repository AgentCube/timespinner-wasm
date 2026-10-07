using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class CursedSlime : Monster
{
	private const int SplashOffsetX = 4;

	private const int CeilingFallDistanceThresholdX = 56;

	private static readonly Color BaseDrawColor = Color.White * 0.8f;

	private readonly bool _didStartOnCeiling;

	private readonly SlimeSplashParticleSystem _landingParticles;

	private readonly CharacterSequenceSpecification _idleSequence;

	private readonly CharacterSequenceSpecification _moveSequence;

	private readonly CharacterSequenceSpecification _jumpSequence;

	private readonly CharacterSequenceSpecification _fallingSequence;

	private readonly CharacterSequenceSpecification _landSequence;

	private bool _hasLandedBefore;

	private bool _isOnCeiling;

	private bool _isJumpingDuringAbility;

	private bool _isFallingFromSlopedCeiling;

	private bool _isFallingBecausePlayerIsClose;

	private float _abilityMovementX;

	private SFXCueInstance _moveLoopCue;

	public bool IsOnCeiling
	{
		get
		{
			return _isOnCeiling;
		}
		set
		{
			_isOnCeiling = value;
			_isAffectedByGravity = !value;
			IsFlippedVertically = value;
		}
	}

	public CursedSlime(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.FollowAttack;
		_movementType = EAIMovementType.Walk;
		_agility = 0.1f;
		_bboxOffset = new Point(2, 2);
		Bbox = new Rectangle(_position.X, _position.Y, 24, 16);
		_isAfraidOfFalling = false;
		base.AggroBboxDimensions = new Point(300, 300);
		_timeToIdleAfterMoving = 0.1f;
		_doAppendagesMatchImageFacing = true;
		_landingParticles = new SlimeSplashParticleSystem(_level.GCM.TxParticleEnergy, 1)
		{
			BaseColor = new Vector4(0.45f, 0.3f, 0.4f, 0.8f)
		};
		_particleSystems.Add(_landingParticles);
		_jumpLaunchVelocity = -200f;
		_attackDistanceThresholdX = 80;
		if (base.CharacterSpecification != null)
		{
			_idleSequence = GetCharacterSequenceByName("Idle");
			_moveSequence = GetCharacterSequenceByName("Move");
			_jumpSequence = GetCharacterSequenceByName("Jump");
			_fallingSequence = GetCharacterSequenceByName("Falling");
			_landSequence = GetCharacterSequenceByName("Land");
		}
		IsOnCeiling = objectSpec.IsFlippedVertically;
		_didStartOnCeiling = IsOnCeiling;
		SetState(EAFSM.Idle);
	}

	public override void Update(float delta)
	{
		if (!_wasGrounded && _isGrounded)
		{
			if (_hasLandedBefore || _didStartOnCeiling)
			{
				_landingParticles.AddParticles(new Vector2(Bbox.Center.X + (IsFacingLeft ? (-4) : 4), Bbox.Bottom));
				PlayCue(ESFX.EnemySlimeLand);
			}
			_hasLandedBefore = true;
		}
		if (_isOnCeiling && !_isFallingFromSlopedCeiling)
		{
			Tile nearestSolidTile = _level.GetNearestSolidTile(Position, EDirection.North, 2);
			if (nearestSolidTile == null || nearestSolidTile.Type == ETileType.Slope)
			{
				StartAbility(1);
				_isFallingFromSlopedCeiling = true;
			}
		}
		base.DrawColor = BaseDrawColor;
		base.Update(delta);
	}

	public override void SetState(EAFSM state)
	{
		EAFSM currentState = _currentState;
		base.SetState(state);
		switch (state)
		{
		case EAFSM.Running:
			if (currentState != EAFSM.Running)
			{
				if (_moveLoopCue == null)
				{
					_moveLoopCue = CreateCue(ESFX.EnemySlimeMoveLoop, Position, isLooped: true);
					if (_moveLoopCue != null)
					{
						_moveLoopCue.PlayWhenInRange();
					}
				}
				else if (_moveLoopCue.IsPaused)
				{
					_moveLoopCue.Resume();
				}
			}
			SetCharacterSequence(_moveSequence);
			break;
		case EAFSM.Idle:
			if (_moveLoopCue != null && !_moveLoopCue.IsPaused)
			{
				_moveLoopCue.Pause(0.15f);
			}
			SetCharacterSequence(_idleSequence);
			break;
		}
	}

	public override void StartAbility(int whichAbility)
	{
		_abilityMovementX = (float)((!IsFacingLeft) ? 1 : (-1)) * 1f;
		base.StartAbility(whichAbility);
	}

	public override void UpdateAbility(float delta)
	{
		if (_abilityTimer <= 0f)
		{
			_isJumpingDuringAbility = false;
			_isFallingBecausePlayerIsClose = false;
			bool flag = true;
			if (IsOnCeiling)
			{
				Point nearestProtagonistPosition = _level.GetNearestProtagonistPosition(Position);
				int value = Position.X - nearestProtagonistPosition.X;
				if (Math.Abs(value) < 56)
				{
					flag = false;
					_isFallingBecausePlayerIsClose = true;
				}
			}
			if (flag)
			{
				PlayCue(ESFX.EnemySlimeLunge);
				SetCharacterSequence(_jumpSequence);
			}
			else
			{
				PlayCue(ESFX.EnemySlimeCeilingDrop);
				SetCharacterSequence(_fallingSequence);
				IsOnCeiling = false;
			}
		}
		else if (_isJumpingDuringAbility && !_isGrounded)
		{
			_movementX = _abilityMovementX;
		}
		else if ((_isJumpingDuringAbility || _isFallingBecausePlayerIsClose) && _isGrounded && _abilityTimer > 0.034f)
		{
			_isJumpingDuringAbility = false;
			_isJumping = false;
			_isCarryingOutAbility = false;
			_currentAction = EAIAction.Idle;
			SetState(EAFSM.Idle);
			_nextActionTimer = _timeToIdleAfterAttacking;
			SetCharacterSequence(_landSequence);
		}
	}

	internal override void TriggerCharacterAction(CharacterAction specification)
	{
		if (specification.IntArgument == 0)
		{
			_isJumpingDuringAbility = true;
			if (IsOnCeiling)
			{
				IsOnCeiling = false;
			}
			else
			{
				_isJumping = true;
			}
		}
	}

	protected override void UpdateDeathScript(float delta)
	{
		BattleAnimation battleAnimation = BattleAnimation.Create(EBattleAnimationType.WetSplashLarge, Bbox.Center, ETeamSide.Enemies, IsFacingLeft, _level, doesPlaySFX: true);
		battleAnimation.DrawColor = new Color(0.9f, 0.5f, 0.65f);
		_level.AddAnimation(battleAnimation);
		DropLootAndRemove();
	}
}
