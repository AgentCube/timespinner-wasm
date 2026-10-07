using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class TransitionWarpLeakParticleSystem : ParticleSystem
{
	private const float MinStartRadius = 8f;

	private const float MaxStartRadius = 32f;

	public TransitionWarpLeakParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 10f;
		_maxInitialSpeed = 25f;
		_minAcceleration = 1f;
		_maxAcceleration = 3f;
		_minLifetime = 0.5f;
		_maxLifetime = 0.75f;
		_minScale = 0.1f;
		_maxScale = 0.15f;
		_minNumParticles = 1;
		_maxNumParticles = 2;
		_minRotationSpeed = -20f;
		_maxRotationSpeed = 20f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		Vector2 value = PickRandomDirection();
		float scaleFactor = ParticleSystem.RandomBetween(8f, 32f);
		Vector2 value2 = Vector2.Multiply(value, scaleFactor);
		base.InitializeParticle(p, Vector2.Add(where, value2));
		p.BaseColor = BaseColor;
	}
}
