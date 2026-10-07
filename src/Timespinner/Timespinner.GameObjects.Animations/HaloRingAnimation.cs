using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Animations;

internal class HaloRingAnimation
{
	internal enum EHaloExpandType
	{
		Sin,
		Cos,
		Linear
	}

	private const int DefaultDiameter = 64;

	private const float DefaultTimeToExpand = 0.25f;

	private readonly Texture2D _ringTexture;

	private readonly Level _level;

	private bool _hasStarted;

	private float _timer;

	private float _percentage;

	internal bool HasStarted => _hasStarted;

	internal bool IsFinished { get; private set; }

	internal bool IsInReverse { get; set; }

	internal EHaloExpandType ExpandType { get; set; }

	internal int Diameter
	{
		set
		{
			Width = value;
			Height = value;
		}
	}

	internal int Width { get; set; }

	internal int Height { get; set; }

	internal float TimeToExpand { get; set; }

	internal Point Center { get; set; }

	internal Color BaseDrawColor { get; set; }

	internal HaloRingAnimation(Level level)
	{
		_level = level;
		_ringTexture = _level.GCM.TxLargeRing;
		Diameter = 64;
		TimeToExpand = 0.25f;
		BaseDrawColor = Color.White;
	}

	internal void Update(float delta)
	{
		_timer += delta;
		if (_timer < TimeToExpand)
		{
			_hasStarted = true;
			_percentage = _timer / TimeToExpand;
			switch (ExpandType)
			{
			case EHaloExpandType.Sin:
				_percentage = (float)Math.Sin(_percentage * ((float)Math.PI / 2f));
				break;
			case EHaloExpandType.Cos:
				_percentage = 1f - (float)Math.Cos(_percentage * ((float)Math.PI / 2f));
				break;
			}
		}
		else
		{
			_percentage = 1f;
			IsFinished = true;
			_hasStarted = false;
		}
		if (IsInReverse)
		{
			_percentage = 1f - _percentage;
		}
	}

	internal void Draw(SpriteBatch spriteBatch)
	{
		int num = (int)((float)Width * _percentage);
		int num2 = (int)((float)Height * _percentage);
		Color color = BaseDrawColor * (1f - _percentage);
		Vector2 value2 = Vector2.Subtract(value2: new Vector2((float)Center.X - (float)num / 2f, (float)Center.Y - (float)num2 / 2f), value1: _level.CameraPosition);
		value2 = Vector2.Subtract(_level.LevelRenderCenter, value2);
		spriteBatch.Draw(destinationRectangle: new Rectangle((int)value2.X, (int)value2.Y, num, num2), texture: _ringTexture, color: color);
	}

	internal void Reset()
	{
		IsFinished = false;
		_timer = 0f;
		_percentage = (IsInReverse ? 1 : 0);
	}

	internal void Start()
	{
		_hasStarted = true;
	}
}
