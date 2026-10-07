using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.LunaisProjectiles.Spell;

namespace Timespinner.GameObjects.Heroes.Spells;

internal class MoonOrbSpell : LunaisSpell
{
	private const int ColumnWidth = 16;

	private const int RowHeight = 20;

	private const int StartOffsetX = -16;

	private const int StartOffsetY = -8;

	private const int BoomOffsetX = 18;

	private const int BoomOffsetY = -10;

	private const int GustOffsetX = 8;

	private const int DamageAreaCount = 8;

	private const float TimeToShoot = 0.1f;

	private const float OrbChargeSpeed = 30f;

	private static readonly int[] OrbCostIntervals = new int[1] { 25 };

	private static readonly int[] DamageColumnCounts = new int[4] { 1, 2, 3, 2 };

	private readonly MoonOrbSpellDamageArea[] _damageAreas = new MoonOrbSpellDamageArea[8];

	private bool _hasShotBefore;

	private bool _isShooting;

	private bool _isShootingToTheLeft;

	private float _shootTimer;

	private Point _spellCreationPosition;

	public override EInventoryOrbType SpellType => EInventoryOrbType.Moon;

	internal MoonOrbSpell(Level level)
		: base(level)
	{
		base.ChargeSpeed = 30f;
		base.ChargeIntervals.AddRange(OrbCostIntervals);
		_sprite = _level.GCM.SpOrbMeleeMoon;
	}

	internal override bool CreateSpellProjectiles(Level level, LunaisObj parentLunais, int spellVariation)
	{
		_isShooting = true;
		_shootTimer = 0f;
		_isShootingToTheLeft = parentLunais.IsFacingLeft;
		_spellCreationPosition = parentLunais.Position.Add(new Point(-16 * (_isShootingToTheLeft ? 1 : (-1)), -8));
		if (!_isShootingToTheLeft)
		{
			_spellCreationPosition = _spellCreationPosition.Add(2, 0);
		}
		Point point = new Point(_spellCreationPosition.X + (_isShootingToTheLeft ? (-18) : 18), _spellCreationPosition.Y + -10);
		BattleAnimation battleAnimation = new BattleAnimation(_sprite, point, _level);
		battleAnimation.IsFacingLeft = !_isShootingToTheLeft;
		battleAnimation.AnimationStart = 20;
		battleAnimation.AnimationLength = 9;
		battleAnimation.AnimationSpeed = 0.033f;
		battleAnimation.TeamSide = ETeamSide.Heroes;
		BattleAnimation newAnimation = battleAnimation;
		Point position = new Point(parentLunais.Position.X + (_isShootingToTheLeft ? (-8) : 8), point.Y);
		_level.AddAnimation(BattleAnimation.Create(EBattleAnimationType.MediumRecoilDust, position, ETeamSide.Heroes, _isShootingToTheLeft, level, doesPlaySFX: false, EElementAnimationColor.Red));
		_level.AddAnimation(newAnimation);
		_level.PlayCue(ESFX.LunaisOrbShatteredSpell, point);
		_level.PlayCue(ESFX.LunaisOrbShatteredSpell2D);
		return true;
	}

	public override BattleAnimation CreateChargeAnimation(int chargeLevel, GameObject orb, Level level, Point location)
	{
		BattleAnimation battleAnimation = new BattleAnimation(_sprite, location, level);
		battleAnimation.TeamSide = ETeamSide.Heroes;
		battleAnimation.AnimationSpeed = 0.06f;
		battleAnimation.DoesRepeat = true;
		battleAnimation.AnchorObject = orb;
		battleAnimation.AnimationStart = 29;
		battleAnimation.AnimationLength = 4;
		return battleAnimation;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && _isShooting)
		{
			_shootTimer += delta;
			if (_shootTimer > 0.1f)
			{
				_isShooting = false;
				AddDamageAreas();
			}
		}
		base.Update(delta);
	}

	private void AddDamageAreas()
	{
		int num = 0;
		int num2 = 0;
		int num3 = DamageColumnCounts[num];
		int num4 = num3 * 20;
		int num5 = 0;
		int num6 = -num4 / 2;
		for (int i = 0; i < 8; i++)
		{
			Point position = new Point(_spellCreationPosition.X + (_isShootingToTheLeft ? (-num5) : num5), _spellCreationPosition.Y + num6);
			AddDamageArea(i, position);
			num2++;
			if (num2 >= num3)
			{
				num5 += 16;
				num++;
				if (num >= DamageColumnCounts.Length)
				{
					break;
				}
				num2 = 0;
				num3 = DamageColumnCounts[num];
				num4 = num3 * 20;
				num6 = -num4 / 2;
			}
			else
			{
				num6 += 20;
			}
		}
		_hasShotBefore = true;
	}

	private void AddDamageArea(int index, Point position)
	{
		MoonOrbSpellDamageArea moonOrbSpellDamageArea;
		if (!_hasShotBefore)
		{
			moonOrbSpellDamageArea = new MoonOrbSpellDamageArea(_level, position, ETeamSide.Heroes, base.SpellDamage, this);
			_damageAreas[index] = moonOrbSpellDamageArea;
		}
		else
		{
			moonOrbSpellDamageArea = _damageAreas[index];
			moonOrbSpellDamageArea.Reset(position, base.SpellDamage);
		}
		_level.AddProjectile(moonOrbSpellDamageArea);
	}
}
