using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Nightmare;

internal sealed class NightmareFireBreathProjectile : Projectile
{
	private const int FloorHeight = 160;

	private const int Anim_FlameIndex = 18;

	private const int FrameWidth = 62;

	private const int HalfFrameWidth = 31;

	private const int BboxWidth = 24;

	private const int BboxOffsetWidth = 19;

	private const float MaxLife = 0.65f;

	private static readonly Color BaseDrawColor = new Color(96, 8, 200, 48);

	private static readonly Color BaseAuraColor = new Color(64, 96, 8, 32);

	private readonly int _startingPower;

	private bool _hasStarted;

	internal bool IsFinished { get; private set; }

	internal bool IsActive
	{
		get
		{
			if (!IsFinished)
			{
				return _hasStarted;
			}
			return false;
		}
	}

	public NightmareFireBreathProjectile(Level inLevel, Point inPosition, Vector2 iV, SpriteSheet sprite, int damage)
		: base(inLevel, inPosition, iV, ETeamSide.Enemies, -1)
	{
		_sprite = sprite;
		_bbox = new Rectangle(inPosition.X, inPosition.Y, 24, 24);
		_bboxOffset = new Point(19, 19);
		DrawOrigin = new Vector2(31f, 31f);
		_startingPower = (int)Math.Ceiling((float)damage * 1.15f);
		_power = _startingPower;
		_force = 0;
		_life = 0.65f;
		_timeToFade = 0f;
		_damageElement = EDamageElement.Dark;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_animationSpeed = 0f;
		_maxMoveSpeed = 5000f;
		_maxFallSpeed = 5000f;
		_isFlying = true;
		_doesRotateBasedOnVelocity = false;
		base.DoesCollideWithTiles = false;
		_doesDieOnTiles = false;
		ChangeAnimation(18);
		_rotationSpeed = 10f;
		base.Rotation = _level.NextRandomInt(0, 6);
		base.DoesDrawAura = true;
		base.AuraColor = BaseAuraColor;
		base.AuraFrequency = 1f;
		base.AuraSize = 0.1f;
		_auraCount = 4f;
	}

	public override void Update(float delta)
	{
		if (base.IsFrozen)
		{
			_isFrozen = false;
		}
		if (Bbox.Bottom >= 160)
		{
			_velocity.Y = 0f;
		}
		float num = _life / 0.65f;
		_scale = 0.5f * (1f - _life / 0.65f) + 0.5f;
		base.DrawColor = BaseDrawColor * num;
		base.AuraColor = BaseAuraColor * num;
		if (_life < 0.1f)
		{
			_power = 0;
		}
		base.Update(delta);
	}

	public override void SilentKill()
	{
		IsFinished = true;
		base.SilentKill();
	}

	internal void Reset(Point position, Vector2 iV)
	{
		Position = position;
		_initialVector = iV;
		_velocity = iV;
		SnapBboxToPosition();
		base.ID = -1;
		_power = _startingPower;
		_isFading = false;
		_life = 0.65f;
		IsFinished = false;
		_hasStarted = true;
	}
}
