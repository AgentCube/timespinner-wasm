using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;

namespace Timespinner.GameObjects.Heroes.Spells;

internal class LunaisSpellManager
{
	private const float MaxLunaisAuraSize = 0.2f;

	private static readonly Vector2 MaxAuraOffset = new Vector2(4f, 5f);

	private readonly LunaisObj _parentLunais;

	private readonly Level _level;

	private readonly List<int> _chargeIntervals = new List<int>();

	private bool _isCharging;

	private bool _isReadyToCast;

	private int _chargeSelectLevel;

	private int _spellVariation;

	private float _chargeSelect;

	private float _castedSpellCost;

	private float _minChargeSelect;

	private float _maxChargeSelect;

	private float _aura;

	private float _maxAura;

	private float _chargeSpeed;

	private float _parentAuraSize;

	private EInventoryOrbType _equippedSpellType;

	private LunaisSpell _equippedSpell;

	public bool IsSpellOrbEquipped => _equippedSpellType != EInventoryOrbType.None;

	public bool IsCharging => _isCharging;

	public bool CanCharge => _aura >= _minChargeSelect * AuraCostMultiplier;

	internal bool IsOOM { get; set; }

	internal bool IsCurrentlyMagnetizing
	{
		get
		{
			if (_isCharging)
			{
				return _isReadyToCast;
			}
			return false;
		}
	}

	public EInventoryOrbType EquippedSpellType => _equippedSpellType;

	public int ChargeSelectLevel => _chargeSelectLevel;

	public float Aura
	{
		get
		{
			return _aura;
		}
		set
		{
			_aura = value;
			if (_aura > _maxAura)
			{
				_aura = _maxAura;
			}
		}
	}

	public float MaxAura => _maxAura;

	public float ChargeSelect => _chargeSelect;

	public float ChargeSelectPercentage
	{
		get
		{
			if (!(_maxChargeSelect > 0f))
			{
				return 0f;
			}
			return _chargeSelect / ((AuraCostMultiplier > 0f) ? (_maxChargeSelect * AuraCostMultiplier) : _maxChargeSelect);
		}
	}

	internal float AuraCostMultiplier { get; set; }

	public List<int> ChargeIntervals => _chargeIntervals;

	public LunaisSpell EquippedSpell => _equippedSpell;

	internal bool IsSpellReadyToBeCast
	{
		get
		{
			if (_equippedSpell != null)
			{
				return _equippedSpell.CanCast;
			}
			return false;
		}
	}

	public LunaisSpellManager(LunaisObj parentLunais)
	{
		_parentLunais = parentLunais;
		_level = _parentLunais.Level;
		AuraCostMultiplier = 1f;
	}

	public void CancelCharge()
	{
		_chargeSelect = 0f;
		_chargeSelectLevel = 0;
		_isCharging = false;
	}

	public void ChangeSpell(EInventoryOrbType newSpell)
	{
		if (_equippedSpell != null)
		{
			_equippedSpell.Dispose();
		}
		CancelCharge();
		_equippedSpellType = newSpell;
		_chargeIntervals.Clear();
		_equippedSpell = LunaisSpell.FromSpellType(_equippedSpellType, _level);
		if (_equippedSpell != null)
		{
			_chargeSpeed = _equippedSpell.ChargeSpeed;
			_minChargeSelect = _equippedSpell.ChargeIntervals.First();
			_maxChargeSelect = _equippedSpell.ChargeIntervals.Last();
			_chargeIntervals.AddRange(_equippedSpell.ChargeIntervals);
			RefreshDamage();
		}
		else
		{
			_chargeSpeed = 0f;
			_minChargeSelect = 0f;
			_maxChargeSelect = 1f;
		}
	}

	public void RefreshStats(GameSave inSave)
	{
		CharacterStats characterStats = inSave.CharacterStats;
		_maxAura = characterStats.MaxAura;
		if (characterStats.HP != 0)
		{
			_aura = characterStats.Aura;
		}
		if (_aura > _maxAura)
		{
			_aura = _maxAura;
		}
		if (inSave.Inventory.EquippedSpellOrb != _equippedSpellType)
		{
			ChangeSpell(inSave.Inventory.EquippedSpellOrb);
		}
		else
		{
			RefreshDamage();
		}
	}

	public void Update(float delta)
	{
		if (_parentLunais != null && _parentLunais.AuraRegenRate > 0f)
		{
			float num = _parentLunais.AuraRegenRate;
			if (_parentLunais.IsOnVilete)
			{
				num += 1.25f;
			}
			if (_parentLunais.IsWearingViletianCrown)
			{
				num += 1f;
			}
			_aura += num * delta;
			if (_aura > _maxAura)
			{
				_aura = _maxAura;
			}
		}
		_isReadyToCast = false;
		if (_isCharging)
		{
			float num2 = _maxChargeSelect * AuraCostMultiplier;
			_chargeSelect += (num2 + 5f) * delta;
			if (_chargeSelect >= num2)
			{
				_chargeSelect = num2;
				_isReadyToCast = true;
			}
			if (_chargeSelect > _aura)
			{
				_chargeSelect = _aura;
			}
		}
		if (_parentLunais != null)
		{
			if (_isCharging)
			{
				_parentLunais.DoesDrawAura = true;
				if (_parentAuraSize < 0.2f)
				{
					GrowAura(delta);
				}
			}
			else if (_parentAuraSize > 0f)
			{
				_parentAuraSize -= delta;
			}
			bool flag = _parentAuraSize > 0f;
			_parentLunais.DoesDrawAura = flag;
			if (flag)
			{
				_parentLunais.AuraSize = _parentAuraSize;
				_parentLunais.AuraOffset = MaxAuraOffset * (_parentAuraSize / 0.2f);
			}
		}
		if (_equippedSpell != null && _parentLunais != null)
		{
			_equippedSpell.Update(delta, IsCharging, (int)ChargeSelect, _parentLunais.Bbox.Center);
		}
	}

	internal void Draw(SpriteBatch spriteBatch)
	{
		if (_equippedSpell != null && _equippedSpell.IsDrawn)
		{
			_equippedSpell.Draw(spriteBatch);
		}
	}

	public void ProcessInput(bool isButtonDown, bool wasButtonDown, bool isParentStunned)
	{
		IsOOM = false;
		if (_equippedSpell != null)
		{
			if (isButtonDown && CanCharge)
			{
				if (!wasButtonDown && _chargeSelect > 0f)
				{
					if (isParentStunned)
					{
						CancelCharge();
					}
					else
					{
						CastSpell();
					}
				}
				else if (_isCharging || !wasButtonDown)
				{
					_isCharging = _chargeSelect < MaxAura;
				}
			}
			else
			{
				if (wasButtonDown && _chargeSelect > 0f)
				{
					if (isParentStunned)
					{
						CancelCharge();
					}
					else
					{
						CastSpell();
					}
				}
				else if (!wasButtonDown && !isButtonDown && _chargeSelect > 0f)
				{
					CancelCharge();
				}
				float num = _minChargeSelect * AuraCostMultiplier;
				if (_isCharging && _chargeSelect < num)
				{
					_chargeSelect = 0f;
				}
				if (isButtonDown && !wasButtonDown)
				{
					IsOOM = true;
				}
				_isCharging = false;
			}
		}
		else
		{
			_isCharging = false;
		}
		if (_parentLunais != null && !_parentLunais.DoesDrawAura && _isCharging)
		{
			_parentAuraSize = 0f;
			_parentLunais.AuraColor = EquippedSpell.GetAuraColor();
		}
	}

	internal void ChangeRoom()
	{
		if (_equippedSpell != null)
		{
			_equippedSpell.ChangeRoom();
		}
	}

	private void CastSpell()
	{
		if (_parentLunais.CanCastSpell && IsSpellReadyToBeCast)
		{
			_spellVariation = -1;
			int num = 0;
			foreach (int chargeInterval in _chargeIntervals)
			{
				int num2 = (int)((float)chargeInterval * AuraCostMultiplier);
				if (!(_chargeSelect < (float)num2))
				{
					_spellVariation++;
					num = num2;
					continue;
				}
				break;
			}
			if (_spellVariation > -1)
			{
				_castedSpellCost = num;
				_chargeSelect = 0f;
				_parentLunais.CastSpell(_equippedSpellType, _spellVariation);
			}
			else if (_equippedSpell != null)
			{
				_equippedSpell.CancelSpell();
			}
		}
		else
		{
			_chargeSelect = 0f;
		}
	}

	public bool CreateSpellProjectiles()
	{
		bool result = false;
		if (EquippedSpell != null)
		{
			result = EquippedSpell.CreateSpellProjectiles(_level, _parentLunais, _spellVariation);
		}
		return result;
	}

	public void FinishCastingSpell()
	{
		_aura -= _castedSpellCost;
		if (_aura < 0f)
		{
			_aura = 0f;
		}
	}

	public void DebugFillAura()
	{
		_aura += 10f;
		if (_aura > _maxAura)
		{
			_aura = _maxAura;
		}
	}

	public void HealAura(int amount)
	{
		if (_aura < MaxAura)
		{
			_aura += amount;
			if (_aura > MaxAura)
			{
				_aura = MaxAura;
			}
		}
	}

	internal bool GiveExperience(int enemyID)
	{
		bool result = false;
		if (EquippedSpell != null && _parentLunais != null && EquippedSpell.HitEnemyRegistry.Contains(enemyID))
		{
			EquippedSpell.HitEnemyRegistry.Remove(enemyID);
			if (_level.GameSave.GiveOrbExperience(EquippedSpell.SpellType))
			{
				RefreshDamage();
				result = true;
			}
		}
		return result;
	}

	internal void RefreshDamage()
	{
		if (_parentLunais != null && EquippedSpell != null)
		{
			int orbSpellDamage = _level.GameSave.GetOrbSpellDamage(EquippedSpellType);
			EquippedSpell.UpdateDamage(orbSpellDamage);
		}
	}

	internal void GrowAura(float delta)
	{
		_parentLunais.DoesDrawAura = true;
		_parentAuraSize += delta;
		if (_parentAuraSize > 0.2f)
		{
			_parentAuraSize = 0.2f;
		}
	}

	public int GetFirstChargeIntervalAmount()
	{
		int result = 0;
		if (ChargeIntervals.Count > 0)
		{
			result = ChargeIntervals[0];
		}
		return result;
	}

	public void CancelSpellOnHit()
	{
		if (_equippedSpell != null)
		{
			_equippedSpell.CancelSpell();
		}
	}

	internal void OnEnemyDeath(Point position)
	{
		if (_equippedSpell != null)
		{
			_equippedSpell.OnEnemyDeath(position);
		}
	}

	internal void ReduceAura(float reductionAmount)
	{
		if (_aura > 0f)
		{
			_aura -= reductionAmount;
			if (_aura < 0f)
			{
				_aura = 0f;
			}
		}
	}
}
