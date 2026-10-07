using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;

namespace Timespinner.GameAbstractions.HUD;

internal class HudItemGetBanner : HudElement
{
	private const string JournalNameKey = "inv_jou_{0}";

	private const string JournalGetPrefixKeyMemory = "GetItem_Journal_Memory";

	private const string JournalGetPrefixKeyLetter = "GetItem_Journal_Letter";

	private const string JournalGetPrefixKeyFile = "GetItem_Journal_Download";

	private const string MapGetPrefixKey = "GetMap_Download";

	private const int MapUseItemValue = 128;

	private const int BorderPaddingX = 12;

	private const int BorderPaddingY = 0;

	private const int BorderHeight = 22;

	private const int ViewButtonOffsetX = -10;

	private const int ViewButtonOffsetY = 2;

	private const float TimeToFadeIn = 0.1f;

	private const float TimeToDisplay = 2f;

	private const float TimeToFadeOut = 0.2f;

	private const float TotalDisplayTime = 2.3f;

	private const float TimeBeforeFadingOut = 2.1f;

	private static readonly Vector2 TextSizeMargin = new Vector2(-2f, 0f);

	private static readonly Color BaseTextColor = new Color(240, 240, 208);

	private static readonly Color BaseButtonColor = Color.White * 0.8f;

	private readonly SpriteSheet _buttonsSpriteSheet;

	private EInventoryUseItemType _useItem;

	private EInventoryEquipmentType _equipment;

	private EInventoryJournalType _journal;

	private float _displayTimer;

	private float _alpha;

	private string _itemName;

	private Color _textDrawColor;

	private Color _borderColor;

	private Color _buttonDrawColor;

	private UIButton _viewButton;

	internal bool IsFinished => _displayTimer > 2.3f;

	internal bool IsShowingViewPrompt { get; set; }

	internal bool IsMapReveal { get; private set; }

	internal EInventoryCategoryType ItemCategory { get; private set; }

	internal int ItemValue { get; private set; }

	internal int MapRevealLevelIndex { get; private set; }

	public HudItemGetBanner(GCM inGCM, Rectangle titleSafeArea)
		: base(inGCM, GetDrawPositionFromTitleSafeArea(titleSafeArea))
	{
		_buttonsSpriteSheet = inGCM.SpUIButtons;
	}

	private static Point GetDrawPositionFromTitleSafeArea(Rectangle titleSafeArea)
	{
		return new Point(titleSafeArea.Left, titleSafeArea.Bottom - Constants.InGameZoom * 4);
	}

	private void ShowNewItem(EInventoryCategoryType category, int value, ControllerMapping controllerMapping)
	{
		_viewButton = controllerMapping.CreateUIButtonFromDestination(ButtonMapping.EDestinationType.Pause);
		ItemCategory = category;
		ItemValue = value;
		_itemName = null;
		IsMapReveal = false;
		switch (category)
		{
		case EInventoryCategoryType.UseItem:
		{
			IsShowingViewPrompt = true;
			_useItem = (EInventoryUseItemType)value;
			if (value < 128)
			{
				_itemName = InventoryItem.NameFromType(_useItem);
				break;
			}
			string text2 = Loc.Get("GetMap_Download");
			int num = value - 128;
			IsMapReveal = true;
			switch (num)
			{
			case 0:
				MapRevealLevelIndex = 7;
				break;
			default:
				MapRevealLevelIndex = 1;
				break;
			case 2:
				MapRevealLevelIndex = 2;
				break;
			case 3:
				MapRevealLevelIndex = 11;
				break;
			}
			string levelNameFromID = Level.GetLevelNameFromID(MapRevealLevelIndex);
			_itemName = text2 + levelNameFromID;
			break;
		}
		case EInventoryCategoryType.Equipment:
			_equipment = (EInventoryEquipmentType)value;
			_itemName = InventoryItem.NameFromType(_equipment);
			IsShowingViewPrompt = true;
			break;
		case EInventoryCategoryType.Journal:
		{
			_journal = (EInventoryJournalType)value;
			string text = ((value < 32) ? Loc.Get("GetItem_Journal_Memory") : ((value >= 64) ? Loc.Get("GetItem_Journal_Download") : Loc.Get("GetItem_Journal_Letter")));
			_itemName = text + Loc.Get($"inv_jou_{_journal}");
			IsShowingViewPrompt = true;
			break;
		}
		}
		_displayTimer = 0f;
	}

	public void Update(float delta, Level level, ControllerMapping controllerMapping)
	{
		if (level != null && level.IsRequestingItemGetPopup)
		{
			ShowNewItem(level.ItemGetCategory, level.ItemGetValue, controllerMapping);
			level.IsRequestingItemGetPopup = false;
		}
		if (!IsFinished)
		{
			_displayTimer += delta;
			_alpha = 0f;
			if (_displayTimer < 0.1f)
			{
				_alpha = (float)Math.Sin((float)Math.PI / 2f * (_displayTimer / 0.1f));
			}
			else if (_displayTimer < 2.1f)
			{
				_alpha = 1f;
			}
			else if (_displayTimer < 2.3f)
			{
				_alpha = (float)Math.Cos((float)Math.PI / 2f * (_displayTimer - 2.1f) / 0.2f);
			}
			else
			{
				IsShowingViewPrompt = false;
			}
			_textDrawColor = BaseTextColor * _alpha;
			_borderColor = Color.White * _alpha;
			_buttonDrawColor = BaseButtonColor * _alpha;
			base.Update(delta);
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (!IsFinished && _itemName != null)
		{
			int num = 12 * base.Zoom;
			_ = base.Zoom;
			int num2 = 0;
			SpriteFont activeFont = _gcm.ActiveFont;
			Vector2 vector = (activeFont.MeasureString(_itemName) + TextSizeMargin) * base.Zoom;
			int num3 = (int)vector.X + num * 2;
			Vector2 drawPos = new Vector2(_drawPosition.X + num, (float)_drawPosition.Y - vector.Y / 2f);
			SpriteEffects[] flipped = new SpriteEffects[3]
			{
				SpriteEffects.None,
				SpriteEffects.None,
				SpriteEffects.FlipHorizontally
			};
			DrawingEx.DrawShortBox(spriteBatch, new Rectangle((int)drawPos.X - num, (int)drawPos.Y + num2, num3, 22), _borderColor, _gcm.SpPauseMenu, base.Zoom, new int[3] { 73, 74, 73 }, flipped, shouldTile: true);
			DrawingEx.DrawString(spriteBatch, activeFont, _itemName, drawPos, _textDrawColor, Vector2.Zero, base.Zoom);
			if (IsShowingViewPrompt && _viewButton != null)
			{
				int num4 = num3 + -10 * base.Zoom;
				int num5 = 2 * base.Zoom;
				_viewButton.Draw(spriteBatch, _buttonsSpriteSheet, activeFont, new Vector2(drawPos.X + (float)num4, drawPos.Y + (float)num5), _buttonDrawColor, base.Zoom, -1);
			}
			base.Draw(spriteBatch);
		}
	}

	public void RefreshZoom(Rectangle titleSafeArea)
	{
		_drawPosition = GetDrawPositionFromTitleSafeArea(titleSafeArea);
	}
}
