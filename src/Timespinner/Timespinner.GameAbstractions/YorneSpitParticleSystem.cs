using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class YorneSpitParticleSystem : ParticleSystem
{
	public YorneSpitParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 50f;
		_maxInitialSpeed = 100f;
		_minAcceleration = 0f;
		_maxAcceleration = 50f;
		_minLifetime = 0.3f;
		_maxLifetime = 0.8f;
		_minScale = 0.1f;
		_maxScale = 0.2f;
		_minNumParticles = 3;
		_maxNumParticles = 3;
		_minRotationSpeed = -20f;
		_maxRotationSpeed = 20f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		p.Acceleration.X = 0f;
		p.Acceleration.Y = 350f;
		p.Velocity.Y = 0f;
		if (p.Velocity.X < 0f)
		{
			p.Velocity.X = 0f - p.Velocity.X;
		}
		if (p.Velocity.X < 50f)
		{
			p.Velocity.X = 50f;
		}
	}
}
