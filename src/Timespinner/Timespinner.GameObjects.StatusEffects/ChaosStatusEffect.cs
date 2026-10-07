using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.StatusEffects;

internal class ChaosStatusEffect : BaseStatusEffect
{
	private const int OxygenBarOffsetY = -52;

	private const float TimeBetweenParticleEmission = 0.1f;

	private const float MaxChaosTime = 20f;

	private const float SandDrainRate = 10f;

	private readonly StatusEffectSparkleParticleSystem _chaosSparklesSystem;

	private float _particleTimer;

	private float _fadeAmount;

	private Vector2 _textPosition;

	public ChaosStatusEffect(Level inLevel, Alive host, bool isPlayer)
		: base(inLevel, host, EStatusEffectType.Chaos, isPlayer)
	{
		_chaosSparklesSystem = new StatusEffectSparkleParticleSystem(base.Sprite, 5, 0, 0)
		{
			BaseColor = new Vector4(0.8f, 0.4f, 0.4f, 1f)
		};
		base.DoesShowStatusText = false;
		base.TextColor1 = new Color(230, 160, 165);
		base.TextColor2 = new Color(160, 56, 64);
	}

	internal override float GetMaxTime()
	{
		return 20f;
	}

	internal override void Update(float delta)
	{
		if (!base.IsFadingOut)
		{
			_fadeAmount = 1f;
			float num = base.Host.MPFloat - delta * 10f;
			if (num >= 0f)
			{
				base.Host.MPFloat = num;
			}
			_particleTimer += delta;
			if (_particleTimer >= 0.1f)
			{
				_particleTimer -= 0.1f;
				_chaosSparklesSystem.AddParticles(base.Host.Bbox);
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
		_chaosSparklesSystem.Update(delta);
		base.Update(delta);
	}

	internal override void Draw(SpriteBatch spriteBatch)
	{
		_chaosSparklesSystem.Draw(spriteBatch, base.Level.LevelRenderCenter, base.Level.CameraPosition, base.Level.CameraZoom);
		Vector2 vector = Vector2.Subtract(base.Level.LevelRenderCenter, Vector2.Subtract(base.Level.CameraPosition, _textPosition));
		Vector2 vector2 = new Vector2(vector.X - (float)(base.StatusTextWidth / 2), vector.Y);
		DrawingEx.DrawString(spriteBatch, base.Font, base.StatusText, vector2.Add(new Point(0, 1)), base.CurrentShadowColor * _fadeAmount);
		DrawingEx.DrawString(spriteBatch, base.Font, base.StatusText, vector2, base.CurrentTextColor * _fadeAmount);
		base.Draw(spriteBatch);
	}

	internal override void ChangeRoom()
	{
		_chaosSparklesSystem.KillOffParticles(0f);
		base.ChangeRoom();
	}
}
