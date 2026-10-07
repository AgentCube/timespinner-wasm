using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Bosses.Z_Zel;
using Timespinner.GameObjects.Enemies;

namespace Timespinner.GameAbstractions.GameObjects;

internal sealed class ZelBoss : BossClass
{
	private enum EZelBossState
	{
		Idle,
		Hellfire,
		Spikes,
		DarkInferno,
		SwordSwipe,
		IntroCutscene,
		TurningAround
	}

	private const int StartOffsetX = 4;

	private const int GyreEnemyID = 388;

	private const int GyreZelArgument = 5;

	private const int GyreZelStartOffsetY = 32;

	private const int HellfireFlameCount = 50;

	private const int SpikeCount = 3;

	private const int RoomCenterX = 384;

	private const int MovesBeforeDestroyingPillars = 2;

	private const int SwordDistanceThreshold = 176;

	private const float IndefiniteActionTime = 1000f;

	private const float TimeBetweenAttacks = 1f;

	private const int HellfireEmissionRadius = 48;

	private const int HellfireFlameSpeed = 200;

	private const float FlameCirclePlacementMultiplier = 0.105f;

	private const float TimeForHellfireGlyphSummon = 0.5f;

	private const float TimeBetweenHellfireFlames = 0.05f;

	private const float TimeForHellfireFlames = 2.5f;

	private const float TimeForHellfireEnd = 0.25f;

	private const float TimeBeforeHellfireEnd = 3f;

	private const float TimeForEntireHellfireAttack = 3.25f;

	private const int CeilingY = 16;

	private const int SpikesBufferX = 96;

	private const int BaseSpikeOffset = 128;

	private const float TimeForSpikesSummonGlyph = 1f;

	private const float SpikeSleepTime = 0.5f;

	private const float TimeForAllSpikesToSummon = 1.5f;

	private const float TimeForSpikesUnsummonGlyph = 0.5f;

	private const float TimeBeforeSpikesUnsummonGlyph = 2.5f;

	private const float TimeToWaitBeforeSpikesNextAttack = 0.5f;

	private const float TimeForSpikesSequenceEntireAttack = 3.5f;

	private const int DarkInfernoPlayerDistanceNearThresholdX = 150;

	private const int DarkInfernoPlayerDistanceMidThresholdX = 224;

	private const int DarkInfernoFlameSpeedX = 400;

	private const int DarkInfernoFlameNearSpeedY = 40;

	private const int DarkInfernoFlameMidSpeedY = 0;

	private const int DarkInfernoFlameFarSpeedY = -40;

	private const float TimeForDarkInfernoGlyphSummon = 0.5f;

	private const float TimeForDarkInfernoCharge = 1f;

	private const float TimeForDarkInfernoEnd = 0.25f;

	private const float TimeBeforeDarkInfernoEnd = 1.5f;

	private const float TimeForEntireDarkInfernoAttack = 1.75f;

	private const float TimeForSwordSwipe = 1f;

	private const float TimeBeforeSwordSwipeCastSequence = 0.25f;

	private const float TimeForZelTurnAroundFade = 0.75f;

	private const float TimeForEntireZelTurnAroundSequence = 1.5f;

	private const int DeathParticlesOffsetX = 0;

	private const int DeathParticlesOffsetY = -48;

	private const float TimeForDeathChargeUp = 2f;

	private const float TimeBeforeDeathExplosionFlash = 1.5f;

	private const float TimeForDeathExplosion = 2f;

	private const float TimeForEntireDeathSequence = 4f;

	private static readonly Color BaseAuraColor = new Color(0.9f, 0.4f, 0.2f, 0.25f);

	private static readonly Color DarkInfernoGlyphColor = new Color(200, 128, 64, 128);

	private static readonly Color HellfireGlyphColor = new Color(200, 128, 64, 128);

	private static readonly Color DeathGlowColor = new Color(1f, 1f, 1f, 0.5f);

	private readonly int _baseDamageCaused;

	private readonly CharacterSequenceSpecification _idleSequence;

	private readonly CharacterSequenceSpecification _castSequence;

	private readonly ZelPagesParticleSystem _verticalPageParticles;

	private readonly ZelPagesParticleSystem _horizontalPageParticles;

	private readonly BirdBossDeathChargeLazerPS _deathLazerParticles;

	private readonly ZelGlyphAppendage _glyph;

	private readonly ZelBossSwordAppendage _sword;

	private readonly GyreZel _zelEnemy;

	private readonly ZelBossHellfire[] _hellfireFlames = new ZelBossHellfire[50];

	private readonly ZelBossSpike[] _spikes = new ZelBossSpike[3];

	private bool _isDrawingGlyph;

	private bool _isPlayerToOurLeft;

	private bool _arePillarsDown;

	private bool _arePillarsToTheLeft;

	private EZelBossState _zelState;

	private EZelBossState _nextZelState;

	private int _lastRandomState;

	private int _createdFireballCount;

	private int _usedSpikeCount;

	private int _movesUsedSincePillarsFell;

	private float _zelTimer;

	private float _lastZelTimer;

	private ZelInfernoProjectile _infernoProjectile;

	public ZelBoss(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		Position = new Point(Position.X + 4, Position.Y);
		ChangeAnimation(-1);
		_doesDrawBaseSprite = false;
		_baseDamageCaused = base.Damage;
		_currentAI = EAIStrategy.CustomScriptAI;
		_currentAction = EAIAction.Idle;
		_zelState = EZelBossState.IntroCutscene;
		_agility = 1f;
		_bboxOffset = new Point(16, 8);
		Bbox = new Rectangle(_position.X, _position.Y, 16, 16);
		base.CannotBeGrabbed = true;
		_isFlying = true;
		_isAffectedByGravity = false;
		_doAppendagesInheritDrawColor = true;
		_doAppendagesMatchImageFacing = true;
		base.DoesDrawAura = true;
		base.DoesDrawAppendageAuras = true;
		base.AuraColor = BaseAuraColor;
		base.AuraOffset = new Vector2(2f, 2f);
		base.AuraFrequency = 9f;
		base.AuraSize = 0.075f;
		_auraCount = 5f;
		_idleSequence = GetCharacterSequenceByName("Idle");
		_castSequence = GetCharacterSequenceByName("StartCast");
		SetCharacterSequence(_idleSequence);
		_glyph = new ZelGlyphAppendage(this, new Point(16, 16), Point.Zero, _level, _sprite);
		_sword = new ZelBossSwordAppendage(this, new Point(1, 1), new Point(23, 0), _level, _sprite);
		_horizontalPageParticles = new ZelPagesParticleSystem(_sprite, 1, isHorizontal: true);
		_verticalPageParticles = new ZelPagesParticleSystem(_sprite, 1, isHorizontal: false);
		_particleSystems.Add(_horizontalPageParticles);
		_particleSystems.Add(_verticalPageParticles);
		_deathLazerParticles = new BirdBossDeathChargeLazerPS(_level.GCM.TxParticleEnergy, 5);
		_damageCaused = 0;
		base.DrawColor = Color.Transparent;
		base.AuraColor = Color.Transparent;
		base.IsSolidWhenFrozen = false;
		base.IsDormant = true;
		_canBeDamaged = false;
		_zelEnemy = new GyreZel(new Point(Position.X, Position.Y + 32), _level, _level.GCM.SpGyreZel, -1, new ObjectTileSpecification(388)
		{
			Argument = 5
		});
		_level.RequestAddObject(_zelEnemy);
		_zelEnemy.StartBossIntroCutscene();
	}

	internal void StartBattle()
	{
		_zelState = EZelBossState.Idle;
		base.IsDormant = false;
		base.IsSolidWhenFrozen = true;
		_damageCaused = _baseDamageCaused;
		_isInvulnerable = false;
		_canBeDamaged = true;
		_isAlwaysInvulnerable = false;
		base.DoesTouchDamageKnockback = true;
		Vector2 where = _zelEnemy.BookCenter.ToVector2();
		_verticalPageParticles.AddParticles(where);
		_horizontalPageParticles.AddParticles(where);
		base.DrawColor = Color.White;
		base.AuraColor = BaseAuraColor;
		_level.SetLevelDrawColor(Color.White);
		_isPlayerToOurLeft = _level.GetPlayerPosition().X < Position.X;
		IsFacingLeft = _isPlayerToOurLeft;
		_zelEnemy.SilentKill();
	}

	protected override void PickNextCustomScriptAIAction(float delta)
	{
		_currentAction = EAIAction.Custom;
		_zelTimer = 0f;
		_lastZelTimer = -1E-07f;
		if (_zelState == EZelBossState.IntroCutscene)
		{
			return;
		}
		Point playerPosition = _level.GetPlayerPosition();
		_isPlayerToOurLeft = playerPosition.X < Position.X;
		int num = Math.Abs(playerPosition.X - Position.X);
		bool flag = num <= 176;
		_nextActionTimer = 1000f;
		int num2 = 0;
		if (_arePillarsDown && _isPlayerToOurLeft == _arePillarsToTheLeft)
		{
			if (_movesUsedSincePillarsFell < 2)
			{
				_movesUsedSincePillarsFell++;
				num2 = _level.NextRandomInt(0, 2);
				switch (num2)
				{
				case 0:
					_zelState = EZelBossState.Hellfire;
					break;
				case 1:
					_zelState = ((!flag) ? EZelBossState.Hellfire : EZelBossState.SwordSwipe);
					break;
				case 2:
					_zelState = EZelBossState.DarkInferno;
					break;
				}
			}
			else
			{
				_zelState = EZelBossState.DarkInferno;
			}
		}
		else
		{
			num2 = _level.NextRandomInt(0, 2);
			if (num2 == _lastRandomState)
			{
				num2 = (num2 + 1) % 3;
			}
			switch (num2)
			{
			case 0:
				_zelState = EZelBossState.Hellfire;
				break;
			case 1:
				_zelState = (flag ? EZelBossState.SwordSwipe : EZelBossState.DarkInferno);
				break;
			case 2:
				_zelState = EZelBossState.Spikes;
				break;
			}
		}
		if (_isPlayerToOurLeft != IsFacingLeft)
		{
			_nextZelState = _zelState;
			_zelState = EZelBossState.TurningAround;
		}
		_lastRandomState = num2;
	}

	protected override void UpdateCustomScriptAIAction(float delta)
	{
		bool flag = false;
		switch (_zelState)
		{
		case EZelBossState.Hellfire:
			UpdateHellfire();
			break;
		case EZelBossState.Spikes:
			UpdateSpikes();
			break;
		case EZelBossState.DarkInferno:
			UpdateDarkInferno();
			break;
		case EZelBossState.SwordSwipe:
			UpdateSwordSwipe();
			break;
		case EZelBossState.IntroCutscene:
			UpdateIntroCutscene();
			break;
		case EZelBossState.TurningAround:
			flag = true;
			UpdateZelTurnAround();
			break;
		default:
			FinishAttack();
			break;
		}
		if (!flag || _zelState == EZelBossState.TurningAround)
		{
			_lastZelTimer = _zelTimer;
			_zelTimer += delta;
		}
	}

	private void FinishAttack()
	{
		_nextActionTimer = 1f;
		_currentAction = EAIAction.Idle;
		SetCharacterSequence(_idleSequence);
	}

	private void UpdateHellfire()
	{
		if (_zelTimer <= 0.5f)
		{
			if (_lastZelTimer < 0f)
			{
				SetCharacterSequence(_castSequence);
				PlayCue(ESFX.BossZelFirespellCast);
			}
			_isDrawingGlyph = true;
			_createdFireballCount = 0;
			float num = _zelTimer / 0.5f;
			Color drawColor = Color.Transparent.Lerp(HellfireGlyphColor, num);
			_glyph.AuraPercentage = MathHelper.Clamp(num * 2f, 0f, 1f);
			_glyph.DrawColor = drawColor;
		}
		else if (_zelTimer <= 3f)
		{
			if (_lastZelTimer <= 0.5f)
			{
				_isPlayerToOurLeft = _level.GetPlayerPosition().X < Position.X;
				_glyph.AuraPercentage = 1f;
				_glyph.DrawColor = HellfireGlyphColor;
			}
			float num2 = _zelTimer - 0.5f;
			for (int i = _createdFireballCount; i < 50; i++)
			{
				float num3 = (float)i * 0.05f;
				if (num3 <= num2)
				{
					EmitHellfireFlame(i);
					_createdFireballCount++;
					continue;
				}
				break;
			}
		}
		else if (_zelTimer < 3.25f)
		{
			float num4 = (_zelTimer - 3f) / 0.25f;
			Color drawColor2 = HellfireGlyphColor.Lerp(Color.Transparent, num4);
			_glyph.DrawColor = drawColor2;
			if (num4 >= 0.5f)
			{
				_glyph.AuraPercentage = MathHelper.Clamp(1f - (num4 - 0.5f) * 2f, 0f, 1f);
			}
		}
		else
		{
			_isDrawingGlyph = false;
			FinishAttack();
		}
	}

	private void EmitHellfireFlame(int flameIndex)
	{
		float num = (float)flameIndex * 0.105f * ((float)Math.PI * 2f) * (float)(_isPlayerToOurLeft ? 1 : (-1));
		int num2 = (int)Math.Round(Math.Cos(num) * 48.0);
		int num3 = (int)Math.Round(Math.Sin(num) * 48.0);
		Point glyphCenter = _glyph.GlyphCenter;
		Point startPoint = new Point(glyphCenter.X + num2, glyphCenter.Y + num3);
		if (_hellfireFlames[flameIndex] == null)
		{
			int basePower = (int)Math.Round((float)base.Damage * 1.15f);
			ZelBossHellfire zelBossHellfire = new ZelBossHellfire(_level, Position, Vector2.Zero, _sprite, basePower);
			_hellfireFlames[flameIndex] = zelBossHellfire;
		}
		Vector2 iV = new Vector2(200 * ((!_isPlayerToOurLeft) ? 1 : (-1)), 0f);
		if (_hellfireFlames[flameIndex] != null)
		{
			ZelBossHellfire zelBossHellfire2 = _hellfireFlames[flameIndex];
			zelBossHellfire2.Reset(startPoint, iV);
			_level.RequestAddObject(zelBossHellfire2);
		}
	}

	private void UpdateSpikes()
	{
		if (_zelTimer < 3.5f)
		{
			if (_lastZelTimer < 0f)
			{
				SetCharacterSequence(_castSequence);
			}
			if (_zelTimer <= 1f)
			{
				return;
			}
			if (_lastZelTimer <= 1f && _zelTimer > 1f)
			{
				_isPlayerToOurLeft = _level.GetPlayerPosition().X < Position.X;
				float num = 0f;
				int num2 = 0;
				int num3 = (_isPlayerToOurLeft ? (-96) : 96);
				for (int i = 0; i < 3; i++)
				{
					int x = 384 + (_isPlayerToOurLeft ? (-128) : 128) + num2;
					AddSpike(new Point(x, 16), num);
					num += 0.5f;
					num2 += num3;
				}
				PlayCue(ESFX.BossZelStonePillarsCast);
			}
			else if (!(_zelTimer > 2.5f))
			{
			}
		}
		else
		{
			_arePillarsDown = true;
			_arePillarsToTheLeft = _isPlayerToOurLeft;
			FinishAttack();
		}
	}

	private void AddSpike(Point startPoint, float sleepTime)
	{
		ZelBossSpike zelBossSpike = null;
		if (_usedSpikeCount < 3)
		{
			zelBossSpike = new ZelBossSpike(_level, startPoint, _sprite, sleepTime, base.Damage);
			_spikes[_usedSpikeCount] = zelBossSpike;
			_usedSpikeCount++;
		}
		else
		{
			for (int i = 0; i < 3; i++)
			{
				if (_spikes[i].IsFinished)
				{
					zelBossSpike = _spikes[i];
					zelBossSpike.Reset(startPoint, sleepTime);
					break;
				}
			}
		}
		if (zelBossSpike != null)
		{
			_level.AddProjectile(zelBossSpike);
		}
	}

	private void UpdateDarkInferno()
	{
		if (_zelTimer <= 0.5f)
		{
			if (_lastZelTimer < 0f)
			{
				SetCharacterSequence(_castSequence);
				if (_infernoProjectile == null)
				{
					_infernoProjectile = new ZelInfernoProjectile(_level, Position, Vector2.Zero, ETeamSide.Enemies, _level.GCM.SpOrbMeleeBook, base.Damage);
				}
				else
				{
					_infernoProjectile.Position = Position;
				}
				_infernoProjectile.PlayCue(ESFX.BossZelMagma);
			}
			_isDrawingGlyph = true;
			_createdFireballCount = 0;
			float num = _zelTimer / 0.5f;
			Color drawColor = Color.Transparent.Lerp(DarkInfernoGlyphColor, num);
			_glyph.AuraPercentage = MathHelper.Clamp(num * 2f, 0f, 1f);
			_glyph.DrawColor = drawColor;
		}
		else if (_zelTimer <= 1.5f)
		{
			if (_lastZelTimer <= 0.5f)
			{
				Point playerPosition = _level.GetPlayerPosition();
				_isPlayerToOurLeft = playerPosition.X < Position.X;
				_glyph.AuraPercentage = 1f;
				_glyph.DrawColor = DarkInfernoGlyphColor;
				EmitDarkInfernoFlame(playerPosition);
			}
		}
		else if (_zelTimer < 1.75f)
		{
			float num2 = (_zelTimer - 1.5f) / 0.25f;
			Color drawColor2 = DarkInfernoGlyphColor.Lerp(Color.Transparent, num2);
			_glyph.DrawColor = drawColor2;
			if (num2 >= 0.5f)
			{
				_glyph.AuraPercentage = MathHelper.Clamp(1f - (num2 - 0.5f) * 2f, 0f, 1f);
			}
		}
		else
		{
			_movesUsedSincePillarsFell = 0;
			_arePillarsDown = false;
			_isDrawingGlyph = false;
			FinishAttack();
		}
	}

	private void EmitDarkInfernoFlame(Point targetPoint)
	{
		Point glyphCenter = _glyph.GlyphCenter;
		Point startPoint = glyphCenter;
		int num = Math.Abs(targetPoint.X - startPoint.X);
		int num2 = -40;
		if (!_arePillarsDown)
		{
			if (num < 150)
			{
				num2 = 40;
			}
			else if (num < 224)
			{
				num2 = 0;
			}
		}
		Vector2 iV = new Vector2(400 * ((!_isPlayerToOurLeft) ? 1 : (-1)), num2);
		if (_infernoProjectile.Reset(startPoint, iV))
		{
			_level.AddProjectile(_infernoProjectile);
		}
	}

	private void UpdateSwordSwipe()
	{
		if (_zelTimer >= 0.25f && _lastZelTimer < 0.25f)
		{
			SetCharacterSequence(_castSequence);
		}
		if (_zelTimer <= 0f)
		{
			_isPlayerToOurLeft = _level.GetPlayerPosition().X < Position.X;
			_sword.Reset(Position, _isPlayerToOurLeft, base.Damage);
			base.Appendages.Add(_sword);
		}
		else if (_zelTimer >= 1f)
		{
			FinishAttack();
		}
	}

	private void UpdateIntroCutscene()
	{
		_nextActionTimer = 10f;
		if (!(_zelTimer < _zelEnemy.TimeForBossIntroSequence))
		{
			StartBattle();
		}
	}

	private void UpdateZelTurnAround()
	{
		if (_zelTimer < 1.5f)
		{
			if (_zelTimer < 0.75f)
			{
				if (_zelTimer <= 0f)
				{
					PlayCue(ESFX.BossZelDisappear);
				}
				float num = (float)Math.Cos((float)Math.PI / 2f * _zelTimer / 0.75f);
				base.DrawColor = Color.White * (num * num);
				base.AuraColor = BaseAuraColor * num;
				if (num < 0.5f)
				{
					_damageCaused = 0;
					_canBeDamaged = false;
				}
				return;
			}
			if (_lastZelTimer < 0.75f)
			{
				_isPlayerToOurLeft = _level.GetPlayerPosition().X < Position.X;
				IsFacingLeft = _isPlayerToOurLeft;
				if (_arePillarsDown && _isPlayerToOurLeft != _arePillarsToTheLeft)
				{
					FadeOutPillars();
				}
				PlayCue(ESFX.BossZelReappear);
			}
			float num2 = (float)Math.Sin((float)Math.PI / 2f * (_zelTimer - 0.75f) / 0.75f);
			base.DrawColor = Color.White * (num2 * num2);
			base.AuraColor = BaseAuraColor * num2;
			if (num2 > 0.5f)
			{
				_canBeDamaged = true;
			}
		}
		else
		{
			_damageCaused = _baseDamageCaused;
			_canBeDamaged = true;
			base.DrawColor = Color.White;
			base.AuraColor = BaseAuraColor;
			_zelState = _nextZelState;
			_zelTimer = 0f;
			_lastZelTimer = -0.001f;
		}
	}

	private void FadeOutPillars()
	{
		ZelBossSpike[] spikes = _spikes;
		for (int i = 0; i < spikes.Length; i++)
		{
			spikes[i]?.FadeKill();
		}
		_arePillarsDown = false;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			if (_isDrawingGlyph)
			{
				_glyph.Update(delta);
			}
			if (!_sword.IsFinished)
			{
				_sword.CurrentTarget = Position;
			}
			else if (!_sword.IsAvailable)
			{
				base.Appendages.Remove(_sword);
				_sword.IsAvailable = true;
			}
		}
		base.Update(delta);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
		if (_isDrawingGlyph)
		{
			_glyph.Draw(spriteBatch);
		}
	}

	protected override void UpdateDeathScript(float delta)
	{
		float deathScriptTimer = _deathScriptTimer;
		_deathScriptTimer += delta;
		if (_deathScriptTimer < 4f)
		{
			Vector2 where = new Vector2(Position.X, Position.Y + -48);
			if (_deathScriptTimer < 2f)
			{
				if (deathScriptTimer <= 0f)
				{
					if (_arePillarsDown)
					{
						FadeOutPillars();
					}
					_glyph.FadeOut();
					SetCharacterSequenceByName("Death");
					_level.PlayCue(ESFX.BossZelDeath, Position);
					_deathParticleColorVect = _deathParticlesColor.ToVector4();
					base.DrawColor = Color.White;
					base.AuraColor = BaseAuraColor;
					_deathParticleSystems[0] = _deathLazerParticles;
					foreach (Appendage appendage in base.Appendages[0].Appendages)
					{
						appendage.DoesInheritDrawColor = true;
					}
				}
				_deathLazerParticles.AddParticles(where);
				_isGlowing = true;
				_glowColor = DeathGlowColor;
				float num = _deathScriptTimer / 2f;
				float num2 = 1f - (float)Math.Cos(num * ((float)Math.PI / 2f));
				_glowBase = 1f + num2 * 10f;
				if (_deathScriptTimer >= 1.5f && deathScriptTimer < 1.5f)
				{
					_level.RequestScreenFlash(1.01f, 1f, 1f);
				}
			}
			else if (deathScriptTimer < 2f)
			{
				Point center = base.OuterBbox.Center;
				_horizontalPageParticles.KillOffParticles(0f);
				_verticalPageParticles.KillOffParticles(0f);
				_level.AddAnimation(new BattleAnimation(null, center, _level)
				{
					ParticleSystem = _horizontalPageParticles
				});
				_level.AddAnimation(new BattleAnimation(null, center, _level)
				{
					ParticleSystem = _verticalPageParticles
				});
				_particleSystems.Remove(_horizontalPageParticles);
				_particleSystems.Remove(_verticalPageParticles);
				_doesDrawSpriteAndAppendages = false;
				_doesDrawTrail = false;
				_doesDrawBrushTrail = false;
				base.DoesDrawAura = false;
				DropLoot();
				_deathLazerParticles.KillOffParticles(0f);
			}
			ParticleSystem[] deathParticleSystems = _deathParticleSystems;
			for (int i = 0; i < deathParticleSystems.Length; i++)
			{
				deathParticleSystems[i]?.Update(delta);
			}
		}
		else
		{
			_level.SetLevelSaveBool("IsGyreBossDead", value: true);
			EndBossDeathScript();
		}
	}

	internal override void InitializeForBestiary()
	{
		base.DrawColor = Color.White;
		base.AuraColor = BaseAuraColor;
	}
}
