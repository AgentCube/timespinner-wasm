using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class FruitSparkleParticleSystem : ParticleSystem
{
	private const int MaxOffsetX = 2;

	private const int MaxOffsetY = 2;

	public FruitSparkleParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 20f;
		_maxInitialSpeed = 30f;
		_minAcceleration = 2f;
		_maxAcceleration = 5f;
		_minLifetime = 0.4f;
		_maxLifetime = 1f;
		_minScale = 0.1f;
		_maxScale = 0.1f;
		_minNumParticles = 1;
		_maxNumParticles = 1;
		_minRotationSpeed = -20f;
		_maxRotationSpeed = 20f;
		_spriteBlendMode = BlendState.AlphaBlend;
		_velocityBias.X = 0f;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		Vector2 value = new Vector2(ParticleSystem.RandomBetween(-2f, 2f), ParticleSystem.RandomBetween(-2f, 0f));
		base.InitializeParticle(p, Vector2.Add(where, value));
		p.BaseColor = BaseColor;
		if (p.Velocity.Y > 0f)
		{
			p.Velocity.Y = 0f - p.Velocity.Y;
		}
	}
}
