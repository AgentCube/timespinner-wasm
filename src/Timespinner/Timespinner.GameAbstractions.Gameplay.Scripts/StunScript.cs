using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameAbstractions.Gameplay.Scripts;

internal class StunScript : ScriptAction
{
	internal enum EStunAnimationType
	{
		Held,
		Wind
	}

	internal EStunAnimationType StunAnimationType { get; set; }

	internal bool DoesAllowTimeStopEscape { get; set; }

	internal StunScript()
	{
		base.ActionType = EScriptActionType.Stun;
	}

	internal virtual void UpdateStun(float delta, Protagonist target, bool isTimeStopButtonDown)
	{
		base.Update(delta);
	}
}
