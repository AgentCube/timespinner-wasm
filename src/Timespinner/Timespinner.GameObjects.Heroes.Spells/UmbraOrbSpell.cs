using Microsoft.Xna.Framework;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.LunaisProjectiles;

namespace Timespinner.GameObjects.Heroes.Spells;

internal class UmbraOrbSpell : LunaisSpell
{
	private const float OrbChargeSpeed = 30f;

	private static readonly Color OrbAuraColor = new Color(0.45f, 0.35f, 0.66f, 0.25f);

	private static readonly Vector4 ParticleColor = new Vector4(0.75f, 0.65f, 1f, 1f);

	private static readonly int[] OrbCostIntervals = new int[1] { 30 };

	public override EInventoryOrbType SpellType => EInventoryOrbType.Umbra;

	internal UmbraOrbSpell(Level level)
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
		Point point = (parentLunais.MainOrb ?? parentLunais.SubOrb)?.Position ?? parentLunais.PrimaryOrbLocation;
		Vector2 iV = new Vector2(parentLunais.IsFacingLeft ? (-1f) : 1f, 0f);
		level.AddAnimation(BattleAnimation.Create(EBattleAnimationType.MediumRecoilDust, point, ETeamSide.Heroes, parentLunais.IsFacingLeft, level, doesPlaySFX: false, EElementAnimationColor.Purple));
		level.AddProjectile(new UmbraOrbSpellProjectile(level, point, iV, ETeamSide.Heroes, base.SpellDamage, isFirst: true, this));
		level.AddProjectile(new UmbraOrbSpellProjectile(level, point, iV, ETeamSide.Heroes, base.SpellDamage, isFirst: false, this));
		return true;
	}

	public override BattleAnimation CreateChargeAnimation(int chargeLevel, GameObject orb, Level level, Point location)
	{
		BattleAnimation battleAnimation = new BattleAnimation(level.GCM.SpOrbMeleeUmbra, location, level);
		battleAnimation.TeamSide = ETeamSide.Heroes;
		battleAnimation.AnimationSpeed = 0.06f;
		battleAnimation.DoesRepeat = true;
		battleAnimation.AnchorObject = orb;
		battleAnimation.AnimationStart = 15;
		battleAnimation.AnimationLength = 4;
		return battleAnimation;
	}
}
