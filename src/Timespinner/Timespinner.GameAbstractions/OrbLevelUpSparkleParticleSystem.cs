using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public class OrbLevelUpSparkleParticleSystem : AnimatedParticleSystem
{
	private const float MinStartRadius = 12f;

	private const float MaxStartRadius = 16f;

	public OrbLevelUpSparkleParticleSystem(SpriteSheet inSprite, int howManyEffects, Color color)
		: base(inSprite, howManyEffects, 36, 6, 0.065f, EAnimationType.Cycle)
	{
		BaseColor = color.ToVector4();
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 125f;
		_maxInitialSpeed = 150f;
		_minAcceleration = 10f;
		_maxAcceleration = 30f;
		_minFriction = 0.05f;
		_maxFriction = 0.1f;
		_minLifetime = 0.5f;
		_maxLifetime = 1f;
		_minScale = 0.5f;
		_maxScale = 1f;
		_minNumParticles = 5;
		_maxNumParticles = 5;
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
