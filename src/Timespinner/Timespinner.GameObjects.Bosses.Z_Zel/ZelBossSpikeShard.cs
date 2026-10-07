using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Bosses.Z_Zel;

internal sealed class ZelBossSpikeShard : Appendage
{
	private const int Anim_SpikeStart = 41;

	private const int SpikeWidth = 32;

	private const int SpikeHeight = 96;

	private const int MinHorizontal = 5;

	private const int HorizontalMultiplier = 20;

	private const int HeightDifferenceThreshold = 24;

	private const int Uplift = -200;

	private const int HorizontalJitterMultiplier = 5;

	private const int RotationJitterMultiplier = 2;

	private const int MaxRotationSpeed = 25;

	private const float RotationSpeedMultiplier = 0.5f;

	private readonly Point _offset;

	private readonly ZelBossSpikeShardsDamageArea _parentDamageArea;

	private float _rotationSpeed;

	internal bool HasHitGround { get; set; }

	public ZelBossSpikeShard(ZelBossSpikeShardsDamageArea parent, Point bboxDimensions, Point inBboxOffset, Level inLevel, SpriteSheet inSprite, Rectangle frameSource, Point offset)
		: base(parent, bboxDimensions, inBboxOffset, inLevel, inSprite)
	{
		_parentDamageArea = parent;
		_offset = offset;
		DrawOrigin = new Vector2(bboxDimensions.X + inBboxOffset.X * 2 / 2, bboxDimensions.Y + inBboxOffset.Y * 2 / 2);
		ChangeAnimation(41);
		SetFrameSource(frameSource);
		base.FollowType = EAppendageFollowType.None;
		_isAffectedByGravity = true;
		_isFlying = false;
		Reset();
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (Math.Abs(_rotationSpeed) > 0.1f)
			{
				float num = base.Rotation + delta * _rotationSpeed;
				if (num >= 6.28f)
				{
					num -= 6.28f;
				}
				base.Rotation = num;
			}
			_velocity.Y += _gravityAcceleration * delta;
			_velocity.Y = MathHelper.Clamp(_velocity.Y, 0f - _maxFallSpeed, _maxFallSpeed);
			_velocity.X = MathHelper.Clamp(_velocity.X, 0f - _maxMoveSpeed, _maxMoveSpeed);
		}
		base.Update(delta);
	}

	internal void Reset()
	{
		int num = _parentDamageArea.Position.X - 32;
		int num2 = _parentDamageArea.Position.Y - 96;
		Position = new Point(num + _offset.X, num2 + _offset.Y);
		SnapBboxToPosition();
		base.Rotation = 0f;
		_velocity = Vector2.Zero;
		base.DrawColor = Color.White;
		HasHitGround = false;
	}

	internal void Push(Vector2 inVelocity, Point origin)
	{
		Point point = Bbox.Center.Subtract(origin);
		float num = (float)_level.NextRandomDouble();
		float num2 = num * 5f;
		float num3 = (num + 0.5f) * 2f;
		float num4 = 5f;
		bool flag;
		if (inVelocity.X > 1f || inVelocity.X < -1f)
		{
			flag = inVelocity.X < -1f;
			num4 = Math.Abs(inVelocity.X);
		}
		else
		{
			flag = point.X < 0;
		}
		int num5 = Math.Abs(point.Y);
		num4 *= 1f - (float)(num5 / 24);
		if (num4 < 5f)
		{
			num4 = 5f;
		}
		num4 += num2;
		num4 *= (float)(((!flag) ? 1 : (-1)) * 20);
		float y = inVelocity.Y + -200f;
		_velocity = new Vector2(num4, y);
		float num6 = (float)point.Y * num3 * 0.5f;
		num6 = ((num6 < 0f) ? Math.Max(num6, -25f) : Math.Min(num6, 25f));
		_rotationSpeed = num6 * (float)((!flag) ? 1 : (-1));
	}
}
