using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameStateManagement.ScreenManager;

namespace Timespinner.GameStateManagement;

internal class MessageBoxScreen : GameScreen
{
	private readonly string _message;

	private readonly ControllerMapping _controllerMapping;

	private readonly List<DialogueLine> _lines = new List<DialogueLine>();

	public bool DoesIncludeButtonText { get; set; }

	internal string Message => _message;

	internal List<DialogueLine> Lines => _lines;

	public event EventHandler<PlayerIndexEventArgs> Accepted;

	public event EventHandler<PlayerIndexEventArgs> Cancelled;

	public MessageBoxScreen(string message, ControllerMapping controllerMapping)
		: this(message, shouldIncludeUsageText: true, controllerMapping)
	{
	}

	public MessageBoxScreen(string message, bool shouldIncludeUsageText, ControllerMapping controllerMapping)
	{
		_message = message;
		_controllerMapping = controllerMapping;
		if (shouldIncludeUsageText)
		{
			string text = "\n      $A  " + Loc.Get("ChoiceOk") + "\n      $B  " + Loc.Get("ChoiceCancel");
			_message += text;
		}
		base.IsPopupScreen = true;
		base.TransitionOnTime = TimeSpan.FromSeconds(0.2);
		base.TransitionOffTime = TimeSpan.FromSeconds(0.2);
	}

	public override void LoadContent()
	{
		base.LoadContent();
		_lines.AddRange(DialogueLine.SplitMessageIntoLines(_message, (int)((float)base.ScreenManager.ViewPortArea.Width * 0.66f), base.ScreenManager.MenuFont, base.ScreenManager.UIControllerButtons, Constants.InGameZoom, _controllerMapping));
	}

	public override void HandleInput(InputState input)
	{
		if (input.IsNewPressConfirm(base.ControllingPlayer, out var pressedPlayer))
		{
			if (this.Accepted != null)
			{
				this.Accepted(this, new PlayerIndexEventArgs(pressedPlayer));
			}
			ExitScreen();
		}
		else if (input.IsNewPressCancel(base.ControllingPlayer, out pressedPlayer))
		{
			if (this.Cancelled != null)
			{
				this.Cancelled(this, new PlayerIndexEventArgs(pressedPlayer));
			}
			ExitScreen();
		}
	}

	public override void Draw(GameTime gameTime)
	{
		base.ScreenManager.FadeBackBufferToBlack(base.TransitionAlpha * 2 / 3);
		SpriteBatch spriteBatch = base.ScreenManager.SpriteBatch;
		SpriteFont menuFont = base.ScreenManager.MenuFont;
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
		Color color = Color.White * (1f - base.TransitionOffPercentage);
		Point padding = new Point(8 * Constants.InGameZoom, 0);
		Vector2 drawPosition = DrawBoxBack(spriteBatch, padding, Vector2.Zero, menuFont, color, _message);
		float num = 1f - base.TransitionOffPercentage;
		color = new Color(240, 240, 208) * num;
		Color shadowDrawColor = new Color(60, 60, 24) * num;
		foreach (DialogueLine line in _lines)
		{
			line.Draw(spriteBatch, drawPosition, color, shadowDrawColor, Constants.InGameZoom, num);
			drawPosition.Y += menuFont.LineSpacing * Constants.InGameZoom;
		}
		spriteBatch.End();
	}

	protected Vector2 DrawBoxBack(SpriteBatch spriteBatch, Point padding, Vector2 centerShift, SpriteFont font, Color color, string inMessage, Vector2 inTextPosition)
	{
		Vector2 vector = inTextPosition;
		Viewport viewport = base.ScreenManager.GraphicsDevice.Viewport;
		Vector2 vector2 = new Vector2(viewport.Width, viewport.Height);
		int num = 0;
		foreach (DialogueLine line in _lines)
		{
			Vector2 vector3 = font.MeasureString(line.Line);
			if (vector3.X > (float)num)
			{
				num = (int)vector3.X;
			}
		}
		Vector2 vector4 = new Vector2(num, font.LineSpacing * _lines.Count) * Constants.InGameZoom;
		vector = ((!(vector == Vector2.Zero)) ? Vector2.Subtract(vector2 / 2f, vector) : ((vector2 - vector4) / 2f + centerShift));
		vector = new Vector2((int)vector.X, (int)vector.Y);
		Rectangle backgroundRectangle = new Rectangle((int)vector.X - padding.X, (int)vector.Y - padding.Y, (int)vector4.X + padding.X * 2, (int)vector4.Y + padding.Y * 2);
		base.ScreenManager.DrawTextBox(spriteBatch, backgroundRectangle, color);
		return vector;
	}

	protected Vector2 DrawBoxBack(SpriteBatch spriteBatch, Point padding, Vector2 centerShift, SpriteFont font, Color color, string inMessage)
	{
		return DrawBoxBack(spriteBatch, padding, centerShift, font, color, inMessage, Vector2.Zero);
	}
}
