using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class DustCrackingParticleSystem : ParticleSystem
{
	public DustCrackingParticleSystem(Texture2D inTexture, int howManyEffects, int levelID)
		: base(inTexture, howManyEffects)
	{
		BaseColor = DustParticleSystem.GetDustColor(levelID) * 1.5f;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 10f;
		_maxInitialSpeed = 100f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minFriction = 0.1f;
		_maxFriction = 0.15f;
		_minLifetime = 0.2f;
		_maxLifetime = 0.5f;
		_minScale = 0.1f;
		_maxScale = 0.5f;
		_minNumParticles = 20;
		_maxNumParticles = 30;
		_minRotationSpeed = -1f;
		_maxRotationSpeed = 1f;
	}

	protected override Vector2 PickRandomDirection()
	{
		float degrees = ParticleSystem.RandomBetween(0f, 360f);
		float num = MathHelper.ToRadians(degrees);
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(num);
		zero.Y = 0f - (float)Math.Sin(num);
		return zero;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		p.BaseColor = BaseColor;
	}
}
