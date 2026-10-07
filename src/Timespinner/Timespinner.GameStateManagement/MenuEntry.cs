using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

namespace Timespinner.GameStateManagement;

internal class MenuEntry
{
	private const int DefaultColumnWidth = 256;

	public static Color SelectedColor = new Color(208, 200, 152);

	public static Color UnselectedColor = Color.LightGray;

	public static Color UnavailableColor = Color.Gray;

	private bool _wasSelected;

	private float _pulsateDelta;

	private float _colorPulsateAmount;

	private float _alpha;

	private Color _drawColor;

	private ScrollableTextBlock _scrollableTextBlock;

	internal bool DoesConfirmationPlaySound { get; set; }

	internal bool WasSelected => _wasSelected;

	internal bool IsHighlighted { get; set; }

	internal bool IsScrolledOff { get; set; }

	internal bool IsCenterAligned { get; set; }

	internal bool DoesDrawLargeShadow { get; set; }

	internal int XOffset { get; private set; }

	internal int ColumnWidth { get; set; }

	internal int EntryTextMarginX { get; set; }

	internal string Text { get; private set; }

	internal string Description { get; set; }

	internal Vector2 DrawPosition { get; set; }

	internal Vector2 DrawOffset { get; set; }

	internal Color BaseDrawColor { get; set; }

	internal event EventHandler<PlayerIndexEventArgs> Selected;

	public MenuEntry(string text)
	{
		DoesConfirmationPlaySound = true;
		BaseDrawColor = UnselectedColor;
		XOffset = 0;
		ColumnWidth = 256;
		SetText(text);
	}

	internal virtual void OnSelectEntry(PlayerIndex playerIndex)
	{
		if (this.Selected != null)
		{
			this.Selected(this, new PlayerIndexEventArgs(playerIndex));
		}
	}

	internal void SetText(string text)
	{
		Text = text;
		_scrollableTextBlock = null;
	}

	public virtual void Update(bool isSelected, float delta, float transitionPercentage, Vector2 position)
	{
		DrawPosition = position;
		_pulsateDelta += delta;
		if (_pulsateDelta > 60f)
		{
			_pulsateDelta -= 60f;
		}
		_colorPulsateAmount = (float)Math.Sin(_pulsateDelta * 4f) + 1f;
		if (isSelected)
		{
			_drawColor = Color.Lerp(SelectedColor, Color.White, _colorPulsateAmount / 2f);
			if (!_wasSelected)
			{
				_pulsateDelta = 0f;
			}
		}
		else
		{
			_drawColor = BaseDrawColor;
		}
		_alpha = 1f - transitionPercentage;
		_drawColor *= _alpha;
		_wasSelected = isSelected;
	}

	public virtual void Draw(SpriteBatch spriteBatch, SpriteFont font, float zoom)
	{
		if (IsCenterAligned && XOffset == 0 && Text != null)
		{
			XOffset = (int)Math.Round(font.MeasureString(Text).X / 2f);
		}
		else if (!IsCenterAligned && XOffset != 0)
		{
			XOffset = 0;
		}
		Vector2 origin = new Vector2(XOffset, (int)((float)(font.LineSpacing + 1) / 2f));
		Vector2 vector = new Vector2((int)(DrawPosition.X + DrawOffset.X), (int)(DrawPosition.Y + DrawOffset.Y));
		Vector2 drawPos = new Vector2(vector.X, vector.Y + zoom);
		Color color = _drawColor * (_wasSelected ? 0.35f : 0.15f);
		if (DoesDrawLargeShadow)
		{
			DrawingEx.DrawLargeTextShadow(spriteBatch, font, Text, vector, zoom, origin, _alpha);
			DrawingEx.DrawString(spriteBatch, font, Text, drawPos, color, origin, zoom);
			DrawingEx.DrawString(spriteBatch, font, Text, vector, _drawColor, origin, zoom);
			return;
		}
		Vector2 vector2 = new Vector2(vector.X, vector.Y - origin.Y * zoom);
		if (_scrollableTextBlock == null)
		{
			_scrollableTextBlock = new ScrollableTextBlock(font, ColumnWidth + EntryTextMarginX, vector2, isTextCentered: false);
			_scrollableTextBlock.SetText(Text);
		}
		if (vector2 != _scrollableTextBlock.TopLeft)
		{
			_scrollableTextBlock.SetTopLeft(vector2);
		}
		_scrollableTextBlock.Draw(spriteBatch, _drawColor, color, null);
	}

	public void OverrideDrawColor(Color newColor)
	{
		_drawColor = newColor;
	}
}
