using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Familiars.Projectiles;

namespace Timespinner.GameObjects.Heroes.Familiars;

internal class FamiliarGriffin : FamiliarBase
{
	private const int MaxBullets = 8;

	private const int BulletStartOffset = 12;

	private const int DefaultSpellTargetOffsetX = 200;

	private const float GunSpeed = 300f;

	private const float IdleAnimationSpeed = 0.1f;

	private const float AttackAnimationSpeed = 0.05f;

	private const float TimeBeforeCreatingDamageArea = 0.2f;

	private const float TimeForEntireMeleeAttack = 0.4f;

	private const float SpellCastAnimationSpeed = 0.1f;

	private const float TimeIntoSpellAnimationBeforeCasting = 0.3f;

	private const float TimeIntoSpellAnimationBeforeEnding = 0.4f;

	private readonly FamiliarGriffinSpellProjectile[] _bullets = new FamiliarGriffinSpellProjectile[8];

	private int _bulletIndex;

	public FamiliarGriffin(Point inPosition, Level inLevel, LunaisObj parentObject, SpriteSheet inSprite, Action<bool> switchFamiliar)
		: base(inPosition, inLevel, inSprite, parentObject, EInventoryFamiliarType.Griffin, switchFamiliar)
	{
		_bboxOffset = new Point(4, 5);
	}

	public override void SetState(EAFSM state)
	{
		switch (state)
		{
		case EAFSM.Idle:
			if (base.LastFamiliarAIState == EFamiliarAIStateType.Sitting)
			{
				ChangeAnimation(0, 6, 0.1f, EAnimationType.Cycle, 16, 3, 0.1f);
				OnEndSit();
			}
			else
			{
				ChangeAnimation(0, 6, 0.1f, EAnimationType.Cycle);
			}
			break;
		case EAFSM.Running:
			ChangeAnimation(6, 6, 0.1f, EAnimationType.Cycle);
			break;
		}
		base.SetState(state);
	}

	protected override void DoSittingAnimation()
	{
		ChangeAnimation(12, 4, 0.1f, EAnimationType.Once);
	}

	public override void CarryOutAbility(float delta)
	{
		switch (_selectedAbility)
		{
		case 0:
			if (_abilityTimer <= 0f)
			{
				ChangeAnimation(new AnimationSpec[5]
				{
					new AnimationSpec
					{
						Start = 19,
						Length = 3,
						Speed = 0.05f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 21,
						Length = 1,
						Speed = 0.05f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 22,
						Length = 1,
						Speed = 0.05f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 23,
						Length = 2,
						Speed = 0.05f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 0,
						Length = 6,
						Speed = 0.1f,
						Type = EAnimationType.Cycle,
						InitialIndex = 2
					}
				});
			}
			else if (_abilityTimer >= 0.2f && _lastAbilityTimer < 0.2f)
			{
				_level.AddProjectile(new FamiliarGriffinMeleeDamageArea(_level, Bbox.Center, ETeamSide.Heroes, this, base.Damage, this));
			}
			else if (_abilityTimer > 0.4f)
			{
				EndAbility();
			}
			break;
		case 1:
			if (_abilityTimer <= 0f)
			{
				ChangeAnimation(new AnimationSpec[2]
				{
					new AnimationSpec
					{
						Start = 25,
						Length = 4,
						Speed = 0.1f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 0,
						Length = 6,
						Speed = 0.1f,
						Type = EAnimationType.Cycle
					}
				});
			}
			else if (_abilityTimer >= 0.3f && _lastAbilityTimer < 0.3f)
			{
				if (base.SpellTargetMonster != null || base.IsPlayable)
				{
					Point center = Bbox.Center;
					Point targetEnd = ((base.SpellTargetMonster == null) ? new Point(center.X + (IsFacingLeft ? (-200) : 200), center.Y) : base.SpellTargetMonster.Bbox.Center);
					int damage = (int)Math.Ceiling((float)base.Damage * 1f);
					CreateSpellProjectile(center, targetEnd, damage, EDirection.NorthWest);
					CreateSpellProjectile(center, targetEnd, damage, EDirection.NorthEast);
					CreateSpellProjectile(center, targetEnd, damage, EDirection.SouthWest);
					CreateSpellProjectile(center, targetEnd, damage, EDirection.SouthEast);
					_level.PlayCue(ESFX.FamiliarWindSpell, Position);
					base.SpellTargetMonster = null;
				}
			}
			else if (_abilityTimer >= 0.4f)
			{
				EndAbility();
			}
			break;
		}
	}

	private void CreateSpellProjectile(Point spellCenter, Point targetEnd, int damage, EDirection direction)
	{
		Point pointFromDirection = Level.GetPointFromDirection(Point.Zero, direction);
		Vector2 iV = new Vector2((float)pointFromDirection.X * 300f, (float)pointFromDirection.Y * 300f);
		Point point = new Point(spellCenter.X + pointFromDirection.X * 12, spellCenter.Y + pointFromDirection.Y * 12);
		FamiliarGriffinSpellProjectile familiarGriffinSpellProjectile;
		if (_bullets[_bulletIndex] == null)
		{
			familiarGriffinSpellProjectile = new FamiliarGriffinSpellProjectile(_level, point, ETeamSide.Heroes, _sprite, this);
			_bullets[_bulletIndex] = familiarGriffinSpellProjectile;
		}
		else
		{
			familiarGriffinSpellProjectile = _bullets[_bulletIndex];
		}
		familiarGriffinSpellProjectile.Reset(iV, point, targetEnd, damage);
		_level.AddProjectile(familiarGriffinSpellProjectile);
		_bulletIndex++;
		if (_bulletIndex >= 8)
		{
			_bulletIndex = 0;
		}
	}

	protected override void DecideNextAction(float delta)
	{
		bool flag = false;
		if (!_level.IsTimeFrozen && base.SpellCastTimer <= 0f && base.FamiliarAIState != EFamiliarAIStateType.Sitting && TargetNearestEnemy())
		{
			flag = true;
			base.SpellCastTimer = base.TimeBeforeCastingSpell;
		}
		if (flag)
		{
			base.FamiliarAIState = EFamiliarAIStateType.Casting;
			StartAbility(1);
		}
		else
		{
			base.DecideNextAction(delta);
		}
	}
}
