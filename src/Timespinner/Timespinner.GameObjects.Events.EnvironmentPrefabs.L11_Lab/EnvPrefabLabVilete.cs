using Microsoft.Xna.Framework;
using Timespinner.Core.Localization;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Events.Cutscene;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L11_Lab;

internal sealed class EnvPrefabLabVilete : EnvPrefabLabWarp
{
	private const string TextKey = "Tutorial_Vilete";

	private static readonly Color TextDrawColor = new Color(220, 160, 200);

	private static readonly Vector4 SparkleParticlesColor = new Vector4(1f, 0.5f, 0.75f, 1f);

	private static readonly Color LightDrawColor = new Color(0.175f, 0.1f, 0.15f, 0.15f);

	public EnvPrefabLabVilete(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType, Loc.Get("Tutorial_Vilete"), TextDrawColor, SparkleParticlesColor, LightDrawColor)
	{
	}

	internal override void OnActivate()
	{
		AddDialogue("cs_vol_lun_00");
		AddDialogue("cs_vol_lun_01");
		AddDialogue("cs_vol_lun_02");
		AddDelegateScript(StartVileteWarp);
	}

	private void StartVileteWarp()
	{
		_level.JukeBox.FadeOutSong(2f);
		CutsceneBase.StartAltEmperorCutscene(isVilete: true, _level);
		_level.GameSave.SetValue("IsLabTSReady", value: false);
	}
}
