using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public class EggShellParticleSystem : AnimatedParticleSystem
{
	public EggShellParticleSystem(SpriteSheet sprite, int howManyEffects)
		: base(sprite, howManyEffects, 10, 4, 0.05f, EAnimationType.Cycle)
	{
		_isStartIndexRandom = true;
		base.DoParticleUseLifeForAlpha = false;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 200f;
		_maxInitialSpeed = 300f;
		_minAcceleration = 800f;
		_maxAcceleration = 900f;
		_minLifetime = 2f;
		_maxLifetime = 3f;
		_minScale = 1f;
		_maxScale = 1f;
		_minNumParticles = 10;
		_maxNumParticles = 15;
		_minRotationSpeed = 0f;
		_maxRotationSpeed = 0f;
		_minAngle = 0f;
		_maxAngle = 0f;
		_spriteBlendMode = BlendState.AlphaBlend;
		_velocityBias.X *= 0.5f;
		_accelerationBias.X *= 0f;
		_accelerationBias.Y *= -1f;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(MathHelper.ToRadians(70f), MathHelper.ToRadians(220f));
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(num);
		zero.Y = 0f - (float)Math.Sin(num);
		return zero;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		p.Acceleration.X += ParticleSystem.RandomBetween(-50f, 50f);
		if (p.Acceleration.Y < 0f)
		{
			p.Acceleration.Y = 0f - p.Acceleration.Y;
		}
	}
}
