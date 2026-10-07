using Timespinner.GameAbstractions.Inventory;

namespace Timespinner.GameObjects.Heroes.Passives;

internal class UmbraOrbPassive : LunaisPassive
{
	public override EInventoryOrbType PassiveType => EInventoryOrbType.Umbra;

	public UmbraOrbPassive(LunaisObj parentLunais)
		: base(parentLunais)
	{
	}

	internal override void OnEnemyDeath()
	{
		int orbPassiveDamage = _level.GameSave.GetOrbPassiveDamage(EInventoryOrbType.Umbra);
		base.ParentLunais.ManageHeal(orbPassiveDamage, shouldShowAnimation: false);
	}
}
