using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameStateManagement.ScreenManager;

namespace Timespinner.GameStateManagement.Screens.InGame.Toasts;

internal class RelicOrbGetToast : BaseToastPopup
{
	private const int LatinBaseHeight = 64;

	private const int AsianBaseHeight = 72;

	private const int BaseWidth = 400;

	private const int BaseDescriptionWidth = 360;

	private const int LatinLineMarginY = -3;

	private const int AsianLineMarginY = -1;

	private const int LatinBaseNameOffsetY = -17;

	private const int AsianBaseNameOffsetY = -19;

	private const int LatinBaseDescriptionOffsetY = -3;

	private const int AsianBaseDescriptionOffsetY = -1;

	private const int PromptOffsetY = -18;

	private const int BaseUnderlineWidth = 144;

	private const int BaseUnderlineDrawOffsetY = -5;

	private const int BaseItemOffsetY = 2;

	private const int IsFinishedIconFrameStart = 31;

	private const int IsFinishedIconFrameCount = 2;

	private const float IsFinishedIconAnimationSpeed = 0.2f;

	private const float TimeToFlash = 0.75f;

	private const float TimeToWait = 0.15f;

	private const float TimeToFade = 0.2f;

	private const float TimeToExpand = 0.15f;

	private static readonly Color TextNameDrawColor = new Color(240, 240, 208);

	private static readonly Color TextDescriptionDrawColor = new Color(208, 200, 152);

	private static readonly Color TextShadowDrawColor = new Color(60, 60, 24);

	private readonly int _baseHeight;

	private readonly int _widestDescriptionWidth;

	private readonly int _lineMarginY;

	private readonly int _baseNameOffsetY;

	private readonly int _baseDescriptionOffsetY;

	private readonly int _baseUnderlineDrawOffsetY;

	private readonly Vector2 _filigreeFrameDrawOrigin;

	private readonly Vector2 _iconBackDrawOrigin;

	private readonly Vector2 _itemIconDrawOrigin;

	private readonly Rectangle _filigreeFrameFrameSource;

	private readonly Rectangle _blackFrameSource;

	private readonly Rectangle _frameLineFrameSource;

	private readonly Rectangle _iconBackFrameSource;

	private readonly Rectangle _underlineCapFrameSource;

	private readonly Rectangle _itemIconFrameSource;

	private readonly DialogueLine _itemName;

	private readonly List<DialogueLine> _itemDescriptions;

	private readonly SpriteFont _font;

	private readonly SpriteSheet _buttonsSprite;

	private readonly SpriteSheet _itemIconsSprite;

	private int _height;

	private int _isFinishedIconAnimationIndex;

	private int _zoom;

	private int _lineHeight;

	private int _width;

	private int _finalHeight;

	private int _blackDrawOffsetX;

	private int _nameOffsetX;

	private int _nameOffsetY;

	private int _descriptionOffsetX;

	private int _descriptionOffsetY;

	private int _underlineWidth;

	private int _underlineDrawOffsetY;

	private int _underlineCapOffsetX;

	private float _expandTimer;

	private float _isFinishedIconTimer;

	private float _isFinishedIcoColorMultiplier;

	private float _textColorMultiplier;

	private Vector2 _drawPosition;

	private Vector2 _promptDrawPosition;

	private Color _isFinishedIconDrawColor = Color.Transparent;

	public RelicOrbGetToast(SpriteSheet sprite, GCM gcm, ScriptAction script, ControllerMapping controllerMapping)
		: base(sprite, doesFreezeGameplay: true, gcm, 0f, 0.75f, 0.15f, 0.2f)
	{
		base.DoesWaitForInputToFinish = true;
		_zoom = Constants.InGameZoom;
		bool isAsianLocale = Loc.IsAsianLocale;
		_filigreeFrameFrameSource = base.Sprite.GetFrameSource(26);
		_blackFrameSource = base.Sprite.GetFrameSource(27);
		_frameLineFrameSource = base.Sprite.GetFrameSource(29);
		_iconBackFrameSource = base.Sprite.GetFrameSource(30);
		_underlineCapFrameSource = base.Sprite.GetFrameSource(28);
		_filigreeFrameDrawOrigin = new Vector2(0f, (int)((float)_filigreeFrameFrameSource.Height * 0.5f));
		_iconBackDrawOrigin = new Vector2((float)_iconBackFrameSource.Width * 0.5f, (int)((float)_iconBackFrameSource.Height * 0.5f));
		_font = gcm.ActiveFont;
		_buttonsSprite = gcm.SpUIButtons;
		_itemIconsSprite = gcm.SpMenuIcons;
		string text = null;
		EInventoryItemIcon iconFromItem;
		string line;
		string text2;
		if (script.ItemToGiveType == EInventoryCategoryType.Relic)
		{
			EInventoryRelicType itemToGive = (EInventoryRelicType)script.ItemToGive;
			iconFromItem = InventoryItem.GetIconFromItem(itemToGive);
			line = InventoryItem.NameFromType(itemToGive);
			text2 = InventoryItem.DescriptionFromType(itemToGive);
			if (itemToGive == EInventoryRelicType.TimespinnerWheel)
			{
				text = Loc.Get("Tutorial_StopTime");
			}
		}
		else if (script.ItemToGiveType == EInventoryCategoryType.Familiar)
		{
			EInventoryFamiliarType itemToGive2 = (EInventoryFamiliarType)script.ItemToGive;
			iconFromItem = InventoryItem.GetIconFromItem(itemToGive2);
			line = Loc.Get("GetItem_Familiar") + InventoryItem.NameFromType(itemToGive2);
			text2 = InventoryItem.DescriptionFromType(itemToGive2);
		}
		else if (script.ItemToGiveType == EInventoryCategoryType.Equipment)
		{
			EInventoryEquipmentType itemToGive3 = (EInventoryEquipmentType)script.ItemToGive;
			iconFromItem = InventoryItem.GetIconFromItem(itemToGive3);
			line = Loc.Get("GetItem_Equipment") + InventoryItem.NameFromType(itemToGive3);
			text2 = InventoryItem.DescriptionFromType(itemToGive3);
		}
		else
		{
			EInventoryOrbType itemToGive4 = (EInventoryOrbType)script.ItemToGive;
			EOrbSlot orbSlot = script.OrbSlot;
			iconFromItem = InventoryItem.GetIconFromItem(itemToGive4, orbSlot);
			line = InventoryItem.NameFromType(itemToGive4, orbSlot);
			text2 = InventoryItem.DescriptionFromType(itemToGive4, orbSlot);
			if (itemToGive4 == EInventoryOrbType.Blade)
			{
				switch (orbSlot)
				{
				case EOrbSlot.Melee:
					text = Loc.Get("Tutorial_MeleeOrb");
					break;
				case EOrbSlot.Spell:
					text = Loc.Get("Tutorial_SpellOrb");
					break;
				case EOrbSlot.Passive:
					text = Loc.Get("Tutorial_PassiveOrb");
					break;
				}
			}
		}
		if (text != null)
		{
			text2 = $"{text2} {text}";
		}
		_itemName = new DialogueLine(line, _font, _buttonsSprite, _zoom, controllerMapping);
		_itemDescriptions = DialogueLine.SplitMessageIntoLines(text2, 360 * _zoom, _font, _buttonsSprite, _zoom, controllerMapping);
		_itemIconFrameSource = _itemIconsSprite.GetFrameSource((int)(iconFromItem - 1));
		_itemIconDrawOrigin = new Vector2((float)_itemIconFrameSource.Width * 0.5f, (int)((float)_itemIconFrameSource.Height * 0.5f));
		_baseHeight = (isAsianLocale ? 72 : 64);
		int num = 0;
		if (_itemDescriptions.Count > 2)
		{
			num = _font.LineSpacing * (_itemDescriptions.Count - 2);
			_baseHeight += num;
		}
		int num2 = num / 2;
		_lineMarginY = (isAsianLocale ? (-1) : (-3));
		_baseNameOffsetY = (isAsianLocale ? (-19) : (-17)) - num2;
		_baseDescriptionOffsetY = (isAsianLocale ? (-1) : (-3)) - num2;
		_baseUnderlineDrawOffsetY = -5 - num2;
		_widestDescriptionWidth = 0;
		if (_itemDescriptions.Count > 0)
		{
			foreach (DialogueLine itemDescription in _itemDescriptions)
			{
				if (itemDescription.Width > _widestDescriptionWidth)
				{
					_widestDescriptionWidth = itemDescription.Width;
				}
			}
		}
		base.IsOverlayScreen = false;
		base.IsPopupScreen = true;
	}

	public override void LoadContent()
	{
		base.LoadContent();
		RefreshSizes();
		base.ScreenManager.Jukebox.PlayCue(ESFX.ItemGetOrb);
	}

	internal override void OnScreenResize()
	{
		base.OnScreenResize();
		RefreshSizes();
	}

	private void RefreshSizes()
	{
		_zoom = Constants.InGameZoom;
		Rectangle rectangle = Rectangle.Empty;
		if (base.ScreenManager != null)
		{
			rectangle = base.ScreenManager.TitleSafeArea;
		}
		Vector2 drawPosition = new Vector2(rectangle.Center.X / _zoom, rectangle.Center.Y / _zoom);
		_drawPosition = drawPosition;
		_finalHeight = _baseHeight * _zoom;
		_width = 400 * _zoom;
		_blackDrawOffsetX = _width / 2;
		_promptDrawPosition = new Vector2(rectangle.Right - 16 * _zoom, (_drawPosition.Y + (float)(_baseHeight / 2) + -18f) * (float)_zoom);
		_nameOffsetX = -(_itemName.Width / 2) * _zoom;
		_nameOffsetY = _baseNameOffsetY * _zoom;
		_descriptionOffsetX = -_widestDescriptionWidth / 2 * _zoom;
		_descriptionOffsetY = _baseDescriptionOffsetY * _zoom;
		_lineHeight = _zoom * (_font.LineSpacing + _lineMarginY);
		_underlineWidth = 144 * _zoom;
		_underlineDrawOffsetY = _baseUnderlineDrawOffsetY * _zoom;
		_underlineCapOffsetX = _underlineCapFrameSource.Width * _zoom;
	}

	public override void HandleInput(InputState input)
	{
		if (base.IsFinishedAndIsWaitingToClose && !base.HasReceivedInputToClose && input.IsNewPressFinished(base.ControllingPlayer))
		{
			base.HasReceivedInputToClose = true;
		}
		base.HandleInput(input);
	}

	public override void Update(GameTime gameTime, bool doesOtherScreenHasFocus, bool isCoveredByOtherScreen)
	{
		if (!doesOtherScreenHasFocus)
		{
			float num = (float)gameTime.ElapsedGameTime.TotalSeconds;
			if (_expandTimer < 0.15f)
			{
				_expandTimer += num;
				if (_expandTimer < 0.15f)
				{
					float num2 = _expandTimer / 0.15f;
					double num3 = 1.0 - Math.Cos(num2 * ((float)Math.PI / 2f));
					_height = (int)(num3 * (double)_finalHeight);
					_textColorMultiplier = (float)num3;
				}
				else
				{
					_height = _finalHeight;
					_textColorMultiplier = 1f;
				}
			}
			else
			{
				_height = _finalHeight;
			}
			if (base.IsFinishedAndIsWaitingToClose && !base.HasReceivedInputToClose)
			{
				_isFinishedIconTimer -= num;
				if (_isFinishedIconTimer < 0f)
				{
					_isFinishedIconTimer = 0.2f;
					_isFinishedIconAnimationIndex = (_isFinishedIconAnimationIndex + 1) % 2;
				}
				if (_isFinishedIcoColorMultiplier < 1f)
				{
					_isFinishedIcoColorMultiplier += num * 10f;
					if (_isFinishedIcoColorMultiplier > 1f)
					{
						_isFinishedIcoColorMultiplier = 1f;
					}
					_isFinishedIconDrawColor = Color.White * _isFinishedIcoColorMultiplier;
				}
			}
		}
		base.Update(gameTime, doesOtherScreenHasFocus, isCoveredByOtherScreen);
	}

	internal override void DrawToastContent(SpriteBatch spriteBatch, Color drawColor, float zoom)
	{
		Vector2 vector = _drawPosition * _zoom;
		Rectangle destinationRectangle = new Rectangle((int)vector.X - _blackDrawOffsetX, (int)vector.Y - _height / 2, _width, _height);
		spriteBatch.Draw(base.Sprite.Texture, destinationRectangle, _blackFrameSource, drawColor, 0f, Vector2.Zero, SpriteEffects.None, 0f);
		Rectangle destinationRectangle2 = new Rectangle(destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width, _zoom);
		spriteBatch.Draw(base.Sprite.Texture, destinationRectangle2, _frameLineFrameSource, drawColor, 0f, Vector2.Zero, SpriteEffects.None, 0f);
		destinationRectangle2.Y = destinationRectangle.Bottom;
		spriteBatch.Draw(base.Sprite.Texture, destinationRectangle2, _frameLineFrameSource, drawColor, 0f, Vector2.Zero, SpriteEffects.None, 0f);
		Vector2 position = new Vector2(vector.X, (float)destinationRectangle.Top + 2f * zoom);
		if (_textColorMultiplier > 0.5f)
		{
			Color drawColor2 = TextNameDrawColor * base.DrawColorPercentage * _textColorMultiplier;
			Color shadowDrawColor = TextShadowDrawColor * base.DrawColorPercentage * _textColorMultiplier;
			Vector2 drawPosition = new Vector2(vector.X + (float)_nameOffsetX, vector.Y + (float)_nameOffsetY);
			Rectangle destinationRectangle3 = new Rectangle((int)vector.X - _underlineWidth / 2 + _underlineCapOffsetX, (int)vector.Y + _underlineDrawOffsetY, _underlineWidth - _underlineCapOffsetX * 2, _zoom);
			spriteBatch.Draw(base.Sprite.Texture, destinationRectangle3, _frameLineFrameSource, drawColor, 0f, Vector2.Zero, SpriteEffects.None, 0f);
			Vector2 position2 = new Vector2(destinationRectangle3.Left - _underlineCapOffsetX, destinationRectangle3.Top);
			spriteBatch.Draw(base.Sprite.Texture, position2, _underlineCapFrameSource, drawColor, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
			spriteBatch.Draw(position: new Vector2(destinationRectangle3.Right, position2.Y), texture: base.Sprite.Texture, sourceRectangle: _underlineCapFrameSource, color: drawColor, rotation: 0f, origin: Vector2.Zero, scale: zoom, effects: SpriteEffects.FlipHorizontally, layerDepth: 0f);
			_itemName.Draw(spriteBatch, drawPosition, drawColor2, shadowDrawColor, _zoom, base.DrawColorPercentage);
			Color drawColor3 = TextDescriptionDrawColor * base.DrawColorPercentage * _textColorMultiplier;
			Vector2 drawPosition2 = new Vector2(vector.X + (float)_descriptionOffsetX, vector.Y + (float)_descriptionOffsetY);
			foreach (DialogueLine itemDescription in _itemDescriptions)
			{
				itemDescription.Draw(spriteBatch, drawPosition2, drawColor3, shadowDrawColor, _zoom, base.DrawColorPercentage);
				drawPosition2 = new Vector2(drawPosition2.X, drawPosition2.Y + (float)_lineHeight);
			}
		}
		spriteBatch.Draw(base.Sprite.Texture, position, _iconBackFrameSource, drawColor, 0f, _iconBackDrawOrigin, zoom, SpriteEffects.None, 0f);
		spriteBatch.Draw(_itemIconsSprite.Texture, position, _itemIconFrameSource, drawColor, 0f, _itemIconDrawOrigin, zoom, SpriteEffects.None, 0f);
		spriteBatch.Draw(base.Sprite.Texture, position, _filigreeFrameFrameSource, drawColor, 0f, _filigreeFrameDrawOrigin, zoom, SpriteEffects.None, 0f);
		spriteBatch.Draw(position: new Vector2(position.X - (float)_filigreeFrameFrameSource.Width * zoom, position.Y), texture: base.Sprite.Texture, sourceRectangle: _filigreeFrameFrameSource, color: drawColor, rotation: 0f, origin: _filigreeFrameDrawOrigin, scale: zoom, effects: SpriteEffects.FlipHorizontally, layerDepth: 0f);
		if (base.IsFinishedAndIsWaitingToClose)
		{
			Rectangle frameSource = base.Sprite.GetFrameSource(_isFinishedIconAnimationIndex + 31);
			Color color = _isFinishedIconDrawColor * base.DrawColorPercentage;
			spriteBatch.Draw(base.Sprite.Texture, _promptDrawPosition, frameSource, color, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
		}
	}
}
