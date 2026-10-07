namespace Timespinner.GameAbstractions.Inventory;

public class InventoryJournal : InventoryItem
{
	internal const int MaxJournalCount = 11;

	private readonly EInventoryJournalType _journalType;

	public bool IsRead { get; set; }

	public EInventoryJournalType JournalType => _journalType;

	public override EInventoryCategoryType Category => EInventoryCategoryType.Journal;

	public override int Key => (int)JournalType;

	internal EJournalCategoryType JournalCategory => GetJournalCategory(_journalType);

	public InventoryJournal(EInventoryJournalType journalType)
		: base("jou_" + journalType)
	{
		_journalType = journalType;
	}

	internal static EJournalCategoryType GetJournalCategory(EInventoryJournalType journalType)
	{
		if (journalType < EInventoryJournalType.Letter0)
		{
			return EJournalCategoryType.Memories;
		}
		if (journalType < EInventoryJournalType.File0)
		{
			return EJournalCategoryType.Letters;
		}
		return EJournalCategoryType.Files;
	}
}
