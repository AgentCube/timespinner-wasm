using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Gameplay.Scripts;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Bosses.Bird;

internal class GodBirdGustStunScript : StunScript
{
	private readonly int _rightWallX;

	private readonly int _damage;

	private readonly BirdBossWallPebblesParticleSystem _pebbleParticles;

	private readonly Level _level;

	private bool _hasPlayedPushCue;

	private float _velocityX;

	private Vector2 _lastTargetVelocity;

	public GodBirdGustStunScript(Level level, int baseDamage, BirdBossWallPebblesParticleSystem pebbleParticles)
	{
		_level = level;
		_pebbleParticles = pebbleParticles;
		base.DoesAllowTimeStopEscape = false;
		base.ActionTimer = 2f;
		_damage = (int)Math.Ceiling((float)baseDamage * 1.1f);
		base.StunAnimationType = EStunAnimationType.Wind;
		_rightWallX = level.RoomSize.X - 16;
	}

	internal override void UpdateStun(float delta, Protagonist target, bool isTimeStopButtonDown)
	{
		if (!_hasPlayedPushCue)
		{
			_hasPlayedPushCue = true;
			target.PlayCue(ESFX.BossBirdAuraPush);
		}
		Vector2 velocity = target.Velocity;
		if (target.Bbox.Right >= _rightWallX || (Math.Abs(_lastTargetVelocity.X) > 10f && Math.Abs(velocity.X) < 1f && base.ActionTimer < 1.9f))
		{
			base.ActionTimer = 0f;
			Point point = new Point(target.Bbox.Right, target.Bbox.Top);
			target.ManageDamage(sourceRectangle: new Rectangle(point.X, point.Y, 16, 16), damage: _damage, velocity: new Vector2(1f, 0f), where: point, type: EDamageType.Squished, element: EDamageElement.Blunt, doesKnockBack: false);
			target.Level.RequestScreenShake(new Vector2(6f, 0f), 0.25f, 10f, isAffectedByTime: true);
			target.PlayCue(ESFX.BossBirdAuraWallHit);
			_level.AddAnimation(new BattleAnimation(null, point, _level)
			{
				ParticleSystem = _pebbleParticles,
				TeamSide = ETeamSide.Neutral,
				DrawPlane = EDrawPlane.Normal
			});
		}
		else
		{
			target.Velocity = new Vector2(velocity.X + 1000f * delta, 0f);
			_velocityX += 33f * delta;
			int num = target.Position.X + (int)_velocityX;
			if (num > _rightWallX)
			{
				num = _rightWallX;
			}
			target.CollisionSetPosition(new Point(num, target.Position.Y), target);
			_lastTargetVelocity = velocity;
		}
		base.UpdateStun(delta, target, isTimeStopButtonDown);
	}
}
