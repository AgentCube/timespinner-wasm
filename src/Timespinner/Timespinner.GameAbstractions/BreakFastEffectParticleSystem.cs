using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public sealed class BreakFastEffectParticleSystem : ParticleSystem
{
	public BreakFastEffectParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 1f;
		_maxInitialSpeed = 3f;
		_minAcceleration = 25f;
		_maxAcceleration = 50f;
		_minLifetime = 0.1f;
		_maxLifetime = 0.25f;
		_minScale = 0.05f;
		_maxScale = 0.1f;
		_minNumParticles = 1;
		_maxNumParticles = 1;
		_minRotationSpeed = -20f;
		_maxRotationSpeed = 20f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		float num = ParticleSystem.RandomBetween(0.1f, 0.6f);
		p.BaseColor = new Vector4(num * 0.98f, num * 1f, num * 0.95f, 1f);
		if (p.Velocity.Y > 0f)
		{
			p.Velocity.Y = 0f - p.Velocity.Y;
		}
		p.Acceleration.X = 0f - p.Velocity.X;
		p.Acceleration.Y = 0f - _maxAcceleration;
	}
}
