using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class FortressGunnerBolt : Projectile
{
	private const int GlowCircleCount = 12;

	private const float GlowCircleRadius = 32f;

	private const float GlowCircleConsecutiveSizeReduction = 0.9f;

	private const float GlowColorMultipler = 0.025f;

	private const float GlowOscillationFrequency = 5f;

	private const float GlowOscillationCenter = 1f;

	private const float GlowOscillationAmplitude = 0.25f;

	private readonly Color _sparkleColor;

	private readonly Texture2D _glowTexture;

	private readonly LunaisChargeLeakParticleSystem _sparkleParticleSystem;

	private float _glowOscillationTimer;

	private float _glowOscillationMultipler = 1f;

	private Color _glowCircleColor;

	public FortressGunnerBolt(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, SpriteSheet sprite, int baseDamage)
		: base(inLevel, inPosition, iV, inSide, -1)
	{
		_sprite = sprite;
		_bbox = new Rectangle(inPosition.X - 7, inPosition.Y - 7, 14, 4);
		_bboxOffset = new Point(1, 0);
		DrawOrigin = new Vector2(8f, 2.5f);
		_power = (int)Math.Ceiling((float)baseDamage * 1.5f);
		_force = 0;
		_life = 3f;
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
		ChangeAnimation(46, 3, 0.066f, EAnimationType.Cycle);
		_glowTexture = inLevel.GCM.TxBlankSquare;
		_sparkleColor = new Color(0.6f, 1f, 0.25f, 0.75f);
		base.AuraColor = _sparkleColor;
		_sparkleParticleSystem = new LunaisChargeLeakParticleSystem(_level.GCM.TxParticleEnergy, 8)
		{
			BaseColor = _sparkleColor.ToVector4()
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
		_glowCircleColor = _sparkleColor * 0.025f * _glowOscillationMultipler;
		base.Update(delta);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Point center = Bbox.Center;
		float num = 32f;
		for (int i = 0; i < 12; i++)
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
}
