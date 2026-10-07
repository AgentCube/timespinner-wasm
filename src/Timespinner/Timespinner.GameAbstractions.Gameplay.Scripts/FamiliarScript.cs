namespace Timespinner.GameAbstractions.Gameplay.Scripts;

internal class FamiliarScript : ScriptAction
{
	internal enum EFamiliarScriptType
	{
		Summon,
		Dismiss,
		SitStill,
		CutsceneFlyTo,
		CutsceneFlyAround,
		CutsceneFlyInFrontOfPlayer,
		ResumeAI,
		Land,
		MeyefMew
	}

	private readonly EFamiliarScriptType _familiarScriptType;

	internal EFamiliarScriptType FamiliarScriptType => _familiarScriptType;

	internal FamiliarScript(EFamiliarScriptType familiarScriptType)
	{
		_familiarScriptType = familiarScriptType;
		base.TargetType = EScriptTargetType.Familiar;
		base.DoesBlockQueue = false;
	}
}
