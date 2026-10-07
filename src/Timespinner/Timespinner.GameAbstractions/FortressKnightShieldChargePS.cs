using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Timespinner.GameAbstractions;

public class FortressKnightShieldChargePS : GravityWellParticleSystem
{
	protected const float ChaseMinStartRadius = 2f;

	protected const float ChaseMaxStartRadius = 4f;

	protected float _defaultMinStartRadius;

	protected float _defaultMaxStartRadius;

	public FortressKnightShieldChargePS(Texture2D inTexture, int howManyEffects)
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
		_minLifetime = 0.2f;
		_maxLifetime = 0.25f;
		_minScale = 0.15f;
		_maxScale = 0.3f;
		_minNumParticles = 1;
		_maxNumParticles = 1;
		_minRotationSpeed = -10f;
		_maxRotationSpeed = 10f;
		base.MinStartRadius = 10f;
		base.MaxStartRadius = 20f;
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
		p.BaseColor = BaseColor;
	}
}
