using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Familiars;

namespace Timespinner.GameObjects.Heroes.Passives;

internal class BarrierOrbPassive : LunaisPassive
{
	private bool _isDamageAreaActive;

	private BarrierOrbPassiveDamageArea _damageArea;

	private FamiliarBase _targettedFamiliar;

	public override EInventoryOrbType PassiveType => EInventoryOrbType.Barrier;

	public BarrierOrbPassive(LunaisObj parentLunais)
		: base(parentLunais)
	{
	}

	public override void Update(float delta)
	{
		FamiliarBase equippedFamiliar = base.ParentLunais.EquippedFamiliar;
		if (!_isDamageAreaActive)
		{
			if (equippedFamiliar != null)
			{
				_targettedFamiliar = equippedFamiliar;
				_damageArea = new BarrierOrbPassiveDamageArea(_level, equippedFamiliar.Position, ETeamSide.Heroes, equippedFamiliar);
				_level.AddProjectile(_damageArea);
				_isDamageAreaActive = true;
			}
		}
		else if (equippedFamiliar == null || equippedFamiliar != _targettedFamiliar)
		{
			_damageArea.Kill();
			_isDamageAreaActive = false;
		}
		base.Update(delta);
	}

	public override void ChangeRoom()
	{
		_isDamageAreaActive = false;
		base.ChangeRoom();
	}

	public override void Unequip()
	{
		_isDamageAreaActive = false;
		if (_damageArea != null)
		{
			_damageArea.Kill();
		}
		base.Unequip();
	}
}
