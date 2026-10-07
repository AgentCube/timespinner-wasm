using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public class RavenBossRavenParticleSystem : AnimatedParticleSystem
{
	public RavenBossRavenParticleSystem(SpriteSheet inSprite, int howManyEffects, int start, int length)
		: base(inSprite, howManyEffects, start, length, 0.1f, EAnimationType.Cycle)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 100f;
		_maxInitialSpeed = 150f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minFriction = 0.03f;
		_maxFriction = 0.04f;
		_minLifetime = 0.5f;
		_maxLifetime = 1f;
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
