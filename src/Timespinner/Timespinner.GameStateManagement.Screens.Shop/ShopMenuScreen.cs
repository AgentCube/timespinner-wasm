using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

namespace Timespinner.GameStateManagement.Screens.Shop;

internal class ShopMenuScreen : InventoryMenuScreen
{
	private const int MaxItemQuantity = 9;

	private const float DisplayStatsPositionRatioX = 0.55f;

	private const float DisplayStatsPositionRatioY = 5f / 32f;

	private const float DisplayStatsWidthRatio = 0.3f;

	private const float StatsBackgroundDrawOffsetY = 7f / 64f;

	private const float StatsBackgroundHeightRatio = 83f / 192f;

	private const float StockBorderDrawPositionRatioX = 0.06f;

	private const float StockBorderDrawWidthRatio = 0.3f;

	private const float MoneyBorderDrawPositionRatioX = 0.64f;

	private const float MoneyBorderDrawWidthRatio = 0.3f;

	private const float StockMoneyBorderDrawPositionRatioY = 0.74583334f;

	private const float StockMoneyTextDrawPositionRatioY = 11f / 15f;

	private const float StockMoneyTextDrawPositionRatioX = 0.125f;

	private const float StockMoneyTextDrawWidthRatio = 0.78f;

	private const string NoStockString = "--";

	private readonly bool _isBuying;

	private readonly float _shopPriceModifier;

	private readonly string _stockPrefix;

	private readonly MerchantInventory _merchandiseInventory;

	private readonly MenuEntry _meleeMenuEntry;

	private readonly MenuEntry _useItemMenuEntry;

	private readonly MenuEntry _equipmentMenuEntry;

	private readonly MenuEntry _relicMenuEntry;

	private readonly ShopMenuEntryCollection _equipmentMenuCollection;

	private readonly ConfirmationMenuEntryCollection _yesNoCollection;

	private readonly StatCollection _selectedItemStats = new StatCollection();

	private readonly StatCollection _playerInventoryStats = new StatCollection();

	private readonly IList<ShopMenuEntryCollection> _categoryMenuCollections = new List<ShopMenuEntryCollection>();

	private int _stockBorderDrawPositionX;

	private int _moneyBorderDrawPositionX;

	private int _stockMoneyDrawPositionY;

	private int _stockBorderDrawWidth;

	private int _moneyBorderDrawWidth;

	private Rectangle _menuBackgroundDrawRectangle;

	private Rectangle _statsBackgroundDrawRectangle;

	private string _lastSelectedInventoryItemStockCount = "--";

	private InventoryItem _lastSelectedEquipment;

	private Action _buySellSelectedItem;

	public ShopMenuScreen(GameSave inSave, GCM gcm, MerchantInventory merchandiseInventory, bool isBuying, Action fullExitAction)
		: base(isBuying ? Loc.Get("shop_buy") : Loc.Get("shop_sell"), inSave, gcm, fullExitAction)
	{
		_merchandiseInventory = merchandiseInventory;
		_isBuying = isBuying;
		_shopPriceModifier = (_isBuying ? 1f : 0.75f);
		base.DoesDrawBrackets = true;
		base.DoesDrawBracketsOverAll = false;
		base.DoesDrawTopLowerFrame = true;
		base.DoesDrawScrollbarWidget = true;
		base.StatCollections.Add(_selectedItemStats);
		base.StatCollections.Add(_playerInventoryStats);
		_stockPrefix = Loc.Get("shop_stock");
		bool flag = _merchandiseInventory.OrbInventory.Inventory.Count > 0;
		bool flag2 = _merchandiseInventory.UseItemInventory.Inventory.Count > 0;
		bool flag3 = _merchandiseInventory.EquipmentInventory.Inventory.Count > 0;
		if (_merchandiseInventory.RelicInventory.Inventory.Count > 0)
		{
			bool flag4 = false;
			foreach (int key in _merchandiseInventory.RelicInventory.Inventory.Keys)
			{
				if (!base.SaveFile.Inventory.RelicInventory.Inventory.ContainsKey(key))
				{
					flag4 = true;
				}
			}
			if (flag4)
			{
				string description = (isBuying ? Loc.Get("shop_buy_relics_desc") : Loc.Get("shop_sell_relics_desc"));
				_relicMenuEntry = new MenuEntry(Loc.Get("shop_relics_header"))
				{
					Description = description
				};
				_relicMenuEntry.Selected += ItemMenuEntrySelected;
				base.MenuEntries.Add(_relicMenuEntry);
				ShopMenuEntryCollection shopMenuEntryCollection = new ShopMenuEntryCollection(_shopPriceModifier, OnItemSelected, base.GCM.SpPauseMenu, _isBuying);
				shopMenuEntryCollection.AddEntries(_merchandiseInventory.RelicInventory.Inventory.Values, EInventoryCategoryType.Relic);
				_categoryMenuCollections.Add(shopMenuEntryCollection);
				_subMenuCollections.Add(shopMenuEntryCollection);
			}
		}
		if (flag)
		{
			_meleeMenuEntry = new MenuEntry(Loc.Get("shop_orbs_header"))
			{
				Description = Loc.Get("shop_buy_orbs_desc")
			};
			_meleeMenuEntry.Selected += ItemMenuEntrySelected;
			base.MenuEntries.Add(_meleeMenuEntry);
			ShopMenuEntryCollection shopMenuEntryCollection2 = new ShopMenuEntryCollection(_shopPriceModifier, OnItemSelected, base.GCM.SpPauseMenu, _isBuying);
			shopMenuEntryCollection2.AddEntries(_merchandiseInventory.OrbInventory.Inventory.Values, EInventoryCategoryType.Orb);
			_categoryMenuCollections.Add(shopMenuEntryCollection2);
			_subMenuCollections.Add(shopMenuEntryCollection2);
		}
		if (flag3)
		{
			string description2 = (isBuying ? Loc.Get("shop_buy_equipment_desc") : Loc.Get("shop_sell_equipment_desc"));
			_equipmentMenuEntry = new MenuEntry(Loc.Get("shop_equipment_header"))
			{
				Description = description2
			};
			_equipmentMenuEntry.Selected += ItemMenuEntrySelected;
			base.MenuEntries.Add(_equipmentMenuEntry);
			_equipmentMenuCollection = new ShopMenuEntryCollection(_shopPriceModifier, OnItemSelected, base.GCM.SpPauseMenu, _isBuying);
			_equipmentMenuCollection.AddEntries(_merchandiseInventory.EquipmentInventory.Inventory.Values, EInventoryCategoryType.Equipment);
			_categoryMenuCollections.Add(_equipmentMenuCollection);
			_subMenuCollections.Add(_equipmentMenuCollection);
		}
		if (flag2)
		{
			string description3 = (isBuying ? Loc.Get("shop_buy_use_item_desc") : Loc.Get("shop_sell_use_item_desc"));
			_useItemMenuEntry = new MenuEntry(Loc.Get("shop_use_item_header"))
			{
				Description = description3
			};
			_useItemMenuEntry.Selected += ItemMenuEntrySelected;
			base.MenuEntries.Add(_useItemMenuEntry);
			ShopMenuEntryCollection shopMenuEntryCollection3 = new ShopMenuEntryCollection(_shopPriceModifier, OnItemSelected, base.GCM.SpPauseMenu, _isBuying);
			shopMenuEntryCollection3.AddEntries(_merchandiseInventory.UseItemInventory.Inventory.Values, EInventoryCategoryType.UseItem);
			_categoryMenuCollections.Add(shopMenuEntryCollection3);
			_subMenuCollections.Add(shopMenuEntryCollection3);
		}
		string promptString = (isBuying ? Loc.Get("shop_buy_confirm") : Loc.Get("shop_sell_confirm"));
		_yesNoCollection = new ConfirmationMenuEntryCollection(Loc.Get("shop_buy_yes"), Loc.Get("shop_buy_no"), promptString, OnConfirmBuy, base.OnCancel)
		{
			ScrollRowHeight = 1
		};
		_subMenuCollections.Add(_yesNoCollection);
	}

	internal override void RefreshSizes()
	{
		base.RefreshSizes();
		int num = (int)(7f / 64f * (float)_topSectionHeight);
		_menuBackgroundDrawRectangle = new Rectangle(_screenLeft + 3 * base.Zoom, _screenTop + num, (int)((float)_screenWidth / 2f - (float)(3 * base.Zoom)), (int)(83f / 192f * (float)_topSectionHeight));
		_statsBackgroundDrawRectangle = new Rectangle(_menuBackgroundDrawRectangle.Right, _menuBackgroundDrawRectangle.Top, _menuBackgroundDrawRectangle.Width, _menuBackgroundDrawRectangle.Height);
		_primaryMenuCollection.SetColumnWidth(base.NarrowListColumnWidth, base.Zoom);
		int num2 = (Loc.IsAsianLocale ? (-2) : 0);
		_selectedItemStats.Location = new Vector2(0.55f * (float)_screenWidth + (float)_screenLeft, (float)_screenTop + 5f / 32f * (float)_topSectionHeight + (float)(num2 * base.Zoom));
		_selectedItemStats.Width = (int)(0.3f * (float)_screenWidth);
		_stockBorderDrawPositionX = _screenLeft + (int)((float)_screenWidth * 0.06f);
		_moneyBorderDrawPositionX = _screenLeft + (int)((float)_screenWidth * 0.64f);
		_stockMoneyDrawPositionY = _screenTop + (int)(0.74583334f * (float)(_topSectionHeight + _bottomSectionHeight));
		_stockBorderDrawWidth = (int)((float)_screenWidth * 0.3f);
		_moneyBorderDrawWidth = (int)((float)_screenWidth * 0.3f);
		_playerInventoryStats.Location = new Vector2((float)_screenLeft + (float)_screenWidth * 0.125f, (float)_screenTop + 11f / 15f * (float)(_topSectionHeight + _bottomSectionHeight));
		_playerInventoryStats.Width = (int)((float)_screenWidth * 0.78f);
		foreach (ShopMenuEntryCollection categoryMenuCollection in _categoryMenuCollections)
		{
			categoryMenuCollection.DrawPosition = base.ListTextDrawPosition;
			categoryMenuCollection.SetColumnWidth(base.ListColumnWidth, base.Zoom);
		}
		Vector2 drawPosition = new Vector2((float)(int)base.DescriptionDrawPosition.X + (float)_screenWidth * 0.125f, (int)(base.DescriptionDrawPosition.Y + (float)_bottomSectionHeight * 0.5f));
		_yesNoCollection.DrawPosition = drawPosition;
		_yesNoCollection.SetColumnWidth(base.ListColumnWidth, base.Zoom);
		for (int num3 = _categoryMenuCollections.Count - 1; num3 >= 0; num3--)
		{
			_categoryMenuCollections[num3].RefreshSizes();
		}
		RefreshDisplayStats(isResize: true);
	}

	internal override bool MoveSelectedInDirection(EMenuMoveDirection direction, PlayerIndex? playerIndex)
	{
		if (_selectedMenuCollection != _yesNoCollection && (direction == EMenuMoveDirection.Left || direction == EMenuMoveDirection.Right))
		{
			return AdjustItemQuantity(direction);
		}
		return base.MoveSelectedInDirection(direction, playerIndex);
	}

	private bool AdjustItemQuantity(EMenuMoveDirection direction)
	{
		bool result = false;
		ShopMenuEntryCollection selectedCategory = GetSelectedCategory();
		if (selectedCategory != null)
		{
			int num = 0;
			switch (direction)
			{
			case EMenuMoveDirection.Right:
				num = 1;
				break;
			case EMenuMoveDirection.Left:
				num = -1;
				break;
			}
			if (num != 0)
			{
				ShopMenuEntry selectedShopEntry = selectedCategory.GetSelectedShopEntry();
				if (selectedShopEntry != null)
				{
					int availableQuantityByItem = GetAvailableQuantityByItem(selectedShopEntry.Item);
					int num2 = selectedShopEntry.QuanityToBuy + num;
					if (num2 > 0 && num2 <= availableQuantityByItem)
					{
						selectedShopEntry.QuanityToBuy = num2;
						result = true;
					}
				}
			}
		}
		return result;
	}

	private ShopMenuEntryCollection GetSelectedCategory()
	{
		ShopMenuEntryCollection result = null;
		foreach (ShopMenuEntryCollection categoryMenuCollection in _categoryMenuCollections)
		{
			if (_selectedMenuCollection == categoryMenuCollection)
			{
				result = categoryMenuCollection;
				break;
			}
		}
		return result;
	}

	private int GetAvailableQuantityByItem(InventoryItem item)
	{
		int result = 9;
		switch (item.Category)
		{
		case EInventoryCategoryType.Orb:
			result = 1;
			break;
		case EInventoryCategoryType.UseItem:
			if (base.SaveFile.Inventory.UseItemInventory.Inventory.ContainsKey(item.Key))
			{
				int count = base.SaveFile.Inventory.UseItemInventory.Inventory[item.Key].Count;
				result = (_isBuying ? (9 - count) : count);
			}
			else if (!_isBuying)
			{
				result = 0;
			}
			break;
		case EInventoryCategoryType.Equipment:
			if (base.SaveFile.Inventory.EquipmentInventory.Inventory.ContainsKey(item.Key))
			{
				int count2 = base.SaveFile.Inventory.EquipmentInventory.Inventory[item.Key].Count;
				result = (_isBuying ? (9 - count2) : count2);
			}
			else if (!_isBuying)
			{
				result = 0;
			}
			break;
		case EInventoryCategoryType.Relic:
			result = ((!base.SaveFile.Inventory.RelicInventory.Inventory.ContainsKey(item.Key)) ? 1 : 0);
			break;
		}
		return result;
	}

	private void ItemMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		int count = _categoryMenuCollections.Count;
		if (count > base.SelectedIndex)
		{
			ShopMenuEntryCollection shopMenuEntryCollection = _categoryMenuCollections[base.SelectedIndex];
			if (shopMenuEntryCollection.Entries.Count > 0)
			{
				ChangeMenuCollection(shopMenuEntryCollection, shouldPush: true);
			}
			else
			{
				base.ScreenManager.Jukebox.PlayCue(ESFX.MenuError);
			}
		}
	}

	private void OnItemSelected(ShopMenuEntry item)
	{
		if (!CanBuy(item))
		{
			return;
		}
		if (_isBuying)
		{
			_buySellSelectedItem = delegate
			{
				AddItemToInventory(item);
				SubtractMoney(item);
				item.QuanityToBuy = 1;
			};
		}
		else
		{
			_buySellSelectedItem = delegate
			{
				RemoveItemFromInventory(item);
				AddMoney(item);
				item.QuanityToBuy = 1;
			};
		}
		ShowConfirmationPrompt();
	}

	private void AddItemToInventory(ShopMenuEntry itemEntry)
	{
		switch (itemEntry.ItemType)
		{
		case EInventoryCategoryType.Orb:
			base.SaveFile.Inventory.OrbInventory.AddItem(itemEntry.Item.Key, itemEntry.OrbSlot);
			break;
		case EInventoryCategoryType.UseItem:
			base.SaveFile.Inventory.UseItemInventory.AddItem(itemEntry.Item.Key, itemEntry.QuanityToBuy);
			break;
		case EInventoryCategoryType.Equipment:
			base.SaveFile.Inventory.EquipmentInventory.AddItem(itemEntry.Item.Key, itemEntry.QuanityToBuy);
			break;
		case EInventoryCategoryType.Relic:
			base.SaveFile.Inventory.RelicInventory.AddItem(itemEntry.Item.Key);
			break;
		case EInventoryCategoryType.Familiar:
		case EInventoryCategoryType.Journal:
			break;
		}
	}

	private void RemoveItemFromInventory(ShopMenuEntry itemEntry)
	{
		switch (itemEntry.ItemType)
		{
		case EInventoryCategoryType.UseItem:
			base.SaveFile.Inventory.UseItemInventory.RemoveItem(itemEntry.Item.Key, itemEntry.QuanityToBuy);
			break;
		case EInventoryCategoryType.Equipment:
			base.SaveFile.Inventory.EquipmentInventory.RemoveItem(itemEntry.Item.Key, itemEntry.QuanityToBuy);
			break;
		}
	}

	private bool CanBuy(ShopMenuEntry itemEntry)
	{
		bool flag;
		if (_isBuying)
		{
			int totalPrice = itemEntry.TotalPrice;
			int availableQuantityByItem = GetAvailableQuantityByItem(itemEntry.Item);
			flag = base.SaveFile.Money >= totalPrice && availableQuantityByItem > 0;
			if (!flag)
			{
				PlayErrorSound();
				if (availableQuantityByItem > 0)
				{
					ChangeDescription(Loc.Get("shop_buy_no_money"), EInventoryItemIcon.None);
				}
			}
		}
		else
		{
			flag = itemEntry.Item.IsSellable;
			if (itemEntry.ItemType == EInventoryCategoryType.Equipment)
			{
				int availableQuantityByItem2 = GetAvailableQuantityByItem(itemEntry.Item);
				int equipmentEquippedCount = base.SaveFile.Inventory.GetEquipmentEquippedCount(itemEntry.Item.Key);
				if (availableQuantityByItem2 - equipmentEquippedCount < itemEntry.QuanityToBuy)
				{
					flag = false;
					PlayErrorSound();
					ChangeDescription(Loc.Get("shop_buy_already_wearing"), EInventoryItemIcon.None);
				}
			}
		}
		return flag;
	}

	private void SubtractMoney(ShopMenuEntry itemEntry)
	{
		base.SaveFile.Money -= itemEntry.TotalPrice;
		if (base.SaveFile.Money < 0)
		{
			base.SaveFile.Money = 0;
		}
	}

	private void AddMoney(ShopMenuEntry itemEntry)
	{
		base.SaveFile.Money += itemEntry.TotalPrice;
	}

	private void ShowConfirmationPrompt()
	{
		_yesNoCollection.IsVisible = true;
		ChangeMenuCollection(_yesNoCollection, shouldPush: true);
		base.ScreenManager.Jukebox.PlayCue(ESFX.MenuSelect);
	}

	private void OnConfirmBuy(object sender, PlayerIndexEventArgs e)
	{
		if (_buySellSelectedItem != null)
		{
			_buySellSelectedItem();
			_yesNoCollection.IsVisible = false;
			GoToPreviousMenuCollection();
			OnBuyItem();
		}
	}

	private void OnBuyItem()
	{
		base.ScreenManager.Jukebox.PlayCue(_isBuying ? ESFX.MenuBuy : ESFX.MenuSell);
	}

	protected override void OnSelectedEntryChanged(int entryIndex)
	{
		base.OnSelectedEntryChanged(entryIndex);
		if (_selectedMenuCollection != _yesNoCollection)
		{
			int num = 0;
			foreach (ShopMenuEntryCollection categoryMenuCollection in _categoryMenuCollections)
			{
				categoryMenuCollection.IsVisible = _selectedMenuCollection == categoryMenuCollection || (_selectedMenuCollection == _primaryMenuCollection && entryIndex == num);
				num++;
			}
			ShopMenuEntryCollection selectedCategory = GetSelectedCategory();
			if (selectedCategory != null)
			{
				if (!_isBuying)
				{
					for (int num2 = selectedCategory.Entries.Count - 1; num2 >= 0; num2--)
					{
						InventoryItem item = selectedCategory.Items[num2].Item;
						int availableQuantityByItem = GetAvailableQuantityByItem(item);
						if (availableQuantityByItem <= 0)
						{
							selectedCategory.RemoveItemAt(num2);
							if (selectedCategory.Entries.Count > 0)
							{
								base.OnSelectedEntryChanged(selectedCategory.SelectedIndex);
							}
							else
							{
								ChangeMenuCollection(_primaryMenuCollection, shouldPush: false);
							}
						}
					}
				}
				InventoryItem selectedItem = selectedCategory.GetSelectedItem();
				if (selectedItem != null)
				{
					string arg = "0";
					switch (selectedItem.Category)
					{
					case EInventoryCategoryType.Orb:
						arg = (base.SaveFile.Inventory.OrbInventory.Inventory.ContainsKey(selectedItem.Key) ? "1" : "0");
						break;
					case EInventoryCategoryType.UseItem:
						if (base.SaveFile.Inventory.UseItemInventory.Inventory.ContainsKey(selectedItem.Key))
						{
							arg = base.SaveFile.Inventory.UseItemInventory.Inventory[selectedItem.Key].Count.ToString(CultureInfo.InvariantCulture);
						}
						break;
					case EInventoryCategoryType.Equipment:
						if (base.SaveFile.Inventory.EquipmentInventory.Inventory.ContainsKey(selectedItem.Key))
						{
							arg = base.SaveFile.Inventory.EquipmentInventory.Inventory[selectedItem.Key].Count.ToString(CultureInfo.InvariantCulture);
						}
						break;
					case EInventoryCategoryType.Relic:
						if (base.SaveFile.Inventory.RelicInventory.Inventory.ContainsKey(selectedItem.Key))
						{
							arg = "1";
							ShopMenuEntry selectedShopEntry = selectedCategory.GetSelectedShopEntry();
							if (selectedShopEntry != null)
							{
								selectedShopEntry.IsAvailable = false;
							}
						}
						else
						{
							arg = "0";
						}
						break;
					}
					_lastSelectedInventoryItemStockCount = $"{_stockPrefix} {arg}";
				}
			}
			else if (_selectedMenuCollection == _primaryMenuCollection)
			{
				_lastSelectedInventoryItemStockCount = string.Format("{0} {1}", _stockPrefix, "--");
			}
		}
		RefreshDisplayStats(isResize: false);
	}

	private void RefreshDisplayStats(bool isResize)
	{
		if (_selectedMenuCollection == _yesNoCollection && !isResize)
		{
			return;
		}
		_selectedItemStats.Entries.Clear();
		_playerInventoryStats.Entries.Clear();
		if (_selectedMenuCollection == _equipmentMenuCollection || (isResize && _lastSelectedEquipment != null))
		{
			_lastSelectedEquipment = _equipmentMenuCollection.GetSelectedItem();
			AddStatEntries(_lastSelectedEquipment);
		}
		else
		{
			_lastSelectedEquipment = null;
		}
		_playerInventoryStats.Entries.Add(new StatEntry
		{
			Title = _lastSelectedInventoryItemStockCount,
			Type = StatEntry.EStatDisplayType.NumberWithIcon,
			Value = base.SaveFile.Money,
			IconIndex = 0
		});
		foreach (ShopMenuEntryCollection categoryMenuCollection in _categoryMenuCollections)
		{
			categoryMenuCollection.UpdateItems(base.SaveFile.Money);
		}
	}

	private void AddStatEntries(InventoryItem item)
	{
		if (item != null)
		{
			CharacterStats characterStats = base.SaveFile.CharacterStats;
			EInventoryOrbType equippedMeleeOrbA = base.SaveFile.Inventory.EquippedMeleeOrbA;
			int orbDamage = base.SaveFile.GetOrbDamage(equippedMeleeOrbA);
			StatEntry statEntry = new StatEntry();
			statEntry.Title = Loc.Get("StatDamageAbbreviation");
			statEntry.Type = StatEntry.EStatDisplayType.NumberComparison;
			statEntry.Value = orbDamage;
			StatEntry statEntry2 = statEntry;
			StatEntry statEntry3 = new StatEntry();
			statEntry3.Title = Loc.Get("StatDefenseAbbreviation");
			statEntry3.Type = StatEntry.EStatDisplayType.NumberComparison;
			statEntry3.Value = characterStats.Defense;
			StatEntry statEntry4 = statEntry3;
			StatEntry statEntry5 = new StatEntry();
			statEntry5.Title = Loc.Get("StatWillpowerAbbreviation");
			statEntry5.Type = StatEntry.EStatDisplayType.NumberComparison;
			statEntry5.Value = characterStats.Willpower;
			StatEntry statEntry6 = statEntry5;
			StatEntry statEntry7 = new StatEntry();
			statEntry7.Title = Loc.Get("StatFortitudeAbbreviation");
			statEntry7.Type = StatEntry.EStatDisplayType.NumberComparison;
			statEntry7.Value = characterStats.Fortitude;
			StatEntry statEntry8 = statEntry7;
			StatEntry statEntry9 = new StatEntry();
			statEntry9.Title = Loc.Get("StatLuckAbbreviation");
			statEntry9.Type = StatEntry.EStatDisplayType.NumberComparison;
			statEntry9.Value = characterStats.Luck;
			StatEntry statEntry10 = statEntry9;
			EInventoryEquipmentType key = (EInventoryEquipmentType)item.Key;
			int slotType = (int)InventoryEquipment.GetSlotType(key);
			int[] itemStats = ((key == EInventoryEquipmentType.None) ? new int[4] : InventoryEquipment.GetEquipmentStats(key));
			int[] equipmentStatsPreview = base.SaveFile.CharacterStats.GetEquipmentStatsPreview(itemStats, slotType);
			int num = equipmentStatsPreview[1];
			int orbDamage2 = base.SaveFile.Inventory.OrbInventory.GetOrbDamage(equippedMeleeOrbA, EOrbSlot.Melee, num);
			statEntry2.Value2 = orbDamage2;
			statEntry4.Value2 = equipmentStatsPreview[0];
			statEntry6.Value2 = num;
			statEntry8.Value2 = equipmentStatsPreview[2];
			statEntry10.Value2 = equipmentStatsPreview[3];
			_selectedItemStats.Entries.Add(statEntry2);
			_selectedItemStats.Entries.Add(statEntry4);
			_selectedItemStats.Entries.Add(statEntry6);
			_selectedItemStats.Entries.Add(statEntry8);
			_selectedItemStats.Entries.Add(statEntry10);
		}
	}

	public override void DrawFrames(SpriteBatch spriteBatch, Color drawColor)
	{
		SpriteEffects[] array = new SpriteEffects[9]
		{
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.FlipHorizontally,
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.FlipHorizontally,
			SpriteEffects.FlipVertically,
			SpriteEffects.FlipVertically,
			SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically
		};
		DrawingEx.DrawIrregularBox(spriteBatch, _menuBackgroundDrawRectangle, drawColor, base.Sprite, base.Zoom, new int[9] { 41, 42, 41, 43, 44, 43, 41, 42, 41 }, array, shouldTile: true);
		DrawingEx.DrawIrregularBox(spriteBatch, _statsBackgroundDrawRectangle, drawColor, base.Sprite, base.Zoom, new int[9] { 45, 46, 45, 47, 48, 47, 45, 46, 45 }, array, shouldTile: true);
		base.DrawFrames(spriteBatch, drawColor);
		array[2] = SpriteEffects.FlipHorizontally;
		DrawingEx.DrawShortBox(spriteBatch, new Rectangle(_stockBorderDrawPositionX, _stockMoneyDrawPositionY, _stockBorderDrawWidth, 16), drawColor, base.Sprite, base.Zoom, new int[3] { 54, 55, 54 }, array, shouldTile: true);
		DrawingEx.DrawShortBox(spriteBatch, new Rectangle(_moneyBorderDrawPositionX, _stockMoneyDrawPositionY, _moneyBorderDrawWidth, 16), drawColor, base.Sprite, base.Zoom, new int[3] { 54, 55, 54 }, array, shouldTile: true);
	}

	public override void DrawHeader(SpriteBatch spriteBatch, Color drawColor)
	{
		DrawingEx.DrawIrregularBox(flipped: new SpriteEffects[9]
		{
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.FlipHorizontally,
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.FlipVertically,
			SpriteEffects.FlipVertically,
			SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically
		}, spriteBatch: spriteBatch, backgroundRectangle: new Rectangle(_screenLeft, _screenTop, _screenWidth, _topSectionHeight), color: drawColor, sprite: base.Sprite, zoom: base.Zoom, frames: new int[9] { 53, 33, 53, 25, -1, 26, 35, 36, 35 }, shouldTile: true);
	}
}
