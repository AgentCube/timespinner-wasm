using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class LabTurretBolt : Projectile
{
	private const int Width = 10;

	private const int Height = 3;

	private const int GlowCircleCount = 6;

	private const float GlowCircleRadius = 24f;

	private const float GlowCircleConsecutiveSizeReduction = 0.9f;

	private const float GlowColorMultipler = 0.025f;

	private const float GlowOscillationFrequency = 5f;

	private const float GlowOscillationCenter = 1f;

	private const float GlowOscillationAmplitude = 0.25f;

	private const float MaxLife = 2f;

	private static readonly Color SparkleColor = new Color(1f, 0.25f, 0.2f, 0.75f);

	private readonly int _basePower;

	private readonly Texture2D _glowTexture;

	private readonly LunaisChargeLeakParticleSystem _sparkleParticleSystem;

	private float _glowOscillationTimer;

	private float _glowOscillationMultipler = 1f;

	private Color _glowCircleColor;

	public LabTurretBolt(Level inLevel, Point inPosition, ETeamSide inSide, SpriteSheet sprite, int baseDamage)
		: base(inLevel, inPosition, Vector2.Zero, inSide, -1)
	{
		_sprite = sprite;
		_bboxOffset = new Point(0, 0);
		_bbox = new Rectangle(inPosition.X - 5, inPosition.Y - 5, 10, 3);
		DrawOrigin = new Vector2(5f, 1.5f);
		_basePower = (int)Math.Ceiling((float)baseDamage * 1.5f);
		_power = _basePower;
		_force = 0;
		_life = 2f;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_maxMoveSpeed = 5000f;
		_maxFallSpeed = 5000f;
		_isFlying = true;
		_doesDieOnTiles = true;
		_doesCollideWithSlopes = true;
		_doesCollideWithFloors = true;
		_doesCollideWithCeilings = true;
		_doesCollideWithWalls = true;
		base.DoesCollideWithTiles = true;
		base.DoesKnockBack = true;
		ChangeAnimation(10, 3, 0.066f, EAnimationType.Cycle);
		_glowTexture = inLevel.GCM.TxBlankSquare;
		base.AuraColor = SparkleColor;
		_sparkleParticleSystem = new LunaisChargeLeakParticleSystem(_level.GCM.TxParticleEnergy, 8)
		{
			BaseColor = SparkleColor.ToVector4()
		};
		_particleSystems.Add(_sparkleParticleSystem);
	}

	public override void Kill(bool useAnimation)
	{
		_level.AddAnimation(EBattleAnimationType.SmallHit, _bbox.Center, _teamSide);
		Kill();
	}

	public override void Update(float delta)
	{
		_sparkleParticleSystem.AddParticles(Bbox.Center.ToVector2());
		_glowOscillationTimer += delta;
		if (_glowOscillationTimer >= (float)Math.PI * 2f)
		{
			_glowOscillationTimer -= (float)Math.PI * 2f;
		}
		_glowOscillationMultipler = (float)(Math.Cos(_glowOscillationTimer * 5f) * 0.25) + 1f;
		_glowCircleColor = SparkleColor * 0.025f * _glowOscillationMultipler;
		base.Update(delta);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Point center = Bbox.Center;
		float num = 24f;
		for (int i = 0; i < 6; i++)
		{
			float num2 = num;
			float num3 = num / 4f;
			Vector2 value = new Vector2((float)center.X - num2 / 2f, (float)center.Y - num3 / 2f);
			Vector2 vector = Vector2.Subtract(_level.LevelRenderCenter, Vector2.Subtract(_level.CameraPosition, value));
			spriteBatch.Draw(destinationRectangle: new Rectangle((int)vector.X, (int)vector.Y, (int)num2, (int)num3), texture: _glowTexture, color: _glowCircleColor);
			num *= 0.9f;
		}
		base.Draw(spriteBatch);
	}

	internal void Reset(Point position, Vector2 iV)
	{
		base.ID = -1;
		_life = 2f;
		_isFading = false;
		_fadeTimer = 0f;
		_power = _basePower;
		Position = position;
		base.Velocity = iV;
		SnapBboxToPosition();
		base.DrawColor = Color.White;
	}
}
