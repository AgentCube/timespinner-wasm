using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions;

public sealed class ZelPortalWispParticleSystem : AnimatedParticleSystem
{
	private readonly int _originalPortalWidth;

	private readonly int _maxOffsetX;

	internal EDirection EmissionDirection { get; set; }

	public ZelPortalWispParticleSystem(SpriteSheet inSprite, int howManyEffects, int portalWidth, EDirection emissionDirection)
		: base(inSprite, howManyEffects, 42, 5, 0.1f, EAnimationType.Cycle)
	{
		_originalPortalWidth = portalWidth;
		EmissionDirection = emissionDirection;
		_maxOffsetX = _originalPortalWidth / 2;
		BaseColor = new Vector4(0.25f, 0.4f, 0.65f, 0.8f);
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 5f;
		_maxInitialSpeed = 20f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minLifetime = 0.25f;
		_maxLifetime = 1f;
		_minScale = 0.25f;
		_maxScale = 1f;
		_minNumParticles = 1;
		_maxNumParticles = 3;
		_minRotationSpeed = -(float)Math.PI / 8f;
		_maxRotationSpeed = (float)Math.PI / 8f;
	}

	protected override Vector2 PickRandomDirection()
	{
		return new Vector2(0f, (EmissionDirection == EDirection.South) ? 1 : (-1));
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		int x = (int)ParticleSystem.RandomBetween(-_maxOffsetX, _maxOffsetX);
		base.InitializeParticle(p, where.Add(new Point(x, 0)));
		p.BaseColor = BaseColor;
	}
}
