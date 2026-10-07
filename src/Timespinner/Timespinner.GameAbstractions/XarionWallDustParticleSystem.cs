using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class XarionWallDustParticleSystem : ParticleSystem
{
	private const float BaseMinScale = 0.1f;

	private const float BaseMaxScale = 0.5f;

	private readonly float _baseSpeed;

	private readonly float _baseMinSpeed;

	private readonly float _baseMaxSpeed;

	public XarionWallDustParticleSystem(Texture2D inTexture, int howManyEffects, int levelID, int inBaseSpeed)
		: base(inTexture, howManyEffects)
	{
		_baseSpeed = inBaseSpeed;
		_baseMinSpeed = _baseSpeed + 50f;
		_baseMaxSpeed = _baseMinSpeed + 75f;
		BaseColor = DustParticleSystem.GetDustColor(levelID);
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = _baseMinSpeed;
		_maxInitialSpeed = _baseMaxSpeed;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minFriction = 0.2f;
		_maxFriction = 0.1f;
		_minLifetime = 0.2f;
		_maxLifetime = 0.5f;
		_minScale = 0.1f;
		_maxScale = 0.5f;
		_minNumParticles = 20;
		_maxNumParticles = 30;
		_minRotationSpeed = -1f;
		_maxRotationSpeed = 1f;
		_velocityBias.X *= 0.5f;
		_accelerationBias.X *= 0.5f;
	}

	public void AddParticles(Vector2 where, float amount)
	{
		float num = ((amount > 20f) ? 20f : amount);
		_maxScale = 0.5f + num / 30f;
		_minInitialSpeed = 0.1f + num * 10f;
		_maxInitialSpeed = _baseMaxSpeed + num * 10f;
		base.AddParticles(where);
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(0f, 30f);
		num = ((ParticleSystem.Random.Next(2) == 0) ? (180f - num) : num);
		float num2 = MathHelper.ToRadians(num);
		Vector2 zero = Vector2.Zero;
		zero.Y = (float)Math.Cos(num2);
		zero.X = 0f - (float)Math.Sin(num2);
		return zero;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		p.Acceleration.Y += ParticleSystem.RandomBetween(-50f, 50f);
		p.BaseColor = BaseColor;
	}
}
