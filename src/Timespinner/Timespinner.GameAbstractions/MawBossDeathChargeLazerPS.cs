using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Timespinner.GameAbstractions;

public class MawBossDeathChargeLazerPS : LazerParticleSystem
{
	public MawBossDeathChargeLazerPS(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
		_origin.X = 0f;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 0f;
		_maxInitialSpeed = 0f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minLifetime = 0.2f;
		_maxLifetime = 0.5f;
		_minScale = 0.1f;
		_maxScale = 0.15f;
		_minNumParticles = 1;
		_maxNumParticles = 2;
		_minRotationSpeed = 0f;
		_maxRotationSpeed = 0f;
		base.MinStartRadius = 96f;
		base.MaxStartRadius = 128f;
		_minAngle = -(float)Math.PI / 4f;
		_maxAngle = (float)Math.PI / 4f;
		base.FollowPercentage = 1f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		p.BaseColor = BaseColor;
	}
}
