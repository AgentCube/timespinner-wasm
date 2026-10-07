using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.LunaisProjectiles;

internal sealed class BlueOrbMeleeDamageArea : LunaisBaseOrbDamageArea
{
	private const float MaxLife = 0.1f;

	internal bool IsFinished { get; private set; }

	public BlueOrbMeleeDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, Mobile inAnchor, int inDamage, LunaisOrb parentOrb)
		: base(inLevel, inPosition, inSide, -1, inAnchor, parentOrb)
	{
		_sprite = null;
		_doesDrawSpriteAndAppendages = false;
		_damageDimensions = new Point(10, 10);
		_bbox = new Rectangle(inPosition.X, inPosition.Y, _damageDimensions.X, _damageDimensions.Y);
		SnapBboxToPosition();
		_power = inDamage;
		_force = 1;
		_life = 0.1f;
		base.DamageTimeoutTime = 0.2f;
		_damageElement = EDamageElement.Blunt;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_isFlying = true;
		_doesCollideWithSlopes = false;
		_doesDieOnTiles = false;
	}

	protected override void DoDamageAnimation(Alive target, Point intersectionCenter)
	{
		_level.AddAnimation(EBattleAnimationType.BlueOrbHit, intersectionCenter, _teamSide, _anchorObject.Position.X < target.Position.X, doesPlaySFX: false);
		_level.PlayCue(ESFX.LunaisOrbImpact, intersectionCenter);
	}

	public override void SilentKill()
	{
		IsFinished = true;
		base.SilentKill();
	}

	internal void Reset(Point position, int damage)
	{
		_power = damage;
		Position = position;
		SnapBboxToPosition();
		base.ID = -1;
		_isFading = false;
		_fadeTimer = 0f;
		_life = 0.1f;
		IsFinished = false;
	}
}
