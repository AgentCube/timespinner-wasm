using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Timespinner.Core;

internal static class SmoothCircle
{
	private const int MaxSize = 64;

	private const int SizeCount = 20;

	private const int OriginalBrushSize = 256;

	private static readonly int[] Sizes = new int[20]
	{
		64, 48, 40, 36, 32, 26, 24, 20, 18, 16,
		14, 12, 10, 8, 7, 6, 5, 4, 3, 1
	};

	private static readonly Rectangle[] FrameSources = new Rectangle[20];

	internal static void Draw(SpriteBatch spriteBatch, SpriteSheet sprite, Vector2 position, Color color, float rotation, float scale)
	{
		int num = (int)Math.Round(256f * scale);
		int num2 = 0;
		int num3 = 64;
		for (int i = 0; i < 20; i++)
		{
			int num4 = Sizes[i];
			if (num4 < num)
			{
				break;
			}
			num2 = i;
			num3 = num4;
			if (num4 == num)
			{
				break;
			}
		}
		if (FrameSources[num2] == Rectangle.Empty)
		{
			ref Rectangle reference = ref FrameSources[num2];
			reference = sprite.GetFrameSource(num2);
		}
		Rectangle value = FrameSources[num2];
		Vector2 origin = new Vector2((float)num3 / 2f, (float)num3 / 2f);
		float scale2 = 1f;
		if (num3 != num && num3 != 0)
		{
			scale2 = (float)num / (float)num3;
		}
		spriteBatch.Draw(sprite.Texture, position, value, color, rotation, origin, scale2, SpriteEffects.None, 0f);
	}

	public static void Draw(SpriteBatch spriteBatch, SpriteSheet sprite, Rectangle destination, Color color)
	{
		int width = destination.Width;
		int num = 0;
		for (int i = 0; i < 20; i++)
		{
			int num2 = Sizes[i];
			if (num2 < width)
			{
				break;
			}
			num = i;
			if (num2 == width)
			{
				break;
			}
		}
		if (FrameSources[num] == Rectangle.Empty)
		{
			ref Rectangle reference = ref FrameSources[num];
			reference = sprite.GetFrameSource(num);
		}
		Rectangle value = FrameSources[num];
		spriteBatch.Draw(sprite.Texture, destination, value, color);
	}
}
