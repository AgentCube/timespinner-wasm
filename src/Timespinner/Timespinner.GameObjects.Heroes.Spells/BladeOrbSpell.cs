using Microsoft.Xna.Framework;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.LunaisProjectiles;

namespace Timespinner.GameObjects.Heroes.Spells;

internal class BladeOrbSpell : LunaisSpell
{
	private const float OrbChargeSpeed = 50f;

	private static readonly Color OrbAuraColor = new Color(0.25f, 0.75f, 0.25f, 0.25f);

	private static readonly Vector4 ParticleColor = new Vector4(0.75f, 1f, 0.65f, 1f);

	private static readonly int[] OrbCostIntervals = new int[1] { 30 };

	public override EInventoryOrbType SpellType => EInventoryOrbType.Blade;

	public BladeOrbSpell(Level level)
		: base(level)
	{
		base.ChargeSpeed = 50f;
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
		LunaGiantSwordProjectile newProjectile = new LunaGiantSwordProjectile(level, parentLunais.Position, parentLunais.IsFacingLeft, ETeamSide.Heroes, parentLunais, base.SpellDamage, this);
		level.AddProjectile(newProjectile);
		parentLunais.PlayCue(ESFX.LunaisGiantSwordWhiff);
		return true;
	}

	public override BattleAnimation CreateChargeAnimation(int chargeLevel, GameObject orb, Level level, Point location)
	{
		BattleAnimation battleAnimation = new BattleAnimation(level.GCM.SpOrbMeleeBlade, location, level);
		battleAnimation.TeamSide = ETeamSide.Heroes;
		battleAnimation.AnimationSpeed = 0.06f;
		battleAnimation.DoesRepeat = true;
		battleAnimation.AnchorObject = orb;
		battleAnimation.AnimationStart = 29;
		battleAnimation.AnimationLength = 4;
		return battleAnimation;
	}
}
