using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;
using Timespinner.GameStateManagement.Screens.InGame;

namespace Timespinner.GameObjects.Events;

public sealed class TreasureChestEvent : GameEvent
{
	private enum ETreasureAppearanceType
	{
		Ancient,
		Normal,
		Rare,
		Relic,
		Equipment,
		FutureNormal
	}

	private enum ETreasureStatBoostType
	{
		HP,
		Aura,
		Sand
	}

	private const int ChestTypeIndexOffset = 7;

	private const float TimeToWaitForLoot = 0.4f;

	private const float TimeBeforeShowingToast = 0.1f;

	private const float NormalRarityThreshold = 0.9f;

	private const float CoinRarityThreshold = 0.75f;

	private const string TreasureChestSaveKey = "Chest{0},{1},{2},{3},{4}";

	private const string LotteryIsFoundSaveKey = "LotteryFound{0},{1},{2},{3},{4}";

	private const string LotteryIsRareSaveKey = "LotteryIsRare{0},{1},{2},{3},{4}";

	private const string PrefixUseItem = "Use: ";

	private const string PrefixEquipment = "Equipment: ";

	private const string PrefixSpecial = "Special: Area";

	private const string PrefixRelic = "Relic: ";

	private const string PrefixStat = "Stat: ";

	private const string PrefixOrb = "Orb: ";

	private const string PostfixSpell = "Spell: ";

	private const string PostfixPassive = "Passive: ";

	private const string PrefixFamiliar = "Familiar: ";

	private readonly bool _isLotteryChest;

	private readonly bool _isRareLotteryChest;

	private readonly ETreasureAppearanceType _treasureAppearanceType;

	private readonly ETreasureStatBoostType _lootStatBoostType;

	private readonly EInventoryRelicType _lootRelicType;

	private readonly EInventoryOrbType _lootOrbType;

	private readonly EOrbSlot _lootOrbSlot;

	private readonly int _animationIndexOffset;

	private readonly Appendage _lidAppendage;

	private readonly ItemPopupAppendage _itemPopupAppendage;

	private readonly ObjectTileSpecification _objectSpec;

	private bool _isOpened;

	private bool _hasDroppedLoot;

	private ETreasureLootType _treasureLootType;

	private EInventoryEquipmentType _lootEquipmentType;

	private EInventoryUseItemType _lootUseItemType;

	private EInventoryFamiliarType _lootFamiliarType;

	private EToastType _toastToShow;

	private float _openTimer;

	private float _toastShowTimer;

	private Protagonist _chestOpener;

	public Point LootEmissionPoint => new Point(Position.X, Bbox.Top + 3);

	public TreasureChestEvent(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_objectSpec = objectSpec;
		_sprite = _level.GCM.SpTreasureChests;
		_bbox = new Rectangle(0, 0, 27, 13);
		Position = new Point(_position.X, _position.Y);
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		_doesPersist = true;
		_isAffectedByGravity = false;
		_isRepeatedTrigger = false;
		_isAffectedByTime = true;
		_doAppendagesInheritDrawColor = false;
		_toastToShow = EToastType.None;
		Point point = DeMuxArgument(objectSpec.Argument);
		switch (point.X)
		{
		case 0:
			_isLotteryChest = true;
			if (_level.GetLevelSaveBool(GetLotteryIsFoundKey()))
			{
				_isRareLotteryChest = _level.GetLevelSaveBool(GetLotteryIsRareKey());
			}
			else
			{
				_level.SetLevelSaveBool(GetLotteryIsFoundKey(), value: true);
				if (_level.GameSave.IsSpeedrunAActive)
				{
					_isRareLotteryChest = true;
				}
				else
				{
					_isRareLotteryChest = DoLotteryRarityRoll(_level.GameSave.Inventory);
				}
				_level.SetLevelSaveBool(GetLotteryIsRareKey(), _isRareLotteryChest);
			}
			_treasureAppearanceType = ((!_isRareLotteryChest) ? ETreasureAppearanceType.Normal : ETreasureAppearanceType.Rare);
			PickLotteryItem(_treasureAppearanceType != ETreasureAppearanceType.Normal);
			break;
		case 1:
			_treasureLootType = ETreasureLootType.UseItem;
			_lootUseItemType = (EInventoryUseItemType)point.Y;
			if (_lootUseItemType == EInventoryUseItemType.EssenceCrystal || _lootUseItemType == EInventoryUseItemType.GoldNecklace || _lootUseItemType == EInventoryUseItemType.GoldRing || _lootUseItemType == EInventoryUseItemType.MagicMarbles || _lootUseItemType == EInventoryUseItemType.GalaxyStone)
			{
				_treasureAppearanceType = ETreasureAppearanceType.Equipment;
			}
			else
			{
				_treasureAppearanceType = ETreasureAppearanceType.Normal;
			}
			break;
		case 2:
			_treasureAppearanceType = ETreasureAppearanceType.Equipment;
			_treasureLootType = ETreasureLootType.Equipment;
			_lootEquipmentType = (EInventoryEquipmentType)point.Y;
			break;
		case 3:
			_treasureAppearanceType = ETreasureAppearanceType.Relic;
			_treasureLootType = ETreasureLootType.Relic;
			_lootRelicType = (EInventoryRelicType)point.Y;
			break;
		case 4:
			_treasureAppearanceType = ETreasureAppearanceType.Ancient;
			_treasureLootType = ETreasureLootType.Stat;
			_lootStatBoostType = (ETreasureStatBoostType)point.Y;
			break;
		case 5:
		{
			_treasureAppearanceType = ETreasureAppearanceType.Equipment;
			_treasureLootType = ETreasureLootType.Orb;
			_lootOrbSlot = ((point.Y % 2 == 0) ? EOrbSlot.Spell : EOrbSlot.Passive);
			_lootOrbType = (EInventoryOrbType)(point.Y / 2);
			bool flag = false;
			int lootOrbType = (int)_lootOrbType;
			if (_level.GameSave.Inventory.OrbInventory.Inventory.ContainsKey(lootOrbType))
			{
				InventoryOrb inventoryOrb = _level.GameSave.Inventory.OrbInventory.Inventory[lootOrbType];
				if ((inventoryOrb.IsPassiveUnlocked && _lootOrbSlot == EOrbSlot.Passive) || (inventoryOrb.IsSpellUnlocked && _lootOrbSlot == EOrbSlot.Spell))
				{
					flag = true;
				}
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				_treasureLootType = ETreasureLootType.UseItem;
				_lootUseItemType = ((_lootOrbSlot == EOrbSlot.Passive) ? EInventoryUseItemType.GoldRing : EInventoryUseItemType.GoldNecklace);
			}
			break;
		}
		case 6:
			_treasureAppearanceType = ETreasureAppearanceType.Ancient;
			_treasureLootType = ETreasureLootType.Familiar;
			_lootFamiliarType = (EInventoryFamiliarType)point.Y;
			break;
		case 7:
			_isLotteryChest = true;
			switch (point.Y)
			{
			case 0:
				_isRareLotteryChest = false;
				_treasureAppearanceType = ETreasureAppearanceType.Normal;
				PickLotteryItem(isRareItem: false);
				break;
			case 1:
				_isRareLotteryChest = true;
				_treasureAppearanceType = ETreasureAppearanceType.Rare;
				PickLotteryItem(isRareItem: true);
				break;
			case 2:
				_isRareLotteryChest = false;
				_treasureAppearanceType = ETreasureAppearanceType.Normal;
				_treasureLootType = ETreasureLootType.UseItem;
				_lootUseItemType = EInventoryUseItemType.WarpCard;
				break;
			}
			break;
		}
		if (_treasureAppearanceType == ETreasureAppearanceType.Normal && Level.GetEraByLevelID(_level.ID) == EEraType.Present && _level.ID != 9)
		{
			_treasureAppearanceType = ETreasureAppearanceType.FutureNormal;
		}
		bool flag2 = _treasureAppearanceType == ETreasureAppearanceType.FutureNormal;
		_animationIndexOffset = Math.Min((int)_treasureAppearanceType * 7, 35);
		ChangeAnimation(_animationIndexOffset);
		_lidAppendage = new Appendage(this, new Point(27, 10), Point.Zero, _level, _sprite)
		{
			FollowType = EAppendageFollowType.ParentObjectLocked,
			AnchorOffset = new Point(0, flag2 ? (-20) : (-19)),
			DrawPriority = 1,
			IsFacingLeft = IsFacingLeft
		};
		_lidAppendage.ChangeAnimation(_animationIndexOffset + 1, 1, 1f, EAnimationType.None);
		_appendages.Add(_lidAppendage);
		_isOpened = (_isLotteryChest ? _level.GetLevelSaveBool(GetSaveKey()) : _level.GameSave.GetSaveBool(GetSaveKey()));
		_hasDroppedLoot = _isOpened;
		_itemPopupAppendage = new ItemPopupAppendage(this, _level, _level.GCM.SpMenuIcons)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			ShouldPlaySFX = (_treasureLootType != ETreasureLootType.Orb && _treasureLootType != ETreasureLootType.Relic && _treasureLootType != ETreasureLootType.Stat)
		};
		_appendages.Add(_itemPopupAppendage);
		if (_isOpened)
		{
			_lidAppendage.ChangeAnimation(_animationIndexOffset + 5, 1, 1f, EAnimationType.None);
		}
	}

	private string GetSaveKey()
	{
		return $"Chest{_level.ID},{_level.RoomID},{_objectSpec.Argument},{_objectSpec.X},{_objectSpec.Y}";
	}

	private string GetLotteryIsFoundKey()
	{
		return $"LotteryFound{_level.ID},{_level.RoomID},{_objectSpec.Argument},{_objectSpec.X},{_objectSpec.Y}";
	}

	private string GetLotteryIsRareKey()
	{
		return $"LotteryIsRare{_level.ID},{_level.RoomID},{_objectSpec.Argument},{_objectSpec.X},{_objectSpec.Y}";
	}

	public override void Initialize()
	{
		Update(0f);
		base.Initialize();
	}

	public override bool TriggerEvent(Alive who, Vector2 depth)
	{
		if (!base.IsFrozen && !_isOpened && !base.IsTriggerableByMonsters)
		{
			Protagonist protagonist = who as Protagonist;
			_isTriggered = true;
			_level.RequestButtonPrompt(4, new Point(Bbox.Center.X, Bbox.Top));
			if (protagonist != null && protagonist.CheckButton(4) && !protagonist.CheckButton(5) && !protagonist.CheckButton(7))
			{
				OpenChest(protagonist);
				return false;
			}
		}
		return base.TriggerEvent(who, depth);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if ((!_isTriggered || _isOpened) && _isOpened && !_hasDroppedLoot)
			{
				_openTimer += delta;
				if (_openTimer >= 0.4f)
				{
					DropLoot();
				}
			}
			if (_toastToShow != 0 && _toastShowTimer > 0f)
			{
				_toastShowTimer -= delta;
				if (_toastShowTimer <= 0f)
				{
					_level.RequestToastPopup(_toastToShow, 0);
					_toastToShow = EToastType.None;
					_toastShowTimer = 0f;
				}
			}
			base.Update(delta);
			_isTriggered = false;
		}
		else
		{
			UpdateIsWithinObjectVisibleArea();
		}
	}

	private void OpenChest(Protagonist opener)
	{
		_isOpened = true;
		_lidAppendage.ChangeAnimation(_animationIndexOffset + 5, 1, 1f, EAnimationType.None, _animationIndexOffset + 2, 5, 0.07f);
		PlayCue(ESFX.FoleyTreasureOpen, Position);
		_chestOpener = opener;
	}

	private void DropLoot()
	{
		_hasDroppedLoot = true;
		int start = 0;
		_isAffectedByTime = false;
		_isFrozen = false;
		EToastType eToastType = EToastType.None;
		switch (_treasureLootType)
		{
		case ETreasureLootType.Stat:
			switch (_lootStatBoostType)
			{
			case ETreasureStatBoostType.HP:
				_chestOpener.GetPowerup(EItemType.MaxHP, 1f);
				start = Math.Max(24, 0);
				eToastType = EToastType.Health;
				break;
			case ETreasureStatBoostType.Aura:
				_chestOpener.GetPowerup(EItemType.MaxAura, 1f);
				start = Math.Max(25, 0);
				eToastType = EToastType.Aura;
				break;
			case ETreasureStatBoostType.Sand:
				_chestOpener.GetPowerup(EItemType.MaxSand, 1f);
				start = Math.Max(26, 0);
				eToastType = EToastType.Sand;
				break;
			}
			break;
		case ETreasureLootType.Equipment:
			_level.GameSave.Inventory.AddItem(_lootEquipmentType);
			start = Math.Max((int)(InventoryItem.GetIconFromItem(_lootEquipmentType) - 1), 0);
			_level.RequestItemGetPopup(_lootEquipmentType);
			break;
		case ETreasureLootType.UseItem:
			_level.GameSave.Inventory.AddItem(_lootUseItemType, 1);
			start = Math.Max((int)(InventoryItem.GetIconFromItem(_lootUseItemType) - 1), 0);
			_level.RequestItemGetPopup(_lootUseItemType);
			NPCBase.TryShowQuestFinishedPopupByUseItem(_level, _lootUseItemType);
			break;
		case ETreasureLootType.Relic:
			_level.UnlockRelic(_lootRelicType);
			start = Math.Max((int)(InventoryItem.GetIconFromItem(_lootRelicType) - 1), 0);
			AddWaitScript(0.625f);
			_level.AddScript(new ScriptAction(_lootRelicType));
			break;
		case ETreasureLootType.Orb:
		{
			int lootOrbType = (int)_lootOrbType;
			if (_level.GameSave.Inventory.OrbInventory.Inventory.ContainsKey(lootOrbType))
			{
				switch (_lootOrbSlot)
				{
				case EOrbSlot.Spell:
					_level.GameSave.Inventory.OrbInventory.Inventory[lootOrbType].IsSpellUnlocked = true;
					break;
				case EOrbSlot.Passive:
					_level.GameSave.Inventory.OrbInventory.Inventory[lootOrbType].IsPassiveUnlocked = true;
					break;
				}
			}
			start = Math.Max((int)(InventoryItem.GetIconFromItem(_lootOrbType, _lootOrbSlot) - 1), 0);
			AddWaitScript(0.625f);
			_level.AddScript(new ScriptAction(_lootOrbType, _lootOrbSlot));
			break;
		}
		case ETreasureLootType.Familiar:
			_level.GameSave.GiveFamiliar(_lootFamiliarType);
			start = Math.Max((int)(InventoryItem.GetIconFromItem(_lootFamiliarType) - 1), 0);
			AddWaitScript(0.625f);
			_level.AddScript(new ScriptAction(_lootFamiliarType));
			break;
		}
		_itemPopupAppendage.ChangeAnimation(start);
		_itemPopupAppendage.IsPopppingUp = true;
		_lidAppendage.DrawPriority = 0;
		if (_isLotteryChest)
		{
			_level.SetLevelSaveBool(GetSaveKey(), value: true);
		}
		else
		{
			_level.GameSave.SetValue(GetSaveKey(), value: true);
		}
		if (eToastType != 0)
		{
			_toastToShow = eToastType;
			_toastShowTimer = 0.1f;
		}
	}

	public static IEnumerable<Tuple<string, int>> GetAllOptionTypes()
	{
		List<Tuple<string, int>> list = new List<Tuple<string, int>>();
		list.Add(new Tuple<string, int>("Special: Area", 0));
		foreach (ETreasureStatBoostType value in Enum.GetValues(typeof(ETreasureStatBoostType)))
		{
			list.Add(new Tuple<string, int>("Stat: " + value, MuxArgument(value)));
		}
		foreach (EInventoryRelicType value2 in Enum.GetValues(typeof(EInventoryRelicType)))
		{
			list.Add(new Tuple<string, int>("Relic: " + value2, MuxArgument(value2)));
		}
		foreach (EInventoryUseItemType value3 in Enum.GetValues(typeof(EInventoryUseItemType)))
		{
			list.Add(new Tuple<string, int>("Use: " + value3, MuxArgument(value3)));
		}
		foreach (EInventoryEquipmentType value4 in Enum.GetValues(typeof(EInventoryEquipmentType)))
		{
			if (value4 != 0)
			{
				list.Add(new Tuple<string, int>("Equipment: " + value4, MuxArgument(value4)));
			}
		}
		foreach (EInventoryOrbType value5 in Enum.GetValues(typeof(EInventoryOrbType)))
		{
			if (value5 != 0)
			{
				list.Add(new Tuple<string, int>(string.Concat("Orb: ", value5, "Spell: "), MuxArgument(value5, EOrbSlot.Spell)));
				list.Add(new Tuple<string, int>(string.Concat("Orb: ", value5, "Passive: "), MuxArgument(value5, EOrbSlot.Passive)));
			}
		}
		foreach (EInventoryFamiliarType value6 in Enum.GetValues(typeof(EInventoryFamiliarType)))
		{
			if (value6 != 0)
			{
				list.Add(new Tuple<string, int>("Familiar: " + value6, MuxArgument(value6)));
			}
		}
		for (int i = 0; i < 3; i++)
		{
			int item = (i << 3) | 7;
			string item2 = "";
			switch (i)
			{
			case 0:
				item2 = "Gyre: Common";
				break;
			case 1:
				item2 = "Gyre: Rare";
				break;
			case 2:
				item2 = "Gyre: Warp";
				break;
			}
			list.Add(new Tuple<string, int>(item2, item));
		}
		return list;
	}

	public static IEnumerable<Tuple<string, int>> GetAllJournalOptionTypes()
	{
		List<Tuple<string, int>> list = new List<Tuple<string, int>>();
		foreach (EInventoryJournalType value in Enum.GetValues(typeof(EInventoryJournalType)))
		{
			string item = InventoryItem.NameFromType(value);
			list.Add(new Tuple<string, int>(item, (int)value));
		}
		return list;
	}

	internal static int MuxArgument(EInventoryUseItemType item)
	{
		return ((int)item << 3) | 1;
	}

	internal static int MuxArgument(EInventoryEquipmentType equipment)
	{
		return ((int)equipment << 3) | 2;
	}

	internal static int MuxArgument(EInventoryRelicType relic)
	{
		return ((int)relic << 3) | 3;
	}

	private static int MuxArgument(ETreasureStatBoostType stat)
	{
		return ((int)stat << 3) | 4;
	}

	internal static int MuxArgument(EInventoryOrbType orb, EOrbSlot slot)
	{
		return ((int)orb * 2 + ((slot == EOrbSlot.Passive) ? 1 : 0) << 3) | 5;
	}

	internal static int MuxArgument(EInventoryFamiliarType familiar)
	{
		return ((int)familiar << 3) | 6;
	}

	internal static Point DeMuxArgument(int argument)
	{
		int x = argument & 7;
		int y = argument >> 3;
		return new Point(x, y);
	}

	private bool DoLotteryRarityRoll(PlayerInventory inventory)
	{
		float num = 0.9f;
		if (inventory.EquippedTrinketA == EInventoryEquipmentType.LuckyCoin || inventory.EquippedTrinketB == EInventoryEquipmentType.LuckyCoin)
		{
			num = 0.75f;
		}
		return _level.NextRandomDouble() > (double)num;
	}

	private void PickLotteryItem(bool isRareItem)
	{
		double roll = _level.NextRandomDouble();
		LotteryTable lotteryTable = new LotteryTable();
		if (!isRareItem)
		{
			switch (_level.ID)
			{
			case 1:
				lotteryTable.Add(EInventoryUseItemType.FutureEther, 0.15f);
				lotteryTable.Add(EInventoryUseItemType.FuturePotion, 1f);
				break;
			case 2:
				lotteryTable.Add(EInventoryUseItemType.FutureEther, 0.25f);
				lotteryTable.Add(EInventoryUseItemType.FuturePotion, 1f);
				break;
			case 3:
				lotteryTable.Add(EInventoryUseItemType.Herb, 0.1f);
				lotteryTable.Add(EInventoryUseItemType.Antidote, 0.35f);
				lotteryTable.Add(EInventoryUseItemType.Potion, 1f);
				break;
			case 4:
				lotteryTable.Add(EInventoryUseItemType.Ether, 0.05f);
				lotteryTable.Add(EInventoryUseItemType.Drumstick, 0.1f);
				lotteryTable.Add(EInventoryUseItemType.Antidote, 0.25f);
				lotteryTable.Add(EInventoryUseItemType.Potion, 1f);
				break;
			case 5:
				lotteryTable.Add(EInventoryUseItemType.SandBottle, 0.05f);
				lotteryTable.Add(EInventoryUseItemType.Ether, 0.15f);
				lotteryTable.Add(EInventoryUseItemType.ChaosHeal, 0.3f);
				lotteryTable.Add(EInventoryUseItemType.Potion, 1f);
				break;
			case 6:
				lotteryTable.Add(EInventoryUseItemType.HiPotion, 0.05f);
				lotteryTable.Add(EInventoryUseItemType.SandBottle, 0.15f);
				lotteryTable.Add(EInventoryUseItemType.Ether, 0.25f);
				lotteryTable.Add(EInventoryUseItemType.ChaosHeal, 0.5f);
				lotteryTable.Add(EInventoryUseItemType.Potion, 1f);
				break;
			case 7:
				lotteryTable.Add(EInventoryUseItemType.Drumstick, 0.1f);
				lotteryTable.Add(EInventoryUseItemType.Herb, 0.2f);
				lotteryTable.Add(EInventoryUseItemType.Antidote, 0.55f);
				lotteryTable.Add(EInventoryUseItemType.Potion, 1f);
				break;
			case 8:
				lotteryTable.Add(EInventoryUseItemType.Herb, 0.1f);
				lotteryTable.Add(EInventoryUseItemType.Potion, 0.3f);
				lotteryTable.Add(EInventoryUseItemType.Antidote, 1f);
				break;
			case 9:
				lotteryTable.Add(EInventoryUseItemType.Potion, 0.15f);
				lotteryTable.Add(EInventoryUseItemType.Ether, 0.3f);
				lotteryTable.Add(EInventoryUseItemType.Antidote, 1f);
				break;
			case 10:
				lotteryTable.Add(EInventoryUseItemType.FutureHiPotion, 0.1f);
				lotteryTable.Add(EInventoryUseItemType.FutureEther, 0.25f);
				lotteryTable.Add(EInventoryUseItemType.FuturePotion, 1f);
				break;
			case 11:
				lotteryTable.Add(EInventoryUseItemType.FutureHiEther, 0.1f);
				lotteryTable.Add(EInventoryUseItemType.FutureEther, 0.25f);
				lotteryTable.Add(EInventoryUseItemType.ChaosHeal, 0.5f);
				lotteryTable.Add(EInventoryUseItemType.FuturePotion, 1f);
				break;
			case 12:
				lotteryTable.Add(EInventoryUseItemType.FutureHiEther, 0.1f);
				lotteryTable.Add(EInventoryUseItemType.FutureHiPotion, 0.2f);
				lotteryTable.Add(EInventoryUseItemType.FutureEther, 0.45f);
				lotteryTable.Add(EInventoryUseItemType.ChaosHeal, 0.7f);
				lotteryTable.Add(EInventoryUseItemType.FuturePotion, 1f);
				break;
			case 14:
				lotteryTable.Add(EInventoryUseItemType.FamiliarTreat, 0.1f);
				lotteryTable.Add(EInventoryUseItemType.GoldRing, 0.2f);
				lotteryTable.Add(EInventoryUseItemType.GoldNecklace, 0.3f);
				lotteryTable.Add(EInventoryUseItemType.EssenceCrystal, 0.4f);
				lotteryTable.Add(EInventoryUseItemType.HiPotion, 0.7f);
				lotteryTable.Add(EInventoryUseItemType.Potion, 1f);
				break;
			default:
				lotteryTable.Add(EInventoryUseItemType.Potion, 1f);
				break;
			}
		}
		else
		{
			switch (_level.ID)
			{
			case 1:
				lotteryTable.Add(EInventoryUseItemType.FutureHiPotion, 1f);
				break;
			case 2:
				lotteryTable.Add(EInventoryUseItemType.FutureHiPotion, 0.25f);
				lotteryTable.Add(EInventoryEquipmentType.MetalWristband, 1f);
				break;
			case 3:
				lotteryTable.Add(EInventoryEquipmentType.PointyHat, 1f);
				break;
			case 4:
				lotteryTable.Add(EInventoryUseItemType.HiPotion, 1f);
				break;
			case 5:
				lotteryTable.Add(EInventoryEquipmentType.BuckleHat, 1f);
				break;
			case 6:
				lotteryTable.Add(EInventoryEquipmentType.MidnightCloak, 1f);
				break;
			case 7:
				lotteryTable.Add(EInventoryEquipmentType.TravelersCloak, 1f);
				break;
			case 8:
				lotteryTable.Add(EInventoryUseItemType.SilverOre, 1f);
				break;
			case 9:
				lotteryTable.Add(EInventoryEquipmentType.BirdStatue, 1f);
				break;
			case 10:
				lotteryTable.Add(EInventoryEquipmentType.LabGlasses, 1f);
				break;
			case 11:
				lotteryTable.Add(EInventoryEquipmentType.LabCoat, 1f);
				break;
			case 12:
				lotteryTable.Add(EInventoryEquipmentType.FiligreeClasp, 1f);
				break;
			case 14:
			{
				bool isSpeedrunAActive = _level.GameSave.IsSpeedrunAActive;
				if (!isSpeedrunAActive)
				{
					lotteryTable.Add(EInventoryEquipmentType.EternalTiara, 0.2f);
					lotteryTable.Add(EInventoryEquipmentType.EternalCoat, 0.4f);
				}
				PlayerInventory inventory = _level.GameSave.Inventory;
				float rate = (isSpeedrunAActive ? 1f : 0.7f);
				bool flag = !inventory.FamiliarInventory.Inventory.ContainsKey(4);
				if ((inventory.EquippedTrinketA == EInventoryEquipmentType.ShinyRock || inventory.EquippedTrinketB == EInventoryEquipmentType.ShinyRock) && !inventory.FamiliarInventory.Inventory.ContainsKey(3))
				{
					flag = false;
					lotteryTable.Add(EInventoryFamiliarType.MerchantCrow, rate);
				}
				if (flag)
				{
					lotteryTable.Add(EInventoryFamiliarType.Kobo, rate);
				}
				if (isSpeedrunAActive)
				{
					lotteryTable.Add(EInventoryEquipmentType.EternalTiara, 0.2f);
					lotteryTable.Add(EInventoryEquipmentType.EternalCoat, 0.4f);
				}
				lotteryTable.Add(EInventoryUseItemType.MagicMarbles, 1f);
				break;
			}
			default:
				lotteryTable.Add(EInventoryUseItemType.HiPotion, 1f);
				break;
			}
		}
		lotteryTable.GetItemByRoll(roll, out _treasureLootType, out _lootUseItemType, out _lootEquipmentType, out _lootFamiliarType);
	}
}
