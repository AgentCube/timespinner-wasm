using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public class ZelPagesParticleSystem : AnimatedParticleSystem
{
	private const int Anim_HorizontalStart = 49;

	private const int Anim_VerticalStart = 53;

	private const int Anim_Length = 4;

	public ZelPagesParticleSystem(SpriteSheet inSprite, int howManyEffects, bool isHorizontal)
		: base(inSprite, howManyEffects, isHorizontal ? 49 : 53, 4, 0.1f, isHorizontal ? EAnimationType.Once : EAnimationType.PingPong)
	{
		base.DoParticleUseLifeForAlpha = false;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 150f;
		_maxInitialSpeed = 750f;
		_minAcceleration = 250f;
		_maxAcceleration = 250f;
		_minFriction = 0.1f;
		_maxFriction = 0.1f;
		_minLifetime = 8f;
		_maxLifetime = 8f;
		_minScale = 1f;
		_maxScale = 1f;
		_minNumParticles = 8;
		_maxNumParticles = 8;
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
		p.Acceleration.Y = _maxAcceleration;
	}
}
