using System;

namespace Timespinner.Core.Specifications;

[Serializable]
public class BestiaryItemDropSpecification
{
	public int Category { get; set; }

	public int Item { get; set; }

	public int DropRate { get; set; }

	public BestiaryItemDropSpecification Duplicate()
	{
		BestiaryItemDropSpecification bestiaryItemDropSpecification = new BestiaryItemDropSpecification();
		bestiaryItemDropSpecification.Category = Category;
		bestiaryItemDropSpecification.Item = Item;
		bestiaryItemDropSpecification.DropRate = DropRate;
		return bestiaryItemDropSpecification;
	}
}
