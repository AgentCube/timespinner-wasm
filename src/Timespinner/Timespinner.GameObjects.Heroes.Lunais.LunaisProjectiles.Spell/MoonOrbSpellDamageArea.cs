using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;
using Timespinner.GameObjects.Heroes.Spells;

namespace Timespinner.GameObjects.Heroes.Lunais.LunaisProjectiles.Spell;

internal class MoonOrbSpellDamageArea : LunaisBaseOrbDamageArea
{
	private const float MaxLife = 0.04f;

	public MoonOrbSpellDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, int baseDamage, LunaisSpell parentSpell)
		: base(inLevel, inPosition, inSide, -1, null, parentSpell)
	{
		_doesDrawSpriteAndAppendages = false;
		Bbox = new Rectangle(0, 0, 16, 16);
		_power = baseDamage;
		_force = 4;
		_life = 0.04f;
		base.DamageTimeoutTime = 0.5f;
		_damageElement = EDamageElement.Blunt;
	}

	public void Reset(Point startPoint, int spellDamage)
	{
		_isFading = false;
		_fadeTimer = 0f;
		_life = 0.04f;
		base.ID = -1;
		_power = spellDamage;
		Position = startPoint;
		SnapBboxToPosition();
		_damagedEnemiesDictionary.Clear();
		Update(0f);
	}
}
