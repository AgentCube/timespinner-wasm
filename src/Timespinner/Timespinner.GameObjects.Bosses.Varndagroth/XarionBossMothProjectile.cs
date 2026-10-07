using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Bosses.Varndagroth;

internal sealed class XarionBossMothProjectile : Projectile
{
	private const float ColorOscillationFrequency = 100f;

	private const float VerticalVelocityMagnitude = 600f;

	private const float VerticalVelocityOscillationFrequency = 15f;

	private const float MaxLife = 1.5f;

	private const float LazerRotationTheta = 0.5105088f;

	private static readonly Color MothColorA = new Color(96, 160, 64, 16);

	private static readonly Color MothColorB = new Color(255, 32, 240, 16);

	private readonly int _startingPower;

	private readonly Matrix _rotationMatrix;

	private bool _isShootingHorizontal;

	private float _mothTimer;

	internal bool IsFinished { get; private set; }

	public XarionBossMothProjectile(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, SpriteSheet sprite, int baseDamage)
		: base(inLevel, inPosition, iV, inSide, -1)
	{
		_sprite = sprite;
		_bboxOffset = new Point(10, 9);
		Bbox = new Rectangle(_position.X, _position.Y, 16, 16);
		_startingPower = (int)Math.Ceiling(1.2f * (float)baseDamage);
		_power = _startingPower;
		_force = 0;
		_life = 1.5f;
		_timeToFade = 0f;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_animationSpeed = 0f;
		_maxMoveSpeed = 5000f;
		_maxFallSpeed = 5000f;
		_isFlying = true;
		base.DoesCollideWithTiles = false;
		_doesDieOnTiles = false;
		_doesCollideWithFloors = false;
		_doesCollideWithWalls = false;
		_doesCollideWithCeilings = false;
		_isIgnoringPlatform = true;
		base.DoesDieOnImpact = false;
		base.DoesDieToEnemyProjectiles = false;
		_doesLifetimeAffectAlpha = false;
		_doesRotateBasedOnVelocity = false;
		_isTrailLengthAffectedByTime = false;
		_doesDrawTrail = true;
		_trailLength = 8;
		_trailFadeRate = 1.5f;
		_doesDieOutsideOfVisibleArea = false;
		ChangeAnimation(0, 5, 0.05f, EAnimationType.Cycle);
		_animationIndex = _level.NextRandomInt(0, 4);
		float num = (float)Math.Cos(0.5105088016891912);
		float num2 = (float)Math.Sin(0.5105088016891912);
		_rotationMatrix = Matrix.Identity;
		_rotationMatrix.M11 = num;
		_rotationMatrix.M12 = 0f - num2;
		_rotationMatrix.M21 = num2;
		_rotationMatrix.M22 = num;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_mothTimer += delta;
			float amount = (float)(Math.Sin(_mothTimer * 100f + 1f) / 2.0);
			base.DrawColor = MothColorA.Lerp(MothColorB, amount);
			float y = (0f - (float)Math.Sin(_mothTimer * 15f)) * 600f;
			_velocity = new Vector2(_initialVector.X, y);
			if (!_isShootingHorizontal)
			{
				_velocity = Vector2.Transform(_velocity, _rotationMatrix);
			}
		}
		base.Update(delta);
	}

	public override void SilentKill()
	{
		IsFinished = true;
		base.SilentKill();
	}

	internal void Reset(Point position, Vector2 iV, bool isHorizontal)
	{
		_isShootingHorizontal = isHorizontal;
		_mothTimer = (float)(_level.NextRandomDouble() * 6.2831854820251465);
		Position = position;
		_initialVector = iV;
		_velocity = iV;
		SnapBboxToPosition();
		base.ID = -1;
		_power = _startingPower;
		_isFading = false;
		_life = 1.5f;
		IsFinished = false;
		ClearTrailHistory();
	}
}
