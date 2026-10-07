using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Z_Xarion;

internal sealed class XarionBossMoth : GameEvent
{
	private const int Anim_Start = 0;

	private const int Anim_Length = 5;

	private const int Anim_GlowIndex = 10;

	private const int FlapCueIndex = 2;

	private const int GlowAppendageCount = 4;

	private const int IntroRingAppendageCount = 2;

	private const int Anim_IntroRingIndex = 19;

	private const int HaloCount = 3;

	private const int FlightWidth = 12;

	private const int FlightHeight = 20;

	private const int MothOffsetX = 0;

	private const int MothOffsetY = -80;

	private const int RecoilRadius = 8;

	private const float RecoilDuration = 0.15f;

	private const float MinimumFlightDuration = 0.25f;

	private const float MaximumFlightDuration = 0.25f;

	private const float TimeForChargeFadeOut = 0.25f;

	private const float TimeForLazerCharging = 0.6f;

	private const float TimeForIntroRing = 0.39600003f;

	private const float TimeBeforeSecondIntroRing = 0.204f;

	private const float GlowBallFinalSize = 0.75f;

	private const float RotationSpeed = 5f;

	private const float RingStartSize = 1.5f;

	private const float RingEndSize = 0.15f;

	private static readonly Color GlowBallFinalColor = new Color(96, 8, 64, 8);

	private static readonly Vector4 GlowChargeLazerColor = new Vector4(0.8f, 0.2f, 0.8f, 0.15f);

	private static readonly Color RingColor = new Color(0.5f, 0.125f, 0.4f, 0.15f);

	private readonly Point _parentPosition;

	private readonly Point _circleCenter;

	private readonly HaloRingAnimation[] _haloRings = new HaloRingAnimation[3];

	private readonly XarionChargeLazerParticleSystem _lazerChargeParticles;

	private readonly Appendage[] _glowAppendages = new Appendage[4];

	private readonly Appendage[] _introRingAppendages = new Appendage[2];

	private bool _isRecoiling;

	private bool _isChargingLazer;

	private bool _isChargeLazerFlying;

	private bool _isChargeFadingOut;

	private bool _isShowingHaloRing;

	private bool _hasDoneFirstSnap;

	private float _flightTimer;

	private float _flightDuration;

	private float _lazerChargeTimer;

	private Point _flightTarget;

	private Point _flightStart;

	internal Point CircleCenter => _circleCenter;

	public XarionBossMoth(Level inLevel, Point inPosition, ObjectTileSpecification objectSpec)
		: base(inLevel, inPosition, -1, objectSpec)
	{
		_parentPosition = inPosition;
		_sprite = _level.GCM.SpCursedMoth;
		ChangeAnimation(0, 5, 0.075f, EAnimationType.Cycle);
		Bbox = new Rectangle(0, 0, 40, 32);
		IsFacingLeft = true;
		base.IsAffectedByTime = true;
		_isSolid = false;
		base.CanBeTriggered = false;
		_isAffectedByGravity = false;
		_isFlying = true;
		_lazerChargeParticles = new XarionChargeLazerParticleSystem(_level.GCM.TxParticleEnergy, 16);
		_particleSystems.Add(_lazerChargeParticles);
		_circleCenter = new Point(_parentPosition.X, _parentPosition.Y + -80);
		float num = 0f;
		for (int i = 0; i < 4; i++)
		{
			Appendage appendage = new Appendage(this, new Point(31, 31), Point.Zero, _level, _sprite)
			{
				FollowType = EAppendageFollowType.AnchorLocked,
				AnchorObject = this,
				AnchorOffset = new Point(15, -15),
				DoesInheritDrawColor = false,
				Rotation = num,
				DrawOrigin = new Vector2(0f, 31f),
				DrawPriority = -1,
				DrawColor = Color.Transparent
			};
			appendage.ChangeAnimation(10);
			_glowAppendages[i] = appendage;
			_appendages.Add(appendage);
			num += (float)Math.PI / 2f;
		}
		SpriteSheet spXarionBoss = _level.GCM.SpXarionBoss;
		for (int j = 0; j < 2; j++)
		{
			Appendage appendage2 = new Appendage(this, new Point(64, 64), Point.Zero, _level, spXarionBoss)
			{
				FollowType = EAppendageFollowType.AnchorLocked,
				AnchorObject = this,
				AnchorOffset = new Point(0, 32),
				DrawPriority = 1,
				DrawOrigin = new Vector2(32f, 24f),
				DoesInheritDrawColor = false
			};
			appendage2.ChangeAnimation(-1);
			_introRingAppendages[j] = appendage2;
			_appendages.Add(appendage2);
		}
		for (int k = 0; k < 3; k++)
		{
			_haloRings[k] = new HaloRingAnimation(_level);
		}
		StartNewFlightPhase();
	}

	public override void Update(float delta)
	{
		UpdateLazerCharging(delta);
		UpdateFlying(delta);
		if (_isShowingHaloRing)
		{
			HaloRingAnimation[] haloRings = _haloRings;
			foreach (HaloRingAnimation haloRingAnimation in haloRings)
			{
				if (haloRingAnimation.HasStarted)
				{
					haloRingAnimation.Update(delta);
				}
			}
		}
		int animationIndex = base.AnimationIndex;
		base.Update(delta);
		if (base.IsWithinObjectVisibleArea && animationIndex != base.AnimationIndex && base.AnimationIndex == 2)
		{
			PlayCue(ESFX.EnemyPoisonMothWingFlap);
		}
	}

	private void UpdateFlying(float delta)
	{
		_flightTimer += delta;
		if (!_hasDoneFirstSnap)
		{
			_hasDoneFirstSnap = true;
			Position = _flightTarget;
			StartNewFlightPhase();
		}
		if (_flightTimer >= _flightDuration)
		{
			Position = _flightTarget;
			if (!_isChargeLazerFlying)
			{
				if (_isRecoiling)
				{
					StartNewFlightPhase();
				}
				else
				{
					DoRecoil();
				}
			}
		}
		else
		{
			float amount = _flightTimer / _flightDuration;
			if (!_isRecoiling)
			{
				Position = _flightStart.Lerp(_flightTarget, amount);
			}
			else
			{
				Position = _flightStart.SineInterpolate(_flightTarget, amount);
			}
		}
	}

	private void UpdateLazerCharging(float delta)
	{
		_lazerChargeParticles.BaseColor = GlowChargeLazerColor;
		if (_isChargingLazer)
		{
			_lazerChargeTimer += delta;
			if (_lazerChargeTimer < 0.6f)
			{
				_lazerChargeParticles.AddParticles(Bbox.Center.ToVector2());
				float num = _lazerChargeTimer / 0.6f;
				Color drawColor = Color.Transparent.SineInterpolate(GlowBallFinalColor, num);
				float scale = MathEx.SineInterpolate(0.1f, 0.75f, num);
				Appendage[] glowAppendages = _glowAppendages;
				foreach (Appendage appendage in glowAppendages)
				{
					appendage.DrawColor = drawColor;
					appendage.Scale = scale;
				}
				UpdateIntroRing(0, _lazerChargeTimer, delta);
				UpdateIntroRing(1, _lazerChargeTimer - 0.204f, delta);
			}
			else
			{
				_isChargingLazer = false;
			}
		}
		if (_isChargeFadingOut)
		{
			_lazerChargeTimer += delta;
			Color drawColor2 = Color.Transparent;
			if (_lazerChargeTimer < 0.25f)
			{
				float amount = _lazerChargeTimer / 0.25f;
				drawColor2 = GlowBallFinalColor.CosInterpolate(Color.Transparent, amount);
			}
			else
			{
				_isChargeFadingOut = false;
			}
			Appendage[] glowAppendages2 = _glowAppendages;
			foreach (Appendage appendage2 in glowAppendages2)
			{
				appendage2.DrawColor = drawColor2;
			}
		}
	}

	private void UpdateIntroRing(int ringIndex, float ringTimer, float delta)
	{
		Appendage appendage = _introRingAppendages[ringIndex];
		if (ringTimer >= 0f)
		{
			if (appendage.AnimationStart == -1)
			{
				appendage.ChangeAnimation(19);
			}
			if (ringTimer < 0.39600003f)
			{
				float num = ringTimer / 0.39600003f;
				appendage.Scale = MathEx.CosInterpolate(1.5f, 0.15f, num);
				appendage.Rotation += 5f * delta;
				float amount = (float)Math.Sin(num * (float)Math.PI);
				appendage.DrawColor = Color.Transparent.Lerp(RingColor, amount);
			}
			else
			{
				appendage.ChangeAnimation(-1);
			}
		}
	}

	private void StartNewFlightPhase()
	{
		_isRecoiling = false;
		_flightTimer = 0f;
		_flightDuration = 0.25f + (float)(_level.NextRandomDouble() * 0.0);
		_flightStart = Position;
		float num = (float)_level.NextRandomDouble() * ((float)Math.PI * 2f);
		int num2 = (int)Math.Round(Math.Cos(num) * 12.0);
		int num3 = (int)Math.Round(Math.Sin(num) * 20.0);
		_flightTarget = new Point(_circleCenter.X + num2, _circleCenter.Y + num3);
		UpdateFlying(0f);
	}

	private void DoRecoil()
	{
		_isRecoiling = true;
		_flightTimer = 0f;
		_flightDuration = 0.15f;
		Vector2 b = new Vector2(_flightStart.X - _flightTarget.X, _flightStart.Y - _flightTarget.Y);
		b.Normalize();
		b *= 8f;
		_flightStart = _flightTarget;
		_flightTarget = _flightTarget.Add(b);
	}

	internal void StartLazerCharge()
	{
		_lazerChargeTimer = 0f;
		_isChargingLazer = true;
		_isChargeFadingOut = false;
		_isChargeLazerFlying = true;
		_flightStart = Position;
		_flightTarget = _circleCenter;
		_flightTimer = 0f;
		_flightDuration = 0.6f;
	}

	internal void EndLazerCharge()
	{
		_isChargeLazerFlying = false;
		StartNewFlightPhase();
		_isChargeFadingOut = true;
		_isChargingLazer = false;
		_lazerChargeTimer = 0f;
	}

	internal void EmitHalo()
	{
		_isShowingHaloRing = true;
		HaloRingAnimation[] haloRings = _haloRings;
		foreach (HaloRingAnimation haloRingAnimation in haloRings)
		{
			if (!haloRingAnimation.HasStarted || haloRingAnimation.IsFinished)
			{
				haloRingAnimation.Reset();
				haloRingAnimation.Start();
				haloRingAnimation.Center = Bbox.Center;
				break;
			}
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
		if (_isShowingHaloRing)
		{
			HaloRingAnimation[] haloRings = _haloRings;
			foreach (HaloRingAnimation haloRingAnimation in haloRings)
			{
				haloRingAnimation.Draw(spriteBatch);
			}
		}
	}
}
