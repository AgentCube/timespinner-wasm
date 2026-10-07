using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Timespinner.Core;
using Timespinner.GameAbstractions;
using Timespinner.GameAbstractions.Assets.Audio;
using Timespinner.GameAbstractions.Gameplay;
using Timespinner.GameObjects.Animations;

namespace Timespinner.GameObjects.Bosses.Emperor;

internal class EmperorOrbManager
{
	internal const int OrbCount = 3;

	private const int ShockWaveOffsetX = 4;

	private const int ShockWaveOffsetY = 4;

	private const int ChargeOriginOffsetX = 0;

	private const int ChargeOriginOffsetY = -50;

	private static readonly Vector4 BladeParticleColor = new Vector4(0.75f, 1f, 0.65f, 1f);

	private static readonly Vector4 BlueParticleColor = new Vector4(0.66f, 0.66f, 1f, 1f);

	private static readonly Vector4 EmpireParticleColor = new Vector4(0.75f, 0.5f, 0.8f, 1f);

	private static readonly Vector4 FlameParticleColor = new Vector4(1f, 0.6f, 0.3f, 1f);

	private static readonly Vector4 IronParticleColor = new Vector4(0.75f, 0.75f, 0.7f, 1f);

	private static readonly Vector4 PlasmaParticleColor = new Vector4(1f, 0.5f, 0.75f, 1f);

	private readonly int _baseDamage;

	private readonly ShockwaveAnimation _shockwave;

	private readonly EmperorChargeParticleSystem _chargeParticles;

	private readonly EmperorChargeLazerParticleSystem _chargeLazerParticles;

	private readonly EmperorChargeBall _chargeBall;

	private readonly EmperorBoss _parentEmperor;

	private readonly Level _level;

	private readonly List<EmperorOrb> _orbs = new List<EmperorOrb>();

	private bool _areOrbsHidden;

	private bool _isCharging;

	private bool _isParentFacingLeft;

	private bool _isShockwaveActive;

	private EEmperorOrbType _currentOrbSet;

	private Point _parentPosition;

	private Vector2 _chargeOrigin;

	private Color _chargeColor;

	private SFXCueInstance _chargeLoopCueInstance;

	internal List<EmperorOrb> Orbs => _orbs;

	internal EmperorOrbManager(SpriteSheet orbSprite, EmperorBoss parentEmperor, EEmperorOrbType startingType, int baseDamage, bool isViletianEmperor)
	{
		_parentEmperor = parentEmperor;
		_level = _parentEmperor.Level;
		_baseDamage = baseDamage;
		Point center = _parentEmperor.OuterBbox.Center;
		_currentOrbSet = startingType;
		for (int i = 0; i < 3; i++)
		{
			EmperorOrb item = new EmperorOrb(center, _level, i, orbSprite, startingType, _baseDamage, i, isViletianEmperor);
			_orbs.Add(item);
		}
		_chargeBall = new EmperorChargeBall(parentEmperor, _level, _level.GCM.SpOrbMeleeBarrier);
		_shockwave = new ShockwaveAnimation(_level.GCM.SpOrbMeleeBarrier, Point.Zero, _level, Color.White);
		_chargeParticles = new EmperorChargeParticleSystem(_level.GCM.TxParticleEnergy, 2);
		_chargeLazerParticles = new EmperorChargeLazerParticleSystem(_level.GCM.TxParticleEnergy, 1);
	}

	internal void Update(float delta)
	{
		_parentPosition = _parentEmperor.Position;
		_isParentFacingLeft = _parentEmperor.IsFacingLeft;
		foreach (EmperorOrb orb in _orbs)
		{
			orb.Update(delta, _parentPosition, _isParentFacingLeft);
		}
		UpdateCharging(delta);
		if (_isShockwaveActive)
		{
			_shockwave.Update(delta);
			if (_shockwave.IsFinished)
			{
				_isShockwaveActive = false;
			}
		}
	}

	private void UpdateCharging(float delta)
	{
		if (_isCharging)
		{
			RefreshChargeOrigin();
			_chargeParticles.AddParticles(_chargeOrigin);
			_chargeLazerParticles.AddParticles(_chargeOrigin);
			_chargeBall.Update(delta);
		}
		_chargeParticles.Update(delta, _parentPosition);
		_chargeLazerParticles.Update(delta, _parentPosition);
	}

	private void RefreshChargeOrigin()
	{
		_chargeOrigin = new Vector2(_parentPosition.X + (_isParentFacingLeft ? 0 : 0), _parentPosition.Y + -50);
	}

	internal void DrawOrbs(SpriteBatch spriteBatch, bool isOnFrontPlane)
	{
		if (!_areOrbsHidden)
		{
			foreach (EmperorOrb orb in _orbs)
			{
				if (orb.IsOnFrontPlane == isOnFrontPlane)
				{
					orb.Draw(spriteBatch);
				}
			}
		}
		if (isOnFrontPlane)
		{
			_chargeParticles.Draw(spriteBatch, _level.LevelRenderCenter, _level.CameraPosition, _level.CameraZoom);
			_chargeLazerParticles.Draw(spriteBatch, _level.LevelRenderCenter, _level.CameraPosition, _level.CameraZoom);
			if (_isCharging)
			{
				_chargeBall.Draw(spriteBatch);
			}
			if (_isShockwaveActive)
			{
				_shockwave.Draw(spriteBatch);
			}
		}
	}

	internal void SetAreOrbsHidden(bool areOrbsHidden, bool doesShowAnimation)
	{
		_areOrbsHidden = areOrbsHidden;
		if (!doesShowAnimation)
		{
			return;
		}
		foreach (EmperorOrb orb in _orbs)
		{
			orb.AddOrbChangeAnimation(areOrbsHidden);
		}
	}

	internal void ChangeOrbSet(EEmperorOrbType newSet)
	{
		if (newSet == _currentOrbSet)
		{
			return;
		}
		_currentOrbSet = newSet;
		foreach (EmperorOrb orb in _orbs)
		{
			orb.ChangeOrbType(_currentOrbSet);
		}
	}

	internal void StartCharging()
	{
		EmperorOrbOrbitSpecification emperorOrbOrbitSpecification = default(EmperorOrbOrbitSpecification);
		emperorOrbOrbitSpecification.AnchorOffset = new Point(12, -5);
		emperorOrbOrbitSpecification.Radius = 6;
		emperorOrbOrbitSpecification.Frequency = 15f;
		emperorOrbOrbitSpecification.TransitionTime = 0.5f;
		emperorOrbOrbitSpecification.DurationTime = 5f;
		emperorOrbOrbitSpecification.OrbOrbitType = EEmperorOrbOrbitType.Charging;
		EmperorOrbOrbitSpecification spec = emperorOrbOrbitSpecification;
		PushOrbSpec(spec, 0f);
		_isCharging = true;
		Vector4 chargeColorByEmperorOrbType = GetChargeColorByEmperorOrbType(_currentOrbSet);
		_chargeParticles.BaseColor = chargeColorByEmperorOrbType;
		_chargeLazerParticles.BaseColor = chargeColorByEmperorOrbType;
		_chargeColor = new Color(chargeColorByEmperorOrbType.X, chargeColorByEmperorOrbType.Y, chargeColorByEmperorOrbType.Z, chargeColorByEmperorOrbType.W);
		_chargeBall.Reset(_chargeColor, _orbs[0].TrailColor);
		if (_chargeLoopCueInstance == null)
		{
			_chargeLoopCueInstance = _parentEmperor.PlayCue(ESFX.BossEmperorChargeLoop, isLooped: true);
		}
		else if (_chargeLoopCueInstance.IsPaused)
		{
			_chargeLoopCueInstance.Resume();
		}
	}

	internal static Vector4 GetChargeColorByEmperorOrbType(EEmperorOrbType orbType)
	{
		Vector4 result = Vector4.One;
		switch (orbType)
		{
		case EEmperorOrbType.Blade:
			result = BladeParticleColor;
			break;
		case EEmperorOrbType.Blue:
			result = BlueParticleColor;
			break;
		case EEmperorOrbType.Empire:
			result = EmpireParticleColor;
			break;
		case EEmperorOrbType.Fire:
			result = FlameParticleColor;
			break;
		case EEmperorOrbType.Plasma:
			result = PlasmaParticleColor;
			break;
		case EEmperorOrbType.Iron:
			result = IronParticleColor;
			break;
		}
		return result;
	}

	internal void StopCharging()
	{
		_isCharging = false;
		ResetShockwave(_chargeColor);
		_level.PlayCue(ESFX.BossEmperorChargeEnd, _shockwave.Position);
		if (_chargeLoopCueInstance != null && !_chargeLoopCueInstance.IsPaused)
		{
			_chargeLoopCueInstance.Pause(0.1f);
		}
		foreach (EmperorOrb orb in _orbs)
		{
			orb.ReturnToBaseOrbit();
		}
	}

	internal void ResetShockwave(Color shockwaveColor)
	{
		RefreshChargeOrigin();
		_shockwave.DrawColor = shockwaveColor;
		Point position = new Point((int)_chargeOrigin.X + 4, (int)_chargeOrigin.Y + 4);
		_shockwave.Reset(position, _isParentFacingLeft);
		_isShockwaveActive = true;
	}

	internal float DoMeleeAttack()
	{
		float result = 10f;
		switch (_currentOrbSet)
		{
		case EEmperorOrbType.Blue:
		case EEmperorOrbType.Fire:
		{
			EmperorOrbOrbitSpecification emperorOrbOrbitSpecification6 = default(EmperorOrbOrbitSpecification);
			emperorOrbOrbitSpecification6.AnchorOffset = new Point(0, 0);
			emperorOrbOrbitSpecification6.Radius = 48;
			emperorOrbOrbitSpecification6.Frequency = 10f;
			emperorOrbOrbitSpecification6.TransitionTime = 0.25f;
			emperorOrbOrbitSpecification6.DurationTime = 0.5f;
			emperorOrbOrbitSpecification6.IndexDurationMultiplier = 0.35f;
			EmperorOrbOrbitSpecification spec = emperorOrbOrbitSpecification6;
			EmperorOrbOrbitSpecification emperorOrbOrbitSpecification7 = default(EmperorOrbOrbitSpecification);
			emperorOrbOrbitSpecification7.AnchorOffset = new Point(0, 32);
			emperorOrbOrbitSpecification7.Radius = 16;
			emperorOrbOrbitSpecification7.Frequency = 8f;
			emperorOrbOrbitSpecification7.TransitionTime = 0.25f;
			emperorOrbOrbitSpecification7.DurationTime = 1f;
			emperorOrbOrbitSpecification7.OrbOrbitType = ((_currentOrbSet == EEmperorOrbType.Blue) ? EEmperorOrbOrbitType.BlueAttack : EEmperorOrbOrbitType.FireAttack);
			EmperorOrbOrbitSpecification spec2 = emperorOrbOrbitSpecification7;
			PushOrbSpec(spec, 0.15f);
			PushOrbSpec(spec2, 0f);
			result = 0.65f;
			break;
		}
		case EEmperorOrbType.Blade:
		case EEmperorOrbType.Iron:
		{
			EmperorOrbOrbitSpecification emperorOrbOrbitSpecification4 = default(EmperorOrbOrbitSpecification);
			emperorOrbOrbitSpecification4.AnchorOffset = new Point(0, 0);
			emperorOrbOrbitSpecification4.Radius = 48;
			emperorOrbOrbitSpecification4.Frequency = 10f;
			emperorOrbOrbitSpecification4.TransitionTime = 0.5f;
			emperorOrbOrbitSpecification4.DurationTime = 0.5f;
			emperorOrbOrbitSpecification4.IndexDurationMultiplier = 0.35f;
			EmperorOrbOrbitSpecification spec = emperorOrbOrbitSpecification4;
			EmperorOrbOrbitSpecification emperorOrbOrbitSpecification5 = default(EmperorOrbOrbitSpecification);
			emperorOrbOrbitSpecification5.AnchorOffset = new Point(0, 44);
			emperorOrbOrbitSpecification5.Radius = 24;
			emperorOrbOrbitSpecification5.Frequency = 1f;
			emperorOrbOrbitSpecification5.TransitionTime = 0.25f;
			emperorOrbOrbitSpecification5.DurationTime = 2.65f;
			emperorOrbOrbitSpecification5.OrbOrbitType = ((_currentOrbSet == EEmperorOrbType.Blade) ? EEmperorOrbOrbitType.BladeAttack : EEmperorOrbOrbitType.IronAttack);
			EmperorOrbOrbitSpecification spec2 = emperorOrbOrbitSpecification5;
			result = 0.65f;
			PushOrbSpec(spec, 0f);
			PushOrbSpec(spec2, 0f);
			break;
		}
		case EEmperorOrbType.Empire:
		{
			EmperorOrbOrbitSpecification emperorOrbOrbitSpecification3 = default(EmperorOrbOrbitSpecification);
			emperorOrbOrbitSpecification3.AnchorOffset = new Point(32, 8);
			emperorOrbOrbitSpecification3.Radius = 36;
			emperorOrbOrbitSpecification3.Frequency = 12f;
			emperorOrbOrbitSpecification3.TransitionTime = 0.25f;
			emperorOrbOrbitSpecification3.DurationTime = 1.5f;
			emperorOrbOrbitSpecification3.OrbOrbitType = EEmperorOrbOrbitType.EmpireAttack;
			EmperorOrbOrbitSpecification spec2 = emperorOrbOrbitSpecification3;
			PushOrbSpec(spec2, 0f);
			break;
		}
		case EEmperorOrbType.Plasma:
		{
			EmperorOrbOrbitSpecification emperorOrbOrbitSpecification = default(EmperorOrbOrbitSpecification);
			emperorOrbOrbitSpecification.AnchorOffset = new Point(0, -64);
			emperorOrbOrbitSpecification.Radius = 24;
			emperorOrbOrbitSpecification.Frequency = 15f;
			emperorOrbOrbitSpecification.TransitionTime = 0.5f;
			emperorOrbOrbitSpecification.DurationTime = 0.25f;
			EmperorOrbOrbitSpecification spec = emperorOrbOrbitSpecification;
			EmperorOrbOrbitSpecification emperorOrbOrbitSpecification2 = default(EmperorOrbOrbitSpecification);
			emperorOrbOrbitSpecification2.AnchorOffset = new Point(0, -64);
			emperorOrbOrbitSpecification2.Radius = 24;
			emperorOrbOrbitSpecification2.Frequency = 15f;
			emperorOrbOrbitSpecification2.TransitionTime = 0.25f;
			emperorOrbOrbitSpecification2.DurationTime = 1.5f;
			emperorOrbOrbitSpecification2.OrbOrbitType = EEmperorOrbOrbitType.PlasmaAttack;
			EmperorOrbOrbitSpecification spec2 = emperorOrbOrbitSpecification2;
			PushOrbSpec(spec, 0f);
			PushOrbSpec(spec2, 0f);
			result = 1.05f;
			break;
		}
		}
		return result;
	}

	private void PushOrbSpec(EmperorOrbOrbitSpecification spec, float delay)
	{
		float num = 0f;
		foreach (EmperorOrb orb in _orbs)
		{
			orb.AddOrbState(spec, num);
			num += delay;
		}
	}

	internal void SetDrawColor(Color drawColor)
	{
		foreach (EmperorOrb orb in _orbs)
		{
			orb.DrawColor = drawColor;
		}
	}

	internal void UpdateDeathGlow(float glowBase, Color glowColor)
	{
		foreach (EmperorOrb orb in _orbs)
		{
			if (orb != null)
			{
				orb.IsGlowing = true;
				orb.GlowBase = glowBase;
				orb.GlowColor = glowColor;
			}
		}
	}

	internal void Kill()
	{
		_isCharging = false;
		if (_chargeLoopCueInstance != null)
		{
			_chargeLoopCueInstance.Stop();
		}
		foreach (EmperorOrb orb in _orbs)
		{
			orb?.Kill();
		}
	}

	internal void Freeze()
	{
		foreach (EmperorOrb orb in _orbs)
		{
			orb.Freeze();
		}
	}

	internal void Unfreeze()
	{
		foreach (EmperorOrb orb in _orbs)
		{
			orb.Unfreeze();
		}
	}
}
