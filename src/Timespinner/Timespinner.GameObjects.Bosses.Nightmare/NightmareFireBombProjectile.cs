using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Nightmare;

internal sealed class NightmareFireBombProjectile : Projectile
{
	private const int FallStartX = 32;

	private const int FallOffsetX = 48;

	private const int FallStartY = -64;

	private const int BottomRemoveOffsetY = 64;

	private readonly int _startingPower;

	private readonly int _levelBottomY;

	private readonly int _bombIndex;

	internal bool IsFinished { get; private set; }

	public NightmareFireBombProjectile(Level inLevel, Point inPosition, Vector2 iV, SpriteSheet sprite, int damage, int bombIndex)
		: base(inLevel, inPosition, iV, ETeamSide.Enemies, -1)
	{
		_bombIndex = bombIndex;
		_sprite = sprite;
		_bboxOffset = new Point(0, 3);
		_bbox = new Rectangle(inPosition.X, inPosition.Y, 16, 60);
		_startingPower = (int)Math.Ceiling((float)damage * 1.1f);
		_power = 0;
		_force = 0;
		_life = 10f;
		_timeToFade = 0f;
		_damageElement = EDamageElement.Dark;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_maxMoveSpeed = 5000f;
		_maxFallSpeed = 5000f;
		_doesRotateBasedOnVelocity = false;
		_isTrailLengthAffectedByTime = false;
		base.DoesCollideWithTiles = false;
		_doesDieOnTiles = false;
		_doesCollideWithFloors = false;
		_doesCollideWithWalls = false;
		_doesCollideWithCeilings = false;
		_isIgnoringPlatform = false;
		base.DoesDieOnImpact = false;
		_doesDrawTrail = true;
		_trailLength = 3;
		_trailFadeRate = 3f;
		ChangeAnimation(12, 3, 0.1f, EAnimationType.Cycle);
		_levelBottomY = _level.RoomSize.Y;
	}

	public override void Update(float delta)
	{
		if (base.IsFrozen)
		{
			_isFrozen = false;
		}
		if (Bbox.Top > _levelBottomY + 64)
		{
			SilentKill();
		}
		base.Update(delta);
	}

	public override void SilentKill()
	{
		IsFinished = true;
		base.SilentKill();
	}

	internal void Reset()
	{
		base.ID = -1;
		_initialVector = Vector2.Zero;
		_velocity = Vector2.Zero;
		Position = new Point(32 + 48 * _bombIndex, -64);
		_isAffectedByGravity = true;
		_isFlying = false;
		base.IsDamageArea = false;
		_power = _startingPower;
		_isFading = false;
		_life = 10f;
		IsFinished = false;
		SnapBboxToPosition();
		ClearTrailHistory();
		Update(0f);
	}
}
