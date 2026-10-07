using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.Passives;

internal sealed class GreenOrbPassiveDamageArea : DamageArea
{
	private readonly LunaisOrb _parentOrb;

	private bool _wasActive;

	public GreenOrbPassiveDamageArea(Level inLevel, Point inPosition, LunaisOrb parentOrb)
		: base(inLevel, inPosition, ETeamSide.Heroes, -1, null)
	{
		_parentOrb = parentOrb;
		_doesDrawBaseSprite = false;
		_doesDrawSpriteAndAppendages = false;
		_damageDimensions = new Point(16, 16);
		_bbox = new Rectangle(inPosition.X, inPosition.Y, _damageDimensions.X, _damageDimensions.Y);
		SnapBboxToPosition();
		_power = _level.GameSave.GetOrbPassiveDamage(EInventoryOrbType.Blade);
		_force = 1;
		_life = 100f;
		_damageElement = EDamageElement.Sharp;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_isFlying = true;
		_doesCollideWithSlopes = false;
		_doesDieOnTiles = false;
		_doesDieOutsideOfVisibleArea = false;
	}

	public override void Update(float delta)
	{
		_life = 100f;
		bool flag = _parentOrb != null;
		if (flag)
		{
			Position = _parentOrb.OrbPassiveCenter;
			SnapBboxToPosition();
		}
		else if (_wasActive)
		{
			_life = 0f;
			ChangeAnimation(-1);
		}
		_wasActive = flag;
		base.Update(delta);
	}

	protected override void DoDamageAnimation(Alive target, Point intersectionCenter)
	{
		PlayCue(ESFX.LunaisOrbImpactSharp, intersectionCenter);
	}
}
