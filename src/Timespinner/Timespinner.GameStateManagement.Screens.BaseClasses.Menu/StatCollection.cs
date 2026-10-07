using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions;

namespace Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

internal class StatCollection
{
	public enum EBracketType
	{
		None,
		Left,
		Right
	}

	private const int BracketWidth = 24;

	private const int LatinBracketHeightOffset = 6;

	private const int AsianBracketHeightOffset = 2;

	private const int LatinRowHeight = 11;

	private const int AsianRowHeight = 12;

	private readonly bool _isAsianLoc;

	private readonly List<StatEntry> _entries = new List<StatEntry>();

	public bool DoesStackHorizontally { get; set; }

	public EBracketType BracketType { get; set; }

	public int Height { get; set; }

	public int Width { get; set; }

	public Vector2 Location { get; set; }

	public List<StatEntry> Entries => _entries;

	public StatCollection()
	{
		_isAsianLoc = Loc.IsAsianLocale;
	}

	public void Draw(SpriteBatch spriteBatch, GCM gcm, float alpha, float scale)
	{
		if (BracketType != 0)
		{
			Color color = Color.White * alpha;
			SpriteEffects spriteEffects = ((BracketType != EBracketType.Left) ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
			Vector2 vector = ((BracketType == EBracketType.Left) ? Location : Vector2.Add(Location, new Vector2((float)Width - scale * 24f, 0f)));
			SpriteSheet spPauseMenu = gcm.SpPauseMenu;
			spriteBatch.Draw(spPauseMenu.Texture, vector, spPauseMenu.GetFrameSource(29), color, 0f, Vector2.Zero, scale, spriteEffects, 0f);
			int num = (_isAsianLoc ? 2 : 6);
			Vector2 position = Vector2.Add(vector, new Vector2(0f, (float)Height - (float)num * scale));
			spriteBatch.Draw(spPauseMenu.Texture, position, spPauseMenu.GetFrameSource(29), color, 0f, Vector2.Zero, scale, spriteEffects | SpriteEffects.FlipVertically, 0f);
		}
		int num2 = (_isAsianLoc ? 12 : 11);
		int num3 = (int)((float)num2 * scale);
		Point location = new Point((int)(Location.X + (float)num3 / 2f), (int)(Location.Y + (float)num3 / 2f));
		foreach (StatEntry entry in Entries)
		{
			entry.Draw(spriteBatch, gcm, location, Width, alpha, scale);
			if (!DoesStackHorizontally)
			{
				location.Y += num3;
			}
			else
			{
				location.X += Width;
			}
		}
	}
}
