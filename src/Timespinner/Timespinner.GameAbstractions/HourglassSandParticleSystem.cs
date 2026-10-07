using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core.Constants;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class HourglassSandParticleSystem : ParticleSystem
{
	private static readonly Vector4 SandBaseColor = new Vector4(208f, 144f, 64f, 255f);

	public HourglassSandParticleSystem(Texture2D inTexture, int howManyEffects)
		: base(inTexture, howManyEffects)
	{
		BaseColor = SandBaseColor;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 25f;
		_maxInitialSpeed = 38f;
		_minAcceleration = 67f;
		_maxAcceleration = 67f;
		_minLifetime = 0.3f;
		_maxLifetime = 0.3f;
		_minScale = 0.033f;
		_maxScale = 0.067f;
		_minNumParticles = 1;
		_maxNumParticles = 3;
		_minRotationSpeed = 0f;
		_maxRotationSpeed = 0f;
		_spriteBlendMode = BlendState.AlphaBlend;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(MathHelper.ToRadians(265f), MathHelper.ToRadians(275f));
		return new Vector2((float)Math.Cos(num), 0f - (float)Math.Sin(num));
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, new Vector2(where.X + ParticleSystem.RandomBetween(-4f, 4f), where.Y));
		p.BaseColor = BaseColor;
		int inGameZoom = Constants.InGameZoom;
		p.Velocity *= (float)inGameZoom;
		p.Acceleration *= (float)inGameZoom;
		p.Scale *= inGameZoom;
	}
}
