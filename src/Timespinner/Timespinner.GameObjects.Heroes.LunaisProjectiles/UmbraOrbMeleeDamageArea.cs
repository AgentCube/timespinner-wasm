using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.LunaisProjectiles;

internal sealed class UmbraOrbMeleeDamageArea : LunaisBaseOrbDamageArea
{
	private const float MaxLife = 0.5f;

	public UmbraOrbMeleeDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, Mobile inAnchor, int inDamage, LunaisOrb parentOrb)
		: base(inLevel, inPosition, inSide, -1, inAnchor, parentOrb)
	{
		_sprite = _level.GCM.SpOrbMeleeUmbra;
		_doesDrawSpriteAndAppendages = false;
		_damageDimensions = new Point(12, 12);
		_bboxOffset = new Point(1, 1);
		_bbox = new Rectangle(inPosition.X, inPosition.Y, _damageDimensions.X, _damageDimensions.Y);
		SnapBboxToPosition();
		_power = inDamage;
		_force = 1;
		_life = 0.5f;
		base.DamageTimeoutTime = 0.2f;
		_damageElement = EDamageElement.Dark;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_isFlying = true;
		_doesCollideWithSlopes = false;
		_doesDieOnTiles = false;
	}

	protected override void DoDamageAnimation(Alive target, Point intersectionCenter)
	{
		_level.AddAnimation(new BattleAnimation(_sprite, intersectionCenter, _level)
		{
			TeamSide = _teamSide,
			IsFacingLeft = (_anchorObject.Position.X < target.Position.X),
			AnimationStart = 11,
			AnimationLength = 4
		});
		_level.PlayCue(ESFX.LunaisOrbImpactDark, intersectionCenter);
	}
}
