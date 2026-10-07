using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

namespace Timespinner.GameStateManagement.Screens.PauseMenu.Journal;

internal class BestiaryScreenItemDrop
{
	private const int NameMaxWidth = 112;

	private readonly bool _isKnown;

	private readonly int _starCount;

	private readonly ScrollableTextBlock _nameTextBlock;

	internal bool IsKnown => _isKnown;

	internal int StarCount => _starCount;

	internal ScrollableTextBlock NameTextBlock => _nameTextBlock;

	internal BestiaryScreenItemDrop(BestiaryEntrySpecification bestiaryEntry, BestiaryItemDropSpecification spec, int itemIndex, int playerLuck, GameSave gameSave, SpriteFont font, Vector2 topLeft)
	{
		_isKnown = gameSave.GetSaveBool(Monster.GetItemDropKeyFromTypeArgumentAndIndex(bestiaryEntry.EnemyType, bestiaryEntry.EnemyArgument, itemIndex));
		_starCount = GetStarCountFromLuckAndDropRate(spec.DropRate, playerLuck);
		EInventoryCategoryType category = (EInventoryCategoryType)spec.Category;
		_nameTextBlock = new ScrollableTextBlock(font, 112, topLeft, isTextCentered: false);
		_nameTextBlock.SetText((category == EInventoryCategoryType.UseItem) ? InventoryItem.NameFromType((EInventoryUseItemType)spec.Item) : InventoryItem.NameFromType((EInventoryEquipmentType)spec.Item));
	}

	private static int GetStarCountFromLuckAndDropRate(int dropRate, int playerLuck)
	{
		float num = (float)playerLuck * 0.05f;
		float num2 = (float)dropRate + num;
		if (num2 >= 12f)
		{
			return 1;
		}
		if (num2 >= 9f)
		{
			return 2;
		}
		if (num2 >= 6f)
		{
			return 3;
		}
		if (num2 >= 3f)
		{
			return 4;
		}
		return 5;
	}
}
