using Timespinner.Core.Localization;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameStateManagement.Screens.PauseMenu;

internal class ControlsMenuEntry : MenuEntry
{
	private readonly ButtonMapping _button;

	public bool IsAwaitingInput { get; set; }

	public ButtonMapping Button => _button;

	internal ControlsMenuEntry(ButtonMapping button, string text)
		: base(text)
	{
		_button = button;
		base.Description = string.Format(Loc.Get("ControlsButtonMappingDescription"), base.Text);
	}
}
