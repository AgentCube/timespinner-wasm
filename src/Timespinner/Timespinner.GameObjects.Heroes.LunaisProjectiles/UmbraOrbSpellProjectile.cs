using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.LunaisProjectiles;

internal sealed class UmbraOrbSpellProjectile : LunaisBaseOrbDamageArea
{
	private const int FlightHeight = 24;

	private const int FlightWidth = 160;

	private const float TimeForXTravel = 1.2f;

	private const float TimeForYTravel = 0.6f;

	private readonly bool _isFiringLeft;

	private readonly bool _isFirst;

	private readonly Point _emissionStartPoint;

	private float _xOscillTimer;

	private float _yOscillTimer;

	private float _rollingChangeInX;

	private float _rollingChangeInY;

	public UmbraOrbSpellProjectile(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, int spellDamage, bool isFirst, LunaisOrbAbility parentOrb)
		: base(inLevel, inPosition, inSide, -1, null, parentOrb)
	{
		_isFiringLeft = iV.X <= 0f;
		_emissionStartPoint = inPosition;
		_isFirst = isFirst;
		_sprite = _level.GCM.SpOrbMeleeUmbra;
		_bbox = new Rectangle(inPosition.X - 12, inPosition.Y - 12, 24, 24);
		DrawOrigin = new Vector2(12f, 12f);
		ChangeAnimation(19, 4, 0.03f, EAnimationType.Cycle);
		_power = spellDamage;
		_force = 2;
		_life = 4f;
		_damageElement = EDamageElement.Dark;
		base.DamageTimeoutTime = 0.25f;
		base.DoesDieOnImpact = false;
		_doesRotateBasedOnVelocity = false;
		_doesProjectileChangeFacingBasedOnVelocity = false;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_maxMoveSpeed = 5000f;
		_maxFallSpeed = 5000f;
		_isFlying = true;
		_doesDieOutsideOfVisibleArea = false;
		_doesDrawTrail = true;
		_trailFadeRate = 3f;
		_normalTrailLength = 12;
		_trailLength = _normalTrailLength;
		if (_isFirst)
		{
			PlayCue(ESFX.LunaisOrbUmbraSpell);
			_level.PlayCue(ESFX.LunaisOrbUmbraSpell2D);
		}
	}

	protected override void ApplyBulletMechanics(float delta)
	{
		if (_xOscillTimer <= 0f)
		{
			_life = 1.2f;
		}
		_xOscillTimer += delta;
		_yOscillTimer += delta;
		double num = Math.Sin(_xOscillTimer / 1.2f * (float)Math.PI);
		double num2 = Math.Sin(_yOscillTimer / 0.6f * ((float)Math.PI * 2f));
		int num3 = (int)Math.Ceiling(num * 160.0);
		int num4 = (int)Math.Ceiling(num2 * 24.0);
		Point position = new Point(_emissionStartPoint.X + num3 * ((!_isFiringLeft) ? 1 : (-1)), _emissionStartPoint.Y + num4 * ((!_isFirst) ? 1 : (-1)));
		int num5 = Position.X - position.X;
		int num6 = Position.Y - position.Y;
		Position = position;
		float rollingChangeInY = _rollingChangeInY;
		_rollingChangeInX = (_rollingChangeInX + (float)num5) * 0.5f;
		_rollingChangeInY = (_rollingChangeInY + (float)num6) * 0.5f;
		if (_rollingChangeInY > 0f != rollingChangeInY > 0f)
		{
			base.BackPane = !base.BackPane;
		}
		IsFacingLeft = _rollingChangeInX > 0f;
		base.Rotation = MathEx.RotationFromVector2(new Vector2(0f - _rollingChangeInX, 0f - _rollingChangeInY));
		base.ApplyBulletMechanics(delta);
	}

	protected override void DoDamageAnimation(Alive target, Point intersectionCenter)
	{
		_level.AddAnimation(new BattleAnimation(_sprite, intersectionCenter, _level)
		{
			TeamSide = _teamSide,
			AnimationStart = 11,
			AnimationLength = 4
		});
		_level.PlayCue(ESFX.LunaisOrbImpactDark, intersectionCenter);
	}
}
