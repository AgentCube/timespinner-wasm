using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.Passives;

internal class LunaisPassiveManager
{
	private readonly LunaisObj _parentLunais;

	private readonly Level _level;

	private bool _isHidingPassive;

	private EInventoryOrbType _equippedPassiveType;

	private LunaisPassive _equippedPassive;

	public EInventoryOrbType EquippedPassiveType => _equippedPassiveType;

	public LunaisPassiveManager(LunaisObj parentLunais)
	{
		_parentLunais = parentLunais;
		_level = _parentLunais.Level;
	}

	public void ChangePassive(EInventoryOrbType newPassive)
	{
		if (_equippedPassive != null)
		{
			_equippedPassive.Unequip();
		}
		_equippedPassiveType = newPassive;
		_equippedPassive = LunaisPassive.FromPassiveType(_equippedPassiveType, _parentLunais);
	}

	public void RefreshStats(GameSave inSave)
	{
		if (inSave.Inventory.EquippedPassiveOrb != _equippedPassiveType)
		{
			ChangePassive(inSave.Inventory.EquippedPassiveOrb);
		}
		else if (_equippedPassive != null)
		{
			_equippedPassive.OnRefreshStats(inSave);
		}
	}

	internal bool GiveExperience()
	{
		bool result = false;
		if (_equippedPassive != null && _parentLunais != null && _level.GameSave.GiveOrbExperience(_equippedPassive.PassiveType))
		{
			result = true;
		}
		return result;
	}

	public void ChangeRoom()
	{
		if (_equippedPassive != null)
		{
			_equippedPassive.ChangeRoom();
		}
	}

	public void Update(float delta)
	{
		if (_equippedPassive != null)
		{
			_equippedPassive.Update(delta);
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		if (_equippedPassive != null && !_isHidingPassive)
		{
			_equippedPassive.Draw(spriteBatch);
		}
	}

	public void DrawUnderOrb(SpriteBatch spriteBatch, bool isMainOrb)
	{
		if (_equippedPassive != null && !_isHidingPassive)
		{
			_equippedPassive.DrawUnderOrb(spriteBatch, isMainOrb);
		}
	}

	public int ManageDamage(int power, EDamageType type)
	{
		int result = power;
		if (_equippedPassive != null)
		{
			result = _equippedPassive.ManageDamage(power, type);
		}
		return result;
	}

	internal virtual void OnMeleeEnemyContact(Alive enemy, LunaisBaseOrbDamageArea damageArea, Rectangle contactBBox)
	{
		if (_equippedPassive != null)
		{
			_equippedPassive.OnMeleeEnemyContact(enemy, damageArea, contactBBox);
		}
	}

	internal void OnSuccessfulMeleeEnemyHit(Alive enemy)
	{
		if (_equippedPassive != null)
		{
			_equippedPassive.OnSuccessfulMeleeEnemyHit(enemy);
		}
	}

	public void OnAttackWhenAllOrbsAreBusy()
	{
		if (_equippedPassive != null)
		{
			_equippedPassive.OnAttackWhenAllOrbsAreBusy();
		}
	}

	internal void OnEnemyDeath()
	{
		if (_equippedPassive != null)
		{
			_equippedPassive.OnEnemyDeath();
		}
	}

	internal void SetHiddenStatus(bool shouldHide)
	{
		_isHidingPassive = shouldHide;
	}

	public void OnTeleportToPoint(Point newPosition)
	{
		if (_equippedPassive != null)
		{
			_equippedPassive.OnTeleportToPoint(newPosition);
		}
	}

	public void ShiftTrailHistory(Point offset)
	{
		if (_equippedPassive != null)
		{
			_equippedPassive.ShiftTrailHistory(offset);
		}
	}
}
