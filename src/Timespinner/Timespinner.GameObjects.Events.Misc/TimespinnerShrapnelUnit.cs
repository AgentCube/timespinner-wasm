using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.Misc;

internal class TimespinnerShrapnelUnit : Appendage
{
	private const int Anim_Start = 13;

	private const int Anim_Length = 8;

	private const float TimeToAbsorb = 0.25f;

	private const float FadeThreshold = 0.5f;

	private readonly float _radius;

	private readonly float _trig;

	private float _absorbTimer;

	internal bool IsAbsorbing { get; set; }

	internal float RadiusMultiplier { get; private set; }

	internal float TrigValue { get; private set; }

	public TimespinnerShrapnelUnit(Animate parent, Level inLevel, SpriteSheet inSprite, float radius, float trig)
		: base(parent, new Point(4, 4), Point.Zero, inLevel, inSprite)
	{
		_radius = radius;
		_trig = trig;
		base.DrawPriority = 1;
		base.FollowType = EAppendageFollowType.AnchorLocked;
		base.DoesInheritDrawColor = false;
		int num = _level.NextRandomInt(0, 2);
		ChangeAnimation(13 + num * 8, 8, 0.1f, EAnimationType.Cycle);
		RadiusMultiplier = _radius;
		TrigValue = _trig;
		base.DoesDrawTrail = true;
		_trailLength = 4;
		_trailFadeRate = 2f;
		base.GlowColor = new Color(0.5f, 0.5f, 0.5f, 0.25f);
	}

	public override void Update(float delta)
	{
		if (IsAbsorbing)
		{
			_absorbTimer += delta;
			if (_absorbTimer < 0.25f)
			{
				float num = _absorbTimer / 0.25f;
				float num2 = 1f - num;
				RadiusMultiplier = _radius * num2;
				if (RadiusMultiplier < 0.5f)
				{
					base.IsGlowing = false;
					float num3 = RadiusMultiplier * 2f;
					base.DrawColor = Color.White * num3;
				}
				else
				{
					base.IsGlowing = true;
					base.GlowBase = MathEx.SineInterpolate(1f, 8f, num * 2f);
				}
			}
			else
			{
				_absorbTimer = 0f;
				RadiusMultiplier = _radius;
			}
		}
		base.Update(delta);
	}
}
