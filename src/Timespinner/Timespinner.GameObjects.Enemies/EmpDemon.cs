using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Misc;
using Timespinner.GameObjects.Heroes.Familiars.Projectiles;

namespace Timespinner.GameObjects.Enemies;

internal sealed class EmpDemon : Monster
{
	private const float IdleAnimationSpeed = 0.1f;

	private readonly CharacterSequenceSpecification _castSequence;

	private readonly EmpDemonSpellDamageArea _projectile;

	private bool _hasPlayedSpellCueBeforeCast;

	private float _recastTimer;

	public EmpDemon(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.FollowAttack;
		_movementType = EAIMovementType.Fly;
		_agility = 0.25f;
		_timeToMove = 0.5f;
		_attackDistanceThresholdX = 128;
		_retreatDistanceThresholdX = 60;
		_isAffectedByGravity = false;
		_isIgnoringPlatform = true;
		base.DoesCollideWithTiles = false;
		_isAlwaysAggroed = true;
		_isFlying = true;
		_bboxOffset = new Point(6, 5);
		Bbox = new Rectangle(_position.X, _position.Y, 16, 20);
		_timeToTurnAround = 0f;
		base.DoesDrawAura = true;
		base.AuraColor = new Color(0.3f, 0.25f, 0.7f, 0.25f);
		base.AuraOffset = new Vector2(2f, 2f);
		base.AuraFrequency = 9f;
		base.AuraSize = 0.075f;
		_auraCount = 5f;
		ChangeAnimation(5, 5, 0.1f, EAnimationType.Cycle);
		_castSequence = GetCharacterSequenceByName("Cast");
		_projectile = new EmpDemonSpellDamageArea(_level, Position, Vector2.Zero, ETeamSide.Enemies, _sprite, base.Damage);
	}

	public override void SetState(EAFSM state)
	{
		base.SetState(state);
		if (state == EAFSM.Running)
		{
			ChangeAnimation(5, 5, 0.1f, EAnimationType.Cycle);
		}
	}

	public override void Update(float delta)
	{
		if (_recastTimer > 0f)
		{
			_recastTimer -= delta;
		}
		base.Update(delta);
	}

	protected override void UpdateDeathScript(float delta)
	{
		DistintegrateEvent newEvent = new DistintegrateEvent(this, _sprite, EDisintegrateType.Chaos, 19, 4, new Vector4(0.8f, 0.3f, 0.9f, 1f));
		_level.AddEvent(newEvent);
		DropLootAndRemove();
		if (_hasPlayedSpellCueBeforeCast && _projectile != null)
		{
			_projectile.StopAllSFX();
		}
	}

	public override void UpdateAbility(float delta)
	{
		if (_abilityTimer <= 0f)
		{
			if (_recastTimer <= 0f)
			{
				_projectile.Position = Bbox.Center;
				_projectile.PlayCue(ESFX.EnemyDemonCast);
				_hasPlayedSpellCueBeforeCast = true;
				SetCharacterSequence(_castSequence);
			}
			else
			{
				FinishAttack(0f);
			}
		}
	}

	internal override void TriggerCharacterAction(CharacterAction specification)
	{
		if (specification != null)
		{
			switch (specification.IntArgument)
			{
			case 0:
			{
				Point nearestProtagonistPosition = _level.GetNearestProtagonistPosition(Position);
				Point center = Bbox.Center;
				Vector2 iV = new Vector2(nearestProtagonistPosition.X - center.X, nearestProtagonistPosition.Y - center.Y);
				iV.Normalize();
				iV *= 100f;
				int spellDamage = (int)Math.Ceiling((float)base.Damage * 1.35f);
				_projectile.Reset(center, iV, spellDamage);
				_level.AddProjectile(_projectile);
				_hasPlayedSpellCueBeforeCast = false;
				_recastTimer = 3.2f;
				break;
			}
			case 1:
				FinishAttack(_timeToIdleAfterAttacking);
				break;
			}
		}
	}

	private void FinishAttack(float idleTime)
	{
		_isCarryingOutAbility = false;
		_currentAction = EAIAction.Idle;
		_nextActionTimer = idleTime;
	}
}
