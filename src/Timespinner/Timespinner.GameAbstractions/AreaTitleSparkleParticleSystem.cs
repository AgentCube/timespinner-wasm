using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core.Constants;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class AreaTitleSparkleParticleSystem : ParticleSystem
{
	public AreaTitleSparkleParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
		BaseColor = new Vector4(0.9f, 0.9f, 0.8f, 1f);
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 30f;
		_maxInitialSpeed = 40f;
		_minAcceleration = 2f;
		_maxAcceleration = 5f;
		_minLifetime = 1f;
		_maxLifetime = 1.5f;
		_minScale = 0.1f;
		_maxScale = 0.15f;
		_minNumParticles = 1;
		_maxNumParticles = 2;
		_minRotationSpeed = -20f;
		_maxRotationSpeed = 20f;
		_minFriction = 0.025f;
		_maxFriction = 0.05f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override void InitializeParticle(Particle p, Vector2 where, Vector2 velocity)
	{
		base.InitializeParticle(p, where);
		int inGameZoom = Constants.InGameZoom;
		float scaleFactor = ParticleSystem.RandomBetween(_minInitialSpeed, _maxInitialSpeed) * (float)inGameZoom;
		p.Velocity = Vector2.Multiply(velocity, scaleFactor);
		p.Acceleration *= (float)inGameZoom;
		p.Scale *= inGameZoom;
		p.BaseColor = BaseColor;
	}
}
