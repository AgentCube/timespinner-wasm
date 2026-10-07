using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Heroes.Spells;

internal class BarrierOrbSpell : LunaisSpell
{
	private const int MaxBarrierCount = 3;

	private const int CreationOffsetX = 24;

	private const int CreationOffsetY = 16;

	private const float OrbChargeSpeed = 30f;

	private static readonly Color OrbAuraColor = new Color(0.66f, 0.6f, 0.35f, 0.25f);

	private static readonly Vector4 ParticleColor = new Vector4(0.9f, 0.85f, 0.5f, 1f);

	private static readonly int[] OrbCostIntervals = new int[1] { 25 };

	private readonly BarrierOrbBarrier[] _barriers = new BarrierOrbBarrier[3];

	private int _nextBarrierIndex;

	public override EInventoryOrbType SpellType => EInventoryOrbType.Barrier;

	internal BarrierOrbSpell(Level level)
		: base(level)
	{
		_sprite = _level.GCM.SpOrbMeleeBarrier;
		base.ChargeSpeed = 30f;
		base.ChargeIntervals.AddRange(OrbCostIntervals);
	}

	internal override bool CreateSpellProjectiles(Level level, LunaisObj parentLunais, int spellVariation)
	{
		Point point = parentLunais.PrimaryOrbLocation;
		point = new Point(point.X + (parentLunais.IsFacingLeft ? (-24) : 24), point.Y + 16);
		if (_barriers[_nextBarrierIndex] == null)
		{
			BarrierOrbBarrier barrierOrbBarrier = new BarrierOrbBarrier(_level, point, -1, new ObjectTileSpecification
			{
				Category = EObjectTileCategory.Event
			}, _sprite, base.SpellDamage, this);
			_barriers[_nextBarrierIndex] = barrierOrbBarrier;
		}
		BarrierOrbBarrier barrierOrbBarrier2 = _barriers[_nextBarrierIndex];
		if (barrierOrbBarrier2.CreateReset(point))
		{
			_level.AddEvent(barrierOrbBarrier2);
		}
		_nextBarrierIndex++;
		if (_nextBarrierIndex >= 3)
		{
			_nextBarrierIndex = 0;
		}
		_level.PlayCue(ESFX.LunaisOrbRadiantSpell, point);
		_level.PlayCue(ESFX.LunaisOrbRadiantSpell2D);
		return true;
	}

	public override Vector4 GetChargeParticleColor()
	{
		return ParticleColor;
	}

	public override Color GetAuraColor()
	{
		return OrbAuraColor;
	}

	public override BattleAnimation CreateChargeAnimation(int chargeLevel, GameObject orb, Level level, Point location)
	{
		BattleAnimation battleAnimation = new BattleAnimation(level.GCM.SpOrbMeleeBarrier, location, level);
		battleAnimation.TeamSide = ETeamSide.Heroes;
		battleAnimation.AnimationSpeed = 0.06f;
		battleAnimation.DoesRepeat = true;
		battleAnimation.AnchorObject = orb;
		battleAnimation.AnimationStart = 28;
		battleAnimation.AnimationLength = 4;
		return battleAnimation;
	}
}
