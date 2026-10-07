using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class FortressKnightShieldBeam : Projectile
{
	internal const int Width = 12;

	internal const int Height = 31;

	private readonly Color _sparkleColor;

	private readonly LunaisChargeLeakParticleSystem _sparkleParticleSystem;

	public FortressKnightShieldBeam(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, SpriteSheet sprite, int baseDamage)
		: base(inLevel, inPosition, iV, inSide, -1)
	{
		_sprite = sprite;
		_bbox = new Rectangle(inPosition.X - 6, inPosition.Y - 6, 12, 31);
		DrawOrigin = new Vector2(6f, 15.5f);
		_power = (int)Math.Ceiling((float)baseDamage * 1.2f);
		_force = 0;
		_life = 1f;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_maxMoveSpeed = 5000f;
		_maxFallSpeed = 5000f;
		_isFlying = true;
		_doesRotateBasedOnVelocity = false;
		_doesDieOnTiles = true;
		_doesCollideWithSlopes = true;
		_doesCollideWithFloors = true;
		_doesCollideWithCeilings = true;
		_doesCollideWithWalls = true;
		base.DoesCollideWithTiles = true;
		base.DoesKnockBack = true;
		base.DoesSurviveImpactIfNoDamageDealt = true;
		ChangeAnimation(21, 1, 0.066f, EAnimationType.Once);
		base.DrawColor = Color.White * 0.75f;
		_sparkleColor = new Color(0.75f, 1f, 0.85f, 0.75f);
		base.AuraColor = _sparkleColor;
		base.DoesDrawAura = true;
		base.AuraCount = 4f;
		base.AuraSize = 0.1f;
		_damageElement = EDamageElement.None;
		_doesDrawTrail = true;
		_trailLength = 8;
		_trailFadeRate = 4f;
		_trailColor = _sparkleColor;
		_isTrailLengthAffectedByTime = false;
		_sparkleParticleSystem = new LunaisChargeLeakParticleSystem(_level.GCM.TxParticleEnergy, 8)
		{
			BaseColor = _sparkleColor.ToVector4()
		};
		_particleSystems.Add(_sparkleParticleSystem);
		_doesAutomaticallyEmitParticles = true;
	}

	public override void Kill(bool useAnimation)
	{
		_level.AddAnimation(EBattleAnimationType.SmallHit, _bbox.Center, _teamSide);
		Kill();
	}
}
