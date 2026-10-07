using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Spells;

namespace Timespinner.GameObjects.Heroes.Orbs;

internal class LunaisOrbManager
{
	private const float MaxOrbCollisionLeash = 22f;

	private const float TimeBeforeOrbsChargeAfterLunaisCharges = 0.1f;

	private readonly BattleAnimation _orbRippleAnim;

	private readonly LunaisDummyOrb _dummyOrb;

	private readonly LunaisObj _parentLunais;

	private readonly Level _level;

	private bool _isChargeParticleColorResetRequired;

	private bool _isCharging;

	private EInventoryOrbType _lastEquippedSpellType;

	private float _lastCharge;

	private float _chargeTimer;

	private Point _currentTarget;

	private Vector2 _lastOffset;

	private BattleAnimation _orbChargeAnim;

	private LunaisOrb _mainOrb;

	private LunaisOrb _subOrb;

	private SFXCueInstance _chargeStartSFXInstance;

	private SFXCueInstance _chargeStartVOInstance;

	private SFXCueInstance _chargeLoopSFXInstance;

	public bool IsPrimaryOrbNext { get; private set; }

	public LunaisOrb MainOrb => _mainOrb;

	public LunaisOrb SubOrb => _subOrb;

	public bool IsMeleeOrbEquipped
	{
		get
		{
			if (MainOrb == null)
			{
				return SubOrb != null;
			}
			return true;
		}
	}

	public bool AreAllOrbsBusy
	{
		get
		{
			if (MainOrb == null || MainOrb.IsAttacking)
			{
				if (SubOrb != null)
				{
					return SubOrb.IsAttacking;
				}
				return true;
			}
			return false;
		}
	}

	public EInventoryOrbType PrimaryOrbSelected
	{
		get
		{
			if (MainOrb != null)
			{
				return MainOrb.OrbColor;
			}
			if (SubOrb != null)
			{
				return SubOrb.OrbColor;
			}
			return EInventoryOrbType.None;
		}
	}

	public Point PrimaryOrbLocation
	{
		get
		{
			if (MainOrb != null)
			{
				return MainOrb.Position;
			}
			if (SubOrb != null)
			{
				return SubOrb.Position;
			}
			return _currentTarget;
		}
	}

	public LunaisOrbManager(LunaisObj lunais)
	{
		_parentLunais = lunais;
		_level = _parentLunais.Level;
		_dummyOrb = new LunaisDummyOrb(_level, _parentLunais, Point.Zero, null, isFrontPlane: true);
		ChangeMeleeOrb(EInventoryOrbType.Blue, EInventoryOrbType.Blue);
		IsPrimaryOrbNext = true;
	}

	public void Update(float delta, float charge)
	{
		_currentTarget = GetOrbCenterTarget(delta);
		if (IsMeleeOrbEquipped)
		{
			if (MainOrb != null)
			{
				MainOrb.Charge = charge;
				MainOrb.Update(delta, _currentTarget);
			}
			if (SubOrb != null)
			{
				SubOrb.Charge = charge;
				SubOrb.Update(delta, _currentTarget);
			}
		}
		else
		{
			_dummyOrb.Charge = charge;
			_dummyOrb.Update(delta, _currentTarget);
		}
		UpdateCharging(delta, charge);
	}

	private void UpdateCharging(float delta, float charge)
	{
		if (_orbRippleAnim != null)
		{
			_orbRippleAnim.Update(delta);
		}
		if (_orbChargeAnim != null)
		{
			_orbChargeAnim.Update(delta);
		}
		Point location = ((MainOrb != null) ? MainOrb.Bbox.Center : ((SubOrb == null) ? _currentTarget : SubOrb.Bbox.Center));
		if (_parentLunais.IsCharging)
		{
			_chargeTimer += delta;
			if (_chargeTimer >= 0.1f)
			{
				bool isCharging = _isCharging;
				SetAreOrbsCharging(isCharging: true);
				_chargeTimer = 0.1f;
				if (!isCharging)
				{
					if (_chargeStartSFXInstance != null)
					{
						_chargeStartSFXInstance.Stop();
					}
					if (_chargeStartVOInstance != null)
					{
						_chargeStartVOInstance.Stop();
					}
					_chargeStartSFXInstance = _parentLunais.PlayCue(ESFX.LunaisChargeStart);
					_chargeStartVOInstance = _parentLunais.PlayCue(ESFX.VO_Lun_Charge, isLooped: false, 0.6f);
				}
			}
		}
		else if (_chargeTimer > 0f)
		{
			_chargeTimer -= delta;
			if (_chargeTimer <= 0f)
			{
				_orbChargeAnim = null;
				_chargeTimer = 0f;
				SetAreOrbsCharging(isCharging: false);
				if (_chargeLoopSFXInstance != null)
				{
					_chargeLoopSFXInstance.Pause(0.1f);
				}
				if (_chargeStartSFXInstance != null)
				{
					_chargeStartSFXInstance.Pause(0.2f);
				}
				if (_chargeStartVOInstance != null)
				{
					_chargeStartVOInstance.Pause(0.2f);
				}
			}
		}
		else if (_parentLunais.IsDoingFancyIdleAnimation)
		{
			if (MainOrb != null && !MainOrb.IsDoingFancyAnimation)
			{
				MainOrb.IsDoingFancyAnimation = true;
			}
			if (SubOrb != null && !SubOrb.IsDoingFancyAnimation)
			{
				SubOrb.IsDoingFancyAnimation = true;
			}
			_dummyOrb.IsDoingFancyAnimation = true;
		}
		else
		{
			if (MainOrb != null && MainOrb.IsDoingFancyAnimation)
			{
				MainOrb.IsDoingFancyAnimation = false;
			}
			if (SubOrb != null && SubOrb.IsDoingFancyAnimation)
			{
				SubOrb.IsDoingFancyAnimation = false;
			}
			_dummyOrb.IsDoingFancyAnimation = false;
		}
		LunaisSpell equippedSpell = _parentLunais.EquippedSpell;
		if (equippedSpell != null && charge > 0f && _parentLunais.ChargeIntervals != null)
		{
			if (_isChargeParticleColorResetRequired || _lastEquippedSpellType != equippedSpell.SpellType)
			{
				_isChargeParticleColorResetRequired = false;
				_lastEquippedSpellType = equippedSpell.SpellType;
				Vector4 chargeParticleColor = equippedSpell.GetChargeParticleColor();
				if (MainOrb != null)
				{
					MainOrb.SetChargeParticlesColor(chargeParticleColor);
				}
				if (SubOrb != null)
				{
					SubOrb.SetChargeParticlesColor(chargeParticleColor);
				}
				_dummyOrb.SetChargeParticlesColor(chargeParticleColor);
			}
			LunaisOrb lunaisOrb = (MainOrb ?? SubOrb) ?? _dummyOrb;
			foreach (int chargeInterval in equippedSpell.ChargeIntervals)
			{
				float num = (float)chargeInterval * _parentLunais.AuraCostMultiplier;
				if (!(charge >= num) || !(_lastCharge < num) || lunaisOrb == null)
				{
					continue;
				}
				if (_orbChargeAnim == null)
				{
					_orbChargeAnim = equippedSpell.CreateChargeAnimation(0, lunaisOrb, _level, location);
					if (_chargeLoopSFXInstance == null)
					{
						_chargeLoopSFXInstance = lunaisOrb.PlayCue(ESFX.LunaisChargeLoop, isLooped: true);
					}
					else if (_chargeLoopSFXInstance != null)
					{
						_chargeLoopSFXInstance.Anchor = lunaisOrb;
						_chargeLoopSFXInstance.Resume();
					}
					Color orbIdleTrailColorByType = LunaisOrb.GetOrbIdleTrailColorByType(equippedSpell.SpellType);
					if (MainOrb != null)
					{
						MainOrb.SetTrailColor(orbIdleTrailColorByType);
					}
					if (SubOrb != null)
					{
						SubOrb.SetTrailColor(orbIdleTrailColorByType);
					}
				}
				else if (_parentLunais.ChargeSelectPercentage < 1f)
				{
					_orbChargeAnim = equippedSpell.CreateChargeAnimation(1, lunaisOrb, _level, location);
					_level.PlayCue(ESFX.LunaisChargeFlashMedium, lunaisOrb.Position);
				}
				else
				{
					_orbChargeAnim = equippedSpell.CreateChargeAnimation(2, lunaisOrb, _level, location);
					_level.PlayCue(ESFX.LunaisChargeFlashLarge, lunaisOrb.Position);
				}
				BattleAnimation battleAnimation2;
				if (!(_parentLunais.ChargeSelectPercentage >= 1f))
				{
					BattleAnimation battleAnimation = new BattleAnimation(_level.GCM.SpEffectsSmall, lunaisOrb.Position, _level);
					battleAnimation.TeamSide = ETeamSide.Heroes;
					battleAnimation.AnchorObject = lunaisOrb;
					battleAnimation.AnimationSpeed = 0.04f;
					battleAnimation.AnimationStart = 0;
					battleAnimation.AnimationLength = 5;
					battleAnimation.DrawColor = equippedSpell.GetAuraColor();
					battleAnimation2 = battleAnimation;
				}
				else
				{
					BattleAnimation battleAnimation3 = new BattleAnimation(_level.GCM.SpEffectsMedium, lunaisOrb.Position, _level);
					battleAnimation3.TeamSide = ETeamSide.Heroes;
					battleAnimation3.AnchorObject = lunaisOrb;
					battleAnimation3.AnimationSpeed = 0.04f;
					battleAnimation3.AnimationStart = 6;
					battleAnimation3.AnimationLength = 5;
					battleAnimation3.DrawColor = equippedSpell.GetAuraColor();
					battleAnimation2 = battleAnimation3;
				}
				BattleAnimation newAnimation = battleAnimation2;
				_level.AddAnimation(newAnimation);
				break;
			}
		}
		_lastCharge = charge;
	}

	private void SetAreOrbsCharging(bool isCharging)
	{
		if (MainOrb != null)
		{
			MainOrb.SetIsCharging(isCharging);
		}
		if (SubOrb != null)
		{
			SubOrb.SetIsCharging(isCharging);
		}
		_dummyOrb.SetIsCharging(isCharging);
		_isCharging = isCharging;
	}

	private Point GetOrbCenterTarget(float delta)
	{
		Point bulletOffset = _parentLunais.GetBulletOffset(22f);
		_lastOffset = _lastOffset.EaseTo(bulletOffset.ToVector2(), delta * 30f);
		Point point = _lastOffset.ToPoint();
		return new Point(_parentLunais.Position.X + point.X, _parentLunais.Position.Y + point.Y);
	}

	public void Draw(SpriteBatch spriteBatch, bool isTopOrb)
	{
		if (IsMeleeOrbEquipped)
		{
			if (SubOrb != null && SubOrb.IsDrawingOnFrontPlane == isTopOrb)
			{
				SubOrb.Draw(spriteBatch);
			}
			if (MainOrb != null && MainOrb.IsDrawingOnFrontPlane == isTopOrb)
			{
				MainOrb.Draw(spriteBatch);
			}
		}
		else
		{
			_dummyOrb.Draw(spriteBatch);
		}
		if (_orbRippleAnim != null)
		{
			_orbRippleAnim.Draw(spriteBatch);
		}
		if (_orbChargeAnim != null)
		{
			_orbChargeAnim.Draw(spriteBatch);
		}
	}

	public void ShiftMainOrbPosition(Point moveTo)
	{
		Point moveTo2 = new Point(_parentLunais.Position.X + moveTo.X, _parentLunais.Position.Y + moveTo.Y);
		if (MainOrb != null)
		{
			MainOrb.ResetPosition(moveTo2);
		}
		if (SubOrb != null)
		{
			SubOrb.ResetPosition(moveTo2);
		}
	}

	public void StopTime()
	{
	}

	public void UnStopTime()
	{
	}

	public void ThrowOrb(int orbIndex, bool direction)
	{
		LunaisOrb lunaisOrb = null;
		if (MainOrb == null || SubOrb == null)
		{
			lunaisOrb = ((MainOrb == null) ? SubOrb : MainOrb);
		}
		else if ((!MainOrb.IsAttacking && IsPrimaryOrbNext) || (SubOrb.IsAttacking && !MainOrb.IsAttacking))
		{
			lunaisOrb = MainOrb;
		}
		else if ((!SubOrb.IsAttacking && !IsPrimaryOrbNext) || (MainOrb.IsAttacking && !SubOrb.IsAttacking))
		{
			lunaisOrb = SubOrb;
		}
		if (lunaisOrb != null)
		{
			int orbColor = (int)lunaisOrb.OrbColor;
			lunaisOrb.IsThrowingLeft = direction;
			lunaisOrb.StartMeleeAttack(orbColor);
		}
		IsPrimaryOrbNext = SubOrb == null || !IsPrimaryOrbNext;
	}

	public void StartAttack(float inTime)
	{
		if (MainOrb != null)
		{
			MainOrb.StartAttack(inTime);
		}
		else if (SubOrb != null)
		{
			SubOrb.StartAttack(inTime);
		}
	}

	public void RefreshStats(GameSave inSave)
	{
		if (MainOrb == null || inSave.Inventory.EquippedMeleeOrbA != MainOrb.OrbColor || (SubOrb == null && inSave.Inventory.EquippedMeleeOrbB != 0) || (SubOrb != null && inSave.Inventory.EquippedMeleeOrbB != SubOrb.OrbColor))
		{
			ChangeMeleeOrb(inSave.Inventory.EquippedMeleeOrbA, inSave.Inventory.EquippedMeleeOrbB);
		}
		else
		{
			RefreshDamage();
		}
	}

	public void ChangeMeleeOrb(EInventoryOrbType firstOrb, EInventoryOrbType secondOrb)
	{
		LunaisOrb lunaisOrb = CreateOrbFromType(firstOrb, _level, _parentLunais, Point.Zero, 0f, isMainOrb: true);
		LunaisOrb subOrb = CreateOrbFromType(secondOrb, _level, _parentLunais, Point.Zero, 0f, isMainOrb: false);
		if (_orbChargeAnim != null)
		{
			BattleAnimation orbChargeAnim = _orbChargeAnim;
			orbChargeAnim.AnchorObject = lunaisOrb;
		}
		if (_mainOrb != null)
		{
			_mainOrb.DisposeOrb();
		}
		if (_subOrb != null)
		{
			_subOrb.DisposeOrb();
		}
		_mainOrb = lunaisOrb;
		_subOrb = subOrb;
		_isChargeParticleColorResetRequired = true;
	}

	internal static LunaisOrb CreateOrbFromType(EInventoryOrbType orbType, Level level, LunaisObj parentLunais, Point newOrbPosition, float oscillationDelta, bool isMainOrb)
	{
		LunaisOrb lunaisOrb = null;
		switch (orbType)
		{
		case EInventoryOrbType.Blue:
			lunaisOrb = new LunaisBlueOrb(level, parentLunais, newOrbPosition, level.GCM.SpOrbMeleeBlue, isMainOrb);
			break;
		case EInventoryOrbType.Blade:
			lunaisOrb = new LunaisBladeOrb(level, parentLunais, newOrbPosition, level.GCM.SpOrbMeleeBlade, isMainOrb);
			break;
		case EInventoryOrbType.Pink:
			lunaisOrb = new LunaisPinkOrb(level, parentLunais, newOrbPosition, level.GCM.SpOrbPlasma, isMainOrb);
			break;
		case EInventoryOrbType.Flame:
			lunaisOrb = new LunaisFlameOrb(level, parentLunais, newOrbPosition, level.GCM.SpOrbMeleeFire, isMainOrb);
			break;
		case EInventoryOrbType.Iron:
			lunaisOrb = new LunaisIronOrb(level, parentLunais, newOrbPosition, level.GCM.SpOrbMeleeIron, isMainOrb);
			break;
		case EInventoryOrbType.Ice:
			lunaisOrb = new LunaisIceOrb(level, parentLunais, newOrbPosition, level.GCM.SpOrbMeleeIce, isMainOrb);
			break;
		case EInventoryOrbType.Wind:
			lunaisOrb = new LunaisWindOrb(level, parentLunais, newOrbPosition, level.GCM.SpOrbMeleeWind, isMainOrb);
			break;
		case EInventoryOrbType.Umbra:
			lunaisOrb = new LunaisUmbraOrb(level, parentLunais, newOrbPosition, level.GCM.SpOrbMeleeUmbra, isMainOrb);
			break;
		case EInventoryOrbType.Eye:
			lunaisOrb = new LunaisEyeOrb(level, parentLunais, newOrbPosition, level.GCM.SpOrbMeleeEye, isMainOrb);
			break;
		case EInventoryOrbType.Empire:
			lunaisOrb = new LunaisEmpireOrb(level, parentLunais, newOrbPosition, level.GCM.SpOrbMeleeEmpire, isMainOrb);
			break;
		case EInventoryOrbType.Gun:
			lunaisOrb = new LunaisGunOrb(level, parentLunais, newOrbPosition, level.GCM.SpOrbMeleeGun, isMainOrb);
			break;
		case EInventoryOrbType.Blood:
			lunaisOrb = new LunaisBloodOrb(level, parentLunais, newOrbPosition, level.GCM.SpOrbMeleeBlood, isMainOrb);
			break;
		case EInventoryOrbType.Book:
			lunaisOrb = new LunaisBookOrb(level, parentLunais, newOrbPosition, level.GCM.SpOrbMeleeBook, isMainOrb);
			break;
		case EInventoryOrbType.Moon:
			lunaisOrb = new LunaisMoonOrb(level, parentLunais, newOrbPosition, level.GCM.SpOrbMeleeMoon, isMainOrb);
			break;
		case EInventoryOrbType.Nether:
			lunaisOrb = new LunaisNetherOrb(level, parentLunais, newOrbPosition, level.GCM.SpOrbMeleeNether, isMainOrb);
			break;
		case EInventoryOrbType.Barrier:
			lunaisOrb = new LunaisBarrierOrb(level, parentLunais, newOrbPosition, level.GCM.SpOrbMeleeBarrier, isMainOrb);
			break;
		}
		if (lunaisOrb != null)
		{
			int orbDamage = level.GameSave.GetOrbDamage(orbType);
			lunaisOrb.UpdateDamage(orbDamage);
			lunaisOrb.OscillationDelta = oscillationDelta;
		}
		return lunaisOrb;
	}

	public void Kill()
	{
		if (MainOrb != null)
		{
			MainOrb.Kill();
		}
		if (SubOrb != null)
		{
			SubOrb.Kill();
		}
	}

	public void ChangeRoom()
	{
		if (MainOrb != null)
		{
			MainOrb.ChangeRoom();
		}
		if (SubOrb != null)
		{
			SubOrb.ChangeRoom();
		}
	}

	public void HideOrbs(float hiddenTime)
	{
		if (MainOrb != null || SubOrb != null)
		{
			_parentLunais.PlayCue(ESFX.LunaisOrbsVanish);
			if (MainOrb != null)
			{
				MainOrb.SetOrbHiddenStatus(hiddenTime, doesHide: true, doesShowAnimation: false);
			}
			if (SubOrb != null)
			{
				SubOrb.SetOrbHiddenStatus(hiddenTime, doesHide: true, doesShowAnimation: false);
			}
		}
	}

	internal void SetOrbHiddenStatus(bool isHidden, bool shouldShowAnimation)
	{
		if (MainOrb != null || SubOrb != null)
		{
			if (shouldShowAnimation)
			{
				_parentLunais.PlayCue(isHidden ? ESFX.LunaisOrbsVanish : ESFX.LunaisOrbsAppear);
			}
			if (MainOrb != null)
			{
				MainOrb.SetOrbHiddenStatus(-1f, isHidden, shouldShowAnimation);
			}
			if (SubOrb != null)
			{
				SubOrb.SetOrbHiddenStatus(-1f, isHidden, shouldShowAnimation);
			}
		}
	}

	public float GetMeleeAnimationSpeed()
	{
		float result = 0.06f;
		if (MainOrb != null)
		{
			result = MainOrb.MeleeAnimationSpeed;
		}
		return result;
	}

	public bool GiveExperience(int enemyID)
	{
		bool result = GiveSpecificOrbExperience(enemyID, MainOrb);
		if (!GiveSpecificOrbExperience(enemyID, SubOrb))
		{
			return result;
		}
		return true;
	}

	private bool GiveSpecificOrbExperience(int enemyID, LunaisOrb orb)
	{
		bool result = false;
		if (orb != null && orb.HitEnemyRegistry.Contains(enemyID))
		{
			orb.HitEnemyRegistry.Remove(enemyID);
			if (_level.GameSave.GiveOrbExperience(orb.OrbColor))
			{
				result = true;
				RefreshDamage();
			}
		}
		return result;
	}

	internal void RefreshDamage()
	{
		if (MainOrb != null)
		{
			int orbDamage = _level.GameSave.GetOrbDamage(MainOrb.OrbColor);
			MainOrb.UpdateDamage(orbDamage);
		}
		if (SubOrb != null)
		{
			int orbDamage2 = _level.GameSave.GetOrbDamage(SubOrb.OrbColor);
			SubOrb.UpdateDamage(orbDamage2);
		}
	}

	internal void UpdateDeathGlow(float glowBase, Color glowColor)
	{
		if (MainOrb != null)
		{
			MainOrb.IsGlowing = true;
			MainOrb.GlowBase = glowBase;
			MainOrb.GlowColor = glowColor;
		}
		if (SubOrb != null)
		{
			SubOrb.IsGlowing = true;
			SubOrb.GlowBase = glowBase;
			SubOrb.GlowColor = glowColor;
		}
	}

	public void UnhideOrbsOnHit()
	{
		if (MainOrb != null && MainOrb.IsHidden)
		{
			MainOrb.SetOrbHiddenStatus(0f, doesHide: false, doesShowAnimation: true);
		}
		if (SubOrb != null && SubOrb.IsHidden)
		{
			SubOrb.SetOrbHiddenStatus(0f, doesHide: false, doesShowAnimation: true);
		}
	}

	public void ShiftTrailHistory(Point offset)
	{
		if (MainOrb != null)
		{
			MainOrb.ShiftTrailHistory(offset);
		}
		if (SubOrb != null)
		{
			SubOrb.ShiftTrailHistory(offset);
		}
	}
}
