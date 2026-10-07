using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Timespinner.GameAbstractions;

public class VarndagrothMissileParticleSystem : ProjectileTrailParticleSystem
{
	public VarndagrothMissileParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
		BaseColor = new Vector4(0.5f, 0.5f, 0.5f, 0.1f);
	}

	protected override void InitializeConstants()
	{
		_decelerationRate = 100f;
		_fanAmount = (float)Math.PI / 30f;
		MaxEmissionCounter = 0.2f;
		_minInitialSpeed = 200f;
		_maxInitialSpeed = 300f;
		_minAcceleration = 0f;
		_maxAcceleration = 50f;
		_minLifetime = 0.3f;
		_maxLifetime = 0.8f;
		_minScale = 0.2f;
		_maxScale = 0.5f;
		_minNumParticles = 5;
		_maxNumParticles = 15;
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
