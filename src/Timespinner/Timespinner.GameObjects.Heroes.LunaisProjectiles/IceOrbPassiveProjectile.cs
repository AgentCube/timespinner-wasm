using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.BaseClasses;

namespace Timespinner.GameObjects.Heroes.LunaisProjectiles;

internal sealed class IceOrbPassiveProjectile : LunaisBaseProjectile
{
	private readonly ParticleSystem _trailParticles;

	public IceOrbPassiveProjectile(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, LunaisOrbAbility parentOrb, int power)
		: base(inLevel, inPosition, iV, inSide, -1, parentOrb)
	{
		_sprite = _level.GCM.SpOrbMeleeIce;
		_bbox = new Rectangle(inPosition.X - 7, inPosition.Y - 4, 14, 8);
		DrawOrigin = new Vector2(7f, 4f);
		_power = power;
		_force = 0;
		_life = 0.5f;
		_damageElement = EDamageElement.Ice;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_animationSpeed = 0f;
		_maxMoveSpeed = 5000f;
		_maxFallSpeed = 5000f;
		_isFlying = true;
		_doesDieOutsideOfVisibleArea = true;
		base.DoesDieOnImpact = true;
		base.DoesCollideWithTiles = true;
		_doesDieOnTiles = true;
		_doesCollideWithWalls = true;
		_doesCollideWithSlopes = true;
		_doesCollideWithFloors = true;
		_doesCollideWithCeilings = true;
		_trailParticles = new LunaisBullet1ParticleSystem(_level.GCM.TxParticleEnergy, 5)
		{
			BaseColor = BattleAnimation.GetElementColorVector(EElementAnimationColor.Blue)
		};
		_particleSystems.Add(_trailParticles);
		_doesDrawTrail = true;
		_trailFadeRate = 1.35f;
		_normalTrailLength = 2;
		_trailLength = _normalTrailLength;
		_trailShrinkRate = 0.2f;
		ChangeAnimation(17);
	}

	protected override void AddImpactAnimation(Alive target, Rectangle collidingRectangle)
	{
	}

	internal override void KillOnGround(bool isVerticalCollision, Point contactPoint)
	{
		AddIceDustAnimation(contactPoint, isEnemy: false);
		base.KillOnGround(isVerticalCollision, contactPoint);
	}

	public override void Kill(bool useAnimation, Point deathPoint, bool deathFromInvulnerable)
	{
		if (useAnimation)
		{
			AddIceDustAnimation(deathPoint, isEnemy: true);
		}
		Kill();
	}

	private void AddIceDustAnimation(Point deathPoint, bool isEnemy)
	{
		_level.AddAnimation(new BattleAnimation(_sprite, deathPoint, _level)
		{
			AnimationStart = 12,
			AnimationLength = 4
		});
		_level.PlayCue(isEnemy ? ESFX.LunaisOrbImpactIceTinyEnemy : ESFX.LunaisOrbImpactIceTinyOther, deathPoint);
	}
}
