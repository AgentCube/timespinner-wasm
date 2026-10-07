using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.LevelEffects;

internal abstract class LevelEffect : GameEvent
{
	protected LevelEffect(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		base.CanBeTriggered = false;
		_isAffectedByTime = true;
		base.DoesDrawWhenOutsideOfObjectVisibleArea = true;
	}

	internal static GameEvent CreateFromLevel(Level level, Point position, int newObjectID, ObjectTileSpecification objectSpec)
	{
		GameEvent result = null;
		switch ((ELevelEffectType)objectSpec.Argument)
		{
		case ELevelEffectType.L1_LakeVacuum:
			result = new LakeVacuumLevelEffect(level, position, newObjectID, objectSpec);
			break;
		case ELevelEffectType.L1_LakeSnow:
			result = new LakeSnowLevelEffect(level, position, newObjectID, objectSpec);
			break;
		case ELevelEffectType.L3_ForestFireflies:
			result = new ForestFirefliesLevelEffect(level, position, newObjectID, objectSpec);
			break;
		case ELevelEffectType.L3_Campfire:
			result = new ForestCampfireLevelEffect(level, position, newObjectID, objectSpec);
			break;
		case ELevelEffectType.L3_CampDarkness:
			result = new ForestCampDarknessLevelEffect(level, position, newObjectID, objectSpec);
			break;
		case ELevelEffectType.L13_Break:
			result = new BreakLevelEffect(level, position, newObjectID, objectSpec);
			break;
		case ELevelEffectType.L15_Nightmare:
			result = new NightmareLevelEffect(level, position, newObjectID, objectSpec);
			break;
		}
		return result;
	}
}
