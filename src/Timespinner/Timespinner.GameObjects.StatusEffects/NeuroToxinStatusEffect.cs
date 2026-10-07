using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.StatusEffects;

internal class NeuroToxinStatusEffect : BaseStatusEffect
{
	private const int OxygenBarOffsetY = -52;

	private const float TimeBetweenParticleEmission = 0.1f;

	private const float MaxNeuroToxinTime = 5f;

	private const float AuraDrainRate = 50f;

	private readonly StatusEffectSparkleParticleSystem _neuroSparklesSystem;

	private readonly Level _level;

	private float _particleTimer;

	private float _fadeAmount;

	private float _colorTimer;

	private Vector2 _textPosition;

	public NeuroToxinStatusEffect(Level inLevel, Alive host, bool isPlayer)
		: base(inLevel, host, EStatusEffectType.NeuroToxin, isPlayer)
	{
		_neuroSparklesSystem = new StatusEffectSparkleParticleSystem(base.Sprite, 5, 0, 0)
		{
			BaseColor = new Vector4(0.6f, 0.7f, 0.4f, 1f)
		};
		base.DoesShowStatusText = false;
		base.TextColor1 = new Color(200, 220, 165);
		base.TextColor2 = new Color(160, 200, 128);
		_level = inLevel;
	}

	internal override float GetMaxTime()
	{
		return 5f;
	}

	internal override void Update(float delta)
	{
		if (!base.IsFadingOut)
		{
			_fadeAmount = 1f;
			if (base.Host is Protagonist protagonist)
			{
				float num = delta * 50f;
				if (num >= 0f)
				{
					protagonist.ReduceAura(num);
				}
			}
			_particleTimer += delta;
			if (_particleTimer >= 0.1f)
			{
				_particleTimer -= 0.1f;
				_neuroSparklesSystem.AddParticles(base.Host.Bbox);
			}
		}
		else
		{
			_fadeAmount = 1f - base.FadeTimer / 0.5f;
		}
		if ((double)base.TextShowTimer > 0.5)
		{
			base.TextShowTimer = 0.5f;
		}
		UpdateShowStatusText(delta);
		_textPosition = new Vector2(base.Host.Position.X, base.Host.Position.Y + -52);
		_neuroSparklesSystem.Update(delta);
		base.Update(delta);
		_colorTimer += delta;
		float num2 = _colorTimer / 1f;
		if (num2 >= 2f)
		{
			num2 -= 2f;
			_colorTimer -= 2f;
		}
		else if (num2 > 1f)
		{
			num2 = 1f - (1f - num2);
		}
		num2 = _fadeAmount * num2;
		Color levelDrawColor = MathEx.SineInterpolate(end: new Color(160, 200, 128), start: Color.White, amount: num2);
		_level.SetLevelDrawColor(levelDrawColor);
		if (base.IsFinished)
		{
			_level.SetLevelDrawColor(Color.White);
		}
	}

	internal override void Draw(SpriteBatch spriteBatch)
	{
		_neuroSparklesSystem.Draw(spriteBatch, base.Level.LevelRenderCenter, base.Level.CameraPosition, base.Level.CameraZoom);
		Vector2 vector = Vector2.Subtract(base.Level.LevelRenderCenter, Vector2.Subtract(base.Level.CameraPosition, _textPosition));
		Vector2 vector2 = new Vector2(vector.X - (float)(base.StatusTextWidth / 2), vector.Y);
		DrawingEx.DrawString(spriteBatch, base.Font, base.StatusText, vector2.Add(new Point(0, 1)), base.CurrentShadowColor * _fadeAmount);
		DrawingEx.DrawString(spriteBatch, base.Font, base.StatusText, vector2, base.CurrentTextColor * _fadeAmount);
		base.Draw(spriteBatch);
	}

	internal override void ChangeRoom()
	{
		_neuroSparklesSystem.KillOffParticles(0f);
		base.ChangeRoom();
	}
}
