using Microsoft.Xna.Framework.Graphics;

namespace Timespinner.GameAbstractions;

public class EmperorDeathParticleSystem : GravityWellParticleSystem
{
	public EmperorDeathParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeConstants()
	{
		_systemMass = 70000f;
		base.GrowsWhenNearCenter = true;
		base.DistanceScaleModifier = 0.001f;
		base.ProximityAgeRate = 0.4f;
		base.ProximityDistance = 20f;
		base.FollowPercentage = 0f;
		base.LostRadius = 34000f;
		base.AbsorbRadius = 40f;
		base.AbsorbRate = 40f;
		_minInitialSpeed = 0f;
		_maxInitialSpeed = 0f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minLifetime = 0.5f;
		_maxLifetime = 1f;
		_minScale = 0.15f;
		_maxScale = 0.3f;
		_minNumParticles = 2;
		_maxNumParticles = 6;
		_minRotationSpeed = -10f;
		_maxRotationSpeed = 10f;
		base.MinStartRadius = 16f;
		base.MaxStartRadius = 32f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}
}
