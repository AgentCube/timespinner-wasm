using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public class RavenBossDeathRavenParticleSystem : AnimatedParticleSystem
{
	public RavenBossDeathRavenParticleSystem(SpriteSheet inSprite, int howManyEffects, int start, int length)
		: base(inSprite, howManyEffects, start, length, 0.15f, EAnimationType.Cycle)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 250f;
		_maxInitialSpeed = 350f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minFriction = 0f;
		_maxFriction = 0f;
		_minLifetime = 1.5f;
		_maxLifetime = 2.5f;
		_minScale = 0.9f;
		_maxScale = 1.1f;
		_minNumParticles = 1;
		_maxNumParticles = 1;
		_minAngle = 0f;
		_maxAngle = 0f;
		_minRotationSpeed = 0f;
		_maxRotationSpeed = 0f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(0f, (float)Math.PI * 2f);
		return new Vector2((float)Math.Cos(num), (float)Math.Sin(num));
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		if (p is AnimatedParticle animatedParticle)
		{
			animatedParticle.IsFacingLeft = p.Velocity.X > 0f;
		}
	}
}
