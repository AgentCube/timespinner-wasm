using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions.GameObjects;

public class TiledAppendage : Appendage
{
	public int TilesIndexStart { get; set; }

	public int TilesIndexLength { get; set; }

	public int TilesColumnWidth { get; set; }

	public int TilesColumnMirrorWidth { get; set; }

	public TiledAppendage(Animate parent, Rectangle inBbox, Point inBboxOffset, Level inLevel, SpriteSheet inSprite)
		: base(parent, inBbox, inBboxOffset, inLevel, inSprite)
	{
	}

	protected override void DrawBaseSprite(SpriteBatch spriteBatch, SpriteSheet sprite, Vector2 drawPos, Rectangle source, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float depth)
	{
		Vector2 zero = Vector2.Zero;
		bool flag = false;
		int num = 0;
		int num2 = 0;
		SpriteEffects spriteEffects = (effects.HasFlag(SpriteEffects.FlipHorizontally) ? (effects & ~SpriteEffects.FlipHorizontally) : (effects | SpriteEffects.FlipHorizontally));
		for (int i = 0; i < TilesIndexLength; i++)
		{
			int index = TilesIndexStart + i + num;
			Rectangle frameSource = sprite.GetFrameSource(index);
			SpriteEffects effects2 = (flag ? spriteEffects : effects);
			spriteBatch.Draw(sprite.Texture, Vector2.Add(drawPos, zero), frameSource, color, rotation, origin, scale, effects2, depth);
			zero.X += frameSource.Width;
			if (TilesColumnWidth != 0)
			{
				num2++;
				if (num2 >= TilesColumnWidth)
				{
					num2 = 0;
					zero.X = 0f;
					zero.Y += frameSource.Height;
					num = ((TilesColumnMirrorWidth > 0) ? (1 - TilesColumnMirrorWidth) : 0);
					flag = false;
				}
				if (TilesColumnMirrorWidth > 0 && num2 >= TilesColumnMirrorWidth)
				{
					flag = true;
					num -= 2;
				}
			}
		}
	}
}
