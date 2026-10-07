using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

internal class TimespinnerAbsorbParticleSystem : GravityWellParticleSystem
{
	private readonly Mobile _lunais;

	public TimespinnerAbsorbParticleSystem(Texture2D inTexture, int howManyEffects, Mobile lunais)
		: base(inTexture, howManyEffects)
	{
		_lunais = lunais;
		base.GrowsWhenNearCenter = false;
		BaseColor = new Vector4(0.8f, 0.8f, 1f, 0.8f);
	}

	protected override void InitializeConstants()
	{
		_systemMass = 1000000f;
		base.DistanceScaleModifier = 0f;
		base.FollowPercentage = 0.85f;
		base.ProximityAgeRate = 30f;
		base.ProximityDistance = 400f;
		base.AbsorbRadius = 1000f;
		base.AbsorbRate = 75f;
		_minInitialSpeed = 75f;
		_maxInitialSpeed = 80f;
		_minAcceleration = 80f;
		_maxAcceleration = 90f;
		_minLifetime = 1f;
		_maxLifetime = 2f;
		_minScale = 0.15f;
		_maxScale = 1f;
		_minNumParticles = 4;
		_maxNumParticles = 6;
		_minRotationSpeed = -10f;
		_maxRotationSpeed = 10f;
		base.MinStartRadius = 16f;
		base.MaxStartRadius = 32f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		_lastWhere = _lunais.Bbox.Center.ToVector2();
		Vector2 vector = PickRandomDirection();
		float num = ParticleSystem.RandomBetween(_minInitialSpeed, _maxInitialSpeed);
		float num2 = ParticleSystem.RandomBetween(_minAcceleration, _maxAcceleration);
		float lifetime = ParticleSystem.RandomBetween(_minLifetime, _maxLifetime);
		float scale = ParticleSystem.RandomBetween(_minScale, _maxScale);
		float rotationSpeed = ParticleSystem.RandomBetween(_minRotationSpeed, _maxRotationSpeed);
		float rotation = ParticleSystem.RandomBetween(0f, (float)Math.PI * 2f);
		float scaleFactor = ParticleSystem.RandomBetween(base.MinStartRadius, base.MaxStartRadius);
		Vector2 value = Vector2.Multiply(vector, scaleFactor);
		p.Initialize(Vector2.Add(where, value), num * new Vector2(vector.Y, 0f - vector.X) * _velocityBias, num2 * vector * _accelerationBias, lifetime, scale, 0f, rotationSpeed, rotation, 0f);
		p.BaseColor = BaseColor;
	}
}
