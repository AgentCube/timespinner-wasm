using Microsoft.Xna.Framework;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.BaseClasses;
using Timespinner.GameObjects.Heroes.LunaisProjectiles;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.Spells;

internal class BlueOrbSpell : LunaisSpell
{
	private const int SmallProjectileCount = 12;

	private const float OrbChargeSpeed = 30f;

	private const float LargeSpellSpeed = 500f;

	private static readonly Color OrbAuraColor = new Color(0.25f, 0.25f, 0.75f, 0.25f);

	private static readonly Vector4 ParticleColor = new Vector4(0.66f, 0.66f, 1f, 1f);

	private static readonly int[] OrbCostIntervals = new int[2] { 10, 25 };

	private static readonly float[] SmallOscillations = new float[3] { 0f, 2.0943f, 4.1888f };

	private static readonly float[] SmallSleeps = new float[4] { 0f, 0.035f, 0.07f, 0.105f };

	private readonly BlueOrbSpellBulletSmall[] _smallProjectiles = new BlueOrbSpellBulletSmall[12];

	private BattleAnimation _mediumEmissionAnimation;

	private BlueOrbSpellBulletMedium _mediumProjectile;

	private BlueOrbSpellBulletLarge _largeProjectile;

	public override EInventoryOrbType SpellType => EInventoryOrbType.Blue;

	public BlueOrbSpell(Level level)
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
		LunaisOrb lunaisOrb = parentLunais.MainOrb ?? parentLunais.SubOrb;
		Point point = lunaisOrb?.Position ?? parentLunais.PrimaryOrbLocation;
		Vector2 value = new Vector2(parentLunais.IsFacingLeft ? (-1f) : 1f, 0f);
		if (spellVariation <= 0)
		{
			Point anchorOffset = new Point((int)(4f * value.X), 0);
			value = Vector2.Multiply(value, 450f);
			if (_mediumEmissionAnimation == null)
			{
				_mediumEmissionAnimation = new BattleAnimation(level.GCM.SpEffectsSmall, point, level)
				{
					TeamSide = ETeamSide.Heroes,
					AnimationSpeed = 0.04f,
					IsFacingLeft = parentLunais.IsFacingLeft,
					AnchorObject = lunaisOrb,
					AnchorOffset = anchorOffset,
					AnimationStart = 9,
					AnimationLength = 5
				};
			}
			else
			{
				_mediumEmissionAnimation.Reset(point, parentLunais.IsFacingLeft);
			}
			level.AddAnimation(_mediumEmissionAnimation);
			bool flag = true;
			if (_mediumProjectile == null)
			{
				_mediumProjectile = new BlueOrbSpellBulletMedium(level, point, value, ETeamSide.Heroes, 0.035f, isFirey: false, base.SpellDamage, this);
			}
			else
			{
				flag = _mediumProjectile.Reset(point, value, 0.035f, base.SpellDamage);
			}
			if (flag)
			{
				level.AddProjectile(_mediumProjectile);
			}
			level.PlayCue(ESFX.LunaisChargeShoot, point);
		}
		else
		{
			CreatePowerfulSpell(_level, point, base.SpellDamage, parentLunais.IsFacingLeft);
		}
		return true;
	}

	internal static void CreatePowerfulSpell(Level level, ETeamSide side, Point startPoint, int damage, LunaisOrbAbility ability, bool isFacingLeft)
	{
		Vector2 iV = new Vector2(isFacingLeft ? (-500f) : 500f, 0f);
		level.AddAnimation(EBattleAnimationType.MediumRecoilDust, startPoint, side, isFacingLeft, doesPlaySFX: false);
		level.AddProjectile(new BlueOrbSpellBulletLarge(level, startPoint, iV, side, damage, ability));
		float[] smallOscillations = SmallOscillations;
		foreach (float oscillOffset in smallOscillations)
		{
			float[] smallSleeps = SmallSleeps;
			foreach (float dormantTime in smallSleeps)
			{
				level.AddProjectile(new BlueOrbSpellBulletSmall(level, startPoint, iV, side, oscillOffset, dormantTime, damage, ability));
			}
		}
		level.PlayCue(ESFX.LunaisChargeShoot, startPoint);
	}

	private void CreatePowerfulSpell(Level level, Point startPoint, int damage, bool isFacingLeft)
	{
		Vector2 iV = new Vector2(isFacingLeft ? (-500f) : 500f, 0f);
		level.AddAnimation(EBattleAnimationType.MediumRecoilDust, startPoint, ETeamSide.Heroes, isFacingLeft, doesPlaySFX: false);
		bool flag = true;
		if (_largeProjectile == null)
		{
			_largeProjectile = new BlueOrbSpellBulletLarge(level, startPoint, iV, ETeamSide.Heroes, damage, this);
		}
		else
		{
			flag = _largeProjectile.Reset(startPoint, iV, damage);
		}
		if (flag)
		{
			level.AddProjectile(_largeProjectile);
		}
		int num = 0;
		float[] smallOscillations = SmallOscillations;
		foreach (float num2 in smallOscillations)
		{
			float[] smallSleeps = SmallSleeps;
			foreach (float num3 in smallSleeps)
			{
				flag = true;
				if (_smallProjectiles[num] == null)
				{
					_smallProjectiles[num] = new BlueOrbSpellBulletSmall(level, startPoint, iV, ETeamSide.Heroes, num2, num3, damage, this);
				}
				else
				{
					flag = _smallProjectiles[num].Reset(startPoint, iV, damage, num2, num3);
				}
				if (flag)
				{
					_level.AddProjectile(_smallProjectiles[num]);
				}
				num++;
			}
		}
		level.PlayCue(ESFX.LunaisChargeShoot, startPoint);
	}
}
