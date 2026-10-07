using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

internal class QuestMenuEntry : MenuEntry
{
	private readonly NPCBase.EQuestStateType _questState;

	private readonly string _questTitle;

	internal NPCBase.EQuestStateType QuestState => _questState;

	internal string QuestTitle => _questTitle;

	public QuestMenuEntry(string title, NPCBase.EQuestStateType state)
		: base("")
	{
		_questTitle = title;
		_questState = state;
	}
}
