using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;

namespace Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

internal class StatEntry
{
	public enum EStatDisplayType
	{
		None,
		Number,
		NumberWithIcon,
		NumberComparison,
		Time,
		Percentage,
		Ratio,
		ColoredText,
		IconOnly
	}

	private const int IconSize = 8;

	private const int IconsStartIndex = 8;

	private const int DefaultCharacterWidth = 6;

	private const int IconMarginX = 10;

	private const int IconDrawOffsetY = 3;

	private const int NumberWithIconMarginX = 12;

	private const int TextOnlyMarginX = 2;

	private const int RatioMarginX = -3;

	private const int NumberComparisonMarginX = 24;

	private const int NumberComparisonArrowMarginX = -2;

	private bool _isInitialized;

	private bool _areNumbersDifferent;

	private int _iconDrawMargin;

	private int _drawStringWidth;

	private int _titleTextWidthReduction;

	private string _drawString;

	private string _drawString2;

	private ScrollableTextBlock _titleTextBlock;

	public bool IsNumberComparisonAlwaysPositive { get; set; }

	public EStatDisplayType Type { get; set; }

	public int IconIndex { get; set; }

	public string Title { get; set; }

	public int Value { get; set; }

	public int Value2 { get; set; }

	public string Text { get; set; }

	public Color TextColor { get; set; }

	public StatEntry()
	{
		Title = string.Empty;
	}

	public void Initialize(SpriteFont font)
	{
		switch (Type)
		{
		case EStatDisplayType.Number:
			_drawString = Value.ToString(CultureInfo.InvariantCulture);
			break;
		case EStatDisplayType.NumberWithIcon:
			_drawString = Value.ToString(CultureInfo.InvariantCulture);
			_iconDrawMargin = _drawString.Length * 6 + 12;
			break;
		case EStatDisplayType.NumberComparison:
			_drawString = Value.ToString(CultureInfo.InvariantCulture);
			_drawString2 = Value2.ToString(CultureInfo.InvariantCulture);
			_areNumbersDifferent = IsNumberComparisonAlwaysPositive || Value != Value2;
			break;
		case EStatDisplayType.Percentage:
			_drawString = Value + "%";
			break;
		case EStatDisplayType.Ratio:
		{
			string text = Value.ToString(CultureInfo.InvariantCulture).PadLeft(4);
			string text2 = Value2.ToString(CultureInfo.InvariantCulture).PadLeft(4);
			_drawString = text + "/" + text2;
			break;
		}
		case EStatDisplayType.Time:
		{
			TimeSpan timeSpan = TimeSpan.FromSeconds(Value);
			_drawString = $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
			break;
		}
		case EStatDisplayType.ColoredText:
			_drawString = Text;
			break;
		case EStatDisplayType.IconOnly:
			_iconDrawMargin = 10;
			break;
		}
		_drawStringWidth = 0;
		if (_drawString != null && Type != 0)
		{
			_drawStringWidth = (int)(font.MeasureString(_drawString).X + 10f);
		}
		_titleTextWidthReduction = 0;
		switch (Type)
		{
		case EStatDisplayType.Number:
		case EStatDisplayType.NumberComparison:
		case EStatDisplayType.Time:
		case EStatDisplayType.Percentage:
		case EStatDisplayType.ColoredText:
			_titleTextWidthReduction = _drawStringWidth + 2;
			break;
		case EStatDisplayType.NumberWithIcon:
			_titleTextWidthReduction = _iconDrawMargin + 10;
			break;
		case EStatDisplayType.Ratio:
			_titleTextWidthReduction = _drawStringWidth + -3;
			break;
		case EStatDisplayType.IconOnly:
			_titleTextWidthReduction = _drawStringWidth + 20;
			break;
		}
		_isInitialized = true;
	}

	public void Draw(SpriteBatch spriteBatch, GCM gcm, Point location, float width, float alpha, float scale)
	{
		SpriteFont activeFont = gcm.ActiveFont;
		Color color = new Color(240, 240, 208) * alpha;
		Color color2 = new Color(60, 60, 24) * alpha;
		Vector2 vector = new Vector2(location.X, (float)location.Y - scale * 2f);
		Vector2 vector2 = vector;
		if (!_isInitialized)
		{
			Initialize(activeFont);
		}
		if (_titleTextBlock == null)
		{
			_titleTextBlock = new ScrollableTextBlock(activeFont, (int)(width / scale) - _titleTextWidthReduction, vector2, isTextCentered: false);
			_titleTextBlock.SetText(Title);
		}
		_titleTextBlock.Draw(spriteBatch, color, color2, gcm.TxBlankSquare);
		if (_drawString != null && Type != 0)
		{
			Color color3 = ((Type == EStatDisplayType.ColoredText) ? (TextColor * alpha) : color);
			vector2.X += width - (float)_drawStringWidth * scale;
			DrawingEx.DrawString(spriteBatch, activeFont, _drawString, new Vector2(vector2.X, vector2.Y + scale), color2, Vector2.Zero, scale);
			DrawingEx.DrawString(spriteBatch, activeFont, _drawString, vector2, color3, Vector2.Zero, scale);
		}
		if ((Type == EStatDisplayType.NumberWithIcon || Type == EStatDisplayType.IconOnly) && IconIndex != -1)
		{
			float num = (float)_iconDrawMargin * scale;
			Vector2 position = new Vector2(vector.X + width - num - 8f * scale, vector2.Y + 3f * scale);
			int num2 = IconIndex;
			if (IconIndex > 10 && IconIndex < 20)
			{
				num2 = IconIndex + 77;
			}
			else if (IconIndex >= 20)
			{
				num2 = 120 + IconIndex - 8;
			}
			Rectangle frameSource = gcm.SpPauseMenu.GetFrameSource(8 + num2);
			Color color4 = Color.White * alpha;
			spriteBatch.Draw(gcm.SpPauseMenu.Texture, position, frameSource, color4, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
		}
		if (Type == EStatDisplayType.NumberComparison)
		{
			Color color5 = color;
			if (_areNumbersDifferent)
			{
				bool flag = IsNumberComparisonAlwaysPositive || Value2 > Value;
				Rectangle frameSource2 = gcm.SpPauseMenu.GetFrameSource(132 + ((!flag) ? 1 : 0));
				Color color6 = Color.White * alpha;
				Vector2 position2 = new Vector2(vector.X + width + -2f * scale, vector.Y + 4f * scale);
				color5 = (flag ? new Color(100, 200, 129) : new Color(228, 108, 72)) * alpha;
				spriteBatch.Draw(gcm.SpPauseMenu.Texture, position2, frameSource2, color6, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			}
			float num3 = activeFont.MeasureString(_drawString2).X * scale;
			Vector2 drawPos = new Vector2(vector.X + width + 24f * scale - num3, vector.Y);
			DrawingEx.DrawString(spriteBatch, activeFont, _drawString2, new Vector2(drawPos.X, drawPos.Y + scale), color2, Vector2.Zero, scale);
			DrawingEx.DrawString(spriteBatch, activeFont, _drawString2, drawPos, color5, Vector2.Zero, scale);
		}
	}
}
