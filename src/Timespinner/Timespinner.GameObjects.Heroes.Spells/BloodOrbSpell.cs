using Microsoft.Xna.Framework;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.LunaisProjectiles.Spell;

namespace Timespinner.GameObjects.Heroes.Spells;

internal sealed class BloodOrbSpell : LunaisSpell
{
	private const int SpellCost = 35;

	private const float OrbChargeSpeed = 50f;

	private static readonly Color OrbAuraColor = new Color(0.65f, 0.15f, 0.2f, 0.35f);

	private static readonly Vector4 ParticleColor = new Vector4(0.75f, 0.25f, 0.3f, 0.9f);

	private static readonly int[] OrbCostIntervals = new int[1] { 35 };

	private readonly BloodOrbSpellDamageArea _spellDamageArea;

	public override EInventoryOrbType SpellType => EInventoryOrbType.Blood;

	internal BloodOrbSpell(Level level)
		: base(level)
	{
		base.ChargeSpeed = 50f;
		base.ChargeIntervals.AddRange(OrbCostIntervals);
		_sprite = _level.GCM.SpOrbMeleeBlood;
		_doesDrawBaseSprite = false;
		_spellDamageArea = new BloodOrbSpellDamageArea(_level, Position, Vector2.Zero, ETeamSide.Heroes, _sprite, this, base.SpellDamage);
	}

	public override Vector4 GetChargeParticleColor()
	{
		return ParticleColor;
	}

	public override Color GetAuraColor()
	{
		return OrbAuraColor;
	}

	internal override bool CreateSpellProjectiles(Level level, LunaisObj parentLunais, int spellVariation)
	{
		Point startPoint = (parentLunais.MainOrb ?? parentLunais.SubOrb)?.Position ?? parentLunais.PrimaryOrbLocation;
		Vector2 iV = new Vector2(100f * (parentLunais.IsFacingLeft ? (-1f) : 1f), 0f);
		_spellDamageArea.Reset(startPoint, iV, base.SpellDamage);
		_level.AddProjectile(_spellDamageArea);
		return true;
	}

	public override BattleAnimation CreateChargeAnimation(int chargeLevel, GameObject orb, Level level, Point location)
	{
		BattleAnimation battleAnimation = new BattleAnimation(level.GCM.SpOrbMeleeBlood, location, level);
		battleAnimation.TeamSide = ETeamSide.Heroes;
		battleAnimation.AnimationSpeed = 0.06f;
		battleAnimation.DoesRepeat = true;
		battleAnimation.AnchorObject = orb;
		battleAnimation.AnimationStart = 42;
		battleAnimation.AnimationLength = 4;
		return battleAnimation;
	}
}
