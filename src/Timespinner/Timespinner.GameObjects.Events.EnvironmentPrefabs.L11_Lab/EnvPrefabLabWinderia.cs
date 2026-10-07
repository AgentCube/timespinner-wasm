using Microsoft.Xna.Framework;
using Timespinner.Core.Localization;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Events.Cutscene;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L11_Lab;

internal sealed class EnvPrefabLabWinderia : EnvPrefabLabWarp
{
	private const string TextKey = "Tutorial_Winderia";

	private static readonly Color TextDrawColor = new Color(128, 160, 240);

	private static readonly Vector4 SparkleParticlesColor = new Vector4(0.5f, 0.5f, 0.75f, 1f);

	private static readonly Color LightDrawColor = new Color(0.1f, 0.125f, 0.2f, 0.15f);

	public EnvPrefabLabWinderia(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType, Loc.Get("Tutorial_Winderia"), TextDrawColor, SparkleParticlesColor, LightDrawColor)
	{
	}

	internal override void OnActivate()
	{
		AddDialogue("cs_nuv_lun_00");
		AddDialogue("cs_nuv_lun_01");
		AddDelegateScript(StartWinderiaWarp);
	}

	private void StartWinderiaWarp()
	{
		_level.JukeBox.FadeOutSong(2f);
		CutsceneBase.StartAltEmperorCutscene(isVilete: false, _level);
		_level.GameSave.SetValue("IsLabTSReady", value: false);
	}
}
