using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;
using Timespinner.GameObjects.Heroes.Spells;

namespace Timespinner.GameObjects.Heroes.Lunais.LunaisProjectiles.Spell;

internal class BarrierOrbSpellDamageArea : LunaisBaseOrbDamageArea
{
	private readonly float _maxLife;

	public BarrierOrbSpellDamageArea(Level inLevel, Point inPosition, ETeamSide inSide, int baseDamage, LunaisSpell parentSpell, float maxLife, SpriteSheet sprite, Mobile anchor)
		: base(inLevel, inPosition, inSide, -1, anchor, parentSpell)
	{
		_sprite = sprite;
		_doesDrawSpriteAndAppendages = false;
		base.AnchorOffset = new Point(0, -32);
		_power = baseDamage;
		_force = 4;
		_maxLife = maxLife;
		_life = maxLife;
		base.DamageTimeoutTime = 0.5f;
		_damageElement = EDamageElement.Light;
	}

	internal void SetDimensions(int width, int height)
	{
		Bbox = new Rectangle(Position.X, Position.Y, width + 2, height + 2);
	}

	internal void Reset(bool shouldAdd)
	{
		if (shouldAdd)
		{
			base.ID = -1;
		}
		_life = _maxLife;
		_isFading = false;
		_fadeTimer = 0f;
	}
}
