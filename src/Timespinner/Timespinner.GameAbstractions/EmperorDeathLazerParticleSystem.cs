using Microsoft.Xna.Framework.Graphics;

namespace Timespinner.GameAbstractions;

public class EmperorDeathLazerParticleSystem : LazerParticleSystem
{
	public EmperorDeathLazerParticleSystem(Texture2D inTexture, int howManyEffects)
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
		_minNumParticles = 6;
		_maxNumParticles = 6;
		_minRotationSpeed = 0f;
		_maxRotationSpeed = 0f;
		base.MinStartRadius = 12f;
		base.MaxStartRadius = 32f;
		base.FollowPercentage = 0.9f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}
}
