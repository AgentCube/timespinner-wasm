namespace Timespinner.GameAbstractions.Inventory;

public class InventoryRelic : InventoryItem
{
	private readonly EInventoryRelicType _relicType;

	public bool IsActive { get; set; }

	public EInventoryRelicType RelicType => _relicType;

	public override EInventoryCategoryType Category => EInventoryCategoryType.Relic;

	public override int Key => (int)RelicType;

	public InventoryRelic(EInventoryRelicType relicType)
		: base("rel_" + relicType)
	{
		_relicType = relicType;
		base.MonetaryValue = GetMonetaryValue(_relicType);
	}

	private static int GetMonetaryValue(EInventoryRelicType relicType)
	{
		int result = 0;
		if (relicType == EInventoryRelicType.ScienceKeycardD)
		{
			result = 25;
		}
		return result;
	}
}
