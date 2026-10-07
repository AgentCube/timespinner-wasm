using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Localization;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.StatusEffects;

internal abstract class BaseStatusEffect
{
	internal const float TimeToFade = 0.5f;

	internal const float TimeForPaletteShift = 0.2f;

	private const float TimeToFadeInText = 0.2f;

	private const float TimeToFadeOutText = 0.2f;

	private const float TimeToShowText = 0.6f;

	private const float TimeToStartFading = 0.8f;

	private const float TimeToShowAllText = 1f;

	private readonly bool _isPlayer;

	private readonly EStatusEffectType _statusEffectType;

	private readonly int _statusTextWidth;

	private readonly string _statusText;

	private readonly SpriteFont _font;

	private readonly SpriteSheet _sprite;

	private readonly Alive _host;

	private readonly Level _level;

	internal static readonly Color BaseShadowColor = new Color(60, 60, 24);

	private float _fadeTimer;

	private float _textPaletteTimer;

	private Point _textStartPoint;

	private Vector2 _textDrawPosition;

	private Color _currentTextColor;

	private Color _currentShadowColor;

	internal bool IsFinished { get; set; }

	internal bool IsFadingOut { get; set; }

	internal bool IsPlayer => _isPlayer;

	internal bool DoesShowStatusText { get; set; }

	internal bool DoesUsePaletteShifting { get; set; }

	internal EStatusEffectType StatusEffectType => _statusEffectType;

	internal int StatusTextWidth => _statusTextWidth;

	internal float Timer { get; set; }

	internal float TextShowTimer { get; set; }

	internal float FadeTimer => _fadeTimer;

	internal Color TextColor1 { get; set; }

	internal Color TextColor2 { get; set; }

	internal Color CurrentShadowColor => _currentShadowColor;

	internal Color CurrentTextColor => _currentTextColor;

	internal string StatusText => _statusText;

	internal SpriteSheet Sprite => _sprite;

	internal SpriteFont Font => _font;

	internal Alive Host => _host;

	internal Level Level => _level;

	internal BaseStatusEffect(Level level, Alive host, EStatusEffectType type, bool isPlayer)
	{
		_level = level;
		_host = host;
		_statusEffectType = type;
		_isPlayer = isPlayer;
		_sprite = _level.GCM.SpStatusEffects;
		_font = _level.GCM.ActiveFont;
		_statusText = Loc.Get("StatStatus" + type);
		_statusTextWidth = (int)_font.MeasureString(_statusText).X;
	}

	internal virtual float GetMaxTime()
	{
		return 0f;
	}

	internal virtual void Update(float delta)
	{
		if (DoesShowStatusText && _isPlayer)
		{
			UpdateShowStatusText(delta);
		}
		if (!IsFadingOut)
		{
			Timer += delta;
			if (Timer >= GetMaxTime())
			{
				Kill();
			}
		}
		else
		{
			_fadeTimer += delta;
			if (_fadeTimer >= 0.5f)
			{
				IsFinished = true;
			}
		}
	}

	internal void UpdateShowStatusText(float delta)
	{
		float textShowTimer = TextShowTimer;
		TextShowTimer += delta;
		if (TextShowTimer > 0f && textShowTimer <= 0f && Host != null)
		{
			_textStartPoint = new Point(Host.Position.X - _statusTextWidth / 2, Host.Bbox.Top);
		}
		if (TextShowTimer >= 1f)
		{
			DoesShowStatusText = false;
			return;
		}
		float num = 1f;
		bool flag = false;
		if (TextShowTimer < 0.2f)
		{
			float num2 = TextShowTimer / 0.2f;
			num = (float)Math.Sin(num2 * ((float)Math.PI / 2f));
		}
		else if (TextShowTimer > 0.8f)
		{
			float num3 = (TextShowTimer - 0.8f) / 0.2f;
			num = 1f - (float)Math.Sin(num3 * ((float)Math.PI / 2f));
			flag = true;
		}
		DoesUsePaletteShifting = !flag;
		float num4 = TextShowTimer / 1f;
		float num5 = num4 * 24f;
		_textDrawPosition = new Vector2(_textStartPoint.X, (float)_textStartPoint.Y - num5);
		if (!flag)
		{
			_textPaletteTimer += delta;
			if (_textPaletteTimer >= 0.2f)
			{
				_textPaletteTimer -= 0.2f;
			}
		}
		Color color = TextColor1.SineInterpolate(TextColor2, _textPaletteTimer / 0.2f);
		_currentTextColor = color * num;
		_currentShadowColor = BaseShadowColor * num;
	}

	internal virtual void Refresh()
	{
	}

	internal virtual void Draw(SpriteBatch spriteBatch)
	{
		if (DoesShowStatusText && _isPlayer)
		{
			DrawStatusText(spriteBatch);
		}
	}

	private void DrawStatusText(SpriteBatch spriteBatch)
	{
		Vector2 vector = Vector2.Subtract(_level.LevelRenderCenter, Vector2.Subtract(_level.CameraPosition, _textDrawPosition));
		DrawingEx.DrawString(spriteBatch, _font, _statusText, vector.Add(new Point(0, 1)), _currentShadowColor);
		DrawingEx.DrawString(spriteBatch, _font, _statusText, vector, _currentTextColor);
	}

	internal virtual void ChangeRoom()
	{
	}

	internal virtual void Kill()
	{
		IsFadingOut = true;
	}

	public static BaseStatusEffect FromType(EStatusEffectType statusType, Alive host, Level level, bool isPlayer, int power)
	{
		BaseStatusEffect result = null;
		switch (statusType)
		{
		case EStatusEffectType.Poison:
			result = new PoisonStatusEffect(level, host, isPlayer);
			break;
		case EStatusEffectType.Burn:
			result = new BurnStatusEffect(level, host, isPlayer, power);
			break;
		case EStatusEffectType.Suffocate:
			result = new SuffocateStatusEffect(level, host, isPlayer, power);
			break;
		case EStatusEffectType.Chaos:
			result = new ChaosStatusEffect(level, host, isPlayer);
			break;
		case EStatusEffectType.NeuroToxin:
			result = new NeuroToxinStatusEffect(level, host, isPlayer);
			break;
		}
		return result;
	}
}
