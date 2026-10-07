using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Misc;

namespace Timespinner.GameAbstractions.GameObjects;

internal sealed class CastleEngineerRock : Projectile
{
	public CastleEngineerRock(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, int baseDamage)
		: base(inLevel, inPosition, Vector2.Zero, ETeamSide.Enemies, -1f, inID)
	{
		_sprite = inSprite;
		_power = (int)Math.Ceiling((float)baseDamage * 2f);
		_bboxOffset = new Point(1, 1);
		Bbox = new Rectangle(_position.X, _position.Y, 18, 18);
		DrawOrigin = new Vector2(10f, 10f);
		_isAffectedByLevelBounds = false;
		_maxFallSpeed = 350f;
		_teamSide = ETeamSide.Neutral;
		int num = _level.NextRandomInt(0, 10);
		_rotationSpeed = num - 5;
		_doesRotateBasedOnVelocity = false;
		_doesCollideWithFloors = true;
		_isAffectedByGravity = true;
		base.DoesDieOnImpact = false;
		base.DoesKnockBack = true;
		base.DoesCollideWithTiles = true;
		base.CanBeStoodOnWhenFrozen = true;
		_life = 3f;
		base.Rotation = (float)_level.NextRandomDouble();
		_doesDieOnTiles = true;
		ChangeAnimation(13, 0, 1f, EAnimationType.None);
	}

	public override void KillOnProjectileImpact(Projectile proj)
	{
		if (!base.IsFrozen)
		{
			ExplodeRock(Position);
			base.KillOnProjectileImpact(proj);
		}
	}

	internal override void KillOnGround(bool isVerticalCollision, Point contactPoint)
	{
		ExplodeRock(contactPoint);
		base.KillOnGround(isVerticalCollision, contactPoint);
	}

	private void ExplodeRock(Point contactPoint)
	{
		for (int i = 0; i < 5; i++)
		{
			Point bboxDimensions = new Point(8, 8);
			Point point = new Point(0, -16);
			switch (i)
			{
			case 0:
				bboxDimensions = new Point(11, 12);
				point = new Point(-5, -9);
				break;
			case 1:
				bboxDimensions = new Point(11, 9);
				point = new Point(0, 4);
				break;
			case 2:
				bboxDimensions = new Point(14, 14);
				point = new Point(5, -8);
				break;
			case 3:
				bboxDimensions = new Point(10, 5);
				point = new Point(0, -8);
				break;
			case 4:
				bboxDimensions = new Point(9, 10);
				point = new Point(-5, 0);
				break;
			}
			point = new Point(point.X, point.Y + 6);
			Appendage appendage = new Appendage(this, bboxDimensions, Point.Zero, _level, _sprite);
			appendage.FollowType = EAppendageFollowType.AnchorLocked;
			appendage.AnchorOffset = point;
			Appendage appendage2 = appendage;
			appendage2.ChangeAnimation(i + 14);
			base.Appendages.Add(appendage2);
		}
		UpdateAppendages(0f);
		DebrisEvent.CreateFromObject(this, new Vector2(0f, -600f), new Point(Position.X, Bbox.Top), _sprite, DebrisEvent.EDebrisDeathType.Dust);
		_level.AddAnimation(EBattleAnimationType.MediumHitYellow, contactPoint, ETeamSide.Neutral, isFacingRight: true, doesPlaySFX: false);
		_level.AddAnimation(EBattleAnimationType.DustBoom, contactPoint, ETeamSide.Neutral);
		_level.PlayCue(ESFX.EnemyEngineerBoulderBreak, contactPoint);
		DamageArea damageArea = new DamageArea(_level, contactPoint.Add(0, -8), ETeamSide.Neutral, -1, null);
		damageArea.Life = 0.033f;
		damageArea.Power = base.Power;
		damageArea.DamageDimensions = new Point(64, 16);
		damageArea.DoesKnockBack = true;
		damageArea.DormantTimer = 0.15f;
		damageArea.DamageTimeoutTime = 0.5f;
		DamageArea damageArea2 = damageArea;
		damageArea2.Update(0f);
		_level.RequestAddObject(damageArea2);
	}
}
