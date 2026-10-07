using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class LunaisChannelParticleSystem : GravityWellParticleSystem
{
	protected int _spinningSpokesCount = 9;

	protected float _spinningPosition;

	protected float _spinningRate = 5f;

	public LunaisChannelParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeConstants()
	{
		_systemMass = 1000000f;
		base.DistanceScaleModifier = 85f;
		base.FollowPercentage = 0.85f;
		base.ProximityAgeRate = 30f;
		base.ProximityDistance = 400f;
		base.AbsorbRadius = 1000f;
		base.AbsorbRate = 75f;
		_minInitialSpeed = 75f;
		_maxInitialSpeed = 80f;
		_minAcceleration = 80f;
		_maxAcceleration = 90f;
		_minLifetime = 1.5f;
		_maxLifetime = 1.8f;
		_minScale = 0.1f;
		_maxScale = 1.5f;
		_minNumParticles = 4;
		_maxNumParticles = 6;
		_minRotationSpeed = -10f;
		_maxRotationSpeed = 10f;
		base.MinStartRadius = 50f;
		base.MaxStartRadius = 55f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override Vector2 PickRandomDirection()
	{
		float spokeRadian = GetSpokeRadian(ParticleSystem.Random.Next(0, _spinningSpokesCount));
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(spokeRadian);
		zero.Y = 0f - (float)Math.Sin(spokeRadian);
		return zero;
	}

	private float GetSpokeRadian(int spoke)
	{
		return _spinningPosition + (float)spoke * ((float)Math.PI * 2f / (float)_spinningSpokesCount);
	}

	public override void Update(float delta, Point tetherBase)
	{
		base.Update(delta, tetherBase);
		if (_spinningRate != 0f)
		{
			_spinningPosition += _spinningRate * delta;
			if (_spinningPosition > (float)Math.PI * 2f)
			{
				_spinningPosition -= (float)Math.PI * 2f;
			}
		}
	}
}
