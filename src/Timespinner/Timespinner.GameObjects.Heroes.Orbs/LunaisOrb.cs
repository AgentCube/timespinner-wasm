using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Base;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameObjects.Animations;
using Timespinner.GameObjects.BaseClasses;
using Timespinner.GameObjects.Heroes.Lunais.BaseClasses;

namespace Timespinner.GameObjects.Heroes.Orbs;

internal class LunaisOrb : LunaisOrbAbility
{
	private const int ChargeXShift = 3;

	internal const float IdleRadiusGrowthFactor = 10f;

	internal const float MaxIdleRadius = 6f;

	internal const float IdleTimeFactor = 4f;

	private const float Recoil1TimeToRecoil = 0.2f;

	private const float Recoil1Radius = 6f;

	private const float GlowFrequency = 20f;

	private const float ChargeTimeFactor = 6f;

	internal const float AnimIdleSpeed = 0.05f;

	private const int FancyOffsetX = 15;

	private const int FancyOffsetY = -16;

	private const float TimeToTransitionToFancy = 1f;

	private const float TimeToTransitionFromFancy = 0.5f;

	private const float FancyOffsetMultiplierX = 0.25f;

	private const float FancyAnimationAmplitude = 8f;

	private const float FancyAnimationFrequency = 1.5f;

	private const int DeathMinRadius = 12;

	private const int DeathMaxRadius = 32;

	private const float DeathTimeToGrowRadius = 1.5f;

	private const float DeathMaxFrequency = 12.75f;

	private static readonly Color ChargingTrailColor = new Color(0.95f, 0.95f, 1f, 0.8f);

	private readonly EInventoryOrbType _color;

	private readonly Vector4 _orbGlowColor;

	private readonly LunaisChargeParticleSystem _chargeSparkleParticles;

	private readonly LunaisChargeLeakParticleSystem _chargeLeakParticles;

	private readonly LunaisChargeLazerParticleSystem _chargeLazerParticles;

	protected readonly LunaisChargeLeakParticleSystem _attackSparkleParticles;

	private readonly LunaisObj _parentLunais;

	private readonly Action<SpriteBatch, bool> _drawOrbPassive;

	private bool _isCharging;

	private bool _isParentCharging;

	private bool _isDoingFancyAnimation;

	private bool _wasDoingFancyAnimation;

	private bool _isFancyFacingLeft;

	private bool _isOrbitingOnFrontPlane;

	protected float _oscillDelta;

	private float _killParticlesTimer;

	private float _attackTimer;

	private float _hiddenTimer;

	private float _fancyAnimationTimer;

	private float _fancyAnimationTransitionTimer;

	private float _fancyAnimationTransitionOutTimer;

	private float _deathTimer;

	protected Point _baseOrbitPosition;

	private Point _fancyPosition;

	protected float _oscillRadius;

	protected float _orbXShift;

	protected float _orbYShift;

	protected float _currentThrowTime;

	protected float _currentRecoilTime;

	protected float _currentRecoilDistance;

	protected float _meleeAnimationSpeed = 0.06f;

	private Color _idleTrailColor;

	private float _glowAmount = 1f;

	private float _glowDelta;

	public bool IsHidden { get; protected set; }

	public bool IsMainOrb { get; set; }

	public bool IsOrbFacingLeft { get; private set; }

	public bool IsThrowingLeft { get; set; }

	public bool IsDrawingOnFrontPlane
	{
		get
		{
			if (!IsOverridingDrawPlane)
			{
				return _isOrbitingOnFrontPlane;
			}
			return IsDrawingOnFrontPlaneOverride;
		}
	}

	public bool IsCharging => _isCharging;

	public bool DoesDrawBrushTrail
	{
		get
		{
			return _doesDrawBrushTrail;
		}
		set
		{
			_doesDrawBrushTrail = value;
		}
	}

	public bool IsAttacking => State == EOrbState.Melee;

	public bool IsAtAttackApex { get; protected set; }

	internal bool IsDoingFancyAnimation
	{
		get
		{
			return _isDoingFancyAnimation;
		}
		set
		{
			if (_isDoingFancyAnimation && !value)
			{
				State = EOrbState.Idle;
			}
			_isDoingFancyAnimation = value;
		}
	}

	internal bool IsOverridingDrawPlane { get; set; }

	internal bool IsDrawingOnFrontPlaneOverride { get; set; }

	internal bool IsThirdOrb { get; set; }

	internal bool DoesAnimationSpeedUpWhenCharging { get; set; }

	public EInventoryOrbType OrbColor => _color;

	public EOrbState State { get; protected set; }

	public int OrbDamage { get; private set; }

	internal float ShadowDamageMultiplier { get; set; }

	public float Charge { get; set; }

	public float OscillationDelta
	{
		get
		{
			return _oscillDelta;
		}
		set
		{
			_oscillDelta = value;
		}
	}

	public float OscillationRadius
	{
		get
		{
			return _oscillRadius;
		}
		set
		{
			_oscillRadius = value;
		}
	}

	public float MeleeAnimationSpeed => _meleeAnimationSpeed;

	public Point CurrentTarget { get; set; }

	internal Point OrbPassiveCenter { get; set; }

	internal Point FancyOffset => new Point(15 * ((!_isFancyFacingLeft) ? 1 : (-1)), -16);

	public Point ParticleEmissionPoint => Bbox.Center;

	internal Color DefaultDrawColor { get; set; }

	public Vector4 OrbGlowColor => _orbGlowColor;

	public LunaisObj ParentLunais => _parentLunais;

	internal Color IdleTrailColor
	{
		get
		{
			if (_idleTrailColor == Color.Transparent)
			{
				_idleTrailColor = GetOrbIdleTrailColorByType(OrbColor);
			}
			return _idleTrailColor;
		}
	}

	public LunaisOrb(Level inLevel, LunaisObj parentLunais, Point inPosition, SpriteSheet inSprite, EInventoryOrbType col, EOrbState inState, bool isFrontPlane)
		: base(inPosition, inLevel, (int)col)
	{
		_sprite = inSprite;
		_parentLunais = parentLunais;
		_drawOrbPassive = _parentLunais.DrawOrbPassive;
		_isAffectedByGravity = false;
		_isAffectedByFriction = false;
		_doesOverrideMaxSpeed = true;
		_isFlying = true;
		DoesAnimationSpeedUpWhenCharging = true;
		State = inState;
		_isOrbitingOnFrontPlane = isFrontPlane;
		IsMainOrb = isFrontPlane;
		base.SlotType = EOrbSlot.Melee;
		ShadowDamageMultiplier = 1f;
		DefaultDrawColor = Color.White;
		_orbGlowColor = GetOrbGlowColorByType(col);
		_chargeSparkleParticles = new LunaisChargeParticleSystem(_level.GCM.TxParticleEnergy, 2);
		_chargeLeakParticles = new LunaisChargeLeakParticleSystem(_level.GCM.TxParticleEnergy, 8);
		_chargeLazerParticles = new LunaisChargeLazerParticleSystem(_level.GCM.TxParticleEnergy, 1);
		_attackSparkleParticles = new LunaisChargeLeakParticleSystem(_level.GCM.TxParticleEnergy, 8);
		_particleSystems.Add(_chargeSparkleParticles);
		_particleSystems.Add(_chargeLeakParticles);
		_particleSystems.Add(_chargeLazerParticles);
		_particleSystems.Add(_attackSparkleParticles);
		_attackSparkleParticles.BaseColor = OrbGlowColor;
		_doesDrawParticleSystems = false;
		_glowBase = 1.5f;
		_color = col;
		_doesDrawTrail = true;
		_doesDrawBrushTrail = true;
		_brushTrailSize = 10;
		_trailFadeRate = 1f;
		_trailInterpolationAmount = 15;
		_trailShrinkRate = 0.00033f;
	}

	public void SetIsCharging(bool value)
	{
		if (!value && _isCharging)
		{
			_trailColor = IdleTrailColor;
			_isOrbitingOnFrontPlane = IsMainOrb;
		}
		_isCharging = value;
		if (IsCharging && (State == EOrbState.Idle || State == EOrbState.Fancy))
		{
			IsDoingFancyAnimation = false;
			if (DoesAnimationSpeedUpWhenCharging)
			{
				_animationSpeed = 0.01f;
			}
			State = EOrbState.Charge;
		}
		else if (!IsCharging && State == EOrbState.Charge)
		{
			if (DoesAnimationSpeedUpWhenCharging)
			{
				_animationSpeed = 0.1f;
			}
			State = EOrbState.Idle;
		}
	}

	internal virtual void StartCharge()
	{
		DoDisappearAnimation();
	}

	internal virtual void EndCharge()
	{
		DoDisappearAnimation();
	}

	private void DoDisappearAnimation()
	{
		if (_doesDrawBaseSprite)
		{
			BattleAnimation battleAnimation = BattleAnimation.Create(EBattleAnimationType.OrbDisappear, Bbox.Center, ETeamSide.Heroes, !IsOrbFacingLeft == IsMainOrb, _level, doesPlaySFX: false);
			battleAnimation.DrawColor = new Color(_orbGlowColor);
			_level.AddAnimation(battleAnimation);
		}
	}

	public virtual void SetOrbHiddenStatus(float hiddenTime, bool doesHide, bool doesShowAnimation)
	{
		_hiddenTimer = hiddenTime;
		IsHidden = doesHide;
		if (doesShowAnimation)
		{
			DoDisappearAnimation();
			if (IsMainOrb && hiddenTime >= 0f)
			{
				PlayCue(ESFX.LunaisOrbsAppear);
			}
		}
	}

	public virtual void Update(float delta, Point targetPoint)
	{
		CurrentTarget = targetPoint;
		IsOrbFacingLeft = _parentLunais.IsFacingLeft;
		_isParentCharging = _parentLunais.IsCharging;
		IsAtAttackApex = false;
		UpdateOrbitOffset(delta);
		if (IsDoingFancyAnimation && State == EOrbState.Idle)
		{
			State = EOrbState.Fancy;
			_fancyAnimationTransitionTimer = 0f;
			_fancyAnimationTimer = 0f;
			_wasDoingFancyAnimation = true;
			_isFancyFacingLeft = _parentLunais != null && _parentLunais.IsImageFacingLeft;
		}
		if (_attackTimer > 0f)
		{
			OrbAttack(delta);
			_attackTimer -= delta;
		}
		switch (State)
		{
		case EOrbState.Idle:
			UpdateIdle(delta);
			break;
		case EOrbState.Charge:
			UpdateOrbCharge(delta);
			break;
		case EOrbState.Melee:
			_wasDoingFancyAnimation = false;
			UpdateMeleeAttack(delta);
			break;
		case EOrbState.Dead:
			UpdateDeathSequence(delta);
			break;
		case EOrbState.Recoil:
			OrbRecoil(delta);
			break;
		case EOrbState.Fancy:
			UpdateFancyAnimation(delta);
			break;
		}
		if (base.IsAnimationQueueEmpty)
		{
			switch (State)
			{
			case EOrbState.Idle:
				_animationSpeed = ((_color == EInventoryOrbType.Pink) ? 0.15f : 0.1f);
				break;
			case EOrbState.Charge:
				_animationSpeed = 0.01f;
				break;
			}
		}
		UpdateOrbGlow(delta);
		Update(delta);
		UpdateParticles(delta);
		UpdateHiddenStatus(delta);
		OrbPassiveCenter = Position;
	}

	private void UpdateFancyAnimation(float delta)
	{
		float amount = 1f;
		if (_fancyAnimationTransitionTimer < 1f)
		{
			_fancyAnimationTransitionTimer += delta;
			amount = (float)Math.Sin(MathHelper.Clamp(_fancyAnimationTransitionTimer / 1f, 0f, 1f) * ((float)Math.PI / 2f));
		}
		_fancyAnimationTimer += delta;
		if (_fancyAnimationTimer >= 1.5f)
		{
			_fancyAnimationTimer -= 1.5f;
		}
		float num = _fancyAnimationTimer / 1.5f;
		float num2 = num * ((float)Math.PI * 2f) + (IsMainOrb ? ((float)Math.PI) : 0f) - (float)Math.PI * 33f / 50f;
		double num3 = Math.Cos(num2);
		double num4 = Math.Sin(num2);
		int num5 = (int)(num4 * 2.0 * 8.0 * 0.25 * (double)(_isFancyFacingLeft ? 1 : (-1)));
		int num6 = (int)(num3 * 8.0);
		_fancyPosition = new Point(CurrentTarget.X + num5, CurrentTarget.Y + num6).Add(FancyOffset);
		Position = _baseOrbitPosition.Lerp(_fancyPosition, amount);
	}

	private void UpdateIdle(float delta)
	{
		if (!_wasDoingFancyAnimation)
		{
			Position = _baseOrbitPosition;
			_fancyAnimationTransitionOutTimer = 0f;
			return;
		}
		float amount = 1f;
		_fancyAnimationTransitionOutTimer += delta;
		if (_fancyAnimationTransitionOutTimer >= 0.5f)
		{
			_fancyAnimationTransitionOutTimer = 0f;
			_wasDoingFancyAnimation = false;
		}
		else
		{
			amount = (float)Math.Sin((float)Math.PI / 2f * _fancyAnimationTransitionOutTimer / 0.5f);
		}
		Position = _fancyPosition.Lerp(_baseOrbitPosition, amount);
	}

	private void UpdateHiddenStatus(float delta)
	{
		if (_hiddenTimer > 0f)
		{
			_hiddenTimer -= delta;
			if (_hiddenTimer <= 0f)
			{
				SetOrbHiddenStatus(0f, doesHide: false, doesShowAnimation: true);
			}
		}
	}

	protected virtual void UpdateMeleeAttack(float delta)
	{
	}

	protected virtual void UpdateThrowAttack(float delta, float timeToAttack, float timeToReturn, Func<float, Vector2> throwAction)
	{
		float num = ((_currentThrowTime <= timeToAttack) ? (_currentThrowTime / timeToAttack) : (1f + (_currentThrowTime - timeToAttack) / timeToReturn));
		float num2 = (float)Math.Sin(num * ((float)Math.PI / 2f));
		float scaleFactor = 1f - num2;
		Vector2 value = throwAction(delta);
		Position = Vector2.Add(Vector2.Multiply(_baseOrbitPosition.ToVector2(), scaleFactor), Vector2.Multiply(value, num2)).ToPoint();
	}

	protected virtual void UpdateUnhookedThrowAttack(float delta, float timeToAttack, float timeToReturn, Point throwStartPoint, Func<float, Vector2> throwAction)
	{
		bool flag = _currentThrowTime <= timeToAttack;
		float num = (flag ? (_currentThrowTime / timeToAttack) : (1f + (_currentThrowTime - timeToAttack) / timeToReturn));
		float num2 = (float)Math.Sin(num * ((float)Math.PI / 2f));
		float scaleFactor = 1f - num2;
		Vector2 value = throwAction(delta);
		Point target = (flag ? throwStartPoint : _baseOrbitPosition);
		Position = Vector2.Add(Vector2.Multiply(target.ToVector2(), scaleFactor), Vector2.Multiply(value, num2)).ToPoint();
	}

	protected virtual void UpdateOrbitOffset(float delta)
	{
		float oscillDelta = _oscillDelta;
		_oscillDelta += delta * (IsCharging ? 6f : 4f);
		if (State != EOrbState.Dead && State != EOrbState.Charge && (((double)oscillDelta < Math.PI && (double)_oscillDelta >= Math.PI) || (oscillDelta < (float)Math.PI * 2f && _oscillDelta >= (float)Math.PI * 2f)))
		{
			_isOrbitingOnFrontPlane = !_isOrbitingOnFrontPlane;
		}
		if (_oscillDelta > (float)Math.PI * 2f)
		{
			_oscillDelta -= (float)Math.PI * 2f;
		}
		if (!IsThirdOrb)
		{
			if (IsMainOrb)
			{
				float num = (float)(Math.Cos(_oscillDelta) * (double)_oscillRadius * 3.0);
				_baseOrbitPosition = new Point((int)Math.Round(num + (float)CurrentTarget.X), (int)Math.Round((Math.Cos(_oscillDelta) * 0.75 + Math.Sin(_oscillDelta) * 0.25) * (double)_oscillRadius * 2.0 + (double)CurrentTarget.Y + (double)_orbYShift));
			}
			else
			{
				float num2 = (float)((0.0 - Math.Cos(_oscillDelta)) * (double)_oscillRadius * 3.0);
				_baseOrbitPosition = new Point((int)Math.Round(num2 + (float)CurrentTarget.X), (int)Math.Round((Math.Cos(_oscillDelta) * 0.75 + Math.Sin(_oscillDelta) * 0.25) * (double)_oscillRadius * 2.0 + (double)CurrentTarget.Y + (double)_orbYShift));
			}
		}
		else
		{
			float num3 = (float)(Math.Cos(_oscillDelta) * 0.75 + Math.Sin(_oscillDelta) * 0.25) * _oscillRadius * 3f;
			float num4 = (float)(Math.Cos(_oscillDelta + (float)Math.PI / 2f) * (double)_oscillRadius * 2.0);
			_baseOrbitPosition = new Point((int)Math.Round(num3 + (float)CurrentTarget.X), (int)Math.Round(num4 + (float)CurrentTarget.Y + _orbYShift));
		}
		if (_oscillRadius < 6f)
		{
			_oscillRadius += delta * 10f;
			if (_oscillRadius > 6f)
			{
				_oscillRadius = 6f;
			}
		}
	}

	private void UpdateParticles(float delta)
	{
		if (IsHidden)
		{
			return;
		}
		if (IsCharging)
		{
			if (_killParticlesTimer <= 0f)
			{
				_chargeLazerParticles.StartRadiusOffset = Charge / 5f;
				_chargeSparkleParticles.AddParticles(new Vector2(_position.X, _position.Y));
				_chargeLazerParticles.AddParticles(new Vector2(_position.X, _position.Y));
			}
			else
			{
				_killParticlesTimer -= delta;
				if (_killParticlesTimer < 0f)
				{
					_killParticlesTimer = 0f;
				}
			}
		}
		if ((_color == EInventoryOrbType.Blue && Charge >= 90f) || (_color == EInventoryOrbType.Blade && Charge >= 190f) || (_color == EInventoryOrbType.Pink && Charge >= 290f))
		{
			_chargeSparkleParticles.AddParticles(new Vector2(_position.X, _position.Y));
		}
		if (IsAttacking)
		{
			_attackSparkleParticles.AddParticles(new Vector2(_position.X, _position.Y));
		}
	}

	private void UpdateOrbCharge(float delta)
	{
		if (_oscillRadius > 0f)
		{
			_oscillRadius -= delta * 10f * 2f;
			if (_oscillRadius < 0f)
			{
				_oscillRadius = 0f;
			}
		}
		if (_orbXShift < 3f)
		{
			_orbXShift += delta * 10f * 4f;
			if (_orbXShift > 3f)
			{
				_orbXShift = 3f;
			}
		}
		Position = _baseOrbitPosition;
	}

	private void OrbAttack(float delta)
	{
		if (_oscillRadius > 0f)
		{
			_oscillRadius -= delta * 10f * 10f;
			if (_oscillRadius < 0f)
			{
				_oscillRadius = 0f;
			}
		}
		if (_orbXShift < 3f)
		{
			_orbXShift += delta * 10f * 4f;
			if (_orbXShift > 3f)
			{
				_orbXShift = 3f;
			}
		}
	}

	private void OrbRecoil(float delta)
	{
		_currentRecoilTime += delta;
		if (_currentRecoilTime > 0.2f || _isParentCharging)
		{
			_currentRecoilTime = 0f;
			State = EOrbState.Idle;
			return;
		}
		float num = _currentRecoilTime / 0.2f;
		_currentRecoilDistance = 6f * (float)Math.Sin((double)num * Math.PI);
		_orbXShift = (float)(IsThrowingLeft ? 1 : (-1)) * _currentRecoilDistance;
		Position = new Point((int)Math.Round((float)CurrentTarget.X + _orbXShift), (int)Math.Round((float)CurrentTarget.Y + _orbYShift));
	}

	private void UpdateOrbGlow(float delta)
	{
		float num = Charge / 100f;
		_glowDelta += delta * 20f;
		if (_glowDelta > 360f)
		{
			_glowDelta -= 360f;
		}
		if (Charge > 0f)
		{
			_doesDrawTrail = Charge >= 0f;
			_glowAmount = ((Charge % 100f == 0f) ? (1f - num * ((float)Math.Cos(_glowDelta) + 1f) / 2f) : 1f);
		}
		else
		{
			_glowAmount = 1f;
		}
	}

	private void UpdateDeathSequence(float delta)
	{
		_deathTimer += delta;
		float num = 1f;
		if (_deathTimer < 1.5f)
		{
			num = 1f - (float)Math.Cos((float)Math.PI / 2f * _deathTimer / 1.5f);
		}
		_oscillDelta += delta * (1f + num * 12.75f);
		float num2 = 12f + num * 32f;
		if (IsMainOrb)
		{
			float num3 = (float)(Math.Cos(_oscillDelta) * (double)num2);
			Position = new Point((int)Math.Round(num3 + (float)CurrentTarget.X), (int)Math.Round((Math.Cos(_oscillDelta) * 0.75 + Math.Sin(_oscillDelta) * 0.25) * (double)num2 + (double)CurrentTarget.Y + (double)_orbYShift));
		}
		else
		{
			float num4 = (float)((0.0 - Math.Cos(_oscillDelta)) * (double)num2);
			Position = new Point((int)Math.Round(num4 + (float)CurrentTarget.X), (int)Math.Round((Math.Cos(_oscillDelta) * 0.75 + Math.Sin(_oscillDelta) * 0.25) * (double)num2 + (double)CurrentTarget.Y + (double)_orbYShift));
		}
	}

	public override void SnapBboxToPosition()
	{
		_bbox.Location = new Point(_position.X - _bbox.Width / 2, _position.Y - _bbox.Height / 2);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		_chargeLazerParticles.Draw(spriteBatch, _level.LevelRenderCenter, _level.CameraPosition, _level.CameraZoom);
		_chargeSparkleParticles.Draw(spriteBatch, _level.LevelRenderCenter, _level.CameraPosition, _level.CameraZoom);
		_chargeLeakParticles.Draw(spriteBatch, _level.LevelRenderCenter, _level.CameraPosition, _level.CameraZoom);
		_attackSparkleParticles.Draw(spriteBatch, _level.LevelRenderCenter, _level.CameraPosition, _level.CameraZoom);
		if (IsHidden)
		{
			return;
		}
		bool doesDrawTrail = _doesDrawTrail;
		if (_doesDrawTrail)
		{
			Color trailColor = _trailColor;
			if (State == EOrbState.Charge)
			{
				_trailColor = ChargingTrailColor;
			}
			DrawTrail(spriteBatch);
			if (State == EOrbState.Charge)
			{
				_trailColor = trailColor;
			}
			_doesDrawTrail = false;
		}
		_drawOrbPassive(spriteBatch, IsMainOrb);
		if (Charge > 0f)
		{
			spriteBatch.End();
			_level.GCM.EfBrighten.Parameters["shinyAmount"].SetValue(_glowBase);
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, _level.GCM.EfBrighten);
			base.DrawColor = new Color(1f, 1f, 1f, _glowAmount);
		}
		else
		{
			base.DrawColor = DefaultDrawColor;
		}
		base.Draw(spriteBatch);
		if (Charge > 0f)
		{
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null);
		}
		_doesDrawTrail = doesDrawTrail;
	}

	public void KillParticles(float time)
	{
		foreach (ParticleSystem particleSystem in _particleSystems)
		{
			particleSystem.KillOffParticles(time);
		}
		_killParticlesTimer = 0.15f;
	}

	public void ResetPosition(Point moveTo)
	{
		CurrentTarget = moveTo;
		ClearTrailHistory();
		Update(0f);
	}

	public void ResetOscillation(float amount)
	{
		_oscillDelta = amount;
	}

	public virtual void StartMeleeAttack(int whichAttack)
	{
		_currentThrowTime = 0f;
	}

	public void StartAttack(float inTime)
	{
		_attackTimer = inTime;
	}

	public override void Kill()
	{
		State = EOrbState.Dead;
		_isCharging = false;
		_deathTimer = 0f;
		_oscillDelta = 0f;
		_doesDrawBaseSprite = true;
		_doesDrawTrail = true;
		_doesDrawBrushTrail = true;
		_trailLength = 75;
		_trailColor = Color.White;
		base.DrawColor = Color.White;
	}

	public virtual void DisposeOrb()
	{
	}

	public virtual void ChangeRoom()
	{
		_fancyPosition = _baseOrbitPosition;
		_wasDoingFancyAnimation = false;
		KillParticles(0f);
		ClearTrailHistory();
	}

	public void SetChargeParticlesColor(Vector4 color)
	{
		_chargeSparkleParticles.BaseColor = color;
		_chargeLeakParticles.BaseColor = color;
		_chargeLazerParticles.BaseColor = color;
	}

	internal void SetTrailColor(Color color)
	{
		_trailColor = color;
	}

	internal virtual void UpdateDamage(int damage)
	{
		OrbDamage = (int)Math.Ceiling((float)damage * ShadowDamageMultiplier);
	}

	internal override void OnEnemyContact(Alive enemy, LunaisBaseOrbDamageArea damageArea, Rectangle contactBbox)
	{
		_parentLunais.OnOrbEnemyContact(enemy, damageArea, contactBbox);
		base.OnEnemyContact(enemy, damageArea, contactBbox);
	}

	internal override void OnSuccessfulEnemyHit(Alive enemy)
	{
		_parentLunais.OnSuccessfulEnemyHit(enemy, EOrbSlot.Melee);
		base.OnSuccessfulEnemyHit(enemy);
	}

	internal static Vector4 GetOrbGlowColorByType(EInventoryOrbType orbType)
	{
		Vector4 result = Vector4.One;
		switch (orbType)
		{
		case EInventoryOrbType.Blue:
			result = new Vector4(0.66f, 0.66f, 1f, 1f);
			break;
		case EInventoryOrbType.Blade:
			result = new Vector4(0.75f, 1f, 0.65f, 1f);
			break;
		case EInventoryOrbType.Flame:
			result = new Vector4(1f, 0.6f, 0.3f, 1f);
			break;
		case EInventoryOrbType.Pink:
			result = new Vector4(1f, 0.5f, 0.75f, 1f);
			break;
		case EInventoryOrbType.Iron:
			result = new Vector4(0.9f, 0.9f, 0.9f, 1f);
			break;
		case EInventoryOrbType.Ice:
			result = new Vector4(0.66f, 0.8f, 0.9f, 1f);
			break;
		case EInventoryOrbType.Wind:
			result = new Vector4(0.33f, 1f, 0.66f, 1f);
			break;
		case EInventoryOrbType.Gun:
			result = new Vector4(0.66f, 0.66f, 1f, 1f);
			break;
		case EInventoryOrbType.Umbra:
			result = new Vector4(0.7f, 0.5f, 0.8f, 1f);
			break;
		case EInventoryOrbType.Empire:
			result = new Vector4(0.8f, 0.66f, 1f, 1f);
			break;
		case EInventoryOrbType.Eye:
			result = new Vector4(1f, 0.5f, 0.75f, 1f);
			break;
		case EInventoryOrbType.Blood:
			result = new Vector4(0.75f, 0.15f, 0.15f, 0.1f);
			break;
		case EInventoryOrbType.Book:
			result = new Vector4(0.9f, 0.75f, 1f, 1f);
			break;
		case EInventoryOrbType.Moon:
			result = new Vector4(0.75f, 0.65f, 0.55f, 1f);
			break;
		case EInventoryOrbType.Nether:
			result = new Vector4(0.6f, 0.8f, 0.7f, 1f);
			break;
		case EInventoryOrbType.Barrier:
			result = new Vector4(0.85f, 0.8f, 0.65f, 1f);
			break;
		case EInventoryOrbType.Monske:
			result = new Vector4(0.75f, 1f, 0.65f, 1f);
			break;
		}
		return result;
	}

	internal static Color GetOrbIdleTrailColorByType(EInventoryOrbType orbType)
	{
		Color result = Color.White;
		switch (orbType)
		{
		case EInventoryOrbType.Blue:
			result = new Color(0.15f, 0.15f, 0.75f, 0.1f);
			break;
		case EInventoryOrbType.Blade:
			result = new Color(0.35f, 0.75f, 0.3f, 0.1f);
			break;
		case EInventoryOrbType.Flame:
			result = new Color(0.75f, 0.5f, 0.25f, 0.1f);
			break;
		case EInventoryOrbType.Pink:
			result = new Color(0.75f, 0.25f, 0.5f, 0.1f);
			break;
		case EInventoryOrbType.Iron:
			result = new Color(0.75f, 0.75f, 0.75f, 0.1f);
			break;
		case EInventoryOrbType.Ice:
			result = new Color(0.15f, 0.75f, 0.75f, 0.1f);
			break;
		case EInventoryOrbType.Wind:
			result = new Color(0.15f, 0.75f, 0.45f, 0.1f);
			break;
		case EInventoryOrbType.Gun:
			result = new Color(0.5f, 0.15f, 0.5f, 0.1f);
			break;
		case EInventoryOrbType.Umbra:
			result = new Color(0.4f, 0.15f, 0.6f, 0.1f);
			break;
		case EInventoryOrbType.Empire:
			result = new Color(0.65f, 0.4f, 0.8f, 0.1f);
			break;
		case EInventoryOrbType.Eye:
			result = new Color(0.75f, 0.25f, 0.5f, 0.1f);
			break;
		case EInventoryOrbType.Blood:
			result = new Color(0.5f, 0.1f, 0.15f, 0.1f);
			break;
		case EInventoryOrbType.Book:
			result = new Color(0.75f, 0.25f, 0.5f, 0.1f);
			break;
		case EInventoryOrbType.Moon:
			result = new Color(0.75f, 0.65f, 0.5f, 0.025f);
			break;
		case EInventoryOrbType.Nether:
			result = new Color(0.25f, 0.15f, 0.4f, 0.1f);
			break;
		case EInventoryOrbType.Barrier:
			result = new Color(0.75f, 0.65f, 0.5f, 0.1f);
			break;
		case EInventoryOrbType.Monske:
			result = new Color(0.75f, 0.25f, 0.5f, 0.1f);
			break;
		}
		return result;
	}
}
