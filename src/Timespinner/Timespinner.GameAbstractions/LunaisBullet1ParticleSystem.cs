using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Timespinner.GameAbstractions;

public class LunaisBullet1ParticleSystem : ProjectileTrailParticleSystem
{
	public LunaisBullet1ParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeConstants()
	{
		_decelerationRate = 150f;
		_fanAmount = (float)Math.PI / 15f;
		MaxEmissionCounter = 0.2f;
		_minInitialSpeed = 200f;
		_maxInitialSpeed = 300f;
		_minAcceleration = 0f;
		_maxAcceleration = 50f;
		_minLifetime = 0.3f;
		_maxLifetime = 0.8f;
		_minScale = 0.1f;
		_maxScale = 0.3f;
		_minNumParticles = 2;
		_maxNumParticles = 5;
		_minRotationSpeed = -20f;
		_maxRotationSpeed = 20f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override void InitializeParticle(Particle p, Vector2 where, Vector2 inVelocity)
	{
		p.BaseColor = BaseColor;
		base.InitializeParticle(p, where, inVelocity);
	}
}
