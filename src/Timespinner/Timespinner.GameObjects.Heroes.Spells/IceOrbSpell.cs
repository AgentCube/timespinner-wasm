using Microsoft.Xna.Framework;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.LunaisProjectiles.Spell;

namespace Timespinner.GameObjects.Heroes.Spells;

internal class IceOrbSpell : LunaisSpell
{
	private const float OrbChargeSpeed = 30f;

	private static readonly Color OrbAuraColor = new Color(0.25f, 0.5f, 0.66f, 0.25f);

	private static readonly Vector4 ParticleColor = new Vector4(0.66f, 0.75f, 0.9f, 1f);

	private static readonly int[] OrbCostIntervals = new int[1] { 40 };

	private bool _isCastingLeft;

	private Point _castPoint;

	private IceOrbSpellSnowProjectile _snowProjectile;

	public override EInventoryOrbType SpellType => EInventoryOrbType.Ice;

	internal IceOrbSpell(Level level)
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
		_castPoint = parentLunais.PrimaryOrbLocation;
		_isCastingLeft = parentLunais.IsFacingLeft;
		level.AddAnimation(BattleAnimation.Create(EBattleAnimationType.MediumRecoilDust, _castPoint, ETeamSide.Heroes, _isCastingLeft, level, doesPlaySFX: false, EElementAnimationColor.Blue));
		level.PlayCue(ESFX.LunaisOrbIceSpellThrow, _castPoint);
		level.PlayCue(ESFX.LunaisOrbIceSpellThrow2D);
		_battleAnimations.Add(new BattleAnimation(_level.GCM.SpEffectsSmall, new Point(Bbox.Center.X - 3, Bbox.Center.Y), _level)
		{
			TeamSide = ETeamSide.Heroes,
			AnimationStart = 69,
			AnimationLength = 4,
			DrawColor = Color.White * 0.75f,
			IsFacingLeft = _isCastingLeft
		});
		Vector2 iV = new Vector2((float)(((!_isCastingLeft) ? 1 : (-1)) * 3000) + parentLunais.Velocity.X * 10f, -150f);
		_snowProjectile = new IceOrbSpellSnowProjectile(_level, _castPoint, iV, ETeamSide.Heroes, base.SpellDamage, this);
		_level.AddProjectile(_snowProjectile);
		_level.PlayCue(ESFX.LunaisOrbIceWhiff, Position);
		return true;
	}

	public override BattleAnimation CreateChargeAnimation(int chargeLevel, GameObject orb, Level level, Point location)
	{
		BattleAnimation battleAnimation = new BattleAnimation(level.GCM.SpOrbMeleeIce, location, level);
		battleAnimation.TeamSide = ETeamSide.Heroes;
		battleAnimation.AnimationSpeed = 0.06f;
		battleAnimation.DoesRepeat = true;
		battleAnimation.AnchorObject = orb;
		battleAnimation.AnimationStart = 30;
		battleAnimation.AnimationLength = 4;
		return battleAnimation;
	}
}
