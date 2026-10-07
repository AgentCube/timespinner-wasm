using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Timespinner.GameAbstractions;

public class DemonBossSparksParticleSystem : InverseLazerParticleSystem
{
	public DemonBossSparksParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
		BaseColor = new Vector4(1f, 0.85f, 0.75f, 0.9f);
	}

	protected override void InitializeConstants()
	{
		_minAngle = (float)Math.PI / 4f;
		_maxAngle = (float)Math.PI * 3f / 4f;
		_minInitialSpeed = 100f;
		_maxInitialSpeed = 150f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minLifetime = 0.2f;
		_maxLifetime = 0.3f;
		_minFriction = 0.2f;
		_maxFriction = 0.25f;
		_minScale = 0.1f;
		_maxScale = 0.15f;
		_minNumParticles = 2;
		_maxNumParticles = 2;
		_minRotationSpeed = 0f;
		_maxRotationSpeed = 0f;
		base.MinStartRadius = 0f;
		base.MaxStartRadius = 0f;
		base.MinWidth = 16f;
		base.MaxWidth = 24f;
		base.FollowPercentage = 1f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		p.BaseColor = BaseColor;
	}
}
