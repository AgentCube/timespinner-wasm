using Microsoft.Xna.Framework;
using Timespinner.Core.Localization;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Events.Cutscene;
using Timespinner.GameObjects.Events.Misc;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs;

internal sealed class EnvPrefabEmpTowerThrone : TextPromptPrefab
{
	private const string TextKey = "Tutorial_Rule";

	private static readonly Color TextDrawColor = new Color(180, 160, 240);

	private static readonly Vector4 SparkleParticlesColor = new Vector4(0.5f, 0.5f, 0.75f, 1f);

	private static readonly Color LightDrawColor = new Color(0.1f, 0.125f, 0.2f, 0.15f);

	private readonly GlowingFloorEvent _glowingFloorEvent;

	public EnvPrefabEmpTowerThrone(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType, Loc.Get("Tutorial_Rule"), TextDrawColor)
	{
		_glowingFloorEvent = new GlowingFloorEvent(_level, Position, LightDrawColor, SparkleParticlesColor);
		_level.RequestAddObject(_glowingFloorEvent);
	}

	public override void Update(float delta)
	{
		_level.PreventPlayerFromWarpingOut();
		base.Update(delta);
	}

	internal override void OnActivate()
	{
		_glowingFloorEvent.FadeOut();
		CutsceneBase.CreateAndCallCutscene(CutsceneBase.ECutsceneType.EmpTower2_Throne, _level, Position);
	}
}
