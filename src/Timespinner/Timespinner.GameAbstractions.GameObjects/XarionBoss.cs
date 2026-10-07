using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.Core.Specifications;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Bosses.Varndagroth;
using Timespinner.GameObjects.Bosses.Z_Xarion;
using Timespinner.GameObjects.Events.EnvironmentPrefabs;

namespace Timespinner.GameAbstractions.GameObjects;

internal sealed class XarionBoss : BossClass
{
	private enum EXarionBossState
	{
		Intro,
		DropCaterpillars,
		SummonVoid,
		MothLazer,
		SwipeAttack
	}

	private const int XarionTileID = 443;

	private const int CaterpillarCount = 8;

	private const int VoidCount = 8;

	private const int MaxMothProjectiles = 16;

	private const int PlayerHeightLazerThreshold = 160;

	private const int WormFallCeilingY = 16;

	private const int WormFallPelvisY = 192;

	private const int WormFallCeilingX1 = 80;

	private const int WormFallCeilingX2 = 144;

	private const int WormFallPelvisX1 = 224;

	private const int WormFallPelvisX2 = 272;

	private const float IndefiniteActionTime = 1000f;

	private const float FrenzyThreshold = 0.5f;

	private const float TimeBetweenAttacks = 1f;

	private const int CaterpillarAddCountEasy = 1;

	private const int CaterpillarAddCountHard = 3;

	private const int CaterpillarHitEmissionThresholdEasy = 5;

	private const int CaterpillarHitEmissionTresholdHard = 2;

	private const float TimeForCaterpillarDropScreenShake = 0.5f;

	private const float TimeForCaterpillarsToFall = 1f;

	private const float TimeForEntireCaterpillarDropAttack = 1.5f;

	private const int VoidSummonCount = 6;

	private const float TimeForVoidEyeBrighten = 1.25f;

	private const float TimeBetweenVoidSummons = 0.3f;

	private const float TimeForVoidSummoning = 1.8000001f;

	private const float TimeForVoidEyeDarken = 1.5f;

	private const float TimeForVoidLevelRestore = 1.25f;

	private const float TimeBeforeVoidEyeDarkening = 3.0500002f;

	private const float TimeBeforeVoidLevelRestore = 4.55f;

	private const float TimeForEntireVoidSummoningAttack = 5.8f;

	private const float VoidSummonEyeFlickerFrequency = 30f;

	private const int LazerMothEmitCount = 16;

	private const float MothProjectileInitialSpeed = 200f;

	private const float TimeBetweenEachLazerMoth = 0.1f;

	private const float TimeForLazerChargeUp = 1f;

	private const float TimeForLazerShooting = 1.6f;

	private const float TimeForLazerWaiting = 0.75f;

	private const float TimeForLazerFadeOut = 0.5f;

	private const float TimeBeforeLazerWaiting = 2.6f;

	private const float TimeBeforeLazerFadeOut = 3.35f;

	private const float TimeForTotalLazerAttack = 3.85f;

	private const float TimeForIntroPureBlack = 0.5f;

	private const float TimeForIntroFadeIn = 0.5f;

	private const float TimeForEntireIntro = 1f;

	private const float DeathScreenFadeTime = 1.5f;

	private const float DeathScreenflashTime = 2f;

	private const float TimeForDeathExplosions = 0.05f;

	private const float DeathFlashBufferTime = 1f;

	private const float TimeForEntireDeathSequence = 2.5f;

	private static readonly Color BaseAuraColor = new Color(0.5f, 0.25f, 0.75f, 0.35f);

	private static readonly Color IntroColor = new Color(96, 64, 80);

	private static readonly Color DeathScreenFadeColor = new Color(96, 64, 80);

	private static readonly Color EyeGlowStartColor = Color.Transparent;

	private static readonly Color EyeGlowEndColor = Color.White;

	private static readonly Color EyeGlowFlickerColor = Color.White * 0.7f;

	private static readonly Color VoidSummonRoomDrawColor = new Color(180, 128, 150);

	private static readonly Color LazerMothRoomDrawColor = new Color(200, 255, 160);

	private static readonly Color DefaultWebColor = new Color(148, 148, 148, 148);

	private readonly XarionWebParticleSystem _webParticles;

	private readonly SFXCueInstance _voidLoopCueInstance;

	private readonly XarionBossMoth _moth;

	private readonly Appendage _eyeGlowAppendage;

	private readonly XarionBossBlocker _blocker;

	private readonly XarionBossHand _hand;

	private readonly XarionBossCaterpillar[] _caterpillars = new XarionBossCaterpillar[8];

	private readonly XarionBossVoidDamageArea[] _voids = new XarionBossVoidDamageArea[8];

	private readonly XarionBossMothProjectile[] _mothProjectiles = new XarionBossMothProjectile[16];

	private EXarionBossState _xarionState;

	private bool _hasDoneIntro;

	private bool _doesNeedToAddCaterpillar;

	private bool _hasDoneDarknessVoidAttack;

	private bool _isLazerAimingHorizontally;

	private int _mothProjectilesEmitted;

	private int _hitsSinceLastCaterpillarEmission;

	private int _caterpillarIndex;

	private float _xarionStateTimer;

	private float _lastXarionStateTimer;

	private float _deathExplosionTimer;

	public XarionBoss(Point inPosition, Level inLevel, SpriteSheet inSprite, int inID, ObjectTileSpecification objectSpec)
		: base(inPosition, inLevel, inSprite, inID, objectSpec)
	{
		Position = new Point(inPosition.X - 8, inPosition.Y - 7);
		_currentAI = EAIStrategy.CustomScriptAI;
		_currentAction = EAIAction.Custom;
		_agility = 1f;
		Bbox = new Rectangle(_position.X, _position.Y, 16, 16);
		_isAffectedByGravity = false;
		_isFlying = true;
		_doAppendagesInheritDrawColor = true;
		_doAppendagesMatchImageFacing = true;
		_doesUseAppendageCollision = true;
		base.DoesDrawAura = true;
		base.DoesDrawAppendageAuras = true;
		base.AuraColor = BaseAuraColor;
		base.AuraOffset = new Vector2(1f, 0f);
		base.AuraFrequency = 1f;
		base.AuraSize = 0.025f;
		_auraCount = 5f;
		base.CannotBeGrabbed = true;
		_moth = new XarionBossMoth(_level, Position, new ObjectTileSpecification());
		if (_appendages.Count > 3)
		{
			Appendage appendage = _appendages[3];
			_eyeGlowAppendage = appendage.Appendages[0];
			_eyeGlowAppendage.DrawColor = Color.Transparent;
		}
		_webParticles = new XarionWebParticleSystem(_sprite, 3, 24, 2);
		_particleSystems.Add(_webParticles);
		Point inPosition2 = new Point(Position.X - 80, Position.Y - 96);
		_hand = new XarionBossHand(inPosition2, _level, _sprite, -1, new ObjectTileSpecification(443)
		{
			Argument = 1
		});
		_level.RequestAddObject(_hand);
		_blocker = new XarionBossBlocker(_level, new Point(Position.X, Position.Y + 24), -1, new ObjectTileSpecification());
		_level.RequestAddObject(_blocker);
		_voidLoopCueInstance = CreateCue(ESFX.BossXarionVoidLoop, Position, isLooped: true);
	}

	protected override void PickNextCustomScriptAIAction(float delta)
	{
		_currentAction = EAIAction.Custom;
		_xarionStateTimer = 0f;
		_lastXarionStateTimer = -1E-07f;
		_nextActionTimer = 1000f;
		if (!_hasDoneIntro)
		{
			_hasDoneIntro = true;
			_xarionState = EXarionBossState.Intro;
			return;
		}
		bool flag = base.HPPercentage < 0.5f;
		int end = ((!flag) ? 1 : 2);
		switch (_level.NextRandomInt(0, end))
		{
		case 0:
			_xarionState = EXarionBossState.DropCaterpillars;
			break;
		case 1:
			_xarionState = ((_level.GetNearestProtagonistPosition(Position).Y > 160) ? EXarionBossState.SwipeAttack : EXarionBossState.MothLazer);
			_isLazerAimingHorizontally = true;
			if (_hand.IsDying && _xarionState == EXarionBossState.SwipeAttack)
			{
				_xarionState = EXarionBossState.MothLazer;
				_isLazerAimingHorizontally = false;
			}
			break;
		default:
			_xarionState = EXarionBossState.SummonVoid;
			break;
		}
		if (!_hasDoneDarknessVoidAttack && flag)
		{
			_xarionState = EXarionBossState.SummonVoid;
			_hasDoneDarknessVoidAttack = true;
		}
		if (_xarionState != EXarionBossState.DropCaterpillars)
		{
			return;
		}
		bool flag2 = false;
		XarionBossCaterpillar[] caterpillars = _caterpillars;
		foreach (XarionBossCaterpillar xarionBossCaterpillar in caterpillars)
		{
			if (xarionBossCaterpillar == null || xarionBossCaterpillar.IsDead)
			{
				flag2 = true;
				break;
			}
		}
		if (!flag2)
		{
			_currentAction = EAIAction.Idle;
			_nextActionTimer = 0.5f;
		}
	}

	protected override void UpdateCustomScriptAIAction(float delta)
	{
		switch (_xarionState)
		{
		case EXarionBossState.Intro:
			UpdateIntro();
			break;
		case EXarionBossState.DropCaterpillars:
			UpdateDropCaterpillars();
			break;
		case EXarionBossState.SummonVoid:
			UpdateSummonVoid();
			break;
		case EXarionBossState.MothLazer:
			UpdateMothLazer();
			break;
		case EXarionBossState.SwipeAttack:
			UpdateSwipeAttack();
			break;
		}
		_lastXarionStateTimer = _xarionStateTimer;
		_xarionStateTimer += delta;
	}

	private void UpdateIntro()
	{
		if (_xarionStateTimer >= 1f)
		{
			FinishAttack();
		}
		else if (_xarionStateTimer > 0.5f)
		{
			float amount = (_xarionStateTimer - 0.5f) / 0.5f;
			SetLevelDrawColor(IntroColor.CosInterpolate(Color.White, amount));
		}
		else
		{
			SetLevelDrawColor(IntroColor);
		}
	}

	private void UpdateDropCaterpillars()
	{
		if (_xarionStateTimer <= 0f)
		{
			_level.RequestScreenShake(new Vector2(0f, 3f), 0.2f, 6f, isAffectedByTime: true);
			PlayCue(ESFX.BossXarionNest, new Point(80, 16));
		}
		if (_xarionStateTimer >= 0.5f && _lastXarionStateTimer < 0.5f)
		{
			int num = ((!_level.IsHardMode) ? 1 : 3);
			for (int i = 0; i < num; i++)
			{
				AddCaterpillar(isFromPelvis: false);
			}
		}
		if (_xarionStateTimer >= 1.5f)
		{
			FinishAttack();
		}
	}

	private void UpdateSummonVoid()
	{
		float amount = (float)(Math.Sin(_xarionStateTimer * 30f) / 2.0) + 0.5f;
		Color color = EyeGlowEndColor.Lerp(EyeGlowFlickerColor, amount);
		if (_xarionStateTimer < 1.25f)
		{
			if (_lastXarionStateTimer <= 0f && _xarionStateTimer > 0f)
			{
				PlayCue(ESFX.BossXarionVoidStart);
				if (_voidLoopCueInstance != null)
				{
					if (_voidLoopCueInstance.IsManuallyPaused)
					{
						_voidLoopCueInstance.Resume();
					}
					else
					{
						_voidLoopCueInstance.Play();
					}
					_voidLoopCueInstance.FadeIn(0.25f);
				}
			}
			float amount2 = _xarionStateTimer / 1.25f;
			_eyeGlowAppendage.DrawColor = EyeGlowStartColor.SineInterpolate(color, amount2);
			SetLevelDrawColor(Color.White.SineInterpolate(VoidSummonRoomDrawColor, amount2));
		}
		else if (_xarionStateTimer < 3.0500002f)
		{
			float num = _xarionStateTimer - 1.25f;
			float num2 = _lastXarionStateTimer - 1.25f;
			float num3 = 0f;
			int num4 = -1;
			for (int i = 0; i < 6; i++)
			{
				if (num >= num3 && num2 < num3)
				{
					num4 = i;
					break;
				}
				num3 += 0.3f;
			}
			if (num4 > -1)
			{
				AddDarknessVoid();
			}
			_eyeGlowAppendage.DrawColor = color;
		}
		else if (_xarionStateTimer < 4.55f)
		{
			float amount3 = (_xarionStateTimer - 3.0500002f) / 1.5f;
			_eyeGlowAppendage.DrawColor = color.SineInterpolate(EyeGlowStartColor, amount3);
		}
		else if (_xarionStateTimer < 5.8f)
		{
			if (_lastXarionStateTimer < 4.55f)
			{
				if (_voidLoopCueInstance != null)
				{
					_voidLoopCueInstance.Pause(1.25f);
				}
				PlayCue(ESFX.BossXarionVoidEnd);
			}
			float amount4 = (_xarionStateTimer - 4.55f) / 1.25f;
			SetLevelDrawColor(VoidSummonRoomDrawColor.SineInterpolate(Color.White, amount4));
			_eyeGlowAppendage.DrawColor = EyeGlowStartColor;
		}
		else
		{
			SetLevelDrawColor(Color.White);
			FinishAttack();
		}
	}

	private void UpdateMothLazer()
	{
		if (_xarionStateTimer < 3.85f)
		{
			if (_xarionStateTimer < 1f)
			{
				if (_lastXarionStateTimer <= 0f && _xarionStateTimer > 0f)
				{
					_moth.StartLazerCharge();
					_moth.PlayCue(ESFX.BossXarionSwarm);
				}
				float amount = _xarionStateTimer / 1f;
				SetLevelDrawColor(Color.White.SineInterpolate(LazerMothRoomDrawColor, amount));
			}
			else if (_xarionStateTimer < 2.6f)
			{
				float num = _xarionStateTimer - 1f;
				float num2 = _lastXarionStateTimer - 1f;
				float num3 = 0f;
				int num4 = -1;
				for (int i = 0; i < 16; i++)
				{
					if (num >= num3 && num2 < num3)
					{
						num4 = i;
						break;
					}
					num3 += 0.1f;
				}
				if (num4 > -1)
				{
					AddMothProjectile();
				}
			}
			else if (!(_xarionStateTimer < 3.35f))
			{
				if (_lastXarionStateTimer < 3.35f)
				{
					_moth.EndLazerCharge();
				}
				float amount2 = (_xarionStateTimer - 3.35f) / 0.5f;
				SetLevelDrawColor(LazerMothRoomDrawColor.SineInterpolate(Color.White, amount2));
			}
		}
		else
		{
			SetLevelDrawColor(Color.White);
			FinishAttack();
		}
	}

	private void UpdateSwipeAttack()
	{
		if (_xarionStateTimer < 2.65f)
		{
			if (_lastXarionStateTimer <= 0f && _xarionStateTimer > 0f)
			{
				_hand.StartAttack();
			}
		}
		else
		{
			FinishAttack();
		}
	}

	private void FinishAttack()
	{
		_nextActionTimer = 1f;
		_currentAction = EAIAction.Idle;
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			_moth.Update(delta);
			if (_doesNeedToAddCaterpillar)
			{
				_doesNeedToAddCaterpillar = false;
				AddCaterpillar(isFromPelvis: true);
			}
		}
		base.Update(delta);
	}

	private void AddCaterpillar(bool isFromPelvis)
	{
		int start = (isFromPelvis ? 224 : 80);
		int end = (isFromPelvis ? 272 : 144);
		int y = (isFromPelvis ? 192 : 16);
		Point point = new Point(_level.NextRandomInt(start, end), y);
		for (int i = 1; i <= 8; i++)
		{
			int num = (i + _caterpillarIndex) % 8;
			if (_caterpillars[num] == null)
			{
				XarionBossCaterpillar xarionBossCaterpillar = new XarionBossCaterpillar(point, _level, _sprite, new ObjectTileSpecification(), base.Damage);
				_caterpillars[num] = xarionBossCaterpillar;
				_level.RequestAddObject(xarionBossCaterpillar);
				break;
			}
			XarionBossCaterpillar xarionBossCaterpillar2 = _caterpillars[num];
			if (xarionBossCaterpillar2.IsDead)
			{
				if (xarionBossCaterpillar2.Reset(point))
				{
					_level.RequestAddObject(xarionBossCaterpillar2);
				}
				break;
			}
			_caterpillarIndex = num;
		}
		if (!isFromPelvis)
		{
			_webParticles.AddParticles(new Vector2(point.X, point.Y + 12));
		}
	}

	private void AddDarknessVoid()
	{
		XarionBossVoidDamageArea xarionBossVoidDamageArea = null;
		for (int i = 0; i < 8; i++)
		{
			if (_voids[i] == null)
			{
				xarionBossVoidDamageArea = new XarionBossVoidDamageArea(_level, Point.Zero, _sprite, base.Damage);
				_voids[i] = xarionBossVoidDamageArea;
				break;
			}
			if (_voids[i].IsFinished)
			{
				xarionBossVoidDamageArea = _voids[i];
				break;
			}
		}
		if (xarionBossVoidDamageArea != null)
		{
			Point nearestProtagonistPosition = _level.GetNearestProtagonistPosition(Position);
			xarionBossVoidDamageArea.Reset(nearestProtagonistPosition);
			_level.RequestAddObject(xarionBossVoidDamageArea);
		}
	}

	private void AddMothProjectile()
	{
		XarionBossMothProjectile xarionBossMothProjectile = null;
		Vector2 iV = new Vector2((float)((!IsFacingLeft) ? 1 : (-1)) * 200f, 0f);
		Point center = _moth.Bbox.Center;
		if (_mothProjectilesEmitted < 16)
		{
			SpriteSheet spCursedMoth = _level.GCM.SpCursedMoth;
			xarionBossMothProjectile = new XarionBossMothProjectile(_level, center, iV, base.DefaultTeam, spCursedMoth, base.Damage);
			_mothProjectiles[_mothProjectilesEmitted] = xarionBossMothProjectile;
			_mothProjectilesEmitted++;
		}
		else
		{
			XarionBossMothProjectile[] mothProjectiles = _mothProjectiles;
			foreach (XarionBossMothProjectile xarionBossMothProjectile2 in mothProjectiles)
			{
				if (xarionBossMothProjectile2.IsFinished)
				{
					xarionBossMothProjectile = xarionBossMothProjectile2;
					break;
				}
			}
		}
		if (xarionBossMothProjectile != null)
		{
			xarionBossMothProjectile.Reset(center, iV, _isLazerAimingHorizontally);
			_level.AddProjectile(xarionBossMothProjectile);
			_moth.EmitHalo();
		}
	}

	protected override void DrawBaseSprite(SpriteBatch spriteBatch, SpriteSheet sprite, Vector2 drawPos, Rectangle source, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float depth)
	{
		if (_isGlowing)
		{
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null);
		}
		_moth.Draw(spriteBatch);
		if (_isGlowing)
		{
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, _level.GCM.EfBrighten);
		}
	}

	private void SetLevelDrawColor(Color newColor)
	{
		_level.SetLevelDrawColor(newColor);
		foreach (Background foreground in _level.Foregrounds)
		{
			if (foreground.TextureType == EBackgroundTextureType.Cursed_Cave_Backdrops3)
			{
				Color drawColor = MathEx.Multiply(DefaultWebColor, newColor);
				foreground.DrawColor = drawColor;
			}
		}
	}

	public override bool ManageDamage(int damage, Vector2 velocity, Point where, Rectangle sourceRectangle, EDamageType type, EDamageElement element, bool doesKnockBack)
	{
		bool flag = base.ManageDamage(damage, velocity, where, sourceRectangle, type, element, doesKnockBack);
		if (flag)
		{
			_hitsSinceLastCaterpillarEmission++;
			int num = (_level.IsHardMode ? 2 : 5);
			if (_hitsSinceLastCaterpillarEmission >= num)
			{
				_hitsSinceLastCaterpillarEmission = 0;
				_doesNeedToAddCaterpillar = true;
			}
		}
		return flag;
	}

	protected override void UpdateDeathScript(float delta)
	{
		if (_deathScriptTimer <= 0f)
		{
			PlayCue(ESFX.BossXarionDeath);
			SetLevelDrawColor(Color.White);
			if (_hand != null)
			{
				_hand.Kill();
			}
			XarionBossCaterpillar[] caterpillars = _caterpillars;
			foreach (XarionBossCaterpillar xarionBossCaterpillar in caterpillars)
			{
				if (xarionBossCaterpillar != null && !xarionBossCaterpillar.IsDead)
				{
					xarionBossCaterpillar.Pacify();
				}
			}
		}
		float deathScriptTimer = _deathScriptTimer;
		_deathScriptTimer += delta;
		if (_deathScriptTimer < 2.5f)
		{
			if (_deathScriptTimer < 1.5f)
			{
				float amount = _deathScriptTimer / 1.5f;
				SetLevelDrawColor(Color.White.Lerp(DeathScreenFadeColor, amount));
			}
			if (deathScriptTimer + 1f < 2.5f && _deathScriptTimer + 1f >= 2.5f)
			{
				_level.RequestScreenFlash(2f, 1f, 1f);
			}
			_deathExplosionTimer += delta;
			if (_deathExplosionTimer > 0.05f)
			{
				_deathExplosionTimer -= 0.05f;
				int num = _random.Next(base.OuterBbox.Left, base.OuterBbox.Right);
				int y = _random.Next(base.OuterBbox.Top, base.OuterBbox.Bottom);
				Point point = new Point(num, y);
				BattleAnimation battleAnimation = new BattleAnimation(_level.GCM.SpBoomAnimation, point, _level);
				battleAnimation.AnimationSpeed = 0.05f;
				battleAnimation.AnimationLength = 13;
				battleAnimation.DrawColor = new Color(128, 64, 200, 200);
				BattleAnimation newAnimation = battleAnimation;
				AddBattleAnimation(newAnimation);
				PlayCue((num % 2 == 0) ? ESFX.EnemyWormFlowerSeedBoom : ESFX.FoleyExplosionLarge, point, isLooped: false, 0.5f);
			}
		}
		else
		{
			SetLevelDrawColor(Color.White);
			foreach (Background foreground in _level.Foregrounds)
			{
				if (foreground.TextureType == EBackgroundTextureType.Cursed_Cave_Backdrops3)
				{
					foreground.DrawColor = Color.Transparent;
				}
			}
			_level.RequestRemoveObject(_blocker);
			IEnumerable<GameEvent> eventAllEventsOfType = _level.GetEventAllEventsOfType(EEventTileType.EnvironmentPrefab);
			foreach (GameEvent item in eventAllEventsOfType)
			{
				if (item is EnvironmentPrefabBase environmentPrefabBase && (environmentPrefabBase.PrefabType == EEnvironmentPrefabType.L9_MothWorms || environmentPrefabBase.PrefabType == EEnvironmentPrefabType.L9_MothNest))
				{
					_level.RequestRemoveObject(item);
				}
			}
			XarionBossCaterpillar[] caterpillars2 = _caterpillars;
			foreach (XarionBossCaterpillar xarionBossCaterpillar2 in caterpillars2)
			{
				if (xarionBossCaterpillar2 != null && !xarionBossCaterpillar2.IsDead)
				{
					xarionBossCaterpillar2.SilentKill();
				}
			}
			if (_hand != null)
			{
				_hand.SilentKill();
			}
			base.DeathPosition = _moth.Bbox.Center;
			BattleAnimation battleAnimation2 = BattleAnimation.Create(EBattleAnimationType.Poof, base.DeathPosition, ETeamSide.Enemies, IsFacingLeft, _level, doesPlaySFX: false);
			battleAnimation2.ParticleSystem = new InsectWingParticleSystem(_level, _level.GCM.SpCursedMoth, base.DeathPosition, 1, 5, 4);
			_level.AddAnimation(battleAnimation2);
			DropLoot();
			EndBossDeathScript();
		}
		UpdateParticleSystems(delta);
	}
}
