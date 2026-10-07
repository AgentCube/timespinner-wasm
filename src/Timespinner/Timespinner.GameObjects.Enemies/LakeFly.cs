using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Enemies;

internal sealed class LakeFly : Monster
{
	private const int PlayerTargetOffsetY = -16;

	private const int FlightVelocity = 200;

	private readonly CharacterSequenceSpecification _idleSequence;

	private readonly CharacterSequenceSpecification _turnSequence;

	private readonly CharacterSequenceSpecification _startFlySequence;

	private readonly CharacterSequenceSpecification _endFlySequence;

	private bool _isTargetInWater;

	private Vector2 _flightVector;

	private Vector2 _targetVector;

	private Vector2 _incrementVector;

	private SFXCueInstance _buzzSFXCueInstance;

	public LakeFly(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.CustomScriptAI;
		_nonAggroAction = EAIAction.FloatInPlace;
		_agility = 0.5f;
		_timeToMove = 1f;
		_bboxOffset = new Point(4, 5);
		Bbox = new Rectangle(_position.X, _position.Y, 13, 14);
		base.AggroBboxDimensions = new Point(400, 250);
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_isFlying = true;
		_isIgnoringPlatform = true;
		_isAffectedByGravity = false;
		_doAppendagesMatchImageFacing = true;
		_timeToTurnAround = 0.05f;
		base.DoesNotMakeSplashesInWater = true;
		if (base.CharacterSpecification != null && base.CharacterSpecification.Sequences.Count > 3)
		{
			_idleSequence = base.CharacterSpecification.Sequences[0];
			_turnSequence = base.CharacterSpecification.Sequences[1];
			_startFlySequence = base.CharacterSpecification.Sequences[2];
			_endFlySequence = base.CharacterSpecification.Sequences[3];
			SetCharacterSequence(_idleSequence);
		}
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (base.IsAggroed)
			{
				_isAlwaysAggroed = true;
			}
			if (IsInWater)
			{
				Kill();
			}
			Protagonist mainHero = _level.MainHero;
			if (mainHero != null && (mainHero.IsInWater || mainHero.WasInWater))
			{
				_isTargetInWater = true;
			}
			else
			{
				_isTargetInWater = false;
			}
			if (_buzzSFXCueInstance == null)
			{
				_buzzSFXCueInstance = CreateCue(ESFX.EnemyFlyLoop, Position, isLooped: true);
				if (_buzzSFXCueInstance != null)
				{
					_buzzSFXCueInstance.RangeMultiplier = 0.5f;
					_buzzSFXCueInstance.UpdateType = SFXCueInstance.ECueInstanceUpdateType.Anchor;
					_buzzSFXCueInstance.Anchor = this;
					_buzzSFXCueInstance.PlayWhenInRange();
				}
			}
		}
		base.Update(delta);
	}

	protected override void PickNextCustomScriptAIAction(float delta)
	{
		if (!_isTargetInWater)
		{
			Point nearestProtagonistPosition = _level.GetNearestProtagonistPosition(_position);
			nearestProtagonistPosition = OvershootTarget(nearestProtagonistPosition.Add(0, -16));
			IsFacingLeft = nearestProtagonistPosition.X < _position.X;
			_currentAction = EAIAction.Custom;
			_targetVector = new Vector2(nearestProtagonistPosition.X - Position.X, nearestProtagonistPosition.Y - Position.Y);
			_targetVector.Normalize();
			_incrementVector = Vector2.Subtract(_targetVector, _flightVector);
			_nextActionTimer = _timeToMove;
			_totalActionTimer = _nextActionTimer;
			_followTimer = 0f;
		}
		else
		{
			_currentAction = _nonAggroAction;
			_nextActionTimer = (float)_random.Next(5, 10) * 0.1f;
		}
	}

	protected override void UpdateCustomScriptAIAction(float delta)
	{
		if (!_isTargetInWater)
		{
			_flightVector = new Vector2(_flightVector.X + _incrementVector.X * delta, _flightVector.Y + _incrementVector.Y * delta);
			ManageState(EAFSM.Moving);
			_velocity = Vector2.Multiply(_flightVector, 200f);
		}
		else
		{
			_currentAction = _nonAggroAction;
			_nextActionTimer = (float)_random.Next(5, 10) * 0.1f;
		}
	}

	public override void SetState(EAFSM state)
	{
		switch (state)
		{
		case EAFSM.Idle:
			if (_endFlySequence != null)
			{
				SetCharacterSequence(_endFlySequence);
			}
			break;
		case EAFSM.Running:
			SetCharacterSequence(_startFlySequence);
			break;
		}
		base.SetState(state);
	}

	protected override void DoTurningAroundAnimation(EAFSM lastState, bool lastFacingRight)
	{
		SetCharacterSequence(_turnSequence);
		base.DoTurningAroundAnimation(lastState, lastFacingRight);
	}

	protected override void UpdateDeathScript(float delta)
	{
		Point center = Bbox.Center;
		BattleAnimation battleAnimation = BattleAnimation.Create(EBattleAnimationType.WetSplashSmall, center, ETeamSide.Enemies, IsFacingLeft, _level, doesPlaySFX: true);
		battleAnimation.ParticleSystem = new InsectWingParticleSystem(_level, _sprite, center, 1, 13, 4);
		_level.AddAnimation(battleAnimation);
		DropLootAndRemove();
	}
}
