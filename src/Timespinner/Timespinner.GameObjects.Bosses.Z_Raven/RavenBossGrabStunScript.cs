using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Gameplay.Scripts;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Bosses.Z_Raven;

internal class RavenBossGrabStunScript : StunScript
{
	private const int WallGirth = 32;

	private readonly int _leftWallX;

	private readonly int _rightWallX;

	private readonly int _damage;

	private bool _isThrowingLeft;

	private float _velocityX;

	private Vector2 _lastTargetVelocity;

	public RavenBossGrabStunScript(Level level, int baseDamage, bool isThrowingLeft)
	{
		_isThrowingLeft = isThrowingLeft;
		base.DoesAllowTimeStopEscape = false;
		base.ActionTimer = 2f;
		_damage = (int)Math.Ceiling((float)baseDamage * 1.1f);
		base.StunAnimationType = EStunAnimationType.Wind;
		_rightWallX = level.RoomSize.X - 32;
		_leftWallX = 32;
	}

	internal override void UpdateStun(float delta, Protagonist target, bool isTimeStopButtonDown)
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
			int num = ((!_isThrowingLeft) ? 1 : (-1));
			target.Velocity = new Vector2(velocity.X + 1000f * delta * (float)num, 0f);
			_velocityX += 33f * delta * (float)num;
			int num2 = target.Position.X + (int)_velocityX;
			if (num2 > _rightWallX)
			{
				num2 = _rightWallX;
			}
			else if (num2 < _leftWallX)
			{
				num2 = _leftWallX;
			}
			target.CollisionSetPosition(new Point(num2, target.Position.Y), target);
			_lastTargetVelocity = velocity;
		}
		base.UpdateStun(delta, target, isTimeStopButtonDown);
	}
}
