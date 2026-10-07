using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameStateManagement.ScreenManager;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

namespace Timespinner.GameStateManagement.Screens.PauseMenu;

internal class OrbMenuScreen : InventoryMenuScreen
{
	private const float DisplayStatsPositionRatioX = 0.55f;

	private const float DisplayStatsPositionRatioY = 5f / 32f;

	private const float DisplayStatsWidthRatio = 0.4f;

	private const float OrbSetDisplayFramePositionRatioX = 0.075f;

	private const float OrbSetDisplayFramePositionRatioY = 11f / 96f;

	private const float OrbSetLeftArrowOffsetX = -9f;

	private const float OrbSetRightArrowOffsetX = 12f;

	private const float MenuFrameDrawPositionRatioX = 0.009375f;

	private const float IconsDisplayPositionRatioX = 0.07f;

	private const float IconsDisplayPositionRatioY = 31f / 192f;

	private const float IconsCursorOffsetMultiplierX = -20f;

	private const float PrimaryMenuDisplayRatioX = 21f / 160f;

	private const float StatsBackgroundDrawOffsetY = 7f / 64f;

	private const float StatsBackgroundHeightRatio = 0.4375f;

	private readonly bool _canToggleOrbSets;

	private readonly Action _onExitEquipmentAction;

	private readonly MenuOrbInventory _meleeOrbAInventory;

	private readonly MenuOrbInventory _meleeOrbBInventory;

	private readonly MenuOrbInventory _spellOrbInventory;

	private readonly MenuOrbInventory _passiveOrbInventory;

	private readonly MenuEntry _meleeMenuEntryA;

	private readonly MenuEntry _meleeMenuEntryB;

	private readonly MenuEntry _spellMenuEntry;

	private readonly MenuEntry _passiveMenuEntry;

	private readonly StatCollection _selectedItemStats = new StatCollection();

	private readonly bool[] _iconHighlightList = new bool[4] { true, true, true, true };

	private Point _iconCursorOffset;

	private Vector2 _orbSetDisplayFramePosition;

	private Vector2 _orbSetLeftArrowDisplayFramePosition;

	private Vector2 _orbSetRightArrowDisplayFramePosition;

	private Vector2 _iconDisplayFramePosition;

	private Rectangle _menuBackgroundDrawRectangle;

	private Rectangle _statsBackgroundDrawRectangle;

	private Rectangle _menuFrameDrawRectangle;

	public OrbMenuScreen(GameSave inSave, GCM gcm, Action onExit, Action fullExitAction)
		: base(Loc.Get("OrbMenuTitle"), inSave, gcm, fullExitAction)
	{
		_onExitEquipmentAction = onExit;
		_canToggleOrbSets = base.SaveFile.IsOrbSwitchingUnlocked;
		base.DoesDrawBrackets = true;
		base.DoesDrawBracketsOverAll = false;
		base.DoesDrawTopLowerFrame = true;
		base.DoesDrawScrollbarWidget = true;
		base.StatCollections.Add(_selectedItemStats);
		_meleeMenuEntryA = new MenuEntry(InventoryItem.NameFromType(inSave.Inventory.EquippedMeleeOrbA, EOrbSlot.Melee))
		{
			Description = Loc.Get("OrbMeleeAMenuDesc")
		};
		_meleeMenuEntryB = new MenuEntry(InventoryItem.NameFromType(inSave.Inventory.EquippedMeleeOrbB, EOrbSlot.Melee))
		{
			Description = Loc.Get("OrbMeleeBMenuDesc")
		};
		_spellMenuEntry = new MenuEntry(InventoryItem.NameFromType(inSave.Inventory.EquippedSpellOrb, EOrbSlot.Spell))
		{
			Description = Loc.Get("OrbSpellMenuDesc")
		};
		_passiveMenuEntry = new MenuEntry(InventoryItem.NameFromType(inSave.Inventory.EquippedPassiveOrb, EOrbSlot.Passive))
		{
			Description = Loc.Get("OrbPassiveMenuDesc")
		};
		_meleeMenuEntryA.Selected += MeleeMenuEntryASelected;
		_meleeMenuEntryB.Selected += MeleeMenuEntryBSelected;
		_spellMenuEntry.Selected += SpellMenuEntrySelected;
		_passiveMenuEntry.Selected += PassiveMenuEntrySelected;
		base.MenuEntries.Add(_meleeMenuEntryA);
		base.MenuEntries.Add(_meleeMenuEntryB);
		base.MenuEntries.Add(_spellMenuEntry);
		base.MenuEntries.Add(_passiveMenuEntry);
		_meleeOrbAInventory = new MenuOrbInventory(inSave.Inventory.OrbInventory, inSave.Inventory.RecentlyEquippedMeleeOrbs, MeleeASubMenuItemSelected, MeleeAUnequip, doesAddUnequip: true, EOrbSlot.Melee, base.Sprite)
		{
			IsVisible = true
		};
		_meleeOrbBInventory = new MenuOrbInventory(inSave.Inventory.OrbInventory, inSave.Inventory.RecentlyEquippedMeleeOrbs, MeleeBSubMenuItemSelected, MeleeBUnequip, doesAddUnequip: true, EOrbSlot.Melee, base.Sprite)
		{
			IsVisible = true
		};
		_spellOrbInventory = new MenuOrbInventory(inSave.Inventory.OrbInventory, inSave.Inventory.RecentlyEquippedSpellOrbs, SpellSubMenuItemSelected, SpellUnequip, doesAddUnequip: true, EOrbSlot.Spell, base.Sprite);
		_passiveOrbInventory = new MenuOrbInventory(inSave.Inventory.OrbInventory, inSave.Inventory.RecentlyEquippedPassiveOrbs, PassiveSubMenuItemSelected, PassiveUnequip, doesAddUnequip: true, EOrbSlot.Passive, base.Sprite);
		_subMenuCollections.Add(_meleeOrbAInventory);
		_subMenuCollections.Add(_meleeOrbBInventory);
		_subMenuCollections.Add(_spellOrbInventory);
		_subMenuCollections.Add(_passiveOrbInventory);
		RefreshAllEquippedOrbIcons();
	}

	internal override void RefreshSizes()
	{
		base.RefreshSizes();
		int num = (int)(7f / 64f * (float)_topSectionHeight);
		_menuBackgroundDrawRectangle = new Rectangle(_screenLeft + 2 * base.Zoom, _screenTop + num, (int)Math.Ceiling((float)_screenWidth / 2f - (float)(2 * base.Zoom)), (int)(0.4375f * (float)_topSectionHeight));
		_statsBackgroundDrawRectangle = new Rectangle(_menuBackgroundDrawRectangle.Right, _menuBackgroundDrawRectangle.Top, _menuBackgroundDrawRectangle.Width, _menuBackgroundDrawRectangle.Height);
		_menuFrameDrawRectangle = new Rectangle(_screenLeft + (int)(0.009375f * (float)_screenWidth), _screenTop + (int)(23f / 192f * (float)_topSectionHeight), (int)(0.490625f * (float)_screenWidth), (int)(27f / 64f * (float)_topSectionHeight));
		_primaryMenuCollection.DrawPosition = new Vector2(21f / 160f * (float)_screenWidth + (float)_screenLeft, _primaryMenuCollection.DrawPosition.Y);
		_primaryMenuCollection.SetColumnWidth(base.NarrowListColumnWidth, base.Zoom);
		int num2 = (Loc.IsAsianLocale ? (-2) : 0);
		_selectedItemStats.Location = new Vector2(0.55f * (float)_screenWidth + (float)_screenLeft, (float)_screenTop + 5f / 32f * (float)_topSectionHeight + (float)(num2 * base.Zoom));
		_selectedItemStats.Width = (int)(0.4f * (float)_screenWidth);
		_meleeOrbAInventory.DrawPosition = base.ListTextDrawPosition;
		_meleeOrbAInventory.SetColumnWidth(base.ListColumnWidth, base.Zoom);
		_meleeOrbBInventory.DrawPosition = base.ListTextDrawPosition;
		_meleeOrbBInventory.SetColumnWidth(base.ListColumnWidth, base.Zoom);
		_spellOrbInventory.DrawPosition = base.ListTextDrawPosition;
		_spellOrbInventory.SetColumnWidth(base.ListColumnWidth, base.Zoom);
		_passiveOrbInventory.DrawPosition = base.ListTextDrawPosition;
		_passiveOrbInventory.SetColumnWidth(base.ListColumnWidth, base.Zoom);
		_iconCursorOffset = new Point((int)(-20f * (float)base.Zoom), 0);
		base.CursorOffset = ((_selectedMenuCollection == _primaryMenuCollection) ? _iconCursorOffset : Point.Zero);
		_orbSetDisplayFramePosition = new Vector2(0.075f * (float)_screenWidth + (float)_screenLeft, (float)_screenTop + 11f / 96f * (float)_topSectionHeight);
		_orbSetLeftArrowDisplayFramePosition = new Vector2(_orbSetDisplayFramePosition.X + -9f * (float)base.Zoom, _orbSetDisplayFramePosition.Y);
		_orbSetRightArrowDisplayFramePosition = new Vector2(_orbSetDisplayFramePosition.X + 12f * (float)base.Zoom, _orbSetDisplayFramePosition.Y);
		_iconDisplayFramePosition = new Vector2(0.07f * (float)_screenWidth + (float)_screenLeft, (float)_screenTop + 31f / 192f * (float)_topSectionHeight);
		RefreshDisplayStats();
	}

	public override void HandleInput(InputState input)
	{
		if (_canToggleOrbSets)
		{
			int num = base.SaveFile.Inventory.EquippedOrbSetIndex;
			if (input.IsNewPressPageRight(base.ControllingPlayer))
			{
				num = (num + 1) % 3;
			}
			else if (input.IsNewPressPageLeft(base.ControllingPlayer))
			{
				num = (num - 1) % 3;
				if (num < 0)
				{
					num += 3;
				}
			}
			if (base.SaveFile.Inventory.EquippedOrbSetIndex != num)
			{
				base.SaveFile.Inventory.EquippedOrbSetIndex = num;
				base.SaveFile.Inventory.RefreshEquippedOrbs();
				_meleeMenuEntryA.SetText(InventoryItem.NameFromType(base.SaveFile.Inventory.EquippedMeleeOrbA, EOrbSlot.Melee));
				_meleeMenuEntryB.SetText(InventoryItem.NameFromType(base.SaveFile.Inventory.EquippedMeleeOrbB, EOrbSlot.Melee));
				_spellMenuEntry.SetText(InventoryItem.NameFromType(base.SaveFile.Inventory.EquippedSpellOrb, EOrbSlot.Spell));
				_passiveMenuEntry.SetText(InventoryItem.NameFromType(base.SaveFile.Inventory.EquippedPassiveOrb, EOrbSlot.Passive));
				RefreshAllEquippedOrbIcons();
				RefreshDisplayStats();
			}
		}
		base.HandleInput(input);
	}

	private void RefreshAllEquippedOrbIcons()
	{
		RefreshOrbEquipped(_meleeOrbAInventory, base.SaveFile.Inventory.EquippedMeleeOrbA);
		RefreshOrbEquipped(_meleeOrbBInventory, base.SaveFile.Inventory.EquippedMeleeOrbB);
		RefreshOrbEquipped(_spellOrbInventory, base.SaveFile.Inventory.EquippedSpellOrb);
		RefreshOrbEquipped(_passiveOrbInventory, base.SaveFile.Inventory.EquippedPassiveOrb);
	}

	private void MeleeMenuEntryASelected(object sender, PlayerIndexEventArgs e)
	{
		if (_meleeOrbAInventory.Entries.Count > 0)
		{
			ChangeMenuCollection(_meleeOrbAInventory, shouldPush: true);
		}
	}

	private void MeleeMenuEntryBSelected(object sender, PlayerIndexEventArgs e)
	{
		if (_meleeOrbBInventory.Entries.Count > 0)
		{
			ChangeMenuCollection(_meleeOrbBInventory, shouldPush: true);
		}
	}

	private void SpellMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		if (_spellOrbInventory.Entries.Count > 0)
		{
			ChangeMenuCollection(_spellOrbInventory, shouldPush: true);
		}
	}

	private void PassiveMenuEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		if (_passiveOrbInventory.Entries.Count > 0)
		{
			ChangeMenuCollection(_passiveOrbInventory, shouldPush: true);
		}
	}

	private void MeleeASubMenuItemSelected(InventoryOrb item)
	{
		base.SaveFile.Inventory.EquippedOrbSet.MeleeOrbA = item.OrbType;
		base.SaveFile.Inventory.AddToRecentlyEquipped(item.OrbType, EOrbSlot.Melee);
		_meleeOrbAInventory.AddRecentEntries(base.SaveFile.Inventory.RecentlyEquippedMeleeOrbs);
		_meleeOrbBInventory.AddRecentEntries(base.SaveFile.Inventory.RecentlyEquippedMeleeOrbs);
		_meleeMenuEntryA.SetText(InventoryItem.NameFromType(item.OrbType, EOrbSlot.Melee));
		RefreshOrbEquipped(_meleeOrbAInventory, item.OrbType);
		_meleeOrbAInventory.Update(0f, isScreenActive: true, 0f);
		_meleeOrbBInventory.Update(0f, isScreenActive: true, 0f);
		OnEquipItem();
	}

	private void MeleeBSubMenuItemSelected(InventoryOrb item)
	{
		base.SaveFile.Inventory.EquippedOrbSet.MeleeOrbB = item.OrbType;
		base.SaveFile.Inventory.AddToRecentlyEquipped(item.OrbType, EOrbSlot.Melee);
		_meleeOrbAInventory.AddRecentEntries(base.SaveFile.Inventory.RecentlyEquippedMeleeOrbs);
		_meleeOrbBInventory.AddRecentEntries(base.SaveFile.Inventory.RecentlyEquippedMeleeOrbs);
		_meleeMenuEntryB.SetText(InventoryItem.NameFromType(item.OrbType, EOrbSlot.Melee));
		RefreshOrbEquipped(_meleeOrbBInventory, item.OrbType);
		_meleeOrbAInventory.Update(0f, isScreenActive: true, 0f);
		_meleeOrbBInventory.Update(0f, isScreenActive: true, 0f);
		OnEquipItem();
	}

	private void SpellSubMenuItemSelected(InventoryOrb item)
	{
		base.SaveFile.Inventory.EquippedOrbSet.SpellOrb = item.OrbType;
		base.SaveFile.Inventory.AddToRecentlyEquipped(item.OrbType, EOrbSlot.Spell);
		_spellOrbInventory.AddRecentEntries(base.SaveFile.Inventory.RecentlyEquippedSpellOrbs);
		_spellMenuEntry.SetText(InventoryItem.NameFromType(item.OrbType, EOrbSlot.Spell));
		RefreshOrbEquipped(_spellOrbInventory, item.OrbType);
		_spellOrbInventory.Update(0f, isScreenActive: true, 0f);
		OnEquipItem();
	}

	private void PassiveSubMenuItemSelected(InventoryOrb item)
	{
		base.SaveFile.Inventory.EquippedOrbSet.PassiveOrb = item.OrbType;
		base.SaveFile.Inventory.AddToRecentlyEquipped(item.OrbType, EOrbSlot.Passive);
		_passiveOrbInventory.AddRecentEntries(base.SaveFile.Inventory.RecentlyEquippedPassiveOrbs);
		_passiveMenuEntry.SetText(InventoryItem.NameFromType(item.OrbType, EOrbSlot.Passive));
		RefreshOrbEquipped(_passiveOrbInventory, item.OrbType);
		_passiveOrbInventory.Update(0f, isScreenActive: true, 0f);
		OnEquipItem();
	}

	private void OnEquipItem()
	{
		base.ScreenManager.Jukebox.PlayCue(ESFX.MenuEquip);
		GoToPreviousMenuCollection();
	}

	private void MeleeAUnequip()
	{
		base.SaveFile.Inventory.EquippedOrbSet.MeleeOrbA = EInventoryOrbType.None;
		_meleeMenuEntryA.SetText("---");
		RefreshOrbEquipped(_meleeOrbAInventory, EInventoryOrbType.None);
		OnUnequipItem();
	}

	private void MeleeBUnequip()
	{
		base.SaveFile.Inventory.EquippedOrbSet.MeleeOrbB = EInventoryOrbType.None;
		_meleeMenuEntryB.SetText("---");
		RefreshOrbEquipped(_meleeOrbBInventory, EInventoryOrbType.None);
		OnUnequipItem();
	}

	private void SpellUnequip()
	{
		base.SaveFile.Inventory.EquippedOrbSet.SpellOrb = EInventoryOrbType.None;
		_spellMenuEntry.SetText("---");
		RefreshOrbEquipped(_spellOrbInventory, EInventoryOrbType.None);
		OnUnequipItem();
	}

	private void PassiveUnequip()
	{
		base.SaveFile.Inventory.EquippedOrbSet.PassiveOrb = EInventoryOrbType.None;
		_passiveMenuEntry.SetText("---");
		RefreshOrbEquipped(_passiveOrbInventory, EInventoryOrbType.None);
		OnUnequipItem();
	}

	private void OnUnequipItem()
	{
		base.ScreenManager.Jukebox.PlayCue(ESFX.MenuCancel);
		GoToPreviousMenuCollection();
	}

	private void RefreshOrbEquipped(MenuOrbInventory targetInventory, EInventoryOrbType equippedType)
	{
		targetInventory.EquippedOrb = equippedType;
	}

	protected override void OnSelectedEntryChanged(int entryIndex)
	{
		base.OnSelectedEntryChanged(entryIndex);
		_meleeOrbAInventory.IsVisible = _selectedMenuCollection == _meleeOrbAInventory || (_selectedMenuCollection == _primaryMenuCollection && entryIndex == 0);
		_meleeOrbBInventory.IsVisible = _selectedMenuCollection == _meleeOrbBInventory || (_selectedMenuCollection == _primaryMenuCollection && entryIndex == 1);
		_spellOrbInventory.IsVisible = _selectedMenuCollection == _spellOrbInventory || (_selectedMenuCollection == _primaryMenuCollection && entryIndex == 2);
		_passiveOrbInventory.IsVisible = _selectedMenuCollection == _passiveOrbInventory || (_selectedMenuCollection == _primaryMenuCollection && entryIndex == 3);
		bool flag = _selectedMenuCollection == _primaryMenuCollection;
		_iconHighlightList[0] = flag || _selectedMenuCollection == _meleeOrbAInventory;
		_iconHighlightList[1] = flag || _selectedMenuCollection == _meleeOrbBInventory;
		_iconHighlightList[2] = flag || _selectedMenuCollection == _spellOrbInventory;
		_iconHighlightList[3] = flag || _selectedMenuCollection == _passiveOrbInventory;
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
		if (_selectedMenuCollection == _meleeOrbAInventory)
		{
			AddStatEntries(_meleeOrbAInventory.GetSelected(), EOrbSlot.Melee);
			return;
		}
		if (_selectedMenuCollection == _meleeOrbBInventory)
		{
			AddStatEntries(_meleeOrbBInventory.GetSelected(), EOrbSlot.Melee);
			return;
		}
		if (_selectedMenuCollection == _spellOrbInventory)
		{
			AddStatEntries(_spellOrbInventory.GetSelected(), EOrbSlot.Spell);
			return;
		}
		if (_selectedMenuCollection == _passiveOrbInventory)
		{
			AddStatEntries(_passiveOrbInventory.GetSelected(), EOrbSlot.Passive);
			return;
		}
		switch (base.SelectedIndex)
		{
		case 0:
			AddStatEntries(base.SaveFile.Inventory.OrbInventory.GetItem((int)base.SaveFile.Inventory.EquippedOrbSet.MeleeOrbA), EOrbSlot.Melee);
			break;
		case 1:
			AddStatEntries(base.SaveFile.Inventory.OrbInventory.GetItem((int)base.SaveFile.Inventory.EquippedOrbSet.MeleeOrbB), EOrbSlot.Melee);
			break;
		case 2:
			AddStatEntries(base.SaveFile.Inventory.OrbInventory.GetItem((int)base.SaveFile.Inventory.EquippedOrbSet.SpellOrb), EOrbSlot.Spell);
			break;
		case 3:
			AddStatEntries(base.SaveFile.Inventory.OrbInventory.GetItem((int)base.SaveFile.Inventory.EquippedOrbSet.PassiveOrb), EOrbSlot.Passive);
			break;
		}
	}

	private void AddStatEntries(InventoryOrb orb, EOrbSlot slot)
	{
		if (orb == null)
		{
			return;
		}
		int willpower = base.SaveFile.CharacterStats.Willpower;
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
		string orbNameBySlot = InventoryItem.GetOrbNameBySlot(orb, EOrbSlot.Melee);
		_selectedItemStats.Entries.Add(new StatEntry
		{
			Title = orbNameBySlot,
			Type = StatEntry.EStatDisplayType.IconOnly,
			IconIndex = (int)orb.OrbType
		});
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
		_selectedItemStats.Entries.Add(new StatEntry
		{
			Title = Loc.Get("StatKills"),
			Type = StatEntry.EStatDisplayType.Number,
			Value = orb.Experience
		});
		_selectedItemStats.Entries.Add(new StatEntry
		{
			Title = Loc.Get("StatNextLevel"),
			Type = StatEntry.EStatDisplayType.Number,
			Value = orb.NextLevel
		});
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
		if (_canToggleOrbSets)
		{
			int equippedOrbSetIndex = base.SaveFile.Inventory.EquippedOrbSetIndex;
			Rectangle frameSource = base.Sprite.GetFrameSource(105 + equippedOrbSetIndex);
			spriteBatch.Draw(base.Sprite.Texture, _orbSetDisplayFramePosition, frameSource, drawColor, 0f, Vector2.Zero, base.Zoom, SpriteEffects.None, 0f);
			frameSource = base.Sprite.GetFrameSource(108);
			spriteBatch.Draw(base.Sprite.Texture, _orbSetLeftArrowDisplayFramePosition, frameSource, drawColor, 0f, Vector2.Zero, base.Zoom, SpriteEffects.None, 0f);
			frameSource = base.Sprite.GetFrameSource(109);
			spriteBatch.Draw(base.Sprite.Texture, _orbSetRightArrowDisplayFramePosition, frameSource, drawColor, 0f, Vector2.Zero, base.Zoom, SpriteEffects.None, 0f);
		}
		Vector2 iconDisplayFramePosition = _iconDisplayFramePosition;
		for (int i = 0; i < 4; i++)
		{
			int index = i + 65 + ((i > 0) ? (-1) : 0);
			Rectangle frameSource = base.Sprite.GetFrameSource(index);
			Color color = ((!_iconHighlightList[i]) ? new Color(drawColor.R / 3, drawColor.G / 3, drawColor.B / 3, drawColor.A) : drawColor);
			spriteBatch.Draw(base.Sprite.Texture, iconDisplayFramePosition, frameSource, color, 0f, Vector2.Zero, base.Zoom, SpriteEffects.None, 0f);
			iconDisplayFramePosition.Y += frameSource.Height * base.Zoom;
		}
		base.DrawFrames(spriteBatch, drawColor);
	}
}
