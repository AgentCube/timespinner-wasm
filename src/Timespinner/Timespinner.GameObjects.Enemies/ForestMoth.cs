using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Enemies._03_Forest;
using Timespinner.GameObjects.StatusEffects;

namespace Timespinner.GameObjects.Enemies;

internal sealed class ForestMoth : Monster
{
	private const int PlayerTargetOffsetY = -48;

	private const int FlightVelocity = 75;

	private const int FlapCueIndex = 2;

	private const float TimeBetweenSporeEmissions = 0.5f;

	private const float UnaggroedAnimationSpeed = 0.1f;

	private readonly ForestMothSporeParticleSystem _sporeParticles;

	private float _sporeEmissionTimer;

	private Vector2 _flightVector;

	public ForestMoth(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.CustomScriptAI;
		_agility = 0.2f;
		_timeToMove = 2f;
		_isAffectedByGravity = false;
		_bboxOffset = new Point(12, 13);
		Bbox = new Rectangle(_position.X, _position.Y, 14, 14);
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_nonAggroAction = EAIAction.FloatInPlace;
		base.AggroBboxDimensions = new Point(400, 300);
		_currentState = EAFSM.Idle;
		ChangeAnimation(0, 5, 0.1f, EAnimationType.Cycle);
		_nonAggroAction = EAIAction.FloatInPlace;
		Color color = new Color(0.6f, 0.45f, 0.75f, 0.75f);
		_sporeParticles = new ForestMothSporeParticleSystem(_sprite, 10, color.ToVector4());
		_particleSystems.Add(_sporeParticles);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && base.IsAggroed)
		{
			_sporeEmissionTimer += delta;
			if (_sporeEmissionTimer >= 0.5f)
			{
				_sporeEmissionTimer -= 0.5f;
				EmitSpores();
			}
		}
		int animationIndex = base.AnimationIndex;
		base.Update(delta);
		if (base.IsWithinObjectVisibleArea && animationIndex != base.AnimationIndex && base.AnimationIndex == 2)
		{
			PlayCue(ESFX.EnemyPoisonMothWingFlap);
		}
	}

	private void EmitSpores()
	{
		_sporeParticles.AddParticles(Bbox.Center.ToVector2());
		_level.AddProjectile(new ForestMothSporeDamageArea(_level, Bbox.Center, base.Damage));
	}

	protected override void PickNextCustomScriptAIAction(float delta)
	{
		if (_currentAction == _nonAggroAction)
		{
			Point nearestProtagonistPosition = _level.GetNearestProtagonistPosition(_position);
			nearestProtagonistPosition = OvershootTarget(nearestProtagonistPosition.Add(0, -48));
			IsFacingLeft = nearestProtagonistPosition.X < _position.X;
			_currentAction = EAIAction.Custom;
			_flightVector = new Vector2(nearestProtagonistPosition.X - Position.X, nearestProtagonistPosition.Y - Position.Y);
			_flightVector.Normalize();
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
		ManageState(EAFSM.Moving);
		_velocity = Vector2.Multiply(_flightVector, 75f);
		if (Position.Y < 0)
		{
			Position = new Point(Position.X, 0);
		}
	}

	protected override void AfterDealingTouchDamage(Alive target, Point effectPosition)
	{
		target.GiveStatusEffect(EStatusEffectType.Poison, 0);
		base.AfterDealingTouchDamage(target, effectPosition);
	}

	protected override void UpdateDeathScript(float delta)
	{
		Point center = Bbox.Center;
		BattleAnimation battleAnimation = BattleAnimation.Create(EBattleAnimationType.Poof, center, ETeamSide.Enemies, IsFacingLeft, _level, doesPlaySFX: true);
		battleAnimation.ParticleSystem = new InsectWingParticleSystem(_level, _sprite, center, 1, 5, 4);
		_level.AddAnimation(battleAnimation);
		DropLootAndRemove();
		_level.AddAnimation(new BattleAnimation(null, Position, _level)
		{
			ParticleSystem = _sporeParticles
		});
	}
}
