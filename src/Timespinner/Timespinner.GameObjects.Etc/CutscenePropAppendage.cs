using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Etc;

internal sealed class CutscenePropAppendage : Appendage
{
	private const float TimeToFadeIn = 0.25f;

	private static readonly Point GlowOffset = new Point(6, 6);

	private readonly GlowTexture _glowTexture;

	private readonly Animate _parent;

	private readonly Dictionary<int, Point> _frameToOffsetDict = new Dictionary<int, Point>();

	private bool _isAmuletGlowing;

	private bool _isFadingOut;

	private int _unhiddenAnimationIndex;

	private float _fadeInTimer;

	internal int GlowCircleCount { get; set; }

	internal int GlowRadius { get; set; }

	internal Color BaseGlowColor { get; set; }

	public CutscenePropAppendage(Animate parent, Point bboxDimensions, Point inBboxOffset, Level inLevel, SpriteSheet inSprite)
		: base(parent, bboxDimensions, inBboxOffset, inLevel, inSprite)
	{
		_parent = parent;
		base.AnchorObject = parent;
		base.FollowType = EAppendageFollowType.AnchorLocked;
		base.DrawPriority = 1;
		_doesUseAppendageCollision = false;
		base.DoesInheritDrawColor = false;
		GlowRadius = 16;
		GlowCircleCount = 8;
		_glowTexture = new GlowTexture(_level)
		{
			BaseColor = Color.Transparent,
			GlowCircleRadius = 20,
			GlowFrequency = (float)Math.PI * 2f
		};
	}

	internal void AddOffsets(List<KeyValuePair<int, Point>> pairs)
	{
		foreach (KeyValuePair<int, Point> pair in pairs)
		{
			_frameToOffsetDict.Add(pair.Key, pair.Value);
		}
	}

	public override void Update(float delta)
	{
		MatchParentOffset();
		UpdateAmuletGlowing(delta);
		base.Update(delta);
	}

	private void MatchParentOffset()
	{
		int key = _parent.AnimationStart + _parent.AnimationIndex;
		if (_frameToOffsetDict.ContainsKey(key))
		{
			base.AnchorOffset = _frameToOffsetDict[key];
		}
	}

	private void UpdateAmuletGlowing(float delta)
	{
		if (!_isAmuletGlowing)
		{
			return;
		}
		if (_fadeInTimer < 0.25f)
		{
			_fadeInTimer += delta;
			float num = 1f;
			if (_fadeInTimer < 0.25f)
			{
				num = (float)Math.Sin(_fadeInTimer / 0.25f * ((float)Math.PI / 2f));
			}
			if (_isFadingOut)
			{
				if (num >= 1f)
				{
					_isAmuletGlowing = false;
				}
				num = 1f - num;
			}
			_glowTexture.BaseColor = BaseGlowColor * num;
		}
		_glowTexture.Update(delta);
		_glowTexture.Center = Bbox.Center.Add(GlowOffset);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
		if (_isAmuletGlowing)
		{
			_glowTexture.Draw(spriteBatch);
		}
	}

	internal void Hide()
	{
		_unhiddenAnimationIndex = base.AnimationStart + base.AnimationIndex;
		ChangeAnimation(-1);
		_isAmuletGlowing = false;
		_fadeInTimer = 0f;
	}

	internal void Unhide()
	{
		ChangeAnimation(_unhiddenAnimationIndex);
	}

	internal void StartGlowing()
	{
		_isFadingOut = false;
		_isAmuletGlowing = true;
		_fadeInTimer = 0f;
	}

	internal void StopGlowing()
	{
		_isFadingOut = true;
		_fadeInTimer = 0f;
	}
}
