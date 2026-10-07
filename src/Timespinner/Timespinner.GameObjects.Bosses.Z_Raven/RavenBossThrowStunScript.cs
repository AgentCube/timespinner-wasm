using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Gameplay.Scripts;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Bosses.Z_Raven;

internal class RavenBossThrowStunScript : StunScript
{
	private const int WallGirth = 32;

	private const float TimeToMatchPosition = 0.2f;

	private readonly int _leftWallX;

	private readonly int _rightWallX;

	private readonly int _damage;

	private readonly Animate _parentObject;

	private bool _isThrowingLeft;

	private bool _hasThrownPlayer;

	private bool _hasStartedMatchingPosition;

	private float _velocityX;

	private float _positionMatchTimer;

	private Point _playerStartingOffset;

	private Vector2 _lastTargetVelocity;

	public RavenBossThrowStunScript(Level level, int baseDamage, Animate parent)
	{
		_parentObject = parent;
		base.DoesAllowTimeStopEscape = false;
		base.ActionTimer = 10f;
		_damage = (int)Math.Ceiling((float)baseDamage * 1.1f);
		base.StunAnimationType = EStunAnimationType.Held;
		_rightWallX = level.RoomSize.X - 32;
		_leftWallX = 32;
	}

	internal void ThrowPlayer(bool isThrowingLeft)
	{
		_isThrowingLeft = isThrowingLeft;
		_hasThrownPlayer = true;
		base.ActionTimer = 2f;
	}

	internal void Cancel()
	{
		base.ActionTimer = 0f;
	}

	internal override void UpdateStun(float delta, Protagonist target, bool isTimeStopButtonDown)
	{
		if (!_hasThrownPlayer)
		{
			Point position = _parentObject.Position;
			if (!_hasStartedMatchingPosition)
			{
				_hasStartedMatchingPosition = true;
				_playerStartingOffset = new Point(target.Position.X - position.X, target.Position.Y - position.Y);
			}
			if (_positionMatchTimer < 0.2f)
			{
				_positionMatchTimer += delta;
				float num = 1f - _positionMatchTimer / 0.2f;
				target.Position = MathEx.Add(b: new Point((int)Math.Round((float)_playerStartingOffset.X * num), (int)Math.Round((float)_playerStartingOffset.Y * num)), a: position);
			}
			else
			{
				target.Position = position;
			}
			target.SnapBboxToPosition();
			if (target.Bbox.Right >= _rightWallX)
			{
				target.Position = new Point(_rightWallX - 8, target.Position.Y);
			}
			if (target.Bbox.Left <= _leftWallX)
			{
				target.Position = new Point(_leftWallX + 8, target.Position.Y);
			}
		}
		else
		{
			Vector2 velocity = target.Velocity;
			if (target.Bbox.Right >= _rightWallX || target.Bbox.Left <= _leftWallX || (Math.Abs(_lastTargetVelocity.X) > 10f && Math.Abs(velocity.X) < 1f && base.ActionTimer < 1.9f))
			{
				base.ActionTimer = 0f;
				Point where = new Point(target.Bbox.Right, target.Bbox.Top);
				target.ManageDamage(sourceRectangle: new Rectangle(where.X, where.Y, 16, 16), damage: _damage, velocity: new Vector2(1f, 0f), where: where, type: EDamageType.Squished, element: EDamageElement.Blunt, doesKnockBack: false);
				target.Level.RequestScreenShake(new Vector2(6f, 0f), 0.25f, 10f, isAffectedByTime: true);
			}
			else
			{
				int num2 = ((!_isThrowingLeft) ? 1 : (-1));
				target.Velocity = new Vector2(velocity.X + 1000f * delta * (float)num2, 0f);
				_velocityX += 33f * delta * (float)num2;
				int num3 = target.Position.X + (int)_velocityX;
				if (num3 > _rightWallX)
				{
					num3 = _rightWallX;
				}
				else if (num3 < _leftWallX)
				{
					num3 = _leftWallX;
				}
				target.CollisionSetPosition(new Point(num3, target.Position.Y), target);
				_lastTargetVelocity = velocity;
			}
		}
		base.UpdateStun(delta, target, isTimeStopButtonDown);
	}
}
