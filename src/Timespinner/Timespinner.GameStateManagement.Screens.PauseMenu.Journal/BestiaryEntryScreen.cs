using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Localization;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameStateManagement.ScreenManager;
using Timespinner.GameStateManagement.Screens.BaseClasses.Inventory;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;

namespace Timespinner.GameStateManagement.Screens.PauseMenu.Journal;

internal sealed class BestiaryEntryScreen : InventoryMenuScreen
{
	private const string UnknownItemText = "???";

	private const int WideButtonOffsetX = 8;

	private const int ElementIconsStartIndex = 78;

	private const int ElementMarginX = 2;

	private const int ElementIconWidth = 16;

	private const int DistanceBetweenElementIcons = 18;

	private const int ColumnStartY = 36;

	private const int RowOffsetY = 16;

	private const int LeftColumnOffsetX = 24;

	private const int RightColumnMarginX = 8;

	private const int RightColumnMaxWidth = 160;

	private const int ItemListDrawOffsetX = 12;

	private const int ItemListDrawOffsetY = 16;

	private const int ItemBackgroundOffsetX = -6;

	private const int ItemBackgroundOffsetY = 1;

	private const int ItemBackgroundWidth = 162;

	private const int ItemBackgroundHeight = 16;

	private const int ItemBackgroundRowOffsetY = 16;

	private const int ItemStarColumnOffsetX = 112;

	private const int ItemStarOffsetY = 4;

	private const int IndividualStarOffsetX = 8;

	private const int LeftFrameDrawOffsetX = 196;

	private const int TopFrameDrawOffsetY = 40;

	private const int PreviewFrameWidthHeight = 104;

	private const int ElementsDrawOffsetX = 88;

	private const int ElementsDrawOffsetY = 152;

	private const int PreviewRenderOffsetX = 454;

	private const int PreviewRenderOffsetY = 171;

	private const int EnemyRenderHeight = 98;

	private const int EnemyRenderWidth = 100;

	private const int EnemyRenderLeft = 206;

	private const int EnemyRenderTop = 30;

	private const int DefaultCameraOffsetY = 16;

	private const float BumperDrawPositionRatioY = 23f / 192f;

	private const float LeftBumperDrawPositionRatioX = 0.0125f;

	private const float RightBumperDrawPositionRatioX = 0.9375f;

	private const float BackgroundDrawOffsetY = 7f / 64f;

	public static Point RenderSize = new Point(512, 512);

	private static readonly Vector2 LevelScreenCenter = new Vector2(256f, 128f);

	private static readonly Color WeaknessColor = new Color(255, 128, 200);

	private static readonly Color StrongColor = new Color(128, 144, 200);

	private readonly int _playerLuck;

	private readonly string _numberTitle;

	private readonly string _nameTitle;

	private readonly string _hpTitle;

	private readonly string _expTitle;

	private readonly string _itemDropTitle;

	private readonly Color _titleDrawColor;

	private readonly Color _shadowDrawColor;

	private readonly BestiaryMenuEntryCollection _menuCollection;

	private readonly Level _dummyLevel;

	private readonly Rectangle[] _elementFrameSources = new Rectangle[9];

	private readonly List<BestiaryScreenItemDrop> _itemDrops = new List<BestiaryScreenItemDrop>();

	private bool _doesEnemyHaveItems;

	private int _bumperDrawPositionY;

	private int _leftBumperDrawPositionX;

	private int _rightBumperDrawPositionX;

	private int _leftColumnX;

	private int _rightColumnX;

	private int _rightColumnWidth;

	private int _itemStarColumnOffsetX;

	private int _itemStarOffsetY;

	private int _individualStarOffsetX;

	private ScrollableTextBlock _numberData;

	private ScrollableTextBlock _nameData;

	private ScrollableTextBlock _hpData;

	private ScrollableTextBlock _expData;

	private Vector2 _numberTitleDrawPosition;

	private Vector2 _nameTitleDrawPosition;

	private Vector2 _hpTitleDrawPosition;

	private Vector2 _expTitleDrawPosition;

	private Vector2 _itemTitleDrawPosition;

	private Vector2 _itemListDrawPosition;

	private Vector2 _numberDataDrawPosition;

	private Vector2 _nameDataDrawPosition;

	private Vector2 _hpDataDrawPosition;

	private Vector2 _expDataDrawPosition;

	private Vector2 _elementsDrawPosition;

	private Rectangle _itemStarFrameSource;

	private Rectangle _backgroundDrawRectangle;

	private Rectangle _enemyPreviewBackgroundDrawRectangle;

	private Rectangle _item1BackgroundDrawRectangle;

	private Rectangle _item2BackgroundDrawRectangle;

	private EElementalWeaknessState[] _weaknesses;

	private BestiaryMenuEntry _menuEntry;

	private Animate _currentModel;

	private UIButton _leftBumperButton;

	private UIButton _rightBumperButton;

	public BestiaryEntryScreen(string title, GameSave inSave, GCM gcm, BestiaryMenuEntry entry, BestiaryMenuEntryCollection bestiaryMenuCollection, Level dummyLevel, Action fullExitAction)
		: base(title, inSave, gcm, fullExitAction)
	{
		_doesUseCursor = false;
		_menuEntry = entry;
		_menuCollection = bestiaryMenuCollection;
		_dummyLevel = dummyLevel;
		_playerLuck = inSave.CharacterStats.Luck;
		_numberTitle = Loc.Get("StatNumber");
		_nameTitle = Loc.Get("StatName");
		_hpTitle = Loc.Get("StatHP");
		_expTitle = Loc.Get("StatEXP");
		_itemDropTitle = Loc.Get("StatItemDrop");
		_titleDrawColor = MenuEntry.UnselectedColor;
		_shadowDrawColor = MenuDescription.DescriptionShadowColor;
	}

	public override void LoadContent()
	{
		ControllerMapping menuControllerMapping = base.ScreenManager.MenuControllerMapping;
		_leftBumperButton = menuControllerMapping.CreateUIButtonFromDestination(ButtonMapping.EDestinationType.PageLeft);
		_rightBumperButton = menuControllerMapping.CreateUIButtonFromDestination(ButtonMapping.EDestinationType.PageRight);
		base.LoadContent();
	}

	private void RefreshSelectedEnemy()
	{
		BestiaryEntrySpecification bestiaryEntry = _menuEntry.BestiaryEntry;
		_numberData = new ScrollableTextBlock(base.Font, _rightColumnWidth, _numberDataDrawPosition, isTextCentered: false);
		_numberData.SetText(bestiaryEntry.Index.ToString("D2"));
		_nameData = new ScrollableTextBlock(base.Font, _rightColumnWidth, _nameDataDrawPosition, isTextCentered: false);
		_nameData.SetText(bestiaryEntry.VisibleName);
		_hpData = new ScrollableTextBlock(base.Font, _rightColumnWidth, _hpDataDrawPosition, isTextCentered: false);
		_hpData.SetText(bestiaryEntry.HP.ToString(CultureInfo.InvariantCulture));
		_expData = new ScrollableTextBlock(base.Font, _rightColumnWidth, _expDataDrawPosition, isTextCentered: false);
		_expData.SetText(bestiaryEntry.Exp.ToString(CultureInfo.InvariantCulture));
		_weaknesses = bestiaryEntry.ElementalWeaknesses;
		_doesEnemyHaveItems = false;
		_itemDrops.Clear();
		int num = 0;
		foreach (BestiaryItemDropSpecification item2 in bestiaryEntry.LootTable)
		{
			_doesEnemyHaveItems = true;
			Vector2 topLeft = ((num == 0) ? _itemListDrawPosition : new Vector2(_itemListDrawPosition.X, _itemListDrawPosition.Y + (float)(16 * base.Zoom)));
			BestiaryScreenItemDrop item = new BestiaryScreenItemDrop(bestiaryEntry, item2, num, _playerLuck, base.SaveFile, base.Font, topLeft);
			_itemDrops.Add(item);
			num++;
		}
		ChangeDescription(_menuEntry.BestiaryEntry.VisibleDescription, EInventoryItemIcon.None);
		SetModel(bestiaryEntry);
	}

	internal override void RefreshSizes()
	{
		base.RefreshSizes();
		_itemStarFrameSource = base.Sprite.GetFrameSource(142);
		for (int i = 0; i < 9; i++)
		{
			ref Rectangle reference = ref _elementFrameSources[i];
			reference = base.Sprite.GetFrameSource(i + 78);
		}
		_elementsDrawPosition = new Vector2(_screenLeft + 88 * base.Zoom, _screenTop + 152 * base.Zoom);
		int num = 8 * base.Zoom;
		_bumperDrawPositionY = _screenTop + (int)((float)_topSectionHeight * (23f / 192f));
		_leftBumperDrawPositionX = _screenLeft + (int)((float)_screenWidth * 0.0125f);
		_rightBumperDrawPositionX = _screenLeft + (int)((float)_screenWidth * 0.9375f) - (_rightBumperButton.IsWide ? num : 0);
		_leftColumnX = _screenLeft + 24 * base.Zoom;
		_numberTitleDrawPosition = new Vector2(_leftColumnX, _screenTop + 36 * base.Zoom);
		_nameTitleDrawPosition = new Vector2(_leftColumnX, _screenTop + 52 * base.Zoom);
		_hpTitleDrawPosition = new Vector2(_leftColumnX, _screenTop + 68 * base.Zoom);
		_expTitleDrawPosition = new Vector2(_leftColumnX, _screenTop + 84 * base.Zoom);
		_itemTitleDrawPosition = new Vector2(_leftColumnX, _screenTop + 100 * base.Zoom);
		float[] values = new float[4]
		{
			base.Font.MeasureString(_numberTitle).X,
			base.Font.MeasureString(_nameTitle).X,
			base.Font.MeasureString(_hpTitle).X,
			base.Font.MeasureString(_expTitle).X
		};
		int num2 = (int)MathEx.Max(values);
		_rightColumnX = _leftColumnX + (num2 + 8) * base.Zoom;
		_numberDataDrawPosition = new Vector2(_rightColumnX, _numberTitleDrawPosition.Y);
		_nameDataDrawPosition = new Vector2(_rightColumnX, _nameTitleDrawPosition.Y);
		_hpDataDrawPosition = new Vector2(_rightColumnX, _hpTitleDrawPosition.Y);
		_expDataDrawPosition = new Vector2(_rightColumnX, _expTitleDrawPosition.Y);
		_rightColumnWidth = 160 - num2;
		_itemStarColumnOffsetX = 112 * base.Zoom;
		_itemStarOffsetY = 4 * base.Zoom;
		_individualStarOffsetX = 8 * base.Zoom;
		_itemListDrawPosition = new Vector2(_itemTitleDrawPosition.X + (float)(12 * base.Zoom), _itemTitleDrawPosition.Y + (float)(16 * base.Zoom));
		_item1BackgroundDrawRectangle = new Rectangle((int)_itemListDrawPosition.X + -6 * base.Zoom, (int)_itemListDrawPosition.Y + base.Zoom, 162 * base.Zoom, 16 * base.Zoom);
		_item2BackgroundDrawRectangle = new Rectangle(_item1BackgroundDrawRectangle.X, _item1BackgroundDrawRectangle.Y + 16 * base.Zoom, _item1BackgroundDrawRectangle.Width, _item2BackgroundDrawRectangle.Height);
		RefreshSelectedEnemy();
		Vector2 vector = new Vector2(_screenLeft + 196 * base.Zoom, _screenTop + 40 * base.Zoom);
		int num3 = 104 * base.Zoom;
		_enemyPreviewBackgroundDrawRectangle = new Rectangle((int)vector.X, (int)vector.Y, num3, num3);
		int num4 = (int)(7f / 64f * (float)_topSectionHeight);
		_backgroundDrawRectangle = new Rectangle(_screenLeft + 2 * base.Zoom, _screenTop + num4, _screenWidth - 4 * base.Zoom, _topSectionHeight - num4 - 8 * base.Zoom);
	}

	public override void HandleInput(InputState input)
	{
		if (input.IsNewPressPageRight(base.ControllingPlayer))
		{
			ToggleNextEnemy(1);
		}
		else if (input.IsNewPressPageLeft(base.ControllingPlayer))
		{
			ToggleNextEnemy(-1);
		}
		base.HandleInput(input);
	}

	private void ToggleNextEnemy(int indexChange)
	{
		BestiaryMenuEntry bestiaryMenuEntry = _menuCollection.ToggleNextEnemy(indexChange);
		if (bestiaryMenuEntry != null)
		{
			_menuEntry = bestiaryMenuEntry;
			RefreshSelectedEnemy();
			base.ScreenManager.Jukebox.PlayCue(ESFX.MenuMove);
		}
	}

	public void SetModel(BestiaryEntrySpecification newSpec)
	{
		if (_currentModel != null)
		{
			_dummyLevel.RequestRemoveObject(_currentModel);
			_dummyLevel.CleanupRoomVariables();
		}
		ObjectTileSpecification objectTileSpecification = new ObjectTileSpecification();
		objectTileSpecification.Category = EObjectTileCategory.Enemy;
		objectTileSpecification.ObjectID = newSpec.EnemyTypeInt;
		objectTileSpecification.Argument = newSpec.EnemyArgument;
		objectTileSpecification.X = _dummyLevel.RoomSize16.X / 2;
		objectTileSpecification.Y = _dummyLevel.RoomSize16.Y - 2;
		ObjectTileSpecification objectTileSpec = objectTileSpecification;
		_currentModel = _dummyLevel.PlaceEvent(objectTileSpec, shouldInitialize: true);
		_currentModel.Update(0f);
		if (_currentModel is Monster monster)
		{
			monster.InitializeForBestiary();
			monster.Update(0f);
		}
		_currentModel.IsWithinObjectVisibleArea = true;
		int num = ((newSpec.CameraOffsetY == 0) ? 16 : newSpec.CameraOffsetY);
		_dummyLevel.CameraPosition = new Vector2(_currentModel.Position.X + newSpec.CameraOffsetX, _currentModel.Position.Y + num);
	}

	public override void DrawMisc(SpriteBatch spriteBatch, Color drawColor)
	{
		SpriteSheet uIControllerButtons = base.ScreenManager.UIControllerButtons;
		_leftBumperButton.Draw(spriteBatch, uIControllerButtons, base.Font, new Vector2(_leftBumperDrawPositionX, _bumperDrawPositionY), drawColor, base.Zoom, -1);
		_rightBumperButton.Draw(spriteBatch, uIControllerButtons, base.Font, new Vector2(_rightBumperDrawPositionX, _bumperDrawPositionY), drawColor, base.Zoom, -1);
		Texture2D txBlankSquare = base.GCM.TxBlankSquare;
		DrawShadowedText(spriteBatch, _numberTitle, _numberTitleDrawPosition, base.Zoom);
		_numberData.Draw(spriteBatch, _titleDrawColor, _shadowDrawColor, txBlankSquare);
		DrawShadowedText(spriteBatch, _nameTitle, _nameTitleDrawPosition, base.Zoom);
		_nameData.Draw(spriteBatch, _titleDrawColor, _shadowDrawColor, txBlankSquare);
		DrawShadowedText(spriteBatch, _hpTitle, _hpTitleDrawPosition, base.Zoom);
		_hpData.Draw(spriteBatch, _titleDrawColor, _shadowDrawColor, txBlankSquare);
		DrawShadowedText(spriteBatch, _expTitle, _expTitleDrawPosition, base.Zoom);
		_expData.Draw(spriteBatch, _titleDrawColor, _shadowDrawColor, txBlankSquare);
		if (_currentModel != null)
		{
			DrawModelToScreen(spriteBatch, drawColor);
		}
		DrawingEx.DrawIrregularBox(flipped: new SpriteEffects[9]
		{
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.FlipHorizontally,
			SpriteEffects.None,
			SpriteEffects.FlipHorizontally
		}, spriteBatch: spriteBatch, backgroundRectangle: _enemyPreviewBackgroundDrawRectangle, color: drawColor, sprite: base.Sprite, zoom: base.Zoom, frames: new int[9] { 19, 20, 21, 25, -1, 26, 24, 23, 22 }, shouldTile: true);
		DrawShadowedText(spriteBatch, _itemDropTitle, _itemTitleDrawPosition, base.Zoom);
		if (_doesEnemyHaveItems)
		{
			Vector2 position = _itemListDrawPosition;
			foreach (BestiaryScreenItemDrop itemDrop in _itemDrops)
			{
				if (itemDrop.IsKnown)
				{
					itemDrop.NameTextBlock.Draw(spriteBatch, _titleDrawColor, _shadowDrawColor, txBlankSquare);
				}
				else
				{
					DrawShadowedText(spriteBatch, "???", position, base.Zoom);
				}
				Vector2 position2 = new Vector2(position.X + (float)_itemStarColumnOffsetX, position.Y + (float)_itemStarOffsetY);
				int starCount = itemDrop.StarCount;
				for (int i = 0; i < starCount; i++)
				{
					spriteBatch.Draw(base.Sprite.Texture, position2, _itemStarFrameSource, drawColor, 0f, Vector2.Zero, base.Zoom, SpriteEffects.None, 0f);
					position2 = new Vector2(position2.X + (float)_individualStarOffsetX, position2.Y);
				}
				position = new Vector2(position.X, position.Y + (float)(16 * base.Zoom));
			}
		}
		Vector2 position3 = _elementsDrawPosition;
		for (int j = 0; j < 8; j++)
		{
			EElementalWeaknessState eElementalWeaknessState = _weaknesses[j];
			bool flag = eElementalWeaknessState == EElementalWeaknessState.None;
			bool flag2 = false;
			if (!flag)
			{
				flag2 = eElementalWeaknessState == EElementalWeaknessState.Weak;
			}
			Rectangle value = (flag ? _elementFrameSources[8] : _elementFrameSources[j]);
			Color color = Color.White;
			if (!flag)
			{
				color = (flag2 ? WeaknessColor : StrongColor);
			}
			spriteBatch.Draw(base.Sprite.Texture, position3, value, color, 0f, Vector2.Zero, base.Zoom, SpriteEffects.None, 0f);
			position3 = new Vector2(position3.X + (float)(18 * base.Zoom), position3.Y);
		}
		base.DrawMisc(spriteBatch, drawColor);
	}

	private void DrawShadowedText(SpriteBatch spriteBatch, string text, Vector2 position, int zoom)
	{
		DrawingEx.DrawString(spriteBatch, base.Font, text, new Vector2(position.X, position.Y + (float)zoom), _shadowDrawColor, Vector2.Zero, base.Zoom);
		DrawingEx.DrawString(spriteBatch, base.Font, text, position, _titleDrawColor, Vector2.Zero, base.Zoom);
	}

	private void DrawModelToScreen(SpriteBatch spriteBatch, Color drawColor)
	{
		spriteBatch.Draw(position: new Vector2(_screenLeft + 454 * base.Zoom, _screenTop + 171 * base.Zoom), sourceRectangle: new Rectangle(206, 30, 100, 98), texture: base.GCM.LevelRenderTarget, color: drawColor, rotation: 0f, origin: LevelScreenCenter, scale: base.Zoom, effects: SpriteEffects.None, layerDepth: 0f);
	}

	private void DrawModelToRenderTarget(SpriteBatch spriteBatch)
	{
		spriteBatch.End();
		base.ScreenManager.GraphicsDevice.SetRenderTarget(base.GCM.LevelRenderTarget);
		base.ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0f, 0);
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
		_currentModel.Draw(spriteBatch);
		spriteBatch.End();
		base.ScreenManager.GraphicsDevice.SetRenderTarget(null);
		base.ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 0f, 0);
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
	}

	public override void DrawFrames(SpriteBatch spriteBatch, Color drawColor)
	{
		if (_currentModel != null)
		{
			DrawModelToRenderTarget(spriteBatch);
		}
		SpriteEffects[] array = new SpriteEffects[9]
		{
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.FlipHorizontally,
			SpriteEffects.None,
			SpriteEffects.None,
			SpriteEffects.FlipHorizontally,
			SpriteEffects.FlipVertically,
			SpriteEffects.FlipVertically,
			SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically
		};
		DrawingEx.DrawIrregularBox(spriteBatch, _backgroundDrawRectangle, drawColor, base.Sprite, base.Zoom, new int[9] { 41, 42, 41, 43, 44, 43, 41, 42, 41 }, array, shouldTile: true);
		DrawingEx.DrawIrregularBox(spriteBatch, _enemyPreviewBackgroundDrawRectangle, drawColor, base.Sprite, base.Zoom, new int[9] { 37, 38, 37, 39, 40, 39, 37, 38, 37 }, array, shouldTile: true);
		int[] frames = new int[3] { 143, 144, 143 };
		DrawingEx.DrawShortBox(spriteBatch, _item1BackgroundDrawRectangle, drawColor, base.Sprite, base.Zoom, frames, array, shouldTile: true);
		array[1] = SpriteEffects.FlipHorizontally;
		DrawingEx.DrawShortBox(spriteBatch, _item2BackgroundDrawRectangle, drawColor, base.Sprite, base.Zoom, frames, array, shouldTile: true);
		base.DrawFrames(spriteBatch, drawColor);
	}
}
