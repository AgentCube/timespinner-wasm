using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameStateManagement.ScreenManager;

namespace Timespinner.GameStateManagement.Screens.InGame.Toasts;

internal class QuestCompleteToast : BaseToastPopup
{
	private const int BaseLatinHeight = 40;

	private const int BaseAsianHeight = 48;

	private const int BaseWidth = 400;

	private const int BaseDescriptionWidth = 360;

	private const int PromptOffsetY = -18;

	private const int LatinLineMarginY = -3;

	private const int AsianLineMarginY = -1;

	private const int BaseUnderlineWidth = 144;

	private const int BaseUnderlineDrawOffsetY = 1;

	private const int LatinBaseNameOffsetY = -11;

	private const int LatinBaseDescriptionOffsetY = 1;

	private const int AsianBaseNameOffsetY = -13;

	private const int AsianBaseDescriptionOffsetY = 3;

	private const int IsFinishedIconFrameStart = 31;

	private const int IsFinishedIconFrameCount = 2;

	private const float IsFinishedIconAnimationSpeed = 0.2f;

	private const float TimeToFlash = 0.75f;

	private const float TimeToWait = 0.15f;

	private const float TimeToFade = 0.2f;

	private const float TimeToExpand = 0.15f;

	private const string NPC_Quest_Title_Key_Format = "inv_jou_quest_{0}_{1}";

	private static readonly Color TextNameDrawColor = new Color(240, 240, 208);

	private static readonly Color TextDescriptionDrawColor = new Color(208, 200, 152);

	private static readonly Color TextShadowDrawColor = new Color(60, 60, 24);

	private readonly bool _isAsianLoc;

	private readonly int _baseHeight;

	private readonly Rectangle _filigreeFrameFrameSource;

	private readonly Rectangle _blackFrameSource;

	private readonly Rectangle _frameLineFrameSource;

	private readonly Rectangle _underlineCapFrameSource;

	private readonly DialogueLine _header;

	private readonly List<DialogueLine> _description;

	private readonly SpriteFont _font;

	private readonly SpriteSheet _buttonsSprite;

	private int _height;

	private int _isFinishedIconAnimationIndex;

	private int _zoom;

	private int _lineHeight;

	private int _width;

	private int _finalHeight;

	private int _blackDrawOffsetX;

	private int _headerOffsetX;

	private int _headerOffsetY;

	private int _descriptionOffsetX;

	private int _descriptionOffsetY;

	private int _underlineWidth;

	private int _underDrawOffsetY;

	private int _underlineCapOffsetX;

	private float _expandTimer;

	private float _isFinishedIconTimer;

	private float _isFinishedIcoColorMultiplier;

	private float _textColorMultiplier;

	private Vector2 _drawPosition;

	private Vector2 _promptDrawPosition;

	private Vector2 _filigreeFrameDrawOrigin;

	private Color _isFinishedIconDrawColor = Color.Transparent;

	public QuestCompleteToast(SpriteSheet sprite, GCM gcm, ScriptAction script, ControllerMapping controllerMapping)
		: base(sprite, doesFreezeGameplay: true, gcm, 0f, 0.75f, 0.15f, 0.2f)
	{
		base.DoesWaitForInputToFinish = true;
		_isAsianLoc = Loc.IsAsianLocale;
		_baseHeight = (_isAsianLoc ? 48 : 40);
		_blackFrameSource = base.Sprite.GetFrameSource(27);
		_frameLineFrameSource = base.Sprite.GetFrameSource(29);
		_underlineCapFrameSource = base.Sprite.GetFrameSource(28);
		_filigreeFrameFrameSource = base.Sprite.GetFrameSource(33);
		_font = gcm.ActiveFont;
		_buttonsSprite = gcm.SpUIButtons;
		NPCBase.ENPCType itemToGive = (NPCBase.ENPCType)script.ItemToGive;
		int itemToGiveCount = script.ItemToGiveCount;
		string line = Loc.Get("QuestComplete");
		string description = Loc.Get($"inv_jou_quest_{itemToGive}_{itemToGiveCount}");
		description = Loc.SurroundWithQuotationMarks(description);
		_zoom = Constants.InGameZoom;
		_header = new DialogueLine(line, _font, _buttonsSprite, _zoom, controllerMapping);
		_description = DialogueLine.SplitMessageIntoLines(description, 360 * _zoom, _font, _buttonsSprite, _zoom, controllerMapping);
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
		_finalHeight = _baseHeight * _zoom;
		_width = 400 * _zoom;
		_blackDrawOffsetX = _width / 2;
		if (base.ScreenManager != null)
		{
			Rectangle titleSafeArea = base.ScreenManager.TitleSafeArea;
			_drawPosition = new Vector2(titleSafeArea.Center.X / _zoom, titleSafeArea.Center.Y / _zoom);
			_promptDrawPosition = new Vector2(titleSafeArea.Right - 16 * _zoom, (_drawPosition.Y + (float)_baseHeight * 0.5f + -18f) * (float)_zoom);
		}
		int num = (_isAsianLoc ? (-1) : (-3));
		int num2 = (_isAsianLoc ? (-13) : (-11));
		int num3 = ((!_isAsianLoc) ? 1 : 3);
		_headerOffsetX = -(_header.Width / 2) * _zoom;
		_headerOffsetY = num2 * _zoom;
		_descriptionOffsetX = ((_description.Count > 0) ? (-_description[0].Width / 2 * _zoom) : 0);
		_descriptionOffsetY = num3 * _zoom;
		_lineHeight = (_font.LineSpacing + num) * _zoom;
		_filigreeFrameDrawOrigin = new Vector2(0f, (int)((float)_filigreeFrameFrameSource.Height * 0.5f));
		_underlineWidth = 144 * _zoom;
		_underDrawOffsetY = _zoom;
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
		if (_textColorMultiplier > 0.5f)
		{
			Color drawColor2 = TextNameDrawColor * base.DrawColorPercentage * _textColorMultiplier;
			Color shadowDrawColor = TextShadowDrawColor * base.DrawColorPercentage * _textColorMultiplier;
			Vector2 drawPosition = new Vector2(vector.X + (float)_headerOffsetX, vector.Y + (float)_headerOffsetY);
			Rectangle destinationRectangle3 = new Rectangle((int)vector.X - _underlineWidth / 2 + _underlineCapOffsetX, (int)vector.Y + _underDrawOffsetY, _underlineWidth - _underlineCapOffsetX * 2, _zoom);
			spriteBatch.Draw(base.Sprite.Texture, destinationRectangle3, _frameLineFrameSource, drawColor, 0f, Vector2.Zero, SpriteEffects.None, 0f);
			Vector2 position = new Vector2(destinationRectangle3.Left - _underlineCapOffsetX, destinationRectangle3.Top);
			spriteBatch.Draw(base.Sprite.Texture, position, _underlineCapFrameSource, drawColor, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
			spriteBatch.Draw(position: new Vector2(destinationRectangle3.Right, position.Y), texture: base.Sprite.Texture, sourceRectangle: _underlineCapFrameSource, color: drawColor, rotation: 0f, origin: Vector2.Zero, scale: zoom, effects: SpriteEffects.FlipHorizontally, layerDepth: 0f);
			_header.Draw(spriteBatch, drawPosition, drawColor2, shadowDrawColor, _zoom, base.DrawColorPercentage);
			Color drawColor3 = TextDescriptionDrawColor * base.DrawColorPercentage * _textColorMultiplier;
			Vector2 drawPosition2 = new Vector2(vector.X + (float)_descriptionOffsetX, vector.Y + (float)_descriptionOffsetY);
			foreach (DialogueLine item in _description)
			{
				item.Draw(spriteBatch, drawPosition2, drawColor3, shadowDrawColor, _zoom, base.DrawColorPercentage);
				drawPosition2 = new Vector2(drawPosition2.X, drawPosition2.Y + (float)_lineHeight);
			}
		}
		Vector2 position3 = new Vector2(vector.X, destinationRectangle.Top);
		spriteBatch.Draw(base.Sprite.Texture, position3, _filigreeFrameFrameSource, drawColor, 0f, _filigreeFrameDrawOrigin, zoom, SpriteEffects.FlipHorizontally, 0f);
		spriteBatch.Draw(position: new Vector2(position3.X - (float)_filigreeFrameFrameSource.Width * zoom, position3.Y), texture: base.Sprite.Texture, sourceRectangle: _filigreeFrameFrameSource, color: drawColor, rotation: 0f, origin: _filigreeFrameDrawOrigin, scale: zoom, effects: SpriteEffects.None, layerDepth: 0f);
		if (base.IsFinishedAndIsWaitingToClose)
		{
			Rectangle frameSource = base.Sprite.GetFrameSource(_isFinishedIconAnimationIndex + 31);
			Color color = _isFinishedIconDrawColor * base.DrawColorPercentage;
			spriteBatch.Draw(base.Sprite.Texture, _promptDrawPosition, frameSource, color, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
		}
	}
}
