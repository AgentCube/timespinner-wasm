using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public class RavenBossFeathersParticleSystem : AnimatedParticleSystem
{
	public RavenBossFeathersParticleSystem(SpriteSheet inSprite, int howManyEffects, int start, int length)
		: base(inSprite, howManyEffects, start, length, 0.1f, EAnimationType.PingPong)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 100f;
		_maxInitialSpeed = 300f;
		_minAcceleration = 200f;
		_maxAcceleration = 250f;
		_minFriction = 0.05f;
		_maxFriction = 0.1f;
		_minLifetime = 1.5f;
		_maxLifetime = 3f;
		_minScale = 0.5f;
		_maxScale = 1f;
		_minNumParticles = 4;
		_maxNumParticles = 7;
		_minRotationSpeed = -10f;
		_maxRotationSpeed = 10f;
		_spriteBlendMode = BlendState.AlphaBlend;
		_velocityBias.X *= 0.5f;
		_accelerationBias.X *= 0f;
		_accelerationBias.Y *= -1f;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(MathHelper.ToRadians(15f), MathHelper.ToRadians(165f));
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(num);
		zero.Y = 0f - (float)Math.Sin(num);
		return zero;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		p.Acceleration.X += ParticleSystem.RandomBetween(-50f, 50f);
	}
}
