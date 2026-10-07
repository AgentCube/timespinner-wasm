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

internal sealed class TowerIceMage : Monster
{
	private const int IceCrystalOscillationRadius = 2;

	private const int IceCrystalCastOffsetX = 3;

	private const int IceCrystalCastOffsetY = -22;

	private const float IceCrystalOscillationFrequency = 10f;

	private const float TimeForIceCrystalToRise = 0.25f;

	private const float TimeForIceCrystalToFall = 0.25f;

	private const float TimeToWindUp = 1.75f;

	private const float TimeToStopSliding = 2.25f;

	private const float TimeForGlyphsToAppear = 0.15f;

	private const float TimeForGlyphFlicker = 0.075f;

	private const float GlyphRotationSpeed = 2.5f;

	private const float IdleAnimationSpeed = 0.15f;

	private const float BackdashAnimationSpeed = 0.1f;

	private const float ChannelAnimationSpeed = 0.1f;

	private static readonly Color BaseGlyphColor = new Color(0.1f, 0.2f, 0.35f, 0.35f);

	private static readonly Color BaseGlyphColor2 = new Color(0.1f, 0.25f, 0.35f, 0.35f);

	private readonly Point _iceCrystalBaseAnchorOffset;

	private readonly IceMageLazerChargeParticleSystem _iceChargeParticles;

	private readonly Appendage _iceAppendage;

	private readonly Appendage[] _glyphAppendages = new Appendage[4];

	private bool _isFlickeringGlyph;

	private int _iceCrystalOffsetY;

	private int _iceCrystalCastOffsetX;

	private int _iceCrystalCastOffsetY;

	private float _glyphFlickerTimer;

	private float _iceCrystalTimer;

	public TowerIceMage(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.StandAttack;
		_bboxOffset = new Point(7, 6);
		Bbox = new Rectangle(_position.X, _position.Y, 16, 35);
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		IsImageFacingLeft = IsFacingLeft;
		ChangeAnimation(0, 4, 0.15f, EAnimationType.Cycle);
		_doesUseAppendageCollision = false;
		_doAppendagesMatchImageFacing = true;
		_doAppendagesInheritDrawColor = false;
		_doesDrawAppendages = false;
		for (int i = 0; i < 2; i++)
		{
			Appendage appendage = new Appendage(this, new Point(1, 1), new Point(28, 28), _level, _sprite)
			{
				FollowType = EAppendageFollowType.AnchorLocked,
				AnchorObject = this,
				AnchorOffset = new Point(1, -20),
				DrawOrigin = new Vector2(28f, 28f),
				DrawPriority = -1,
				DrawColor = Color.Transparent,
				DoesInheritDrawColor = false
			};
			if (i > 0)
			{
				appendage.Rotation = (float)Math.PI;
			}
			appendage.ChangeAnimation(12);
			base.Appendages.Add(appendage);
			_glyphAppendages[i] = appendage;
		}
		for (int j = 0; j < 2; j++)
		{
			Appendage appendage2 = new Appendage(this, new Point(1, 1), new Point(20, 20), _level, _sprite)
			{
				FollowType = EAppendageFollowType.AnchorLocked,
				AnchorObject = this,
				AnchorOffset = new Point(1, -20),
				DrawOrigin = new Vector2(20f, 20f),
				DrawPriority = -1,
				DrawColor = Color.Transparent,
				DoesInheritDrawColor = false
			};
			if (j > 0)
			{
				appendage2.Rotation = (float)Math.PI;
			}
			appendage2.ChangeAnimation(13);
			base.Appendages.Add(appendage2);
			_glyphAppendages[j + 2] = appendage2;
		}
		_iceCrystalBaseAnchorOffset = new Point(-3, -20);
		_iceAppendage = new Appendage(this, new Point(7, 7), new Point(0, 0), _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorOffset = _iceCrystalBaseAnchorOffset,
			DrawPriority = 1,
			DoesDrawAura = true,
			AuraColor = Color.Magenta * 0.75f,
			AuraSize = 0.25f,
			AuraFrequency = 15f,
			DoesDrawTrail = true,
			TrailLength = 4,
			TrailFadeRate = 0.5f
		};
		_iceAppendage.ChangeAnimation(8, 4, 0.1f, EAnimationType.Cycle);
		_appendages.Add(_iceAppendage);
		_iceChargeParticles = new IceMageLazerChargeParticleSystem(_level.GCM.TxParticleEnergy, 6);
		_particleSystems.Add(_iceChargeParticles);
	}

	public override void InitializeMob()
	{
		if (_level.MainHero != null)
		{
			int num = Math.Abs(_level.MainHero.Position.X - Position.X);
			int num2 = Math.Abs(_level.MainHero.Position.Y - Position.Y);
			if (num <= 48 && num2 <= 48)
			{
				SilentKill();
			}
		}
		base.InitializeMob();
	}

	public override void SetState(EAFSM state)
	{
		base.SetState(state);
		if (state == EAFSM.Idle)
		{
			ChangeAnimation(0, 4, 0.15f, EAnimationType.Cycle);
		}
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_iceCrystalTimer += delta * 10f;
			if (_iceCrystalTimer >= (float)Math.PI * 2f)
			{
				_iceCrystalTimer -= (float)Math.PI * 2f;
			}
			_iceCrystalOffsetY = (int)(Math.Sin(_iceCrystalTimer) * 2.0);
			_iceAppendage.AnchorOffset = new Point(_iceCrystalBaseAnchorOffset.X + _iceCrystalCastOffsetX, _iceCrystalBaseAnchorOffset.Y + _iceCrystalOffsetY + _iceCrystalCastOffsetY);
		}
		base.Update(delta);
	}

	public override void UpdateAbility(float delta)
	{
		if (_abilityTimer <= 0f)
		{
			ChangeAnimation(5, 3, 0.1f, EAnimationType.Once, 5, 1, 0.1f);
			PlayCue(ESFX.EnemyIceMageGlyph);
		}
		float num = delta * 2.5f;
		for (int i = 0; i < 2; i++)
		{
			Appendage appendage = _glyphAppendages[i];
			appendage.Rotation += num;
			if (appendage.Rotation >= (float)Math.PI * 2f)
			{
				appendage.Rotation -= (float)Math.PI * 2f;
			}
		}
		for (int j = 2; j < 4; j++)
		{
			Appendage appendage2 = _glyphAppendages[j];
			appendage2.Rotation -= num;
			if (appendage2.Rotation <= 0f)
			{
				appendage2.Rotation += (float)Math.PI * 2f;
			}
		}
		if (_abilityTimer < 1.75f)
		{
			if (_abilityTimer < 0.25f)
			{
				float percentage = _abilityTimer / 0.25f;
				_iceCrystalCastOffsetX = (int)MathEx.SineInterpolate(0f, 3f, percentage);
				_iceCrystalCastOffsetY = (int)MathEx.SineInterpolate(0f, -22f, percentage);
			}
			else
			{
				_iceCrystalCastOffsetX = 3;
				_iceCrystalCastOffsetY = -22;
				_iceChargeParticles.AddParticles(_iceAppendage.Bbox.Center.ToVector2());
			}
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
			if (_lastAbilityTimer < 1.75f)
			{
				_iceChargeParticles.KillOffParticles(0.1f);
			}
			float num2 = _abilityTimer - 1.75f;
			if (num2 < 0.25f)
			{
				float percentage2 = num2 / 0.25f;
				_iceCrystalCastOffsetX = (int)MathEx.SineInterpolate(3f, 0f, percentage2);
				_iceCrystalCastOffsetY = (int)MathEx.SineInterpolate(-22f, 0f, percentage2);
			}
			else
			{
				_iceCrystalCastOffsetX = 0;
				_iceCrystalCastOffsetY = 0;
			}
			if (num2 < 0.15f)
			{
				float amount2 = num2 / 0.15f;
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
			ShootIceCrystal();
		}
		if (_abilityTimer > 2.25f)
		{
			_movementX = 0f;
			_isCarryingOutAbility = false;
			_currentAction = EAIAction.Idle;
			_nextActionTimer = _timeToIdleAfterAttacking;
		}
	}

	private void ShootIceCrystal()
	{
		Point center = _iceAppendage.Bbox.Center;
		Point nearestProtagonistPosition = _level.GetNearestProtagonistPosition(center);
		Vector2 iV = new Vector2(nearestProtagonistPosition.X - center.X, nearestProtagonistPosition.Y - center.Y);
		if (Math.Abs(iV.X) < Math.Abs(iV.Y))
		{
			bool flag = iV.X < 0f;
			iV = new Vector2(Math.Abs(iV.Y) * (float)((!flag) ? 1 : (-1)), iV.Y);
		}
		iV.Normalize();
		_level.AddProjectile(new TowerIceMageProjectile(_level, center, iV, ETeamSide.Enemies, _sprite, base.Damage));
		ChangeAnimation(new AnimationSpec[2]
		{
			new AnimationSpec
			{
				Start = 6,
				Length = 1,
				Speed = 0.1f
			},
			new AnimationSpec
			{
				Start = 0,
				Length = 4,
				Speed = 0.15f,
				Type = EAnimationType.Cycle
			}
		});
	}

	protected override void UpdateDeathScript(float delta)
	{
		_level.AddAnimation(EBattleAnimationType.AuraExplosion, Position, ETeamSide.Enemies, IsFacingLeft, doesPlaySFX: true);
		DropLootAndRemove();
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		DrawAppendages(spriteBatch, drawUnder: true);
		base.Draw(spriteBatch);
		DrawAppendages(spriteBatch, drawUnder: false);
	}
}
