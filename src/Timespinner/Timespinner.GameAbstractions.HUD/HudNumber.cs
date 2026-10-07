using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameAbstractions.HUD;

public class HudNumber
{
	private const int RiseHeight = 36;

	private const float TimeToRise = 0.4f;

	private const float TimeToRest = 0.2f;

	private const float TimeToFade = 0.1f;

	private const float TimeToStartFading = 0.6f;

	private const float MaxLifeTime = 0.7f;

	public const int KerningOffset = 5;

	private readonly int _totalDigits;

	private readonly int _frameOffset;

	private readonly int _amount;

	private readonly ENumberColor _color;

	private readonly Point _startPoint;

	private readonly SpriteSheet _sprite;

	private readonly List<int> _digits = new List<int>();

	private float _lifetime;

	private Vector2 _floatPosition;

	private Color _drawColor = Color.White;

	public bool IsFinished => _lifetime >= 0.7f;

	public bool IsLeftAdjusted { get; set; }

	public bool DoesDrawSign { get; set; }

	public float Width => _totalDigits * 5;

	public ENumberColor NumberColor => _color;

	public ENumberMovementType MovementType { get; set; }

	public int Amount => _amount;

	public Point StartPoint => _startPoint;

	public Point Position
	{
		get
		{
			return new Point((int)_floatPosition.X, (int)_floatPosition.Y);
		}
		set
		{
			_floatPosition = value.ToVector2();
		}
	}

	public HudNumber(int amount, Point startPoint, GCM gcm)
		: this(amount, startPoint, gcm, ENumberColor.White)
	{
	}

	public HudNumber(int amount, Point startPoint, GCM gcm, ENumberColor color)
	{
		_amount = amount;
		_startPoint = startPoint;
		_sprite = gcm.SpNumbers;
		_color = color;
		_floatPosition = new Vector2(_startPoint.X, _startPoint.Y);
		switch (_color)
		{
		case ENumberColor.Red:
			_frameOffset = 12;
			break;
		case ENumberColor.Green:
			_frameOffset = 24;
			break;
		case ENumberColor.Yellow:
			_frameOffset = 36;
			break;
		case ENumberColor.Orange:
			_frameOffset = 48;
			break;
		case ENumberColor.Gray:
			_frameOffset = 60;
			break;
		default:
			_frameOffset = 0;
			break;
		}
		if (_amount == 0)
		{
			_totalDigits = 1;
			_digits.Add(0);
			return;
		}
		int num = Math.Abs(_amount);
		_totalDigits = 0;
		while (num > 0)
		{
			_digits.Add(num % 10);
			num /= 10;
			_totalDigits++;
		}
	}

	internal static int GetDigitsFromNumber(int number)
	{
		int num = 0;
		int num2 = Math.Abs(number);
		if (num2 > 0)
		{
			while (num2 > 0)
			{
				num2 /= 10;
				num++;
			}
		}
		return num;
	}

	public void Update(float delta)
	{
		_lifetime += delta;
		ENumberMovementType movementType = MovementType;
		if (movementType == ENumberMovementType.FloatUp)
		{
			if (_lifetime <= 0.4f)
			{
				float num = _lifetime / 0.7f;
				_floatPosition.Y = (float)_startPoint.Y - (float)(Math.Sin((float)Math.PI / 2f * num) * 36.0);
			}
			else if (_lifetime >= 0.6f)
			{
				_drawColor = Color.White * (1f - (_lifetime - 0.6f) / 0.1f);
			}
		}
	}

	public void Draw(SpriteBatch spriteBatch, Camera2D camera)
	{
		Vector2 position = Vector2.Subtract(value2: new Vector2((float)camera.Position.X - _floatPosition.X, (float)camera.Position.Y - _floatPosition.Y), value1: camera.LevelRenderCenter);
		position.X += _totalDigits * 5;
		foreach (int digit in _digits)
		{
			spriteBatch.Draw(_sprite.Texture, position, _sprite.GetFrameSource(digit + _frameOffset), _drawColor);
			position.X -= 5f;
		}
	}

	public void Draw(SpriteBatch spriteBatch, float zoom, float alphaAmount)
	{
		Color color = _drawColor * alphaAmount;
		float num = 5f * zoom;
		Vector2 position = (IsLeftAdjusted ? new Vector2((int)(_floatPosition.X + (float)_totalDigits * num), (int)_floatPosition.Y) : new Vector2((int)(_floatPosition.X + (float)_totalDigits * num * 0.5f), (int)_floatPosition.Y));
		if (DoesDrawSign)
		{
			int num2 = ((_amount > 0) ? 10 : 11);
			spriteBatch.Draw(_sprite.Texture, new Vector2((int)(_floatPosition.X + num), (int)_floatPosition.Y), _sprite.GetFrameSource(num2 + _frameOffset), color, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
			position.X += num;
		}
		foreach (int digit in _digits)
		{
			spriteBatch.Draw(_sprite.Texture, position, _sprite.GetFrameSource(digit + _frameOffset), color, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0f);
			position.X -= num;
		}
	}
}
