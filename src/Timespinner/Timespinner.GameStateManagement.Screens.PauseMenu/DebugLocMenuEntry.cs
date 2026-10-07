using System;
using Microsoft.Xna.Framework;

namespace Timespinner.GameStateManagement.Screens.PauseMenu;

internal class DebugLocMenuEntry : MenuEntry
{
	private readonly string _key;

	private readonly Action<string> _onSelect;

	public DebugLocMenuEntry(string text, Action<string> onSelect)
		: base(text)
	{
		_key = text;
		_onSelect = onSelect;
	}

	internal override void OnSelectEntry(PlayerIndex playerIndex)
	{
		base.OnSelectEntry(playerIndex);
		_onSelect(_key);
	}
}
