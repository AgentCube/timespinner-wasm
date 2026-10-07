using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class LakeSnowParticleSystem : ParticleSystem
{
	private const float DirectionChangeLifeThreshold = 3f;

	private readonly bool _isNear;

	public LakeSnowParticleSystem(Texture2D inTexture, int howManyEffects, bool isNear)
		: base(inTexture, howManyEffects)
	{
		_isNear = isNear;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 10f;
		_maxInitialSpeed = 30f;
		_minAcceleration = 17f;
		_maxAcceleration = 17f;
		_minLifetime = 10f;
		_maxLifetime = 12f;
		if (!_isNear)
		{
			_minLifetime *= 0.5f;
			_maxLifetime *= 0.5f;
		}
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
		base.InitializeParticle(p, where);
		p.BaseColor = BaseColor;
		if (p.Velocity.Y < 0f)
		{
			p.Velocity.Y = 0f - p.Velocity.Y;
		}
		p.Acceleration.X = 0f - p.Velocity.X;
		p.Acceleration.Y = _maxAcceleration;
	}

	public override void Update(float delta, Point tetherBase)
	{
		Particle[] particles = _particles;
		foreach (Particle particle in particles)
		{
			if (particle.IsActive)
			{
				float timeSinceStart = particle.TimeSinceStart;
				particle.Update(delta);
				if (particle.TimeSinceStart >= 3f && timeSinceStart < 3f)
				{
					particle.Acceleration = new Vector2(0f - particle.Acceleration.X, particle.Acceleration.Y);
				}
				if (!particle.IsActive)
				{
					_freeParticles.Enqueue(particle);
				}
			}
		}
	}

	public override void Draw(SpriteBatch spriteBatch, Vector2 screenCenter, Vector2 cameraPosition, float cameraZoom)
	{
		if (!_isNear)
		{
			cameraPosition *= 0.75f;
		}
		base.Draw(spriteBatch, screenCenter, cameraPosition, cameraZoom);
	}
}
