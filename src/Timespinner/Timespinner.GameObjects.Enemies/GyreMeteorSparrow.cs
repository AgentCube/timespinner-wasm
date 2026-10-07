using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class GyreMeteorSparrow : Monster
{
	private enum EMeteorSparrowState
	{
		Idle,
		Target,
		ChargeUp,
		Dash,
		Recover,
		Extract
	}

	private const int RoomTop = -32;

	private const int RoomLeft = 0;

	private const int TriangulatedXRadius = 48;

	private const int IdleFloatingRadius = 3;

	private const float IdleFloatingFrequency = 5f;

	private const float TimeToTargetPlayer = 0.5f;

	private const float TimeToChargeUp = 0.5f;

	private const float TimeToRecover = 0.5f;

	private const float TimeToExtract = 1f;

	private const float DashVelocity = 800f;

	private const float ExtractVelocity = 100f;

	private const float RateOfRotation = 5f;

	private const float RateOfRotationSlow = 1.5f;

	private const float ScreenShakeRadius = 2.5f;

	private readonly int _roomRight;

	private readonly int _roomCenterY;

	private EMeteorSparrowState _sparrowState;

	private float _sparrowTimer;

	private float _targetRotation;

	private Point _sparrowTargetPoint;

	private Vector2 _vectorToHero;

	private Vector2 _dashVector;

	public GyreMeteorSparrow(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAction = EAIAction.Custom;
		_nonAggroAction = EAIAction.Custom;
		_currentAI = EAIStrategy.CustomScriptAI;
		_bboxOffset = new Point(16, 9);
		Bbox = new Rectangle(_position.X, _position.Y, 16, 16);
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		DrawOrigin = new Vector2(24f, 17f);
		_agility = 0.6f;
		_isFlying = true;
		_isAffectedByGravity = false;
		_isAffectedByLevelBounds = false;
		_roomRight = _level.RoomSize.X + 32;
		_roomCenterY = _level.RoomSize.Y / 2;
		base.DoesDrawAura = true;
		base.AuraColor = Color.Red * 0.5f;
		base.AuraSize = 0.25f;
		base.AuraFrequency = 9f;
		_doesDrawTrail = true;
		_trailFadeRate = 2f;
		_trailLength = 8;
		_sparrowTargetPoint = Position;
	}

	protected override void UpdateCustomScriptAIAction(float delta)
	{
		_maxMoveSpeed = 800f;
		_canLoseAggro = false;
		if (_isAggroed)
		{
			_sparrowTimer += delta;
			switch (_sparrowState)
			{
			case EMeteorSparrowState.Idle:
				_sparrowState = EMeteorSparrowState.Target;
				_sparrowTimer = 0f;
				PlayCue(ESFX.EnemyMeteorSparrowAimStart);
				break;
			case EMeteorSparrowState.Target:
			{
				if (_sparrowTimer >= 0.5f)
				{
					_sparrowState = EMeteorSparrowState.ChargeUp;
					_sparrowTimer = 0f;
					PlayCue(ESFX.EnemyMeteorSparrowAimLocked);
					ChangeAnimation(new AnimationSpec[3]
					{
						new AnimationSpec
						{
							Start = 0,
							Length = 4,
							Speed = 0.05f,
							Type = EAnimationType.Once
						},
						new AnimationSpec
						{
							Start = 3,
							Length = 0,
							Speed = 0.18f,
							Type = EAnimationType.Once
						},
						new AnimationSpec
						{
							Start = 0,
							Length = 4,
							Speed = 0.05f,
							Type = EAnimationType.Once,
							IsInReverse = true
						}
					});
				}
				_sparrowTargetPoint = _level.GetNearestProtagonistPosition(Position);
				_vectorToHero = new Vector2(_sparrowTargetPoint.X - Position.X, _sparrowTargetPoint.Y - Position.Y);
				float num2 = MathEx.RotationFromVector2(_vectorToHero) - (float)Math.PI / 2f;
				if (_vectorToHero.X < 0f)
				{
					num2 -= (float)Math.PI;
				}
				SetTargetRotation(num2);
				break;
			}
			case EMeteorSparrowState.ChargeUp:
				if (_sparrowTimer >= 0.5f)
				{
					_sparrowState = EMeteorSparrowState.Dash;
					_sparrowTimer = 0f;
					PlayCue(ESFX.EnemyMeteorSparrowDashStart);
					_vectorToHero.Normalize();
					_dashVector = _vectorToHero * 800f;
				}
				break;
			case EMeteorSparrowState.Dash:
				if (Position.X < 0 || Position.X > _roomRight || Position.Y < -32)
				{
					if (Position.X < 0)
					{
						Position = new Point(10, Position.Y);
					}
					else if (Position.X > _roomRight)
					{
						Position = new Point(_roomRight, Position.Y);
					}
					if (Position.Y < -32)
					{
						Position = new Point(Position.X, -32);
					}
					EndDash();
				}
				else
				{
					base.Velocity = _dashVector;
				}
				break;
			case EMeteorSparrowState.Recover:
				if (_sparrowTimer >= 0.5f)
				{
					_sparrowState = EMeteorSparrowState.Extract;
					_sparrowTimer = 0f;
					_dashVector = -_vectorToHero * 100f;
					_sparrowTargetPoint = Position;
				}
				break;
			case EMeteorSparrowState.Extract:
			{
				if (_sparrowTimer >= 1f)
				{
					_sparrowState = EMeteorSparrowState.Idle;
					_sparrowTimer = 0f;
					break;
				}
				float amount = _sparrowTimer / 1f;
				float num = _vectorToHero.X * 48f;
				int x = (int)Math.Ceiling(MathHelper.Lerp(_sparrowTargetPoint.X, (float)_sparrowTargetPoint.X - num, amount));
				int y = (int)Math.Ceiling(MathHelper.Lerp(_sparrowTargetPoint.Y, _roomCenterY, amount));
				Position = new Point(x, y);
				SetTargetRotation(0f);
				break;
			}
			}
		}
		else
		{
			_sparrowTimer += delta * 5f;
			if (_sparrowTimer >= (float)Math.PI * 2f)
			{
				_sparrowTimer -= (float)Math.PI * 2f;
			}
			int num3 = (int)Math.Ceiling(Math.Sin(_sparrowTimer) * 3.0);
			Position = new Point(_sparrowTargetPoint.X, _sparrowTargetPoint.Y + num3);
		}
		if (Math.Abs(base.Rotation - _targetRotation) > 0.01f)
		{
			float num4 = ((_sparrowState == EMeteorSparrowState.Target) ? 5f : 1.5f);
			if (base.Rotation < _targetRotation)
			{
				base.Rotation += num4 * delta;
				if (base.Rotation > _targetRotation)
				{
					base.Rotation = _targetRotation;
				}
			}
			else if (base.Rotation > _targetRotation)
			{
				base.Rotation -= num4 * delta;
				if (base.Rotation < _targetRotation)
				{
					base.Rotation = _targetRotation;
				}
			}
		}
		base.UpdateCustomScriptAIAction(delta);
	}

	private void SetTargetRotation(float targetRotation)
	{
		float num = targetRotation + (float)Math.PI * 2f;
		float value = base.Rotation - targetRotation;
		float value2 = base.Rotation - num;
		if (Math.Abs(value) > Math.Abs(value2))
		{
			_targetRotation = num;
		}
		else
		{
			_targetRotation = targetRotation;
		}
	}

	private void EndDash()
	{
		_sparrowState = EMeteorSparrowState.Recover;
		_sparrowTimer = 0f;
		_velocity = Vector2.Zero;
		_level.RequestScreenShake(-_vectorToHero * 2.5f, 0.33f, 10f, isAffectedByTime: true);
		PlayCue(ESFX.EnemyMeteorSparrowDashImpact);
		PlayCue2D(ESFX.EnemyMeteorSparrowDashImpact2D);
	}

	public override bool CollideSolidTile(Tile tile, Vector2 depth)
	{
		bool flag = base.CollideSolidTile(tile, depth);
		if (flag && _sparrowState == EMeteorSparrowState.Dash)
		{
			OnDashCollide(tile.Bbox);
		}
		return flag;
	}

	public override bool CollideSolidObject(Animate target, ETileType tileType, Vector2 depth)
	{
		bool flag = base.CollideSolidObject(target, tileType, depth);
		if (flag && _sparrowState == EMeteorSparrowState.Dash)
		{
			OnDashCollide(target.Bbox);
		}
		return flag;
	}

	private void OnDashCollide(Rectangle targetBbox)
	{
		EndDash();
		bool flag = targetBbox.Center.Y > Position.Y;
		_vectorToHero -= new Vector2(0f, (!flag) ? 1 : (-1));
		Point intersectionCenter = RectangleExtensions.GetIntersectionCenter(Bbox, targetBbox);
		_level.AddAnimation(EBattleAnimationType.DustBoom, intersectionCenter, ETeamSide.Neutral, IsFacingLeft);
	}
}
