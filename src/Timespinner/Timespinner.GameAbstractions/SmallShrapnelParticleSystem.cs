using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public class SmallShrapnelParticleSystem : AnimatedParticleSystem
{
	public SmallShrapnelParticleSystem(SpriteSheet sprite, int howManyEffects)
		: base(sprite, howManyEffects, 0, 3, 1f, EAnimationType.None)
	{
		_isStartIndexRandom = true;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 200f;
		_maxInitialSpeed = 300f;
		_minAcceleration = 800f;
		_maxAcceleration = 900f;
		_minLifetime = 2f;
		_maxLifetime = 3f;
		_minScale = 0.3f;
		_maxScale = 1f;
		_minNumParticles = 5;
		_maxNumParticles = 10;
		_minRotationSpeed = -20f;
		_maxRotationSpeed = 20f;
		_spriteBlendMode = BlendState.AlphaBlend;
		_velocityBias.X *= 0.5f;
		_accelerationBias.X *= 0f;
		_accelerationBias.Y *= -1f;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(MathHelper.ToRadians(45f), MathHelper.ToRadians(135f));
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
