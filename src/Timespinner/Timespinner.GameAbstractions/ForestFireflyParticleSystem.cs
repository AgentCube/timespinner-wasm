using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class ForestFireflyParticleSystem : ParticleSystem
{
	private readonly bool _isNear;

	public ForestFireflyParticleSystem(Texture2D inTexture, int howManyEffects, bool isNear)
		: base(inTexture, howManyEffects)
	{
		_isNear = isNear;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 10f;
		_maxInitialSpeed = 30f;
		_minAcceleration = 2f;
		_maxAcceleration = 5f;
		_minLifetime = 2f;
		_maxLifetime = 5f;
		if (!_isNear)
		{
			_minLifetime *= 0.5f;
			_maxLifetime *= 0.5f;
		}
		_minScale = 0.1f;
		_maxScale = 0.35f;
		_minNumParticles = 1;
		_maxNumParticles = 1;
		_minRotationSpeed = -20f;
		_maxRotationSpeed = 20f;
		_velocityBias = new Vector2(1f, 0.75f);
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
	}

	public override void Draw(SpriteBatch spriteBatch, Vector2 screenCenter, Vector2 cameraPosition, float cameraZoom)
	{
		if (!_isNear)
		{
			cameraPosition *= 0.75f;
		}
		else
		{
			cameraPosition *= 0.625f;
		}
		base.Draw(spriteBatch, screenCenter, cameraPosition, cameraZoom);
	}
}
