using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class FortressGunnerMiniBolt : Projectile
{
	private static readonly Color BaseTrailColor = new Color(128, 64, 32, 56);

	private static readonly Color DarkTrailColor = new Color(112, 48, 32, 48);

	private static readonly Color LightTrailColor = new Color(160, 80, 40, 64);

	private readonly float _oscillationDeltaOffset;

	private readonly Vector2 _trajectory;

	private float _timeSinceThrow;

	public FortressGunnerMiniBolt(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, SpriteSheet sprite, int baseDamage, bool isTop)
		: base(inLevel, inPosition, iV, inSide, -1)
	{
		_sprite = sprite;
		Vector2 trajectory = iV;
		trajectory.Normalize();
		_trajectory = trajectory;
		_oscillationDeltaOffset = (isTop ? 0f : ((float)Math.PI));
		ChangeAnimation(49);
		_bbox = new Rectangle(inPosition.X - 5, inPosition.Y - 5, 10, 10);
		_bboxOffset = new Point(-2, -2);
		_power = (int)Math.Ceiling((float)baseDamage * 1.5f);
		_force = 0;
		_life = 3f;
		_isAffectedByGravity = false;
		_isAffectedByFriction = true;
		_doesRotateBasedOnVelocity = false;
		_maxMoveSpeed = 5000f;
		_maxFallSpeed = 5000f;
		_isFlying = true;
		_doesDieOnTiles = true;
		_doesCollideWithSlopes = true;
		_doesCollideWithFloors = false;
		_doesCollideWithCeilings = false;
		_doesCollideWithWalls = true;
		base.DoesCollideWithTiles = true;
		base.DoesKnockBack = true;
		_isTrailLengthAffectedByTime = false;
		_doesDrawTrail = true;
		_doesDrawBrushTrail = true;
		_brushTrailSize = 6;
		_trailFadeRate = 1f;
		_trailInterpolationAmount = 5;
		_trailShrinkRate = 3.3E-05f;
		_trailLength = 50;
		_isTrailLengthAffectedByTime = false;
	}

	public override void Kill(bool useAnimation)
	{
		_level.AddAnimation(EBattleAnimationType.SmallHit, _bbox.Center, _teamSide);
		Kill();
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_timeSinceThrow += delta;
			float num = _timeSinceThrow * 15f * ((float)Math.PI / 2f) + _oscillationDeltaOffset;
			float num2 = (float)Math.Sin(num);
			float scaleFactor = num2 * 300f;
			Vector2 value = Vector2.Multiply(_trajectory, 500f);
			_velocity = Vector2.Add(value, Vector2.Multiply(new Vector2(0f - _trajectory.Y, _trajectory.X), scaleFactor));
			float num3 = (float)Math.Cos(num);
			if (num3 > 0f)
			{
				_trailColor = BaseTrailColor.SineInterpolate(LightTrailColor, num3);
			}
			else if (num3 < 0f)
			{
				_trailColor = BaseTrailColor.SineInterpolate(DarkTrailColor, 0f - num3);
			}
		}
		base.Update(delta);
	}
}
