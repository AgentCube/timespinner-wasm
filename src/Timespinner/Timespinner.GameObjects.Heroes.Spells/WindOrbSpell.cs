using Microsoft.Xna.Framework;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.LunaisProjectiles.Spell;

namespace Timespinner.GameObjects.Heroes.Spells;

internal class WindOrbSpell : LunaisSpell
{
	private const int StartOffsetX = 0;

	private const int StartOffsetY = 0;

	private const float OrbChargeSpeed = 30f;

	private static readonly Color OrbAuraColor = new Color(0.2f, 0.66f, 0.4f, 0.25f);

	private static readonly Vector4 ParticleColor = new Vector4(0.5f, 1f, 0.25f, 1f);

	private static readonly int[] OrbCostIntervals = new int[1] { 35 };

	private WindOrbSpellDamageArea _damageArea;

	public override EInventoryOrbType SpellType => EInventoryOrbType.Wind;

	internal override bool CanCast
	{
		get
		{
			if (_damageArea != null)
			{
				return _damageArea.IsFinished;
			}
			return true;
		}
	}

	internal WindOrbSpell(Level level)
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
		parentLunais.PlayCue(ESFX.LunaisOrbWindSpell);
		_damageArea = new WindOrbSpellDamageArea(_level, position, ETeamSide.Heroes, base.SpellDamage, this, parentLunais);
		_level.AddProjectile(_damageArea);
		return true;
	}

	internal override void ChangeRoom()
	{
		_damageArea = null;
		base.ChangeRoom();
	}

	public override BattleAnimation CreateChargeAnimation(int chargeLevel, GameObject orb, Level level, Point location)
	{
		BattleAnimation battleAnimation = new BattleAnimation(level.GCM.SpOrbMeleeWind, location, level);
		battleAnimation.TeamSide = ETeamSide.Heroes;
		battleAnimation.AnimationSpeed = 0.06f;
		battleAnimation.DoesRepeat = true;
		battleAnimation.AnchorObject = orb;
		battleAnimation.AnimationStart = 17;
		battleAnimation.AnimationLength = 4;
		return battleAnimation;
	}
}
