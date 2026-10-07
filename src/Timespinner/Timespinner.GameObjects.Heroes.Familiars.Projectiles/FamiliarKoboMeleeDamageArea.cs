using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Heroes.Familiars.Projectiles;

internal sealed class FamiliarKoboMeleeDamageArea : FamiliarBaseDamageArea
{
	private const float MaxLife = 0.05f;

	public FamiliarKoboMeleeDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, Mobile inAnchor, int inDamage, FamiliarBase parentFamiliar)
		: base(inLevel, inPosition, inSide, inAnchor, parentFamiliar)
	{
		base.IsMeleeAttack = true;
		_doesDrawSpriteAndAppendages = false;
		_damageDimensions = new Point(22, 28);
		_bbox = new Rectangle(inPosition.X, inPosition.Y, _damageDimensions.X, _damageDimensions.Y);
		IsImageFacingLeft = parentFamiliar.IsFacingLeft;
		base.AnchorOffset = new Point(-4, -12);
		SnapBboxToPosition();
		_power = inDamage;
		_force = 1;
		_life = 0.05f;
		base.DamageTimeoutTime = 0.5f;
		_damageElement = EDamageElement.Blunt;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_isFlying = true;
		_doesCollideWithSlopes = false;
		_doesDieOnTiles = false;
	}

	protected override void DoDamageAnimation(Alive target, Point intersectionCenter)
	{
		_level.AddAnimation(EBattleAnimationType.SmallHit, intersectionCenter, _teamSide, _anchorObject.Position.X < target.Position.X, doesPlaySFX: false);
		_level.PlayCue(ESFX.LunaisOrbImpact, intersectionCenter);
	}
}
