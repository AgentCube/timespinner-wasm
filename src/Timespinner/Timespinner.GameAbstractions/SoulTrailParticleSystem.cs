using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class SoulTrailParticleSystem : ParticleSystem
{
	public SoulTrailParticleSystem(Texture2D inTexture, int howManyEffects, Vector4 baseColor)
		: base(inTexture, howManyEffects)
	{
		BaseColor = baseColor;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 15f;
		_maxInitialSpeed = 25f;
		_minAcceleration = 1f;
		_maxAcceleration = 3f;
		_minLifetime = 0.25f;
		_maxLifetime = 0.5f;
		_minScale = 0.1f;
		_maxScale = 0.15f;
		_minNumParticles = 1;
		_maxNumParticles = 1;
		_minRotationSpeed = -20f;
		_maxRotationSpeed = 20f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		p.BaseColor = BaseColor;
	}
}
