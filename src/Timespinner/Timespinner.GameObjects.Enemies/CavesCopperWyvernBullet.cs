using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class CavesCopperWyvernBullet : Projectile
{
	private readonly ParticleSystem _bulletTrailParticles;

	public CavesCopperWyvernBullet(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, SpriteSheet sprite, int damage)
		: base(inLevel, inPosition, iV, inSide, -1)
	{
		_sprite = sprite;
		_bbox = new Rectangle(inPosition.X - 4, inPosition.Y - 4, 8, 6);
		_bboxOffset = new Point(0, 1);
		DrawOrigin = new Vector2(4f, 5f);
		_power = damage;
		_force = 0;
		_life = 1f;
		_bulletTrailParticles = new LunaisBullet1ParticleSystem(_level.GCM.TxParticleEnergy, 5);
		_particleSystems.Add(_bulletTrailParticles);
		_isAffectedByGravity = true;
		_gravityAcceleration = 500f;
		_isAffectedByFriction = false;
		_animationSpeed = 0f;
		_maxMoveSpeed = 5000f;
		_maxFallSpeed = 5000f;
		_rotationSpeed = 5f;
		_isFlying = true;
		ChangeAnimation(15, 3, 0.05f, EAnimationType.Cycle);
	}

	protected override void AddImpactAnimation(Alive target, Rectangle collidingRectangle)
	{
		_level.PlayCue(ESFX.EnemyCopperWyvernSpitImpact, Position);
		base.AddImpactAnimation(target, collidingRectangle);
	}

	public override void Kill(bool useAnimation)
	{
		_level.AddAnimation(EBattleAnimationType.SmallHit, _bbox.Center, _teamSide);
		Kill();
	}
}
