using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameStateManagement.Screens.PauseMenu;

internal class ControlsMenuEntryCollection : MenuEntryCollection
{
	private const int ButtonMarginX = -32;

	private const int ButtonMarginY = -8;

	private const int ControlsTextMarginX = -32;

	private const float TimeForTextToFlash = 0.5f;

	private static readonly Color TextFlashStartColor = Color.White;

	private static readonly Color TextFlashEndColor = Color.PaleVioletRed;

	private static readonly int[] ProhibitedHeroButtonMappings = new int[2] { 5, 6 };

	private static readonly int[] ProhibitedFamiliarButtonMappings = new int[1] { 6 };

	private static readonly int[] ProhibitedMenuButtonMappings = new int[0];

	private readonly EControllerMappingType _controlType;

	private readonly SpriteSheet _buttonSpriteSheet;

	private readonly Action<ControlsMenuEntry> _onSelectedAction;

	private readonly List<ControlsMenuEntry> _buttonEntries = new List<ControlsMenuEntry>();

	private float _textColorFlashCounter;

	private ControllerMapping _controllerMapping;

	internal bool IsActive { get; set; }

	internal bool IsAnEntryBeingHighlighted { get; set; }

	internal IEnumerable<ControlsMenuEntry> ButtonEntries => _buttonEntries;

	internal ControlsMenuEntryCollection(ControllerMapping controllerMapping, SpriteSheet buttonSpriteSheet, Action<ControlsMenuEntry> onSelectedAction, EControllerMappingType controlType)
	{
		_controllerMapping = controllerMapping;
		_buttonSpriteSheet = buttonSpriteSheet;
		_onSelectedAction = onSelectedAction;
		_controlType = controlType;
		base.ColumnCount = 2;
		base.ScrollRowHeight = 5;
		base.DoesMenuAllowScrolling = true;
		base.TextMarginX = -32;
		PopulateEntries();
	}

	private void PopulateEntries()
	{
		_buttonEntries.Clear();
		base.Entries.Clear();
		int[] array = _controlType switch
		{
			EControllerMappingType.Familiar => ProhibitedFamiliarButtonMappings, 
			EControllerMappingType.Menu => ProhibitedMenuButtonMappings, 
			_ => ProhibitedHeroButtonMappings, 
		};
		foreach (KeyValuePair<int, ButtonMapping> mapping in _controllerMapping.Mappings)
		{
			bool flag = true;
			int[] array2 = array;
			foreach (int num in array2)
			{
				if (mapping.Key == num)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				ControlsMenuEntry item = new ControlsMenuEntry(mapping.Value, mapping.Value.GetDestinationText(_controlType));
				_buttonEntries.Add(item);
				base.Entries.Add(item);
			}
		}
	}

	public override bool SelectEntry(PlayerIndex playerIndex)
	{
		if (base.Entries.Count > 0)
		{
			_onSelectedAction(_buttonEntries[base.SelectedIndex]);
		}
		return true;
	}

	public override void Update(float delta, bool isScreenActive, float transitionPercentage)
	{
		base.Update(delta, isScreenActive, transitionPercentage);
		if (IsAnEntryBeingHighlighted)
		{
			_textColorFlashCounter += delta;
			if (_textColorFlashCounter >= 0.5f)
			{
				_textColorFlashCounter -= 0.5f;
			}
		}
	}

	public override void Draw(SpriteBatch spriteBatch, float zoom)
	{
		if (!IsActive)
		{
			return;
		}
		if (base.Font != null)
		{
			base.IsVisible = true;
			int num = (int)(-32f * zoom);
			int y = (int)(-8f * zoom);
			Vector2 origin = new Vector2(0f, (float)base.Font.LineSpacing / 2f);
			Color color = Color.Lerp(TextFlashStartColor, TextFlashEndColor, (float)Math.Sin(_textColorFlashCounter / 0.5f * ((float)Math.PI / 2f)));
			Color color2 = (IsAnEntryBeingHighlighted ? MenuEntry.UnavailableColor : MenuEntry.UnselectedColor);
			Color color3 = (IsAnEntryBeingHighlighted ? Color.Gray : Color.White);
			foreach (ControlsMenuEntry buttonEntry in _buttonEntries)
			{
				if (!buttonEntry.IsScrolledOff)
				{
					if (buttonEntry.Button.UIButton != null)
					{
						Vector2 position = buttonEntry.DrawPosition.Add(new Point(base.ColumnWidth + num, y));
						Color color4 = (buttonEntry.IsAwaitingInput ? Color.White : color3);
						buttonEntry.Button.UIButton.Draw(spriteBatch, _buttonSpriteSheet, base.Font, position, color4, (int)zoom, -1);
						continue;
					}
					string sourceText = buttonEntry.Button.GetSourceText();
					float num2 = base.Font.MeasureString(sourceText).X * zoom;
					Color color5 = (buttonEntry.IsAwaitingInput ? color : color2);
					Vector2 a = buttonEntry.DrawPosition.Add(new Point(base.ColumnWidth - 32, 0));
					Vector2 drawPos = a.Add(new Point(-(int)num2, 0));
					DrawingEx.DrawString(spriteBatch, base.Font, sourceText, drawPos, color5, origin, zoom);
				}
			}
		}
		base.Draw(spriteBatch, zoom);
	}

	public void ChangeMapping(ControllerMapping newControllerMapping)
	{
		_controllerMapping = newControllerMapping;
		PopulateEntries();
		RefreshEntryWidths();
		Update(0f, isScreenActive: true, 1f);
	}
}
