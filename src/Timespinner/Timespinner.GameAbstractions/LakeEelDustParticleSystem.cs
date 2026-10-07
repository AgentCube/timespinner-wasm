using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public class LakeEelDustParticleSystem : AnimatedParticleSystem
{
	private static readonly Vector4 GreyDustColor = new Vector4(0.35f, 0.4f, 0.3f, 0.3f);

	private Vector2 _emissionVector;

	public LakeEelDustParticleSystem(SpriteSheet inSprite, int howManyEffect)
		: base(inSprite, howManyEffect, 9, 1, 1f, EAnimationType.None)
	{
		BaseColor = GreyDustColor;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 150f;
		_maxInitialSpeed = 250f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minFriction = 0.2f;
		_maxFriction = 0.1f;
		_minLifetime = 1.5f;
		_maxLifetime = 2f;
		_minScale = 0.25f;
		_maxScale = 1f;
		_minNumParticles = 3;
		_maxNumParticles = 5;
		_minRotationSpeed = -1f;
		_maxRotationSpeed = 1f;
	}

	public override void AddParticles(Vector2 where, Vector2 emissionVector)
	{
		_emissionVector = emissionVector;
		base.AddParticles(where);
	}

	protected override Vector2 PickRandomDirection()
	{
		float x = ParticleSystem.RandomBetween(-0.1f, 0.1f);
		float y = ParticleSystem.RandomBetween(-0.1f, 0.1f);
		Vector2 vector = new Vector2(x, y);
		return vector + _emissionVector;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		p.BaseColor = BaseColor;
	}
}
