using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions.Inventory;
using Timespinner.GameAbstractions.Saving;
using Timespinner.GameObjects.Heroes;

namespace Timespinner.GameAbstractions.HUD;

internal class HudLunais : HudElement
{
	private const int HealthBarFrameIndex = 3;

	private const int AuraBarFrameIndex = 6;

	private const int PlasmaAuraBarFrameIndex = 39;

	private const int DamageBarFrameIndex = 9;

	private const int UnderBarFrameIndex = 12;

	private const int BarSectionWidth = 4;

	private const int HealthBarWidth = 64;

	private const int AuraBarWidth = 52;

	private const int SpellOrbFrameIndex = 21;

	private const float TimeBetweenAuraVileteFlashes = 0.4f;

	private const float TimeForAuraVileteFlash = 0.7f;

	private const float TimeForEntireAuraVileteFlash = 1.1f;

	private const float YellowHealthThreshold = 0.4f;

	private const float RedHealthThreshold = 0.2f;

	private const float SandFlashGlowTime = 0.5f;

	private const float SandFlashGlowSeverity = 0.5f;

	private const float BaseSandGlow = 0.9f;

	private const float BaseSandDimness = 0f;

	private const float SandGlowFrequencyRecover = 1f;

	private const float SandGlowFrequencyFull = 0.5f;

	private const float TimeToAuraOOMFlash = 1f;

	private const float TimeChangeIncrement = 25f;

	private const float TimeToDropHourglassSand = 0.1f;

	private const float TimeToLoseRedHealth = 0.5f;

	private const float MoneyPercentageToAddPerTick = 0.1f;

	private const float TimeToTransferOneChunkOfMoney = 0.066f;

	private const float TimeToDisplayMoneyAfterTransactionFinished = 1f;

	private const float TimeForMoneyToFade = 0.15f;

	private const float AuraGlowFrequency = 10f;

	private static readonly Point HealthBarOffset = new Point(46, 7);

	private static readonly Point HealthHUDOffset = new Point(30, 6);

	private static readonly Point AuraBarOffset = new Point(57, 15);

	private static readonly Point HourglassDrawOffset = new Point(7, 3);

	private static readonly Point HourglassSandDrawOffset = new Point(8, 5);

	private static readonly Point MoneyIconDrawOffset = new Point(57, 22);

	private static readonly Point MoneyDrawOffset = new Point(64, 22);

	private static readonly Point OrbSetDrawOffset = new Point(28, 17);

	private static readonly Point SpellOrbDrawOffset = new Point(48, 15);

	private readonly Rectangle _clockFrameSourceRectangle;

	private readonly Rectangle _hourglassSourceRectangle;

	private readonly Rectangle _hourglassSandSourceRectangle;

	private readonly Rectangle _moneyIconSourceRectangle;

	private readonly Rectangle _orbSetOneSourceRectangle;

	private readonly Rectangle _orbSetTwoSourceRectangle;

	private readonly Rectangle _orbSetThreeSourceRectangle;

	private readonly HourglassSandParticleSystem _hourglassSandParticles;

	private readonly SpriteSheet _sprite;

	private Color _whiteDraw = Color.White;

	private bool _isAuraOOMFlashing;

	private int _health = 2;

	private int _maxHealth = 2;

	private int _aura = 2;

	private int _maxAura = 2;

	private int _auraChargeSelect;

	private int _auraVileteFlashPositionX;

	private float _auraOOMSpellAmountPercentage;

	private float _healthPercent = 1f;

	private float _redHealthPercent = 1f;

	private float _auraPercent = 1f;

	private float _auraChargeSelectPercent;

	private float _auraOOMFlashTime;

	private float _auraOOMFlashPercentage;

	private float _auraVileteFlashTimer;

	private HudNumber _healthHUDNumber;

	private readonly List<HealthBlip> _healthBlips = new List<HealthBlip>();

	private bool _canStopTime;

	private bool _isMPDraining;

	private float _lastMaxMP;

	private float _targetMP;

	private float _visibleMP;

	private float _currentTimePercent = 1f;

	private float _hourglassSandTimer;

	private bool _isSandGlowing;

	private bool _isHourglassGlowing;

	private float _sandGlowTimer;

	private float _sandGlowinessDelta;

	private float _sandGlowiness = 1f;

	private float _auraGlowAmount;

	private float _auraGlowDelta;

	private int _healthAtLoss;

	private int _targetHealth;

	private float _lossHealth;

	private float _redHealthLossTimer;

	private bool _canSelectOrbSets;

	private EInventoryOrbType _spellOrbType;

	private int _orbSetIndex;

	private Rectangle _spellOrbFrameSource;

	private bool _isMoneyVisible;

	private bool _isMoneyFading;

	private bool _isOnVilete;

	private int _moneyDisplayedAmount = -1;

	private int _moneyActualAmount = -1;

	private int _moneyAddedAmount;

	private int _moneyAddedDisplayedAmount;

	private float _moneyTransferTimer;

	private float _moneyDisplayTimer;

	private float _moneyFadingTimer;

	private float _moneyFadeAmount;

	private HudNumber _moneyHUDNumber;

	private HudNumber _moneyAddedHUDNumber;

	private Point HourglassBottomSandDrawOffset => new Point(HourglassSandDrawOffset.X, HourglassSandDrawOffset.Y + _hourglassSandSourceRectangle.Height - 1);

	private Vector2 HourglassSandParticleOffset => new Vector2((float)HourglassSandDrawOffset.X + (float)_hourglassSandSourceRectangle.Width / 2f, HourglassBottomSandDrawOffset.Y);

	public HudLunais(GCM inGCM, Point drawPosition)
		: base(inGCM, drawPosition)
	{
		_hourglassSandParticles = new HourglassSandParticleSystem(inGCM.TxBlankSquare, 3);
		_sprite = inGCM.SpLunaisHUD;
		_clockFrameSourceRectangle = _sprite.GetFrameSource(0);
		_hourglassSourceRectangle = _sprite.GetFrameSource(1);
		_hourglassSandSourceRectangle = _sprite.GetFrameSource(2);
		_moneyIconSourceRectangle = _sprite.GetFrameSource(15);
		_orbSetOneSourceRectangle = _sprite.GetFrameSource(17);
		_orbSetTwoSourceRectangle = _sprite.GetFrameSource(18);
		_orbSetThreeSourceRectangle = _sprite.GetFrameSource(19);
	}

	public virtual void Update(float delta, Protagonist inLuny, GameSave inSave)
	{
		base.Update(delta);
		LunaisObj lunaisObj = (LunaisObj)inLuny;
		if (lunaisObj != null)
		{
			_canStopTime = inSave.IsTimeStopUnlocked;
			_isValidToDraw = true;
			UpdateHealth(delta, lunaisObj);
			UpdateAura(delta, lunaisObj);
			UpdateTimeMagic(delta, lunaisObj);
			UpdateMoney(delta, inSave);
			_canSelectOrbSets = inSave.IsOrbSwitchingUnlocked;
			_orbSetIndex = inSave.Inventory.EquippedOrbSetIndex;
			if (_spellOrbType != lunaisObj.EquippedSpellType)
			{
				_spellOrbType = lunaisObj.EquippedSpellType;
				int index = (int)(_spellOrbType + 21);
				_spellOrbFrameSource = _sprite.GetFrameSource(index);
			}
		}
		else
		{
			_isValidToDraw = false;
		}
	}

	protected void UpdateHealth(float delta, LunaisObj inLuny)
	{
		int targetHealth = _targetHealth;
		_targetHealth = inLuny.HP;
		_maxHealth = inLuny.MaxHP;
		if (targetHealth != _targetHealth)
		{
			RefreshHealthNumbers();
		}
		_health = _targetHealth;
		_healthPercent = ((_health == 0) ? 0f : ((float)(_health - 1) / (float)(_maxHealth - 1)));
		if ((float)_health >= _lossHealth)
		{
			_lossHealth = _health;
			_redHealthLossTimer = 0.5f;
			_healthAtLoss = _health;
		}
		else if (_redHealthLossTimer > 0f)
		{
			if (_lossHealth > (float)_targetHealth)
			{
				_lossHealth = (float)_targetHealth + (float)((double)(_healthAtLoss - _targetHealth) * Math.Pow(_redHealthLossTimer / 0.5f, 2.0));
			}
			_redHealthLossTimer -= delta;
			if (_redHealthLossTimer <= 0f)
			{
				_redHealthLossTimer = 0f;
			}
		}
		else
		{
			_lossHealth = _health;
		}
		_redHealthPercent = ((_lossHealth == 0f) ? 0f : ((_lossHealth - 1f) / (float)(_maxHealth - 1)));
		for (int num = _healthBlips.Count - 1; num >= 0; num--)
		{
			HealthBlip healthBlip = _healthBlips[num];
			healthBlip.Life += delta;
			if (healthBlip.Life >= healthBlip.MaxLife)
			{
				_healthBlips.RemoveAt(num);
			}
		}
	}

	private void RefreshHealthNumbers()
	{
		ENumberColor color = ENumberColor.White;
		if (_maxHealth > 0)
		{
			float num = (float)_targetHealth / (float)_maxHealth;
			if (num <= 0.2f)
			{
				color = ENumberColor.Red;
			}
			else if (num <= 0.4f)
			{
				color = ENumberColor.Yellow;
			}
		}
		_healthHUDNumber = new HudNumber(_targetHealth, new Point(_drawPosition.X + HealthHUDOffset.X * base.Zoom, _drawPosition.Y + HealthHUDOffset.Y * base.Zoom), _gcm, color);
	}

	private void UpdateAura(float delta, LunaisObj inLuny)
	{
		_aura = inLuny.Aura;
		_maxAura = inLuny.MaxAura;
		_auraChargeSelect = inLuny.ChargeSelect;
		_isOnVilete = inLuny.IsOnVilete;
		_auraPercent = ((_aura == 0) ? 0f : ((float)(_aura - 1) / (float)(_maxAura - 1)));
		_auraChargeSelectPercent = ((_auraChargeSelect == 0) ? 0f : ((float)(_auraChargeSelect - 1) / (float)(_maxAura - 1)));
		_auraGlowDelta += delta * 10f;
		if (_auraGlowDelta > 360f)
		{
			_auraGlowDelta -= 360f;
		}
		if (_auraPercent > 0f)
		{
			_auraGlowAmount = 0.5f + (float)Math.Cos(_auraGlowDelta) / 2f;
		}
		else
		{
			_auraGlowAmount = 1f;
		}
		if (inLuny.IsOOM)
		{
			inLuny.IsOOM = false;
			if (!_isAuraOOMFlashing)
			{
				_isAuraOOMFlashing = true;
				int oOMSpellAmount = inLuny.OOMSpellAmount;
				_auraOOMSpellAmountPercentage = ((oOMSpellAmount == 0) ? 0f : ((float)(oOMSpellAmount - 1) / (float)(_maxAura - 1)));
				_auraOOMFlashTime = 0f;
			}
		}
		if (_isAuraOOMFlashing)
		{
			_auraOOMFlashTime += delta;
			if (_auraChargeSelect > 0 || _auraOOMFlashTime > 1f)
			{
				_isAuraOOMFlashing = false;
			}
			else
			{
				float num = _auraOOMFlashTime / 1f;
				_auraOOMFlashPercentage = (float)Math.Cos(num * ((float)Math.PI * 2f) * 2f);
			}
		}
		if (!_isOnVilete)
		{
			return;
		}
		_auraVileteFlashPositionX = -1;
		_auraVileteFlashTimer += delta;
		if (!(_auraVileteFlashTimer >= 0.4f))
		{
			return;
		}
		float num2;
		if (_auraVileteFlashTimer > 1.1f)
		{
			_auraVileteFlashTimer = 0f;
			num2 = 1f;
		}
		else
		{
			num2 = (_auraVileteFlashTimer - 0.4f) / 0.7f;
			num2 = 1f - (float)Math.Cos(num2 * ((float)Math.PI / 2f));
		}
		if (num2 <= _auraPercent)
		{
			_auraVileteFlashPositionX = (int)(num2 * 52f) - 4;
			if (_auraVileteFlashPositionX < 0)
			{
				_auraVileteFlashPositionX = -1;
			}
		}
	}

	private void UpdateTimeMagic(float delta, LunaisObj inLuny)
	{
		float visibleMP = _visibleMP;
		_targetMP = inLuny.MPFloat;
		if (_lastMaxMP != (float)inLuny.MaxMP)
		{
			_visibleMP = _targetMP;
		}
		_lastMaxMP = inLuny.MaxMP;
		if (_targetMP != _visibleMP)
		{
			_visibleMP = HudElement.EaseHUDValue(_targetMP, _visibleMP, 25f, delta);
		}
		_currentTimePercent = _visibleMP / _lastMaxMP;
		float num = 4f / (0.3f + _currentTimePercent);
		_sandGlowinessDelta += delta * num;
		if (_sandGlowinessDelta > 360f)
		{
			_sandGlowinessDelta -= 360f;
		}
		_sandGlowiness = 0.9f - (float)Math.Cos(_sandGlowinessDelta);
		_isMPDraining = visibleMP > _visibleMP;
		if (_isMPDraining)
		{
			_hourglassSandTimer += delta;
			if (_hourglassSandTimer >= 0.1f)
			{
				_hourglassSandTimer = 0f;
				_hourglassSandParticles.AddParticles(Vector2.Add(_drawPosition.ToVector2(), Vector2.Multiply(HourglassSandParticleOffset, base.Zoom)));
			}
		}
		_hourglassSandParticles.Update(delta);
		if (inLuny.IsMPBeingRestored)
		{
			inLuny.IsMPBeingRestored = false;
			if (!_isSandGlowing)
			{
				_isSandGlowing = true;
				_sandGlowTimer = 0f;
			}
		}
		bool flag = _currentTimePercent >= 1f;
		if (!_isSandGlowing && flag)
		{
			_isSandGlowing = true;
			_sandGlowTimer = 0f;
		}
		if (_isSandGlowing)
		{
			_sandGlowTimer += delta * (flag ? 0.5f : 1f);
			float num2 = _sandGlowTimer / 0.5f;
			if (_sandGlowTimer > 1000f)
			{
				_sandGlowTimer = 0f;
			}
			if (num2 <= 1f || flag)
			{
				_sandGlowiness = 0.9f - (float)(Math.Cos(Math.PI * (double)num2) * 0.5);
			}
			else
			{
				_isSandGlowing = false;
			}
		}
		_isHourglassGlowing = _isSandGlowing && !flag;
	}

	private void UpdateMoney(float delta, GameSave inSave)
	{
		int money = inSave.Money;
		int moneyDisplayedAmount = _moneyDisplayedAmount;
		int moneyAddedDisplayedAmount = _moneyAddedDisplayedAmount;
		if (money != _moneyActualAmount)
		{
			if (_moneyActualAmount == -1)
			{
				_moneyActualAmount = money;
				_moneyDisplayedAmount = money;
			}
			else
			{
				int num = money - _moneyActualAmount;
				_moneyAddedAmount += num;
				_moneyAddedDisplayedAmount += num;
				_moneyActualAmount = money;
			}
		}
		_isMoneyVisible = true;
		_isMoneyFading = false;
		if (_moneyAddedAmount != 0)
		{
			_moneyDisplayTimer = 0f;
			_moneyFadingTimer = 0f;
			_moneyTransferTimer += delta;
			if (_moneyTransferTimer >= 0.066f)
			{
				_moneyTransferTimer -= 0.066f;
				int num2 = Math.Abs(_moneyAddedAmount);
				int num3 = (int)Math.Min(Math.Ceiling((float)num2 * 0.1f), num2);
				if (_moneyAddedAmount < 0)
				{
					num3 *= -1;
				}
				_moneyDisplayedAmount += num3;
				_moneyAddedAmount -= num3;
			}
			if (moneyDisplayedAmount != _moneyDisplayedAmount)
			{
				_moneyHUDNumber = new HudNumber(_moneyDisplayedAmount, new Point(_drawPosition.X + MoneyDrawOffset.X * base.Zoom, _drawPosition.Y + MoneyDrawOffset.Y * base.Zoom), _gcm)
				{
					IsLeftAdjusted = true
				};
			}
			if (moneyAddedDisplayedAmount != _moneyAddedDisplayedAmount && _moneyAddedAmount != 0)
			{
				float num4;
				if (_moneyActualAmount > _moneyDisplayedAmount || _moneyHUDNumber == null)
				{
					int digitsFromNumber = HudNumber.GetDigitsFromNumber(_moneyActualAmount);
					num4 = (digitsFromNumber + 1) * 5;
				}
				else
				{
					num4 = _moneyHUDNumber.Width + 5f;
				}
				_moneyAddedHUDNumber = new HudNumber(startPoint: new Point((int)((float)_drawPosition.X + ((float)MoneyDrawOffset.X + num4) * (float)base.Zoom), _drawPosition.Y + MoneyDrawOffset.Y * base.Zoom), amount: _moneyAddedDisplayedAmount, gcm: _gcm, color: (_moneyAddedAmount <= 0) ? ENumberColor.Red : ENumberColor.Green)
				{
					IsLeftAdjusted = true,
					DoesDrawSign = true
				};
			}
			else if (_moneyAddedAmount == 0)
			{
				_moneyAddedDisplayedAmount = 0;
			}
		}
		else if (_moneyDisplayTimer < 1f)
		{
			_moneyDisplayTimer += delta;
		}
		else
		{
			_isMoneyFading = true;
			if (_moneyFadingTimer < 0.15f)
			{
				_moneyFadingTimer += delta;
				_moneyFadeAmount = 1f - _moneyFadingTimer / 0.15f;
			}
			else
			{
				_moneyAddedHUDNumber = null;
				_isMoneyVisible = false;
			}
		}
	}

	public void Draw(SpriteBatch spriteBatch, float alphaAmount)
	{
		base.Draw(spriteBatch);
		if (_isValidToDraw)
		{
			_whiteDraw = Color.White * alphaAmount;
			DrawHealth(spriteBatch);
			DrawHealthBlips(spriteBatch);
			DrawAuraBar(spriteBatch, alphaAmount);
			spriteBatch.Draw(_sprite.Texture, new Vector2(_drawPosition.X, _drawPosition.Y), _clockFrameSourceRectangle, _whiteDraw, 0f, new Vector2(0f, 0f), base.Zoom, SpriteEffects.None, 0f);
			DrawHealthNumbers(spriteBatch, alphaAmount);
			DrawMoney(spriteBatch, alphaAmount);
			DrawHourglass(spriteBatch, alphaAmount);
			DrawSelectedOrb(spriteBatch, alphaAmount);
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
		}
	}

	private void DrawBar(SpriteBatch spriteBatch, Vector2 position, int startIndex, int totalWidth, int width, int xOffset, float heightPercentage, Color color)
	{
		if (width <= 0 || !(heightPercentage > 0f))
		{
			return;
		}
		int num = xOffset + width;
		int num2 = totalWidth - 4;
		int val = num - num2;
		int num3 = Math.Max(0, xOffset - num2);
		int num4 = Math.Min(width, 4 - xOffset);
		int num5 = Math.Min(width, val);
		int num6 = width - ((num4 > 0) ? num4 : 0) - num3;
		if (xOffset < 4)
		{
			Vector2 position2 = position;
			if (xOffset > 0)
			{
				position2 = new Vector2(position2.X + (float)(xOffset * base.Zoom), position2.Y);
			}
			Rectangle value = _sprite.GetFrameSource(startIndex);
			if (num4 != 4)
			{
				value = new Rectangle(value.X + xOffset, value.Y, num4, value.Height);
			}
			if (heightPercentage < 1f)
			{
				value = new Rectangle(value.X, value.Y, value.Width, (int)((float)value.Height * heightPercentage));
			}
			spriteBatch.Draw(_sprite.Texture, position2, value, color, 0f, Vector2.Zero, base.Zoom, SpriteEffects.None, 0f);
		}
		if (num6 > 0)
		{
			Point point = new Point((int)position.X + Math.Max(4, xOffset) * base.Zoom, (int)position.Y);
			Rectangle value = _sprite.GetFrameSource(startIndex + 1);
			if (heightPercentage < 1f)
			{
				value = new Rectangle(value.X, value.Y, value.Width, (int)((float)value.Height * heightPercentage));
			}
			spriteBatch.Draw(_sprite.Texture, new Rectangle(point.X, point.Y, num6 * base.Zoom, value.Height * base.Zoom), value, color);
			_ = 0;
		}
		if (num5 > 0)
		{
			Vector2 position3 = new Vector2(position.X + (float)((totalWidth - 4 + num3) * base.Zoom), position.Y);
			Rectangle value = _sprite.GetFrameSource(startIndex + 2);
			if (num5 != 4)
			{
				value = new Rectangle(value.X + num3, value.Y, num5, value.Height);
			}
			if (heightPercentage < 1f)
			{
				value = new Rectangle(value.X, value.Y, value.Width, (int)((float)value.Height * heightPercentage));
			}
			spriteBatch.Draw(_sprite.Texture, position3, value, color, 0f, Vector2.Zero, base.Zoom, SpriteEffects.None, 0f);
		}
	}

	private void DrawHealth(SpriteBatch spriteBatch)
	{
		DrawBar(spriteBatch, new Vector2(_drawPosition.X + HealthBarOffset.X * base.Zoom, _drawPosition.Y + HealthBarOffset.Y * base.Zoom), 12, 64, 64, 0, 1f, _whiteDraw);
		DrawBar(spriteBatch, new Vector2(_drawPosition.X + HealthBarOffset.X * base.Zoom, _drawPosition.Y + HealthBarOffset.Y * base.Zoom), 9, 64, (int)Math.Floor(64f * _redHealthPercent), 0, 1f, _whiteDraw);
		DrawBar(spriteBatch, new Vector2(_drawPosition.X + HealthBarOffset.X * base.Zoom, _drawPosition.Y + HealthBarOffset.Y * base.Zoom), 3, 64, (int)Math.Floor(64f * _healthPercent), 0, 1f, _whiteDraw);
	}

	private void DrawHealthBlips(SpriteBatch spriteBatch)
	{
		foreach (HealthBlip healthBlip in _healthBlips)
		{
			DrawBar(spriteBatch, new Vector2(_drawPosition.X + HealthBarOffset.X * base.Zoom, _drawPosition.Y + HealthBarOffset.Y * base.Zoom), 3, 64, healthBlip.Width, (int)healthBlip.StartingX, healthBlip.LifePercentage, _whiteDraw);
		}
	}

	private void DrawHealthNumbers(SpriteBatch spriteBatch, float alphaAmount)
	{
		if (_healthHUDNumber != null)
		{
			_healthHUDNumber.Draw(spriteBatch, base.Zoom, alphaAmount);
		}
	}

	private void DrawAuraBar(SpriteBatch spriteBatch, float alphaAmount)
	{
		DrawBar(spriteBatch, new Vector2(_drawPosition.X + AuraBarOffset.X * base.Zoom, _drawPosition.Y + AuraBarOffset.Y * base.Zoom), 12, 52, 52, 0, 1f, _whiteDraw);
		if (_isAuraOOMFlashing)
		{
			int width = (int)Math.Floor(52f * _auraOOMSpellAmountPercentage);
			int num = _drawPosition.X + AuraBarOffset.X * base.Zoom;
			Color color = Color.White * alphaAmount * 0.25f * _auraOOMFlashPercentage;
			DrawBar(spriteBatch, new Vector2(num, _drawPosition.Y + AuraBarOffset.Y * base.Zoom), 9, 52, width, 0, 1f, color);
		}
		Color color2 = Color.White;
		bool flag = alphaAmount == 1f && _isAuraOOMFlashing;
		if (flag)
		{
			color2 = new Color(1f, 1f, 1f, _auraOOMFlashPercentage);
			spriteBatch.End();
			_gcm.EfBrighten.Parameters["shinyAmount"].SetValue(1f);
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, _gcm.EfBrighten);
		}
		int num2 = (int)Math.Floor(52f * _auraPercent);
		Vector2 position = new Vector2(_drawPosition.X + AuraBarOffset.X * base.Zoom, _drawPosition.Y + AuraBarOffset.Y * base.Zoom);
		if (!_isOnVilete)
		{
			DrawBar(spriteBatch, position, 6, 52, num2, 0, 1f, color2 * alphaAmount);
		}
		else
		{
			DrawBar(spriteBatch, position, 39, 52, num2, 0, 1f, color2 * alphaAmount);
			if (_auraVileteFlashPositionX > 0)
			{
				Rectangle frameSource = _sprite.GetFrameSource(42);
				spriteBatch.Draw(position: new Vector2(position.X + (float)(_auraVileteFlashPositionX * base.Zoom), position.Y), texture: _sprite.Texture, sourceRectangle: frameSource, color: Color.White * alphaAmount, rotation: 0f, origin: Vector2.Zero, scale: base.Zoom, effects: SpriteEffects.None, layerDepth: 0f);
			}
		}
		if (_auraChargeSelectPercent > 0f)
		{
			float alpha = _auraGlowAmount;
			if (alphaAmount != 1f)
			{
				alpha = 1f;
			}
			bool flag2 = !flag && alphaAmount == 1f;
			flag = flag2;
			if (flag2)
			{
				spriteBatch.End();
				_gcm.EfBrighten.Parameters["shinyAmount"].SetValue(1f);
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, _gcm.EfBrighten);
			}
			int num3 = (int)Math.Floor(52f * _auraChargeSelectPercent);
			int xOffset = num2 - num3;
			DrawBar(spriteBatch, new Vector2(_drawPosition.X + AuraBarOffset.X * base.Zoom, _drawPosition.Y + AuraBarOffset.Y * base.Zoom), 9, 52, num3, xOffset, 1f, new Color(1f, 1f, 0f, alpha) * alphaAmount);
			if (flag)
			{
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
			}
		}
	}

	private void DrawHourglass(SpriteBatch spriteBatch, float alphaAmount)
	{
		if (_lastMaxMP > 0f && _canStopTime && alphaAmount > 0f)
		{
			_hourglassSandParticles.Draw(spriteBatch, Vector2.Zero, Vector2.Zero, 1f);
			float num = _visibleMP / _lastMaxMP;
			float num2 = num;
			int num3 = (int)((float)_hourglassSandSourceRectangle.Height * num2);
			int num4 = _hourglassSandSourceRectangle.Height - num3;
			int num5 = (HourglassSandDrawOffset.Y + num4) * base.Zoom;
			Color color = _whiteDraw;
			if (_isMPDraining || _isSandGlowing)
			{
				spriteBatch.End();
				_gcm.EfBrighten.Parameters["shinyAmount"].SetValue(1f);
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, _gcm.EfBrighten);
				color = new Color(1f, 1f, 1f, _sandGlowiness) * alphaAmount;
			}
			Rectangle value = new Rectangle(_hourglassSandSourceRectangle.X, _hourglassSandSourceRectangle.Y + num4, _hourglassSandSourceRectangle.Width, num3);
			spriteBatch.Draw(_sprite.Texture, new Vector2(_drawPosition.X + HourglassSandDrawOffset.X * base.Zoom, _drawPosition.Y + num5), value, color, 0f, new Vector2(0f, 0f), base.Zoom, SpriteEffects.None, 0f);
			spriteBatch.End();
			_gcm.EfBrighten.Parameters["shinyAmount"].SetValue(0.6f);
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, _gcm.EfBrighten);
			color = new Color(1f, 1f, 1f, 0f) * alphaAmount;
			num3 = (int)((float)_hourglassSandSourceRectangle.Height * num2);
			num5 = (HourglassBottomSandDrawOffset.Y + num3) * base.Zoom;
			value.Height = _hourglassSandSourceRectangle.Height - num3;
			value.Y = _hourglassSandSourceRectangle.Y + num3;
			spriteBatch.Draw(_sprite.Texture, new Vector2(_drawPosition.X + HourglassBottomSandDrawOffset.X * base.Zoom, _drawPosition.Y + num5), value, color, 0f, new Vector2(0f, 0f), base.Zoom, SpriteEffects.FlipVertically, 0f);
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
		}
		Color color2 = _whiteDraw;
		if (_isHourglassGlowing && alphaAmount >= 1f)
		{
			spriteBatch.End();
			_gcm.EfBrighten.Parameters["shinyAmount"].SetValue(1f);
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, _gcm.EfBrighten);
			color2 = new Color(1f, 1f, 1f, _sandGlowiness) * alphaAmount;
		}
		spriteBatch.Draw(_sprite.Texture, new Vector2(_drawPosition.X + HourglassDrawOffset.X * base.Zoom, _drawPosition.Y + HourglassDrawOffset.Y * base.Zoom), _hourglassSourceRectangle, color2, 0f, Vector2.Zero, base.Zoom, SpriteEffects.None, 0f);
		if (_isHourglassGlowing && alphaAmount >= 1f)
		{
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
		}
	}

	private void DrawMoney(SpriteBatch spriteBatch, float alphaAmount)
	{
		if (_isMoneyVisible)
		{
			float num = alphaAmount;
			if (_isMoneyFading)
			{
				num *= _moneyFadeAmount;
			}
			if (_moneyHUDNumber != null)
			{
				spriteBatch.Draw(_sprite.Texture, new Vector2(_drawPosition.X + MoneyIconDrawOffset.X * base.Zoom, _drawPosition.Y + MoneyIconDrawOffset.Y * base.Zoom), _moneyIconSourceRectangle, Color.White * num, 0f, new Vector2(0f, 0f), base.Zoom, SpriteEffects.None, 0f);
				_moneyHUDNumber.Draw(spriteBatch, base.Zoom, num);
			}
			if (_moneyAddedHUDNumber != null)
			{
				_moneyAddedHUDNumber.Draw(spriteBatch, base.Zoom, num);
			}
		}
	}

	private void DrawSelectedOrb(SpriteBatch spriteBatch, float alphaAmount)
	{
		Color color = Color.White * alphaAmount;
		if (_canSelectOrbSets && _orbSetIndex >= 0 && _orbSetIndex < 3)
		{
			spriteBatch.Draw(sourceRectangle: _orbSetIndex switch
			{
				1 => _orbSetTwoSourceRectangle, 
				2 => _orbSetThreeSourceRectangle, 
				_ => _orbSetOneSourceRectangle, 
			}, texture: _sprite.Texture, position: new Vector2(_drawPosition.X + OrbSetDrawOffset.X * base.Zoom, _drawPosition.Y + OrbSetDrawOffset.Y * base.Zoom), color: color, rotation: 0f, origin: Vector2.Zero, scale: base.Zoom, effects: SpriteEffects.None, layerDepth: 0f);
		}
		if (_spellOrbType == EInventoryOrbType.None)
		{
			return;
		}
		if (_auraChargeSelectPercent > 0f)
		{
			float alpha = _auraGlowAmount;
			if (alphaAmount != 1f)
			{
				alpha = 1f;
			}
			color = new Color(1f, 1f, 1f, alpha) * alphaAmount;
			spriteBatch.End();
			_gcm.EfBrighten.Parameters["shinyAmount"].SetValue(1f);
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, _gcm.EfBrighten);
		}
		spriteBatch.Draw(_sprite.Texture, new Vector2(_drawPosition.X + SpellOrbDrawOffset.X * base.Zoom, _drawPosition.Y + SpellOrbDrawOffset.Y * base.Zoom), _spellOrbFrameSource, color, 0f, Vector2.Zero, base.Zoom, SpriteEffects.None, 0f);
		if (_auraChargeSelectPercent > 0f)
		{
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
		}
	}

	internal void RefreshDrawPosition(Point newDrawPosition)
	{
		_drawPosition = newDrawPosition;
		RefreshHealthNumbers();
	}
}
