using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public abstract class EnergyStreamParticleSystem : ParticleSystem
{
	private bool _isHorizontal;

	private Point _lastTetherBase;

	private Vector2 _lastVelocity;

	protected float _minHeightStart;

	protected float _maxHeightStart;

	protected float _minDistanceStart;

	protected float _maxDistanceStart;

	protected float _minBrightness;

	protected float _maxBrightness = 1f;

	public float FollowPercentage { get; set; }

	protected EnergyStreamParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeParticle(Particle p, Vector2 where, Vector2 inVelocity)
	{
		_lastVelocity = inVelocity;
		_isHorizontal = Math.Abs(_lastVelocity.X) > 0.1f;
		float num = ParticleSystem.RandomBetween(_minInitialSpeed, _maxInitialSpeed);
		float num2 = ParticleSystem.RandomBetween(_minAcceleration, _maxAcceleration);
		float lifetime = ParticleSystem.RandomBetween(_minLifetime, _maxLifetime);
		float scale = ParticleSystem.RandomBetween(_minScale, _maxScale);
		float rotationSpeed = ParticleSystem.RandomBetween(_minRotationSpeed, _maxRotationSpeed);
		float rotation = ParticleSystem.RandomBetween(0f, (float)Math.PI * 2f);
		float num3 = ParticleSystem.RandomBetween(_minHeightStart, _maxHeightStart);
		float num4 = ParticleSystem.RandomBetween(_minDistanceStart, _maxDistanceStart);
		float brightness = ParticleSystem.RandomBetween(_minBrightness, _maxBrightness);
		p.Initialize(new Vector2(where.X + num4 * _lastVelocity.X + num3 * _lastVelocity.Y, where.Y + num3), num * _lastVelocity * _velocityBias, num2 * _lastVelocity * _accelerationBias, lifetime, scale, 0f, rotationSpeed, rotation, 0f);
		p.BaseColor = BaseColor;
		p.Brightness = brightness;
	}

	public override void Update(float delta, Point tetherBase)
	{
		Vector2 value = ((_lastTetherBase != Point.Zero) ? new Vector2(_lastTetherBase.X - tetherBase.X, _lastTetherBase.Y - tetherBase.Y) : Vector2.Zero);
		Particle[] particles = _particles;
		foreach (Particle particle in particles)
		{
			if (particle.IsActive)
			{
				if (FollowPercentage != 0f)
				{
					particle.Position = Vector2.Subtract(particle.Position, Vector2.Multiply(value, FollowPercentage));
				}
				particle.Update(delta);
				if (!particle.IsActive)
				{
					_freeParticles.Enqueue(particle);
				}
			}
		}
		_lastTetherBase = tetherBase;
	}

	public override void Draw(SpriteBatch spriteBatch, Vector2 screenCenter, Vector2 cameraPosition, float cameraZoom)
	{
		spriteBatch.End();
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null);
		Particle[] particles = _particles;
		foreach (Particle particle in particles)
		{
			if (particle.IsActive)
			{
				float num = particle.TimeSinceStart / particle.Lifetime;
				float num2 = 2f * num * (1f - num);
				Vector4 baseColor = particle.BaseColor;
				baseColor.W = particle.Brightness;
				Color color = new Color(baseColor) * num2;
				Vector2 value = Vector2.Subtract(cameraPosition, new Vector2(particle.Position.X, particle.Position.Y));
				value = Vector2.Subtract(screenCenter, Vector2.Multiply(value, cameraZoom));
				Rectangle destinationRectangle = new Rectangle((int)value.X, (int)value.Y, (int)(particle.Scale * (_isHorizontal ? (20f + particle.RotationSpeed) : 1f)), (int)(particle.Scale * (_isHorizontal ? 1f : (20f + particle.RotationSpeed))));
				spriteBatch.Draw(_texture, destinationRectangle, null, color, 0f, _origin, SpriteEffects.None, 0f);
			}
		}
		spriteBatch.End();
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null);
	}
}
