using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core.Constants;
using Timespinner.Core.Localization;

namespace Timespinner.Core;

public static class DrawingEx
{
	private const int UnitSize = 16;

	private const int AsianFontOffsetY = 2;

	private static readonly Color LargeShadowColor = new Color(16, 16, 16);

	public static void DrawRectangleBorder(this SpriteBatch spriteBatch, Texture2D texture, Rectangle dest, int thickness, Color color)
	{
		spriteBatch.Draw(texture, new Rectangle(dest.X, dest.Y, dest.Width, thickness), color);
		spriteBatch.Draw(texture, new Rectangle(dest.X, dest.Y + dest.Height, dest.Width, thickness), color);
		spriteBatch.Draw(texture, new Rectangle(dest.X, dest.Y, thickness, dest.Height), color);
		spriteBatch.Draw(texture, new Rectangle(dest.X + dest.Width, dest.Y, thickness, dest.Height + thickness), color);
	}

	public static void DrawTextBox(SpriteBatch spriteBatch, Rectangle backgroundRectangle, Color color, SpriteSheet sprite)
	{
		DrawTextBox(spriteBatch, backgroundRectangle, color, sprite, Timespinner.Core.Constants.Constants.InGameZoom);
	}

	public static void DrawTextBox(SpriteBatch spriteBatch, Rectangle backgroundRectangle, Color color, SpriteSheet sprite, float zoom)
	{
		Rectangle destinationRectangle = backgroundRectangle;
		int num = (int)(16f * zoom);
		Rectangle frameSource = sprite.GetFrameSource(4);
		spriteBatch.Draw(sprite.Texture, destinationRectangle, frameSource, color);
		frameSource = sprite.GetFrameSource(1);
		destinationRectangle.Height = num;
		destinationRectangle.Location = new Point(destinationRectangle.Location.X, destinationRectangle.Location.Y - num);
		spriteBatch.Draw(sprite.Texture, destinationRectangle, frameSource, color);
		frameSource = sprite.GetFrameSource(7);
		destinationRectangle.Location = new Point(destinationRectangle.Location.X, destinationRectangle.Top + backgroundRectangle.Height + num);
		spriteBatch.Draw(sprite.Texture, destinationRectangle, frameSource, color);
		frameSource = sprite.GetFrameSource(3);
		destinationRectangle.Height = backgroundRectangle.Height;
		destinationRectangle.Width = num;
		destinationRectangle.Location = new Point(backgroundRectangle.Location.X - num, backgroundRectangle.Location.Y);
		spriteBatch.Draw(sprite.Texture, destinationRectangle, frameSource, color);
		frameSource = sprite.GetFrameSource(5);
		destinationRectangle.Location = new Point(backgroundRectangle.Location.X + backgroundRectangle.Width, backgroundRectangle.Location.Y);
		spriteBatch.Draw(sprite.Texture, destinationRectangle, frameSource, color);
		frameSource = sprite.GetFrameSource(0);
		destinationRectangle.Height = num;
		destinationRectangle.Location = new Point(backgroundRectangle.Location.X - num, backgroundRectangle.Location.Y - num);
		spriteBatch.Draw(sprite.Texture, destinationRectangle, frameSource, color);
		frameSource = sprite.GetFrameSource(2);
		destinationRectangle.Location = new Point(backgroundRectangle.Location.X + backgroundRectangle.Width, backgroundRectangle.Location.Y - num);
		spriteBatch.Draw(sprite.Texture, destinationRectangle, frameSource, color);
		frameSource = sprite.GetFrameSource(8);
		destinationRectangle.Location = new Point(backgroundRectangle.Location.X + backgroundRectangle.Width, backgroundRectangle.Location.Y + backgroundRectangle.Height);
		spriteBatch.Draw(sprite.Texture, destinationRectangle, frameSource, color);
		frameSource = sprite.GetFrameSource(6);
		destinationRectangle.Location = new Point(backgroundRectangle.Location.X - num, backgroundRectangle.Location.Y + backgroundRectangle.Height);
		spriteBatch.Draw(sprite.Texture, destinationRectangle, frameSource, color);
	}

	public static void DrawTextBoxHeader(SpriteBatch spriteBatch, Rectangle backgroundRectangle, int headerWidth, Color color, SpriteSheet sprite, bool hasPortrait)
	{
		DrawTextBoxHeader(spriteBatch, backgroundRectangle, headerWidth, color, sprite, Timespinner.Core.Constants.Constants.InGameZoom, hasPortrait);
	}

	public static void DrawTextBoxHeader(SpriteBatch spriteBatch, Rectangle backgroundRectangle, int headerWidth, Color color, SpriteSheet sprite, float zoom, bool hasPortrait)
	{
		Rectangle destinationRectangle = backgroundRectangle;
		int num = (int)(16f * zoom);
		int num2 = (int)(4f * zoom);
		int num3 = (int)(hasPortrait ? ((float)(num * 3)) : (8f * zoom));
		int num4 = (int)Math.Max(4.0, Math.Ceiling((float)headerWidth / (float)num));
		int num5 = num * num4 - num3 / 2;
		Rectangle frameSource = sprite.GetFrameSource(12);
		destinationRectangle.Height = num;
		destinationRectangle.Width = num5;
		destinationRectangle.Location = new Point(destinationRectangle.Location.X + num + num3, destinationRectangle.Location.Y - num + num2);
		spriteBatch.Draw(sprite.Texture, destinationRectangle, frameSource, color);
		frameSource = sprite.GetFrameSource(9);
		destinationRectangle.Width = num;
		destinationRectangle.Location = new Point(backgroundRectangle.Location.X - num, destinationRectangle.Location.Y);
		spriteBatch.Draw(sprite.Texture, destinationRectangle, frameSource, color);
		frameSource = sprite.GetFrameSource(10);
		destinationRectangle.Width = num3;
		destinationRectangle.Location = new Point(backgroundRectangle.Location.X, destinationRectangle.Location.Y);
		spriteBatch.Draw(sprite.Texture, destinationRectangle, frameSource, color);
		frameSource = sprite.GetFrameSource(11);
		destinationRectangle.Width = num;
		destinationRectangle.Location = new Point(backgroundRectangle.Location.X + num3, destinationRectangle.Location.Y);
		spriteBatch.Draw(sprite.Texture, destinationRectangle, frameSource, color);
		frameSource = sprite.GetFrameSource(13);
		destinationRectangle.Location = new Point(backgroundRectangle.Location.X + num5 + num + num3, destinationRectangle.Location.Y);
		spriteBatch.Draw(sprite.Texture, destinationRectangle, frameSource, color);
	}

	public static void DrawIrregularBox(SpriteBatch spriteBatch, Rectangle backgroundRectangle, Color color, SpriteSheet sprite, float zoom, int[] frames, SpriteEffects[] flipped)
	{
		DrawIrregularBox(spriteBatch, backgroundRectangle, color, sprite, zoom, frames, flipped, shouldTile: false);
	}

	public static void DrawIrregularBox(SpriteBatch spriteBatch, Rectangle backgroundRectangle, Color color, SpriteSheet sprite, float zoom, int[] frames, SpriteEffects[] flipped, bool shouldTile)
	{
		if (frames.Length >= 9 && flipped.Length >= 9)
		{
			Rectangle rectangle = backgroundRectangle;
			SpriteEffects effects = SpriteEffects.None;
			int num = frames[4];
			if (num != -1)
			{
				Rectangle frameSource = sprite.GetFrameSource(num);
				DrawFrame(spriteBatch, sprite, rectangle, frameSource, color, zoom, effects, shouldTile);
			}
			num = frames[1];
			if (num != -1)
			{
				effects = flipped[1];
				Rectangle frameSource = sprite.GetFrameSource(num);
				rectangle.Height = (int)((float)frameSource.Height * zoom);
				DrawFrame(spriteBatch, sprite, rectangle, frameSource, color, zoom, effects, shouldTile);
			}
			num = frames[7];
			if (num != -1)
			{
				effects = flipped[7];
				Rectangle frameSource = sprite.GetFrameSource(num);
				rectangle.Height = (int)((float)frameSource.Height * zoom);
				rectangle.Location = new Point(rectangle.Left, rectangle.Top + backgroundRectangle.Height - rectangle.Height);
				DrawFrame(spriteBatch, sprite, rectangle, frameSource, color, zoom, effects, shouldTile);
			}
			num = frames[3];
			if (num != -1)
			{
				effects = flipped[3];
				Rectangle frameSource = sprite.GetFrameSource(num);
				rectangle.Height = backgroundRectangle.Height;
				rectangle.Width = (int)((float)frameSource.Width * zoom);
				rectangle.Location = new Point(backgroundRectangle.Left, backgroundRectangle.Top);
				DrawFrame(spriteBatch, sprite, rectangle, frameSource, color, zoom, effects, shouldTile);
			}
			num = frames[5];
			if (num != -1)
			{
				effects = flipped[5];
				Rectangle frameSource = sprite.GetFrameSource(num);
				rectangle.Height = backgroundRectangle.Height;
				rectangle.Width = (int)((float)frameSource.Width * zoom);
				rectangle.Location = new Point(backgroundRectangle.Left + backgroundRectangle.Width - rectangle.Width, backgroundRectangle.Top);
				DrawFrame(spriteBatch, sprite, rectangle, frameSource, color, zoom, effects, shouldTile);
			}
			num = frames[0];
			if (num != -1)
			{
				effects = flipped[0];
				Rectangle frameSource = sprite.GetFrameSource(num);
				rectangle.Height = (int)((float)frameSource.Height * zoom);
				rectangle.Width = (int)((float)frameSource.Width * zoom);
				rectangle.Location = new Point(backgroundRectangle.Left, backgroundRectangle.Top);
				spriteBatch.Draw(sprite.Texture, rectangle, frameSource, color, 0f, Vector2.Zero, effects, 0f);
			}
			num = frames[2];
			if (num != -1)
			{
				effects = flipped[2];
				Rectangle frameSource = sprite.GetFrameSource(num);
				rectangle.Height = (int)((float)frameSource.Height * zoom);
				rectangle.Width = (int)((float)frameSource.Width * zoom);
				rectangle.Location = new Point(backgroundRectangle.Left + backgroundRectangle.Width - rectangle.Width, backgroundRectangle.Top);
				spriteBatch.Draw(sprite.Texture, rectangle, frameSource, color, 0f, Vector2.Zero, effects, 0f);
			}
			num = frames[8];
			if (num != -1)
			{
				effects = flipped[8];
				Rectangle frameSource = sprite.GetFrameSource(num);
				rectangle.Height = (int)((float)frameSource.Height * zoom);
				rectangle.Width = (int)((float)frameSource.Width * zoom);
				rectangle.Location = new Point(backgroundRectangle.Left + backgroundRectangle.Width - rectangle.Width, backgroundRectangle.Top + backgroundRectangle.Height - rectangle.Height);
				spriteBatch.Draw(sprite.Texture, rectangle, frameSource, color, 0f, Vector2.Zero, effects, 0f);
			}
			num = frames[6];
			if (num != -1)
			{
				effects = flipped[6];
				Rectangle frameSource = sprite.GetFrameSource(num);
				rectangle.Height = (int)((float)frameSource.Height * zoom);
				rectangle.Width = (int)((float)frameSource.Width * zoom);
				rectangle.Location = new Point(backgroundRectangle.Left, backgroundRectangle.Top + backgroundRectangle.Height - rectangle.Height);
				spriteBatch.Draw(sprite.Texture, rectangle, frameSource, color, 0f, Vector2.Zero, effects, 0f);
			}
		}
	}

	public static void DrawShortBox(SpriteBatch spriteBatch, Rectangle backgroundRectangle, Color color, SpriteSheet sprite, float zoom, int[] frames, SpriteEffects[] flipped, bool shouldTile)
	{
		if (frames.Length >= 3 && flipped.Length >= 3)
		{
			Rectangle destinationRectangle = backgroundRectangle;
			int num = 0;
			int num2 = 0;
			int num3 = frames[0];
			int num4 = frames[2];
			if (num3 != -1)
			{
				num = (int)((float)sprite.GetFrameSource(num3).Width * zoom);
			}
			if (num4 != -1)
			{
				num2 = (int)((float)sprite.GetFrameSource(num4).Width * zoom);
			}
			int num5 = frames[1];
			if (num5 != -1)
			{
				SpriteEffects effects = flipped[1];
				Rectangle frameSource = sprite.GetFrameSource(num5);
				destinationRectangle.Height = (int)((float)frameSource.Height * zoom);
				Rectangle drawRectangle = new Rectangle(destinationRectangle.X + num, destinationRectangle.Y, destinationRectangle.Width - (num + num2), destinationRectangle.Height);
				DrawFrame(spriteBatch, sprite, drawRectangle, frameSource, color, zoom, effects, shouldTile);
			}
			num5 = frames[0];
			if (num5 != -1)
			{
				SpriteEffects effects = flipped[0];
				Rectangle frameSource = sprite.GetFrameSource(num5);
				destinationRectangle.Height = (int)((float)frameSource.Height * zoom);
				destinationRectangle.Width = (int)((float)frameSource.Width * zoom);
				destinationRectangle.Location = new Point(backgroundRectangle.Left, backgroundRectangle.Top);
				spriteBatch.Draw(sprite.Texture, destinationRectangle, frameSource, color, 0f, Vector2.Zero, effects, 0f);
			}
			num5 = frames[2];
			if (num5 != -1)
			{
				SpriteEffects effects = flipped[2];
				Rectangle frameSource = sprite.GetFrameSource(num5);
				destinationRectangle.Height = (int)((float)frameSource.Height * zoom);
				destinationRectangle.Width = (int)((float)frameSource.Width * zoom);
				destinationRectangle.Location = new Point(backgroundRectangle.Left + backgroundRectangle.Width - destinationRectangle.Width, backgroundRectangle.Top);
				spriteBatch.Draw(sprite.Texture, destinationRectangle, frameSource, color, 0f, Vector2.Zero, effects, 0f);
			}
		}
	}

	private static void DrawFrame(SpriteBatch spriteBatch, SpriteSheet sprite, Rectangle drawRectangle, Rectangle frameSource, Color color, float zoom, SpriteEffects effects, bool shouldTile)
	{
		if (shouldTile)
		{
			DrawTiled(spriteBatch, sprite, drawRectangle, frameSource, color, zoom, effects);
		}
		else
		{
			spriteBatch.Draw(sprite.Texture, drawRectangle, frameSource, color, 0f, Vector2.Zero, effects, 0f);
		}
	}

	private static void DrawTiled(SpriteBatch spriteBatch, SpriteSheet sprite, Rectangle drawRectangle, Rectangle frameSource, Color color, float zoom, SpriteEffects effects)
	{
		int num = (int)((float)frameSource.Width * zoom);
		int num2 = (int)((float)frameSource.Height * zoom);
		for (int i = drawRectangle.Top; i < drawRectangle.Bottom; i += num2)
		{
			int height = num2;
			int num3 = i + num2;
			if (num3 > drawRectangle.Bottom)
			{
				height = num2 - (num3 - drawRectangle.Bottom);
			}
			for (int j = drawRectangle.Left; j < drawRectangle.Right; j += num)
			{
				int width = num;
				int num4 = j + num;
				if (num4 > drawRectangle.Right)
				{
					width = num - (num4 - drawRectangle.Right);
				}
				spriteBatch.Draw(destinationRectangle: new Rectangle(j, i, width, height), texture: sprite.Texture, sourceRectangle: frameSource, color: color, rotation: 0f, origin: Vector2.Zero, effects: effects, layerDepth: 0f);
			}
		}
	}

	public static void DrawLine(SpriteBatch spriteBatch, Texture2D texture, Rectangle frameSource, Vector2 start, int length, int thickness, float angle, Color color, SpriteEffects effects)
	{
		spriteBatch.Draw(texture, new Rectangle((int)start.X, (int)start.Y, length, thickness), frameSource, color, angle, new Vector2(0f, (float)frameSource.Height / 2f), effects, 0f);
	}

	public static void DrawLargeTextShadow(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 drawPosition, float zoom, Vector2 origin, float alpha)
	{
		Color color = LargeShadowColor * 0.25f * alpha;
		for (int i = -2; i <= 2; i++)
		{
			for (int j = -2; j <= 3; j++)
			{
				Vector2 vector = new Vector2(i, j) * zoom;
				DrawString(spriteBatch, font, text, drawPosition + vector, color, origin, zoom);
			}
		}
		color = LargeShadowColor * 0.5f * alpha;
		for (int k = -1; k <= 1; k++)
		{
			for (int l = -1; l <= 2; l++)
			{
				Vector2 vector2 = new Vector2(k, l) * zoom;
				DrawString(spriteBatch, font, text, drawPosition + vector2, color, origin, zoom);
			}
		}
	}

	public static void DrawString(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 drawPos, Color color)
	{
		spriteBatch.DrawString(font, text, drawPos, color);
	}

	public static void DrawString(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 drawPos, Color color, Vector2 origin, float zoom)
	{
		if (Loc.IsAsianLocale)
		{
			drawPos = new Vector2(drawPos.X, drawPos.Y + 2f * zoom);
		}
		spriteBatch.DrawString(font, text, drawPos, color, 0f, origin, zoom, SpriteEffects.None, 0f);
	}
}
