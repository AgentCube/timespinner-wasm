using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.GameObjects;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.BaseClasses;
using Timespinner.GameObjects.Heroes.Orbs;

namespace Timespinner.GameObjects.Heroes.LunaisProjectiles;

internal sealed class BookOrbSpellProjectile : LunaisBaseOrbDamageArea
{
	private const int BboxSize = 48;

	private const int BboxOffsetSize = 8;

	private const int OriginSize = 32;

	private const int AppendageAnchorOffsetY = 24;

	private const int Anim_LavaStart = 33;

	private const int Anim_RockStart = 34;

	private const float MaxLife = 1.5f;

	private const float RateOfRotationLava = 2f;

	private const float RateOfRotationRock = 4f;

	private const float TimeToCharge = 0.5f;

	private static readonly Color ShockwaveColor = new Color(240, 64, 0, 64);

	private static readonly Color BaseBackGlowColor = new Color(255, 128, 0);

	private readonly Appendage _lavaAppendage;

	private readonly Appendage _rockAppendage;

	private readonly GlowTexture _backGlowTexture;

	private readonly ShockwaveAnimation _shockwaveAnimation;

	private bool _isCharging;

	private float _infernoTimer;

	private float _lavaRotation;

	private float _rockRotation;

	private Point _spawnPoint;

	internal bool IsFinished { get; private set; }

	public BookOrbSpellProjectile(Level inLevel, Point inPosition, Vector2 iV, ETeamSide inSide, int spellDamage, LunaisOrbAbility parentOrb)
		: base(inLevel, inPosition, inSide, -1, null, parentOrb)
	{
		_initialVector = iV;
		_spawnPoint = inPosition;
		_maxMoveSpeed = 1000f;
		_isCharging = true;
		_sprite = _level.GCM.SpOrbMeleeBook;
		_bbox = new Rectangle(inPosition.X - 24, inPosition.Y - 24, 48, 48);
		_bboxOffset = new Point(8, 48);
		DrawOrigin = new Vector2(32f, 32f);
		_power = spellDamage;
		_force = 0;
		_life = 1.5f;
		base.DamageTimeoutTime = 1f;
		_damageElement = EDamageElement.Fire;
		_doAppendagesInheritDrawColor = false;
		_doAppendagesMatchImageFacing = false;
		_doesUseAppendageCollision = false;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_isFlying = true;
		_doesRotateBasedOnVelocity = false;
		base.DoesDieOnImpact = false;
		base.DoesCollideWithTiles = false;
		base.DoesKnockBack = true;
		ChangeAnimation(-1);
		_doesDrawBaseSprite = false;
		_shockwaveAnimation = new ShockwaveAnimation(_level.GCM.SpOrbMeleeBarrier, Point.Zero, _level, ShockwaveColor);
		_lavaAppendage = new Appendage(this, new Point(48, 48), new Point(8, 8), _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorOffset = new Point(0, 24),
			DrawPriority = -1,
			DrawOrigin = new Vector2(32f, 32f)
		};
		_lavaAppendage.ChangeAnimation(33);
		_appendages.Add(_lavaAppendage);
		_rockAppendage = new Appendage(this, new Point(48, 48), new Point(8, 8), _level, _sprite)
		{
			FollowType = EAppendageFollowType.AnchorLocked,
			AnchorOffset = new Point(0, 24),
			DrawPriority = 1,
			DrawOrigin = new Vector2(32f, 32f)
		};
		_rockAppendage.ChangeAnimation(34);
		_appendages.Add(_rockAppendage);
		_backGlowTexture = new GlowTexture(_level)
		{
			BaseColor = BaseBackGlowColor,
			GlowCircleRadius = 76,
			GlowCircleConsecutiveSizeReduction = 0.975f
		};
		SetDoesDrawAppendageTrails(value: true, isHost: true, 4, 2f);
	}

	public override void Update(float delta)
	{
		if (!base.IsFrozen)
		{
			float num = 1f;
			if (_isFading)
			{
				num = 1f - _fadeTimer / _timeToFade;
			}
			if (_isCharging)
			{
				float infernoTimer = _infernoTimer;
				_infernoTimer += delta;
				if (infernoTimer <= 0f)
				{
					PlayCue(ESFX.LunaisOrbBookSpell);
					_level.PlayCue(ESFX.LunaisOrbBookSpell2D);
				}
				if (_infernoTimer <= 0.5f)
				{
					_velocity = Vector2.Zero;
					Position = _spawnPoint;
					float num2 = (float)Math.Sin(_infernoTimer / 0.5f * ((float)Math.PI / 2f));
					Color drawColor = Color.White * num2 * num;
					_lavaAppendage.DrawColor = drawColor;
					_rockAppendage.DrawColor = drawColor;
					_backGlowTexture.BaseColor = BaseBackGlowColor;
				}
				else
				{
					_lavaAppendage.DrawColor = Color.White;
					_rockAppendage.DrawColor = Color.White;
					_backGlowTexture.BaseColor = BaseBackGlowColor;
					_shockwaveAnimation.Position = Bbox.Center.Add(4, 4);
					_isCharging = false;
					_infernoTimer = 0f;
					_velocity = _initialVector;
					_isAffectedByGravity = true;
					_gravityAcceleration = 250f;
				}
			}
			else if (num < 1f)
			{
				_lavaAppendage.DrawColor = Color.White * num;
				_rockAppendage.DrawColor = Color.White * num;
				_backGlowTexture.BaseColor = BaseBackGlowColor * num;
			}
			if (!_isCharging)
			{
				_infernoTimer += delta;
			}
			if (!_isCharging && !_shockwaveAnimation.IsDead)
			{
				_shockwaveAnimation.Update(delta);
			}
			_lavaRotation += 2f * delta;
			_rockRotation -= 4f * delta;
			if (_lavaRotation > (float)Math.PI * 2f)
			{
				_lavaRotation -= (float)Math.PI * 2f;
			}
			if (_rockRotation < 0f)
			{
				_rockRotation += (float)Math.PI * 2f;
			}
			_lavaAppendage.Rotation = _lavaRotation;
			_rockAppendage.Rotation = _rockRotation;
		}
		base.Update(delta);
		_backGlowTexture.Center = Bbox.Center;
		_backGlowTexture.Update(delta);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		_backGlowTexture.Draw(spriteBatch);
		if (!_isCharging && !_shockwaveAnimation.IsDead)
		{
			_shockwaveAnimation.Draw(spriteBatch);
		}
		base.Draw(spriteBatch);
		if (_isCharging)
		{
			_backGlowTexture.Draw(spriteBatch);
		}
	}

	public override void SilentKill()
	{
		IsFinished = true;
		base.SilentKill();
	}

	public bool Reset(Point startPoint, Vector2 iV, int spellDamage)
	{
		bool isFinished = IsFinished;
		if (isFinished)
		{
			base.ID = -1;
		}
		_power = spellDamage;
		IsFinished = false;
		_isFading = false;
		_fadeTimer = 0f;
		_life = 1.5f;
		_isCharging = true;
		_infernoTimer = 0f;
		_spawnPoint = startPoint;
		Position = startPoint;
		_initialVector = iV;
		base.Velocity = iV;
		SnapBboxToPosition();
		_shockwaveAnimation.Reset(Position, isFacingLeft: true);
		Update(0f);
		return isFinished;
	}
}
