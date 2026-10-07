using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public sealed class PlasmaFlowParticleSystem : AnimatedParticleSystem
{
	private readonly Point _dimensions;

	private readonly Vector2 _emissionVector;

	public PlasmaFlowParticleSystem(SpriteSheet inSprite, int howManyEffects, Point dimensions, EDirection emissionDirection)
		: base(inSprite, howManyEffects, 2, 5, 0.066f, EAnimationType.Cycle)
	{
		_dimensions = dimensions;
		_spriteOrigin = new Vector2(16f, 24f);
		BaseColor = (Color.White * 0.95f).ToVector4();
		_emissionVector = Level.GetPointFromDirection(Point.Zero, emissionDirection).ToVector2();
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 500f;
		_maxInitialSpeed = 500f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minLifetime = 0.25f;
		_maxLifetime = 0.5f;
		_minScale = 1.25f;
		_maxScale = 1.5f;
		_minNumParticles = 2;
		_maxNumParticles = 2;
		_minAngle = 0f;
		_maxAngle = 0f;
		_minRotationSpeed = 0f;
		_maxRotationSpeed = 0f;
	}

	protected override Vector2 PickRandomDirection()
	{
		return _emissionVector;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		Vector2 value = new Vector2(ParticleSystem.RandomBetween(0f, _dimensions.X / 2), 0f);
		base.InitializeParticle(p, Vector2.Add(where, value));
		p.BaseColor = BaseColor;
		if (p is AnimatedParticle animatedParticle)
		{
			animatedParticle.AnimationIndex = (int)ParticleSystem.RandomBetween(0f, 4f);
			if (ParticleSystem.RandomBetween(0f, 100f) > 50f)
			{
				animatedParticle.IsFlippedVertically = true;
			}
		}
	}
}
