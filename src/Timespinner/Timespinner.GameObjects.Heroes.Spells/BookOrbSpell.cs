using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.LunaisProjectiles;

namespace Timespinner.GameObjects.Heroes.Spells;

internal class BookOrbSpell : LunaisSpell
{
	private const int DarkInfernoFlameSpeedX = 400;

	private const int DarkInfernoFlameSpeedY = -50;

	private const int StartOffsetY = -16;

	private const float OrbChargeSpeed = 30f;

	private static readonly Color OrbAuraColor = new Color(0.75f, 0.25f, 0.25f, 0.25f);

	private static readonly Vector4 ParticleColor = new Vector4(1f, 0.6f, 0.3f, 1f);

	private static readonly int[] OrbCostIntervals = new int[1] { 45 };

	private BookOrbSpellProjectile _spellProjectile;

	public override EInventoryOrbType SpellType => EInventoryOrbType.Book;

	internal BookOrbSpell(Level level)
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
		Point point = parentLunais.Position.Add(0, -16);
		Vector2 iV = new Vector2(400 * ((!parentLunais.IsFacingLeft) ? 1 : (-1)), -50f);
		bool flag = true;
		if (_spellProjectile == null)
		{
			_spellProjectile = new BookOrbSpellProjectile(_level, point, iV, ETeamSide.Heroes, base.SpellDamage, this);
		}
		else
		{
			flag = _spellProjectile.Reset(point, iV, base.SpellDamage);
		}
		if (flag)
		{
			_level.AddProjectile(_spellProjectile);
			level.PlayCue(ESFX.LunaisChargeShoot, point);
		}
		return flag;
	}

	public override BattleAnimation CreateChargeAnimation(int chargeLevel, GameObject orb, Level level, Point location)
	{
		BattleAnimation battleAnimation = new BattleAnimation(level.GCM.SpOrbMeleeFire, location, level);
		battleAnimation.TeamSide = ETeamSide.Heroes;
		battleAnimation.AnimationSpeed = 0.06f;
		battleAnimation.DoesRepeat = true;
		battleAnimation.AnchorObject = orb;
		battleAnimation.AnimationStart = 42;
		battleAnimation.AnimationLength = 4;
		return battleAnimation;
	}
}
