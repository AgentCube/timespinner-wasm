using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Base;

namespace Timespinner.GameAbstractions;

public class DustParticleSystem : ParticleSystem
{
	public static Vector4 PurpleDustColor = new Vector4(0.22f, 0.2f, 0.22f, 0.3f);

	public static Vector4 GreenDustColor = new Vector4(0.3f, 0.35f, 0.3f, 0.3f);

	public static Vector4 GreyDustColor = new Vector4(0.3f, 0.3f, 0.3f, 0.3f);

	public static Vector4 BrownDustColor = new Vector4(0.3f, 0.25f, 0.2f, 0.3f);

	public static Vector4 BlueDustColor = new Vector4(0.2f, 0.2125f, 0.25f, 0.3f);

	public DustParticleSystem(Texture2D inTexture, int howManyEffects, int levelID)
		: base(inTexture, howManyEffects)
	{
		BaseColor = GetDustColor(levelID) * 1.5f;
	}

	protected override void InitializeConstants()
	{
		_minInitialSpeed = 50f;
		_maxInitialSpeed = 150f;
		_minAcceleration = 0f;
		_maxAcceleration = 0f;
		_minFriction = 0.2f;
		_maxFriction = 0.1f;
		_minLifetime = 0.2f;
		_maxLifetime = 0.5f;
		_minScale = 0.1f;
		_maxScale = 0.5f;
		_minNumParticles = 20;
		_maxNumParticles = 30;
		_minRotationSpeed = -1f;
		_maxRotationSpeed = 1f;
		_velocityBias.Y *= 0.5f;
		_accelerationBias.Y *= 0.5f;
	}

	protected override Vector2 PickRandomDirection()
	{
		float num = ParticleSystem.RandomBetween(0f, 30f);
		num = ((ParticleSystem.Random.Next(2) == 0) ? (180f - num) : num);
		float num2 = MathHelper.ToRadians(num);
		Vector2 zero = Vector2.Zero;
		zero.X = (float)Math.Cos(num2);
		zero.Y = 0f - (float)Math.Sin(num2);
		return zero;
	}

	protected override void InitializeParticle(Particle p, Vector2 where)
	{
		base.InitializeParticle(p, where);
		p.Acceleration.X += ParticleSystem.RandomBetween(-50f, 50f);
		p.BaseColor = BaseColor;
	}

	public static Vector4 GetDustColor(int levelID)
	{
		Vector4 result = GreyDustColor;
		switch (levelID)
		{
		case 1:
		case 2:
		case 12:
			result = PurpleDustColor;
			break;
		case 0:
		case 4:
		case 16:
			result = GreenDustColor;
			break;
		case 3:
		case 7:
			result = BrownDustColor;
			break;
		case 10:
		case 14:
			result = BlueDustColor;
			break;
		}
		return result;
	}
}
