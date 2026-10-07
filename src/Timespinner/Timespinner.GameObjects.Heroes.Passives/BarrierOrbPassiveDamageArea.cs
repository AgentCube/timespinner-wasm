using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Heroes.Passives;

internal sealed class BarrierOrbPassiveDamageArea : DamageArea
{
	private static readonly Color BarrierColor = new Color(0.175f, 0.15f, 0.1f, 0.075f);

	private readonly Appendage _reverseHaloAppendage;

	private readonly Animate _parentAnchor;

	public BarrierOrbPassiveDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, Animate inAnchor)
		: base(inLevel, inPosition, inSide, -1, inAnchor)
	{
		_sprite = inLevel.GCM.SpOrbMeleeBarrier;
		_parentAnchor = inAnchor;
		_bbox = new Rectangle(0, 0, 40, 40);
		base.AnchorOffset = new Point(0, -10);
		DrawOrigin = new Vector2(20f, 20f);
		base.CanDamageEnemies = false;
		base.CanDamageEvents = false;
		base.CanDamageEnemyProjectiles = true;
		base.DoesKillProjectilesOnImpact = true;
		base.DrawColor = BarrierColor;
		_life = 100f;
		_doesRotateBasedOnVelocity = false;
		_rotationSpeed = 1.5f;
		_doesUseAppendageCollision = false;
		_doAppendagesInheritDrawColor = false;
		_reverseHaloAppendage = new Appendage(this, new Point(40, 40), Point.Zero, _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			DrawColor = BarrierColor,
			DrawOrigin = DrawOrigin,
			AnchorOffset = new Point(0, 20)
		};
		_reverseHaloAppendage.ChangeAnimation(24);
		_appendages.Add(_reverseHaloAppendage);
		ChangeAnimation(24);
	}

	public override void Update(float delta)
	{
		_life = 100f;
		_reverseHaloAppendage.Rotation -= _rotationSpeed * delta;
		if (_reverseHaloAppendage.Rotation < 0f)
		{
			_reverseHaloAppendage.Rotation += (float)Math.PI * 2f;
		}
		base.Update(delta);
	}

	internal override void OnKillOtherProjectile(Projectile enemyProjectile, Vector2 depth)
	{
		Point intersectionCenter = RectangleExtensions.GetIntersectionCenter(enemyProjectile.Bbox, Bbox);
		_level.AddAnimation(new BattleAnimation(_sprite, intersectionCenter, _level)
		{
			AnimationStart = 19,
			AnimationLength = 5,
			DoesFadeOut = true
		});
		PlayCue(ESFX.LunaisOrbImpactDark);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (_parentAnchor.DoesDrawSpriteAndAppendages && _parentAnchor.DrawColor.A > 0 && !_level.IsPlayerInputBlocked)
		{
			base.Draw(spriteBatch);
		}
	}
}
