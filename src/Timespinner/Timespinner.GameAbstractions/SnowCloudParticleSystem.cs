using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public class SnowCloudParticleSystem : AnimatedParticleSystem
{
	private static readonly Vector4 GreyDustColor = new Vector4(0.4f, 0.6f, 0.8f, 0.5f);

	public SnowCloudParticleSystem(SpriteSheet inSprite, int howManyEffect)
		: base(inSprite, howManyEffect, 28, 1, 1f, EAnimationType.None)
	{
		BaseColor = GreyDustColor;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 300f;
		_maxInitialSpeed = 500f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minFriction = 0.15f;
		_maxFriction = 0.1f;
		_minLifetime = 0.25f;
		_maxLifetime = 0.5f;
		_minScale = 1f;
		_maxScale = 3f;
		_minNumParticles = 10;
		_maxNumParticles = 10;
		_minRotationSpeed = -1f;
		_maxRotationSpeed = 1f;
		_velocityBias.X = 0.75f;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(0f, (float)Math.PI * 2f);
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(num);
		zero.Y = (float)Math.Sin(num);
		return zero;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		p.BaseColor = BaseColor;
	}
}
