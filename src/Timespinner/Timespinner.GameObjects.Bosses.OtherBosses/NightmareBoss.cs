using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Bosses.Nightmare;
using Timespinner.GameObjects.Events.Cutscene;
using Timespinner.GameObjects.Events.EnvironmentPrefabs;
using Timespinner.GameObjects.Events.EnvironmentPrefabs.L16_Temple;
using Timespinner.GameObjects.Events.Misc;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameObjects.Bosses.OtherBosses;

internal sealed class NightmareBoss : BossClass
{
	private enum ENightmareAbility
	{
		None,
		FireBreath,
		Slash,
		Firebomb,
		Hellfire,
		Apocalypse,
		Intro
	}

	private enum ENightmareState
	{
		Floating,
		Invisible,
		Latched,
		ToFloatingFromLatched,
		ToFloatingFromInvisible,
		ToInvisible,
		ToLatched
	}

	private const int PrefabID = 491;

	private const int HellfireArgument = 1603;

	private const int MaxFireBreathProjectiles = 64;

	private const int MaxFireBombProjectiles = 6;

	private const int MaxApocHellfireDamageAreas = 4;

	private const float CriticalModeHealthThreshold = 0.5f;

	private const float TimeThatIsTooLongForPlayerToNotBeGrounded = 5f;

	private const float TimeToLatch = 1f;

	private const float TimeToInvisible = 1f;

	private const float TimeToFloat = 1f;

	private const float TimeToStayInvisible = 1f;

	private const float TimeForEntireSlashAbility = 2f;

	private const int FireBombInitialSpeed = 500;

	private const float TimeForDroppingFirebombs = 1.5f;

	private const float TimeForFireBombRecover = 1f;

	private const float TimeForEntireFirebombAbility = 2.5f;

	private const float BaseFireAngle = 0.65f;

	private const float BaseFireSpeed = 425f;

	private const float FireAngleJitter = 0.3f;

	private const float TimeForFireBreathWindup = 1f;

	private const float TimeForFireBreathBreathing = 1.25f;

	private const float TimeForEntireFireBreathAbility = 2.25f;

	private const float TimeBetweenFlameEmission = 0.03f;

	private const float TimeForEntireHellfireAbility = 3f;

	private const float TimeForApocDisappear = 1.5f;

	private const float TimeForApocHellfire = 2f;

	private const float TimeForApocReappear = 0.75f;

	private const float TimeBeforeApocReappear = 3.5f;

	private const float TimeForEntireApocAbility = 4.25f;

	private const int DeathFallOffsetX = 32;

	private const int DeathFallOffsetY = 340;

	private const int CornerX = 400;

	private const int CornerY = 240;

	private const float TimeForDeathSequenceFall = 2.6f;

	private const float TimeForDeathSequenceWait = 1f;

	private const float TimeForDeathSequenceExplosion = 0.75f;

	private const float TimeForDeathSequencePostExplosion = 2f;

	private const float TimeForDeathSequenceLevelFadeOut = 2f;

	private const float TimeForDeathSequenceScreenFadeOut = 1f;

	private const float TimeBeforeDeathSequenceExplosion = 3.6f;

	private const float TimeBeforeDeathSequencePostExplosion = 4.35f;

	private const float TimeBeforeDeathSequenceLevelFadeOut = 6.35f;

	private const float TimeBeforeDeathSequenceScreenFadeOut = 8.35f;

	private const float TimeForEntireDeathSequence = 9.35f;

	private const float TimeBetweenDeathParticleEmission = 0.03f;

	private const int IntroOffsetX = 96;

	private const int IntroOffsetY = -48;

	private const float TimeForIntro = 1.5f;

	private static readonly Color BaseAuraColor = new Color(0.15f, 0.33f, 0f, 0.25f);

	private static readonly Color DeathColor = new Color(0.95f, 0.8f, 1f);

	private readonly int _baseTouchDamage;

	private readonly Point _spawnPoint;

	private readonly CharacterSequenceSpecification _idleSequence;

	private readonly CharacterSequenceSpecification _clawSequence;

	private readonly CharacterSequenceSpecification _breathSequence;

	private readonly CharacterSequenceSpecification _hellfireSequence;

	private readonly CharacterSequenceSpecification _firebombSequence;

	private readonly CharacterSequenceSpecification _apocalypseSequence;

	private readonly CharacterSequenceSpecification _deathSequence;

	private readonly Appendage _headAppendage;

	private readonly Appendage _handAppendage;

	private readonly NightmarePreBombProjectile _preFireBombProjectile;

	private readonly NightmareHellfireDamageArea _hellfireDamageArea;

	private readonly NightmareHellfireDamageArea[] _apocHellfireDamageAreas = new NightmareHellfireDamageArea[4];

	private readonly NightmareFireBreathProjectile[] _fireBreathProjectiles = new NightmareFireBreathProjectile[64];

	private readonly NightmareFireBombProjectile[] _fireBombProjectiles = new NightmareFireBombProjectile[6];

	private bool _isInCriticalMode;

	private bool _hasUsedApocalypse;

	private bool _hasDoneIntro;

	private ENightmareState _nightmareState;

	private ENightmareAbility _nightmareAbility;

	private int _fireBreathesEmitted;

	private int _lastMoveInt = -1;

	private float _stateTimer;

	private float _timeSincePlayerGrounded;

	private float _fireBreathEmissionTimer;

	private float _particleEmissionTimer;

	private NightmareDeathSmokeParticleSystem _deathSmokeParticles;

	private NightmareDeathAshParticleSystem _deathAshParticles;

	private NightmareExplosionBurstParticleSystem _explosionBurstParticles;

	private NightmareExplosionSmokeParticleSystem _explosionSmokeParticles;

	public NightmareBoss(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		_spawnPoint = _position;
		Bbox = new Rectangle(_position.X, _position.Y, 16, 16);
		base.DoesDrawBaseSprite = false;
		ChangeAnimation(-1);
		IsFacingLeft = objectSpec == null || !objectSpec.IsFlippedHorizontally;
		_currentAI = EAIStrategy.CustomScriptAI;
		_currentAction = EAIAction.Custom;
		_agility = 1f;
		_isAffectedByLevelBounds = false;
		_isAffectedByGravity = false;
		_isFlying = true;
		base.CannotBeGrabbed = true;
		_doAppendagesMatchImageFacing = true;
		_doAppendagesInheritDrawColor = true;
		_doesUseAppendageCollision = true;
		base.DoesDrawAura = true;
		base.DoesDrawAppendageAuras = true;
		base.AuraColor = BaseAuraColor;
		base.AuraOffset = new Vector2(-1f, 0f);
		base.AuraFrequency = 1f;
		base.AuraSize = 0.02f;
		_auraCount = 5f;
		SetDoesDrawAppendageTrails(value: true, isHost: true, 6, 3f);
		_baseTouchDamage = base.Damage;
		SpriteSheet spDifferenceCloud = _level.GCM.SpDifferenceCloud;
		_hellfireDamageArea = new NightmareHellfireDamageArea(_level, Point.Zero, base.Damage, spDifferenceCloud, 0);
		_preFireBombProjectile = new NightmarePreBombProjectile(_level, Point.Zero, Vector2.Zero, _sprite, base.Damage);
		for (int i = 0; i < 4; i++)
		{
			int hellfireType = i + 1;
			NightmareHellfireDamageArea nightmareHellfireDamageArea = new NightmareHellfireDamageArea(_level, Point.Zero, base.Damage, spDifferenceCloud, hellfireType);
			_apocHellfireDamageAreas[i] = nightmareHellfireDamageArea;
		}
		if (base.Appendages.Count > 0)
		{
			_headAppendage = base.Appendages[0];
			_handAppendage = base.Appendages[1];
			_headAppendage.DoesDrawAppendagesInReverse = true;
		}
		_idleSequence = GetCharacterSequenceByName("Idle");
		_clawSequence = GetCharacterSequenceByName("Claw");
		_breathSequence = GetCharacterSequenceByName("Breath");
		_firebombSequence = GetCharacterSequenceByName("Firebomb");
		_hellfireSequence = GetCharacterSequenceByName("Hellfire");
		_apocalypseSequence = GetCharacterSequenceByName("Apocalypse");
		_deathSequence = GetCharacterSequenceByName("Death");
		SetCharacterSequence(_idleSequence);
		EnvPrefabTempleHellfire newObject = new EnvPrefabTempleHellfire(inPosition: new Point(_level.RoomSize.X / 2, _level.RoomSize.Y / 2), inLevel: _level, inID: -1, objectSpec: new ObjectTileSpecification(491)
		{
			Argument = 1603
		}, prefabType: EEnvironmentPrefabType.L16_Hellfire, isOpening: false);
		_level.RequestAddObject(newObject);
	}

	public override void InitializeMob()
	{
		_level.ToggleExits(isEnabled: false);
		_level.OpenAllBossDoors(-1f);
		_level.LockAllBossDoors(0.5f);
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.Idle, 0f, 0f, Vector4.Zero));
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.StopCast, 0f, 0f, Vector4.Zero));
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.LookDirection, 0f, 0f, new Vector4(1f, 0f, 0f, 0f)));
		_level.AddScriptToPlayer1(new ScriptAction(EScriptActionType.Idle, 0f, 0.55f, Vector4.Zero));
		base.IsBossIntroInProgress = true;
		StartBossIntroCutscene();
	}

	protected override void StartBossIntroCutscene()
	{
		AddWaitScript(0.05f);
		AddDelegateScript(EndBossIntroCutscene);
	}

	public override void Update(float delta)
	{
		if (_isFrozen)
		{
			_isFrozen = false;
		}
		if (!_isInCriticalMode && base.HPPercentage < 0.5f)
		{
			_isInCriticalMode = true;
		}
		Protagonist mainHero = _level.MainHero;
		if (mainHero != null)
		{
			if (mainHero.IsGrounded)
			{
				_timeSincePlayerGrounded = 0f;
			}
			else
			{
				_timeSincePlayerGrounded += delta;
			}
		}
		base.Update(delta);
	}

	protected override void PickNextCustomScriptAIAction(float delta)
	{
		int start = 1;
		int num = (_isInCriticalMode ? 5 : 4);
		if (_timeSincePlayerGrounded > 5f)
		{
			start = 4;
		}
		int num2 = _level.NextRandomInt(start, num);
		if (num2 == _lastMoveInt)
		{
			num2++;
			if (num2 > num)
			{
				num2 = 1;
			}
		}
		if (_isInCriticalMode && !_hasUsedApocalypse)
		{
			_hasUsedApocalypse = true;
			num2 = 5;
		}
		if (!_hasDoneIntro)
		{
			_hasDoneIntro = true;
			num2 = 6;
		}
		_nightmareAbility = (ENightmareAbility)num2;
		_lastMoveInt = num2;
		_currentAction = EAIAction.Custom;
		_stateTimer = 0f;
		_nextActionTimer = 100f;
	}

	protected override void UpdateCustomScriptAIAction(float delta)
	{
		_stateTimer += delta;
		switch (_nightmareState)
		{
		case ENightmareState.ToLatched:
			if (_stateTimer >= 1f)
			{
				_nightmareState = ENightmareState.Latched;
				_stateTimer = 0f;
			}
			break;
		case ENightmareState.ToInvisible:
			if (_stateTimer >= 1f)
			{
				_nightmareState = ENightmareState.Invisible;
				_stateTimer = 0f;
			}
			break;
		case ENightmareState.ToFloatingFromLatched:
			if (_stateTimer >= 1f)
			{
				_nightmareState = ENightmareState.Floating;
				_stateTimer = 0f;
			}
			break;
		case ENightmareState.ToFloatingFromInvisible:
			if (_stateTimer >= 1f)
			{
				_nightmareState = ENightmareState.Floating;
				_stateTimer = 0f;
			}
			break;
		case ENightmareState.Latched:
			if (_nightmareAbility == ENightmareAbility.FireBreath || _nightmareAbility == ENightmareAbility.Slash)
			{
				StartAbility((int)_nightmareAbility);
				break;
			}
			_nightmareState = ENightmareState.ToFloatingFromLatched;
			_stateTimer = 0f;
			break;
		case ENightmareState.Invisible:
			if (_nightmareAbility == ENightmareAbility.Apocalypse)
			{
				StartAbility((int)_nightmareAbility);
			}
			else if (_stateTimer >= 1f)
			{
				_nightmareState = ENightmareState.ToFloatingFromInvisible;
				_stateTimer = 0f;
			}
			break;
		case ENightmareState.Floating:
			if (_nightmareAbility == ENightmareAbility.Firebomb || _nightmareAbility == ENightmareAbility.Hellfire || _nightmareAbility == ENightmareAbility.Intro)
			{
				StartAbility((int)_nightmareAbility);
				break;
			}
			if (_nightmareAbility == ENightmareAbility.FireBreath || _nightmareAbility == ENightmareAbility.Slash)
			{
				_nightmareState = ENightmareState.ToLatched;
			}
			else
			{
				_nightmareState = ENightmareState.ToInvisible;
			}
			_stateTimer = 0f;
			break;
		}
	}

	public override void UpdateAbility(float delta)
	{
		switch (_nightmareAbility)
		{
		case ENightmareAbility.None:
			EndAbility();
			break;
		case ENightmareAbility.FireBreath:
			UpdateFireBreathAbility(delta);
			break;
		case ENightmareAbility.Slash:
			UpdateSlashAbility();
			break;
		case ENightmareAbility.Apocalypse:
			UpdateApocalypseAbility();
			break;
		case ENightmareAbility.Firebomb:
			UpdateFirebombAbility();
			break;
		case ENightmareAbility.Hellfire:
			UpdateHellfireAbility();
			break;
		case ENightmareAbility.Intro:
			UpdateIntro();
			break;
		}
	}

	private void EndAbility()
	{
		_isCarryingOutAbility = false;
		_currentAction = EAIAction.Idle;
		_nightmareAbility = ENightmareAbility.None;
		_nextActionTimer = 0.5f;
		_lastAbilityTimer = -1f;
	}

	private void UpdateIntro()
	{
		if (_abilityTimer <= 0f && _lastAbilityTimer <= 0f)
		{
			SetCharacterSequenceByName("Intro");
			_level.PlayCue(ESFX.BossNightmareFightBegin);
		}
		if (_abilityTimer >= 1.5f)
		{
			EndAbility();
			_nextActionTimer = 0f;
			Position = _spawnPoint;
		}
		else
		{
			Point start = new Point(_spawnPoint.X + 96, _spawnPoint.Y + -48);
			float amount = _abilityTimer / 1.5f;
			Position = start.SineInterpolate(_spawnPoint, amount);
		}
	}

	private void UpdateFireBreathAbility(float delta)
	{
		if (_abilityTimer <= 0f)
		{
			SetCharacterSequence(_breathSequence);
		}
		if (_abilityTimer < 1f)
		{
			return;
		}
		if (_abilityTimer < 2.25f)
		{
			if (_lastAbilityTimer < 1f)
			{
				_headAppendage.PlayCue(ESFX.BossNightmareBreath);
			}
			_fireBreathEmissionTimer -= delta;
			if (_fireBreathEmissionTimer <= 0f)
			{
				EmitFireBreath();
				_fireBreathEmissionTimer = 0.03f;
			}
		}
		else
		{
			EndAbility();
			_fireBreathEmissionTimer = 0f;
		}
	}

	private void EmitFireBreath()
	{
		if (_headAppendage == null)
		{
			return;
		}
		Point position = _headAppendage.Position;
		int num = ((!IsFacingLeft) ? 1 : (-1));
		position.X += -56 * num;
		position.Y -= 36;
		float num2 = 0.65f + (float)_level.NextRandomDouble() * 0.3f;
		Vector2 iV = new Vector2((float)num * (float)Math.Cos(num2) * 425f, (float)Math.Sin(num2) * 425f);
		NightmareFireBreathProjectile nightmareFireBreathProjectile = null;
		if (_fireBreathesEmitted < 64)
		{
			nightmareFireBreathProjectile = new NightmareFireBreathProjectile(_level, position, iV, _sprite, base.Damage);
			_fireBreathProjectiles[_fireBreathesEmitted] = nightmareFireBreathProjectile;
			_fireBreathesEmitted++;
		}
		else
		{
			NightmareFireBreathProjectile[] fireBreathProjectiles = _fireBreathProjectiles;
			foreach (NightmareFireBreathProjectile nightmareFireBreathProjectile2 in fireBreathProjectiles)
			{
				if (nightmareFireBreathProjectile2.IsFinished)
				{
					nightmareFireBreathProjectile2.Reset(position, iV);
					nightmareFireBreathProjectile = nightmareFireBreathProjectile2;
					break;
				}
			}
		}
		if (nightmareFireBreathProjectile != null)
		{
			_level.AddProjectile(nightmareFireBreathProjectile);
		}
	}

	private void UpdateSlashAbility()
	{
		if (_abilityTimer <= 0f)
		{
			_handAppendage.PlayCue(ESFX.BossNightmareSlash);
			SetCharacterSequence(_clawSequence);
		}
		if (_abilityTimer >= 2f)
		{
			EndAbility();
		}
	}

	private void UpdateFirebombAbility()
	{
		if (_abilityTimer <= 0f)
		{
			_handAppendage.PlayCue(ESFX.BossNightmareBombShoot);
			SetCharacterSequence(_firebombSequence);
		}
		if (_abilityTimer >= 1.5f && _lastAbilityTimer < 1.5f)
		{
			DropFirebombs();
		}
		else if (_abilityTimer >= 2.5f)
		{
			EndAbility();
		}
	}

	private void ShootFirebombs()
	{
		Point center = _bbox.Center;
		int num = ((!IsFacingLeft) ? 1 : (-1));
		center.X += 30 * num;
		center.Y = center.Y;
		Vector2 iV = new Vector2(500 * num, -500f);
		_preFireBombProjectile.Reset(center, iV);
		_level.AddProjectile(_preFireBombProjectile);
	}

	private void DropFirebombs()
	{
		for (int i = 0; i < 6; i++)
		{
			if (_fireBombProjectiles[i] == null)
			{
				_fireBombProjectiles[i] = new NightmareFireBombProjectile(_level, Point.Zero, Vector2.Zero, _sprite, base.Damage, i);
			}
			NightmareFireBombProjectile nightmareFireBombProjectile = _fireBombProjectiles[i];
			nightmareFireBombProjectile.Reset();
			_level.AddProjectile(nightmareFireBombProjectile);
		}
		PlayCue(ESFX.BossNightmareBombFall, new Point(200, 0));
	}

	private void UpdateHellfireAbility()
	{
		if (_abilityTimer <= 0f)
		{
			_handAppendage.PlayCue(ESFX.BossNightmareHellfireCast);
			SetCharacterSequence(_hellfireSequence);
		}
		if (_abilityTimer >= 3f)
		{
			EndAbility();
		}
	}

	private void ShootHellfire()
	{
		Protagonist mainHero = _level.MainHero;
		if (mainHero != null)
		{
			_hellfireDamageArea.Reset(new Point(mainHero.Position.X, 240));
			_level.AddProjectile(_hellfireDamageArea);
		}
	}

	private void UpdateApocalypseAbility()
	{
		float num = 1f;
		if (_abilityTimer <= 0f)
		{
			_headAppendage.PlayCue(ESFX.BossNightmareCackle);
			SetCharacterSequence(_apocalypseSequence);
		}
		if (_abilityTimer < 1.5f)
		{
			float num2 = _abilityTimer / 1.5f;
			num = 1f - num2;
			_damageCaused = 0;
		}
		else if (_abilityTimer < 3.5f)
		{
			if (_lastAbilityTimer < 1.5f)
			{
				SetHiddenState(isHidden: true);
				ShootApocalypseHellfires();
				SetCharacterSequence(_idleSequence);
			}
			num = 0f;
		}
		else if (_abilityTimer < 4.25f)
		{
			if (_lastAbilityTimer < 3.5f)
			{
				_canBeDamaged = true;
			}
			float num3 = (_abilityTimer - 3.5f) / 0.75f;
			num = num3;
		}
		else
		{
			SetHiddenState(isHidden: false);
			EndAbility();
		}
		base.DrawColor = Color.White * num;
		base.AuraColor = BaseAuraColor * num;
	}

	private void ShootApocalypseHellfires()
	{
		NightmareHellfireDamageArea[] apocHellfireDamageAreas = _apocHellfireDamageAreas;
		foreach (NightmareHellfireDamageArea nightmareHellfireDamageArea in apocHellfireDamageAreas)
		{
			nightmareHellfireDamageArea.Reset(Position);
			_level.RequestAddObject(nightmareHellfireDamageArea);
		}
	}

	private void SetHiddenState(bool isHidden)
	{
		_damageCaused = ((!isHidden) ? _baseTouchDamage : 0);
		_canBeDamaged = !isHidden;
	}

	internal override void TriggerCharacterAction(CharacterAction specification)
	{
		switch (specification.IntArgument)
		{
		case 0:
			ShootFirebombs();
			break;
		case 1:
			ShootHellfire();
			break;
		}
	}

	protected override void UpdateDeathScript(float delta)
	{
		if (_deathScriptTimer <= 0f)
		{
			PlayCue(ESFX.BossNightmareDeathCry, _headAppendage.Position);
			SetCharacterSequence(_deathSequence);
			if (_deathSmokeParticles == null)
			{
				_deathSmokeParticles = new NightmareDeathSmokeParticleSystem(_level.GCM.TxParticleSmoke, 128);
				_deathAshParticles = new NightmareDeathAshParticleSystem(_level.GCM.TxParticleEnergy, 64);
				_explosionBurstParticles = new NightmareExplosionBurstParticleSystem(_level.GCM.TxParticleSmoke, 1);
				_explosionSmokeParticles = new NightmareExplosionSmokeParticleSystem(_level.GCM.TxParticleSmoke, 1);
			}
		}
		float deathScriptTimer = _deathScriptTimer;
		_deathScriptTimer += delta;
		if (_deathScriptTimer < 3.6f)
		{
			_particleEmissionTimer += delta;
			if (_particleEmissionTimer >= 0.03f)
			{
				_particleEmissionTimer -= 0.03f;
				Rectangle outerBbox = _handAppendage.OuterBbox;
				for (int i = 0; i < 4; i++)
				{
					AddDeathParticles(outerBbox);
				}
				outerBbox = _headAppendage.OuterBbox;
				for (int j = 0; j < 4; j++)
				{
					AddDeathParticles(outerBbox);
				}
			}
			if (_deathScriptTimer < 2.6f)
			{
				float num = _deathScriptTimer / 2.6f;
				num *= num;
				num = MathEx.CosInterpolate(0f, 1f, num);
				Position = base.DeathPosition.CosInterpolate(base.DeathPosition.Add(32, 340), num);
				float num2 = 1f - num;
				base.DrawColor = DeathColor * num2;
				base.AuraColor = BaseAuraColor * num2;
			}
			else
			{
				Position = base.DeathPosition.Add(32, 340);
				base.DrawColor = Color.Transparent;
				base.AuraColor = Color.Transparent;
			}
		}
		else if (_deathScriptTimer < 4.35f)
		{
			if (deathScriptTimer < 3.6f)
			{
				_level.RequestScreenFadeOut(0.15f, 1f, 1f, 1f);
				_explosionBurstParticles.AddParticles(new Vector2(400f, 240f));
				PlayCue(ESFX.BossNightmareDeathExplosion, new Point(400, 240));
			}
		}
		else if (_deathScriptTimer < 6.35f)
		{
			if (deathScriptTimer < 4.35f)
			{
				_explosionSmokeParticles.AddParticles(new Vector2(400f, 240f));
				_level.RequestAddObject(new SandStreamerEvent(_level, new Point(400, 240), ESandStreamerType.NightmareDeath));
			}
		}
		else if (_deathScriptTimer < 8.35f)
		{
			float num3 = (_deathScriptTimer - 6.35f) / 2f;
			SetBackgroundColors((float)Math.Cos(num3 * ((float)Math.PI / 2f)));
		}
		else if (_deathScriptTimer < 9.35f)
		{
			if (deathScriptTimer < 8.35f)
			{
				_level.RequestScreenFadeOut(1f, 1f, 1f, 0f);
				_level.IsUIRequestingHide = true;
			}
		}
		else
		{
			if (_level.IsHardMode && _level.GameSave.IsLevelCap1)
			{
				_level.GameSave.SetValue("IsNightmareNightmare", value: true);
			}
			SaveBossDeath();
			RemoveInstance();
			_level.TogglePlayerIsInvulnerable(isInvulnerable: false);
			_level.ToggleExits(isEnabled: true);
			LevelChangeRequest levelChangeRequest = new LevelChangeRequest();
			levelChangeRequest.LevelID = 16;
			levelChangeRequest.RoomID = 27;
			levelChangeRequest.CutsceneToCall = CutsceneBase.ECutsceneType.Temple2_End;
			LevelChangeRequest levelRoomChangeRequest = levelChangeRequest;
			_level.AddScript(new ScriptAction(levelRoomChangeRequest));
		}
		if (_deathSmokeParticles != null)
		{
			_deathSmokeParticles.Update(delta);
			_deathAshParticles.Update(delta);
			_explosionBurstParticles.Update(delta);
			_explosionSmokeParticles.Update(delta);
		}
	}

	private void AddDeathParticles(Rectangle rangeBbox)
	{
		int num = _level.NextRandomInt(rangeBbox.Left, rangeBbox.Right);
		int num2 = _level.NextRandomInt(rangeBbox.Top, rangeBbox.Bottom);
		_deathSmokeParticles.AddParticles(new Vector2(num, num2));
		num = _level.NextRandomInt(rangeBbox.Left, rangeBbox.Right);
		num2 = _level.NextRandomInt(rangeBbox.Top, rangeBbox.Bottom);
		_deathAshParticles.AddParticles(new Vector2(num, num2));
	}

	private void SetBackgroundColors(float percentage)
	{
		List<Background> backgrounds = _level.Backgrounds;
		foreach (Background item in backgrounds)
		{
			item.DrawColor = item.BaseColor * percentage;
		}
		List<Background> foregrounds = _level.Foregrounds;
		foreach (Background item2 in foregrounds)
		{
			item2.DrawColor = item2.BaseColor * percentage;
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
		if (_isRunningDeathScript && _deathSmokeParticles != null)
		{
			_deathSmokeParticles.Draw(spriteBatch, _level.LevelRenderCenter, _level.CameraPosition, _level.CameraZoom);
			_deathAshParticles.Draw(spriteBatch, _level.LevelRenderCenter, _level.CameraPosition, _level.CameraZoom);
			_explosionSmokeParticles.Draw(spriteBatch, _level.LevelRenderCenter, _level.CameraPosition, _level.CameraZoom);
			_explosionBurstParticles.Draw(spriteBatch, _level.LevelRenderCenter, _level.CameraPosition, _level.CameraZoom);
		}
	}

	internal override void InitializeForBestiary()
	{
		SetCharacterSequence(_idleSequence);
		UpdateCharacterSequences(1f);
		Update(1f);
	}
}
