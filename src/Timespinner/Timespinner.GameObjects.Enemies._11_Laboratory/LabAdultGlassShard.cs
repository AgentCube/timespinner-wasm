using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies._11_Laboratory;

internal sealed class LabAdultGlassShard : DamageArea
{
	private enum EGlassShardState
	{
		Idle,
		Vertical,
		Horizontal,
		ToVertical,
		ToHorizontal,
		ToIdle,
		Dead
	}

	private const int Anim_ShardStart = 16;

	private const int Width = 12;

	private const int HalfWidth = 6;

	private readonly int _shardIndex;

	private readonly int _baseDamage;

	private readonly int _parentX;

	private readonly int _parentY;

	private readonly float _idleOffset;

	private readonly Point _idleCenter;

	private readonly Mobile _parent;

	private bool _isAttackingToTheLeft;

	private EGlassShardState _shardState;

	private float _idleTimer;

	private float _shardTimer;

	private Point _idlePosition;

	private Point _stateStartPosition;

	private Point _stateWindupPosition;

	private Point _stateDestinationPosition;

	private Point _stateEndWindupPosition;

	public LabAdultGlassShard(Level inLevel, Point inPosition, Mobile inAnchor, SpriteSheet sprite, int index, int damage)
		: base(inLevel, inPosition, ETeamSide.Enemies, -1, null)
	{
		_sprite = sprite;
		_shardIndex = index;
		_parent = inAnchor;
		int num = -64;
		_idleOffset = (float)Math.PI * 2f * ((float)_shardIndex / 4f);
		_parentX = _parent.Position.X;
		_parentY = _parent.Position.Y;
		_idleCenter = new Point(_parentX, _parentY + num);
		_baseDamage = damage;
		base.Power = (int)Math.Ceiling((float)_baseDamage * 1.15f);
		base.DoesKnockBack = false;
		base.HasInfiniteLife = true;
		_bboxOffset = Point.Zero;
		Bbox = new Rectangle(Position.X, Position.Y, 12, 12);
		DrawOrigin = new Vector2(6f, 6f);
		_doesRotateBasedOnVelocity = false;
		_isTrailLengthAffectedByTime = false;
		ChangeAnimation(16 + _shardIndex % 4);
		Color auraColor = new Color(0.5f, 0.1f, 0.15f, 0.25f);
		base.DoesDrawAura = true;
		base.AuraColor = auraColor;
		base.AuraSize = 0.33f;
		base.AuraFrequency = 10f;
		_auraCount = 5f;
		_doesDrawTrail = true;
		_trailFadeRate = 2f;
		_trailLength = 8;
		_trailInterpolationAmount = 8;
	}

	public override void Update(float delta)
	{
		float rotationSpeed = 5f;
		Color drawColor = Color.White * 0.8f;
		if (!base.IsFrozen)
		{
			_rotationSpeed = rotationSpeed;
			base.DrawColor = drawColor;
			UpdateIdle(delta);
			UpdateShardState(delta);
		}
		base.Update(delta);
	}

	private void UpdateIdle(float delta)
	{
		int num = 32;
		float num2 = 2f;
		_idleTimer += delta * num2;
		if (_idleTimer >= (float)Math.PI * 2f)
		{
			_idleTimer -= (float)Math.PI * 2f;
		}
		float num3 = _idleTimer + _idleOffset;
		int num4 = (int)Math.Ceiling(Math.Cos(num3) * (double)num);
		int num5 = (int)Math.Ceiling(Math.Sin(num3) * (double)num);
		_idlePosition = new Point(_idleCenter.X + num4, _idleCenter.Y + num5);
	}

	internal void StartAttack(bool isHorizontal, bool isPlayerToTheLeft)
	{
		_shardTimer = 0f;
		_shardState = (isHorizontal ? EGlassShardState.ToHorizontal : EGlassShardState.ToVertical);
		_isAttackingToTheLeft = isPlayerToTheLeft;
	}

	private void UpdateShardState(float delta)
	{
		if (_shardTimer <= 0f)
		{
			_stateStartPosition = Position;
			int num = ((!_isAttackingToTheLeft) ? 1 : (-1));
			switch (_shardState)
			{
			case EGlassShardState.Vertical:
				_stateWindupPosition = new Point(_stateStartPosition.X, _stateStartPosition.Y - 16);
				_stateDestinationPosition = new Point(_stateStartPosition.X, _stateStartPosition.Y + 144);
				_stateEndWindupPosition = new Point(_stateDestinationPosition.X, _stateDestinationPosition.Y + 16);
				break;
			case EGlassShardState.Horizontal:
				_stateWindupPosition = new Point(_stateStartPosition.X - 16 * num, _stateStartPosition.Y);
				_stateDestinationPosition = new Point(_stateStartPosition.X + 128 * num, _stateStartPosition.Y);
				_stateEndWindupPosition = new Point(_stateDestinationPosition.X + 16 * num, _stateDestinationPosition.Y);
				break;
			case EGlassShardState.ToVertical:
				_stateDestinationPosition = new Point(_parentX + (48 + 36 * _shardIndex) * num, _parentY + -128);
				break;
			case EGlassShardState.ToHorizontal:
				_stateDestinationPosition = new Point(_parentX + 48 * num, _parentY + -16 + -32 * _shardIndex);
				break;
			}
		}
		float shardTimer = _shardTimer;
		_shardTimer += delta;
		EGlassShardState shardState = _shardState;
		switch (_shardState)
		{
		case EGlassShardState.Idle:
			Position = _idlePosition;
			break;
		case EGlassShardState.Vertical:
		case EGlassShardState.Horizontal:
			if (_shardTimer < 0.5f)
			{
				float amount = _shardTimer / 0.5f;
				Position = _stateStartPosition.SineInterpolate(_stateWindupPosition, amount);
			}
			else if (_shardTimer < 0.8f)
			{
				if (shardTimer < 0.5f)
				{
					PlayCue(ESFX.EnemyLabAdultGlassSlash);
				}
				float amount = (_shardTimer - 0.5f) / 0.3f;
				Position = _stateWindupPosition.SineInterpolate(_stateDestinationPosition, amount);
			}
			else if (_shardTimer < 1.8f)
			{
				float amount = (_shardTimer - 0.8f) / 1f;
				Position = _stateDestinationPosition.SineInterpolate(_stateEndWindupPosition, amount);
			}
			else if (_shardTimer < 2.1f)
			{
				float amount = (_shardTimer - 1.8f) / 0.3f;
				Position = _stateEndWindupPosition.SineInterpolate(_stateStartPosition, amount);
			}
			else
			{
				_shardState = EGlassShardState.ToIdle;
				Position = _stateStartPosition;
			}
			break;
		case EGlassShardState.ToVertical:
		case EGlassShardState.ToHorizontal:
			if (_shardTimer < 0.5f)
			{
				float amount = _shardTimer / 0.5f;
				Position = _stateStartPosition.SineInterpolate(_stateDestinationPosition, amount);
			}
			else
			{
				_shardState = ((_shardState == EGlassShardState.ToVertical) ? EGlassShardState.Vertical : EGlassShardState.Horizontal);
				Position = _stateDestinationPosition;
			}
			break;
		case EGlassShardState.ToIdle:
			if (_shardTimer < 0.5f)
			{
				float amount = _shardTimer / 0.5f;
				Position = _stateStartPosition.SineInterpolate(_idlePosition, amount);
			}
			else
			{
				_shardState = EGlassShardState.Idle;
				Position = _idlePosition;
			}
			break;
		}
		if (shardState != _shardState)
		{
			_shardTimer = 0f;
		}
	}

	internal void KillShard()
	{
		_power = 0;
		_canDamageThings = false;
		base.CanDamageEnemies = false;
		_isAffectedByGravity = true;
		_doesDieOutsideOfVisibleArea = true;
		_isFlying = false;
		base.HasInfiniteLife = false;
		base.DrawColor = Color.White * 0.5f;
		base.DoesDrawAura = false;
		_doesDrawTrail = false;
		_shardState = EGlassShardState.Dead;
	}
}
