using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Etc;
using Timespinner.GameObjects.Heroes.Lunais.LunaisProjectiles;
using Timespinner.GameObjects.Heroes.Lunais.LunaisProjectiles.Spell;

namespace Timespinner.GameObjects.Heroes.Spells;

internal class NetherOrbSpell : LunaisSpell
{
	private const int MaxSimultaneousSouls = 12;

	private const int MaxStorableSouls = 16;

	private const int StartOffsetLeftX = 8;

	private const int StartOffsetRightX = 2;

	private const int StartOffsetY = -12;

	private const int SoulAddedPerSoul = 2;

	private const float OrbChargeSpeed = 30f;

	private static readonly int[] OrbCostIntervals = new int[1] { 25 };

	private static readonly Color SoulTrailColor = new Color(0.5f, 0.25f, 0.75f, 0.25f);

	private static readonly Vector4 SoulParticlesColor = new Vector4(0.25f, 0.66f, 0.5f, 0.5f);

	private readonly SpriteSheet _radiantOrbSprite;

	private readonly SoulTrailAppendage[] _netherSouls = new SoulTrailAppendage[12];

	private readonly NetherOrbSpellProjectile[] _homingSouls = new NetherOrbSpellProjectile[16];

	private bool _hasPlayedSeekCue;

	private int _soulCount;

	private LunaisObj _parentLunais;

	public override EInventoryOrbType SpellType => EInventoryOrbType.Nether;

	internal NetherOrbSpell(Level level)
		: base(level)
	{
		base.ChargeSpeed = 30f;
		base.ChargeIntervals.AddRange(OrbCostIntervals);
		_sprite = _level.GCM.SpOrbMeleeNether;
		ChangeAnimation(-1);
		_radiantOrbSprite = _level.GCM.SpOrbMeleeBarrier;
		base.IsDrawn = true;
	}

	public override BattleAnimation CreateChargeAnimation(int chargeLevel, GameObject orb, Level level, Point location)
	{
		BattleAnimation battleAnimation = new BattleAnimation(level.GCM.SpOrbMeleeNether, location, level);
		battleAnimation.TeamSide = ETeamSide.Heroes;
		battleAnimation.AnimationSpeed = 0.06f;
		battleAnimation.DoesRepeat = true;
		battleAnimation.AnchorObject = orb;
		battleAnimation.AnimationStart = 10;
		battleAnimation.AnimationLength = 4;
		return battleAnimation;
	}

	internal override bool CreateSpellProjectiles(Level level, LunaisObj parentLunais, int spellVariation)
	{
		_parentLunais = parentLunais;
		Point point = parentLunais.Position;
		bool isFacingLeft = parentLunais.IsFacingLeft;
		point = new Point(point.X + (isFacingLeft ? 8 : 2), point.Y + -12);
		level.PlayCue(ESFX.LunaisOrbNetherSpellCast, point);
		_level.AddProjectile(new NetherOrbSpellDamageArea(_level, point, ETeamSide.Heroes, base.SpellDamage, this, _radiantOrbSprite));
		if (_soulCount > 0)
		{
			float num = (float)Math.PI * 2f / (float)_soulCount;
			float num2 = 0f;
			_hasPlayedSeekCue = false;
			for (int i = 0; i < _soulCount; i++)
			{
				if (_homingSouls[i] == null)
				{
					NetherOrbSpellProjectile netherOrbSpellProjectile = new NetherOrbSpellProjectile(_level, point, ETeamSide.Heroes, _sprite, this, parentLunais, PlaySeekCue);
					netherOrbSpellProjectile.StartingAngle = num2;
					NetherOrbSpellProjectile netherOrbSpellProjectile2 = netherOrbSpellProjectile;
					_homingSouls[i] = netherOrbSpellProjectile2;
				}
				NetherOrbSpellProjectile netherOrbSpellProjectile3 = _homingSouls[i];
				netherOrbSpellProjectile3.Reset(point, base.SpellDamage, num2);
				_level.RequestAddObject(netherOrbSpellProjectile3);
				num2 += num;
			}
			_soulCount = 0;
		}
		return true;
	}

	private void PlaySeekCue()
	{
		if (!_hasPlayedSeekCue && _parentLunais != null)
		{
			PlayCue(ESFX.LunaisOrbNetherSpellSeek, _parentLunais.Bbox.Center);
			_hasPlayedSeekCue = true;
		}
	}

	internal override void OnEnemyDeath(Point position)
	{
		if (_soulCount >= 16)
		{
			return;
		}
		for (int i = 0; i < 12; i++)
		{
			if (_netherSouls[i] == null)
			{
				SoulTrailAppendage soulTrailAppendage = new SoulTrailAppendage(this, new Point(8, 8), Point.Zero, _level, _sprite, SoulTrailColor, SoulParticlesColor);
				soulTrailAppendage.Position = position;
				SoulTrailAppendage soulTrailAppendage2 = soulTrailAppendage;
				base.Appendages.Add(soulTrailAppendage2);
				_netherSouls[i] = soulTrailAppendage2;
				break;
			}
			if (_netherSouls[i].IsFinished)
			{
				SoulTrailAppendage soulTrailAppendage3 = _netherSouls[i];
				soulTrailAppendage3.Reset(position);
				break;
			}
		}
		_soulCount += 2;
		if (_soulCount > 16)
		{
			_soulCount = 16;
		}
	}

	internal override void ChangeRoom()
	{
		for (int i = 0; i < 12; i++)
		{
			if (_netherSouls[i] != null)
			{
				SoulTrailAppendage soulTrailAppendage = _netherSouls[i];
				if (!soulTrailAppendage.IsFinished)
				{
					soulTrailAppendage.ForceEnd();
				}
			}
		}
		base.ChangeRoom();
	}
}
