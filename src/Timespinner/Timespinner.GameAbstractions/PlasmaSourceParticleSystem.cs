using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public sealed class PlasmaSourceParticleSystem : AnimatedParticleSystem
{
	private readonly Point _dimensions;

	private readonly Vector2 _emissionVector;

	public PlasmaSourceParticleSystem(SpriteSheet inSprite, int howManyEffects, Point dimensions, EDirection emissionDirection)
		: base(inSprite, howManyEffects, 7, 5, 0.066f, EAnimationType.Cycle)
	{
		_dimensions = dimensions;
		_spriteOrigin = new Vector2(16f, 16f);
		BaseColor = (Color.White * 0.9f).ToVector4();
		_emissionVector = Level.GetPointFromDirection(Point.Zero, emissionDirection).ToVector2();
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 0f;
		_maxInitialSpeed = 10f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minLifetime = 0.25f;
		_maxLifetime = 0.5f;
		_minScale = 1.5f;
		_maxScale = 1.75f;
		_minNumParticles = 1;
		_maxNumParticles = 1;
		_minAngle = -1f;
		_maxAngle = 1f;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
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
