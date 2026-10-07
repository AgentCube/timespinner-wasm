using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.LunaisProjectiles;

namespace Timespinner.GameObjects.Heroes.Spells;

internal class PinkOrbSpell : LunaisSpell
{
	private const ETeamSide Side = ETeamSide.Heroes;

	private const int StartOffsetX = -20;

	private const int StartOffsetY = -23;

	private const int ThunderCount = 8;

	private const float DormantIncrement = 0.15f;

	private const float BulletSpeed = 500f;

	private const float OrbChargeSpeed = 50f;

	private static readonly Color OrbAuraColor = new Color(0.75f, 0.25f, 0.5f, 0.25f);

	private static readonly Vector4 ParticleColor = new Vector4(1f, 0.5f, 0.75f, 1f);

	private static readonly int[] OrbCostIntervals = new int[1] { 40 };

	private readonly ThunderBoltDamageArea[] _thunderbolts = new ThunderBoltDamageArea[8];

	private bool _isShootingThunderbolts;

	private bool _isShootingLeft;

	private int _thunderboltsShotSoFar;

	private float _thunderboltTimer;

	private PinkOrbPlasmaLazer _lazer;

	private LunaisObj _parentLunais;

	public override EInventoryOrbType SpellType => EInventoryOrbType.Pink;

	public PinkOrbSpell(Level level)
		: base(level)
	{
		base.ChargeSpeed = 50f;
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

	public override BattleAnimation CreateChargeAnimation(int chargeLevel, GameObject orb, Level level, Point location)
	{
		BattleAnimation battleAnimation = new BattleAnimation(level.GCM.SpOrbPlasma, location, level);
		battleAnimation.TeamSide = ETeamSide.Heroes;
		battleAnimation.AnimationSpeed = 0.06f;
		battleAnimation.DoesRepeat = true;
		battleAnimation.AnchorObject = orb;
		battleAnimation.AnimationStart = 39;
		battleAnimation.AnimationLength = 4;
		return battleAnimation;
	}

	internal override bool CreateSpellProjectiles(Level level, LunaisObj parentLunais, int spellVariation)
	{
		_parentLunais = parentLunais;
		_isShootingLeft = parentLunais.IsFacingLeft;
		Point anchorOffset = new Point(-20, -23);
		Point point = parentLunais.Position.Add(new Point(-20 * (_isShootingLeft ? 1 : (-1)), -23));
		Vector2 value = new Vector2(_isShootingLeft ? (-1f) : 1f, 0f);
		value = Vector2.Multiply(value, 500f);
		if (_lazer == null)
		{
			_lazer = new PinkOrbPlasmaLazer(level, point, value, ETeamSide.Heroes, parentLunais, base.SpellDamage, this, isWide: false)
			{
				AnchorOffset = anchorOffset
			};
		}
		_lazer.Reset(point, value, parentLunais, base.SpellDamage);
		level.AddProjectile(_lazer);
		_isShootingThunderbolts = true;
		_thunderboltsShotSoFar = 0;
		_thunderboltTimer = 0f;
		level.PlayCue(ESFX.LunaisLargeLazerShoot, point);
		level.PlayCue(ESFX.LunaisLargeLazerShoot2D);
		return true;
	}

	public override void Update(float delta)
	{
		if (_isShootingThunderbolts && _parentLunais != null)
		{
			_thunderboltTimer -= delta;
			if (_thunderboltTimer <= 0f)
			{
				_thunderboltTimer += 0.15f;
				Point point = _parentLunais.Position.Add(new Point(-20 * (_isShootingLeft ? 1 : (-1)), -23));
				Point end = new Point(point.X + 400 * ((!_isShootingLeft) ? 1 : (-1)), point.Y);
				List<Vector4> intervalsBetween = ThunderBoltDamageArea.GetIntervalsBetween(point, end, _level);
				ThunderBoltDamageArea thunderBoltDamageArea;
				if (_thunderbolts[_thunderboltsShotSoFar] == null)
				{
					thunderBoltDamageArea = new ThunderBoltDamageArea(_level, point, ETeamSide.Heroes, base.SpellDamage, intervalsBetween, _isShootingLeft, this, EThunderBoltType.Lunais);
					_thunderbolts[_thunderboltsShotSoFar] = thunderBoltDamageArea;
				}
				else
				{
					thunderBoltDamageArea = _thunderbolts[_thunderboltsShotSoFar];
					thunderBoltDamageArea.Reset(point, base.SpellDamage, intervalsBetween, _isShootingLeft, 0f);
				}
				_level.AddProjectile(thunderBoltDamageArea);
				_thunderboltsShotSoFar++;
				if (_thunderboltsShotSoFar >= 8)
				{
					_isShootingThunderbolts = false;
				}
			}
		}
		base.Update(delta);
	}

	internal override void CancelSpell()
	{
		_isShootingThunderbolts = false;
		_thunderboltsShotSoFar = 0;
		_thunderboltTimer = 0f;
		if (_lazer != null)
		{
			_lazer.Cancel();
		}
		for (int i = 0; i < 8; i++)
		{
			if (_thunderbolts[i] != null)
			{
				_thunderbolts[i].Cancel();
			}
		}
	}
}
