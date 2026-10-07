using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameStateManagement.ScreenManager;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

namespace Timespinner.GameStateManagement.Screens.Shop;

internal class OrbShopMenuScreen : InventoryMenuScreen
{
	private const float DisplayStatsPositionRatioX = 0.55f;

	private const float DisplayStatsPositionRatioY = 5f / 32f;

	private const float DisplayStatsWidthRatio = 0.4f;

	private const float DisplayStatsCompareWidthRatio = 0.295f;

	private const float StatsBackgroundDrawOffsetY = 7f / 64f;

	private const float StatsBackgroundHeightRatio = 83f / 192f;

	private const float StockBorderDrawPositionRatioX = 0.57f;

	private const float StockBorderDrawWidthRatio = 0.35f;

	private const float StockBorderDrawPositionRatioY = 0.74583334f;

	private const float StockTextDrawPositionRatioX = 0.62f;

	private const float StockTextDrawPositionRatioY = 11f / 15f;

	private const float StockTextDrawWidth = 22f;

	private const float IconsDisplayPositionRatioX = 0.07f;

	private const float IconsDisplayPositionRatioY = 31f / 192f;

	private const float IconsCursorOffsetMultiplierX = -20f;

	private const float PrimaryMenuDisplayRatioX = 21f / 160f;

	private const float PrimaryMenuDisplayOffsetY = -1f;

	private readonly bool _isReinforceAvailable;

	private readonly string _createJewelryConfirmtationDescription;

	private readonly string _infuseOrbConfirmationDescription;

	private readonly MerchantInventory _merchandiseInventory;

	private readonly Action _onExitAction;

	private readonly MenuEntry _spellMenuEntry;

	private readonly MenuEntry _passiveMenuEntry;

	private readonly MenuEntry _infuseMenuEntry;

	private readonly ConfirmationMenuEntryCollection _yesNoCollection;

	private readonly bool[] _iconHighlightList = new bool[3];

	private readonly StatCollection _selectedItemStats = new StatCollection();

	private readonly StatCollection _selectedItemComparisonStats = new StatCollection();

	private readonly StatCollection _playerInventoryStats = new StatCollection();

	private readonly OrbShopMenuEntryCollection _spellOrbMenuCollection;

	private readonly OrbShopMenuEntryCollection _passiveOrbMenuCollection;

	private readonly OrbShopMenuEntryCollection _infuseOrbMenuCollection;

	private readonly IList<OrbShopMenuEntryCollection> _categoryMenuCollections = new List<OrbShopMenuEntryCollection>();

	private bool _didInfuseAnOrb;

	private EOrbSlot _lastSelectedOrbSlot;

	private int _stockBorderDrawPositionX;

	private int _stockBorderDrawPositionY;

	private int _stockBorderDrawWidth;

	private int _currentGoldRingStock;

	private int _currentGoldNecklaceStock;

	private int _currentEssenceGemStock;

	private int _currentMagicMarblesStock;

	private Point _iconCursorOffset;

	private Vector2 _iconDisplayFramePosition;

	private Rectangle _menuBackgroundDrawRectangle;

	private Rectangle _statsBackgroundDrawRectangle;

	private Action _buySelectedItem;

	public OrbShopMenuScreen(GameSave inSave, GCM gcm, Action onExitAction)
		: base(Loc.Get("shop_orb"), inSave, gcm, null)
	{
		_onExitAction = onExitAction;
		_merchandiseInventory = CreateMerchandiseFromPlayerInventory(inSave);
		base.DoesDrawBrackets = true;
		base.DoesDrawBracketsOverAll = false;
		base.DoesDrawTopLowerFrame = true;
		base.DoesDrawScrollbarWidget = true;
		base.StatCollections.Add(_selectedItemStats);
		base.StatCollections.Add(_selectedItemComparisonStats);
		base.StatCollections.Add(_playerInventoryStats);
		_playerInventoryStats.DoesStackHorizontally = true;
		_spellMenuEntry = new MenuEntry(Loc.Get("shop_orb_spell_header"))
		{
			Description = Loc.Get("shop_orb_spell_desc")
		};
		_spellMenuEntry.Selected += ItemMenuEntrySelected;
		base.MenuEntries.Add(_spellMenuEntry);
		_spellOrbMenuCollection = new OrbShopMenuEntryCollection(OnItemSelected, base.GCM.SpPauseMenu, EOrbSlot.Spell);
		_spellOrbMenuCollection.AddEntries(_merchandiseInventory.OrbInventory.Inventory.Values);
		_categoryMenuCollections.Add(_spellOrbMenuCollection);
		_subMenuCollections.Add(_spellOrbMenuCollection);
		_passiveMenuEntry = new MenuEntry(Loc.Get("shop_orb_passive_header"))
		{
			Description = Loc.Get("shop_orb_passive_desc")
		};
		_passiveMenuEntry.Selected += ItemMenuEntrySelected;
		base.MenuEntries.Add(_passiveMenuEntry);
		_passiveOrbMenuCollection = new OrbShopMenuEntryCollection(OnItemSelected, base.GCM.SpPauseMenu, EOrbSlot.Passive);
		_passiveOrbMenuCollection.AddEntries(_merchandiseInventory.OrbInventory.Inventory.Values);
		_categoryMenuCollections.Add(_passiveOrbMenuCollection);
		_subMenuCollections.Add(_passiveOrbMenuCollection);
		_infuseMenuEntry = new MenuEntry(Loc.Get("shop_orb_melee_header"))
		{
			Description = Loc.Get("shop_orb_melee_desc")
		};
		_infuseMenuEntry.Selected += ItemMenuEntrySelected;
		int primaryQuestState = NPCBase.GetPrimaryQuestState(NPCBase.ENPCType.Astrologer, base.SaveFile);
		if (primaryQuestState >= 1)
		{
			_isReinforceAvailable = true;
			base.MenuEntries.Add(_infuseMenuEntry);
		}
		_infuseOrbMenuCollection = new OrbShopMenuEntryCollection(OnItemSelected, base.GCM.SpPauseMenu, EOrbSlot.Melee);
		_infuseOrbMenuCollection.AddEntries(base.SaveFile.Inventory.OrbInventory.Inventory.Values);
		_categoryMenuCollections.Add(_infuseOrbMenuCollection);
		_subMenuCollections.Add(_infuseOrbMenuCollection);
		_createJewelryConfirmtationDescription = Loc.Get("shop_orb_buy_confirm");
		_infuseOrbConfirmationDescription = Loc.Get("shop_orb_infuse_confirm");
		_yesNoCollection = new ConfirmationMenuEntryCollection(Loc.Get("shop_buy_yes"), Loc.Get("shop_buy_no"), _createJewelryConfirmtationDescription, OnConfirmBuy, base.OnCancel)
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
		_selectedItemStats.Location = new Vector2(0.55f * (float)_screenWidth + (float)_screenLeft, (float)_screenTop + 5f / 32f * (float)_topSectionHeight + (float)num2);
		_selectedItemStats.Width = (int)(0.4f * (float)_screenWidth);
		_selectedItemComparisonStats.Location = _selectedItemStats.Location;
		_selectedItemComparisonStats.Width = (int)(0.295f * (float)_screenWidth);
		_stockBorderDrawPositionX = _screenLeft + (int)((float)_screenWidth * 0.57f);
		_stockBorderDrawPositionY = _screenTop + (int)(0.74583334f * (float)(_topSectionHeight + _bottomSectionHeight));
		_stockBorderDrawWidth = (int)(0.35f * (float)_screenWidth);
		_playerInventoryStats.Location = new Vector2((float)_screenLeft + (float)_screenWidth * 0.62f, (float)_screenTop + 11f / 15f * (float)(_topSectionHeight + _bottomSectionHeight));
		_playerInventoryStats.Width = (int)((float)base.Zoom * 22f);
		_primaryMenuCollection.DrawPosition = new Vector2((float)_screenLeft + 21f / 160f * (float)_screenWidth, _primaryMenuCollection.DrawPosition.Y + -1f * (float)base.Zoom);
		foreach (OrbShopMenuEntryCollection categoryMenuCollection in _categoryMenuCollections)
		{
			categoryMenuCollection.DrawPosition = base.ListTextDrawPosition;
			categoryMenuCollection.SetColumnWidth(base.ListColumnWidth, base.Zoom);
		}
		Vector2 drawPosition = new Vector2((int)(base.DescriptionDrawPosition.X + (float)_screenWidth * 0.125f), (int)(base.DescriptionDrawPosition.Y + (float)_bottomSectionHeight * 0.5f));
		_yesNoCollection.DrawPosition = drawPosition;
		_yesNoCollection.SetColumnWidth(base.ListColumnWidth, base.Zoom);
		_iconDisplayFramePosition = new Vector2(0.07f * (float)_screenWidth + (float)_screenLeft, (float)_screenTop + 31f / 192f * (float)_topSectionHeight);
		_iconCursorOffset = new Point((int)(-20f * (float)base.Zoom), 0);
		base.CursorOffset = ((_selectedMenuCollection == _primaryMenuCollection) ? _iconCursorOffset : Point.Zero);
		_spellOrbMenuCollection.RefreshSizes();
		_passiveOrbMenuCollection.RefreshSizes();
		_infuseOrbMenuCollection.RefreshSizes();
		RefreshDisplayStats(isResize: true);
	}

	private static MerchantInventory CreateMerchandiseFromPlayerInventory(GameSave playerSave)
	{
		MerchantInventory merchantInventory = new MerchantInventory();
		foreach (InventoryOrb value in playerSave.Inventory.OrbInventory.Inventory.Values)
		{
			merchantInventory.OrbInventory.AddItem((int)value.OrbType);
			if (value.IsSpellUnlocked)
			{
				merchantInventory.OrbInventory.AddItem((int)value.OrbType, EOrbSlot.Spell);
			}
			if (value.IsPassiveUnlocked)
			{
				merchantInventory.OrbInventory.AddItem((int)value.OrbType, EOrbSlot.Passive);
			}
		}
		return merchantInventory;
	}

	private OrbShopMenuEntryCollection GetSelectedCategory()
	{
		OrbShopMenuEntryCollection result = null;
		foreach (OrbShopMenuEntryCollection categoryMenuCollection in _categoryMenuCollections)
		{
			if (_selectedMenuCollection == categoryMenuCollection)
			{
				result = categoryMenuCollection;
				break;
			}
		}
		return result;
	}

	private bool IsOrbOwned(OrbShopMenuEntry orbEntry)
	{
		bool result = false;
		if (base.SaveFile.Inventory.OrbInventory.Inventory.ContainsKey((int)orbEntry.Orb.OrbType))
		{
			InventoryOrb inventoryOrb = base.SaveFile.Inventory.OrbInventory.Inventory[(int)orbEntry.Orb.OrbType];
			result = ((orbEntry.OrbSlot == EOrbSlot.Spell) ? inventoryOrb.IsSpellUnlocked : inventoryOrb.IsPassiveUnlocked);
		}
		return result;
	}

	private void ItemMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		int count = _categoryMenuCollections.Count;
		if (count > base.SelectedIndex)
		{
			OrbShopMenuEntryCollection orbShopMenuEntryCollection = _categoryMenuCollections[base.SelectedIndex];
			if (orbShopMenuEntryCollection.Entries.Count > 0)
			{
				ChangeMenuCollection(orbShopMenuEntryCollection, shouldPush: true);
			}
			else
			{
				base.ScreenManager.Jukebox.PlayCue(ESFX.MenuError);
			}
		}
	}

	private void OnItemSelected(OrbShopMenuEntry item)
	{
		if (CanBuy(item))
		{
			_buySelectedItem = delegate
			{
				AddItemToInventory(item);
				SubtractMoney(item);
				item.QuanityToBuy = 1;
			};
			ShowConfirmationPrompt(item.OrbSlot != EOrbSlot.Melee);
		}
	}

	private void AddItemToInventory(OrbShopMenuEntry itemEntry)
	{
		if (itemEntry.OrbSlot != 0)
		{
			base.SaveFile.GiveOrb(itemEntry.Orb.OrbType, itemEntry.OrbSlot);
			return;
		}
		_didInfuseAnOrb = true;
		base.SaveFile.Inventory.OrbInventory.GiveOrbInfusionExperience(itemEntry.Orb.OrbType);
	}

	private bool CanBuy(OrbShopMenuEntry itemEntry)
	{
		bool flag;
		if (itemEntry.OrbSlot == EOrbSlot.Melee)
		{
			flag = base.SaveFile.Inventory.GetUseItemCount(EInventoryUseItemType.MagicMarbles) > 0;
		}
		else
		{
			flag = base.SaveFile.Inventory.GetUseItemCount(EInventoryUseItemType.EssenceCrystal) > 0;
			if (flag)
			{
				flag = base.SaveFile.Inventory.GetUseItemCount((itemEntry.OrbSlot == EOrbSlot.Spell) ? EInventoryUseItemType.GoldNecklace : EInventoryUseItemType.GoldRing) > 0;
			}
		}
		if (!flag)
		{
			base.ScreenManager.Jukebox.PlayCue(ESFX.MenuError);
			ChangeDescription(Loc.Get("shop_buy_no_ingredients"), EInventoryItemIcon.None);
		}
		return flag;
	}

	private void SubtractMoney(OrbShopMenuEntry itemEntry)
	{
		if (itemEntry.OrbSlot == EOrbSlot.Melee)
		{
			base.SaveFile.Inventory.UseItemInventory.RemoveItem(32, 1);
			return;
		}
		base.SaveFile.Inventory.UseItemInventory.RemoveItem(33, 1);
		if (itemEntry.OrbSlot == EOrbSlot.Spell)
		{
			base.SaveFile.Inventory.UseItemInventory.RemoveItem(35, 1);
		}
		else
		{
			base.SaveFile.Inventory.UseItemInventory.RemoveItem(34, 1);
		}
	}

	private void ShowConfirmationPrompt(bool isCreatingJewelry)
	{
		_yesNoCollection.SetDescription(isCreatingJewelry ? _createJewelryConfirmtationDescription : _infuseOrbConfirmationDescription);
		_yesNoCollection.IsVisible = true;
		ChangeMenuCollection(_yesNoCollection, shouldPush: true);
		base.ScreenManager.Jukebox.PlayCue(ESFX.MenuSelect);
	}

	private void OnConfirmBuy(object sender, PlayerIndexEventArgs e)
	{
		if (_buySelectedItem != null)
		{
			_buySelectedItem();
			_yesNoCollection.IsVisible = false;
			GoToPreviousMenuCollection();
			OnBuyItem();
		}
	}

	private void OnBuyItem()
	{
		base.ScreenManager.Jukebox.PlayCue(ESFX.MenuBuy);
	}

	protected override void OnSelectedEntryChanged(int entryIndex)
	{
		base.OnSelectedEntryChanged(entryIndex);
		if (_selectedMenuCollection != _yesNoCollection)
		{
			int num = 0;
			foreach (OrbShopMenuEntryCollection categoryMenuCollection in _categoryMenuCollections)
			{
				categoryMenuCollection.IsVisible = _selectedMenuCollection == categoryMenuCollection || (_selectedMenuCollection == _primaryMenuCollection && entryIndex == num);
				num++;
			}
			if (_selectedMenuCollection != _infuseOrbMenuCollection)
			{
				OrbShopMenuEntryCollection selectedCategory = GetSelectedCategory();
				if (selectedCategory != null)
				{
					for (int num2 = selectedCategory.Entries.Count - 1; num2 >= 0; num2--)
					{
						OrbShopMenuEntry orbEntry = selectedCategory.Items[num2];
						if (IsOrbOwned(orbEntry))
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
			}
			bool flag = _selectedMenuCollection == _primaryMenuCollection;
			_iconHighlightList[0] = flag || _selectedMenuCollection == _spellOrbMenuCollection;
			_iconHighlightList[1] = flag || _selectedMenuCollection == _passiveOrbMenuCollection;
			_iconHighlightList[2] = flag || _selectedMenuCollection == _infuseOrbMenuCollection;
			base.CursorOffset = (flag ? _iconCursorOffset : Point.Zero);
		}
		RefreshDisplayStats(isResize: false);
	}

	public override void HandleInput(InputState input)
	{
		bool flag = true;
		if (input.IsNewPressExit(base.ControllingPlayer))
		{
			flag = false;
			base.ScreenManager.Jukebox.PlayCue(ESFX.MenuCancel);
			ExitScreen();
		}
		if (flag)
		{
			base.HandleInput(input);
		}
	}

	public override void ExitScreen()
	{
		if (_didInfuseAnOrb)
		{
			_onExitAction();
		}
		base.ExitScreen();
	}

	private void RefreshDisplayStats(bool isResize)
	{
		if (_selectedMenuCollection == _yesNoCollection && !isResize)
		{
			return;
		}
		_selectedItemStats.Entries.Clear();
		_selectedItemComparisonStats.Entries.Clear();
		_playerInventoryStats.Entries.Clear();
		if (_selectedMenuCollection == _spellOrbMenuCollection || (isResize && _lastSelectedOrbSlot == EOrbSlot.Spell))
		{
			_lastSelectedOrbSlot = EOrbSlot.Spell;
			AddStatEntries(_spellOrbMenuCollection.GetSelected(), EOrbSlot.Spell);
		}
		else if (_selectedMenuCollection == _passiveOrbMenuCollection || (isResize && _lastSelectedOrbSlot == EOrbSlot.Passive))
		{
			_lastSelectedOrbSlot = EOrbSlot.Passive;
			AddStatEntries(_passiveOrbMenuCollection.GetSelected(), EOrbSlot.Passive);
		}
		else if (_selectedMenuCollection == _infuseOrbMenuCollection || (isResize && _lastSelectedOrbSlot == EOrbSlot.Melee))
		{
			_lastSelectedOrbSlot = EOrbSlot.Melee;
			AddStatEntries(_infuseOrbMenuCollection.GetSelected(), EOrbSlot.Melee);
		}
		else
		{
			_lastSelectedOrbSlot = EOrbSlot.All;
		}
		_currentEssenceGemStock = base.SaveFile.Inventory.GetUseItemCount(EInventoryUseItemType.EssenceCrystal);
		_currentGoldRingStock = base.SaveFile.Inventory.GetUseItemCount(EInventoryUseItemType.GoldRing);
		_currentGoldNecklaceStock = base.SaveFile.Inventory.GetUseItemCount(EInventoryUseItemType.GoldNecklace);
		_currentMagicMarblesStock = base.SaveFile.Inventory.GetUseItemCount(EInventoryUseItemType.MagicMarbles);
		_playerInventoryStats.Entries.Add(new StatEntry
		{
			Title = "",
			Type = StatEntry.EStatDisplayType.NumberWithIcon,
			Value = _currentEssenceGemStock,
			IconIndex = 17
		});
		_playerInventoryStats.Entries.Add(new StatEntry
		{
			Title = "",
			Type = StatEntry.EStatDisplayType.NumberWithIcon,
			Value = _currentGoldNecklaceStock,
			IconIndex = 19
		});
		_playerInventoryStats.Entries.Add(new StatEntry
		{
			Title = "",
			Type = StatEntry.EStatDisplayType.NumberWithIcon,
			Value = _currentGoldRingStock,
			IconIndex = 18
		});
		_playerInventoryStats.Entries.Add(new StatEntry
		{
			Title = "",
			Type = StatEntry.EStatDisplayType.NumberWithIcon,
			Value = _currentMagicMarblesStock,
			IconIndex = 20
		});
		foreach (OrbShopMenuEntryCollection categoryMenuCollection in _categoryMenuCollections)
		{
			categoryMenuCollection.UpdateItems(_currentEssenceGemStock, _currentGoldNecklaceStock, _currentGoldRingStock, _currentMagicMarblesStock);
		}
	}

	private void AddStatEntries(InventoryOrb orb, EOrbSlot slot)
	{
		if (orb == null)
		{
			return;
		}
		orb = base.SaveFile.Inventory.OrbInventory.GetItem((int)orb.OrbType);
		int willpower = base.SaveFile.CharacterStats.Willpower;
		string orbNameBySlot = InventoryItem.GetOrbNameBySlot(orb, EOrbSlot.Melee);
		int num = 0;
		switch (slot)
		{
		case EOrbSlot.Melee:
			num = orb.GetMeleeDamage(willpower);
			break;
		case EOrbSlot.Spell:
			num = orb.GetSpellDamage(willpower);
			break;
		case EOrbSlot.Passive:
			if (InventoryOrb.DoesOrbPassiveDealDamage(orb.OrbType))
			{
				num = orb.GetPassiveDamage(willpower);
			}
			break;
		}
		_selectedItemStats.Entries.Add(new StatEntry
		{
			Title = orbNameBySlot,
			Type = StatEntry.EStatDisplayType.IconOnly,
			IconIndex = (int)orb.OrbType
		});
		if (slot != 0)
		{
			_selectedItemStats.Entries.Add(new StatEntry
			{
				Title = Loc.Get("StatOrbLevel"),
				Type = StatEntry.EStatDisplayType.Number,
				Value = orb.Level
			});
			if (num > 0)
			{
				_selectedItemStats.Entries.Add(new StatEntry
				{
					Title = Loc.Get("StatDamage"),
					Type = StatEntry.EStatDisplayType.Number,
					Value = num
				});
			}
		}
		else
		{
			StatEntry statEntry = new StatEntry();
			statEntry.Title = Loc.Get("StatOrbLevel");
			statEntry.Type = StatEntry.EStatDisplayType.NumberComparison;
			statEntry.Value = orb.Level;
			statEntry.IsNumberComparisonAlwaysPositive = true;
			StatEntry statEntry2 = statEntry;
			StatEntry statEntry3 = new StatEntry();
			statEntry3.Title = Loc.Get("StatDamage");
			statEntry3.Type = StatEntry.EStatDisplayType.NumberComparison;
			statEntry3.Value = num;
			statEntry3.IsNumberComparisonAlwaysPositive = true;
			StatEntry statEntry4 = statEntry3;
			InventoryOrb reinforcedPreview = orb.GetReinforcedPreview();
			statEntry2.Value2 = reinforcedPreview.Level;
			statEntry4.Value2 = reinforcedPreview.GetMeleeDamage(willpower);
			_selectedItemComparisonStats.Entries.Add(new StatEntry());
			_selectedItemComparisonStats.Entries.Add(statEntry2);
			_selectedItemComparisonStats.Entries.Add(statEntry4);
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
		DrawingEx.DrawShortBox(spriteBatch, new Rectangle(_stockBorderDrawPositionX, _stockBorderDrawPositionY, _stockBorderDrawWidth, 16), drawColor, base.Sprite, base.Zoom, new int[3] { 54, 55, 54 }, array, shouldTile: true);
		Vector2 iconDisplayFramePosition = _iconDisplayFramePosition;
		for (int i = 0; i < 3; i++)
		{
			int index = ((i >= 2) ? 139 : (i + 66));
			if (i != 2 || _isReinforceAvailable)
			{
				Rectangle frameSource = base.Sprite.GetFrameSource(index);
				Color color = ((!_iconHighlightList[i]) ? new Color(drawColor.R / 3, drawColor.G / 3, drawColor.B / 3, drawColor.A) : drawColor);
				spriteBatch.Draw(base.Sprite.Texture, iconDisplayFramePosition, frameSource, color, 0f, Vector2.Zero, base.Zoom, SpriteEffects.None, 0f);
				iconDisplayFramePosition.Y += frameSource.Height * base.Zoom;
			}
		}
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
