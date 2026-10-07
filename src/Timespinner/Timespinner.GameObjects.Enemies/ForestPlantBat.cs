using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class ForestPlantBat : Monster
{
	private const int PlayerTargetOffsetY = 16;

	private const int LiftForceForFlap = -7500;

	private const int LiftForceDecay = 15000;

	private const float TimeToFallFromCeiling = 0.5f;

	private const float BaseGravityRate = 1500f;

	private const float IdleFlightVelocity = 100f;

	private const float AttackFlightVelocity = 250f;

	private const float TimeToWaitBeforeAttackingAgain = 4f;

	private readonly bool _isExterminationQuestActive;

	private readonly Point _originalPosition;

	private readonly CharacterSequenceSpecification _idleSequence;

	private readonly CharacterSequenceSpecification _flySequence;

	private readonly CharacterSequenceSpecification _attackSequence;

	private bool _isOnCeiling = true;

	private bool _isFallingFromCeiling;

	private bool _isAttacking;

	private bool _wasAttacking;

	private int _waterHeight;

	private float _fallFromCeilingTimer;

	private float _liftForce;

	private float _timeSinceAttacking;

	private Point _lastHeroLocation;

	private Vector2 _flightVector;

	private Vector2 _incrementVector;

	public ForestPlantBat(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_originalPosition = inPosition;
		_doAppendagesMatchImageFacing = true;
		_doAppendagesInheritDrawColor = true;
		_doesUseAppendageCollision = false;
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_agility = 0.6f;
		_timeToIdleAfterMoving = 2f;
		base.IsDormant = true;
		base.AggroBboxDimensions = new Point(400, 300);
		_isExterminationQuestActive = NPCBase.GetPrimaryQuestState(NPCBase.ENPCType.Captain, _level.GameSave) == 0 && NPCBase.GetSubQuestState(NPCBase.ENPCType.Captain, _level.GameSave) > 0;
		if (base.CharacterSpecification != null)
		{
			_idleSequence = GetCharacterSequenceByName("Idle");
			_flySequence = GetCharacterSequenceByName("Fly");
			_attackSequence = GetCharacterSequenceByName("Attack");
			if (_idleSequence != null)
			{
				SetCharacterSequence(_idleSequence);
			}
		}
	}

	public override void InitializeMob()
	{
		_waterHeight = _level.GetWaterTopFromAboveWater(Position);
		base.InitializeMob();
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_isOnCeiling)
			{
				UpdateWatchForPlayer();
			}
			else if (_isFallingFromCeiling)
			{
				UpdateFallFromCeiling(delta);
			}
		}
		base.Update(delta);
	}

	private void UpdateWatchForPlayer()
	{
		if (!_isAggroed)
		{
			return;
		}
		_currentTarget = _level.GetNearestProtagonist(Position);
		if (_currentTarget != null)
		{
			if (_lastHeroLocation != Point.Zero && Position.Y < _currentTarget.Position.Y && Position.X >= _currentTarget.Position.X != Position.X >= _lastHeroLocation.X)
			{
				StartFall();
			}
			_lastHeroLocation = _currentTarget.Position;
		}
	}

	private void UpdateFallFromCeiling(float delta)
	{
		_fallFromCeilingTimer += delta;
		if (_fallFromCeilingTimer >= 0.5f)
		{
			base.Rotation = 0f;
			_airDragFactor = 0.3f;
			_isAffectedByGravity = false;
			_isFlying = true;
			_isFallingFromCeiling = false;
			_currentAI = EAIStrategy.CustomScriptAI;
			_timeToMove = 1f;
			_nonAggroAction = EAIAction.Custom;
			SetCharacterSequence(_flySequence);
			_isAttacking = true;
		}
		else
		{
			float num = _fallFromCeilingTimer / 0.5f;
			float num2 = (float)Math.Cos(num * ((float)Math.PI / 2f));
			base.Rotation = -(float)Math.PI * num2;
			_gravityAcceleration = 1500f * num2;
		}
	}

	public override bool ManageDamage(int damage, Vector2 velocity, Point where, Rectangle sourceRectangle, EDamageType type, EDamageElement element, bool doesKnockBack)
	{
		bool flag = base.ManageDamage(damage, velocity, where, sourceRectangle, type, element, doesKnockBack);
		if (flag && _isOnCeiling)
		{
			StartFall();
		}
		return flag;
	}

	private void StartFall()
	{
		_isOnCeiling = false;
		_isFallingFromCeiling = true;
		_isAffectedByGravity = true;
		_isFlying = false;
		base.IsDormant = false;
		_airDragFactor = 0f;
		_fallFromCeilingTimer = 0f;
		PlayCue(ESFX.EnemyPlantBatDrop);
	}

	private void DoLift()
	{
		_liftForce = -7500f;
		PlayCue(ESFX.EnemyPlantBatWingFlap);
	}

	internal override void TriggerCharacterAction(CharacterAction specification)
	{
		switch (specification.IntArgument)
		{
		case 0:
			_currentAI = EAIStrategy.None;
			_nonAggroAction = EAIAction.None;
			_currentState = EAFSM.Idle;
			_isOnCeiling = true;
			_isFallingFromCeiling = false;
			_isAffectedByGravity = false;
			_isFlying = true;
			Position = _originalPosition;
			base.Rotation = -(float)Math.PI;
			break;
		case 1:
			StartFall();
			break;
		case 2:
			DoLift();
			break;
		}
	}

	protected override void PickNextCustomScriptAIAction(float delta)
	{
		Point nearestProtagonistPosition = _level.GetNearestProtagonistPosition(_position);
		nearestProtagonistPosition = OvershootTarget(nearestProtagonistPosition.Add(0, 16));
		IsFacingLeft = nearestProtagonistPosition.X < _position.X;
		if (nearestProtagonistPosition.Y > _waterHeight)
		{
			nearestProtagonistPosition = new Point(nearestProtagonistPosition.X, _waterHeight);
		}
		_currentAction = EAIAction.Custom;
		Vector2 value = new Vector2(nearestProtagonistPosition.X - Position.X, nearestProtagonistPosition.Y - Position.Y);
		value.Normalize();
		_incrementVector = Vector2.Subtract(value, _flightVector);
		_nextActionTimer = _timeToMove;
		_totalActionTimer = _nextActionTimer;
		_followTimer = 0f;
	}

	protected override void AfterDealingTouchDamage(Alive target, Point effectPosition)
	{
		if (_isAttacking)
		{
			_isAttacking = false;
			_timeSinceAttacking = 0f;
			SetCharacterSequence(_flySequence);
		}
		base.AfterDealingTouchDamage(target, effectPosition);
	}

	protected override void UpdateCustomScriptAIAction(float delta)
	{
		if (!_isAttacking && base.IsAggroed)
		{
			_timeSinceAttacking += delta;
			if (_timeSinceAttacking >= 4f)
			{
				_isAttacking = true;
			}
		}
		if (_isAttacking && !_wasAttacking)
		{
			SetCharacterSequence(_attackSequence);
			PlayCue(ESFX.EnemyPlantBatGnaw);
		}
		_liftForce += 15000f * delta;
		_flightVector = new Vector2(_flightVector.X + _incrementVector.X * delta, _flightVector.Y + _incrementVector.Y * delta);
		ManageState(EAFSM.Moving);
		_velocity = Vector2.Multiply(_flightVector, _isAttacking ? 250f : 100f);
		_velocity = new Vector2(_velocity.X, _velocity.Y + _liftForce * delta);
		if (_isAttacking && _velocity.Y < -200f)
		{
			_velocity.Y = -200f;
		}
		_wasAttacking = _isAttacking;
		base.DoesTouchDamageKnockback = _isAttacking;
	}

	protected override void UpdateDeathScript(float delta)
	{
		_level.AddAnimation(EBattleAnimationType.WetSplashLarge, Bbox.Center, ETeamSide.Enemies, IsFacingLeft, doesPlaySFX: true);
		DropLootAndRemove();
		if (_isExterminationQuestActive && !_level.GameSave.GetSaveBool("HasShownPlantBatQuestFinished"))
		{
			int newEnemyKillCount = NPCBase.GetNewEnemyKillCount(EEnemyTileType.ForestPlantBat, _level.GameSave);
			if (newEnemyKillCount >= 15)
			{
				_level.AddScript(new ScriptAction(NPCBase.ENPCType.Captain, 1));
				_level.GameSave.SetValue("HasShownPlantBatQuestFinished", value: true);
			}
		}
	}
}
