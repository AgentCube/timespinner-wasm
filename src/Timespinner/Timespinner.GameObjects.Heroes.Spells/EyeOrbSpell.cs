using Microsoft.Xna.Framework;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.LunaisProjectiles.Spell;

namespace Timespinner.GameObjects.Heroes.Spells;

internal class EyeOrbSpell : LunaisSpell
{
	private const int StartOffsetX = 0;

	private const int StartOffsetY = 0;

	private const float OrbChargeSpeed = 30f;

	private static readonly Color OrbAuraColor = new Color(0.65f, 0.55f, 0.45f, 0.25f);

	private static readonly Vector4 ParticleColor = new Vector4(0.9f, 0.8f, 0.7f, 1f);

	private static readonly int[] OrbCostIntervals = new int[1] { 40 };

	public override EInventoryOrbType SpellType => EInventoryOrbType.Eye;

	internal EyeOrbSpell(Level level)
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
		bool isFacingLeft = parentLunais.IsFacingLeft;
		position = new Point(position.X, position.Y);
		level.AddAnimation(BattleAnimation.Create(EBattleAnimationType.MediumRecoilDust, new Point(position.X, position.Y - 16), ETeamSide.Heroes, isFacingLeft, level, doesPlaySFX: false, EElementAnimationColor.Blue));
		parentLunais.PlayCue(ESFX.LunaisOrbEyeSpell);
		_level.PlayCue(ESFX.LunaisOrbEyeSpell2D);
		_level.AddProjectile(new EyeOrbSpellDamageArea(_level, position, ETeamSide.Heroes, base.SpellDamage, this, parentLunais));
		return true;
	}

	public override BattleAnimation CreateChargeAnimation(int chargeLevel, GameObject orb, Level level, Point location)
	{
		BattleAnimation battleAnimation = new BattleAnimation(level.GCM.SpOrbMeleeEye, location, level);
		battleAnimation.TeamSide = ETeamSide.Heroes;
		battleAnimation.AnimationSpeed = 0.06f;
		battleAnimation.DoesRepeat = true;
		battleAnimation.AnchorObject = orb;
		battleAnimation.AnimationStart = 26;
		battleAnimation.AnimationLength = 4;
		return battleAnimation;
	}
}
