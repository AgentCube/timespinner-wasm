using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions.MonsterParticleEffects;

public class TowerPlasmaPodZapParticleSystem : AnimatedParticleSystem
{
	public TowerPlasmaPodZapParticleSystem(SpriteSheet inSprite, int howManyEffects, int start)
		: base(inSprite, howManyEffects, start, 1, 1f, EAnimationType.None)
	{
		BaseColor = new Vector4(0.8f, 0.8f, 0.8f, 0.6f);
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = -10f;
		_maxInitialSpeed = 10f;
		_minAcceleration = 0f;
		_maxAcceleration = 10f;
		_minLifetime = 0.15f;
		_maxLifetime = 0.2f;
		_minScale = 0.6f;
		_maxScale = 0.6f;
		_minScaleGrowthRate = 3f;
		_maxScaleGrowthRate = 4.5f;
		_minNumParticles = 1;
		_maxNumParticles = 1;
		_minRotationSpeed = (float)Math.PI / 4f;
		_maxRotationSpeed = (float)Math.PI / 2f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		p.BaseColor = BaseColor;
		base.InitializeParticle(p, where);
	}
}
