using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.LunaisProjectiles;

namespace Timespinner.GameObjects.Heroes.Spells;

internal class FlameOrbSpell : LunaisSpell
{
	private const int StartOffsetX = -20;

	private const int StartOffsetY = -23;

	private const int FlameHeightJitter = 150;

	private const float FlameSpeed = 200f;

	private const float OrbChargeSpeed = 50f;

	private const float TimeToFireFireballs = 1f;

	private const float TimeBetweenIndividualFireballs = 0.03f;

	private static readonly Color OrbAuraColor = new Color(0.75f, 0.25f, 0.25f, 0.25f);

	private static readonly Vector4 ParticleColor = new Vector4(1f, 0.6f, 0.3f, 1f);

	private static readonly int[] OrbCostIntervals = new int[1] { 35 };

	private readonly LunaisFireSpellDamageArea _fireDamageArea;

	private bool _isCreatingFireballs;

	private bool _isShootingToTheLeft;

	private float _fireballCreationTimer;

	private float _individualFireballTimer;

	private Point _spellCreationPosition;

	private SFXCueInstance _fireLoopCueInstance;

	private LunaisObj _parentLunais;

	public override EInventoryOrbType SpellType => EInventoryOrbType.Flame;

	public FlameOrbSpell(Level level)
		: base(level)
	{
		base.ChargeSpeed = 50f;
		base.ChargeIntervals.AddRange(OrbCostIntervals);
		_fireDamageArea = new LunaisFireSpellDamageArea(_level, Point.Zero, ETeamSide.Heroes, base.SpellDamage, this);
	}

	public override Vector4 GetChargeParticleColor()
	{
		return ParticleColor;
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

	public override Color GetAuraColor()
	{
		return OrbAuraColor;
	}

	internal override bool CreateSpellProjectiles(Level level, LunaisObj parentLunais, int spellVariation)
	{
		_parentLunais = parentLunais;
		_isCreatingFireballs = true;
		_fireballCreationTimer = 0f;
		_isShootingToTheLeft = parentLunais.IsFacingLeft;
		_spellCreationPosition = parentLunais.Position.Add(new Point(-20 * (_isShootingToTheLeft ? 1 : (-1)), -23));
		if (!_isShootingToTheLeft)
		{
			_spellCreationPosition = _spellCreationPosition.Add(2, 0);
		}
		PlayCue(ESFX.LunaisOrbFireSpell, _spellCreationPosition);
		if (_fireLoopCueInstance == null)
		{
			_fireLoopCueInstance = PlayCue(ESFX.BossEyeFlamethrower, _spellCreationPosition, isLooped: true);
		}
		else
		{
			_fireLoopCueInstance.SourcePosition = _spellCreationPosition;
			_fireLoopCueInstance.Play();
		}
		return true;
	}

	public override void Update(float delta)
	{
		if (_isCreatingFireballs)
		{
			if (_parentLunais != null)
			{
				_spellCreationPosition = _parentLunais.Position.Add(new Point(-20 * (_isShootingToTheLeft ? 1 : (-1)), -23));
				if (!_isShootingToTheLeft)
				{
					_spellCreationPosition = _spellCreationPosition.Add(2, 0);
				}
				if (_fireLoopCueInstance != null)
				{
					_fireLoopCueInstance.SourcePosition = _spellCreationPosition;
				}
			}
			if (_fireballCreationTimer <= 0f)
			{
				_fireDamageArea.Reset(_spellCreationPosition, base.SpellDamage, _isShootingToTheLeft, _parentLunais);
				_level.AddProjectile(_fireDamageArea);
			}
			_fireballCreationTimer += delta;
			if (_fireballCreationTimer >= 1f)
			{
				EndFireballs();
			}
			else
			{
				_individualFireballTimer -= delta;
				if (_individualFireballTimer <= 0f)
				{
					_individualFireballTimer = 0.03f;
					CreateFireball();
					CreateFireball();
				}
			}
		}
		base.Update(delta);
	}

	private void CreateFireball()
	{
		int num = _level.NextRandomInt(-150, 150);
		Vector2 iV = new Vector2(_isShootingToTheLeft ? (-200f) : 200f, num);
		_fireDamageArea.EmitFireball(_spellCreationPosition, iV);
	}

	internal override void ChangeRoom()
	{
		EndFireballs();
		base.ChangeRoom();
	}

	private void EndFireballs()
	{
		_isCreatingFireballs = false;
		_fireballCreationTimer = 0f;
		_individualFireballTimer = 0f;
		_fireDamageArea.End();
		if (_fireLoopCueInstance != null)
		{
			_fireLoopCueInstance.Pause(0.2f);
		}
	}

	internal override void CancelSpell()
	{
		if (_isCreatingFireballs)
		{
			EndFireballs();
		}
	}
}
