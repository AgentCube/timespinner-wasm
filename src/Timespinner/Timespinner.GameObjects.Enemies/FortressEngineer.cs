using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class FortressEngineer : Monster
{
	private enum EFortressEngineerStrategyType
	{
		None,
		MurderHole
	}

	private const int BombBagSize = 5;

	private const float TimeBetweenThrowingRock = 1f;

	private const float TimeBetweenSideThrows = 0.25f;

	private const float TimeBeforeRunningAwayAfterPanicking = 1f;

	private static readonly Point PanicThresholdDimensions = new Point(112, 80);

	private readonly EFortressEngineerStrategyType _type;

	private readonly CharacterSequenceSpecification _idleSequence;

	private readonly CharacterSequenceSpecification _kneelSequence;

	private readonly CharacterSequenceSpecification _kneelWorkSequence;

	private readonly CharacterSequenceSpecification _shockedSequence;

	private readonly CharacterSequenceSpecification _runSequence;

	private readonly FortressEngineerBomb[] _bombBag = new FortressEngineerBomb[5];

	private bool _isPanicking;

	private bool _isRunningAway;

	private int _bombBagIndex;

	private float _panickTimer;

	public FortressEngineer(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification spec)
		: base(inPosition, inLevel, inSprite, inID, spec)
	{
		IsFacingLeft = !spec.IsFlippedHorizontally;
		Position = Position.Add(IsFacingLeft ? (-8) : 8, 0);
		_currentAI = EAIStrategy.CustomScriptAI;
		_agility = 0.75f;
		_isAffectedByGravity = true;
		_isFlying = false;
		_bboxOffset = new Point(8, 6);
		Bbox = new Rectangle(_position.X, _position.Y, 16, 32);
		_timeToTurnAround = 0f;
		_doesUseAppendageCollision = false;
		_doAppendagesMatchImageFacing = true;
		_doAppendagesInheritDrawColor = false;
		_isIgnoringPlatform = false;
		if (base.CharacterSpecification != null && base.CharacterSpecification.Sequences.Count > 4)
		{
			_idleSequence = base.CharacterSpecification.Sequences[0];
			_kneelSequence = base.CharacterSpecification.Sequences[1];
			_kneelWorkSequence = base.CharacterSpecification.Sequences[2];
			_shockedSequence = base.CharacterSpecification.Sequences[3];
			_runSequence = base.CharacterSpecification.Sequences[4];
		}
		if (spec.DoesHaveArgument)
		{
			int argument = spec.Argument;
			if (argument == 1)
			{
				_type = EFortressEngineerStrategyType.MurderHole;
				_isAlwaysAggroed = true;
				if (IsFacingLeft)
				{
					_currentAction = EAIAction.Idle;
					_nextActionTimer = 0.5f;
				}
				SetCharacterSequence(_kneelWorkSequence);
			}
			else
			{
				_type = EFortressEngineerStrategyType.None;
				SetCharacterSequence(_idleSequence);
			}
		}
		else
		{
			SetCharacterSequence(_idleSequence);
		}
		SnapBboxToPosition();
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_isPanicking && !_isRunningAway)
			{
				_panickTimer += delta;
				if (_panickTimer >= 1f)
				{
					SetCharacterSequence(_runSequence);
					_isRunningAway = true;
					IsFacingLeft = !IsFacingLeft;
					_isMovingLeft = IsFacingLeft;
					_currentAction = EAIAction.Move;
					_currentAI = EAIStrategy.Pace;
					_paceLength = 5f;
					_isAfraidOfFalling = false;
					_isAlwaysAggroed = true;
					base.DoesTouchDamageKnockback = true;
				}
			}
			else if (!_isPanicking && base.HP < base.MaxHP)
			{
				Point nearestProtagonistPosition = _level.GetNearestProtagonistPosition(Position);
				StartPanic(nearestProtagonistPosition);
			}
		}
		base.Update(delta);
	}

	protected override void OnAggroed()
	{
		_nextActionTimer = (IsImageFacingLeft ? 0f : 0.25f);
		if (!_isPanicking && _type == EFortressEngineerStrategyType.MurderHole)
		{
			SetCharacterSequence(_kneelWorkSequence);
		}
		base.OnAggroed();
	}

	protected override void OnDeAggroed()
	{
		if (!_isPanicking && _type == EFortressEngineerStrategyType.MurderHole)
		{
			SetCharacterSequence(_kneelSequence);
		}
	}

	private void StartPanic(Point target)
	{
		_isPanicking = true;
		SetCharacterSequence(_shockedSequence);
		_isCarryingOutAbility = false;
		_currentAction = EAIAction.Idle;
		PlayCue(ESFX.EnemyEngineerSurprised, Position);
		IsFacingLeft = Position.X > target.X;
	}

	protected override void PickNextCustomScriptAIAction(float delta)
	{
		if (_isPanicking)
		{
			return;
		}
		Point nearestProtagonistPosition = _level.GetNearestProtagonistPosition(Position);
		if (Math.Abs(nearestProtagonistPosition.X - Position.X) < PanicThresholdDimensions.X && Math.Abs(nearestProtagonistPosition.Y - Position.Y) < PanicThresholdDimensions.Y)
		{
			StartPanic(nearestProtagonistPosition);
			return;
		}
		EFortressEngineerStrategyType type = _type;
		if (type == EFortressEngineerStrategyType.MurderHole)
		{
			if (_lastAction == EAIAction.DoAbility)
			{
				_currentAction = EAIAction.Idle;
				_nextActionTimer = 1f;
			}
			else
			{
				_currentAction = EAIAction.DoAbility;
				_selectedAbility = 1;
				_nextActionTimer = 0f;
			}
		}
	}

	public override void UpdateAbility(float delta)
	{
		EFortressEngineerStrategyType type = _type;
		if (type == EFortressEngineerStrategyType.MurderHole)
		{
			ThrowMurderHoleRock();
			_isCarryingOutAbility = false;
		}
	}

	private void ThrowMurderHoleRock()
	{
		Point point = Position.Add(IsFacingLeft ? (-24) : 24, 0);
		FortressEngineerBomb fortressEngineerBomb;
		if (_bombBag[_bombBagIndex] == null)
		{
			fortressEngineerBomb = new FortressEngineerBomb(point, _level, _sprite, -1, base.Damage);
			_bombBag[_bombBagIndex] = fortressEngineerBomb;
		}
		else
		{
			fortressEngineerBomb = _bombBag[_bombBagIndex];
			fortressEngineerBomb.Reset(point);
		}
		_level.AddProjectile(fortressEngineerBomb);
		_bombBagIndex++;
		if (_bombBagIndex >= 5)
		{
			_bombBagIndex = 0;
		}
	}

	protected override void UpdateDeathScript(float delta)
	{
		_level.AddAnimation(EBattleAnimationType.AuraExplosion, Position, ETeamSide.Enemies, IsFacingLeft, doesPlaySFX: true);
		DropLootAndRemove();
	}
}
