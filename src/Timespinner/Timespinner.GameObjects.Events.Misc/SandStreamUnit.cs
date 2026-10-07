using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Misc;

internal sealed class SandStreamUnit : Appendage
{
	private const int TextureWidth = 128;

	private const float DefaultTimeMultiplier = 5f;

	private readonly float _baseTimeMultiplier;

	private readonly float _baseAmplitude;

	private readonly float _baseFrequency;

	private readonly float _baseShiftXSpeed;

	private float _waveTime;

	private float _waveOffsetX;

	internal float TimeMultiplierMultiplier { get; set; }

	internal float ShiftXSpeedMultiplier { get; set; }

	internal float WaveFrequencyMultiplier { get; set; }

	internal float WaveAmplitudeMultiplier { get; set; }

	private float TimeMultiplier => _baseTimeMultiplier * TimeMultiplierMultiplier;

	private float ShiftXSpeed => _baseShiftXSpeed * ShiftXSpeedMultiplier;

	private float WaveFrequency => _baseFrequency * WaveFrequencyMultiplier;

	private float WaveAmplitude => _baseAmplitude * WaveAmplitudeMultiplier;

	internal Vector3 SandValues { get; set; }

	public SandStreamUnit(Animate parent, Level inLevel, SpriteSheet inSprite, float baseFrequency, float baseAmplitude, float baseShiftXSpeed, float timeOffset)
		: base(parent, new Point(128, 128), Point.Zero, inLevel, inSprite)
	{
		_baseTimeMultiplier = 5f;
		_baseShiftXSpeed = baseShiftXSpeed;
		_baseAmplitude = baseAmplitude;
		_baseFrequency = baseFrequency;
		_waveTime = timeOffset;
		TimeMultiplierMultiplier = 1f;
		ShiftXSpeedMultiplier = 1f;
		WaveFrequencyMultiplier = 1f;
		WaveAmplitudeMultiplier = 1f;
		ChangeAnimation(0);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_waveTime += delta * TimeMultiplier;
			if (_waveTime > 1000000f)
			{
				_waveTime = 0f;
			}
			_waveOffsetX += delta * ShiftXSpeed;
			if (_waveOffsetX >= 1f)
			{
				_waveOffsetX -= 1f;
			}
			else if (_waveOffsetX < 0f)
			{
				_waveOffsetX += 1f;
			}
		}
		base.Update(delta);
	}

	private void ApplySineShaderValues(float time, float frequency, float amplitude, float offsetX, bool isOver)
	{
		Vector4 value = new Vector4(time, frequency, amplitude, offsetX);
		_level.GCM.EfSandTrigDeform.Parameters["WaveValues"].SetValue(value);
		_level.GCM.EfSandTrigDeform.Parameters["SandValues"].SetValue(new Vector4(SandValues.X, SandValues.Y, SandValues.Z, isOver ? 1 : (-1)));
	}

	public void Draw(SpriteBatch spriteBatch, bool isOver)
	{
		spriteBatch.End();
		ApplySineShaderValues(_waveTime, WaveFrequency, WaveAmplitude, _waveOffsetX, isOver);
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, _level.GCM.EfSandTrigDeform);
		Draw(spriteBatch);
		spriteBatch.End();
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null);
	}
}
