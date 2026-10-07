using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.BaseClasses;

namespace Timespinner.GameObjects.Heroes.Spells;

internal abstract class LunaisSpell : LunaisOrbAbility
{
	private const float DefaultChargeSpeed = 30f;

	private static readonly Color DefaultAuraColor = Color.White * 0.25f;

	private static readonly Color DefaultSpellColor = Color.White;

	private static readonly Vector4 DefaultChargeParticleColor = Color.White.ToVector4();

	private readonly List<int> _chargeIntervals = new List<int>();

	internal bool IsDrawn { get; set; }

	public abstract EInventoryOrbType SpellType { get; }

	public int SpellDamage { get; protected set; }

	public float ChargeSpeed { get; protected set; }

	public List<int> ChargeIntervals => _chargeIntervals;

	internal virtual bool CanCast => true;

	protected LunaisSpell(Level level)
		: base(Point.Zero, level, 0)
	{
		ChargeSpeed = 30f;
		base.SlotType = EOrbSlot.Spell;
	}

	public static LunaisSpell FromSpellType(EInventoryOrbType spellType, Level level)
	{
		LunaisSpell result = null;
		switch (spellType)
		{
		case EInventoryOrbType.Blue:
			result = new BlueOrbSpell(level);
			break;
		case EInventoryOrbType.Blade:
			result = new BladeOrbSpell(level);
			break;
		case EInventoryOrbType.Flame:
			result = new FlameOrbSpell(level);
			break;
		case EInventoryOrbType.Pink:
			result = new PinkOrbSpell(level);
			break;
		case EInventoryOrbType.Iron:
			result = new IronOrbSpell(level);
			break;
		case EInventoryOrbType.Ice:
			result = new IceOrbSpell(level);
			break;
		case EInventoryOrbType.Wind:
			result = new WindOrbSpell(level);
			break;
		case EInventoryOrbType.Umbra:
			result = new UmbraOrbSpell(level);
			break;
		case EInventoryOrbType.Gun:
			result = new GunOrbSpell(level);
			break;
		case EInventoryOrbType.Empire:
			result = new EmpireOrbSpell(level);
			break;
		case EInventoryOrbType.Eye:
			result = new EyeOrbSpell(level);
			break;
		case EInventoryOrbType.Blood:
			result = new BloodOrbSpell(level);
			break;
		case EInventoryOrbType.Book:
			result = new BookOrbSpell(level);
			break;
		case EInventoryOrbType.Moon:
			result = new MoonOrbSpell(level);
			break;
		case EInventoryOrbType.Nether:
			result = new NetherOrbSpell(level);
			break;
		case EInventoryOrbType.Barrier:
			result = new BarrierOrbSpell(level);
			break;
		}
		return result;
	}

	public virtual Color GetAuraColor()
	{
		return DefaultAuraColor;
	}

	public virtual Vector4 GetChargeParticleColor()
	{
		return DefaultChargeParticleColor;
	}

	public virtual BattleAnimation CreateChargeAnimation(int chargeLevel, GameObject orb, Level level, Point location)
	{
		BattleAnimation battleAnimation = new BattleAnimation(level.GCM.SpEffectsSmall, location, level);
		battleAnimation.TeamSide = ETeamSide.Heroes;
		battleAnimation.AnimationSpeed = 0.06f;
		battleAnimation.DoesRepeat = true;
		battleAnimation.AnchorObject = orb;
		battleAnimation.AnimationStart = 76;
		battleAnimation.AnimationLength = 4;
		BattleAnimation battleAnimation2 = battleAnimation;
		switch (chargeLevel)
		{
		case 1:
			battleAnimation2.AnimationStart = 80;
			break;
		case 2:
			battleAnimation2.AnimationStart = 84;
			break;
		}
		return battleAnimation2;
	}

	internal void UpdateDamage(int damage)
	{
		SpellDamage = damage;
	}

	internal virtual bool CreateSpellProjectiles(Level level, LunaisObj parentLunais, int spellVariation)
	{
		return true;
	}

	internal virtual void Update(float delta, bool isCharging, int chargeSelect, Point parentCenter)
	{
		Update(delta);
	}

	internal virtual void ChangeRoom()
	{
	}

	internal void Dispose()
	{
		CancelSpell();
	}

	internal virtual void CancelSpell()
	{
	}

	internal virtual void OnEnemyDeath(Point position)
	{
	}
}
