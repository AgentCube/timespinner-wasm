using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core.Constants;
using Timespinner.GameAbstractions.Inventory;

namespace Timespinner.GameStateManagement;

internal class MenuEntryCollection
{
	private readonly List<MenuEntry> _entries = new List<MenuEntry>();

	private bool _doesMenuAllowScrolling;

	private int _currentRow;

	private int _entryHeight;

	private int _entryHeightOffset;

	private int _entryTextColumnWidth;

	private float _zoom;

	private SpriteFont _font;

	public bool DoesMenuSlideOnTransition { get; set; }

	internal bool DoesMenuAllowScrolling
	{
		get
		{
			return _doesMenuAllowScrolling;
		}
		set
		{
			_doesMenuAllowScrolling = value;
			ScrollPercentage = ((!value) ? (-1) : 0);
		}
	}

	public bool IsVisible { get; set; }

	internal bool IsCenterAligned { get; private set; }

	public int SelectedIndex { get; set; }

	public int EntryHeight => _entryHeight;

	public int EntryHeightOffset
	{
		get
		{
			return _entryHeightOffset;
		}
		set
		{
			_entryHeightOffset = value;
			RefreshEntryHeight();
		}
	}

	public int ColumnCount { get; set; }

	public int ColumnWidth { get; private set; }

	internal int TextMarginX { get; set; }

	internal int ScrollRowHeight { get; set; }

	internal int LineSpacing
	{
		get
		{
			if (_font == null)
			{
				return 0;
			}
			return _font.LineSpacing;
		}
	}

	internal float ScrollPercentage { get; private set; }

	public Vector2 DrawPosition { get; set; }

	public IList<MenuEntry> Entries => _entries;

	public SpriteFont Font
	{
		get
		{
			return _font;
		}
		set
		{
			_font = value;
			RefreshEntryHeight();
		}
	}

	public MenuEntryCollection()
	{
		DoesMenuSlideOnTransition = true;
		ColumnCount = 1;
		ColumnWidth = 600;
		ScrollPercentage = -1f;
	}

	internal void SetColumnWidth(int width, int zoom)
	{
		ColumnWidth = width;
		if (zoom > 0)
		{
			_entryTextColumnWidth = width / zoom + TextMarginX;
			RefreshEntryWidths();
		}
	}

	internal void RefreshEntryWidths()
	{
		foreach (MenuEntry entry in _entries)
		{
			entry.ColumnWidth = _entryTextColumnWidth;
		}
	}

	private void RefreshEntryHeight()
	{
		if (_font != null)
		{
			_entryHeight = Constants.InGameZoom * (_font.LineSpacing + EntryHeightOffset);
		}
	}

	internal Point GetMenuDimensions()
	{
		RefreshEntryHeight();
		int y = _entryHeight * _entries.Count;
		int num = 0;
		foreach (MenuEntry entry in _entries)
		{
			Vector2 vector = _font.MeasureString(entry.Text) * Constants.InGameZoom;
			if (vector.X > (float)num)
			{
				num = (int)vector.X;
			}
		}
		return new Point(num, y);
	}

	internal virtual EInventoryItemIcon GetSelectedIcon()
	{
		return EInventoryItemIcon.None;
	}

	internal void SetIsCenterAligned(bool isCenterAligned)
	{
		IsCenterAligned = isCenterAligned;
		foreach (MenuEntry entry in _entries)
		{
			entry.IsCenterAligned = isCenterAligned;
		}
	}

	internal void SetDoesDrawLargeShadow(bool doesDrawLargeShadow)
	{
		foreach (MenuEntry entry in _entries)
		{
			entry.DoesDrawLargeShadow = doesDrawLargeShadow;
		}
	}

	public virtual void Update(float delta, bool isScreenActive, float transitionPercentage)
	{
		Vector2 drawPosition = DrawPosition;
		int num = 0;
		if (DoesMenuAllowScrolling)
		{
			drawPosition.Y -= _currentRow * EntryHeight;
		}
		for (int i = 0; i < Entries.Count; i++)
		{
			drawPosition.X = DrawPosition.X + (float)(i % ColumnCount * ColumnWidth);
			bool isSelected = isScreenActive && i == SelectedIndex;
			Entries[i].Update(isSelected, delta, transitionPercentage, drawPosition);
			if (DoesMenuAllowScrolling)
			{
				Entries[i].IsScrolledOff = num < _currentRow || num > _currentRow + ScrollRowHeight - 1;
			}
			if (ColumnCount == 1 || (i + 1) % ColumnCount == 0)
			{
				drawPosition.Y += EntryHeight;
				num++;
			}
		}
	}

	public virtual void Draw(SpriteBatch spriteBatch, float zoom)
	{
		_zoom = zoom;
		if (!IsVisible)
		{
			return;
		}
		foreach (MenuEntry entry in Entries)
		{
			if (!DoesMenuAllowScrolling || !entry.IsScrolledOff)
			{
				entry.Draw(spriteBatch, Font, zoom);
			}
		}
	}

	public Vector2 GetCursorPosition()
	{
		int num = (int)Math.Floor((float)SelectedIndex / (float)ColumnCount);
		if (DoesMenuAllowScrolling)
		{
			num -= _currentRow;
		}
		Vector2 value;
		if (IsCenterAligned)
		{
			int num2 = 0;
			if (SelectedIndex < _entries.Count)
			{
				num2 = -_entries[SelectedIndex].XOffset;
			}
			value = new Vector2((float)(ColumnWidth * (SelectedIndex % ColumnCount)) + (float)num2 * _zoom, num * EntryHeight);
		}
		else
		{
			value = new Vector2(ColumnWidth * (SelectedIndex % ColumnCount), num * EntryHeight);
		}
		return Vector2.Add(DrawPosition, value);
	}

	public virtual bool MoveSelection(EMenuMoveDirection direction, PlayerIndex? controllingPlayer)
	{
		bool result = false;
		int count = Entries.Count;
		bool flag = ColumnCount > 1;
		bool flag2 = count > ColumnCount;
		if (count > 0)
		{
			int num = 0;
			switch (direction)
			{
			case EMenuMoveDirection.Up:
				if (flag2)
				{
					num = -ColumnCount;
				}
				break;
			case EMenuMoveDirection.Right:
				if (flag)
				{
					num = 1;
				}
				else if (count > ScrollRowHeight)
				{
					num = ScrollRowHeight;
					if (num + SelectedIndex > count)
					{
						num = count - SelectedIndex - 1;
					}
				}
				break;
			case EMenuMoveDirection.Down:
				if (flag2)
				{
					num = ColumnCount;
				}
				break;
			case EMenuMoveDirection.Left:
				if (flag)
				{
					num = -1;
				}
				else if (count > ScrollRowHeight)
				{
					num = -ScrollRowHeight;
					if (SelectedIndex + num < 0)
					{
						num = -SelectedIndex;
					}
				}
				break;
			}
			if (num != 0)
			{
				result = true;
				SelectedIndex += num;
				if (SelectedIndex < 0)
				{
					SelectedIndex = Math.Max(count - 1, 0);
				}
				else if (SelectedIndex >= count)
				{
					SelectedIndex = 0;
				}
				if (DoesMenuAllowScrolling && ColumnCount > 0)
				{
					RefreshScrollWindow();
				}
			}
		}
		return result;
	}

	internal bool SetSelectedIndex(int index)
	{
		bool result = false;
		if (index < _entries.Count)
		{
			SelectedIndex = index;
			result = true;
		}
		return result;
	}

	internal void RefreshScrollWindow()
	{
		int num = (int)Math.Floor((float)SelectedIndex / (float)ColumnCount);
		if (_currentRow > num)
		{
			_currentRow = num;
		}
		else if (_currentRow + ScrollRowHeight - 1 < num)
		{
			_currentRow = num - (ScrollRowHeight - 1);
		}
		float num2 = (float)Math.Ceiling((float)Entries.Count / (float)ColumnCount) - (float)ScrollRowHeight;
		ScrollPercentage = ((num2 > 0f) ? ((float)_currentRow / num2) : (-1f));
	}

	public virtual bool SelectEntry(PlayerIndex playerIndex)
	{
		bool result = false;
		if (Entries.Count > SelectedIndex)
		{
			MenuEntry menuEntry = Entries[SelectedIndex];
			menuEntry.OnSelectEntry(playerIndex);
			result = menuEntry.DoesConfirmationPlaySound;
		}
		return result;
	}
}
