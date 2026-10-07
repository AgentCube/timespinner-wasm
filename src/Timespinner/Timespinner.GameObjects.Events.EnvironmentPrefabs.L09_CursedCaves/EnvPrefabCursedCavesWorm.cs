using Microsoft.Xna.Framework;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Events.EnvironmentPrefabs.L09_CursedCaves;

internal sealed class EnvPrefabCursedCavesWorm : EnvironmentPrefabBase
{
	private const int Anim_Start = 3;

	private const int Anim_Length = 6;

	private const int JiggerRadius = 4;

	private const float MinimumAnimationChangeTime = 0.1f;

	private const float MaximumAnimationChangeTime = 2f;

	private const float Anim_SpeedStart = 0.1f;

	private const float Anim_SpeedJigger = 0.2f;

	private const float ChanceToSitStill = 0.25f;

	private const float StoppedAnimationSpeed = 10f;

	private float _animationSpeedChangeTimer;

	public EnvPrefabCursedCavesWorm(Level inLevel, Point inPosition, int inID, ObjectTileSpecification objectSpec, EEnvironmentPrefabType prefabType)
		: base(inLevel, inPosition, inID, objectSpec, prefabType)
	{
		_sprite = _level.GCM.SpXarionBoss;
		ChangeAnimation(3, 6, 0.1f, EAnimationType.PingPong);
		_animationIndex = _level.NextRandomInt(0, 5);
		base.DrawPlane = EDrawPlane.Front;
		Position = new Point(Position.X + _level.NextRandomInt(-4, 4), Position.Y + _level.NextRandomInt(-8, 0));
		Bbox = new Rectangle(0, 0, 10, 10);
		DrawOrigin = new Vector2(5f, 5f);
		base.Rotation = (float)(_level.NextRandomDouble() * 6.2831854820251465);
		_isAffectedByTime = true;
		_isAffectedByGravity = false;
		_isFlying = true;
		_isSolid = false;
		base.CanBeTriggered = false;
		ResetAnimationSpeedTimer();
		RefreshAnimationSpeed();
	}

	private void ResetAnimationSpeedTimer()
	{
		_animationSpeedChangeTimer += 0.1f + (float)(_level.NextRandomDouble() * 2.0);
	}

	private void RefreshAnimationSpeed()
	{
		double num = _level.NextRandomDouble();
		float animationSpeed = ((!(num > 0.25)) ? 10f : (0.1f + (float)(_level.NextRandomDouble() * 0.20000000298023224)));
		_animationSpeed = animationSpeed;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_animationSpeedChangeTimer -= delta;
			if (_animationSpeedChangeTimer <= 0f)
			{
				ResetAnimationSpeedTimer();
				RefreshAnimationSpeed();
			}
		}
		base.Update(delta);
	}
}
