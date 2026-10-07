using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameStateManagement.ScreenManager;

namespace Timespinner.GameAbstractions.HUD;

public class DialogueBox
{
	private enum EDialogBoxState
	{
		Opening,
		Displaying,
		Closing
	}

	public enum EDialogueBoxType
	{
		Default,
		Ghost,
		AutoplayGhost
	}

	public enum ECharacterPortraitType
	{
		None,
		Lunais,
		Selen,
		Neliste,
		Haristel,
		Rameda,
		Seykis,
		Eschem,
		MerchantCrow,
		Meyef,
		EschemVilete,
		EschemHealthy,
		TimeWitch,
		Marella,
		Yorne,
		Pano,
		Jiana,
		Faron,
		Elder,
		Aristocrat,
		Librarian,
		JianaShocked,
		Queen,
		Child,
		EternalMother,
		Aelana,
		Succubus,
		Incubus,
		Genza,
		FakeSelen,
		Nuvius,
		PrinceNuvius,
		Terrilis,
		Cantoran,
		EmperorNuvius,
		PlaceholderBoss,
		Cultist
	}

	private const int TextPaddingLeft = 0;

	private const int TextPaddingY = 0;

	private const int HeaderTextPaddingX = 10;

	private const int HeaderTextPaddingY = -11;

	private const int NewLineIconMarginX = -2;

	private const int NewLineIconMarginY = -5;

	private const int TextBoxHeight = 40;

	private const int LineMarginY = -3;

	private const int MaxLines = 3;

	private const int PortraitPaddingXMultiplier = 64;

	private const int PortraitHeaderPaddingXMultiplier = 40;

	private const int PortraitTextPaddingLeftMultiplier = 56;

	private const float NewLineIconAnimationSpeed = 0.2f;

	private const float DefaultNextLetterRate = 0.016f;

	private const float TimeForDialogToOpen = 0.225f;

	private const float TimeForDialogToClose = 0.2f;

	private const float TimeForPreviousPortraitToFade = 0.07f;

	private const float AutoplayNextLetterRate = 0.034f;

	private const float TimeBeforeAutoplayNewline = 4f;

	private static readonly Color TextColor = new Color(0.9f, 0.875f, 0.75f);

	private static readonly Color ShadowColor = new Color(0.3f, 0.25f, 0.1f);

	private static readonly Color SpeakerTextColor = new Color(0.4f, 0.55f, 0.95f);

	private static readonly Color SpeakerShadowColor = new Color(0.1f, 0.2f, 0.3f);

	private readonly bool _doesHaveASpeaker;

	private readonly bool _hasPortrait;

	private readonly bool _isGhostDialogue;

	private readonly bool _isAutoplay;

	private readonly EDialogueBoxType _dialogueType;

	private readonly ECharacterPortraitType _portraitType;

	private readonly float _nextLetterRate;

	private readonly string _message;

	private readonly string _speakerEnglish;

	private readonly string _speakerDisplay;

	private readonly Rectangle _portraitFrameSource;

	private readonly SpriteSheet _portraitSpriteSheet;

	private readonly SpriteFont _font;

	private readonly SFXCueInstance _ticCueInstance;

	private readonly GCM _gcm;

	private readonly Jukebox _jukebox;

	private readonly GameSave _saveFile;

	private readonly List<DialogueLine> _lines;

	private bool _isPlayingCue;

	private bool _isReadyForNextLine;

	private bool _isThereAPreviousPortrait;

	private bool _shouldDrawPreviousPortrait;

	private EDialogBoxState _state;

	private int _currentLastLetter;

	private int _visibleMessageLength;

	private int _newLineIconFrame;

	private int _lineHeight;

	private int _textHeaderOffset;

	private int _textPaddingLeft;

	private int _headerPaddingX;

	private int _headerPaddingY;

	private int _headerWidth;

	private int _newLineIconMarginX;

	private int _newLineIconMarginY;

	private int _zoom;

	private int _portraitPaddingX;

	private int _portraitHeaderPaddingX;

	private int _portraitTextPaddingLeft;

	private float _nextLetterTimer;

	private float _newLineIconTimer;

	private float _stateTimer;

	private float _previousPortraitFadeTimer;

	private float _previousPortraitFadePercentage;

	private float _autoplayNextLineTimer;

	private Vector2 _portraitDrawLocation;

	private Rectangle _titleSafeArea;

	private Rectangle _messageBoxDimensions;

	private Rectangle _previousPortraitFrameSource;

	private SpriteSheet _previousPortraitSpriteSheet;

	internal bool IsGhostDialogue => _isGhostDialogue;

	public bool DoesBlockScriptEvents { get; set; }

	public bool DoesBlockPlayerInput { get; set; }

	public bool ShouldCloseOnEnd { get; set; }

	public bool IsFinished { get; set; }

	public bool DoesHideHUD { get; private set; }

	public bool ShouldStartOpened
	{
		set
		{
			if (value)
			{
				_state = EDialogBoxState.Displaying;
			}
		}
	}

	internal ECharacterPortraitType Portrait => _portraitType;

	internal ECharacterPortraitType PreviousPortrait { get; set; }

	public string Message => _message;

	public Jukebox Jukebox => _jukebox;

	public DialogueBox(string inMessage, string inSpeaker, bool inHideUI, GCM gcm, Jukebox jukebox, GameSave saveFile, EDialogueBoxType dialogueType, ControllerMapping controllerMapping)
	{
		_gcm = gcm;
		_jukebox = jukebox;
		_font = _gcm.ActiveFont;
		_saveFile = saveFile;
		_dialogueType = dialogueType;
		_isGhostDialogue = _dialogueType != EDialogueBoxType.Default;
		_isAutoplay = _dialogueType == EDialogueBoxType.AutoplayGhost;
		_nextLetterRate = (_isAutoplay ? 0.034f : 0.016f);
		DoesHideHUD = inHideUI;
		_speakerEnglish = inSpeaker;
		_doesHaveASpeaker = !string.IsNullOrEmpty(_speakerEnglish) && !_isGhostDialogue;
		_speakerDisplay = ((Loc.CurrentLocale != 0) ? Loc.Get(_speakerEnglish) : _speakerEnglish);
		_ticCueInstance = _jukebox.CreateCue(ESFX.DialogTic, isLooped: true, shouldPlay: false, Point.Zero, null);
		_portraitType = (_doesHaveASpeaker ? PortraitFromName(_speakerEnglish, _saveFile) : ECharacterPortraitType.None);
		_hasPortrait = _portraitType != ECharacterPortraitType.None;
		_message = inMessage;
		if (_hasPortrait)
		{
			_portraitSpriteSheet = GetPortraitSpriteSheetFromPortraitType(_portraitType);
			_portraitFrameSource = FrameSourceFromPortrait(_portraitType, _portraitSpriteSheet);
		}
		RefreshSizes();
		int width = _messageBoxDimensions.Width - (_hasPortrait ? _portraitPaddingX : 0);
		_lines = DialogueLine.SplitMessageIntoLines(_message, width, _font, _gcm.SpUIButtons, _zoom, controllerMapping);
		UpdateVisibleMessageLength();
		_state = EDialogBoxState.Opening;
		ShouldCloseOnEnd = true;
		DoesBlockPlayerInput = true;
		DoesBlockScriptEvents = true;
	}

	internal void RefreshSizes()
	{
		_zoom = Constants.InGameZoom;
		_titleSafeArea = _gcm.TitleSafeArea;
		if (_titleSafeArea.Left == 0 && _titleSafeArea.Top == 0 && _titleSafeArea.Width > 320 * _zoom)
		{
			int num = (int)((float)_titleSafeArea.Width * 0.1f);
			int num2 = (int)((float)_titleSafeArea.Height * 0.1f);
			int width = _titleSafeArea.Width - num * 2;
			int height = _titleSafeArea.Height - num2 * 2;
			_titleSafeArea = new Rectangle(num, num2, width, height);
		}
		_portraitPaddingX = 64 * _zoom;
		_portraitHeaderPaddingX = 40 * _zoom;
		_portraitTextPaddingLeft = 56 * _zoom;
		_newLineIconMarginX = -2 * _zoom;
		_newLineIconMarginY = -5 * _zoom;
		_textHeaderOffset = (_doesHaveASpeaker ? (_zoom * 16 / 3) : 0);
		Point displaySize = _gcm.DisplaySize;
		int num3 = (displaySize.X - 400 * _zoom) / 2;
		int num4 = (displaySize.Y - 240 * _zoom) / 2;
		_messageBoxDimensions = new Rectangle(num3 + 32 * _zoom, num4 + 32 * _zoom, 336 * _zoom, 40 * _zoom);
		_portraitDrawLocation = _messageBoxDimensions.Location.Add(-11 * _zoom, -19 * _zoom).ToVector2();
		_headerPaddingX = 10 * _zoom + (_hasPortrait ? _portraitHeaderPaddingX : 0);
		_headerPaddingY = -11 * _zoom;
		_textPaddingLeft = (_hasPortrait ? _portraitTextPaddingLeft : 0);
		_headerWidth = (_doesHaveASpeaker ? ((int)(_font.MeasureString(_speakerDisplay).X * (float)_zoom)) : 0);
		_lineHeight = _zoom * (_font.LineSpacing + -3);
	}

	private void UpdateVisibleMessageLength()
	{
		_visibleMessageLength = 0;
		for (int i = 0; i < 3 && i < _lines.Count; i++)
		{
			_visibleMessageLength += _lines[i].Length;
		}
	}

	public void Update(float delta, InputState input, PlayerIndex? controllingPlayer)
	{
		if (_isThereAPreviousPortrait && _previousPortraitFadeTimer > 0f)
		{
			_previousPortraitFadeTimer -= delta;
			if (_previousPortraitFadeTimer <= 0f)
			{
				_shouldDrawPreviousPortrait = false;
			}
			else
			{
				_previousPortraitFadePercentage = (float)Math.Sin(1f - _previousPortraitFadeTimer / 0.07f);
			}
		}
		switch (_state)
		{
		case EDialogBoxState.Opening:
			if (_stateTimer <= 0f)
			{
				OnDialogueOpen();
			}
			_stateTimer += delta;
			if (_stateTimer >= 0.225f)
			{
				_state = EDialogBoxState.Displaying;
				_stateTimer = 0f;
			}
			break;
		case EDialogBoxState.Closing:
			if (_stateTimer <= 0f)
			{
				OnDialogueClose();
			}
			_stateTimer += delta;
			if (_stateTimer >= 0.2f)
			{
				FinishDialogue();
			}
			break;
		case EDialogBoxState.Displaying:
			if (!_isPlayingCue && !_isGhostDialogue)
			{
				if (_ticCueInstance != null)
				{
					_ticCueInstance.Play();
				}
				_isPlayingCue = true;
			}
			if (!_isReadyForNextLine)
			{
				_nextLetterTimer += delta;
				if (!_isAutoplay && input.IsNewPressConfirmCancel(controllingPlayer))
				{
					_currentLastLetter = _visibleMessageLength;
				}
				else if (_nextLetterTimer >= _nextLetterRate)
				{
					_currentLastLetter++;
					_nextLetterTimer -= _nextLetterRate;
				}
				if (_currentLastLetter >= _visibleMessageLength)
				{
					_currentLastLetter = _visibleMessageLength;
					if (_ticCueInstance != null)
					{
						_ticCueInstance.Pause();
					}
					_isReadyForNextLine = true;
				}
			}
			else
			{
				if (_isAutoplay)
				{
					_autoplayNextLineTimer += delta;
				}
				if ((_isAutoplay && _autoplayNextLineTimer >= 4f) || (!_isAutoplay && input.IsNewPressConfirmCancel(controllingPlayer)))
				{
					_autoplayNextLineTimer = 0f;
					if (_lines.Count <= 3)
					{
						if (ShouldCloseOnEnd)
						{
							_state = EDialogBoxState.Closing;
						}
						else
						{
							FinishDialogue();
						}
					}
					else
					{
						_isReadyForNextLine = false;
						_currentLastLetter = 0;
						for (int i = 0; i < 3; i++)
						{
							_lines.RemoveAt(0);
						}
						UpdateVisibleMessageLength();
						if (_ticCueInstance != null && !_isGhostDialogue)
						{
							_ticCueInstance.Resume();
						}
					}
				}
			}
			if (!_isAutoplay && _isReadyForNextLine)
			{
				_newLineIconTimer -= delta;
				if (_newLineIconTimer < 0f)
				{
					_newLineIconTimer = 0.2f;
					_newLineIconFrame = (_newLineIconFrame + 1) % 2;
				}
			}
			break;
		}
	}

	internal virtual void OnDialogueOpen()
	{
		_ = _isGhostDialogue;
	}

	internal virtual void OnDialogueClose()
	{
		_ = _isGhostDialogue;
	}

	internal void Skip()
	{
		_state = EDialogBoxState.Closing;
		_stateTimer = 0f;
	}

	public void FinishDialogue()
	{
		IsFinished = true;
		if (_ticCueInstance != null)
		{
			_ticCueInstance.Stop();
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		switch (_state)
		{
		case EDialogBoxState.Opening:
			if (!_isGhostDialogue)
			{
				float percentage = (float)Math.Sin(_stateTimer / 0.225f * ((float)Math.PI / 2f));
				DrawSmallerTextBox(spriteBatch, _messageBoxDimensions, percentage, doesShrinkBothAxes: true);
			}
			break;
		case EDialogBoxState.Closing:
		{
			float percentage = (float)Math.Cos(_stateTimer / 0.2f * ((float)Math.PI / 2f));
			if (!_isGhostDialogue)
			{
				DrawSmallerTextBox(spriteBatch, _messageBoxDimensions, percentage, doesShrinkBothAxes: false);
				break;
			}
			Vector2 vector = new Vector2(_messageBoxDimensions.X + _textPaddingLeft, _messageBoxDimensions.Y + _textHeaderOffset);
			{
				foreach (DialogueLine line in _lines)
				{
					int num4 = (_messageBoxDimensions.Width - line.Width * _zoom) / 2;
					line.DrawGhostText(spriteBatch, new Vector2(vector.X + (float)num4, vector.Y), _zoom, percentage);
					vector.Y += _lineHeight;
				}
				break;
			}
		}
		case EDialogBoxState.Displaying:
		{
			if (!_isGhostDialogue)
			{
				DrawingEx.DrawTextBox(spriteBatch, _messageBoxDimensions, Color.White, _gcm.SpTextBox);
			}
			Vector2 drawPosition = new Vector2(_messageBoxDimensions.X + _textPaddingLeft, _messageBoxDimensions.Y + _textHeaderOffset);
			int num = _currentLastLetter;
			int num2 = 0;
			while (num > 0 && num2 < 3 && num2 < _lines.Count)
			{
				DialogueLine dialogueLine = _lines[num2];
				num -= dialogueLine.Length;
				dialogueLine.VisibleCharacters = dialogueLine.Length + num;
				if (_isGhostDialogue)
				{
					int num3 = (_messageBoxDimensions.Width - dialogueLine.Width * _zoom) / 2;
					dialogueLine.DrawGhostText(spriteBatch, new Vector2(drawPosition.X + (float)num3, drawPosition.Y), _zoom, 1f);
				}
				else
				{
					dialogueLine.Draw(spriteBatch, drawPosition, TextColor, ShadowColor, _zoom, 1f);
				}
				drawPosition.Y += _lineHeight;
				num2++;
			}
			if (_doesHaveASpeaker)
			{
				DrawingEx.DrawTextBoxHeader(spriteBatch, _messageBoxDimensions, _headerWidth, Color.White, _gcm.SpTextBox, _hasPortrait);
				drawPosition = new Vector2(_messageBoxDimensions.X + _headerPaddingX, _messageBoxDimensions.Y + _headerPaddingY);
				spriteBatch.DrawString(_font, _speakerDisplay, drawPosition + new Vector2(0f, _zoom), SpeakerShadowColor, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
				spriteBatch.DrawString(_font, _speakerDisplay, drawPosition, SpeakerTextColor, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
			}
			if (!_isAutoplay && _isReadyForNextLine)
			{
				Rectangle frameSource = _gcm.SpTextBox.GetFrameSource(14 + _newLineIconFrame);
				spriteBatch.Draw(_gcm.SpTextBox.Texture, new Vector2(_messageBoxDimensions.Right + _newLineIconMarginX, _messageBoxDimensions.Bottom + _newLineIconMarginY), frameSource, Color.White, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
			}
			if (!_isThereAPreviousPortrait && PreviousPortrait != 0 && PreviousPortrait != _portraitType && Portrait != 0)
			{
				LoadPreviousPortrait();
			}
			if (_shouldDrawPreviousPortrait && _previousPortraitSpriteSheet != null)
			{
				Color color = Color.White * (1f - _previousPortraitFadePercentage);
				spriteBatch.Draw(_previousPortraitSpriteSheet.Texture, _portraitDrawLocation, _previousPortraitFrameSource, color, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
			}
			if (_hasPortrait)
			{
				Color color2 = Color.White;
				if (_shouldDrawPreviousPortrait)
				{
					color2 = Color.White * _previousPortraitFadePercentage;
				}
				spriteBatch.Draw(_portraitSpriteSheet.Texture, _portraitDrawLocation, _portraitFrameSource, color2, 0f, Vector2.Zero, _zoom, SpriteEffects.None, 0f);
			}
			break;
		}
		}
	}

	private void LoadPreviousPortrait()
	{
		_isThereAPreviousPortrait = true;
		_previousPortraitSpriteSheet = GetPortraitSpriteSheetFromPortraitType(PreviousPortrait);
		_previousPortraitFrameSource = FrameSourceFromPortrait(PreviousPortrait, _previousPortraitSpriteSheet);
		_previousPortraitFadeTimer = 0.07f;
		_shouldDrawPreviousPortrait = true;
		_previousPortraitFadePercentage = 0f;
	}

	private void DrawSmallerTextBox(SpriteBatch spriteBatch, Rectangle originalDimensions, float percentage, bool doesShrinkBothAxes)
	{
		Rectangle backgroundRectangle = originalDimensions;
		float num = 1f - percentage;
		backgroundRectangle.Height = (int)((float)backgroundRectangle.Height * percentage);
		backgroundRectangle.Width = (doesShrinkBothAxes ? ((int)((float)backgroundRectangle.Width * percentage)) : backgroundRectangle.Width);
		backgroundRectangle.X = originalDimensions.X + (doesShrinkBothAxes ? ((int)((float)originalDimensions.Width / 2f * num)) : 0);
		backgroundRectangle.Y = originalDimensions.Y + (doesShrinkBothAxes ? ((int)((float)originalDimensions.Height / 2f * num)) : 0);
		DrawingEx.DrawTextBox(spriteBatch, backgroundRectangle, Color.White * percentage, _gcm.SpTextBox);
	}

	internal static Rectangle FrameSourceFromPortrait(ECharacterPortraitType portraitType, SpriteSheet sprite)
	{
		int num = (int)(portraitType - 1);
		if (num >= 24)
		{
			num -= 24;
		}
		else if (num >= 12)
		{
			num -= 12;
		}
		return sprite.GetFrameSource(num);
	}

	private SpriteSheet GetPortraitSpriteSheetFromPortraitType(ECharacterPortraitType portraitType)
	{
		int num = (int)(portraitType - 1);
		if (num < 12)
		{
			return _gcm.SpPortraits;
		}
		if (num < 24)
		{
			return _gcm.SpPortraits2;
		}
		return _gcm.SpBossPortraits;
	}

	internal static ECharacterPortraitType PortraitFromName(string name, GameSave saveFile)
	{
		ECharacterPortraitType result = ECharacterPortraitType.None;
		switch (name)
		{
		case "Lunais":
		{
			InventoryRelicCollection relicInventory = saveFile.Inventory.RelicInventory;
			result = ((!relicInventory.IsRelicActive(EInventoryRelicType.EternalBrooch)) ? ((!relicInventory.IsRelicActive(EInventoryRelicType.EmpireBrooch)) ? ECharacterPortraitType.Lunais : ECharacterPortraitType.TimeWitch) : ECharacterPortraitType.EternalMother);
			break;
		}
		case "Selen":
			result = ECharacterPortraitType.Selen;
			break;
		case "Neliste":
		case "Alchemist":
			result = ECharacterPortraitType.Neliste;
			break;
		case "Haristel":
		case "Captain":
			result = ECharacterPortraitType.Haristel;
			break;
		case "Rameda":
		case "Medic":
			result = ECharacterPortraitType.Rameda;
			break;
		case "Seykis":
		case "Soldier":
			result = ECharacterPortraitType.Seykis;
			break;
		case "Eschem":
		{
			if (saveFile.GetSaveBool("IsEnding4"))
			{
				result = ECharacterPortraitType.EschemVilete;
				break;
			}
			int primaryQuestState = NPCBase.GetPrimaryQuestState(NPCBase.ENPCType.Medic, saveFile);
			int primaryQuestState2 = NPCBase.GetPrimaryQuestState(NPCBase.ENPCType.SickSoldier, saveFile);
			result = ((primaryQuestState >= 4 && primaryQuestState2 >= 2) ? ECharacterPortraitType.EschemHealthy : ECharacterPortraitType.Eschem);
			break;
		}
		case "Crow":
		case "Merchant Crow":
			result = ECharacterPortraitType.MerchantCrow;
			break;
		case "Meyef":
		case "Dragon":
			result = ECharacterPortraitType.Meyef;
			break;
		case "Yorne":
			result = ECharacterPortraitType.Yorne;
			break;
		case "Marella":
			result = ECharacterPortraitType.Marella;
			break;
		case "Pano":
			result = ECharacterPortraitType.Pano;
			break;
		case "Jiana":
			result = (saveFile.GetSaveBool("IsJianaShocked") ? ECharacterPortraitType.JianaShocked : ECharacterPortraitType.Jiana);
			break;
		case "Faron":
			result = ECharacterPortraitType.Faron;
			break;
		case "Clanmother Undar":
			result = ECharacterPortraitType.Elder;
			break;
		case "Librarian":
			result = ((NPCBase.GetPrimaryQuestState(NPCBase.ENPCType.Librarian, saveFile) >= 3) ? ECharacterPortraitType.Aristocrat : ECharacterPortraitType.Librarian);
			break;
		case "Cultist":
			result = ECharacterPortraitType.Cultist;
			break;
		case "Philia":
			result = ECharacterPortraitType.Queen;
			break;
		case "Child":
			result = ECharacterPortraitType.Child;
			break;
		case "Eternal Mother":
			result = ECharacterPortraitType.EternalMother;
			break;
		case "Aelana":
			result = ECharacterPortraitType.Aelana;
			break;
		case "Succubus":
			result = ECharacterPortraitType.Succubus;
			break;
		case "Incubus":
			result = ECharacterPortraitType.Incubus;
			break;
		case "Genza":
			result = ECharacterPortraitType.Genza;
			break;
		case "'Selen'":
			result = ECharacterPortraitType.FakeSelen;
			break;
		case "Nuvius":
			result = ECharacterPortraitType.Nuvius;
			break;
		case "Terrilis":
			result = ECharacterPortraitType.Terrilis;
			break;
		case "Cantoran":
			result = ECharacterPortraitType.Cantoran;
			break;
		case "Emperor Nuvius":
			result = ECharacterPortraitType.EmperorNuvius;
			break;
		case "Prince Nuvius":
			result = ECharacterPortraitType.PrinceNuvius;
			break;
		}
		return result;
	}

	internal static ECharacterPortraitType PortraitFromNPCType(NPCBase.ENPCType npcType)
	{
		ECharacterPortraitType result = ECharacterPortraitType.None;
		switch (npcType)
		{
		case NPCBase.ENPCType.Astrologer:
			result = ECharacterPortraitType.Neliste;
			break;
		case NPCBase.ENPCType.Captain:
			result = ECharacterPortraitType.Haristel;
			break;
		case NPCBase.ENPCType.Medic:
			result = ECharacterPortraitType.Rameda;
			break;
		case NPCBase.ENPCType.MerchantCrow:
			result = ECharacterPortraitType.MerchantCrow;
			break;
		case NPCBase.ENPCType.Quartermaster:
			result = ECharacterPortraitType.Seykis;
			break;
		case NPCBase.ENPCType.Selen:
			result = ECharacterPortraitType.Selen;
			break;
		case NPCBase.ENPCType.SickSoldier:
			result = ECharacterPortraitType.Eschem;
			break;
		case NPCBase.ENPCType.Librarian:
			result = ECharacterPortraitType.Librarian;
			break;
		case NPCBase.ENPCType.CultistPriest:
		case NPCBase.ENPCType.CultistWorshipper:
			result = ECharacterPortraitType.Cultist;
			break;
		}
		return result;
	}
}
