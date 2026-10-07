using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameAbstractions;

public sealed class SandmanPortalParticleSystem : ParticleSystem
{
	private static readonly Vector4 SandColorA = new Vector4(0.63f, 0.5f, 0.3f, 1f);

	private static readonly Vector4 SandColorB = new Vector4(0.25f, 0.2f, 0.125f, 1f);

	private readonly int _originalPortalWidth;

	private readonly int _maxOffsetX;

	internal EDirection EmissionDirection { get; set; }

	public SandmanPortalParticleSystem(Texture2D texture, int howManyEffects, int portalWidth, EDirection emissionDirection)
		: base(texture, howManyEffects)
	{
		_originalPortalWidth = portalWidth;
		EmissionDirection = emissionDirection;
		_maxOffsetX = _originalPortalWidth / 2;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 5f;
		_maxInitialSpeed = 20f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minLifetime = 0.25f;
		_maxLifetime = 1f;
		_minScale = 0.1f;
		_maxScale = 0.15f;
		_minNumParticles = 3;
		_maxNumParticles = 9;
		_minRotationSpeed = -20f;
		_maxRotationSpeed = 20f;
		_spriteBlendMode = BlendState.AlphaBlend;
		_velocityBias.X = 0.2f;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		int x = (int)ParticleSystem.RandomBetween(-_maxOffsetX, _maxOffsetX);
		base.InitializeParticle(p, where.Add(new Point(x, 0)));
		float amount = ParticleSystem.RandomBetween(0f, 1f);
		p.BaseColor = SandColorA.Lerp(SandColorB, amount);
		if (p.Velocity.Y > 0f)
		{
			p.Velocity.Y = 0f - p.Velocity.Y;
		}
	}
}
