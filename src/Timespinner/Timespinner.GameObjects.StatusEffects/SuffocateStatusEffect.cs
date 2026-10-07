using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.StatusEffects;

internal class SuffocateStatusEffect : BaseStatusEffect
{
	private const int OxygenBarOffsetX = -24;

	private const int OxygenBarOffsetY = -64;

	private const int OxygenBarTextOffsetY = 16;

	private const int OxygenFrameOffsetX = -2;

	private const int OxygenFrameOffsetY = -1;

	private const int MaxSuffocationTime = 2;

	private const float HealthDamagePercentPerTic = 0.1f;

	private const float TimeBetweenDamageTics = 1f;

	private const float DangerThresholdPercentage = 0.5f;

	private const float OxygenBarMax = 100f;

	private const float BaseOxygenDepletionRate = 7.5f;

	private const float OxygenDepletionRatePowerMultiplier = 1f;

	private const float TimeToStartHealing = 0.5f;

	private const float BaseOxygenHealRate = 10f;

	private const float BaseShineFrequency = 5f;

	private const float ShineFrequencyPercentageMultiplier = 5f;

	private static readonly Color TextHealingColor = new Color(72, 176, 104);

	private readonly int _basePower;

	private bool _isHealing;

	private bool _isDamaging;

	private bool _isInCutscene;

	private bool _isFadingIn;

	private float _fadeInTimer;

	private float _fadeAmount;

	private float _damageTimer;

	private float _healTimer;

	private float _shineTimer;

	private float _shinePercentage;

	private float _currentOxygenLevel;

	private float _currentOxygenPercentage;

	private Vector2 _oxygenBarPosition;

	public SuffocateStatusEffect(Level level, Alive host, bool isPlayer, int power)
		: base(level, host, EStatusEffectType.Suffocate, isPlayer)
	{
		_basePower = power;
		_currentOxygenLevel = 100f;
		base.TextColor1 = new Color(172, 80, 172);
		base.TextColor2 = new Color(232, 104, 80);
		_isFadingIn = true;
	}

	internal override float GetMaxTime()
	{
		return 2f;
	}

	internal override void Refresh()
	{
		_healTimer = 0f;
		base.Refresh();
	}

	internal override void Update(float delta)
	{
		_isInCutscene = base.Level.IsPlayerInputBlocked;
		_healTimer += delta;
		_isHealing = _healTimer >= 0.5f;
		base.Timer = 0f;
		if ((double)base.TextShowTimer > 0.5)
		{
			base.TextShowTimer = 0.5f;
		}
		if (base.IsFadingOut)
		{
			if (_isFadingIn)
			{
				_fadeInTimer -= delta;
				if (_fadeInTimer < 0f)
				{
					_fadeInTimer = 0f;
				}
				_fadeAmount = _fadeInTimer / 0.5f;
			}
			else
			{
				_fadeAmount = 1f - base.FadeTimer / 0.5f;
			}
		}
		else
		{
			_fadeAmount = 1f;
			if (_fadeInTimer < 0.5f)
			{
				_fadeInTimer += delta;
				if (_fadeInTimer < 0.5f)
				{
					_fadeAmount = _fadeInTimer / 0.5f;
				}
			}
			else
			{
				_isFadingIn = false;
			}
		}
		UpdateShowStatusText(delta);
		if (!_isInCutscene)
		{
			if (!_isHealing)
			{
				_currentOxygenLevel -= 7.5f * ((float)_basePower * 1f) * delta;
			}
			else if (!base.IsFadingOut)
			{
				_currentOxygenLevel += 10f * delta;
				if (_currentOxygenLevel >= 100f)
				{
					Kill();
				}
			}
			_isDamaging = _currentOxygenLevel <= 0f;
			if (_isDamaging)
			{
				_currentOxygenLevel = 0f;
				_damageTimer += delta;
				if (_damageTimer >= 1f)
				{
					_damageTimer -= 1f;
					int damage = (int)Math.Ceiling((float)base.Host.MaxHP * 0.1f);
					base.Host.ManageSubtleDamage(damage, isLethal: true, EElementalWeaknessState.None);
				}
			}
		}
		_currentOxygenPercentage = _currentOxygenLevel / 100f;
		_oxygenBarPosition = new Vector2(base.Host.Position.X, base.Host.Position.Y + -64);
		float num = ((_currentOxygenPercentage > 0f) ? (1f / _currentOxygenPercentage * 5f + 5f) : 0f);
		_shineTimer += delta * num;
		if (_shineTimer >= (float)Math.PI * 2f)
		{
			_shineTimer -= (float)Math.PI * 2f;
		}
		_shinePercentage = ((float)Math.Sin(_shineTimer) + 1f) / 2f;
		base.Update(delta);
	}

	internal override void Draw(SpriteBatch spriteBatch)
	{
		if (!_isInCutscene)
		{
			Vector2 vector = Vector2.Subtract(base.Level.LevelRenderCenter, Vector2.Subtract(base.Level.CameraPosition, _oxygenBarPosition));
			Vector2 vector2 = new Vector2(vector.X - (float)(base.StatusTextWidth / 2), vector.Y);
			DrawingEx.DrawString(spriteBatch, base.Font, base.StatusText, vector2.Add(new Point(0, 1)), base.CurrentShadowColor * _fadeAmount);
			Color color = (_isHealing ? TextHealingColor : base.CurrentTextColor);
			DrawingEx.DrawString(spriteBatch, base.Font, base.StatusText, vector2, color * _fadeAmount);
			vector2 = new Vector2(vector.X + -24f, vector.Y + 16f);
			Rectangle frameSource = base.Sprite.GetFrameSource(8);
			spriteBatch.Draw(base.Sprite.Texture, vector2, frameSource, Color.White * _fadeAmount);
			if (_fadeAmount > 0.1f)
			{
				Color color2 = new Color(1f, 0.9f, 0.9f, _shinePercentage) * _fadeAmount;
				spriteBatch.End();
				base.Level.GCM.EfBrighten.Parameters["shinyAmount"].SetValue(1f);
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, base.Level.GCM.EfBrighten);
				frameSource = ((_currentOxygenPercentage > 0.5f) ? base.Sprite.GetFrameSource(6) : base.Sprite.GetFrameSource(7));
				frameSource.Width = (int)((float)frameSource.Width * _currentOxygenPercentage);
				spriteBatch.Draw(base.Sprite.Texture, vector2, frameSource, color2);
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
			}
			frameSource = base.Sprite.GetFrameSource(9);
			spriteBatch.Draw(position: new Vector2(vector2.X + -2f, vector2.Y + -1f), texture: base.Sprite.Texture, sourceRectangle: frameSource, color: Color.White * _fadeAmount);
			base.Draw(spriteBatch);
		}
	}
}
