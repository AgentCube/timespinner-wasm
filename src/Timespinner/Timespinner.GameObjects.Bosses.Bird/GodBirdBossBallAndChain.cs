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

namespace Timespinner.GameObjects.Bosses.Bird;

internal sealed class GodBirdBossBallAndChain : GameEvent
{
	private enum EGodBirdBossBallState
	{
		Sleeping,
		Floating,
		Slam,
		Dying
	}

	private const int FrontChainCount = 44;

	private const int RearChainCount = 14;

	private const int LinkWidth = 6;

	private const int LinkAnimationIndex = 22;

	private const int FarLinkAnimationIndex = 25;

	private const int BallAnimationIndex = 24;

	private const int FarBallAnimationIndex = 27;

	private const int FarBallOffsetX = 198;

	private const int NearBallOffsetX = 168;

	private const int RearBallOffsetX = -70;

	private const int RearBallOffsetY = -17;

	private const int BuriedBallOffsetY = 16;

	private const int BallFloatHeight = 16;

	private const int BallFloatWidth = 8;

	private const float BallFloatFrequency = 4f;

	private const int BallShakeCount = 4;

	private const int BallRaiseWidth = 24;

	private const int BallRaiseHeightNear = 128;

	private const int BallRaiseHeightFar = 124;

	private const int BallReadyFallHeight = 16;

	private const float BallShakeRadius = 1.5f;

	private const float TimeToShakeBalls = 0.5f;

	private const float TimeToRaiseBalls = 0.5f;

	private const float TimeToReadyBalls = 0.25f;

	private const float TimeBeforeReadyingBalls = 1f;

	private const float TimeForEntireSummonBallSequence = 1.25f;

	private const int SlamWindupWidth = 12;

	private const int SlamWindupHeight = 24;

	private const int SlamFloorOffsetY = 0;

	private const float TimeForSlamWindup = 0.5f;

	private const float TimeForSlamDown = 0.25f;

	private const float TimeForSlamWaitInGround = 0.3f;

	private const float TimeForSlamUp = 0.5f;

	private const float TimeForSlamRecover = 0.1f;

	private const float TimeBeforeSlamWaitInGround = 0.75f;

	private const float TimeBeforeSlamUp = 1.05f;

	private const float TimeBeforeSlamRecover = 1.55f;

	internal const float TimeForEntireSlamSequence = 1.65f;

	private const float TimeForDeathFall = 0.75f;

	private const float GlowFrequency = 5f;

	private static readonly Color BallAuraColor = new Color(0.25f, 0.25f, 0.75f, 0.25f);

	private static readonly Color HaloRingBaseColor = new Color(0.9f, 0.9f, 1f, 1f);

	private static readonly Vector4 BaseBallGlowColor = new Vector4(0.9f, 0.95f, 1f, 1f);

	private readonly bool _isFrontPlane;

	private readonly int _ballBaseDamage;

	private readonly int _startX;

	private readonly int _startY;

	private readonly Appendage _parentAppendage;

	private readonly Appendage _anchorLinkAppendage;

	private readonly Appendage _anchorCapAppendage;

	private readonly DamageArea _damageArea;

	private readonly HaloRingAnimation _startingRingAnimation;

	private readonly LandingDustParticleSystem _dustParticles;

	private bool _isAuraActivated;

	private bool _isDrawingHaloRing;

	private EGodBirdBossBallState _ballState;

	private float _glowTimer;

	private float _floatTimer;

	private float _lastSequenceTimer;

	private Point _floatCenter;

	private Point _targetFloatCenter;

	private Point _slamStart;

	private Point _deathStart;

	internal bool IsIdle => _ballState == EGodBirdBossBallState.Floating;

	public GodBirdBossBallAndChain(Level inLevel, SpriteSheet inSprite, Appendage parentAppendage, Point inPosition, int inID, ObjectTileSpecification objectSpec, bool isFrontPlane, bool isRearBall, int damage, bool isFacingLeft)
		: base(inLevel, inPosition, inID, objectSpec)
	{
		_sprite = inSprite;
		_parentAppendage = parentAppendage;
		_isFrontPlane = isFrontPlane;
		_ballBaseDamage = damage;
		ChangeAnimation(isFrontPlane ? 24 : 27);
		_bboxOffset = new Point(1, 8);
		Bbox = new Rectangle(0, 0, 18, 18);
		if (isRearBall)
		{
			Position = new Point(Position.X + -70, Position.Y + -17);
		}
		else
		{
			int y = Position.Y + 16;
			Position = (_isFrontPlane ? new Point(Position.X + 168, y) : new Point(Position.X + 198, y));
		}
		_startX = Position.X;
		_startY = Position.Y;
		SnapBboxToPosition();
		_airDragFactor = 0.05f;
		base.DoesCollideWithTiles = false;
		_isAffectedByGravity = false;
		_isFlying = true;
		base.IsAffectedByTime = true;
		_defaultTeam = ETeamSide.Enemies;
		base.DrawPlane = ((!isFrontPlane) ? EDrawPlane.Back : EDrawPlane.Normal);
		_doesUseAppendageCollision = false;
		_ballState = EGodBirdBossBallState.Sleeping;
		_anchorLinkAppendage = new Appendage(this, new Point(8, 8), Point.Zero, _level, _sprite)
		{
			EndPointOffset = new Point(0, -20)
		};
		_anchorLinkAppendage.ChangeAnimation(-1);
		base.Appendages.Add(_anchorLinkAppendage);
		SetLinks(isRearBall ? 14 : 44, 6);
		_anchorCapAppendage = new Appendage(this, new Point(8, 8), Point.Zero, _level, _sprite)
		{
			IsFacingLeft = isFacingLeft,
			DrawPriority = 1,
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorObject = _anchorLinkAppendage
		};
		if (_isFrontPlane)
		{
			if (isRearBall)
			{
				_anchorCapAppendage.ChangeAnimation(21);
				_anchorCapAppendage.BboxOffset = new Point(4, -8);
			}
			else
			{
				_anchorCapAppendage.ChangeAnimation(20);
				_anchorCapAppendage.BboxOffset = new Point(5, -5);
			}
		}
		else
		{
			_anchorCapAppendage.ChangeAnimation(-1);
		}
		base.Appendages.Add(_anchorCapAppendage);
		_damageArea = new DamageArea(_level, Position, ETeamSide.Enemies, -1, this)
		{
			Power = _ballBaseDamage,
			CanDamageThings = false,
			DamageDimensions = new Point(18, 18),
			AnchorOffset = new Point(0, -8),
			Life = 9999f
		};
		_level.AddProjectile(_damageArea);
		_startingRingAnimation = new HaloRingAnimation(_level)
		{
			Diameter = 80,
			Center = Position,
			BaseDrawColor = HaloRingBaseColor
		};
		_dustParticles = new LandingDustParticleSystem(_level.GCM.TxParticleDust, 10, _level.ID, 100);
		_particleSystems.Add(_dustParticles);
	}

	internal void SetLinks(int linkCount, int linkSize)
	{
		int[] array = new int[linkCount];
		Point[] array2 = new Point[linkCount];
		Point[] array3 = new Point[linkCount];
		Point[] array4 = new Point[linkCount];
		Vector2[] array5 = new Vector2[linkCount];
		int num = (_isFrontPlane ? 22 : 25);
		for (int i = 0; i < linkCount; i++)
		{
			array[i] = num + i % 2;
			ref Point reference = ref array2[i];
			reference = new Point(1, 1);
			ref Point reference2 = ref array3[i];
			reference2 = new Point(3, 0);
			ref Point reference3 = ref array4[i];
			reference3 = new Point(0, 0);
			ref Vector2 reference4 = ref array5[i];
			reference4 = new Vector2(3.5f, 3f);
		}
		_anchorLinkAppendage.AddChainLinks(array, array2, array3, array4, array5, doHingesHaveAngularLimits: true);
		int num2 = 0;
		foreach (Appendage appendage in _anchorLinkAppendage.Appendages)
		{
			appendage.DrawPriority = ((num2 % 2 != 0) ? 1 : (-1));
			num2++;
		}
	}

	internal void SetTargetX(int targetX)
	{
		int y = (_isFrontPlane ? 128 : 124);
		if (targetX < 185)
		{
			targetX = 185;
		}
		_targetFloatCenter = new Point(targetX + 8, y);
	}

	internal void ActivateAura()
	{
		_isAuraActivated = true;
		base.DoesDrawAura = true;
		base.AuraColor = BallAuraColor;
		base.AuraSize = 0.1f;
		base.AuraFrequency = 4f;
		base.DoesDrawAppendageAuras = true;
		base.IsGlowing = true;
		base.GlowBase = 1f;
		UpdateBallGlowing(0f);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_anchorLinkAppendage.Position = _parentAppendage.Position;
			UpdateBallState(delta);
			UpdateBallGlowing(delta);
			_damageArea.ExtendLife(delta);
			if (!_startingRingAnimation.IsFinished)
			{
				if (_isDrawingHaloRing)
				{
					_startingRingAnimation.Update(delta);
				}
			}
			else
			{
				_isDrawingHaloRing = false;
			}
		}
		base.Update(delta);
	}

	private void UpdateBallGlowing(float delta)
	{
		if (_isAuraActivated && _ballState != EGodBirdBossBallState.Dying)
		{
			_glowTimer += delta * 5f;
			if (_glowTimer >= (float)Math.PI * 2f)
			{
				_glowTimer -= (float)Math.PI * 2f;
			}
			Vector4 baseBallGlowColor = BaseBallGlowColor;
			baseBallGlowColor.W = (float)Math.Cos(_glowTimer) * 0.15f + 0.85f;
			base.GlowColor = new Color(baseBallGlowColor);
		}
	}

	private void UpdateBallState(float delta)
	{
		switch (_ballState)
		{
		case EGodBirdBossBallState.Floating:
			UpdateBallFloating(delta);
			break;
		case EGodBirdBossBallState.Sleeping:
		case EGodBirdBossBallState.Slam:
		case EGodBirdBossBallState.Dying:
			break;
		}
	}

	private void UpdateBallFloating(float delta)
	{
		_floatTimer += delta * 4f;
		if (_floatTimer > (float)Math.PI * 2f)
		{
			_floatTimer -= (float)Math.PI * 2f;
		}
		int num = (int)Math.Ceiling(Math.Cos(_floatTimer) * 8.0);
		int num2 = (int)Math.Ceiling(Math.Sin(_floatTimer) * 16.0);
		Position = new Point(_floatCenter.X + num, _floatCenter.Y + num2);
	}

	internal void UpdateBallSlam(float timer)
	{
		int num = (_isFrontPlane ? 128 : 124);
		if (timer >= 0f)
		{
			if (timer <= 0.5f)
			{
				if (timer <= 0f || _lastSequenceTimer < 0f || _lastSequenceTimer > timer)
				{
					_slamStart = Position;
					_ballState = EGodBirdBossBallState.Slam;
					_isDrawingHaloRing = true;
					_startingRingAnimation.Center = Bbox.Center;
					_startingRingAnimation.Reset();
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
					_damageArea.CanDamageThings = true;
					PlayCue(_isFrontPlane ? ESFX.BossBirdChainAttackA : ESFX.BossBirdChainAttackB);
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
					_level.RequestScreenShake(new Vector2(0f, 3f), 0.4f, 6f, isAffectedByTime: true);
					_dustParticles.AddParticles(new Vector2(Position.X, _startY - 12), 100f);
					_damageArea.CanDamageThings = false;
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
			else if (_lastSequenceTimer <= 1.65f)
			{
				_ballState = EGodBirdBossBallState.Floating;
				_floatTimer = -(float)Math.PI / 2f;
			}
		}
		_lastSequenceTimer = timer;
	}

	internal void EndBallSlam()
	{
		_ballState = EGodBirdBossBallState.Floating;
		_floatTimer = -(float)Math.PI / 2f;
	}

	internal void UpdateBallSummon(float timer)
	{
		int num = (_isFrontPlane ? 128 : 124);
		if (timer >= 0f)
		{
			if (timer <= 0.5f)
			{
				if (timer <= 0f || _lastSequenceTimer <= 0f)
				{
					ActivateAura();
					_ballState = EGodBirdBossBallState.Sleeping;
				}
				int num2 = (int)Math.Round(Math.Sin(timer / 0.5f * ((float)Math.PI * 2f) * 4f) * 1.5);
				Position = new Point(_startX, _startY + num2);
			}
			else if (timer <= 1f)
			{
				float num3 = (timer - 0.5f) / 0.5f;
				float num4 = 1f - (float)Math.Cos(num3 * ((float)Math.PI / 2f));
				int num5 = (int)Math.Ceiling(num4 * (float)num);
				int num6 = (int)Math.Ceiling(num4 * 24f);
				Position = new Point(_startX - num6, _startY - num5);
			}
			else if (timer <= 1.25f)
			{
				float num7 = (timer - 1f) / 0.25f;
				num7 = (float)Math.Sin(num7 * (float)Math.PI);
				int num8 = num + (int)Math.Ceiling(num7 * 16f);
				Position = new Point(_startX - 24, _startY - num8);
			}
			else if (_lastSequenceTimer <= 1.25f)
			{
				_floatTimer = 0f;
				_floatCenter = new Point(_startX - 24 - 8, _startY - num);
				_ballState = EGodBirdBossBallState.Floating;
			}
		}
		_lastSequenceTimer = timer;
	}

	internal void UpdateBallDying(float timer)
	{
		if (timer >= 0f)
		{
			if (timer <= 0.75f)
			{
				if (_ballState != EGodBirdBossBallState.Dying)
				{
					_deathStart = Position;
					_ballState = EGodBirdBossBallState.Dying;
					_damageArea.CanDamageThings = false;
				}
				float num = timer / 0.75f;
				int num2 = _startY - _deathStart.Y;
				num = (float)(1.0 - Math.Cos(num * ((float)Math.PI / 2f)));
				num *= num;
				int num3 = (int)Math.Ceiling(num * (float)num2);
				Position = new Point(_deathStart.X, _deathStart.Y + num3);
			}
			else if (_lastSequenceTimer <= 0.75f)
			{
				int num4 = _startY - _deathStart.Y;
				Position = new Point(_deathStart.X, _deathStart.Y + num4);
				_level.RequestScreenShake(new Vector2(0f, 3f), 0.4f, 6f, isAffectedByTime: true);
			}
		}
		_lastSequenceTimer = timer;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (!_startingRingAnimation.IsFinished && _isDrawingHaloRing)
		{
			_startingRingAnimation.Draw(spriteBatch);
		}
		base.Draw(spriteBatch);
	}
}
