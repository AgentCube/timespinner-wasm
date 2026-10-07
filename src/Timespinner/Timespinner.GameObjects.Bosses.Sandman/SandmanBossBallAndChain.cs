using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Sandman;

internal sealed class SandmanBossBallAndChain : DamageArea
{
	private const int Anim_AnchorIndex = 16;

	private const int FloorY = 224;

	private const int BallFloatHeight = 16;

	private const int BallRaiseWidth = 24;

	private const int BallRaiseHeightNear = 128;

	private const int BallRaiseHeightFar = 124;

	private const int SlamWindupWidth = 12;

	private const int SlamWindupHeight = 24;

	private const int SlamFloorOffsetY = 0;

	private const int SlamStartXLeft = 264;

	private const int SlamStartXRight = 440;

	private const int SlamStartY = 80;

	private const float TimeForSlamWindup = 0.5f;

	private const float TimeForSlamDown = 0.25f;

	private const float TimeForSlamWaitInGround = 0.3f;

	private const float TimeForSlamUp = 0.5f;

	private const float TimeForSlamRecover = 0.1f;

	private const float TimeBeforeSlamWaitInGround = 0.75f;

	private const float TimeBeforeSlamUp = 1.05f;

	private const float TimeBeforeSlamRecover = 1.55f;

	internal const float TimeForEntireSlamSequence = 1.65f;

	private static readonly Color HaloRingBaseColor = new Color(0.8f, 0.6f, 0.25f, 1f);

	private readonly bool _isFrontPlane;

	private readonly int _baseDamage;

	private readonly Vector2 _sandTextureRatio = new Vector2(2f, 2f);

	private readonly SandDrawHelper _sandDrawHelper;

	private readonly HaloRingAnimation _startingRingAnimation;

	private readonly SandmanSlamDustParticleSystem _dustParticles;

	private bool _isDrawingHaloRing;

	private bool _isDrawingSand;

	private bool _isFadingIn;

	private int _startY;

	private float _lastSequenceTimer;

	private float _fadeInTimer;

	private Point _slamStart;

	private Point _floatCenter;

	private Point _targetFloatCenter;

	internal bool IsActive { get; set; }

	public SandmanBossBallAndChain(Level inLevel, Point inPosition, SpriteSheet sprite, int baseDamage, bool isFrontPlane)
		: base(inLevel, inPosition, ETeamSide.Enemies, -1, null)
	{
		_baseDamage = (int)Math.Ceiling((float)baseDamage * 1.15f);
		_isFrontPlane = isFrontPlane;
		_sprite = sprite;
		ChangeAnimation(16);
		base.Power = _baseDamage;
		base.CanDamageThings = true;
		_bboxOffset = new Point(4, 7);
		Bbox = new Rectangle(0, 0, 26, 20);
		base.Life = 9999f;
		_sandDrawHelper = new SandDrawHelper(this);
		_startingRingAnimation = new HaloRingAnimation(_level)
		{
			Diameter = 80,
			Center = Position,
			BaseDrawColor = HaloRingBaseColor
		};
		_doesAutomaticallyEmitParticles = false;
		_dustParticles = new SandmanSlamDustParticleSystem(_level.GCM.TxParticleEnergy, 2);
	}

	internal void StartBallSlam()
	{
		IsActive = true;
		base.ID = -1;
		base.Life = 9999f;
		_isFading = false;
		_fadeTimer = 0f;
		_isFadingIn = true;
		_fadeInTimer = 0f;
		_level.RequestAddObject(this);
		int x = (_isFrontPlane ? 264 : 440);
		Point position = new Point(x, 80);
		Position = position;
		SnapBboxToPosition();
	}

	internal void EndBallSlam()
	{
		_isFading = true;
		_life = _timeToFade;
		_fadeTimer = 0f;
	}

	public override void SilentKill()
	{
		IsActive = false;
		base.SilentKill();
	}

	internal void SetTargetX(int targetX)
	{
		int y = (_isFrontPlane ? 128 : 124);
		_targetFloatCenter = new Point(targetX + 8, y);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_isFadingIn)
			{
				_fadeInTimer += delta;
				float num = 1f;
				if (_fadeInTimer >= _timeToFade)
				{
					_isFadingIn = false;
				}
				else
				{
					num = _fadeInTimer / _timeToFade;
				}
				base.DrawColor = Color.White * num;
			}
			_sandDrawHelper.Update(delta);
			_dustParticles.Update(delta);
			if (_isDrawingHaloRing)
			{
				_startingRingAnimation.Update(delta);
			}
		}
		base.Update(delta);
	}

	internal void UpdateSlam(float timer)
	{
		int num = (_isFrontPlane ? 128 : 124);
		_startY = 214;
		_floatCenter.Y = 80;
		if (timer >= 0f)
		{
			if (timer <= 0.5f)
			{
				if (timer <= 0f || _lastSequenceTimer < 0f || _lastSequenceTimer > timer)
				{
					_slamStart = Position;
					_isDrawingHaloRing = true;
					_startingRingAnimation.Center = Bbox.Center;
					_startingRingAnimation.Reset();
					PlayCue(_isFrontPlane ? ESFX.BossSandmanChainA : ESFX.BossSandmanChainB);
				}
				float percentage = timer / 0.5f;
				Position = new Point((int)MathEx.SineInterpolate(_slamStart.X, _targetFloatCenter.X - 12, percentage), (int)MathEx.SineInterpolate(_slamStart.Y, _targetFloatCenter.Y - 24, percentage));
			}
			else if (timer <= 0.75f)
			{
				if (_lastSequenceTimer <= 0.5f)
				{
					_slamStart = Position;
					_doesDrawTrail = true;
					_trailLength = 3;
					_trailFadeRate = 0.95f;
					base.CanDamageThings = true;
				}
				int num2 = _startY - _slamStart.Y;
				float num3 = (timer - 0.5f) / 0.25f;
				int num4 = (int)Math.Ceiling((1.0 - Math.Cos(num3 * ((float)Math.PI / 2f))) * (double)num2);
				Position = new Point(_slamStart.X, _slamStart.Y + num4);
			}
			else if (timer <= 1.05f)
			{
				if (_lastSequenceTimer <= 0.75f)
				{
					_level.RequestScreenShake(new Vector2(0f, 3f), 0.4f, 6f, isAffectedByTime: false);
					_dustParticles.AddParticles(new Vector2(Position.X, 224f));
					base.CanDamageThings = false;
				}
				Position = new Point(_slamStart.X, _startY);
			}
			else if (timer <= 1.55f)
			{
				if (_lastSequenceTimer <= 1.05f)
				{
					_doesDrawTrail = false;
					ClearTrailHistory();
				}
				float num5 = (timer - 1.05f) / 0.5f;
				float num6 = 1f - (float)Math.Cos(num5 * ((float)Math.PI / 2f));
				int num7 = (int)Math.Ceiling(num6 * (float)num);
				int num8 = (int)Math.Ceiling(num6 * 24f);
				Position = new Point(_slamStart.X - num8, _startY - num7);
			}
			else if (timer <= 1.65f)
			{
				if (_lastSequenceTimer <= 1.55f)
				{
					_slamStart = Position;
					_floatCenter.X = Position.X;
				}
				float amount = (timer - 1.55f) / 0.1f;
				Position = _slamStart.SineInterpolate(new Point(_floatCenter.X, _floatCenter.Y - 16), amount);
			}
		}
		_lastSequenceTimer = timer;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (!_isDrawingSand)
		{
			_isDrawingSand = true;
			_sandDrawHelper.Draw(spriteBatch, this, _sandTextureRatio);
			_isDrawingSand = false;
			_dustParticles.Draw(spriteBatch, _level.LevelRenderCenter, _level.CameraPosition, _level.CameraZoom);
			if (_isDrawingHaloRing)
			{
				_startingRingAnimation.Draw(spriteBatch);
			}
		}
		else
		{
			base.Draw(spriteBatch);
		}
	}
}
