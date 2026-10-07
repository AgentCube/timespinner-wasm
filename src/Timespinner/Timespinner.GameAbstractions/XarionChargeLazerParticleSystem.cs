using Microsoft.Xna.Framework.Graphics;

namespace Timespinner.GameAbstractions;

public class XarionChargeLazerParticleSystem : LazerParticleSystem
{
	public XarionChargeLazerParticleSystem(Texture2D inTexture, int howManyEffects)
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
		_maxNumParticles = 1;
		_minRotationSpeed = 0f;
		_maxRotationSpeed = 0f;
		base.MinStartRadius = 32f;
		base.MaxStartRadius = 64f;
		base.FollowPercentage = 0.9f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}
}
