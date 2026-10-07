using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Timespinner.GameAbstractions;

public class ScreenFade : ScreenEffect
{
	private readonly float _fadeOutTime;

	private readonly float _fadeInTime;

	private readonly float _totalDuration;

	private readonly float _timeBeforeFadingIn;

	private float _timer;

	private Color _finalEffectColor;

	public ScreenFade(float fadeOutTime)
	{
		_fadeOutTime = fadeOutTime;
		_fadeInTime = fadeOutTime;
		_timeBeforeFadingIn = _fadeOutTime * 2f;
		_totalDuration = _fadeOutTime * 3f;
		base.EffectColor = Color.Black;
	}

	public ScreenFade(float fadeOutTime, float blackTime, float fadeInTime)
	{
		_fadeOutTime = fadeOutTime;
		_fadeInTime = fadeInTime;
		_timeBeforeFadingIn = fadeOutTime + blackTime;
		_totalDuration = _timeBeforeFadingIn + fadeInTime;
		base.EffectColor = Color.Black;
	}

	public override void Update(float delta)
	{
		_timer += delta;
		if (_timer >= _totalDuration)
		{
			base.IsFinished = true;
			_timer = _totalDuration;
		}
		float num = 0f;
		if (_timer < _fadeOutTime)
		{
			num = _timer / _fadeOutTime;
		}
		else if (_timer <= _timeBeforeFadingIn)
		{
			num = 1f;
		}
		else if (_timer < _totalDuration)
		{
			num = 1f - (_timer - _timeBeforeFadingIn) / _fadeInTime;
		}
		_finalEffectColor = base.EffectColor * num;
	}

	public override void Draw(SpriteBatch spriteBatch, Texture2D flashTexture, Rectangle drawRect)
	{
		spriteBatch.Draw(flashTexture, drawRect, _finalEffectColor);
	}
}
