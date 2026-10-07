using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class CavesSnailFloorGoopUnit : Appendage
{
	private const float TimeBeforeFading = 3f;

	private const float TimeToFade = 0.2f;

	private const float TimeToAppear = 0.25f;

	private float _lifeTimer;

	private float _fadeTimer;

	private float _appearTimer;

	private Rectangle _originalFrameSource;

	internal bool IsFinished { get; private set; }

	internal bool HasBeenRemoved { get; set; }

	public CavesSnailFloorGoopUnit(Animate parent, Point bboxDimensions, Point inBboxOffset, Level inLevel, SpriteSheet inSprite)
		: base(parent, bboxDimensions, inBboxOffset, inLevel, inSprite)
	{
		base.DoesInheritDrawColor = false;
	}

	public override void Update(float delta)
	{
		if (!_isFrozen)
		{
			_lifeTimer += delta;
			if (_lifeTimer > 3f)
			{
				_fadeTimer += delta;
				if (_fadeTimer < 0.2f)
				{
					base.DrawColor = Color.White * (1f - _fadeTimer / 0.2f);
				}
				else
				{
					IsFinished = true;
				}
			}
			if (_appearTimer < 0.25f)
			{
				_appearTimer += delta;
				float num = ((!(_appearTimer < 0.25f)) ? 1f : ((float)Math.Sin((float)Math.PI / 2f * _appearTimer / 0.25f)));
				int num2 = (int)Math.Ceiling(num * (float)_originalFrameSource.Height);
				_frameSource = new Rectangle(_originalFrameSource.X, _originalFrameSource.Y + (_originalFrameSource.Height - num2), _originalFrameSource.Width, num2);
			}
			base.Update(delta);
		}
		base.Update(delta);
	}

	internal void Reset(Point position)
	{
		IsFinished = false;
		HasBeenRemoved = false;
		_fadeTimer = 0f;
		_lifeTimer = 0f;
		_appearTimer = 0f;
		base.DrawColor = Color.White;
		Position = new Point(position.X, position.Y + 16);
		int start = _level.NextRandomInt(0, 1) + 20;
		ChangeAnimation(start);
		_originalFrameSource = _frameSource;
	}
}
