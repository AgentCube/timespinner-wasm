using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class OrbLevelUpParticleSystem : ParticleSystem
{
	private const float MinStartRadius = 12f;

	private const float MaxStartRadius = 16f;

	public OrbLevelUpParticleSystem(Texture2D inTexture, int howManyEffects, Color color)
		: base(inTexture, howManyEffects)
	{
		BaseColor = color.ToVector4();
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 150f;
		_maxInitialSpeed = 200f;
		_minAcceleration = 10f;
		_maxAcceleration = 30f;
		_minFriction = 0.05f;
		_maxFriction = 0.1f;
		_minLifetime = 0.5f;
		_maxLifetime = 1f;
		_minScale = 0.1f;
		_maxScale = 0.15f;
		_minNumParticles = 15;
		_maxNumParticles = 15;
		_minRotationSpeed = -20f;
		_maxRotationSpeed = 20f;
		_spriteBlendMode = BlendState.AlphaBlend;
		_minAngle = -(float)Math.PI;
		_maxAngle = 0f;
		_velocityBias.X = 0.66f;
		_velocityBias.Y = 1f;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		Vector2 value = new Vector2(ParticleSystem.RandomBetween(-1f, 1f), ParticleSystem.RandomBetween(-1f, 1f));
		float scaleFactor = ParticleSystem.RandomBetween(12f, 16f);
		Vector2 value2 = Vector2.Multiply(value, scaleFactor);
		base.InitializeParticle(p, Vector2.Add(where, value2));
		p.BaseColor = BaseColor;
	}
}
