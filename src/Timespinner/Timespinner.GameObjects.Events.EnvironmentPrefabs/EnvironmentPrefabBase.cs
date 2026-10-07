using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.EnvironmentPrefabs.L00_Prologue;
using Timespinner.GameObjects.Events.EnvironmentPrefabs.L01_LakeDesolation;
using Timespinner.GameObjects.Events.EnvironmentPrefabs.L02_City;
using Timespinner.GameObjects.Events.EnvironmentPrefabs.L03_Forest;
using Timespinner.GameObjects.Events.EnvironmentPrefabs.L09_CursedCaves;
using Timespinner.GameObjects.Events.EnvironmentPrefabs.L10_Hangar;
using Timespinner.GameObjects.Events.EnvironmentPrefabs.L11_Lab;
using Timespinner.GameObjects.Events.EnvironmentPrefabs.L15_DarkForest;
using Timespinner.GameObjects.Events.EnvironmentPrefabs.L16_Temple;
using Timespinner.GameObjects.Events.EnvironmentPrefabs.L17_End;
using Timespinner.GameObjects.Events.EnvironmentPrefabs.L8_Caves;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs;

internal class EnvironmentPrefabBase : GameEvent
{
	private readonly EEnvironmentPrefabType _prefabType;

	internal EEnvironmentPrefabType PrefabType => _prefabType;

	public EnvironmentPrefabBase(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.EventType = EEventTileType.EnvironmentPrefab;
		_prefabType = prefabType;
		base.CanBeTriggered = false;
		_isAffectedByTime = true;
	}

	public static EnvironmentPrefabBase Create(Level level, Point position, int objectID, ObjectTileSpecification objectSpec)
	{
		EEnvironmentPrefabType eEnvironmentPrefabType = (EEnvironmentPrefabType)objectSpec.Argument;
		switch (eEnvironmentPrefabType)
		{
		case EEnvironmentPrefabType.L0_Table:
			return new EnvPrefabProTable(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L0_Rock:
			return new EnvPrefabProRock(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L0_Dummy:
			return new EnvPrefabProDummy(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L0_TableCake:
			return new EnvPrefabProTableCake(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L0_CricketsAmbient:
			return new EnvPrefabProCricketsAmbient(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L1_Ship:
			return new EnvPrefabLakeDesolationShip(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L2_ConveyorFloor:
			return new EnvPrefabCityConveyorFloor(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L2_ConveyorSwitch:
			return new EnvPrefabCityConveyorSwitch(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L2_FountainSign:
			return new EnvPrefabCityFountainSign(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L2_WaterBubbles:
			return new EnvPrefabCityWaterBubbles(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L3_Signpost:
			return new EnvPrefabForestSignpost(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L3_Barricade:
			return new EnvPrefabForestBarricade(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L3_NightAmbienceA:
			return new EnvPrefabForestNightAmbiA(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L3_EscortBlocker:
			return new EnvPrefabForestPlayerBlocker(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L4_DrawbridgeWinch:
			return new EnvPrefabCurtainWinch(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L4_DungeonDoor:
			return new EnvPrefabCurtainOneWayDoor(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L6_RailingCollider:
			return new EnvPrefabTowerRailingCollider(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L8_WaterfallAmbient:
		case EEnvironmentPrefabType.L8_WaterfallAmbientOffscreen:
			return new EnvPrefabCavesWaterfallAmbient(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L8_MawDoor:
			return new EnvPrefabCavesMawDoor(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L8_ElevatorBlockade:
			return new EnvPrefabCavesElevatorBlockade(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L8_RadiationCrystals:
			return new EnvPrefabCavesRadiationCrystal(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L8_PortalRubble:
			return new EnvPrefabCavesPortalRubble(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L9_ScientistCorpse:
			return new EnvPrefabCursedCavesCorpse(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L9_MothWorms:
			return new EnvPrefabCursedCavesWorm(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L9_MothNest:
			return new EnvPrefabCursedCavesNest(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L10_CrashedShip:
			return new EnvPrefabHangarCrashedShip(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L10_ForceFieldA:
		case EEnvironmentPrefabType.L10_ForceFieldB:
		case EEnvironmentPrefabType.L10_ForceFieldC:
			return new EnvPrefabHangarForceField(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L10_Spike:
			return new EnvPrefabHangarSpike(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L10_Bulwark:
			return new EnvPrefabHangarBulwark(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L10_BulwarkForceFieldP:
		case EEnvironmentPrefabType.L10_BulwarkForceFieldC:
		case EEnvironmentPrefabType.L10_BulwarkForceFieldB:
			return new EnvPrefabHangarBulwarkForceField(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L11_ConveyorFloor:
		{
			EAnimationType animationType = ((!level.IsPowerOff) ? EAnimationType.Cycle : EAnimationType.None);
			return new AnimatedTilePrefab(level, level.GCM.SpMiscLab, position, objectID, objectSpec, eEnvironmentPrefabType, 0, 3, 0.1f, animationType, EDrawPlane.Front);
		}
		case EEnvironmentPrefabType.L11_ConveyorFloorRampA:
			return new AnimatedTilePrefab(level, level.GCM.SpMiscLab, position.Add(0, 4), objectID, objectSpec, eEnvironmentPrefabType, 3, 3, 0.1f, EAnimationType.Cycle, EDrawPlane.Front);
		case EEnvironmentPrefabType.L11_ConveyorFloorRampB:
			return new AnimatedTilePrefab(level, level.GCM.SpMiscLab, position.Add(0, 12), objectID, objectSpec, eEnvironmentPrefabType, 3, 3, 0.1f, EAnimationType.Cycle, EDrawPlane.Front);
		case EEnvironmentPrefabType.L11_ComputerMonitor:
			return new EnvPrefabLabComputer(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L11_TubeLargeEmpty:
		case EEnvironmentPrefabType.L11_TubeLargeEmptyDark:
		case EEnvironmentPrefabType.L11_TubeLargeChild:
		case EEnvironmentPrefabType.L11_TubeLargeAdolescent:
		case EEnvironmentPrefabType.L11_TubeLargeAdult:
		case EEnvironmentPrefabType.L11_TubeLargeBroken:
		case EEnvironmentPrefabType.L11_TubeLargeAdolescentDark:
			if (level.IsPowerOff)
			{
				EEnvironmentPrefabType eEnvironmentPrefabType2 = eEnvironmentPrefabType;
				switch (eEnvironmentPrefabType)
				{
				case EEnvironmentPrefabType.L11_TubeLargeEmpty:
					eEnvironmentPrefabType2 = EEnvironmentPrefabType.L11_TubeLargeEmptyDark;
					break;
				case EEnvironmentPrefabType.L11_TubeLargeChild:
					eEnvironmentPrefabType2 = EEnvironmentPrefabType.L11_TubeLargeBroken;
					break;
				case EEnvironmentPrefabType.L11_TubeLargeAdolescent:
					eEnvironmentPrefabType2 = EEnvironmentPrefabType.L11_TubeLargeAdolescentDark;
					break;
				}
				if (eEnvironmentPrefabType2 != eEnvironmentPrefabType)
				{
					eEnvironmentPrefabType = (EEnvironmentPrefabType)(objectSpec.Argument = (int)eEnvironmentPrefabType2);
				}
			}
			return new EnvPrefabLabTube(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L11_ConveyorAmbient:
		case EEnvironmentPrefabType.L11_ConveyorAmbientOffscreen:
			return new EnvPrefabLabConveyorAmbient(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L11_PlayerBlocker:
			return new EnvPrefabLabPlayerBlocker(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L11_ForceField:
			return new EnvPrefabLabForceField(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L11_PowerCore:
			return new EnvPrefabLabPowerCore(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L11_Pedestal:
			return new EnvPrefabLabPedestal(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L11_HistoricalDocuments:
			return new EnvPrefabLabHistoricalDocuments(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L11_SwitchWinderia:
			return new EnvPrefabLabWinderia(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L11_SwitchVilete:
			return new EnvPrefabLabVilete(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L12_ThroneTrigger:
			return new EnvPrefabEmpTowerThrone(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L12_ExitTrigger:
			return new EnvPrefabEmpTowerExit(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L15_DoorSmoke:
			return new EnvPrefabDarkForestSmoke(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L15_SandShade:
			return new EnvPrefabDarkForestShade(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L16_Looper:
			return new EnvPrefabTempleLooper(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L16_Wall:
			return new EnvPrefabTempleWall(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L16_Glass:
			return new EnvPrefabTempleGlass(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L16_Hellfire:
			return new EnvPrefabTempleHellfire(level, position, objectID, objectSpec, eEnvironmentPrefabType, isOpening: true);
		case EEnvironmentPrefabType.L16_Platform:
			return new EnvPrefabTemplePlatform(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L17_SwitchA:
		case EEnvironmentPrefabType.L17_SwitchB:
		case EEnvironmentPrefabType.L17_SwitchC:
		case EEnvironmentPrefabType.L17_SwitchD:
			return new EnvPrefabEndSwitch(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L17_MorningAmbience:
			return new EnvPrefabEndMorningAmbi(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L17_TubeLargeChild:
			return new EnvPrefabLabTube(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		case EEnvironmentPrefabType.L17_Ascend:
			return new EnvPrefabEndAscend(level, position, objectID, objectSpec, eEnvironmentPrefabType);
		default:
			return null;
		}
	}
}
