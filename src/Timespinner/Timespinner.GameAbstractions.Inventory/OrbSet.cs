namespace Timespinner.GameAbstractions.Inventory;

public class OrbSet
{
	public const int OrbSetCount = 3;

	public EInventoryOrbType MeleeOrbA { get; set; }

	public EInventoryOrbType MeleeOrbB { get; set; }

	public EInventoryOrbType SpellOrb { get; set; }

	public EInventoryOrbType PassiveOrb { get; set; }
}
