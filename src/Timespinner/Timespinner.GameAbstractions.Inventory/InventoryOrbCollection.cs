namespace Timespinner.GameAbstractions.Inventory;

public class InventoryOrbCollection : InventoryCollection<InventoryOrb>
{
	public override EInventoryCategoryType Type => EInventoryCategoryType.Orb;

	public override void AddItem(int item)
	{
		AddItem(item, EOrbSlot.Melee);
	}

	internal void AddItem(int item, EOrbSlot slot)
	{
		InventoryOrb inventoryOrb;
		if (!base.Inventory.ContainsKey(item))
		{
			inventoryOrb = new InventoryOrb((EInventoryOrbType)item);
			base.Inventory.Add(item, inventoryOrb);
		}
		else
		{
			inventoryOrb = base.Inventory[item];
		}
		if (inventoryOrb != null)
		{
			switch (slot)
			{
			case EOrbSlot.Passive:
				inventoryOrb.IsPassiveUnlocked = true;
				break;
			case EOrbSlot.Spell:
				inventoryOrb.IsSpellUnlocked = true;
				break;
			case EOrbSlot.All:
				inventoryOrb.IsPassiveUnlocked = true;
				inventoryOrb.IsSpellUnlocked = true;
				break;
			}
		}
	}

	public bool GiveOrbExperience(EInventoryOrbType orbColor, bool isDoublingExp)
	{
		bool flag = false;
		if (base.Inventory.ContainsKey((int)orbColor))
		{
			flag = base.Inventory[(int)orbColor].GiveExperience();
			if (isDoublingExp)
			{
				flag = base.Inventory[(int)orbColor].GiveExperience() || flag;
			}
		}
		return flag;
	}

	internal bool GiveOrbInfusionExperience(EInventoryOrbType orbColor)
	{
		bool result = false;
		if (base.Inventory.ContainsKey((int)orbColor))
		{
			result = base.Inventory[(int)orbColor].GiveInfusionExperience();
		}
		return result;
	}

	public void InitializePostLoad()
	{
		foreach (InventoryOrb value in base.Inventory.Values)
		{
			value.RefreshOrbLevel();
		}
	}

	internal int GetOrbDamage(EInventoryOrbType orbColor, EOrbSlot slot, int willpower)
	{
		int result = 0;
		if (base.Inventory.ContainsKey((int)orbColor))
		{
			InventoryOrb inventoryOrb = base.Inventory[(int)orbColor];
			result = slot switch
			{
				EOrbSlot.Spell => inventoryOrb.GetSpellDamage(willpower), 
				EOrbSlot.Passive => inventoryOrb.GetPassiveDamage(willpower), 
				_ => inventoryOrb.GetMeleeDamage(willpower), 
			};
		}
		return result;
	}

	public override void RefreshItemNameAndDescriptions()
	{
		foreach (InventoryOrb value in base.Inventory.Values)
		{
			value.RefreshNameAndDescription();
		}
	}
}
