using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.LunaisProjectiles.Spell;

namespace Timespinner.GameObjects.Heroes.Spells;

internal class GunOrbSpell : LunaisSpell
{
	private const int MaxBullets = 32;

	private const int StartOffsetX = -20;

	private const int StartOffsetY = -23;

	private const int GunArmFrameIndex = 28;

	private const int GunArmFacingLeftOffsetX = -2;

	private const int GunArmOffsetX = -33;

	private const int GunArmOffsetGroundedY = -16;

	private const int GunArmOffsetUngroundedY = -19;

	private const int LazerChargeOffsetX = 30;

	private const int BulletOffsetX = 16;

	private const int DefaultShootDistance = 200;

	private const int LazerOffsetX = 6;

	private const int LazerOffsetY = 18;

	private const int HaloOffsetX = 30;

	private const int LazerTipChargeOffsetY = 18;

	private const int LazerTipChargeOffsetX = 4;

	private const float BrightTipGlowBase = 10f;

	private const float TimeToNarrow = 0.54999995f;

	private const float GunSpeed = 300f;

	private const float TimeBetweenIndividualBullets = 0.1f;

	private const float TimeForGunArmToAppear = 0.65f;

	private const float TimeToChargeUp = 0.35f;

	private const float TimeBeforeAddingLazerTipCharge = 0.84999996f;

	private const float TimeBeforeShootingFinalLazer = 1f;

	private const float TimeToWait = 0.6f;

	private const float TimeToFadeOut = 0.1f;

	private const float TimeBeforeFadingOut = 1.25f;

	private const float TimeBeforeEndingSpell = 1.35f;

	private const float OrbChargeSpeed = 100f;

	private const float TimeToShootBullets = 0.65f;

	private static readonly Color BrightTipColor = new Color(1f, 1f, 1f, 0.5f);

	private static readonly Color OrbAuraColor = new Color(0.75f, 0.5f, 0.6f, 0.25f);

	private static readonly Vector4 ParticleColor = new Vector4(0.9f, 0.7f, 0.75f, 1f);

	private static readonly int[] OrbCostIntervals = new int[1] { 45 };

	private readonly Rectangle _gunArmFrameSource;

	private readonly GunSpellChargeLazerParticleSystem _lazerChargeParticles;

	private readonly HaloRingAnimation _haloRingAnimation;

	private readonly GunOrbSpellProjectile[] _bullets = new GunOrbSpellProjectile[32];

	private bool _isCreatingBullets;

	private bool _isShootingToTheLeft;

	private bool _isShootingUp;

	private bool _isParentShootingWhileGrounded;

	private bool _isDrawingHalo;

	private int _bulletIndex;

	private float _fireballCreationTimer;

	private float _individualBulletTimer;

	private float _narrowingTimer;

	private float _gunArmAppearTimer;

	private float _gunArmAppearPercentage;

	private Point _spellCreationPosition;

	private GunOrbSpellDamageArea _lazer;

	private LunaisObj _parentLunais;

	public override EInventoryOrbType SpellType => EInventoryOrbType.Gun;

	public GunOrbSpell(Level level)
		: base(level)
	{
		base.ChargeSpeed = 100f;
		base.ChargeIntervals.AddRange(OrbCostIntervals);
		_sprite = _level.GCM.SpOrbMeleeGun;
		_gunArmFrameSource = _sprite.GetFrameSource(28);
		base.IsDrawn = true;
		_lazerChargeParticles = new GunSpellChargeLazerParticleSystem(level.GCM.TxParticleEnergy, 8);
		_particleSystems.Add(_lazerChargeParticles);
		_haloRingAnimation = new HaloRingAnimation(level)
		{
			Width = 48,
			Height = 128
		};
	}

	public override Vector4 GetChargeParticleColor()
	{
		return ParticleColor;
	}

	public override Color GetAuraColor()
	{
		return OrbAuraColor;
	}

	internal override bool CreateSpellProjectiles(Level level, LunaisObj parentLunais, int spellVariation)
	{
		_parentLunais = parentLunais;
		_narrowingTimer = 0f;
		_gunArmAppearTimer = 0f;
		_gunArmAppearPercentage = 0f;
		_isCreatingBullets = true;
		_fireballCreationTimer = 0f;
		_isShootingToTheLeft = parentLunais.IsFacingLeft;
		_isParentShootingWhileGrounded = parentLunais.IsGrounded;
		_spellCreationPosition = parentLunais.Position.Add(new Point(-20 * (_isShootingToTheLeft ? 1 : (-1)), -23));
		PlayCue(ESFX.LunaisOrbGunSpell, _spellCreationPosition);
		level.PlayCue(ESFX.LunaisOrbGunSpell2D);
		if (!_isShootingToTheLeft)
		{
			_spellCreationPosition = _spellCreationPosition.Add(2, 0);
		}
		return true;
	}

	public override void Update(float delta)
	{
		if (_isCreatingBullets)
		{
			if (_parentLunais != null)
			{
				_spellCreationPosition = _parentLunais.Position.Add(new Point(-20 * (_isShootingToTheLeft ? 1 : (-1)), -23));
			}
			bool flag = false;
			if (_fireballCreationTimer <= 0f)
			{
				flag = true;
				_doesDrawSpriteAndAppendages = true;
			}
			IsFacingLeft = _isShootingToTheLeft;
			float fireballCreationTimer = _fireballCreationTimer;
			_fireballCreationTimer += delta;
			_narrowingTimer += delta;
			_gunArmAppearTimer += delta;
			_gunArmAppearPercentage = _gunArmAppearTimer / 0.65f;
			if (_gunArmAppearPercentage > 1f)
			{
				_gunArmAppearPercentage = 1f;
				if (_fireballCreationTimer < 1f)
				{
					Vector2 where = new Vector2(_spellCreationPosition.X + (_isShootingToTheLeft ? (-30) : 30), _spellCreationPosition.Y);
					_lazerChargeParticles.AddParticles(where);
				}
			}
			else
			{
				_isGlowing = true;
				base.GlowBase = 5f * _gunArmAppearPercentage;
				base.GlowColor = new Color(0.85f, 0.85f, 0.9f, 0.5f);
			}
			int num = -33 + (_isShootingToTheLeft ? (-2) : 0);
			int num2 = (_isParentShootingWhileGrounded ? (-16) : (-19));
			int num3 = (int)Math.Ceiling((1f - _gunArmAppearPercentage) * (float)_gunArmFrameSource.Width);
			int num4 = num3 + num;
			Position = new Point(_spellCreationPosition.X + (_isShootingToTheLeft ? num4 : (-num4)), _spellCreationPosition.Y + num2);
			_frameSource = new Rectangle(_gunArmFrameSource.X + num3, _gunArmFrameSource.Y, _gunArmFrameSource.Width - num3, _gunArmFrameSource.Height);
			if (_fireballCreationTimer >= 1.35f)
			{
				EndBullets();
			}
			else if (_fireballCreationTimer < 0.65f)
			{
				_individualBulletTimer -= delta;
				if (_individualBulletTimer <= 0f)
				{
					_individualBulletTimer = 0.1f;
					CreateBullet(num3 - _gunArmFrameSource.Width);
				}
			}
			else if (_fireballCreationTimer >= 1.25f)
			{
				float num5 = 1f - (_fireballCreationTimer - 1.25f) / 0.1f;
				base.DrawColor = Color.White * num5;
			}
			if (_fireballCreationTimer >= 0.84999996f && fireballCreationTimer < 0.84999996f)
			{
				Point inPosition = new Point(Position.X + (IsFacingLeft ? 4 : (-4)), Position.Y + 18);
				AddBattleAnimation(new BattleAnimation(_sprite, inPosition, _level)
				{
					AnimationStart = 33,
					AnimationLength = 4,
					AnimationSpeed = 0.05f
				});
			}
			if (_fireballCreationTimer >= 1f && fireballCreationTimer < 1f)
			{
				CreateFinalShot();
			}
			if (flag)
			{
				SnapBboxToPosition();
				RefreshDrawPos();
				base.DrawColor = Color.White;
			}
		}
		if (_isDrawingHalo)
		{
			_haloRingAnimation.Update(delta);
		}
		base.Update(delta);
	}

	private void CreateBullet(int xAppearOffset)
	{
		float num = (float)Math.Cos(_narrowingTimer / 0.54999995f * ((float)Math.PI / 2f));
		float num2 = 300f * (1f - num);
		float num3 = 300f * num;
		Vector2 iV = new Vector2(_isShootingToTheLeft ? (0f - num2) : num2, _isShootingUp ? (0f - num3) : num3);
		_isShootingUp = !_isShootingUp;
		List<Monster> visibleEnemies = _level.GetVisibleEnemies();
		int count = visibleEnemies.Count;
		Point newTarget = new Point(_spellCreationPosition.X + (_isShootingToTheLeft ? (-200) : 200), _spellCreationPosition.Y);
		if (count > 0)
		{
			Monster monster = visibleEnemies[_level.NextRandomInt(0, count - 1)];
			if (monster != null && monster.Bbox.X > _spellCreationPosition.X != _isShootingToTheLeft)
			{
				newTarget = monster.OuterBbox.Center;
			}
		}
		int num4 = xAppearOffset + 16;
		Point point = new Point(_spellCreationPosition.X + (_isShootingToTheLeft ? num4 : (-num4)), _spellCreationPosition.Y);
		GunOrbSpellProjectile gunOrbSpellProjectile;
		if (_bullets[_bulletIndex] == null)
		{
			gunOrbSpellProjectile = new GunOrbSpellProjectile(_level, point, ETeamSide.Heroes, _sprite, this);
			_bullets[_bulletIndex] = gunOrbSpellProjectile;
		}
		else
		{
			gunOrbSpellProjectile = _bullets[_bulletIndex];
		}
		gunOrbSpellProjectile.Reset(iV, point, newTarget, base.SpellDamage);
		_level.AddProjectile(gunOrbSpellProjectile);
		_bulletIndex++;
		if (_bulletIndex >= 32)
		{
			_bulletIndex = 0;
		}
	}

	private void CreateFinalShot()
	{
		Point spellCreationPosition = _spellCreationPosition;
		Point anchorOffset = new Point(6, 18);
		if (_lazer == null)
		{
			_lazer = new GunOrbSpellDamageArea(_level, spellCreationPosition, ETeamSide.Heroes, this, this, _sprite, base.SpellDamage, _isShootingToTheLeft)
			{
				AnchorOffset = anchorOffset
			};
		}
		else
		{
			_lazer.Reset(spellCreationPosition, base.SpellDamage, _isShootingToTheLeft);
			_lazer.AnchorOffset = anchorOffset;
		}
		_level.AddProjectile(_lazer);
		_level.RequestScreenFlash(0.33f, 0.33f, 1f);
		_isDrawingHalo = true;
		Point center = new Point(_spellCreationPosition.X + (_isShootingToTheLeft ? (-30) : 30), _spellCreationPosition.Y);
		_haloRingAnimation.Reset();
		_haloRingAnimation.Center = center;
		_isGlowing = false;
		base.DrawColor = Color.White;
	}

	internal static void CreatePowerfulSpell(Level level, ETeamSide side, Point startPoint, int damage, LunaisOrbAbility ability, bool isFacingLeft)
	{
		level.AddAnimation(BattleAnimation.Create(EBattleAnimationType.MediumRecoilDust, startPoint, side, isFacingLeft, level, doesPlaySFX: false, EElementAnimationColor.Red));
	}

	internal override void ChangeRoom()
	{
		EndBullets();
		base.ChangeRoom();
	}

	private void EndBullets()
	{
		_isCreatingBullets = false;
		_fireballCreationTimer = 0f;
		_individualBulletTimer = 0f;
		_doesDrawSpriteAndAppendages = false;
		_isDrawingHalo = false;
	}

	internal override void CancelSpell()
	{
		if (_isCreatingBullets)
		{
			EndBullets();
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (_isCreatingBullets)
		{
			base.Draw(spriteBatch);
			if (_gunArmAppearPercentage > 0f && _gunArmAppearPercentage < 1f)
			{
				SpriteEffects spriteEffects = _spriteEffects;
				if (!IsImageFacingLeft)
				{
					spriteEffects = SpriteEffects.FlipHorizontally | spriteEffects;
				}
				Vector2 value = (IsFacingLeft ? _drawPos : new Vector2(_drawPos.X - (float)_frameSource.Width, _drawPos.Y));
				Rectangle source = new Rectangle(_frameSource.X, _frameSource.Y, 1, _frameSource.Height);
				spriteBatch.End();
				_level.GCM.EfBrighten.Parameters["shinyAmount"].SetValue(10f);
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, _level.GCM.EfBrighten);
				DrawBaseSprite(spriteBatch, _sprite, Vector2.Subtract(_level.LevelRenderCenter, value), source, BrightTipColor, base.Rotation, DrawOrigin, _scale, spriteEffects, 0f);
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null);
			}
		}
		if (_isDrawingHalo)
		{
			_haloRingAnimation.Draw(spriteBatch);
		}
	}

	public override BattleAnimation CreateChargeAnimation(int chargeLevel, GameObject orb, Level level, Point location)
	{
		BattleAnimation battleAnimation = new BattleAnimation(level.GCM.SpOrbMeleeGun, location, level);
		battleAnimation.TeamSide = ETeamSide.Heroes;
		battleAnimation.AnimationSpeed = 0.06f;
		battleAnimation.DoesRepeat = true;
		battleAnimation.AnchorObject = orb;
		battleAnimation.AnimationStart = 29;
		battleAnimation.AnimationLength = 4;
		return battleAnimation;
	}
}
