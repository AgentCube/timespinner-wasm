using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameStateManagement.ScreenManager;

namespace Timespinner.GameStateManagement.Screens.PauseMenu.Journal;

internal abstract class JournalEntryScreen : GameScreen
{
	private const int LineMarginY = 0;

	private const int DialogueBoxPadding = 15;

	internal const int TitleSafeX = 40;

	internal const int TitleSafeY = 24;

	private const float TransitionToBlackTime = 0.05f;

	private const float TransitionToMenuTime = 0.1f;

	private const float TotalTransitionTime = 0.15f;

	private const float ScreenShowThresholdPercentage = 1f / 3f;

	private const float NextLetterRateNewline = 1.25f;

	private const float NextLetterRate = 0f;

	private const float NewLineIconAnimationSpeed = 0.2f;

	private readonly int _maxLines;

	private readonly SpriteFont _font;

	private readonly SpriteSheet _journalSprite;

	private readonly GCM _gcm;

	private readonly GameConfigSave _configSave;

	private readonly List<string> _entries;

	private readonly List<DialogueLine> _dialogueLines;

	private readonly Action _fullExitAction;

	private bool _isNotInFocus;

	private bool _isReadyForNextLine;

	private bool _wasPressingAccept;

	private int _currentLastLetter;

	private int _visibleMessageLength;

	private int _newLineIconFrame;

	private int _letterBoxOffsetX;

	private int _letterBoxOffsetY;

	private int _visibleScreenWidth;

	private int _visibleScreenHeight;

	private int _scale;

	private int _lineHeight;

	private float _nextLetterTimer;

	private float _newLineIconTimer;

	private Point _displaySize;

	private Vector2 _backgroundDrawPosition;

	private Rectangle _backgroundDrawRectangle;

	internal bool DoesDelayFromNewlines { get; set; }

	internal bool DoesDelayEachLetter { get; set; }

	internal int Scale => _scale;

	internal int LineHeight => _lineHeight;

	internal int CurrentLastLetter => _currentLastLetter;

	internal int MaxLines => _maxLines;

	internal int NewLineIconIndex { get; set; }

	internal int LetterBoxOffsetX => _letterBoxOffsetX;

	internal int LetterBoxOffsetY => _letterBoxOffsetY;

	internal int VisibleScreenWidth => _visibleScreenWidth;

	internal int VisibleScreenHeight => _visibleScreenHeight;

	internal Point DisplaySize => _displaySize;

	internal Vector2 BackgroundDrawPosition => _backgroundDrawPosition;

	internal Vector2 NewlineIconDrawPosition { get; set; }

	internal GCM GCM => _gcm;

	internal GameConfigSave ConfigSave => _configSave;

	internal List<DialogueLine> DialogueLines => _dialogueLines;

	internal JournalEntryScreen(List<string> entries, SpriteFont font, GCM gcm, int scale, int dialogueWidth, int maxLines, bool doesAddNewlines, GameConfigSave configSave, Action fullExitAction)
	{
		_configSave = configSave;
		_fullExitAction = fullExitAction;
		_entries = entries;
		_font = font;
		_gcm = gcm;
		_scale = scale;
		_maxLines = maxLines;
		_journalSprite = _gcm.SpJournalMenu;
		base.DoesLeaveIfNotInFocus = false;
		base.TransitionOnTime = TimeSpan.FromSeconds(0.15000000223517418);
		base.TransitionOffTime = TimeSpan.FromSeconds(0.15000000223517418);
		NewLineIconIndex = 9;
		_nextLetterTimer = 0.15f;
		_lineHeight = font.LineSpacing * scale;
		string text = "";
		bool flag = true;
		foreach (string entry in _entries)
		{
			if (!flag && doesAddNewlines)
			{
				text += "\r\n";
			}
			text = text + "    " + entry + "\r\n";
			flag = false;
		}
		int num = dialogueWidth + 15;
		_dialogueLines = DialogueLine.SplitMessageIntoLines(text, num * _scale, _font, _gcm.SpUIButtons, _scale, configSave.MenuControllerMapping);
		UpdateVisibleMessageLength();
	}

	public override void LoadContent()
	{
		base.LoadContent();
		RefreshSizes();
	}

	internal override void OnScreenResize()
	{
		base.OnScreenResize();
		RefreshSizes();
	}

	private void RefreshSizes()
	{
		_scale = Constants.InGameZoom;
		_lineHeight = _font.LineSpacing * _scale;
		_displaySize = new Point(base.ScreenManager.GraphicsDevice.Viewport.Width, base.ScreenManager.GraphicsDevice.Viewport.Height);
		_visibleScreenWidth = 400 * Scale;
		_visibleScreenHeight = 240 * Scale;
		_letterBoxOffsetX = (_displaySize.X - _visibleScreenWidth) / 2;
		_letterBoxOffsetY = (_displaySize.Y - _visibleScreenHeight) / 2;
		_backgroundDrawPosition = new Vector2(_letterBoxOffsetX, _letterBoxOffsetY);
		_backgroundDrawRectangle = new Rectangle(_letterBoxOffsetX, _letterBoxOffsetY, _visibleScreenWidth, _visibleScreenHeight);
	}

	private void UpdateVisibleMessageLength()
	{
		_visibleMessageLength = 0;
		for (int i = 0; i < MaxLines && i < _dialogueLines.Count; i++)
		{
			_visibleMessageLength += _dialogueLines[i].Length;
		}
	}

	public override void HandleInput(InputState input)
	{
		bool flag = input.IsNewPressConfirm(base.ControllingPlayer);
		if (!_isReadyForNextLine)
		{
			if (flag)
			{
				_currentLastLetter = _visibleMessageLength;
				_isReadyForNextLine = true;
			}
		}
		else if (flag && !_wasPressingAccept)
		{
			if (_dialogueLines.Count <= MaxLines)
			{
				ExitScreen();
			}
			else
			{
				_isReadyForNextLine = false;
				_currentLastLetter = 0;
				_nextLetterTimer = 0f;
				for (int i = 0; i < MaxLines; i++)
				{
					_dialogueLines.RemoveAt(0);
				}
				if (_dialogueLines.Count > 0 && _dialogueLines[0].Length <= 1)
				{
					_dialogueLines.RemoveAt(0);
				}
				UpdateVisibleMessageLength();
			}
		}
		if (input.IsNewPressCancel(base.ControllingPlayer))
		{
			ExitScreen();
		}
		else if (input.IsNewPressExit(base.ControllingPlayer))
		{
			base.ScreenManager.Jukebox.PlayCue(ESFX.MenuCancel);
			ExitScreen();
			_fullExitAction();
		}
		base.HandleInput(input);
		_wasPressingAccept = flag;
	}

	public override void Update(GameTime gameTime, bool doesOtherScreenHasFocus, bool isCoveredByOtherScreen)
	{
		_isNotInFocus = doesOtherScreenHasFocus;
		float num = (float)gameTime.ElapsedGameTime.TotalSeconds;
		if (!_isNotInFocus)
		{
			if (!_isReadyForNextLine)
			{
				if (!DoesDelayEachLetter)
				{
					_currentLastLetter = _visibleMessageLength;
					_isReadyForNextLine = true;
				}
				else
				{
					_nextLetterTimer -= num;
					if (_nextLetterTimer <= 0f)
					{
						_currentLastLetter++;
						if (_currentLastLetter >= _visibleMessageLength)
						{
							_currentLastLetter = _visibleMessageLength;
							_isReadyForNextLine = true;
						}
						else
						{
							int num2 = _currentLastLetter;
							int num3 = 0;
							bool flag = false;
							float nextLetterTimer = 0f;
							if (DoesDelayFromNewlines)
							{
								int num4 = 0;
								int num5 = 0;
								bool flag2 = false;
								while (num2 > 0 && num3 < MaxLines && num3 < DialogueLines.Count)
								{
									DialogueLine dialogueLine = DialogueLines[num3];
									int length = dialogueLine.Length;
									num2 -= length;
									flag = length <= 1;
									if (flag2)
									{
										num4 = 0;
										num5 = 0;
									}
									if (!flag)
									{
										num4++;
										num5 += length;
									}
									num3++;
									flag2 = flag;
								}
								if (flag)
								{
									nextLetterTimer = ((num4 > 3) ? 2.5f : ((num4 > 2) ? 1.875f : ((num4 <= 1) ? ((float)num5 * 0.015f) : 1.5625f)));
								}
							}
							_nextLetterTimer = nextLetterTimer;
						}
					}
				}
			}
			else
			{
				_newLineIconTimer -= num;
				if (_newLineIconTimer < 0f)
				{
					_newLineIconTimer = 0.2f;
					_newLineIconFrame = (_newLineIconFrame + 1) % 2;
				}
			}
		}
		base.Update(gameTime, doesOtherScreenHasFocus, isCoveredByOtherScreen);
	}

	public override void Draw(GameTime gameTime)
	{
		bool flag = 1f - base.TransitionOffPercentage >= 1f / 3f;
		bool flag2 = flag;
		if (!_isNotInFocus && (base.ScreenState != 0 || _isFirstTransitionOn))
		{
			int alpha = (int)Math.Min(255f * ((1f - base.TransitionOffPercentage) * 2f), 255f);
			base.ScreenManager.FadeBackBufferToBlack(alpha);
		}
		else
		{
			flag = true;
			flag2 = false;
			base.ScreenManager.FadeBackBufferToBlack(255);
		}
		if (!flag)
		{
			return;
		}
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
		DrawBackground(spriteBatch);
		DrawText(spriteBatch);
		if (_isReadyForNextLine)
		{
			Rectangle frameSource = _journalSprite.GetFrameSource(NewLineIconIndex + _newLineIconFrame);
			spriteBatch.Draw(_journalSprite.Texture, NewlineIconDrawPosition, frameSource, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
		}
		GCM.CropScreen(spriteBatch, _backgroundDrawRectangle, _displaySize);
		spriteBatch.End();
		if (flag2)
		{
			int num = (int)MathHelper.Clamp(255f * (base.TransitionOffPercentage + 1f / 3f), 0f, 255f);
			if (base.ScreenState == EScreenState.TransitionOn || (base.ScreenState == EScreenState.TransitionOff && num != 255))
			{
				base.ScreenManager.FadeBackBufferToBlack(num);
			}
		}
		else
		{
			int alpha2 = (int)MathHelper.Clamp(255f * (base.TransitionOffPercentage + 1f / 3f), 0f, 255f);
			base.ScreenManager.FadeBackBufferToBlack(alpha2);
		}
	}

	internal virtual void DrawBackground(SpriteBatch spriteBatch)
	{
	}

	internal virtual void DrawText(SpriteBatch spriteBatch)
	{
	}
}
