using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public sealed class PlasmaEmissionParticleSystem : AnimatedParticleSystem
{
	private readonly Point _dimensions;

	private readonly Vector2 _emissionVector;

	public PlasmaEmissionParticleSystem(SpriteSheet inSprite, int howManyEffects, Point dimensions, EDirection emissionDirection)
		: base(inSprite, howManyEffects, 2, 5, 0.066f, EAnimationType.Cycle)
	{
		_dimensions = dimensions;
		_spriteOrigin = new Vector2(16f, 24f);
		_isStartIndexRandom = true;
		BaseColor = (Color.White * 0.9f).ToVector4();
		_emissionVector = Level.GetPointFromDirection(Point.Zero, emissionDirection).ToVector2();
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 0f;
		_maxInitialSpeed = 5f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minLifetime = 0.5f;
		_maxLifetime = 1f;
		_minScale = 1.5f;
		_maxScale = 1.75f;
		_minNumParticles = 1;
		_maxNumParticles = 1;
		_minAngle = -0.5f;
		_maxAngle = 0.5f;
		_minRotationSpeed = 0f;
		_maxRotationSpeed = 0f;
	}

	protected override Vector2 PickRandomDirection()
	{
		return _emissionVector;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, Vector2.Add(where, new Vector2(14f, 0f)));
		p.BaseColor = BaseColor;
		if (p is AnimatedParticle animatedParticle)
		{
			animatedParticle.IsFacingLeft = true;
			if (ParticleSystem.RandomBetween(0f, 100f) > 50f)
			{
				animatedParticle.IsFlippedVertically = true;
			}
		}
	}
}
