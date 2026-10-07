using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Heroes.Familiars.Projectiles;

internal sealed class FamiliarCrowSpellDamageArea : FamiliarBaseDamageArea
{
	private const int DamageSize = 32;

	private const float MaxLife = 0.5f;

	public FamiliarCrowSpellDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, Mobile inAnchor, int inDamage, FamiliarBase parentFamiliar)
		: base(inLevel, inPosition, inSide, inAnchor, parentFamiliar)
	{
		_doesDrawSpriteAndAppendages = false;
		_damageDimensions = new Point(32, 32);
		_bbox = new Rectangle(inPosition.X, inPosition.Y, _damageDimensions.X, _damageDimensions.Y);
		SnapBboxToPosition();
		_power = inDamage;
		_force = 1;
		_life = 0.5f;
		base.DamageTimeoutTime = 0.3f;
		_damageElement = EDamageElement.Light;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_isFlying = true;
		_doesCollideWithSlopes = false;
		_doesDieOnTiles = false;
	}

	protected override void DoDamageAnimation(Alive target, Point intersectionCenter)
	{
		_level.PlayCue(ESFX.LunaisOrbImpactIceTinyEnemy, intersectionCenter);
	}
}
