using System;
using Microsoft.Xna.Framework;

namespace Timespinner.GameStateManagement;

internal class PlayerIndexEventArgs : EventArgs
{
	private readonly PlayerIndex _playerIndex;

	public PlayerIndex PlayerIndex => _playerIndex;

	public PlayerIndexEventArgs(PlayerIndex playerIndex)
	{
		_playerIndex = playerIndex;
	}
}
