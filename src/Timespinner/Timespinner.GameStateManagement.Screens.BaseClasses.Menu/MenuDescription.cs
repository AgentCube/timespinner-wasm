using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;

namespace Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

internal class MenuDescription
{
	private const int ThreeLineDescriptionSpacingMarginY = -4;

	private const int ThreeLineDescriptionStartOffsetY = -4;

	private const int FourLineDescriptionSpacingMarginY = -6;

	private const int FourLineDescriptionStartOffsetY = -8;

	private const int BaseIconOffsetX = -17;

	private const int BaseIconOffsetY = 7;

	private const int BaseIconTextOffsetX = 10;

	internal const int DescriptionColumnWidth = 275;

	internal static readonly Color DescriptionDrawColor = new Color(240, 240, 208);

	internal static readonly Color DescriptionShadowColor = new Color(60, 60, 24);

	private readonly bool _hasIcon;

	private readonly bool _isCentered;

	private readonly int _newlineHeight;

	private readonly int _textStartOffsetY;

	private readonly EInventoryItemIcon _icon;

	private readonly string _text;

	private readonly SpriteFont _font;

	private readonly SpriteSheet _iconSheet;

	private readonly List<DialogueLine> _descriptionLines;

	private int _zoom;

	private int _descriptionWidth;

	private int _iconOffsetX;

	private int _iconOffsetY;

	private int _iconTextOffsetX;

	internal bool HasIcon => _hasIcon;

	internal EInventoryItemIcon Icon { get; set; }

	internal MenuDescription(string text, SpriteFont font, EInventoryItemIcon icon, SpriteSheet iconSheet, SpriteSheet buttonSheet, bool isCentered, ControllerMapping controllerMapping)
	{
		_text = text;
		_font = font;
		_icon = icon;
		_iconSheet = iconSheet;
		_isCentered = isCentered;
		_hasIcon = _icon != 0 && _iconSheet != null;
		RefreshZoom(Constants.InGameZoom);
		_descriptionLines = ((text != null) ? DialogueLine.SplitMessageIntoLines(text, _descriptionWidth, _font, buttonSheet, _zoom, controllerMapping) : null);
		_newlineHeight = font.LineSpacing;
		if (_descriptionLines != null && _descriptionLines.Count > 2)
		{
			int count = _descriptionLines.Count;
			if (count == 3)
			{
				_newlineHeight += -4;
				_textStartOffsetY = -4;
			}
			else
			{
				_newlineHeight += -6;
				_textStartOffsetY = -8;
			}
		}
	}

	private void RefreshZoom(int zoom)
	{
		_zoom = zoom;
		_descriptionWidth = 275 * _zoom;
		_iconOffsetX = -17 * _zoom;
		_iconOffsetY = 7 * _zoom;
		_iconTextOffsetX = 10 * _zoom;
	}

	internal void Draw(SpriteBatch spriteBatch, Vector2 drawPosition, int zoom)
	{
		if (_text == null || _descriptionLines == null)
		{
			return;
		}
		if (zoom != _zoom)
		{
			RefreshZoom(zoom);
		}
		if (_hasIcon)
		{
			Rectangle frameSource = _iconSheet.GetFrameSource((int)(_icon - 1));
			spriteBatch.Draw(_iconSheet.Texture, drawPosition.Add(new Point(_iconOffsetX, _iconOffsetY)), frameSource, Color.White, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
			drawPosition.X += _iconTextOffsetX;
		}
		Vector2 drawPosition2 = new Vector2(drawPosition.X, drawPosition.Y + (float)(_textStartOffsetY * zoom));
		foreach (DialogueLine descriptionLine in _descriptionLines)
		{
			if (_isCentered)
			{
				int num = (int)((float)(_descriptionWidth - descriptionLine.Width * zoom) * 0.5f);
				drawPosition2 = new Vector2(drawPosition.X + (float)num, drawPosition2.Y);
			}
			descriptionLine.Draw(spriteBatch, drawPosition2, DescriptionDrawColor, DescriptionShadowColor, zoom, 1f);
			drawPosition2.Y += _newlineHeight * zoom;
		}
	}
}
