using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal class WormFlower : Monster
{
	private const float OscillXScale = 1f;

	private const float OscillYScale = 0.75f;

	private const float OscillFrequencyX = 2f;

	private const float OscillFrequencyY = 2f;

	private const float OscillRadius = 6f;

	private const float TimeBetweenSeeds = 0.5f;

	private const int MaxSeedDistanceX = 256;

	private float _oscillDelta;

	private Appendage _budAppendage;

	private Point _budBaseLocationOffset;

	public Point BudBaseLocation => new Point(Position.X + _budBaseLocationOffset.X, Position.Y + _budBaseLocationOffset.Y);

	public WormFlower(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.StandAttack;
		_agility = 0.75f;
		_bboxOffset = new Point(0, 4);
		Bbox = new Rectangle(_position.X, _position.Y, 16, 7);
		base.DeaggroBboxDimensions = base.AggroBboxDimensions;
		ChangeAnimation(10, 4, 0.1f, EAnimationType.Cycle);
		_agility = 0f;
		_timeToIdleAfterAttacking = 0.5f;
	}

	public override void InitializeMob()
	{
		Random random = new Random(GetHashCode());
		_oscillDelta = random.Next(6);
		_budBaseLocationOffset = new Point(-1, -32);
		_budAppendage = new Appendage(this, new Rectangle(BudBaseLocation.X, BudBaseLocation.Y, 15, 21), new Point(9, 4), _level, _sprite)
		{
			OscillAmplitude = 6f,
			OscillFrequency = 2f,
			OscillIncrement = 0.25f,
			OscillDelta = _oscillDelta,
			SpriteFrameOffset = base.SpriteFrameOffset
		};
		_appendages.Add(_budAppendage);
		Point zero = Point.Zero;
		_budAppendage.ChangeAnimation(14, 3, 0.15f, EAnimationType.PingPong);
		_budAppendage.AddLinks(8, EAppendageFollowType.TrigoInterpolate, new Rectangle(_budBaseLocationOffset.X, _budBaseLocationOffset.Y, 7, 7), new Point(0, 1), base.SpriteFrameOffset, zero);
		base.InitializeMob();
	}

	public override void Update(float delta)
	{
		if (!_isFrozen && _budAppendage != null)
		{
			_oscillDelta += delta;
			if (_oscillDelta > 6.28f)
			{
				_oscillDelta -= 6.28f;
			}
			double num = Math.Cos(_oscillDelta * 2f) * 6.0 * 1.0;
			double num2 = Math.Sin(_oscillDelta * 2f) * 6.0 * 0.75;
			_budAppendage.Position = new Point((int)Math.Round((double)BudBaseLocation.X + num), (int)Math.Round((double)BudBaseLocation.Y + num2));
		}
		base.Update(delta);
	}

	protected override void UpdateDeathScript(float delta)
	{
		_deathScriptTimer += delta;
		base.IsSolidWhenFrozen = false;
		if (_deathScriptTimer > 0.1f)
		{
			if (!_budAppendage.KillChild(startWithThis: true))
			{
				_level.AddAnimation(EBattleAnimationType.Boom, _bbox.Center, ETeamSide.Enemies);
				DropLootAndRemove();
			}
			_deathScriptTimer = 0f;
		}
	}

	public override void UpdateAbility(float delta)
	{
		if (_abilityTimer == 0f)
		{
			PlayCue(ESFX.EnemyBlossomAutomatonOpen);
			SetCharacterSequenceByName("Shoot");
		}
		if (_abilityTimer >= 0.75f && _lastAbilityTimer < 0.75f)
		{
			_targetPosition = _level.GetNearestProtagonistPosition(_position);
			int num = _position.X - _targetPosition.X;
			num = ((Math.Abs(num) > 256) ? (256 * ((num >= 0) ? 1 : (-1))) : num);
			int x = _budAppendage.Position.X / 16;
			int num2 = _budAppendage.Position.Y / 16;
			int num3 = 0;
			bool flag = false;
			for (int num4 = num2; num4 >= 0; num4--)
			{
				Point key = new Point(x, num4);
				if (_level.SolidTiles.ContainsKey(key) && _level.SolidTiles[key].Type != ETileType.Platform)
				{
					num3 = num4;
					flag = true;
					break;
				}
			}
			int num5 = num2 - num3;
			int num6 = ((flag && num5 < 10) ? (-(600 - (10 - num5) * 30)) : (-600));
			Vector2 iV = new Vector2((float)(-num) * 1.2f, num6);
			Point center = _budAppendage.Bbox.Center;
			center.Y -= 8;
			bool isFacingLeft = iV.X > 0f;
			_level.AddAnimation(new BattleAnimation(_sprite, center, _level)
			{
				TeamSide = ETeamSide.Enemies,
				IsFacingLeft = isFacingLeft,
				AnimationSpeed = 0.035f,
				AnimationStart = 6,
				AnimationLength = 4
			});
			_level.AddProjectile(new WormBullet(_level, center, iV, base.DefaultTeam, _sprite, base.Damage));
			PlayCue(ESFX.EnemyOrganicShoot, center);
		}
		if (_abilityTimer >= 1.1f && _lastAbilityTimer < 1.1f)
		{
			PlayCue(ESFX.EnemyBlossomAutomatonClose);
		}
		if (_abilityTimer >= 1.3f)
		{
			_isCarryingOutAbility = false;
		}
	}
}
