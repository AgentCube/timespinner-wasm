using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public sealed class NightmareDripParticleSystem : ParticleSystem
{
	private readonly bool _isNear;

	public NightmareDripParticleSystem(Texture2D inTexture, int howManyEffects, bool isNear)
		: base(inTexture, howManyEffects)
	{
		_isNear = isNear;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 1f;
		_maxInitialSpeed = 3f;
		_minAcceleration = 25f;
		_maxAcceleration = 50f;
		_minLifetime = 3f;
		_maxLifetime = 5f;
		_minScale = 0.1f;
		_maxScale = 0.3f;
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
		if (p.Velocity.Y > 0f)
		{
			p.Velocity.Y = 0f - p.Velocity.Y;
		}
		p.Acceleration.X = 0f - p.Velocity.X;
		p.Acceleration.Y = 0f - _maxAcceleration;
	}

	public override void Draw(SpriteBatch spriteBatch, Vector2 screenCenter, Vector2 cameraPosition, float cameraZoom)
	{
		if (!_isNear)
		{
			cameraPosition *= 0.75f;
		}
		base.Draw(spriteBatch, screenCenter, cameraPosition, cameraZoom);
	}
}
