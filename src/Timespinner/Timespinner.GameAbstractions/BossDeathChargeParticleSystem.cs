using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Timespinner.GameAbstractions;

public class BossDeathChargeParticleSystem : GravityWellParticleSystem
{
	protected const float ChaseMinStartRadius = 2f;

	protected const float ChaseMaxStartRadius = 4f;

	protected float _defaultMinStartRadius;

	protected float _defaultMaxStartRadius;

	public BossDeathChargeParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
	}

	protected override void InitializeConstants()
	{
		_systemMass = 100000f;
		base.GrowsWhenNearCenter = true;
		base.DistanceScaleModifier = 0.001f;
		base.ProximityAgeRate = 0.4f;
		base.ProximityDistance = 10f;
		base.FollowPercentage = 0f;
		base.LostRadius = 3000000f;
		base.AbsorbRadius = 20f;
		base.AbsorbRate = 20f;
		_minInitialSpeed = 0f;
		_maxInitialSpeed = 0f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minLifetime = 0.5f;
		_maxLifetime = 1f;
		_minScale = 0.15f;
		_maxScale = 0.3f;
		_minNumParticles = 4;
		_maxNumParticles = 8;
		_minRotationSpeed = -10f;
		_maxRotationSpeed = 10f;
		base.MinStartRadius = 30f;
		base.MaxStartRadius = 50f;
		_defaultMinStartRadius = base.MinStartRadius;
		_defaultMaxStartRadius = base.MaxStartRadius;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		if (where != _lastWhere)
		{
			base.MinStartRadius = 2f;
			base.MaxStartRadius = 4f;
		}
		else
		{
			base.MinStartRadius = _defaultMinStartRadius;
			base.MaxStartRadius = _defaultMaxStartRadius;
		}
		base.InitializeParticle(p, where);
	}
}
