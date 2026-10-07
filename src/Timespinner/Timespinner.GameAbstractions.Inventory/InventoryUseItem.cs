using Timespinner.Core.Localization;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.Heroes;
using Timespinner.GameObjects.StatusEffects;

namespace Timespinner.GameAbstractions.Inventory;

public class InventoryUseItem : InventoryItem
{
	private enum EUseItemRestorationType
	{
		None,
		Health,
		Aura,
		Time,
		Status,
		NegativeStatus,
		WarpCard,
		FamiliarTreat
	}

	private readonly EInventoryUseItemType _useItemType;

	public EInventoryUseItemType UseItemType => _useItemType;

	public int Count { get; set; }

	public int StackCap { get; set; }

	public override EInventoryCategoryType Category => EInventoryCategoryType.UseItem;

	public override int Key => (int)UseItemType;

	internal bool IsItemUsableInMenu => IsUseItemUsableInMenu(_useItemType);

	public InventoryUseItem(EInventoryUseItemType useItemType)
		: base("use_" + useItemType)
	{
		_useItemType = useItemType;
		base.MonetaryValue = GetMonetaryValue(_useItemType);
		switch (useItemType)
		{
		case EInventoryUseItemType.MagicMarbles:
		case EInventoryUseItemType.EssenceCrystal:
		case EInventoryUseItemType.GoldRing:
		case EInventoryUseItemType.GoldNecklace:
			StackCap = 99;
			break;
		default:
			StackCap = 9;
			break;
		}
	}

	public virtual bool Use()
	{
		return true;
	}

	internal static string GetUseItemName(EInventoryUseItemType itemType)
	{
		return Loc.Get("inv_use_" + itemType);
	}

	private static int GetMonetaryValue(EInventoryUseItemType itemType)
	{
		int result = 0;
		switch (itemType)
		{
		case EInventoryUseItemType.Potion:
			result = 300;
			break;
		case EInventoryUseItemType.Ether:
			result = 400;
			break;
		case EInventoryUseItemType.SandBottle:
			result = 500;
			break;
		case EInventoryUseItemType.HiPotion:
			result = 1500;
			break;
		case EInventoryUseItemType.HiEther:
			result = 1350;
			break;
		case EInventoryUseItemType.HiSandBottle:
			result = 1500;
			break;
		case EInventoryUseItemType.FuturePotion:
			result = 150;
			break;
		case EInventoryUseItemType.FutureHiPotion:
			result = 750;
			break;
		case EInventoryUseItemType.FutureEther:
			result = 300;
			break;
		case EInventoryUseItemType.FutureHiEther:
			result = 1000;
			break;
		case EInventoryUseItemType.Antidote:
			result = 50;
			break;
		case EInventoryUseItemType.ChaosHeal:
			result = 100;
			break;
		case EInventoryUseItemType.WarpCard:
			result = 125;
			break;
		case EInventoryUseItemType.FamiliarTreat:
			result = 1;
			break;
		case EInventoryUseItemType.LachiemiSun:
			result = 100;
			break;
		case EInventoryUseItemType.Jerky:
			result = 120;
			break;
		case EInventoryUseItemType.Biscuit:
			result = 240;
			break;
		case EInventoryUseItemType.FriedCheveux:
			result = 250;
			break;
		case EInventoryUseItemType.SauteedTail:
			result = 350;
			break;
		case EInventoryUseItemType.UnagiRoll:
			result = 500;
			break;
		case EInventoryUseItemType.CheveuxAuVin:
			result = 650;
			break;
		case EInventoryUseItemType.Casserole:
			result = 800;
			break;
		case EInventoryUseItemType.Spaghetti:
			result = 600;
			break;
		case EInventoryUseItemType.PlumpMaggot:
			result = 50;
			break;
		case EInventoryUseItemType.OrangeJuice:
			result = 0;
			break;
		case EInventoryUseItemType.FiligreeTea:
			result = 0;
			break;
		case EInventoryUseItemType.EmpressCake:
			result = 0;
			break;
		case EInventoryUseItemType.RottenTail:
			result = 6;
			break;
		case EInventoryUseItemType.MagicMarbles:
			result = 1000;
			break;
		case EInventoryUseItemType.EssenceCrystal:
			result = 250;
			break;
		case EInventoryUseItemType.GoldRing:
			result = 500;
			break;
		case EInventoryUseItemType.GoldNecklace:
			result = 500;
			break;
		case EInventoryUseItemType.Herb:
			result = 30;
			break;
		case EInventoryUseItemType.Drumstick:
			result = 40;
			break;
		case EInventoryUseItemType.Mushroom:
			result = 60;
			break;
		case EInventoryUseItemType.WyvernTail:
			result = 50;
			break;
		case EInventoryUseItemType.EelMeat:
			result = 100;
			break;
		case EInventoryUseItemType.CheveuxBreast:
			result = 80;
			break;
		case EInventoryUseItemType.CheveuxFeather:
			result = 70;
			break;
		case EInventoryUseItemType.SirenInk:
			result = 66;
			break;
		case EInventoryUseItemType.PlasmaCore:
			result = 150;
			break;
		case EInventoryUseItemType.SilverOre:
			result = 125;
			break;
		}
		return result;
	}

	internal static float GetDropRate(EInventoryUseItemType itemType)
	{
		return 0.1f;
	}

	private static int GetItemPower(EInventoryUseItemType itemType)
	{
		int result = 0;
		switch (itemType)
		{
		case EInventoryUseItemType.Potion:
			result = 75;
			break;
		case EInventoryUseItemType.Ether:
			result = 75;
			break;
		case EInventoryUseItemType.SandBottle:
			result = 50;
			break;
		case EInventoryUseItemType.HiPotion:
			result = 250;
			break;
		case EInventoryUseItemType.HiEther:
			result = 150;
			break;
		case EInventoryUseItemType.HiSandBottle:
			result = 100;
			break;
		case EInventoryUseItemType.FuturePotion:
			result = 50;
			break;
		case EInventoryUseItemType.FutureHiPotion:
			result = 150;
			break;
		case EInventoryUseItemType.FutureEther:
			result = 30;
			break;
		case EInventoryUseItemType.FutureHiEther:
			result = 100;
			break;
		case EInventoryUseItemType.LachiemiSun:
			result = 200;
			break;
		case EInventoryUseItemType.Jerky:
			result = 39;
			break;
		case EInventoryUseItemType.Biscuit:
			result = 80;
			break;
		case EInventoryUseItemType.FriedCheveux:
			result = 100;
			break;
		case EInventoryUseItemType.SauteedTail:
			result = 150;
			break;
		case EInventoryUseItemType.UnagiRoll:
			result = 200;
			break;
		case EInventoryUseItemType.CheveuxAuVin:
			result = 250;
			break;
		case EInventoryUseItemType.Casserole:
			result = 300;
			break;
		case EInventoryUseItemType.Spaghetti:
			result = 300;
			break;
		case EInventoryUseItemType.PlumpMaggot:
			result = 25;
			break;
		case EInventoryUseItemType.OrangeJuice:
			result = 35;
			break;
		case EInventoryUseItemType.FiligreeTea:
			result = 100;
			break;
		case EInventoryUseItemType.EmpressCake:
			result = 400;
			break;
		}
		return result;
	}

	private static bool IsUseItemUsableInMenu(EInventoryUseItemType itemType)
	{
		return GetRestorationType(itemType) != EUseItemRestorationType.None;
	}

	private static EUseItemRestorationType GetRestorationType(EInventoryUseItemType itemType)
	{
		EUseItemRestorationType result = EUseItemRestorationType.None;
		switch (itemType)
		{
		case EInventoryUseItemType.Potion:
			result = EUseItemRestorationType.Health;
			break;
		case EInventoryUseItemType.Ether:
			result = EUseItemRestorationType.Aura;
			break;
		case EInventoryUseItemType.SandBottle:
			result = EUseItemRestorationType.Time;
			break;
		case EInventoryUseItemType.HiPotion:
			result = EUseItemRestorationType.Health;
			break;
		case EInventoryUseItemType.HiEther:
			result = EUseItemRestorationType.Aura;
			break;
		case EInventoryUseItemType.HiSandBottle:
			result = EUseItemRestorationType.Time;
			break;
		case EInventoryUseItemType.FuturePotion:
			result = EUseItemRestorationType.Health;
			break;
		case EInventoryUseItemType.FutureHiPotion:
			result = EUseItemRestorationType.Health;
			break;
		case EInventoryUseItemType.FutureEther:
			result = EUseItemRestorationType.Aura;
			break;
		case EInventoryUseItemType.FutureHiEther:
			result = EUseItemRestorationType.Aura;
			break;
		case EInventoryUseItemType.Antidote:
			result = EUseItemRestorationType.Status;
			break;
		case EInventoryUseItemType.ChaosHeal:
			result = EUseItemRestorationType.Status;
			break;
		case EInventoryUseItemType.WarpCard:
			result = EUseItemRestorationType.WarpCard;
			break;
		case EInventoryUseItemType.FamiliarTreat:
			result = EUseItemRestorationType.FamiliarTreat;
			break;
		case EInventoryUseItemType.LachiemiSun:
			result = EUseItemRestorationType.Health;
			break;
		case EInventoryUseItemType.Jerky:
			result = EUseItemRestorationType.Health;
			break;
		case EInventoryUseItemType.Spaghetti:
			result = EUseItemRestorationType.Health;
			break;
		case EInventoryUseItemType.PlumpMaggot:
			result = EUseItemRestorationType.Health;
			break;
		case EInventoryUseItemType.OrangeJuice:
			result = EUseItemRestorationType.Aura;
			break;
		case EInventoryUseItemType.FiligreeTea:
			result = EUseItemRestorationType.Aura;
			break;
		case EInventoryUseItemType.EmpressCake:
			result = EUseItemRestorationType.Health;
			break;
		case EInventoryUseItemType.RottenTail:
			result = EUseItemRestorationType.NegativeStatus;
			break;
		case EInventoryUseItemType.Biscuit:
			result = EUseItemRestorationType.Health;
			break;
		case EInventoryUseItemType.FriedCheveux:
			result = EUseItemRestorationType.Health;
			break;
		case EInventoryUseItemType.SauteedTail:
			result = EUseItemRestorationType.Health;
			break;
		case EInventoryUseItemType.UnagiRoll:
			result = EUseItemRestorationType.Health;
			break;
		case EInventoryUseItemType.CheveuxAuVin:
			result = EUseItemRestorationType.Health;
			break;
		case EInventoryUseItemType.Casserole:
			result = EUseItemRestorationType.Health;
			break;
		}
		return result;
	}

	public bool IsUsable(Protagonist protagonist, GameSave saveFile)
	{
		bool result = false;
		switch (GetRestorationType(UseItemType))
		{
		case EUseItemRestorationType.Health:
			if (protagonist.HP < protagonist.MaxHP)
			{
				result = true;
			}
			break;
		case EUseItemRestorationType.Aura:
			if (protagonist.Aura < protagonist.MaxAura)
			{
				result = true;
			}
			break;
		case EUseItemRestorationType.Time:
			if (protagonist.MP < protagonist.MaxMP)
			{
				result = true;
			}
			break;
		case EUseItemRestorationType.Status:
		{
			EStatusEffectType statusEffectFromItem = GetStatusEffectFromItem(UseItemType);
			if (statusEffectFromItem != 0 && protagonist.HasStatusEffect(statusEffectFromItem, checkForLife: true))
			{
				result = true;
			}
			break;
		}
		case EUseItemRestorationType.NegativeStatus:
		{
			EStatusEffectType statusEffectFromItem2 = GetStatusEffectFromItem(UseItemType);
			if (statusEffectFromItem2 != 0 && !protagonist.HasStatusEffect(statusEffectFromItem2, checkForLife: true))
			{
				result = true;
			}
			break;
		}
		case EUseItemRestorationType.WarpCard:
			if (saveFile.Inventory.RelicInventory.IsRelicActive(EInventoryRelicType.PyramidsKey) && saveFile.LastWarpLevel > 0 && !protagonist.Level.IsInBossRoom)
			{
				result = true;
			}
			break;
		case EUseItemRestorationType.FamiliarTreat:
			if (saveFile.Inventory.EquippedFamiliar != 0)
			{
				result = true;
			}
			break;
		}
		return result;
	}

	private static EStatusEffectType GetStatusEffectFromItem(EInventoryUseItemType useItemType)
	{
		EStatusEffectType result = EStatusEffectType.None;
		switch (useItemType)
		{
		case EInventoryUseItemType.Antidote:
			result = EStatusEffectType.Poison;
			break;
		case EInventoryUseItemType.ChaosHeal:
			result = EStatusEffectType.Chaos;
			break;
		case EInventoryUseItemType.RottenTail:
			result = EStatusEffectType.Poison;
			break;
		}
		return result;
	}

	public bool UseItem(Protagonist protagonist, GameSave saveFile)
	{
		bool result = false;
		EUseItemRestorationType restorationType = GetRestorationType(UseItemType);
		if (restorationType != 0)
		{
			int itemPower = GetItemPower(UseItemType);
			switch (restorationType)
			{
			case EUseItemRestorationType.Health:
				protagonist.HP += (short)itemPower;
				break;
			case EUseItemRestorationType.Aura:
				protagonist.Aura += itemPower;
				break;
			case EUseItemRestorationType.Time:
				protagonist.MP += itemPower;
				break;
			case EUseItemRestorationType.Status:
			{
				EStatusEffectType statusEffectFromItem2 = GetStatusEffectFromItem(UseItemType);
				protagonist.HealStatus(statusEffectFromItem2);
				break;
			}
			case EUseItemRestorationType.NegativeStatus:
			{
				EStatusEffectType statusEffectFromItem = GetStatusEffectFromItem(UseItemType);
				protagonist.GiveStatusEffect(statusEffectFromItem, 0);
				break;
			}
			case EUseItemRestorationType.WarpCard:
				DoWarpCardWarp(protagonist, saveFile);
				result = true;
				break;
			case EUseItemRestorationType.FamiliarTreat:
				LevelUpFamiliar(saveFile);
				break;
			}
		}
		return result;
	}

	private static void DoWarpCardWarp(Protagonist protagonist, GameSave saveFile)
	{
		protagonist.Level.JukeBox.StopSong();
		protagonist.Level.RequestChangeLevel(new LevelChangeRequest
		{
			LevelID = saveFile.LastWarpLevel,
			RoomID = saveFile.LastWarpRoom,
			IsUsingWarp = true,
			IsUsingWhiteFadeOut = true,
			PreviousLevelID = protagonist.Level.ID,
			FadeInTime = 0.5f,
			FadeOutTime = 0.25f
		});
	}

	private static void LevelUpFamiliar(GameSave saveFile)
	{
		EInventoryFamiliarType equippedFamiliar = saveFile.Inventory.EquippedFamiliar;
		if (equippedFamiliar != 0)
		{
			saveFile.Inventory.FamiliarInventory.GetFamiliarItem(equippedFamiliar)?.GiveExperience(100);
		}
	}
}
