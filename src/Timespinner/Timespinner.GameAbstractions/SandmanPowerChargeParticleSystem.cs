using Microsoft.Xna.Framework.Graphics;

namespace Timespinner.GameAbstractions;

public class SandmanPowerChargeParticleSystem : GravityWellParticleSystem
{
	public SandmanPowerChargeParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeConstants()
	{
		_systemMass = 35000f;
		base.GrowsWhenNearCenter = true;
		base.DistanceScaleModifier = 0.001f;
		base.ProximityAgeRate = 0.4f;
		base.ProximityDistance = 10f;
		base.FollowPercentage = 0f;
		base.LostRadius = 17000f;
		base.AbsorbRadius = 20f;
		base.AbsorbRate = 20f;
		_minInitialSpeed = 5f;
		_maxInitialSpeed = 10f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minLifetime = 0.5f;
		_maxLifetime = 1f;
		_minScale = 0.15f;
		_maxScale = 0.3f;
		_minNumParticles = 2;
		_maxNumParticles = 4;
		_minRotationSpeed = -10f;
		_maxRotationSpeed = 10f;
		base.MinStartRadius = 10f;
		base.MaxStartRadius = 20f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}
}
