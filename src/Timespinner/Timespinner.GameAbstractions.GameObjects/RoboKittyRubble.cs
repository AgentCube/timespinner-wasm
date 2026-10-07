using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions.GameObjects;

internal sealed class RoboKittyRubble : Projectile
{
	public RoboKittyRubble(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, int baseDamage)
		: base(inLevel, inPosition, Vector2.Zero, ETeamSide.Enemies, 0.5f, inID)
	{
		_sprite = inSprite;
		_power = (int)Math.Ceiling((float)baseDamage * 1.1f);
		_bboxOffset = new Point(5, 2);
		Bbox = new Rectangle(_position.X, _position.Y, 16, 16);
		DrawOrigin = new Vector2(12f, 12f);
		_isAffectedByLevelBounds = false;
		_maxFallSpeed = 350f;
		int num = _level.NextRandomInt(0, 10);
		_rotationSpeed = num - 5;
		_doesRotateBasedOnVelocity = false;
		_doesCollideWithFloors = true;
		_isAffectedByGravity = true;
		base.DoesCollideWithTiles = true;
		_life = 3f;
		_doesDieOnTiles = true;
		ChangeAnimation(18 + num % 2, 0, 1f, EAnimationType.None);
		_level.AddAnimation(EBattleAnimationType.Pebbles, Position, ETeamSide.Enemies, isFacingRight: true);
	}

	internal override void KillOnGround(bool isVerticalCollision, Point contactPoint)
	{
		contactPoint = contactPoint.Add(0, -4);
		_level.AddAnimation(EBattleAnimationType.Boom, contactPoint, ETeamSide.Enemies, isFacingRight: true);
		base.KillOnGround(isVerticalCollision, contactPoint);
	}
}
