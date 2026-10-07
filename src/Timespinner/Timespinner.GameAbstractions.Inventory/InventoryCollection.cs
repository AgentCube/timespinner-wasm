using System.Collections.Generic;

namespace Timespinner.GameAbstractions.Inventory;

public abstract class InventoryCollection<T>
{
	private readonly Dictionary<int, T> _inventory = new Dictionary<int, T>();

	public Dictionary<int, T> Inventory => _inventory;

	public abstract EInventoryCategoryType Type { get; }

	public abstract void AddItem(int item);

	public abstract void RefreshItemNameAndDescriptions();

	public virtual void RemoveItem(int item)
	{
		if (_inventory.ContainsKey(item))
		{
			_inventory.Remove(item);
		}
	}

	public virtual T GetItem(int item)
	{
		T result = default(T);
		if (_inventory.ContainsKey(item))
		{
			return _inventory[item];
		}
		return result;
	}
}
