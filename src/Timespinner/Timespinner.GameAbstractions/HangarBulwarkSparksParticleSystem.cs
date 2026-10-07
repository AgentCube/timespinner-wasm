using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Timespinner.GameAbstractions;

public class HangarBulwarkSparksParticleSystem : InverseLazerParticleSystem
{
	public HangarBulwarkSparksParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
		BaseColor = new Vector4(1f, 0.85f, 0.75f, 0.9f);
	}

	protected override void InitializeConstants()
	{
		_minAngle = 4.712389f;
		_maxAngle = 7.853982f;
		_minInitialSpeed = 50f;
		_maxInitialSpeed = 100f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minLifetime = 0.1f;
		_maxLifetime = 0.2f;
		_minFriction = 0.2f;
		_maxFriction = 0.25f;
		_minScale = 0.1f;
		_maxScale = 0.15f;
		_minNumParticles = 8;
		_maxNumParticles = 8;
		_minRotationSpeed = 0f;
		_maxRotationSpeed = 0f;
		base.MinStartRadius = 0f;
		base.MaxStartRadius = 0f;
		base.MinWidth = 8f;
		base.MaxWidth = 16f;
		base.FollowPercentage = 1f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		p.BaseColor = BaseColor;
	}
}
