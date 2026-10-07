using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

namespace Timespinner.GameStateManagement.Screens.PauseMenu;

internal class EquipmentMenuScreen : InventoryMenuScreen
{
	private const float DisplayStatsPositionRatioX = 0.55f;

	private const float DisplayStatsPositionRatioY = 5f / 32f;

	private const float DisplayStatsWidthRatio = 0.3f;

	private const float MenuFrameDrawPositionRatioX = 0.009375f;

	private const float IconsDisplayPositionRatioX = 0.07f;

	private const float IconsDisplayPositionRatioY = 31f / 192f;

	private const float IconsCursorOffsetMultiplierX = -20f;

	private const float PrimaryMenuDisplayRatioX = 21f / 160f;

	private const float PrimaryMenuDisplayOffsetY = -1f;

	private const float StatsBackgroundDrawOffsetY = 7f / 64f;

	private const float StatsBackgroundHeightRatio = 0.4375f;

	private readonly Action _onExitEquipmentAction;

	private readonly MenuEquipmentInventory _hatEquipmentInventory;

	private readonly MenuEquipmentInventory _bodyEquipmentInventory;

	private readonly MenuEquipmentInventory _trinketAEquipmentInventory;

	private readonly MenuEquipmentInventory _trinketBEquipmentInventory;

	private readonly MenuEntry _hatMenuEntry;

	private readonly MenuEntry _bodyMenuEntry;

	private readonly MenuEntry _trinketAMenuEntry;

	private readonly MenuEntry _trinketBMenuEntry;

	private readonly StatCollection _selectedItemStats = new StatCollection();

	private readonly bool[] _iconHighlightList = new bool[4] { true, true, true, true };

	private Point _iconCursorOffset;

	private Vector2 _iconDisplayFramePosition;

	private Rectangle _menuBackgroundDrawRectangle;

	private Rectangle _statsBackgroundDrawRectangle;

	private Rectangle _menuFrameDrawRectangle;

	public EquipmentMenuScreen(GameSave inSave, GCM gcm, Action onExit, Action fullExitAction)
		: base(Loc.Get("EquipmentMenuTitle"), inSave, gcm, fullExitAction)
	{
		_onExitEquipmentAction = onExit;
		base.DoesDrawBrackets = true;
		base.DoesDrawBracketsOverAll = false;
		base.DoesDrawTopLowerFrame = true;
		base.DoesDrawScrollbarWidget = true;
		base.StatCollections.Add(_selectedItemStats);
		_hatMenuEntry = new MenuEntry(InventoryItem.NameFromType(inSave.Inventory.EquippedHelmet))
		{
			Description = Loc.Get("EquipmentHeadMenuDesc")
		};
		_bodyMenuEntry = new MenuEntry(InventoryItem.NameFromType(inSave.Inventory.EquippedArmor))
		{
			Description = Loc.Get("EquipmentBodyMenuDesc")
		};
		_trinketAMenuEntry = new MenuEntry(InventoryItem.NameFromType(inSave.Inventory.EquippedTrinketA))
		{
			Description = Loc.Get("EquipmentTrinketMenuDesc")
		};
		_trinketBMenuEntry = new MenuEntry(InventoryItem.NameFromType(inSave.Inventory.EquippedTrinketB))
		{
			Description = Loc.Get("EquipmentTrinketMenuDesc")
		};
		_hatMenuEntry.Selected += HatMenuEntrySelected;
		_bodyMenuEntry.Selected += BodyMenuEntrySelected;
		_trinketAMenuEntry.Selected += TrinketAMenuEntrySelected;
		_trinketBMenuEntry.Selected += TrinketBMenuEntrySelected;
		base.MenuEntries.Add(_hatMenuEntry);
		base.MenuEntries.Add(_bodyMenuEntry);
		base.MenuEntries.Add(_trinketAMenuEntry);
		base.MenuEntries.Add(_trinketBMenuEntry);
		_hatEquipmentInventory = new MenuEquipmentInventory(inSave.Inventory.EquipmentInventory, inSave.Inventory.RecentlyEquippedHeadItems, HeadSubMenuItemSelected, HeadUnequip, EEquipmentSlotType.Head, doesAddUnequip: true, base.Sprite)
		{
			IsVisible = true
		};
		_bodyEquipmentInventory = new MenuEquipmentInventory(inSave.Inventory.EquipmentInventory, inSave.Inventory.RecentlyEquippedBodyItems, BodySubMenuItemSelected, BodyUnequip, EEquipmentSlotType.Body, doesAddUnequip: true, base.Sprite);
		_trinketAEquipmentInventory = new MenuEquipmentInventory(inSave.Inventory.EquipmentInventory, inSave.Inventory.RecentlyEquippedTrinketItems, TrinketASubMenuItemSelected, TrinketAUnequip, EEquipmentSlotType.Trinket, doesAddUnequip: true, base.Sprite);
		_trinketBEquipmentInventory = new MenuEquipmentInventory(inSave.Inventory.EquipmentInventory, inSave.Inventory.RecentlyEquippedTrinketItems, TrinketBSubMenuItemSelected, TrinketBUnequip, EEquipmentSlotType.Trinket, doesAddUnequip: true, base.Sprite);
		_subMenuCollections.Add(_hatEquipmentInventory);
		_subMenuCollections.Add(_bodyEquipmentInventory);
		_subMenuCollections.Add(_trinketAEquipmentInventory);
		_subMenuCollections.Add(_trinketBEquipmentInventory);
		RefreshAllEquippedItemIcons();
	}

	internal override void RefreshSizes()
	{
		base.RefreshSizes();
		int num = (int)(7f / 64f * (float)_topSectionHeight);
		_menuBackgroundDrawRectangle = new Rectangle(_screenLeft + 2 * base.Zoom, _screenTop + num, (int)Math.Ceiling((float)_screenWidth / 2f - (float)(2 * base.Zoom)), (int)(0.4375f * (float)_topSectionHeight));
		_statsBackgroundDrawRectangle = new Rectangle(_menuBackgroundDrawRectangle.Right, _menuBackgroundDrawRectangle.Top, _menuBackgroundDrawRectangle.Width, _menuBackgroundDrawRectangle.Height);
		_menuFrameDrawRectangle = new Rectangle(_screenLeft + (int)(0.009375f * (float)_screenWidth), _screenTop + (int)(23f / 192f * (float)_topSectionHeight), (int)(0.490625f * (float)_screenWidth), (int)(27f / 64f * (float)_topSectionHeight));
		_primaryMenuCollection.DrawPosition = new Vector2(21f / 160f * (float)_screenWidth + (float)_screenLeft, _primaryMenuCollection.DrawPosition.Y + -1f * (float)base.Zoom);
		_primaryMenuCollection.SetColumnWidth(base.NarrowListColumnWidth, base.Zoom);
		int num2 = (Loc.IsAsianLocale ? (-2) : 0);
		_selectedItemStats.Location = new Vector2(0.55f * (float)_screenWidth + (float)_screenLeft, (float)_screenTop + 5f / 32f * (float)_topSectionHeight + (float)(num2 * base.Zoom));
		_selectedItemStats.Width = (int)(0.3f * (float)_screenWidth);
		_hatEquipmentInventory.DrawPosition = base.ListTextDrawPosition;
		_hatEquipmentInventory.SetColumnWidth(base.ListColumnWidth, base.Zoom);
		_bodyEquipmentInventory.DrawPosition = base.ListTextDrawPosition;
		_bodyEquipmentInventory.SetColumnWidth(base.ListColumnWidth, base.Zoom);
		_trinketAEquipmentInventory.DrawPosition = base.ListTextDrawPosition;
		_trinketAEquipmentInventory.SetColumnWidth(base.ListColumnWidth, base.Zoom);
		_trinketBEquipmentInventory.DrawPosition = base.ListTextDrawPosition;
		_trinketBEquipmentInventory.SetColumnWidth(base.ListColumnWidth, base.Zoom);
		_iconCursorOffset = new Point((int)(-20f * (float)base.Zoom), 0);
		base.CursorOffset = ((_selectedMenuCollection == _primaryMenuCollection) ? _iconCursorOffset : Point.Zero);
		_iconDisplayFramePosition = new Vector2(0.07f * (float)_screenWidth + (float)_screenLeft, (float)_screenTop + 31f / 192f * (float)_topSectionHeight);
		RefreshDisplayStats();
	}

	private void RefreshAllEquippedItemIcons()
	{
		RefreshItemEquipped(_hatEquipmentInventory, base.SaveFile.Inventory.EquippedHelmet, EInventoryEquipmentType.None);
		RefreshItemEquipped(_bodyEquipmentInventory, base.SaveFile.Inventory.EquippedArmor, EInventoryEquipmentType.None);
		RefreshItemEquipped(_trinketAEquipmentInventory, base.SaveFile.Inventory.EquippedTrinketA, base.SaveFile.Inventory.EquippedTrinketB);
		RefreshItemEquipped(_trinketBEquipmentInventory, base.SaveFile.Inventory.EquippedTrinketB, base.SaveFile.Inventory.EquippedTrinketA);
	}

	private void HatMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		if (_hatEquipmentInventory.Entries.Count > 0)
		{
			ChangeMenuCollection(_hatEquipmentInventory, shouldPush: true);
		}
	}

	private void BodyMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		if (_bodyEquipmentInventory.Entries.Count > 0)
		{
			ChangeMenuCollection(_bodyEquipmentInventory, shouldPush: true);
		}
	}

	private void TrinketAMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		if (_trinketAEquipmentInventory.Entries.Count > 0)
		{
			ChangeMenuCollection(_trinketAEquipmentInventory, shouldPush: true);
		}
	}

	private void TrinketBMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		if (_trinketBEquipmentInventory.Entries.Count > 0)
		{
			ChangeMenuCollection(_trinketBEquipmentInventory, shouldPush: true);
		}
	}

	private void HeadSubMenuItemSelected(InventoryEquipment item)
	{
		base.SaveFile.Inventory.EquippedHelmet = item.EquipmentType;
		base.SaveFile.Inventory.AddToRecentlyEquipped(item.EquipmentType, EEquipmentSlotType.Head);
		_hatEquipmentInventory.AddRecentEntries(base.SaveFile.Inventory.RecentlyEquippedHeadItems);
		_hatMenuEntry.SetText(InventoryItem.NameFromType(item.EquipmentType));
		RefreshItemEquipped(_hatEquipmentInventory, item.EquipmentType, EInventoryEquipmentType.None);
		_hatEquipmentInventory.Update(0f, isScreenActive: true, 0f);
		OnEquipItem();
	}

	private void BodySubMenuItemSelected(InventoryEquipment item)
	{
		base.SaveFile.Inventory.EquippedArmor = item.EquipmentType;
		base.SaveFile.Inventory.AddToRecentlyEquipped(item.EquipmentType, EEquipmentSlotType.Body);
		_bodyEquipmentInventory.AddRecentEntries(base.SaveFile.Inventory.RecentlyEquippedBodyItems);
		_bodyMenuEntry.SetText(InventoryItem.NameFromType(item.EquipmentType));
		RefreshItemEquipped(_bodyEquipmentInventory, item.EquipmentType, EInventoryEquipmentType.None);
		_bodyEquipmentInventory.Update(0f, isScreenActive: true, 0f);
		OnEquipItem();
	}

	private void TrinketASubMenuItemSelected(InventoryEquipment item)
	{
		EInventoryEquipmentType equipmentType = item.EquipmentType;
		if (base.SaveFile.Inventory.EquippedTrinketB == equipmentType && base.SaveFile.Inventory.EquipmentInventory.GetCount(equipmentType) < 2)
		{
			ChangeDescription(Loc.Get("EquipmentNotEnoughError"), EInventoryItemIcon.None);
			PlayErrorSound();
			return;
		}
		base.SaveFile.Inventory.EquippedTrinketA = item.EquipmentType;
		base.SaveFile.Inventory.AddToRecentlyEquipped(item.EquipmentType, EEquipmentSlotType.Trinket);
		_trinketAEquipmentInventory.AddRecentEntries(base.SaveFile.Inventory.RecentlyEquippedTrinketItems);
		_trinketBEquipmentInventory.AddRecentEntries(base.SaveFile.Inventory.RecentlyEquippedTrinketItems);
		_trinketAMenuEntry.SetText(InventoryItem.NameFromType(item.EquipmentType));
		RefreshItemEquipped(_trinketAEquipmentInventory, item.EquipmentType, base.SaveFile.Inventory.EquippedTrinketB);
		RefreshItemEquipped(_trinketBEquipmentInventory, base.SaveFile.Inventory.EquippedTrinketB, item.EquipmentType);
		_trinketAEquipmentInventory.Update(0f, isScreenActive: true, 0f);
		_trinketBEquipmentInventory.Update(0f, isScreenActive: true, 0f);
		OnEquipItem();
	}

	private void TrinketBSubMenuItemSelected(InventoryEquipment item)
	{
		EInventoryEquipmentType equipmentType = item.EquipmentType;
		if (base.SaveFile.Inventory.EquippedTrinketA == equipmentType && base.SaveFile.Inventory.EquipmentInventory.GetCount(equipmentType) < 2)
		{
			ChangeDescription(Loc.Get("EquipmentNotEnoughError"), EInventoryItemIcon.None);
			PlayErrorSound();
			return;
		}
		base.SaveFile.Inventory.EquippedTrinketB = item.EquipmentType;
		base.SaveFile.Inventory.AddToRecentlyEquipped(item.EquipmentType, EEquipmentSlotType.Trinket);
		_trinketBEquipmentInventory.AddRecentEntries(base.SaveFile.Inventory.RecentlyEquippedTrinketItems);
		_trinketAEquipmentInventory.AddRecentEntries(base.SaveFile.Inventory.RecentlyEquippedTrinketItems);
		_trinketBMenuEntry.SetText(InventoryItem.NameFromType(item.EquipmentType));
		RefreshItemEquipped(_trinketBEquipmentInventory, item.EquipmentType, base.SaveFile.Inventory.EquippedTrinketA);
		RefreshItemEquipped(_trinketAEquipmentInventory, base.SaveFile.Inventory.EquippedTrinketA, item.EquipmentType);
		_trinketAEquipmentInventory.Update(0f, isScreenActive: true, 0f);
		_trinketBEquipmentInventory.Update(0f, isScreenActive: true, 0f);
		OnEquipItem();
	}

	private void OnEquipItem()
	{
		base.ScreenManager.Jukebox.PlayCue(ESFX.MenuEquip);
		GoToPreviousMenuCollection();
	}

	private void HeadUnequip()
	{
		base.SaveFile.Inventory.EquippedHelmet = EInventoryEquipmentType.None;
		_hatMenuEntry.SetText("---");
		RefreshItemEquipped(_hatEquipmentInventory, EInventoryEquipmentType.None, EInventoryEquipmentType.None);
		OnUnequipItem();
	}

	private void BodyUnequip()
	{
		base.SaveFile.Inventory.EquippedArmor = EInventoryEquipmentType.None;
		_bodyMenuEntry.SetText("---");
		RefreshItemEquipped(_bodyEquipmentInventory, EInventoryEquipmentType.None, EInventoryEquipmentType.None);
		OnUnequipItem();
	}

	private void TrinketAUnequip()
	{
		base.SaveFile.Inventory.EquippedTrinketA = EInventoryEquipmentType.None;
		_trinketAMenuEntry.SetText("---");
		RefreshItemEquipped(_trinketAEquipmentInventory, EInventoryEquipmentType.None, base.SaveFile.Inventory.EquippedTrinketB);
		RefreshItemEquipped(_trinketBEquipmentInventory, base.SaveFile.Inventory.EquippedTrinketB, EInventoryEquipmentType.None);
		OnUnequipItem();
	}

	private void TrinketBUnequip()
	{
		base.SaveFile.Inventory.EquippedTrinketB = EInventoryEquipmentType.None;
		_trinketBMenuEntry.SetText("---");
		RefreshItemEquipped(_trinketBEquipmentInventory, EInventoryEquipmentType.None, base.SaveFile.Inventory.EquippedTrinketA);
		RefreshItemEquipped(_trinketAEquipmentInventory, base.SaveFile.Inventory.EquippedTrinketA, EInventoryEquipmentType.None);
		OnUnequipItem();
	}

	private void OnUnequipItem()
	{
		base.ScreenManager.Jukebox.PlayCue(ESFX.MenuCancel);
		GoToPreviousMenuCollection();
	}

	private void RefreshItemEquipped(MenuEquipmentInventory targetInventory, EInventoryEquipmentType equippedType, EInventoryEquipmentType equippedType2)
	{
		targetInventory.EquippedItem = equippedType;
		targetInventory.EquippedItem2 = equippedType2;
		base.SaveFile.CharacterStats.RefreshEquipmentStats(base.SaveFile.Inventory.GetEquipmentStats());
	}

	protected override void OnSelectedEntryChanged(int entryIndex)
	{
		base.OnSelectedEntryChanged(entryIndex);
		_hatEquipmentInventory.IsVisible = _selectedMenuCollection == _hatEquipmentInventory || (_selectedMenuCollection == _primaryMenuCollection && entryIndex == 0);
		_bodyEquipmentInventory.IsVisible = _selectedMenuCollection == _bodyEquipmentInventory || (_selectedMenuCollection == _primaryMenuCollection && entryIndex == 1);
		_trinketAEquipmentInventory.IsVisible = _selectedMenuCollection == _trinketAEquipmentInventory || (_selectedMenuCollection == _primaryMenuCollection && entryIndex == 2);
		_trinketBEquipmentInventory.IsVisible = _selectedMenuCollection == _trinketBEquipmentInventory || (_selectedMenuCollection == _primaryMenuCollection && entryIndex == 3);
		bool flag = _selectedMenuCollection == _primaryMenuCollection;
		_iconHighlightList[0] = flag || _selectedMenuCollection == _hatEquipmentInventory;
		_iconHighlightList[1] = flag || _selectedMenuCollection == _bodyEquipmentInventory;
		_iconHighlightList[2] = flag || _selectedMenuCollection == _trinketAEquipmentInventory;
		_iconHighlightList[3] = flag || _selectedMenuCollection == _trinketBEquipmentInventory;
		base.CursorOffset = (flag ? _iconCursorOffset : Point.Zero);
		RefreshDisplayStats();
	}

	public override void ExitScreen()
	{
		_onExitEquipmentAction();
		base.ExitScreen();
	}

	private void RefreshDisplayStats()
	{
		_selectedItemStats.Entries.Clear();
		if (_selectedMenuCollection == _hatEquipmentInventory)
		{
			AddStatEntries(_hatEquipmentInventory.GetSelected());
		}
		else if (_selectedMenuCollection == _bodyEquipmentInventory)
		{
			AddStatEntries(_bodyEquipmentInventory.GetSelected());
		}
		else if (_selectedMenuCollection == _trinketAEquipmentInventory)
		{
			AddStatEntries(_trinketAEquipmentInventory.GetSelected());
		}
		else if (_selectedMenuCollection == _trinketBEquipmentInventory)
		{
			AddStatEntries(_trinketBEquipmentInventory.GetSelected());
		}
		else
		{
			AddStatEntries(null);
		}
	}

	private void AddStatEntries(InventoryEquipment equipment)
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
		if (_selectedMenuCollection != _primaryMenuCollection)
		{
			int selectedIndex = _primaryMenuCollection.SelectedIndex;
			int[] itemStats = ((equipment == null) ? new int[4] : InventoryEquipment.GetEquipmentStats(equipment.EquipmentType));
			int[] equipmentStatsPreview = base.SaveFile.CharacterStats.GetEquipmentStatsPreview(itemStats, selectedIndex);
			int num = equipmentStatsPreview[1];
			int orbDamage2 = base.SaveFile.Inventory.OrbInventory.GetOrbDamage(equippedMeleeOrbA, EOrbSlot.Melee, num);
			statEntry2.Value2 = orbDamage2;
			statEntry4.Value2 = equipmentStatsPreview[0];
			statEntry6.Value2 = num;
			statEntry8.Value2 = equipmentStatsPreview[2];
			statEntry10.Value2 = equipmentStatsPreview[3];
		}
		else
		{
			statEntry2.Value2 = statEntry2.Value;
			statEntry4.Value2 = statEntry4.Value;
			statEntry6.Value2 = statEntry6.Value;
			statEntry8.Value2 = statEntry8.Value;
			statEntry10.Value2 = statEntry10.Value;
		}
		_selectedItemStats.Entries.Add(statEntry2);
		_selectedItemStats.Entries.Add(statEntry4);
		_selectedItemStats.Entries.Add(statEntry6);
		_selectedItemStats.Entries.Add(statEntry8);
		_selectedItemStats.Entries.Add(statEntry10);
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
		array[2] = SpriteEffects.FlipHorizontally;
		array[6] = SpriteEffects.FlipVertically;
		array[8] = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
		DrawingEx.DrawIrregularBox(spriteBatch, _menuFrameDrawRectangle, drawColor, base.Sprite, base.Zoom, new int[9] { -1, -1, 34, -1, -1, -1, -1, -1, 34 }, array);
		Vector2 iconDisplayFramePosition = _iconDisplayFramePosition;
		for (int i = 0; i < 4; i++)
		{
			int index = i + 129 + ((i == 3) ? (-1) : 0);
			Rectangle frameSource = base.Sprite.GetFrameSource(index);
			Color color = ((!_iconHighlightList[i]) ? new Color(drawColor.R / 3, drawColor.G / 3, drawColor.B / 3, drawColor.A) : drawColor);
			spriteBatch.Draw(base.Sprite.Texture, iconDisplayFramePosition, frameSource, color, 0f, Vector2.Zero, base.Zoom, SpriteEffects.None, 0f);
			iconDisplayFramePosition.Y += frameSource.Height * base.Zoom;
		}
		base.DrawFrames(spriteBatch, drawColor);
	}

	internal void GoToEquipment(int itemValue)
	{
		Dictionary<int, InventoryEquipment> inventory = base.SaveFile.Inventory.EquipmentInventory.Inventory;
		if (!inventory.ContainsKey(itemValue))
		{
			return;
		}
		InventoryEquipment inventoryEquipment = inventory[itemValue];
		switch (inventoryEquipment.SlotType)
		{
		case EEquipmentSlotType.Head:
			_primaryMenuCollection.SetSelectedIndex(0);
			ChangeMenuCollection(_hatEquipmentInventory, shouldPush: true);
			_hatEquipmentInventory.SelectItem(itemValue);
			break;
		case EEquipmentSlotType.Body:
			_primaryMenuCollection.SetSelectedIndex(1);
			ChangeMenuCollection(_bodyEquipmentInventory, shouldPush: true);
			_bodyEquipmentInventory.SelectItem(itemValue);
			break;
		case EEquipmentSlotType.Trinket:
			if (base.SaveFile.Inventory.EquippedTrinketA == EInventoryEquipmentType.None || base.SaveFile.Inventory.EquippedTrinketB != 0)
			{
				_primaryMenuCollection.SetSelectedIndex(2);
				ChangeMenuCollection(_trinketAEquipmentInventory, shouldPush: true);
				_trinketAEquipmentInventory.SelectItem(itemValue);
			}
			else
			{
				_primaryMenuCollection.SetSelectedIndex(3);
				ChangeMenuCollection(_trinketBEquipmentInventory, shouldPush: true);
				_trinketBEquipmentInventory.SelectItem(itemValue);
			}
			break;
		}
		OnSelectedEntryChanged(_selectedMenuCollection.SelectedIndex);
	}
}
