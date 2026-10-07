using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;

namespace Timespinner.GameObjects.Enemies;

internal sealed class EmpAristocrat : Monster
{
	private const int ProjectileCount = 5;

	private const float TimeToWindUp = 1.75f;

	private const float TimeToStopSliding = 2.25f;

	private const float ProjectileDormantTimeOffset = 0.266667f;

	private const float TimeForGlyphsToAppear = 0.15f;

	private const float TimeForGlyphFlicker = 0.075f;

	private const float GlyphRotationSpeed = 2.5f;

	private const float IdleAnimationSpeed = 0.15f;

	private const float BackdashAnimationSpeed = 0.1f;

	private const float ChannelAnimationSpeed = 0.1f;

	private static readonly Color BaseGlyphColor = new Color(0.1f, 0.2f, 0.3f, 0.35f);

	private static readonly Color BaseGlyphColor2 = new Color(0.1f, 0.25f, 0.25f, 0.35f);

	private readonly Appendage _flameAppendage;

	private readonly Appendage[] _glyphAppendages = new Appendage[4];

	private readonly EmpAristocratProjectile[] _projectiles = new EmpAristocratProjectile[5];

	private bool _isFlickeringGlyph;

	private bool _hasCreatedProjectiles;

	private float _glyphFlickerTimer;

	public EmpAristocrat(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.FollowAttack;
		_movementType = EAIMovementType.Fly;
		_nonAggroAction = EAIAction.FloatInPlace;
		_canLoseAggro = false;
		_agility = 0.25f;
		_bboxOffset = new Point(9, 2);
		Bbox = new Rectangle(_position.X, _position.Y, 16, 38);
		base.AggroBboxDimensions = new Point(400, 300);
		_timeToIdleAfterMoving = 0f;
		_timeToIdleAfterAttacking = 0f;
		_retreatDistanceThresholdX = 128;
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		IsImageFacingLeft = IsFacingLeft;
		ChangeAnimation(0, 5, 0.15f, EAnimationType.Cycle);
		_doesUseAppendageCollision = false;
		_doAppendagesMatchImageFacing = true;
		_doAppendagesInheritDrawColor = false;
		_doesDrawAppendages = false;
		_isAffectedByGravity = false;
		_isFlying = false;
		base.DoesDrawAura = true;
		base.AuraColor = new Color(0.25f, 0.5f, 0.75f, 0.25f);
		base.AuraOffset = new Vector2(2f, 2f);
		base.AuraFrequency = 9f;
		base.AuraSize = 0.1f;
		_auraCount = 5f;
		for (int i = 0; i < 2; i++)
		{
			Appendage appendage = new Appendage(this, new Point(1, 1), new Point(28, 28), _level, _sprite)
			{
				FollowType = EAppendageFollowType.AnchorLocked,
				AnchorObject = this,
				AnchorOffset = new Point(0, -20),
				DrawOrigin = new Vector2(28f, 28f),
				DrawPriority = -1,
				DrawColor = Color.Transparent
			};
			if (i > 0)
			{
				appendage.Rotation = (float)Math.PI;
			}
			appendage.ChangeAnimation(37);
			base.Appendages.Add(appendage);
			_glyphAppendages[i] = appendage;
		}
		for (int j = 0; j < 2; j++)
		{
			Appendage appendage2 = new Appendage(this, new Point(1, 1), new Point(20, 20), _level, _sprite)
			{
				FollowType = EAppendageFollowType.AnchorLocked,
				AnchorObject = this,
				AnchorOffset = new Point(0, -20),
				DrawOrigin = new Vector2(20f, 20f),
				DrawPriority = -1,
				DrawColor = Color.Transparent
			};
			if (j > 0)
			{
				appendage2.Rotation = (float)Math.PI;
			}
			appendage2.ChangeAnimation(38);
			base.Appendages.Add(appendage2);
			_glyphAppendages[j + 2] = appendage2;
		}
		_flameAppendage = new Appendage(this, new Point(8, 16), new Point(0, 0), _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorOffset = new Point(12, -25)
		};
		_flameAppendage.ChangeAnimation(18, 5, 0.1f, EAnimationType.Cycle);
		_appendages.Add(_flameAppendage);
	}

	public override void SetState(EAFSM state)
	{
		base.SetState(state);
		if (state == EAFSM.Idle)
		{
			ChangeAnimation(0, 5, 0.15f, EAnimationType.Cycle);
		}
	}

	public override void UpdateAbility(float delta)
	{
		if (_abilityTimer <= 0f)
		{
			ChangeAnimation(6, 3, 0.1f, EAnimationType.PingPong, 5, 1, 0.1f);
			Point center = Bbox.Center;
			int num = ((!IsImageFacingLeft) ? 1 : (-1));
			Vector2 iV = new Vector2((float)num * 225f, -225f);
			if (!_hasCreatedProjectiles)
			{
				_hasCreatedProjectiles = true;
				for (int i = 0; i < 5; i++)
				{
					EmpAristocratProjectile empAristocratProjectile = new EmpAristocratProjectile(_level, center, iV, ETeamSide.Enemies, _sprite, base.Damage, this);
					_projectiles[i] = empAristocratProjectile;
				}
			}
			for (int j = 0; j < 5; j++)
			{
				_projectiles[j].Reset(center, iV, 0.266667f * (float)j);
				_level.AddProjectile(_projectiles[j]);
			}
			PlayCue(ESFX.EnemyGalacticSageSpellGlyph);
		}
		float num2 = delta * 2.5f;
		for (int k = 0; k < 2; k++)
		{
			Appendage appendage = _glyphAppendages[k];
			appendage.Rotation += num2;
			if (appendage.Rotation >= (float)Math.PI * 2f)
			{
				appendage.Rotation -= (float)Math.PI * 2f;
			}
		}
		for (int l = 2; l < 4; l++)
		{
			Appendage appendage2 = _glyphAppendages[l];
			appendage2.Rotation -= num2;
			if (appendage2.Rotation <= 0f)
			{
				appendage2.Rotation += (float)Math.PI * 2f;
			}
		}
		if (_abilityTimer < 1.75f)
		{
			if (_abilityTimer < 0.15f)
			{
				float amount = _abilityTimer / 0.15f;
				Appendage[] glyphAppendages = _glyphAppendages;
				foreach (Appendage appendage3 in glyphAppendages)
				{
					appendage3.DrawColor = Color.Transparent.SineInterpolate(BaseGlyphColor, amount);
				}
			}
			else
			{
				_glyphFlickerTimer += delta;
				if (_glyphFlickerTimer >= 0.075f)
				{
					_isFlickeringGlyph = !_isFlickeringGlyph;
					_glyphFlickerTimer -= 0.075f;
				}
				Appendage[] glyphAppendages2 = _glyphAppendages;
				foreach (Appendage appendage4 in glyphAppendages2)
				{
					appendage4.DrawColor = (_isFlickeringGlyph ? BaseGlyphColor : BaseGlyphColor2);
				}
			}
		}
		else
		{
			float num3 = _abilityTimer - 1.75f;
			if (num3 < 0.15f)
			{
				float amount2 = num3 / 0.15f;
				Color drawColor = BaseGlyphColor.CosInterpolate(Color.Transparent, amount2);
				Appendage[] glyphAppendages3 = _glyphAppendages;
				foreach (Appendage appendage5 in glyphAppendages3)
				{
					appendage5.DrawColor = drawColor;
				}
			}
			else
			{
				Appendage[] glyphAppendages4 = _glyphAppendages;
				foreach (Appendage appendage6 in glyphAppendages4)
				{
					appendage6.DrawColor = Color.Transparent;
				}
			}
		}
		if (_abilityTimer >= 1.75f && _lastAbilityTimer < 1.75f)
		{
			ChangeAnimation(new AnimationSpec[3]
			{
				new AnimationSpec
				{
					Start = 10,
					Length = 1,
					Speed = 0.1f
				},
				new AnimationSpec
				{
					Start = 9,
					Length = 4,
					Speed = 0.1f
				},
				new AnimationSpec
				{
					Start = 0,
					Length = 5,
					Speed = 0.15f,
					Type = EAnimationType.Cycle
				}
			});
			_movementX = (IsFacingLeft ? 1 : (-1));
			if (_hasCreatedProjectiles)
			{
				EmpAristocratProjectile[] projectiles = _projectiles;
				foreach (EmpAristocratProjectile empAristocratProjectile2 in projectiles)
				{
					empAristocratProjectile2.StartDash();
				}
			}
			PlayCue(ESFX.EnemyGalacticSageSpellCast);
		}
		if (_abilityTimer >= 2.15f)
		{
			_movementX /= 2f;
		}
		if (_abilityTimer > 2.25f)
		{
			_movementX = 0f;
			_isCarryingOutAbility = false;
			_currentAction = EAIAction.FloatInPlace;
			_nextActionTimer = _timeToIdleAfterAttacking;
		}
	}

	protected override void UpdateDeathScript(float delta)
	{
		_level.AddAnimation(EBattleAnimationType.AuraExplosion, Position, ETeamSide.Enemies, IsFacingLeft, doesPlaySFX: true);
		if (_isCarryingOutAbility && _abilityTimer <= 1.75f)
		{
			EmpAristocratProjectile[] projectiles = _projectiles;
			foreach (EmpAristocratProjectile empAristocratProjectile in projectiles)
			{
				empAristocratProjectile.DoDeathAnimation(empAristocratProjectile.Bbox.Center);
				empAristocratProjectile.SilentKill();
			}
		}
		DropLootAndRemove();
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		DrawAppendages(spriteBatch, drawUnder: true);
		base.Draw(spriteBatch);
		DrawAppendages(spriteBatch, drawUnder: false);
	}
}
