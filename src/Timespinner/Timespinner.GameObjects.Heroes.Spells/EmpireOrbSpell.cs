using Microsoft.Xna.Framework;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.LunaisProjectiles.Spell;

namespace Timespinner.GameObjects.Heroes.Spells;

internal class EmpireOrbSpell : LunaisSpell
{
	private const float OrbChargeSpeed = 30f;

	private static readonly Color OrbAuraColor = new Color(0.55f, 0.4f, 0.66f, 0.25f);

	private static readonly Vector4 ParticleColor = new Vector4(0.75f, 0.5f, 0.8f, 1f);

	private static readonly int[] OrbCostIntervals = new int[1] { 45 };

	private EmpireOrbSpellDamageArea _damageArea;

	public override EInventoryOrbType SpellType => EInventoryOrbType.Empire;

	internal EmpireOrbSpell(Level level)
		: base(level)
	{
		base.ChargeSpeed = 30f;
		base.ChargeIntervals.AddRange(OrbCostIntervals);
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
		Point position = parentLunais.Position;
		if (_damageArea == null)
		{
			_damageArea = new EmpireOrbSpellDamageArea(_level, position, ETeamSide.Heroes, base.SpellDamage, this, parentLunais.IsFacingLeft, isUpsideDown: false);
		}
		else
		{
			_damageArea.Reset(position, parentLunais.IsFacingLeft, base.SpellDamage, isUpsideDown: false);
		}
		_level.AddProjectile(_damageArea);
		level.PlayCue(ESFX.LunaisOrbEmpireSpell, position);
		level.PlayCue(ESFX.LunaisOrbEmpireSpell2D);
		return true;
	}

	public override BattleAnimation CreateChargeAnimation(int chargeLevel, GameObject orb, Level level, Point location)
	{
		BattleAnimation battleAnimation = new BattleAnimation(level.GCM.SpOrbMeleeEmpire, location, level);
		battleAnimation.TeamSide = ETeamSide.Heroes;
		battleAnimation.AnimationSpeed = 0.06f;
		battleAnimation.DoesRepeat = true;
		battleAnimation.AnchorObject = orb;
		battleAnimation.AnimationStart = 17;
		battleAnimation.AnimationLength = 4;
		return battleAnimation;
	}
}
