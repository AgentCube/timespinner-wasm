using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.LunaisProjectiles;

internal sealed class EmpireOrbMeleeDamageArea : LunaisBaseOrbDamageArea
{
	private const int DamageWidth = 60;

	private const int DamageHeight = 20;

	private const float MaxLife = 0.25f;

	public EmpireOrbMeleeDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, Mobile inAnchor, int inDamage, LunaisOrb parentOrb, SpriteSheet sprite)
		: base(inLevel, inPosition, inSide, -1, inAnchor, parentOrb)
	{
		_doesDrawBaseSprite = false;
		_sprite = sprite;
		IsFacingLeft = inAnchor?.IsFacingLeft ?? false;
		_damageDimensions = new Point(60, 20);
		_bbox = new Rectangle(inPosition.X, inPosition.Y, 60, 20);
		SnapBboxToPosition();
		_power = inDamage;
		_force = 1;
		_life = 0.25f;
		base.DamageTimeoutTime = 1.25f;
		_damageElement = EDamageElement.Aura;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_isFlying = true;
		_doesCollideWithSlopes = false;
		_doesDieOnTiles = false;
		base.AnchorOffset = new Point(0, -10);
		ChangeAnimation(10);
	}

	protected override void DoDamageAnimation(Alive target, Point intersectionCenter)
	{
		_level.AddAnimation(new BattleAnimation(_sprite, intersectionCenter, _level)
		{
			TeamSide = _teamSide,
			AnimationStart = 12,
			AnimationLength = 3,
			DrawColor = Color.White * 0.75f
		});
		_level.PlayCue(ESFX.LunaisOrbImpactEmpire, intersectionCenter);
	}
}
