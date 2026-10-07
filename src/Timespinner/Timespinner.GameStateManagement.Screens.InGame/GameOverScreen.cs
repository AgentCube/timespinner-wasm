using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameStateManagement.Screens.BaseClasses;
using Timespinner.GameStateManagement.Screens.BaseClasses.Menu;
using Timespinner.GameStateManagement.Screens.MainMenu;

namespace Timespinner.GameStateManagement.Screens.InGame;

internal class GameOverScreen : MenuScreen
{
	private const int YesNoMenuColumnWidth = 60;

	private const int YesNoOffsetY = 24;

	private const int ContinueTextOffsetY = 0;

	private const float TimeBeforeShowingEntries = 1f;

	private const float TimeToContinueFade = 0.25f;

	private readonly string _continueText;

	private readonly MenuEntry _yesEntry;

	private readonly MenuEntry _noEntry;

	private readonly GameSave _gameSave;

	private readonly Action<GameSave> _reloadSaveAction;

	private bool _areEntriesAvailable;

	private bool _isContinuing;

	private bool _isLoadingContinue;

	private int _zoom;

	private float _continueFadeTimer;

	private float _continueFadePercentage;

	private float _showEntriesWaitTimer;

	private Vector2 _continueTextPosition;

	private SpriteFont _font;

	private GameSave _continueSave;

	public GameOverScreen(GameSave gameSave, Action<GameSave> reloadSaveAction)
		: base("")
	{
		_gameSave = gameSave;
		_reloadSaveAction = reloadSaveAction;
		_doesUseBlackGradientBox = false;
		_doesUseCursor = false;
		_primaryMenuCollection.ColumnCount = 2;
		_continueText = Loc.Get("GameOverContinue");
		_yesEntry = new MenuEntry(Loc.Get("GameOverYes"));
		_yesEntry.Selected += OnYesEntrySelected;
		_noEntry = new MenuEntry(Loc.Get("GameOverNo"));
		_noEntry.Selected += OnNoEntrySelected;
	}

	public override void LoadContent()
	{
		base.LoadContent();
		base.ScreenManager.SaveFileManager.StartLoadAllSaves();
	}

	internal override void RefreshSizes()
	{
		base.RefreshSizes();
		_zoom = Constants.InGameZoom;
		_font = base.ScreenManager.MenuFont;
		_primaryMenuCollection.SetColumnWidth(60 * _zoom, _zoom);
		Vector2 vector = base.ScreenManager.MenuFont.MeasureString(_yesEntry.Text) * _zoom;
		int num = 60 * _zoom;
		base.MenuOffset = new Point(-(int)((vector.X + (float)num) / 2f) + _zoom, 24 * _zoom);
		Vector2 vector2 = _font.MeasureString(_continueText) / 2f;
		_continueTextPosition = new Vector2((int)((float)base.TitleSafeArea.Center.X - vector2.X * (float)_zoom), base.TitleSafeArea.Center.Y);
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		float num = (float)gameTime.ElapsedGameTime.TotalSeconds;
		if (_showEntriesWaitTimer < 1f)
		{
			_showEntriesWaitTimer += num;
		}
		if (!_areEntriesAvailable && _showEntriesWaitTimer >= 1f && base.ScreenManager.SaveFileManager.IsFinishedLoading)
		{
			ShowMenuEntries();
		}
		if (_isContinuing)
		{
			if (_continueFadeTimer < 0.25f)
			{
				_continueFadeTimer += num;
				if (_continueFadeTimer < 0.25f)
				{
					_continueFadePercentage = _continueFadeTimer / 0.25f;
				}
				else
				{
					_continueFadePercentage = 1f;
				}
			}
			else if (!_isLoadingContinue)
			{
				_isLoadingContinue = true;
				LoadContinueGame();
			}
		}
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
	}

	private void ShowMenuEntries()
	{
		base.MenuEntries.Add(_yesEntry);
		base.MenuEntries.Add(_noEntry);
		_areEntriesAvailable = true;
		_doesUseCursor = true;
	}

	private void OnYesEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		_continueSave = base.ScreenManager.SaveFileManager.GetFreshSaveFromOld(_gameSave);
		if (_continueSave != null)
		{
			_isContinuing = true;
			base.IsMenuDisabled = true;
		}
		else
		{
			LoadingScreen.Load(base.ScreenManager, false, null, new TitleBackgroundScreen(shouldDoFullIntro: true));
		}
	}

	private void OnNoEntrySelected(object sender, PlayerIndexEventArgs e)
	{
		LoadingScreen.Load(base.ScreenManager, false, null, new TitleBackgroundScreen(shouldDoFullIntro: true));
	}

	protected override void OnCancel(PlayerIndex playerIndex)
	{
		if (_areEntriesAvailable)
		{
			SetMenuSelectedIndex(1);
		}
	}

	private void LoadContinueGame()
	{
		_reloadSaveAction(_continueSave);
		ExitScreen();
	}

	public override void Draw(GameTime gameTime)
	{
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
		if (_areEntriesAvailable && _continueFadePercentage < 1f)
		{
			float num = 1f - base.TransitionReverse;
			DrawingEx.DrawString(spriteBatch, _font, _continueText, Vector2.Add(_continueTextPosition, new Vector2(0f, _zoom)), MenuScreen.TitleShadowColor * num, Vector2.Zero, _zoom);
			DrawingEx.DrawString(spriteBatch, _font, _continueText, _continueTextPosition, MenuScreen.TitleBaseColor * num, Vector2.Zero, _zoom);
		}
		spriteBatch.End();
		base.Draw(gameTime);
		if (_isContinuing && _continueFadePercentage > 0f && !_isLoadingContinue)
		{
			base.ScreenManager.FadeBackBufferToBlack((int)(255f * _continueFadePercentage));
		}
	}
}
