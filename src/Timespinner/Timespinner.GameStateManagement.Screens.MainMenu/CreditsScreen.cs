using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameStateManagement.ScreenManager;
using Timespinner.GameStateManagement.Screens.BaseClasses;
using Timespinner.GameStateManagement.Screens.MainMenu.Credits;

namespace Timespinner.GameStateManagement.Screens.MainMenu;

internal class CreditsScreen : GameScreen
{
	private const int MinCameraY = -180;

	private const float DefaultScrollSpeed = 30f;

	private const float TimeToWaitAfterFinishedScrolling = 2f;

	private static readonly Color ClearColor = new Color(8, 8, 8);

	private readonly bool _isAtEndOfGame;

	private readonly ScrollThrottle _scrollThrottle;

	private readonly GameConfigSave _configSave;

	private readonly List<CreditsSection> _creditsList = new List<CreditsSection>();

	private bool _isGoingBackwards;

	private bool _isPlayerControlling;

	private EBGM _lastSongPlaying;

	private int _zoom;

	private int _farthestBottomY;

	private int _unzoomedScreenHeight;

	private float _cameraY;

	private float _playerScroll;

	private float _endTimer;

	private Rectangle _mainBarSource;

	private Rectangle _subBarSource;

	private SpriteFont _primaryFont;

	private SpriteFont _latinFont;

	private SpriteSheet _sprite;

	private SpriteSheet _toolsSprite;

	public CreditsScreen(bool isAtEndOfGame, GameConfigSave configSave)
	{
		_configSave = configSave;
		_isAtEndOfGame = isAtEndOfGame;
		base.TransitionOnTime = TimeSpan.FromSeconds(1.0);
		base.TransitionOffTime = TimeSpan.FromSeconds(0.25);
		_scrollThrottle = new ScrollThrottle();
		_cameraY = -180f;
	}

	public override void LoadContent()
	{
		_primaryFont = base.ScreenManager.MenuFont;
		_latinFont = base.ScreenManager.GCM.LatinFont;
		_sprite = base.ScreenManager.GCM.SpPauseMenu;
		_toolsSprite = base.ScreenManager.GCM.SpToolsLogo;
		_mainBarSource = _sprite.GetFrameSource(147);
		_subBarSource = _sprite.GetFrameSource(148);
		Jukebox jukebox = base.ScreenManager.Jukebox;
		_lastSongPlaying = jukebox.CurrentSongEnum;
		jukebox.PlaySong(EBGM.Credits, shouldForceRestart: false, shouldImmediatelyStopPreviousSong: false);
		RefreshSizes();
		FillCredits();
	}

	private void RefreshSizes()
	{
		_zoom = Constants.InGameZoom;
		_unzoomedScreenHeight = base.ScreenManager.ViewPortArea.Height / _zoom;
	}

	internal override void OnScreenResize()
	{
		base.OnScreenResize();
		RefreshSizes();
	}

	public override void ExitScreen()
	{
		if (_isAtEndOfGame)
		{
			base.ScreenManager.Jukebox.FadeOutSong(1f);
			if (_configSave != null && !_configSave.HasGameBeenCleared)
			{
				MessageBoxScreen screen = new MessageBoxScreen(Loc.Get("NewGamePlusUnlocked"), shouldIncludeUsageText: false, base.ScreenManager.MenuControllerMapping);
				base.ScreenManager.AddScreen(screen, base.ControllingPlayer);
				_configSave.HasGameBeenCleared = true;
				base.ScreenManager.SaveFileManager.RequestGameConfigSave();
			}
		}
		else
		{
			base.ScreenManager.Jukebox.PlaySong(_lastSongPlaying, shouldForceRestart: true, shouldImmediatelyStopPreviousSong: false);
		}
		base.ExitScreen();
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
		if (!base.IsActive)
		{
			return;
		}
		float num = (float)gameTime.ElapsedGameTime.TotalSeconds;
		_cameraY += num * (30f + _playerScroll * (float)((!_isGoingBackwards) ? 1 : (-1)));
		if (_cameraY < -180f)
		{
			_cameraY = -180f;
		}
		if (_cameraY >= (float)_farthestBottomY)
		{
			_endTimer += num;
			_cameraY = _farthestBottomY;
			_playerScroll = 0f;
			if (_endTimer > 2f)
			{
				ExitScreen();
			}
		}
		else
		{
			_endTimer = 0f;
		}
	}

	public override void HandleInput(InputState input)
	{
		if (!base.ControllingPlayer.HasValue || input == null)
		{
			return;
		}
		if (input.IsNewPressCancel(base.ControllingPlayer) || input.IsNewPressPause(base.ControllingPlayer) || input.IsNewPressCutsceneSkip(base.ControllingPlayer))
		{
			ExitScreen();
			return;
		}
		_isPlayerControlling = false;
		if (input.IsPressMenuUp(base.ControllingPlayer))
		{
			if (!_isGoingBackwards)
			{
				_playerScroll = 30f;
				_scrollThrottle.Reset();
			}
			_isGoingBackwards = true;
			_isPlayerControlling = true;
		}
		else
		{
			if (input.IsPressMenuDown(base.ControllingPlayer) || input.IsPressConfirm(base.ControllingPlayer))
			{
				if (_isGoingBackwards)
				{
					_playerScroll = 0f;
					_scrollThrottle.Reset();
				}
				_isPlayerControlling = true;
			}
			_isGoingBackwards = false;
		}
		if (_isPlayerControlling && _scrollThrottle.IsScrollReady())
		{
			_playerScroll += 50f;
		}
		else if (!_isPlayerControlling)
		{
			_playerScroll = 0f;
		}
	}

	public override void Draw(GameTime gameTime)
	{
		base.ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, ClearColor, 0f, 0);
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
		float cameraY = _cameraY;
		float num = cameraY + (float)_unzoomedScreenHeight;
		int centerX = (int)base.ScreenManager.ScreenCenter.X;
		foreach (CreditsSection credits in _creditsList)
		{
			if ((float)credits.BottomY > cameraY && (float)credits.TopY < num)
			{
				credits.Draw(spriteBatch, _primaryFont, _latinFont, _cameraY, centerX, _zoom, _sprite, _mainBarSource, _subBarSource, _unzoomedScreenHeight);
			}
		}
		spriteBatch.End();
		if (base.TransitionOffPercentage > 0f)
		{
			base.ScreenManager.FadeBackBufferToBlack(255 - base.TransitionAlpha);
		}
	}

	private void FillCredits()
	{
		CreditsSection creditsSection = new CreditsSection(Loc.Get("credits_creator"), Loc.Get("credits_creator_contributions"));
		creditsSection.AddContributor("Bodie Lee");
		_creditsList.Add(creditsSection);
		CreditsSection creditsSection2 = new CreditsSection(Loc.Get("credits_music"), "Jeff Ball - Composer");
		creditsSection2.AddContributor("Kristin Naigus - Woodwinds");
		creditsSection2.AddContributor("Cain German - Guitars");
		creditsSection2.AddContributor("Christian Pacaud - Bass");
		_creditsList.Add(creditsSection2);
		CreditsSection creditsSection3 = new CreditsSection(Loc.Get("credits_sound"), "Power Up Audio");
		creditsSection3.AddContributor("Jeff Tangsoc");
		creditsSection3.AddContributor("Kevin Regamey");
		creditsSection3.AddContributor("Cole Verderber");
		creditsSection3.AddContributor("Joey Godard");
		_creditsList.Add(creditsSection3);
		CreditsSection item = new CreditsSection(Loc.Get("credits_publisher"), "Chucklefish");
		_creditsList.Add(item);
		CreditsSection item2 = new CreditsSection(Loc.Get("credits_character"), "Luciana Nascimento");
		_creditsList.Add(item2);
		CreditsSection creditsSection4 = new CreditsSection(Loc.Get("credits_narrative"), "Silverstring Media");
		creditsSection4.AddContributor("Lucas J.W. Johnson");
		creditsSection4.AddContributor("Claris Cyarron");
		_creditsList.Add(creditsSection4);
		CreditsSection item3 = new CreditsSection(Loc.Get("credits_writing"), "Aubrey Quinton");
		_creditsList.Add(item3);
		CreditsSection item4 = new CreditsSection("Localization", "Riotloc");
		_creditsList.Add(item4);
		CreditsSection creditsSection5 = new CreditsSection("Localization Coordination");
		creditsSection5.AddContributor("Anna Kiseleva");
		creditsSection5.AddContributor("Vladimir Konoplitsky");
		_creditsList.Add(creditsSection5);
		CreditsSection creditsSection6 = new CreditsSection("French");
		creditsSection6.AddContributor("Nadège Gayon-Debonnet");
		creditsSection6.AddContributor("Ophélie Colin");
		_creditsList.Add(creditsSection6);
		CreditsSection creditsSection7 = new CreditsSection("German");
		creditsSection7.AddContributor("Roland Strube");
		creditsSection7.AddContributor("Gesine Preusser");
		creditsSection7.AddContributor("Tilman Löffler");
		_creditsList.Add(creditsSection7);
		CreditsSection creditsSection8 = new CreditsSection("Spanish");
		creditsSection8.AddContributor("Ramón Méndez");
		creditsSection8.AddContributor("David Martínez");
		creditsSection8.AddContributor("Alba Calvo");
		_creditsList.Add(creditsSection8);
		CreditsSection creditsSection9 = new CreditsSection("Brazilian Portuguese");
		creditsSection9.AddContributor("Bruno Dias");
		creditsSection9.AddContributor("Janilson Torres");
		creditsSection9.AddContributor("Luis Peralta");
		_creditsList.Add(creditsSection9);
		CreditsSection creditsSection10 = new CreditsSection("Russian");
		creditsSection10.AddContributor("Natalia Baulina");
		creditsSection10.AddContributor("Katerina Florinskaya");
		creditsSection10.AddContributor("Anna Kiseleva");
		_creditsList.Add(creditsSection10);
		CreditsSection creditsSection11 = new CreditsSection("Simplified Chinese");
		creditsSection11.AddContributor("Kage Chen");
		creditsSection11.AddContributor("Shang Ke");
		creditsSection11.AddContributor("Meiyan Rita Huang");
		_creditsList.Add(creditsSection11);
		CreditsSection creditsSection12 = new CreditsSection("Japanese");
		creditsSection12.AddContributor("Tomoko Kono");
		creditsSection12.AddContributor("Yumi Niitani");
		creditsSection12.AddContributor("Yuta Kurosawa");
		_creditsList.Add(creditsSection12);
		CreditsSection creditsSection13 = new CreditsSection(Loc.Get("credits_porting"));
		creditsSection13.AddContributor("Sickhead Games, LLC");
		creditsSection13.AddContributor("Ethan Lee");
		creditsSection13.LogoSpriteSheet = _toolsSprite;
		creditsSection13.AddLogo(_toolsSprite.GetFrameSource(0));
		_creditsList.Add(creditsSection13);
		CreditsSection item5 = new CreditsSection(Loc.Get("credits_production"), "Nicolle Rodgers");
		_creditsList.Add(item5);
		CreditsSection creditsSection14 = new CreditsSection(Loc.Get("credits_voice"));
		creditsSection14.AddContributor("Elspeth Eastman - Lunais");
		creditsSection14.AddContributor("Jacob Burgess - Emperor Nuvius");
		_creditsList.Add(creditsSection14);
		CreditsSection item6 = new CreditsSection(Loc.Get("credits_muse"), "Elizabeth Matthews");
		_creditsList.Add(item6);
		CreditsSection item7 = new CreditsSection(Loc.Get("credits_ks_header"));
		_creditsList.Add(item7);
		CreditsSection creditsSection15 = new CreditsSection(Loc.Get("credits_ks_boss"));
		CreditsBossBackers creditsBossBackers = new CreditsBossBackers();
		creditsSection15.AddContributors(creditsBossBackers.Backers);
		_creditsList.Add(creditsSection15);
		CreditsSection creditsSection16 = new CreditsSection(Loc.Get("credits_ks_orb"));
		CreditsOrbBackers creditsOrbBackers = new CreditsOrbBackers();
		creditsSection16.AddContributors(creditsOrbBackers.Backers);
		_creditsList.Add(creditsSection16);
		CreditsSection creditsSection17 = new CreditsSection(Loc.Get("credits_ks_enemy"));
		CreditsEnemyBackers creditsEnemyBackers = new CreditsEnemyBackers();
		creditsSection17.AddContributors(creditsEnemyBackers.Backers);
		_creditsList.Add(creditsSection17);
		CreditsSection creditsSection18 = new CreditsSection(Loc.Get("credits_ks_alpha"));
		CreditsAlphaBackers creditsAlphaBackers = new CreditsAlphaBackers();
		creditsSection18.AddContributors(creditsAlphaBackers.Backers);
		_creditsList.Add(creditsSection18);
		CreditsSection creditsSection19 = new CreditsSection(Loc.Get("credits_ks_backers"));
		CreditsBasicBackers creditsBasicBackers = new CreditsBasicBackers();
		creditsSection19.AddContributors(creditsBasicBackers.Backers);
		_creditsList.Add(creditsSection19);
		int num = 0;
		int lineSpacing = _primaryFont.LineSpacing;
		int lineSpacing2 = _latinFont.LineSpacing;
		int num2 = lineSpacing * 4;
		foreach (CreditsSection credits in _creditsList)
		{
			credits.CalculateSize(num, lineSpacing, lineSpacing2, _zoom);
			num += credits.Height + num2;
		}
		_farthestBottomY = num;
	}
}
