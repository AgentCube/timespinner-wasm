using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.LunaisProjectiles;

internal sealed class BladeOrbMeleeDamageArea : LunaisBaseOrbDamageArea
{
	private const int SlashAnimationLength = 4;

	private const float AttackAnimationSpeed = 0.035f;

	private const float AnimationDuration = 0.14f;

	public const float TotalAnimationTime = 0.14f;

	private readonly Point _animationOffset = new Point(-2, -16);

	public BladeOrbMeleeDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, Mobile inAnchor, bool isFacingLeft, int inDamage, LunaisOrb parentOrb)
		: base(inLevel, inPosition, inSide, -1, inAnchor, parentOrb)
	{
		_sprite = _level.GCM.SpOrbMeleeBlade;
		_doesDrawBaseSprite = true;
		_damageDimensions = new Point(80, 40);
		_bbox = new Rectangle(inPosition.X, inPosition.Y, _damageDimensions.X, _damageDimensions.Y);
		IsFacingLeft = isFacingLeft;
		base.AnchorOffset = _animationOffset;
		_doesProjectileChangeFacingBasedOnVelocity = false;
		_power = inDamage;
		_force = 2;
		_life = 0.14f;
		base.DamageTimeoutTime = 0.2f;
		_timeToFade = 0f;
		_damageElement = EDamageElement.Sharp;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_isFlying = true;
		_doesCollideWithSlopes = false;
		_doesDieOnTiles = false;
		_doesUseAppendageCollision = true;
		_doAppendagesMatchImageFacing = true;
		Appendage item = new Appendage(this, new Point(48, 24), Point.Zero, _level, null)
		{
			AnchorObject = this,
			AnchorOffset = new Point(-16, 4),
			FollowType = EAppendageFollowType.AnchorLocked
		};
		Appendage item2 = new Appendage(this, new Point(16, 24), Point.Zero, _level, null)
		{
			AnchorObject = this,
			AnchorOffset = new Point(16, 12),
			FollowType = EAppendageFollowType.AnchorLocked
		};
		Appendage item3 = new Appendage(this, new Point(48, 8), Point.Zero, _level, null)
		{
			AnchorObject = this,
			AnchorOffset = new Point(16, 20),
			FollowType = EAppendageFollowType.AnchorLocked
		};
		Appendage item4 = new Appendage(this, new Point(12, 14), Point.Zero, _level, null)
		{
			AnchorObject = this,
			AnchorOffset = new Point(30, 12),
			FollowType = EAppendageFollowType.AnchorLocked
		};
		base.Appendages.Add(item);
		base.Appendages.Add(item2);
		base.Appendages.Add(item3);
		base.Appendages.Add(item4);
		ChangeAnimation(4, 4, 0.035f, EAnimationType.Once);
		base.DrawColor = new Color(0.9f, 0.9f, 0.9f, 0.5f);
		SnapBboxToPosition();
		SnapFrameToBbox();
		Update(0f);
	}

	protected override void DoDamageAnimation(Alive target, Point intersectionCenter)
	{
		_level.AddAnimation(new BattleAnimation(_sprite, intersectionCenter, _level)
		{
			AnimationStart = 22,
			AnimationLength = 3,
			TeamSide = ETeamSide.Heroes,
			IsFacingLeft = IsFacingLeft
		});
		_level.PlayCue(ESFX.LunaisOrbImpactSharp, intersectionCenter);
	}
}
