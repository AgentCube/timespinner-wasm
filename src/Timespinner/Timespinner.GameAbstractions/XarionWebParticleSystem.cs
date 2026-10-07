using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public class XarionWebParticleSystem : AnimatedParticleSystem
{
	private readonly int _webStart;

	private readonly int _possibleFrames;

	public XarionWebParticleSystem(SpriteSheet inSprite, int howManyEffects, int start, int length)
		: base(inSprite, howManyEffects, start, 1, 0.1f, EAnimationType.PingPong)
	{
		_possibleFrames = length;
		_webStart = start;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 250f;
		_maxInitialSpeed = 350f;
		_minAcceleration = 100f;
		_maxAcceleration = 125f;
		_minFriction = 0.15f;
		_maxFriction = 0.2f;
		_minLifetime = 2f;
		_maxLifetime = 3f;
		_minScale = 0.5f;
		_maxScale = 1f;
		_minNumParticles = 2;
		_maxNumParticles = 5;
		_minRotationSpeed = -2.5f;
		_maxRotationSpeed = 2.5f;
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
		if (p is AnimatedParticle animatedParticle)
		{
			animatedParticle.AnimationStart = _webStart + (int)Math.Round(ParticleSystem.RandomBetween(0f, _possibleFrames));
		}
		if (p.Velocity.Y < 0f)
		{
			p.Velocity.Y = 0f - p.Velocity.Y;
		}
		p.Acceleration.X += ParticleSystem.RandomBetween(-50f, 50f);
	}
}
