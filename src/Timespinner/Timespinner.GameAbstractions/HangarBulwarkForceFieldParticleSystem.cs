using Microsoft.Xna.Framework.Graphics;

namespace Timespinner.GameAbstractions;

public class HangarBulwarkForceFieldParticleSystem : EnergyStreamParticleSystem
{
	public HangarBulwarkForceFieldParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeConstants()
	{
		base.FollowPercentage = 1f;
		_minInitialSpeed = 500f;
		_maxInitialSpeed = 800f;
		_minAngle = 0f;
		_maxAngle = 0f;
		_minHeightStart = -4f;
		_maxHeightStart = 4f;
		_minDistanceStart = 0f;
		_maxDistanceStart = 14f;
		_minAcceleration = 20f;
		_maxAcceleration = 50f;
		_minLifetime = 0.15f;
		_maxLifetime = 0.3f;
		_minScale = 1f;
		_maxScale = 2f;
		_minNumParticles = 1;
		_maxNumParticles = 2;
		_minRotationSpeed = 0f;
		_maxRotationSpeed = 20f;
		_minBrightness = 0.65f;
		_maxBrightness = 0.85f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}
}
