using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Etc;

namespace Timespinner.GameObjects.Enemies;

internal sealed class ForestWormFlower : Monster
{
	private const int MaxSimultaneousSaplings = 3;

	private const int MaxSeedDistanceX = 256;

	private readonly ObjectTileSpecification _saplingObjectSpec;

	private readonly MinionContainer _saplingContainer;

	private Point _budBaseLocationOffset;

	private Appendage _budAppendage;

	private Appendage _stemAppendage;

	public Point BudBaseLocation => new Point(Position.X + _budBaseLocationOffset.X, Position.Y + _budBaseLocationOffset.Y);

	public ForestWormFlower(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.StandAttack;
		_agility = 0.75f;
		_bboxOffset = new Point(0, 0);
		Bbox = new Rectangle(_position.X, _position.Y, 16, 8);
		base.DeaggroBboxDimensions = base.AggroBboxDimensions;
		_timeToIdleAfterAttacking = 0.25f;
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		ChangeAnimation(1, 4, 0.2f, EAnimationType.Cycle);
		_saplingContainer = new MinionContainer(3, _level);
		_saplingObjectSpec = objectSpec;
	}

	public override void InitializeMob()
	{
		_stemAppendage = new Appendage(this, new Point(8, 24), new Point(2, 0), _level, _sprite)
		{
			AnchorOffset = new Point(1, -8),
			FollowType = EAppendageFollowType.AnchorLocked
		};
		_stemAppendage.ChangeAnimation(5, 4, 0.2f, EAnimationType.Cycle);
		_appendages.Add(_stemAppendage);
		_budBaseLocationOffset = new Point(0, -30);
		_budAppendage = new Appendage(this, new Rectangle(BudBaseLocation.X, BudBaseLocation.Y, 15, 21), new Point(9, 4), _level, _sprite);
		_appendages.Add(_budAppendage);
		_budAppendage.ChangeAnimation(9);
		_saplingContainer.Initialize();
		base.InitializeMob();
		Update(0f);
	}

	public override void Update(float delta)
	{
		if (!_isFrozen && _budAppendage != null)
		{
			float num = ((_stemAppendage.AnimationIndex % 2 != 0) ? 1 : 0);
			if (_stemAppendage.AnimationIndex == 3)
			{
				num = 0f - num;
			}
			float num2 = 0f - num;
			_budAppendage.Position = new Point((int)Math.Round((float)BudBaseLocation.X + num), (int)Math.Round((float)BudBaseLocation.Y + num2));
		}
		base.Update(delta);
		_saplingContainer.Update(delta);
	}

	protected override void UpdateDeathScript(float delta)
	{
		_deathScriptTimer += delta;
		if (_deathScriptTimer > 0.1f)
		{
			_level.AddAnimation(EBattleAnimationType.WetSplashLarge, _bbox.Center, ETeamSide.Enemies);
			_level.AddAnimation(EBattleAnimationType.WetSplashLarge, _budAppendage.Bbox.Center, ETeamSide.Enemies, isFacingRight: true, doesPlaySFX: false);
			DropLootAndRemove();
			_deathScriptTimer = 0f;
		}
	}

	public override void UpdateAbility(float delta)
	{
		if (_abilityTimer <= 0f)
		{
			PlayCue(ESFX.EnemyWormFlowerOpen);
			_budAppendage.ChangeAnimation(9, 9, 0.125f, EAnimationType.Once);
		}
		if (_abilityTimer >= 1f && _lastAbilityTimer < 1f)
		{
			_targetPosition = _level.GetNearestProtagonistPosition(_position);
			int num = _position.X - _targetPosition.X;
			num = ((Math.Abs(num) > 256) ? (256 * ((num >= 0) ? 1 : (-1))) : num);
			Vector2 iV = new Vector2((float)(-num) * 1.2f, -600f);
			Point center = _budAppendage.Bbox.Center;
			center.Y -= 2;
			bool isFacingLeft = iV.X > 0f;
			_level.AddAnimation(new BattleAnimation(_sprite, center, _level)
			{
				TeamSide = ETeamSide.Enemies,
				IsFacingLeft = isFacingLeft,
				AnimationSpeed = 0.035f,
				AnimationStart = 27,
				AnimationLength = 4
			});
			_level.AddProjectile(new ForestWormBullet(_level, center, iV, base.DefaultTeam, SproutSapling, _sprite, base.Damage));
			PlayCue(ESFX.EnemyOrganicShoot, center);
			AnimationSpec[] newAnimations = new AnimationSpec[2]
			{
				new AnimationSpec
				{
					Start = 16,
					Length = 1,
					Speed = 0.15f
				},
				new AnimationSpec
				{
					Start = 9,
					Length = 6,
					Speed = 0.1f,
					IsInReverse = true
				}
			};
			_budAppendage.ChangeAnimation(newAnimations);
		}
		if (_abilityTimer >= 1.35f && _lastAbilityTimer < 1.35f)
		{
			PlayCue(ESFX.EnemyWormFlowerClose);
		}
		if (_abilityTimer >= 2f)
		{
			_isCarryingOutAbility = false;
		}
	}

	private bool SproutSapling(Point location)
	{
		bool result = false;
		if (!_saplingContainer.IsFull)
		{
			ForestWormFlowerSapling newMinion = new ForestWormFlowerSapling(location, _level, _sprite, -1, _saplingObjectSpec, base.MaxHP, base.Damage);
			_saplingContainer.AddMinion(newMinion);
			result = true;
		}
		return result;
	}
}
