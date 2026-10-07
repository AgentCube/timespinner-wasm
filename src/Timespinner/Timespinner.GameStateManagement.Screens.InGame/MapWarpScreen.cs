using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.HUD;
using Timespinner.GameStateManagement.ScreenManager;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;
using Timespinner.GameStateManagement.Screens.PauseMenu;

namespace Timespinner.GameStateManagement.Screens.InGame;

internal class MapWarpScreen : MenuScreen
{
	private const int EraTitleBaseDrawPositionOffsetY = 36;

	private const int BumperShadowOffsetRadius = 2;

	private const int WideButtonOffsetX = 8;

	private const float BumperDrawPositionY = 38f;

	private const float LeftBumperDrawOffsetX = -80f;

	private const float RightBumperDrawOffsetX = 64f;

	private const float FadeInOutTime = 0.07f;

	private const float BracketGrowthFrequency = 8f;

	private const float TimeBetweenMovingCursor = 0.1f;

	private const float LevelNameBaseDrawPositionOffsetY = 52f;

	private const float BumperGlowFrequency = 5f;

	private const float BumperGlowAmplitude = 0.35f;

	private const float BumperGlowAmplitudeOffset = 0.65f;

	private const float BumperBaseGlowAmount = 1.5f;

	private readonly EMinimapEraType _startingEra;

	private readonly PlayerIndex _controllingPlayer;

	private readonly int _startingLevelID;

	private readonly int _startingRoomID;

	private readonly MinimapBlock _startingBlock;

	private readonly HudMinimap _minimapHud;

	private readonly MinimapSpecification _minimapSpec;

	private readonly SpriteFont _font;

	private readonly SpriteSheet _minimapSpritesheet;

	private readonly SpriteSheet _buttonSprite;

	private readonly GCM _gcm;

	private readonly List<EMinimapEraType> _availableEras = new List<EMinimapEraType>();

	private readonly List<MinimapBlock> _horizontalBlocksInEra = new List<MinimapBlock>();

	private readonly List<MinimapBlock> _verticalBlocksInEra = new List<MinimapBlock>();

	private readonly Dictionary<int, List<MinimapBlock>> _knownWarpBlocks = new Dictionary<int, List<MinimapBlock>>();

	private readonly Action<LevelChangeRequest> _onWarpSelected;

	private EMinimapEraType _selectedEra;

	private int _selectedHorizontalBlockIndex;

	private int _selectedVerticalBlockIndex;

	private int _zoom;

	private int _leftBumperDrawPositionX;

	private int _rightBumperDrawPositionX;

	private int _bumperDrawPositionY;

	private int _wideButtonOffsetX;

	private float _timeSinceMovingCursor;

	private float _bracketGrowTimer;

	private float _bracketGrowPercentage;

	private float _bumperGlowTimer;

	private float _bumperGlowPercentage = 1f;

	private Point _mapDrawOffset;

	private Point _eraViewOffset;

	private Point _bumperShadowOffset;

	private Vector2 _eraTitleDrawPosition;

	private Vector2 _levelNameDrawPosition;

	private Vector2 _eraTitleBaseDrawPosition;

	private Vector2 _levelNameBaseDrawPosition;

	private string _eraName;

	private string _levelName;

	private MinimapBlock _selectedBlock;

	private UIButton _leftBumperButton;

	private UIButton _rightBumperButton;

	internal MapWarpScreen(GCM gcm, MinimapSpecification minimapSpec, int levelID, int roomID, int controllingPlayerIndex, Action<LevelChangeRequest> onWarpSelected)
		: base("")
	{
		_gcm = gcm;
		_minimapSpec = minimapSpec;
		_minimapSpritesheet = _gcm.SpMiniMap;
		_buttonSprite = _gcm.SpUIButtons;
		_font = _gcm.ActiveFont;
		_zoom = Constants.InGameZoom;
		_onWarpSelected = onWarpSelected;
		_controllingPlayer = XnaEx.IntToPlayerIndex(controllingPlayerIndex);
		_startingLevelID = levelID;
		_startingRoomID = roomID;
		MinimapRoom roomFromLevelAndRoom = _minimapSpec.GetRoomFromLevelAndRoom(_startingLevelID, _startingRoomID);
		_startingBlock = roomFromLevelAndRoom.Blocks[new Point(0, 0)];
		_minimapHud = new HudMinimap(_minimapSpec, _gcm, Point.Zero, isMenuMap: true)
		{
			DoesDrawDebugRooms = false
		};
		_minimapHud.ToggleSize(EMinimapToggleState.Large);
		base.IsPopupScreen = true;
		base.IsOverlayScreen = false;
		base.DoesLeaveIfNotInFocus = false;
		base.TransitionOnTime = TimeSpan.FromSeconds(0.07000000029802322);
		base.TransitionOffTime = base.TransitionOnTime;
		base.DoesOverridePrimaryMenuPosition = true;
		_doesUseBlackGradientBox = false;
		_doesUseCursor = false;
		foreach (MinimapArea area in _minimapSpec.Areas)
		{
			foreach (MinimapRoom room in area.Rooms)
			{
				foreach (MinimapBlock value in room.Blocks.Values)
				{
					if (value.IsVisited && value.IsTransition)
					{
						EMinimapEraType eraFromMinimapColor = MinimapSpecification.GetEraFromMinimapColor(value.RoomColor);
						int key = (int)eraFromMinimapColor;
						if (_knownWarpBlocks.ContainsKey(key))
						{
							_knownWarpBlocks[key].Add(value);
						}
						else
						{
							_knownWarpBlocks[key] = new List<MinimapBlock> { value };
						}
						if (value == _startingBlock)
						{
							_selectedBlock = value;
							_selectedEra = eraFromMinimapColor;
						}
					}
				}
			}
		}
		_startingEra = _selectedEra;
		foreach (int key2 in _knownWarpBlocks.Keys)
		{
			_availableEras.Add((EMinimapEraType)key2);
		}
		ToggleEra(0);
	}

	public override void LoadContent()
	{
		base.LoadContent();
		ControllerMapping menuControllerMapping = base.ScreenManager.MenuControllerMapping;
		_leftBumperButton = menuControllerMapping.CreateUIButtonFromDestination(ButtonMapping.EDestinationType.PageLeft);
		_rightBumperButton = menuControllerMapping.CreateUIButtonFromDestination(ButtonMapping.EDestinationType.PageRight);
	}

	internal override void RefreshSizes()
	{
		_zoom = Constants.InGameZoom;
		int x = (_gcm.DisplaySize.X - 400 * _zoom) / 2;
		int num = (_gcm.DisplaySize.Y - 240 * _zoom) / 2;
		int num2 = (int)((float)(46 * _zoom) * 1.5f);
		_wideButtonOffsetX = 8 * _zoom;
		_mapDrawOffset = new Point(x, num + 46 * _zoom);
		Point newDrawPosition = new Point(_gcm.DisplaySize.X - _mapDrawOffset.X, (int)_gcm.ScreenCenter.Y - num2);
		_minimapHud.Zoom = _zoom;
		_minimapHud.RefreshZoom(newDrawPosition);
		_eraTitleBaseDrawPosition = new Vector2((float)_gcm.DisplaySize.X / 2f, num + 36 * _zoom);
		_levelNameBaseDrawPosition = new Vector2(_eraTitleBaseDrawPosition.X, (float)num + 52f * (float)_zoom);
		_leftBumperDrawPositionX = (int)(_eraTitleBaseDrawPosition.X + -80f * (float)_zoom);
		_rightBumperDrawPositionX = (int)(_eraTitleBaseDrawPosition.X + 64f * (float)_zoom);
		_bumperDrawPositionY = num + (int)(38f * (float)_zoom);
		RefreshEraName();
		RefreshLevelName();
	}

	public override void HandleInput(InputState input)
	{
		if (input.IsNewPressPageRight(base.ControllingPlayer))
		{
			ToggleEra(-1);
		}
		else if (input.IsNewPressPageLeft(base.ControllingPlayer))
		{
			ToggleEra(1);
		}
		if (_timeSinceMovingCursor >= 0.1f)
		{
			if (input.IsNewPressMenuRight(_controllingPlayer))
			{
				MoveRoomCursor(EDirection.East);
			}
			else if (input.IsNewPressMenuLeft(_controllingPlayer))
			{
				MoveRoomCursor(EDirection.West);
			}
			else if (input.IsNewPressMenuUp(_controllingPlayer))
			{
				MoveRoomCursor(EDirection.North);
			}
			else if (input.IsNewPressMenuDown(_controllingPlayer))
			{
				MoveRoomCursor(EDirection.South);
			}
		}
		if (input.IsNewPressConfirm(base.ControllingPlayer))
		{
			if (_selectedBlock != _startingBlock)
			{
				bool flag = _selectedEra != _startingEra;
				MinimapRoom parentRoom = _selectedBlock.ParentRoom;
				_onWarpSelected(new LevelChangeRequest
				{
					LevelID = parentRoom.ParentArea.LevelID,
					PreviousLevelID = _startingLevelID,
					RoomID = parentRoom.RoomID,
					IsUsingWarp = true,
					IsUsingWhiteFadeOut = true,
					AdditionalBlackScreenTime = 0.25f,
					FadeOutTime = 0.25f,
					FadeInTime = (flag ? 1f : 0.25f)
				});
				base.ScreenManager.Jukebox.PlayCue(ESFX.MenuSelect);
				ExitScreen();
			}
			else
			{
				OnCancel(_controllingPlayer);
			}
		}
		base.HandleInput(input);
	}

	private void ToggleEra(int indexOffset)
	{
		int num = _availableEras.IndexOf(_selectedEra);
		num += indexOffset;
		int count = _availableEras.Count;
		if (num < 0)
		{
			num = count - 1;
		}
		if (num >= count)
		{
			num = 0;
		}
		_selectedEra = _availableEras[num];
		RefreshEraName();
		RefreshBlocksInEra();
		if (indexOffset != 0 && _availableEras.Count > 1)
		{
			Point b = NormalizeRoomPosition(_selectedBlock.ParentRoom.Position);
			_selectedHorizontalBlockIndex = 0;
			_selectedBlock = _horizontalBlocksInEra[_selectedHorizontalBlockIndex];
			int num2 = int.MaxValue;
			int num3 = 0;
			foreach (MinimapBlock item in _horizontalBlocksInEra)
			{
				Point a = NormalizeRoomPosition(item.ParentRoom.Position);
				if (a.X == b.X)
				{
					num2 = 0;
					_selectedHorizontalBlockIndex = num3;
					_selectedBlock = item;
					if (a.Y == b.Y)
					{
						break;
					}
				}
				else if (num2 > 0)
				{
					int num4 = a.DistanceSquared(b);
					if (num4 < num2)
					{
						num2 = num4;
						_selectedBlock = item;
						_selectedHorizontalBlockIndex = num3;
					}
				}
				num3++;
			}
		}
		RefreshSelectedIndices();
		RefreshCursor();
	}

	private void RefreshBlocksInEra()
	{
		int selectedEra = (int)_selectedEra;
		if (_knownWarpBlocks.ContainsKey(selectedEra))
		{
			List<MinimapBlock> list = _knownWarpBlocks[selectedEra];
			list.Sort(delegate(MinimapBlock p1, MinimapBlock p2)
			{
				int x = p1.ParentRoom.Position.X;
				return x.CompareTo(p2.ParentRoom.Position.X);
			});
			_horizontalBlocksInEra.Clear();
			_horizontalBlocksInEra.AddRange(list);
			list.Sort(delegate(MinimapBlock p1, MinimapBlock p2)
			{
				int y = p1.ParentRoom.Position.Y;
				return y.CompareTo(p2.ParentRoom.Position.Y);
			});
			_verticalBlocksInEra.Clear();
			_verticalBlocksInEra.AddRange(list);
		}
	}

	private void RefreshSelectedIndices()
	{
		int num = 0;
		foreach (MinimapBlock item in _horizontalBlocksInEra)
		{
			if (item != _selectedBlock)
			{
				num++;
				continue;
			}
			break;
		}
		_selectedHorizontalBlockIndex = num;
		num = 0;
		foreach (MinimapBlock item2 in _verticalBlocksInEra)
		{
			if (item2 != _selectedBlock)
			{
				num++;
				continue;
			}
			break;
		}
		_selectedVerticalBlockIndex = num;
		RefreshLevelName();
	}

	private static Point NormalizeRoomPosition(Point position)
	{
		Point result = position;
		if (result.Y > 100)
		{
			result = new Point(result.X, result.Y - 100);
		}
		else if (result.Y > 50)
		{
			result = new Point(result.X, result.Y - 50);
		}
		return result;
	}

	private void MoveRoomCursor(EDirection direction)
	{
		_timeSinceMovingCursor = 0f;
		if (_horizontalBlocksInEra == null || _horizontalBlocksInEra.Count <= 0)
		{
			return;
		}
		int num = 0;
		if (direction != EDirection.Center)
		{
			num = ((direction == EDirection.South || direction == EDirection.East) ? 1 : (-1));
		}
		int num2 = ((direction == EDirection.North || direction == EDirection.South) ? _selectedVerticalBlockIndex : _selectedHorizontalBlockIndex);
		int num3 = num2 + num;
		int count = _horizontalBlocksInEra.Count;
		if (num3 < 0)
		{
			num3 = count - 1;
		}
		else if (num3 >= count)
		{
			num3 = 0;
		}
		if (num2 != num3 || direction == EDirection.Center)
		{
			if (direction == EDirection.North || direction == EDirection.South)
			{
				_selectedVerticalBlockIndex = num3;
				_selectedBlock = _verticalBlocksInEra[_selectedVerticalBlockIndex];
			}
			else
			{
				_selectedHorizontalBlockIndex = num3;
				_selectedBlock = _horizontalBlocksInEra[_selectedHorizontalBlockIndex];
			}
			RefreshSelectedIndices();
			OnSelectionChanged();
		}
	}

	private void OnSelectionChanged()
	{
		base.ScreenManager.Jukebox.PlayCue(ESFX.MenuMove);
		_bracketGrowTimer = 0f;
		_bracketGrowPercentage = 0f;
		RefreshCursor();
	}

	private void RefreshCursor()
	{
		_eraViewOffset = MinimapSpecification.GetViewOffsetFromEra(_selectedEra);
	}

	private void RefreshEraName()
	{
		_eraName = MapMenuScreen.EraNameFromEra(_selectedEra);
		_eraTitleDrawPosition = Vector2.Add(_eraTitleBaseDrawPosition, new Vector2((int)((0f - _font.MeasureString(_eraName).X) * (float)_zoom / 2f), 0f));
	}

	private void RefreshLevelName()
	{
		MinimapRoom parentRoom = _selectedBlock.ParentRoom;
		_levelName = Level.GetLevelNameFromID(parentRoom.ParentArea.LevelID, parentRoom.RoomID);
		_levelNameDrawPosition = Vector2.Add(_levelNameBaseDrawPosition, new Vector2((int)((0f - _font.MeasureString(_levelName).X) * (float)_zoom / 2f), 0f));
	}

	public override void Update(GameTime gameTime, bool doesOtherScreenHasFocus, bool isCoveredByOtherScreen)
	{
		float num = (float)gameTime.ElapsedGameTime.TotalSeconds;
		_minimapHud.Update(num, _selectedBlock);
		if (_timeSinceMovingCursor < 0.1f)
		{
			_timeSinceMovingCursor += num;
		}
		_bracketGrowTimer += num * 8f;
		if (_bracketGrowTimer >= (float)Math.PI * 2f)
		{
			_bracketGrowTimer -= (float)Math.PI * 2f;
		}
		_bracketGrowPercentage = (float)((Math.Sin(_bracketGrowTimer) + 1.0) / 3.0) + 1f;
		if (base.TransitionOffPercentage <= 0f)
		{
			_bumperGlowTimer += num * 5f;
			if (_bumperGlowTimer >= (float)Math.PI * 2f)
			{
				_bumperGlowTimer -= (float)Math.PI * 2f;
			}
			_bumperGlowPercentage = (float)(Math.Cos(_bumperGlowTimer) * 0.3499999940395355) + 0.65f;
			_bumperShadowOffset = new Point((int)(Math.Round(Math.Cos(_bumperGlowPercentage * (float)Math.PI) * 2.0) * (double)_zoom), 0);
		}
		else
		{
			_bumperGlowPercentage = 1f;
		}
		base.Update(gameTime, doesOtherScreenHasFocus, isCoveredByOtherScreen);
	}

	public override void Draw(GameTime gameTime)
	{
		base.ScreenManager.FadeBackBufferToBlack((int)((1f - base.TransitionOffPercentage) * 0.5f * 255f));
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
		_minimapHud.Draw(spriteBatch, 1f - base.TransitionOffPercentage);
		if (_horizontalBlocksInEra != null)
		{
			Color color = Color.White * (1f - base.TransitionOffPercentage);
			int num = 4 * _zoom;
			int num2 = (int)((float)num * 1.25f);
			Rectangle frameSource = _minimapSpritesheet.GetFrameSource(11);
			foreach (MinimapBlock item in _horizontalBlocksInEra)
			{
				Vector2 vector = new Vector2(_mapDrawOffset.X + item.ParentRoom.Position.X * num, _mapDrawOffset.Y + _zoom + (item.ParentRoom.Position.Y - _eraViewOffset.Y + 1) * num);
				float num3 = ((item == _selectedBlock) ? ((float)Math.Ceiling((float)num2 * _bracketGrowPercentage)) : ((float)num2));
				spriteBatch.Draw(_minimapSpritesheet.Texture, new Vector2(vector.X - num3, vector.Y - num3), frameSource, color, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
				spriteBatch.Draw(_minimapSpritesheet.Texture, new Vector2(vector.X + num3, vector.Y - num3), frameSource, color, 0f, Vector2.Zero, _zoom, SpriteEffects.FlipHorizontally, 0f);
				spriteBatch.Draw(_minimapSpritesheet.Texture, new Vector2(vector.X - num3, vector.Y + num3), frameSource, color, 0f, Vector2.Zero, _zoom, SpriteEffects.FlipVertically, 0f);
				spriteBatch.Draw(_minimapSpritesheet.Texture, new Vector2(vector.X + num3, vector.Y + num3), frameSource, color, 0f, Vector2.Zero, _zoom, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0f);
			}
		}
		Color color2 = Color.White * (1f - base.TransitionOffPercentage);
		DrawBumpers(spriteBatch, color2);
		DrawingEx.DrawString(spriteBatch, _font, _eraName, _eraTitleDrawPosition, color2, Vector2.Zero, _zoom);
		color2 = MenuEntry.UnselectedColor * (1f - base.TransitionOffPercentage);
		DrawingEx.DrawString(spriteBatch, _font, _levelName, _levelNameDrawPosition, color2, Vector2.Zero, _zoom);
		spriteBatch.End();
		base.Draw(gameTime);
	}

	private void DrawBumpers(SpriteBatch spriteBatch, Color baseDrawColor)
	{
		bool flag = _bumperGlowPercentage < 1f;
		Color drawColor;
		if (flag)
		{
			spriteBatch.End();
			_gcm.EfBrighten.Parameters["shinyAmount"].SetValue(1.5f);
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, _gcm.EfBrighten);
			drawColor = new Color(1f, 1f, 1f, _bumperGlowPercentage);
		}
		else
		{
			drawColor = baseDrawColor;
		}
		DrawBumper(spriteBatch, isLeft: true, drawColor);
		DrawBumper(spriteBatch, isLeft: false, drawColor);
		if (flag)
		{
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
		}
	}

	private void DrawBumper(SpriteBatch spriteBatch, bool isLeft, Color drawColor)
	{
		UIButton uIButton = (isLeft ? _leftBumperButton : _rightBumperButton);
		Vector2 position = new Vector2(isLeft ? (_leftBumperDrawPositionX - (uIButton.IsWide ? _wideButtonOffsetX : 0)) : _rightBumperDrawPositionX, _bumperDrawPositionY);
		if (_bumperGlowPercentage < 1f)
		{
			Color color = drawColor * 0.5f;
			Vector2 position2 = new Vector2(position.X + (float)(isLeft ? _bumperShadowOffset.X : (-_bumperShadowOffset.X)), position.Y + (float)_bumperShadowOffset.Y);
			uIButton.Draw(spriteBatch, _buttonSprite, _font, position2, color, _zoom, 0);
		}
		uIButton.Draw(spriteBatch, _buttonSprite, _font, position, drawColor, _zoom, 0);
	}
}
