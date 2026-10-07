using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class FamiliarSpellLeakParticleSystem : ParticleSystem
{
	public FamiliarSpellLeakParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 75f;
		_maxInitialSpeed = 150f;
		_minAcceleration = 2f;
		_maxAcceleration = 5f;
		_minFriction = 0.1f;
		_maxFriction = 0.1f;
		_minLifetime = 0.3f;
		_maxLifetime = 0.8f;
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
