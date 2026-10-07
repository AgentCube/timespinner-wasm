using System;
using Microsoft.Xna.Framework;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Familiars.Projectiles;

namespace Timespinner.GameObjects.Heroes.Familiars;

internal class FamiliarMeyef : FamiliarBase
{
	private const int FlameStartOffsetX = 6;

	private const int FlameStartOffsetY = -8;

	private const int FlameEndOffsetX = 16;

	private const int FlameEndOffsetY = -20;

	private const float FlameSpeed = 80f;

	private const float FlameStartingAngle = (float)Math.PI / 4f;

	private const float FlameEndingAngle = -(float)Math.PI / 8f;

	private const float TimeToFireFireballs = 0.25f;

	private const float TimeBetweenIndividualFireballs = 0.033f;

	private const int IdleAnimationLength = 5;

	internal const float IdleAnimationSpeed = 0.1f;

	private const float AttackAnimationSpeed = 0.05f;

	private const float TimeBeforeCreatingDamageArea = 0.2f;

	private const float TimeBeforeEndingMeleeAttack = 0.4f;

	private const float SpellCastAnimationSpeed = 0.1f;

	private const float TimeIntoSpellAnimationBeforeCasting = 0.3f;

	private const float TimeIntoSpellAnimationBeforeEnding = 0.8f;

	internal const int Anim_SleepStart = 39;

	internal const int Anim_SleepLength = 2;

	internal const int Anim_YawnStart = 41;

	internal const int Anim_YawnLength = 3;

	internal const int Anim_PreFlyStart = 44;

	internal const int Anim_PreFlyLength = 3;

	internal const int Anim_IdleStart = 0;

	internal const int Anim_IdleLength = 5;

	internal const float Anim_IdleSpeed = 0.1f;

	internal const int Anim_LandStart = 10;

	internal const int Anim_HissLandStart = 47;

	internal const int Anim_HissStart = 48;

	internal const int Anim_GrowlLength = 3;

	internal const int Anim_TakeOffStart = 15;

	private readonly FamiliarMeyefSpellDamageArea _fireDamageArea;

	private bool _isCreatingFireballs;

	private bool _isShootingToTheLeft;

	private float _fireballCreationTimer;

	private float _individualFireballTimer;

	private Point _spellCreationPosition;

	public FamiliarMeyef(Point inPosition, Level inLevel, LunaisObj parentObject, SpriteSheet inSprite, Action<bool> switchFamiliar)
		: base(inPosition, inLevel, inSprite, parentObject, EInventoryFamiliarType.Meyef, switchFamiliar)
	{
		_bboxOffset = new Point(9, 5);
		base.SittingOffset = new Point(0, -6);
		int baseOrbDamage = (int)Math.Ceiling((float)base.Damage * 1f);
		_fireDamageArea = new FamiliarMeyefSpellDamageArea(_level, Point.Zero, ETeamSide.Heroes, baseOrbDamage, this, inSprite);
	}

	public override void SetState(EAFSM state)
	{
		switch (state)
		{
		case EAFSM.Idle:
			if (base.LastFamiliarAIState == EFamiliarAIStateType.Sitting)
			{
				ChangeAnimation(0, 5, 0.1f, EAnimationType.Cycle, 14, 3, 0.1f);
				OnEndSit();
			}
			else
			{
				ChangeAnimation(0, 5, 0.1f, EAnimationType.Cycle);
			}
			break;
		case EAFSM.Running:
			ChangeAnimation(5, 5, 0.1f, EAnimationType.Cycle);
			break;
		}
		base.SetState(state);
	}

	protected override void DoSittingAnimation()
	{
		ChangeAnimation(10, 4, 0.1f, EAnimationType.Once);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen && _isCreatingFireballs)
		{
			if (_fireballCreationTimer <= 0f)
			{
				int power = (int)Math.Ceiling((float)base.Damage * 1f);
				_fireDamageArea.Reset(_spellCreationPosition, power);
				_level.AddProjectile(_fireDamageArea);
			}
			_fireballCreationTimer += delta;
			if (_fireballCreationTimer >= 0.25f)
			{
				EndFireballs();
			}
			else
			{
				_individualFireballTimer -= delta;
				if (_individualFireballTimer <= 0f)
				{
					_individualFireballTimer = 0.033f;
					float percentage = _fireballCreationTimer / 0.25f;
					CreateFireball(percentage);
				}
			}
		}
		base.Update(delta);
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
						Length = 3,
						Speed = 0.05f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 19,
						Length = 1,
						Speed = 0.05f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 20,
						Length = 4,
						Speed = 0.05f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 0,
						Length = 5,
						Speed = 0.1f,
						Type = EAnimationType.Cycle,
						InitialIndex = 2
					}
				});
			}
			else if (_abilityTimer >= 0.2f && _lastAbilityTimer < 0.2f)
			{
				_level.AddProjectile(new FamiliarMeyefMeleeDamageArea(_level, Bbox.Center, ETeamSide.Heroes, this, base.Damage, this));
			}
			else if (_abilityTimer > 0.4f)
			{
				EndAbility();
			}
			break;
		case 1:
			if (_abilityTimer <= 0f)
			{
				ChangeAnimation(new AnimationSpec[3]
				{
					new AnimationSpec
					{
						Start = 24,
						Length = 8,
						Speed = 0.1f,
						Type = EAnimationType.Once
					},
					new AnimationSpec
					{
						Start = 2,
						Length = 3,
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
			else if (_abilityTimer >= 0.3f && _lastAbilityTimer < 0.3f)
			{
				if (base.SpellTargetMonster != null || base.IsPlayable)
				{
					base.SpellTargetMonster = null;
					StartBreathingFire();
					_level.PlayCue(ESFX.FamiliarFireBreath, Position);
				}
			}
			else if (_abilityTimer >= 0.8f)
			{
				EndAbility();
			}
			break;
		}
	}

	protected override void UpdateCasting(float delta)
	{
		if (!_isCarryingOutAbility)
		{
			StartGoToPoint(1f, EFamiliarAIStateType.Following, EFamiliarGoToType.Leash);
		}
	}

	private void StartBreathingFire()
	{
		_isCreatingFireballs = true;
		_fireballCreationTimer = 0f;
		_isShootingToTheLeft = IsFacingLeft;
	}

	private void CreateFireball(float percentage)
	{
		float num = MathHelper.Lerp((float)Math.PI / 4f, -(float)Math.PI / 8f, percentage);
		float num2 = (float)Math.Cos(num) * (float)((!_isShootingToTheLeft) ? 1 : (-1));
		float num3 = (float)Math.Sin(num);
		Vector2 iV = new Vector2(num2 * 80f, num3 * 80f);
		int num4 = (int)MathHelper.Lerp(6f, 16f, percentage);
		int y = (int)MathHelper.Lerp(-8f, -20f, percentage);
		_spellCreationPosition = Position.Add(new Point(num4 * ((!_isShootingToTheLeft) ? 1 : (-1)), y));
		_fireDamageArea.EmitFireball(_spellCreationPosition, iV);
	}

	public override void ChangeRoom()
	{
		EndFireballs();
		base.ChangeRoom();
	}

	private void EndFireballs()
	{
		_isCreatingFireballs = false;
		_fireballCreationTimer = 0f;
		_individualFireballTimer = 0f;
		_fireDamageArea.End();
	}

	protected override void DecideNextAction(float delta)
	{
		bool flag = false;
		if (!_level.IsTimeFrozen && base.SpellCastTimer <= 0f && base.FamiliarAIState != EFamiliarAIStateType.Sitting && TargetNearestEnemy())
		{
			_currentTarget = base.SpellTargetMonster;
			StartGoToPoint(0.75f, EFamiliarAIStateType.Casting, EFamiliarGoToType.Target);
			flag = true;
			base.SpellCastTimer = base.TimeBeforeCastingSpell;
		}
		if (!flag)
		{
			base.DecideNextAction(delta);
		}
	}

	internal override void ChangeSkin(bool isAltSkin)
	{
		_sprite = (isAltSkin ? _level.GCM.SpFamiliarAltMeyef : _level.GCM.SpFamiliarMeyef);
		_fireDamageArea.ChangeSprite(_sprite);
	}
}
