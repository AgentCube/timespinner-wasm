using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.BaseClasses;

namespace Timespinner.GameObjects.Heroes.LunaisProjectiles;

internal sealed class BlueOrbSpellBulletSmall : LunaisBaseProjectile
{
	private const int Width = 32;

	private const int Height = 15;

	private const float MaxLife = 1.25f;

	internal bool IsFinished { get; private set; }

	public BlueOrbSpellBulletSmall(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, float oscillOffset, float dormantTime, int spellDamage, LunaisOrbAbility parentOrb)
		: base(inLevel, inPosition, iV, inSide, dormantTime, 0, parentOrb)
	{
		_sprite = _level.GCM.SpOrbMeleeBlue;
		_bbox = new Rectangle(inPosition.X - 16, inPosition.Y - 7, 32, 15);
		DrawOrigin = new Vector2(16f, 7.5f);
		SetPower(spellDamage);
		_force = 4;
		_life = 1.25f;
		_timeOffset = oscillOffset;
		_damageElement = EDamageElement.Aura;
		SetScaleByDormantTime(dormantTime);
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_maxMoveSpeed = 5000f;
		_maxFallSpeed = 5000f;
		_isFlying = true;
		_frequency = 20f;
		_amplitude = 250f;
		_doesCollideWithSlopes = false;
		_doesDieOnTiles = false;
		_doesDieOutsideOfVisibleArea = inSide != ETeamSide.Enemies;
		ChangeAnimation(0, 4, 0.05f, EAnimationType.Cycle);
		_doesDrawTrail = true;
		_trailFadeRate = 1.25f;
		_normalTrailLength = 2;
		_trailLength = _normalTrailLength;
		_trailShrinkRate = 0.15f;
	}

	private void SetScaleByDormantTime(float dormantTime)
	{
		_scale = 1f;
		if (dormantTime > 0f)
		{
			_scale -= (dormantTime - 0.02f) * 6.5f;
			if (_scale < 0.1f)
			{
				_scale = 1f;
			}
		}
	}

	private void SetPower(int damage)
	{
		_power = (int)Math.Ceiling((float)damage * 0.33f);
	}

	protected override void ApplyBulletMechanics(float delta)
	{
		_timeOffset += _frequency * delta;
		float num = (float)Math.Cos(_timeOffset);
		_velocity.Y = _amplitude * num;
		base.BackPane = num > 0f;
	}

	public override void Kill(bool useAnimation, Point deathPoint, bool deathFromInvulnerable)
	{
		if (useAnimation)
		{
			_level.AddAnimation(EBattleAnimationType.MediumHit, deathPoint, _teamSide, isFacingRight: false);
		}
		Kill();
	}

	internal override void KillOnGround(bool isVerticalCollision, Point contactPoint)
	{
		Kill(useAnimation: true, contactPoint, deathFromInvulnerable: true);
	}

	public override void SilentKill()
	{
		IsFinished = true;
		base.SilentKill();
	}

	public bool Reset(Point startPoint, Vector2 iV, int damage, float oscillation, float sleepTime)
	{
		base.DormantTimer = sleepTime;
		_initialVector = iV;
		_velocity = ((!_isDormant) ? iV : Vector2.Zero);
		Position = startPoint;
		_timeOffset = oscillation;
		SetScaleByDormantTime(sleepTime);
		ClearTrailHistory();
		SnapBboxToPosition();
		SetPower(damage);
		_isFading = false;
		_life = 1.25f;
		_isFading = false;
		_canDamageThings = true;
		_fadeTimer = 0f;
		base.DrawColor = Color.White;
		bool isFinished = IsFinished;
		IsFinished = false;
		if (isFinished)
		{
			base.ID = -1;
		}
		return isFinished;
	}
}
