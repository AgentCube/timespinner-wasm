using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class FloorSparkleParticleSystem : ParticleSystem
{
	private const int MaxOffsetX = 21;

	public FloorSparkleParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 20f;
		_maxInitialSpeed = 30f;
		_minAcceleration = 8f;
		_maxAcceleration = 10f;
		_minLifetime = 0.6f;
		_maxLifetime = 1.2f;
		_minScale = 0.1f;
		_maxScale = 0.1f;
		_minNumParticles = 1;
		_maxNumParticles = 1;
		_minRotationSpeed = -20f;
		_maxRotationSpeed = 20f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		Vector2 value = new Vector2(ParticleSystem.RandomBetween(-21f, 21f), 0f);
		base.InitializeParticle(p, Vector2.Add(where, value));
		p.BaseColor = BaseColor;
		p.Velocity = new Vector2(0f, 0f - ParticleSystem.RandomBetween(_minInitialSpeed, _maxInitialSpeed));
		p.Acceleration = new Vector2(0f, 0f - ParticleSystem.RandomBetween(_minAcceleration, _maxAcceleration));
	}
}
