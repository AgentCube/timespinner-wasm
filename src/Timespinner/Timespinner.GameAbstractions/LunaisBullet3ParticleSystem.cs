using System;
using Microsoft.Xna.Framework.Graphics;

namespace Timespinner.GameAbstractions;

public class LunaisBullet3ParticleSystem : ProjectileTrailParticleSystem
{
	public LunaisBullet3ParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeConstants()
	{
		_decelerationRate = 350f;
		_fanAmount = (float)Math.PI / 4f;
		MaxEmissionCounter = 0.15f;
		_accelerationBias.Y = 0.5f;
		_minInitialSpeed = 200f;
		_maxInitialSpeed = 300f;
		_minAcceleration = 0f;
		_maxAcceleration = 50f;
		_minLifetime = 0.4f;
		_maxLifetime = 0.9f;
		_minScale = 0.2f;
		_maxScale = 0.45f;
		_minNumParticles = 2;
		_maxNumParticles = 5;
		_minRotationSpeed = -20f;
		_maxRotationSpeed = 20f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}
}
