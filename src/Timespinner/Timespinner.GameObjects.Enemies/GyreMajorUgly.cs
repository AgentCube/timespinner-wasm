using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Enemies._14_Gyre;
using Timespinner.GameObjects.Events.Misc;

namespace Timespinner.GameObjects.Enemies;

internal sealed class GyreMajorUgly : Monster
{
	private const int MaxBoofs = 3;

	private const float TimeBeforeEmittingBoofWave = 0.1f;

	private const float TimeBeforeNextBoof = 0.5f;

	private const float TimeForOneBoof = 0.6f;

	private const float TimeForAllBoofs = 1.8f;

	private const float TimeForEntireAbility = 2.3f;

	private const float TimeDeadBeforeFading = 0.25f;

	private readonly GyreMajorUglyBoofDamageArea[] _boofs = new GyreMajorUglyBoofDamageArea[3];

	private bool _hasCreatedBoofs;

	public GyreMajorUgly(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.StandAttack;
		_attackDistanceThresholdX = 160;
		_attackDistanceThresholdY = -16;
		_agility = 0.6f;
		_bboxOffset = new Point(8, 14);
		Bbox = new Rectangle(_position.X, _position.Y, 24, 50);
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_maxJumpTime = 0.2f;
		_jumpLaunchVelocity = -400f;
		_currentSproingTime = 0.11f;
		_wasGrounded = true;
		_timeToTurnAround = 0.05f;
		_isGrounded = true;
		_wasGrounded = true;
		base.DoesTouchDamageKnockback = true;
		base.DoesNotTurnToFacePlayer = true;
		base.TimeToTurnAround = 0f;
		ChangeAnimation(0);
	}

	public override void UpdateAbility(float delta)
	{
		if (_abilityTimer <= 0f)
		{
			SetState(EAFSM.Idle);
			ChangeAnimation(1, 3, 0.1f, EAnimationType.Once);
			PlayCue(ESFX.EnemyOrnagyRutBark);
		}
		for (int i = 0; i < 3; i++)
		{
			float num = (float)i * 0.6f + 0.1f;
			if (_abilityTimer >= num && _lastAbilityTimer < num)
			{
				EmitBoof(i);
			}
			float num2 = (float)i * 0.6f;
			if (_abilityTimer >= num2 && _lastAbilityTimer < num2)
			{
				ChangeAnimation(1, 3, 0.1f, EAnimationType.Once);
			}
		}
		if (_abilityTimer >= 2.3f)
		{
			_isCarryingOutAbility = false;
			ChangeAnimation(0, 1, 0f, EAnimationType.Once, 1, 1, 0.1f);
			_currentAction = EAIAction.Idle;
			_nextActionTimer = _timeToIdleAfterAttacking;
		}
	}

	private void EmitBoof(int boofIndex)
	{
		float num = (IsFacingLeft ? (-1f) : 1f);
		Point center = _bbox.Center;
		center.X += (int)(18f * num);
		center.Y -= 14;
		if (!_hasCreatedBoofs)
		{
			_hasCreatedBoofs = true;
			for (int i = 0; i < 3; i++)
			{
				GyreMajorUglyBoofDamageArea gyreMajorUglyBoofDamageArea = new GyreMajorUglyBoofDamageArea(_level, center, base.Damage);
				_boofs[i] = gyreMajorUglyBoofDamageArea;
			}
		}
		GyreMajorUglyBoofDamageArea gyreMajorUglyBoofDamageArea2 = _boofs[boofIndex];
		gyreMajorUglyBoofDamageArea2.Reset(center);
		_level.AddProjectile(gyreMajorUglyBoofDamageArea2);
	}

	protected override void UpdateDeathScript(float delta)
	{
		if (_deathScriptTimer <= 0f && delta > 0f)
		{
			DropLoot();
			_level.PlayCue(ESFX.EnemyOrnagyRutDeath, Position);
			ChangeAnimation(1, 2, 0.1f, EAnimationType.Once);
		}
		if (_deathScriptTimer > 0.25f)
		{
			DistintegrateEvent newEvent = new DistintegrateEvent(this, _sprite, EDisintegrateType.Sand, -1, 0, new Vector4(0.75f, 0.65f, 0.55f, 1f));
			_level.AddEvent(newEvent);
			RemoveInstance();
		}
		else
		{
			UpdateAnimation(delta);
		}
		_deathScriptTimer += delta;
	}
}
