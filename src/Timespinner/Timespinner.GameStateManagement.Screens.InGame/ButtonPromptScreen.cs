using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameStateManagement.ScreenManager;

namespace Timespinner.GameStateManagement.Screens.InGame;

internal class ButtonPromptScreen : MessageBoxScreen
{
	private const float AnimationSpeed = 0.33f;

	private readonly Level _level;

	private bool _shouldHide;

	private int _animationIndex;

	private float _animationCounter;

	private float _lifeCounter;

	private float _currentAmount;

	private Point _targetDrawPosition;

	private Vector2 _cameraPosition;

	private Vector2 _drawPosition;

	public bool IsDead;

	public ButtonPromptScreen(int inWhichButton, Point inPosition, float inAmount, Level inLevel)
		: base(GetButtonText(inWhichButton), shouldIncludeUsageText: false, inLevel.PlayerControllerMapping)
	{
		_lifeCounter = 0.1f;
		_currentAmount = inAmount;
		_targetDrawPosition = inPosition;
		_level = inLevel;
		base.IsPopupScreen = true;
		base.IsOverlayScreen = true;
		base.TransitionOnTime = TimeSpan.FromSeconds(0.2);
		base.TransitionOffTime = TimeSpan.FromSeconds(0.2);
	}

	private static string GetButtonText(int inWhichButton)
	{
		string result = "";
		switch (inWhichButton)
		{
		case 0:
			result = "$A";
			break;
		case 4:
			result = "$U";
			break;
		case 5:
			result = "$V";
			break;
		}
		return result;
	}

	public override void HandleInput(InputState input)
	{
	}

	public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
	{
		float num = (float)gameTime.ElapsedGameTime.TotalSeconds;
		_lifeCounter -= num;
		if (_lifeCounter <= 0f)
		{
			IsDead = true;
			ExitScreen();
		}
		_animationCounter += num;
		if (_animationCounter >= 0.33f)
		{
			_animationCounter = 0f;
			_animationIndex = (_animationIndex + 1) % 2;
		}
		if (_currentAmount >= 1f)
		{
			_shouldHide = true;
		}
		if (_level != null)
		{
			_cameraPosition = _level.CameraPosition;
		}
		SpriteFont menuFont = base.ScreenManager.MenuFont;
		Vector2 vector = menuFont.MeasureString(base.Message) * Constants.InGameZoom;
		Viewport viewport = base.ScreenManager.GraphicsDevice.Viewport;
		Vector2 vector2 = new Vector2(viewport.Width, viewport.Height);
		Vector2 vector3 = new Vector2((float)_targetDrawPosition.X - _cameraPosition.X, (float)_targetDrawPosition.Y - _cameraPosition.Y) * Constants.InGameZoom;
		_drawPosition = vector2 / 2f - vector;
		_drawPosition += vector3;
		_drawPosition.Y += -32 * Constants.InGameZoom;
		base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
	}

	public void Refresh(Point inPosition, float inAmount)
	{
		_currentAmount = inAmount;
		_targetDrawPosition = inPosition;
		_lifeCounter = 0.1f;
	}

	public override void Draw(GameTime gameTime)
	{
		if (_shouldHide)
		{
			return;
		}
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		SpriteFont menuFont = base.ScreenManager.MenuFont;
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
		float num = 1f - base.TransitionOffPercentage;
		Color drawColor = new Color(240, 240, 208) * num;
		Color shadowDrawColor = new Color(60, 60, 24) * num;
		foreach (DialogueLine line in base.Lines)
		{
			line.Draw(spriteBatch, _drawPosition, drawColor, shadowDrawColor, Constants.InGameZoom, num);
			_drawPosition.Y += menuFont.LineSpacing * Constants.InGameZoom;
		}
		spriteBatch.End();
	}
}
