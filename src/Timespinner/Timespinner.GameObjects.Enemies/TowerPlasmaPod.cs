using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.LunaisProjectiles;

namespace Timespinner.GameObjects.Enemies;

internal sealed class TowerPlasmaPod : Monster
{
	private const float TimeToOpen = 0.3f;

	private const float TimeToClose = 0.5f;

	private const float TimeToThrowSpit = 0.33f;

	private const float TimeToEndAbility = 1.32f;

	private readonly int _zapDamage;

	private readonly Appendage _vinesAppendage;

	private readonly Appendage _rootAppendage;

	private float _openCloseTimer = 0.5f;

	private Point _boltTargetPosition;

	private TowerPlasmaPodDamageArea _damageArea;

	private SFXCueInstance _electricLoopCue;

	public TowerPlasmaPod(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.FollowAttack;
		_timeToMove = 0.5f;
		_agility = 0.05f;
		_bboxOffset = new Point(8, 6);
		Bbox = new Rectangle(_position.X, _position.Y, 16, 19);
		_doesUseAppendageCollision = false;
		_zapDamage = (int)Math.Ceiling((float)base.Damage * 1.25f);
		_nonAggroAction = EAIAction.FloatInPlace;
		_movementType = EAIMovementType.Fly;
		base.AggroBboxDimensions = new Point(128, 128);
		base.DeaggroBboxDimensions = new Point(500, 500);
		_oscillationMultiplierX = 0f;
		ChangeAnimation(0, 1, 1f, EAnimationType.None);
		_isAffectedByGravity = false;
		_vinesAppendage = new Appendage(this, new Point(4, 4), new Point(6, 9), _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorObject = this,
			AnchorOffset = new Point(0, -19)
		};
		_rootAppendage = new Appendage(this, new Point(4, 4), new Point(6, 5), _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorObject = this,
			AnchorOffset = new Point(0, 4)
		};
		_vinesAppendage.ChangeAnimation(5, 4, 0.1f, EAnimationType.Cycle);
		_rootAppendage.ChangeAnimation(9, 4, 0.1f, EAnimationType.Cycle);
		_appendages.Add(_vinesAppendage);
		_appendages.Add(_rootAppendage);
		SnapBboxToPosition();
	}

	public override void Update(float delta)
	{
		if (!_isFrozen)
		{
			UpdatePodState(delta);
		}
		base.Update(delta);
	}

	protected override void OnAggroed()
	{
		PlayCue(ESFX.EnemyPlasmaPodAggro);
		ChangeAnimation(new AnimationSpec[2]
		{
			new AnimationSpec
			{
				Start = 0,
				Length = 4,
				Speed = 0.1f
			},
			new AnimationSpec
			{
				Start = 3,
				Length = 2,
				Speed = 0.04f,
				Type = EAnimationType.Cycle
			}
		});
		base.OnAggroed();
	}

	protected override void OnDeAggroed()
	{
		if (_electricLoopCue != null)
		{
			_electricLoopCue.Pause();
		}
		ChangeAnimation(new AnimationSpec[1]
		{
			new AnimationSpec
			{
				Start = 0,
				Length = 4,
				IsInReverse = true,
				Speed = 0.1f
			}
		});
		_openCloseTimer = 0f;
		base.OnDeAggroed();
	}

	private void UpdatePodState(float delta)
	{
		if (_isAggroed)
		{
			if (_openCloseTimer < 0.3f)
			{
				_openCloseTimer += delta;
				if (_openCloseTimer > 0.3f)
				{
					_openCloseTimer = 0.3f;
				}
			}
			else if (_damageArea == null)
			{
				if (_electricLoopCue == null)
				{
					_electricLoopCue = PlayCue(ESFX.EnemyElectricStorm, isLooped: true);
				}
				else
				{
					_electricLoopCue.Resume();
				}
				_damageArea = new TowerPlasmaPodDamageArea(_level, Bbox.Center, base.DefaultTeam, this, _sprite, _zapDamage)
				{
					AnchorOffset = new Point(0, -12)
				};
				_level.AddProjectile(_damageArea);
			}
			else
			{
				_damageArea.RefreshLife();
			}
		}
		else if (_openCloseTimer < 0.5f)
		{
			_openCloseTimer += delta;
			if (_openCloseTimer > 0.5f)
			{
				_openCloseTimer = 0.5f;
			}
			if (_damageArea != null)
			{
				_damageArea.KillParticles();
				_damageArea = null;
			}
		}
	}

	public override void UpdateAbility(float delta)
	{
		if (_abilityTimer <= 0f)
		{
			SetState(EAFSM.Idle);
			PlayCue(ESFX.EnemyPlasmaPodAttackStart);
		}
		if (_abilityTimer >= 0.33f && _lastAbilityTimer < 0.33f)
		{
			_boltTargetPosition = _level.GetNearestProtagonistPosition(Position).Add(0, -8);
			Point point = Position.Add(0, -8);
			List<Vector4> intervalsBetween = ThunderBoltDamageArea.GetIntervalsBetween(point, _boltTargetPosition, base.Level);
			ThunderBoltDamageArea thunderBoltDamageArea = new ThunderBoltDamageArea(_level, point, ETeamSide.Enemies, _zapDamage, intervalsBetween, IsFacingLeft, null, EThunderBoltType.PlasmaPod);
			thunderBoltDamageArea.AddEndPointAnimation(IsFacingLeft, point);
			thunderBoltDamageArea.MakePreBolt();
			_level.AddProjectile(thunderBoltDamageArea);
		}
		if (_abilityTimer >= 1.32f)
		{
			_isCarryingOutAbility = false;
			_currentAction = EAIAction.Idle;
			_nextActionTimer = _timeToIdleAfterAttacking;
		}
	}
}
