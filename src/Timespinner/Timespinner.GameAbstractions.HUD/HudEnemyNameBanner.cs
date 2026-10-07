using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Constants;
using Timespinner.GameAbstractions.Gameplay;

namespace Timespinner.GameAbstractions.HUD;

internal class HudEnemyNameBanner : HudElement
{
	private const int BorderPaddingX = 12;

	private const int BorderPaddingY = 0;

	private const int BorderHeight = 22;

	private const float TimeToFadeIn = 0.1f;

	private const float TimeToDisplay = 2f;

	private const float TimeToFadeOut = 0.2f;

	private const float TotalDisplayTime = 2.3f;

	private const float TimeBeforeFadingOut = 2.1f;

	private static readonly Vector2 TextSizeMargin = new Vector2(-2f, 0f);

	private static readonly Color BaseTextColor = new Color(240, 240, 208);

	private float _displayTimer;

	private float _alpha;

	private string _enemyName;

	private Color _textDrawColor;

	private Color _borderColor;

	internal bool IsFinished => _displayTimer > 2.3f;

	public HudEnemyNameBanner(GCM inGCM, Rectangle titleSafeArea)
		: base(inGCM, GetDrawPositionFromTitleSafeArea(titleSafeArea))
	{
	}

	private static Point GetDrawPositionFromTitleSafeArea(Rectangle titleSafeArea)
	{
		return new Point(titleSafeArea.Right, titleSafeArea.Bottom - Constants.InGameZoom * 4);
	}

	private void ShowNewEnemyName(string enemyName)
	{
		_enemyName = enemyName;
		_displayTimer = ((!IsFinished) ? 0.1f : 0f);
	}

	public void Update(float delta, Level level)
	{
		if (level != null && level.EnemyHitName != null && level.EnemyHitName != "")
		{
			ShowNewEnemyName(level.EnemyHitName);
			level.EnemyHitName = "";
		}
		if (!IsFinished)
		{
			_displayTimer += delta;
			_alpha = 0f;
			if (_displayTimer < 0.1f)
			{
				_alpha = (float)Math.Sin((float)Math.PI / 2f * (_displayTimer / 0.1f));
			}
			else if (_displayTimer < 2.1f)
			{
				_alpha = 1f;
			}
			else if (_displayTimer < 2.3f)
			{
				_alpha = (float)Math.Cos((float)Math.PI / 2f * (_displayTimer - 2.1f) / 0.2f);
			}
			_textDrawColor = BaseTextColor * _alpha;
			_borderColor = Color.White * _alpha;
			base.Update(delta);
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (!IsFinished && _enemyName != null)
		{
			int num = 12 * base.Zoom;
			_ = base.Zoom;
			int num2 = 0;
			SpriteFont activeFont = _gcm.ActiveFont;
			Vector2 vector = (activeFont.MeasureString(_enemyName) + TextSizeMargin) * base.Zoom;
			int num3 = (int)vector.X + num * 2;
			Vector2 drawPos = new Vector2(_drawPosition.X - num3 + num, (float)_drawPosition.Y - vector.Y / 2f);
			SpriteEffects[] flipped = new SpriteEffects[3]
			{
				SpriteEffects.None,
				SpriteEffects.None,
				SpriteEffects.FlipHorizontally
			};
			DrawingEx.DrawShortBox(spriteBatch, new Rectangle((int)drawPos.X - num, (int)drawPos.Y + num2, num3, 22), _borderColor, _gcm.SpPauseMenu, base.Zoom, new int[3] { 73, 74, 73 }, flipped, shouldTile: true);
			DrawingEx.DrawString(spriteBatch, activeFont, _enemyName, drawPos, _textDrawColor, Vector2.Zero, base.Zoom);
			base.Draw(spriteBatch);
		}
	}

	public void RefreshZoom(Rectangle titleSafeArea)
	{
		_drawPosition = GetDrawPositionFromTitleSafeArea(titleSafeArea);
	}
}
