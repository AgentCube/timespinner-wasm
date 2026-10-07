using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class BirdBossWallPebblesParticleSystem : ParticleSystem
{
	internal bool IsFacingRight { get; set; }

	public BirdBossWallPebblesParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 25f;
		_maxInitialSpeed = 100f;
		_minAcceleration = 900f;
		_maxAcceleration = 1000f;
		_minLifetime = 0.25f;
		_maxLifetime = 0.5f;
		_minScale = 0.075f;
		_maxScale = 0.15f;
		_minNumParticles = 5;
		_maxNumParticles = 8;
		_minRotationSpeed = 0f;
		_maxRotationSpeed = 0f;
		_velocityBias.Y *= 0.5f;
		_accelerationBias.X *= 0f;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween((float)Math.PI / 2f, 4.712389f);
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(num);
		zero.Y = 0f - (float)Math.Sin(num);
		return zero;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		p.Acceleration.X += ParticleSystem.RandomBetween(0f, 50f);
		if (IsFacingRight)
		{
			p.Velocity.X = 0f - p.Velocity.X;
			p.Acceleration.X = 0f - p.Acceleration.X;
		}
		if (p.Acceleration.Y < 0f)
		{
			p.Acceleration.Y = 0f - p.Acceleration.Y;
		}
		p.BaseColor = BaseColor;
	}
}
