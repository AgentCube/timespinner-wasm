using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Timespinner.GameAbstractions;

public class BirdBossLazerChargeParticleSystem : LazerParticleSystem
{
	private const float BaseMinAngle = -(float)Math.PI / 2f;

	private const float BaseMaxAngle = -(float)Math.PI;

	public BirdBossLazerChargeParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
		_origin.X = 0f;
		BaseColor = (new Color(0.8f, 0.8f, 1f) * 0.9f).ToVector4();
	}

	public void AddParticles(Vector2 where, bool isParentFacingLeft)
	{
		_minAngle = -(float)Math.PI / 2f + (isParentFacingLeft ? 0f : ((float)Math.PI));
		_maxAngle = -(float)Math.PI + (isParentFacingLeft ? 0f : ((float)Math.PI));
		base.AddParticles(where);
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
		_minNumParticles = 5;
		_maxNumParticles = 5;
		_minRotationSpeed = 0f;
		_maxRotationSpeed = 0f;
		base.MinStartRadius = 15f;
		base.MaxStartRadius = 30f;
		base.FollowPercentage = 1f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		p.BaseColor = BaseColor;
	}
}
