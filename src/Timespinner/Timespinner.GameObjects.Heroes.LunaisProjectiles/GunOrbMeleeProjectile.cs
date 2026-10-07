using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.BaseClasses;

namespace Timespinner.GameObjects.Heroes.LunaisProjectiles;

internal sealed class GunOrbMeleeProjectile : LunaisBaseProjectile
{
	private bool _hasHitSomething;

	public GunOrbMeleeProjectile(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, LunaisOrbAbility parentOrb, int power, SpriteSheet sprite)
		: base(inLevel, inPosition, iV, inSide, -1, parentOrb)
	{
		_sprite = sprite;
		_bbox = new Rectangle(inPosition.X - 2, inPosition.Y - 2, 4, 4);
		_doesDrawBaseSprite = false;
		_power = power;
		_force = 0;
		_life = 0.5f;
		_damageElement = EDamageElement.Sharp;
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
		_doesCollideWithCeilings = false;
		_doesUse16X16TileCollisionBbox = true;
		_timeToFade = 0f;
	}

	public override void Update(float delta)
	{
		base.Update(delta);
		if (_level.IsOutsideVisibleArea(Position))
		{
			SilentKill();
		}
	}

	internal override void KillOnGround(bool isVerticalCollision, Point contactPoint)
	{
		AddHitAnimation(contactPoint);
	}

	protected override void AddImpactAnimation(Alive target, Rectangle collidingRectangle)
	{
		Point impactPoint = FindDeathPoint(target, collidingRectangle);
		AddHitAnimation(impactPoint);
	}

	private void AddHitAnimation(Point impactPoint)
	{
		if (!_hasHitSomething)
		{
			BattleAnimation battleAnimation = new BattleAnimation(_sprite, impactPoint, _level);
			battleAnimation.AnimationStart = 23;
			battleAnimation.AnimationLength = 4;
			battleAnimation.TeamSide = ETeamSide.Heroes;
			battleAnimation.IsFacingLeft = IsFacingLeft;
			battleAnimation.ParticleSystem = new LunaisOrbHitParticleSystem(_level.GCM.TxParticleEnergy, 5);
			BattleAnimation newAnimation = battleAnimation;
			_level.PlayCue(ESFX.LunaisOrbImpactBullet, impactPoint);
			_level.AddAnimation(newAnimation);
			_canDamageThings = false;
			base.CanDamageEnemies = false;
			_hasHitSomething = true;
		}
	}
}
