using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events;

internal sealed class ItemPopupAppendage : Appendage
{
	private const int ItemSize = 16;

	private const int PopupHeight = 48;

	private const int GlowBaseStart = 3;

	private const float TimeToPopup = 0.75f;

	private const float TimeToLinger = 1f;

	private const float TimeToFade = 0.2f;

	private const float TimeToStartFading = 1.75f;

	private const float TimeToLive = 1.95f;

	private const float FlashFrameTime = 0.05f;

	private const float TimeToStopFlashing = 0.25f;

	private const float TimeForItemSoundEmission = 0.25f;

	private bool _isFlashing;

	private bool _isFlashingThisFrame;

	private bool _wasPoppingUp;

	private int _popupHeight;

	private int _endPopupHeight;

	private float _popupCounter;

	private float _flashTimer;

	internal bool IsFinished { get; private set; }

	internal bool IsPopppingUp { get; set; }

	internal bool ShouldPlaySFX { get; set; }

	public ItemPopupAppendage(Animate parent, Level inLevel, SpriteSheet inSprite)
		: base(parent, new Point(16, 16), Point.Zero, inLevel, inSprite)
	{
		_glowBase = 3f;
		_glowColor = new Color(1f, 1f, 0.9f, 0.8f);
		ShouldPlaySFX = true;
	}

	public override void Update(float delta)
	{
		if (IsPopppingUp)
		{
			if (!_wasPoppingUp)
			{
				Point point = new Point(base.ParentAnchorPosition.X / 16, base.ParentAnchorPosition.Y / 16);
				_endPopupHeight = 48;
				for (int i = 2; i < 5; i++)
				{
					Point key = new Point(point.X, point.Y - i);
					if (_level.SolidTiles.ContainsKey(key))
					{
						_endPopupHeight = (i - 2) * 16;
						break;
					}
				}
			}
			float popupCounter = _popupCounter;
			_popupCounter += delta;
			base.DrawColor = Color.White;
			if (_popupCounter <= 0.75f)
			{
				float num = _popupCounter / 0.75f;
				_popupHeight = (int)(Math.Sin(num * ((float)Math.PI / 2f)) * (double)_endPopupHeight);
				base.AnchorOffset = new Point(0, -_popupHeight);
				_isFlashing = true;
			}
			else if (_popupCounter < 1.75f)
			{
				float num2 = 1f - (_popupCounter - 0.75f) / 0.25f;
				if (num2 < 0f)
				{
					_isFlashing = false;
				}
				else
				{
					_glowBase = 3f * num2;
				}
			}
			else if (_popupCounter > 1.75f && _popupCounter < 1.95f)
			{
				_isFlashing = false;
				float num3 = (_popupCounter - 1.75f) / 0.2f;
				base.DrawColor = Color.White * (float)Math.Cos(num3 * ((float)Math.PI / 2f));
			}
			else if (_popupCounter >= 1.95f)
			{
				IsFinished = true;
				_doesDrawBaseSprite = false;
			}
			if (_isFlashing)
			{
				_flashTimer += delta;
				if (_flashTimer >= 0.05f)
				{
					_flashTimer -= 0.05f;
					_isFlashingThisFrame = !_isFlashingThisFrame;
				}
				_isGlowing = _isFlashingThisFrame;
			}
			else
			{
				_isGlowing = false;
			}
			if (_popupCounter >= 0.25f && popupCounter < 0.25f && ShouldPlaySFX)
			{
				PlayCue(ESFX.ItemGetGeneral, Position);
			}
		}
		_wasPoppingUp = IsPopppingUp;
		base.Update(delta);
	}

	internal void ResetLife()
	{
		_popupCounter = 0f;
		IsFinished = false;
		_doesDrawBaseSprite = true;
		IsPopppingUp = false;
		base.AnchorOffset = Point.Zero;
		_glowBase = 3f;
		Update(0f);
	}
}
