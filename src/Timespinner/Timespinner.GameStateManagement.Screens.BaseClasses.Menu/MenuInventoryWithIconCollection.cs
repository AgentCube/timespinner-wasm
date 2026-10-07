using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Inventory;

namespace Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

internal abstract class MenuInventoryWithIconCollection : MenuInventoryCollection
{
	private const int EquippedItemMarginX = 10;

	private const int EquippedItemMarginY = 5;

	internal static readonly Color RecentDrawColor = Color.SkyBlue;

	private readonly SpriteSheet _pauseSprite;

	internal int IconFrameIndex { get; set; }

	internal MenuInventoryWithIconCollection(IEnumerable<InventoryItem> items, bool doesAddUnequipEntry, SpriteSheet pauseSprite)
		: base(items, doesAddUnequipEntry)
	{
		_pauseSprite = pauseSprite;
	}

	internal virtual bool IsIconVisibleByIndex(int index)
	{
		return false;
	}

	public override void Draw(SpriteBatch spriteBatch, float zoom)
	{
		if (base.IsVisible)
		{
			int num = 0;
			foreach (MenuEntry entry in base.Entries)
			{
				bool flag = false;
				if (!entry.IsScrolledOff && IsIconVisibleByIndex(num))
				{
					flag = true;
					int y = (int)(-5f * zoom);
					Rectangle frameSource = _pauseSprite.GetFrameSource(IconFrameIndex);
					Vector2 position = entry.DrawPosition.Add(new Point(0, y));
					spriteBatch.Draw(_pauseSprite.Texture, position, frameSource, Color.White, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
				}
				if (flag)
				{
					int num2 = (int)(10f * zoom);
					entry.DrawOffset = new Vector2(num2, 0f);
				}
				else
				{
					entry.DrawOffset = Vector2.Zero;
				}
				num++;
			}
		}
		base.Draw(spriteBatch, zoom);
	}
}
