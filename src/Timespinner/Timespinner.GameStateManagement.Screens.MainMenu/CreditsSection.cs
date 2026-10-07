using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;

namespace Timespinner.GameStateManagement.Screens.MainMenu;

internal class CreditsSection
{
	private const int GoldBarOffsetY = 12;

	private const int SubGoldBarOffsetY = 15;

	private const int SubtitleOffset = 4;

	private const int ContributorsOffsetY = 4;

	private const int LinesBetweenSections = 4;

	private const int LogoMarginY = 8;

	private static readonly Color HeaderColor = new Color(208, 200, 152);

	private static readonly Color SubheaderColor = new Color(128, 168, 248);

	private static readonly Color NonHeaderColor = new Color(192, 200, 216);

	private static readonly Color ShadowColor = new Color(48, 40, 32);

	private readonly bool _hasSubtitle;

	private readonly string _header;

	private readonly string _subtitle;

	private readonly List<string> _contributors = new List<string>();

	private readonly List<Rectangle> _logos = new List<Rectangle>();

	private int _logoHeight;

	private string[] _extendedContributors;

	internal bool HasSubtitle => _hasSubtitle;

	internal int ColumnCount { get; set; }

	internal int TopY { get; private set; }

	internal int BottomY { get; private set; }

	internal int Height { get; private set; }

	internal string Header => _header;

	internal string Subtitle => _subtitle;

	internal SpriteSheet LogoSpriteSheet { get; set; }

	internal IEnumerable<string> Contributors => _contributors;

	internal CreditsSection(string header, string subtitle)
	{
		_header = header;
		_subtitle = subtitle;
		_hasSubtitle = true;
		ColumnCount = 1;
	}

	internal CreditsSection(string header)
	{
		_header = header;
		_hasSubtitle = false;
		ColumnCount = 1;
	}

	internal void AddContributor(string name)
	{
		_contributors.Add(name);
	}

	internal void AddContributors(string[] names)
	{
		_extendedContributors = names;
	}

	internal void AddLogo(Rectangle rect)
	{
		_logoHeight += rect.Height;
		_logos.Add(rect);
	}

	internal void CalculateSize(int previousCreditBottom, int primaryFontHeight, int latinFontHeight, int zoom)
	{
		int num = 1 + (HasSubtitle ? 1 : 0) + _contributors.Count;
		int num2 = ((_extendedContributors != null) ? _extendedContributors.Length : 0);
		int num3 = (HasSubtitle ? 8 : 0) + 4;
		float num4 = 1f;
		if (_extendedContributors != null)
		{
			if (_extendedContributors.Length > 100)
			{
				if (zoom >= 4)
				{
					ColumnCount = 4;
				}
				else if (zoom >= 2)
				{
					ColumnCount = 3;
					num4 = 2f / 3f;
				}
				else
				{
					ColumnCount = 2;
				}
			}
			else if (_extendedContributors.Length > 50)
			{
				ColumnCount = 2;
			}
		}
		int num5 = _logoHeight;
		if (_logoHeight > 0)
		{
			int count = _logos.Count;
			num5 += count * 8;
		}
		int num6 = (int)Math.Ceiling((float)num2 / (float)ColumnCount);
		int num7 = (int)Math.Ceiling((float)(num * primaryFontHeight + num6 * latinFontHeight) * num4);
		Height = num7 + num3 + num5;
		TopY = previousCreditBottom + 4 * primaryFontHeight;
		BottomY = TopY + Height;
	}

	internal void Draw(SpriteBatch spriteBatch, SpriteFont primaryFont, SpriteFont latinFont, float cameraY, int centerX, int zoom, SpriteSheet sprite, Rectangle mainBarSource, Rectangle subBarSource, int screenHeight)
	{
		Vector2 drawPosition = new Vector2(centerX, ((float)TopY - cameraY) * (float)zoom);
		Vector2 position = new Vector2(drawPosition.X, drawPosition.Y + (float)(12 * zoom));
		spriteBatch.Draw(origin: new Vector2(mainBarSource.Width, 0f), texture: sprite.Texture, position: position, sourceRectangle: mainBarSource, color: Color.White, rotation: 0f, scale: zoom, effects: SpriteEffects.None, layerDepth: 0f);
		spriteBatch.Draw(sprite.Texture, position, mainBarSource, Color.White, 0f, Vector2.Zero, zoom, SpriteEffects.FlipHorizontally, 0f);
		DrawLine(spriteBatch, primaryFont, _header, drawPosition, zoom, zoom, isHeader: true, isSubheader: false, screenHeight, isPrimary: true);
		if (_hasSubtitle)
		{
			drawPosition.Y += (primaryFont.LineSpacing + 4) * zoom;
			bool isSubheader = false;
			if (_contributors.Count > 0)
			{
				isSubheader = true;
				position = new Vector2(drawPosition.X, drawPosition.Y + (float)(15 * zoom));
				spriteBatch.Draw(origin: new Vector2(subBarSource.Width, 0f), texture: sprite.Texture, position: position, sourceRectangle: subBarSource, color: Color.White, rotation: 0f, scale: zoom, effects: SpriteEffects.None, layerDepth: 0f);
				spriteBatch.Draw(sprite.Texture, position, subBarSource, Color.White, 0f, Vector2.Zero, zoom, SpriteEffects.FlipHorizontally, 0f);
			}
			DrawLine(spriteBatch, primaryFont, _subtitle, drawPosition, zoom, zoom, isHeader: false, isSubheader, screenHeight, isPrimary: true);
			drawPosition.Y += 4 * zoom;
		}
		drawPosition.Y += (latinFont.LineSpacing + 4) * zoom;
		if (_contributors.Count > 0)
		{
			foreach (string contributor in Contributors)
			{
				DrawLine(spriteBatch, latinFont, contributor, drawPosition, zoom, zoom, isHeader: false, isSubheader: false, screenHeight, isPrimary: true);
				drawPosition.Y += latinFont.LineSpacing * zoom;
			}
		}
		if (_logoHeight > 0)
		{
			foreach (Rectangle logo in _logos)
			{
				drawPosition.Y += 8 * zoom;
				DrawLogo(drawPosition: new Vector2(drawPosition.X - (float)(logo.Width * zoom) * 0.5f, drawPosition.Y), spriteBatch: spriteBatch, sprite: LogoSpriteSheet, frameSource: logo, zoom: zoom, originalZoom: zoom, screenHeight: screenHeight);
			}
		}
		if (_extendedContributors == null || _extendedContributors.Length <= 0)
		{
			return;
		}
		int num = 400 * zoom;
		int num2 = zoom;
		float[] array;
		switch (ColumnCount)
		{
		case 2:
		{
			num2 = zoom;
			float num5 = (float)num * 0.25f;
			array = new float[2]
			{
				drawPosition.X - num5,
				drawPosition.X + num5
			};
			break;
		}
		case 3:
		{
			num2 = Math.Max(zoom - 1, 1);
			float num4 = (float)num * 0.3333f;
			array = new float[3]
			{
				drawPosition.X - num4,
				drawPosition.X,
				drawPosition.X + num4
			};
			break;
		}
		case 4:
		{
			num2 = Math.Max(zoom - 2, 1);
			float num3 = (float)num * 0.125f;
			array = new float[4]
			{
				drawPosition.X - 3f * num3,
				drawPosition.X - num3,
				drawPosition.X + num3,
				drawPosition.X + 3f * num3
			};
			break;
		}
		default:
			array = new float[1] { drawPosition.X };
			break;
		}
		int num6 = 0;
		string[] extendedContributors = _extendedContributors;
		foreach (string line in extendedContributors)
		{
			Vector2 drawPosition3 = new Vector2(array[num6], drawPosition.Y);
			DrawLine(spriteBatch, latinFont, line, drawPosition3, num2, zoom, isHeader: false, isSubheader: false, screenHeight, isPrimary: false);
			num6 = (num6 + 1) % ColumnCount;
			if (num6 == 0)
			{
				drawPosition.Y += latinFont.LineSpacing * num2;
			}
		}
	}

	private static void DrawLine(SpriteBatch spriteBatch, SpriteFont font, string line, Vector2 drawPosition, int zoom, int originalZoom, bool isHeader, bool isSubheader, int screenHeight, bool isPrimary)
	{
		int num = -16 * originalZoom;
		int num2 = screenHeight * originalZoom;
		if (drawPosition.Y >= (float)num && drawPosition.Y < (float)num2)
		{
			Color color = (isHeader ? HeaderColor : (isSubheader ? SubheaderColor : NonHeaderColor));
			int num3 = (int)(font.MeasureString(line).X * (float)zoom * 0.5f);
			if (isPrimary)
			{
				DrawingEx.DrawString(spriteBatch, font, line, new Vector2(drawPosition.X - (float)num3, drawPosition.Y + (float)zoom), ShadowColor, Vector2.Zero, zoom);
				DrawingEx.DrawString(spriteBatch, font, line, new Vector2(drawPosition.X - (float)num3, drawPosition.Y), color, Vector2.Zero, zoom);
			}
			else
			{
				spriteBatch.DrawString(font, line, new Vector2(drawPosition.X - (float)num3, drawPosition.Y + (float)zoom), ShadowColor, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
				spriteBatch.DrawString(font, line, new Vector2(drawPosition.X - (float)num3, drawPosition.Y), color, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
			}
		}
	}

	private static void DrawLogo(SpriteBatch spriteBatch, SpriteSheet sprite, Rectangle frameSource, Vector2 drawPosition, int zoom, int originalZoom, int screenHeight)
	{
		int num = -frameSource.Height * originalZoom;
		int num2 = screenHeight * originalZoom;
		if (drawPosition.Y >= (float)num && drawPosition.Y < (float)num2)
		{
			spriteBatch.Draw(sprite.Texture, drawPosition, frameSource, Color.White, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
		}
	}
}
