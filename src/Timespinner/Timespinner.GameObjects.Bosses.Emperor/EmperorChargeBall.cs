using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Emperor;

internal sealed class EmperorChargeBall : Appendage
{
	private const int Anim_StartIndex = 32;

	private const int Anim_Length = 4;

	private const float Anim_Speed = 0.075f;

	private const float TimeToGrow = 0.75f;

	private const float FinalScale = 1f;

	private const float FinalScaleUnder = 1.25f;

	private readonly Appendage _overBall;

	private readonly Appendage _underBall;

	private readonly Animate _parent;

	private float _chargeTimer;

	private Color _spellParticleColor;

	private Color _spellTrailColor;

	internal bool IsFullSize { get; private set; }

	public EmperorChargeBall(Animate parent, Level inLevel, SpriteSheet inSprite)
		: base(parent, new Point(20, 20), Point.Zero, inLevel, inSprite)
	{
		_parent = parent;
		DrawOrigin = new Vector2(10f, 10f);
		_doAppendagesInheritDrawColor = false;
		_overBall = new Appendage(this, new Point(20, 20), Point.Zero, _level, inSprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorObject = this,
			AnchorOffset = Point.Zero,
			DrawPriority = 1,
			DrawOrigin = new Vector2(10f, 10f)
		};
		_underBall = new Appendage(this, new Point(20, 20), Point.Zero, _level, inSprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorObject = this,
			AnchorOffset = Point.Zero,
			DrawPriority = -1,
			DrawOrigin = new Vector2(10f, 10f)
		};
		base.Appendages.Add(_underBall);
		base.Appendages.Add(_overBall);
		ChangeAnimation(32);
		_overBall.ChangeAnimation(32);
		_underBall.ChangeAnimation(32);
	}

	internal void Reset(Color particleColor, Color trailColor)
	{
		_chargeTimer = 0f;
		_spellParticleColor = particleColor;
		IsFullSize = false;
		_spellTrailColor = new Color(trailColor.R / 2, trailColor.G / 2, trailColor.B / 2, trailColor.A);
		ChangeAnimation(32, 4, 0.075f, EAnimationType.Cycle);
		_overBall.ChangeAnimation(32, 4, 0.075f, EAnimationType.Cycle);
		_underBall.ChangeAnimation(32, 4, 0.075f, EAnimationType.Cycle, 2, 2, 0.075f);
		Update(0f);
		PlayCue(ESFX.BossEmperorChargeStart);
	}

	public override void Update(float delta)
	{
		_chargeTimer += delta;
		float num = 1f;
		if (_chargeTimer < 0.75f)
		{
			num = _chargeTimer / 0.75f;
			num = (float)Math.Sin(num * ((float)Math.PI / 2f));
		}
		if (num >= 1f)
		{
			IsFullSize = true;
		}
		_scale = num * 1f;
		_overBall.Scale = num * 1.25f;
		_underBall.Scale = _overBall.Scale;
		base.DrawColor = _spellParticleColor * num;
		_overBall.DrawColor = _spellTrailColor * num;
		_underBall.DrawColor = _overBall.DrawColor;
		Point position = _parent.Position;
		Position = new Point(position.X, position.Y + -40);
		SnapBboxToPosition();
		base.Update(delta);
	}
}
