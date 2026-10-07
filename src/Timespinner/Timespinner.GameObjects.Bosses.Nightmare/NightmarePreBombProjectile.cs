using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Nightmare;

internal sealed class NightmarePreBombProjectile : Projectile
{
	private const int FallStartY = -48;

	private readonly int _startingPower;

	internal bool IsFinished { get; private set; }

	public NightmarePreBombProjectile(Level inLevel, Point inPosition, Vector2 iV, SpriteSheet sprite, int damage)
		: base(inLevel, inPosition, iV, ETeamSide.Enemies, -1)
	{
		_sprite = sprite;
		_bboxOffset = Point.Zero;
		_bbox = new Rectangle(inPosition.X, inPosition.Y, 16, 16);
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
		_trailLength = 6;
		_trailFadeRate = 3f;
		ChangeAnimation(15, 3, 0.05f, EAnimationType.Cycle);
	}

	public override void Update(float delta)
	{
		if (base.IsFrozen)
		{
			_isFrozen = false;
		}
		if (Position.Y < -48 && _velocity.Y < 0f)
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

	internal void Reset(Point position, Vector2 iV)
	{
		base.ID = -1;
		Position = position;
		_initialVector = iV;
		_velocity = iV;
		_isAffectedByGravity = false;
		_isFlying = true;
		base.IsDamageArea = true;
		_power = _startingPower;
		_isFading = false;
		_life = 10f;
		IsFinished = false;
		SnapBboxToPosition();
		ClearTrailHistory();
		Update(0f);
	}
}
