using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public sealed class ZelShardDustParticleSystem : ParticleSystem
{
	private const int OffsetX = 32;

	private static readonly Vector4 DustColorA = new Vector4(0.63f, 0.5f, 0.3f, 0.2f);

	private static readonly Vector4 DustColorB = new Vector4(0.25f, 0.2f, 0.125f, 0.2f);

	public ZelShardDustParticleSystem(Texture2D texture, int howManyEffects)
		: base(texture, howManyEffects)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 100f;
		_maxInitialSpeed = 200f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minFriction = 0.2f;
		_maxFriction = 0.1f;
		_minLifetime = 0.2f;
		_maxLifetime = 0.5f;
		_minScale = 0.2f;
		_maxScale = 0.5f;
		_minNumParticles = 2;
		_maxNumParticles = 3;
		_minRotationSpeed = -1f;
		_maxRotationSpeed = 1f;
		_velocityBias.Y *= 0.5f;
		_accelerationBias.Y *= 0.5f;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(0f, 30f);
		num = ((ParticleSystem.Random.Next(2) == 0) ? (180f - num) : num);
		float num2 = MathHelper.ToRadians(num);
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(num2);
		zero.Y = 0f - (float)Math.Sin(num2);
		return zero;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		int x = (int)Math.Round(ParticleSystem.RandomBetween(-32f, 32f));
		base.InitializeParticle(p, where.Add(new Point(x, 0)));
		float amount = ParticleSystem.RandomBetween(0f, 1f);
		p.BaseColor = DustColorA.Lerp(DustColorB, amount);
		p.Acceleration.X += ParticleSystem.RandomBetween(-50f, 50f);
	}
}
