using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;

namespace Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

internal class ScrollableTextBlock
{
	private const int LatinCharacterWidth = 6;

	private const float UpdateRate = 1f / 60f;

	private const float FinalCharacterColorMultiplier = 0.5f;

	private const float TimeForScrollWait = 1f;

	private const float TimeForScrollOutWait = 1f;

	private const float TimeForSingleLetterToScroll = 0.2f;

	private readonly bool _isTextCentered;

	private readonly int _baseWidth;

	private readonly SpriteFont _font;

	private bool _doesScroll;

	private int _baseTextWidth;

	private int _zoom;

	private int _displayTextWidth;

	private int _sourceTextLength;

	private int _centerTextLength;

	private int _scrollIndex;

	private int _totalLettersToScroll;

	private string _sourceText;

	private string _displayText;

	private string _finalCharacterText;

	private float _scrollTimer;

	private Vector2 _topLeft;

	private Vector2 _textOrigin;

	private Vector2 _center;

	internal Vector2 TopLeft => _topLeft;

	internal ScrollableTextBlock(SpriteFont spriteFont, int baseWidth, Vector2 topLeft, bool isTextCentered)
	{
		_font = spriteFont;
		_baseWidth = baseWidth;
		_isTextCentered = isTextCentered;
		_zoom = Constants.InGameZoom;
		SetTopLeft(topLeft);
	}

	internal void SetText(string text)
	{
		if (_sourceText != text)
		{
			_sourceText = text;
			MeasureString();
		}
	}

	internal void SetTopLeft(Vector2 topLeft)
	{
		_topLeft = topLeft;
		if (_zoom != Constants.InGameZoom)
		{
			RefreshSizes();
		}
		else
		{
			RecalculateCenter();
		}
	}

	private void RecalculateCenter()
	{
		_center = new Vector2(_topLeft.X + (float)(_zoom * (int)((float)_baseWidth * 0.5f)), _topLeft.Y);
	}

	internal void RefreshSizes()
	{
		_zoom = Constants.InGameZoom;
		RecalculateCenter();
		ResetScrolling();
	}

	private void MeasureString()
	{
		_sourceTextLength = _sourceText.Length;
		Vector2 vector = _font.MeasureString(_sourceText);
		_textOrigin = new Vector2((int)(vector.X * 0.5f), 0f);
		int num = 6;
		if (_sourceTextLength > 0)
		{
			num = (int)Math.Ceiling(vector.X / (float)_sourceTextLength);
		}
		_baseTextWidth = _sourceText.Length * num;
		_doesScroll = _baseTextWidth >= _baseWidth;
		_displayText = _sourceText;
		_finalCharacterText = "";
		if (_doesScroll)
		{
			_centerTextLength = _baseWidth / num;
			_totalLettersToScroll = _sourceTextLength - _centerTextLength;
			if (_centerTextLength > 1)
			{
				_centerTextLength--;
			}
		}
		else
		{
			_centerTextLength = _sourceTextLength;
			_totalLettersToScroll = 0;
		}
		ResetScrolling();
	}

	private void ResetScrolling()
	{
		_scrollIndex = -1;
		_scrollTimer = 0f;
		UpdateScrolling(0f);
	}

	internal void Update(float delta)
	{
		UpdateScrolling(delta);
	}

	private void UpdateScrolling(float delta)
	{
		if (!_doesScroll)
		{
			return;
		}
		int scrollIndex = _scrollIndex;
		_scrollTimer += delta;
		if (_scrollIndex == -1)
		{
			_scrollIndex = 0;
		}
		if (_scrollIndex == 0)
		{
			if (_scrollTimer >= 1f)
			{
				_scrollIndex++;
				_scrollTimer = 0f;
			}
		}
		else if (_scrollIndex < _totalLettersToScroll)
		{
			if (_scrollTimer >= 0.2f)
			{
				_scrollIndex++;
				_scrollTimer -= 0.2f;
			}
		}
		else if (_scrollTimer >= 1f)
		{
			_scrollIndex = 0;
			_scrollTimer = 0f;
		}
		if (scrollIndex != _scrollIndex)
		{
			int num = _sourceTextLength - (_scrollIndex + _centerTextLength);
			_displayText = _sourceText.SafeSubstring(_scrollIndex, _centerTextLength);
			_displayTextWidth = (int)_font.MeasureString(_displayText).X;
			_finalCharacterText = ((num > 0) ? _sourceText.SafeSubstring(_scrollIndex + _centerTextLength, 1) : "");
		}
	}

	internal void Draw(SpriteBatch spriteBatch, Color textColor, Color textShadowColor, Texture2D squareTexture)
	{
		if (_doesScroll)
		{
			Update(1f / 60f);
		}
		if (_doesScroll || !_isTextCentered)
		{
			DrawingEx.DrawString(spriteBatch, _font, _displayText, new Vector2(_topLeft.X, _topLeft.Y + (float)_zoom), textShadowColor, Vector2.Zero, _zoom);
			DrawingEx.DrawString(spriteBatch, _font, _displayText, _topLeft, textColor, Vector2.Zero, _zoom);
			if (!string.IsNullOrEmpty(_finalCharacterText))
			{
				Vector2 drawPos = new Vector2(_topLeft.X + (float)(_displayTextWidth * _zoom), _topLeft.Y);
				DrawingEx.DrawString(spriteBatch, _font, _finalCharacterText, new Vector2(drawPos.X, drawPos.Y + (float)_zoom), textShadowColor * 0.5f, Vector2.Zero, _zoom);
				DrawingEx.DrawString(spriteBatch, _font, _finalCharacterText, drawPos, textColor * 0.5f, Vector2.Zero, _zoom);
			}
		}
		else
		{
			DrawingEx.DrawString(spriteBatch, _font, _displayText, new Vector2(_center.X, _center.Y + (float)_zoom), textShadowColor, _textOrigin, _zoom);
			DrawingEx.DrawString(spriteBatch, _font, _displayText, _center, textColor, _textOrigin, _zoom);
		}
	}

	internal void DrawShadowed(SpriteBatch spriteBatch, Color textColor, Color textShadowColor)
	{
		if (_doesScroll)
		{
			Update(1f / 60f);
		}
		if (_doesScroll || !_isTextCentered)
		{
			DrawShadowedString(spriteBatch, _font, _displayText, _topLeft, textColor, textShadowColor, Vector2.Zero, _zoom);
			if (!string.IsNullOrEmpty(_finalCharacterText))
			{
				DrawShadowedString(drawPosition: new Vector2(_topLeft.X + (float)(_displayTextWidth * _zoom), _topLeft.Y), spriteBatch: spriteBatch, font: _font, text: _finalCharacterText, drawColor: textColor * 0.5f, baseShadowColor: textShadowColor * 0.5f, origin: Vector2.Zero, zoom: _zoom);
			}
		}
		else
		{
			DrawShadowedString(spriteBatch, _font, _displayText, _center, textColor, textShadowColor, _textOrigin, _zoom);
		}
	}

	private static void DrawShadowedString(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 drawPosition, Color drawColor, Color baseShadowColor, Vector2 origin, int zoom)
	{
		Color color = baseShadowColor * 0.25f;
		for (int i = -2; i <= 2; i++)
		{
			for (int j = -2; j <= 2; j++)
			{
				DrawingEx.DrawString(spriteBatch, font, text, drawPosition.Add(new Point(i * zoom, j * zoom)), color, origin, zoom);
			}
		}
		Color color2 = baseShadowColor * 0.5f;
		for (int k = -1; k <= 1; k++)
		{
			for (int l = -1; l <= 1; l++)
			{
				DrawingEx.DrawString(spriteBatch, font, text, drawPosition.Add(new Point(k * zoom, l * zoom)), color2, origin, zoom);
			}
		}
		DrawingEx.DrawString(spriteBatch, font, text, drawPosition, drawColor, origin, zoom);
	}
}
