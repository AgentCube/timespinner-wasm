using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Familiars.Projectiles;

namespace Timespinner.GameObjects.Heroes.Familiars;

internal class FamiliarKobo : FamiliarBase
{
	private const int MaxBullets = 2;

	private const int DefaultSpellTargetOffsetX = 200;

	private const int GunSpeed = 500;

	private const int BulletStartOffset = 4;

	private const float IdleAnimationSpeed = 0.1f;

	private const float AttackAnimationSpeed = 0.05f;

	private const float TimeBeforeCreatingDamageArea = 0.15f;

	private const float TimeForEntireMeleeAttack = 0.4f;

	private const float SpellCastAnimationSpeed = 0.1f;

	private const float TimeIntoSpellAnimationBeforeCasting = 0.4f;

	private const float TimeIntoSpellAnimationBeforeEnding = 0.6f;

	private readonly FamiliarKoboSpellProjectile[] _bullets = new FamiliarKoboSpellProjectile[2];

	private int _bulletIndex;

	public FamiliarKobo(Point inPosition, Level inLevel, LunaisObj parentObject, SpriteSheet inSprite, Action<bool> switchFamiliar)
		: base(inPosition, inLevel, inSprite, parentObject, EInventoryFamiliarType.Kobo, switchFamiliar)
	{
		_bboxOffset = new Point(7, 7);
		base.SittingOffset = new Point(0, -4);
	}

	public override void SetState(EAFSM state)
	{
		switch (state)
		{
		case EAFSM.Idle:
			if (base.LastFamiliarAIState == EFamiliarAIStateType.Sitting)
			{
				ChangeAnimation(0, 5, 0.1f, EAnimationType.Cycle, 15, 2, 0.1f);
				OnEndSit();
			}
			else
			{
				ChangeAnimation(0, 5, 0.1f, EAnimationType.Cycle);
			}
			break;
		case EAFSM.Running:
			ChangeAnimation(5, 6, 0.1f, EAnimationType.Cycle);
			break;
		}
		base.SetState(state);
	}

	protected override void DoSittingAnimation()
	{
		ChangeAnimation(11, 3, 0.1f, EAnimationType.Once);
	}

	public override void CarryOutAbility(float delta)
	{
		switch (_selectedAbility)
		{
		case 0:
			if (_abilityTimer <= 0f)
			{
				ChangeAnimation(new AnimationSpec[4]
				{
					new AnimationSpec
					{
						Start = 17,
						Length = 2,
						Speed = 0.05f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 18,
						Length = 1,
						Speed = 0.05f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 19,
						Length = 4,
						Speed = 0.05f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 0,
						Length = 5,
						Speed = 0.1f,
						Type = EAnimationType.Cycle
					}
				});
			}
			else if (_abilityTimer >= 0.15f && _lastAbilityTimer < 0.15f)
			{
				_level.AddProjectile(new FamiliarKoboMeleeDamageArea(_level, Bbox.Center, ETeamSide.Heroes, this, base.Damage, this));
			}
			else if (_abilityTimer > 0.4f)
			{
				EndAbility();
			}
			break;
		case 1:
			if (_abilityTimer <= 0f)
			{
				PlayCue(ESFX.FamiliarKoboRangedAttack);
				ChangeAnimation(new AnimationSpec[2]
				{
					new AnimationSpec
					{
						Start = 23,
						Length = 6,
						Speed = 0.1f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 0,
						Length = 5,
						Speed = 0.1f,
						Type = EAnimationType.Cycle
					}
				});
			}
			else if (_abilityTimer >= 0.4f && _lastAbilityTimer < 0.4f)
			{
				if (base.SpellTargetMonster != null || base.IsPlayable)
				{
					Point center = Bbox.Center;
					Point targetEnd = ((base.SpellTargetMonster == null) ? new Point(center.X + (IsFacingLeft ? (-200) : 200), center.Y) : base.SpellTargetMonster.Bbox.Center);
					int damage = (int)Math.Ceiling((float)base.Damage * 5f);
					CreateSpellProjectile(center, targetEnd, damage);
					base.SpellTargetMonster = null;
				}
			}
			else if (_abilityTimer >= 0.6f)
			{
				EndAbility();
			}
			break;
		}
	}

	private void CreateSpellProjectile(Point spellCenter, Point targetEnd, int damage)
	{
		Point point = new Point((!IsFacingLeft) ? 1 : (-1), 0);
		Vector2 iV = new Vector2(point.X * 500, point.Y * 500);
		Point point2 = new Point(spellCenter.X + point.X * 4, spellCenter.Y + point.Y * 4);
		FamiliarKoboSpellProjectile familiarKoboSpellProjectile;
		if (_bullets[_bulletIndex] == null)
		{
			familiarKoboSpellProjectile = new FamiliarKoboSpellProjectile(_level, point2, ETeamSide.Heroes, _sprite, this);
			_bullets[_bulletIndex] = familiarKoboSpellProjectile;
		}
		else
		{
			familiarKoboSpellProjectile = _bullets[_bulletIndex];
		}
		familiarKoboSpellProjectile.Reset(iV, point2, targetEnd, damage);
		_level.AddProjectile(familiarKoboSpellProjectile);
		_bulletIndex++;
		if (_bulletIndex >= 2)
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
