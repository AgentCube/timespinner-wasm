using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class OrbPedestalLeakParticleSystem : ParticleSystem
{
	private const float MinStartRadius = 4f;

	private const float MaxStartRadius = 8f;

	public OrbPedestalLeakParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 5f;
		_maxInitialSpeed = 10f;
		_minAcceleration = 1f;
		_maxAcceleration = 3f;
		_minLifetime = 0.5f;
		_maxLifetime = 1.2f;
		_minScale = 0.1f;
		_maxScale = 0.15f;
		_minNumParticles = 1;
		_maxNumParticles = 1;
		_minRotationSpeed = -20f;
		_maxRotationSpeed = 20f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		Vector2 value = PickRandomDirection();
		float scaleFactor = ParticleSystem.RandomBetween(4f, 8f);
		Vector2 value2 = Vector2.Multiply(value, scaleFactor);
		base.InitializeParticle(p, Vector2.Add(where, value2));
		p.BaseColor = BaseColor;
	}
}
