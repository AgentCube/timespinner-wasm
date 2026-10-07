using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameStateManagement.ScreenManager;

namespace Timespinner.GameStateManagement.Screens.MainMenu;

internal class LocSelectionScreen : GameScreen
{
	private const int FlagIndexStart = 0;

	private const int FrameIndexStart = 8;

	private const int ArrowIndexStart = 10;

	private const int FrameWidth = 76;

	private const int FrameHeight = 52;

	private const int HalfFrameHeight = 26;

	private const int FlagOffsetX = 6;

	private const int FlagOffsetY = 4;

	private const int FrameOffsetX = -38;

	private const int FrameOffsetY = 8;

	private const int LeftArrowOffsetX = -17;

	private const int RightArrowOffsetX = 3;

	private const int ArrowOffsetY = 18;

	private const int ArrowOscillationRadius = 3;

	private const int ArrowOscillationFrequency = 5;

	private const int TitleDrawOffsetY = -16;

	private const int DescriptDrawOffsetY = 24;

	private const int AcceptButtonDrawOffsetY = 16;

	private const float TimeoutBetweenLanguageChanges = 0.1f;

	private const string LocTitleKey = "LocMenuTitle";

	private const string LocDescriptionKey = "LocMenuChangeLocale";

	private readonly bool _isFirstTimeShowing;

	private readonly int _startingLocaleInt;

	private readonly UIButton _acceptButton;

	private readonly GameConfigSave _configSave;

	private readonly GCM _gcm;

	private readonly Queue<ELanguageLocale> _loadingQueue = new Queue<ELanguageLocale>();

	private readonly List<ELanguageLocale> _availableLocales = new List<ELanguageLocale>();

	private readonly Dictionary<ELanguageLocale, StringLibrary> _loadedLanguages = new Dictionary<ELanguageLocale, StringLibrary>();

	private readonly Task _loadTask;

	private bool _hasStartedExit;

	private bool _isTryingToExit;

	private ELanguageLocale _selectedLocale;

	private int _selectedIndex;

	private int _zoom;

	private float _lastTransitionAlpha;

	private float _arrowTimer;

	private float _languageChangeTimeout;

	private Vector2 _flagDrawPosition;

	private Vector2 _topFrameDrawPosition;

	private Vector2 _bottomFrameDrawPosition;

	private Vector2 _baseLeftArrowDrawPosition;

	private Vector2 _baseRightArrowDrawPosition;

	private Vector2 _leftArrowDrawPosition;

	private Vector2 _rightArrowDrawPosition;

	private Vector2 _baseTitleDrawPosition;

	private Vector2 _baseDescriptionDrawPosition;

	private Vector2 _titleDrawPosition;

	private Vector2 _descriptionDrawPosition;

	private Vector2 _acceptDrawPosition;

	private Rectangle _flagFrameSource;

	private Rectangle _topFrameSource;

	private Rectangle _bottomFrameSource;

	private Rectangle _arrowFrameSource;

	private Color _drawColor = Color.Transparent;

	private string _selectedTitle;

	private string _selectedDescription;

	private StringLibrary _selectedLibrary;

	private SpriteSheet _flagSprite;

	private SpriteSheet _buttonSprite;

	private SpriteFont _activeFont;

	private SpriteFont _latinFont;

	private SpriteFont _jpFont;

	private SpriteFont _cnFont;

	private ContentManager _content;

	public LocSelectionScreen(GameConfigSave configSave, GCM gcm, bool isFirstTimeShowing)
	{
		_configSave = configSave;
		_gcm = gcm;
		_isFirstTimeShowing = isFirstTimeShowing;
		_acceptButton = configSave.MenuControllerMapping.CreateUIButtonFromDestination(ButtonMapping.EDestinationType.Confirm);
		_startingLocaleInt = _configSave.LocaleType;
		_availableLocales.Add(ELanguageLocale.EN);
		_availableLocales.Add(ELanguageLocale.BP);
		_availableLocales.Add(ELanguageLocale.CN);
		_availableLocales.Add(ELanguageLocale.DE);
		_availableLocales.Add(ELanguageLocale.ES);
		_availableLocales.Add(ELanguageLocale.FR);
		_availableLocales.Add(ELanguageLocale.JP);
		_availableLocales.Add(ELanguageLocale.RU);
		int num = 0;
		foreach (ELanguageLocale availableLocale in _availableLocales)
		{
			if (_startingLocaleInt == (int)availableLocale)
			{
				_selectedIndex = num;
				break;
			}
			num++;
		}
		base.TransitionOnTime = TimeSpan.FromSeconds(0.25);
		base.TransitionOffTime = TimeSpan.FromSeconds(0.25);
		int count = _availableLocales.Count;
		for (int i = 0; i < count; i++)
		{
			int num2 = _selectedIndex + i;
			if (num2 < count)
			{
				_loadingQueue.Enqueue(_availableLocales[num2]);
			}
			int num3 = _selectedIndex - (i + 1);
			if (num3 >= 0 && num3 < count)
			{
				_loadingQueue.Enqueue(_availableLocales[num3]);
			}
		}
		if (OperatingSystem.IsBrowser())
		{
			_selectedLocale = _availableLocales[_selectedIndex];
			_loadedLanguages[_selectedLocale] = new StringLibrary(_selectedLocale);
		}
		else
		{
			_loadTask = new Task(LoadAllLanguages);
			_loadTask.Start();
		}
	}

	public override void LoadContent()
	{
		base.LoadContent();
		if (_content == null)
		{
			_content = new ContentManager(base.ScreenManager.Game.Services, "Content");
		}
		_buttonSprite = base.ScreenManager.GCM.SpUIButtons;
		_latinFont = _content.Load<SpriteFont>("Fonts/LatinFont");
		_jpFont = _content.Load<SpriteFont>("Fonts/JPFont");
		_cnFont = _content.Load<SpriteFont>("Fonts/CNFont");
		_flagSprite = _gcm.GetTextureAtlas("Overlays/Menu/LocFlags", _content);
		_topFrameSource = _flagSprite.GetFrameSource(8);
		_bottomFrameSource = _flagSprite.GetFrameSource(9);
		_arrowFrameSource = _flagSprite.GetFrameSource(10);
		RefreshSizes();
	}

	private void RefreshSizes()
	{
		_zoom = Constants.InGameZoom;
		Point center = base.ScreenManager.TitleSafeArea.Center;
		_topFrameDrawPosition = new Vector2(center.X + -38 * _zoom, center.Y + 8 * _zoom);
		_bottomFrameDrawPosition = new Vector2(_topFrameDrawPosition.X, _topFrameDrawPosition.Y + (float)(26 * _zoom));
		_flagDrawPosition = new Vector2(_topFrameDrawPosition.X + (float)(6 * _zoom), _topFrameDrawPosition.Y + (float)(4 * _zoom));
		_baseLeftArrowDrawPosition = new Vector2(_topFrameDrawPosition.X + (float)(-17 * _zoom), _topFrameDrawPosition.Y + (float)(18 * _zoom));
		_baseRightArrowDrawPosition = new Vector2(_topFrameDrawPosition.X + (float)(76 * _zoom) + (float)(3 * _zoom), _baseLeftArrowDrawPosition.Y);
		_leftArrowDrawPosition = _baseLeftArrowDrawPosition;
		_rightArrowDrawPosition = _baseRightArrowDrawPosition;
		_baseTitleDrawPosition = new Vector2(center.X, _topFrameDrawPosition.Y + (float)(-16 * _zoom));
		_baseDescriptionDrawPosition = new Vector2(center.X, _bottomFrameDrawPosition.Y + (float)(24 * _zoom));
		_acceptDrawPosition = new Vector2(center.X + (_acceptButton.IsWide ? (-12) : (-8)) * _zoom, _baseDescriptionDrawPosition.Y + (float)(16 * _zoom));
		RefreshLanguage();
	}

	internal override void OnScreenResize()
	{
		base.OnScreenResize();
		RefreshSizes();
	}

	private void LoadAllLanguages()
	{
		while (_loadingQueue.Count > 0)
		{
			ELanguageLocale eLanguageLocale = _loadingQueue.Dequeue();
			StringLibrary value = new StringLibrary(eLanguageLocale);
			_loadedLanguages[eLanguageLocale] = value;
			if (_isTryingToExit)
			{
				break;
			}
		}
	}

	public override void UnloadContent()
	{
		base.UnloadContent();
		if (_content != null)
		{
			_content.Unload();
		}
	}

	public override void ExitScreen()
	{
		_isTryingToExit = true;
		base.ExitScreen();
		if (!_hasStartedExit)
		{
			base.ScreenManager.AddScreen(new SaveValidationScreen(base.ControllingPlayer), base.ControllingPlayer);
		}
		_hasStartedExit = true;
	}

	private void RefreshLanguage()
	{
		_selectedLocale = _availableLocales[_selectedIndex];
		_flagFrameSource = _flagSprite.GetFrameSource((int)_selectedLocale);
		switch (_selectedLocale)
		{
		case ELanguageLocale.CN:
			_activeFont = _cnFont;
			Loc.IsAsianLocale = true;
			break;
		case ELanguageLocale.JP:
			_activeFont = _jpFont;
			Loc.IsAsianLocale = true;
			break;
		default:
			_activeFont = _latinFont;
			Loc.IsAsianLocale = false;
			break;
		}
		if (!_loadedLanguages.ContainsKey(_selectedLocale))
		{
			_loadedLanguages[_selectedLocale] = new StringLibrary(_selectedLocale);
		}
		RefreshText();
	}

	private void RefreshText()
	{
		_selectedLibrary = _loadedLanguages[_selectedLocale];
		_selectedTitle = _selectedLibrary.Get("LocMenuTitle");
		_selectedDescription = _selectedLibrary.Get("LocMenuChangeLocale");
		Vector2 vector = _activeFont.MeasureString(_selectedTitle);
		Vector2 vector2 = _activeFont.MeasureString(_selectedDescription);
		_titleDrawPosition = new Vector2(_baseTitleDrawPosition.X + (float)((int)((0f - vector.X) * 0.5f) * _zoom), _baseTitleDrawPosition.Y);
		_descriptionDrawPosition = new Vector2(_baseDescriptionDrawPosition.X + (float)((int)((0f - vector2.X) * 0.5f) * _zoom), _baseDescriptionDrawPosition.Y);
	}

	public override void Update(GameTime gameTime, bool doesOtherScreenHasFocus, bool isCoveredByOtherScreen)
	{
		base.Update(gameTime, doesOtherScreenHasFocus, isCoveredByOtherScreen);
		byte transitionAlpha = base.TransitionAlpha;
		if ((float)(int)transitionAlpha != _lastTransitionAlpha)
		{
			_drawColor = new Color(transitionAlpha, transitionAlpha, transitionAlpha, transitionAlpha);
		}
		_lastTransitionAlpha = (int)transitionAlpha;
		float num = (float)gameTime.ElapsedGameTime.TotalSeconds;
		_arrowTimer += num * 5f;
		if (_arrowTimer >= (float)Math.PI * 2f)
		{
			_arrowTimer -= (float)Math.PI * 2f;
		}
		int num2 = (int)(Math.Sin(_arrowTimer) * (double)(3 * _zoom));
		_leftArrowDrawPosition.X = _baseLeftArrowDrawPosition.X - (float)num2;
		_rightArrowDrawPosition.X = _baseRightArrowDrawPosition.X + (float)num2;
		if (_languageChangeTimeout > 0f)
		{
			_languageChangeTimeout -= num;
		}
		if (_selectedLibrary == null && _loadedLanguages.ContainsKey(_selectedLocale))
		{
			RefreshText();
		}
	}

	public override void HandleInput(InputState input)
	{
		if (_hasStartedExit)
		{
			return;
		}
		bool flag = false;
		if (_languageChangeTimeout <= 0f)
		{
			if (input.IsNewPressMenuUp(base.ControllingPlayer) || input.IsNewPressMenuLeft(base.ControllingPlayer))
			{
				ChangeIndex(-1);
				flag = true;
			}
			else if (input.IsNewPressMenuDown(base.ControllingPlayer) || input.IsNewPressMenuRight(base.ControllingPlayer))
			{
				ChangeIndex(1);
				flag = true;
			}
		}
		if (input.IsNewPressConfirm(base.ControllingPlayer, out var pressedPlayer))
		{
			if (_isFirstTimeShowing || _selectedLocale != (ELanguageLocale)_startingLocaleInt)
			{
				base.ScreenManager.SwitchLocale(_selectedLocale);
				_configSave.LocaleType = (int)_selectedLocale;
				_configSave.HasPickedLocale = true;
				base.ScreenManager.SaveFileManager.RequestGameConfigSave();
				base.ScreenManager.SaveFileManager.ReloadAllSaves();
			}
			base.ScreenManager.Jukebox.PlayCue(ESFX.MenuEquip);
			ExitScreen();
		}
		else if (input.IsNewPressCancel(base.ControllingPlayer, out pressedPlayer) && !_isFirstTimeShowing)
		{
			base.ScreenManager.Jukebox.PlayCue(ESFX.MenuCancel);
			ELanguageLocale startingLocaleInt = (ELanguageLocale)_startingLocaleInt;
			Loc.IsAsianLocale = startingLocaleInt == ELanguageLocale.CN || startingLocaleInt == ELanguageLocale.JP;
			ExitScreen();
		}
		if (flag)
		{
			base.ScreenManager.Jukebox.PlayCue(ESFX.MenuMove);
			_languageChangeTimeout = 0.1f;
		}
	}

	private void ChangeIndex(int offset)
	{
		_selectedIndex += offset;
		if (_selectedIndex >= _availableLocales.Count)
		{
			_selectedIndex = 0;
		}
		if (_selectedIndex < 0)
		{
			_selectedIndex = _availableLocales.Count - 1;
		}
		RefreshLanguage();
	}

	public override void Draw(GameTime gameTime)
	{
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		if (base.ScreenState != EScreenState.TransitionOff)
		{
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
			DrawMenu(spriteBatch);
			spriteBatch.End();
		}
	}

	private void DrawMenu(SpriteBatch spriteBatch)
	{
		Texture2D texture = _flagSprite.Texture;
		spriteBatch.Draw(texture, _topFrameDrawPosition, _topFrameSource, _drawColor, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
		spriteBatch.Draw(texture, _bottomFrameDrawPosition, _bottomFrameSource, _drawColor, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
		spriteBatch.Draw(texture, _flagDrawPosition, _flagFrameSource, _drawColor, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
		if (_activeFont != null)
		{
			float num = _lastTransitionAlpha / 255f;
			if (_selectedTitle != null)
			{
				Vector2 drawPos = new Vector2(_titleDrawPosition.X, _titleDrawPosition.Y + (float)_zoom);
				Color color = _drawColor * 0.35f;
				DrawingEx.DrawLargeTextShadow(spriteBatch, _activeFont, _selectedTitle, _titleDrawPosition, _zoom, Vector2.Zero, num);
				DrawingEx.DrawString(spriteBatch, _activeFont, _selectedTitle, drawPos, color, Vector2.Zero, _zoom);
				DrawingEx.DrawString(spriteBatch, _activeFont, _selectedTitle, _titleDrawPosition, _drawColor, Vector2.Zero, _zoom);
			}
			if (_selectedDescription != null)
			{
				Vector2 drawPos2 = new Vector2(_descriptionDrawPosition.X, _descriptionDrawPosition.Y + (float)_zoom);
				Color color2 = MenuEntry.UnselectedColor * num;
				Color color3 = color2 * 0.35f;
				DrawingEx.DrawLargeTextShadow(spriteBatch, _activeFont, _selectedDescription, _descriptionDrawPosition, _zoom, Vector2.Zero, num);
				DrawingEx.DrawString(spriteBatch, _activeFont, _selectedDescription, drawPos2, color3, Vector2.Zero, _zoom);
				DrawingEx.DrawString(spriteBatch, _activeFont, _selectedDescription, _descriptionDrawPosition, color2, Vector2.Zero, _zoom);
			}
		}
		spriteBatch.Draw(texture, _leftArrowDrawPosition, _arrowFrameSource, _drawColor, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
		spriteBatch.Draw(texture, _rightArrowDrawPosition, _arrowFrameSource, _drawColor, 0f, Vector2.Zero, _zoom, SpriteEffects.FlipHorizontally, 0f);
		_acceptButton.Draw(spriteBatch, _buttonSprite, _activeFont, _acceptDrawPosition, _drawColor, _zoom, -1);
	}
}
