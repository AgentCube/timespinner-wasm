using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class LunaisOrbHitParticleSystem : ParticleSystem
{
	internal bool IsFacingLeft { get; set; }

	public LunaisOrbHitParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
		_isAccelerationDirectional = false;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 150f;
		_maxInitialSpeed = 250f;
		_minAcceleration = 500f;
		_maxAcceleration = 750f;
		_minLifetime = 0.3f;
		_maxLifetime = 0.5f;
		_minScale = 0.1f;
		_maxScale = 0.25f;
		_minNumParticles = 1;
		_maxNumParticles = 5;
		_spriteBlendMode = BlendState.AlphaBlend;
		_accelerationBias.X = 0f;
		_accelerationBias.Y *= 1f;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(MathHelper.ToRadians(-35f), MathHelper.ToRadians(60f));
		return new Vector2((float)(IsFacingLeft ? Math.Cos(num) : (0.0 - Math.Cos(num))), 0f - (float)Math.Sin(num));
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		p.BaseColor = BaseColor;
	}
}
