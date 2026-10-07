using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.LunaisProjectiles;

internal sealed class WindOrbMeleeDamageArea : LunaisBaseOrbDamageArea
{
	private const int SlashAnimationLength = 6;

	private const float AttackAnimationSpeed = 0.033f;

	private const float AnimationDuration = 0.198f;

	public const float TotalAnimationTime = 0.198f;

	private readonly Point _animationOffset = new Point(24, -4);

	public WindOrbMeleeDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, Mobile inAnchor, bool isFacingLeft, int inDamage, LunaisOrb parentOrb)
		: base(inLevel, inPosition, inSide, -1, inAnchor, parentOrb)
	{
		_sprite = _level.GCM.SpOrbMeleeWind;
		_doesDrawBaseSprite = true;
		_damageDimensions = new Point(44, 3);
		_bboxOffset = new Point(3, 22);
		_bbox = new Rectangle(inPosition.X, inPosition.Y, _damageDimensions.X, _damageDimensions.Y);
		IsFacingLeft = isFacingLeft;
		base.AnchorOffset = _animationOffset.Add(-40, 4);
		_doesProjectileChangeFacingBasedOnVelocity = false;
		_power = inDamage;
		_force = 2;
		_life = 0.198f;
		base.DamageTimeoutTime = 0.2f;
		_timeToFade = 0f;
		_damageElement = EDamageElement.Sharp;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_isFlying = true;
		_doesCollideWithSlopes = false;
		_doesDieOnTiles = false;
		ChangeAnimation(0, 6, 0.033f, EAnimationType.Once);
		base.DrawColor = new Color(0.9f, 0.9f, 0.9f, 0.5f);
		SnapBboxToPosition();
		SnapFrameToBbox();
	}

	protected override void DoDamageAnimation(Alive target, Point intersectionCenter)
	{
		_level.AddAnimation(new BattleAnimation(_sprite, intersectionCenter, _level)
		{
			AnimationStart = 21,
			AnimationLength = 3,
			IsFacingLeft = IsFacingLeft,
			TeamSide = base.TeamSide
		});
		_level.PlayCue(ESFX.LunaisPiercingHit, intersectionCenter);
	}

	internal void ReleaseDamageArea()
	{
		base.IsAnchored = false;
	}
}
