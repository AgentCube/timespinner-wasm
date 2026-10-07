using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.GameAbstractions;
using Timespinner.GameStateManagement.ScreenManager;
using Timespinner.GameStateManagement.Screens.InGame;

namespace Timespinner.GameStateManagement.Screens.MainMenu;

internal class TitleBackgroundScreen : GameScreen
{
	private const int SmallScreenRenderWidth = 512;

	private const int SmallScreenRenderHeight = 256;

	private const int Anim_StoneIndex = 0;

	private const int Anim_PortalIndex = 1;

	private const int Anim_TitleLeftIndex = 2;

	private const int Anim_StoneGlowIndex = 3;

	private const int Anim_BlankSquareIndex = 4;

	private const int Anim_SideFrameIndex = 6;

	private const int Anim_BottomFrameIndex = 5;

	private const int Anim_LeftOrbFrameIndex = 8;

	private const int Anim_RightOrbFrameIndex = 7;

	private const int Anim_PreGlowSideIndex = 9;

	private const int Anim_PreGlowTopIndex = 10;

	private const int PortalFrameWidth = 128;

	private const int PortalFrameHeight = 192;

	private const int HalfPortalFrameWidth = 64;

	private const int PortalDrawOffsetX = -64;

	private const int PortalDrawOffsetY = 16;

	private const int TitleDrawOffsetX = -104;

	private const int TitleDrawOffsetY = 74;

	private const int LeftSideFrameOffsetX = -142;

	private const int RightSideFrameOffsetX = 101;

	private const int SideFrameOffsetY = 1;

	private const int LeftBottomFrameOffsetX = -102;

	private const int RightBottomFrameOffsetX = 22;

	private const int BottomFrameOffsetY = 34;

	private const int LeftOrbOffsetX = 22;

	private const int RightOrbOffsetX = 8;

	private const int OrbOffsetY = 21;

	private const int LeftStoneGlowOffsetX = -80;

	private const int PreGlowSideWidth = 32;

	private const int PreGlowSideHeight = 112;

	private const int LeftPreGlowSideOffsetX = -61;

	private const int RightPreGlowSideOffsetX = 29;

	private const int PreGlowSideOffsetY = 96;

	private const int LeftPreGlowTopOffsetX = -61;

	private const int RightPreGlowTopOffsetX = 0;

	private const int PreGlowTopOffsetY = 16;

	private const int SmallScreenRenderCenterX = 256;

	private const int ScreenBorderHeight = 270;

	private const int HalfScreenBorderHeight = 135;

	private const int VerticalFlashStartWidth = 1;

	private const int VerticalFlashEndWidth = 200;

	private const int VerticalFlashOffsetStartX = 0;

	private const int VerticalFlashOffsetEndX = 200;

	private const int StoneGlowColorDarkestA = 12;

	private const int StoneGlowColorBrightestA = 32;

	private const int BlackFillerR = 8;

	private const int BlackFillerG = 8;

	private const int BlackFillerB = 16;

	private const int StoneGlowColorR = 24;

	private const int StoneGlowColorG = 32;

	private const int StoneGlowColorB = 48;

	private const float StoneGlowFrequency = 10f;

	private const int CameraOffsetEndY = 0;

	private const float TimeForScreenFade = 1f;

	private const float DistortionFrequency = 0.015f;

	private const int CameraOffsetStartY = 240;

	private const float TimeForCameraRise = 2.5f;

	private const float ScreenFlashBaseColorR = 0.35f;

	private const float ScreenFlashBaseColorG = 0.45f;

	private const float ScreenFlashBaseColorB = 0.65f;

	private const float ScreenFlashBaseColorA = 0.15f;

	private const float TimeBeforeScreenFlash = 1.5f;

	private const float TimeForScreenFlashIn = 1f;

	private const float TimeForScreenFlashStay = 0f;

	private const float TimeForScreenFlashOut = 0.5f;

	private const float TimeBeforeScreenFlashStay = 2.5f;

	private const float TimeBeforeScreenFlashOut = 2.5f;

	private const float TimeForEntireScreenFlash = 3f;

	private const float TimeAfterScreenFlashBeforeAddingTitleScreen = 1f;

	private const float TimeBeforeAddingTitleScreen = 4f;

	private const float TimeBeforePlayingHarp = 1f;

	private const float TimeBeforePlayingSong = 3f;

	private const float TimeForHarpToFade = 0.25f;

	private static readonly Color PreGlowBaseDrawColor = new Color(32, 24, 32, 16);

	private static readonly Color BlankPortalStartColor = new Color(32, 24, 32);

	private static readonly Color BlankPortalEndColor = new Color(128, 116, 128);

	private static readonly Color BlankPortalFlashColor = new Color(128, 116, 128);

	private static readonly Color PreStoneGlowEndColor = new Color(64, 56, 64, 32);

	private static readonly Color VerticalFlashStartColor = new Color(16, 24, 32, 12);

	private readonly bool _shouldDoFullIntro;

	private readonly Point[] _titleShadowOffsets = new Point[4]
	{
		new Point(0, 1),
		new Point(0, -1),
		new Point(1, 0),
		new Point(-1, 0)
	};

	private bool _isDrawingPortal;

	private bool _isDrawingScreenFlash;

	private bool _isDrawingTitle;

	private bool _hasAddedTitleScreen;

	private bool _hasSetShaderTexture;

	private bool _isDrawingVerticalFlash;

	private bool _hasPlayedSong;

	private bool _isFadingHarp;

	private bool _hasStartedPassiveSaveLoad;

	private bool? _isSaveAvailable;

	private EGameResolutionType _lastScreenResolution;

	private int _zoom;

	private int _cameraOffsetY;

	private int _gameScreenTop;

	private int _letterBoxWidth;

	private int _letterBoxHeight;

	private int _rightLetterBoxX;

	private int _verticalFlashWidth;

	private int _verticalFlashOffsetX;

	private float _cameraRiseTimer;

	private float _screenFlashTimer;

	private float _screenFlashDrawPercentage;

	private float _trigOffsetX;

	private float _trigOffsetY;

	private float _trigTimer;

	private float _stoneGlowTimer;

	private float _titleDrawPercentage;

	private float _harpFadeTimer;

	private Point _screenCenter;

	private Vector2 _titleDrawPosition;

	private Vector2 _portalDrawPosition;

	private Vector2 _leftStoneDrawPosition;

	private Vector2 _rightStoneDrawPosition;

	private Vector2 _leftStoneGlowDrawPosition;

	private Vector2 _rightStoneGlowDrawPosition;

	private Vector2 _leftPreGlowTopDrawPosition;

	private Vector2 _rightPreGlowTopDrawPosition;

	private Vector2 _leftSideFrameDrawPosition;

	private Vector2 _leftBottomFrameDrawPosition;

	private Vector2 _rightSideFrameDrawPosition;

	private Vector2 _rightBottomFrameDrawPosition;

	private Vector2 _leftOrbDrawPosition;

	private Vector2 _rightOrbDrawPosition;

	private Vector2 _backgroundDrawPosition;

	private Color _screenFlashDrawColor;

	private Color _blankPortalDrawColor;

	private Color _stoneGlowColor;

	private Color _verticalFlashDrawColor;

	private Rectangle _smallScreenRect;

	private Rectangle _blankPortalDrawRectangle;

	private Rectangle _leftPreGlowSideDrawRectangle;

	private Rectangle _rightPreGlowSideDrawRectangle;

	private Rectangle _leftVerticalFlashDrawRectangle;

	private Rectangle _rightVerticalFlashDrawRectangle;

	private Rectangle _screenFlashDrawRectangle;

	private Rectangle _stoneFrameSource;

	private Rectangle _stoneGlowFrameSource;

	private Rectangle _preGlowSideFrameSource;

	private Rectangle _preGlowTopFrameSource;

	private Rectangle _portalFrameSource;

	private Rectangle _titleFrameSource;

	private Rectangle _blankSquareFrameSource;

	private Rectangle _sideFrameFrameSource;

	private Rectangle _bottomFrameFrameSrouce;

	private Rectangle _leftOrbFrameSource;

	private Rectangle _rightOrbFrameSource;

	private ContentManager _content;

	private SpriteSheet _sprite;

	private Texture2D _distortionTexture;

	private Effect _deformEffect;

	private SoundEffect _introHarpSFX;

	private SoundEffectInstance _introHarpCueInstance;

	private RenderTarget2D _smallScreenRenderTarget;

	private Task _saveCheckTask;

	public TitleBackgroundScreen(bool shouldDoFullIntro)
	{
		_shouldDoFullIntro = shouldDoFullIntro;
		base.TransitionOnTime = TimeSpan.FromSeconds(1.0);
		base.TransitionOffTime = TimeSpan.FromSeconds(1.0);
		_stoneGlowColor = Color.Transparent;
	}

	public override void LoadContent()
	{
		if (_content == null)
		{
			_content = new ContentManager(base.ScreenManager.Game.Services, "Content");
		}
		GCM gCM = base.ScreenManager.GCM;
		_sprite = gCM.GetTextureAtlas("Overlays/Title/TitleScreen", _content);
		_stoneFrameSource = _sprite.GetFrameSource(0);
		_stoneGlowFrameSource = _sprite.GetFrameSource(3);
		_preGlowSideFrameSource = _sprite.GetFrameSource(9);
		_preGlowTopFrameSource = _sprite.GetFrameSource(10);
		_portalFrameSource = _sprite.GetFrameSource(1);
		_titleFrameSource = _sprite.GetFrameSource(2);
		_blankSquareFrameSource = _sprite.GetFrameSource(4);
		_sideFrameFrameSource = _sprite.GetFrameSource(6);
		_bottomFrameFrameSrouce = _sprite.GetFrameSource(5);
		_leftOrbFrameSource = _sprite.GetFrameSource(8);
		_rightOrbFrameSource = _sprite.GetFrameSource(7);
		_deformEffect = _content.Load<Effect>("Effects/PortalDraw");
		_distortionTexture = _content.Load<Texture2D>("Animations/DifferenceCloud");
		_introHarpSFX = _content.Load<SoundEffect>("Audio/SFX/Other/sfx_title_intro");
		GraphicsDevice graphicsDevice = base.ScreenManager.GraphicsDevice;
		PresentationParameters presentationParameters = graphicsDevice.PresentationParameters;
		_smallScreenRenderTarget = new RenderTarget2D(graphicsDevice, 512, 256, mipMap: true, presentationParameters.BackBufferFormat, presentationParameters.DepthStencilFormat, 0, RenderTargetUsage.PreserveContents);
		RefreshSizes();
		if (OperatingSystem.IsBrowser())
		{
			base.ScreenManager.SaveFileManager.PassiveCheckForGameSaveFile();
		}
		else
		{
			_saveCheckTask = new Task(base.ScreenManager.SaveFileManager.PassiveCheckForGameSaveFile);
			_saveCheckTask.Start();
		}
		_hasStartedPassiveSaveLoad = true;
		if (_shouldDoFullIntro)
		{
			base.ScreenManager.Jukebox.StopSong();
		}
		GC.Collect();
	}

	internal override void OnScreenResize()
	{
		base.OnScreenResize();
		RefreshSizes();
	}

	private void RefreshSizes()
	{
		_zoom = Constants.InGameZoom;
		_lastScreenResolution = base.ScreenManager.CurrentResolution;
		RefreshDrawPositions();
	}

	public override void UnloadContent()
	{
		if (_introHarpCueInstance != null && !_introHarpCueInstance.IsDisposed && _introHarpCueInstance.State != SoundState.Stopped)
		{
			_introHarpCueInstance.Stop();
			_introHarpCueInstance.Dispose();
		}
		if (_content != null)
		{
			_content.Unload();
		}
		if (_smallScreenRenderTarget != null && !_smallScreenRenderTarget.IsDisposed)
		{
			_smallScreenRenderTarget.Dispose();
		}
	}

	public override void HandleInput(InputState input)
	{
		if (!_hasAddedTitleScreen)
		{
			if (!_shouldDoFullIntro)
			{
				Skip();
			}
			else if (input.IsNewPressFinished(null))
			{
				Skip();
			}
		}
		base.HandleInput(input);
	}

	private void Skip()
	{
		_cameraRiseTimer = 100f;
		_screenFlashTimer = 100f;
		_cameraOffsetY = 0;
		_isDrawingPortal = true;
		_isDrawingScreenFlash = false;
		_isDrawingVerticalFlash = false;
		_blankPortalDrawColor = Color.Transparent;
		_isDrawingTitle = true;
		_titleDrawPercentage = 1f;
		RefreshDrawPositions();
		AddTitleScreen();
		if (_introHarpCueInstance != null && !_introHarpCueInstance.IsDisposed && _introHarpCueInstance.State == SoundState.Playing)
		{
			_isFadingHarp = true;
		}
	}

	private void AddTitleScreen()
	{
		if (!_hasAddedTitleScreen)
		{
			_isSaveAvailable = null;
			if (OperatingSystem.IsBrowser())
			{
				_isSaveAvailable = base.ScreenManager.SaveFileManager.WasSaveAvailable;
			}
			else if (_saveCheckTask != null && _saveCheckTask.IsCompleted)
			{
				_isSaveAvailable = base.ScreenManager.SaveFileManager.WasSaveAvailable;
			}
			base.ScreenManager.AddScreen(new TitleScreen(_isSaveAvailable), null);
		}
		_hasAddedTitleScreen = true;
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		base.Update(gameTime, otherScreenHasFocus, isCoveredByOtherScreen: false);
		float num = (float)gameTime.ElapsedGameTime.TotalSeconds;
		UpdateIntro(num);
		if (_isDrawingPortal)
		{
			UpdateShader(num);
			UpdateStoneGlow(num);
		}
		if (_isFadingHarp && _introHarpCueInstance != null && !_introHarpCueInstance.IsDisposed && _introHarpCueInstance.State == SoundState.Playing)
		{
			_harpFadeTimer += num;
			if (_harpFadeTimer < 0.25f)
			{
				float num2 = _harpFadeTimer / 0.25f;
				_introHarpCueInstance.Volume = (1f - num2) * base.ScreenManager.Jukebox.MusicVolume;
			}
			else
			{
				_introHarpCueInstance.Stop();
				_introHarpCueInstance.Dispose();
			}
		}
	}

	private void UpdateIntro(float delta)
	{
		if (_cameraRiseTimer < 2.5f)
		{
			_cameraRiseTimer += delta;
			float percentage = 1f;
			if (_cameraRiseTimer < 2.5f)
			{
				percentage = _cameraRiseTimer / 2.5f;
			}
			_cameraOffsetY = (int)Math.Floor(MathEx.SineInterpolate(240f, 0f, percentage));
			RefreshDrawPositions();
		}
		if (_screenFlashTimer < 3f)
		{
			float screenFlashTimer = _screenFlashTimer;
			_screenFlashTimer += delta;
			if (_screenFlashTimer >= 1f && screenFlashTimer < 1f)
			{
				_introHarpCueInstance = _introHarpSFX.CreateInstance();
				if (_introHarpCueInstance != null)
				{
					base.ScreenManager.Jukebox.UnfadeMusicVolume();
					_introHarpCueInstance.Volume = base.ScreenManager.Jukebox.MusicVolume;
					_introHarpCueInstance.Play();
				}
			}
			if (_screenFlashTimer < 3f)
			{
				if (_screenFlashTimer < 2.5f)
				{
					float amount = _screenFlashTimer / 2.5f;
					_blankPortalDrawColor = BlankPortalStartColor.CosInterpolate(BlankPortalEndColor, amount);
					_stoneGlowColor = Color.Transparent.Lerp(PreStoneGlowEndColor, amount);
				}
				if (_screenFlashTimer >= 1.5f)
				{
					if (_screenFlashTimer < 2.5f)
					{
						_isDrawingScreenFlash = true;
						float num = MathEx.CosInterpolate(1f, 0f, (_screenFlashTimer - 1.5f) / 1f);
						_screenFlashDrawPercentage = (float)Math.Cos((float)Math.PI / 2f * num);
					}
					else if (_screenFlashTimer < 2.5f)
					{
						_screenFlashDrawPercentage = 1f;
						_isDrawingPortal = true;
					}
					else
					{
						_isDrawingPortal = true;
						float num2 = (_screenFlashTimer - 2.5f) / 0.5f;
						_screenFlashDrawPercentage = (float)Math.Cos((float)Math.PI / 2f * num2);
						float num3 = 1f - _screenFlashDrawPercentage;
						_blankPortalDrawColor = BlankPortalFlashColor.Lerp(Color.Transparent, num3);
						_isDrawingTitle = true;
						_titleDrawPercentage = num3;
						float num4 = num2 * num2 * 2f;
						if (num4 <= 1f)
						{
							_isDrawingVerticalFlash = true;
							_verticalFlashWidth = (int)Math.Ceiling(MathEx.SineInterpolate(1f, 200f, num4) * (float)_zoom);
							_verticalFlashOffsetX = (int)Math.Ceiling(MathEx.SineInterpolate(0f, 200f, num4) * (float)_zoom);
							_verticalFlashDrawColor = VerticalFlashStartColor * (1f - num4);
							_leftVerticalFlashDrawRectangle = new Rectangle(-_verticalFlashOffsetX + _screenCenter.X, 0, _verticalFlashWidth, _screenFlashDrawRectangle.Height);
							_rightVerticalFlashDrawRectangle = new Rectangle(_verticalFlashOffsetX - _verticalFlashWidth + _screenCenter.X, 0, _verticalFlashWidth, _screenFlashDrawRectangle.Height);
						}
						else
						{
							_isDrawingVerticalFlash = false;
						}
					}
					_screenFlashDrawColor = new Color(0.35f * _screenFlashDrawPercentage, 0.45f * _screenFlashDrawPercentage, 0.65f * _screenFlashDrawPercentage, 0.15f * _screenFlashDrawPercentage);
				}
			}
			else
			{
				_isDrawingScreenFlash = false;
				_isDrawingVerticalFlash = false;
				_isDrawingTitle = true;
				_titleDrawPercentage = 1f;
			}
		}
		else if (_screenFlashTimer < 4f)
		{
			_screenFlashTimer += delta;
			if (_screenFlashTimer >= 4f)
			{
				AddTitleScreen();
			}
		}
		if (!_hasPlayedSong && _screenFlashTimer >= 3f)
		{
			_hasPlayedSong = true;
			base.ScreenManager.Jukebox.PlaySong(EBGM.TitleScreen, shouldForceRestart: false, shouldImmediatelyStopPreviousSong: false);
		}
	}

	private void RefreshDrawPositions()
	{
		int x = GameplayScreen.SmallScreenSize.X;
		int y = GameplayScreen.SmallScreenSize.Y;
		_smallScreenRect = new Rectangle((512 - x) / 2, (256 - y) / 2, x, y);
		Point center = _smallScreenRect.Center;
		int width = base.ScreenManager.GraphicsDevice.Viewport.Width;
		int height = base.ScreenManager.GraphicsDevice.Viewport.Height;
		_screenCenter = new Point(width / 2, height / 2);
		int num = Math.Max(0, _screenCenter.Y - 135 * _zoom);
		_letterBoxWidth = (width - _smallScreenRect.Width * _zoom) / 2;
		_letterBoxHeight = height;
		_rightLetterBoxX = width - _letterBoxWidth;
		_backgroundDrawPosition = new Vector2(-_smallScreenRect.Left * _zoom + _letterBoxWidth, num - _smallScreenRect.Top * _zoom);
		_screenFlashDrawRectangle = new Rectangle(0, 0, width, height);
		int num2 = _smallScreenRect.Top - _cameraOffsetY;
		_portalDrawPosition = new Vector2(center.X + -64, num2 + 16);
		_blankPortalDrawRectangle = new Rectangle((int)_portalDrawPosition.X, (int)_portalDrawPosition.Y, 128, 192);
		_leftStoneDrawPosition = new Vector2(_smallScreenRect.Left, num2);
		_rightStoneDrawPosition = new Vector2(center.X, num2);
		_leftStoneGlowDrawPosition = new Vector2(_rightStoneDrawPosition.X + -80f, _rightStoneDrawPosition.Y);
		_rightStoneGlowDrawPosition = _rightStoneDrawPosition;
		_leftPreGlowSideDrawRectangle = new Rectangle(center.X + -61, num2 + 96, 32, 112);
		_rightPreGlowSideDrawRectangle = new Rectangle(center.X + 29, num2 + 96, 32, 112);
		_leftPreGlowTopDrawPosition = new Vector2(center.X + -61, num2 + 16);
		_rightPreGlowTopDrawPosition = new Vector2(center.X, num2 + 16);
		_titleDrawPosition = new Vector2(152f, 74f);
		_leftSideFrameDrawPosition = new Vector2(114f, _titleDrawPosition.Y + 1f);
		_rightSideFrameDrawPosition = new Vector2(357f, _titleDrawPosition.Y + 1f);
		_leftBottomFrameDrawPosition = new Vector2(154f, _titleDrawPosition.Y + 34f);
		_rightBottomFrameDrawPosition = new Vector2(278f, _titleDrawPosition.Y + 34f);
		_leftOrbDrawPosition = new Vector2(_leftSideFrameDrawPosition.X + 22f, _leftSideFrameDrawPosition.Y + 21f);
		_rightOrbDrawPosition = new Vector2(_rightSideFrameDrawPosition.X + 8f, _rightSideFrameDrawPosition.Y + 21f);
	}

	private void UpdateShader(float delta)
	{
		_trigTimer += delta * 0.015f;
		if (_trigTimer > 1f)
		{
			_trigTimer -= 1f;
		}
		_trigOffsetX = _trigTimer;
		_trigOffsetY = _trigTimer;
	}

	private void UpdateStoneGlow(float delta)
	{
		_stoneGlowTimer += delta * 10f;
		if (_stoneGlowTimer >= (float)Math.PI * 2f)
		{
			_stoneGlowTimer -= (float)Math.PI * 2f;
		}
		float amount = (float)(Math.Sin(_stoneGlowTimer) + 1.0) * 0.5f;
		int alpha = (int)Math.Round(MathHelper.Lerp(12f, 32f, amount));
		_stoneGlowColor = new Color(24, 32, 48, alpha);
	}

	private void ApplySineShaderValues(float scroll, float time)
	{
		Vector2 value = new Vector2(scroll, time);
		_deformEffect.Parameters["DisplacementScroll"].SetValue(value);
	}

	public override void Draw(GameTime gameTime)
	{
		byte transitionAlpha = base.TransitionAlpha;
		float num = (float)(int)transitionAlpha / 255f;
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		base.ScreenManager.GraphicsDevice.SetRenderTarget(_smallScreenRenderTarget);
		base.ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0f, 0);
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
		DrawBackground(spriteBatch);
		spriteBatch.End();
		base.ScreenManager.GraphicsDevice.SetRenderTarget(null);
		Color color = ((transitionAlpha == byte.MaxValue) ? new Color(8, 8, 16) : new Color((int)(8f * num), (int)(8f * num), (int)(16f * num)));
		base.ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, color, 0f, 0);
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
		spriteBatch.Draw(_smallScreenRenderTarget, _backgroundDrawPosition, null, Color.White, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
		spriteBatch.Draw(_sprite.Texture, new Rectangle(0, 0, _letterBoxWidth, _letterBoxHeight), _blankSquareFrameSource, color);
		spriteBatch.Draw(_sprite.Texture, new Rectangle(_rightLetterBoxX, 0, _letterBoxWidth, _letterBoxHeight), _blankSquareFrameSource, color);
		Texture2D texture = _sprite.Texture;
		if (_isDrawingVerticalFlash)
		{
			spriteBatch.Draw(texture, _leftVerticalFlashDrawRectangle, _preGlowSideFrameSource, _verticalFlashDrawColor, 0f, Vector2.Zero, SpriteEffects.None, 0f);
			spriteBatch.Draw(texture, _rightVerticalFlashDrawRectangle, _preGlowSideFrameSource, _verticalFlashDrawColor, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
		}
		if (_isDrawingScreenFlash)
		{
			spriteBatch.Draw(texture, _screenFlashDrawRectangle, _blankSquareFrameSource, _screenFlashDrawColor);
		}
		spriteBatch.End();
	}

	private void DrawBackground(SpriteBatch spriteBatch)
	{
		byte transitionAlpha = base.TransitionAlpha;
		float num = (float)(int)transitionAlpha / 255f;
		Texture2D texture = _sprite.Texture;
		Color color = new Color(transitionAlpha, transitionAlpha, transitionAlpha);
		if (_isDrawingPortal)
		{
			spriteBatch.End();
			ApplySineShaderValues(_trigOffsetX, _trigOffsetY);
			if (!_hasSetShaderTexture)
			{
				_hasSetShaderTexture = true;
				_deformEffect.GraphicsDevice.Textures[1] = _distortionTexture;
			}
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, _deformEffect);
			spriteBatch.Draw(texture, _portalDrawPosition, _portalFrameSource, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
		}
		spriteBatch.Draw(texture, _blankPortalDrawRectangle, _blankSquareFrameSource, _blankPortalDrawColor);
		if (!_isDrawingPortal)
		{
			Color color2 = PreGlowBaseDrawColor * num;
			spriteBatch.Draw(texture, _leftPreGlowSideDrawRectangle, _preGlowSideFrameSource, color2, 0f, Vector2.Zero, SpriteEffects.None, 0f);
			spriteBatch.Draw(texture, _rightPreGlowSideDrawRectangle, _preGlowSideFrameSource, color2, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
			spriteBatch.Draw(texture, _leftPreGlowTopDrawPosition, _preGlowTopFrameSource, color2, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
			spriteBatch.Draw(texture, _rightPreGlowTopDrawPosition, _preGlowTopFrameSource, color2, 0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0f);
		}
		spriteBatch.Draw(texture, _leftStoneDrawPosition, _stoneFrameSource, color, 0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0f);
		spriteBatch.Draw(texture, _rightStoneDrawPosition, _stoneFrameSource, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		spriteBatch.Draw(texture, _leftStoneGlowDrawPosition, _stoneGlowFrameSource, _stoneGlowColor * num, 0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0f);
		spriteBatch.Draw(texture, _rightStoneGlowDrawPosition, _stoneGlowFrameSource, _stoneGlowColor * num, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		if (_isDrawingTitle)
		{
			float num2 = _titleDrawPercentage * num;
			Color color3 = new Color(16, 16, 32, 220) * num2;
			Point[] titleShadowOffsets = _titleShadowOffsets;
			for (int i = 0; i < titleShadowOffsets.Length; i++)
			{
				Point point = titleShadowOffsets[i];
				Vector2 vector = new Vector2(point.X, point.Y);
				spriteBatch.Draw(texture, _titleDrawPosition + vector, _titleFrameSource, color3, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
			}
			color3 = new Color(16, 16, 32, 160) * num2;
			Point[] titleShadowOffsets2 = _titleShadowOffsets;
			for (int j = 0; j < titleShadowOffsets2.Length; j++)
			{
				Point point2 = titleShadowOffsets2[j];
				Vector2 vector2 = new Vector2(point2.X * 2, point2.Y * 2);
				spriteBatch.Draw(texture, _titleDrawPosition + vector2, _titleFrameSource, color3, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
			}
			Color color4 = Color.White * num2;
			spriteBatch.Draw(texture, _leftSideFrameDrawPosition, _sideFrameFrameSource, color4, 0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0f);
			spriteBatch.Draw(texture, _rightSideFrameDrawPosition, _sideFrameFrameSource, color4, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
			spriteBatch.Draw(texture, _leftBottomFrameDrawPosition, _bottomFrameFrameSrouce, color4, 0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0f);
			spriteBatch.Draw(texture, _rightBottomFrameDrawPosition, _bottomFrameFrameSrouce, color4, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
			spriteBatch.Draw(texture, _leftOrbDrawPosition, _leftOrbFrameSource, color4, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
			spriteBatch.Draw(texture, _rightOrbDrawPosition, _rightOrbFrameSource, color4, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
			spriteBatch.Draw(texture, _titleDrawPosition, _titleFrameSource, color4, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		}
	}
}
