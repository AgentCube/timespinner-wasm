using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Animations;

internal class TextPopupAnimation : BattleAnimation
{
	private const int RiseHeight = 12;

	private const int TrailLength = 30;

	private const float TimeToRise = 0.8f;

	private const float TimeToRest = 0.2f;

	private const float TimeToFade = 0.1f;

	private const float TimeToStartFading = 1f;

	private const float MaxLifeTime = 1.1f;

	private const float TrailFadeRate = 0.9f;

	private static readonly Color ShadowColor = new Color(0.1f, 0.1f, 0.1f, 1f);

	private static readonly Color BrightestColor = Color.White;

	private readonly Point _startPoint;

	private readonly SpriteFont _font;

	private readonly string _displayText;

	private readonly Point _textOffset;

	private readonly Color _originalColor;

	private readonly List<Point> _drawHistories = new List<Point>();

	private bool _isTextFinished;

	private float _lifetime;

	private Point _textPosition;

	private Vector2 _floatPosition;

	internal override bool IsFinished
	{
		get
		{
			if (_isTextFinished)
			{
				return base.IsFinished;
			}
			return false;
		}
	}

	public TextPopupAnimation(string text, SpriteSheet inSprite, Point inPosition, Level inLevel, Color color)
		: base(inSprite, inPosition, inLevel)
	{
		_displayText = text;
		_font = base.Level.GCM.ActiveFont;
		_startPoint = inPosition;
		_floatPosition = _startPoint.ToVector2();
		Vector2 vector = _font.MeasureString(_displayText);
		_textOffset = new Point(-(int)(vector.X / 2f), -(int)vector.Y);
		_originalColor = color;
		base.DrawColor = _originalColor;
		base.ParticleSystem = new OrbLevelUpParticleSystem(base.Level.GCM.TxParticleEnergy, 1, color);
		base.AnimationStart = 31;
		base.AnimationLength = 5;
		base.AnimationSpeed = 0.05f;
	}

	public override void Update(float delta)
	{
		_lifetime += delta;
		if (_lifetime <= 0.8f)
		{
			float num = _lifetime / 0.8f;
			if (num < 0.5f)
			{
				_drawHistories.Add(_floatPosition.ToPoint());
			}
			else
			{
				_drawHistories.Clear();
			}
			float num2 = (float)Math.Sin((float)Math.PI * num);
			base.DrawColor = Color.Lerp(_originalColor, BrightestColor, num2);
			_floatPosition.Y = (float)_startPoint.Y - num2 * 12f;
		}
		else if (_lifetime >= 1f)
		{
			base.DrawColor = _originalColor * (1f - (_lifetime - 1f) / 0.1f);
		}
		_textPosition = _floatPosition.ToPoint();
		if (_lifetime > 1.1f)
		{
			_isTextFinished = true;
		}
		base.Update(delta);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
		float num = 0.9f;
		for (int num2 = _drawHistories.Count - 1; num2 >= 0; num2--)
		{
			Point a = _drawHistories[num2];
			DrawTextAtPosition(spriteBatch, a.Add(_textOffset), num);
			num *= num;
		}
		Point position = _textPosition.Add(_textOffset);
		DrawTextAtPosition(spriteBatch, position, 1f);
		while (_drawHistories.Count > 30)
		{
			_drawHistories.RemoveAt(0);
		}
	}

	private void DrawTextAtPosition(SpriteBatch spriteBatch, Point position, float alphaPercentage)
	{
		Vector2 value = Vector2.Subtract(base.Level.CameraPosition, new Vector2(position.X, position.Y));
		value = Vector2.Subtract(base.Level.LevelRenderCenter, Vector2.Multiply(value, base.Level.CameraZoom));
		DrawingEx.DrawString(spriteBatch, _font, _displayText, Vector2.Add(value, Vector2.One), ShadowColor * alphaPercentage);
		DrawingEx.DrawString(spriteBatch, _font, _displayText, value, base.DrawColor * alphaPercentage);
	}
}
