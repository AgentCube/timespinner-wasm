using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies._07_LakeSerene;

internal sealed class LakeCheveuxFireProjectile : Projectile
{
	private const int Radius = 16;

	private const int HalfRadius = 8;

	private const int BboxRadius = 14;

	private const float MaxLife = 0.3f;

	private readonly int _startingPower;

	internal bool IsFinished { get; private set; }

	public LakeCheveuxFireProjectile(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, SpriteSheet sprite, int damage)
		: base(inLevel, inPosition, iV, inSide, -1)
	{
		_sprite = sprite;
		_bbox = new Rectangle(inPosition.X - 7, inPosition.Y - 7, 14, 14);
		_bboxOffset = new Point(1, 1);
		DrawOrigin = new Vector2(8f, 8f);
		_startingPower = damage;
		_power = _startingPower;
		_force = 0;
		_life = 0.3f;
		_timeToFade = 0f;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_animationSpeed = 0f;
		_maxMoveSpeed = 5000f;
		_maxFallSpeed = 5000f;
		_isFlying = true;
		base.DoesCollideWithTiles = true;
		_doesDieOnTiles = true;
		_doesCollideWithFloors = true;
		_doesCollideWithWalls = false;
		_doesCollideWithCeilings = true;
		_isIgnoringPlatform = false;
		ChangeAnimation(25, 3, 0.03f, EAnimationType.Cycle);
		_animationIndex = _level.NextRandomInt(0, 2);
	}

	public override void Update(float delta)
	{
		float num = _life / 0.3f;
		_scale = 1.25f * (1f - _life / 0.3f) + 0.75f;
		base.DrawColor = new Color(1f, 1f, 1f, num) * num;
		if (_life < 0.1f)
		{
			_power = 0;
		}
		base.Update(delta);
	}

	public override void Kill(bool useAnimation)
	{
		_level.AddAnimation(EBattleAnimationType.SmallHit, _bbox.Center, _teamSide);
		Kill();
	}

	public override void SilentKill()
	{
		IsFinished = true;
		base.SilentKill();
	}

	internal void Reset(Point position, Vector2 iV)
	{
		Position = position;
		_initialVector = iV;
		_velocity = iV;
		SnapBboxToPosition();
		base.ID = -1;
		_power = _startingPower;
		_isFading = false;
		_life = 0.3f;
		IsFinished = false;
	}
}
