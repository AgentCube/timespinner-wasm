using System;

namespace Timespinner.Core.Constants;

internal static class Constants
{
	internal const string GameVersion = "1.033";

	internal const string AelanaEmpathySaveKey = "AelEmpath";

	internal const string IsLabPowerOffSaveKey = "11_LabPower";

	internal const string IsExperiment13DeadKey = "11_Exp13";

	internal const string IsVileteSavedSaveKey = "IsVileteSaved";

	internal const string IsPastClearedSaveKey = "IsPastCleared";

	internal const string HasPlayerBeenToThePastKey = "HasUsedCityTS";

	internal const string HasBeenToPresentAfterDemons = "HasBeenToNewPresent";

	internal const string IsCantoranQuestActive = "IsCantoranActive";

	internal const string GyreDungeonSeedKey = "GyreDungeonSeed";

	internal const string GyreBossDeadKey = "IsGyreBossDead";

	internal const string IsEnding4Key = "IsEnding4";

	internal const string IsTerrilisDeadKey = "IsTerrilisDead";

	internal const string IsPrinceDeadKey = "IsPrinceDead";

	internal const string IsEmperorKilledAfterAlts = "IsEmperorKilledAfterAlts";

	internal const string IsNightmareKilledInNightmareMode = "IsNightmareNightmare";

	internal const string IsLabTimespinnerReadyKey = "IsLabTSReady";

	internal const string IsJianaShocked = "IsJianaShocked";

	internal const string HasShownPlantBatQuestFinish = "HasShownPlantBatQuestFinished";

	internal const string HasShownSirenQuestFinish = "HasShownSirenQuestFinished";

	internal const string HasShownSoldierQuestFinish = "HasShownSoldierQuestFinished";

	internal const string HasShownDemonQuestFinish = "HasShownDemonQuestFinished";

	internal const string IsDoingRamedaFoundCutsceneKey = "IsDoingRamedaFoundCutscene";

	internal const string IsFlaggedSpeedrunA = "IsFlaggedSpeedrunA";

	internal const string IsFlaggedSpeedrunB = "IsFlaggedSpeedrunB";

	internal const string IsActiveSpeedrunA = "IsActiveSpeedrunA";

	internal const string IsActiveSpeedrunB = "IsActiveSpeedrunB";

	internal const int LevelScreenSizeWidth = 400;

	internal const int LevelScreenSizeHeight = 240;

	internal const int HardMode_EnemyDamageFlatIncrease = 65;

	internal const float HardMode_PlayerDamageMultiplier = 0.85f;

	internal const float HardMode_EnemyDamageThresholdMultiplier = 1.5f;

	internal const float HardMode_SpikeDamagePercent = 0.2f;

	internal const float WeaknessDamageMultiplier = 1.5f;

	internal const float StrongAgainstDamageMultiplier = 0.5f;

	internal const int BossFeatMaskWin = 1;

	internal const int BossFeatMaskNoHit = 2;

	internal const int BossFeatMaskNoTime = 4;

	internal const int BossFeatMaskPerfect = 8;

	internal const int MaxSaveFiles = 8;

	internal const int HeroBaseHP_Lunais = 128;

	internal const int HeroBaseSand_Lunais = 50;

	internal const int HeroBaseAura_Lunais = 80;

	internal const int HeroBaseWill_Lunais = 5;

	internal const int HeroBaseFort_Lunais = 4;

	internal const int HeroBaseLuck_Lunais = 2;

	internal const int HeroHPMaxMultiplier_Lunais = 20;

	internal const int HeroSandMaxMultiplier_Lunais = 5;

	internal const int HeroAuraMaxMultiplier_Lunais = 5;

	internal const int HeroMaxStatFoundCount = 99;

	internal const int HeroMaxHPCap = 999;

	internal const int HeroMaxAuraCap = 999;

	internal const int HeroMaxSandCap = 200;

	internal const int HeroHPLevelMultiplier_Lunais = 8;

	internal const int HeroAuraLevelMultiplier_Lunais = 5;

	internal const float HeroWillLevelMultiplier_Lunais = 0.7f;

	internal const float HeroFortLevelMultiplier_Lunais = 0.5f;

	internal const float HeroLuckLevelMultiplier_Lunais = 0.3f;

	internal const float HeroLuckItemDropMultiplier = 0.05f;

	internal const float HeroBaseSandRegen_Lunais = 0f;

	internal const float HeroBaseAuraRegen_Lunais = 1.25f;

	internal const float VileteAuraRegenRate = 1.25f;

	internal const float ViletianCrownAuraRegenRate = 1f;

	internal const float EternalCrownSandRegenRate = 0.5f;

	internal const float BaseTimeStopDrainRate = 10f;

	internal const float GlassPumpkinTimeDrainMultiplier = 0.5f;

	internal const float SelenBangleExperienceMultiplier = 2f;

	internal const int FamiliarEggExperienceMultiplier = 3;

	internal const int HeroSandRestoreOnBossHit = 1;

	internal const int HeroSandRestoreOnKill = 5;

	internal const int FamiliarBaseMaxHealth_Crow = 40;

	internal const int FamiliarBaseMaxHealth_Griffin = 60;

	internal const int FamiliarBaseMaxHealth_Kobo = 60;

	internal const int FamiliarBaseMaxHealth_Meyef = 50;

	internal const int FamiliarBaseMaxHealth_Sprite = 30;

	internal const int FamiliarBaseMaxHealth_Demon = 66;

	internal const float FamiliarMaxHealthScaling_Crow = 10f;

	internal const float FamiliarMaxHealthScaling_Griffin = 10f;

	internal const float FamiliarMaxHealthScaling_Kobo = 10f;

	internal const float FamiliarMaxHealthScaling_Meyef = 10f;

	internal const float FamiliarMaxHealthScaling_Sprite = 10f;

	internal const float FamiliarMaxHealthScaling_Demon = 6f;

	internal const int FamiliarBaseDamage_Crow = 1;

	internal const int FamiliarBaseDamage_Griffin = 1;

	internal const int FamiliarBaseDamage_Kobo = 1;

	internal const int FamiliarBaseDamage_Meyef = 1;

	internal const int FamiliarBaseDamage_Sprite = 1;

	internal const int FamiliarBaseDamage_Demon = 1;

	internal const float FamiliarDamageScaling_Crow = 1f;

	internal const float FamiliarDamageScaling_Griffin = 1f;

	internal const float FamiliarDamageScaling_Kobo = 1f;

	internal const float FamiliarDamageScaling_Meyef = 1f;

	internal const float FamiliarDamageScaling_Sprite = 1f;

	internal const float FamiliarDamageScaling_Demon = 1f;

	internal const float FamiliarSpellMultiplier_Crow = 2f;

	internal const float FamiliarSpellMultiplier_Griffin = 1f;

	internal const float FamiliarSpellMultiplier_Kobo = 5f;

	internal const float FamiliarSpellMultiplier_Meyef = 1f;

	internal const float FamiliarSpellMultiplier_Sprite = 3f;

	internal const float FamiliarSpellMultiplier_Demon = 2f;

	private const float TotalFamiliarLevelDamageMultiplier = 1f / 6f;

	internal const int OrbBaseDamage_Blue = 4;

	internal const int OrbBaseDamage_Green = 7;

	internal const int OrbBaseDamage_Iron = 10;

	internal const int OrbBaseDamage_Red = 6;

	internal const int OrbBaseDamage_Pink = 6;

	internal const int OrbBaseDamage_Ice = 3;

	internal const int OrbBaseDamage_Wind = 3;

	internal const int OrbBaseDamage_Gun = 9;

	internal const int OrbBaseDamage_Umbra = 4;

	internal const int OrbBaseDamage_Empire = 10;

	internal const int OrbBaseDamage_Eye = 3;

	internal const int OrbBaseDamage_Blood = 3;

	internal const int OrbBaseDamage_Book = 6;

	internal const int OrbBaseDamage_Moon = 3;

	internal const int OrbBaseDamage_Nether = 6;

	internal const int OrbBaseDamage_Barrier = 8;

	internal const int OrbBaseDamage_Monske = 6;

	internal const int OrbPinkMeleeAuraCost = 3;

	internal const float OrbMeleeMultiplier_IceSnowflake = 0.5f;

	internal const float OrbMeleeMultiplier_NetherCounter = 2f;

	internal const float OrbSpellScaling_Blue = 3f;

	internal const float OrbSpellScaling_Blue_Small = 0.33f;

	internal const float OrbSpellScaling_Green = 6f;

	internal const float OrbSpellScaling_Iron = 6f;

	internal const float OrbSpellScaling_Red = 1f;

	internal const float OrbSpellScaling_Pink = 0.5f;

	internal const float OrbSpellScaling_Ice = 2.5f;

	internal const float OrbSpellScaling_Wind = 1f;

	internal const float OrbSpellScaling_Gun = 1f;

	internal const int OrbSpellScaling_GunFinalShot = 3;

	internal const float OrbSpellScaling_Umbra = 1.5f;

	internal const float OrbSpellScaling_Empire = 0.4f;

	internal const float OrbSpellScaling_Eye = 1.75f;

	internal const float OrbSpellScaling_Blood = 3f;

	internal const float OrbSpellScaling_Book = 9f;

	internal const float OrbSpellScaling_Moon = 3f;

	internal const float OrbSpellScaling_Nether = 3f;

	internal const float OrbSpellScaling_Barrier = 1f;

	internal const float OrbSpellScaling_Monske = 1f;

	internal const float OrbPassiveScaling_Blue = 1f;

	internal const float OrbPassiveScaling_Green = 0.5f;

	internal const float OrbPassiveScaling_Red = 0.25f;

	internal const float OrbPassiveScaling_PinkFlat = 1.2f;

	internal const float OrbPassiveScaling_PinkScaling = 0.025f;

	internal const float OrbPassiveScaling_Ice = 0.25f;

	internal const float OrbPassiveScaling_Wind = 1.5f;

	internal const float OrbPassiveScaling_Gun = 0.5f;

	internal const float OrbPassiveScaling_BloodFlat = 0.3f;

	internal const float OrbPassiveScaling_BloodScaling = 0.05f;

	internal const float OrbPassiveScaling_Umbra = 0.25f;

	internal const float OrbPassiveScaling_Empire = 1f;

	internal const float OrbPassiveScaling_Nether = 2f;

	internal const float OrbPassiveScaling_BookFlat = 0.25f;

	internal const float OrbPassiveScaling_BookScaling = 0.025f;

	internal const int OrbSpellCostBlueSmall = 10;

	internal const int OrbSpellCostBlueLarge = 25;

	internal const int OrbSpellCostBlade = 30;

	internal const int OrbSpellCostIron = 35;

	internal const int OrbSpellCostFire = 35;

	internal const int OrbSpellCostPink = 40;

	internal const int OrbSpellCostIce = 40;

	internal const int OrbSpellCostWind = 35;

	internal const int OrbSpellCostGun = 45;

	internal const int OrbSpellCostUmbra = 30;

	internal const int OrbSpellCostEmpire = 45;

	internal const int OrbSpellCostEye = 40;

	internal const int OrbSpellCostBlood = 35;

	internal const int OrbSpellCostBook = 45;

	internal const int OrbSpellCostMoon = 25;

	internal const int OrbSpellCostNether = 25;

	internal const int OrbSpellCostBarrier = 25;

	internal const int OrbSpellCostMonske = 25;

	internal const int OrbPassiveCost_Wind = 5;

	internal const int OrbValue_Green = 100;

	internal const int CharacterBaseExpThreshold = 48;

	internal const int CharacterExpScaling = 24;

	internal const int CharacterMaxLevelDefault = 99;

	internal const int CharacterMaxLevelCap1 = 0;

	internal const int CharacterMaxLevelCap255 = 254;

	internal const int FamiliarBaseExpThreshold = 10;

	internal const int FamiliarExpScaling = 3;

	internal const int FamiliarMaxLevel = 99;

	internal const int OrbExpScaling = 50;

	internal const int OrbReinforceExperienceAmount = 250;

	internal const int MaxOrbLevel = 999;

	internal const int LunaisLevel99Exp = 125000;

	internal const int FamiliarLevel99Exp = 16000;

	internal const int OrbLevel99Exp = 50000;

	internal const int ItemPower_Potion = 75;

	internal const int ItemPower_Ether = 75;

	internal const int ItemPower_SandBottle = 50;

	internal const int ItemPower_HiPotion = 250;

	internal const int ItemPower_HiEther = 150;

	internal const int ItemPower_HiSandBottle = 100;

	internal const int ItemPower_FuturePotion = 50;

	internal const int ItemPower_FutureHiPotion = 150;

	internal const int ItemPower_FutureEther = 30;

	internal const int ItemPower_FutureHiEther = 100;

	internal const int ItemPower_LachiemiSun = 200;

	internal const int ItemPower_Jerky = 39;

	internal const int ItemPower_Biscuit = 80;

	internal const int ItemPower_FriedCheveux = 100;

	internal const int ItemPower_SauteedTail = 150;

	internal const int ItemPower_UnagiRoll = 200;

	internal const int ItemPower_CheveuxAuVin = 250;

	internal const int ItemPower_Casserole = 300;

	internal const int ItemPower_Spaghetti = 300;

	internal const int ItemPower_PlumpMaggot = 25;

	internal const int ItemPower_OrangeJuice = 35;

	internal const int ItemPower_FiligreeTea = 100;

	internal const int ItemPower_EmpressCake = 400;

	internal const int ItemPower_FamiliarTreat = 100;

	internal const int ItemValue_Potion = 300;

	internal const int ItemValue_Ether = 400;

	internal const int ItemValue_SandBottle = 500;

	internal const int ItemValue_HiPotion = 1500;

	internal const int ItemValue_HiEther = 1350;

	internal const int ItemValue_HiSandBottle = 1500;

	internal const int ItemValue_FuturePotion = 150;

	internal const int ItemValue_FutureHiPotion = 750;

	internal const int ItemValue_FutureEther = 300;

	internal const int ItemValue_FutureHiEther = 1000;

	internal const int ItemValue_Antidote = 50;

	internal const int ItemValue_ChaosHeal = 100;

	internal const int ItemValue_WarpCard = 125;

	internal const int ItemValue_FamiliarTreat = 1;

	internal const int ItemValue_LachiemiSun = 100;

	internal const int ItemValue_Jerky = 120;

	internal const int ItemValue_Biscuit = 240;

	internal const int ItemValue_FriedCheveux = 250;

	internal const int ItemValue_SauteedTail = 350;

	internal const int ItemValue_UnagiRoll = 500;

	internal const int ItemValue_CheveuxAuVin = 650;

	internal const int ItemValue_Casserole = 800;

	internal const int ItemValue_Spaghetti = 600;

	internal const int ItemValue_PlumpMaggot = 50;

	internal const int ItemValue_OrangeJuice = 0;

	internal const int ItemValue_FiligreeTea = 0;

	internal const int ItemValue_EmpressCake = 0;

	internal const int ItemValue_RottenTail = 6;

	internal const int ItemValue_MagicMarbles = 1000;

	internal const int ItemValue_EssenceCrystal = 250;

	internal const int ItemValue_GoldRing = 500;

	internal const int ItemValue_GoldNecklace = 500;

	internal const int ItemValue_Herb = 30;

	internal const int ItemValue_Drumstick = 40;

	internal const int ItemValue_Mushroom = 60;

	internal const int ItemValue_WyvernTail = 50;

	internal const int ItemValue_EelMeat = 100;

	internal const int ItemValue_CheveuxBreast = 80;

	internal const int ItemValue_CheveuxFeather = 70;

	internal const int ItemValue_SirenInk = 66;

	internal const int ItemValue_PlasmaCore = 150;

	internal const int ItemValue_SilverOre = 125;

	internal const int Journal_Money_Memory = 250;

	internal const int Journal_Money_Letter = 250;

	internal const int Journal_Money_Download = 75;

	internal const int RelicValue_ScienceKeycardD = 25;

	internal const int EquipmentDefense_Sunglasses = 1;

	internal const int EquipmentDefense_SecurityVisor = 2;

	internal const int EquipmentDefense_EngineerGoggles = 2;

	internal const int EquipmentDefense_LeatherHelmet = 3;

	internal const int EquipmentDefense_PointyHat = 3;

	internal const int EquipmentDefense_CopperHelmet = 4;

	internal const int EquipmentDefense_CalvaryHelmet = 5;

	internal const int EquipmentDefense_BuckleHat = 5;

	internal const int EquipmentDefense_AdvisorHat = 3;

	internal const int EquipmentDefense_LibrarianHat = 4;

	internal const int EquipmentDefense_CombatHelmet = 6;

	internal const int EquipmentDefense_CaptainsCap = 6;

	internal const int EquipmentDefense_LabGlasses = 1;

	internal const int EquipmentDefense_LachiemCrown = 6;

	internal const int EquipmentDefense_VileteCrown = 6;

	internal const int EquipmentDefense_EternalTiara = 8;

	internal const int EquipmentDefense_OldCoat = 1;

	internal const int EquipmentDefense_TrendyJacket = 2;

	internal const int EquipmentDefense_SecurityVest = 3;

	internal const int EquipmentDefense_LeatherArmor = 4;

	internal const int EquipmentDefense_TravelersCloak = 5;

	internal const int EquipmentDefense_CopperArmor = 6;

	internal const int EquipmentDefense_CalvaryArmor = 8;

	internal const int EquipmentDefense_MidnightCloak = 7;

	internal const int EquipmentDefense_AdvisorRobe = 5;

	internal const int EquipmentDefense_LibrarianRobe = 6;

	internal const int EquipmentDefense_MilitaryArmor = 10;

	internal const int EquipmentDefense_CaptainsJacket = 10;

	internal const int EquipmentDefense_LabCoat = 7;

	internal const int EquipmentDefense_EmpressCoat = 10;

	internal const int EquipmentDefense_VileteDress = 8;

	internal const int EquipmentDefense_EternalCoat = 15;

	internal const int EquipmentDefense_SyntheticPlume = 0;

	internal const int EquipmentDefense_CheveuxPlume = 0;

	internal const int EquipmentDefense_MetalWristband = 1;

	internal const int EquipmentDefense_SirenHairband = 2;

	internal const int EquipmentDefense_MotherOfPearl = 1;

	internal const int EquipmentDefense_BirdStatue = 0;

	internal const int EquipmentDefense_DemonStole = 1;

	internal const int EquipmentDefense_Pendulum = 0;

	internal const int EquipmentDefense_DemonHorn = 2;

	internal const int EquipmentDefense_FiligreeClasp = 0;

	internal const int EquipmentDefense_AzureStole = 1;

	internal const int EquipmentDefense_LuckyCoin = 2;

	internal const int EquipmentDefense_ShinyRock = 0;

	internal const int EquipmentDefense_NelisteEarring = 0;

	internal const int EquipmentDefense_SelenBangle = 1;

	internal const int EquipmentDefense_Pumpkin = 0;

	internal const int EquipmentDefense_FamiliarEgg = 0;

	internal const int EquipmentStat_EngineerGoggles_Will = 1;

	internal const int EquipmentStat_PointyHat_Will = 2;

	internal const int EquipmentStat_BuckleHat_Will = 1;

	internal const int EquipmentStat_BuckleHat_Luck = 1;

	internal const int EquipmentStat_AdvisorHat_Will = 3;

	internal const int EquipmentStat_LibrarianHat_Will = 4;

	internal const int EquipmentStat_CombatHat_Fort = 2;

	internal const int EquipmentStat_CaptainsCap_Will = 8;

	internal const int EquipmentStat_LabGlasses_Will = 8;

	internal const int EquipmentStat_LachiemCrown_Will = 12;

	internal const int EquipmentStat_VileteCrown_Will = 6;

	internal const int EquipmentStat_EternalTiara_Will = 9;

	internal const int EquipmentStat_TravelersRobe_Will = 1;

	internal const int EquipmentStat_CalvaryArmor_Fort = 2;

	internal const int EquipmentStat_MidnightRobe_Will = 2;

	internal const int EquipmentStat_MidnightRobe_Luck = 2;

	internal const int EquipmentStat_AdvisorRobe_Will = 4;

	internal const int EquipmentStat_LibrarianRobe_Will = 5;

	internal const int EquipmentStat_MilitaryArmor_Fort = 4;

	internal const int EquipmentStat_CaptainsJacket_Luck = 7;

	internal const int EquipmentStat_LabCoat_Will = 8;

	internal const int EquipmentStat_EmpressCoat_Will = 6;

	internal const int EquipmentStat_EmpressCoat_Fort = 3;

	internal const int EquipmentStat_VileteDress_Will = 7;

	internal const int EquipmentStat_VileteDress_Fort = 3;

	internal const int EquipmentStat_EternalCoat_Will = 10;

	internal const int EquipmentStat_EternalCoat_Fort = 5;

	internal const int EquipmentStat_SyntheticPlume_Luck = 1;

	internal const int EquipmentStat_CheveuxPlume_Luck = 2;

	internal const int EquipmentStat_SirenHairband_Will = 1;

	internal const int EquipmentStat_MotherOfPearl_Will = 1;

	internal const int EquipmentStat_MotherOfPearl_Fort = 1;

	internal const int EquipmentStat_MotherOfPearl_Luck = 1;

	internal const int EquipmentStat_DemonStole_Will = 4;

	internal const int EquipmentStat_DemonHorn_Fort = 4;

	internal const int EquipmentStat_FiligreeClasp_Luck = 10;

	internal const int EquipmentStat_AzureStole_Will = 6;

	internal const int EquipmentStat_LuckyCoin_Luck = 3;

	internal const int EquipmentValue_Sunglasses = 50;

	internal const int EquipmentValue_SecurityVisor = 75;

	internal const int EquipmentValue_EngineerGoggles = 160;

	internal const int EquipmentValue_LeatherHelmet = 125;

	internal const int EquipmentValue_PointyHat = 150;

	internal const int EquipmentValue_CopperHelmet = 200;

	internal const int EquipmentValue_CalvaryHelmet = 360;

	internal const int EquipmentValue_BuckleHat = 300;

	internal const int EquipmentValue_AdvisorHat = 300;

	internal const int EquipmentValue_LibrarianHat = 400;

	internal const int EquipmentValue_CombatHelmet = 600;

	internal const int EquipmentValue_CaptainsCap = 900;

	internal const int EquipmentValue_LabGlasses = 200;

	internal const int EquipmentValue_LachiemCrown = 0;

	internal const int EquipmentValue_VileteCrown = 0;

	internal const int EquipmentValue_EternalTiara = 0;

	internal const int EquipmentValue_OldCoat = 30;

	internal const int EquipmentValue_TrendyJacket = 100;

	internal const int EquipmentValue_SecurityVest = 150;

	internal const int EquipmentValue_TravelersCloak = 200;

	internal const int EquipmentValue_LeatherArmor = 250;

	internal const int EquipmentValue_CopperArmor = 350;

	internal const int EquipmentValue_CalvaryArmor = 500;

	internal const int EquipmentValue_MidnightCloak = 666;

	internal const int EquipmentValue_AdvisorRobe = 650;

	internal const int EquipmentValue_LibrarianRobe = 750;

	internal const int EquipmentValue_MilitaryArmor = 800;

	internal const int EquipmentValue_CaptainsJacket = 1000;

	internal const int EquipmentValue_LabCoat = 500;

	internal const int EquipmentValue_EmpressCoat = 0;

	internal const int EquipmentValue_VileteDress = 0;

	internal const int EquipmentValue_EternalCoat = 0;

	internal const int EquipmentValue_SyntheticPlume = 15;

	internal const int EquipmentValue_CheveuxPlume = 30;

	internal const int EquipmentValue_MetalWristband = 40;

	internal const int EquipmentValue_SirenHairband = 60;

	internal const int EquipmentValue_MotherOfPearl = 100;

	internal const int EquipmentValue_BirdStatue = 200;

	internal const int EquipmentValue_DemonStole = 90;

	internal const int EquipmentValue_Pendulum = 500;

	internal const int EquipmentValue_DemonHorn = 100;

	internal const int EquipmentValue_FiligreeClasp = 400;

	internal const int EquipmentValue_AzureStole = 777;

	internal const int EquipmentValue_LuckyCoin = 3000;

	internal const int EquipmentValue_ShinyRock = 9999;

	internal const int EquipmentValue_NelisteEarring = 0;

	internal const int EquipmentValue_SelenBangle = 0;

	internal const int EquipmentValue_Pumpkin = 4;

	internal const int EquipmentValue_FamiliarEgg = 99999;

	internal const int SpikeDamage = 5;

	internal const float SpikeDamagePercent = 0.05f;

	internal const float EnemyDamage_CeilingStarZap = 1.25f;

	internal const float EnemyDamage_RedCheveuxDashing = 1.5f;

	internal const float BossDamage_RoboKittyLazer = 1.4f;

	internal const float BossDamage_RoboKittyRubble = 1.1f;

	internal const float EnemyDamage_CitySecurityGuardBaton = 1.25f;

	internal const float EnemyDamage_CitySecurityGuardGrenade = 0.5f;

	internal const float EnemyDamage_CitySecurityGuardGrenadeExplosion = 1.5f;

	internal const float BossDamage_VarndargrothFire = 1.2f;

	internal const float BossDamage_VarndargrothMissile = 1.2f;

	internal const float EnemyHP_ForestWormSapling = 0.33f;

	internal const float EnemyDamage_ForestWormFlowerSapling = 0.75f;

	internal const float EnemyDamage_CastleArcherArrow = 1.5f;

	internal const float EnemyDamage_CastleLargeSoldierHammer = 1.25f;

	internal const float EnemyDamage_CastleEngineerRock = 2f;

	internal const float EnemyDamage_CastleEngineerLog = 2.5f;

	internal const float EnemyDamage_KeepAristocratSpell = 1.5f;

	internal const float EnemyDamage_KeepDemonSpell = 1.35f;

	internal const float BossDamage_DemonSawblade = 1.15f;

	internal const float EnemyDamage_TowerPlasmaPodSpell = 1.25f;

	internal const float EnemyDamage_TowerRoyalGuardSpell = 1.25f;

	internal const float EnemyDamage_TowerIceMageSpell = 1.5f;

	internal const float BossDamage_AelanaPlasmaBeam = 1.25f;

	internal const float BossDamage_AelanaPlasmaBolt = 1.1f;

	internal const float BossDamage_AelanaOrb = 1.15f;

	internal const float BossDamage_GodBirdGoop = 0.75f;

	internal const float BossDamage_GodBirdSlam = 1.1f;

	internal const float BossDamage_CantoranBarrier = 1.25f;

	internal const float BossDamage_CantoranOrb = 1.15f;

	internal const float EnemyDamage_CavesSnailGoop = 0.9f;

	internal const float BossDamage_MawHorizontalLazer = 1.2f;

	internal const float BossDamage_MawVerticalLazer = 1.2f;

	internal const float BossDamage_MawSpike = 1.1f;

	internal const float BossHP_MawMinion = 0.05f;

	internal const float BossDamage_MawMinion = 0.5f;

	internal const float EnemyDamage_CursedSnailGoop = 0.75f;

	internal const float EnemyHP_CursedMushroomMinion = 0.1f;

	internal const float EnemyDamage_CursedMushroomMinion = 0.75f;

	internal const float EnemyDamage_CursedAnemoneSpine = 1.15f;

	internal const float BossDamage_XarionHorizontalLazer = 1.2f;

	internal const float BossDamage_XarionCaterpillar = 0.95f;

	internal const float EnemyDamage_FortressKnightShieldBeam = 1.2f;

	internal const float EnemyDamage_FortressGunnerBolt = 1.5f;

	internal const float EnemyDamage_FortressLargeSoldierHammer = 1.25f;

	internal const float EnemyDamage_FortressLargeSoldierSpike = 1.1f;

	internal const float EnemyDamage_FortressEngineerRock = 2f;

	internal const float EnemyDamage_LabTurretBolt = 1.5f;

	internal const float EnemyDamage_LabAdultGlass = 1.15f;

	internal const int EnemyDamage_LabRobotJunk = 48;

	internal const int EnemyHP_LabRobotJunk = 32;

	internal const float BossDamage_Shapeshifter_Crusher = 1.15f;

	internal const float BossDamage_Shapeshifter_Arrow = 1.1f;

	internal const float BossDamage_Shapeshifter_Spikes = 1.2f;

	internal const float EnemyDamage_EmpAristocratSpell = 1.5f;

	internal const float EnemyDamage_EmpDemonSpell = 1.35f;

	internal const float BossDamage_Emperor_Blue_Melee = 1f;

	internal const float BossDamage_Emperor_Blue_Spell = 1.5f;

	internal const float BossDamage_Emperor_Blade_Melee = 1.25f;

	internal const float BossDamage_Emperor_Blade_Spell = 1.5f;

	internal const float BossDamage_Emperor_Empire_Melee = 1.15f;

	internal const float BossDamage_Emperor_Empire_Spell = 1.5f;

	internal const float BossDamage_Emperor_Plasma_Melee = 1.1f;

	internal const float BossDamage_Emperor_Plasma_Spell = 1.25f;

	internal const float BossDamage_Emperor_Fire_Melee = 1f;

	internal const float BossDamage_Emperor_Fire_Spell = 1f;

	internal const float BossDamage_Emperor_Iron_Melee = 1.25f;

	internal const float BossDamage_Emperor_Iron_Spell = 1.5f;

	internal const float BossDamage_Emperor_Ice_Passive = 1f;

	internal const float BossDamage_Emperor_Fire_Passive = 1f;

	internal const float EnemyDamage_GyreZelSword = 1.15f;

	internal const float BossDamage_RavenlordSingle = 0.9f;

	internal const float BossDamage_RavenlordFlock = 1.1f;

	internal const float BossDamage_Zel_Hellfire = 1.15f;

	internal const float BossDamage_Zel_DarkInferno = 1.25f;

	internal const float BossDamage_SandmanRubble = 1.1f;

	internal const float BossDamage_SandmanStatue = 1.15f;

	internal const float BossDamage_SandmanScepter = 1.2f;

	internal const float BossDamage_SandmanSpike = 1.1f;

	internal const float BossDamage_SandmanCrusher = 1.15f;

	internal const float BossDamage_Nightmare_FireBreath = 1.15f;

	internal const float BossDamage_Nightmare_Firebomb = 1.1f;

	internal const float BossDamage_Nightmare_Hellfire = 1.25f;

	internal const float BossDamage_Nightmare_Squish = 1.35f;

	internal static bool IsDrawingBorderFrameByDefault;

	internal static bool IsAnySpeedrunActive;

	internal static int InGameZoom = 3;

	internal static int ButtonDisplayType;

	internal static bool IsKeyboardPreferred;

	internal static float GameLoadProgress;

	internal static int GetCharacterLevelFromExperience(int experience, int maxLevel)
	{
		int num = 0;
		int num2 = experience;
		while (num < maxLevel && num2 > 0)
		{
			num2 -= 48 + 24 * num;
			if (num2 >= 0)
			{
				num++;
			}
		}
		return num;
	}

	internal static int GetCharacterLevelUpThresholdFromLevel(int level, int maxLevel)
	{
		int num = 0;
		if (level < maxLevel)
		{
			for (int i = 0; i <= level; i++)
			{
				num += 48 + i * 24;
			}
		}
		else
		{
			num = 0;
		}
		return num;
	}

	internal static int[] GetCharacterStatsByLevel(int level)
	{
		float num = level;
		return new int[5]
		{
			128 + (int)Math.Round(num * 8f),
			80 + (int)Math.Round(num * 5f),
			5 + (int)Math.Round(num * 0.7f),
			4 + (int)Math.Round(num * 0.5f),
			2 + (int)Math.Round(num * 0.3f)
		};
	}

	internal static int OrbLevelFromExperience(int experience)
	{
		int num = experience / 50 + 1;
		if (num <= 999)
		{
			return num;
		}
		return 999;
	}

	internal static int FamiliarLevelFromExperience(int experience)
	{
		int num = 0;
		int num2 = experience;
		while (num < 99 && num2 > 0)
		{
			num2 -= 10 + 3 * num;
			if (num2 >= 0)
			{
				num++;
			}
		}
		return num;
	}

	internal static int FamiliarLevelUpThresholdFromLevel(int level)
	{
		int num = 0;
		if (level < 99)
		{
			for (int i = 0; i <= level; i++)
			{
				num += 10 + i * 3;
			}
		}
		else
		{
			num = 0;
		}
		return num;
	}

	internal static int FamiliarDamage(int baseDamage, int level, float scaling, int sumOfFamiliarLevels)
	{
		int num = (int)Math.Ceiling((float)(level - 1) * scaling);
		int num2 = (int)Math.Ceiling((float)sumOfFamiliarLevels * (1f / 6f));
		return baseDamage + (int)Math.Ceiling((float)(num + num2) * 0.5f);
	}

	internal static int FamiliarMaxHealth(int baseDamage, int level, float scaling)
	{
		return baseDamage + (int)Math.Ceiling((float)(level - 1) * scaling);
	}

	internal static void ToggleZoom()
	{
		InGameZoom++;
		if (InGameZoom > 3)
		{
			InGameZoom = 1;
		}
	}
}
