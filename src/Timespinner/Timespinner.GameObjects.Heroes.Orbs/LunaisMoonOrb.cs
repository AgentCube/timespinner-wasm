using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.LunaisProjectiles;

namespace Timespinner.GameObjects.Heroes.Orbs;

internal sealed class LunaisMoonOrb : LunaisOrb
{
	private const int ShardCount = 3;

	private const int Shard2Length = 8;

	private const int Shard3Length = 8;

	private const int Shard2Start = 3;

	private const int Shard3Start = 11;

	private const int Shard4Start = 19;

	private const int OrbTrailLength = 75;

	private const float ThrowTimeToAttack = 0.2f;

	private const float ThrowTimeToReturn = 0.35f;

	private const float ThrowRadius = 72f;

	private const float MainShardRotationSpeed = 3f;

	private static readonly Point OrbBboxOffset = new Point(2, 2);

	private static readonly Point OrbBboxDimensions = new Point(8, 8);

	private static readonly Point MainShardAnchorOffset = new Point(0, 4);

	private static readonly Vector2 OrbDrawOrigin = new Vector2(6f, 6f);

	private static readonly Color WhiteTrailColor = new Color(0.95f, 0.95f, 1f, 0.2f);

	private static readonly Color ThrowTrailColor = new Color(0.7f, 0.7f, 1f, 0.1f);

	private readonly Appendage _overShards;

	private readonly Appendage _underShards;

	private readonly MoonOrbShard[] _shards = new MoonOrbShard[3];

	private readonly MoonOrbMeleeDamageArea _damageArea;

	public static EInventoryOrbType DefaultOrbColor => EInventoryOrbType.Moon;

	public LunaisMoonOrb(Level inLevel, LunaisObj parentLunais, Point inPosition, SpriteSheet inSprite, bool isFrontPlane)
		: base(inLevel, parentLunais, inPosition, inSprite, DefaultOrbColor, EOrbState.Idle, isFrontPlane)
	{
		_trailColor = base.IdleTrailColor;
		_trailLength = 75;
		_doesDrawBrushTrail = true;
		_bbox = new Rectangle(0, 0, OrbBboxDimensions.X, OrbBboxDimensions.Y);
		_bboxOffset = new Point(1, 1);
		ChangeAnimation(1);
		_doesDrawBaseSprite = false;
		_overShards = new Appendage(this, OrbBboxDimensions, OrbBboxOffset, _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorObject = this,
			DrawOrigin = OrbDrawOrigin,
			AnchorOffset = MainShardAnchorOffset,
			DrawPriority = 1
		};
		_underShards = new Appendage(this, OrbBboxDimensions, OrbBboxOffset, _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorObject = this,
			DrawOrigin = OrbDrawOrigin,
			AnchorOffset = MainShardAnchorOffset,
			DrawPriority = 1
		};
		_overShards.ChangeAnimation(1);
		_underShards.ChangeAnimation(2);
		_appendages.Add(_overShards);
		_appendages.Add(_underShards);
		MoonOrbShard moonOrbShard = new MoonOrbShard(this, new Point(6, 6), new Point(1, 3), _level, _sprite, this)
		{
			DrawOrigin = new Vector2(4f, 6f)
		};
		MoonOrbShard moonOrbShard2 = new MoonOrbShard(this, new Point(2, 2), new Point(1, 1), _level, _sprite, moonOrbShard);
		MoonOrbShard moonOrbShard3 = new MoonOrbShard(this, new Point(1, 1), new Point(1, 1), _level, _sprite, moonOrbShard2);
		moonOrbShard.ChangeAnimation(3, 8, 0.07f, EAnimationType.Cycle);
		moonOrbShard2.ChangeAnimation(11, 8, 0.07f, EAnimationType.Cycle);
		moonOrbShard3.ChangeAnimation(19);
		_shards[0] = moonOrbShard;
		_shards[1] = moonOrbShard2;
		_shards[2] = moonOrbShard3;
		MoonOrbShard[] shards = _shards;
		foreach (MoonOrbShard moonOrbShard4 in shards)
		{
			_appendages.Add(moonOrbShard4);
			moonOrbShard4.InitializeShard(Position);
		}
		_damageArea = new MoonOrbMeleeDamageArea(_level, Position, ETeamSide.Heroes, this, base.OrbDamage, this);
		Update(0f);
	}

	public override void StartMeleeAttack(int whichAttack)
	{
		base.StartMeleeAttack(whichAttack);
		base.State = EOrbState.Melee;
		_trailColor = ThrowTrailColor;
		_battleAnimations.Add(new BattleAnimation(_level.GCM.SpEffectsSmall, new Point(Bbox.Center.X - 3, Bbox.Center.Y), _level)
		{
			TeamSide = ETeamSide.Heroes,
			AnimationStart = 69,
			AnimationLength = 4,
			DrawColor = Color.White * 0.75f,
			IsFacingLeft = base.IsOrbFacingLeft
		});
		_damageArea.Reset(base.OrbDamage);
		_level.AddProjectile(_damageArea);
		PlayCue(ESFX.LunaisOrbShatteredWhiff);
	}

	protected override void UpdateMeleeAttack(float delta)
	{
		float num = ((_currentThrowTime <= 0.2f) ? (_currentThrowTime / 0.2f) : (1f + (_currentThrowTime - 0.2f) / 0.35f));
		float num2 = (float)Math.Sin(num * ((float)Math.PI / 2f));
		float scaleFactor = 1f - num2;
		Vector2 value = UpdateThrow(delta);
		Position = Vector2.Add(Vector2.Multiply(_baseOrbitPosition.ToVector2(), scaleFactor), Vector2.Multiply(value, num2)).ToPoint();
	}

	public override void Update(float delta, Point targetPoint)
	{
		if (!base.IsFrozen)
		{
			_overShards.Rotation += delta * 3f;
			if (_overShards.Rotation >= (float)Math.PI * 2f)
			{
				_overShards.Rotation -= (float)Math.PI * 2f;
			}
			_underShards.Rotation = 0f - _overShards.Rotation;
		}
		base.Update(delta, targetPoint);
	}

	private Vector2 UpdateThrow(float delta)
	{
		float currentThrowTime = _currentThrowTime;
		_currentThrowTime += delta;
		Vector2 result;
		if (_currentThrowTime > 0.55f)
		{
			base.State = EOrbState.Idle;
			_trailColor = base.IdleTrailColor;
			result = _baseOrbitPosition.ToVector2();
			_level.RequestRemoveObject(_damageArea);
		}
		else
		{
			if (_currentThrowTime >= 0.2f && currentThrowTime < 0.2f)
			{
				base.IsAtAttackApex = true;
			}
			float num = ((_currentThrowTime <= 0.2f) ? (_currentThrowTime / 0.4f) : (0.5f + (_currentThrowTime - 0.2f) / 0.7f));
			_trailColor = ((_currentThrowTime <= 0.3f) ? WhiteTrailColor : ThrowTrailColor);
			float num2 = 72f * (float)Math.Sin((double)num * Math.PI);
			_orbXShift = (float)((!base.IsThrowingLeft) ? 1 : (-1)) * num2;
			result = new Vector2((int)Math.Round((float)base.CurrentTarget.X + _orbXShift), base.CurrentTarget.Y);
		}
		return result;
	}

	public override void ChangeRoom()
	{
		base.ChangeRoom();
		UpdateOrbitOffset(0f);
		MoonOrbShard[] shards = _shards;
		foreach (MoonOrbShard moonOrbShard in shards)
		{
			moonOrbShard.ChangeRoom(_baseOrbitPosition);
		}
	}

	public override void DisposeOrb()
	{
		_damageArea.SilentKill();
	}
}
