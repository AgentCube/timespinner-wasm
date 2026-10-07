using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameObjects.Animations;

internal sealed class GlowTexture
{
	private const int DefaultGlowCircleCount = 6;

	private const int DefaultGlowCircleRadius = 32;

	private const float DefaultGlowAmplitude = 0.25f;

	private const float DefaultGlowOffset = 1f;

	private const float DefaultGlowMultiplier = 1f;

	private const float DefaultGlowColorMultiplier = 0.035f;

	private const float DefaultGlowOscillationMultipler = 1f;

	private const float DefaultGlowFrequency = (float)Math.PI;

	private const float DefaultGlowCircleConsecutiveSizeReduction = 0.9f;

	private readonly SpriteSheet _glowSprite;

	private readonly Level _level;

	private int _frameIndex;

	private int _glowCircleRadius;

	private float _glowOscillationTimer;

	private Rectangle _frameSource;

	private Color _glowCircleColor;

	internal int GlowCircleCount { get; set; }

	internal int GlowCircleRadius
	{
		get
		{
			return _glowCircleRadius;
		}
		set
		{
			_glowCircleRadius = value;
			GlowCircleWidth = value;
			GlowCircleHeight = value;
		}
	}

	internal int GlowCircleWidth { get; set; }

	internal int GlowCircleHeight { get; set; }

	internal float GlowAmplitude { get; set; }

	internal float GlowOffset { get; set; }

	internal float GlowFrequency { get; set; }

	internal float GlowMultiplier { get; set; }

	internal float GlowColorMultiplier { get; set; }

	internal float GlowOscillationMultipler { get; set; }

	internal float GlowCircleConsecutiveSizeReduction { get; set; }

	internal Point Center { get; set; }

	internal Color BaseColor { get; set; }

	internal SpriteSheet GlowSpriteSheet { get; set; }

	internal int FrameIndex
	{
		get
		{
			return _frameIndex;
		}
		set
		{
			_frameIndex = value;
			if (GlowSpriteSheet != null)
			{
				_frameSource = GlowSpriteSheet.GetFrameSource(value);
			}
		}
	}

	internal GlowTexture(Level level)
	{
		_level = level;
		_glowSprite = _level.GCM.SpSmoothCircles;
		BaseColor = Color.White;
		GlowCircleCount = 6;
		GlowCircleRadius = 32;
		GlowAmplitude = 0.25f;
		GlowOffset = 1f;
		GlowFrequency = (float)Math.PI;
		GlowMultiplier = 1f;
		GlowColorMultiplier = 0.035f;
		GlowOscillationMultipler = 1f;
		GlowCircleConsecutiveSizeReduction = 0.9f;
	}

	internal void Update(float delta)
	{
		_glowOscillationTimer += delta * GlowFrequency;
		if (_glowOscillationTimer >= (float)Math.PI * 2f)
		{
			_glowOscillationTimer -= (float)Math.PI * 2f;
		}
		float num = (float)(Math.Cos(_glowOscillationTimer) * (double)GlowAmplitude) + GlowOffset;
		_glowCircleColor = BaseColor * GlowColorMultiplier * num * GlowMultiplier;
	}

	internal void Draw(SpriteBatch spriteBatch)
	{
		float num = GlowCircleWidth;
		float num2 = GlowCircleHeight;
		int num3 = (int)((float)GlowCircleCount * GlowMultiplier);
		Color glowCircleColor = _glowCircleColor;
		bool flag = GlowSpriteSheet != null && _frameSource != Rectangle.Empty;
		for (int i = 0; i < num3; i++)
		{
			Vector2 value = new Vector2((float)Center.X - num / 2f, (float)Center.Y - num2 / 2f);
			Vector2 vector = Vector2.Subtract(_level.LevelRenderCenter, Vector2.Subtract(_level.CameraPosition, value));
			Rectangle rectangle = new Rectangle((int)vector.X, (int)vector.Y, (int)num, (int)num2);
			if (flag)
			{
				spriteBatch.Draw(GlowSpriteSheet.Texture, rectangle, _frameSource, glowCircleColor);
			}
			else
			{
				SmoothCircle.Draw(spriteBatch, _glowSprite, rectangle, glowCircleColor);
			}
			num *= GlowCircleConsecutiveSizeReduction;
			num2 *= GlowCircleConsecutiveSizeReduction;
		}
	}
}
