using Microsoft.Xna.Framework;

namespace Timespinner.GameStateManagement;

internal interface IMessageDisplay : IDrawable, IUpdateable
{
	void ShowMessage(string message, params object[] parameters);
}
