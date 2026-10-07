using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Enemies._14_Gyre;
using Timespinner.GameObjects.Events.Misc;

namespace Timespinner.GameObjects.Enemies;

internal sealed class GyreRyshia : Monster
{
	private const int GlyphCount = 4;

	private const int TotalGlyphCount = 8;

	private const int GlyphOffsetY = -16;

	private const int OuterGlyphRadius = 28;

	private const int InnerGlyphRadius = 23;

	private const int TearsOffsetX = -4;

	private const int TearsOffsetY = -4;

	private const float TimeBetweenTearDrops = 0.25f;

	private const float TimeForGlyphsToAppear = 0.15f;

	private const float TimeForGlyphFlicker = 0.075f;

	private const float GlyphRotationSpeed = 2.5f;

	private const float TimeToWindUp = 1.5f;

	private const float TimeToStopSummoning = 2f;

	private const int DeathPushOffsetX = 96;

	private const int DeathPushOffsetY = -16;

	private const int MaskPushForce = 5;

	private const float TimeDeadBeforeBurning = 2f;

	private static readonly Color BaseGlyphColor = new Color(0.1f, 0.2f, 0.3f, 0.35f);

	private static readonly Color BaseGlyphColor2 = new Color(0.1f, 0.25f, 0.25f, 0.35f);

	private static readonly Color BaseAuraColor = new Color(0.25f, 0.75f, 0.65f, 0.25f);

	private readonly CharacterSequenceSpecification _idleSequence;

	private readonly CharacterSequenceSpecification _playSequence;

	private readonly CharacterSequenceSpecification _stopSequence;

	private readonly CharacterSequenceSpecification _deathSequence;

	private readonly GyreRyshiaTearsParticleSystem _teardropParticleSystem;

	private readonly GyreRyshiaSummoningZone _summoningZone;

	private readonly Appendage[] _glyphAppendages = new Appendage[8];

	private bool _isFlickeringGlyph;

	private float _glyphFlickerTimer;

	private float _teardropsTimer;

	private Point _deathPoint;

	public GyreRyshia(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_currentAI = EAIStrategy.FollowAttack;
		_movementType = EAIMovementType.Fly;
		_nonAggroAction = EAIAction.FloatInPlace;
		_agility = 0.25f;
		_bboxOffset = new Point(2, 0);
		Bbox = new Rectangle(_position.X, _position.Y, 12, 22);
		_isAlwaysAggroed = true;
		_canLoseAggro = false;
		IsFacingLeft = !objectSpec.IsFlippedHorizontally;
		IsImageFacingLeft = IsFacingLeft;
		ChangeAnimation(0);
		_doesUseAppendageCollision = true;
		_doAppendagesMatchImageFacing = true;
		_doAppendagesInheritDrawColor = true;
		_doesDrawAppendages = false;
		_isAffectedByGravity = false;
		_isFlying = false;
		base.DoesDrawAura = true;
		base.DoesDrawAppendageAuras = true;
		base.AuraColor = BaseAuraColor;
		base.AuraOffset = new Vector2(1f, 1f);
		base.AuraFrequency = 9f;
		base.AuraSize = 0.1f;
		_auraCount = 5f;
		_summoningZone = new GyreRyshiaSummoningZone(_level, Position, _sprite);
		_level.RequestAddObject(_summoningZone);
		float num = 0f;
		for (int i = 0; i < 4; i++)
		{
			Appendage appendage = new Appendage(this, new Point(1, 1), new Point(28, 28), _level, _sprite)
			{
				FollowType = EAppendageFollowType.AnchorLocked,
				AnchorObject = this,
				AnchorOffset = new Point(0, -16),
				DrawOrigin = new Vector2(28f, 28f),
				DrawPriority = -1,
				DrawColor = Color.Transparent,
				DoesCollideWithAnything = false,
				DoesInheritDrawColor = false,
				IsFacingLocked = true,
				Rotation = num
			};
			appendage.ChangeAnimation(21);
			base.Appendages.Add(appendage);
			_glyphAppendages[i] = appendage;
			num += (float)Math.PI / 2f;
		}
		num = 0f;
		for (int j = 0; j < 4; j++)
		{
			Appendage appendage2 = new Appendage(this, new Point(1, 1), new Point(23, 23), _level, _sprite)
			{
				FollowType = EAppendageFollowType.AnchorLocked,
				AnchorObject = this,
				AnchorOffset = new Point(0, -16),
				DrawOrigin = new Vector2(23f, 23f),
				DrawPriority = -1,
				DrawColor = Color.Transparent,
				DoesCollideWithAnything = false,
				DoesInheritDrawColor = false,
				IsFacingLocked = true,
				Rotation = num
			};
			appendage2.ChangeAnimation(22);
			base.Appendages.Add(appendage2);
			_glyphAppendages[j + 4] = appendage2;
			num += (float)Math.PI / 2f;
		}
		_teardropParticleSystem = new GyreRyshiaTearsParticleSystem(_level.GCM.TxParticleEnergy, 8);
		_particleSystems.Add(_teardropParticleSystem);
		_idleSequence = GetCharacterSequenceByName("Idle");
		_playSequence = GetCharacterSequenceByName("Play");
		_stopSequence = GetCharacterSequenceByName("Stop");
		_deathSequence = GetCharacterSequenceByName("Death");
		SetCharacterSequence(_idleSequence);
		base.TimeToTurnAround = 0f;
	}

	public override void UpdateAbility(float delta)
	{
		if (_abilityTimer <= 0f)
		{
			if (!PickSummoningLocation())
			{
				_isCarryingOutAbility = false;
				_currentAction = EAIAction.FloatInPlace;
				_nextActionTimer = _timeToIdleAfterAttacking;
				return;
			}
			_summoningZone.IsActive = true;
			_summoningZone.PlayCue(ESFX.EnemyGyreRyshiaSummon);
			SetCharacterSequence(_playSequence);
		}
		float num = delta * 2.5f;
		for (int i = 0; i < 4; i++)
		{
			Appendage appendage = _glyphAppendages[i];
			appendage.Rotation += num;
			if (appendage.Rotation >= (float)Math.PI * 2f)
			{
				appendage.Rotation -= (float)Math.PI * 2f;
			}
		}
		for (int j = 4; j < 8; j++)
		{
			Appendage appendage2 = _glyphAppendages[j];
			appendage2.Rotation -= num;
			if (appendage2.Rotation <= 0f)
			{
				appendage2.Rotation += (float)Math.PI * 2f;
			}
		}
		if (_abilityTimer < 1.5f)
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
			float num2 = _abilityTimer - 1.5f;
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
		if (_abilityTimer >= 1.5f && _lastAbilityTimer < 1.5f)
		{
			SummonMonster();
		}
		if (_abilityTimer > 2f)
		{
			SetCharacterSequence(_stopSequence);
			_movementX = 0f;
			_isCarryingOutAbility = false;
			_currentAction = EAIAction.FloatInPlace;
			_nextActionTimer = _timeToIdleAfterAttacking;
		}
	}

	private bool PickSummoningLocation()
	{
		bool result = false;
		Point nearestProtagonistPosition = _level.GetNearestProtagonistPosition(Position);
		Point b = Position.Subtract(nearestProtagonistPosition).Multiply(0.5f);
		Point position = nearestProtagonistPosition.Add(b);
		int num = -1;
		int num2 = ((nearestProtagonistPosition.X < Position.X) ? (-16) : 16);
		int i = position.X;
		for (int x = _level.RoomSize.X; i > 0 && i < x; i += num2)
		{
			num = _level.GetFloorY(position);
			if (num != -1)
			{
				break;
			}
		}
		if (num != -1)
		{
			result = true;
			_summoningZone.Move(new Point(i, num));
		}
		return result;
	}

	private void SummonMonster()
	{
		_summoningZone.IsActive = false;
		_summoningZone.IsReadyToSummon = true;
		_summoningZone.IsSummonFacingLeft = _summoningZone.Position.X < Position.X;
	}

	protected override void UpdateDeathScript(float delta)
	{
		if (_deathScriptTimer <= 0f && delta > 0f)
		{
			_summoningZone.IsActive = false;
			DropLoot();
			base.IsSolidWhenFrozen = false;
			PlayCue(ESFX.EnemyGyreRyshiaDeath);
			_level.PlayCue(ESFX.EnemyGyreRyshiaDeath2D);
			SetCharacterSequence(_deathSequence);
			_deathPoint = Position;
			Appendage[] glyphAppendages = _glyphAppendages;
			foreach (Appendage appendage in glyphAppendages)
			{
				appendage.ChangeAnimation(-1);
			}
			if (base.Appendages.Count > 4)
			{
				Appendage appendage2 = base.Appendages[4];
				Vector2 force = new Vector2(IsFacingLeft ? (-5) : 5, -5f);
				DebrisEvent.CreateFromAppendage(appendage2, force, appendage2.Position, _sprite, DebrisEvent.EDebrisDeathType.Dust);
				appendage2.ChangeAnimation(-1);
			}
		}
		if (_deathScriptTimer > 2f)
		{
			RemoveInstance();
		}
		else
		{
			float num = _deathScriptTimer / 2f;
			int num2 = (int)(Math.Sin(num * ((float)Math.PI / 2f)) * 96.0);
			int num3 = (int)Math.Ceiling(Math.Sin(num * (float)Math.PI) * -16.0);
			Position = new Point(_deathPoint.X + (IsFacingLeft ? num2 : (-num2)), _deathPoint.Y + num3);
			float num4 = 1f - num;
			base.DrawColor = Color.White * num4;
			base.AuraColor = BaseAuraColor * num4;
			SnapBboxToPosition();
			SnapFrameToBbox();
			if (num < 0.9f)
			{
				_teardropsTimer += delta;
				if (_teardropsTimer >= 0.25f)
				{
					_teardropsTimer -= 0.25f;
					num2 = (IsFacingLeft ? 4 : (-4));
					_teardropParticleSystem.AddParticles(new Vector2(Position.X + num2, Bbox.Top + -4));
				}
			}
			UpdateCharacterSequences(delta);
			UpdateAnimation(delta);
			UpdateAppendages(delta);
			UpdateTrail(delta);
			UpdateParticleSystems(delta);
		}
		_deathScriptTimer += delta;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		DrawAppendages(spriteBatch, drawUnder: true);
		base.Draw(spriteBatch);
		DrawAppendages(spriteBatch, drawUnder: false);
	}
}
