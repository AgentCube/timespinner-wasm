using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.Core;

internal class DialogueLine
{
	private const char ButtonEscapeCharacter = '$';

	private const string ButtonReplacementCharacter = " ";

	private const string ButtonReplacementCharacterWide = "\t";

	private const int TextPaddingLeft = 0;

	private const int TextPaddingRight = 30;

	private static readonly Color GodTextShadowColor1 = Color.Black * 0.25f;

	private static readonly Color GodTextShadowColor2 = Color.Black * 0.5f;

	private static readonly Color GodTextGlowColor1 = Color.White * 0.05f;

	private static readonly Color GodTextGlowColor2 = Color.White * 0.1f;

	private static readonly Color GodTextMainColor = Color.White;

	private static readonly Vector2[] GhostShadowOffsets = new Vector2[4]
	{
		new Vector2(-2f, 0f),
		new Vector2(0f, -2f),
		new Vector2(2f, 0f),
		new Vector2(0f, 2f)
	};

	private static readonly char[] ForbiddenLineStartersChinese = new char[16]
	{
		'%', ')', '.', ']', '}', '·', '’', '、', '。', '！',
		'）', '，', '：', '；', '？', '！'
	};

	private static readonly char[] ForbiddenLineStartersJapanese = new char[33]
	{
		'・', '、', ':', ',', '。', '.', ')', ']', '」', '』',
		'’', '"', 'ー', 'ァ', 'ィ', 'ゥ', 'ェ', 'ォ', 'ッ', 'ャ',
		'ュ', 'ョ', 'ぁ', 'ぅ', 'っ', 'ゃ', 'ゅ', 'ょ', '々', '—',
		'…', '！', '？'
	};

	private readonly bool _hasButtons;

	private readonly int _zoom;

	private readonly string _line;

	private readonly int _length;

	private readonly List<UIButton> _buttons = new List<UIButton>();

	private readonly SpriteFont _font;

	private readonly SpriteSheet _buttonSheet;

	private bool _areNotAllCharactersVisible;

	private bool _hasMeasuredWidth;

	private int _visibleCharacters;

	private int _lastVisibleCharacters = -1;

	private int _width;

	private string _visibleLine;

	public int Length => _length;

	public int VisibleCharacters
	{
		get
		{
			return _visibleCharacters;
		}
		set
		{
			_visibleCharacters = value;
			_areNotAllCharactersVisible = true;
		}
	}

	public int Width
	{
		get
		{
			if (!_hasMeasuredWidth)
			{
				return GetWidth();
			}
			return _width;
		}
	}

	public string Line => _line;

	internal DialogueLine(string line, SpriteFont font, SpriteSheet buttonSheet, int zoom, ControllerMapping controllerMapping)
	{
		_font = font;
		_buttonSheet = buttonSheet;
		_zoom = zoom;
		bool isAsianLocale = Loc.IsAsianLocale;
		int length = line.Length;
		char c = ' ';
		string text = line;
		int num = 0;
		for (int i = 0; i < length; i++)
		{
			char c2 = line[i];
			if (c == '$')
			{
				UIButton uIButton = UIButton.CreateFromCharacter(c2, controllerMapping);
				uIButton.IndexInLine = num - 1;
				uIButton.BaseOffsetX = (int)_font.MeasureString(text.SafeSubstring(0, num - 1)).X - 1;
				_buttons.Add(uIButton);
				_hasButtons = true;
				text = text.Insert(num, " ");
				text = text.Remove(num + 1, 1);
				if (isAsianLocale)
				{
					if (uIButton.IsWide)
					{
						text = text.Insert(num, "  ");
						num += 2;
					}
				}
				else
				{
					text = text.Insert(num, uIButton.IsWide ? "\t" : " ");
					num++;
				}
			}
			if (c2 == '$')
			{
				text = text.Insert(num, " ");
				text = text.Remove(num + 1, 1);
			}
			c = c2;
			num++;
		}
		_line = text;
		_length = _line.Length;
	}

	private int GetWidth()
	{
		int result = 0;
		if (_line != null && _font != null)
		{
			_hasMeasuredWidth = true;
			_width = (int)_font.MeasureString(_line).X;
			result = _width;
		}
		return result;
	}

	private string GetDrawLine()
	{
		if (_areNotAllCharactersVisible)
		{
			if (_lastVisibleCharacters != _visibleCharacters)
			{
				_lastVisibleCharacters = _visibleCharacters;
				_visibleLine = _line.SafeSubstring(0, _visibleCharacters);
			}
			return _visibleLine;
		}
		return _line;
	}

	internal void Draw(SpriteBatch spriteBatch, Vector2 drawPosition, Color drawColor, Color shadowDrawColor, int zoom, float alpha)
	{
		string drawLine = GetDrawLine();
		DrawingEx.DrawString(spriteBatch, _font, drawLine, new Vector2(drawPosition.X, drawPosition.Y + (float)zoom), shadowDrawColor, Vector2.Zero, zoom);
		DrawingEx.DrawString(spriteBatch, _font, drawLine, drawPosition, drawColor, Vector2.Zero, zoom);
		if (!_hasButtons)
		{
			return;
		}
		foreach (UIButton button in _buttons)
		{
			button.Draw(spriteBatch, _buttonSheet, _font, drawPosition, Color.White * alpha, zoom, _areNotAllCharactersVisible ? _visibleCharacters : (-1));
		}
	}

	internal void DrawGhostText(SpriteBatch spriteBatch, Vector2 drawPosition, int zoom, float alpha)
	{
		string drawLine = GetDrawLine();
		Color color = GodTextShadowColor1 * alpha;
		if (!Loc.IsAsianLocale)
		{
			for (int i = -3; i <= 3; i++)
			{
				for (int j = -3; j <= 3; j++)
				{
					DrawingEx.DrawString(drawPos: drawPosition + new Vector2(i * zoom, j * zoom), spriteBatch: spriteBatch, font: _font, text: drawLine, color: color, origin: Vector2.Zero, zoom: zoom);
				}
			}
			color = GodTextShadowColor2 * alpha;
		}
		else
		{
			color = Color.Black * 0.65f * alpha;
		}
		for (int k = -2; k <= 2; k++)
		{
			for (int l = -2; l <= 2; l++)
			{
				DrawingEx.DrawString(drawPos: drawPosition + new Vector2(k * zoom, l * zoom), spriteBatch: spriteBatch, font: _font, text: drawLine, color: color, origin: Vector2.Zero, zoom: zoom);
			}
		}
		color = GodTextGlowColor1 * alpha;
		Vector2[] ghostShadowOffsets = GhostShadowOffsets;
		foreach (Vector2 vector in ghostShadowOffsets)
		{
			DrawingEx.DrawString(spriteBatch, _font, drawLine, drawPosition + vector * _zoom, color, Vector2.Zero, zoom);
		}
		color = GodTextGlowColor2 * alpha;
		for (int n = -1; n <= 1; n++)
		{
			for (int num = -1; num <= 1; num++)
			{
				DrawingEx.DrawString(drawPos: drawPosition + new Vector2(n * zoom, num * zoom), spriteBatch: spriteBatch, font: _font, text: drawLine, color: color, origin: Vector2.Zero, zoom: zoom);
			}
		}
		color = GodTextMainColor * alpha;
		DrawingEx.DrawString(spriteBatch, _font, drawLine, drawPosition, color, Vector2.Zero, zoom);
	}

	internal static List<DialogueLine> SplitMessageIntoLines(string message, int width, SpriteFont font, SpriteSheet buttonsSprite, int zoom, ControllerMapping controllerMapping)
	{
		List<DialogueLine> list = new List<DialogueLine>();
		int length = message.Length;
		bool isAsianLocale = Loc.IsAsianLocale;
		bool flag = Loc.CurrentLocale == ELanguageLocale.JP;
		int num = width - 30;
		float num2 = 0f;
		int num3 = 0;
		int num4 = -1;
		int num5 = -1;
		int num6 = 0;
		for (int i = 0; i < length; i++)
		{
			num6++;
			bool flag2 = false;
			char c = message[i];
			float num7 = font.MeasureString(c.ToString(CultureInfo.InvariantCulture)).X * (float)zoom;
			num2 += num7;
			bool flag3 = c == '\n';
			if (flag3)
			{
				flag2 = true;
			}
			else
			{
				switch (c)
				{
				case ' ':
					num4 = i;
					num5 = -1;
					break;
				case '$':
					num5 = i;
					break;
				}
			}
			if (num2 > (float)num || i == length - 1)
			{
				flag2 = true;
				if ((!isAsianLocale || num5 != -1) && num4 != -1 && i < length - 1)
				{
					int num8 = i - num4;
					i = num4;
					num6 -= num8;
				}
			}
			if (flag2 && isAsianLocale)
			{
				int num9 = i + 1;
				if (num9 < length)
				{
					char c2 = message[num9];
					char[] array = (flag ? ForbiddenLineStartersJapanese : ForbiddenLineStartersChinese);
					char[] array2 = array;
					foreach (char c3 in array2)
					{
						if (c2 == c3)
						{
							i--;
							num6--;
							break;
						}
					}
				}
			}
			if (flag2)
			{
				int length2 = (flag3 ? (num6 - 1) : num6);
				string line = message.SafeSubstring(num3, length2);
				list.Add(new DialogueLine(line, font, buttonsSprite, zoom, controllerMapping));
				num3 += num6;
				num6 = 0;
				num4 = -1;
				num5 = -1;
				num2 = 0f;
			}
		}
		return list;
	}
}
