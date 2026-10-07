using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;

namespace Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

internal class ScrollbarWidget
{
	private const int Anim_LineStart = 71;

	private const int Anim_CapStart = 70;

	private const int Anim_SliderStart = 72;

	private const int Anim_GlowArrowStart = 150;

	private const int GlowArrowOffsetX = 3;

	private const int GlowArrowTopOffsetY = 2;

	private const int GlowArrowBottomOffsetY = 7;

	private const int CapHeight = 14;

	private const int LineWidth = 7;

	private const int SliderTopOffset = 12;

	private const int SliderOffsetX = 2;

	private const float UpdateDelta = 1f / 60f;

	private const float ArrowGlowBase = 2f;

	private const float ArrowGlowFrequency = 10f;

	private float _glowTimer;

	internal float Percentage { get; set; }

	internal void Draw(SpriteBatch spriteBatch, Vector2 position, SpriteSheet sprite, Effect brightenEffect, Color drawColor, float zoom, int height)
	{
		float num = 14f * zoom;
		Rectangle frameSource = sprite.GetFrameSource(71);
		spriteBatch.Draw(sprite.Texture, new Rectangle((int)(position.X + zoom), (int)(position.Y + num), (int)(7f * zoom), (int)((float)height - num)), frameSource, drawColor, 0f, Vector2.Zero, SpriteEffects.None, 0f);
		frameSource = sprite.GetFrameSource(70);
		spriteBatch.Draw(sprite.Texture, position, frameSource, drawColor, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
		spriteBatch.Draw(sprite.Texture, new Vector2(position.X, position.Y + (float)height), frameSource, drawColor, 0f, Vector2.Zero, zoom, SpriteEffects.FlipVertically, 0f);
		if (Percentage >= 0f)
		{
			float num2 = 12f * zoom;
			float num3 = Percentage * ((float)height - num) + num2;
			frameSource = sprite.GetFrameSource(72);
			spriteBatch.Draw(sprite.Texture, new Vector2(position.X + 2f * zoom, position.Y + num3), frameSource, drawColor, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
			spriteBatch.End();
			brightenEffect.Parameters["shinyAmount"].SetValue(2f);
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, brightenEffect);
			_glowTimer += 1f / 6f;
			if (_glowTimer >= (float)Math.PI * 2f)
			{
				_glowTimer -= (float)Math.PI * 2f;
			}
			float num4 = (float)((Math.Sin(_glowTimer) + 1.0) * 0.25) + 0.5f;
			Color color = drawColor;
			if (drawColor.A >= byte.MaxValue)
			{
				color = new Color(1f, 1f, num4, num4);
			}
			frameSource = sprite.GetFrameSource(150);
			float num5 = 3f * zoom;
			spriteBatch.Draw(sprite.Texture, new Vector2(position.X + num5, position.Y + 2f * zoom), frameSource, color, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
			spriteBatch.Draw(sprite.Texture, new Vector2(position.X + num5, position.Y + (float)height + 7f * zoom), frameSource, color, 0f, Vector2.Zero, zoom, SpriteEffects.FlipVertically, 0f);
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
		}
	}
}
