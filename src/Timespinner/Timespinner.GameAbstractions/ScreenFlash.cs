using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Timespinner.GameAbstractions;

public class ScreenFlash : ScreenEffect
{
	private readonly float _duration;

	private float _lastAmplitude;

	private float _intensity;

	private Color _finalFlashColor;

	public float Amplitude { get; set; }

	public float Frequency { get; set; }

	public float Timer { get; private set; }

	public ScreenFlash(float duration)
	{
		_duration = duration;
		Frequency = 1f;
		Amplitude = 1f;
		base.EffectColor = Color.White;
	}

	public override void Update(float delta)
	{
		Timer += delta;
		if (Timer >= _duration)
		{
			base.IsFinished = true;
			Timer = _duration;
		}
		float num = ((_duration != 0f) ? (Timer / _duration) : 1f);
		float num2 = (float)(1.0 - Math.Cos(num * ((float)Math.PI * 2f) * Frequency)) / 2f;
		float num3 = 1f;
		if (Frequency > 0f)
		{
			float num4 = _duration / Frequency;
			if (Timer > num4)
			{
				num3 = 1f - (Timer - num4) / (_duration - num4);
			}
		}
		_intensity = num2 * num3;
		if (Amplitude != _lastAmplitude)
		{
			_finalFlashColor = base.EffectColor * Amplitude;
			_lastAmplitude = Amplitude;
		}
	}

	public override void Draw(SpriteBatch spriteBatch, Texture2D flashTexture, Rectangle drawRect)
	{
		spriteBatch.Draw(flashTexture, drawRect, _finalFlashColor * _intensity);
	}
}
