using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions.GameObjects;

internal sealed class FortressEngineerBomb : Projectile
{
	private const float MaxLife = 3f;

	private readonly int _startingDamage;

	public FortressEngineerBomb(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, int baseDamage)
		: base(inLevel, inPosition, Vector2.Zero, ETeamSide.Enemies, -1f, inID)
	{
		_sprite = inSprite;
		_power = (int)Math.Ceiling((float)baseDamage * 2f);
		_startingDamage = _power;
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
		base.DoesDieOnImpact = true;
		base.DoesKnockBack = true;
		base.DoesCollideWithTiles = true;
		base.DoesDieToEnemyProjectiles = true;
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

	protected override void AddImpactAnimation(Alive target, Rectangle collidingRectangle)
	{
		ExplodeRock(Position);
	}

	internal override void KillOnGround(bool isVerticalCollision, Point contactPoint)
	{
		ExplodeRock(contactPoint);
		base.KillOnGround(isVerticalCollision, contactPoint);
	}

	private void ExplodeRock(Point contactPoint)
	{
		BattleAnimation battleAnimation = new BattleAnimation(_sprite, contactPoint, _level);
		battleAnimation.AnimationStart = 28;
		battleAnimation.AnimationLength = 8;
		battleAnimation.AnimationSpeed = 0.033f;
		battleAnimation.DoesFadeOut = false;
		battleAnimation.TeamSide = ETeamSide.Neutral;
		BattleAnimation newAnimation = battleAnimation;
		_level.AddAnimation(newAnimation);
		if (SFXCueInstance.IsSFXAudible(contactPoint, _level.CameraPosition))
		{
			_level.PlayCue(ESFX.EnemyFortressEngineerBomb, contactPoint);
		}
		DamageArea damageArea = new DamageArea(_level, Bbox.Center, ETeamSide.Neutral, -1, null);
		damageArea.Life = 0.033f;
		damageArea.Power = base.Power;
		damageArea.DamageDimensions = new Point(40, 40);
		damageArea.DoesKnockBack = true;
		damageArea.DamageTimeoutTime = 0.5f;
		DamageArea damageArea2 = damageArea;
		damageArea2.Update(0f);
		_level.RequestAddObject(damageArea2);
	}

	public void Reset(Point startPoint)
	{
		Position = startPoint;
		_initialVector = Vector2.Zero;
		_velocity = _initialVector;
		SnapBboxToPosition();
		base.ID = -1;
		_power = _startingDamage;
		_isFading = false;
		_life = 3f;
	}
}
