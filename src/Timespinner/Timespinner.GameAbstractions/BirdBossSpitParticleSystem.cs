using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Timespinner.GameAbstractions;

public class BirdBossSpitParticleSystem : ProjectileTrailParticleSystem
{
	public BirdBossSpitParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
		BaseColor = Color.Beige.ToVector4();
	}

	protected override void InitializeConstants()
	{
		_decelerationRate = 500f;
		_fanAmount = (float)Math.PI / 15f;
		MaxEmissionCounter = 0.2f;
		_minInitialSpeed = 200f;
		_maxInitialSpeed = 300f;
		_minAcceleration = 0f;
		_maxAcceleration = 50f;
		_minLifetime = 1f;
		_maxLifetime = 1f;
		_minScale = 0.2f;
		_maxScale = 0.3f;
		_minNumParticles = 1;
		_maxNumParticles = 3;
		_minRotationSpeed = -20f;
		_maxRotationSpeed = 20f;
		_spriteBlendMode = BlendState.AlphaBlend;
		_accelerationBias = new Vector2(0f, 1f);
	}

	protected override void InitializeParticle(Particle p, Vector2 where, Vector2 inVelocity)
	{
		p.BaseColor = BaseColor;
		base.InitializeParticle(p, where, inVelocity);
		p.Acceleration = new Vector2(0f - p.Velocity.X, 500f);
	}
}
