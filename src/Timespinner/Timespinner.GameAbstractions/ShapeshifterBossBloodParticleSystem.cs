using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class ShapeshifterBossBloodParticleSystem : ParticleSystem
{
	internal bool IsFacingLeft { get; set; }

	public ShapeshifterBossBloodParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 20f;
		_maxInitialSpeed = 70f;
		_minAcceleration = 2000f;
		_maxAcceleration = 2000f;
		_minFriction = 0.025f;
		_maxFriction = 0.025f;
		_minLifetime = 0.25f;
		_maxLifetime = 0.5f;
		_minScale = 0.1f;
		_maxScale = 0.15f;
		_minNumParticles = 10;
		_maxNumParticles = 12;
		_minRotationSpeed = -20f;
		_maxRotationSpeed = 20f;
		_minAngle = 3.2f;
		_maxAngle = 3.3f;
		_accelerationBias.X = 0f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		p.BaseColor = BaseColor;
		p.Velocity.Y = 0f;
		if (p.Velocity.X < 0f != IsFacingLeft)
		{
			p.Velocity.X = 0f - p.Velocity.X;
		}
		if (p.Acceleration.Y < 0f)
		{
			p.Acceleration.Y = 0f - p.Acceleration.Y;
		}
	}
}
