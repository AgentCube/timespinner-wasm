using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Bird;

internal sealed class GodBirdAuraProjectile : Projectile
{
	private static readonly Color BallBaseColor = new Color(0.75f, 0.75f, 0.85f, 0.8f);

	public GodBirdAuraProjectile(Level inLevel, Point inPosition, Vector2 iV, int damage, SpriteSheet sprite)
		: base(inLevel, inPosition, iV, ETeamSide.Enemies, 0f, -1)
	{
		_sprite = sprite;
		_bboxOffset = new Point(4, 4);
		_bbox = new Rectangle(inPosition.X - 8, inPosition.Y - 8, 16, 16);
		DrawOrigin = new Vector2(15f, 12f);
		base.DrawColor = BallBaseColor;
		_power = damage;
		_force = 3;
		_life = 1.5f;
		_damageElement = EDamageElement.Aura;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_maxMoveSpeed = 5000f;
		_maxFallSpeed = 5000f;
		_isFlying = true;
		_doesDieOutsideOfVisibleArea = true;
		_doesRotateBasedOnVelocity = true;
		ChangeAnimation(45, 4, 0.04f, EAnimationType.Cycle);
		_doesDrawTrail = true;
		_trailFadeRate = 1.5f;
		_trailLength = 8;
		_trailShrinkRate = 0.05f;
		_isTrailLengthAffectedByTime = false;
	}

	internal void Reset(Point position, Vector2 iV)
	{
		Position = position;
		base.Velocity = iV;
		base.ID = -1;
		_isFading = false;
		_fadeTimer = 0f;
		base.CanDamageThings = true;
		_life = 1f;
		base.DrawColor = BallBaseColor;
		ClearTrailHistory();
	}

	public override void Kill(bool useAnimation, Point deathPoint, bool deathFromInvulnerable)
	{
		if (useAnimation)
		{
			bool isFacingRight = true;
			Point position = deathPoint;
			if (!deathFromInvulnerable)
			{
				if (_velocity.X > 0f)
				{
					isFacingRight = false;
					position.X += 3;
				}
				else
				{
					position.X -= 3;
				}
				_level.AddAnimation(EBattleAnimationType.BigHit, position, _teamSide, isFacingRight);
			}
			else
			{
				_level.AddAnimation(EBattleAnimationType.BigHit, position, _teamSide, isFacingRight: true);
			}
		}
		Kill();
	}
}
