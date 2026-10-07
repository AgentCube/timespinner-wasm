using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public abstract class InverseLazerParticleSystem : ParticleSystem
{
	protected float _systemMass = 1f;

	protected Vector2 _lastWhere;

	private Point _lastTetherBase;

	public float FollowPercentage { get; set; }

	public float MinStartRadius { get; set; }

	public float MaxStartRadius { get; set; }

	public float StartRadiusOffset { get; set; }

	internal float MinWidth { get; set; }

	internal float MaxWidth { get; set; }

	protected InverseLazerParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	internal void ResetLastTetherBase(Point newTetherBase)
	{
		_lastTetherBase = newTetherBase;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		_lastWhere = where;
		float num = ParticleSystem.RandomBetween(_minAngle, _maxAngle);
		Vector2 vector = new Vector2((float)Math.Cos(num), (float)Math.Sin(num));
		float num2 = ParticleSystem.RandomBetween(_minAcceleration, _maxAcceleration);
		float lifetime = ParticleSystem.RandomBetween(_minLifetime, _maxLifetime);
		float scaleFactor = ParticleSystem.RandomBetween(MinStartRadius, MaxStartRadius) + StartRadiusOffset;
		Vector2 value = Vector2.Multiply(vector, scaleFactor);
		float rotationSpeed = ParticleSystem.RandomBetween(MinWidth, MaxWidth);
		float rotation = num + (float)Math.PI;
		Vector2 velocity = Vector2.Multiply(vector, ParticleSystem.RandomBetween(_minInitialSpeed, _maxInitialSpeed));
		p.Initialize(Vector2.Add(where, value), velocity, num2 * vector * _accelerationBias, lifetime, 0f, 0f, rotationSpeed, rotation, 0f);
		p.BaseColor = BaseColor;
	}

	public override void Update(float delta, Point tetherBase)
	{
		Vector2 value = new Vector2(_lastTetherBase.X - tetherBase.X, _lastTetherBase.Y - tetherBase.Y);
		Particle[] particles = _particles;
		foreach (Particle particle in particles)
		{
			if (particle.IsActive)
			{
				if (FollowPercentage > 0f)
				{
					particle.Position = Vector2.Subtract(particle.Position, Vector2.Multiply(value, FollowPercentage));
				}
				float num = particle.TimeSinceStart / particle.Lifetime;
				if (num < 0.5f)
				{
					particle.Scale = num * 2f * particle.RotationSpeed;
				}
				else
				{
					float num2 = (1f - num) * 2f;
					particle.Scale = num2 * particle.RotationSpeed;
					particle.Position = Vector2.Subtract(particle.Position, particle.Velocity * (delta * 4f));
				}
				particle.TimeSinceStart += delta;
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
				Vector4 baseColor = particle.BaseColor;
				baseColor.W = particle.Brightness;
				Color color = new Color(baseColor);
				Vector2 value = Vector2.Subtract(cameraPosition, new Vector2(particle.Position.X, particle.Position.Y));
				value = Vector2.Subtract(screenCenter, Vector2.Multiply(value, cameraZoom));
				spriteBatch.Draw(destinationRectangle: new Rectangle((int)value.X, (int)value.Y, (int)particle.Scale, 1), texture: _texture, sourceRectangle: null, color: color, rotation: particle.Rotation, origin: _origin, effects: SpriteEffects.None, layerDepth: 0f);
			}
		}
		spriteBatch.End();
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null);
	}
}
