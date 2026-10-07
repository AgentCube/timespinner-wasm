namespace Timespinner.GameAbstractions.Inventory;

public class InventoryJournalCollection : InventoryCollection<InventoryJournal>
{
	public override EInventoryCategoryType Type => EInventoryCategoryType.Journal;

	public override void AddItem(int item)
	{
		if (!base.Inventory.ContainsKey(item))
		{
			InventoryJournal value = new InventoryJournal((EInventoryJournalType)item);
			base.Inventory.Add(item, value);
		}
	}

	public bool IsJournalRead(EInventoryJournalType type)
	{
		if (base.Inventory.ContainsKey((int)type))
		{
			return base.Inventory[(int)type].IsRead;
		}
		return false;
	}

	public override void RefreshItemNameAndDescriptions()
	{
		foreach (InventoryJournal value in base.Inventory.Values)
		{
			value.RefreshNameAndDescription();
		}
	}

	internal bool AreAllEntriesInCategoryFound(EJournalCategoryType category)
	{
		bool flag = true;
		switch (category)
		{
		case EJournalCategoryType.Memories:
		{
			for (int k = 0; k <= 10; k++)
			{
				if (!base.Inventory.ContainsKey(k))
				{
					flag = false;
					break;
				}
			}
			break;
		}
		case EJournalCategoryType.Letters:
		{
			for (int l = 32; l <= 42; l++)
			{
				if (!base.Inventory.ContainsKey(l))
				{
					flag = false;
					break;
				}
			}
			break;
		}
		case EJournalCategoryType.Files:
		{
			for (int i = 64; i <= 71; i++)
			{
				if (!base.Inventory.ContainsKey(i))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				flag = base.Inventory.ContainsKey(74);
			}
			if (!flag)
			{
				break;
			}
			for (int j = 80; j <= 84; j++)
			{
				if (!base.Inventory.ContainsKey(j))
				{
					flag = false;
					break;
				}
			}
			break;
		}
		}
		return flag;
	}
}
