using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public class NethershadeSmokeParticleSystem : AnimatedParticleSystem
{
	private const int Anim_Smoke = 0;

	public NethershadeSmokeParticleSystem(SpriteSheet sprite, int howManyEffects)
		: base(sprite, howManyEffects, 0, 1, 1f, EAnimationType.None)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 15f;
		_maxInitialSpeed = 40f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minFriction = 0.01f;
		_maxFriction = 0.015f;
		_minLifetime = 1f;
		_maxLifetime = 2f;
		_minScale = 0.5f;
		_maxScale = 1f;
		_minNumParticles = 1;
		_maxNumParticles = 1;
		_velocityBias = new Vector2(0.5f, 1f);
		_minRotationSpeed = -(float)Math.PI / 4f;
		_maxRotationSpeed = (float)Math.PI / 4f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(0f, (float)Math.PI * 2f);
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(num);
		zero.Y = 0f - (float)Math.Sin(num);
		return zero;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		Vector4 start = new Vector4(0f, 0.25f, 0.25f, 0.1f);
		Vector4 end = new Vector4(0.25f, 0f, 0.25f, 0.1f);
		base.InitializeParticle(p, where);
		if (p.Velocity.Y < 0f)
		{
			p.Velocity.Y = 0f - p.Velocity.Y;
		}
		float amount = (float)ParticleSystem.Random.NextDouble();
		Vector4 baseColor = start.Lerp(end, amount);
		p.BaseColor = baseColor;
	}
}
