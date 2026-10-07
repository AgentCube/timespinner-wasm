using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Heroes.Passives;

internal class IronOrbPassive : LunaisPassive
{
	private readonly IronOrbPassiveDamageArea _damageArea;

	public override EInventoryOrbType PassiveType => EInventoryOrbType.Iron;

	public IronOrbPassive(LunaisObj parentLunais)
		: base(parentLunais)
	{
		_damageArea = new IronOrbPassiveDamageArea(_level, parentLunais.Position, ETeamSide.Heroes, -1, parentLunais);
		_level.AddProjectile(_damageArea);
	}

	public override void ChangeRoom()
	{
		_level.AddProjectile(_damageArea);
		base.ChangeRoom();
	}

	public override void Unequip()
	{
		_damageArea.Kill();
		base.Unequip();
	}
}
