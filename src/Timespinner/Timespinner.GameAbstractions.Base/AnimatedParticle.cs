using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameAbstractions.Base;

public class AnimatedParticle : Particle
{
	private bool _isInReverse;

	private int _lastFrame = -1;

	public bool IsAnimationDone;

	public Rectangle FrameSource;

	public bool IsFacingLeft { get; set; }

	public bool IsFlippedVertically { get; set; }

	public EAnimationType AnimationType { get; set; }

	public int AnimationIndex { get; set; }

	public int AnimationStart { get; set; }

	public int AnimationEnd { get; set; }

	public int AnimationLength { get; set; }

	public float AnimationSpeed { get; set; }

	public float AnimationCounter { get; set; }

	public SpriteSheet Sprite { get; set; }

	public override void Initialize(Vector2 position, Vector2 velocity, Vector2 acceleration, float lifetime, float scale, float scaleGrowthRate, float rotationSpeed, float rotation, float friction)
	{
		AnimationEnd = AnimationStart + AnimationLength;
		base.Initialize(position, velocity, acceleration, lifetime, scale, scaleGrowthRate, rotationSpeed, rotation, friction);
	}

	public override void Update(float delta)
	{
		AnimationIncrement(delta);
		if (Sprite != null)
		{
			int num = AnimationStart + AnimationIndex;
			if (_lastFrame != num)
			{
				FrameSource = Sprite.GetFrameSource(num);
			}
			_lastFrame = num;
		}
		base.Update(delta);
	}

	protected void AnimationIncrement(float delta)
	{
		AnimationCounter += delta;
		if (!(AnimationCounter >= AnimationSpeed))
		{
			return;
		}
		AnimationCounter = ((AnimationSpeed < delta) ? 0f : (AnimationCounter - AnimationSpeed));
		switch (AnimationType)
		{
		case EAnimationType.Cycle:
			AnimationIndex++;
			if (AnimationIndex >= AnimationLength)
			{
				AnimationIndex = 0;
			}
			else if (AnimationIndex < 0)
			{
				AnimationIndex = 0;
			}
			break;
		case EAnimationType.Once:
			if (!IsAnimationDone)
			{
				AnimationIndex++;
				if (AnimationIndex >= AnimationLength)
				{
					AnimationIndex = AnimationLength - 1;
					IsAnimationDone = true;
				}
				else if (AnimationIndex < 0)
				{
					AnimationIndex = 0;
				}
			}
			break;
		case EAnimationType.PingPong:
			if (!_isInReverse)
			{
				AnimationIndex++;
			}
			else
			{
				AnimationIndex--;
			}
			if (AnimationIndex >= AnimationLength)
			{
				AnimationIndex = AnimationLength - 1;
				_isInReverse = true;
			}
			else if (AnimationIndex < 0)
			{
				AnimationIndex = 0;
				_isInReverse = false;
			}
			break;
		default:
			AnimationIndex = 0;
			break;
		}
	}
}
