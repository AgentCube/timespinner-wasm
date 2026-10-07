using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.Core.Localization;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.HUD;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Events.Cutscene;
using Timespinner.GameObjects.Heroes;
using Timespinner.GameObjects.StatusEffects;
using Timespinner.GameStateManagement.ScreenManager;
using Timespinner.GameStateManagement.Screens.BaseClasses;
using Timespinner.GameStateManagement.Screens.MainMenu;
using Timespinner.GameStateManagement.Screens.PauseMenu;

namespace Timespinner.GameStateManagement.Screens.InGame;

internal sealed class GameplayScreen : GameScreen
{
	private const int SaveDisplayTextOffsetX = -30;

	private const int SaveDisplayTextOffsetY = -26;

	private const int LoadDisplayTextOffsetX = 4;

	private const int LoadDisplayTextOffsetY = -26;

	private const int MiniCounterTicks = 5;

	private const int MaxLoadCount = 17;

	private const float TimeForRoomTransition = 0.1f;

	private const float TimeForHUDTransitionHiding = 0.15f;

	private const string GameSavedKey = "GameSaved";

	private const string SkipCutsceneKey = "Tutorial_SkipCutscene";

	private const int CutsceneSkipPromptDrawOffsetY = -32;

	private const float TimeForSkipCutscenePromptToFade = 0.2f;

	private const float TimeBeforeSkipCutscenePromptFades = 3f;

	private const float TimeBeforeSkipCutscenePromptEndFade = 2.8f;

	internal static readonly Point SmallScreenSize = new Point(400, 240);

	private static readonly Color SaveDisplayTextColor = new Color(248, 232, 232);

	private static readonly Color SaveDisplayShadowColor = new Color(24, 16, 16);

	private static readonly Color BaseSkipCutscenePromptColor = new Color(216, 200, 176);

	private static readonly Color BaseSkipCutscenePromptShadowColor = new Color(24, 16, 16);

	private readonly List<ScreenEffect> _screenEffects = new List<ScreenEffect>();

	private readonly Dictionary<int, IEnumerable<BackgroundSpecification>> _cachedWarpBackgrounds = new Dictionary<int, IEnumerable<BackgroundSpecification>>();

	private bool _isTransitionFadeOutInProgress;

	private bool _isRoomChangeInProgress;

	private float _transitionWatchdogTimer;

	private bool _isUsingWhiteFadeOut;

	private bool _isGeneralHUDHidden;

	private bool _wasGameActive;

	private bool _isSavingNextFrame;

	private bool _isWaitingForSaveResult;

	private bool _isShowingSkipCutscenePrompt;

	private bool _isWaitingToPlayLevelSong;

	private bool _hasUpdatedOnce;

	private bool _doesDrawLoadingTextOnLevelChange;

	private EGameResolutionType _lastGameResolution;

	private int _levelIndex;

	private int _player1Index;

	private float _currentTransitionTimer;

	private float _fadeInOutMax;

	private float _savingDisplayTimer;

	private float _hideHUDTimer;

	private float _saveMessageWidth;

	private float _loadingMessageWidth;

	private float _skipCutscenePromptTimer;

	private string _saveMessage;

	private string _skipCutsceneText;

	private string _loadingMessage;

	private Point _gameScreenSize;

	private Vector2 _gameScreenCenter;

	private Vector2 _levelScreenCenter;

	private Vector2 _skipCutsceneTextDrawLocation;

	private Vector2 _skipCutsceneTextOrigin;

	private Rectangle _smallScreenRect;

	private Rectangle _titleSafeArea;

	private Rectangle _screenEffectRect;

	private Color _skipCutsceneTextColor;

	private Color _skipCutsceneTextShadowColor;

	private GCM _gcm;

	private Level _level;

	private MinimapSpecification _minimapSpecification;

	private HudMinimap _minimapHud;

	private HudLunais _lunaisClockBar;

	private ButtonPromptScreen _currentButtonPromptScreen;

	private HudItemGetBanner _itemGetBanner;

	private HudEnemyNameBanner _enemyNameBanner;

	private DialogueBox _currentDialogue;

	private bool _doesDrawFPS;

	private bool _wasHideUIDown;

	private int _frameRate;

	private int _frameCounter;

	private int _fpsTicks;

	private int _loadCounter;

	private int _miniLoadCounter;

	private float _elapsedGameSeconds;

	private float _camZoom;

	private float _effectDebug;

	private float _gameTimeMultiplier = 1f;

	private long _longFrameCounter;

	private TimeSpan _elapsedTime = TimeSpan.Zero;

	private Point _topLeftTitleSafe;

	private Point _topRightTitleSafe;

	private LevelChangeRequest _levelChangeRequest;

	private InputState _lastInputState;

	internal static Point SmallScreenSizeZoomed => new Point(SmallScreenSize.X * Constants.InGameZoom, SmallScreenSize.Y * Constants.InGameZoom);

	private new bool IsActive => base.IsActive;

	public bool IsPlayerInputBlocked { get; private set; }

	public bool SecondPlayerActive { get; set; }

	public bool IsHidingHUD { get; set; }

	public int ElapsedGameSeconds => (int)_elapsedGameSeconds;

	public GameSave SaveFile { get; private set; }

	public GameConfigSave GameConfigSave { get; private set; }

	public GameplayScreen(GameSave inSave, GameConfigSave configSave)
	{
		_camZoom = Constants.InGameZoom;
		SaveFile = inSave;
		GameConfigSave = configSave;
		Constants.IsAnySpeedrunActive = SaveFile.IsAnySpeedrunActive;
		_elapsedGameSeconds = SaveFile.ElapsedGameSeconds;
		base.TransitionOnTime = TimeSpan.FromSeconds(1.0);
		base.TransitionOffTime = TimeSpan.FromSeconds(0.5);
		base.IsVibrationEnabled = true;
	}

	public override void LoadContent()
	{
		_gcm = base.ScreenManager.GCM;
		base.ScreenManager.Game.ResetElapsedTime();
	}

	internal override void LoadAtOnce()
	{
		Constants.GameLoadProgress = 0f;
		Constants.GameLoadProgress = SlowLoad();
		while (Constants.GameLoadProgress < 1f && Constants.GameLoadProgress >= 0f)
		{
			Constants.GameLoadProgress = SlowLoad();
		}
	}

	internal override float SlowLoad()
	{
		ContentManager generalContentManager = base.ScreenManager.GeneralContentManager;
		_miniLoadCounter--;
		if (_miniLoadCounter <= 0)
		{
			_miniLoadCounter = 1;
			switch (_loadCounter)
			{
			case 0:
				_gcm.PrepareLoad(generalContentManager, base.ScreenManager.GraphicsDevice);
				break;
			case 1:
				_gcm.LoadShaders(generalContentManager);
				break;
			case 2:
				_gcm.LoadSpritesStage(0);
				break;
			case 3:
				_gcm.LoadSpritesStage(1);
				break;
			case 4:
				_gcm.LoadSpritesStage(2);
				break;
			case 5:
				_gcm.LoadSpritesStage(3);
				break;
			case 6:
				_gcm.LoadSpritesStage(4);
				break;
			case 7:
				_gcm.LoadTilesetsStage(0, generalContentManager);
				break;
			case 8:
				_gcm.LoadTilesetsStage(1, generalContentManager);
				break;
			case 9:
				_gcm.LoadEvents(generalContentManager);
				break;
			case 10:
				_gcm.LoadFX(generalContentManager);
				break;
			case 11:
				_gcm.LoadBackgroundsStage(0, generalContentManager);
				break;
			case 12:
				_gcm.LoadBackgroundsStage(1, generalContentManager);
				break;
			case 13:
				_gcm.LoadBackgroundsStage(2, generalContentManager);
				break;
			case 14:
				_gcm.LoadUI(generalContentManager);
				break;
			case 15:
				base.ScreenManager.Jukebox.LoadAllUnloadedCues();
				base.ScreenManager.Jukebox.SetVolumesBySave(GameConfigSave);
				break;
			case 16:
				RefreshScreenResolution();
				_lunaisClockBar = new HudLunais(_gcm, _topLeftTitleSafe);
				break;
			case 17:
				_minimapSpecification = MinimapSpecification.FromCompressedFile("Content/Levels/Minimap.dat");
				_minimapHud = new HudMinimap(_minimapSpecification, _gcm, _topRightTitleSafe, isMenuMap: false);
				break;
			case 18:
				if (SaveFile.MinimapSave != null)
				{
					SaveFile.MinimapSave.PopulateMinimap(_minimapSpecification);
				}
				break;
			case 19:
				_itemGetBanner = new HudItemGetBanner(_gcm, _titleSafeArea);
				_enemyNameBanner = new HudEnemyNameBanner(_gcm, _titleSafeArea);
				_player1Index = XnaEx.PlayerIndexToInt(base.ControllingPlayer);
				break;
			case 20:
				LoadWarpBackgrounds(2);
				break;
			case 21:
				LoadWarpBackgrounds(15);
				break;
			case 22:
				LoadWarpBackgrounds(18);
				break;
			case 23:
				LoadLevel(SaveFile.CurrentLevel, SaveFile.CurrentRoom, SaveFile.CurrentCheckpoint, isDebug: false);
				break;
			}
			_loadCounter++;
		}
		if (_loadCounter <= 24)
		{
			return (float)_loadCounter / 24f;
		}
		return -1f;
	}

	private void ReloadSaveFile(GameSave save)
	{
		SaveFile = save;
		if (_level != null)
		{
			_level.Dispose();
			base.ScreenManager.Jukebox.StopAllSFX();
			base.ScreenManager.Jukebox.StopSong();
			_level = null;
		}
		if (SaveFile.MinimapSave != null)
		{
			_minimapSpecification.ClearAfterGameOverContinue();
			SaveFile.MinimapSave.PopulateMinimap(_minimapSpecification);
		}
		_levelIndex = -1;
		LevelChangeRequest levelChangeRequest = new LevelChangeRequest();
		levelChangeRequest.LevelID = SaveFile.CurrentLevel;
		levelChangeRequest.RoomID = SaveFile.CurrentRoom;
		levelChangeRequest.CheckpointID = SaveFile.CurrentCheckpoint;
		levelChangeRequest.CutsceneToCall = CutsceneBase.ECutsceneType.Misc0_Revive;
		levelChangeRequest.FadeInTime = 0.5f;
		levelChangeRequest.ShouldPlayLevelSong = true;
		LevelChangeRequest levelChangeRequest2 = levelChangeRequest;
		LoadLevel(levelChangeRequest2);
	}

	internal override void OnScreenResize()
	{
		base.OnScreenResize();
		RefreshScreenResolution();
	}

	private void RefreshScreenResolution()
	{
		_lastGameResolution = base.ScreenManager.CurrentResolution;
		_gameScreenSize = new Point(base.ScreenManager.GraphicsDevice.Viewport.Width, base.ScreenManager.GraphicsDevice.Viewport.Height);
		_gameScreenCenter = new Vector2((float)_gameScreenSize.X / 2f, (float)_gameScreenSize.Y / 2f);
		_levelScreenCenter = new Vector2(256f, 128f);
		_smallScreenRect = new Rectangle((_gameScreenSize.X - SmallScreenSizeZoomed.X) / 2, (_gameScreenSize.Y - SmallScreenSizeZoomed.Y) / 2, SmallScreenSizeZoomed.X, SmallScreenSizeZoomed.Y);
		_screenEffectRect = new Rectangle(_smallScreenRect.X, _smallScreenRect.Y, _smallScreenRect.Width, _smallScreenRect.Height);
		_titleSafeArea = base.ScreenManager.TitleSafeArea;
		if (_titleSafeArea.Left == 0 && _titleSafeArea.Top == 0 && _titleSafeArea.Width > 1000)
		{
			_topLeftTitleSafe = new Point(_smallScreenRect.Left + 4, _smallScreenRect.Top + 8);
			_topRightTitleSafe = new Point(_smallScreenRect.Right - 4, _smallScreenRect.Top + 8);
		}
		else
		{
			_topLeftTitleSafe = new Point(Math.Max(_titleSafeArea.Left, _smallScreenRect.Left) + 8, _titleSafeArea.Location.Y);
			_topRightTitleSafe = new Point(Math.Min(_titleSafeArea.Right, _smallScreenRect.Right) - 8, _titleSafeArea.Location.Y);
		}
		int gameZoomFromType = GraphicsEx.GetGameZoomFromType(_lastGameResolution);
		_camZoom = gameZoomFromType;
		if (_lunaisClockBar != null)
		{
			_lunaisClockBar.Zoom = gameZoomFromType;
			_lunaisClockBar.RefreshDrawPosition(_topLeftTitleSafe);
		}
		if (_minimapHud != null)
		{
			_minimapHud.Zoom = gameZoomFromType;
			_minimapHud.RefreshZoom(_topRightTitleSafe);
		}
		if (_itemGetBanner != null)
		{
			_itemGetBanner.Zoom = gameZoomFromType;
			_itemGetBanner.RefreshZoom(_titleSafeArea);
		}
		if (_enemyNameBanner != null)
		{
			_enemyNameBanner.Zoom = gameZoomFromType;
			_enemyNameBanner.RefreshZoom(_titleSafeArea);
		}
		if (_level != null)
		{
			_level.BackgroundZoom = gameZoomFromType;
			_level.RefreshBackgroundCamera();
			_level.UpdateScreenCenter(_gameScreenCenter, _levelScreenCenter);
		}
		if (_currentDialogue != null)
		{
			_currentDialogue.RefreshSizes();
		}
	}

	private void OnPauseMenuExit()
	{
		base.TransitionOnTime = TimeSpan.FromSeconds(0.10000000149011612);
		if (_level != null && _level.MainHero != null && SaveFile != null)
		{
			_level.RefreshProtagonistControls(GameConfigSave);
			_level.MainHero.RefreshStats(SaveFile);
		}
		if (_lastGameResolution != base.ScreenManager.CurrentResolution)
		{
			RefreshScreenResolution();
		}
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		float num = (float)gameTime.ElapsedGameTime.TotalSeconds * _gameTimeMultiplier;
		if (num > 0.05f)
		{
			num = 0.05f;
		}
		_elapsedGameSeconds += num;
		if (IsActive && _level != null)
		{
			bool isTransitionActive = _isTransitionFadeOutInProgress || _isRoomChangeInProgress || _level.IsLevelChangeRequested || _level.HasPendingRoomChange;
			if (isTransitionActive)
			{
				_transitionWatchdogTimer += num;
				if (_transitionWatchdogTimer > 3.0f)
				{
					Console.WriteLine($"[Transition Watchdog] Transition softlock detected (> 3.0s)! Forcing completion... State: fadeOutInProgress={_isTransitionFadeOutInProgress}, roomChangeInProgress={_isRoomChangeInProgress}, timer={_currentTransitionTimer}, max={_fadeInOutMax}, levelReq={_levelChangeRequest != null}, levelPending={_level.HasPendingRoomChange}");
					_isTransitionFadeOutInProgress = false;
					_isRoomChangeInProgress = false;
					_currentTransitionTimer = 0f;
					_isUsingWhiteFadeOut = false;
					_levelChangeRequest = null;
					_transitionWatchdogTimer = 0f;
					_level.ForceFinalizeTransition();
					Console.WriteLine("[Transition Watchdog] Transition recovery finalized.");
				}
			}
			else
			{
				_transitionWatchdogTimer = 0f;
			}

			UpdateLevel(num);
			UpdateLevelRequests();
			UpdateSaving();
			UpdateFadeInOut(num);
			UpdateHUD(num);
			UpdateScreenEffects(num);
			if (_doesDrawFPS)
			{
				_elapsedTime += gameTime.ElapsedGameTime;
				if (_elapsedTime > TimeSpan.FromSeconds(1.0))
				{
					_elapsedTime -= TimeSpan.FromSeconds(1.0);
					_frameRate = _frameCounter;
					_frameCounter = 0;
					_fpsTicks++;
					_longFrameCounter += _frameRate;
					if (_fpsTicks > 10)
					{
						Console.WriteLine("FPS: " + _longFrameCounter);
						_longFrameCounter = 0L;
						_fpsTicks = 0;
					}
				}
			}
		}
		else if (_level != null)
		{
			UpdateScreenEffects(num);
		}
		if (_hasUpdatedOnce)
		{
			if (_wasGameActive && !IsActive)
			{
				base.ScreenManager.Jukebox.PauseScreenSounds();
				if (base.ControllingPlayer.HasValue)
				{
					GamePad.SetVibration(base.ControllingPlayer.Value, 0f, 0f);
				}
			}
			else if (!_wasGameActive && IsActive)
			{
				base.ScreenManager.Jukebox.UnpauseScreenSounds();
			}
		}
		if (SaveFile != null)
		{
			SaveFile.Update();
		}
		_wasGameActive = IsActive;
		_hasUpdatedOnce = true;
	}

	private void UpdateLevel(float delta)
	{
		if (_isWaitingToPlayLevelSong)
		{
			_level.PlayLevelSong();
			_isWaitingToPlayLevelSong = false;
		}
		if (!_isTransitionFadeOutInProgress)
		{
			_level.IsPlayerInputBlocked = IsPlayerInputBlocked;
			_level.Update(delta, _wasGameActive);
			if (_level.IsFadeOutRequested)
			{
				Console.WriteLine($"[Transition] Fade-out started via IsFadeOutRequested: timer = 0.1s, white = {_level.IsUsingWhiteFadeOut}");
				_isTransitionFadeOutInProgress = true;
				_isRoomChangeInProgress = true;
				_currentTransitionTimer = 0.1f;
				_fadeInOutMax = _currentTransitionTimer;
				_isUsingWhiteFadeOut = _level.IsUsingWhiteFadeOut;
			}
			_isGeneralHUDHidden = _level.IsUIRequestingHide;
		}
	}

	private void UpdateLevelRequests()
	{
		if (_level.IsRequestingGameOverScreen)
		{
			base.ScreenManager.AddScreen(new GameOverScreen(SaveFile, ReloadSaveFile), base.ControllingPlayer);
			return;
		}
		if (_level.IsLevelChangeRequested)
		{
			_level.IsLevelChangeRequested = false;
			_levelChangeRequest = _level.LevelChangeRequest;
			_loadingMessage = Loc.Get("Loading");
			_loadingMessageWidth = base.ScreenManager.MenuFont.MeasureString(_loadingMessage).X;
			_doesDrawLoadingTextOnLevelChange = false;
			_isTransitionFadeOutInProgress = true;
			_isUsingWhiteFadeOut = _levelChangeRequest.IsUsingWhiteFadeOut;
			_currentTransitionTimer = ((_levelChangeRequest.FadeOutTime > 0f) ? _levelChangeRequest.FadeOutTime : 0.1f);
			_fadeInOutMax = _currentTransitionTimer;
			_isRoomChangeInProgress = true;
			Console.WriteLine($"[Transition] Level change requested to Level {_levelChangeRequest.LevelID}, Room {_levelChangeRequest.RoomID}, fadeOutTimer = {_currentTransitionTimer}s, white = {_isUsingWhiteFadeOut}");
			return;
		}
		if (_level.IsRoomChanged)
		{
			_level.IsRoomChanged = false;
			base.ScreenManager.Game.ResetElapsedTime();
		}
		if (_level.IsEndGameRequested)
		{
			if (_level.IsEndGameRequestingEndScreen)
			{
				MessageBoxScreen messageBoxScreen = new MessageBoxScreen(Loc.Get("demo_end"), shouldIncludeUsageText: false, base.ScreenManager.MenuControllerMapping);
				messageBoxScreen.Accepted += delegate
				{
					LoadingScreen.Load(base.ScreenManager, false, null, new TitleBackgroundScreen(shouldDoFullIntro: true));
				};
				messageBoxScreen.Cancelled += delegate
				{
					LoadingScreen.Load(base.ScreenManager, false, null, new TitleBackgroundScreen(shouldDoFullIntro: true));
				};
				base.ScreenManager.AddScreen(messageBoxScreen, base.ControllingPlayer);
			}
			else
			{
				LoadingScreen.Load(base.ScreenManager, false, null, new TitleBackgroundScreen(shouldDoFullIntro: true));
			}
		}
		if (_level.IsRequestingRollCredits)
		{
			LoadingScreen.Load(base.ScreenManager, false, base.ControllingPlayer, new PublisherSplashScreen(), new CreditsScreen(isAtEndOfGame: true, GameConfigSave));
		}
		if (_level.IsRequestingToast)
		{
			ControllerMapping playerControllerMapping = GameConfigSave.PlayerControllerMapping;
			BaseToastPopup baseToastPopup = BaseToastPopup.ToastFromType(_level.RequestedToastType, _level.RequestedToastArgument, _level.RequestedToastScript, _gcm, _titleSafeArea, _camZoom, playerControllerMapping);
			if (baseToastPopup != null)
			{
				base.ScreenManager.AddScreen(baseToastPopup, base.ControllingPlayer);
			}
			_level.IsRequestingToast = false;
		}
	}

	private void UpdateSaving()
	{
		if (_isWaitingForSaveResult && base.ScreenManager.SaveFileManager.IsFinishedSaving)
		{
			_isWaitingForSaveResult = false;
			if (!base.ScreenManager.SaveFileManager.WasSaveSuccessful)
			{
				base.ScreenManager.SaveFileManager.NotifyPlayerOfSaveFailure(base.ControllingPlayer);
				_savingDisplayTimer = 0f;
			}
		}
		if (_isSavingNextFrame)
		{
			if (base.ScreenManager.SaveFileManager.RequestGameSave(SaveFile, base.ControllingPlayer))
			{
				_savingDisplayTimer = 0f;
			}
			_isSavingNextFrame = false;
			_isWaitingForSaveResult = true;
		}
		if (SaveFile.DoesNeedSave)
		{
			SaveFile.ElapsedGameSeconds = (int)_elapsedGameSeconds;
			SaveFile.MinimapSave = MinimapSpecificationSave.FromMinimap(_minimapSpecification);
			if (!base.ScreenManager.SaveFileManager.IsThereNoSaveDevice)
			{
				_savingDisplayTimer = 2.5f;
				_saveMessage = Loc.Get("GameSaved");
				_saveMessageWidth = base.ScreenManager.MenuFont.MeasureString(_saveMessage).X;
			}
			_isSavingNextFrame = true;
			_isWaitingForSaveResult = false;
			SaveFile.DoesNeedSave = false;
		}
	}

	private void UpdateFadeInOut(float delta)
	{
		if (!(_currentTransitionTimer > 0f))
		{
			return;
		}
		_currentTransitionTimer -= delta;
		if (!(_currentTransitionTimer < 0f))
		{
			return;
		}
		if (_isTransitionFadeOutInProgress)
		{
			if (_levelChangeRequest != null)
			{
				if (_levelChangeRequest.BlackScreenTimeTimer > _levelChangeRequest.AdditionalBlackScreenTime)
				{
					try
					{
						Console.WriteLine($"[Transition] Loading level {_levelChangeRequest.LevelID}, Room {_levelChangeRequest.RoomID}...");
						LoadLevel(_levelChangeRequest);
						if (_level != null)
						{
							_level.PlacePlayerAtDoor(_levelChangeRequest);
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine($"[Transition ERROR] Failed to load level during transition: {ex}");
					}
					finally
					{
						_isTransitionFadeOutInProgress = false;
						_isRoomChangeInProgress = false;
						_currentTransitionTimer = ((_levelChangeRequest != null && _levelChangeRequest.FadeInTime > 0f) ? _levelChangeRequest.FadeInTime : 0.1f);
						_fadeInOutMax = _currentTransitionTimer;
						_levelChangeRequest = null;
						Console.WriteLine($"[Transition] Level transition complete. Fade-in timer: {_currentTransitionTimer}s");
					}
				}
				else
				{
					_currentTransitionTimer += delta;
				}
				if (_levelChangeRequest != null)
				{
					_levelChangeRequest.BlackScreenTimeTimer += delta;
				}
			}
			else
			{
				_isTransitionFadeOutInProgress = false;
				_currentTransitionTimer = 0.1f;
				_fadeInOutMax = _currentTransitionTimer;
				Console.WriteLine("[Transition] Room change fade-out complete. Advancing to new room update.");
			}
		}
		else if (_isRoomChangeInProgress)
		{
			_currentTransitionTimer = 0f;
			_isRoomChangeInProgress = false;
			_isUsingWhiteFadeOut = false;
			Console.WriteLine("[Transition] Fade-in complete. Gameplay active.");
		}
	}

	private void UpdateHUD(float delta)
	{
		_lunaisClockBar.Update(delta, (_level.Heroes.Count > 0) ? _level.MainHero : null, SaveFile);
		_minimapHud.Update(delta, _level);
		_itemGetBanner.Update(delta, _level, base.ScreenManager.MenuControllerMapping);
		_enemyNameBanner.Update(delta, _level);
		if (_level.IsButtonPromptRequested)
		{
			if (_currentButtonPromptScreen == null)
			{
				_currentButtonPromptScreen = new ButtonPromptScreen(_level.ButtonTutorialDisplayIndex, _level.ButtonPromptPosition, _level.ButtonTutorialDisplayAmount, _level);
				base.ScreenManager.AddScreen(_currentButtonPromptScreen, base.ControllingPlayer);
			}
			if (_currentButtonPromptScreen != null)
			{
				_currentButtonPromptScreen.Refresh(_level.ButtonPromptPosition, _level.ButtonTutorialDisplayAmount);
			}
			_level.IsButtonPromptRequested = false;
		}
		else if (_currentButtonPromptScreen != null && _currentButtonPromptScreen.IsDead)
		{
			_currentButtonPromptScreen = null;
		}
		IsPlayerInputBlocked = false;
		if (_currentDialogue == null && _level.IsNewDialogueAvailable)
		{
			_currentDialogue = _level.GetNextDialogue();
		}
		if (_currentDialogue != null)
		{
			_isGeneralHUDHidden = _currentDialogue.DoesHideHUD;
			if (_currentDialogue.IsFinished)
			{
				if (_level.IsNewDialogueAvailable)
				{
					_currentDialogue = _level.GetNextDialogue();
					IsPlayerInputBlocked = IsPlayerInputBlocked || _currentDialogue.DoesBlockPlayerInput;
				}
				else
				{
					_currentDialogue = null;
					_level.IsDialoguePlaying = false;
				}
			}
			else
			{
				_currentDialogue.Update(delta, _lastInputState, base.ControllingPlayer);
				IsPlayerInputBlocked = IsPlayerInputBlocked || _currentDialogue.DoesBlockPlayerInput;
			}
		}
		if (_level != null && _level.ScreenAddQueue.Count > 0)
		{
			base.ScreenManager.AddScreen(_level.ScreenAddQueue.Dequeue(), base.ControllingPlayer);
		}
		if (_isGeneralHUDHidden)
		{
			_hideHUDTimer += delta;
			if (_hideHUDTimer > 0.15f)
			{
				_hideHUDTimer = 0.15f;
			}
		}
		else if (_hideHUDTimer > 0f)
		{
			_hideHUDTimer -= delta;
			if (_hideHUDTimer < 0f)
			{
				_hideHUDTimer = 0f;
			}
		}
		if (_savingDisplayTimer > 0f)
		{
			_savingDisplayTimer -= delta;
			if (_savingDisplayTimer < 0f)
			{
				_savingDisplayTimer = 0f;
			}
		}
		if (!_isShowingSkipCutscenePrompt)
		{
			return;
		}
		_skipCutscenePromptTimer += delta;
		if (_skipCutscenePromptTimer >= 3f)
		{
			_isShowingSkipCutscenePrompt = false;
			return;
		}
		_skipCutsceneTextDrawLocation = new Vector2(_titleSafeArea.Center.X, _titleSafeArea.Bottom + (int)(-32f * _camZoom));
		if (_level != null && !_level.IsPlayerInputBlocked && _skipCutscenePromptTimer < 2.8f)
		{
			_skipCutscenePromptTimer = 2.8f;
		}
		if (_skipCutscenePromptTimer < 0.2f)
		{
			float num = _skipCutscenePromptTimer / 0.2f;
			_skipCutsceneTextColor = BaseSkipCutscenePromptColor * num;
			_skipCutsceneTextShadowColor = BaseSkipCutscenePromptShadowColor * num;
		}
		else if (_skipCutscenePromptTimer > 2.8f)
		{
			float num2 = 1f - (_skipCutscenePromptTimer - 2.8f) / 0.2f;
			_skipCutsceneTextColor = BaseSkipCutscenePromptColor * num2;
			_skipCutsceneTextShadowColor = BaseSkipCutscenePromptShadowColor * num2;
		}
		else
		{
			_skipCutsceneTextColor = BaseSkipCutscenePromptColor;
			_skipCutsceneTextShadowColor = BaseSkipCutscenePromptShadowColor;
		}
	}

	private void UpdateScreenEffects(float delta)
	{
		if (!IsActive)
		{
			return;
		}
		while (_level != null && _level.ScreenEffectQueue.Count > 0)
		{
			_screenEffects.Add(_level.ScreenEffectQueue.Dequeue());
		}
		for (int num = _screenEffects.Count - 1; num >= 0; num--)
		{
			ScreenEffect screenEffect = _screenEffects[num];
			screenEffect.Update(delta);
			if (screenEffect.IsFinished)
			{
				_screenEffects.RemoveAt(num);
			}
		}
	}

	public override void HandleInput(InputState input)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		_lastInputState = input;
		if (base.ControllingPlayer.HasValue && _level != null)
		{
			int value = (int)base.ControllingPlayer.Value;
			GamePadState gamePadState = input.CurrentGamePadStates[value];
			bool flag = !gamePadState.IsConnected && input.GamePadWasConnected[value];
			if (!_level.IsDoingPlayerDeathCutscene)
			{
				bool flag2 = false;
				bool flag3 = input.IsNewPressPause(base.ControllingPlayer);
				bool flag4 = input.IsNewPressCutsceneSkip(base.ControllingPlayer);
				if (flag3 || flag || flag4)
				{
					Console.WriteLine($"[GameplayScreen HandleInput] flag3(Pause)={flag3} flag4(Skip)={flag4} _level.IsPlayerInputBlocked={_level.IsPlayerInputBlocked} Screen.IsPlayerInputBlocked={IsPlayerInputBlocked} _isShowingSkipCutscenePrompt={_isShowingSkipCutscenePrompt}");
					if (!_level.IsPlayerInputBlocked)
					{
						if (flag3 && !_level.IsPreventingPauseMenuUsage)
						{
							flag2 = true;
							CreatePauseMenu(isShowingMap: false);
							input.GamePadWasConnected[value] = false;
						}
					}
					else if (flag4 && !_level.IsActiveScriptUnskippable)
					{
						if (_isShowingSkipCutscenePrompt)
						{
							_level.SkipCutscene();
							_isShowingSkipCutscenePrompt = false;
						}
						else
						{
							_isShowingSkipCutscenePrompt = true;
							_skipCutscenePromptTimer = 0f;
							_skipCutsceneText = Loc.Get("Tutorial_SkipCutscene");
							_skipCutsceneTextColor = Color.Transparent;
							_skipCutsceneTextShadowColor = Color.Transparent;
							Vector2 vector = base.ScreenManager.MenuFont.MeasureString(_skipCutsceneText);
							_skipCutsceneTextOrigin = new Vector2((int)(vector.X * 0.5f), (int)(vector.Y * 0.5f));
						}
					}
				}
				if (!flag2 && !flag && !_level.IsPlayerInputBlocked && !_level.IsPreventingPauseMenuUsage && input.IsNewPressMap(base.ControllingPlayer) && _level.Minimap.AreAnyRoomsVisible())
				{
					CreatePauseMenu(isShowingMap: true);
				}
				if (_level.IsFamiliarAvailableToPlay && GameConfigSave.FamiliarControllerMapping != null)
				{
					ControllerMapping familiarControllerMapping = GameConfigSave.FamiliarControllerMapping;
					if (familiarControllerMapping.Mappings.ContainsKey(5))
					{
						ButtonMapping buttonMapping = familiarControllerMapping.Mappings[5];
						if (buttonMapping.Sources.Count > 0)
						{
							ButtonMappingSource buttonMappingSource = buttonMapping.Sources[0];
							bool flag5 = false;
							PlayerIndex playerIndex = PlayerIndex.One;
							if (buttonMappingSource.SourceType == ButtonMappingSource.ESourceType.Keyboard)
							{
								if (input.IsNewKeyPress(buttonMappingSource.KeyboardKey, null, out playerIndex))
								{
									flag5 = true;
									if (playerIndex == base.ControllingPlayer)
									{
										playerIndex = XnaEx.GetFirstPlayerIndexExcept(playerIndex);
									}
								}
							}
							else if (buttonMappingSource.SourceType == ButtonMappingSource.ESourceType.GamepadButton)
							{
								for (int i = 1; i <= 4; i++)
								{
									if (i != _player1Index)
									{
										PlayerIndex value2 = XnaEx.IntToPlayerIndex(i);
										if (input.IsNewButtonPress(buttonMappingSource.GamepadButton, value2, out playerIndex))
										{
											flag5 = true;
											break;
										}
									}
								}
							}
							if (flag5)
							{
								_level.ActivateFamiliar(playerIndex);
							}
						}
					}
				}
			}
		}
		if (input.IsKeyHold(Keys.Escape, base.ControllingPlayer, out var playerIndex2) && input.IsKeyHold(Keys.LeftShift, base.ControllingPlayer, out playerIndex2))
		{
			base.ScreenManager.Game.Exit();
		}
	}

	private void CreatePauseMenu(bool isShowingMap)
	{
		Protagonist mainHero = _level.MainHero;
		if (mainHero != null)
		{
			CharacterStats characterStats = SaveFile.CharacterStats;
			characterStats.HP = mainHero.HP;
			characterStats.Sand = mainHero.MP;
			characterStats.Aura = mainHero.Aura;
			characterStats.CurrentStatus = ((mainHero.StatusEffects.Count > 0) ? mainHero.StatusEffects[0].StatusEffectType : EStatusEffectType.None);
		}
		if (isShowingMap)
		{
			ControllerMapping menuControllerMapping = base.ScreenManager.MenuControllerMapping;
			MapMenuScreen screen = new MapMenuScreen(SaveFile, _gcm, _level, menuControllerMapping, delegate
			{
			});
			base.ScreenManager.AddScreen(screen, base.ControllingPlayer);
		}
		else if (_itemGetBanner != null && !_itemGetBanner.IsFinished && _itemGetBanner.IsShowingViewPrompt)
		{
			_itemGetBanner.IsShowingViewPrompt = false;
			PauseMenuScreen pauseMenuScreen = new PauseMenuScreen(_level, _gcm, SaveFile, ElapsedGameSeconds, GetMapCompletionPercentage(), OnPauseMenuExit);
			base.ScreenManager.AddScreen(pauseMenuScreen, base.ControllingPlayer);
			if (_itemGetBanner.IsMapReveal)
			{
				pauseMenuScreen.GoToMap(_itemGetBanner.MapRevealLevelIndex);
			}
			else
			{
				pauseMenuScreen.GoToItem(_itemGetBanner.ItemCategory, _itemGetBanner.ItemValue);
			}
		}
		else
		{
			base.ScreenManager.AddScreen(new PauseMenuScreen(_level, _gcm, SaveFile, ElapsedGameSeconds, GetMapCompletionPercentage(), OnPauseMenuExit), base.ControllingPlayer);
		}
	}

	public override void Draw(GameTime gameTime)
	{
		if (_level != null)
		{
			SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
			DrawLevel(spriteBatch);
			DrawBackgrounds(spriteBatch);
			spriteBatch.Draw(_gcm.LevelRenderTarget, _gameScreenCenter, null, _level.LevelDrawColor, 0f, _levelScreenCenter, _camZoom, SpriteEffects.None, 0f);
			DrawForegrounds(spriteBatch);
			DrawHud(spriteBatch);
			DrawScreenEffects(spriteBatch);
			if (_currentTransitionTimer > 0f)
			{
				DrawLevelTransition(spriteBatch);
			}
			_gcm.CropScreen(spriteBatch, _smallScreenRect, _gameScreenSize);
			spriteBatch.End();
			if (base.TransitionOffPercentage > 0f)
			{
				base.ScreenManager.FadeBackBufferToBlack(255 - base.TransitionAlpha);
			}
		}
	}

	private void DrawBackgrounds(SpriteBatch spriteBatch)
	{
		base.ScreenManager.GraphicsDevice.SetRenderTarget(null);
		base.ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, _level.GetBackgroundWipeColor(), 0f, 0);
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
		if (_level.IsTimeFrozen)
		{
			BeginSepiaBatch(spriteBatch);
		}
		_level.DrawBackgrounds(spriteBatch, _camZoom);
		if (_level.IsTimeFrozen)
		{
			EndSepiaBatch(spriteBatch);
		}
	}

	private void DrawForegrounds(SpriteBatch spriteBatch)
	{
		if (_level.IsTimeFrozen)
		{
			BeginSepiaBatch(spriteBatch);
		}
		_level.DrawForegrounds(spriteBatch, _camZoom);
		if (_level.IsTimeFrozen)
		{
			EndSepiaBatch(spriteBatch);
		}
	}

	private void DrawLevel(SpriteBatch spriteBatch)
	{
		if (!_level.IsTimeFrozen)
		{
			SwitchRenderTarget(_gcm.LevelRenderTarget, spriteBatch, doesEndSpriteBatch: false, doesClear: true);
			_level.SetRenderEffectValues();
			_level.DrawGameEvents(spriteBatch, ETeamSide.Neutral, EDrawPlane.Parralax);
			_level.DrawBackgroundTiles(spriteBatch);
			_level.DrawPlatforms(spriteBatch);
			_level.DrawGameEvents(spriteBatch, ETeamSide.Neutral, EDrawPlane.Back);
			_level.DrawGameEvents(spriteBatch, ETeamSide.Heroes, EDrawPlane.Back);
			_level.DrawMonsters(spriteBatch, isAffectedByTime: false);
			_level.DrawNPCs(spriteBatch, EDrawPlane.Normal);
			_level.DrawMonsters(spriteBatch, isAffectedByTime: true);
			_level.DrawBattleAnimations(spriteBatch, ETeamSide.Enemies, EDrawPlane.Normal);
			if (!_level.IsDoingPlayerDeathCutscene)
			{
				_level.DrawHeroes(spriteBatch);
				_level.DrawProjectiles(spriteBatch, ETeamSide.Heroes);
			}
			_level.DrawProjectiles(spriteBatch, ETeamSide.Enemies);
			_level.DrawBattleAnimations(spriteBatch, ETeamSide.Neutral, EDrawPlane.Normal);
			_level.DrawBattleAnimations(spriteBatch, ETeamSide.Heroes, EDrawPlane.Normal);
			_level.DrawGameEvents(spriteBatch, ETeamSide.Heroes, EDrawPlane.Normal);
			_level.DrawItems(spriteBatch);
			_level.DrawBattleAnimations(spriteBatch, ETeamSide.Heroes, EDrawPlane.Front);
			_level.DrawNPCs(spriteBatch, EDrawPlane.Front);
			_level.DrawGameEvents(spriteBatch, ETeamSide.Neutral, EDrawPlane.Normal);
			_level.DrawTiles(spriteBatch);
			_level.DrawGameEvents(spriteBatch, ETeamSide.Neutral, EDrawPlane.Front);
			_level.DrawBattleAnimations(spriteBatch, ETeamSide.Neutral, EDrawPlane.Front);
			_level.DrawBattleAnimations(spriteBatch, ETeamSide.Enemies, EDrawPlane.Front);
			_level.DrawForegroundTiles(spriteBatch);
			_level.DrawNumbers(spriteBatch);
			if (_level.IsDoingPlayerDeathCutscene)
			{
				_level.DrawHeroes(spriteBatch);
			}
			spriteBatch.End();
			return;
		}

		SwitchRenderTarget(_gcm.TemporaryLevelRenderTarget, spriteBatch, doesEndSpriteBatch: false, doesClear: true);
		_level.SetRenderEffectValues();
		_level.DrawGameEvents(spriteBatch, ETeamSide.Neutral, EDrawPlane.Parralax);
		_level.DrawBackgroundTiles(spriteBatch);
		_level.DrawPlatforms(spriteBatch);
		_level.DrawGameEvents(spriteBatch, ETeamSide.Neutral, EDrawPlane.Back);
		PublishLevelRenderTarget(spriteBatch, _gcm.TemporaryLevelRenderTarget, _gcm.LevelRenderTarget, ETeamSide.Neutral, doesSwitchBack: true, doesClearDestination: true);
		_level.DrawGameEvents(spriteBatch, ETeamSide.Heroes, EDrawPlane.Back);
		_level.DrawMonsters(spriteBatch, isAffectedByTime: false);
		PublishLevelRenderTarget(spriteBatch, _gcm.TemporaryLevelRenderTarget, _gcm.LevelRenderTarget, ETeamSide.Heroes, doesSwitchBack: true, doesClearDestination: false);
		_level.DrawNPCs(spriteBatch, EDrawPlane.Normal);
		_level.DrawMonsters(spriteBatch, isAffectedByTime: true);
		_level.DrawBattleAnimations(spriteBatch, ETeamSide.Enemies, EDrawPlane.Normal);
		PublishLevelRenderTarget(spriteBatch, _gcm.TemporaryLevelRenderTarget, _gcm.LevelRenderTarget, ETeamSide.Enemies, doesSwitchBack: true, doesClearDestination: false);
		if (!_level.IsDoingPlayerDeathCutscene)
		{
			_level.DrawHeroes(spriteBatch);
			_level.DrawProjectiles(spriteBatch, ETeamSide.Heroes);
			PublishLevelRenderTarget(spriteBatch, _gcm.TemporaryLevelRenderTarget, _gcm.LevelRenderTarget, ETeamSide.Heroes, doesSwitchBack: true, doesClearDestination: false);
		}
		_level.DrawProjectiles(spriteBatch, ETeamSide.Enemies);
		_level.DrawBattleAnimations(spriteBatch, ETeamSide.Neutral, EDrawPlane.Normal);
		PublishLevelRenderTarget(spriteBatch, _gcm.TemporaryLevelRenderTarget, _gcm.LevelRenderTarget, ETeamSide.Enemies, doesSwitchBack: true, doesClearDestination: false);
		_level.DrawBattleAnimations(spriteBatch, ETeamSide.Heroes, EDrawPlane.Normal);
		_level.DrawGameEvents(spriteBatch, ETeamSide.Heroes, EDrawPlane.Normal);
		_level.DrawItems(spriteBatch);
		_level.DrawBattleAnimations(spriteBatch, ETeamSide.Heroes, EDrawPlane.Front);
		PublishLevelRenderTarget(spriteBatch, _gcm.TemporaryLevelRenderTarget, _gcm.LevelRenderTarget, ETeamSide.Heroes, doesSwitchBack: true, doesClearDestination: false);
		_level.DrawNPCs(spriteBatch, EDrawPlane.Front);
		_level.DrawGameEvents(spriteBatch, ETeamSide.Neutral, EDrawPlane.Normal);
		_level.DrawTiles(spriteBatch);
		_level.DrawGameEvents(spriteBatch, ETeamSide.Neutral, EDrawPlane.Front);
		_level.DrawBattleAnimations(spriteBatch, ETeamSide.Neutral, EDrawPlane.Front);
		_level.DrawBattleAnimations(spriteBatch, ETeamSide.Enemies, EDrawPlane.Front);
		_level.DrawForegroundTiles(spriteBatch);
		_level.DrawNumbers(spriteBatch);
		if (!_level.IsDoingPlayerDeathCutscene)
		{
			PublishLevelRenderTarget(spriteBatch, _gcm.TemporaryLevelRenderTarget, _gcm.LevelRenderTarget, ETeamSide.Neutral, doesSwitchBack: false, doesClearDestination: false);
		}
		else
		{
			PublishLevelRenderTarget(spriteBatch, _gcm.TemporaryLevelRenderTarget, _gcm.LevelRenderTarget, ETeamSide.Neutral, doesSwitchBack: true, doesClearDestination: false);
			_level.DrawHeroes(spriteBatch);
			PublishLevelRenderTarget(spriteBatch, _gcm.TemporaryLevelRenderTarget, _gcm.LevelRenderTarget, ETeamSide.Heroes, doesSwitchBack: false, doesClearDestination: false);
		}
		spriteBatch.End();
	}

	private void DrawScreenEffects(SpriteBatch spriteBatch)
	{
		Texture2D txBlankSquare = _gcm.TxBlankSquare;
		foreach (ScreenEffect screenEffect in _screenEffects)
		{
			screenEffect.Draw(spriteBatch, txBlankSquare, _screenEffectRect);
		}
	}

	private void SwitchRenderTarget(RenderTarget2D renderTarget, SpriteBatch spriteBatch, bool doesEndSpriteBatch, bool doesClear)
	{
		if (doesEndSpriteBatch)
		{
			spriteBatch.End();
		}
		base.ScreenManager.GraphicsDevice.SetRenderTarget(renderTarget);
		if (doesClear)
		{
			base.ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0f, 0);
		}
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
	}

	private void PublishLevelRenderTarget(SpriteBatch spriteBatch, RenderTarget2D source, RenderTarget2D destination, ETeamSide teamSide, bool doesSwitchBack, bool doesClearDestination)
	{
		SwitchRenderTarget(destination, spriteBatch, doesEndSpriteBatch: true, doesClearDestination);
		if (_level.IsTimeFrozen && teamSide != ETeamSide.Heroes)
		{
			BeginSepiaBatch(spriteBatch);
		}
		spriteBatch.Draw(source, _gameScreenCenter, null, Color.White, 0f, _gameScreenCenter, 1f, SpriteEffects.None, 0f);
		if (doesSwitchBack)
		{
			SwitchRenderTarget(source, spriteBatch, doesEndSpriteBatch: true, doesClear: true);
		}
	}

	public void BeginSepiaBatch(SpriteBatch spriteBatch)
	{
		Effect effect = (_level.IsUsingGrayscaleEffect ? _gcm.EfGrayscale : _gcm.EfSepiaTone);
		spriteBatch.End();
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, effect);
	}

	public void EndSepiaBatch(SpriteBatch spriteBatch)
	{
		spriteBatch.End();
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
	}

	private void LoadWarpBackgrounds(int key)
	{
		if (_cachedWarpBackgrounds.ContainsKey(key))
		{
			return;
		}
		try
		{
			List<BackgroundSpecification> list = LevelSpecification.LoadCompressedLevelWarpBackgrounds(Level.GetLevelPathFromID(key, isCompressed: true));
			if (list != null)
			{
				_cachedWarpBackgrounds[key] = list;
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("Failed to load backgrounds from level " + key + "\n" + ex.Message);
		}
	}

	private void DebugLoadAdjacentLevel(bool isPlus)
	{
		_levelIndex = _level.ID + (isPlus ? 1 : (-1));
		int roomID = 0;
		if (_levelIndex > 18)
		{
			_levelIndex = 0;
		}
		else if (_levelIndex < 0)
		{
			_levelIndex = 18;
		}
		if (_levelIndex == 7)
		{
			roomID = 1;
		}
		int num = ((_level == null) ? (-1) : _level.ID);
		LevelChangeRequest levelChangeRequest = new LevelChangeRequest();
		levelChangeRequest.LevelID = _levelIndex;
		levelChangeRequest.RoomID = roomID;
		levelChangeRequest.CheckpointID = 0;
		levelChangeRequest.ShouldPlayLevelSong = num != _levelIndex;
		levelChangeRequest.IsDebugRequest = true;
		levelChangeRequest.EnterDirection = EDirection.West;
		LevelChangeRequest levelChangeRequest2 = levelChangeRequest;
		LoadLevel(levelChangeRequest2);
	}

	public void LoadLevel(int levelID, int roomNumber, int whichCheckpoint, bool isDebug)
	{
		int num = ((_level == null) ? (-1) : _level.ID);
		if (levelID == 1 && roomNumber == 0 && whichCheckpoint == 0)
		{
			levelID = 0;
		}
		LevelChangeRequest levelChangeRequest = new LevelChangeRequest();
		levelChangeRequest.LevelID = levelID;
		levelChangeRequest.RoomID = roomNumber;
		levelChangeRequest.CheckpointID = whichCheckpoint;
		levelChangeRequest.ShouldPlayLevelSong = levelID != num;
		levelChangeRequest.IsDebugRequest = isDebug;
		LevelChangeRequest levelChangeRequest2 = levelChangeRequest;
		LoadLevel(levelChangeRequest2);
	}

	public void LoadLevel(LevelChangeRequest levelChangeRequest)
	{
		LevelSpecification levelSpecification = null;
		int num = levelChangeRequest.LevelID;
		if (num < 0)
		{
			num = 18;
		}
		else if (num > 18)
		{
			num = 0;
		}
		try
		{
			string levelPath = Level.GetLevelPathFromID(num, isCompressed: true);
			Console.WriteLine($"[GameplayScreen] Loading level from path: {levelPath}");
			levelSpecification = LevelSpecification.FromCompressedFile(levelPath);
			if (levelSpecification == null || levelSpecification.ID != num)
			{
				base.ScreenManager.AddScreen(new MessageBoxScreen("Failed to load level.\nPlease email info@lunarraygames.com for help!", base.ScreenManager.MenuControllerMapping), base.ControllingPlayer);
				ExitScreen();
				return;
			}
			levelSpecification.ID = num;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[GameplayScreen LoadLevel ERROR] Failed to load level: {ex}");
		}
		if (levelSpecification == null)
		{
			return;
		}
		if (!_cachedWarpBackgrounds.ContainsKey(levelSpecification.ID))
		{
			_cachedWarpBackgrounds.Add(levelSpecification.ID, levelSpecification.WarpBackgrounds.ToArray());
		}
		if (_level != null)
		{
			if (_level.MainHero != null)
			{
				Protagonist mainHero = _level.MainHero;
				CharacterStats characterStats = SaveFile.CharacterStats;
				characterStats.HP = mainHero.HP;
				characterStats.Sand = mainHero.MP;
				characterStats.Aura = mainHero.Aura;
				characterStats.CurrentStatus = ((mainHero.StatusEffects.Count > 0) ? mainHero.StatusEffects[0].StatusEffectType : EStatusEffectType.None);
			}
			_level.Dispose();
			base.ScreenManager.Jukebox.StopAllSFX();
			GC.Collect();
		}
		_level = new Level(_gcm, levelSpecification, _minimapSpecification, base.ScreenManager.Jukebox, _gameScreenCenter, _levelScreenCenter, _player1Index, SaveFile, GameConfigSave, levelChangeRequest, _cachedWarpBackgrounds)
		{
			BackgroundZoom = _camZoom
		};
		if (levelChangeRequest.ShouldPlayLevelSong)
		{
			_isWaitingToPlayLevelSong = true;
		}
		_currentTransitionTimer = ((levelChangeRequest.FadeInTime > 0f) ? levelChangeRequest.FadeInTime : 0.1f);
		_fadeInOutMax = _currentTransitionTimer;
		base.ScreenManager.UpdateRichPresence(num);
		base.ScreenManager.Game.ResetElapsedTime();
	}

	private void DrawHud(SpriteBatch spriteBatch)
	{
		if (IsHidingHUD)
		{
			return;
		}
		float alphaAmount = 1f - _hideHUDTimer / 0.15f;
		_lunaisClockBar.Draw(spriteBatch, alphaAmount);
		_minimapHud.Draw(spriteBatch, alphaAmount);
		_itemGetBanner.Draw(spriteBatch);
		_enemyNameBanner.Draw(spriteBatch);
		if (_currentDialogue != null)
		{
			_currentDialogue.Draw(spriteBatch);
		}
		if (_savingDisplayTimer > 0f)
		{
			float num = 1f;
			if (_savingDisplayTimer > 2f)
			{
				num = (2.5f - _savingDisplayTimer) * 2f;
			}
			else if (_savingDisplayTimer < 0.5f)
			{
				num = _savingDisplayTimer * 2f;
			}
			Vector2 drawPos = new Vector2((int)((float)_titleSafeArea.Right + (-30f - _saveMessageWidth) * _camZoom), (int)((float)_titleSafeArea.Bottom + -26f * _camZoom));
			DrawingEx.DrawString(spriteBatch, base.ScreenManager.MenuFont, _saveMessage, new Vector2(drawPos.X + _camZoom, drawPos.Y + _camZoom), SaveDisplayShadowColor * num, Vector2.Zero, _camZoom);
			DrawingEx.DrawString(spriteBatch, base.ScreenManager.MenuFont, _saveMessage, drawPos, SaveDisplayTextColor * num, Vector2.Zero, _camZoom);
		}
		if (_isShowingSkipCutscenePrompt)
		{
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					DrawingEx.DrawString(spriteBatch, base.ScreenManager.MenuFont, _skipCutsceneText, new Vector2(_skipCutsceneTextDrawLocation.X + (float)i * _camZoom, _skipCutsceneTextDrawLocation.Y + (float)j * _camZoom), _skipCutsceneTextShadowColor, _skipCutsceneTextOrigin, _camZoom);
				}
			}
			DrawingEx.DrawString(spriteBatch, base.ScreenManager.MenuFont, _skipCutsceneText, _skipCutsceneTextDrawLocation, _skipCutsceneTextColor, _skipCutsceneTextOrigin, _camZoom);
		}
		DrawDebugHUD(spriteBatch);
	}

	private void DrawDebugHUD(SpriteBatch spriteBatch)
	{
		if (_doesDrawFPS)
		{
			_frameCounter++;
			string value = $"fps: {_frameRate}";
			DrawShadowedString(spriteBatch, _gcm.ActiveFont, value, new Vector2(_titleSafeArea.Right - 100, _titleSafeArea.Y), Color.Gold, _camZoom);
		}
	}

	private static void DrawShadowedString(SpriteBatch spriteBatch, SpriteFont font, string value, Vector2 position, Color color, float zoom)
	{
		DrawingEx.DrawString(spriteBatch, font, value, position + new Vector2(zoom, zoom), Color.Black, Vector2.Zero, zoom);
		DrawingEx.DrawString(spriteBatch, font, value, position, color, Vector2.Zero, zoom);
	}

	private void DrawLevelTransition(SpriteBatch spriteBatch)
	{
		float num = ((_fadeInOutMax > 0f) ? _fadeInOutMax : 0.1f);
		float num2 = ((!_isRoomChangeInProgress) ? MathHelper.Clamp(_currentTransitionTimer / num, 0f, 1f) : ((!_isTransitionFadeOutInProgress) ? MathHelper.Clamp(_currentTransitionTimer / num, 0f, 1f) : MathHelper.Clamp(1f - _currentTransitionTimer / num, 0f, 1f)));
		if (num2 > 0f)
		{
			Color color = (_isUsingWhiteFadeOut ? new Color(num2, num2, num2, num2) : new Color(0f, 0f, 0f, num2));
			spriteBatch.Draw(_gcm.TxBlankSquare, base.ScreenManager.ViewPortArea, color);
			if (_doesDrawLoadingTextOnLevelChange && (_levelChangeRequest != null || !_isRoomChangeInProgress) && _loadingMessage != null)
			{
				Color color2 = (_isUsingWhiteFadeOut ? Color.Black : SaveDisplayTextColor);
				float num3 = num2;
				DrawingEx.DrawString(drawPos: new Vector2((int)((float)_titleSafeArea.Center.X + (4f + (0f - _loadingMessageWidth) * 0.5f) * _camZoom), (int)((float)_titleSafeArea.Bottom + -26f * _camZoom)), spriteBatch: spriteBatch, font: base.ScreenManager.MenuFont, text: _loadingMessage, color: color2 * num3, origin: Vector2.Zero, zoom: _camZoom);
			}
		}
	}

	public float GetMapCompletionPercentage()
	{
		float result = 0f;
		if (_minimapSpecification != null)
		{
			result = _minimapSpecification.GetCompletionPercentage();
		}
		return result;
	}

	public override void ExitScreen()
	{
		base.ScreenManager.Jukebox.StopAllSFX();
		base.ExitScreen();
	}
}
